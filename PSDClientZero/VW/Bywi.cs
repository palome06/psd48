using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PSD.Base;

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
        public const int MSG_SIZE = 4096;
        // token to terminate all running thread when Aywi is closed
        private CancellationTokenSource ctoken;

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
                if (line == null)
                    return false;
                else if (line.StartsWith("C2CN,") && !watch)
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
                    StartListenTask(KeepOnListenRecv);
                    if (!watch)
                    {
                        StartListenTask(KeepOnListenSend);
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
            if (!watch)
                Base.VW.WHelper.SentByteLine(tcpStream, "C2CO,0," + name + "," + avatar + "," + hopeTeam);
            else
                Base.VW.WHelper.SentByteLine(tcpStream, "C2QI,0," + name);
            IDictionary<ushort, IchiPlayer> uidict = new Dictionary<ushort, IchiPlayer>();
            while (true)
            {
                string line = Base.VW.WHelper.ReadByteLine(tcpStream);
                if (line == null)
                    return false;
                else if (line.StartsWith("C2CN,") && !watch)
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
                    StartListenTask(KeepOnListenRecv);
                    if (!watch)
                        StartListenTask(KeepOnListenSend);
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
                if (line == null)
                    return false;
                else if (line.StartsWith("C4CS,"))
                {
                    ushort ut = ushort.Parse(line.Substring("C4CS,".Length));
                    if (ut == 0)
                        return false;
                    Uid = ut;
                    stream = tcpStream;
                    StartListenTask(KeepOnListenRecv);
                    StartListenTask(KeepOnListenSend);
                    return true;
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
            try
            {
                while (true)
                {
                    string line = Base.VW.WHelper.ReadByteLine(stream);
                    if (string.IsNullOrEmpty(line))
                    {
                        if (OnLoseConnection != null)
                            OnLoseConnection();
                        stream.Close();
                        break;
                    }
                    else if (line.StartsWith("Y"))
                        msgTalk.Add(line, ctoken.Token);
                    else
                        msgNPools.Add(line, ctoken.Token);
                }
            }
            catch (IOException)
            {
                if (OnLoseConnection != null)
                    OnLoseConnection();
            }
        }
        private void KeepOnListenSend()
        {
            try
            {
                while (true)
                    Base.VW.WHelper.SentByteLine(stream, msg0Pools.Take(ctoken.Token));
            }
            catch (IOException)
            {
                if (OnLoseConnection != null)
                    OnLoseConnection();
            }
        }

        #endregion Connect Issue

        /// <summary>
        /// msgNPools: message from 0 to $i
        /// </summary>
        private BlockingCollection<string> msgNPools;
        /// <summary>
        /// msg0Pools: message from $i to 0
        /// </summary>
        private BlockingCollection<string> msg0Pools;
        /// <summary>
        /// msgTalk: talk and instant message
        /// </summary>
        private BlockingCollection<string> msgTalk;

        public delegate void LoseConnectionDelegate();
        public LoseConnectionDelegate OnLoseConnection;

        internal ClLog Log { set; get; }
        // direct true : Single Room, RM/NW exists; otherwise, hall
        public Bywi(string serverName, int port, string name,
            int avatar, int hopeTeam, ushort uid)
        {
            this.serverName = serverName;
            this.port = port;

            this.name = name;
            this.avatar = avatar;
            this.hopeTeam = hopeTeam;
            if (uid != 0)
                Uid = uid;

            msgNPools = new BlockingCollection<string>(new ConcurrentQueue<string>());
            msg0Pools = new BlockingCollection<string>(new ConcurrentQueue<string>());
            msgTalk = new BlockingCollection<string>(new ConcurrentQueue<string>());
            ctoken = new CancellationTokenSource();
        }

        #region Implemetation

        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (from == 0)
            {
                string rvDeq = msgNPools.Take();
                if (rvDeq != null)
                    Log.Logg("<" + rvDeq);
                return rvDeq;
            }
            else return null;
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
        // Close the socket for recycling
        public void Shutdown()
        {
            ctoken.Cancel(); ctoken.Dispose();
            msg0Pools.Dispose(); msgNPools.Dispose(); msgTalk.Dispose();
            try { stream.Close(); }
            catch (IOException) { }
        }
        // Hear any text message from others
        public string Hear()
        {
            string talk = msgTalk.Take();
            if (talk != null)
                Log.Logg("<" + talk);
            return talk;
        }
        public void Dispose() { }
        #endregion Implementation

        /// <summary>
        /// start an async Listening task
        /// </summary>
        /// <param name="action">the acutal listen action</param>
        private void StartListenTask(Action action)
        {
            Action<Exception> ae = (e) => { if (Log != null) Log.Logg(e.ToString()); };
            Task.Factory.StartNew(() => ZI.SafeExecute(action, ae), ctoken.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
