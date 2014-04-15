using PSD.Base.Card;
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
            if (anyFriends)
            {
                Hero hero = XI.LibTuple.HL.InstanceHero(npc.Hero);
                if (hero == null)
                    return false;
                Hero hrc = XI.LibTuple.HL.InstanceHero(hero.Archetype);
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.SelectHero == 0)
                        continue;
                    if (py.SelectHero == npc.Hero && py.IsAlive)
                        return false;
                    foreach (int isoId in hero.Isomorphic)
                    {
                        if (isoId == py.SelectHero && py.IsAlive) // hero=10202,isoId=10203,py.Sel=10203
                            return false;
                    }
                    Hero hpy = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                    if (hrc != null && hpy.Avatar == hrc.Avatar)
                        return false;
                    else if (hpy.Archetype == hero.Avatar)
                        return false;
                }
                foreach (int ib in XI.Board.BannedHero)
                {
                    if (ib == npc.Hero)
                        return false;
                    foreach (int isoId in hero.Isomorphic)
                    {
                        if (isoId == ib)
                            return false;
                    }
                    Hero hpy = XI.LibTuple.HL.InstanceHero(ib);
                    if (hrc != null && hpy.Avatar == hrc.Avatar)
                        return false;
                    else if (hpy.Archetype == hero.Avatar)
                        return false;
                }
                return true;
            }
            else
                return false;
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
            string npcCode = fuse.Substring(0, fuse.IndexOf(';'));
            fuse = fuse.Substring(fuse.IndexOf(';') + 1);
            ushort ut = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode(npcCode));

            if (!player.Escue.Contains(ut))
            {
                player.Escue.Add(ut);
                XI.RaiseGMessage("G2IL," + player.Uid + "," + ut);
            }
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
                XI.Board.MonDises.Add(pop);
                XI.RaiseGMessage("G2IN,1,1");
                XI.RaiseGMessage("G2QC,1," + pop);
                XI.RaiseGMessage("G2ON,1," + pop);
            }
        }
    }
}
