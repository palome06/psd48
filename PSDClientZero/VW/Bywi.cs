using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using PSD.Base;
using System.IO;

namespace PSD.ClientZero.VW
{
    // Server In it
    public class Bywi : Base.VW.IWICL
    {
        internal class IchiPlayer
        {
            internal string Name { set; get; }
            internal int Avatar { set; get; }
            internal ushort Uid { set; get; }
        }

        public Thread recvThread, sendThread;

        private string serverName;
        private int port;

        private int avatar;
        private string name;
        private int hopeTeam;
        public ushort Uid { private set; get; }

        private NetworkStream stream;

        #region Connect Issue
        private bool ConnectDo(TcpClient client, bool watch)
        {
            NetworkStream tcpStream = client.GetStream();
            if (!watch)
                SentByteLine(tcpStream, "C2CO," + Uid + "," + name + "," + avatar + "," + hopeTeam);
            else
                SentByteLine(tcpStream, "C2QI," + Uid + "," + name);
            while (true)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("C2CN,") && !watch)
                {
                    ushort ut = ushort.Parse(line.Substring("C2CN,".Length));
                    if (ut == 0)
                        return false;
                }
                else if (line.StartsWith("C2QJ,") && watch)
                {
                    ushort ut = ushort.Parse(line.Substring("C2QJ,".Length));
                    if (ut == 0)
                        return false;
                    Uid = ut;
                }
                else if (line.StartsWith("C2SA,"))
                {
                    stream = tcpStream;
                    recvThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenRecv(),
                        delegate(Exception e) { Log.Logg(e.ToString()); }));
                    recvThread.Start();
                    if (!watch)
                    {
                    //    SentByteLine(tcpStream, "C2ST," + Uid);
                        sendThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenSend(),
                            delegate(Exception e) { Log.Logg(e.ToString()); }));
                        sendThread.Start();
                        Send("C2ST," + Uid, Uid, 0);
                    }
                    return true;
                }
                else if (line.StartsWith("C"))
                    return false;
            }
        }
        private bool ConnectDoDirect(TcpClient client, bool watch, Base.VW.IVI vi)
        {
            NetworkStream tcpStream = client.GetStream();
            if (!watch)
                SentByteLine(tcpStream, "C2CO,0," + name + "," + avatar + "," + hopeTeam);
            else
                SentByteLine(tcpStream, "C2QI,0," + name);
            IDictionary<ushort, IchiPlayer> uidict = new Dictionary<ushort, IchiPlayer>();
            while (true)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("C2CN,") && !watch)
                {
                    ushort ut = ushort.Parse(line.Substring("C2CN,".Length));
                    if (ut == 0)
                        return false;
                    Uid = ut;
                }
                else if (line.StartsWith("C2QJ,") && watch)
                {
                    ushort ut = ushort.Parse(line.Substring("C2QJ,".Length));
                    if (ut == 0)
                        return false;
                    Uid = ut;
                }
                else if (line.StartsWith("C2RM,"))
                {
                    string[] blocks = line.Split(',');
                    for (int i = 1; i < blocks.Length; i += 3)
                    {
                        ushort ouid = ushort.Parse(blocks[i]);
                        IchiPlayer ip = new IchiPlayer()
                        {
                            Name = blocks[i + 1],
                            Avatar = int.Parse(blocks[i + 2]),
                            Uid = ouid
                        };
                        uidict.Add(ouid, ip);
                    }
                    vi.Cout(Uid, "Current other gamers: {0}", string.Join(",",
                        uidict.Values.Select(p => p.Name)));
                }
                else if (line.StartsWith("C2NW,"))
                {
                    int idx = line.IndexOf(',');
                    int jdx = line.IndexOf(',', idx + 1);
                    int kdx = line.LastIndexOf(',');

                    ushort ouid = ushort.Parse(Base.Utils.Algo.Substring(line, idx + 1, jdx));
                    IchiPlayer ip = new IchiPlayer()
                    {
                        Name = Base.Utils.Algo.Substring(line, jdx + 1, kdx),
                        Avatar = int.Parse(line.Substring(kdx + 1)),
                        Uid = ouid
                    };
                    uidict.Add(ouid, ip);
                    vi.Cout(Uid, "Newcomer: {0}", Base.Utils.Algo.Substring(line, jdx + 1, kdx));
                }
                else if (line.StartsWith("C2SA,"))
                {
                    stream = tcpStream;
                    recvThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenRecv(),
                        delegate(Exception e) { Log.Logg(e.ToString()); }));
                    recvThread.Start();
                    if (!watch)
                    {
                        sendThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenSend(),
                            delegate(Exception e) { Log.Logg(e.ToString()); }));
                        sendThread.Start();
                    }
                    return true;
                }
                else if (line.StartsWith("C"))
                    return false;
            }
        }
        // Do Reconnection
        private bool ConnectDoResume(TcpClient client, ushort oldUid, string passCode)
        {
            NetworkStream tcpStream = client.GetStream();
            SentByteLine(tcpStream, "C4CR," + Uid + "," + oldUid + "," + name + "," + passCode);
            while (true)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("C4CS,"))
                {
                    ushort ut = ushort.Parse(line.Substring("C4CS,".Length));
                    if (ut == 0)
                        return false;
                    else {
                    	Uid = ut;
                    	stream = tcpStream;
	                    recvThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenRecv(),
	                        delegate(Exception e) { Log.Logg(e.ToString()); }));
	                    recvThread.Start();
                        sendThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenSend(),
                            delegate(Exception e) { Log.Logg(e.ToString()); }));
                        sendThread.Start();
                    	return true;
                    }
                }
                else if (line.StartsWith("C"))
                    return false;
            }
        }
        public bool StartConnect(bool watch)
        {
            try
            {
                TcpClient client = new TcpClient(serverName, port);
                return ConnectDo(client, watch);
            }
            catch (Exception) { return false; }
        }
        // return whether success or not.
        public bool StartConnectDirect(bool watch, Base.VW.IVI vi)
        {
            try
            {
                TcpClient client = new TcpClient(serverName, port);
                return ConnectDoDirect(client, watch, vi);
            }
            catch (Exception) { return false; }
        }
        public bool StartConnectResume(ushort oldUid, string passCode)
        {
            try
            {
                TcpClient client = new TcpClient(serverName, port);
                return ConnectDoResume(client, oldUid, passCode);
            }
            catch (Exception) { return false; }
        }
		
        private void KeepOnListenRecv()
        {
            byte[] recv = new byte[2048];
            try
            {
                while (true)
                {
                    string line = ReadByteLine(stream);
                    //Console.WriteLine("=============>>>> Do Receive:" + line);
                    if (line.StartsWith("\\D"))
                    {
                        line = line.Substring("\\D".Length);
                        msgNQueues.Enqueue(line);
                    }
                    else if (line.StartsWith("\\I"))
                    {
                        line = line.Substring("\\I".Length);
                        if (line.StartsWith("Y"))
                            msgTalk.Enqueue(line);
                        else
                            msgNQueues.Enqueue(line);
                    }
                    else
                        Thread.Sleep(100);
                }
            }
            catch (IOException)
            {
                xic.ReportConnectionLost();
            }
        }
        private void KeepOnListenSend()
        {
            try
            {
                while (true)
                {
                    if (msg0Queues.Count > 0)
                    {
                        string msgs = msg0Queues.Dequeue();
                        //Base.VW.Msgs msg = new Base.VW.Msgs(msgs, Uid, 0);
                        //byte[] sent = Encoding.Unicode.GetBytes(msg.Msg);
                        //stream.Write(sent, 0, sent.Length);
                        //StreamWriter sw = new StreamWriter(stream); //Encoding.Unicode
                        //sw.WriteLine(msgs);
                        //sw.Flush();
                        SentByteLine(stream, msgs);
                        //Thread.Sleep(20);
                    }
                    else
                        Thread.Sleep(100);
                }
            }
            catch (IOException)
            {
                xic.ReportConnectionLost();
            }
        }

        #endregion Connect Issue

        /// <summary>
        /// msg0Queues: message from $i to 0
        /// msgNQueues: message from 0 to $i
        /// </summary>
        private Queue<string> msgNQueues;
        private Queue<string> msg0Queues;
        private Queue<string> msgTalk;

        private XIClient xic;

        internal Log Log { set; get; }
        // direct true : Single Room, RM/NW exists; otherwise, hall
        public Bywi(string serverName, int port, string name,
            int avatar, int hopeTeam, ushort uid, XIClient xic)
        {
            this.serverName = serverName;
            this.port = port;

            this.name = name;
            this.avatar = avatar;
            this.hopeTeam = hopeTeam;
            if (uid != 0)
                Uid = uid;

            msg0Queues = new Queue<string>();
            msgNQueues = new Queue<string>();
            msgTalk = new Queue<string>();

            this.xic = xic;
        }

        #region Implemetation

        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (from == 0)
            {
                bool valid = false;
                string rvDeq = null;
                while (!valid)
                {
                    lock (msgNQueues)
                    {
                        valid = msgNQueues.Count != 0;
                        if (valid)
                        {
                            rvDeq = msgNQueues.Dequeue();
                            break;
                        }
                    }
                    Thread.Sleep(100);
                }
                if (rvDeq != null)
                    Log.Logg("<" + rvDeq);
                return rvDeq;
            }
            else
                return null;
        }
        // Send raw message from $me to $to
        public void Send(string msg, ushort me, ushort to)
        {
            if (to == 0)
            {
                if (Log != null)
                    Log.Logg(">" + msg);
                lock (msg0Queues)
                {
                    msg0Queues.Enqueue("\\I" + msg);
                }
            }
        }
        // Send direct message that won't be caught by RecvInfRecv from $me to 0
        public void SendDirect(string msg, ushort me)
        {
            if (Log != null)
                Log.Logg(">" + msg);
            lock (msg0Queues)
            {
                msg0Queues.Enqueue("\\D" + msg);
            }
        }
        // Close the socket for recycling
        public void Close()
        {
            try
            {
                if (recvThread != null && recvThread.IsAlive)
                    recvThread.Abort();
                if (sendThread != null && sendThread.IsAlive)
                    sendThread.Abort();
                stream.Close();
            }
            catch (IOException) { }
        }
        //// Talk text message to others
        //public void Talk(string msg)
        //{
        //    SendDirect("Y1," + msg, Uid);
        //}
        // Hear any text message from others
        public string Hear()
        {
            string talk = null;
            while (true)
            {
                lock (msgTalk)
                {
                    if (msgTalk.Count > 0)
                    {
                        talk = msgTalk.Dequeue();
                        break;
                    }
                }
                Thread.Sleep(100);
            }
            if (talk != null)
                Log.Logg("<" + talk);
            return talk;
        }
        #endregion Implementation

        private static string ReadByteLine(NetworkStream ns)
        {
            byte[] byte2 = new byte[2];
            ns.Read(byte2, 0, 2);
            ushort value = (ushort)((byte2[0] << 8) + byte2[1]);
            byte[] actual = new byte[2048];
            if (value > 2048)
                value = 2048;
            ns.Read(actual, 0, value);
            return Encoding.Unicode.GetString(actual, 0, value);
        }

        private static void SentByteLine(NetworkStream ns, string value)
        {
            byte[] actual = Encoding.Unicode.GetBytes(value);
            int al = actual.Length;
            byte[] buf = new byte[al + 2];
            buf[0] = (byte)(al >> 8);
            buf[1] = (byte)(al & 0xFF);
            actual.CopyTo(buf, 2);
            ns.Write(buf, 0, al + 2);
            ns.Flush();
        }
    }
}
