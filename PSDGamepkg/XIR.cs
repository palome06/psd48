using PSD.Base;
using PSD.Base.Card;
using PSD.ClientZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg
{
    public partial class XI
    {
        private bool isFinished;

        private List<Thread> listOfThreads;

        private string jumpTareget, jumpEnd;

        private Random randomSeed;

        //private bool IsGameCompete { set; get; }

        #region DebugCondition
        private void DebugCondition()
        {
            //RaiseGMessage("G0HQ,1,0,1,10,56,37,4,47,48,49,50,51,52,53,54,55,12");
            //RaiseGMessage("G0HQ,2,0,1,40,42,38,14,11");
            //RaiseGMessage("G0HQ,3,0,1,3");
            //RaiseGMessage("G0HQ,2,0,1,44");
            //RaiseGMessage("G0HQ,1,0,1,10,11,12,52,36,4,1,33");
            //RaiseGMessage("G0HQ,2,0,1,48,34,7");
            //RaiseGMessage("G0HQ,3,0,1,34");
            //RaiseGMessage("G0HQ,1,0,1,54,49");
            //RaiseGMessage("G0HQ,2,3,0,44,8,16");
            //RaiseGMessage("G0HQ,2,6,0,28,42,4");
            //RaiseGMessage("G0HQ,2,5,0,24,31,45");
            //RaiseGMessage("G0HQ,2,1,0,47,48");//7,1,13
            //RaiseGMessage("G0HQ,2,2,0,15,32");//54
            //RaiseGMessage("G0HQ,2,4,0,51,26");//33
            //RaiseGMessage("G0HQ,2,1,0,0,17");

            //RaiseGMessage("G0HQ,2,2,0,0,88,44");
            //RaiseGMessage("G0HQ,2,5,0,0,77");
            //RaiseGMessage("G0IJ,5,0,4");
            //RaiseGMessage("G0IX,5,0,2");
            //garden[2].Weapon = 51;
            //garden[1].Pets[3] = 16;
            //garden[1].Pets[4] = 20;
            //garden[2].Pets[0] = 4;
            //Board.MonPiles.PushBack(13);
            //Board.MonPiles.PushBack(1011);
            //Board.MonPiles.PushBack(8);
            //RaiseGMessage("G0HC,0,1,19");
            //Board.MonPiles.PushBack(1022);
            //Board.MonPiles.PushBack(1041);
            //Board.MonPiles.PushBack(57);
            //Board.MonPiles.PushBack(1059);
            //Board.MonPiles.PushBack(1062);
            //Board.MonPiles.PushBack(2);
            //Board.MonPiles.PushBack(1);
            //Board.MonPiles.PushBack(17);
            //Board.MonPiles.PushBack(14);
            //Board.MonPiles.PushBack(8);
            ////Board.MonPiles.PushBack(24);
            ////Board.MonPiles.PushBack(25);
            ////Board.MonPiles.PushBack(24);
            ////Board.MonPiles.PushBack(23);
            ////Board.MonPiles.PushBack(22);
            //Board.MonPiles.PushBack(1);
            //Board.MonPiles.PushBack(1022);
            ////Board.MonPiles.PushBack(1030);
            //Board.EvePiles.PushBack(29);
            ////Board.EvePiles.PushBack(1);
            //Board.EvePiles.PushBack(32);
            //Board.EvePiles.PushBack(36);
            //Board.EvePiles.PushBack(33);
            //Board.EvePiles.PushBack(35);
            //Board.RestNPCPiles.PushBack(1047);
            //Board.EvePiles.PushBack(23);
            //while (Board.MonPiles.Count > 0)
            //    Board.MonPiles.Dequeue();
            //Board.MonPiles.PushBack(28);
            //Board.MonPiles.PushBack(1019);
            //Board.MonPiles.PushBack(1022);
            //Board.MonPiles.PushBack(7);
            //Board.MonPiles.PushBack(1015);
            //Board.MonPiles.PushBack(28);
            //Board.MonPiles.PushBack(1013);
            //Board.MonPiles.PushBack(1002);
            //Board.MonPiles.PushBack(4);
            //Board.MonPiles.PushBack(1001);
            //Board.MonPiles.PushBack(15);
            //Board.MonPiles.PushBack(8);
            //Board.MonPiles.PushBack(1003);
            //Board.MonPiles.PushBack(7);
            //Board.MonPiles.PushBack(1030);
            //Board.MonPiles.PushBack(23);
            //Board.MonPiles.PushBack(19);
            //Board.MonPiles.PushBack(2);
            //Board.MonPiles.PushBack(37);
            //Board.MonPiles.PushBack(1003);
            //List<ushort> mons = new List<ushort>()
            //{
            //    31,32,33,34,35,36,37,38,39,40
            //};
            //mons.Shuffle();
            //foreach (ushort mon in mons)
            //    Board.MonPiles.PushBack(mons);

            //for (int i = 0; i < 28; ++i)
            //    Board.MonPiles.Dequeue();
            //Board.MonPiles.PushBack(1001);
            //Board.MonPiles.PushBack(29);
            //Board.MonPiles.PushBack(54);
            //Board.MonPiles.PushBack(33);
            //Board.MonPiles.PushBack(4);
            //Board.MonPiles.PushBack(10);
            //Board.MonPiles.PushBack(8);
            //Board.MonPiles.PushBack(1045);
            //Board.MonPiles.PushBack(5);
            //Board.MonPiles.PushBack(1040);
            //Board.MonPiles.PushBack(9);
            //Board.MonPiles.PushBack(1031);
            //Board.MonPiles.PushBack(1);
            //Board.MonPiles.PushBack(1007);
            //RaiseGMessage("G0HQ,2,1,0,52,10,11");
            //RaiseGMessage("G0HQ,2,1,0,47,49,10,11,12");
            //Board.MonPiles.PushBack(17);
            //RaiseGMessage("G0IJ,1,0,3");
            //Board.MonPiles.PushBack(1016);
            //Board.MonPiles.PushBack(6);
            //Board.TuxPiles.Dequeue(30); // 56 - 18 = 38
            //RaiseGMessage("G0IJ,6,0,1");
            //RaiseGMessage("G0HQ,2,1,0,10,47,53,6");
            //RaiseGMessage("G0HQ,2,3,0,20,26");
            //RaiseGMessage("G0HQ,2,1,0,0,49,11,10");
            //Board.RestNPCPiles.PushBack(1057);
            //RaiseGMessage("G0HQ,2,1,0,0,17,70");
            //RaiseGMessage("G0HQ,2,2,0,0,37,70");
            //RaiseGMessage("G0HQ,2,3,0,0,40");
            //RaiseGMessage("G0HQ,2,1,0,0,77,95,9");
            //RaiseGMessage("G0HQ,2,2,0,0,4");
            //RaiseGMessage("G0HQ,2,1,0,0,36,37");
            //RaiseGMessage("G0HQ,2,6,0,0,88");
            //RaiseGMessage("G0HQ,2,1,0,47,50,49,5,63,8,69");
            //RaiseGMessage("G0HQ,2,1,0,0,10,70,72");
            //RaiseGMessage("G0HQ,2,1,0,0,48,49,95");
            //RaiseGMessage("G0HQ,2,3,0,0,92");
            //RaiseGMessage("G0HQ,2,5,0,0,90");
            //RaiseGMessage("G0HQ,2,4,0,0,47,48,52");
            //RaiseGMessage("G0HQ,2,6,0,0,26");
            RaiseGMessage("G0HQ,2,1,0,0,124,101");
            //RaiseGMessage("G0HQ,2,2,0,0,71");
            //RaiseGMessage("G0HQ,2,1,0,61,64,73,74,75,76,65,17,69,71,10,70");
            //RaiseGMessage("G0IJ,3,0,1");
            //RaiseGMessage("G0IJ,3,0,1");
            //RaiseGMessage("G0IX,5,0,1");
            //RaiseGMessage("G0HQ,2,1,0,0,93,94,9,18,96,5,2,71,72,49,60,11");
            //RaiseGMessage("G0HQ,2,1,0,0,1,18");
            //RaiseGMessage("G0HQ,2,4,0,0,33,35");
            //RaiseGMessage("G0HQ,2,3,0,0,92");
            //RaiseGMessage("G0HQ,2,4,0,0,89");
            //RaiseGMessage("G0HQ,2,2,0,0,96,59");
            //RaiseGMessage("G0HQ,2,1,0,0,96,18");
            //RaiseGMessage("G0HQ,2,5,0,0,65,66");
            //RaiseGMessage("G0HQ,2,1,0,0,71,37,95");
            //RaiseGMessage("G0HQ,2,1,0,0,84");
            //RaiseGMessage("G0HQ,2,1,0,0,55,73,95,1,5");
            //RaiseGMessage("G0HQ,2,2,0,0,60");
            //RaiseGMessage("G0HQ,2,3,0,0,66");
            //RaiseGMessage("G0HQ,2,2,0,0,96");
            //RaiseGMessage("G0HQ,2,2,0,0,90,34,89,88,95");
            //RaiseGMessage("G0HQ,2,1,0,0,95,88,10");
            //RaiseGMessage("G0HQ,2,1,0,0,10,11,12");
            //RaiseGMessage("G0HQ,2,1,0,10,38,39");
            //RaiseGMessage("G0HQ,2,1,0,1,47,48,49,51,52");
            //RaiseGMessage("G0HQ,2,1,0,71,72,10,79,8");
            //RaiseGMessage("G0HQ,2,2,0,55,84");
            //RaiseGMessage("G0HQ,2,3,0,49");
            //RaiseGMessage("G0HQ,2,4,0,50,32,1,2");
            //RaiseGMessage("G0HQ,2,5,0,51");
            //RaiseGMessage("G0HQ,2,6,0,0,52");
            //RaiseGMessage("G0HQ,2,1,0,51,37,10,53,11,40,16,18,25");
            //RaiseGMessage("G0HQ,2,2,0,48,49,13,14,34");
            //RaiseGMessage("G0HQ,2,3,0,1,52");
            //RaiseGMessage("G0HQ,2,4,0,2,4,5,6");
            //RaiseGMessage("G0HQ,2,1,0,10,34,50,40");
            //RaiseGMessage("G0HQ,2,2,0,42,6,3,49");
            //RaiseGMessage("G0HQ,2,3,0,23,41,20");
            //RaiseGMessage("G0HQ,2,4,0,16,19,43");
            //RaiseGMessage("G0HQ,2,5,0,37,5,27");
            //RaiseGMessage("G0HQ,2,6,0,32,46,53");
            //Board.EvePiles.PushBack(3);
            //Board.EvePiles.PushBack(1);
            //Board.RestNPCPiles.PushBack(1001);
            //RaiseGMessage("G0HQ,2,3,0,50");
            //RaiseGMessage("G0ZB,3,0,50");
            //RaiseGMessage("G0HQ,2,1,0,1,50,49");
            //RaiseGMessage("G0HQ,2,2,0,44,51");
            //RaiseGMessage("G0HQ,2,1,0,16,43");
            //Board.MonPiles.Dequeue(30);
            //Board.MonPiles.PushBack(1009);
            //RaiseGMessage("G0ZB,6,0,47");
            //RaiseGMessage("G0ZB,2,0,55");
            //RaiseGMessage("G0ZB,1,0,73");
            //RaiseGMessage("G0ZB,1,0,74");
            //RaiseGMessage("G0ZB,4,0,52");
            //RaiseGMessage("G0ZB,5,0,54");
            //RaiseGMessage("G0ZB,2,0,49");
            //RaiseGMessage("G0ZB,3,0,52");
            //RaiseGMessage("G0ZB,2,0,55");
            //RaiseGMessage("G0ZB,2,0,51");
            //RaiseGMessage("G0HQ,2,1,0,49");
            //RaiseGMessage("G0HQ,2,3,0,19,73");
            //RaiseGMessage("G0HQ,2,4,0,47,48,52");
            //RaiseGMessage("G0ZB,1,0,49");
            //RaiseGMessage("G0ZB,3,0,73");
            //RaiseGMessage("G0ZB,4,0,47");
            //RaiseGMessage("G0ZB,4,0,48");
            //RaiseGMessage("G0ZB,4,0,52");
            //RaiseGMessage("G0HD,1,5,0,5");
            //RaiseGMessage("G0HD,1,5,0,17");
            //RaiseGMessage("G0HD,1,1,0,16");
            //RaiseGMessage("G0HD,1,1,0,30");
            //RaiseGMessage("G0HD,1,5,0,36");
            //RaiseGMessage("G0HD,1,6,0,16");
            //RaiseGMessage("G0HD,1,2,0,2");
            //RaiseGMessage("G0HD,1,2,0,31");
            //RaiseGMessage("G0HD,1,3,0,7");
            //RaiseGMessage("G0HD,1,3,0,12");
            //RaiseGMessage("G0HD,1,5,0,22");
            //RaiseGMessage("G0HD,1,4,0,19");
            //RaiseGMessage("G0HD,1,6,0,9");
            //RaiseGMessage("G0HD,1,5,0,15");
            //RaiseGMessage("G0HQ,1,0,1,16,5,44,50");
            //RaiseGMessage("G0HQ,4,0,1,43");
            //RaiseGMessage("G0OH,1,0,0,5,2,0,0,4,3,0,0,5,4,0,0,5,5,0,0,5,6,0,0,5");
            //RaiseGMessage("G0OH,2,0,0,2,3,0,0,3,4,0,0,5,6,0,0,2");
            //RaiseGMessage("G0OH,1,0,4,12,2,0,4,12");
            foreach (Player player in Board.Garden.Values)
                RaiseGMessage("G0HQ,2," + player.Uid + ",1,3");
            //RaiseGMessage("G0HQ,2,4,1,1");
            //RaiseGMessage("G0HQ,2,6,1,2");
            //RaiseGMessage("G0HQ,2,2,1,3");
            //RaiseGMessage("G0HQ,2,3,1,3");
            //RaiseGMessage("G0HQ,2,4,1,3");
            //RaiseGMessage("G0HQ,2,5,1,3");
            //RaiseGMessage("G0HQ,2,6,1,3");
        }
        #endregion DebugCondition

        public void Run(int levelCode, bool inDebug)
        {
            var garden = Board.Garden;
            LastUVs = new Dictionary<ushort, string>();
            ConstructPiles(levelCode);
            WI.BCast(string.Format("H0DP,{0},{1},{2}",
                Board.TuxPiles.Count, Board.MonPiles.Count, Board.EvePiles.Count));
            tx01 = new JNS.TuxCottage(this, VI).RegisterDelegates(LibTuple.TL, levelCode);
            sk01 = new JNS.SkillCottage(this, VI).RegisterDelegates(LibTuple.SL);
            cz01 = new JNS.OperationCottage(this, VI).RegisterDelegates(LibTuple.ZL);
            nj01 = new JNS.NPCCottage(this, VI).RegisterDelegates(LibTuple.NJL);
            new JNS.NPCCottage(this, VI).RegisterNPCDelegates(LibTuple.NL);
            ev01 = new JNS.EveCottage(this, VI).RegisterDelegates(LibTuple.EL);
            sf01 = new JNS.RuneCottage(this, VI).RegisterDelegates(LibTuple.RL);
            MappingSksp(out sk02, out sk03, levelCode);
            mt01 = new JNS.MonsterCottage(this, VI).RegisterDelegates(LibTuple.ML);

            IDictionary<ushort, int> hipc = new Dictionary<ushort, int>();
            foreach (Player player in garden.Values)
            {
                Base.Card.Hero hero = LibTuple.HL.InstanceHero(player.SelectHero);
                player.InitFromHero(hero, true, false, false);
            }
            //hipc.Add(player.Uid, player.SelectHero);
            //xiclients[player.Uid].Heros = hipc;
            foreach (Player player in garden.Values)
            {
                Base.Card.Hero hero = LibTuple.HL.InstanceHero(player.SelectHero);
                if (hero.Skills.Count > 0)
                    RaiseGMessage("G0IS," + player.Uid + ",0," + string.Join(",", hero.Skills));
            }
            Board.RoundIN = "H0ST";
            RunQuadStage("H0ST", 0); // Game start stage
            if (inDebug)
                DebugCondition();
            else
            {
                foreach (ushort ut in Board.OrderedPlayer((ushort)1))
                    RaiseGMessage("G0HQ,2," + ut + ",1,3");
            }

            listOfThreads = new List<Thread>();
            jumpTareget = "R100"; jumpEnd = "H0TM";
            while (!isFinished)
            {
                System.Threading.Thread.Sleep(1000);
                bool valid;
                lock (jumpTareget)
                {
                    valid = (jumpTareget != "" && jumpEnd != "");
                }
                if (valid)
                {
                    foreach (Thread td in listOfThreads)
                        td.Abort();
                    listOfThreads.Clear();
                    string jt = jumpTareget.ToString();
                    string je = jumpEnd.ToString();
                    var thread0 = new Thread(() => Util.SafeExecute(() => RunRound(jt, je),
                        delegate(Exception e)
                        {
                            if (!(e is ThreadAbortException))
                                Log.Logger(e.ToString());
                        }));
                    thread0.Start();
                    listOfThreads.Add(thread0);
                    jumpTareget = ""; jumpEnd = "";
                }
            }
            //RunRound("R100", "H0TM");
        }

        public void RunRound(string rstage, string endRstage)
        {
            while (rstage != null && !rstage.Equals(endRstage))
            {
                if (rstage[0] != 'R')
                    break;
                if (Board.JumpTable.ContainsKey(rstage))
                {
                    string[] infos = Board.JumpTable[rstage].Split(',');
                    string next = infos[0];
                    for (int i = 1; i < infos.Length; ++i)
                        Board.JumpTable.Remove(infos[i]);
                    Board.JumpTable.Remove(rstage);
                    rstage = next;
                }
                ushort rounder = (ushort)(rstage[1] - '0');
                Board.Rounder = Board.Garden[rounder];
                Board.RoundIN = rstage;
                //string sage = rstage.Substring(2);
                //ushort[] staff = garden.Keys.ToArray();
                //VI.Cout(0, "☆◇○" + rstage + "○◇☆");
                Log.Logger(rstage);
                switch (rstage.Substring(2))
                {
                    case "00":
                        Board.Garden[rounder].ResetRAM();
                        if (!Board.Garden[rounder].Immobilized)
                        {
                            WI.BCast(rstage + "1,0");
                            rstage = "R" + rounder + "OC";
                        }
                        else
                        {
                            //--Board.Garden[rounder].Immobilized;
                            //if (Board.Garden[rounder].Immobilized != 0)
                            //    WI.BCast(rstage + "1,1");
                            //else
                            //    WI.BCast(rstage + "1,2");
                            WI.BCast(rstage + "1,2");
                            RaiseGMessage("G0QR," + rounder);
                            Board.Garden[rounder].Immobilized = false;
                            rstage = "R" + rounder + "ED";
                        }
                        break;
                    case "OC":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "ST";
                        break;
                    case "ST":
                        WI.BCast(rstage + ",0");
                        RunQuadStage(rstage, 0);
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "EP";
                        break;
                    case "EP":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "EV";
                        break;
                    case "EV":
                        {
                            WI.BCast(rstage + "1,0");
                            string replyEve = AsyncInput(rounder, "#请决定是否翻看事件牌##不翻看##翻看,Y2",
                                "R" + rounder + "EV1", "0");
                            if (replyEve == "2")
                                RaiseGMessage("G1EV," + rounder + ",0");
                            else
                                WI.BCast("R" + rounder + "EV2,0");
                            rstage = "R" + rounder + "EE";
                        }
                        break;
                    case "EE":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "GS"; break;
                    case "GS":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "GR"; break;
                    case "GR":
                        WI.BCast(rstage + ",0");
                        RunQuadStage(rstage, 3);
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "GE"; break;
                    case "GE":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "GF"; break;
                    //rstage = rounder == 1 ?"R200":"R100"; break;
                    case "GF":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "Z0"; break;
                    case "Z0":
                        Board.Monster1 = 0; Board.Monster2 = 0;
                        Board.RPool = 0; Board.OPool = 0;
                        Board.RPoolGain.Clear(); Board.OPoolGain.Clear();
                        Board.Battler = null;
                        RaiseGMessage("G1SG,0");
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "ZW"; break;
                    case "ZW":
                        {
                            // TODO: to substitute old ZW event on considering the capability
                            Board.PosHinders.Clear();
                            ushort[] hMember = Board.Garden.Values.Where(p => p.IsAlive &&
                                p.Team == Board.Rounder.OppTeam).Select(p => p.Uid).ToArray();
                            Board.PosHinders.AddRange(hMember.Select(p => "T" + p).ToList());
                            Board.PosSupporters.Clear();
                            ushort[] sMember = Board.Garden.Values.Where(p => p.IsAlive &&
                                p.Team == Board.Rounder.Team).Select(p => p.Uid).ToArray();
                            Board.PosSupporters.AddRange(sMember.Select(p => "T" + p).ToList());
                            Board.AllowNoSupport = true;
                            Board.AllowNoHinder = true;
                            RunQuadStage(rstage, 0);
                            // Trigger side
                            bool isFight = false; // decide to show fight or just pass
                            string spsm = "#为支援者(决定)", spsn = "#为支援者(建议)";
                            string spsb = string.Format("—{0}:{1}则不支援,", Board.Rounder.Uid,
                                 LibTuple.HL.InstanceHero(Board.Rounder.SelectHero).Name);
                            if (Board.AllowNoSupport)
                                spsb += "/";
                            if (Board.PosSupporters.Count > 0)
                                spsb += "J1(p" + string.Join("p", Board.PosSupporters) + ")";
                            else
                                spsb = "/";
                            string decision = MajorAsyncInput(Board.Rounder.Uid, spsm + spsb, sMember.Except(
                                new ushort[] { Board.Rounder.Uid }), spsn + spsb, (advUt, advStr) =>
                                {
                                    WI.Send(rstage + "3," + advUt + "," + advStr, sMember);
                                    WI.Send(rstage + "3," + advUt, ExceptStaff(sMember));
                                    WI.Live(rstage + "3," + advUt);
                                });
                            ushort sprUid = 0;
                            if (decision.StartsWith("T"))
                            {
                                ushort who = ushort.Parse(decision.Substring("T".Length));
                                sprUid = (who != Board.Rounder.Uid) ? who : (ushort)0;
                                isFight = true;
                            }
                            else if (decision.StartsWith("P"))
                            {
                                ushort cdCode = ushort.Parse(decision.Substring("PT".Length));
                                isFight = true; sprUid = (ushort)(cdCode + 1000);
                            }
                            else if (decision.StartsWith("/"))
                            {
                                isFight = false; sprUid = 0;
                            }

                            if (!isFight)
                            {
                                RaiseGMessage("G17F,O");
                                // Ensure XI.Board.Mon1From == 0
                                ushort mons = DequeueOfPile(Board.MonPiles);
                                RaiseGMessage("G2IN,1,1");
                                Board.Battler = null;
                                WI.BCast(rstage + "7,0," + mons);
                                RaiseGMessage("G0ON,0,M,1," + mons);
                                rstage = "R" + rounder + "ZF";
                            }
                            else
                            {
                                RaiseGMessage("G17F,S," + sprUid);
                                // Hinder side
                                isFight = false; // decide to show fight or just pass
                                string hnsm = "#妨碍者(决定)", hnsn = "#妨碍者(建议)";
                                string hnsb = ",";
                                if (Board.AllowNoHinder)
                                    hnsb += "/";
                                if (Board.PosHinders.Count > 0)
                                    hnsb += "J1(p" + string.Join("p", Board.PosHinders) + ")";
                                else
                                    hnsb = "/";
                                decision = MajorAsyncInput(Board.Opponent.Uid, hnsm + hnsb, hMember.Except(
                                    new ushort[] { Board.Opponent.Uid }), hnsn + hnsb, (advUt, advStr) =>
                                    {
                                        WI.Send(rstage + "3," + advUt + "," + advStr, hMember);
                                        WI.Send(rstage + "3," + advUt, ExceptStaff(hMember));
                                        WI.Live(rstage + "3," + advUt);
                                    });
                                ushort hndUid = 0;
                                if (decision.StartsWith("T"))
                                {
                                    ushort who = ushort.Parse(decision.Substring("T".Length));
                                    hndUid = who;
                                }
                                else if (decision.StartsWith("P"))
                                {
                                    ushort cdCode = ushort.Parse(decision.Substring("PT".Length));
                                    hndUid = (ushort)(cdCode + 1000);
                                }
                                else if (decision.StartsWith("/"))
                                    hndUid = 0;
                                RaiseGMessage("G17F,H," + hndUid);
                                //WI.BCast(rstage + "7,2," + Board.Hinder.Uid);
                                rstage = "R" + rounder + "ZU";
                            }
                        }
                        break;
                    case "ZU":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "ZM";
                        break;
                    case "ZM":
                        {
                            //Board.Monster1 = 0; Board.Monster2 = 0;
                            //Board.IsTangled = false;
                            if (Board.Mon1From == 0)　// Board.Monster1 hasn't been set ready.
                            {
                                ushort mons = DequeueOfPile(Board.MonPiles);
                                RaiseGMessage("G2IN,1,1");
                                // actually judge who wins won't happens here
                                //if (mons == 0) { rstage = "H0TM"; break; }
                                Board.Monster1 = mons;
                            }
                            WI.BCast("R" + rounder + "ZM1," + Board.Monster1);
                            RaiseGMessage("G0YM,0," + Board.Monster1 + "," + Board.Mon1From);
                            Board.Battler = NMBLib.Decode(Board.Monster1, LibTuple.ML, LibTuple.NL);
                            RunQuadStage(rstage, 0);
                            if (NMBLib.IsNPC(Board.Monster1))
                                rstage = "R" + rounder + "NP";
                            else if (NMBLib.IsMonster(Board.Monster1))
                                rstage = "R" + rounder + "Z7";
                            else
                                rstage = "H0TM";
                            break;
                        }
                    case "NP":
                        {
                            WI.BCast(rstage + "1,0");
                            ushort npcut = Board.Monster1;
                            RaiseGMessage("G1NI," + Board.Rounder.Uid + "," + npcut);
                            NPC npc = LibTuple.NL.Decode(NMBLib.OriginalNPC(npcut));
                            UEchoCode r5ed = HandleWithNPCEffect(Board.Rounder, npc, true);
                            if (r5ed == UEchoCode.NO_OPTIONS) // cannot take any action, check whether finished
                            {
                                AsyncInput(Board.Rounder.Uid, "//", rstage, "1");
                                if (Board.MonPiles.Count <= 0)
                                    RaiseGMessage("G1WJ,0");
                            }
                            if (r5ed != UEchoCode.END_ACTION) // not take action, skip
                            {
                                RaiseGMessage("G0ON,0,M,1," + Board.Monster1);
                                Board.Monster1 = 0;
                                if (Board.MonPiles.Count > 0)
                                {
                                    ushort mons = DequeueOfPile(Board.MonPiles);
                                    RaiseGMessage("G2IN,1,1");
                                    Board.Monster1 = mons; Board.Mon1From = 0;
                                    Board.Battler = NMBLib.Decode(Board.Monster1,
                                        LibTuple.ML, LibTuple.NL);
                                    WI.BCast(rstage + "2," + Board.Monster1);
                                    RaiseGMessage("G0YM,0," + Board.Monster1 + "," + Board.Mon1From);
                                    if (NMBLib.IsNPC(Board.Monster1))
                                        rstage = "R" + rounder + "NP";
                                    else if (NMBLib.IsMonster(Board.Monster1))
                                        rstage = "R" + rounder + "Z7";
                                    else
                                        rstage = "H0TM";
                                }
                                else
                                    RaiseGMessage("G1WJ,0");
                            }
                            else // take action
                            {
                                RaiseGMessage("G1YP," + Board.Rounder.Uid + "," + npcut);
                                WI.BCast(rstage + "1,1");
                                if (Board.Monster1 != 0) // In case Monster1 has been taken
                                {
                                    RaiseGMessage("G0ON,10,M,1," + Board.Monster1);
                                    Board.Monster1 = 0;
                                }
                                rstage = "R" + rounder + "Z3";
                            }
                            break;
                        }
                    case "Z7":
                        Board.InFightThrough = true;
                        Board.FightTangled = false;
                        AwakeABCValue(false);
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "Z1"; break;
                    case "Z1":
                        WI.BCast(rstage + ",0");
                        RaiseGMessage("G09P,0");
                        RunQuadStage(rstage, 0);
                        WI.BCast(rstage + ",1");
                        RaiseGMessage("G0CZ,2");
                        rstage = "R" + rounder + "Z8"; break;
                    case "Z8":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "CC"; break;
                    case "CC":
                        WI.BCast(rstage + ",0");
                        RunQuadStage(rstage, 0);
                        WI.BCast(rstage + ",1");
                        //Board.Battler.Debut();
                        Board.IsMonsterDebut = true;
                        LibTuple.ML.Decode(Board.Monster1).Debut();
                        rstage = "R" + rounder + "PD"; break;
                    case "PD":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "ZC"; break;
                    case "ZC":
                        Board.InFight = true;
                        AwakeABCValue(true);
                        RunQuadStage(rstage, 0);
                        RaiseGMessage("G09P,0");
                        rstage = "R" + rounder + "ZI"; break;
                    case "ZI":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "ZD"; break;
                    case "ZD":
                        WI.BCast(rstage + ",0");
                        //bool roundWay = true; // IsGameCompete
                        //if (roundWay)
                        //{
                        //    bool ichi = false, ni = false;
                        //    while (!ichi || !ni)
                        //    {
                        //        bool adv = Board.CalculateRPool() >= Board.CalculateOPool();
                        //        if (adv ^ ichi)
                        //            Board.UseCardRound = Board.Garden[rounder].OppTeam;
                        //        else
                        //            Board.UseCardRound = Board.Garden[rounder].Team;
                        //        if (RunQuadStage(rstage, 5)) { ichi = false; ni = false; }
                        //        else if (!ichi) { ichi = true; }
                        //        else { ni = true; }
                        //    }
                        //    Board.UseCardRound = 0;
                        //}
                        //else
                        //    RunQuadStage(rstage, 1);
                        RunSeperateStage(rstage, 1, delegate(Board bd)
                            { return bd.IsRounderBattleWin(); });
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "ZN"; break;
                    case "ZN":
                        Board.IsBattleWin = Board.IsRounderBattleWin();
                        Board.PoolDelta = Board.CalculateRPool() - Board.CalculateOPool();
                        WI.BCast(rstage + "," + (Board.IsBattleWin ? "0" : "1"));
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "VS"; break;
                    case "VT":
                        {
                            Board.InFight = false;
                            //WI.BCast(rstage + "1,0");
                            //Board.IsBattleWin = Board.IsRounderBattleWin();
                            bool mon1zero = false, mon2zero = false;
                            if (Board.IsBattleWin) // OK, win
                            {
                                bool hasMonster2 = Board.Monster2 != 0 && NMBLib.IsMonster(Board.Monster2);
                                RaiseGMessage("G0HC,0," + Board.Rounder.Uid +
                                    "," + Board.Mon1From + "," + Board.Monster1);
                                mon1zero = true;
                                if (hasMonster2)
                                {
                                    RaiseGMessage("G0HC,0," + Board.Rounder.Uid + ",0," + Board.Monster2);
                                    mon2zero = true;
                                }
                            }
                            //WI.BCast(rstage + "2," + (Board.IsBattleWin ? "0" : "1"));
                            RunQuadStage(rstage, 0);

                            RecycleMonster(mon1zero, mon2zero);
                            Board.InFightThrough = false;
                            RaiseGMessage("G1ZK,1");
                            RaiseGMessage("G1HK,1");
                            WI.BCast(rstage + "3");
                            rstage = "R" + rounder + "Z2"; break;
                        }
                    case "VS":
                        {
                            Board.InFight = false;
                            //WI.BCast(rstage + "1,0");
                            //Board.IsBattleWin = Board.IsRounderBattleWin();
                            bool mon1zero = false, mon2zero = false;
                            Board.Mon1Catchable = true; Board.Mon2Catchable = true;
                            if (Board.IsBattleWin) // OK, win
                            {
                                if (Board.Monster1 != 0)
                                {
                                    var mon1 = LibTuple.ML.Decode(Board.Monster1);
                                    mon1.WinEff();
                                }
                                bool hasMonster2 = Board.Monster2 != 0 && NMBLib.IsMonster(Board.Monster2);
                                if (hasMonster2)
                                {
                                    var mon2 = LibTuple.ML.Decode(Board.Monster2);
                                    mon2.WinEff();
                                }
                                if (Board.Monster1 != 0 && Board.Mon1Catchable)
                                {
                                    RaiseGMessage("G0HC,0," + Board.Rounder.Uid +
                                        "," + Board.Mon1From + "," + Board.Monster1);
                                    mon1zero = true;
                                }
                                if (hasMonster2 && Board.Mon2Catchable)
                                {
                                    RaiseGMessage("G0HC,0," + Board.Rounder.Uid + ",0," + Board.Monster2);
                                    mon2zero = true;
                                }
                            }
                            else
                            {
                                if (Board.Monster1 != 0)
                                {
                                    var mon1 = LibTuple.ML.Decode(Board.Monster1);
                                    mon1.LoseEff();
                                }
                                if (Board.Monster2 != 0 && NMBLib.IsMonster(Board.Monster2))
                                {
                                    var mon2 = LibTuple.ML.Decode(Board.Monster2);
                                    mon2.LoseEff();
                                }
                            }
                            //WI.BCast(rstage + "2," + (Board.IsBattleWin ? "0" : "1"));
                            RunQuadStage(rstage, 0);

                            RecycleMonster(mon1zero, mon2zero);
                            Board.InFightThrough = false;
                            RaiseGMessage("G1ZK,1");
                            RaiseGMessage("G1HK,1");
                            WI.BCast(rstage + "3");
                            rstage = "R" + rounder + "Z2"; break;
                        }
                    case "Z2":
                        Board.InFight = false; Board.InFightThrough = false;
                        WI.BCast(rstage + ",0");
                        RaiseGMessage("G1ZK,1");
                        RaiseGMessage("G1HK,1");
                        foreach (Player player in Board.Garden.Values)
                            RaiseGMessage("G0AX," + player.Uid);
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "Z3"; break;
                    case "Z3":
                        Board.InFight = false; Board.InFightThrough = false;
                        Board.RPool = 0; Board.OPool = 0;
                        Board.RPoolGain.Clear(); Board.OPoolGain.Clear();
                        RecycleMonster(false, false);
                        //WI.BCast(rstage + ",0");
                        //RaiseGMessage("G0FI,U," + rounder);
                        RunQuadStage(rstage, 0);
                        Board.CleanBattler();
                        rstage = "R" + rounder + "ZZ"; break;
                    case "ZF":
                        WI.BCast(rstage + ",0");
                        Board.InFight = false; Board.InFightThrough = false;
                        foreach (Player player in Board.Garden.Values)
                            RaiseGMessage("G0AX," + player.Uid);
                        RunQuadStage(rstage, 0);
                        Board.CleanBattler();
                        rstage = "R" + rounder + "ZZ"; break;
                    case "ZZ":
                        RaiseGMessage("G17F,U," + rounder);
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "BB"; break;
                    case "BB":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "BC"; break;
                    case "BC":
                        WI.BCast(rstage + ",0");
                        Board.InFight = false; Board.InFightThrough = false;
                        RunQuadStage(rstage, 0);
                        if (Board.Battler != null)
                            RaiseGMessage("G0HT," + Board.Rounder.Uid + ",2");
                        else
                            RaiseGMessage("G0HT," + Board.Rounder.Uid + ",1");
                        rstage = "R" + rounder + "BD"; break;
                    case "BD":
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "QR"; break;
                    case "QR":
                        RunQuadStage(rstage, 0);
                        RaiseGMessage("G0QR," + Board.Rounder.Uid);
                        rstage = "R" + rounder + "TM"; break;
                    case "TM":
                        WI.BCast(rstage + ",0");
                        Board.InFight = false; Board.InFightThrough = false;
                        RunQuadStage(rstage, 0);
                        rstage = "R" + rounder + "ED"; break;
                    case "ED":
                        {
                            WI.BCast(rstage + ",0");
                            Board.InFight = false; Board.InFightThrough = false;
                            Board.Battler = null; Board.CleanBattler();
                            if (Board.MonPiles.Count <= 0)
                                RaiseGMessage("G1WJ,0");
                            RecycleMonster(false, false);
                            if (Board.Eve != 0)
                            {
                                RaiseGMessage("G0ON,10,E,1," + Board.Eve);
                                RaiseGMessage("G0YM,2,0,0");
                                Board.Eve = 0;
                            }
                            foreach (Player player in Board.Garden.Values)
                                RaiseGMessage("G0AX," + player.Uid);
                            RunQuadStage(rstage, 0);
                            if (Board.PendingTux.Count > 0)
                            {
                                IDictionary<ushort, List<ushort>> imt = new Dictionary<ushort, List<ushort>>();
                                foreach (string pendItem in Board.PendingTux)
                                {
                                    ushort pendWho = ushort.Parse(pendItem.Substring(0, pendItem.IndexOf(',')));
                                    ushort pendTux = ushort.Parse(pendItem.Substring(pendItem.LastIndexOf(',') + 1));
                                    Util.AddToMultiMap(imt, pendWho, pendTux);
                                }
                                if (imt.Count > 0)
                                    RaiseGMessage("G0ON," + string.Join(",", imt.Select(p => p.Key + ",C," +
                                        p.Value.Count + "," + string.Join(",", p.Value))));
                            }
                            if (!Board.Rounder.IsAlive && Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                                RaiseGMessage("G0ZH,1");
                            List<ushort> ordered = Board.OrderedPlayer();
                            bool found = false;
                            foreach (ushort ut in ordered)
                            {
                                if (ut != rounder && Board.Garden[ut].IsAlive)
                                {
                                    found = true;
                                    rstage = "R" + ut + "00";
                                    break;
                                }
                            }
                            if (!found)
                            {
                                if (Board.Rounder.IsAlive)
                                    RaiseGMessage("G0WN," + Board.Rounder.Team);
                                else
                                    RaiseGMessage("G0WN,0");
                            }
                            foreach (Player player in Board.Garden.Values)
                                player.ResetRAM();
                        }
                        break;
                }
            }
        }
        //private void RunQuadStage(string zero) { RunQuadStage(zero, 1); }
        // return whether actual action has been taken
        private bool RunQuadStage(string zero, int sina)
        {
            var garden = Board.Garden;
            if (!sk02.ContainsKey(zero))
            {
                foreach (Player py in Board.Garden.Values)
                    py.IsZhu = false;
                return false;
            }
            List<SKE> pocket = ParseFromSKTriples(sk02[zero], zero, (sina & 4) != 0);

            bool[] involved = new bool[garden.Count + 1];
            string[] pris = new string[garden.Count + 1];

            bool actualAction = false;
            WI.RecvInfStart();
            do
            {
                Fill(involved, false);
                Fill(pris, "");
                List<string> locks = new List<string>();
                List<SKE> purse = new List<SKE>();

                AddZhuSkillBackward(pocket, zero, (sina & 4) != 0);
                foreach (SKE ske in pocket)
                {
                    if ((sina & 1) != 0 && ske.Type == SKTType.TX && garden[ske.Tg].IsAlive)
                        involved[ske.Tg] = true;
                    bool ias = SKE2Message(ske, zero, involved, pris, locks);
                    if (ias)
                        purse.Add(ske);
                    // if somebody added now, then re-scan it's related with $zero
                }

                // round mode, priority not needed.
                if (locks.Count > 0)
                {
                    locks.Sort(LockSkillCompare);
                    Queue<string> queue = new Queue<string>(locks);
                    while (queue.Count > 0)
                    {
                        string msg = queue.Dequeue();
                        int idx = msg.IndexOf(',');
                        ushort me = ushort.Parse(msg.Substring(0, idx));
                        int jdx = msg.LastIndexOf(';');
                        string mai = Util.Substring(msg, idx + 1, jdx);
                        string inType = Util.Substring(msg, jdx + 1, -1);

                        string skName;
                        mai = DecodeSimplifiedCommand(mai, out skName);
                        SKE ske = SKE.Find(skName, me, purse);
                        if (ske != null)
                            HandleU24Message(me, involved, mai, ske);
                        //UKEvenMessage(involved, pocket, null);
                    }
                }
                if (!garden.Keys.Where(p => involved[p]).Any())
                    break; // No skills could be called, cancel
                foreach (ushort ut in Board.Garden.Keys)
                    if (pris[ut] != "")
                        pris[ut] = pris[ut].Substring(1);
                SendOutU1Message(involved, pris, sina);
                UEchoCode echo = UKEvenMessage(involved, purse, pris, sina);
                actualAction |= (echo == UEchoCode.END_ACTION);
                if (actualAction && ((sina & 4) != 0))
                    break;
                if (echo == UEchoCode.END_TERMIN) // skill updated
                    pocket = ParseFromSKTriples(sk02[zero], zero, (sina & 4) != 0);
                //actualAction |= UKEvenMessage(involved, purse, pris, sina);
                //if (actualAction && ((sina & 4) != 0))
                //    break;
            } while (!IsAllClear(involved, false));
            RaiseGMessage("G2AS,0");
            WI.RecvInfEnd();
            return actualAction;
        }
        
        // Run in Separate Ways
        // return whether actual action has been taken
        private bool RunSeperateStage(string zero, int sina, Func<Board, bool> judge)
        {
            var garden = Board.Garden;
            if (!sk02.ContainsKey(zero))
            {
                foreach (Player py in Board.Garden.Values)
                    py.IsZhu = false;
                return false;
            }
            List<SKE> pocket = ParseFromSKTriples(sk02[zero], zero, false);

            bool[] involved = new bool[garden.Count + 1];
            string[] pris = new string[garden.Count + 1];

            WI.RecvInfStart();
            int insstage = 0; // 0: ichi=F, 1: ichi=T,ni=F, 2:ni=T
            ushort rounder = (ushort)(zero[1] - '0');
            while (insstage < 2)
            {
                bool adv = judge(Board);
                int side;
                if (insstage == 0)
                    side = adv ? garden[rounder].OppTeam : garden[rounder].Team;
                else if (insstage == 1)
                    side = !adv ? garden[rounder].OppTeam : garden[rounder].Team;
                else
                    side = 0;

                bool actualAction = false;
                do
                {
                    Fill(involved, false);
                    Fill(pris, "");
                    List<string> locks = new List<string>();
                    List<SKE> purse = new List<SKE>();
                    AddZhuSkillBackward(pocket, zero, (sina & 4) != 0);
                    foreach (SKE ske in pocket)
                    {
                        if (garden[ske.Tg].Team == side)
                        {
                            if ((sina & 1) != 0 && ske.Type == SKTType.TX && garden[ske.Tg].IsAlive)
                                involved[ske.Tg] = true;
                            bool ias = SKE2Message(ske, zero, involved, pris, locks);
                            if (ias)
                                purse.Add(ske);
                        }
                    }
                    // round mode, priority not needed.
                    if (locks.Count > 0)
                    {
                        locks.Sort(LockSkillCompare);
                        Queue<string> queue = new Queue<string>(locks);
                        while (queue.Count > 0)
                        {
                            string msg = queue.Dequeue();
                            int idx = msg.IndexOf(',');
                            ushort me = ushort.Parse(msg.Substring(0, idx));
                            int jdx = msg.LastIndexOf(';');
                            string mai = Util.Substring(msg, idx + 1, jdx);
                            string inType = Util.Substring(msg, jdx + 1, -1);

                            string skName;
                            mai = DecodeSimplifiedCommand(mai, out skName);
                            SKE ske = SKE.Find(skName, me, purse);
                            if (ske != null)
                                HandleU24Message(me, involved, mai, ske);
                            //UKEvenMessage(involved, pocket, null);
                        }
                    }
                    if (!garden.Keys.Where(p => involved[p]).Any())
                        break; // No skills could be called, cancel
                    foreach (ushort ut in Board.Garden.Keys)
                        if (pris[ut] != "")
                            pris[ut] = pris[ut].Substring(1);
                    SendOutU1Message(involved, pris, sina);
                    UEchoCode echo = UKEvenMessage(involved, purse, pris, sina);
                    actualAction |= (echo == UEchoCode.END_ACTION);
                    // if actualAction happens, force back to insstage = 0;
                    // else continue until all involved solved, increase insstage
                    if (actualAction)
                    {
                        insstage = 0;
                        break;
                    }
                    if (echo == UEchoCode.END_TERMIN) // skill updated
                        pocket = ParseFromSKTriples(sk02[zero], zero, false);
                } while (!IsAllClear(involved, false));
                if (!actualAction)
                {
                    if (insstage == 0)
                        insstage = 1; // ask advantage side
                    else if (insstage == 1)
                        insstage = 2; // both cancel
                }
            }
            RaiseGMessage("G2AS,0");
            WI.RecvInfEnd();
            return true;
        }
        private UEchoCode UKEvenMessage(bool[] involved,
            List<SKE> purse, string[] pris, int sina)
        {
            return UKEvenMessage(involved, purse, pris, Util.RepeatToArray(sina, involved.Length));
        }
        // return whether actual action has been taken, otherwise all cancel.
        private UEchoCode UKEvenMessage(bool[] involved,
            List<SKE> purse, string[] pris, int[] sina)
        {
            involved[0] = false;
            var garden = Board.Garden;
            //bool isUK5Received = false;
            //while (!isUK5Received && !IsAllClear(involved, false))
            while (!IsAllClear(involved, false))
            {
                Base.VW.Msgs msg = WI.RecvInfRecvPending();
                UEchoCode next = HandleUMessage(msg.Msg, purse, msg.From, involved, sina);
                if (next == UEchoCode.END_ACTION || next == UEchoCode.END_TERMIN)
                    //isUK5Received = true;
                    return next;
                else if (next == UEchoCode.RE_REQUEST)
                    ResendU1Message(msg.From, involved, pris, false, sina);
            }
            return UEchoCode.END_CANCEL;
            //return isUK5Received;
        }
        // $zero1 means $monster1 has been taken to another places, only set to 0
        // otherwise, handle with it
        private void RecycleMonster(bool zero1, bool zero2)
        {
            if (Board.Monster1 != 0)
            {
                Monster mon1 = LibTuple.ML.Decode(NMBLib.OriginalMonster(Board.Monster1));
                if (mon1 != null && Board.IsMonsterDebut)
                    mon1.Curtain();
                if (!zero1)
                {
                    if (Board.Mon1From != 0)
                        RaiseGMessage("G0WB," + Board.Monster1);
                    else
                    {
                        RaiseGMessage("G0ON,10,M,1," + Board.Monster1);
                        RaiseGMessage("G0WB," + Board.Monster1);
                    }
                    RaiseGMessage("G0YM,0,0,0");
                }
                Board.Mon1From = 0;
                Board.Monster1 = 0;
            }
            if (Board.Monster2 != 0)
            {
                Monster mon2 = LibTuple.ML.Decode(NMBLib.OriginalMonster(Board.Monster2));
                //if (mon2 != null)
                //    mon2.Curtain();
                if (!zero2)
                {
                    RaiseGMessage("G0ON,10,M,1," + Board.Monster2);
                    RaiseGMessage("G0WB," + Board.Monster2);
                    RaiseGMessage("G0YM,1,0,0");
                }
                Board.Monster2 = 0;
            }
            Board.IsMonsterDebut = false;
        }
        // Handle With NPC
        public UEchoCode HandleWithNPCEffect(Player player, NPC npc, bool watchValid)
        {
            string rstage = Board.RoundIN;
            ushort rd = player.Uid;

            bool[] involved = new bool[Board.Garden.Count + 1];
            string[] pris = new string[Board.Garden.Count + 1];
            Fill(involved, false);
            Fill(pris, "");
            List<SKE> purse = new List<SKE>();
            foreach (string npsk in npc.Skills)
                if (nj01.ContainsKey(npsk))
                {
                    string nfuse = npc.Code + ";" + rstage;
                    if (nj01[npsk].Valid(player, nfuse))
                    {
                        SKE skt = new SKE(new SkTriple()
                        {
                            Name = npsk,
                            Priorty = 0,
                            Owner = 0,
                            InType = 0,
                            Type = SKTType.NJ,
                            Consume = 0,
                            Lock = false,
                            IsOnce = false,
                        }) { Tg = player.Uid, Fuse = nfuse };
                        purse.Add(skt);
                        string ip = nj01[npsk].Input(player, nfuse, "");
                        if (ip != "") ip = "," + ip;
                        pris[rd] += ";" + npsk + ip;
                    }
                }
            UEchoCode r5ed = UEchoCode.END_CANCEL;
            if (pris[rd] != "")
            {
                pris[rd] = pris[rd].Substring(1);
                involved[rd] = true;
                if (watchValid)
                    RaiseGMessage("G1SG,0");
                SendOutU1Message(involved, pris, 0);
                WI.RecvInfStart();
                while (r5ed != UEchoCode.END_ACTION && r5ed != UEchoCode.END_TERMIN && involved[rd])
                {
                    Base.VW.Msgs msg = WI.RecvInfRecvPending();
                    r5ed = HandleUMessage(msg.Msg, purse, msg.From, involved, 0);
                    if (r5ed == UEchoCode.RE_REQUEST)
                        ResendU1Message(msg.From, involved, pris, false, 0);
                    else if (r5ed == UEchoCode.END_CANCEL && Board.MonPiles.Count <= 0
                            && msg.From == player.Uid)
                        ResendU1Message(player.Uid, involved, pris, true, 0);
                    // critical, must take action.
                }
                WI.RecvInfEnd();
            }
            else
                r5ed = UEchoCode.NO_OPTIONS; // cannot take any action, seem as skip
            return r5ed;
        }
        private void AwakeABCValue(bool containsC)
        {
            foreach (Player py in Board.Garden.Values)
            {
                if (!containsC)
                {
                    py.SDaSet = true;
                    py.STRa = py.STRb;
                    py.DEXa = py.DEXb;
                }
                if (containsC)
                {
                    py.SDcSet = true;
                    py.STRc = py.STRa;
                    py.DEXc = py.DEXa;
                }
            }
        }
    }
}