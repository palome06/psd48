using PSD.Base;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using Algo = PSD.Base.Utils.Algo;
using System.Collections.Concurrent;

namespace PSD.ClientAo.VW
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
        public const int MSG_SIZE = 4096;
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
                Base.VW.WHelper.SentByteLine(tcpStream, "C2CO," + Uid + "," + name + "," + avatar + "," + hopeTeam);
            else
                Base.VW.WHelper.SentByteLine(tcpStream, "C2QI," + Uid + "," + name);
            while (true)
            {
                string line = Base.VW.WHelper.ReadByteLine(tcpStream);
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
                        delegate (Exception e) { Log.Logg(e.ToString()); }));
                    recvThread.Start();
                    if (!watch)
                    {
                        // Base.VW.WHelper.SentByteLine(tcpStream, "C2ST," + Uid);
                        sendThread = new Thread(() => ZI.SafeExecute(() => KeepOnListenSend(),
                            delegate (Exception e) { Log.Logg(e.ToString()); }));
                        sendThread.Start();
                        Send("C2ST," + Uid, Uid, 0);
                    }
                    return true;
                }
                else if (line.StartsWith("C2SB,"))
                    return false;
                else if (line.StartsWith("C"))
                    return false;
            }
        }
        private bool ConnectDoDirect(TcpClient client, bool watch, Base.VW.IVI vi)
        {
            NetworkStream tcpStream = client.GetStream();
            VW.Cyvi cvi = vi as VW.Cyvi;
            if (!watch)
                Base.VW.WHelper.SentByteLine(tcpStream, "C2CO,0," + name + "," + avatar + "," + hopeTeam);
            else
                Base.VW.WHelper.SentByteLine(tcpStream, "C2QI,0," + name);
            IDictionary<ushort, IchiPlayer> uidict = new Dictionary<ushort, IchiPlayer>();
            while (true)
            {
                string line = Base.VW.WHelper.ReadByteLine(tcpStream);
                if (line.StartsWith("C2CN,") && !watch)
                {
                    ushort ut = ushort.Parse(line.Substring("C2CN,".Length));
                    if (ut == 0)
                        return false;
                    Uid = ut;
                    cvi.SetNick(1, name, avatar);
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
                        cvi.SetNick(1 + uidict.Count, ip.Name, ip.Avatar);
                    }
                }
                else if (line.StartsWith("C2NW,"))
                {
                    int idx = line.IndexOf(',');
                    int jdx = line.IndexOf(',', idx + 1);
                    int kdx = line.LastIndexOf(',');

                    ushort ouid = ushort.Parse(Algo.Substring(line, idx + 1, jdx));
                    IchiPlayer ip = new IchiPlayer()
                    {
                        Name = Algo.Substring(line, jdx + 1, kdx),
                        Avatar = int.Parse(line.Substring(kdx + 1)),
                        Uid = ouid
                    };
                    uidict.Add(ouid, ip);
                    cvi.SetNick(1 + uidict.Count, ip.Name, ip.Avatar);
                    vi.Cout(Uid, "Newcomer: {0}", Algo.Substring(line, jdx + 1, kdx));
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
            Base.VW.WHelper.SentByteLine(tcpStream, "C4CR," + Uid + "," + oldUid + "," + name + "," + passCode);
            while (true)
            {
                string line = Base.VW.WHelper.ReadByteLine(tcpStream);
                if (line.StartsWith("C4CS,"))
                {
                    ushort ut = ushort.Parse(line.Substring("C4CS,".Length));
                    if (ut == 0)
                        return false;
                    else
                    {
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
            byte[] recv = new byte[MSG_SIZE];
            try
            {
                while (true)
                {
                    string line = Base.VW.WHelper.ReadByteLine(stream);
                    if (string.IsNullOrEmpty(line))
                    {
                        visi.ReportConnectionLost(); break;
                    }
                    if (line.StartsWith("Y"))
                        msgTalk.Add(line);
                    else
                        msgNPools.Add(line);
                }
            }
            catch (IOException)
            {
                visi.ReportConnectionLost();
            }
        }
        private void KeepOnListenSend()
        {
            try
            {
                while (true)
                    Base.VW.WHelper.SentByteLine(stream, msg0Pools.Take());
            }
            catch (IOException)
            {
                visi.ReportConnectionLost();
            }
        }

        #endregion Connect Issue

        /// <summary>
        /// msg0Queues: message from $i to 0
        /// msgNQueues: message from 0 to $i
        /// </summary>
        private BlockingCollection<string> msgNPools;
        private BlockingCollection<string> msg0Pools;
        private BlockingCollection<string> msgTalk;

        private XIVisi visi;

        internal ClLog Log { set; get; }
        // direct true : Single Room, RM/NW exists; otherwise, hall
        public Bywi(string serverName, int port, string name,
            int avatar, int hopeTeam, ushort uid, XIVisi visi)
        {
            this.serverName = serverName;
            this.port = port;

            this.name = name;
            this.avatar = avatar;
            this.hopeTeam = hopeTeam;
            if (uid != 0)
                Uid = uid;

            msg0Pools = new BlockingCollection<string>(new ConcurrentQueue<string>());
            msgNPools = new BlockingCollection<string>(new ConcurrentQueue<string>());
            msgTalk = new BlockingCollection<string>(new ConcurrentQueue<string>());

            this.visi = visi;
        }

        #region Implemetation

        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (from == 0)
            {
                string rvDeq = msgNPools.Take();
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
                msg0Pools.Add(msg);
            }
        }
        // Send direct message that won't be caught by RecvInfRecv from $me to 0
        public void SendDirect(string msg, ushort me)
        {
            Send(msg, me, 0);
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

        public void Dispose() { }
        //// Talk text message to others
        //public void Talk(string msg)
        //{
        //    SendDirect("Y1," + msg, Uid);
        //}
        // Hear any text message from others
        public string Hear()
        {
            string talk = msgTalk.Take();
            if (talk != null)
                Log.Logg("<" + talk);
            return talk;
        }
        #endregion Implementation
    }
}
