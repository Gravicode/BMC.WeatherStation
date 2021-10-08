using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using GHI.Processor;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using System.IO.Ports;
using System.Text;
using Microsoft.SPOT.Hardware;

namespace BMC.WeatherStation
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            // Timeout 5 seconds
            int timeout = 1000 * 5;

            // Enable Watchdog
            GHI.Processor.Watchdog.Enable(timeout);

            // Start a time counter reset thread
            WDTCounterReset = new Thread(WDTCounterResetLoop);
            WDTCounterReset.Start();


            var th1 = new Thread(new ThreadStart(loop));
            th1.Start();
            //ReadSerial();
        }
        static SerialPort uart;
        void ReadSerial()
        {
            //uart.DataReceived += uart_DataReceived;
            /*
            while (true)
            {
                // read one byte
                read_count = uart.Read(rx_byte, 0, 1);
                if (read_count > 0)// do we have data?
                {
                    // create a string
                    string counter_string =
                            "You typed: " + rx_byte[0].ToString() + "\r\n";
                    // convert the string to bytes
                    byte[] buffer = Encoding.UTF8.GetBytes(counter_string);
                    // send the bytes on the serial port
                    uart.Write(buffer, 0, buffer.Length);
                    //wait...
                    Thread.Sleep(10);
                }
            }*/

        }
        static double temp;
        static byte[] databuffer = new byte[35];
        /*
        void uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int read_count = 0;
            byte[] rx_byte = new byte[35];

            read_count = uart.Read(rx_byte, 0, rx_byte.Length);
            if (read_count > 0)// do we have data?
            {
                string buffer = new string( Encoding.UTF8.GetChars(rx_byte));
                databuffer += buffer;
                if (buffer.IndexOf("\r\n")>-1)
                {
                    Debug.Print("rec: " + databuffer);
                    databuffer = string.Empty;
                }
            }
        }*/
        void loop()
        {

            string ComPort = GHI.Pins.FEZRaptor.Socket10.SerialPortName;
            uart = new SerialPort(ComPort, 9600);

            uart.Open();

            UART = new SimpleSerial(GHI.Pins.FEZRaptor.Socket4.SerialPortName, 57600);
            UART.ReadTimeout = 0;
            UART.DataReceived += UART_DataReceived;
            Debug.Print("57600");
            Debug.Print("RN2483 Test");
            PrintToLcd("RN2483 Test");
            OutputPort reset = new OutputPort(GHI.Pins.FEZRaptor.Socket4.Pin6, false);
            OutputPort reset2 = new OutputPort(GHI.Pins.FEZRaptor.Socket4.Pin3, false);

            reset.Write(true);
            reset2.Write(true);

            Thread.Sleep(100);
            reset.Write(false);
            reset2.Write(false);

            Thread.Sleep(100);
            reset.Write(true);
            reset2.Write(true);

            Thread.Sleep(100);

            waitForResponse();

            sendCmd("sys factoryRESET");
            sendCmd("sys get hweui");
            sendCmd("mac get deveui");
            Thread.Sleep(3000);
            // For TTN
            sendCmd("mac set devaddr AAABBBDD");  // Set own address
            Thread.Sleep(3000);
            sendCmd("mac set appskey 2B7E151628AED2A6ABF7158809CF4F3D");
            Thread.Sleep(3000);

            sendCmd("mac set nwkskey 2B7E151628AED2A6ABF7158809CF4F3D");
            Thread.Sleep(3000);

            sendCmd("mac set adr off");
            Thread.Sleep(3000);

            sendCmd("mac set rx2 3 868400000");//869525000
            Thread.Sleep(3000);

            sendCmd("mac join abp");
            Thread.Sleep(3000);
            sendCmd("mac get status");
            sendCmd("mac get devaddr");
            Thread.Sleep(2000);


            while (true)
            {
                getBuffer();
                //lora
                var data = new SensorData()
                {
                    WindDirection = WindDirection(),
                    WindSpeedMax = WindSpeedMax(),
                    BarPressure = BarPressure(),
                    Humidity = Humidity(),
                    RainfallOneDay = RainfallOneDay(),
                    RainfallOneHour = RainfallOneHour(),
                    Temperature = Temperature()
                    , WindSpeedAverage = WindSpeedAverage()


                };//Begin!
                Debug.Print("Wind Direction: "+data.WindDirection);
                Debug.Print("Average Wind Speed (One Minute): " + data.WindSpeedAverage + "m/s  ");
                Debug.Print("Max Wind Speed (Five Minutes): " + data.WindSpeedMax + "m/s");
                Debug.Print("Rain Fall (One Hour): " + data.RainfallOneHour + "mm  ");
                Debug.Print("Rain Fall (24 Hour): " + data.RainfallOneDay + "mm");
                Debug.Print("Temperature: " + data.Temperature + "C  ");
                Debug.Print("Humidity: " + data.Humidity + "%  ");
                Debug.Print("Barometric Pressure: " + data.BarPressure + "hPa");
                Debug.Print("----------------------");
                
               
                var jsonStr = Json.NETMF.JsonSerializer.SerializeObject(data);
                Debug.Print("kirim :" + jsonStr);
                sendData(jsonStr);
                Thread.Sleep(5000);
                byte[] rx_data = new byte[20];

                if (UART.CanRead)
                {
                    var count = UART.Read(rx_data, 0, rx_data.Length);
                    if (count > 0)
                    {
                        Debug.Print("count:" + count);
                        var hasil = new string(System.Text.Encoding.UTF8.GetChars(rx_data));
                        Debug.Print("read:" + hasil);
                        //mac_rx 2 AABBCC
                    }
                }
                var TimeStr = DateTime.Now.ToString("dd/MM/yy HH:mm");
                var th2 = new Thread(new ThreadStart(blinkLed));
                th2.Start();
                Thread.Sleep(2000);
            }



        }

        static Thread WDTCounterReset;
        static void WDTCounterResetLoop()
        {
            while (true)
            {
                // reset time counter every 3 seconds
                Thread.Sleep(3000);

                GHI.Processor.Watchdog.ResetCounter();
            }
        }

        void blinkLed()
        {
            bool state = true;
            for (int i = 0; i < 6; i++)
            {
                Mainboard.SetDebugLED(state);
                Thread.Sleep(300);
                state = !state;
            }

        }
        void getBuffer()                                                                    //Get weather status data
        {
            int index;
            for (index = 0; index < 35; index++)
            {
                if (uart.BytesToRead>0)
                {
                    databuffer[index] = (byte) uart.ReadByte();
                    if (databuffer[0] != 'c')
                    {
                        index = -1;
                    }
                }
                else
                {
                    index--;
                }
            }
        }

        int transCharToInt(byte[] _buffer, int _start, int _stop)                               //char to int）
        {
            int _index;
            int result = 0;
            int num = _stop - _start + 1;
            var _temp = new int[num];
            for (_index = _start; _index <= _stop; _index++)
            {
                _temp[_index - _start] = _buffer[_index] - '0';
                result = 10 * result + _temp[_index - _start];
            }
            return result;
        }

        int WindDirection()                                                                  //Wind Direction
        {
            return transCharToInt(databuffer, 1, 3);
        }

        double WindSpeedAverage()                                                             //air Speed (1 minute)
        {
            temp = 0.44704 * transCharToInt(databuffer, 5, 7);
            return temp;
        }

        double WindSpeedMax()                                                                 //Max air speed (5 minutes)
        {
            temp = 0.44704 * transCharToInt(databuffer, 9, 11);
            return temp;
        }

        double Temperature()                                                                  //Temperature ("C")
        {
            temp = (transCharToInt(databuffer, 13, 15) - 32.00) * 5.00 / 9.00;
            return temp;
        }

        double RainfallOneHour()                                                              //Rainfall (1 hour)
        {
            temp = transCharToInt(databuffer, 17, 19) * 25.40 * 0.01;
            return temp;
        }

        double RainfallOneDay()                                                               //Rainfall (24 hours)
        {
            temp = transCharToInt(databuffer, 21, 23) * 25.40 * 0.01;
            return temp;
        }

        int Humidity()                                                                       //Humidity
        {
            return transCharToInt(databuffer, 25, 26);
        }

        double BarPressure()                                                                  //Barometric Pressure
        {
            temp = transCharToInt(databuffer, 28, 32);
            return temp / 10.00;
        }

        private static string[] _dataInLora;
        private static string rx;


        void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            _dataInLora = UART.Deserialize();
            for (int index = 0; index < _dataInLora.Length; index++)
            {
                rx = _dataInLora[index];
                //if error
                if (_dataInLora[index].Length > 5)
                {

                    //if receive data
                    if (rx.Substring(0, 6) == "mac_rx")
                    {
                        string hex = _dataInLora[index].Substring(9);

                        //update display

                        byte[] data = StringToByteArrayFastest(hex);
                        string decoded = new String(UTF8Encoding.UTF8.GetChars(data));
                        Debug.Print("decoded:" + decoded);
                        //txtMessage.Text = decoded;//Unpack(hex);
                        
                    }
                }
            }
            Debug.Print(rx);
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        SimpleSerial UART = null;

        void PrintToLcd(string Message)
        {
            //update display
            //txtTime.Text = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss");
            //txtMessage.Text = "Data Transmitted Successfully.";
            //txtTime.Invalidate();
            //txtMessage.Invalidate();
            //window.Invalidate();
        }



        void sendCmd(string cmd)
        {
            byte[] rx_data = new byte[20];
            Debug.Print(cmd);
            Debug.Print("\n");
            // flush all data
            UART.Flush();
            // send some data
            var tx_data = Encoding.UTF8.GetBytes(cmd);
            UART.Write(tx_data, 0, tx_data.Length);
            tx_data = Encoding.UTF8.GetBytes("\r\n");
            UART.Write(tx_data, 0, tx_data.Length);
            Thread.Sleep(100);
            while (!UART.IsOpen)
            {
                UART.Open();
                Thread.Sleep(100);
            }
            if (UART.CanRead)
            {
                var count = UART.Read(rx_data, 0, rx_data.Length);
                if (count > 0)
                {
                    Debug.Print("count cmd:" + count);
                    var hasil = new string(System.Text.Encoding.UTF8.GetChars(rx_data));
                    Debug.Print("read cmd:" + hasil);
                }
            }
        }

        void waitForResponse()
        {
            byte[] rx_data = new byte[20];

            while (!UART.IsOpen)
            {
                UART.Open();
                Thread.Sleep(100);
            }
            if (UART.CanRead)
            {
                var count = UART.Read(rx_data, 0, rx_data.Length);
                if (count > 0)
                {
                    Debug.Print("count res:" + count);
                    var hasil = new string(System.Text.Encoding.UTF8.GetChars(rx_data));
                    Debug.Print("read res:" + hasil);
                }

            }
        }
        public static string Unpack(string input)
        {
            byte[] b = new byte[input.Length / 2];

            for (int i = 0; i < input.Length; i += 2)
            {
                b[i / 2] = (byte)((FromHex(input[i]) << 4) | FromHex(input[i + 1]));
            }
            return new string(Encoding.UTF8.GetChars(b));
        }
        public static int FromHex(char digit)
        {
            if ('0' <= digit && digit <= '9')
            {
                return (int)(digit - '0');
            }

            if ('a' <= digit && digit <= 'f')
                return (int)(digit - 'a' + 10);

            if ('A' <= digit && digit <= 'F')
                return (int)(digit - 'A' + 10);

            throw new ArgumentException("digit");
        }

        char getHexHi(char ch)
        {
            int nibbleInt = ch >> 4;
            char nibble = (char)nibbleInt;
            int res = (nibble > 9) ? nibble + 'A' - 10 : nibble + '0';
            return (char)res;
        }
        char getHexLo(char ch)
        {
            int nibbleInt = ch & 0x0f;
            char nibble = (char)nibbleInt;
            int res = (nibble > 9) ? nibble + 'A' - 10 : nibble + '0';
            return (char)res;
        }

        void sendData(string msg)
        {
            byte[] rx_data = new byte[20];
            char[] data = msg.ToCharArray();
            Debug.Print("mac tx uncnf 1 ");
            var tx_data = Encoding.UTF8.GetBytes("mac tx uncnf 1 ");
            UART.Write(tx_data, 0, tx_data.Length);

            // Write data as hex characters
            foreach (char ptr in data)
            {
                tx_data = Encoding.UTF8.GetBytes(new string(new char[] { getHexHi(ptr) }));
                UART.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes(new string(new char[] { getHexLo(ptr) }));
                UART.Write(tx_data, 0, tx_data.Length);


                Debug.Print(new string(new char[] { getHexHi(ptr) }));
                Debug.Print(new string(new char[] { getHexLo(ptr) }));
            }
            tx_data = Encoding.UTF8.GetBytes("\r\n");
            UART.Write(tx_data, 0, tx_data.Length);
            Debug.Print("\n");
            Thread.Sleep(5000);

            if (UART.CanRead)
            {
                var count = UART.Read(rx_data, 0, rx_data.Length);
                if (count > 0)
                {
                    Debug.Print("count after:" + count);
                    var hasil = new string(System.Text.Encoding.UTF8.GetChars(rx_data));
                    Debug.Print("read after:" + hasil);
                }
            }
        }
    }

    public class SensorData
    {
        public double WindSpeedAverage { get; set; }
        public double WindDirection { get; set; }
        public double WindSpeedMax { get; set; }
        public double RainfallOneHour { get; set; }
        public double RainfallOneDay { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double BarPressure { get; set; }
    }








}
