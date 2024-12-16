using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace GameServer
{
    public static class Utils
    {
        public static long TickCount { get { return System.Environment.TickCount64; } }

        public static IPAddress GetLocalIP()
        {
            var ipHost = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in ipHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return IPAddress.Loopback;
        }
    }
}
