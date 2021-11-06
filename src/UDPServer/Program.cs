using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start udp server!");

            var th = new Action(Loop);
            th.Invoke();
            Thread.Sleep(Timeout.Infinite);
        }

        static void Loop()
        {
            #region udp client
            var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var ip = new IPAddress(new byte[] { 192, 168, 1, 101 });
            var endPoint = new IPEndPoint(ip, 8888);

            socket.Connect(endPoint);

            byte[] bytesToSend;
            var counter = 0;
            while (true)
            {
                try
                {
                    var msg = "count-" + counter++;
                    bytesToSend = Encoding.UTF8.GetBytes(msg);
                    socket.SendTo(bytesToSend, bytesToSend.Length, SocketFlags.None, endPoint);
                    Console.WriteLine("sending: "+msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error -> " + ex.Message);
                }
                Thread.Sleep(500);
            }
            #endregion
            #region udp server
            /*
            UdpClient udpServer = new UdpClient(8888);
            var remoteEP = new IPEndPoint(IPAddress.Any, 8888);

            while (true)
            {
                try
                {
                    var data = udpServer.Receive(ref remoteEP); // listen on port 8888

                    var datastr = System.Text.Encoding.Default.GetString(data);
                    Console.WriteLine("receive data from " + remoteEP.ToString());
                    Console.WriteLine("data: " + datastr);

                    var dataStr = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss");
                    var dBytes = System.Text.Encoding.UTF8.GetBytes(dataStr);
                    udpServer.Send(dBytes,dBytes.Length,remoteEP);

                    var obj = JsonConvert.DeserializeObject<RootObject>(datastr);
                    if (obj != null)
                    {
                        byte[] databyte = Convert.FromBase64String(obj.rx.userdata.payload);
                        string decodedString = Encoding.UTF8.GetString(databyte);
                        var originalValue = Unpack(decodedString);
                        Console.WriteLine("unpack :" + originalValue);
                        var sensorValue = JsonConvert.DeserializeObject<SensorData>(originalValue);
                        sensorValue.Tanggal = DateTime.Now;
                    //    //call power bi api
                    //    //SendToPowerBI(sensorValue);
                    //    //send data to gateway

                    //    {
                    //        Transmitter.ObjMoteTx objtx = new Transmitter.ObjMoteTx();
                    //        objtx.tx = new Transmitter.Tx();
                    //        objtx.tx.moteeui = "00000000AAABBBEE";
                    //        objtx.tx.txmsgid = "000000000001";
                    //        objtx.tx.trycount = 5;
                    //        objtx.tx.txsynch = false;
                    //        objtx.tx.ackreq = false;

                    //        //string to hex str, hex str to base64 string
                    //        relay = !relay;
                    //        byte[] ba = Encoding.Default.GetBytes("relay:" + (relay ? "1" : "0"));
                    //        var hexString = BitConverter.ToString(ba);
                    //        hexString = hexString.Replace("-", "");
                    //        hexString = Base64Encode(hexString);
                    //        objtx.tx.userdata = new Transmitter.Userdata() { payload = hexString, port = 5 };//"Njg2NTZjNmM2ZjIwNjM2ZjZkNzA3NTc0NjU3Mg==" -> hello computer
                    //        var jsonStr = JsonConvert.SerializeObject(objtx);
                    //        byte[] bytes = Encoding.ASCII.GetBytes(jsonStr);
                    //        udpServer.Send(bytes, bytes.Length, remoteEP);
                    //    }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error -> " + ex.Message);
                }
                Thread.Sleep(200);




            }
            */
            #endregion
        }
    }
}
