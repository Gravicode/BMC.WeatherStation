using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Watchdog;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
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
using Weather.Monitor.Properties;

namespace Weather.Monitor
{
    class Program
   : Application
    {
        public Program(DisplayController d) : base(d)
        {
        }
        static Thread udpThread;
        static Program app;
        static string SSID = "BMC Makerspace";//"wifi lemot";
        static string SSID_Pass = "123qweasd";
        static bool DisplayReady = false;
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
            ConnectWifi();
            
           

            app = new Program(display);

            //touch
            /*
            var touch = new FT5xx6Controller(i2cController.GetDevice(FT5xx6Controller.GetConnectionSettings()),
            GpioController.GetDefault().OpenPin(SC20260.GpioPin.PG9));*/
            var i2cController = I2cController.FromName(SC20260.I2cBus.I2c1);
            var device = i2cController.GetDevice(FT5xx6Controller.GetConnectionSettings());

            var irq = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PG9);
            irq.SetDriveMode(GpioPinDriveMode.InputPullDown);

            var touch = new FT5xx6Controller(device, irq);
            touch.Orientation = FT5xx6Controller.TouchOrientation.Degrees0; //Rotate touch coordinates.

            touch.TouchUp += (_, e) => {
                app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Up, DateTime.UtcNow);
                //app.InputProvider.RaiseButton(btn, btnState, DateTime.UtcNow);

            };
            touch.TouchDown += (_, e) => {
                app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Down, DateTime.UtcNow);
                //app.InputProvider.RaiseButton(btn, btnState, DateTime.UtcNow);

            };
            touch.TouchMove += (_, e) => {
                app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Move, DateTime.UtcNow);
                //app.InputProvider.RaiseButton(btn, btnState, DateTime.UtcNow);
            };
            watchThread = new Thread(new ThreadStart(RunWatchDog));
            watchThread.Start();

            app.Run(Program.CreateWindow(display));
        }
        static Thread watchThread;
        static void RunWatchDog()
        {

            // Set watchdog to 5 seconds and reset it every 4 seconds
            var WatchDog = WatchdogController.GetDefault();
            WatchDog.Enable(5000);

            while (true)
            {
                //reset the timer
                WatchDog.Reset();
                Thread.Sleep(4000);
            }
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

            window.Child = Elements();
            DisplayReady = true;
            return window;
        }
        static string IPAddr;
        static Text []txtSensor;
        static Text txtTitle;
        static Text txtUpdate;
        const string Title = "WEATHER SENSOR WITH LORAWAN";
        private static UIElement Elements()
        {
            var font = Resources.GetFont(Resources.FontResources.NinaB);
            var panel = new StackPanel(Orientation.Vertical);
            var solid = new SolidColorBrush(Colors.Yellow);
            txtTitle = new Text()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                ForeColor = Colors.Yellow
            };
            txtTitle.Font = font;
            txtTitle.SetMargin(2);
            txtTitle.TextContent = $"{Title}";
            panel.Children.Add(txtTitle);

            txtUpdate = new Text()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                ForeColor = Colors.Black
            };
            txtUpdate.Font = font;
            txtUpdate.SetMargin(2);
            txtUpdate.TextContent = $"[TIME]";
            panel.Children.Add(txtUpdate);

            txtSensor = new Text[8];
            for (int i = 0; i < txtSensor.Length; i++)
            {
                var txt = new Text()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    ForeColor =  Colors.White
                };
                txt.Font = font;
                
                txt.SetMargin(5);
                txt.TextContent = $"Sensor {i} = 0.0";
                txtSensor[i] = txt;

                panel.Children.Add(txt);

            }



            //var rect = new GHIElectronics.TinyCLR.UI.Shapes.Rectangle(200, 10)
            //{
            //    Fill = new SolidColorBrush(Colors.Green),
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //};
            // Create a scrollviewer
            /*
            var scrollViewer = new ScrollViewer
            {
                Background = new SolidColorBrush(Colors.Transparent),

                // scroll line by line with 10 pixels per line
                ScrollingStyle = ScrollingStyle.LineByLine,
                LineWidth = 10,
                LineHeight = 10
            };
            scrollViewer.TouchUp += ScrollViewer_TouchUp;
            scrollViewer.Child = panel;
            */
            var txtBtn = new Text(font, "RESET")
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            
            var button = new Button()
            {
                Child = txtBtn,
                Width = 100,
                Height = 35,
            };

            button.Click += BtnReset_Click;
            panel.Children.Add(button);

            return panel;
            
        }
        private static void ScrollViewer_TouchUp(object sender, GHIElectronics.TinyCLR.UI.Input.TouchEventArgs e)
        {
            var s = (ScrollViewer)sender;

            s.LineDown();
        }


        static void UpdateUI(Hashtable data)
        {
            if (!DisplayReady) return;
            int count = 0;
            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ =>
            {
                txtTitle.TextContent = $"{Title} ({IPAddr})";
                txtTitle.Invalidate();
                txtUpdate.TextContent = $"updated at {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")}";
                txtUpdate.Invalidate();
                foreach (var item in data.Keys)
                {
                    txtSensor[count].TextContent = $"{item} = {data[item]}";
                    txtSensor[count].Invalidate();
                    count++;
                }
                return null;
            }, null);

        }
        private static void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            GHIElectronics.TinyCLR.Native.Power.Reset();

            // Add button click event code here...
        }


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
            IPAddr = address[0] + "." + address[1] + "." + address[2] +
                "." + address[3];
            Debug.WriteLine("IP: " + IPAddr);
            if (address[3] > 0)
            {
                OpenUrl();
                udpThread = new Thread(new ThreadStart(RunUDP));
                udpThread.Start();
            }
        }
        #region decoder encoder
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
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

        public static string Unpack(string input)
        {

            byte[] b = new byte[input.Length / 2];



            for (int i = 0; i < input.Length; i += 2)
            {

                b[i / 2] = (byte)((FromHex(input[i]) << 4) | FromHex(input[i + 1]));

            }

            return new string(Encoding.UTF8.GetChars(b));

        }

        #endregion
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
       
        static void RunUDP()
        {
            #region udp server
            //as a server
            using (Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 8888);

                serverSocket.Bind(remoteEndPoint);
                while (true)
                {
                    if (serverSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        byte[] inBuffer = new byte[serverSocket.Available];
                        int count = serverSocket.ReceiveFrom(inBuffer, ref remoteEndPoint);
                        string message = new string(Encoding.UTF8.GetChars(inBuffer));
                        Debug.WriteLine("Received '" + message + "'.");
                        //deserialize
                        try
                        {
                           var obj = Json.TinyCLR.JsonSerializer.DeserializeString(message);
                            //var obj = (Rootobject)GHIElectronics.TinyCLR.Data.Json.JsonConverter.FromBson(inBuffer, typeof(Rootobject));
                            if (obj != null && obj is Hashtable)
                            {
                                var json = (Hashtable)obj;
                                foreach (var item in json.Keys)
                                {
                                    if(item.ToString() == "rx")
                                    {
                                        var rx = (Hashtable)json[item];
                                        foreach (var item2 in rx.Keys)
                                        {
                                            if (item2.ToString() == "userdata")
                                            {
                                                var userdata = (Hashtable)rx[item2];
                                                foreach (var item3 in userdata.Keys)
                                                {
                                                    if (item3.ToString() == "payload")
                                                    {
                                                        var payload = userdata[item3].ToString();
                                                        byte[] databyte = Convert.FromBase64String(payload);
                                                        string decodedString = Encoding.UTF8.GetString(databyte);
                                                        var originalValue = Unpack(decodedString);
                                                        var obj2 = Json.TinyCLR.JsonSerializer.DeserializeString(originalValue);
                                                        var sensorData = (Hashtable)obj2;
                                                        foreach(var itemSensor in sensorData.Keys)
                                                        {
                                                            Debug.WriteLine($"{itemSensor} : {sensorData[itemSensor]}");
                                                        }
                                                        UpdateUI(sensorData);
                                                        goto exit1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            exit1:
                                ;
                                //byte[] databyte = Convert.FromBase64String(obj.rx.userdata.payload);
                                //string decodedString = Encoding.UTF8.GetString(databyte);
                                //var originalValue = Unpack(decodedString);
                                //Debug.WriteLine("unpack :" + originalValue);
                                //deserialize object n show to UI
                                //var sensorValue = JsonConvert.DeserializeObject<SensorData>(originalValue);
                                //sensorValue.Tanggal = DateTime.Now;
                            }
                            

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("err:"+ex);
                        }
                    }
                    Thread.Sleep(500);
                }
            }
            #endregion
            #region udp client
            /*
            var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var ip = new IPAddress(new byte[] { 192, 168, 1, 108 });
            var endPoint = new IPEndPoint(ip, 8888);
            
            socket.Connect(endPoint);
            
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
            }*/
            #endregion
        }
    }
}
