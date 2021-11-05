using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Weather.Monitor
{
    class Program
   : Application
    {
        public Program(DisplayController d) : base(d)
        {
        }
        static Program app;
        static string SSID = "wifi lemot";
        static string SSID_Pass = "123qweasd";
        static void Main()
        {
            GpioPin backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);
            var display = DisplayController.GetDefault();

            var controllerSetting = new
                GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings
            {
                Width = 480,
                Height = 272,
                DataFormat = GHIElectronics.TinyCLR.Devices.Display.DisplayDataFormat.Rgb565,
                Orientation = DisplayOrientation.Degrees0, //Rotate display.
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            };

            display.SetConfiguration(controllerSetting);
            display.Enable();

            var screen = Graphics.FromHdc(display.Hdc);
            var controller = I2cController.FromName(SC20260.I2cBus.I2c1);
            //var device = controller.GetDevice(settings);
            //var controller = I2cController.GetDefault();
            ConnectWifi();
            app = new Program(display);
            app.Run(Program.CreateWindow(display));
        }

        private static Window CreateWindow(DisplayController display)
        {
            var window = new Window
            {
                Height = (int)display.ActiveConfiguration.Height,
                Width = (int)display.ActiveConfiguration.Width
            };

            window.Background = new LinearGradientBrush
                (Colors.Blue, Colors.Teal, 0, 0, window.Width, window.Height);

            window.Visibility = Visibility.Visible;

            return window;
        }
        /*
        private static UIElement Elements()
        {
            var canvas = new Canvas();

            var txt = new Text(font, "TinyCLR is Great!")
            {
                ForeColor = Colors.White,
            };

            var rect = new GHIElectronics.TinyCLR.UI.Shapes.Rectangle(150, 30)
            {
                Fill = new SolidColorBrush(Colors.Green),
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Canvas.SetLeft(rect, 20);
            Canvas.SetBottom(rect, 20);

            canvas.Children.Add(rect);

            Canvas.SetLeft(txt, 30);
            Canvas.SetBottom(txt, 25);

            canvas.Children.Add(txt);

            return canvas;
        }
        */

        //static void Main()
        //    {
        //        /*
        //        GHIElectronics.TinyCLR.Native.Memory.ExtendHeap();
        //        GHIElectronics.TinyCLR.Native.Power.Reset();


        //        GHIElectronics.TinyCLR.Native.Flash.EnableExtendDeployment();
        //        GHIElectronics.TinyCLR.Native.Power.Reset();
        //        */


        //        //RunUDP();
        //        Thread.Sleep(Timeout.Infinite);
        //    }
        static void ConnectWifi()
        {
            var enablePin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA8);
            enablePin.SetDriveMode(GpioPinDriveMode.Output);
            enablePin.Write(GpioPinValue.High);

            SpiNetworkCommunicationInterfaceSettings netInterfaceSettings =
                new SpiNetworkCommunicationInterfaceSettings();

            var cs = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA6);

            var settings = new SpiConnectionSettings()
            {
                ChipSelectLine = cs,
                ClockFrequency = 4000000,
                Mode = SpiMode.Mode0,
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectHoldTime = TimeSpan.FromTicks(10),
                ChipSelectSetupTime = TimeSpan.FromTicks(10)
            };

            netInterfaceSettings.SpiApiName = SC20260.SpiBus.Spi3;

            netInterfaceSettings.GpioApiName = SC20260.GpioPin.Id;

            netInterfaceSettings.SpiSettings = settings;
            netInterfaceSettings.InterruptPin = GpioController.GetDefault().
                OpenPin(SC20260.GpioPin.PF10);

            netInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
            netInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
            netInterfaceSettings.ResetPin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PC3);
            netInterfaceSettings.ResetActiveState = GpioPinValue.Low;

            var networkController = NetworkController.FromName
                (SC20260.NetworkController.ATWinc15x0);

            WiFiNetworkInterfaceSettings wifiSettings = new WiFiNetworkInterfaceSettings()
            {
                Ssid = SSID,
                Password = SSID_Pass,
            };

            wifiSettings.Address = new IPAddress(new byte[] { 192, 168, 1, 43 });
            wifiSettings.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
            wifiSettings.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
            wifiSettings.DnsAddresses = new IPAddress[] { new IPAddress(new byte[]
        { 192,168,1,1 }), new IPAddress(new byte[] { 8, 8, 8, 8 }) };

            wifiSettings.MacAddress = new byte[] { 0x00, 0x4, 0x00, 0x00, 0x00, 0x00 };
            wifiSettings.DhcpEnable = true;
            wifiSettings.DynamicDnsEnable = true;

            networkController.SetInterfaceSettings(wifiSettings);
            networkController.SetCommunicationInterfaceSettings(netInterfaceSettings);
            networkController.SetAsDefaultController();

            networkController.NetworkAddressChanged += NetworkController_NetworkAddressChanged;

            networkController.NetworkLinkConnectedChanged +=
                NetworkController_NetworkLinkConnectedChanged;

            networkController.Enable();

            // Network is ready to use
            //Thread.Sleep(Timeout.Infinite);
        }

        private static void NetworkController_NetworkLinkConnectedChanged
            (NetworkController sender, NetworkLinkConnectedChangedEventArgs e)
        {
            // Raise event connect/disconnect
        }

        private static void NetworkController_NetworkAddressChanged
            (NetworkController sender, NetworkAddressChangedEventArgs e)
        {
            var ipProperties = sender.GetIPProperties();
            var address = ipProperties.Address.GetAddressBytes();
            Debug.WriteLine("IP: " + address[0] + "." + address[1] + "." + address[2] +
                "." + address[3]);
            if (address[3] > 0)
            {
                OpenUrl();
                RunUDP();
            }
        }
        static void OpenUrl()
        {
            var url = "http://www.bing.com/robots.txt";

            int read = 0, total = 0;
            byte[] result = new byte[512];

            try
            {
                using (var req = HttpWebRequest.Create(url) as HttpWebRequest)
                {
                    req.KeepAlive = false;
                    req.ReadWriteTimeout = 2000;

                    using (var res = req.GetResponse() as HttpWebResponse)
                    {
                        using (var stream = res.GetResponseStream())
                        {
                            do
                            {
                                read = stream.Read(result, 0, result.Length);
                                total += read;

                                Debug.WriteLine("read : " + read);
                                Debug.WriteLine("total : " + total);

                                String page = "";

                                page = new String(System.Text.Encoding.UTF8.GetChars
                                    (result, 0, read));

                                Debug.WriteLine("Response : " + page);
                            }

                            while (read != 0);
                        }
                    }
                }
            }
            catch
            {
            }
        }
        //static void Loop()
        //{
        //    UdpClient udpServer = new UdpClient(8888);
        //    bool relay = false;
        //    while (true)
        //    {
        //        try
        //        {
        //            var remoteEP = new IPEndPoint(IPAddress.Any, 8888);
        //            var data = udpServer.Receive(ref remoteEP); // listen on port 8888

        //            var datastr = System.Text.Encoding.Default.GetString(data);
        //            Console.WriteLine("receive data from " + remoteEP.ToString());
        //            Console.WriteLine("data: " + datastr);
        //            var obj = JsonConvert.DeserializeObject<RootObject>(datastr);
        //            if (obj != null)
        //            {
        //                byte[] databyte = Convert.FromBase64String(obj.rx.userdata.payload);
        //                string decodedString = Encoding.UTF8.GetString(databyte);
        //                var originalValue = Unpack(decodedString);
        //                Console.WriteLine("unpack :" + originalValue);
        //                var sensorValue = JsonConvert.DeserializeObject<SensorData>(originalValue);
        //                sensorValue.Tanggal = DateTime.Now;
        //                //call power bi api
        //                SendToPowerBI(sensorValue);
        //                //send data to gateway

        //                {
        //                    Transmitter.ObjMoteTx objtx = new Transmitter.ObjMoteTx();
        //                    objtx.tx = new Transmitter.Tx();
        //                    objtx.tx.moteeui = "00000000AAABBBEE";
        //                    objtx.tx.txmsgid = "000000000001";
        //                    objtx.tx.trycount = 5;
        //                    objtx.tx.txsynch = false;
        //                    objtx.tx.ackreq = false;

        //                    //string to hex str, hex str to base64 string
        //                    relay = !relay;
        //                    byte[] ba = Encoding.Default.GetBytes("relay:" + (relay ? "1" : "0"));
        //                    var hexString = BitConverter.ToString(ba);
        //                    hexString = hexString.Replace("-", "");
        //                    hexString = Base64Encode(hexString);
        //                    objtx.tx.userdata = new Transmitter.Userdata() { payload = hexString, port = 5 };//"Njg2NTZjNmM2ZjIwNjM2ZjZkNzA3NTc0NjU3Mg==" -> hello computer
        //                    var jsonStr = JsonConvert.SerializeObject(objtx);
        //                    byte[] bytes = Encoding.ASCII.GetBytes(jsonStr);
        //                    udpServer.Send(bytes, bytes.Length, remoteEP);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("error -> " + ex.Message);
        //        }
        //        Thread.Sleep(5000);




        //    }
        static void RunUDP()
        {
            var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var ip = new IPAddress(new byte[] { 192, 168, 1, 108 });
            var endPoint = new IPEndPoint(ip, 8888);
            
            socket.Connect(endPoint);
            //var buff = new byte[1000];
            //var remoteEP = new IPEndPoint(IPAddress.Any, 8888);
            //socket.Connect(endPoint);


            //while (true)
            //{
            //    try
            //    {

            //        var rec = socket.Receive(buff); // listen on port 8888
            //        if (rec > 0)
            //        {
            //            var datastr = System.Text.Encoding.UTF8.GetString(buff);
            //            Debug.WriteLine("receive data from " + remoteEP.ToString());
            //            Debug.WriteLine("data: " + datastr);
            //        }
            //        Thread.Sleep(100);
            //    }
            //    catch
            //    {

            //    }
            //}

            byte[] bytesToSend;
            var counter = 0;
            while (true)
            {
                bytesToSend = Encoding.UTF8.GetBytes("count-" + counter++);
                socket.SendTo(bytesToSend, bytesToSend.Length, SocketFlags.None, endPoint);
                while (socket.Poll(500000, SelectMode.SelectRead))
                {
                    if (socket.Available > 0)
                    {
                        byte[] inBuf = new byte[socket.Available];
                        EndPoint recEndPoint = new IPEndPoint(IPAddress.Any, 8888);
                        socket.ReceiveFrom(inBuf, ref recEndPoint);
                        if (!recEndPoint.Equals(endPoint))// Check if the received packet is from the 192.168.0.2
                            continue;
                        Debug.WriteLine(new String(Encoding.UTF8.GetChars(inBuf)));
                    }
                }
                Thread.Sleep(100);
            }

        }
    }
}
