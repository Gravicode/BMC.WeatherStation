using System;

namespace Weather.Monitor
{
    /*public class Motetx
    {
        public int freq { get; set; }
        public string modu { get; set; }
        public string datr { get; set; }
        public string codr { get; set; }
    }

    public class Userdata
    {
        public int seqno { get; set; }
        public int port { get; set; }
        public string payload { get; set; }
        public Motetx motetx { get; set; }
    }

    public class Gwrx
    {
        public string time { get; set; }
        public int chan { get; set; }
        public int rfch { get; set; }
        public int rssi { get; set; }
        public double lsnr { get; set; }
    }

    public class Rx
    {
        public string moteeui { get; set; }
        public Userdata userdata { get; set; }
        public Gwrx[] gwrx { get; set; }
    }

    public class RootObject
    {
        public Rx rx { get; set; }
    }*/

    public class Rootobject
    {
        public Rx rx { get; set; }
    }

    public class Rx
    {
        public string moteeui { get; set; }
        public Userdata userdata { get; set; }
        public Gwrx[] gwrx { get; set; }
    }

    public class Userdata
    {
        public int seqno { get; set; }
        public int port { get; set; }
        public string payload { get; set; }
        public Motetx motetx { get; set; }
    }

    public class Motetx
    {
        public int freq { get; set; }
        public string modu { get; set; }
        public string datr { get; set; }
        public string codr { get; set; }
    }

    public class Gwrx
    {
        public string time { get; set; }
        public int chan { get; set; }
        public int rfch { get; set; }
        public int rssi { get; set; }
        public float lsnr { get; set; }
    }

    class Model
    {
    }
}
