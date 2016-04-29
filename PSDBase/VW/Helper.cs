using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PSD.Base.VW
{
    public static class WHelper
    {
        // public const int MSG_SIZE = 4096;
        // Read from Socket Tunnel
        public static string ReadByteLine(NetworkStream ns)
        {
            try
            {
                byte[] byteInt = new byte[sizeof(int)];
                // read raw size from the remote stream
                ns.Read(byteInt, 0, sizeof(int));
                // convert it into network form
                int clenN2H = BitConverter.ToInt32(byteInt, 0);
                // convert in into local host form
                int clen = IPAddress.NetworkToHostOrder(clenN2H);
                // read actual messge string
                byte[] actual = new byte[clen];
                ns.Read(actual, 0, clen);
                // convert byte arrya to string
                return clen > 0 ? Encoding.Unicode.GetString(actual, 0, clen) : null;
            }
            catch (IOException) { return ""; }
        }
        // Write into Socket Tunnel
        public static void SentByteLine(NetworkStream ns, string value)
        {
            // copy string to a byte array
            byte[] dataArray = Encoding.Unicode.GetBytes(value);
            // get string length
            int reqLen = dataArray.Length;
            // convert string length value to network order
            int reqLenH2N = IPAddress.HostToNetworkOrder(reqLen);
            // get string length value into a byte array
            byte[] reqLenArray = BitConverter.GetBytes(reqLenH2N);
            // send the length value
            ns.Write(reqLenArray, 0, sizeof(int));
            // send the string array
            ns.Write(dataArray, 0, reqLen);
            // flush the stream
            ns.Flush();
        }
    }
}