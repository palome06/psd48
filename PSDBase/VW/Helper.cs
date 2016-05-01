using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace PSD.Base.VW
{
    public static class WHelper
    {
        private static IDictionary<NetworkStream, BinaryReader> rTable =
            new Dictionary<NetworkStream, BinaryReader>();
        private static IDictionary<NetworkStream, BinaryWriter> wTable =
            new Dictionary<NetworkStream, BinaryWriter>();
        // public const int MSG_SIZE = 4096;
        // Read from Socket Tunnel
        public static string ReadByteLine(BinaryReader br)
        {
            try
            {
                return br.ReadString();
            }
            catch (IOException) { return ""; }
        }
        // Write into Socket Tunnel
        public static void SentByteLine(BinaryWriter bw, string value)
        {
            try
            {
                bw.Write(value);
                bw.Flush();
            }
            catch (IOException) { }
        }

        public static string ReadByteLine(NetworkStream nw)
        {
            if (!rTable.ContainsKey(nw))
            {
                BinaryReader br = new BinaryReader(nw);
                rTable[nw] = br;
                return ReadByteLine(br);
            }
            else
                return ReadByteLine(rTable[nw]);
        }
        public static void SentByteLine(NetworkStream nw, string value)
        {
            if (!wTable.ContainsKey(nw))
            {
                BinaryWriter bw = new BinaryWriter(nw);
                wTable[nw] = bw;
                SentByteLine(bw, value);
            }
            else
                SentByteLine(wTable[nw], value);
        }
    }
}