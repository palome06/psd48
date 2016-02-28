using PSD.Base;
using PSD.Base.Card;
using PSD.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg
{
    public partial class XI
    {
        #region G-Loop
        // public void RaiseNGT(NGT ngt)
        // {
        //    if (ngt != null)
        //    {
        //         Log.Logger(ngt.ToMessage());
        //         Mint mint = new Mint(ngt, int.MinValue);
        //         InnerMint(mint);
        //    }
        // }
        // Raise Command from skill declaration, without Priory Control
        public void RaiseGMessage(string cmd)
        {
            if (cmd.StartsWith("G"))
            {
                Log.Logger(cmd);
                if (cmd.StartsWith("G2"))
                    SimpleGMessage100(cmd);
                else
                    InnerGMessage(cmd, int.MinValue);
            }
        }
        // Raise Command from skill declaration, with Priory appended
        public void InnerGMessage(string cmd, int priorty)
        {
            if (string.IsNullOrEmpty(cmd) || !cmd.StartsWith("G"))
                return;
            //if (past == null)
            //    past = new HashSet<string>();
            string zero = Algo.Substring(cmd, 0, cmd.IndexOf(','));
            List<SkTriple> _pocket;
            if (!sk02.TryGetValue(zero, out _pocket) || _pocket.Count == 0)
            {
                foreach (Player py in Board.Garden.Values)
                    py.IsZhu = false;
                return;
            }
            List<SKE> pocket = ParseFromSKTriples(_pocket, cmd, false);

            bool[] involved = new bool[Board.Garden.Count + 1];
            string[] roads = new string[Board.Garden.Count + 1];
            bool isTermini = false;
            bool isAnySet = false;
            do
            {
                Fill(roads, "");
                isAnySet = false;
                isTermini = false;
                Fill(involved, false);
                List<string> locks = new List<string>();
                List<SKE> purse = new List<SKE>();
                //AddZhuSkillBackward(pocket, zero, false);

                foreach (SKE ske in pocket)
                {
                    if (!isAnySet && ske.Priorty < priorty)
                        continue;
                    // base as the first one if not set
                    if (!isAnySet || ske.Priorty == priorty)
                    {
                        if (ske.Name.StartsWith("~"))
                        {
                            SimpleGMessage(cmd, ske.Priorty); return;
                        }
                        //bool ias = SKT2Message(skt, cmd, true, involved, roads, locks);
                        //if (!past.Contains(skt.ToTagString()))
                        //{
                        bool ias = SKE2Message(ske, cmd, involved, roads, locks);
                        if (ias)
                            purse.Add(ske);
                        isAnySet |= ias;
                        priorty = ske.Priorty;
                        //}
                    }
                    else break;
                }
                //if (!isAnySet) { SimpleGMessage(cmd); return; }
                if (!isAnySet) { RaiseGMessage("G2AS,0"); return; }

                // string cop = zero.Substring(2);
                isAnySet = false;
                if (locks.Count > 0)
                {
                    locks.Sort(LockSkillCompare);
                    while (!isTermini && locks.Count > 0)
                    {
                        string msg = locks.First();
                        int idx = msg.IndexOf(',');
                        ushort me = ushort.Parse(msg.Substring(0, idx));
                        int jdx = msg.LastIndexOf(';');
                        string mai = Algo.Substring(msg, idx + 1, jdx);
                        //string inType = Algo.Substring(msg, jdx + 1, -1);

                        string skName;
                        mai = DecodeSimplifiedCommand(mai, out skName);
                        SKE ske = SKE.Find(skName, me, purse);
                        if (ske != null)
                        {
                            UEchoCode echo = HandleU24Message(me, involved, mai, ske);
                            if (echo == UEchoCode.END_TERMIN)
                                isTermini = true;
                            else if (echo == UEchoCode.END_ACTION)
                                isAnySet = true;
                        }
                        RaiseGMessage("G2AS,0");
                        locks.RemoveAt(0);
                    }
                }
                if (Board.Garden.Keys.Where(p => involved[p]).Any() && !isTermini)
                {
                    if (involved[0])
                        Fill(involved, true);
                    foreach (ushort ut in Board.Garden.Keys)
                        if (roads[ut] != "")
                            roads[ut] = roads[ut].Substring(1);
                    //int sinaG = IsGameCompete ? 3 : 2;
                    int[] sinaG = new int[Board.Garden.Count + 1];
                    sinaG[0] = 2;
                    for (ushort i = 1; i <= Board.Garden.Count; ++i)
                        sinaG[i] = Board.Garden[i].IsTPOpt ? 2 : 3;
                    SendOutU1Message(involved, roads, sinaG);
                    UEchoCode echo = UKEvenMessage(involved, purse, roads, sinaG);
                    if (echo == UEchoCode.END_TERMIN)
                        isTermini = true;
                    else if (echo == UEchoCode.END_ACTION)
                        isAnySet = true;
                }
            } while (!IsAllClear(involved, false) && !isTermini);
            RaiseGMessage("G2AS,0");
            if (!isTermini)
                InnerGMessage(cmd, priorty + 1);
        }
        #endregion G-Loop

        #region G-Detail
        // Post Command from Inner, no skill called, so the basic handling
        private void SimpleGMessage(string cmd, int priority)
        {
            string[] args = cmd.Split(',');
            string cmdrst = Algo.Substring(cmd, "G0xx,".Length, -1);
            int nextPriority = priority + 1; // might change during execution
            switch (args[0])
            {
                case "G1TH":
                    if (priority == 100)
                    {
                        Artiad.HpIssueSemaphore.Telegraph(WI.BCast, Artiad.Harm.Parse(cmd).Select(p =>
                            new Artiad.HpIssueSemaphore(p.Who, false, p.Element, -p.N, Board.Garden[p.Who].HP)));
                    }
                    else if (priority == 200)
                        Artiad.Procedure.ArticuloMortis(this, WI, true);
                    break;
                case "G0ZW":
                    if (priority == 100)
                    {
                        WI.BCast("E0ZW," + cmdrst);
                        int teamMe = 0, teamOp = 0;
                        for (int i = 1; i < args.Length; ++i)
                        {
                            ushort me = ushort.Parse(args[i]);
                            Player player = Board.Garden[me];
                            player.IsAlive = false;
                            player.IsTared = false;
                            player.HP = 0;
                            if (player.Team == Board.Rounder.Team)
                                ++teamMe;
                            else if (player.Team == Board.Rounder.OppTeam)
                                ++teamOp;
                        }
                        bool teamMeSuv = false, teamOpSuv = false;
                        foreach (Player py in Board.Garden.Values)
                        {
                            if (py.Team == Board.Rounder.Team && py.IsAlive)
                                teamMeSuv = true;
                            if (py.Team == Board.Rounder.OppTeam && py.IsAlive)
                                teamOpSuv = true;
                        }
                        if (!teamMeSuv && !teamOpSuv)
                            RaiseGMessage("G0WN," + 0);
                        else if (teamMeSuv && !teamOpSuv)
                            RaiseGMessage("G0WN," + Board.Rounder.Team);
                        else if (!teamMeSuv && teamOpSuv)
                            RaiseGMessage("G0WN," + Board.Rounder.OppTeam);
                        if (!teamMeSuv || !teamOpSuv)
                            nextPriority = int.MaxValue;
                    }
                    if (priority == 200)
                    { // discard tux, pets and Escues
                        IEnumerable<ushort> players = Algo.TakeRange(args, 1, args.Length).Select(p => ushort.Parse(p));
                        RaiseGMessage("G0DH," + string.Join(",", players.Select(p => p + ",3")));
                        foreach (ushort ut in players)
                        {
                            Player player = Board.Garden[ut];
                            ushort[] pets = player.Pets.Where(p => p != 0).ToArray();
                            if (pets.Length > 0)
                                RaiseGMessage(new Artiad.LosePet() { Owner = ut, Pets = pets }.ToMessage());
                            if (player.Escue.Count > 0)
                            {
                                List<ushort> esc = player.Escue.ToList();
                                player.Escue.Clear();
                                RaiseGMessage("G2OL," + string.Join(",", esc.Select(
                                    p => (player.Uid + "," + p))));
                                RaiseGMessage("G0ON," + ut + ",M," + Algo.ListToString(esc));
                            }
                            if (player.Runes.Count > 0)
                                RaiseGMessage("G0OF," + player.Uid + "," + string.Join(",", player.Runes));
                        }
                    }
                    else if (priority == 300) // let teammates obtain tux
                    {
                        for (int i = 1; i < args.Length; ++i)
                        {
                            ushort me = ushort.Parse(args[i]);
                            Player player = Board.Garden[me];
                            string input = AsyncInput(me, "#获得补牌的,T1(p" + string.Join("p", Board.Garden.Values
                                .Where(p => p.IsAlive && p.Team == player.Team).Select(p => p.Uid)) + ")", "G0ZW", "0");
                            RaiseGMessage("G0HG," + input + ",2");
                        }
                    }
                    else if (priority == 400) // leave
                    {
                        List<Player> players = Algo.TakeRange(args, 1, args.Length)
                            .Select(p => Board.Garden[ushort.Parse(p)]).Where(p => !p.IsAlive).ToList();
                        if (players.Count > 0)
                            RaiseGMessage("G0OY," + string.Join(",", players.Select(p => "2," + p.Uid)));
                    }
                    break;
                case "G0OY":
                    if (priority == 100)
                    {
                        string g1zl = "";
                        List<Artiad.PetEffectUnit> peuList = new List<Artiad.PetEffectUnit>();
                        List<string> g0qzs = new List<string>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            int changeType = int.Parse(args[i]);
                            ushort who = ushort.Parse(args[i + 1]);
                            Player player = Board.Garden[who];
                            if (player.Weapon != 0)
                                g1zl += "," + player.Uid + "," + player.Weapon;
                            if (player.Armor != 0)
                                g1zl += "," + player.Uid + "," + player.Armor;
                            if (player.Trove != 0)
                                g1zl += "," + player.Uid + "," + player.Trove;
                            if (player.ExEquip != 0)
                                g1zl += "," + player.Uid + "," + player.ExEquip;
                            if (!player.PetDisabled)
                            {
                                ushort[] pets = player.Pets.Where(p => p != 0 &&
                                    !Board.NotActionPets.Contains(p)).ToArray();
                                if (pets.Length > 0)
                                {
                                    peuList.Add(new Artiad.PetEffectUnit()
                                    {
                                        Owner = player.Uid,
                                        Pets = pets,
                                        Reload = (changeType == 1) ? Artiad.PetEffectUnit.ReloadType.ABLE :
                                            Artiad.PetEffectUnit.ReloadType.BORROW
                                    });
                                }
                            }
                            if (changeType == 0 || changeType == 2)
                            {
                                List<ushort> excds = new List<ushort>();
                                excds.AddRange(player.ExCards);
                                if (excds.Count > 0)
                                    g0qzs.Add("G0QZ," + player.Uid + "," + string.Join(",", excds));
                                if (player.Skills.Count > 0)
                                    RaiseGMessage("G0OS," + player.Uid + ",0," + string.Join(",", player.Skills));
                                if (player.Guardian != 0)
                                    RaiseGMessage("G0MA," + player.Uid + ",0");
                                while (player.Coss.Count > 0)
                                    RaiseGMessage("G0OV," + player.Uid + ",0");
                            }
                        }
                        if (g1zl != "")
                            RaiseGMessage("G1OZ" + g1zl);
                        if (peuList.Count > 0)
                            RaiseGMessage(new Artiad.CollapsePetEffects() { List = peuList }.ToMessage());
                        foreach (string g0qz in g0qzs)
                            RaiseGMessage(g0qz);
                        WI.BCast("E0OY," + cmdrst);
                    }
                    else if (priority == 200)
                    {
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            int changeType = int.Parse(args[i]);
                            ushort who = ushort.Parse(args[i + 1]);
                            Player player = Board.Garden[who];

                            player.STRb = 0;
                            player.DEXb = 0;
                            player.SDaSet = player.SDcSet = false;
                            //if (changeType == 2)
                            //    player.HP = 0;
                            if (changeType == 0 || changeType == 2)
                                Artiad.Procedure.ErasePlayerToken(player, Board, RaiseGMessage);
                            if (!Board.BannedHero.Contains(player.SelectHero))
                                Board.HeroDises.Add(player.SelectHero);
                            player.SelectHero = 0;
                        }
                    }
                    else if (priority == 300)
                    {
                        bool rounded = false, _9ped = false;
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            int changeType = int.Parse(args[i]);
                            ushort who = ushort.Parse(args[i + 1]);
                            Player player = Board.Garden[who];
                            if (changeType != 1) // Not reset
                                player.ResetStatus();
                            if (changeType == 2 && player.Uid == Board.Rounder.Uid)
                                rounded = true;
                            else if (Board.IsAttendWar(player))
                            {
                                bool mightInvolve = false;
                                if (Board.Supporter.Uid == who)
                                {
                                    // if changeType = 1, trigger REFRESH and handle in G0IY
                                    if (changeType == 2)
                                        RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                                        {
                                            Role = Artiad.CoachingHelper.PType.SUPPORTER, Coach = 0
                                        } }.ToMessage());
                                    mightInvolve = true;
                                }
                                else if (Board.Hinder.Uid == who)
                                {
                                    if (changeType == 2)
                                        RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                                        {
                                            Role = Artiad.CoachingHelper.PType.HINDER, Coach = 0
                                        } }.ToMessage());
                                    mightInvolve = true;
                                }
                                if (mightInvolve && Board.PoolEnabled)
                                    _9ped = true;
                            }
                        }
                        if (rounded)
                            RaiseGMessage(new Artiad.Goto() { Terminal = "R" + Board.Rounder.Uid + "ED" }.ToMessage());
                        if (_9ped)
                            RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
                    }
                    break;
                case "G0CC": // prepare to use card
                    if (priority == 100)
                    {
                        // G0CC,A,T,B,TP02,17,36
                        ushort provider = ushort.Parse(args[1]);
                        ushort trigger = ushort.Parse(args[3]);
                        string cardname = args[4];
                        int hdx = cmd.IndexOf(';');
                        //int idx = cmd.IndexOf(',', hdx);
                        //int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        //string sktFuse = Algo.Substring(cmd, idx + 1, -1);
                        Tux tux = tx01[cardname];
                        string[] argv = cmd.Substring(0, hdx).Split(',');

                        if (provider != 0)
                        {
                            List<ushort> cards = Algo.TakeRange(argv, 5, argv.Length).Select(p => ushort.Parse(p))
                                .Where(p => p > 0 && Board.Garden[provider].ListOutAllCards().Contains(p)).ToList();
                            if (cards.Any())
                            {
                                RaiseGMessage("G2TZ,0," + provider + "," + string.Join(",", cards.Select(p => "C" + p)));
                                RaiseGMessage("G0OT," + provider + "," + cards.Count + "," + string.Join(",", cards));
                            }
                            Board.PendingTux.Enqueue(cards.Select(p => trigger + ",G0CC," + p));
                        }
                        else
                        {
                            List<ushort> cards = Algo.TakeRange(argv, 5, argv.Length).Select(p => ushort.Parse(p))
                                .Where(p => p > 0).ToList();
                            Board.PendingTux.Enqueue(cards.Select(p => trigger + ",G0CC," + p));
                        }
                        WI.BCast("E0CC," + Algo.Substring(cmd, "G0CC,".Length, hdx));
                    }
                    else if (priority == 200)
                    {
                        ushort adapter = ushort.Parse(args[2]);
                        ushort trigger = ushort.Parse(args[3]);

                        if (Board.Garden[trigger].IsAlive)
                        {
                            // need adapter tp set Action/Free Action
                            if (adapter == 1)
                            {
                                string cardname = args[4];
                                int hdx = cmd.IndexOf(';');
                                int idx = cmd.IndexOf(',', hdx);
                                int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                                sktInType = Artiad.ContentRule.GetLocustFreeType(cardname, sktInType);
                                string sktFuse = Algo.Substring(cmd, idx + 1, -1);
                                string[] argv = cmd.Substring(0, hdx).Split(',');
                                string[] cards = Algo.TakeRange(argv, 5, argv.Length);
                                RaiseGMessage("G0CD," + trigger + "," + adapter + "," + cardname + "," +
                                    string.Join(",", cards) + ";" + sktInType + "," + sktFuse);
                            }
                            else
                            {
                                RaiseGMessage("G0CD," + trigger + "," + adapter + "," +
                                    string.Join(",", Algo.TakeRange(args, 4, args.Length)));
                            }
                        }
                    }
                    else if (priority == 300)
                    {
                        ushort provider = ushort.Parse(args[1]);
                        ushort trigger = ushort.Parse(args[3]);
                        string cardname = args[4];
                        int hdx = cmd.IndexOf(';');
                        //int idx = cmd.IndexOf(',', hdx);
                        //int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        //string sktFuse = Algo.Substring(cmd, idx + 1, -1);
                        Base.Card.Tux tux = tx01[cardname];
                        string[] argv = cmd.Substring(0, hdx).Split(',');
                        List<ushort> cards = Algo.TakeRange(argv, 5, argv.Length).Select(p =>
                            ushort.Parse(p)).Where(p => p > 0).ToList();
                        cards.RemoveAll(p => !Board.PendingTux.Contains(trigger + ",G0CC," + p));
                        if (cards.Count > 0)
                        {
                            cards.ForEach(p => Board.PendingTux.Remove(trigger + ",G0CC," + p));
                            RaiseGMessage("G0ON," + provider + ",C," + cards.Count + "," + string.Join(",", cards));
                        }
                    }
                    break;
                case "G0HZ":
                    if (priority == 100)
                    {
                        //Board.IsTangled = true;
                        ushort mon = ushort.Parse(args[2]);
                        if (mon != 0)
                        {
                            Board.Monster2 = mon;
                            Board.FightTangled = true;
                        }
                    }
                    else if (priority == 200)
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort mon = Board.Monster2;
                        if (mon == 0)
                            WI.BCast("E0HZ,0," + who);
                        else if (NMBLib.IsMonster(mon))
                        {
                            WI.BCast("E0HZ,1," + who + "," + mon);
                            RaiseGMessage("G0YM,1," + mon + ",0");
                        }
                        else if (NMBLib.IsNPC(mon))
                            WI.BCast("E0HZ,2," + who + "," + mon);
                    }
                    else if (priority == 300)
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort mon = Board.Monster2;
                        if (mon != 0)
                        {
                            if (NMBLib.IsMonster(mon))
                            {
                                RaiseGMessage("G0IP," + Board.Rounder.OppTeam + "," +
                                    LibTuple.ML.Decode(NMBLib.OriginalMonster(mon)).STR);
                            }
                            else if (NMBLib.IsNPC(mon))
                            {
                                RaiseGMessage("G1NI," + Board.Rounder.Uid + "," + mon);
                                NPC npc = LibTuple.NL.Decode(NMBLib.OriginalNPC(mon));
                                bool doubled = false;
                                Hero nho = LibTuple.HL.InstanceHero(npc.Hero);

                                var g0 = Board.Garden.Values.Where(p => p.IsAlive &&
                                    p.Team == Board.Rounder.Team).ToList();
                                foreach (Player py in g0)
                                {
                                    Hero pero = LibTuple.HL.InstanceHero(py.SelectHero);
                                    if (pero != null)
                                    {
                                        if (pero.Spouses.Contains(npc.Hero.ToString()))
                                        {
                                            doubled = true; break;
                                        }
                                        else if (nho != null && pero.Spouses.Contains(nho.Archetype.ToString()))
                                        {
                                            doubled = true; break;
                                        }
                                        Hero par = LibTuple.HL.InstanceHero(pero.Archetype);
                                        if (par != null && par.Spouses.Contains(npc.Hero.ToString()))
                                        {
                                            doubled = true; break;
                                        }
                                    }
                                }
                                if (doubled)
                                {
                                    WI.BCast("E0HZ,3," + who + "," + mon);
                                    RaiseGMessage("G0IP," + Board.Rounder.Team + "," + (npc.STR * 2));
                                }
                                else
                                    RaiseGMessage("G0IP," + Board.Rounder.Team + "," + npc.STR);
                                RaiseGMessage("G0YM,1," + mon + ",0");
                            }
                        }
                    }
                    break;
                case "G1WJ":
                    if (priority == 100)
                    {
                        IDictionary<int, int> dicts = CalculatePetsScore();
                        Board.FinalAkaScore = dicts[1];
                        Board.FinalAoScore = dicts[2];
                    }
                    else if (priority == 200)
                    {
                        if (Board.FinalAkaScore > Board.FinalAoScore)
                            RaiseGMessage("G0WN,1"); // Aka wins if Aka is larger
                        else
                            RaiseGMessage("G0WN,2"); // Otherwise, Ao wins
                    }
                    break;
                case "G1EV":
                    if (priority == 100)
                    {
                        ushort eveCard = DequeueOfPile(Board.EvePiles);
                        RaiseGMessage("G2IN,2,1");
                        //WI.BCast("E0EV," + eveCard);
                        if (Board.Eve != 0)
                        {
                            RaiseGMessage("G0ON,10,E,1," + Board.Eve);
                            RaiseGMessage("G0YM,2,0,0");
                            Board.Eve = 0;
                        }
                        Board.Eve = eveCard;
                        RaiseGMessage("G0YM,2," + Board.Eve + ",0");
                        Base.Card.Evenement eve = LibTuple.EL.DecodeEvenement(Board.Eve);
                        if (eve != null && eve.IsSilence())
                            Board.Silence.Add(eve.Code);
                    }
                    else if (priority == 200)
                    {
                        ushort trigger = ushort.Parse(args[1]);
                        Base.Card.Evenement eve = LibTuple.EL.DecodeEvenement(Board.Eve);
                        eve.Action(Board.Garden[trigger]);
                        if (eve != null && eve.IsSilence())
                            Board.Silence.Remove(eve.Code);
                    }
                    break;
                default:
                    if (priority == 100)
                        SimpleGMessage100(cmd);
                    break;
            }
            InnerGMessage(cmd, nextPriority);
        }
        // Post Command from Inner, no skill called, so the basic handling
        private void SimpleGMessage100(string cmd)
        {
            string[] args = cmd.Split(',');
            string cmdrst = Algo.Substring(cmd, cmd.IndexOf(',') + 1, -1);
            var g = Board.Garden;
            if (args[0].StartsWith("G2"))
            {
                if (args[0].StartsWith("G2FU"))
                {
                    ushort type = ushort.Parse(args[1]);
                    if (type == 0)
                    {
                        ushort op = ushort.Parse(args[2]);
                        ushort nofp = ushort.Parse(args[3]);
                        char cap = args[4 + nofp][0];
                        if (op == 0)
                        {
                            if (nofp != 0)
                            {
                                ushort[] invs = Algo.TakeRange(args, 4, 4 + nofp).Select(p => ushort.Parse(p)).ToArray();
                                WI.Send("E0FU,0," + cap + "," + string.Join(",", Algo.TakeRange(args, 5 + nofp, args.Length)), invs);
                                WI.Send("E0FU,1," + cap + "," + (args.Length - 5 - nofp), ExceptStaff(invs));
                                WI.Live("E0FU,1," + cap + "," + (args.Length - 5 - nofp));
                            }
                            else
                                WI.BCast("E0FU,0," + cap + "," + string.Join(",", Algo.TakeRange(args, 5, args.Length)));
                        }
                        else
                        {
                            if (nofp != 0)
                            {
                                ushort[] invs = Algo.TakeRange(args, 4, 4 + nofp).Select(p => ushort.Parse(p)).ToArray();
                                WI.Send("E0FU,0," + cap + "," + string.Join(",", Algo.TakeRange(
                                    args, 5 + nofp, args.Length)), invs.Except(new ushort[] { op }).ToArray());
                                WI.Send("E0FU,1," + cap + "," + (args.Length - 5 - nofp), ExceptStaff(invs));
                                WI.Live("E0FU,1," + cap + "," + (args.Length - 5 - nofp));
                                WI.Send("E0FU,4," + cap + "," + string.Join(",", Algo.TakeRange(args, 5 + nofp, args.Length)), 0, op);
                            }
                            else
                            {
                                WI.Send("E0FU,0," + cap + "," + string.Join(",", Algo.TakeRange(
                                    args, 5, args.Length)), ExceptStaff(op));
                                WI.Live("E0FU,0," + cap + "," + string.Join(",", Algo.TakeRange(args, 5, args.Length)));
                                WI.Send("E0FU,4," + cap + "," + string.Join(",", Algo.TakeRange(args, 5 + nofp, args.Length)), 0, op);
                            }
                        }
                    }
                    else if (type == 3)
                        WI.BCast("E0FU,3");
                }
                else if (args[0].StartsWith("G2QU"))
                {
                    if (args[1] == "0")
                    {
                        string pile = args[2];
                        ushort nofp = ushort.Parse(args[3]);
                        if (nofp != 0)
                        {
                            ushort[] invs = Algo.TakeRange(args, 4, 4 + nofp).Select(p => ushort.Parse(p)).ToArray();
                            WI.Send("E0QU,0," + pile + "," + string.Join(",", Algo.TakeRange(args, 4 + nofp, args.Length)), invs);
                            WI.Send("E0QU,1," + pile + "," + (args.Length - 4 - nofp), ExceptStaff(invs));
                            WI.Live("E0QU,1," + pile + "," + (args.Length - 4 - nofp));
                        }
                        else
                            WI.BCast("E0QU,0," + pile + "," + string.Join(",", Algo.TakeRange(args, 4, args.Length)));
                    }
                    else if (args[1] == "1")
                        WI.BCast("E0QU,2");
                }
                else
                    WI.BCast("E0" + cmd.Substring("G2".Length));
            }
            switch (args[0])
            {
                case "G0IT": // actual obtain Tux
                    {
                        IDictionary<ushort, string> msgs = new Dictionary<ushort, string>();
                        foreach (ushort ut in Board.Garden.Keys)
                            msgs.Add(ut, "");
                        msgs.Add(0, "");
                        for (int idx = 1; idx < args.Length;)
                        {
                            ushort who = ushort.Parse(args[idx]);
                            int n = int.Parse(args[idx + 1]);
                            if (n > 0)
                            {
                                List<ushort> cards = Algo.TakeRange(args, idx + 2, idx + 2 + n)
                                    .Select(p => ushort.Parse(p)).ToList();
                                g[who].Tux.AddRange(cards);
                                msgs[who] += ("," + who + ",0," + n + "," + string.Join(",", cards));
                                foreach (ushort ut in Board.Garden.Keys.Where(p => p != who))
                                    msgs[ut] += ("," + who + ",1," + n);
                                msgs[0] += ("," + who + ",1," + n);
                            }
                            idx += (2 + n);
                        }
                        foreach (var pair in msgs)
                        {
                            if (pair.Value.Length > 0)
                            {
                                if (pair.Key != 0)
                                    WI.Send("E0IT" + pair.Value, 0, pair.Key);
                                else
                                    WI.Live("E0IT" + pair.Value);
                            }
                        }
                    }
                    break;
                case "G0OT": // actual lose Tux
                    {
                        IDictionary<ushort, string> msgs = new Dictionary<ushort, string>();
                        IDictionary<ushort, List<ushort>> lossTux = new Dictionary<ushort, List<ushort>>();
                        foreach (ushort ut in Board.Garden.Keys)
                        {
                            msgs.Add(ut, "");
                            lossTux.Add(ut, new List<ushort>());
                        }
                        msgs.Add(0, "");
                        for (int idx = 1; idx < args.Length;)
                        {
                            ushort who = ushort.Parse(args[idx]);
                            int n = int.Parse(args[idx + 1]);
                            List<ushort> cards = Algo.TakeRange(args, idx + 2, idx + 2 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            List<ushort> bright = new List<ushort>();
                            Player py = g[who];
                            foreach (ushort card in cards)
                            {
                                if (py.Tux.Contains(card))
                                    py.Tux.Remove(card);
                                if (py.Weapon == card)
                                {
                                    if (!py.WeaponDisabled)
                                    {
                                        Tux tx = LibTuple.TL.DecodeTux(py.Weapon);
                                        TuxEqiup te = tx as TuxEqiup;
                                        te.DelAction(py);
                                    }
                                    RaiseGMessage("G1OZ," + who + "," + py.Weapon);
                                    py.Weapon = 0; bright.Add(card);
                                }
                                if (py.Armor == card)
                                {
                                    if (!py.ArmorDisabled)
                                    {
                                        Tux tx = LibTuple.TL.DecodeTux(py.Armor);
                                        TuxEqiup te = tx as TuxEqiup;
                                        te.DelAction(py);
                                    }
                                    RaiseGMessage("G1OZ," + who + "," + py.Armor);
                                    py.Armor = 0; bright.Add(card);
                                }
                                if (py.Trove == card)
                                {
                                    if (!py.LuggageDisabled)
                                    {
                                        Tux tx = LibTuple.TL.DecodeTux(py.Trove);
                                        TuxEqiup te = tx as TuxEqiup;
                                        te.DelAction(py);
                                    }
                                    RaiseGMessage("G1OZ," + who + "," + py.Trove);
                                    py.Trove = 0; bright.Add(card);
                                }
                                if (py.ExEquip == card)
                                {
                                    Tux tx = LibTuple.TL.DecodeTux(card);
                                    if (tx.Type == Tux.TuxType.WQ && !py.WeaponDisabled)
                                    {
                                        TuxEqiup te = tx as TuxEqiup;
                                        te.DelAction(py);
                                    }
                                    else if (tx.Type == Tux.TuxType.FJ && !py.ArmorDisabled)
                                    {
                                        TuxEqiup te = tx as TuxEqiup;
                                        te.DelAction(py);
                                    }
                                    else if (tx.Type == Tux.TuxType.XB && !py.LuggageDisabled)
                                    {
                                        TuxEqiup te = tx as TuxEqiup;
                                        te.DelAction(py);
                                    }
                                    RaiseGMessage("G1OZ," + who + "," + py.ExEquip);
                                    py.ExEquip = 0; bright.Add(card);
                                }
                                if (py.Fakeq.ContainsKey(card))
                                {
                                    py.Fakeq.Remove(card); bright.Add(card);
                                }
                                if (py.ExCards.Contains(card))
                                {
                                    py.ExCards.Remove(card); bright.Add(card);
                                }
                            }
                            if (bright.Count > 0)
                            {
                                string item = "," + who + ",0," + bright.Count + "," + string.Join(",", bright);
                                foreach (ushort ut in Board.Garden.Keys.Where(p => p != who))
                                    msgs[ut] += item;
                                msgs[0] += item;
                            }
                            if (cards.Count - bright.Count > 0)
                            {
                                string item = "," + who + ",1," + (cards.Count - bright.Count);
                                foreach (ushort ut in Board.Garden.Keys.Where(p => p != who))
                                    msgs[ut] += item;
                                msgs[0] += item;
                                lossTux[who].AddRange(cards.Except(bright));
                            }
                            msgs[who] += ("," + who + ",0," + n + "," + string.Join(",", cards));
                            idx += (2 + n);
                        }
                        foreach (var pair in msgs)
                        {
                            if (pair.Value.Length > 0)
                            {
                                if (pair.Key != 0)
                                    WI.Send("E0OT" + pair.Value, 0, pair.Key);
                                else
                                    WI.Live("E0OT" + pair.Value);
                            }
                        }
                        if (lossTux.Any(p => p.Value.Count > 0))
                        {
                            RaiseGMessage("G1LY," + string.Join(",", lossTux.Where(p => p.Value.Count > 0)
                                .Select(p => p.Key + "," + p.Value.Count + "," + string.Join(",", p.Value))));
                        }
                    }
                    break;
                case "G0HQ": // Get Cards
                    {
                        ushort type = ushort.Parse(args[1]);
                        ushort me = ushort.Parse(args[2]);
                        if (type == 0)
                        { // player to player
                            int idx = 3;
                            while (idx < args.Length)
                            {
                                ushort from = ushort.Parse(args[idx]);
                                ushort utype = ushort.Parse(args[idx + 1]);
                                int n = int.Parse(args[idx + 2]);
                                if (utype == 0 || utype == 1)
                                {
                                    List<ushort> card = Algo.TakeRange(args, idx + 3, idx + 3 + n)
                                        .Select(p => ushort.Parse(p)).ToList();
                                    RaiseGMessage("G0OT," + from + "," + n + "," + string.Join(",", card));
                                    RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", card));
                                    if (utype == 0)
                                        WI.BCast("E0HQ,0," + me + "," + from + ",0," + n + "," + string.Join(",", card));
                                    else if (utype == 1)
                                    {
                                        ushort[] invs = me != from ? new ushort[] { me, from } : new ushort[] { me };
                                        WI.Send("E0HQ,0," + me + "," + from + ",0," + n + "," + string.Join(",", card), invs);
                                        WI.Send("E0HQ,0," + me + "," + from + ",1," + n, ExceptStaff(invs));
                                        WI.Live("E0HQ,0," + me + "," + from + ",1," + n);
                                    }
                                    idx += (3 + n);
                                }
                                else if (utype == 2)
                                {
                                    if (g[from].Tux.Count > 0 && from != me)
                                    {
                                        Player fromPlayer = g[from];
                                        List<ushort> vals = fromPlayer.Tux.Except(Board.ProtectedTux).ToList();
                                        if (vals.Count <= n)
                                            n = vals.Count;
                                        vals.Shuffle();
                                        List<ushort> card = vals.Take(n).ToList();
                                        RaiseGMessage("G0OT," + from + "," + n + "," + string.Join(",", card));
                                        RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", card));
                                        ushort[] invs = new ushort[] { me, from };
                                        WI.Send("E0HQ,0," + me + "," + from + ",0," + n + "," + string.Join(",", card), invs);
                                        WI.Send("E0HQ,0," + me + "," + from + ",1," + n, ExceptStaff(invs));
                                        WI.Live("E0HQ,0," + me + "," + from + ",1," + n);
                                    }
                                    idx += 3;
                                }
                            }
                        }
                        else if (type == 1)
                        {// all:player to player
                            for (int idx = 3; idx < args.Length; ++idx)
                            {
                                ushort from = ushort.Parse(args[idx]);
                                Player fromPlayer = g[from];
                                List<ushort> txs = fromPlayer.Tux.Except(Board.ProtectedTux).ToList();
                                if (txs.Count > 0)
                                {
                                    var card = fromPlayer.Tux.Except(Board.ProtectedTux).ToList();
                                    var n = card.Count;
                                    RaiseGMessage("G0OT," + from + "," + n + "," + string.Join(",", card));
                                    RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", card));
                                    ushort[] invs = new ushort[] { me, from };
                                    WI.Send("E0HQ,0," + me + "," + from + ",0," + n + "," + string.Join(",", card), invs);
                                    WI.Send("E0HQ,0," + me + "," + from + ",1," + n, ExceptStaff(invs));
                                    WI.Live("E0HQ,0," + me + "," + from + ",1," + n);
                                }
                                List<ushort> cds = fromPlayer.ListOutAllEquips().Except(Board.ProtectedTux).ToList();
                                if (cds.Count > 0)
                                {
                                    var n = cds.Count;
                                    var card = cds;
                                    RaiseGMessage("G0OT," + from + "," + n + "," + string.Join(",", card));
                                    RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", card));
                                    WI.BCast("E0HQ,0," + me + "," + from + ",0," + n + "," + string.Join(",", card));
                                }
                            }
                        }
                        else if (type == 2)
                        {
                            ushort utype = ushort.Parse(args[3]);
                            if (utype == 0)
                            {
                                ushort seesz = ushort.Parse(args[4]);
                                List<ushort> card = Algo.TakeRange(args, 5 + seesz, args.Length)
                                    .Select(p => ushort.Parse(p)).ToList();
                                ushort[] pzs = Board.PZone.Intersect(card).ToArray();
                                if (pzs.Length > 0)
                                {
                                    RaiseGMessage("G2CN," + string.Join(",", pzs));
                                    foreach (ushort pz in pzs)
                                        Board.PZone.Remove(pz);
                                }
                                ushort[] pls = Board.TuxPiles.Intersect(card).ToArray();
                                if (pls.Length > 0)
                                {
                                    RaiseGMessage("G2IN,0," + pls.Count());
                                    foreach (ushort pl in pls)
                                        Board.TuxPiles.Remove(pl);
                                }
                                //Board.TuxDises.Intersect(card);
                                VI.Cout(0, "{0}正面向上获得手牌{1}.", DisplayPlayer(me), DisplayTux(card));
                                RaiseGMessage("G0IT," + me + "," + card.Count() + "," + string.Join(",", card));
                                if (seesz == 0)
                                    WI.BCast("E0HQ,2," + me + "," + string.Join(",", card));
                                else
                                {
                                    ushort[] invs = Algo.TakeRange(args, 5, 5 + seesz)
                                        .Select(p => ushort.Parse(p)).ToArray();
                                    WI.Send("E0HQ,2," + me + "," + string.Join(",", card), invs);
                                    WI.Send("E0HQ,3," + me + "," + card.Count, ExceptStaff(invs));
                                    WI.Live("E0HQ,3," + me + "," + card.Count);
                                }
                            }
                            else if (utype == 1)
                            {
                                int n = int.Parse(args[4]);
                                RaiseGMessage("G2IN,0," + n);
                                ushort[] tuxs = DequeueOfPile(Board.TuxPiles, n);
                                RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", tuxs));
                                WI.Send("E0HQ,2," + me + "," + string.Join(",", tuxs), 0, me);
                                WI.Send("E0HQ,3," + me + "," + tuxs.Length, ExceptStaff(me));
                                WI.Live("E0HQ,3," + me + "," + tuxs.Length);
                            }
                            else if (utype == 2)
                            {
                                ushort[] tuxs = Algo.TakeRange(args, 4, args.Length)
                                    .Select(p => ushort.Parse(p)).ToArray();
                                RaiseGMessage("G0IT," + me + "," + tuxs.Length + "," + string.Join(",", tuxs));
                                WI.Send("E0HQ,2," + me + "," + string.Join(",", tuxs), 0, me);
                                WI.Send("E0HQ,3," + me + "," + tuxs.Length, ExceptStaff(me));
                                WI.Live("E0HQ,3," + me + "," + tuxs.Length);
                            }
                        }
                        else if (type == 3)
                        {
                            for (int idx = 3; idx < args.Length;)
                            {
                                //ushort fromZone = ushort.Parse(args[idx]);
                                int n = int.Parse(args[idx + 1]);
                                ushort[] tuxes = Algo.TakeRange(args, idx + 2, idx + 2 + n)
                                    .Select(p => ushort.Parse(p)).ToArray();
                                RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", tuxes));
                                idx += (n + 2);
                            }
                            WI.BCast("E0HQ,4," + cmdrst.Substring("3,".Length));
                        }
                        else if (type == 4) // Tux Exchange
                        {
                            List<string> msg = new List<string>();
                            List<string> nsg = new List<string>();
                            int idx = 2;
                            ushort u1 = ushort.Parse(args[idx++]);
                            ushort u2 = ushort.Parse(args[idx++]);
                            int n1 = ushort.Parse(args[idx++]);
                            int n2 = ushort.Parse(args[idx++]);
                            string t1 = string.Join(",", Algo.TakeRange(args, idx, idx + n1));
                            if (n1 > 0)
                            {
                                msg.Add(u1 + "," + n1 + "," + t1);
                                nsg.Add(u2 + "," + n1 + "," + t1);
                            }
                            idx += n1;
                            string t2 = string.Join(",", Algo.TakeRange(args, idx, idx + n2));
                            if (n2 > 0)
                            {
                                msg.Add(u2 + "," + n2 + "," + t2);
                                nsg.Add(u1 + "," + n2 + "," + t2);
                            }
                            if (msg.Count > 0)
                            {
                                RaiseGMessage("G0OT," + string.Join(",", msg));
                                RaiseGMessage("G0IT," + string.Join(",", nsg));
                                ushort[] invs = new ushort[] { u1, u2 };
                                if (n2 > 0)
                                {
                                    WI.Send("E0HQ,0," + u1 + "," + u2 + ",0," + n2 + "," + t2, invs);
                                    WI.Send("E0HQ,0," + u1 + "," + u2 + ",1," + n2, ExceptStaff(invs));
                                    WI.Live("E0HQ,0," + u1 + "," + u2 + ",1," + n2);
                                }
                                if (n1 > 0)
                                {
                                    WI.Send("E0HQ,0," + u2 + "," + u1 + ",0," + n1 + "," + t1, invs);
                                    WI.Send("E0HQ,0," + u2 + "," + u1 + ",1," + n1, ExceptStaff(invs));
                                    WI.Live("E0HQ,0," + u2 + "," + u1 + ",1," + n1);
                                }
                            }
                        }
                    }
                    break;
                case "G0QZ":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> cards = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToList();
                        WI.BCast("E0QZ," + who + "," + string.Join(",", cards));
                        List<ushort> inTux = Board.Garden[who].Tux.Intersect(cards).ToList();
                        List<ushort> allOrdered = inTux.ToList(); allOrdered.AddRange(cards.Except(inTux).ToList());
                        RaiseGMessage("G0OT," + who + "," + cards.Count + "," + string.Join(",", cards));
                        RaiseGMessage("G1DI," + who + ",1," + cards.Count + "," +
                            inTux.Count + "," + string.Join(",", allOrdered));
                    }
                    break;
                case "G0DH":
                    {
                        IDictionary<ushort, string> discards = new Dictionary<ushort, string>();

                        IDictionary<ushort, List<ushort>> gains = new Dictionary<ushort, List<ushort>>();
                        IDictionary<ushort, List<ushort>> loses = new Dictionary<ushort, List<ushort>>();
                        //List<string> losers = new List<string>();
                        for (int i = 1; i < args.Length;)
                        {
                            ushort me = ushort.Parse(args[i]);
                            ushort lose = ushort.Parse(args[i + 1]);
                            if (lose == 0) // Get Card
                            {
                                int n = int.Parse(args[i + 2]);
                                ushort[] tuxs = DequeueOfPile(Board.TuxPiles, n);
                                if (tuxs.Length > 0)
                                    gains.Add(me, tuxs.ToList());
                                //g1di += "," + me + ",0," + tuxs.Length + "," + string.Join(",", tuxs);
                                i += 3;
                            }
                            else if (lose == 1) // Lose Card
                            {
                                int n = int.Parse(args[i + 2]);
                                var tuxes = Board.Garden[me].Tux.Except(Board.ProtectedTux).ToList();
                                if (n > tuxes.Count)
                                    n = tuxes.Count;
                                if (n > 0)
                                    discards.Add(me, "#待弃置的,Q" + n + "(p" + string.Join("p", tuxes) + ")");
                                i += 3;
                            }
                            else if (lose == 2) // Lose Random Card
                            {
                                int n = int.Parse(args[i + 2]);
                                var tuxes = Board.Garden[me].Tux.Except(Board.ProtectedTux).ToList();
                                if (n > tuxes.Count)
                                    n = tuxes.Count;
                                if (n > 0)
                                {
                                    if (n == tuxes.Count)
                                        loses.Add(me, tuxes.ToList());
                                    else
                                    {
                                        tuxes.Shuffle();
                                        var range = tuxes.Take(n).ToList();
                                        if (range.Count > 0)
                                            loses.Add(me, range);
                                    }
                                }
                                i += 3;
                            }
                            else if (lose == 3)
                            { // Lose All Cards
                                var range = g[me].ListOutAllCards().Except(Board.ProtectedTux).ToList();
                                if (range.Count > 0)
                                    loses.Add(me, range);
                                i += 2;
                            }
                            else
                                break;
                        }
                        IDictionary<ushort, string> result = MultiAsyncInput(discards);
                        foreach (var pair in result)
                        {
                            List<ushort> dis = pair.Value.Split(',').Select(p => ushort.Parse(p)).ToList();
                            if (dis.Count > 0)
                                loses.Add(pair.Key, dis);
                        }
                        int onCount = loses.Sum(p => p.Value.Count);
                        int inCount = gains.Sum(p => p.Value.Count);
                        string g1di = "";
                        if (inCount > 0)
                        {
                            string g1it = string.Join(",", gains.Select(p =>
                                p.Key + "," + p.Value.Count + "," + string.Join(",", p.Value)));
                            g1di += "," + string.Join(",", gains.Select(p =>
                                p.Key + ",0," + p.Value.Count + "," + p.Value.Count + "," + string.Join(",", p.Value)));
                            RaiseGMessage("G0IT," + g1it);
                            RaiseGMessage("G2IN,0," + inCount);
                            foreach (var pair in gains)
                            {
                                int count = pair.Value.Count();
                                WI.Send("E0HQ,2," + pair.Key + "," + string.Join(",", pair.Value), 0, pair.Key);
                                WI.Send("E0HQ,3," + pair.Key + "," + count, ExceptStaff(pair.Key));
                                WI.Live("E0HQ,3," + pair.Key + "," + count);
                            }
                        }
                        if (onCount > 0)
                        {
                            string g1ot = "";
                            foreach (var pair in loses)
                            {
                                g1ot += "," + pair.Key + "," + pair.Value.Count + "," + string.Join(",", pair.Value);
                                List<ushort> inTux = Board.Garden[pair.Key].Tux.Intersect(pair.Value).ToList();
                                List<ushort> allOrdered = inTux.ToList();
                                allOrdered.AddRange(pair.Value.Except(inTux).ToList());
                                g1di += "," + pair.Key + ",1," + pair.Value.Count + "," + inTux.Count +
                                    "," + string.Join(",", pair.Value);
                            }
                            foreach (var pair in loses)
                                WI.BCast("E0QZ," + pair.Key + "," + string.Join(",", pair.Value));
                            RaiseGMessage("G0OT" + g1ot);
                        }
                        if (g1di != "")
                            RaiseGMessage("G1DI" + g1di);
                    }
                    break;
                case "G0OH":
                    {
                        List<Artiad.Harm> harms = Artiad.Harm.Parse(cmd);
                        // FiveElement.YINN
                        List<Artiad.Harm> yinns = harms.Where(p => p.Element == FiveElement.YINN &&
                            !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask)).ToList();
                        if (yinns.Count > 0)
                        {
                            IDictionary<ushort, string> yinnAsk = new Dictionary<ushort, string>();
                            yinns.ForEach(p => yinnAsk[p.Who] = "#是否弃置全部手牌抵御阴伤##弃置手牌##HP-" + p.N + ",Y2");
                            IDictionary<ushort, string> yinnAns = MultiAsyncInput(yinnAsk);
                            List<Player> escapes = new List<Player>();
                            foreach (var pair in yinnAns)
                            {
                                if (pair.Value == "1")
                                {
                                    Player py = Board.Garden[pair.Key];
                                    if (py.IsAlive && py.Tux.Count > 0)
                                        escapes.Add(py);
                                    harms.RemoveAll(p => p.Who == pair.Key && p.Element == FiveElement.YINN &&
                                        !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
                                }
                            }
                            if (escapes.Count > 0)
                                RaiseGMessage("G0DH," + string.Join(",", escapes.Select(p => p.Uid + ",2," + p.Tux.Count)));
                        }
                        foreach (Artiad.Harm harm in harms)
                        {
                            Player py = Board.Garden[harm.Who];
                            if (!py.IsValidPlayer() || harm.N < 0) { harm.N = -1; continue; }
                            if (harm.N > 0)
                                harm.N = (harm.N < py.HP) ? harm.N : py.HP;
                            if (HPEvoMask.ALIVE.IsSet(harm.Mask) && harm.N == py.HP)
                                harm.N = py.HP - 1;
                            py.HP -= harm.N;
                        }
                        harms.RemoveAll(p => p.N <= 0);
                        if (harms.Count > 0)
                            RaiseGMessage(Artiad.HarmResult.ToMessage(harms));
                    }
                    break;
                case "G0IH":
                    {
                        List<Artiad.Cure> cures = Artiad.Cure.Parse(cmd);
                        foreach (Artiad.Cure cure in cures)
                        {
                            Player py = Board.Garden[cure.Who];
                            if (py.IsValidPlayer())
                            {
                                if (py.HP + cure.N >= py.HPb)
                                    cure.N = (py.HPb - py.HP);
                                py.HP += cure.N;
                            }
                        }
                        if (cures.Any(p => p.N != 0))
                        {
                            Artiad.HpIssueSemaphore.Telegraph(WI.BCast, cures.Where(p => p.N != 0).Select(p => 
                                new Artiad.HpIssueSemaphore(p.Who, false, p.Element, p.N, Board.Garden[p.Who].HP)));
                        }
                        break;
                    }
                case "G0ZH": // cmdrst of G0ZH won't affect the operations.
                    {
                        IDictionary<ushort, int> loses = new Dictionary<ushort, int>();
                        IDictionary<ushort, List<string>> gains = new Dictionary<ushort, List<string>>();
                        List<Player> zeros = Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0 && !p.Loved).ToList();
                        foreach (Player player in zeros)
                        {
                            List<string> candidates = new List<string>();
                            Hero hero = LibTuple.HL.InstanceHero(player.SelectHero);
                            List<string> spCollection = hero.Spouses.ToList();
                            spCollection.AddRange(player.ExSpouses);
                            foreach (string spos in spCollection.Distinct())
                            {
                                if (!spos.StartsWith("!"))
                                {
                                    int spo = int.Parse(spos); // 10303,danshiwoyou,10304
                                    //var pys = Board.Garden.Values.Where(
                                    //    p => p.IsAlive && p.HP > 0 && spo == p.SelectHero);
                                    HashSet<Player> pys = new HashSet<Player>();
                                    foreach (Player py in Board.Garden.Values)
                                    {
                                        if (py.IsAlive && py.HP > 0)
                                        {
                                            if (py.SelectHero == spo)
                                                pys.Add(py);
                                            else
                                            {
                                                Hero hro = LibTuple.HL.InstanceHero(py.SelectHero);
                                                if (hro != null && hro.Archetype == spo)
                                                    pys.Add(py);
                                            }
                                        }
                                    }
                                    if (pys.Count > 0)
                                    {
                                        Player py = pys.First();
                                        candidates.Add(py.Uid.ToString());
                                        player.Loved = true;
                                        if (loses.ContainsKey(py.Uid))
                                            loses[py.Uid] = loses[py.Uid] + 1;
                                        else
                                            loses.Add(py.Uid, 1);
                                    }
                                }
                                else
                                {
                                    int spo = int.Parse(spos.Substring("!".Length));
                                    //!1:MurongZiying, !5:Yushen, !6:Kongxiu
                                    if (spo == 1 || spo == 5 || spo == 6)
                                    {
                                        Func<Player, bool> genJudge;
                                        if (spo == 5)
                                            genJudge = p => p.Gender == 'F';
                                        else if (spo == 6)
                                            genJudge = p => p.Gender == 'M';
                                        else
                                            genJudge = p => true;
                                        var pos = Board.Garden.Values.Where(p => p.IsAlive &&
                                            p.HP > 0 && p.Uid != player.Uid && genJudge(p)).Select(p => p.Uid);
                                        if (pos.Any())
                                        {
                                            string input = AsyncInput(player.Uid, "#您倾慕,/T1(p" +
                                                string.Join("p", pos) + ")", "G0LV", "0");
                                            player.Loved = true;
                                            if (input != "0" && input != "" && !input.StartsWith("/"))
                                            {
                                                ushort ut = ushort.Parse(input);
                                                candidates.Add(ut.ToString());
                                                if (loses.ContainsKey(ut))
                                                    loses[ut] = loses[ut] + 1;
                                                else
                                                    loses.Add(ut, 1);
                                            }
                                        }
                                    }
                                    else if (spo == 2) // !2:Baiyue
                                    {
                                        ushort card = LibTuple.ML.Encode("GS04");
                                        if (Board.Monster1 == card || Board.Monster2 == card ||
                                            Board.Garden.Values.Where(p => p.Pets.Contains(card)).Any())
                                        {
                                            player.Loved = true;
                                            candidates.Add("!PT" + card);
                                        }
                                    }
                                    // !3:TR-Lingyin, !4:TR-Xuanji, !7-QiliXiaoyuan
                                    else if (spo == 3 || spo == 4 || spo == 7)
                                    {
                                        Func<Player, bool> genJudge;
                                        if (spo == 3)
                                            genJudge = p => LibTuple.HL
                                                .InstanceHero(p.SelectHero).Bio.Contains("A");
                                        else if (spo == 4)
                                            genJudge = p => LibTuple.HL
                                                .InstanceHero(p.SelectHero).Bio.Contains("B");
                                        else if (spo == 7)
                                            genJudge = p => LibTuple.HL
                                                .InstanceHero(p.SelectHero).Bio.Contains("D");
                                        else
                                            genJudge = p => true;
                                        var pys = Board.Garden.Values.Where(p => p.IsAlive && p.HP > 0 &&
                                            p.SelectHero != player.SelectHero && genJudge(p)).ToList();
                                        foreach (Player py in pys)
                                        {
                                            candidates.Add(py.Uid.ToString());
                                            player.Loved = true;
                                            if (loses.ContainsKey(py.Uid))
                                                loses[py.Uid] = loses[py.Uid] + 1;
                                            else
                                                loses.Add(py.Uid, 1);
                                        }
                                    }
                                    else if (spo == 8) // !8:Mojian
                                    {
                                        ushort cardId = (LibTuple.TL.EncodeTuxCode("WQ04") as TuxEqiup).SingleEntry;
                                        bool found = false;
                                        foreach (Player py in Board.Garden.Values)
                                        {
                                            foreach (ushort eq in py.ListOutAllEquips())
                                            {
                                                if (eq == cardId)
                                                    found = true;
                                                Tux tux = LibTuple.TL.DecodeTux(eq);
                                                if (tux.IsTuxEqiup())
                                                {
                                                    TuxEqiup tue = tux as TuxEqiup;
                                                    if (tue.IsLuggage())
                                                    {
                                                        Luggage lg = tue as Luggage;
                                                        if (lg.Capacities.Contains("C" + cardId))
                                                            found = true;
                                                    }
                                                }
                                            }
                                            if (py.TokenExcl.Contains("C" + cardId))
                                                found = true;
                                        }
                                        if (found)
                                        {
                                            player.Loved = true;
                                            candidates.Add("!WQ04");
                                        }
                                    }
                                }
                            }
                            if (candidates.Count > 0)
                                gains.Add(player.Uid, candidates);
                        }
                        if (gains.Count > 0)
                        {
                            RaiseGMessage(Artiad.Love.ToMessage(gains.Select(p =>
                                new Artiad.Love(p.Key, p.Value))));
                        }
                        // if HP is still 0, then marked as death
                        zeros = Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).ToList();
                        if (zeros.Count > 0)
                            RaiseGMessage("G0ZW," + string.Join(",", zeros.Select(p => p.Uid)));
                    }
                    break;
                case "G0LV":
                    {
                        WI.BCast("E0LV," + cmdrst);
                        List<Artiad.Love> loves = Artiad.Love.Parse(cmd);
                        IDictionary<Player, int> change = new Dictionary<Player, int>();
                        foreach (Artiad.Love love in loves)
                        {
                            Player py = Board.Garden[love.Princess];
                            int n = love.Prince.Count;
                            Algo.PlusToMap(change, py, n);
                            foreach (string prince in love.Prince)
                            {
                                if (!prince.StartsWith("!"))
                                {
                                    ushort pr = ushort.Parse(prince);
                                    Player giver = Board.Garden[pr];
                                    if (giver != null)
                                        Algo.PlusToMap(change, giver, -1);
                                }
                            }
                        }
                        IDictionary<Player, int> purgedChange = new Dictionary<Player, int>();
                        foreach (var pair in change)
                        {
                            int value = pair.Value;
                            if (value < -pair.Key.HP)
                                value = -pair.Key.HP;
                            if (value > pair.Key.HPb - pair.Key.HP)
                                value = pair.Key.HPb - pair.Key.HP;
                            if (value != 0)
                                purgedChange[pair.Key] = value;
                            pair.Key.HP += value;
                        }
                        if (purgedChange.Count > 0)
                        {
                            Artiad.HpIssueSemaphore.Telegraph(WI.BCast, purgedChange.Select(
                                p => new Artiad.HpIssueSemaphore(p.Key.Uid, true, null, p.Value, p.Key.HP)));
                        }
                        Artiad.Procedure.ArticuloMortis(this, WI, true);
                    }
                    break;
                case "G0IY":
                    {
                        int changeType = int.Parse(args[1]);
                        //bool changed = (args[1] == "0");
                        ushort who = ushort.Parse(args[2]);
                        int heroNum = int.Parse(args[3]);
                        Base.Card.Hero hero = LibTuple.HL.InstanceHero(heroNum);
                        Player player = Board.Garden[who];
                        player.SelectHero = heroNum;
                        //if (changed)
                        if (changeType == 0 || changeType == 2)
                        {
                            player.ResetROM(Board);
                            player.InitFromHero(hero, true, Board.PoolEnabled, Board.PlayerPoolEnabled);
                            Artiad.ContentRule.LoadDefaultPrice(player);
                        }
                        else
                            player.InitFromHero(hero, false, Board.PoolEnabled, Board.PlayerPoolEnabled);
                        if (changeType == 2 || (changeType == 1 && args.Length > 4))
                        {
                            player.HP = int.Parse(args[4]);
                            if (player.HP > player.HPb)
                                player.HP = player.HPb;
                        }
                        if (Board.PoolEnabled)
                            AwakeABCValue(false, player);
                        if (Board.PlayerPoolEnabled)
                            AwakeABCValue(true, player);
                        RaiseGMessage("G2AK," + player.Uid + ","
                            + player.HP + "," + player.HPb + "," + player.STR + "," + player.DEX);
                        // remove all cosses containing the player
                        foreach (Player py in Board.Garden.Values)
                        {
                            if (py.IsAlive && py.Coss.Count > 0 && py.Coss.Peek() == heroNum)
                                RaiseGMessage("G0OV," + player.Uid + "," + heroNum);
                        }
                        Board.HeroPiles.Remove(heroNum);
                        Board.HeroDises.Remove(heroNum);
                        WI.BCast("E0IY," + cmdrst);
                        if (changeType == 0 || changeType == 2)
                        {
                            if (hero.Skills.Count > 0)
                                RaiseGMessage("G0IS," + player.Uid + ",0," + string.Join(",", hero.Skills));
                        }
                        else if (changeType == 1)
                        {
                            if (Board.IsAttendWar(player))
                                RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                                {
                                    Role = Artiad.CoachingHelper.PType.REFRESH, Coach = player.Uid
                                } }.ToMessage());
                        }
                        string zs = "";
                        if (player.Weapon != 0)
                            zs += "," + player.Uid + "," + player.Weapon;
                        if (player.Armor != 0)
                            zs += "," + player.Uid + "," + player.Armor;
                        if (player.Trove != 0)
                            zs += "," + player.Uid + "," + player.Trove;
                        if (player.ExEquip != 0)
                            zs += "," + player.Uid + "," + player.ExEquip;
                        if (zs != "")
                            RaiseGMessage("G1IZ" + zs);
                        if (!player.PetDisabled)
                        {
                            ushort[] pets = player.Pets.Where(p => p != 0 &&
                                 !Board.NotActionPets.Contains(p)).ToArray();
                            if (pets.Length > 0)
                            {
                                RaiseGMessage(new Artiad.JoinPetEffects() { SingleUnit = new Artiad.PetEffectUnit()
                                {
                                    Owner = player.Uid,
                                    Pets = pets,
                                    Reload = (changeType == 1) ? Artiad.PetEffectUnit.ReloadType.ABLE :
                                        Artiad.PetEffectUnit.ReloadType.BORROW
                                } }.ToMessage());
                            }
                        }
                        if (Board.IsAttendWar(player) && Board.PoolEnabled)
                            RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
                    }
                    break;
                case "G0DS":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            int count = int.Parse(args[3]);
                            if (count > 0)
                                Board.Garden[me].Immobilized = true;
                            WI.BCast("E0DS," + me + "," + type + "," + count);
                        }
                        else if (type == 1)
                        {
                            Board.Garden[me].Immobilized = false;
                            WI.BCast("E0DS," + me + "," + type);
                        }
                        break;
                    }
                case "G1DI":
                    {
                        string g0on = "";
                        for (int idx = 1; idx < args.Length;)
                        {
                            ushort who = ushort.Parse(args[idx]);
                            bool drIn = (args[idx + 1] == "0");
                            int n = int.Parse(args[idx + 2]);
                            if (!drIn)
                            {
                                string[] cards = Algo.TakeRange(args, idx + 4, idx + 4 + n);
                                g0on += "," + who + ",C," + n + "," + string.Join(",", cards);
                            }
                            idx += (4 + n);
                        }
                        if (g0on.Length > 0)
                            RaiseGMessage("G0ON" + g0on);
                    }
                    break;
                case "G1IU":
                    Board.PZone.AddRange(Algo.TakeRange(args, 1, args.Length).Select(p => ushort.Parse(p)));
                    break;
                case "G1OU":
                    for (int i = 1; i < args.Length; ++i)
                    {
                        ushort x = ushort.Parse(args[i]);
                        Board.PZone.Remove(x);
                    }
                    break;
                case "G0CD": // use card and want a target
                    {
                        // G0CD,A,T,JP02,17,36;1,G0OH,...
                        int hdx = cmd.IndexOf(';');
                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Algo.Substring(cmd, idx + 1, -1);

                        string[] argv = cmd.Substring(0, hdx).Split(',');
                        ushort ust = ushort.Parse(argv[1]);
                        string cardName = argv[3];
                        WI.BCast("E0CD," + argv[1] + "," + argv[2] + "," + argv[3]);
                        if (!Artiad.ContentRule.IsTuxVestige(cardName, sktInType))
                            RaiseGMessage("G0CE," + ust + "," + argv[2] + ",0," + cardName +
                                ";" + sktInType + "," + sktFuse);
                        else
                            RaiseGMessage("G0CE," + ust + "," + argv[2] + ",1," + cardName +
                                "," + argv[4] + ";" + sktInType + "," + sktFuse);
                        break;
                    }
                case "G0CE": // use card and take action
                    {
                        // G0CE,A,T,0/1(eq),JP04,(3,1);TF
                        int hdx = cmd.IndexOf(';');
                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Algo.Substring(cmd, idx + 1, -1);
                        string[] parts = cmd.Substring(0, hdx).Split(',');

                        ushort ust = ushort.Parse(parts[1]);
                        ushort taction = ushort.Parse(parts[2]);
                        ushort notEq = ushort.Parse(parts[3]);
                        if (notEq == 0)
                        {
                            Base.Card.Tux tux = tx01[parts[4]];
                            string argvt = Algo.Substring(cmd, parts[0].Length + parts[1].Length +
                                parts[2].Length + parts[3].Length + parts[4].Length + 5, hdx);
                            if (argvt.Length > 0)
                                argvt = "," + argvt;
                            WI.BCast("E0CE," + ust + "," + taction + "," + parts[4] +
                                (argvt.Length > 0 ? ("," + argvt) : ""));
                            if (taction != 2)
                                tux.Action(Board.Garden[ust], sktInType, sktFuse, argvt);
                        }
                        else
                        {
                            Base.Card.Tux tux = tx01[parts[4]];
                            WI.BCast("E0CE," + ust + "," + taction + "," + parts[4]);
                            ushort ut = ushort.Parse(parts[5]);
                            if (taction != 2)
                                tux.Vestige(Board.Garden[ust], sktInType, sktFuse, ut);
                        }
                    }
                    break;
                case "G1CW": // two targets that a tux will take action
                    {
                        // G1CW,A[1st:Org],B[2nd:Target],C[2nd:Provider],JP04;cdFuse;TF
                        int fdx = cmd.IndexOf(';');
                        int hdx = cmd.IndexOf(';', fdx + 1);
                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Algo.Substring(cmd, idx + 1, -1);
                        string cdFuse = Algo.Substring(cmd, fdx + 1, hdx);

                        string[] g1cw = cmd.Substring(0, fdx).Split(',');
                        //ushort first = ushort.Parse(g1cw[1]);
                        ushort second = ushort.Parse(g1cw[2]);
                        Player provider = Board.Garden[ushort.Parse(g1cw[3])];
                        Tux tux = LibTuple.TL.EncodeTuxCode(g1cw[4]);
                        ushort it = ushort.Parse(g1cw[5]);
                        tux.Locust(provider, sktInType, sktFuse, cdFuse, Board.Garden[second], tux, it);
                    }
                    break;
                case "G0XZ":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort dicesType = ushort.Parse(args[2]);
                        if (dicesType == 0)
                        {
                            ushort who = ushort.Parse(args[3]);
                            var gps = Board.Garden[who].Tux;
                            WI.Send("E0XZ," + me + ",5," + args[3] + "," + string.Join(",", gps), 0, me);
                            WI.Send("E0XZ," + me + ",6," + args[3], ExceptStaff(me));
                            WI.Live("E0XZ," + me + ",6," + args[3]);
                        }
                        else
                        {
                            Base.Utils.Rueue<ushort> piles = null;
                            if (dicesType == 1)
                                piles = Board.TuxPiles;
                            else if (dicesType == 2)
                                piles = Board.MonPiles;
                            else if (dicesType == 3)
                                piles = Board.EvePiles;

                            ushort control = ushort.Parse(args[3]);
                            int count = int.Parse(args[4]);
                            if (control == 0)
                            {
                                ushort[] gps = piles.Watch(count);
                                WI.Send("E0XZ," + me + ",0," + dicesType + "," + string.Join(",", gps), 0, me);
                                WI.Send("E0XZ," + me + ",1," + dicesType + "," + gps.Length, ExceptStaff(me));
                                WI.Live("E0XZ," + me + ",1," + dicesType + "," + gps.Length);
                                // string order = AsyncInput(me, "//", "G0XZ", "0");
                                WI.BCast("E0XZ," + me + ",4,0");
                            }
                            else
                            {
                                ushort[] gps = piles.Watch(count);
                                //WI.Send("E0XZ," + me + ",0," + args[3] + "," + Algo.SatoString(gps), 0, me);
                                WI.Send("E0XZ," + me + ",1," + dicesType + "," + count, ExceptStaff(me));
                                WI.Live("E0XZ," + me + ",1," + dicesType + "," + count);
                                int pick = args.Length > 5 ? int.Parse(args[5]) : count;
                                string dicesCode = "";
                                if (dicesType == 1) dicesCode = "C";
                                else if (dicesType == 2) dicesCode = "M";
                                else if (dicesType == 3) dicesCode = "E";
                                string serpChar = "p" + dicesCode;
                                string order = AsyncInput(me, "X" + pick + "(" + serpChar +
                                    string.Join(serpChar, gps) + ")", "G0XZ", "0");
                                if (order != "" && order != "0")
                                {
                                    List<ushort> orders = new List<ushort>(
                                        order.Split(',').Select(p => ushort.Parse(p)));
                                    int[] result = new int[gps.Length];
                                    Fill(result, -1);
                                    for (int i = 0; i < orders.Count; ++i)
                                    {
                                        for (int j = 0; j < gps.Length; ++j)
                                        {
                                            if (orders[i] == gps[j])
                                            {
                                                result[i] = j + 1; break;
                                            }
                                        }
                                    }
                                    if (!result.Contains(-1) && orders.Count == gps.Length)
                                    {
                                        WI.Send("E0XZ," + me + ",2," + dicesType + "," + order, 0, me);
                                        WI.Send("E0XZ," + me + ",3," + dicesType +
                                            "," + string.Join(",", result), ExceptStaff(me));
                                        WI.Live("E0XZ," + me + ",3," + dicesType + "," + string.Join(",", result));
                                        piles.Dequeue(count);
                                        piles.PushBack(orders);
                                    }
                                    else
                                        WI.BCast("E0XZ," + me + ",4,0");
                                }
                                else
                                    WI.BCast("E0XZ," + me + ",4,0");

                                if (pick < count)
                                {
                                    int discard = count - pick;
                                    ushort[] pops = piles.Dequeue(discard);
                                    char[] diceNames = new char[] { '0', 'C', 'M', 'E' };
                                    RaiseGMessage("G2IN," + (dicesType - 1) + "," + discard);
                                    RaiseGMessage("G0ON,0," + diceNames[dicesType] + "," + discard
                                        + "," + string.Join(",", pops));
                                }
                            }
                        }
                        break;
                    }
                case "G0ZB":
                    if (Artiad.ClothingHelper.IsStandard(cmd))
                    {
                        Artiad.EquipStandard eis = Artiad.EquipStandard.Parse(cmd);
                        Player player = Board.Garden[eis.Who];
                        if (eis.SlotAssign)
                        {
                            IDictionary<Tux.TuxType, List<ushort>> adict = new Dictionary<Tux.TuxType, List<ushort>>();
                            foreach (ushort card in eis.Cards)
                            {
                                Tux tux = LibTuple.TL.DecodeTux(card);
                                Algo.AddToMultiMap(adict, tux.Type, card);
                            }
                            List<ushort> eisRemoves = new List<ushort>();
                            foreach (var pair in adict)
                            {
                                int cap = player.GetSlotCapacity(pair.Key);
                                int cur = player.GetCurrentEquipCount(pair.Key);
                                int nin = pair.Value.Count;
                                // Don't ask for substitude now
                                if (cur == cap)
                                {
                                    pair.Value.ForEach((card) => 
                                    {
                                        if (Board.Garden[eis.Source].HasCard(card))
                                            RaiseGMessage("G0QZ," + eis.Source + "," + card);
                                        else
                                        {
                                            Board.PendingTux.Remove(eis.Source + ",G0ZB," + card);
                                            RaiseGMessage("G0ON,0,C,1," + card);
                                        }
                                        eisRemoves.Add(card);
                                    });
                                }
                                else if (nin > cap - cur)
                                {
                                    string askSel = AsyncInput(eis.Coach, "#保留装备的,C" + (cap - cur) + "(p"
                                        + string.Join("p", pair.Value), "G0ZB", "0");
                                    ushort[] keeps = askSel.Split(',').Select(p => ushort.Parse(p)).ToArray();
                                    pair.Value.Except(keeps).ToList().ForEach((card) =>
                                    {
                                        if (Board.Garden[eis.Source].HasCard(card))
                                            RaiseGMessage("G0QZ," + eis.Source + "," + card);
                                        else
                                        {
                                            Board.PendingTux.Remove(eis.Source + ",G0ZB," + card);
                                            RaiseGMessage("G0ON,0,C,1," + card);
                                        }
                                        eisRemoves.Add(card);
                                    });
                                }
                            }
                            eis.Cards = eis.Cards.Except(eisRemoves).ToArray();
                        }
                        foreach (ushort card in eis.Cards)
                        {
                            Tux tux = LibTuple.TL.DecodeTux(card);
                            if (!tux.IsTuxEqiup())
                                continue;
                            if (eis.Source != 0 && !Board.Garden[eis.Source].HasCard(card) &&
                                 !Board.PendingTux.Contains(eis.Source + ",G0ZB," + card)) { continue; }
                            TuxEqiup te = tux as TuxEqiup;
                            // Put it back here, thus not trigger G0OT
                            if (Artiad.ClothingHelper.IsEquipable(player, te.Type))
                            {
                                if (eis.Source != 0)
                                {
                                    if (Board.Garden[eis.Source].HasCard(card))
                                        RaiseGMessage("G0OT," + eis.Source + ",1," + card);
                                    else
                                        Board.PendingTux.Remove(eis.Source + ",G0ZB," + card);
                                }
                                Artiad.ClothingHelper.SlotType slot = Artiad.ClothingHelper.SlotType.NL;

                                int replacer = Artiad.ClothingHelper.GetSubstitude(player, te.Type,
                                    eis.SlotAssign, p => AsyncInput(eis.Coach, p, cmd, "1"));
                                if (replacer == 0)
                                {
                                    if (te.Type == Tux.TuxType.WQ && player.Weapon == 0)
                                    {
                                        player.Weapon = card;
                                        slot = Artiad.ClothingHelper.SlotType.WQ;
                                    }
                                    else if (te.Type == Tux.TuxType.FJ && player.Armor == 0)
                                    {
                                        player.Armor = card;
                                        slot = Artiad.ClothingHelper.SlotType.FJ;
                                    }
                                    else if (te.Type == Tux.TuxType.XB && player.Trove == 0)
                                    {
                                        player.Trove = card;
                                        slot = Artiad.ClothingHelper.SlotType.XB;
                                    }
                                    else if (player.ExEquip == 0)
                                    {
                                        player.ExEquip = card;
                                        slot = Artiad.ClothingHelper.SlotType.EE;
                                    }
                                }
                                else if (replacer > 0)
                                {
                                    if (player.Weapon == replacer)
                                    {
                                        RaiseGMessage("G0QZ," + eis.Who + "," + replacer);
                                        player.Weapon = card;
                                        slot = Artiad.ClothingHelper.SlotType.WQ;
                                    }
                                    else if (player.Armor == replacer)
                                    {
                                        RaiseGMessage("G0QZ," + eis.Who + "," + replacer);
                                        player.Armor = card;
                                        slot = Artiad.ClothingHelper.SlotType.FJ;
                                    }
                                    else if (player.Trove == replacer)
                                    {
                                        RaiseGMessage("G0QZ," + eis.Who + "," + replacer);
                                        player.Trove = card;
                                        slot = Artiad.ClothingHelper.SlotType.XB;
                                    }
                                    else if (player.ExEquip == replacer)
                                    {
                                        RaiseGMessage("G0QZ," + eis.Who + "," + replacer);
                                        player.ExEquip = card;
                                        slot = Artiad.ClothingHelper.SlotType.EE;
                                    }
                                }
                                if (slot != Artiad.ClothingHelper.SlotType.NL)
                                {
                                    new Artiad.EquipSemaphore()
                                    {
                                        Who = eis.Who,
                                        Source = eis.Source,
                                        Slot = slot,
                                        SingleCard = card
                                    }.Telegraph(WI.BCast);

                                    RaiseGMessage("G1IZ," + eis.Who + "," + card);
                                    if (te.Type == Tux.TuxType.WQ && !player.WeaponDisabled)
                                        te.InsAction(player);
                                    else if (te.Type == Tux.TuxType.FJ && !player.ArmorDisabled)
                                        te.InsAction(player);
                                    else if (te.Type == Tux.TuxType.XB && !player.LuggageDisabled)
                                        te.InsAction(player);   
                                }
                            }
                            else if (eis.Source != eis.Who)
                            {
                                if (eis.Source != 0)
                                    RaiseGMessage("G0QZ," + eis.Source + "," + card);
                                else
                                    RaiseGMessage("G0ON,0,C,1," + card);
                            }
                            // else, leave it in eis.Who's hand
                        }
                    }
                    else if (Artiad.ClothingHelper.IsEx(cmd))
                    {
                        Artiad.EquipExCards eec = Artiad.EquipExCards.Parse(cmd);
                        if (eec.Source != 0 && !Board.Garden[eec.Source].HasCards(eec.Cards))
                            break;
                        if (eec.Source != 0)
                        {
                            RaiseGMessage("G0OT," + eec.Source + "," + eec.Cards.Length + 
                                "," + string.Join(",", eec.Cards));
                        }
                        Board.Garden[eec.Who].ExCards.AddRange(eec.Cards);
                        new Artiad.EquipSemaphore()
                        {
                            Who = eec.Who,
                            Source = eec.Source,
                            Slot = Artiad.ClothingHelper.SlotType.EX,
                            Cards = eec.Cards
                        }.Telegraph(WI.BCast);
                    }
                    else if (Artiad.ClothingHelper.IsFakeq(cmd))
                    {
                        Artiad.EquipFakeq ef = Artiad.EquipFakeq.Parse(cmd);
                        if (ef.Source != 0 && Board.Garden[ef.Source].Fakeq.ContainsKey(ef.Card))
                        {
                            RaiseGMessage("G0OT," + ef.Source + ",1," + ef.Card);
                            Board.Garden[ef.Who].Fakeq[ef.Card] = ef.CardAs;
                            new Artiad.EquipSemaphore()
                            {
                                Who = ef.Who,
                                Source = ef.Source,
                                Slot = Artiad.ClothingHelper.SlotType.FQ,
                                SingleCard = ef.Card,
                                CardAs = ef.CardAs
                            }.Telegraph(WI.BCast);
                        }
                        else
                        {
                            foreach (string tuxInfo in Board.PendingTux)
                            {
                                string[] parts = tuxInfo.Split(',');
                                ushort who = ushort.Parse(parts[0]);
                                string g0cc = parts[1];
                                ushort cook = ushort.Parse(parts[2]);
                                if (who == ef.Who && g0cc == "G0CC" && ef.Card == cook)
                                {
                                    Board.Garden[ef.Who].Fakeq[ef.Card] = ef.CardAs;
                                    Board.PendingTux.Remove(tuxInfo);
                                    new Artiad.EquipSemaphore()
                                    {
                                        Who = ef.Who,
                                        Source = ef.Source,
                                        Slot = Artiad.ClothingHelper.SlotType.FQ,
                                        SingleCard = ef.Card,
                                        CardAs = ef.CardAs
                                    }.Telegraph(WI.BCast);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case "G0ZC":
                    {
                        // G0ZC,A,0,x,y..;T,F
                        int hdx = cmd.IndexOf(';');
                        string mainPart = cmd.Substring(0, hdx);
                        string[] mainParts = mainPart.Split(',');

                        ushort me = ushort.Parse(mainParts[1]);
                        ushort consumeType = ushort.Parse(mainParts[2]);
                        ushort card = ushort.Parse(mainParts[3]);

                        ushort target; int jdx;
                        if (consumeType < 3)
                        {
                            target = 0;
                            jdx = mainParts[0].Length + mainParts[1].Length +
                                mainParts[2].Length + mainParts[3].Length + 4;
                        }
                        else // consumeType == 3 means ActionHolder, 4 will burst; 2 is empty now
                        {
                            target = ushort.Parse(mainParts[4]);
                            jdx = mainParts[0].Length + mainParts[1].Length +
                                mainParts[2].Length + mainParts[3].Length + mainParts[4].Length + 5;
                        }
                        string argsv = Algo.Substring(cmd, jdx, hdx);
                        string cargsv = argsv != "" ? "," + argsv : "";

                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Algo.Substring(cmd, idx + 1, -1);

                        Player player = Board.Garden[me];
                        TuxEqiup tux = LibTuple.TL.DecodeTux(card) as TuxEqiup;
                        if (consumeType % 3 == 1)
                            RaiseGMessage("G0ZI," + me + "," + card);
                        WI.BCast("E0ZC," + me + "," + consumeType + "," + card + "," + sktInType + cargsv);
                        if (consumeType < 3)
                            tux.ConsumeAction(player, consumeType, sktInType, sktFuse, argsv);
                        else
                            tux.ConsumeActionHolder(player, Board.Garden[target],
                                consumeType - 3, sktInType, sktFuse, argsv);
                    }
                    break;
                case "G0ZI":
                    {
                        IDictionary<ushort, List<ushort>> jmc = new Dictionary<ushort, List<ushort>>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort pet = ushort.Parse(args[i + 1]);
                            Algo.AddToMultiMap(jmc, who, pet);
                        }
                        if (Board.InCampaign) // to mark as to be discard
                        {
                            jmc.Keys.ToList().ForEach(p =>
                            {
                                jmc[p].ForEach(q => RaiseGMessage("G1ZK,0," + p + "," + q));
                                RaiseGMessage(new Artiad.AnnouceCard()
                                {
                                    Action = Artiad.AnnouceCard.Type.FLASH,
                                    Officer = p,
                                    Genre = Card.Genre.Tux,
                                    Cards = jmc[p].ToArray()
                                }.ToMessage());
                            });
                        }
                        else // to discard immediately
                        {
                            jmc.Keys.ToList().ForEach(p => 
                                RaiseGMessage("G0QZ," + p + "," + string.Join(",", jmc[p])));
                        }
                        WI.BCast("E0ZI," + cmdrst);
                    }
                    break;
                case "G1ZK":
                    if (args[1].Equals("0"))
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort ep = ushort.Parse(args[3]);
                        Board.CsEqiups.Add(who + "," + ep);
                    }
                    else if (args[1].Equals("1"))
                    {
                        foreach (string line in Board.CsEqiups)
                        {
                            int idx = line.IndexOf(',');
                            ushort who = ushort.Parse(line.Substring(0, idx));
                            ushort ep = ushort.Parse(line.Substring(idx + 1));
                            Player py = Board.Garden[who];
                            if (py.ListOutAllCards().Contains(ep))
                                RaiseGMessage("G0QZ," + who + "," + ep);
                        }
                        Board.CsEqiups.Clear();
                    }
                    break;
                case "G1IZ":
                    {
                        string zls = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            Player player = Board.Garden[who];
                            ushort card = ushort.Parse(args[i + 1]);
                            var cardSelf = LibTuple.TL.DecodeTux(card);
                            bool enabled =
                                (cardSelf.Type == Base.Card.Tux.TuxType.FJ && !player.ArmorDisabled)
                                ||
                                (cardSelf.Type == Base.Card.Tux.TuxType.WQ && !player.WeaponDisabled)
                                ||
                                (cardSelf.Type == Base.Card.Tux.TuxType.XB && !player.LuggageDisabled);
                            if (enabled)
                                zls += "," + who + "," + card;
                        }
                        if (zls != "")
                            RaiseGMessage("G0ZS" + zls);
                    }
                    break;
                case "G1OZ":
                    {
                        string zls = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            Player player = Board.Garden[who];
                            ushort card = ushort.Parse(args[i + 1]);
                            var cardSelf = LibTuple.TL.DecodeTux(card);
                            bool enabled =
                                (cardSelf.Type == Base.Card.Tux.TuxType.FJ && !player.ArmorDisabled)
                                ||
                                (cardSelf.Type == Base.Card.Tux.TuxType.WQ && !player.WeaponDisabled)
                                ||
                                (cardSelf.Type == Base.Card.Tux.TuxType.XB && !player.LuggageDisabled);
                            if (enabled)
                                zls += "," + who + "," + card;
                        }
                        if (zls != "")
                            RaiseGMessage("G0ZL" + zls);
                    }
                    break;
                case "G0ZS":
                    for (int i = 1; i < args.Length; i += 2)
                    {
                        ushort who = ushort.Parse(args[i]);
                        ushort card = ushort.Parse(args[i + 1]);
                        Player player = Board.Garden[who];
                        Tux tux = LibTuple.TL.DecodeTux(card);
                        if (tux.IsTuxEqiup())
                        {
                            TuxEqiup tue = (TuxEqiup)tux;
                            tue.IncrAction(player);
                            WI.BCast("E0ZS," + cmdrst);
                        }
                    }
                    break;
                case "G0ZL":
                    for (int i = 1; i < args.Length; i += 2)
                    {
                        ushort who = ushort.Parse(args[i]);
                        ushort card = ushort.Parse(args[i + 1]);
                        Player player = Board.Garden[who];
                        Tux tux = LibTuple.TL.DecodeTux(card);
                        if (tux.IsTuxEqiup())
                        {
                            TuxEqiup tue = (TuxEqiup)tux;
                            tue.DecrAction(player);
                            WI.BCast("E0ZL," + cmdrst);
                        }
                    }
                    break;
                case "G0IA":
                    {
                        ushort me = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        Player player = null;
                        if (me < 1000)
                            player = Board.Garden[me];
                        else if (Board.Hinder.Uid == me)
                            player = Board.Hinder;
                        else if (Board.Supporter.Uid == me)
                            player = Board.Supporter;

                        if (type == 0 || type == 1 || type == 3)
                        {
                            int n = int.Parse(args[3]);
                            if (type == 3)
                                player.STRh += n;
                            if (type == 0 || type == 3)
                                player.STRb = player.mSTRb + n;
                            if (Board.PlayerPoolEnabled)
                            {
                                player.STRc = player.STRc + n;
                                WI.BCast("E0IA," + me + "," + type + "," + n + "," + player.STRa + "," + player.STRc);
                            }
                            else if (Board.PoolEnabled)
                            {
                                player.STRa = player.mSTRa + n;
                                WI.BCast("E0IA," + me + "," + type + "," + n + "," + player.STRa);
                            }
                            else
                                WI.BCast("E0IA," + me + "," + type + "," + n + "," + player.STRb);
                        }
                        else if (type == 2) // Suppress case
                        {
                            player.STRi = 1;
                            WI.BCast("E0IA," + me + ",2");
                        }
                        if (Board.PoolEnabled)
                            RaiseGMessage(new Artiad.PondRefresh() { CheckHit = false }.ToMessage());
                        break;
                    }
                case "G0OA":
                    {
                        ushort me = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        Player player = null;
                        if (me < 1000)
                            player = Board.Garden[me];
                        else if (Board.Hinder.Uid == me)
                            player = Board.Hinder;
                        else if (Board.Supporter.Uid == me)
                            player = Board.Supporter;

                        if (type == 0 || type == 1 || type == 3)
                        {
                            int n = int.Parse(args[3]);
                            if (type == 3)
                                player.STRh -= n;
                            if (type == 0 || type == 3)
                                player.STRb = player.mSTRb - n;
                            if (Board.PlayerPoolEnabled)
                            {
                                player.STRc = player.STRc - n;
                                WI.BCast("E0OA," + me + "," + type + "," + n + "," + player.STRa + "," + player.STRc);
                            }
                            else if (Board.PoolEnabled)
                            {
                                player.STRa = player.mSTRa - n;
                                WI.BCast("E0OA," + me + "," + type + "," + n + "," + player.STRa);
                            }
                            else
                                WI.BCast("E0OA," + me + "," + type + "," + n + "," + player.STRb);
                        }
                        else if (type == 2)
                        {
                            player.STRi = -1;
                            WI.BCast("E0OA," + me + ",2");
                        }
                        if (Board.PoolEnabled)
                            RaiseGMessage(new Artiad.PondRefresh() { CheckHit = false }.ToMessage());
                        break;
                    }
                case "G0IX":
                    {
                        ushort me = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        Player player = null;
                        if (me < 1000)
                            player = Board.Garden[me];
                        else if (Board.Hinder.Uid == me)
                            player = Board.Hinder;
                        else if (Board.Supporter.Uid == me)
                            player = Board.Supporter;
                        if (player.Uid != 0)
                        {
                            if (type == 0 || type == 1 || type == 3)
                            {
                                int n = int.Parse(args[3]);
                                if (type == 3)
                                    player.DEXh += n;
                                if (type == 0 || type == 3)
                                    player.DEXb = player.mDEXb + n;
                                if (Board.PlayerPoolEnabled)
                                {
                                    player.DEXc = player.DEXc + n;
                                    WI.BCast("E0IX," + me + "," + type + "," + n + "," + player.DEXa + "," + player.DEXc);
                                }
                                else if (Board.PoolEnabled)
                                {
                                    player.DEXa = player.mDEXa + n;
                                    WI.BCast("E0IX," + me + "," + type + "," + n + "," + player.DEXa);
                                }
                                else
                                    WI.BCast("E0IX," + me + "," + type + "," + n + "," + player.DEXb);
                            }
                            else if (type == 2)
                            {
                                player.DEXi = 1;
                                WI.BCast("E0IX," + me + ",2");
                            }
                            if (Board.PoolEnabled)
                                RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
                        }
                        break;
                    }
                case "G0OX":
                    {
                        ushort me = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        Player player = null;
                        if (me < 1000)
                            player = Board.Garden[me];
                        else if (Board.Hinder.Uid == me)
                            player = Board.Hinder;
                        else if (Board.Supporter.Uid == me)
                            player = Board.Supporter;
                        if (player.Uid != 0)
                        {
                            if (type == 0 || type == 1 || type == 3)
                            {
                                int n = int.Parse(args[3]);
                                if (type == 3)
                                    player.DEXh -= n;
                                if (type == 0 || type == 3)
                                    player.DEXb = player.mDEXb - n;
                                if (Board.PlayerPoolEnabled)
                                {
                                    player.DEXc = player.DEXc - n;
                                    WI.BCast("E0OX," + me + "," + type + "," + n + "," + player.DEXa + "," + player.DEXc);
                                }
                                else if (Board.PoolEnabled)
                                {
                                    player.DEXa = player.mDEXa - n;
                                    WI.BCast("E0OX," + me + "," + type + "," + n + "," + player.DEXa);
                                }
                                else
                                    WI.BCast("E0OX," + me + "," + type + "," + n + "," + player.DEXb);
                            }
                            else if (type == 2)
                            {
                                player.DEXi = 1;
                                WI.BCast("E0OX," + me + ",2");
                            }
                            if (Board.PoolEnabled)
                                RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
                        }
                    }
                    break;
                case "G0AX":
                    Artiad.ResetAX.Parse(cmd).Handle(this); break;
                case "G0IB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int n = ushort.Parse(args[2]);
                        NMB nmb = NMBLib.Decode(x, LibTuple.ML, LibTuple.NL);
                        if (nmb.IsMonster())
                        {
                            Monster mon = (Monster)nmb;
                            mon.STR = mon.mSTR + n;
                            WI.BCast("E0IB," + x + "," + n + "," + mon.STR);
                            if (Board.PoolEnabled)
                                RaiseGMessage(new Artiad.PondRefresh() { CheckHit = false }.ToMessage());
                            RaiseGMessage("G2WK," + string.Join(",",
                                CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                        }
                        else if (nmb.IsNPC())
                        {
                            NPC npc = (NPC)nmb;
                            npc.STR += n;
                            WI.BCast("E0IB," + x + "," + n + "," + npc.STR);
                        }
                        break;
                    }
                case "G0OB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int n = ushort.Parse(args[2]);
                        NMB nmb = NMBLib.Decode(x, LibTuple.ML, LibTuple.NL);
                        if (nmb.IsMonster())
                        {
                            Monster mon = (Monster)nmb;
                            mon.STR = mon.mSTR - n;
                            WI.BCast("E0OB," + x + "," + n + "," + mon.STR);
                            if (Board.PoolEnabled)
                                RaiseGMessage(new Artiad.PondRefresh() { CheckHit = false }.ToMessage());
                            RaiseGMessage("G2WK," + string.Join(",",
                                CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                        }
                        else if (nmb.IsNPC())
                        {
                            NPC npc = (NPC)nmb;
                            npc.STR -= n;
                            WI.BCast("E0OB," + x + "," + n + "," + npc.STR);
                        }
                        break;
                    }
                case "G0IW":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int n = ushort.Parse(args[2]);
                        NMB nmb = NMBLib.Decode(x, LibTuple.ML, LibTuple.NL);
                        if (nmb.IsMonster() && n > 0)
                        {
                            Monster mon = (Monster)nmb;
                            mon.AGL = mon.mAGL + n;
                            WI.BCast("E0IW," + x + "," + n + "," + mon.AGL);
                            if (Board.PoolEnabled)
                                RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
                        }
                        break;
                    }
                case "G0OW":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int n = ushort.Parse(args[2]);
                        NMB nmb = NMBLib.Decode(x, LibTuple.ML, LibTuple.NL);
                        if (nmb.IsMonster() && n > 0)
                        {
                            Monster mon = (Monster)nmb;
                            mon.AGL = mon.mAGL - n;
                            WI.BCast("E0OW," + x + "," + n + "," + mon.AGL);
                            if (Board.PoolEnabled)
                                RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
                        }
                        break;
                    }
                case "G0WB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        NMB nmb = NMBLib.Decode(x, LibTuple.ML, LibTuple.NL);
                        if (nmb.IsMonster())
                        {
                            Monster mon = (Monster)nmb;
                            bool change = false;
                            if (mon.mSTR != mon.STRb)
                            {
                                change |= (mon.STR != mon.STRb);
                                mon.STR = mon.STRb;
                                RaiseGMessage("G2WK," + string.Join(",",
                                    CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                            }
                            if (mon.mAGL != mon.AGLb)
                            {
                                change |= (mon.AGL != mon.AGLb);
                                mon.AGL = mon.AGLb;
                            }
                            if (change)
                                WI.BCast("E0WB," + x + "," + mon.STR + "," + mon.AGL);
                        }
                        else if (nmb.IsNPC())
                        {
                            NPC npc = (NPC)nmb;
                            if (npc.STR != npc.STRb)
                            {
                                npc.STR = npc.STRb;
                                WI.BCast("E0WB," + x + "," + npc.STR);
                            }
                        }
                    }
                    break;
                case "G09P":
                    Artiad.PondRefresh.Parse(cmd).Handle(this, WI); break;
                case "G1WP":
                    for (int i = 1; i < args.Length; i += 4)
                    {
                        ushort side = ushort.Parse(args[i]);
                        ushort host = ushort.Parse(args[i + 1]);
                        string reason = args[i + 2];
                        int newVal = int.Parse(args[i + 3]);
                        if (side == Board.Rounder.Team)
                        {
                            string key = host + "," + reason;
                            int delta = newVal - (Board.RPoolGain.ContainsKey(key) ? Board.RPoolGain[key] : 0);
                            if (delta > 0)
                                RaiseGMessage("G0IP," + side + "," + delta);
                            else if (delta < 0)
                                RaiseGMessage("G0OP," + side + "," + (-delta));
                            Board.RPoolGain[key] = newVal;
                        }
                        else if (side == Board.Rounder.OppTeam)
                        {
                            string key = host + "," + reason;
                            int delta = newVal - (Board.OPoolGain.ContainsKey(key) ? Board.OPoolGain[key] : 0);
                            if (delta > 0)
                                RaiseGMessage("G0IP," + side + "," + delta);
                            else if (delta < 0)
                                RaiseGMessage("G0OP," + side + "," + (-delta));
                            Board.OPoolGain[key] = newVal;
                        }
                    }
                    break;
                case "G0IP":
                    if (Board.PoolEnabled)
                    {
                        ushort side = ushort.Parse(args[1]);
                        ushort delta = ushort.Parse(args[2]);
                        if (side == Board.Rounder.Team)
                        {
                            Board.RPool += delta;
                            WI.BCast("E0IP," + side + "," + delta);
                        }
                        else if (side == Board.Rounder.OppTeam)
                        {
                            Board.OPool += delta;
                            WI.BCast("E0IP," + side + "," + delta);
                        }
                        RaiseGMessage(new Artiad.PondRefresh() { CheckHit = false }.ToMessage());
                    }
                    break;
                case "G0OP":
                    if (Board.PoolEnabled)
                    {
                        ushort side = ushort.Parse(args[1]);
                        int delta = int.Parse(args[2]);
                        if (side == Board.Rounder.Team)
                        {
                            Board.RPool -= delta;
                            WI.BCast("E0OP," + side + "," + delta);
                        }
                        else if (side == Board.Rounder.OppTeam)
                        {
                            Board.OPool -= delta;
                            WI.BCast("E0OP," + side + "," + delta);
                        }
                        RaiseGMessage(new Artiad.PondRefresh() { CheckHit = false }.ToMessage());
                    }
                    break;
                case "G0CZ":
                    if (args[1] == "0")
                        --Board.Garden[ushort.Parse(args[2])].RestZP;
                    else if (args[1] == "1")
                        Board.Garden[ushort.Parse(args[2])].RestZP = 1;
                    else if (args[1] == "2")
                        foreach (Player player in Board.Garden.Values)
                            player.RestZP = 1;
                    WI.BCast("E0CZ," + cmdrst);
                    break;
                case "G1SG":
                    break;
                case "G0HC":
                    if (Artiad.KittyHelper.IsHarvest(cmd))
                    {
                        Artiad.HarvestPet hvp = Artiad.HarvestPet.Parse(cmd);
                        new Artiad.HarvestPetSemaphore()
                        {
                            Farmer = hvp.Farmer,
                            Pets = hvp.Pets
                        }.Telegraph(WI.BCast);
                        // Item,         HL, ON
                        // Farmland = 0  Y   -
                        //          / 0  -   -
                        // Reposite = T  N   N
                        //          = F  -   -
                        // Plow     = T  Y   Y
                        //          = F  N   Y
                        Player player = Board.Garden[hvp.Farmer];
                        int fivepc = FiveElementHelper.PropCount;
                        List<ushort>[] cpets = new List<ushort>[fivepc];
                        for (int i = 0; i < fivepc; ++i)
                        {
                            cpets[i] = new List<ushort>();
                            if (player.Pets[i] != 0)
                                cpets[i].Add(player.Pets[i]);
                        }
                        foreach (ushort petUt in hvp.Pets)
                        {
                            Monster pet = LibTuple.ML.Decode(petUt);
                            int pe = pet.Element.Elem2Index();
                            if (!cpets[pe].Contains(petUt))
                                cpets[pe].Add(petUt);
                        }
                        List<ushort> result = new List<ushort>();
                        List<ushort> giveBack = new List<ushort>();
                        bool needHL = hvp.Farmland != 0 && hvp.Plow;
                        for (int i = 0; i < fivepc; ++i)
                        {
                            if (cpets[i].Count == 0)
                                continue;
                            else if (cpets[i].Count == 1)
                            {
                                ushort pt = cpets[i].First();
                                if (hvp.Pets.Contains(pt))
                                {
                                    if (needHL)
                                    {
                                        RaiseGMessage(new Artiad.LosePet()
                                        {
                                            Owner = hvp.Farmland,
                                            SinglePet = pt,
                                            Stepper = hvp.Farmer,
                                            Recycle = false
                                        }.ToMessage());
                                    }
                                    result.Add(pt);
                                }
                                continue;
                            }
                            Artiad.HarvestPet.Treaty treaty = hvp.TreatyAct;
                            if (cpets[i].Count > 2)
                                treaty = Artiad.HarvestPet.Treaty.ACTIVE; // more than two selection
                            if (hvp.Farmland == 0 && treaty == Artiad.HarvestPet.Treaty.KOKAN)
                                treaty = Artiad.HarvestPet.Treaty.ACTIVE;
                            ushort myPt = player.Pets[i];
                            if (treaty == Artiad.HarvestPet.Treaty.KOKAN) // KOKAN always recycle
                            {
                                ushort ayPt = hvp.SinglePet;
                                if (needHL)
                                {
                                    RaiseGMessage(new Artiad.LosePet()
                                    {
                                        Owner = hvp.Farmland,
                                        SinglePet = ayPt,
                                        Stepper = hvp.Farmer,
                                        Recycle = false
                                    }.ToMessage());
                                }
                                result.Add(ayPt);
                                RaiseGMessage(new Artiad.LosePet()
                                {
                                    Owner = hvp.Farmer,
                                    SinglePet = myPt,
                                    Stepper = hvp.Farmland,
                                    Recycle = false
                                }.ToMessage());
                                giveBack.Add(myPt);
                            }
                            else if (treaty == Artiad.HarvestPet.Treaty.PASSIVE)
                            {
                                ushort ayPt = hvp.SinglePet;
                                RaiseGMessage(new Artiad.LosePet()
                                {
                                    Owner = hvp.Farmer,
                                    SinglePet = myPt
                                }.ToMessage());
                                if (needHL)
                                {
                                    RaiseGMessage(new Artiad.LosePet()
                                    {
                                        Owner = hvp.Farmland,
                                        SinglePet = ayPt,
                                        Stepper = hvp.Farmer,
                                        Recycle = false
                                    }.ToMessage());
                                }
                                result.Add(ayPt);
                            }
                            else // ACTIVE
                            {
                                List<ushort> others = cpets[i].ToList(); others.Remove(myPt);
                                string mai = "#保留的,M1(p" + string.Join("p", cpets[i]) + ")";
                                ushort sel = ushort.Parse(AsyncInput(hvp.Farmer, mai, cmd, "0"));
                                if (sel == myPt) // Keep the old one
                                {
                                    if (!hvp.Reposit) // if reposit, then leave it where it was
                                    {
                                        if (needHL)
                                        {
                                            RaiseGMessage(new Artiad.LosePet()
                                            {
                                                Owner = hvp.Farmland,
                                                Pets = others.ToArray(),
                                                Recycle = false
                                            }.ToMessage());
                                        }
                                        RaiseGMessage("G0ON," + hvp.Farmland + ",M," + Algo.ListToString(others));
                                    }
                                }
                                else
                                {
                                    others.Remove(sel);
                                    // lose old myself
                                    RaiseGMessage(new Artiad.LosePet()
                                    {
                                        Owner = hvp.Farmer,
                                        SinglePet = myPt
                                    }.ToMessage());
                                    // lose the selection
                                    if (needHL)
                                    {
                                        RaiseGMessage(new Artiad.LosePet()
                                        {
                                            Owner = hvp.Farmland,
                                            SinglePet = sel,
                                            Stepper = hvp.Farmer,
                                            Recycle = false
                                        }.ToMessage());
                                    }
                                    result.Add(sel);
                                    // remove if reposit is not set, otherwise put it back
                                    if (others.Count > 0 && !hvp.Reposit)
                                    {
                                        if (needHL)
                                        {
                                            RaiseGMessage(new Artiad.LosePet()
                                            {
                                                Owner = hvp.Farmland,
                                                Pets = others.ToArray(),
                                                Recycle = false
                                            }.ToMessage());
                                        }
                                        RaiseGMessage("G0ON," + hvp.Farmland + ",M," + Algo.ListToString(others));
                                    }
                                }
                            }
                        }
                        if (result.Count > 0)
                        {
                            RaiseGMessage(new Artiad.ObtainPet()
                            {
                                Farmer = hvp.Farmer,
                                Farmland = hvp.Farmland,
                                Trophy = hvp.Trophy,
                                Pets = result.ToArray()
                            }.ToMessage());
                        }
                        if (giveBack.Count > 0)
                        {
                            RaiseGMessage(new Artiad.ObtainPet()
                            {
                                Farmer = hvp.Farmland,
                                Farmland = hvp.Farmer,
                                Trophy = hvp.Trophy,
                                Pets = giveBack.ToArray()
                            }.ToMessage());
                        }
                    }
                    else if (Artiad.KittyHelper.IsTrade(cmd))
                    {
                        Artiad.TradePet tdp = Artiad.TradePet.Parse(cmd);
                        if (tdp.AGoods.Length > 0)
                            RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = tdp.A,
                                Pets = tdp.AGoods,
                                Stepper = tdp.B,
                                Recycle = false
                            }.ToMessage());
                        if (tdp.BGoods.Length > 0)
                            RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = tdp.B,
                                Pets = tdp.BGoods,
                                Stepper = tdp.A,
                                Recycle = false
                            }.ToMessage());
                        if (tdp.BGoods.Length > 0)
                            RaiseGMessage(new Artiad.HarvestPet()
                            {
                                Farmer = tdp.A,
                                Farmland = tdp.B,
                                Pets = tdp.BGoods.ToArray(),
                                Reposit = false,
                                Plow = false
                            }.ToMessage());
                        if (tdp.AGoods.Length > 0)
                            RaiseGMessage(new Artiad.HarvestPet()
                            {
                                Farmer = tdp.B,
                                Farmland = tdp.A,
                                Pets = tdp.AGoods.ToArray(),
                                Reposit = false,
                                Plow = false
                            }.ToMessage());
                    }
                    break;
                case "G0HD":
                    Artiad.ObtainPet.Parse(cmd).Handle(this, WI); break;
                case "G0HH":
                    {
                        // G0HH,A,0/1,x,y..;T,F
                        int hdx = cmd.IndexOf(';');
                        string mainPart = cmd.Substring(0, hdx);
                        string[] mainParts = mainPart.Split(',');

                        ushort me = ushort.Parse(mainParts[1]);
                        ushort consumeType = ushort.Parse(mainParts[2]);
                        ushort mons = ushort.Parse(mainParts[3]);

                        int jdx = mainParts[0].Length + mainParts[1].Length +
                            mainParts[2].Length + mainParts[3].Length + 4;

                        string argsv = Algo.Substring(cmd, jdx, hdx);
                        string cargsv = argsv != "" ? "," + argsv : "";

                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Algo.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Algo.Substring(cmd, idx + 1, -1);

                        Player player = Board.Garden[me];
                        Monster monster = LibTuple.ML.Decode(mons);
                        int pe = monster.Element.Elem2Index();
                        if (player.Pets[pe] == mons && consumeType == 1)
                            RaiseGMessage("G0HI," + me + "," + mons);
                        WI.BCast("E0HH," + me + "," + consumeType + "," + mons + "," + sktInType + cargsv);
                        monster.ConsumeAction(player, consumeType, sktInType, sktFuse, argsv);
                        break;
                    }
                case "G0HI":
                    {
                        IDictionary<ushort, List<ushort>> jmc = new Dictionary<ushort, List<ushort>>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort pet = ushort.Parse(args[i + 1]);
                            Algo.AddToMultiMap(jmc, who, pet);
                        }
                        if (Board.InCampaign) // to mark as to be discard
                        {
                            jmc.Keys.ToList().ForEach(p =>
                            {
                                jmc[p].ForEach(q => RaiseGMessage("G1HK,0," + p + "," + q));
                                RaiseGMessage(new Artiad.AnnouceCard()
                                {
                                    Action = Artiad.AnnouceCard.Type.FLASH,
                                    Officer = p,
                                    Genre = Card.Genre.NMB,
                                    Cards = jmc[p].ToArray()
                                }.ToMessage());
                            });
                        }
                        else // to discard immediately
                        {
                            jmc.Keys.ToList().ForEach(p => RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = p,
                                Pets = jmc[p].ToArray()
                            }.ToMessage()));
                        }
                        WI.BCast("E0HI," + cmdrst);
                    }
                    break;
                case "G1HK":
                    if (args[1].Equals("0"))
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort mons = ushort.Parse(args[3]);
                        Board.CsPets.Add(who + "," + mons);
                    }
                    else if (args[1].Equals("1"))
                    {
                        foreach (string line in Board.CsPets)
                        {
                            int idx = line.IndexOf(',');
                            ushort who = ushort.Parse(line.Substring(0, idx));
                            Player py = Board.Garden[who];
                            ushort mons = ushort.Parse(line.Substring(idx + 1));
                            if (py.Pets.Contains(mons))
                                RaiseGMessage(new Artiad.LosePet() { Owner = who, SinglePet = mons }.ToMessage());
                        }
                        Board.CsPets.Clear();
                    }
                    break;
                case "G0HL":
                    Artiad.LosePet.Parse(cmd).Handle(this, WI); break;
                case "G0IC":
                    Artiad.JoinPetEffects.Parse(cmd).Handle(this, WI); break;
                case "G0OC":
                    Artiad.CollapsePetEffects.Parse(cmd).Handle(this, WI); break;
                case "G0HT":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int n = int.Parse(args[2]);
                        RaiseGMessage("G0DH," + who + ",0," + n);
                        break;
                    }
                case "G0HG":
                    for (int i = 1; i < args.Length; i += 2)
                    {
                        ushort who = ushort.Parse(args[i]);
                        int n = int.Parse(args[i + 1]);
                        RaiseGMessage("G0DH," + who + ",0," + n);
                    }
                    break;
                case "G0QR":
                    {
                        ushort who = ushort.Parse(args[1]);
                        Player player = Board.Garden[who];
                        if (player.Tux.Count > player.TuxLimit)
                            RaiseGMessage("G0DH," + who + ",1," + (player.Tux.Count - player.TuxLimit));
                        break;
                    }
                case "G0TT":
                    {
                        ushort who = ushort.Parse(args[1]);
                        AsyncInput(who, "//", "G0TT", "0");
                        int number = randomSeed.Next(6);
                        if (number == 0) number = 6;
                        WI.BCast("E0TT," + who + "," + number);
                        Board.DiceValue = number;
                    }
                    break;
                case "G0T7":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort orgPt = ushort.Parse(args[2]);
                        ushort newPt = ushort.Parse(args[3]);
                        WI.BCast("E0T7," + who + "," + orgPt + "," + newPt);
                        Board.DiceValue = newPt;
                    }
                    break;
                case "G0JM":
                    Artiad.Goto.Parse(cmd).Handle(this, WI); break;
                case "G0WN":
                    {
                        WI.RecvInfTermin();
                        WI.Send("F0WN," + cmdrst, Board.Garden.Keys.ToArray());
                        int count = Board.Garden.Keys.Count;
                        WI.RecvInfStart();
                        while (count > 0)
                        {
                            Base.VW.Msgs msg = WI.RecvInfRecvPending();
                            if (msg.Msg.StartsWith("F0WN"))
                                --count;
                            else
                                WI.Send("F0WN," + cmdrst, 0, msg.From);
                        }
                        WI.RecvInfEnd();
                        WI.Live("F0WN," + cmdrst);
                        WriteBytes(ps, "C3TM," + cmdrst);
                        lock (jumpTareget)
                        {
                            jumpTareget = "H0TM";
                            jumpEnd = "H0TM";
                        }
                        System.Threading.Thread.CurrentThread.Abort();
                    }
                    break;
                case "G0IJ":
                    {
                        ushort who = ushort.Parse(args[1]);
                        Player py = Board.Garden[who];
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            ushort n = ushort.Parse(args[3]);
                            py.TokenCount += n;
                            WI.BCast("E0IJ," + who + ",0," + n + "," + py.TokenCount);
                        }
                        else if (type == 1)
                        {
                            int n = int.Parse(args[3]);
                            List<string> heros = Algo.TakeRange(args, 4, 4 + n).ToList();
                            py.TokenExcl.AddRange(heros);
                            WI.BCast("E0IJ," + who + ",1," + n + "," + string.Join(",", heros) +
                                "," + py.TokenExcl.Count + "," + string.Join(",", py.TokenExcl));
                        }
                        else if (type == 2)
                        {
                            int n = int.Parse(args[3]);
                            List<ushort> tars = Algo.TakeRange(args, 4, 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            py.TokenTars.AddRange(tars);
                            WI.BCast("E0IJ," + who + ",2," + n + "," + string.Join(",", tars) +
                                "," + py.TokenTars.Count + "," + string.Join(",", py.TokenTars));
                        }
                        else if (type == 3)
                        {
                            if (!py.TokenAwake)
                            {
                                py.TokenAwake = true;
                                WI.BCast("E0IJ," + who + ",3");
                            }
                        }
                        else if (type == 4)
                        {
                            int n = int.Parse(args[3]);
                            List<ushort> folders = Algo.TakeRange(args, 4, 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            py.TokenFold.AddRange(folders);
                            WI.Send("E0IJ," + who + ",4,0," + n + "," + string.Join(",", folders) +
                                "," + py.TokenFold.Count + "," + string.Join(",", py.TokenFold), 0, who);
                            WI.Send("E0IJ," + who + ",4,1," + n + "," + py.TokenFold.Count, ExceptStaff(who));
                            WI.Live("E0IJ," + who + ",4,1," + n + "," + py.TokenFold.Count);
                        }
                    }
                    break;
                case "G0OJ":
                    {
                        ushort who = ushort.Parse(args[1]);
                        Player py = Board.Garden[who];
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            ushort n = ushort.Parse(args[3]);
                            py.TokenCount -= n;
                            WI.BCast("E0OJ," + who + ",0," + n + "," + py.TokenCount);
                        }
                        else if (type == 1)
                        {
                            int n = int.Parse(args[3]);
                            List<string> heros = Algo.TakeRange(args, 4, 4 + n).ToList();
                            py.TokenExcl.RemoveAll(p => heros.Contains(p));
                            if (py.TokenExcl.Count > 0)
                                WI.BCast("E0OJ," + who + ",1," + n + "," + string.Join(",", heros) +
                                    "," + py.TokenExcl.Count + "," + string.Join(",", py.TokenExcl));
                            else
                                WI.BCast("E0OJ," + who + ",1," + n + "," + string.Join(",", heros) + ",0");
                        }
                        else if (type == 2)
                        {
                            int n = int.Parse(args[3]);
                            List<ushort> tars = Algo.TakeRange(args, 4, 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            py.TokenTars.RemoveAll(p => tars.Contains(p));
                            if (py.TokenTars.Count > 0)
                                WI.BCast("E0OJ," + who + ",2," + n + "," + string.Join(",", tars) +
                                    "," + py.TokenTars.Count + "," + string.Join(",", py.TokenTars));
                            else
                                WI.BCast("E0OJ," + who + ",2," + n + "," + string.Join(",", tars) + ",0");
                        }
                        else if (type == 3)
                        {
                            if (py.TokenAwake)
                            {
                                py.TokenAwake = false;
                                WI.BCast("E0OJ," + who + ",3");
                            }
                        }
                        else if (type == 4)
                        {
                            int n = int.Parse(args[3]);
                            List<ushort> folders = Algo.TakeRange(args, 4, 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            py.TokenFold.RemoveAll(p => folders.Contains(p));
                            if (py.TokenFold.Count > 0)
                                WI.Send("E0OJ," + who + ",4,0," + n + "," + string.Join(",", folders) +
                                    "," + py.TokenFold.Count + "," + string.Join(",", py.TokenFold), 0, who);
                            else
                                WI.Send("E0OJ," + who + ",4,0," + n + ","
                                    + string.Join(",", folders) + ",0", 0, who);
                            WI.Send("E0OJ," + who + ",4,1," + n + "," + py.TokenFold.Count, ExceptStaff(who));
                            WI.Live("E0OJ," + who + ",4,1," + n + "," + py.TokenFold.Count);
                        }
                    }
                    break;
                case "G0IE":
                    if (Artiad.KittyHelper.IsEnablePlayerPetEffect(cmd))
                        Artiad.EnablePlayerPetEffect.Parse(cmd).Handle(this, WI);
                    else if (Artiad.KittyHelper.IsEnableItPetEffect(cmd))
                        Artiad.EnableItPetEffect.Parse(cmd).Handle(this, WI);
                    break;
                case "G0OE":
                    if (Artiad.KittyHelper.IsDisablePlayerPetEffect(cmd))
                        Artiad.DisablePlayerPetEffect.Parse(cmd).Handle(this, WI);
                    else if (Artiad.KittyHelper.IsDisableItPetEffect(cmd))
                        Artiad.DisableItPetEffect.Parse(cmd).Handle(this, WI);
                    break;
                case "G0IS":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort op = ushort.Parse(args[2]);
                        bool hind = (op & 1) == 0;
                        bool roolBack = (op & 2) == 0;
                        // bool fromChange = op & 4;
                        string e0is = "";
                        for (int i = 3; i < args.Length; ++i)
                        {
                            Skill skill;
                            if (sk01.TryGetValue(args[i], out skill))
                            {
                                Player py = Board.Garden[who];
                                if (!py.Skills.Contains(skill.Code))
                                {
                                    py.Skills.Add(skill.Code);
                                    AddSingleSkill(who, skill, sk02, sk03);
                                    if (roolBack)
                                        py.IsZhu = true;
                                    e0is += "," + args[i];
                                }
                            }
                        }
                        if (e0is != "" && !hind)
                            WI.BCast("E0IS," + who + e0is);
                    }
                    break;
                case "G0OS":
                    {
                        ushort who = ushort.Parse(args[1]);
                        bool hind = (args[2] == "0");
                        string e0os = "";
                        for (int i = 3; i < args.Length; ++i)
                        {
                            Skill skill;
                            if (sk01.TryGetValue(args[i], out skill))
                            {
                                Player py = Board.Garden[who];
                                if (py.Skills.Remove(skill.Code))
                                {
                                    RemoveSingleSkill(who, skill, sk02, sk03);
                                    e0os += "," + args[i];
                                }
                            }
                        }
                        if (e0os != "" && !hind)
                            WI.BCast("E0OS," + who + e0os);
                    }
                    break;
                case "G0LA":
                    Artiad.InnateChange.Parse(cmd).Handle(this); break;
                case "G0IV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        int hero = int.Parse(args[2]);
                        Player py = Board.Garden[ut];
                        py.Coss.Push(hero);
                        WI.BCast("E0IV," + ut + "," + hero);
                    }
                    break;
                case "G0OV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        Player py = Board.Garden[ut];
                        int hero = py.Coss.Pop();
                        int next = py.Coss.Count > 0 ? py.Coss.Peek() : 0;
                        WI.BCast("E0OV," + ut + "," + hero + "," + next);
                    }
                    break;
                case "G0PB":
                    {
                        IDictionary<ushort, string> dict = new Dictionary<ushort, string>();
                        foreach (ushort ut in Board.Garden.Keys)
                            dict[ut] = "";
                        string word0 = "";
                        for (int i = 2; i < args.Length;)
                        {
                            ushort who = ushort.Parse(args[i]);
                            int n = ushort.Parse(args[i + 1]);
                            List<ushort> cards = Algo.TakeRange(args, i + 2, i + 2 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            if (args[1] == "0")
                            {
                                // We believe we don't claim the card source and its flow now
                                // RaiseGMessage("G0OT," + who + "," + n + "," + string.Join(",", cards));
                                Board.TuxPiles.PushBack(cards);
                            }
                            else if (args[1] == "1")
                                Board.MonPiles.PushBack(cards);
                            else if (args[1] == "2")
                                Board.EvePiles.PushBack(cards);
                            i += (2 + n);
                            word0 += "," + who + ",1," + n;
                            foreach (ushort ut in Board.Garden.Keys)
                            {
                                if (ut == who)
                                    dict[who] += "," + who + ",0," + n + "," + string.Join(",", cards);
                                else
                                    dict[ut] += "," + who + ",1," + n;
                            }
                        }
                        foreach (var pair in dict)
                        {
                            if (pair.Value.Length > 0)
                                WI.Send("E0PB," + args[0] + pair.Value, 0, pair.Key);
                        }
                        WI.Live("E0PB," + args[0] + word0);
                    }
                    break;
                case "G0YM":
                    WI.BCast("E0YM," + cmdrst);
                    break;
                case "G1XR":
                    if (args[1] == "0" || args[1] == "2")
                    {
                        ushort from = ushort.Parse(args[2]);
                        ushort to = ushort.Parse(args[3]);
                        int m = int.Parse(args[4]);
                        int n = int.Parse(args[5]);
                        Player pf = Board.Garden[from];
                        Player pt = Board.Garden[to];

                        int o = Math.Min(pf.Tux.Count, pt.Tux.Count);
                        if (n == 0 || m > o)
                            m = n = o;
                        else if (n > o)
                            n = o;
                        string nm = "";
                        if (m == 0 && n == 1)
                            nm = "/Q" + n;
                        else if (m == 0)
                            nm = "/Q1~" + n;
                        else if (m == n)
                            nm = "Q" + m;
                        else
                            nm = "Q" + m + "~" + n;
                        nm = "#交予的," + nm;
                        int acnt = 0;
                        List<ushort> pftx = pf.Tux.ToList();
                        do
                        {
                            string chooseOne = AsyncInput(from, nm + "(p" +
                                string.Join("p", pftx) + ")", "G1XR", "0");
                            List<ushort> ut = (chooseOne.StartsWith("/") || chooseOne == VI.CinSentinel) ?
                                new List<ushort>() : chooseOne.Split(',')
                                .Select(p => ushort.Parse(p)).Intersect(pf.Tux).ToList();
                            if (ut.Count >= m && ut.Count <= n)
                            {
                                acnt = ut.Count;
                                if (acnt != 0)
                                    RaiseGMessage("G0HQ,0," + to + "," + from + ",1," +
                                        ut.Count + "," + string.Join(",", ut));
                                break;
                            }
                        } while (true);
                        if (acnt != 0)
                        {
                            do
                            {
                                List<ushort> pttx = (args[1] == "2") ?
                                    pt.Tux.Except(pftx).ToList() : pt.Tux.ToList();
                                string chooseTwo = AsyncInput(to, "#交予的,Q" + acnt + "(p" +
                                    string.Join("p", pttx) + ")", "G1XR", "0");
                                List<ushort> ut = (chooseTwo.StartsWith("/") || chooseTwo == VI.CinSentinel) ?
                                    new List<ushort>() : chooseTwo.Split(',')
                                    .Select(p => ushort.Parse(p)).Intersect(pt.Tux).ToList();
                                if (ut.Count == acnt)
                                {
                                    RaiseGMessage("G0HQ,0," + from + "," + to + ",1," +
                                        ut.Count + "," + string.Join(",", ut));
                                    break;
                                }
                            } while (true);
                        }
                    }
                    else if (args[1] == "1")
                    {
                        int m = int.Parse(args[2]);
                        int n = int.Parse(args[3]);
                        List<ushort> invs = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        IDictionary<ushort, string> dict = new Dictionary<ushort, string>();
                        //IDictionary<ushort, int> rvAlls = new Dictionary<ushort, int>();
                        IDictionary<ushort, List<ushort>> actual = new Dictionary<ushort, List<ushort>>();
                        foreach (ushort ut in invs)
                        {
                            int c = Board.Garden[ut].Tux.Count;
                            if (c != 0)
                            {
                                if (n == 0)
                                {
                                    //rvAlls[ut] = c;
                                    //string allTux = string.Join(",", Board.Garden[ut].Tux);
                                    //g0ot += "," + ut + "," + c + "," + allTux;
                                    actual[ut] = Board.Garden[ut].Tux.ToList();
                                }
                                else if (c <= m)
                                {
                                    string nm = "Q" + c;
                                    string body = "(p" + string.Join("p", Board.Garden[ut].Tux) + ")";
                                    dict[ut] = nm + body;
                                }
                                else
                                {
                                    if (c > n)
                                        c = n;
                                    string nm = "";
                                    if (m == 0 && c == 1)
                                        nm = "/Q" + c;
                                    else if (m == 0)
                                        nm = "/Q1~" + c;
                                    else if (m == c)
                                        nm = "Q" + m;
                                    else
                                        nm = "Q" + m + "~" + c;
                                    string body = "(p" + string.Join("p", Board.Garden[ut].Tux) + ")";
                                    dict[ut] = nm + body;
                                }
                            }
                        }
                        IDictionary<ushort, string> reply = MultiAsyncInput(dict);
                        foreach (var pair in reply)
                        {
                            if (!pair.Value.StartsWith("/") && pair.Value != VI.CinSentinel)
                            {
                                List<ushort> uts = pair.Value.Split(',')
                                    .Select(p => ushort.Parse(p)).ToList();
                                actual[pair.Key] = uts;
                            }
                        }
                        if (actual.Count > 0)
                        {
                            RaiseGMessage("G0OT," + string.Join(",", actual.Select(p =>
                                p.Key + "," + p.Value.Count + "," + string.Join(",", p.Value))));
                            foreach (var pair in actual)
                            {
                                WI.BCast("E0QZ," + pair.Key + "," + string.Join(",", pair.Value));
                                RaiseGMessage("G0ON," + pair.Key + ",C,"
                                     + pair.Value.Count + "," + string.Join(",", pair.Value));
                            }
                        }
                        foreach (var pair in actual)
                            RaiseGMessage("G0HQ,2," + pair.Key + ",1," + pair.Value.Count);
                    }
                    break;
                case "G0HR":
                    if (args[1] == "0")
                    {
                        if (args[2] == "0" && !Board.ClockWised)
                        {
                            Board.ClockWised = true;
                            WI.BCast("E0HR,0");
                        }
                        else if (args[2] == "1")
                        {
                            Board.ClockWised = !Board.ClockWised;
                            WI.BCast("E0HR," + (Board.ClockWised ? 0 : 1));
                        }
                    }
                    else if (args[1] == "1")
                    {
                        if (args[2] == "0" && !Board.ClockWised)
                        {
                            Board.ClockWised = true;
                            WI.BCast("E0HR,0");
                        }
                        else if (args[2] == "1" && Board.ClockWised)
                        {
                            Board.ClockWised = false;
                            WI.BCast("E0HR,1");
                        }
                    }
                    break;
                case "G17F":
                    Artiad.CoachingSign.Parse(cmd).Handle(this); break;
                case "G0FI":
                    Artiad.CoachingChange.Parse(cmd).Handle(this, WI); break;
                case "G0ON":
                    for (int idx = 1; idx < args.Length;)
                    {
                        //string fromZone = args[idx];
                        string cardType = args[idx + 1];
                        int cnt = int.Parse(args[idx + 2]);
                        if (cnt > 0)
                        {
                            List<ushort> cds = Algo.TakeRange(args, idx + 3, idx + 3 + cnt)
                                .Select(p => ushort.Parse(p)).ToList();
                            if (cardType == "C")
                                Board.TuxDises.AddRange(cds);
                            else if (cardType == "M")
                                Board.MonDises.AddRange(cds);
                            else if (cardType == "E")
                                Board.EveDises.AddRange(cds);
                        }
                        idx += (3 + cnt);
                    }
                    WI.BCast("E0ON," + cmdrst);
                    break;
                case "G0SN":
                    {
                        ushort lugUt = ushort.Parse(args[2]);
                        bool dirIn = args[3] == "0";
                        string[] cards = Algo.TakeRange(args, 4, args.Length);
                        Base.Card.Luggage lug = LibTuple.TL.DecodeTux(lugUt) as Base.Card.Luggage;
                        if (lug != null)
                        {
                            if (dirIn)
                            {
                                lug.Capacities.AddRange(cards);
                                WI.BCast("E0SN," + cmdrst);
                                //RaiseGMessage("G2TZ," + who + ",0," + string.Join(",", cards));
                            }
                            else
                            {
                                List<string> rms = new List<string>();
                                foreach (string cd in cards)
                                {
                                    if (lug.Capacities.Remove(cd))
                                        rms.Add(cd);
                                }
                                WI.BCast("E0SN," + cmdrst);
                                //if (rms.Count > 0)
                                //    RaiseGMessage("G2TZ," + who + ",1," + string.Join(",", rms));
                            }
                        }
                    }
                    break;
                case "G0MA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort guad = ushort.Parse(args[2]);
                        Board.Garden[who].Guardian = guad;
                        WI.BCast("E0MA," + cmdrst);
                    }
                    break;
                case "G0ZJ":
                    {
                        Artiad.EqSlotVariation esv = Artiad.EqSlotVariation.Parse(cmd);
                        Player py = Board.Garden[esv.Who];
                        if (esv.Increase)
                        {
                            Action<int> action = (mask) =>
                            {
                                if ((py.FyMask & mask) != 0)
                                    py.FyMask &= ~mask;
                                else
                                    py.ExMask |= 0x1;
                            };
                            if (esv.Slot == Artiad.ClothingHelper.SlotType.WQ)
                                action(0x1);
                            else if (esv.Slot == Artiad.ClothingHelper.SlotType.WQ)
                                action(0x2);
                            else if (esv.Slot == Artiad.ClothingHelper.SlotType.WQ)
                                action(0x4);
                        }
                        else
                        {
                            Action<int, ushort, Artiad.ClothingHelper.SlotType, Action<ushort>> action =
                                (mask, org, slot, setOrg) =>
                            {
                                if ((py.ExMask & mask) != 0)
                                {
                                    if (py.ExEquip != 0) // only consider when exequip is not empty
                                    {
                                        if (org == 0) // OK, move it.
                                        {
                                            ushort now = py.ExEquip;
                                            setOrg(now); py.ExEquip = 0;
                                            new Artiad.EqSlotMoveSemaphore()
                                            {
                                                Who = esv.Who,
                                                Slot = slot,
                                                Card = now
                                            }.Telegraph(WI.BCast);
                                        }
                                        else
                                        {
                                            ushort choose = ushort.Parse(AsyncInput(esv.Who, "#保留的,C1(p"
                                                + org + "p" + py.ExEquip + ")", "G0ZJ", "0"));
                                            if (choose == org)
                                                RaiseGMessage("G0QZ," + esv.Who + "," + py.ExEquip);
                                            else
                                            {
                                                RaiseGMessage("G0QZ," + esv.Who + "," + org);
                                                ushort now = py.ExEquip;
                                                setOrg(now); py.ExEquip = 0;
                                                new Artiad.EqSlotMoveSemaphore()
                                                {
                                                    Who = esv.Who,
                                                    Slot = slot,
                                                    Card = now
                                                }.Telegraph(WI.BCast);
                                            }
                                        }
                                    }
                                    py.ExMask &= ~mask;
                                }
                                else
                                {
                                    if (org != 0)
                                        RaiseGMessage("G0QZ," + esv.Who + "," + org);
                                    py.FyMask |= mask;
                                }
                            };
                            if (esv.Slot == Artiad.ClothingHelper.SlotType.WQ)
                                action(0x1, py.Weapon, esv.Slot, p => py.Weapon = p);
                            else if (esv.Slot == Artiad.ClothingHelper.SlotType.FJ)
                                action(0x2, py.Armor, esv.Slot, p => py.Armor = p);
                            else if (esv.Slot == Artiad.ClothingHelper.SlotType.XB)
                                action(0x4, py.Trove, esv.Slot, p => py.Trove = p);
                        }
                    }
                    break;
                case "G1NI":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort ut = ushort.Parse(args[2]);
                        if (ut > 0)
                        {
                            NPC npc = LibTuple.NL.Decode(NMBLib.OriginalNPC(ut));
                            npc.Debut(Board.Garden[who]);
                        }
                    }
                    break;
                case "G0IF":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> runes = new List<ushort>();
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort sf = ushort.Parse(args[i]);
                            if (!Board.Garden[who].Runes.Contains(sf))
                            {
                                Board.Garden[who].Runes.Add(sf);
                                runes.Add(sf);
                            }
                        }
                        if (runes.Count > 0)
                            WI.BCast("E0IF," + who + "," + string.Join(",", runes));
                    }
                    break;
                case "G0OF":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> runes = new List<ushort>();
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort sf = ushort.Parse(args[i]);
                            if (Board.Garden[who].Runes.Contains(sf))
                            {
                                Board.Garden[who].Runes.Remove(sf);
                                runes.Add(sf);
                            }
                        }
                        if (runes.Count > 0)
                            WI.BCast("E0OF," + who + "," + string.Join(",", runes));
                    }
                    break;
                case "G1GE":
                    for (int i = 1; i < args.Length; i += 2)
                    {
                        bool? isWin = null;
                        if (args[i] == "W")
                            isWin = true;
                        else if (args[i] == "L")
                            isWin = false;
                        ushort mon = ushort.Parse(args[i + 1]);
                        Monster monster = LibTuple.ML.Decode(mon);
                        if (monster != null)
                        {
                            if (isWin == true)
                                monster.WinEff();
                            else if (isWin == false)
                                monster.LoseEff();
                        }
                    }
                    break;
                case "G1UE":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort provider = ushort.Parse(args[2]);
                        ushort cardUt = ushort.Parse(args[3]);
                        TuxEqiup te = LibTuple.TL.DecodeTux(cardUt) as TuxEqiup;
                        te.UseAction(cardUt, Board.Garden[who], provider);
                    }
                    break;
                case "G0PQ":
                    {
                        RaiseGMessage("G0OT," + cmdrst);
                        int idx = 1;
                        while (idx < args.Length)
                        {
                            ushort who = ushort.Parse(args[idx++]);
                            int n = int.Parse(args[idx++]);
                            ushort[] tuxes = Algo.TakeRange(args, idx, idx + n)
                                .Select(p => ushort.Parse(p)).ToArray();
                            WI.Send("E0PQ,0," + who + "," + string.Join(",", tuxes), 0, who);
                            WI.Send("E0PQ,1," + who + "," + n, ExceptStaff(who));
                            WI.Live("E0PQ,1," + who + "," + n);
                            idx += n;
                        }
                    }
                    break;
            }
        }
        #endregion G-Detail
    }
}
