using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using System.IO.Ports;
using System.Text;

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

            while (true)
            {
                getBuffer();                                                                      //Begin!
                Debug.Print("Wind Direction: "+WindDirection());
                Debug.Print("Average Wind Speed (One Minute): " + WindSpeedAverage() + "m/s  ");
                Debug.Print("Max Wind Speed (Five Minutes): " + WindSpeedMax() + "m/s");
                Debug.Print("Rain Fall (One Hour): " + RainfallOneHour() + "mm  ");
                Debug.Print("Rain Fall (24 Hour): " + RainfallOneDay() + "mm");
                Debug.Print("Temperature: " + Temperature() + "C  ");
                Debug.Print("Humidity: " + Humidity() + "%  ");
                Debug.Print("Barometric Pressure: " + BarPressure() + "hPa");
                Debug.Print("----------------------");
                Thread.Sleep(100);
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

    }
}
