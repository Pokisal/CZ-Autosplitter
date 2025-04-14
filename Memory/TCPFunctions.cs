﻿using CZAutosplitter.UI.Components;
using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CZAutosplitter.Memory
{
    public class TCPFunctions
    {
        public static string IP = "";
        public static string TitleID = "58410A8D";

        public static bool IsGameRunning()
        {
            string Result = Encoding.ASCII.GetString(RequestMemory(0xC201B7E4, 8));
            if (Result == TitleID)
            {
                return true;
            }
            return false;
        }
        public static bool ParseIP(string input)
        {
            IPAddress address;
            return IPAddress.TryParse(input, out address) && address.ToString() == input;
        }
        public static byte[] RequestMemory(uint address, uint length)
        {
            /// Check if inputted IP is actually a valid IP and someone didn't just accidentally hit set
            if (!ParseIP(IP)) return new byte[length];
            var tcp = new TcpClient();
            tcp.ReceiveTimeout = 1000;
            tcp.SendTimeout = 1000;
            if (!tcp.Client.ConnectAsync(IP, 730).Wait(1000))
            {
                /// Wait 1 second to connect to the Xbox if it doesn't IP probably isn't valid so close the connection and return nothing
                /// If you continue to try to spam TCP requests to a client that doesn't exist Livesplit will hang
                IP = "";
                tcp.Close();
                return new byte[length];
            }
            /// Requires 2 requests of 1024 bytes to clear that a successful connection has been established
            var response1 = new byte[1024];
            var response2 = new byte[1024];
            tcp.Client.Receive(response1);
            /// Requires a read of an entire memory chunk regardless if that's what you ask for and only doing it from Livesplit seems to have this issue for some reason
            /// You can even just request a full memory chunk but not receive it and it doesn't care
            tcp.Client.Send(Encoding.ASCII.GetBytes(string.Format("GETMEMEX ADDR={0} LENGTH={1}\r\n", address, 1024)));
            tcp.Client.Receive(response2);
            /// First two bytes are header data that needs to be discarded
            byte[] data = new byte[length];
            byte[] data2 = new byte[length + 2];
            tcp.Client.Receive(data2);
            Array.Copy(data2, 2, data, 0, length);
            tcp.Close();
            return data;
        }
    }
}
