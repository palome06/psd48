using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PSD.Base.VW
{
    public static class WHelper
    {
        public const int MSG_SIZE = 4096;
        // Read from Socket Tunnel
        public static string ReadByteLine(NetworkStream ns)
        {
            try
            {
                byte[] byteInt = new byte[sizeof(int)];
                ns.Read(byteInt, 0, sizeof(int));
                int clen = Math.Min(BitConverter.ToInt32(byteInt, 0), MSG_SIZE);
                byte[] actual = new byte[MSG_SIZE];
                ns.Read(actual, 0, clen);
                return clen > 0 ? Encoding.Unicode.GetString(actual, 0, clen) : null;
            }
            catch (IOException) { return ""; }
        }
        // Write into Socket Tunnel
        public static void SentByteLine(NetworkStream ns, string value)
        {
            byte[] buf = new byte[MSG_SIZE];
            byte[] actual = Encoding.Unicode.GetBytes(value);
            int al = actual.Length;
            BitConverter.GetBytes(al).CopyTo(buf, 0);
            actual.CopyTo(buf, sizeof(int));
            ns.Write(buf, 0, al + sizeof(int));
            ns.Flush();
        }
    }
}