using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base;

namespace PSD.PSDGamepkg.JNS
{
    public class NPCCottage : JNSBase
    {
        public NPCCottage(XI xi, Base.VW.IVI vi) : base(xi, vi) { }

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

                var methodEscueAction = njc.GetType().GetMethod(njCode + "EscueAction");
                if (methodEscueAction != null)
                    nj.EscueAction += (p, ut, t, f, a) => { methodEscueAction.Invoke(njc, new object[] { p, ut, t, f, a }); };
                var methodEscueValid = njc.GetType().GetMethod(njCode + "EscueValid");
                if (methodEscueValid != null)
                    nj.EscueValid += (p, ut, t, f) => { return (bool)methodEscueValid.Invoke(njc, new object[] { p, ut, t, f }); };
                var methodEscueInput = njc.GetType().GetMethod(njCode + "EscueInput");
                if (methodEscueInput != null)
                    nj.EscueInput += (p, ut, t, f, pv) => { return (string)methodEscueInput.Invoke(njc, new object[] { p, ut, t, f, pv }); };
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
                    npc.Debut += (player => { methodDebut.Invoke(njc, new object[] { player }); });
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
            return (prev == "") ? AnyoneAliveString() : "";
        }
        public void NJ03Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(player.Uid, 2000, FiveElement.A, 1, 0)));
            XI.RaiseGMessage("G0DH," + who + ",0,1");
        }
        public string NJ03Input(Player player, string fuse, string prev)
        {
            return (prev == "") ? AnyoneAliveString() : "";
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
            return (prev == "") ? AnyoneAliveString() : "";
        }
        public void NJ06Action(Player player, string fuse, string args)
        {
            int idx = args.IndexOf(',');
            ushort from = ushort.Parse(args.Substring(0, idx));
            ushort to = ushort.Parse(args.Substring(idx + 1));
            TargetPlayer(from, to);
            Player py = XI.Board.Garden[from];
            string imc = XI.AsyncInput(from, "Q1(p" + string.Join("p", py.Tux) + ")", "NJ06", "0");
            ushort card = ushort.Parse(imc);
            XI.RaiseGMessage("G0HQ,0," + to + "," + from + ",1,1," + card);
        }
        public string NJ06Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "#交出牌的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0 &&
                    XI.Board.Garden.Values.Where(q => q.IsAlive && q.Uid != p.Uid && q.Team == p.Team).Any()).Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                Player pho = XI.Board.Garden[who];
                return "#交予牌的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
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
            TargetPlayer(from, to);
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
            DefaultPutIntoEscueAction(player, fuse, player);
        }
        public void NJ09EscueAction(Player player, ushort npcUt, int type, string fuse, string args)
        {
            ushort side = ushort.Parse(args);
            EscueDiscard(player, npcUt);
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
        public void NJT2Action(Player player, string fuse, string args)
        {
            string sel = XI.AsyncInput(player.Uid, "T1" + AAllTareds(player) +
                ",F1(p" + string.Join("p", XI.LibTuple.RL.GetFullAppendableList()) + ")", "NJT2", "0");
            if (!string.IsNullOrEmpty(sel) && !sel.StartsWith(VI.CinSentinel))
                XI.RaiseGMessage("G0IF," + sel);
        }
        public void NJH1Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            Player py = XI.Board.Garden[who];
            if (py.Tux.Count > 0)
                XI.RaiseGMessage("G0DH," + who + ",2," + py.Tux.Count);
            DefaultPutIntoEscueAction(player, fuse, player);
        }
        public string NJH1Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "#弃掉所有手牌的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Team == player.Team && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool NJH1Valid(Player player, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.Team && p.Tux.Count > 0);
        }
        public void NJH1EscueAction(Player player, ushort npcUt, int type, string fuse, string args)
        {
            if (type == 0)
            {
                ushort side = ushort.Parse(args);
                NPC npc = XI.LibTuple.NL.Decode(Base.Card.NMBLib.OriginalNPC(npcUt));
                npc.ROMUshort = 1;
                XI.RaiseGMessage("G0IP," + side + ",4");
            }
            else if (type == 1)
            {
                Player oy = XI.Board.GetOpponenet(player);
                string next = XI.AsyncInput(oy.Uid, "#获得【阮英扬】的," + AnyoneAliveString(), "NJH1", "0");
                ushort nx = ushort.Parse(next);
                NPC npc = XI.LibTuple.NL.Decode(Base.Card.NMBLib.OriginalNPC(npcUt));
                npc.ROMUshort = 0;
                if (nx != player.Uid)
                {
                    player.Escue.Remove(npcUt);
                    XI.RaiseGMessage("G2OL," + player.Uid + "," + npcUt);
                    XI.Board.Garden[nx].Escue.Add(npcUt);
                    XI.RaiseGMessage("G2IL," + nx + "," + npcUt);
                }
            }
        }
        public bool NJH1EscueValid(Player player, ushort npcUt, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
            {
                NPC npc = XI.LibTuple.NL.Decode(Base.Card.NMBLib.OriginalNPC(npcUt));
                return npc != null && npc.ROMUshort != 0;
            }
            else
                return false;
        }
        public string NJH1EscueInput(Player player, ushort npcUt, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "S";
            else
                return "";
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
                XI.RaiseGMessage("G2FU,0," + ut + ",0,C," + string.Join(",", XI.Board.PZone));
                string input = XI.AsyncInput(ut, "Z1" + pubTux, "NJH2", "0");
                ushort cd;
                if (ushort.TryParse(input, out cd) && XI.Board.PZone.Contains(cd))
                {
                    XI.RaiseGMessage("G1OU," + cd);
                    XI.RaiseGMessage("G2QU,0,0," + cd);
                    XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                    pops.Remove(cd);
                }
                XI.RaiseGMessage("G2FU,3");
            }
            if (pops.Count > 0)
            {
                XI.RaiseGMessage("G1OU," + string.Join(",", pops));
                XI.RaiseGMessage("G2QU,0,0," + string.Join(",", pops));
                XI.RaiseGMessage("G0ON,0,C," + pops.Count + "," + string.Join(",", pops));
            }
        }
        public void NJH3Action(Player player, string fuse, string args)
        {
            DefaultPutIntoEscueAction(player, fuse, player);
        }
        public void NJH3EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            EscueDiscard(player, npcUt);
            XI.RaiseGMessage("G0DH," + player.Uid + ",0," + (player.TuxLimit - player.Tux.Count));
        }
        public void NJH4Action(Player player, string fuse, string args)
        {
            ushort tar = ushort.Parse(args);
            TargetPlayer(player.Uid, tar);
            DefaultPutIntoEscueAction(player, fuse, XI.Board.Garden[tar]);
        }
        public string NJH4Input(Player player, string fuse, string prev)
        {
            return (prev == "") ? AnyoneAliveString() : "";
        }
        public void NJH4EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            Monster monster = XI.Board.Battler as Monster;
            int n = (monster != null && monster.Level == Monster.ClLevel.BOSS) ? 3 : 1;
            EscueDiscard(player, npcUt);
            XI.RaiseGMessage("G0IA," + player.Uid + ",1," + n);
            XI.RaiseGMessage("G0IX," + player.Uid + ",1," + n);
        }
        public void NJH5Action(Player player, string fuse, string args)
        {
            ushort tar = ushort.Parse(args);
            TargetPlayer(player.Uid, tar);
            DefaultPutIntoEscueAction(player, fuse, XI.Board.Garden[tar]);
        }
        public string NJH5Input(Player player, string fuse, string prev)
        {
            return (prev == "") ? AnyoneAliveString() : "";
        }
        public void NJH5EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(player.Uid, 2000, FiveElement.YIN, 1, 0)));
            else if (type == 1)
                EscueDiscard(player, npcUt);
        }
        public bool NJH5EscueValid(Player player, ushort npcUt, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1) // G0HD,0/1,A,B,x
            {
                string[] g0hd = fuse.Split(',');
                ushort who = ushort.Parse(g0hd[2]);
                return who == player.Uid;
            }
            else
                return false;
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
        public void NJH7Action(Player player, string fuse, string args)
        {
            while (XI.Board.TuxPiles.Count > 0)
            {
                ushort ut = XI.DequeueOfPile(XI.Board.TuxPiles);
                XI.RaiseGMessage("G2IN,0,1");
                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    XI.RaiseGMessage("G0ON,0,C,1," + ut);
                    if (tux.IsTuxEqiup())
                    {
                        if (tux.Type == Base.Card.Tux.TuxType.WQ && XI.Board.TuxDises.Contains(ut))
                        {
                            string whoStr = XI.AsyncInput(player.Uid, "#获得【" + tux.Name + "】," +
                                AnyoneAliveString(), "NJH7", "0");
                            ushort who = ushort.Parse(whoStr);
                            XI.RaiseGMessage("G2CN,0,1");
                            XI.RaiseGMessage("G0HQ,2," + who + ",0,0," + ut);
                            XI.Board.TuxDises.Remove(ut);
                        }
                        break;
                    }
                }
            }
        }
        public void NJH8Action(Player player, string fuse, string args)
        {
            DefaultPutIntoEscueAction(player, fuse, player);
        }
        public bool NJH8EscueValid(Player player, ushort npcUt, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.Team && p.GetPetCount() > 0);
        }
        public void NJH8EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            string[] args = argst.Split(',');
            ushort petOwner = ushort.Parse(args[0]);
            ushort pet = ushort.Parse(args[1]);
            ushort caller = ushort.Parse(args[2]);

            EscueDiscard(player, npcUt);
            XI.RaiseGMessage("G0HI," + petOwner + "," + pet);
            TargetPlayer(petOwner, caller);
            XI.RaiseGMessage("G0IA," + caller + ",1,3");
        }
        public string NJH8EscueInput(Player player, ushort npcUt, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#要爆发宠物,/T1" + FormatPlayers(p => p.IsAlive && p.GetPetCount() > 0 && p.Team == player.Team);
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                return "#爆发,/M1(p" + string.Join("p",
                    XI.Board.Garden[who].Pets.Where(p => p != 0)) + "),#凭神的,/T1" + AAlls(player);
            }
            else return "";
        }
        public void NJH9Action(Player player, string fuse, string args)
        {
            DefaultPutIntoEscueAction(player, fuse, player);
        }
        public bool NJH9EscueValid(Player player, ushort npcUt, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.Team == player.Team && p.IsAlive && p.HP == 0);
        }
        public void NJH9EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            EscueDiscard(player, npcUt);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(XI.Board.Garden.Values.Where(p => p.Team == player.Team &&
                p.HP == 0).Select(p => new Artiad.Cure(p.Uid, 0, FiveElement.A, 2))));
            Artiad.Procedure.AssignCurePointToTeam(XI, XI.Board.GetOpponenet(player), 3, "NJH9",
                p => Cure(null, p.Keys.ToList(), p.Values.ToList()));
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", -15);
        }
        #region NPC Single
        public void NCT41Debut(Player trigger)
        {
            int incr = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == trigger.OppTeam).Max(p => p.Tux.Count) - 1;
            ushort me = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCT41"));
            if (incr >= 0)
                XI.RaiseGMessage("G0IB," + me + "," + incr);
        }
        public void NCH05Debut(Player trigger)
        {
            XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count != 3)
                .Select(p => p.Uid).ToList().ForEach(p => XI.RaiseGMessage("G0DS," + p + ",0,1"));
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
        public void NCH10Debut(Player trigger)
        {
            ushort card = (XI.LibTuple.TL.EncodeTuxCode("WQ03") as TuxEqiup).SingleEntry;
            ushort who = XI.Board.Garden.Values.Where(p => p.ListOutAllEquips().Contains(card)).Select(p => p.Uid).FirstOrDefault();
            if (who != 0)
            {
                ushort me = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCH10"));
                XI.RaiseGMessage("G0QZ," + who + "," + card);
                XI.RaiseGMessage("G0IB," + me + "," + 3);
            }
        }
        #endregion NPC Single
        #region NPC General
        public void DefaultPutIntoEscueAction(Player player, string fuse, Player target)
        {
            string npcCode = fuse.Substring(0, fuse.IndexOf(';'));
            fuse = fuse.Substring(fuse.IndexOf(';') + 1);
            ushort ut = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode(npcCode));

            if (!target.Escue.Contains(ut))
            {
                target.Escue.Add(ut);
                XI.RaiseGMessage("G2IL," + target.Uid + "," + ut);
                if (XI.Board.Monster1 == ut)
                    XI.Board.Monster1 = 0;
            }
        }
        public void EscueDiscard(Player player, ushort npcUt)
        {
            player.Escue.Remove(npcUt);
            XI.RaiseGMessage("G2OL," + player.Uid + "," + npcUt);
            XI.RaiseGMessage("G0ON," + player.Uid + ",M,1," + npcUt);
        }
        #endregion NPC Single
    }
}
