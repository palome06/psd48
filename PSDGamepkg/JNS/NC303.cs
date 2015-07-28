﻿using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base;

namespace PSD.PSDGamepkg.JNS
{
    public class NPCCottage
    {
        private Base.VW.IVI VI { set; get; }
        //private VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public NPCCottage(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }

        public IDictionary<string, NCAction> RegisterDelegates(NCActionLib lib)
        {
            NPCCottage njc = this;
            IDictionary<string, NCAction> nj01 = new Dictionary<string, NCAction>();
            foreach (NCAction nj in lib.Firsts)
            {
                nj01.Add(nj.Code, nj);
                string njCode = nj.Code;
                var methodAction = njc.GetType().GetMethod(njCode + "Action");
                if (methodAction != null)
                    nj.Action += new NCAction.ActionDelegate(delegate(Player player, string fuse, string argst)
                    {
                        methodAction.Invoke(njc, new object[] { player, fuse, argst });
                    });
                var methodValid = njc.GetType().GetMethod(njCode + "Valid");
                if (methodValid != null)
                    nj.Valid += new NCAction.ValidDelegate(delegate(Player player, string fuse)
                    {
                        return (bool)methodValid.Invoke(njc, new object[] { player, fuse });
                    });
                var methodInput = njc.GetType().GetMethod(njCode + "Input");
                if (methodInput != null)
                    nj.Input += new NCAction.InputDelegate(delegate(Player player, string fuse, string prev)
                    {
                        return (string)methodInput.Invoke(njc, new object[] { player, fuse, prev });
                    });
            }
            return nj01;
        }
        public IDictionary<string, NPC> RegisterNPCDelegates(NPCLib lib)
        {
            NPCCottage njc = this;
            IDictionary<string, NPC> nj02 = new Dictionary<string, NPC>();
            foreach (NPC npc in lib.First)
            {
                nj02.Add(npc.Code, npc);
                string njCode = npc.Code;
                var methodDebut = njc.GetType().GetMethod(njCode + "Debut");
                if (methodDebut != null)
                    npc.Debut += new NPC.DebutDelegate(delegate(Player player)
                    {
                        methodDebut.Invoke(njc, new object[] { player });
                    });
            }
            return nj02;
        }

        public void NJ01Action(Player player, string fuse, string args)
        {
            int idx = args.IndexOf(',');
            ushort who = ushort.Parse(args.Substring(0, idx));
            Player wp = XI.Board.Garden[who];
            ushort to = ushort.Parse(args.Substring(idx + 1));
            Player tp = XI.Board.Garden[to];

            string npcCode = fuse.Substring(0, fuse.IndexOf(';'));
            fuse = fuse.Substring(fuse.IndexOf(';') + 1);

            NPC npc = XI.LibTuple.NL.Decode(XI.LibTuple.NL.Encode(npcCode));
            int tuxCount = wp.Tux.Count;
            XI.RaiseGMessage("G0DH," + who + ",2," + tuxCount);
            if (tp.SelectHero != 0)
                XI.RaiseGMessage("G0OY,0," + to);
            
            int hp = 2 * tuxCount;
            if (fuse != "R" + XI.Board.Rounder.Uid + "NP" && hp > 3)
                hp = 3;
            XI.RaiseGMessage("G0IY,2," + to + "," + npc.Hero + "," + hp);
        }
        public string NJ01Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "#弃掉所有手牌的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Team == player.Team && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                return "#待加入的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.Team == player.Team).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        public bool NJ01Valid(Player player, string fuse)
        {
            string npcCode = fuse.Substring(0, fuse.IndexOf(';'));
            fuse = fuse.Substring(fuse.IndexOf(';') + 1);

            NPC npc = XI.LibTuple.NL.Decode(XI.LibTuple.NL.Encode(npcCode));
            bool anyFriends = XI.Board.Garden.Values.Where(p => p.IsAlive
                && p.Team == player.Team && p.Tux.Count > 0).Any();
            return anyFriends && Artiad.ContentRule.IsNPCJoinable(npc, XI.LibTuple.HL, XI.Board);
        }
        public void NJ02Action(Player player, string fuse, string args) {
            ushort who = ushort.Parse(args);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                            new Artiad.Cure(who, 0, FiveElement.A, 1)));
        }
        public string NJ02Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public void NJ03Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(player.Uid, 2000, FiveElement.A, 1, 0)));
            XI.RaiseGMessage("G0DH," + who + ",0,1");
        }
        public string NJ03Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public void NJ04Action(Player player, string fuse, string args)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public void NJ05Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(who, 2000, FiveElement.A, 1, 0)));
        }
        public string NJ05Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public void NJ06Action(Player player, string fuse, string args)
        {
            int idx = args.IndexOf(',');
            ushort from = ushort.Parse(args.Substring(0, idx));
            ushort to = ushort.Parse(args.Substring(idx + 1));
            Player py = XI.Board.Garden[from];
            string imc = XI.AsyncInput(from, "Q1(p" + string.Join("p", py.Tux) + ")", "NJ06", "0");
            ushort card = ushort.Parse(imc);
            XI.RaiseGMessage("G0HQ,0," + to + "," + from + ",1,1," + card);
        }
        public string NJ06Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0 &&
                    XI.Board.Garden.Values.Where(q => q.IsAlive && q.Uid != p.Uid && q.Team == p.Team).Any()).Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                Player pho = XI.Board.Garden[who];
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Uid != who && p.Team == pho.Team).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        public bool NJ06Valid(Player player, string fuse)
        {
            return XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0 &&
                XI.Board.Garden.Values.Where(q => q.IsAlive && q.Uid != p.Uid && q.Team == p.Team).Any()).Any();
        }
        public void NJ07Action(Player player, string fuse, string args)
        {
            int idx = args.IndexOf(',');
            int jdx = args.IndexOf(',', idx + 1);
            ushort from = ushort.Parse(args.Substring(0, idx));
            ushort to = ushort.Parse(Util.Substring(args, idx + 1, jdx));
            ushort pet = ushort.Parse(args.Substring(jdx + 1));
            //XI.RaiseGMessage("G0HL," + from + "," + pet);
            XI.RaiseGMessage("G0HC,1," + to + "," + from + ",0," + pet);
        }
        public string NJ07Input(Player player, string fuse, string prev)
        {
            if (prev == "")
            {
                return "#交出宠物的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Pets.Where(q => q != 0).Any() &&
                    XI.Board.Garden.Values.Where(q => q.IsAlive &&
                        q.Uid != p.Uid && q.Team == p.Team).Any()).Select(p => p.Uid)) + ")";
            }
            int idx = prev.IndexOf(',');
            if (idx < 0)
            {
                ushort who = ushort.Parse(prev);
                Player pho = XI.Board.Garden[who];
                return "#交予宠物的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                        p => p.IsAlive && p.Uid != who && p.Team == pho.Team).Select(p => p.Uid)) + ")";
            }
            else
            {
                int jdx = prev.IndexOf(',', idx + 1);
                if (jdx < 0)
                {
                    ushort who = ushort.Parse(prev.Substring(0, idx));
                    //ushort to = ushort.Parse(prev.Substring(idx + 1));
                    Player pho = XI.Board.Garden[who];
                    return "M1(p" + string.Join("p", pho.Pets.Where(p => p != 0)) + ")";
                }
                else
                    return "";
            }
        }
        public bool NJ07Valid(Player player, string fuse)
        {
            return XI.Board.Garden.Values.Where(p => p.IsAlive && p.Pets.Where(q => q != 0).Any() &&
                XI.Board.Garden.Values.Where(q => q.IsAlive && q.Uid != p.Uid && q.Team == p.Team).Any()).Any();
        }
        public void NJ08Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            string c0 = Util.RepeatString("p0", XI.Board.Garden[who].Tux.Count);
            XI.AsyncInput(player.Uid, "#弃置的,C1(" + c0 + ")", "NJ08", "0");
            XI.RaiseGMessage("G0DH," + who + ",2,1");
        }
        public string NJ08Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool NJ08Valid(Player player, string fuse)
        {
            return XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0).Any();
        }
        public void NJ09Action(Player player, string fuse, string args)
        {
            DefaultPutIntoEscueAction(player, fuse);
        }
        public void NJ09EscueAction(Player player, ushort npcUt, int type, string fuse, string args)
        {
            ushort side = ushort.Parse(args);
            NPCStandardEscueAction(player, npcUt);
            XI.RaiseGMessage("G0IP," + side + ",1");
        }
        public string NJ09EscueInput(Player player, ushort npcUt, int type, string fuse, string prev)
        {
            if (prev == "")
                return "S";
            else
                return "";
        }
        public bool NJT1Valid(Player player, string fuse)
        {
            return XI.Board.MonPiles.Count > 0;
        }
        public void NJT1Action(Player player, string fuse, string args)
        {
            XI.RaiseGMessage("G0XZ," + player.Uid + ",2,0,1");
            string yes = XI.AsyncInput(player.Uid, "#请选择是否保留？##是##否,Y2", "NJT1", "0");
            if (yes == "2")
            {
                ushort pop = XI.Board.MonPiles.Dequeue();
                XI.RaiseGMessage("G2IN,1,1");
                XI.RaiseGMessage("G0ON,0,M,1," + pop);
            }
        }
        public void NJH2Action(Player player, string fuse, string args)
        {
            List<ushort> pops = XI.DequeueOfPile(XI.Board.TuxPiles, 7).ToList();
            XI.RaiseGMessage("G2IN,0,7");
            XI.RaiseGMessage("G1IU," + string.Join(",", pops));

            List<ushort> players = XI.Board.OrderedPlayer(player.Uid);
            foreach (ushort ut in players)
            {
                if (pops.Count <= 0) break;
                string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                string input = XI.AsyncInput(ut, "Z1" + pubTux, "NJH2Action", "0");
                ushort cd;
                if (ushort.TryParse(input, out cd) && XI.Board.PZone.Contains(cd))
                {
                    XI.RaiseGMessage("G1OU," + cd);
                    XI.RaiseGMessage("G2QU,0,0," + cd);
                    XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                    pops.Remove(cd);
                }
            }
            if (pops.Count > 0)
            {
                XI.RaiseGMessage("G1OU," + string.Join(",", pops));
                XI.RaiseGMessage("G2QU,0,0," + string.Join(",", pops));
                XI.RaiseGMessage("G0ON,0,C," + pops.Count + "," + pops);
            }
            XI.RaiseGMessage("G2FU,3");
        }
        public void NJH3Action(Player player, string fuse, string args)
        {
            DefaultPutIntoEscueAction(player, fuse);
        }
        public void NJH3EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            NPCStandardEscueAction(player, npcUt);
            XI.RaiseGMessage("G0DH," + player.Uid + ",0," + (player.TuxLimit - player.Tux.Count));
        }
        public void NJH6Action(Player player, string fuse, string args)
        {
            int idx = args.IndexOf(',');
            ushort who = ushort.Parse(args.Substring(0, idx));
            ushort ut = ushort.Parse(args.Substring(idx + 1));
            XI.RaiseGMessage("G0QZ," + who + "," + ut);
        }
        public string NJH6Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.GetEquipCount() > 0).Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort ut = ushort.Parse(prev);
                return "C1(p" + string.Join("p", XI.Board.Garden[ut].ListOutAllEquips()) + ")";
            }
            else
                return "";
        }
        public bool NJH6Valid(Player player, string fuse)
        {
            return XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetEquipCount() > 0).Any();
        }

        #region NPC Single
        public void NPCStandardEscueAction(Player player, ushort npcUt) // Standard CZ03
        {
            if (player.Escue.Contains(npcUt))
            {
                player.Escue.Remove(npcUt);
                XI.RaiseGMessage("G2OL," + player.Uid + "," + npcUt);
                XI.RaiseGMessage("G0ON," + player.Uid + ",M,1," + npcUt);
                ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "S", "CZ03", "0"));
                XI.RaiseGMessage("G0IP," + side + ",1");
            }
        }
        public void DefaultPutIntoEscueAction(Player player, string fuse)
        {
            string npcCode = fuse.Substring(0, fuse.IndexOf(';'));
            fuse = fuse.Substring(fuse.IndexOf(';') + 1);
            ushort ut = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode(npcCode));

            if (!player.Escue.Contains(ut))
            {
                player.Escue.Add(ut);
                XI.RaiseGMessage("G2IL," + player.Uid + "," + ut);
                if (XI.Board.Monster1 == ut)
                    XI.Board.Monster1 = 0;
            }
        }
        public void NCT41Debut(Player trigger)
        {
            int incr = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == trigger.OppTeam).Max(p => p.Tux.Count);
            ushort me = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCT41"));
            XI.RaiseGMessage("G0IB," + me + "," + incr);
        }
        public void NCH07Debut(Player trigger)
        {
            List<ushort> nmbs = new List<ushort>();
            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            if (pop > 0)
                nmbs.Add(pop);
            ushort[] pops = XI.Board.RestMonPiles.Dequeue(2);
            nmbs.AddRange(pops);
            if (nmbs.Count > 0)
            {
                XI.Board.MonPiles.PushBack(nmbs);
                XI.RaiseGMessage("G0YM,7," + nmbs.Count);
                XI.Board.MonPiles.Shuffle();
            }
        }
        #endregion NPC Single
    }
}
