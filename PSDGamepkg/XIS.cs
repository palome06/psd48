using PSD.Base;
using PSD.ClientZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PSD.Base.Rules;
using System.Net;
using System.IO.Pipes;
using System.IO;

namespace PSD.PSDGamepkg
{
    public partial class XI
    {
        private const int playerCapacity = 6;

        private NamedPipeClientStream ps = null;

        private static string GetValue(string[] args, int index, string hint)
        {
            // args[0] = 0
            if (args != null && args.Length > index)
                return args[index];
            else
            {
                Console.WriteLine("=>" + hint);
                return Console.ReadLine().ToUpper().Trim();
            }
        }

        private void StartRoom(string[] args)
        {
            string netMode = GetValue(args, 1, "联机模式(SF:单机/NT:联网)").Trim().ToUpper();
            if (netMode != "SF") netMode = "NT";
            string sel = GetValue(args, 2, "选人模式(31:三选一/RM:随机/" +
                "BP:禁选/RD:轮选/ZY:昭鹰/CP:协同/SS:北软/CJ:召唤/TC:明暗)").Trim().ToUpper();
            int selCode = RuleCode.CastMode(sel);
            int pkgCode = RuleCode.PKG_ALL;
            if (netMode == "SF")
            {
                //IsGameCompete = false;
                VW.Djwi djwi = new VW.Djwi(playerCapacity, this.Log);
                WI = djwi;
                Board.Garden = new Dictionary<ushort, Player>();
                List<VW.Neayer> neayers = new List<VW.Neayer>();
                string[] xingsen = new string[] { "撞仙", "笑犬", "翼君", "失影", "厌凉", "彰危" };
                while (neayers.Count < playerCapacity)
                {
                    string py = xingsen[neayers.Count];
                    ushort pv = (ushort)(neayers.Count * neayers.Count);
                    VW.Neayer neayer = new VW.Neayer(py, pv);
                    neayers.Add(neayer);
                }

                VI.Init();
                // setup finish, re-newed player's uid
                List<int> uidList = new List<int>(Enumerable.Range(1, playerCapacity));
                uidList.Shuffle();
                for (ushort i = 0; i < neayers.Count; ++i)
                {
                    Player player = new Player(neayers[i].Name, neayers[i].Avatar, (ushort)uidList[i])
                    {
                        Team = (uidList[i] % 2 == 1) ? 1 : 2,
                        IsAlive = true
                    };
                    Board.Garden.Add(player.Uid, player);
                }
                Board.SortGarden();
                Log.Start();

                IDictionary<ushort, PSD.ClientZero.XIClient> xiclients =
                    new Dictionary<ushort, PSD.ClientZero.XIClient>();
                foreach (Player player in Board.Garden.Values)
                {
                    XIClient xc = new XIClient(player.Uid, djwi, VI);
                    xiclients.Add(player.Uid, xc);
                    xc.RunAsync();
                }
                Thread.Sleep(100);
                WI.BCast("H0SD," + string.Join(",", Board.Garden.Values.Select(
                    p => p.Uid + "," + p.AUid + "," + p.Name)));
                foreach (Player player in Board.Garden.Values)
                    VI.Cout(0, "player " + player.Uid + "# belongs to Team " + player.Team + ".");

                isFinished = false;
                pkgCode = RuleCode.PKG_ALL;
            }
            else if (netMode == "NT")
            {
                string teamModeStr = GetValue(args, 3, "请选择是否支持选队(YJ:选队/NJ:不允许选队)");
                bool teamMode = (teamModeStr == "YJ") ? true : false;
                string pkg = GetValue(args, 4, "采用包，求和(1:标包/2:凤鸣玉誓/4:SP/8:TR三世轮回/16:TR云来奇缘)");
                
                if (!int.TryParse(pkg, out pkgCode) || pkgCode <= 0 || pkgCode > RuleCode.PKG_ALL)
                    pkgCode = RuleCode.PKG_ALL;
                //string gameModeStr = GetValue(args, 5, "请选择游戏模式(JY:休闲模式/CT:竞技模式)");
                //IsGameCompete = (gameModeStr == "CT") ? true : false; // true -> CT
                string portStr = GetValue(args, 5, "请输入房间编号(0为默认值)");
                int port;
                if (!int.TryParse(portStr, out port))
                    port = 0;
                port += Base.NetworkCode.DIR_PORT;
                if (port >= 65535 || port < 1024)
                    port = Base.NetworkCode.DIR_PORT;

                IPAddress[] ipHost = Dns.GetHostAddresses(Dns.GetHostName());
                if (ipHost.Length > 0)
                {
                    Console.WriteLine("本机IP合法地址为：");
                    foreach (IPAddress ip in ipHost)
                    {
                        string ipt = ip.ToString();
                        if (ipt.StartsWith("10.") || ipt.StartsWith("127.0.") || ipt.StartsWith("192.168."))
                            Console.WriteLine("    [内网地址]" + ipt);
                        else
                            Console.WriteLine("   " + ipt);
                    }
                }
                else
                    Console.WriteLine("本机目前网卡异常，仅支持本地模式。");

                VW.Aywi aywi = new VW.Aywi(port, Log, HandleYMessage);
                WI = aywi;

                VI.Cout(0, "Wait for others players joining");
                VI.Init();

                Log.Start();
                aywi.TcpListenerStart();
                Board.Garden = aywi.Connect(VI, teamMode, null);
                Board.SortGarden();

                WI.RecvInfStart();
                int count = Board.Garden.Count;
                while (count > 0)
                {
                    Base.VW.Msgs msg = WI.RecvInfRecvPending();
                    if (msg.Msg.StartsWith("C2ST,"))
                        --count;
                }
                WI.RecvInfEnd();
                isFinished = false;
                WI.BCast("H0SD," + string.Join(",", Board.Garden.Values.Select(
                    p => p.Uid + "," + p.AUid + "," + p.Name)));
            }
            Board.RoundIN = "H0PR";
            if (WI is VW.Aywi)
                HoldRoomTunnel();
            SelectHero(selCode, pkgCode);
            Run(pkgCode, selCode == Base.Rules.RuleCode.MODE_00);
        }
        // invs: players' uid
        private void StartRoom(int room, int[] opts, ushort[] invs)
        {
            int port = Base.NetworkCode.HALL_PORT + room;
            string pipeName = "psd48pipe" + room;
            //string piperName = "psd48piper" + room;
            VW.Aywi aywi = new VW.Aywi(port, Log, HandleYMessage);
            WI = aywi;

            bool teamMode = (opts[0] == RuleCode.HOPE_YES);

            VI.Init(); // VI inits here?
            Log.Start();
            aywi.TcpListenerStart();
            ps = new NamedPipeClientStream(pipeName);
            ps.Connect();
            aywi.Ps = ps;
            WriteBytes(ps, "C3RD," + room);
            //sw = new StreamWriter(ps);
            //aywi.Sw = sw;
            //sw.Write("C3RD," + room);
            //using (var pipeStream = new System.IO.Pipes.NamedPipeClientStream(piperName))
            //{
            //    pipeStream.Connect();
            //    using (var sw = new System.IO.StreamWriter(pipeStream))
            //    {
            //        sw.Write("C3RD," + room);
            //    }
            //} // :[
            Board.Garden = aywi.Connect(VI, teamMode, invs.ToList());
            Board.SortGarden();

            WI.RecvInfStart();
            int count = Board.Garden.Count;
            while (count > 0)
            {
                Base.VW.Msgs msg = WI.RecvInfRecvPending();
                Console.WriteLine("We received message : " + msg.Msg);
                if (msg.Msg.StartsWith("C2ST,"))
                    --count;
            }
            WI.RecvInfEnd();
            isFinished = false;
            WI.BCast("H0SD," + string.Join(",", Board.Garden.Values.Select(
                p => p.Uid + "," + p.AUid + "," + p.Name)));
            Board.RoundIN = "H0PR";
            if (WI is VW.Aywi)
                HoldRoomTunnel();
            SelectHero(opts[1], opts[2]);
            Run(opts[2], opts[1] == Base.Rules.RuleCode.MODE_00);
        }
        
		private void HandleHoldOfReconnect(ushort wuid) {
			VW.Aywi aywi = WI as VW.Aywi;
			VI.Cout(0, "{0}#玩家恢复连接。", wuid);
            // If detected all recovered, BCase H0RK,0
            string h09m = "H09M," + PCS.Groups;
            WI.Send(h09m, 0, wuid);
            if (Board.RoundIN != "H0PR")
            {
                string h09n = "H09N," + string.Join(",",
                    Board.Garden.Values.Select(p => p.Uid + "," + p.Name));
                WI.Send(h09n, 0, wuid);
                WI.Send("H09G," + Board.GenerateSerialGamerMessage(), 0, wuid);
                string h09p = Board.GenerateSerialFieldMessage();
                WI.Send("H09P," + h09p + "," + string.Join(",",
                    CalculatePetsScore().Select(p => p.Key + "," + p.Value)), 0, wuid);
                // TODO: remove the score field, calculate on the demand
                WI.Send("H09F," + Board.GeneratePrivateMessage(wuid), 0, wuid);
            }
            HandleHoldOfWatcher(wuid);
            // TODO: needs private data (e.g. Tux) in such connection
		}
		private void HandleHoldOfWatcher(ushort wuid) {
			VW.Aywi aywi = WI as VW.Aywi;
            WI.Send("H0SM," + SelCode + "," + PCS.Groups, 0, wuid);
            if (Board.RoundIN != "H0PR")
            {
                string h09n = "H09N," + string.Join(",",
                    Board.Garden.Values.Select(p => p.Uid + "," + p.Name));
                WI.Send(h09n, 0, wuid);
                WI.Send("H09G," + Board.GenerateSerialGamerMessage(), 0, wuid);
                string h09p = Board.GenerateSerialFieldMessage();
                WI.Send("H09P," + h09p + "," + string.Join(",",
                    CalculatePetsScore().Select(p => p.Key + "," + p.Value)), 0, wuid);
                // TODO: remove the score field, calculate on the demand
            }
            else
            {
                WI.Send("H0SD," + string.Join(",", Board.Garden.Values.Select(
                    p => p.Uid + "," + p.AUid + "," + p.Name)), 0, wuid);
                if (Casting != null)
                {
                    WI.BCast("H0SM," + SelCode + "," + PCS.Groups);
                    if (Casting is Base.Rules.CastingPick)
                    {
                        WI.Send("H0RT,0", 0, wuid);
                        WI.Send("H09I,1", 0, wuid);
                    }
                    else if (Casting is Base.Rules.CastingTable)
                    {
                        WI.Send("H0SL," + string.Join(",", Board.Garden.Values.Select(
                            p => p.Uid + "," + p.SelectHero)), 0, wuid);
                        WI.Send("H09I,2", 0, wuid);
                        var ct = Casting as Base.Rules.CastingTable;
                        WI.Send("H0TT," + ct.ToMessage(), 0, wuid);
                    }
                    else if (Casting is Base.Rules.CastingPublic)
                    {
                        WI.Send("H0SL," + string.Join(",", Board.Garden.Values.Select(
                            p => p.Uid + "," + p.SelectHero)), 0, wuid);
                        WI.Send("H09I,3", 0, wuid);
                        var cp = Casting as Base.Rules.CastingPublic;
                        WI.Send("H0PT," + cp.ToMessage(), 0, wuid);
                    }
                    else if (Casting is Base.Rules.CastingCongress)
                    {
                        WI.Send("H09I,4", 0, wuid);
                        var cc = Casting as Base.Rules.CastingCongress;
                        //if (cc.Viewable)
                        WI.Send("H0CI," + cc.ToMessage(true, true), 0, wuid);
                    }
                }
            }
		}
        private void HoldRoomTunnel()
        {
            new Thread(() => Util.SafeExecute(() =>
            {
                while (true)
                {
                    ushort wuid = (WI as VW.Aywi).CatchNewRoomComer();
                    if (wuid > 1000) // Watcher case
						HandleHoldOfWatcher(wuid);
                    else // Reconnection case
						HandleHoldOfReconnect(wuid);
                }
            }, delegate(Exception e) { Log.Logger(e.ToString()); })).Start();
        }
        #region Stream Utils
        private static string ReadBytes(NamedPipeClientStream ps)
        {
            if (ps != null)
            {
                byte[] byte2 = new byte[4096];
                int readCount = ps.Read(byte2, 0, 4096);
                if (readCount > 0)
                    return Encoding.Unicode.GetString(byte2, 0, readCount);
                else
                    return "";
            }
            else
                return "";
        }
        private static void WriteBytes(NamedPipeClientStream ps, string value)
        {
            if (ps != null)
            {
                byte[] byte2 = Encoding.Unicode.GetBytes(value);
                if (byte2.Length > 0)
                    ps.Write(byte2, 0, byte2.Length);
                ps.Flush();
            }
        }
        #endregion Stream Utils
    }
}
