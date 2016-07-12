using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg
{
    public partial class XI
    {
        private bool isFinished;

        private List<Thread> listOfThreads;

        private string jumpTareget, jumpEnd;

        private Random randomSeed;

        //private bool IsGameCompete { set; get; }
        public void Run(int levelCode, bool inDebug)
        {
            var garden = Board.Garden;
            LastUVs = new Dictionary<ushort, string>();
            uvMsgSerialNum = 0;
            ConstructPiles(levelCode);
            WI.BCast(string.Format("H0DP,{0},{1},{2}",
                Board.TuxPiles.Count, Board.MonPiles.Count, Board.EvePiles.Count));
            tx01 = new JNS.TuxCottage(this, VI).RegisterDelegates(LibTuple.TL, levelCode);
            sk01 = new JNS.SkillCottage(this, VI).RegisterDelegates(LibTuple.SL);
            cz01 = new JNS.OperationCottage(this, VI).RegisterDelegates(LibTuple.ZL);
            var npcCottage = new JNS.NPCCottage(this, VI);
            nj01 = npcCottage.RegisterDelegates(LibTuple.NJL);
            npcCottage.RegisterNPCDelegates(LibTuple.NL); // trigger directly, no SKE generated
            ev01 = new JNS.EveCottage(this, VI).RegisterDelegates(LibTuple.EL);
            sf01 = new JNS.RuneCottage(this, VI).RegisterDelegates(LibTuple.RL);
            MappingSksp(out sk02, out sk03, levelCode);
            mt01 = new JNS.MonsterCottage(this, VI).RegisterDelegates(LibTuple.ML);

            Board.RoundIN = "H0ST";
            foreach (Player player in garden.Values)
            {
                Base.Card.Hero hero = LibTuple.HL.InstanceHero(player.SelectHero);
                player.InitFromHero(hero, true, false, false);
                Artiad.ContentRule.LoadDefaultPrice(player);
            }
            foreach (Player player in garden.Values)
            {
                Base.Card.Hero hero = LibTuple.HL.InstanceHero(player.SelectHero);
                if (hero.Skills.Count > 0)
                    RaiseGMessage("G0IS," + player.Uid + ",0," + string.Join(",", hero.Skills));
            }
            RunQuadStage("H0ST"); // Game start stage
            if (inDebug)
                DebugCondition();
            else
                Board.OrderedPlayer((ushort)1).ForEach(p => RaiseGMessage("G0HQ,2," + p + ",1,3"));

            listOfThreads = new List<Thread>();
            jumpTareget = "R100"; jumpEnd = "H0TM";
            while (!isFinished)
            {
                Thread.Sleep(1000);
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
                    var thread0 = new Thread(() => XI.SafeExecute(() => RunRound(jt, je),
                        delegate (Exception e)
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
        public void BlockSetJumpTable(string jumpTarget, string jumpEnd)
        {
            lock (this.jumpTareget)
            {
                this.jumpTareget = jumpTarget;
                this.jumpEnd = jumpEnd;
            }
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
                //VI.Cout(0, "☆◇○" + rstage + "○◇☆");
                Log.Logger(rstage);
                switch (rstage.Substring(2))
                {
                    case "00":
                        ResetAllPlayerRAM(); // 0:00
                        Board.Garden[rounder].ResetRFM();
                        if (!Board.Garden[rounder].Immobilized)
                        {
                            WI.BCast(rstage + "1,0");
                            rstage = "R" + rounder + "OC";
                        }
                        else
                        {
                            WI.BCast(rstage + "1,2");
                            RaiseGMessage("G0QR," + rounder);
                            RaiseGMessage("G0DS," + rounder + ",1");
                            rstage = "R" + rounder + "ED";
                        }
                        break;
                    case "OC":
                        ResetAllPlayerRAM(); // 1:ST
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "ST";
                        break;
                    case "ST":
                        WI.BCast(rstage + ",0");
                        RunQuadStage(rstage);
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "EP";
                        break;
                    case "EP":
                        ResetAllPlayerRAM(); // 2:EV
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "EV";
                        break;
                    case "EV":
                        {
                            WI.BCast(rstage + "1,0");
                            string replyEve = AsyncInput(rounder,
                                "#请决定是否翻看事件牌##不翻看##翻看,Y2", rstage, "0");
                            if (replyEve == "2")
                                RaiseGMessage("G1EV," + rounder + ",0");
                            else
                                WI.BCast("R" + rounder + "EV2,0");
                            rstage = "R" + rounder + "EE";
                        }
                        break;
                    case "EE":
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "GS"; break;
                    case "GS":
                        ResetAllPlayerRAM(); // 3:GR
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "GR"; break;
                    case "GR":
                        WI.BCast(rstage + ",0");
                        RunQuadMixedStage(rstage, 3, null, null);
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "GE"; break;
                    case "GE":
                        RunQuadMixedStage(rstage, 0, null, null);
                        rstage = "R" + rounder + "Z0"; break;
                    case "Z0":
                        ResetAllPlayerRAM(); // 4:ZD
                        Board.Monster1 = 0; Board.Monster2 = 0;
                        Board.RPool = 0; Board.OPool = 0;
                        Board.RPoolGain.Clear(); Board.OPoolGain.Clear();
                        Board.Battler = null;
                        RaiseGMessage("G1SG,0");
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "ZW"; break;
                    case "ZW":
                        {
                            // to substitute old ZW event on considering the capability
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

                            RunQuadMixedStage(rstage, 0, null, null);
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
                            string decision;
                            if (Board.Rounder.IsMyOpt)
                                decision = NFSAsyncInput(Board.Rounder.Uid, sMember, spsm + spsb);
                            else
                            {
                                decision = MayorAsyncInput(Board.Rounder.Uid, spsm + spsb, sMember.Except(
                                    new ushort[] { Board.Rounder.Uid }), spsn + spsb, (advUt, advStr) =>
                                    {
                                        WI.Send(rstage + "3," + advUt + "," + advStr, sMember);
                                        WI.Send(rstage + "3," + advUt, ExceptStaff(sMember));
                                        WI.Live(rstage + "3," + advUt);
                                    });
                            }
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
                                RaiseGMessage(new Artiad.AnnouceCard()
                                {
                                    Action = Artiad.AnnouceCard.Type.FLASH,
                                    Officer = Artiad.ContentRule.GetPetOwnership(cdCode, this),
                                    Genre = Card.Genre.NMB,
                                    SingleCard = cdCode
                                }.ToMessage());
                            }
                            else if (decision.StartsWith("I"))
                            {
                                ushort cdCode = ushort.Parse(decision.Substring("I".Length));
                                isFight = true; sprUid = (ushort)(cdCode + 3000);
                                RaiseGMessage(new Artiad.AnnouceCard()
                                {
                                    Action = Artiad.AnnouceCard.Type.FLASH,
                                    Officer = Artiad.ContentRule.GetExspOwnership(cdCode, this),
                                    Genre = Card.Genre.Exsp,
                                    SingleCard = cdCode
                                }.ToMessage());
                            }
                            else if (decision.StartsWith("/"))
                            {
                                isFight = false; sprUid = 0;
                            }

                            if (!isFight)
                            {
                                RaiseGMessage(new Artiad.CoachingSign()
                                {
                                    SingleUnit = new Artiad.CoachingSignUnit()
                                    {
                                        Role = Artiad.CoachingHelper.PType.GIVEUP
                                    }
                                }.ToMessage());
                                // Ensure XI.Board.Mon1From == 0
                                if (Board.Mon1From == 0 || Board.Monster1 == 0)
                                {
                                    ushort mons = DequeueOfPile(Board.MonPiles);
                                    RaiseGMessage("G2IN,1,1");
                                    WI.BCast(rstage + "7,0," + mons);
                                    RaiseGMessage(new Artiad.Abandon()
                                    {
                                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                                        Genre = Card.Genre.NMB,
                                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = mons }
                                    }.ToMessage());
                                }
                                Board.Battler = null;
                                rstage = "R" + rounder + "ZF";
                            }
                            else
                            {
                                RaiseGMessage(new Artiad.CoachingSign()
                                {
                                    SingleUnit = new Artiad.CoachingSignUnit()
                                    {
                                        Role = Artiad.CoachingHelper.PType.SUPPORTER,
                                        Coach = sprUid
                                    }
                                }.ToMessage());
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
                                if (Board.Opponent.IsMyOpt)
                                    decision = NFSAsyncInput(Board.Opponent.Uid, hMember, hnsm + hnsb);
                                else
                                {
                                    decision = MayorAsyncInput(Board.Opponent.Uid, hnsm + hnsb, hMember.Except(
                                        new ushort[] { Board.Opponent.Uid }), hnsn + hnsb, (advUt, advStr) =>
                                        {
                                            WI.Send(rstage + "3," + advUt + "," + advStr, hMember);
                                            WI.Send(rstage + "3," + advUt, ExceptStaff(hMember));
                                            WI.Live(rstage + "3," + advUt);
                                        });
                                }
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
                                    RaiseGMessage(new Artiad.AnnouceCard()
                                    {
                                        Action = Artiad.AnnouceCard.Type.FLASH,
                                        Officer = Artiad.ContentRule.GetPetOwnership(cdCode, this),
                                        Genre = Card.Genre.NMB,
                                        SingleCard = cdCode
                                    }.ToMessage());
                                }
                                else if (decision.StartsWith("I"))
                                {
                                    ushort cdCode = ushort.Parse(decision.Substring("I".Length));
                                    hndUid = (ushort)(cdCode + 3000);
                                    RaiseGMessage(new Artiad.AnnouceCard()
                                    {
                                        Action = Artiad.AnnouceCard.Type.FLASH,
                                        Officer = Artiad.ContentRule.GetExspOwnership(cdCode, this),
                                        Genre = Card.Genre.Exsp,
                                        SingleCard = cdCode
                                    }.ToMessage());
                                }
                                else if (decision.StartsWith("/"))
                                    hndUid = 0;
                                RaiseGMessage(new Artiad.CoachingSign()
                                {
                                    SingleUnit = new Artiad.CoachingSignUnit()
                                    {
                                        Role = Artiad.CoachingHelper.PType.HINDER,
                                        Coach = hndUid
                                    }
                                }.ToMessage());
                                //WI.BCast(rstage + "7,2," + Board.Hinder.Uid);
                                rstage = "R" + rounder + "ZU";
                            }
                        }
                        break;
                    case "ZU":
                        RunQuadStage(rstage);
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
                            RunQuadStage(rstage);
                            if (NMBLib.IsNPC(Board.Monster1))
                                rstage = "R" + rounder + "NP";
                            else if (NMBLib.IsMonster(Board.Monster1))
                                rstage = "R" + rounder + "Z1";
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
                            UEchoCode r5ed = HandleWithNPCEffect(Board.Rounder, npc, rstage);
                            if (r5ed == UEchoCode.NO_OPTIONS) // cannot take any action, check whether finished
                            {
                                AsyncInput(Board.Rounder.Uid, "#无法执行,//", rstage, "1");
                                if (Board.MonPiles.Count <= 0)
                                    RaiseGMessage("G1WJ,0");
                            }
                            if (r5ed != UEchoCode.END_ACTION) // not take action, skip
                            {
                                RaiseGMessage(new Artiad.Abandon()
                                {
                                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                                    Genre = Card.Genre.NMB,
                                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = Board.Monster1 }
                                }.ToMessage());
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
                                        rstage = "R" + rounder + "Z1";
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
                                    RaiseGMessage(new Artiad.Abandon()
                                    {
                                        Zone = Artiad.CustomsHelper.ZoneType.IMPLICIT,
                                        Genre = Card.Genre.NMB,
                                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = Board.Monster1 }
                                    }.ToMessage());
                                    Board.Monster1 = 0;
                                }
                                rstage = "R" + rounder + "ZF";
                            }
                            break;
                        }
                    // Monster's Silence happens here.
                    case "Z1":
                        WI.BCast(rstage + ",0");
                        RunQuadMixedStage(rstage, 0,
                            new int[] { -400, -300, -100, 300 },
                            new Action[] { () => {
                                Monster monster = Board.Battler as Monster;
                                if (monster != null && monster.IsSilence())
                                    Board.Silence.Add(Board.Battler.Code);
                            }, () => {
                                Board.InCampaign = true;
                                Board.PoolEnabled = true;
                                Board.FightTangled = false;
                                AwakeABCValue(false);
                            }, () => RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage()),
                            () => RaiseGMessage("G0CZ,2") });
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "Z8"; break;
                    case "Z8":
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "CC"; break;
                    case "CC":
                        WI.BCast(rstage + ",0");
                        RunQuadMixedStage(rstage, 0,
                            new int[] { -10, 100 },
                            new Action[] { () =>
                            {
                                if (!string.IsNullOrEmpty((Board.Battler as Monster).DebutText))
                                    Board.IsMonsterDebut = true;
                            }, () => {
                                if (Board.IsMonsterDebut) // Enable Curtain
                                    LibTuple.ML.Decode(Board.Monster1).Debut();
                            } });
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "PD"; break;
                    case "PD":
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "ZC"; break;
                    case "ZC":
                        RunQuadMixedStage(rstage, 0, new int[] { -100, 100 },
                            new Action[] { () => {
                                Board.PlayerPoolEnabled = true;
                                AwakeABCValue(true);
                            }, () => RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage()) });
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
                        RunSeperateStage(rstage, 1, delegate (Board bd)
                            { return bd.IsRounderBattleWin(); });
                        WI.BCast(rstage + ",1");
                        rstage = "R" + rounder + "ZN"; break;
                    case "ZN":
                        Board.IsBattleWin = Board.IsRounderBattleWin();
                        Board.PoolDelta = Board.CalculateRPool() - Board.CalculateOPool();
                        WI.BCast(rstage + "," + (Board.IsBattleWin ? "0" : "1"));
                        RunQuadStage(rstage);
                        Board.PoolEnabled = false;
                        Board.PlayerPoolEnabled = false;
                        rstage = "R" + rounder + "VS"; break;
                    case "VT":
                    case "VS":
                        {
                            bool skip = rstage.Substring("R#".Length) == "VT";
                            Board.Mon1Catchable = true; Board.Mon2Catchable = true;
                            if (Board.IsBattleWin) // OK, win
                            {
                                if (Board.Monster1 != 0 && !skip)
                                    RaiseGMessage("G1GE,W," + Board.Monster1);
                                bool hasMonster2 = Board.Monster2 != 0 && NMBLib.IsMonster(Board.Monster2);
                                if (hasMonster2 && !skip)
                                    RaiseGMessage("G1GE,W," + Board.Monster2);
                                if (Board.Monster1 != 0 && (Board.Mon1Catchable || skip)) // skip will get
                                {
                                    RaiseGMessage(new Artiad.HarvestPet()
                                    {
                                        Farmer = Board.Rounder.Uid,
                                        Farmland = Board.Mon1From,
                                        SinglePet = Board.Monster1,
                                        Trophy = true,
                                        Reposit = true,
                                        Plow = true
                                    }.ToMessage());
                                }
                                hasMonster2 = Board.Monster2 != 0 && NMBLib.IsMonster(Board.Monster2);
                                if (hasMonster2 && (Board.Mon2Catchable || skip))
                                {
                                    RaiseGMessage(new Artiad.HarvestPet()
                                    {
                                        Farmer = Board.Rounder.Uid,
                                        Farmland = 0,
                                        SinglePet = Board.Monster2,
                                        Trophy = true,
                                        Reposit = true,
                                        Plow = true
                                    }.ToMessage());
                                }
                            }
                            else
                            {
                                if (!skip)
                                {
                                    if (Board.Monster1 != 0)
                                        RaiseGMessage("G1GE,L," + Board.Monster1);
                                    if (Board.Monster2 != 0 && NMBLib.IsMonster(Board.Monster2))
                                        RaiseGMessage("G1GE,L," + Board.Monster2);
                                }
                            }
                            //WI.BCast(rstage + "2," + (Board.IsBattleWin ? "0" : "1"));
                            RunQuadStage(rstage);

                            RecycleMonster(); // BUG: if not taken away, here mon1zero should be false?
                            WI.BCast(rstage + "3");
                            rstage = "R" + rounder + "Z2"; break;
                        }
                    case "Z2":
                        WI.BCast(rstage + ",0");
                        foreach (Player player in Board.Garden.Values)
                            RaiseGMessage("G0AX," + player.Uid);
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "ZF"; break;
                    case "ZF":
                        WI.BCast(rstage + ",0");
                        Board.PoolEnabled = false;
                        Board.RPool = 0; Board.OPool = 0;
                        Board.RPoolGain.Clear(); Board.OPoolGain.Clear();
                        RecycleMonster();
                        if (Board.Battler as Monster != null && (Board.Battler as Monster).IsSilence())
                            Board.Silence.Remove(Board.Battler.Code);
                        foreach (Player player in Board.Garden.Values)
                            RaiseGMessage("G0AX," + player.Uid);
                        Board.CleanBattler();
                        Board.InCampaign = false;
                        RaiseGMessage("G1ZK,1");
                        RaiseGMessage("G1HK,1");
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "ZZ"; break;
                    case "ZZ":
                        RaiseGMessage(new Artiad.CoachingSign()
                        {
                            SingleUnit = new Artiad.CoachingSignUnit()
                            {
                                Role = Artiad.CoachingHelper.PType.DONE,
                                Coach = rounder
                            }
                        }.ToMessage());
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "BC";
                        break;
                    case "BC":
                        ResetAllPlayerRAM(); // 5:BC
                        WI.BCast(rstage + ",0");
                        RunQuadMixedStage(rstage, 0, new int[] { 100 },
                            new Action[] { () => {
                                int tuxCount = Board.Battler != null ? 2 : 1;
                                RaiseGMessage("G0HT," + Board.Rounder.Uid + "," + tuxCount);
                            } });
                        rstage = "R" + rounder + "QR"; break;
                    case "QR":
                        RunQuadStage(rstage);
                        RaiseGMessage("G0QR," + Board.Rounder.Uid);
                        rstage = "R" + rounder + "TM"; break;
                    case "TM":
                        ResetAllPlayerRAM(); // 6:TM
                        WI.BCast(rstage + ",0");
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "IC"; break;
                    case "IC":
                        RunQuadStage(rstage);
                        rstage = "R" + rounder + "ED"; break;
                    case "ED":
                        ResetAllPlayerRAM(); // 7:ED
                        WI.BCast(rstage + ",0");
                        RunQuadMixedStage(rstage, 0,
                            new int[] { -100, 100 },
                            new Action[] { () => {
                                Board.PoolEnabled = false;
                                Board.Battler = null; Board.CleanBattler();
                                if (Board.MonPiles.Count <= 0)
                                    RaiseGMessage("G1WJ,0");
                                RecycleMonster();
                                Board.InCampaign = false;
                                Board.PlayerPoolEnabled = Board.PoolEnabled = false;
                                RaiseGMessage("G1ZK,1");
                                RaiseGMessage("G1HK,1");
                                if (Board.Wang.Count > 0)
                                {
                                    ushort[] wangs = Board.Wang.ToArray();
                                    Board.Wang.Clear();
                                    RaiseGMessage("G0YM,3,1,0");
                                    RaiseGMessage(new Artiad.Abandon()
                                    {
                                        Zone = Artiad.CustomsHelper.ZoneType.IMPLICIT,
                                        Genre = Card.Genre.NMB,
                                        SingleUnit = new Artiad.CustomsUnit() { Cards = wangs }
                                    }.ToMessage());
                                }
                                if (Board.Eve != 0)
                                {
                                    RaiseGMessage(new Artiad.Abandon()
                                    {
                                        Zone = Artiad.CustomsHelper.ZoneType.IMPLICIT,
                                        Genre = Card.Genre.Eve,
                                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = Board.Eve }
                                    }.ToMessage());
                                    RaiseGMessage("G0YM,2,0,0");
                                    Board.Eve = 0;
                                }
                                foreach (Player player in Board.Garden.Values)
                                    RaiseGMessage("G0AX," + player.Uid);
                                if (Board.Silence.Count > 0)
                                    Board.Silence.Clear();
                            }, () => {
                                ClearLeftPendingTux();
                                Board.CsPets.Clear();
                                Board.CsEqiups.Clear();
                                if (!Board.Rounder.IsAlive && Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                                    RaiseGMessage("G0ZH,1");
                            } });
                        if (Board.GetNextPlayer(rounder) != 0)
                            rstage = "R" + Board.GetNextPlayer(rounder) + "00";
                        else if (Board.Rounder.IsAlive)
                            RaiseGMessage("G0WN," + Board.Rounder.Team);
                        else
                            RaiseGMessage("G0WN,0");
                        foreach (Player player in Board.Garden.Values)
                        {
                            player.ResetRFM();
                            player.Pets.Where(p => p != 0).Select(p => LibTuple.ML.Decode(p))
                                .Where(p => p != null).ToList().ForEach(p => p.ResetRFM());
                        }
                        break;
                }
            }
            if (rstage == "H0TM")
            {
                // 1. Don't accept reconnection request
                // 2. Sleep for 5 minutes to terminate the room
                // 3. Terminate the room if everyone leaves
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).RoomGameEnd();
                Task lastFive = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(5 * 60 * 1000);
                }, TaskCreationOptions.LongRunning);
                lastFive.Wait();
                // VI.Cout(0, "天若有情天亦老，我为长者续一秒.");
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).RoomBury();
                Environment.Exit(0);
            }
        }
        private void RunQuadStage(string zero) { RunQuadMixedStage(zero, 0, null, null); }
        // return whether actual action has been taken
        private bool RunQuadMixedStage(string zero, int sina, int[] silentPriority, Action[] silentAction)
        {
            if (!sk02.ContainsKey(zero) || sk02[zero].Count == 0) // no special SKT registered
            {
                foreach (Player py in Board.Garden.Values)
                    py.IsZhu = false;
                if (silentAction != null) // still take silent action
                    Array.ForEach(silentAction, p => p());
                return false;
            }
            sina |= 0x2; // seems weired, when did sina = 0 happens?
            var garden = Board.Garden;
            List<SKE> pocket = ParseFromSKTriples(zero, (sina & 4) != 0);

            bool[] involved = new bool[garden.Count + 1];
            string[] pris = new string[garden.Count + 1];

            bool actualAction = false;
            int priorty = int.MinValue; int silentIdx = 0;
            bool isAllThrough = false;
            do
            {
                do
                {
                    Fill(involved, false);
                    Fill(pris, "");
                    List<string> locks = new List<string>();
                    List<SKE> purse = new List<SKE>();

                    bool isAnySet = false;
                    AddZhuSkillBackward(pocket, zero, (sina & 4) != 0);
                    foreach (SKE ske in pocket)
                    {
                        if (!isAnySet && ske.Priorty < priorty)
                            continue;
                        // if not set and priority equals given event, then handle the event directly
                        if (silentPriority != null && !isAnySet)
                            while (silentIdx < silentPriority.Length && ske.Priorty > silentPriority[silentIdx])
                            {
                                priorty = silentPriority[silentIdx];
                                silentAction[silentIdx]();
                                ++silentIdx;
                            }
                        // base as the first one if not set
                        if (!isAnySet || ske.Priorty == priorty)
                        {
                            if ((sina & 1) != 0 && ske.Type == SKTType.TX && garden[ske.Tg].IsAlive)
                                involved[ske.Tg] = true;
                            bool ias = SKE2Message(ske, zero, involved, pris, locks);
                            if (ias)
                                purse.Add(ske);
                            isAnySet |= ias;
                            priorty = ske.Priorty;
                            //}
                        }
                        // if somebody added now, then re-scan it's related with $zero
                    }

                    if (!isAnySet) { isAllThrough = true; }

                    // round mode, priority not needed.
                    if (locks.Count > 0)
                    {
                        locks.Sort(LockSkillCompare);
                        // handle locks one by one
                        string msg = locks.First();
                        int idx = msg.IndexOf(',');
                        ushort me = ushort.Parse(msg.Substring(0, idx));
                        int jdx = msg.LastIndexOf(';');
                        string mai = Algo.Substring(msg, idx + 1, jdx);

                        string skName;
                        mai = DecodeSimplifiedCommand(mai, out skName);
                        SKE ske = SKE.Find(skName, me, purse);
                        if (ske != null)
                            HandleU24Message(me, involved, mai, ske);
                        //UKEvenMessage(involved, pocket, null);
                        involved[me] = true;
                        continue; // validation would be re-calculated
                    }
                    if (!garden.Keys.Where(p => involved[p]).Any())
                        break; // No skills could be called, cancel
                    if (!purse.Any(p => p.Lock != true) && (sina & 1) == 0)
                        break; // Ignore pure tux empty query if necessary
                    foreach (ushort ut in Board.Garden.Keys)
                        if (pris[ut] != "")
                            pris[ut] = pris[ut].Substring(1);
                    SendOutU1Message(involved, pris, sina, null);
                    // put u1 and u2 together
                    UEchoCode echo = UKEvenMessage(involved, purse, pris, sina);
                    actualAction |= (echo == UEchoCode.END_ACTION);
                    if (actualAction && ((sina & 4) != 0))
                        break;
                    if (echo == UEchoCode.END_TERMIN) // skill updated
                        pocket = ParseFromSKTriples(zero, (sina & 4) != 0);
                } while (!IsAllClear(involved, false));
                ++priorty;
            } while (!isAllThrough);
            RaiseGMessage("G2AS,0");
            if (silentPriority != null)
            {
                while (silentIdx < silentPriority.Length)
                    silentAction[silentIdx++]();
            }
            return actualAction;
        }

        // Run in Separate Ways
        // return whether actual action has been taken
        private bool RunSeperateStage(string zero, int sina, Func<Board, bool> judge)
        {
            var garden = Board.Garden;
            if (!sk02.ContainsKey(zero) || sk02[zero].Count == 0)
            {
                foreach (Player py in Board.Garden.Values)
                    py.IsZhu = false;
                return false;
            }
            List<SKE> pocket = ParseFromSKTriples(zero, false);

            bool[] involved = new bool[garden.Count + 1];
            string[] pris = new string[garden.Count + 1];

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
                        // handle locks one by one
                        string msg = locks.First();
                        int idx = msg.IndexOf(',');
                        ushort me = ushort.Parse(msg.Substring(0, idx));
                        int jdx = msg.LastIndexOf(';');
                        string mai = Algo.Substring(msg, idx + 1, jdx);

                        string skName;
                        mai = DecodeSimplifiedCommand(mai, out skName);
                        SKE ske = SKE.Find(skName, me, purse);
                        if (ske != null)
                            HandleU24Message(me, involved, mai, ske);
                        //UKEvenMessage(involved, pocket, null);
                        involved[me] = true;
                        continue; // validation would be re-calculated
                    }
                    if (!garden.Keys.Where(p => involved[p]).Any())
                        break; // No skills could be called, cancel
                    if (!purse.Any(p => p.Lock != true) && (sina & 1) == 0)
                        break; // Ignore pure tux empty query if necessary
                    foreach (ushort ut in Board.Garden.Keys)
                        if (pris[ut] != "")
                            pris[ut] = pris[ut].Substring(1);
                    SendOutU1Message(involved, pris, sina, null);
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
                        pocket = ParseFromSKTriples(zero, false);
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
            return true;
        }
        private UEchoCode UKEvenMessage(bool[] involved,
            List<SKE> purse, string[] pris, int sina)
        {
            return UKEvenMessage(involved, purse, pris, Algo.RepeatToArray(sina, involved.Length));
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
                Base.VW.Msgs msg = WI.RecvInfRecv();
                UEchoCode next = HandleUMessage(msg.Msg, purse, msg.From, involved, sina);
                if (next == UEchoCode.END_ACTION || next == UEchoCode.END_TERMIN)
                    //isUK5Received = true;
                    return next;
                else if (next == UEchoCode.RE_REQUEST)
                    ResendU1Message(msg.From, involved, pris, AcquireUVSN(), false, sina);
            }
            return UEchoCode.END_CANCEL;
            //return isUK5Received;
        }
        // $zero1 means $monster1 has been taken to another places, only set to 0
        // otherwise, handle with it
        private void RecycleMonster()
        {
            if (Board.Monster1 != 0)
            {
                Monster mon1 = LibTuple.ML.Decode(NMBLib.OriginalMonster(Board.Monster1));
                if (mon1 != null && Board.IsMonsterDebut)
                    mon1.Curtain();
                if (Board.Mon1From == 0)
                {
                    if (!Artiad.ContentRule.FindCardExistance(Board.Monster1, Card.Genre.NMB, this, false))
                        RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.IMPLICIT,
                            Genre = Card.Genre.NMB,
                            SingleUnit = new Artiad.CustomsUnit() { SingleCard = Board.Monster1 }
                        }.ToMessage());
                }
                RaiseGMessage("G0WB," + Board.Monster1);
                RaiseGMessage("G0YM,0,0,0");

                Board.Mon1From = 0;
                Board.Monster1 = 0;
            }
            if (Board.Monster2 != 0)
            { // monster 2 doesn't curtain, and mon2from always is 0
                if (!Artiad.ContentRule.FindCardExistance(Board.Monster2, Card.Genre.NMB, this, false))
                    RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.IMPLICIT,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = Board.Monster2 }
                    }.ToMessage());
                RaiseGMessage("G0WB," + Board.Monster2);
                RaiseGMessage("G0YM,1,0,0");
                Board.Monster2 = 0;
            }
            Board.IsMonsterDebut = false;
        }
        // Handle With NPC
        public UEchoCode HandleWithNPCEffect(Player player, NPC npc, string reason)
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
                    // special kind of nfuse = Code + reason + pureFuse
                    string nfuse = npc.Code + ";" + reason;
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
                        })
                        { Tg = player.Uid, Fuse = nfuse };
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
                if (reason == "R" + Board.Rounder.Uid + "NP")
                    RaiseGMessage("G1SG,0");
                SendOutU1Message(involved, pris, 0, null);
                while (r5ed != UEchoCode.END_ACTION && r5ed != UEchoCode.END_TERMIN && involved[rd])
                {
                    Base.VW.Msgs msg = WI.RecvInfRecv();
                    r5ed = HandleUMessage(msg.Msg, purse, msg.From, involved, 0);
                    if (r5ed == UEchoCode.RE_REQUEST)
                        ResendU1Message(msg.From, involved, pris, uvsn, false, 0);
                    else if (r5ed == UEchoCode.END_CANCEL && Board.MonPiles.Count <= 0
                            && msg.From == player.Uid)
                        ResendU1Message(player.Uid, involved, pris, uvsn, true, 0);
                    // critical, must take action.
                }
            }
            else
                r5ed = UEchoCode.NO_OPTIONS; // cannot take any action, seem as skip
            return r5ed;
        }
        private void AwakeABCValue(bool containsC)
        {
            foreach (Player py in Board.Garden.Values)
                AwakeABCValue(containsC, py);
        }
        private void AwakeABCValue(bool containsC, Player py)
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
        public void ResetAllPlayerRAM()
        {
            foreach (Player player in Board.Garden.Values)
            {
                player.ResetRAM();
                player.Pets.Where(p => p != 0).Select(p => LibTuple.ML.Decode(p))
                    .Where(p => p != null).ToList().ForEach(p => p.ResetRAM());
            }
        }
        public void ClearLeftPendingTux()
        {
            if (Board.PendingTux.Count > 0)
            {
                IDictionary<ushort, List<ushort>> imt = new Dictionary<ushort, List<ushort>>();
                foreach (string pendItem in Board.PendingTux)
                {
                    string[] pends = pendItem.Split(',');
                    ushort pendWho = ushort.Parse(pends[0]);
                    for (int i = 2; i < pends.Length; ++i)
                        Algo.AddToMultiMap(imt, pendWho, ushort.Parse(pends[i]));
                }
                RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    List = imt.Select(p => new Artiad.CustomsUnit()
                    {
                        Source = p.Key,
                        Cards = p.Value.ToArray()
                    }).ToList()
                }.ToMessage());
                Board.PendingTux.Dequeue(Board.PendingTux.Count); // Clear the pending tux
            }
        }
    }
}