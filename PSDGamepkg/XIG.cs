using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base.Card;
using System.IO.Pipes;
using System.IO;

namespace PSD.PSDGamepkg
{
    public partial class XI
    {
        #region G-Loop
        // Raise Command from skill declaration, without Priory Control
        public void RaiseGMessage(string cmd)
        {
            if (cmd.StartsWith("G"))
            {
                //VI.Cout(0, "☆◇○" + cmd + "○◇☆");
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
            string zero = Util.Substring(cmd, 0, cmd.IndexOf(','));
            List<SkTriple> _pocket;
            if (!sk02.TryGetValue(zero, out _pocket)) { return; }
            else if (_pocket.Count == 0) { return; }
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

                string cop = zero.Substring(2);
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
                        string mai = Util.Substring(msg, idx + 1, jdx);
                        string inType = Util.Substring(msg, jdx + 1, -1);

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
                        //Fill(involved, false);
                        //involved[me] |= true;
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
            string cmdrst = Util.Substring(cmd, "G0xx,".Length, -1);
            int nextPriority = priority + 1; // might change during execution
            switch (args[0])
            {
                case "G0OH":
                    if (priority == 100)
                    {
                        string result = "";
                        List<Artiad.Harm> harms = Artiad.Harm.Parse(cmd);
                        foreach (Artiad.Harm harm in harms)
                        {
                            Player py = Board.Garden[harm.Who];
                            //if (Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.ALIVE) && py.HP - harm.N <= 0)
                            //    harm.N = py.HP - 1;
                            if (harm.N > 0)
                            {
                                if (py.HP - harm.N <= 0)
                                    harm.N = py.HP;
                                py.HP -= harm.N;
                                if (harm.N > 0)
                                {
                                    result += "," + harm.Who + "," + Artiad.IntHelper.Elem2Int(harm.Element)
                                        + "," + harm.N + "," + py.HP;
                                }
                                //if (player.HP == 0)
                                //    death += "," + me;
                            }
                        }
                        if (!result.Equals(""))
                            WI.BCast("E0OH" + result);
                        //if (!death.Equals(""))
                        //    RaiseGMessage("G0ZH" + death);
                    }
                    else if (priority == 200)
                    {
                        List<ushort> zeros = Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0)
                            .Select(p => p.Uid).ToList();
                        if (zeros.Count > 0)
                        {
                            WI.BCast("E0ZH," + string.Join(",", zeros));
                            RaiseGMessage("G0ZH,0");
                        }
                        // ISet<ushort> death = new HashSet<ushort>();
                        // List<Artiad.Harm> harms = Artiad.Harm.Parse(cmd);
                        // foreach (Artiad.Harm harm in harms)
                        // {
                        //     if (Board.Garden[harm.Who].IsAlive && Board.Garden[harm.Who].HP == 0)
                        //         death.Add(harm.Who);
                        // }
                        // if (death.Count > 0)
                        // {
                        //     WI.BCast("E0ZH," + string.Join(",", death));
                        //     RaiseGMessage("G0ZH," + string.Join(",", death));
                        // }
                    }
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
                        IEnumerable<ushort> players = Util.TakeRange(args, 1, args.Length).Select(p => ushort.Parse(p));
                        RaiseGMessage("G0DH," + string.Join(",", players.Select(p => p + ",3")));
                        foreach (ushort ut in players)
                        {
                            Player player = Board.Garden[ut];
                            List<ushort> g2onuts = new List<ushort>();
                            foreach (ushort pt in player.Pets)
                            {
                                if (pt != 0)
                                {
                                    RaiseGMessage("G0HL," + ut + "," + pt);
                                    g2onuts.Add(pt);
                                }
                            }
                            if (player.Escue.Count > 0)
                            {
                                List<ushort> esc = player.Escue.ToList();
                                player.Escue.Clear();
                                RaiseGMessage("G2OL," + string.Join(",", esc.Select(
                                    p => (player.Uid + "," + p))));
                                g2onuts.AddRange(esc);
                            }
                            if (g2onuts.Count > 0)
                                RaiseGMessage("G0ON," + ut + ",M," + g2onuts.Count
                                     + "," + string.Join(",", g2onuts));
                        }
                    }
                    else if (priority == 300) // let teammates obtain tux
                    {
                        for (int i = 1; i < args.Length; ++i)
                        {
                            ushort me = ushort.Parse(args[i]);
                            Player player = Board.Garden[me];
                            string range = Util.SSelect(Board,
                                p => p.IsAlive && p.Team == player.Team);
                            string input = AsyncInput(me, "#获得补牌的,T1" + range, "G0ZW", "0");
                            RaiseGMessage("G0DH," + input + ",0,2");
                        }
                    }
                    else if (priority == 400) // leave
                    {
                        List<Player> players = Util.TakeRange(args, 1, args.Length)
                            .Select(p => Board.Garden[ushort.Parse(p)]).Where(p => !p.IsAlive).ToList();
                        if (players.Count > 0)
                            RaiseGMessage("G0OY," + string.Join(",", players.Select(p => "2," + p.Uid)));
                    }
                    break;
                case "G0OY":
                    if (priority == 100)
                    {
                        string g1zl = "", g0oc = "";
                        List<string> g0qzs = new List<string>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            int changeType = int.Parse(args[i]);
                            ushort who = ushort.Parse(args[i + 1]);
                            Player player = Board.Garden[who];
                            Hero hero = LibTuple.HL.InstanceHero(player.SelectHero);

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
                                foreach (ushort ut in player.Pets)
                                    if (ut != 0 && !Board.NotActionPets.Contains(ut))
                                    {
                                        if (changeType == 1)
                                            g0oc += ",1," + player.Uid + "," + ut;
                                        else
                                            g0oc += ",0," + player.Uid + "," + ut;
                                    }
                            }
                            if (changeType == 0 || changeType == 2)
                            {
                                List<ushort> excds = new List<ushort>();
                                if (player.ExEquip != 0)
                                    excds.Add(player.ExEquip);
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
                        if (g0oc != "")
                            RaiseGMessage("G0OC" + g0oc);
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
                                Artiad.ContentRule.ErasePlayerToken(player, Board, RaiseGMessage);
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
                                    if (changeType == 2)
                                    {
                                        Board.Supporter = null;
                                        Board.SupportSucc = false;
                                    }
                                    mightInvolve = true;
                                }
                                else if (Board.Hinder.Uid == who)
                                {
                                    if (changeType == 2)
                                    {
                                        Board.Hinder = null;
                                        Board.HinderSucc = false;
                                    }
                                    mightInvolve = true;
                                }
                                if (mightInvolve && Board.InFight)
                                    _9ped = true;
                            }
                        }
                        if (rounded)
                            RaiseGMessage("G0JM,R" + Board.Rounder.Uid + "ED");
                        if (_9ped)
                            RaiseGMessage("G09P,0");
                    }
                    break;
                case "G0CC": // prepare to use card
                    if (priority == 100)
                    {
                        // G0CC,A,T,B,TP02,17,36
                        ushort ust = ushort.Parse(args[1]);
                        ushort adapter = ushort.Parse(args[2]);
                        ushort hst = ushort.Parse(args[3]);
                        string cardname = args[4];
                        int hdx = cmd.IndexOf(';');
                        int idx = cmd.IndexOf(',', hdx);

                        int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Util.Substring(cmd, idx + 1, -1);
                        Base.Card.Tux tux = tx01[cardname];
                        string[] argv = cmd.Substring(0, hdx).Split(',');

                        List<ushort> cards = Util.TakeRange(argv, 5, argv.Length).Select(p =>
                            ushort.Parse(p)).Where(p => p > 0 && Board.Garden[ust].Tux.Contains(p)).ToList();
                        if (cards.Any())
                        {
                            RaiseGMessage("G0OT," + ust + "," + cards.Count + "," + string.Join(",", cards));
                            Board.PendingTux.Enqueue(cards.Select(p => hst + ",G0CC," + p));
                            RaiseGMessage("G2TZ,0," + ust + "," + string.Join(",", cards.Select(p => "C" + p)));
                        }
                        WI.BCast("E0CC," + Util.Substring(cmd, "G0CC,".Length, hdx));
                    } else if (priority == 200) {
                        ushort ust = ushort.Parse(args[1]);
                        ushort adapter = ushort.Parse(args[2]);
                        ushort hst = ushort.Parse(args[3]);
                        string cardname = args[4];
                        int hdx = cmd.IndexOf(';');
                        int idx = cmd.IndexOf(',', hdx);

                        int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Util.Substring(cmd, idx + 1, -1);
                        Base.Card.Tux tux = tx01[cardname];
                        string[] argv = cmd.Substring(0, hdx).Split(',');
                        List<ushort> cards = Util.TakeRange(argv, 5, argv.Length).Select(p =>
                            ushort.Parse(p)).Where(p => p > 0).ToList();
                        if (cards.Any())
                        {
                            if ((tux.IsEq[sktInType] & 3) == 0)
                            {
                                foreach (ushort p in cards)
                                    Board.PendingTux.Remove(hst + ",G0CC," + p);
                                RaiseGMessage("G0ON," + ust + ",C," + cards.Count + "," + string.Join(",", cards));
                            }
                            else if ((tux.IsEq[sktInType] & 1) != 0)
                            {
                                foreach (ushort p in cards)
                                    Board.PendingTux.Remove(hst + ",G0CC," + p);
                                Board.PendingTux.Enqueue(cards.Select(p => hst + ",G0ZB," + tux.Code + "," + p));
                            }
                        }
                    }
                    else if (priority == 300)
                    {
                        RaiseGMessage("G0CD," + args[3] + "," + args[2] + "," +
                            string.Join(",", Util.TakeRange(args, 4, args.Length)));
                    }
                    else if (priority == 400)
                    {
                        int hdx = cmd.IndexOf(';');
                        string[] argv = cmd.Substring(0, hdx).Split(',');
                        List<ushort> cards = Util.TakeRange(argv, 5, argv.Length).Select(p =>
                            ushort.Parse(p)).ToList();
                        List<ushort> accu = new List<ushort>();
                        foreach (ushort card in cards)
                        {
                            List<string> rms = new List<string>();
                            foreach (string tuxInfo in Board.PendingTux)
                            {
                                string[] parts = tuxInfo.Split(',');
                                if (parts[1] == "G0CC")
                                {
                                    for (int j = 2; j < parts.Length; ++j)
                                        if (parts[j] == card.ToString())
                                        {
                                            rms.Add(tuxInfo);
                                            accu.Add(card);
                                        }
                                }
                                else if (parts[1] == "G0ZB")
                                {
                                    if (parts[3] == card.ToString())
                                    {
                                        rms.Add(tuxInfo);
                                        accu.Add(card);
                                    }
                                }
                            }
                            foreach (string rm in rms)
                                Board.PendingTux.Remove(rm);
                        }
                        if (accu.Count > 0)
                            RaiseGMessage("G0ON,10,C," + accu.Count + "," + string.Join(",", accu));
                    } break;
                case "G0HZ":
                    if (priority == 100)
                    {
                        //Board.IsTangled = true;
                        ushort who = ushort.Parse(args[1]);
                        ushort mon = ushort.Parse(args[2]);
                        if (mon != 0)
                            Board.Monster2 = mon;
                    }
                    else if (priority == 200)
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort mon = Board.Monster2;
                        if (mon == 0)
                            WI.BCast("E0HZ,0," + who);
                        else
                        {
                            if (NMBLib.IsMonster(mon))
                            {
                                RaiseGMessage("G0IP," + Board.Rounder.OppTeam + "," +
                                    LibTuple.ML.Decode(NMBLib.OriginalMonster(mon)).STR);
                                WI.BCast("E0HZ,1," + who + "," + mon);
                                RaiseGMessage("G0YM,1," + mon + ",0");
                            }
                            else if (NMBLib.IsNPC(mon))
                            {
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
                                    RaiseGMessage("G0IP," + Board.Rounder.Team + "," + (npc.STR * 2));
                                    WI.BCast("E0HZ,3," + who + "," + mon);
                                }
                                else
                                {
                                    RaiseGMessage("G0IP," + Board.Rounder.Team + "," + npc.STR);
                                    WI.BCast("E0HZ,2," + who + "," + mon);
                                }
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
                        ushort trigger = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);

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
                    }
                    else if (priority == 200)
                    {
                        ushort trigger = ushort.Parse(args[1]);
                        Base.Card.Evenement eve = LibTuple.EL.DecodeEvenement(Board.Eve);
                        eve.Action(Board.Garden[trigger]);
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
            string cmdrst = Util.Substring(cmd, cmd.IndexOf(',') + 1, -1);
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
                        if (op == 0)
                        {
                            if (nofp != 0)
                            {
                                ushort[] invs = Util.TakeRange(args, 4, 4 + nofp).Select(p => ushort.Parse(p)).ToArray();
                                WI.Send("E0FU,0," + string.Join(",", Util.TakeRange(args, 4 + nofp, args.Length)), invs);
                                WI.Send("E0FU,1," + (args.Length - 4 - nofp), ExceptStaff(invs));
                                WI.Live("E0FU,1," + (args.Length - 4 - nofp));
                            }
                            else
                                WI.BCast("E0FU,0," + string.Join(",", Util.TakeRange(args, 4, args.Length)));
                        }
                        else
                        {
                            if (nofp != 0)
                            {
                                ushort[] invs = Util.TakeRange(args, 4, 4 + nofp).Select(p => ushort.Parse(p)).ToArray();
                                WI.Send("E0FU,0," + string.Join(",", Util.TakeRange(
                                    args, 4 + nofp, args.Length)), invs.Except(new ushort[] { op }).ToArray());
                                WI.Send("E0FU,1," + (args.Length - 4 - nofp), ExceptStaff(invs));
                                WI.Live("E0FU,1," + (args.Length - 4 - nofp));
                                WI.Send("E0FU,4," + string.Join(",", Util.TakeRange(args, 4 + nofp, args.Length)), 0, op);
                            }
                            else
                            {
                                WI.Send("E0FU,0," + string.Join(",", Util.TakeRange(
                                    args, 4 + nofp, args.Length)), ExceptStaff(op));
                                WI.Live("E0FU,0," + string.Join(",", Util.TakeRange(args, 4, args.Length)));
                                WI.Send("E0FU,4," + string.Join(",", Util.TakeRange(args, 4 + nofp, args.Length)), 0, op);
                            }
                        }
                    }
                    //else if (type == 1)
                    //{
                    //    ushort nofp = ushort.Parse(args[2]);
                    //    ushort[] invs = Util.TakeRange(args, 3, 3 + nofp).Select(p => ushort.Parse(p)).ToArray();
                    //    //ushort nocp = ushort.Parse(args[3 + nofp]);
                    //    //ushort[] jnvs = Util.TakeRange(args, 4 + nofp, 4 + nofp + nocp)
                    //    //    .Select(p => ushort.Parse(p)).ToArray();
                    //    //WI.Send("E0FU,0," + string.Join(",", Util.TakeRange(args, 4 + nofp + nocp, args.Length)), invs);
                    //    //WI.Send("E0FU,1," + (args.Length - 4 - nofp - nocp), jnvs);
                    //    //WI.Live("E0FU,1," + (args.Length - 4 - nofp - nocp));
                    //    WI.Send("E0FU,0," + string.Join(",",
                    //        Util.TakeRange(args, 3 + nofp, args.Length)), ExceptStaff(invs));
                    //    WI.Live("E0FU,0," + string.Join(",", Util.TakeRange(args, 3 + nofp, args.Length)));
                    //}
                    else if (type == 2)
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort[] invs = Util.TakeRange(args, 3, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        WI.BCast("E0FU,2," + who + "," + string.Join(",", invs));
                    }
                    else if (type == 3)
                        WI.BCast("E0FU,3");
                    else if (type == 4)
                        WI.BCast("E0FU,5," + string.Join(",", Util.TakeRange(args, 2, args.Length)));
                }
                else if (args[0].StartsWith("G2QU"))
                {
                    if (args[1] == "0")
                    {
                        ushort nofp = ushort.Parse(args[2]);
                        if (nofp != 0)
                        {
                            ushort[] invs = Util.TakeRange(args, 3, 3 + nofp).Select(p => ushort.Parse(p)).ToArray();
                            WI.Send("E0QU,0," + string.Join(",", Util.TakeRange(args, 3 + nofp, args.Length)), invs);
                            WI.Send("E0QU,1," + (args.Length - 3 - nofp), ExceptStaff(invs));
                            WI.Live("E0QU,1," + (args.Length - 3 - nofp));
                        }
                        else
                            WI.BCast("E0QU,0," + string.Join(",", Util.TakeRange(args, 3, args.Length)));
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
                        for (int idx = 1; idx < args.Length; )
                        {
                            ushort who = ushort.Parse(args[idx]);
                            int n = int.Parse(args[idx + 1]);
                            if (n > 0)
                            {
                                List<ushort> cards = Util.TakeRange(args, idx + 2, idx + 2 + n)
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
                        foreach (ushort ut in Board.Garden.Keys)
                            msgs.Add(ut, "");
                        msgs.Add(0, "");
                        for (int idx = 1; idx < args.Length; )
                        {
                            ushort who = ushort.Parse(args[idx]);
                            int n = int.Parse(args[idx + 1]);
                            List<ushort> cards = Util.TakeRange(args, idx + 2, idx + 2 + n)
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
                                    List<ushort> card = Util.TakeRange(args, idx + 3, idx + 3 + n)
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
                                List<ushort> card = Util.TakeRange(args, 5 + seesz, args.Length)
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
                                    ushort[] invs = Util.TakeRange(args, 5, 5 + seesz)
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
                                VI.Cout(0, "{0}摸取手牌{1}.", DisplayPlayer(me), DisplayTux(tuxs));
                                RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", tuxs));
                                WI.Send("E0HQ,2," + me + "," + string.Join(",", tuxs), 0, me);
                                WI.Send("E0HQ,3," + me + "," + tuxs.Length, ExceptStaff(me));
                                WI.Live("E0HQ,3," + me + "," + tuxs.Length);
                            }
                        }
                        else if (type == 3)
                        {
                            for (int idx = 3; idx < args.Length; )
                            {
                                ushort fromZone = ushort.Parse(args[idx]);
                                int n = int.Parse(args[idx + 1]);
                                ushort[] tuxes = Util.TakeRange(args, idx + 2, idx + 2 + n)
                                    .Select(p => ushort.Parse(p)).ToArray();
                                RaiseGMessage("G0IT," + me + "," + n + "," + string.Join(",", tuxes));
                                idx += (n + 2);
                            }
                            WI.BCast("E0HQ,4," + cmdrst.Substring("3,".Length));
                        }
                    }
                    break;
                case "G0QZ":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> cards = Util.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToList();
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

                        List<ushort> involved = new List<ushort>();
                        IDictionary<ushort, List<ushort>> gains = new Dictionary<ushort, List<ushort>>();
                        IDictionary<ushort, List<ushort>> loses = new Dictionary<ushort, List<ushort>>();
                        //List<string> losers = new List<string>();
                        for (int i = 1; i < args.Length; )
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
                case "G0IH":
                    {
                        string result = "";
                        List<Artiad.Cure> cures = Artiad.Cure.Parse(cmd);
                        foreach (Artiad.Cure cure in cures)
                        {
                            Player py = Board.Garden[cure.Who];
                            if (py.IsValidPlayer())
                            {
                                if (py.HP + cure.N >= py.HPb)
                                    cure.N = (py.HPb - py.HP);
                                py.HP += cure.N;
                                if (cure.N > 0)
                                {
                                    result += "," + cure.Who + "," + Artiad.IntHelper.Elem2Int(cure.Element)
                                        + "," + cure.N + "," + py.HP;
                                }
                            }
                        }
                        if (!result.Equals(""))
                            WI.BCast("E0IH" + result);
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
                            foreach (string spos in hero.Spouses)
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
                                                Base.Card.Hero hro = LibTuple.HL.InstanceHero(py.SelectHero);
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
                                    // !3:TR-Lingyin, !4:TR-Xuanji
                                    else if (spo == 3 || spo == 4)
                                    {
                                        Func<Player, bool> genJudge;
                                        if (spo == 3)
                                            genJudge = p => LibTuple.HL
                                                .InstanceHero(p.SelectHero).Bio.Contains("A");
                                        else if (spo == 4)
                                            genJudge = p => LibTuple.HL
                                                .InstanceHero(p.SelectHero).Bio.Contains("B");
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
                                }
                            }
                            if (candidates.Count > 0)
                                gains.Add(player.Uid, candidates);
                        }
                        if (gains.Count > 0)
                            RaiseGMessage("G0LV," + string.Join(",", gains.Select(p => p.Key + "," +
                                 p.Value.Count() + "," + string.Join(",", p.Value))));
                        if (gains.Count > 0)
                            RaiseGMessage(Artiad.Cure.ToMessage(gains.Select(p =>
                                new Artiad.Cure(p.Key, 0, FiveElement.LOVE, p.Value.Count()))));
                        if (loses.Count > 0)
                            RaiseGMessage(Artiad.Harm.ToMessage(loses.Select(p =>
                                new Artiad.Harm(p.Key, 0, FiveElement.LOVE, p.Value, 0))));

                        // if HP is still 0, then marked as death
                        zeros = Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).ToList();
                        if (zeros.Count > 0)
                            RaiseGMessage("G0ZW," + string.Join(",", zeros.Select(p => p.Uid)));
                    }
                    break;
                case "G0LV":
                    WI.BCast("E0LV," + cmdrst);
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
                            player.InitFromHero(hero, true, Board.InFightThrough, Board.InFight);
                        }
                        else
                            player.InitFromHero(hero, false, Board.InFightThrough, Board.InFight);
                        if (changeType == 2)
                        {
                            player.HP = int.Parse(args[4]);
                            if (player.HP > player.HPb)
                                player.HP = player.HPb;
                        }
                        RaiseGMessage("G2AK," + player.Uid + ","
                            + player.HP + "," + player.HPb + "," + player.STR + "," + player.DEX);
                        // remove all cosses containing the player
                        foreach (Player py in Board.Garden.Values)
                            if (py.IsAlive && py.Coss.Peek() == heroNum)
                                RaiseGMessage("G0OV," + player.Uid + "," + heroNum);
                        if (changeType == 0 || changeType == 2)
                        {
                            if (hero.Skills.Count > 0)
                                RaiseGMessage("G0IS," + player.Uid + ",0," + string.Join(",", hero.Skills));
                        }
                        WI.BCast("E0IY," + cmdrst);
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
                            foreach (ushort ut in player.Pets)
                                if (ut != 0 && !Board.NotActionPets.Contains(ut))
                                {
                                    if (changeType == 1)
                                        RaiseGMessage("G0IC,1," + player.Uid + "," + ut);
                                    else
                                        RaiseGMessage("G0IC,0," + player.Uid + "," + ut);
                                }
                        }
                        if (Board.IsAttendWar(player) && Board.InFight)
                            RaiseGMessage("G09P,0");
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
                        for (int idx = 1; idx < args.Length; )
                        {
                            ushort who = ushort.Parse(args[idx]);
                            bool drIn = (args[idx + 1] == "0");
                            int n = int.Parse(args[idx + 2]);
                            if (!drIn)
                            {
                                string[] cards = Util.TakeRange(args, idx + 4, idx + 4 + n);
                                g0on += "," + who + ",C," + n + "," + string.Join(",", cards);
                            }
                            idx += (4 + n);
                        }
                        if (g0on.Length > 0)
                            RaiseGMessage("G0ON" + g0on);
                    }
                    break;
                case "G1IU":
                    Board.PZone.AddRange(Util.TakeRange(args, 1, args.Length).Select(p => ushort.Parse(p)));
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
                        int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Util.Substring(cmd, idx + 1, -1);

                        string[] argv = cmd.Substring(0, hdx).Split(',');
                        ushort ust = ushort.Parse(argv[1]);
                        Base.Card.Tux tux = tx01[argv[3]];
                        WI.BCast("E0CD," + argv[1] + "," + argv[2] + "," + argv[3]);
                        //if (!tux.IsEq[sktInType])
                            //string input = "";
                            //while (true)
                            //{
                            //    string ipt = tux.Input(Board.Garden[ust], sktInType, sktFuse, input);
                            //    if (ipt != null && ipt != "")
                            //        input += (input == "" ? "" : ",") + AsyncInput(ust, ipt, argv[2], "0");
                            //    else break;
                            //}
                            //if (input.Length > 0)
                            //    argv[3] += "," + input;
                        if ((tux.IsEq[sktInType] & 1) == 0)
                            RaiseGMessage("G0CE," + argv[1] + "," + argv[2] + ",0," + argv[3] +
                                ";" + sktInType + "," + sktFuse);
                        else
                            RaiseGMessage("G0CE," + argv[1] + "," + argv[2] + ",1," + argv[3] +
                                "," + argv[4] + ";" + sktInType + "," + sktFuse);
                        break;
                    }
                case "G0CE": // use card and take action
                    {
                        // G0CE,A,0,JP04,3,1
                        int hdx = cmd.IndexOf(';');
                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Util.Substring(cmd, idx + 1, -1);
                        string[] argv = cmd.Substring(0, hdx).Split(',');

                        ushort ust = ushort.Parse(argv[1]);
                        ushort uaction = ushort.Parse(argv[2]);
                        ushort notEq = ushort.Parse(argv[3]);
                        if (notEq == 0)
                        {
                            Base.Card.Tux tux = tx01[argv[4]];
                            string argvt = Util.Substring(cmd, argv[0].Length + argv[1].Length +
                                argv[2].Length + argv[3].Length + argv[4].Length + 5, hdx);
                            if (argvt.Length > 0)
                                argvt = "," + argvt;
                            WI.BCast("E0CE," + ust + "," + uaction + "," + argv[4] + argvt);
                            if (uaction != 2)
                                tux.Action(Board.Garden[ust], sktInType, sktFuse, argvt);
                        }
                        else
                        {
                            ushort ut = ushort.Parse(argv[5]);
                            if (uaction != 2)
                                RaiseGMessage("G0ZB," + Board.Garden[ust].Uid + ",3," + ut + "," + argv[4]);
                        }
                        break;
                    }
                case "G0XZ":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort dicesType = ushort.Parse(args[2]);
                        if (dicesType == 0)
                        {
                            ushort who = ushort.Parse(args[3]);
                            var gps = Board.Garden[who].Tux;
                            WI.Send("E0XZ," + me + ",5," + args[3] + "," + Util.SatoString(gps), 0, me);
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
                                //WI.Send("E0XZ," + me + ",0," + args[3] + "," + Util.SatoString(gps), 0, me);
                                WI.Send("E0XZ," + me + ",1," + dicesType + "," + count, ExceptStaff(me));
                                WI.Live("E0XZ," + me + ",1," + dicesType + "," + count);
                                int pick = args.Length > 5 ? int.Parse(args[5]) : count;
                                string order = AsyncInput(me, "X" + pick + "(p" + Util.Sato(gps, "p") + ")", "G0XZ", "0");
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
                                        WI.Send("E0XZ," + me + ",3," + dicesType + ","
                                            + Util.SatoString(result), ExceptStaff(me));
                                        WI.Live("E0XZ," + me + ",3," + dicesType + ","
                                             + Util.SatoString(result));
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
                    if (args[2] == "0")
                    {
                        // G0ZB,A,0,x...
                        ushort me = ushort.Parse(args[1]);
                        for (int i = 3; i < args.Length; ++i)
                        {
                            ushort card = ushort.Parse(args[i]);
                            Player player = Board.Garden[me];
                            Tux tux = LibTuple.TL.DecodeTux(card);
                            if (player.Tux.Contains(card) && tux.IsTuxEqiup())
                            {
                                TuxEqiup te = tux as TuxEqiup;
                                RaiseGMessage("G0OT," + me + ",1," + card);
                                if (tux.Type == Tux.TuxType.WQ)
                                {
                                    if (player.Weapon == 0 && player.ExEquip == 0)
                                    {
                                        player.Weapon = card;
                                        WI.BCast("E0ZB," + me + ",0,1," + card);
                                    }
                                    else if ((player.ExMask & 0x1) == 0)
                                    {
                                        if (player.Weapon != card && player.Weapon != 0)
                                            RaiseGMessage("G0QZ," + me + "," + player.Weapon);
                                        player.Weapon = card;
                                        WI.BCast("E0ZB," + me + ",0,1," + card);
                                    }
                                    else
                                    {
                                        string mai = "#替换的,C1(p" + player.Weapon
                                             + "p" + player.ExEquip + ")";
                                        ushort sel = ushort.Parse(AsyncInput(me, mai, cmd, "0"));
                                        if (player.ExEquip == sel) {
                                            if (player.ExEquip != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.ExEquip);
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",0,5," + card);
                                        } else { // player.Weapon == sel
                                            if (player.Weapon != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.Weapon);
                                            player.Weapon = card;
                                            WI.BCast("E0ZB," + me + ",0,1," + card);
                                        }
                                    }
                                    RaiseGMessage("G1IZ," + me + "," + card);
                                    if (!player.WeaponDisabled)
                                        te.InsAction(player);
                                }
                                else if (tux.Type == Tux.TuxType.FJ)
                                {
                                    if (player.Armor == 0 && player.ExEquip == 0) {
                                        player.Armor = card;
                                        WI.BCast("E0ZB," + me + ",0,2," + card);
                                    }
                                    else if ((player.ExMask & 0x2) == 0)
                                    {
                                        if (player.Armor != card && player.Armor != 0)
                                            RaiseGMessage("G0QZ," + me + "," + player.Armor);
                                        player.Armor = card;
                                        WI.BCast("E0ZB," + me + ",0,2," + card);
                                    }
                                    else
                                    {
                                        string mai = "#替换的,C1(p" + player.Armor
                                             + "p" + player.ExEquip + ")";
                                        ushort sel = ushort.Parse(AsyncInput(me, mai, cmd, "0"));
                                        if (player.ExEquip == sel) {
                                            if (player.ExEquip != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.ExEquip);
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",0,5," + card);
                                        } else { // player.Armor == sel
                                            if (player.Armor != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.Armor);
                                            player.Armor = card;
                                            WI.BCast("E0ZB," + me + ",0,2," + card);
                                        }
                                    }
                                    RaiseGMessage("G1IZ," + me + "," + card);
                                    if (!player.ArmorDisabled)
                                        te.InsAction(player);
                                }
                                if (tux.Type == Tux.TuxType.XB)
                                {
                                    if (player.Trove == 0 && player.ExEquip == 0)
                                    {
                                        player.Trove = card;
                                        WI.BCast("E0ZB," + me + ",0,6," + card);
                                    }
                                    else if ((player.ExMask & 0x4) == 0)
                                    {
                                        if (player.Trove != card && player.Trove != 0)
                                            RaiseGMessage("G0QZ," + me + "," + player.Trove);
                                        player.Trove = card;
                                        WI.BCast("E0ZB," + me + ",0,6," + card);
                                    } else
                                    {
                                        string mai = "#替换的,C1(p" + player.Trove
                                             + "p" + player.ExEquip + ")";
                                        ushort sel = ushort.Parse(AsyncInput(me, mai, cmd, "0"));
                                        if (player.ExEquip == sel) {
                                            if (player.ExEquip != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.ExEquip);
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",0,5," + card);
                                        } else { // player.Trove == sel
                                            if (player.Trove != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.Trove);
                                            player.Trove = card;
                                            WI.BCast("E0ZB," + me + ",0,6," + card);
                                        }
                                    }
                                    RaiseGMessage("G1IZ," + me + "," + card);
                                    if (!player.LuggageDisabled)
                                        te.InsAction(player);
                                }
                            }
                        }
                    }
                    else if (args[2] == "1") // G0ZB,to,1,master,0:normal;1:force-fill,from,[tux]*
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort master = ushort.Parse(args[3]);
                        bool fillForce = args[4] == "1";
                        ushort from = ushort.Parse(args[5]);
                        for (int i = 6; i < args.Length; ++i)
                        {
                            ushort card = ushort.Parse(args[i]);
                            Tux tux = LibTuple.TL.DecodeTux(card);

                            bool cardCheck = (from == 0 || Board.Garden[from].ListOutAllCards().Contains(card));
                            if (from == 0 && !tux.IsTuxEqiup())
                            {
                                Board.Garden[me].Fakeq[card] = tux.Code;
                                WI.BCast("E0ZB," + me + ",1," + from + ",4," + card + "," + tux.Code);
                            }
                            else if (from != 0 && !tux.IsTuxEqiup())
                            {
                                if (cardCheck)
                                {
                                    string ccode = Board.Garden[from].Fakeq[card];
                                    Board.Garden[me].Fakeq[card] = ccode;
                                    RaiseGMessage("G0OT," + from + ",1," + card);
                                    WI.BCast("E0ZB," + me + ",1," + from + ",4," + card + "," + ccode);
                                }
                            }
                            else if (cardCheck)
                            {
                                if (from != 0)
                                    RaiseGMessage("G0OT," + from + ",1," + card);
                                TuxEqiup te = tux as TuxEqiup;
                                Player player = Board.Garden[me];
                                if (tux.Type == Tux.TuxType.WQ)
                                {
                                    if (fillForce)
                                    {
                                        if (player.Weapon == 0)
                                        {
                                            player.Weapon = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",1," + card);
                                        }
                                        else if ((player.ExMask & 0x1) != 0 && player.ExEquip == 0)
                                        {
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",5," + card);
                                        }
                                    }
                                    else if (player.Weapon == 0 && player.ExEquip == 0)
                                    {
                                        player.Weapon = card;
                                        WI.BCast("E0ZB," + me + ",1," + from + ",1," + card);
                                    }
                                    else if ((player.ExMask & 0x1) == 0)
                                    {
                                        if (player.Weapon != card && player.Weapon != 0)
                                            RaiseGMessage("G0QZ," + me + "," + player.Weapon);
                                        player.Weapon = card;
                                        WI.BCast("E0ZB," + me + ",1," + from + ",1," + card);
                                    }
                                    else
                                    {
                                        string mai = "#替换的,C1(p" + player.Weapon
                                             + "p" + player.ExEquip + ")";
                                        ushort sel = ushort.Parse(AsyncInput(master, mai, cmd, "0"));
                                        if (player.ExEquip == sel)
                                        {
                                            if (player.ExEquip != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.ExEquip);
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",5," + card);
                                        }
                                        else
                                        { // player.Weapon == sel
                                            if (player.Weapon != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.Weapon);
                                            player.Weapon = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",1," + card);
                                        }
                                    }
                                    RaiseGMessage("G1IZ," + me + "," + card);
                                    if (!player.WeaponDisabled)
                                        te.InsAction(player);
                                }
                                else if (tux.Type == Tux.TuxType.FJ)
                                {
                                    if (fillForce)
                                    {
                                        if (player.Armor == 0)
                                        {
                                            player.Armor = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",2," + card);
                                        }
                                        else if ((player.ExMask & 0x2) != 0 && player.ExEquip == 0)
                                        {
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",5," + card);
                                        }
                                    }
                                    else if (player.Armor == 0 && player.ExEquip == 0)
                                    {
                                        player.Armor = card;
                                        WI.BCast("E0ZB," + me + ",1," + from + ",2," + card);
                                    }
                                    else if ((player.ExMask & 0x2) == 0)
                                    {
                                        if (player.Armor != card && player.Armor != 0)
                                            RaiseGMessage("G0QZ," + me + "," + player.Armor);
                                        player.Armor = card;
                                        WI.BCast("E0ZB," + me + ",1," + from + ",2," + card);
                                    }
                                    else
                                    {
                                        string mai = "#替换的,C1(p" + player.Armor
                                             + "p" + player.ExEquip + ")";
                                        ushort sel = ushort.Parse(AsyncInput(master, mai, cmd, "0"));
                                        if (player.ExEquip == sel)
                                        {
                                            if (player.ExEquip != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.ExEquip);
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",5," + card);
                                        }
                                        else
                                        { // player.Armor == sel
                                            if (player.Armor != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.Armor);
                                            player.Armor = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",2," + card);
                                        }
                                    }
                                    RaiseGMessage("G1IZ," + me + "," + card);
                                    if (!player.ArmorDisabled)
                                        te.InsAction(player);
                                }
                                else if (tux.Type == Tux.TuxType.XB)
                                {
                                    if (fillForce)
                                    {
                                        if (player.Trove == 0)
                                        {
                                            player.Trove = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",6," + card);
                                        }
                                        else if ((player.ExMask & 0x1) != 0 && player.ExEquip == 0)
                                        {
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",5," + card);
                                        }
                                    }
                                    else if (player.Trove == 0 && player.ExEquip == 0)
                                    {
                                        player.Trove = card;
                                        WI.BCast("E0ZB," + me + ",1," + from + ",6," + card);
                                    }
                                    else if ((player.ExMask & 0x4) == 0)
                                    {
                                        if (player.Trove != card && player.Trove != 0)
                                            RaiseGMessage("G0QZ," + me + "," + player.Trove);
                                        player.Trove = card;
                                        WI.BCast("E0ZB," + me + ",1," + from + ",6," + card);
                                    }
                                    else
                                    {
                                        string mai = "#替换的,C1(p" + player.Trove
                                             + "p" + player.ExEquip + ")";
                                        ushort sel = ushort.Parse(AsyncInput(master, mai, cmd, "0"));
                                        if (player.ExEquip == sel)
                                        {
                                            if (player.ExEquip != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.ExEquip);
                                            player.ExEquip = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",5," + card);
                                        }
                                        else
                                        { // player.Trove == sel
                                            if (player.Trove != 0)
                                                RaiseGMessage("G0QZ," + me + "," + player.Trove);
                                            player.Trove = card;
                                            WI.BCast("E0ZB," + me + ",1," + from + ",6," + card);
                                        }
                                    }
                                    RaiseGMessage("G1IZ," + me + "," + card);
                                    if (!player.LuggageDisabled)
                                        te.InsAction(player);
                                }
                            }
                        }
                    }
                    else if (args[2] == "2")
                    {
                        // G0ZB,A,2,from,x
                        ushort me = ushort.Parse(args[1]);
                        ushort from = ushort.Parse(args[3]);
                        for (int i = 4; i < args.Length; ++i)
                        {
                            ushort card = ushort.Parse(args[i]);
                            Player player = Board.Garden[me];
                            Tux tux = LibTuple.TL.DecodeTux(card);
                            if (from != 0 && Board.Garden[from].Tux.Contains(card))
                                RaiseGMessage("G0OT," + me + ",1," + card);
                            player.ExCards.Add(card);
                            WI.BCast("E0ZB," + me + ",0,3," + card);
                        }
                    }
                    else if (args[2] == "3")
                    {
                        // G0ZB,A,3,x
                        ushort me = ushort.Parse(args[1]);
                        ushort card = ushort.Parse(args[3]);
                        Player player = Board.Garden[me];
                        Tux tux = LibTuple.TL.DecodeTux(card);
                        foreach (string tuxInfo in Board.PendingTux)
                        {
                            string[] parts = tuxInfo.Split(',');
                            if (parts[0] == me.ToString() && parts[1] == "G0ZB")
                            {
                                if (parts[3] == card.ToString())
                                {
                                    string cardAs = parts[2];
                                    player.Fakeq[card] = cardAs;
                                    WI.BCast("E0ZB," + me + ",0,4," + card + "," + cardAs);
                                    Board.PendingTux.Remove(tuxInfo); break;
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

                        if (consumeType == 2)
                        {
                            //string sktFuse = Util.Substring(cmd, hdx + 1, -1);
                            //string rst = "R" + Board.Rounder.Uid + "ZD";
                            //if (sktFuse.StartsWith(rst))
                            if (Board.InFight)
                            {
                                RaiseGMessage("G1ZK,0," + me + "," + card);
                                RaiseGMessage("G2ZU,1," + me + "," + card);
                            }
                            else
                            {
                                RaiseGMessage("G2ZU,0," + me + "," + card);
                                RaiseGMessage("G0QZ," + me + "," + card);
                            }
                        }
                        else if (consumeType == 0 || consumeType == 1)
                        {
                            int jdx = mainParts[0].Length + mainParts[1].Length +
                                mainParts[2].Length + mainParts[3].Length + 4;

                            string argsv = Util.Substring(cmd, jdx, hdx);
                            string cargsv = argsv != "" ? "," + argsv : "";

                            int idx = cmd.IndexOf(',', hdx);
                            int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                            string sktFuse = Util.Substring(cmd, idx + 1, -1);

                            Player player = Board.Garden[me];
                            Tux tux = LibTuple.TL.DecodeTux(card);
                            if (tux.IsTuxEqiup())
                            {
                                TuxEqiup tue = (TuxEqiup)tux;
                                int equipType = 0;
                                if (tue.Type == Tux.TuxType.WQ)
                                    equipType = 1;
                                else if (tue.Type == Tux.TuxType.FJ)
                                    equipType = 2;
                                else if (tue.Type == Tux.TuxType.XB)
                                    equipType = 6;
                                if (equipType > 0)
                                {
                                    if (consumeType == 1)
                                    {
                                        //string rst = "R" + Board.Rounder.Uid + "ZD";
                                        //if (sktFuse.StartsWith(rst))
                                        if (Board.InFight)
                                        {
                                            RaiseGMessage("G1ZK,0," + me + "," + card);
                                            RaiseGMessage("G2ZU,1," + me + "," + card);
                                        }
                                        else
                                        {
                                            RaiseGMessage("G2ZU,0," + me + "," + card);
                                            RaiseGMessage("G0QZ," + me + "," + card);
                                        }
                                    }
                                    WI.BCast("E0ZC," + me + "," + consumeType + "," + equipType +
                                        "," + card + "," + sktInType + cargsv);
                                }
                                tue.ConsumeAction(player, consumeType, sktInType, sktFuse, argsv);
                            }
                        }
                        else if (consumeType == 3 || consumeType == 4)
                        {
                            ushort target = ushort.Parse(mainParts[4]);
                            int jdx = mainParts[0].Length + mainParts[1].Length +
                                mainParts[2].Length + mainParts[3].Length + mainParts[4].Length + 5;

                            string argsv = Util.Substring(cmd, jdx, hdx);
                            string cargsv = argsv != "" ? "," + argsv : "";

                            int idx = cmd.IndexOf(',', hdx);
                            int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                            string sktFuse = Util.Substring(cmd, idx + 1, -1);

                            Player player = Board.Garden[me];
                            Tux tux = LibTuple.TL.DecodeTux(card);
                            if (tux.IsTuxEqiup())
                            {
                                TuxEqiup tue = (TuxEqiup)tux;
                                int equipType = 0;
                                if (tue.Type == Tux.TuxType.WQ)
                                    equipType = 1;
                                else if (tue.Type == Tux.TuxType.FJ)
                                    equipType = 2;
                                else if (tue.Type == Tux.TuxType.XB)
                                    equipType = 6;
                                if (equipType > 0)
                                {
                                    if (consumeType == 4)
                                    {
                                        //string rst = "R" + Board.Rounder.Uid + "ZD";
                                        //if (sktFuse.StartsWith(rst))
                                        if (Board.InFight)
                                        {
                                            RaiseGMessage("G1ZK,0," + me + "," + card);
                                            RaiseGMessage("G2ZU,1," + me + "," + card);
                                        }
                                        else
                                        {
                                            RaiseGMessage("G2ZU,0," + me + "," + card);
                                            RaiseGMessage("G0QZ," + me + "," + card);
                                        }
                                    }
                                    WI.BCast("E0ZC," + me + "," + consumeType + "," + equipType +
                                        "," + card + "," + target + "," + sktInType + cargsv);
                                }
                                tue.ConsumeActionHolder(player, Board.Garden[target],
                                    consumeType - 3, sktInType, sktFuse, argsv);
                            }
                        }
                        break;
                    }
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
                            {
                                RaiseGMessage("G2ZU,0," + who + "," + ep);
                                RaiseGMessage("G0QZ," + who + "," + ep);
                            }
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

                        if (type == 0 || type == 1)
                        {
                            int n = int.Parse(args[3]);
                            if (type == 0)
                                player.STRb = player.mSTRb + n;
                            if (Board.InFightThrough && !Board.InFight) {
                                player.STRa = player.mSTRa + n;
                                WI.BCast("E0IA," + me + "," + type + "," + n + "," + player.STRa);
                            }
                            else if (Board.InFight) {
                                player.STRc = player.STRc + n;
                                WI.BCast("E0IA," + me + "," + type + "," + n + "," + player.STRa + "," + player.STRc);
                            } else
                                WI.BCast("E0IA," + me + "," + type + "," + n + "," + player.STRb);
                        }
                        else if (type == 2) // Suppress case
                        {
                            player.STRi = 1;
                            WI.BCast("E0IA," + me + ",2");
                        }
                        if (Board.InFight)
                            RaiseGMessage("G09P,1");
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

                        if (type == 0 || type == 1)
                        {
                            int n = int.Parse(args[3]);
                            if (type == 0)
                                player.STRb = player.mSTRb - n;
                            if (Board.InFightThrough && !Board.InFight) {
                                player.STRa = player.mSTRa - n;
                                WI.BCast("E0OA," + me + "," + type + "," + n + "," + player.STRa);
                            }
                            else if (Board.InFight) {
                                player.STRc = player.STRc - n;
                                WI.BCast("E0OA," + me + "," + type + "," + n + "," + player.STRa + "," + player.STRc);
                            } else
                                WI.BCast("E0OA," + me + "," + type + "," + n + "," + player.STRb);
                        }
                        else if (type == 2)
                        {
                            player.STRi = -1;
                            WI.BCast("E0OA," + me + ",2");
                        }
                        if (Board.InFight)
                            RaiseGMessage("G09P,1");
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
                            if (type == 0 || type == 1)
                            {
                                int n = int.Parse(args[3]);
                                if (type == 0)
                                    player.DEXb = player.mDEXb + n;
                                if (Board.InFightThrough && !Board.InFight) {
                                    player.DEXa = player.mDEXa + n;
                                    WI.BCast("E0IX," + me + "," + type + "," + n + "," + player.DEXa);
                                }
                                else if (Board.InFight) {
                                    player.DEXc = player.DEXc + n;
                                    WI.BCast("E0IX," + me + "," + type + "," + n + "," + player.DEXa + "," + player.DEXc);
                                } else
                                    WI.BCast("E0IX," + me + "," + type + "," + n + "," + player.DEXb);
                            }
                            else if (type == 2)
                            {
                                player.DEXi = 1;
                                WI.BCast("E0IX," + me + ",2");
                            } if (Board.InFight)
                                RaiseGMessage("G09P,0");
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
                            if (type == 0 || type == 1)
                            {
                                int n = int.Parse(args[3]);
                                if (type == 0)
                                    player.DEXb = player.mDEXb - n;
                                if (Board.InFightThrough && !Board.InFight) {
                                    player.DEXa = player.mDEXa - n;
                                    WI.BCast("E0OX," + me + "," + type + "," + n + "," + player.DEXa);
                                }
                                else if (Board.InFight) {
                                    player.DEXc = player.DEXc - n;
                                    WI.BCast("E0OX," + me + "," + type + "," + n + "," + player.DEXa + "," + player.DEXc);
                                } else
                                    WI.BCast("E0OX," + me + "," + type + "," + n + "," + player.DEXb);
                            }
                            else if (type == 2)
                            {
                                player.DEXi = 1;
                                WI.BCast("E0OX," + me + ",2");
                            } if (Board.InFight)
                                RaiseGMessage("G09P,0");
                        }
                        if (Board.InFight)
                            RaiseGMessage("G09P,0");
                        break;
                    }
                case "G0AX":
                    {
                        ushort me = ushort.Parse(args[1]);
                        Player player = Board.Garden[me];
                        if (player.IsAlive)
                        {
                            if (player.DEXc != player.DEXb || player.DEXa != player.DEXb
                                || player.STRc != player.STRb || player.STRa != player.STRb)
                            {
                                WI.BCast("E0AX," + me + "," + player.STRb + "," + player.DEXb);
                            }
                            player.SDaSet = player.SDcSet = false;
                            player.DEXi = 0; player.STRi = 0;
                            player.RestZP = 1;
                        } // JN50302 to override the G0AX events
                        break;
                    }
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
                            if (Board.InFight)
                                RaiseGMessage("G09P,1");
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
                            if (Board.InFight)
                                RaiseGMessage("G09P,1");
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
                        if (nmb.IsMonster())
                        {
                            Monster mon = (Monster)nmb;
                            mon.AGL += (ushort)n;
                            WI.BCast("E0IW," + x + "," + n + "," + mon.AGL);
                            if (Board.InFight)
                                RaiseGMessage("G09P,0");
                        }
                        break;
                    }
                case "G0OW":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int n = ushort.Parse(args[2]);
                        NMB nmb = NMBLib.Decode(x, LibTuple.ML, LibTuple.NL);
                        if (nmb.IsMonster())
                        {
                            Monster mon = (Monster)nmb;
                            mon.AGL -= (ushort)n;
                            WI.BCast("E0OW," + x + "," + n + "," + mon.AGL);
                            if (Board.InFight)
                                RaiseGMessage("G09P,0");
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
                            if (mon.STR != mon.STRb)
                            {
                                mon.STR = mon.STRb;
                                change |= true;
                                RaiseGMessage("G2WK," + string.Join(",",
                                    CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                            }
                            if (mon.AGL != mon.AGLb)
                            {
                                mon.AGL = mon.AGLb;
                                change |= true;
                            }
                            if (change)
                                WI.BCast("E0WB," + x + "," + mon.STR + "," + mon.AGL);
                        }
                        else if (nmb.IsNPC())
                        {
                            NPC npc = (NPC)nmb;
                            npc.STR = npc.STRb;
                            WI.BCast("E0WB," + x + "," + npc.STR);
                        }
                    }
                    break;
                case "G09P":
                    if (args[1] == "0")
                    {
                        Board.HinderSucc = Board.Hinder.DEXi > 0 ||
                            (Board.Hinder.DEXi == 0 && (Board.Hinder.DEX >= Board.Battler.AGL));
                        Board.SupportSucc = Board.Supporter.DEXi > 0 ||
                            (Board.Supporter.DEXi == 0 && (Board.Supporter.DEX >= Board.Battler.AGL));
                        WI.BCast("E09P,0," + Board.Supporter.Uid + (Board.SupportSucc ? ",1," : ",0,")
                                + Board.Hinder.Uid + (Board.HinderSucc ? ",1" : ",0"));
                    } if (args[1] == "0" || args[1] == "1")
                        WI.BCast("E09P,1," + Board.Rounder.Team + "," + Board.CalculateRPool()
                                + "," + Board.Rounder.OppTeam + "," + Board.CalculateOPool());
                    break;
                case "G0IP":
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
                        if (Board.InFight)
                            RaiseGMessage("G09P,1");
                        break;
                    }
                case "G0OP":
                    {
                        ushort side = ushort.Parse(args[1]);
                        int delta = int.Parse(args[2]);
                        if (side == Board.Rounder.Team)
                        {
                            if (delta > Board.RPool)
                                delta = Board.RPool;
                            Board.RPool -= delta;
                            WI.BCast("E0OP," + side + "," + delta);
                        }
                        else if (side == Board.Rounder.OppTeam)
                        {
                            if (delta > Board.OPool)
                                delta = Board.OPool;
                            Board.OPool -= delta;
                            WI.BCast("E0OP," + side + "," + delta);
                        }
                        if (Board.InFight)
                            RaiseGMessage("G09P,1");
                        break;
                    }
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
                    WI.BCast("E0HC," + cmdrst);
                    if (args[1] == "0")
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort from = ushort.Parse(args[3]);
                        Player player = Board.Garden[who];
                        List<ushort>[] cpets = new List<ushort>[5];
                        for (int i = 0; i < 5; ++i)
                            cpets[i] = new List<ushort>();
                        for (int i = 0; i < 5; ++i)
                        {
                            if (player.Pets[i] != 0)
                                cpets[i].Add(player.Pets[i]);
                        }
                        for (int i = 4; i < args.Length; ++i)
                        {
                            ushort mons = ushort.Parse(args[i]);
                            Monster pet = LibTuple.ML.Decode(mons);
                            int pe = Util.GetFiveElementId(pet.Element);
                            cpets[pe].Add(mons);
                            if (from != 0)
                                RaiseGMessage("G0HL," + from + "," + mons);
                        }
                        for (int i = 0; i < 5; ++i)
                        {
                            if (cpets[i].Count == 1 && player.Pets[i] == 0)
                                RaiseGMessage("G0HD,0," + who + "," + from + "," + cpets[i].First());
                            else if (cpets[i].Count > 1)
                            {
                                string mai = "#保留的,M1(p" + Util.Sato(cpets[i], "p") + ")";
                                ushort sel = ushort.Parse(AsyncInput(who, mai, cmd, "0"));
                                ushort old = player.Pets[i];
                                if (old != 0 && sel != old)
                                {
                                    RaiseGMessage("G0HL," + who + "," + old);
                                    RaiseGMessage("G0ON," + who + ",M,1," + old);
                                }
                                if (sel != old)
                                    RaiseGMessage("G0HD,0," + who + "," + from + "," + sel);
                                foreach (ushort ut in cpets[i])
                                {
                                    if (ut != old && ut != sel)
                                        RaiseGMessage("G0ON," + who + ",M,1," + ut);
                                }
                            }
                        }
                    }
                    else if (args[1] == "1")
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort from = ushort.Parse(args[3]);
                        ushort kokan = ushort.Parse(args[4]);
                        ushort which = ushort.Parse(args[5]);

                        Monster pet = LibTuple.ML.Decode(which);
                        int pe = Util.GetFiveElementId(pet.Element);
                        Player player = Board.Garden[who];
                        if (player.Pets[pe] == 0)
                        {
                            if (from != 0)
                                RaiseGMessage("G0HL," + from + "," + which);
                            RaiseGMessage("G0HD,1," + who + "," + from + "," + which);
                        }
                        else
                        {
                            ushort pt = player.Pets[pe];
                            if (kokan == 0) // Switch
                            {
                                if (from != 0)
                                {
                                    RaiseGMessage("G0HL," + from + "," + which);
                                    RaiseGMessage("G0HL," + who + "," + pt);
                                    RaiseGMessage("G0HD,1," + who + "," + from + "," + which);
                                    RaiseGMessage("G0HD,1," + from + "," + who + "," + pt);
                                }
                                else
                                {
                                    RaiseGMessage("G0HL," + who + "," + pt);
                                    RaiseGMessage("G0HD,1," + who + ",0," + which);
                                }
                            }
                            else if (kokan == 1) // Positive-Choose
                            {
                                string choose = AsyncInput(who, "#保留,M1(p" + pt + "p" + which + ")", "G0HC,1", "1");
                                ushort left = ushort.Parse(choose);
                                if (from != 0)
                                    RaiseGMessage("G0HL," + from + "," + which);
                                if (left == which)
                                {
                                    RaiseGMessage("G0HL," + who + "," + pt);
                                    RaiseGMessage("G0HD,1," + who + "," + from + "," + which);
                                }
                            }
                            else if (kokan == 2) // Negative-Choose
                            {
                                if (from != 0)
                                    RaiseGMessage("G0HL," + from + "," + which);
                                RaiseGMessage("G0HL," + who + "," + pt);
                                RaiseGMessage("G0HD,1," + who + "," + from + "," + which);
                            }
                        }
                    }
                    else if (args[1] == "2")
                    {
                        ushort t1 = ushort.Parse(args[2]), t2 = ushort.Parse(args[3]);
                        Player py1 = Board.Garden[t1], py2 = Board.Garden[t2];
                        List<ushort> pt1s = py1.Pets.Where(p => p != 0).ToList();
                        List<ushort> pt2s = py2.Pets.Where(p => p != 0).ToList();
                        foreach (ushort ut in pt1s)
                            RaiseGMessage("G0HL," + t1 + "," + ut);
                        foreach (ushort ut in pt2s)
                            RaiseGMessage("G0HL," + t2 + "," + ut);
                        foreach (ushort ut in pt1s)
                            RaiseGMessage("G0HD,1," + t2 + "," + t1 + "," + ut);
                        foreach (ushort ut in pt2s)
                            RaiseGMessage("G0HD,1," + t1 + "," + t2 + "," + ut);
                    }
                    break;
                case "G0HD":
                    {
                        ushort type = ushort.Parse(args[1]);
                        ushort who = ushort.Parse(args[2]);
                        ushort from = ushort.Parse(args[3]);
                        ushort which = ushort.Parse(args[4]);
                        Monster mon = LibTuple.ML.Decode(which);
                        int pe = Util.GetFiveElementId(mon.Element);
                        Player player = Board.Garden[who];
                        player.Pets[pe] = which;
                        RaiseGMessage("G0WB," + which);
                        if (!player.PetDisabled && !Board.NotActionPets.Contains(which))
                            RaiseGMessage("G0IC,0," + who + "," + which);
                        WI.BCast("E0HD," + who + "," + from + "," + which);
                        RaiseGMessage("G2WK," + string.Join(",",
                                CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                        break;
                    }
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

                        string argsv = Util.Substring(cmd, jdx, hdx);
                        string cargsv = argsv != "" ? "," + argsv : "";

                        int idx = cmd.IndexOf(',', hdx);
                        int sktInType = int.Parse(Util.Substring(cmd, hdx + 1, idx));
                        string sktFuse = Util.Substring(cmd, idx + 1, -1);

                        Player player = Board.Garden[me];
                        Monster monster = LibTuple.ML.Decode(mons);
                        int pe = Util.GetFiveElementId(monster.Element);
                        if (player.Pets[pe] == mons)
                        {
                            if (consumeType == 1)
                            {
                                //string rst = "R" + Board.Rounder.Uid + "ZD";
                                //if (sktFuse.StartsWith(rst))
                                if (Board.InFight)
                                {
                                    RaiseGMessage("G1HK,0," + me + "," + mons);
                                    RaiseGMessage("G2HU," + me + "," + mons);
                                }
                                else
                                {
                                    RaiseGMessage("G0HL," + me + "," + mons);
                                    RaiseGMessage("G0ON," + me + ",M,1," + mons);
                                }
                            }
                            // TODO: discard pets after fight finished
                            WI.BCast("E0HH," + me + "," + consumeType + "," + mons + "," + sktInType + cargsv);
                            monster.ConsumeAction(player, consumeType, sktInType, sktFuse, argsv);
                        }
                        else if (consumeType == 2)
                        {
                            WI.BCast("E0HH," + me + "," + consumeType + "," + mons + "," + sktInType + cargsv);
                            monster.ConsumeAction(player, consumeType, sktInType, sktFuse, argsv);
                        }
                        break;
                    }
                case "G0HI":
                    {
                        // to mark as to be discard
                        IDictionary<ushort, List<ushort>> imc = new Dictionary<ushort, List<ushort>>();
                        // to discard immediately
                        IDictionary<ushort, List<ushort>> jmc = new Dictionary<ushort, List<ushort>>();
                        List<ushort> dices = new List<ushort>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort pet = ushort.Parse(args[i + 1]);
                            if (Board.InFight)
                            {
                                RaiseGMessage("G1HK,0," + who + "," + pet);
                                Util.AddToMultiMap(imc, who, pet);
                            }
                            else
                            {
                                RaiseGMessage("G0HL," + who + "," + pet);
                                Util.AddToMultiMap(jmc, who, pet);
                            }
                        }
                        foreach (var pair in imc)
                            RaiseGMessage("G2HU," + pair.Key + "," + string.Join(",", pair.Value));
                        if (jmc.Count > 0)
                            RaiseGMessage("G0ON," + string.Join(",", jmc.Select(p => p.Key + ",M," +
                                 p.Value.Count + "," + string.Join(",", p.Value))));
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
                            {
                                RaiseGMessage("G0HL," + who + "," + mons);
                                RaiseGMessage("G0ON," + who + ",M,1," + mons);
                            }
                        }
                        Board.CsPets.Clear();
                    }
                    break;
                case "G0HL":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort which = ushort.Parse(args[2]);
                        Monster pet = LibTuple.ML.Decode(which);
                        int pe = Util.GetFiveElementId(pet.Element);
                        Player player = Board.Garden[who];
                        if (player.Pets[pe] == which)
                        {
                            if (!player.PetDisabled && !Board.NotActionPets.Contains(which))
                                RaiseGMessage("G0OC,0," + who + "," + which);
                            WI.BCast("E0HL," + who + "," + which);
                            Board.Garden[who].Pets[pe] = 0;
                            RaiseGMessage("G0WB," + which);
                            RaiseGMessage("G2WK," + string.Join(",",
                                CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                        }
                        break;
                    }
                case "G0IC":
                    {
                        for (int i = 1; i < args.Length; i += 3)
                        {
                            bool reset = (args[i] != "1");
                            ushort who = ushort.Parse(args[i + 1]);
                            ushort which = ushort.Parse(args[i + 2]);
                            Player player = Board.Garden[who];
                            Monster pet = LibTuple.ML.Decode(which);
                            if (reset)
                                pet.RAMUshort = 0;
                            pet.IncrAction(player);
                            WI.BCast("E0IC," + who + "," + which);
                        }
                        break;
                    }
                case "G0OC":
                    {
                        for (int i = 1; i < args.Length; i += 3)
                        {
                            bool reset = (args[i] != "1");
                            ushort who = ushort.Parse(args[i + 1]);
                            ushort which = ushort.Parse(args[i + 2]);
                            Player player = Board.Garden[who];
                            Monster pet = LibTuple.ML.Decode(which);
                            int pe = Util.GetFiveElementId(pet.Element);
                            if (player.Pets[pe] == which)
                            {
                                pet.DecrAction(player);
                                if (reset)
                                    pet.RAMUshort = 0;
                                WI.BCast("E0OC," + who + "," + which);
                            }
                        }
                        break;
                    }
                case "G0HT":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int n = int.Parse(args[2]);
                        RaiseGMessage("G0DH," + who + ",0," + n);
                        break;
                    }
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
                        string order = AsyncInput(who, "//", "G0TT", "0");
                        int number = randomSeed.Next(6);
                        if (number == 0) number = 6;
                        WI.BCast("E0TT," + who + "," + number);
                        Board.DiceValue = number;
                    }
                    break;
                case "G0JM":
                    {
                        WI.RecvInfTermin();
                        // Reset board information
                        Board.UseCardRound = 0;
                        string stage = cmdrst;
                        WI.BCast("F0JM," + stage);
                        // count how many players have received the F0JM message
                        int count = Board.Garden.Keys.Count;
                        WI.RecvInfStart();
                        while (count > 0)
                        {
                            Base.VW.Msgs msg = WI.RecvInfRecvPending();
                            if (msg.Msg.StartsWith("F0JM"))
                                --count;
                            else
                                WI.Send("F0JM," + stage, 0, msg.From);
                        }
                        WI.RecvInfEnd();
                        WI.Live("F0JM," + stage);
                        lock (jumpTareget)
                        {
                            jumpTareget = stage;
                            jumpEnd = "H0TM";
                        }
                        System.Threading.Thread.CurrentThread.Abort();
                    }
                    break;
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
                            List<string> heros = Util.TakeRange(args, 4, 4 + n).ToList();
                            py.TokenExcl.AddRange(heros);
                            WI.BCast("E0IJ," + who + ",1," + n + "," + string.Join(",", heros) +
                                "," + py.TokenExcl.Count + "," + string.Join(",", py.TokenExcl));
                        }
                        else if (type == 2)
                        {
                            int n = int.Parse(args[3]);
                            List<ushort> tars = Util.TakeRange(args, 4, 4 + n)
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
                            List<ushort> folders = Util.TakeRange(args, 4, 4 + n)
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
                            List<string> heros = Util.TakeRange(args, 4, 4 + n).ToList();
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
                            List<ushort> tars = Util.TakeRange(args, 4, 4 + n)
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
                            List<ushort> folders = Util.TakeRange(args, 4, 4 + n)
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
                    {
                        string ioc = "", e0ie = "";
                        if (args[1] == "0")
                        {
                            for (int i = 2; i < args.Length; ++i)
                            {
                                ushort who = ushort.Parse(args[i]);
                                Player player = Board.Garden[who];
                                if (player.PetDisabled)
                                {
                                    foreach (ushort pt in player.Pets)
                                        if (pt != 0 && !Board.NotActionPets.Contains(pt))
                                            ioc += ",1," + who + "," + pt;
                                    player.PetDisabled = false;
                                    e0ie += "," + who;
                                }
                            }
                            if (ioc != "")
                                RaiseGMessage("G0IC" + ioc);
                            if (e0ie != "")
                                WI.BCast("E0IE,0" + e0ie);
                        }
                        else if (args[1] == "1")
                        {
                            for (int i = 2; i < args.Length; ++i)
                            {
                                ushort pt = ushort.Parse(args[i]);
                                if (Board.NotActionPets.Contains(pt))
                                {
                                    Board.NotActionPets.Remove(pt);
                                    int elem = Util.GetFiveElementId(LibTuple.ML.Decode(pt).Element);
                                    foreach (Player py in Board.Garden.Values)
                                    {
                                        if (py.Pets[elem] == pt && !py.PetDisabled)
                                            ioc += ",1," + py.Uid + "," + pt;
                                    }
                                    e0ie += "," + pt;
                                }
                            }
                            if (ioc != "")
                                RaiseGMessage("G0IC" + ioc);
                            if (e0ie != "")
                                WI.BCast("E0IE,1" + e0ie);
                        }
                    }
                    break;
                case "G0OE":
                    {
                        string ioc = "", e0oe = "";
                        if (args[1] == "0")
                        {
                            for (int i = 2; i < args.Length; ++i)
                            {
                                ushort who = ushort.Parse(args[i]);
                                Player player = Board.Garden[who];
                                if (!player.PetDisabled)
                                {
                                    foreach (ushort pt in player.Pets)
                                        if (pt != 0 && !Board.NotActionPets.Contains(pt))
                                            ioc += ",1," + who + "," + pt;
                                    player.PetDisabled = true;
                                    e0oe += "," + who;
                                }
                            }
                            if (ioc != "")
                                RaiseGMessage("G0OC" + ioc);
                            if (e0oe != "")
                                WI.BCast("E0OE,0" + e0oe);
                        }
                        else if (args[1] == "1")
                        {
                            for (int i = 2; i < args.Length; ++i)
                            {
                                ushort pt = ushort.Parse(args[i]);
                                if (!Board.NotActionPets.Contains(pt))
                                {
                                    Board.NotActionPets.Add(pt);
                                    int elem = Util.GetFiveElementId(LibTuple.ML.Decode(pt).Element);
                                    foreach (Player py in Board.Garden.Values)
                                    {
                                        if (py.Pets[elem] == pt && !py.PetDisabled)
                                            ioc += ",1," + py.Uid + "," + pt;
                                    }
                                    e0oe += "," + pt;
                                }
                            }
                            if (ioc != "")
                                RaiseGMessage("G0OC" + ioc);
                            if (e0oe != "")
                                WI.BCast("E0IE,1" + e0oe);
                        }
                    }
                    break;
                case "G0IS":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort op = ushort.Parse(args[2]);
                        bool hind = (op & 1) == 0;
                        bool roolBack = (op & 2) == 0;
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
                case "G0LH":
                    {
                        ISet<Player> fullBye = new HashSet<Player>();
                        for (int i = 1; i < args.Length; i += 3)
                        {
                            ushort incr = ushort.Parse(args[i]);
                            ushort ut = ushort.Parse(args[i + 1]);
                            ushort to = ushort.Parse(args[i + 2]);
                            if (incr == 0 || incr == 1)
                            {
                                if (to <= 0) { to = 0; args[i + 2] = "0"; }
                                Board.Garden[ut].HPb = to;
                                if (Board.Garden[ut].HP > Board.Garden[ut].HPb)
                                    Board.Garden[ut].HP = Board.Garden[ut].HPb;
                                if (Board.Garden[ut].HPb == 0)
                                    fullBye.Add(Board.Garden[ut]);
                            }
                        }
                        WI.BCast("E0LH," + string.Join(",", Util.TakeRange(args, 1, args.Length)));
                        if (fullBye.Count > 0)
                            RaiseGMessage("G0ZW," + string.Join(",", fullBye.Select(p => p.Uid)));
                    }
                    break;
                case "G0IV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        int hero = int.Parse(args[2]);
                        Player py = Board.Garden[ut];
                        py.Coss.Push(hero);
                        WI.BCast("E0IV," + ut + "," + hero);
                        Hero hro = LibTuple.HL.InstanceHero(hero);
                        if (hro != null)
                        {
                            RaiseGMessage("G2TZ," + ut + ",0,H" + hero);
                            List<string> skills = new List<string>();
                            foreach (string skstr in hro.Skills)
                            {
                                Skill skill = LibTuple.SL.EncodeSkill(skstr);
                                if (!skill.IsChange)
                                    skills.Add(skill.Code);
                            }
                            if (skills.Count > 0)
                                RaiseGMessage("G0IS," + ut + ",1," + string.Join(",", skills));
                        }
                    }
                    break;
                case "G0OV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        Player player = Board.Garden[ut];
                        int hero = player.Coss.Pop();
                        int next = player.Coss.Count > 0 ? player.Coss.Peek() : 0;
                        WI.BCast("E0OV," + ut + "," + hero + "," + next);

                        List<ushort> excds = new List<ushort>();
                        if (player.ExEquip != 0)
                            excds.Add(player.ExEquip);
                        excds.AddRange(player.ExCards);
                        if (excds.Count > 0)
                            RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", excds));
                        Artiad.ContentRule.ErasePlayerToken(player, Board, RaiseGMessage);

                        Hero hro = LibTuple.HL.InstanceHero(hero);
                        if (hro != null)
                        {
                            RaiseGMessage("G2TZ,0," + ut + ",H" + hero);
                            List<string> skills = new List<string>();
                            foreach (string skstr in hro.Skills)
                            {
                                Skill skill = LibTuple.SL.EncodeSkill(skstr);
                                if (!skill.IsChange)
                                    skills.Add(skill.Code);
                            }
                            if (skills.Count > 0)
                                RaiseGMessage("G0OS," + ut + ",1," + string.Join(",", skills));
                        }
                    }
                    break;
                case "G0PB":
                    {
                        IDictionary<ushort, string> dict = new Dictionary<ushort, string>();
                        foreach (ushort ut in Board.Garden.Keys)
                            dict[ut] = "";
                        string word0 = "";
                        for (int i = 2; i < args.Length; )
                        {
                            ushort who = ushort.Parse(args[i]);
                            int n = ushort.Parse(args[i + 1]);
                            List<ushort> cards = Util.TakeRange(args, i + 2, i + 2 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            if (args[1] == "0")
                            {
                                RaiseGMessage("G0OT," + who + "," + n + "," + string.Join(",", cards));
                                Board.TuxPiles.PushBack(cards);
                            } // TODO: Put Back Monster/NPC etc.
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
                    WI.BCast("E0YM" + cmd.Substring("E0YM".Length));
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
                        List<ushort> invs = Util.TakeRange(args, 4, args.Length)
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
                                    string allTux = string.Join(",", Board.Garden[ut].Tux);
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
                case "G0AF":
                    if (args[2] == "0") // Not show fight
                        WI.BCast("E0AF,0,0");
                    else if (args[1] == "0")
                        WI.BCast("E0AF," + args[2] + ",0");
                    else
                    {
                        IDictionary<ushort, int> selecto = new Dictionary<ushort, int>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            int delta = int.Parse(args[i + 1]);
                            selecto[who] = delta;
                        }
                        foreach (var pair in selecto)
                        {
                            if (pair.Value == 5 && Board.Supporter.Uid == pair.Key)
                                Board.Supporter = null;
                            else if (pair.Value == 6 && Board.Hinder.Uid == pair.Value)
                                Board.Hinder = null;
                            else
                            {
                                Player py;
                                if (pair.Key > 0 && pair.Key < 1000)
                                    py = Board.Garden[pair.Key];
                                else
                                {
                                    ushort mut = (ushort)(pair.Key - 1000);
                                    Base.Card.NMB nmb = Base.Card.NMBLib.Decode(mut, LibTuple.ML, LibTuple.NL);
                                    if (nmb != null)
                                        py = Board.Lumberjack(nmb, mut);
                                    else
                                        py = null;
                                }
                                if (pair.Value == 1)
                                    Board.Supporter = py;
                                else if (pair.Value == 2)
                                    Board.Hinder = py;
                            }
                        }
                        string e0af = string.Join(",", selecto.Select(
                            p => (p.Value > 4 ? 0 : p.Value) + "," + p.Key));
                        if (e0af.Length > 0)
                            WI.BCast("E0AF," + e0af);
                    }
                    break;
                case "G0ON":
                    for (int idx = 1; idx < args.Length; )
                    {
                        string fromZone = args[idx];
                        string cardType = args[idx + 1];
                        int cnt = int.Parse(args[idx + 2]);
                        if (cnt > 0)
                        {
                            List<ushort> cds = Util.TakeRange(args, idx + 3, idx + 3 + cnt)
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
                        ushort who = ushort.Parse(args[1]);
                        ushort lugUt = ushort.Parse(args[2]);
                        bool dirIn = args[3] == "0";
                        string[] cards = Util.TakeRange(args, 4, args.Length);
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
                case "G0PH":
                    {
                        string result = "";
                        List<Artiad.Toxi> toxis = Artiad.Toxi.Parse(cmd);
                        foreach (Artiad.Toxi toxi in toxis)
                        {
                            Player py = Board.Garden[toxi.Who];
                            if (toxi.N > 0)
                            {
                                if (py.HP - toxi.N <= 0)
                                    toxi.N = py.HP;
                                py.HP -= toxi.N;
                                if (toxi.N > 0)
                                    result += "," + toxi.Who + "," + Artiad.IntHelper.Elem2Int(
                                        toxi.Element) + "," + toxi.N + "," + py.HP;
                            }
                        }
                        if (!result.Equals(""))
                            WI.BCast("E0PH" + result);
                        List<ushort> zeros = Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0)
                            .Select(p => p.Uid).ToList();
                        if (zeros.Count > 0)
                        {
                            WI.BCast("E0ZH," + string.Join(",", zeros));
                            RaiseGMessage("G0ZH,0");
                        }
                    }
                    break;
                case "G0ZJ":
                    {
                        ushort ut = ushort.Parse(cmdrst);
                        Player py = Board.Garden[ut];
                        if ((py.ExMask & 0x1) != 0 && py.ExEquip != 0)
                        {
                            if (py.Weapon == 0)
                            {
                                py.Weapon = py.ExEquip; py.ExEquip = 0;
                                WI.BCast("E0ZJ," + ut + ",1," + py.ExEquip);
                            } else {
                                ushort choose = ushort.Parse(AsyncInput(ut, "#保留的,C1(p"
                                    + py.Weapon + "p" + py.ExEquip + ")", "G0ZJ", "0"));
                                if (choose == py.Weapon)
                                    RaiseGMessage("G0QZ," + ut + "," + py.ExEquip);
                                else
                                {
                                    RaiseGMessage("G0QZ," + ut + "," + py.Weapon);
                                    py.Weapon = py.ExEquip; py.ExEquip = 0;
                                    WI.BCast("E0ZJ," + ut + ",1," + py.ExEquip);
                                }
                            }
                            py.ExMask &= (~0x1);
                        }
                        else if ((py.ExMask & 0x2) != 0 && py.ExEquip != 0)
                        {
                            if (py.Armor == 0)
                            {
                                py.Armor = py.ExEquip; py.ExEquip = 0;
                                WI.BCast("E0ZJ," + ut + ",2," + py.ExEquip);
                            } else {
                                ushort choose = ushort.Parse(AsyncInput(ut, "#保留的,C1(p"
                                    + py.Armor + "p" + py.ExEquip + ")", "G0ZJ", "0"));
                                if (choose == py.Armor)
                                    RaiseGMessage("G0QZ," + ut + "," + py.ExEquip);
                                else
                                {
                                    RaiseGMessage("G0QZ," + ut + "," + py.Armor);
                                    py.Armor = py.ExEquip; py.ExEquip = 0;
                                    WI.BCast("E0ZJ," + ut + ",2," + py.ExEquip);
                                }
                            }
                            py.ExMask &= (~0x2);
                        }
                        else if ((py.ExMask & 0x4) != 0 && py.ExEquip != 0)
                        {
                            if (py.Trove == 0)
                            {
                                py.Trove = py.ExEquip; py.ExEquip = 0;
                                WI.BCast("E0ZJ," + ut + ",3," + py.ExEquip);
                            } else {
                                ushort choose = ushort.Parse(AsyncInput(ut, "#保留的,C1(p"
                                    + py.Trove + "p" + py.ExEquip + ")", "G0ZJ", "0"));
                                if (choose == py.Trove)
                                    RaiseGMessage("G0QZ," + ut + "," + py.ExEquip);
                                else
                                {
                                    RaiseGMessage("G0QZ," + ut + "," + py.Trove);
                                    py.Trove = py.ExEquip; py.ExEquip = 0;
                                    WI.BCast("E0ZJ," + ut + ",3," + py.ExEquip);
                                }
                            }
                            py.ExMask &= (~0x4);
                        }
                    }
                    break;
            }
        }
        #endregion G-Detail
    }
}
