using PSD.Base;
using PSD.Base.Card;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

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
                    nj.Action += new NCAction.ActionDelegate(delegate (Player player, string fuse, string argst)
                    {
                        methodAction.Invoke(njc, new object[] { player, fuse, argst });
                    });
                var methodValid = njc.GetType().GetMethod(njCode + "Valid");
                if (methodValid != null)
                    nj.Valid += new NCAction.ValidDelegate(delegate (Player player, string fuse)
                    {
                        return (bool)methodValid.Invoke(njc, new object[] { player, fuse });
                    });
                var methodInput = njc.GetType().GetMethod(njCode + "Input");
                if (methodInput != null)
                    nj.Input += new NCAction.InputDelegate(delegate (Player player, string fuse, string prev)
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
            string reason = fuse.Substring(fuse.IndexOf(';') + 1);

            NPC npc = XI.LibTuple.NL.Decode(XI.LibTuple.NL.Encode(npcCode));
            int tuxCount = wp.Tux.Count;
            XI.RaiseGMessage("G0DH," + who + ",2," + tuxCount);
            if (tp.SelectHero != 0)
                XI.RaiseGMessage("G0OY,0," + to);

            int hp = 2 * tuxCount;
            if ((reason.StartsWith("JP") || reason.StartsWith("SJ")) && hp > 3)
                hp = 3;
            else if (reason.StartsWith("XBT6"))
            {
                ushort xbt6Owner = Artiad.ContentRule.GetEquipmentOwnership("XBT6", XI);
                if (xbt6Owner != 0)
                {
                    ushort xbt6 = (XI.LibTuple.TL.EncodeTuxCode("XBT6") as TuxEqiup).SingleEntry;
                    XI.RaiseGMessage("G0ZI," + xbt6Owner + "," + xbt6);
                }
            }
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
            NPC npc = XI.LibTuple.NL.Decode(XI.LibTuple.NL.Encode(npcCode));
            bool anyFriends = XI.Board.Garden.Values.Where(p => p.IsAlive
                && p.Team == player.Team && p.Tux.Count > 0).Any();
            return anyFriends && Artiad.ContentRule.IsNPCJoinable(npc, XI);
        }
        public void NJ02Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            Cure(XI.Board.Garden[who], 1);
        }
        public string NJ02Input(Player player, string fuse, string prev)
        {
            return (prev == "") ? AnyoneAliveString() : "";
        }
        public void NJ03Action(Player player, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            Harm(player, 1);
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
            Harm(XI.Board.Garden[who], 1);
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
            ushort to = ushort.Parse(Algo.Substring(args, idx + 1, jdx));
            TargetPlayer(from, to);
            ushort pet = ushort.Parse(args.Substring(jdx + 1));
            XI.RaiseGMessage(new Artiad.HarvestPet()
            {
                Farmer = to,
                Farmland = from,
                SinglePet = pet,
                TreatyAct = Artiad.HarvestPet.Treaty.KOKAN
            }.ToMessage());
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
            string c0 = Algo.RepeatString("p0", XI.Board.Garden[who].Tux.Count);
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
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = pop }
                }.ToMessage());
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
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(npcUt));
                npc.ROMUshort = 1;
                XI.RaiseGMessage(new Artiad.AnnouceCard()
                {
                    Action = Artiad.AnnouceCard.Type.FLASH,
                    Genre = Card.Genre.NMB,
                    Officer = player.Uid,
                    SingleCard = npcUt
                }.ToMessage());
                XI.RaiseGMessage("G0IP," + side + ",4");
            }
            else if (type == 1)
            {
                Player oy = XI.Board.GetOpponenet(player);
                string next = XI.AsyncInput(oy.Uid, "#获得【阮英扬】的," + AnyoneAliveString(), "NJH1", "0");
                ushort nx = ushort.Parse(next);
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(npcUt));
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
                if (pops.Count <= 0) { break; }
                if (!XI.Board.Garden[ut].IsAlive) { continue; }
                XI.RaiseGMessage("G2FU,0," + ut + ",0,C," + string.Join(",", XI.Board.PZone));
                string input = XI.AsyncInput(ut, "Z1(p" +
                    string.Join("p", XI.Board.PZone) + ")", "NJH2", "0");
                ushort cd;
                if (ushort.TryParse(input, out cd) && XI.Board.PZone.Contains(cd))
                {
                    XI.RaiseGMessage("G1OU," + cd);
                    XI.RaiseGMessage("G2QU,0,C,0," + cd);
                    XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                    pops.Remove(cd);
                }
                XI.RaiseGMessage("G2FU,3");
            }
            if (pops.Count > 0)
            {
                XI.RaiseGMessage("G1OU," + string.Join(",", pops));
                XI.RaiseGMessage("G2QU,0,C,0," + string.Join(",", pops));
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit() { Cards = pops.ToArray() }
                }.ToMessage());
            }
        }
        public void NJH3Action(Player player, string fuse, string args)
        {
            ushort tar = ushort.Parse(args);
            TargetPlayer(player.Uid, tar);
            Harm(player, 1);
            DefaultPutIntoEscueAction(player, fuse, XI.Board.Garden[tar]);
        }
        public string NJH3Input(Player player, string fuse, string prev)
        {
            return (prev == "") ? AnyoneAliveString() : "";
        }
        public void NJH3EscueAction(Player player, ushort npcUt, int type, string fuse, string argst)
        {
            EscueDiscard(player, npcUt);
            if (player.TuxLimit > player.Tux.Count)
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
            XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
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
                Harm(player, 1, FiveElement.A, (int)HPEvoMask.TUX_INAVO);
            else if (type == 1)
                EscueDiscard(player, npcUt);
        }
        public bool NJH5EscueValid(Player player, ushort npcUt, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
                return Artiad.ObtainPet.Parse(fuse).Farmer == player.Uid;
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
            System.Func<ushort, bool> isEq = (p) => XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup();
            List<ushort> picks = Artiad.Procedure.CardHunter(XI, Card.PileGenre.Tux,
                (p) => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.WQ,
                (a, r) => a.Count(p => isEq(p)) + r.Count(p => isEq(p)) == 2, true);
            if (picks.Count > 0)
            {
                string whoStr = XI.AsyncInput(player.Uid, string.Format("#获得{0},{1}",
                    string.Join("与", picks.Select(p => "【" + XI.LibTuple.TL.DecodeTux(p).Name + "】")),
                    AnyoneAliveString()), "NJH7", "0");
                ushort who = ushort.Parse(whoStr);
                XI.RaiseGMessage("G0HQ,2," + who + ",0,0," + string.Join(",", picks));
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
            Cure(XI.Board.Garden.Values.Where(p => p.Team == player.Team && p.HP == 0), 2);
            Artiad.Procedure.AssignCurePointToTeam(XI, XI.Board.GetOpponenet(player), 3, "NJH9",
                p => Cure(null, p.Keys.ToList(), p.Values.ToList()));
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", -15);
        }
        #region NPC Single
        public void NCT27Debut(Player trigger)
        {
            ushort me = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCT27"));
            List<ushort> restNpc = XI.Board.MonDises.Where(p => NMBLib.IsNPC(p)).ToList();
            if (restNpc.Count > 0)
            {
                string pick = XI.AsyncInput(trigger.Uid, "#替换,/M1(p" +
                    string.Join("p", restNpc) + ")", "NCT27Debut", "0");
                if (!pick.StartsWith("/"))
                {
                    ushort substitude = ushort.Parse(pick);
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = me }
                    }.ToMessage());

                    XI.Board.MonDises.Remove(substitude);
                    XI.RaiseGMessage("G2CN,1,1");
                    XI.RaiseGMessage(new Artiad.ImperialLeft()
                    {
                        Zone = Artiad.ImperialLeft.ZoneType.M2,
                        Trigger = trigger.Uid,
                        Card = substitude
                    }.ToMessage());
                }
            }
        }
        public void NCT32Debut(Player trigger)
        {
            if (XI.Board.InCampaign)
                ++XI.Board.Rounder.RestZP;
        }
        public void NCT33Debut(Player trigger)
        {
            int tval = XI.Board.Rounder.Tux.Count;
            ushort me = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCT33"));
            if (tval < 5)
                XI.RaiseGMessage("G0IB," + me + "," + (5 - tval));
        }
        public void NCT42Debut(Player trigger)
        {
            int incr = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == trigger.OppTeam).Max(p => p.Tux.Count) - 1;
            ushort me = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCT42"));
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
            ushort[] pops = XI.Board.RestNPCPiles.Dequeue(2);
            nmbs.AddRange(pops);
            pops = XI.Board.RestMonPiles.Dequeue(2);
            nmbs.AddRange(pops);
            if (nmbs.Count > 0)
            {
                XI.RaiseGMessage(new Artiad.ImperialRight()
                {
                    Genre = Card.Genre.NMB,
                    Encrypted = true,
                    SingleItem = new Artiad.ImperialRightUnit() { Source = 0, Cards = nmbs.ToArray() }
                }.ToMessage());
                XI.Board.MonPiles.Shuffle();
            }
        }
        public void NCH10Debut(Player trigger)
        {
            System.Func<ushort, ushort, bool> cardBurstable =
                (pyut, equt) => equt != 0 && !XI.Board.CsEqiups.Contains(pyut + "," + equt);
            List<ushort> hasWqs = XI.Board.Garden.Values.Where(p => p.IsAlive && (cardBurstable(p.Uid, p.Weapon) ||
                (cardBurstable(p.Uid, p.ExEquip) && (p.ExMask & 0x1) != 0))).Select(p => p.Uid).ToList();
            if (hasWqs.Count > 0)
            {
                string whoSel = XI.AsyncInput(trigger.Uid, "#爆发武器,T1(p" +
                    string.Join("p", hasWqs) + ")", "NCH10Debut", "0");
                ushort who = ushort.Parse(whoSel);
                Player py = XI.Board.Garden[who];
                List<ushort> wqs = new List<ushort>();
                if (py.Weapon != 0) wqs.Add(py.Weapon);
                if (py.ExEquip != 0 && (py.ExMask & 0x1) != 0) wqs.Add(py.ExEquip);
                string tuxSel = XI.AsyncInput(trigger.Uid, "#爆发," + (trigger.Uid == who ? "Q" : "C") +
                    "1(p" + string.Join("p", wqs) + ")", "NCH10Debut", "1");
                ushort ut = ushort.Parse(tuxSel);
                XI.RaiseGMessage("G0ZI," + who + "," + ut);
                TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as TuxEqiup;
                int adj = tue.IncrOfSTR + 1;
                if (adj > 0)
                {
                    ushort nch10 = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode("NCH10"));
                    XI.RaiseGMessage("G0IB," + nch10 + "," + adj);
                }
            }
        }
        #endregion NPC Single
        #region NPC General
        public void DefaultPutIntoEscueAction(Player player, string fuse, Player target)
        {
            string npcCode = fuse.Substring(0, fuse.IndexOf(';'));
            ushort ut = NMBLib.CodeOfNPC(XI.LibTuple.NL.Encode(npcCode));

            if (!target.Escue.Contains(ut))
            {
                target.Escue.Add(ut);
                XI.RaiseGMessage("G2IL," + target.Uid + "," + ut);
                if (XI.Board.Monster1 == ut)
                    XI.Board.Monster1 = 0;
                else if (XI.Board.Wang.Count > 0 && XI.Board.Wang.Peek() == ut)
                {
                    XI.RaiseGMessage(new Artiad.ImperialLeft()
                    {
                        Zone = Artiad.ImperialLeft.ZoneType.W,
                        IsReset = true
                    }.ToMessage());
                }
            }
        }
        public void EscueDiscard(Player player, ushort npcUt)
        {
            player.Escue.Remove(npcUt);
            XI.RaiseGMessage("G2OL," + player.Uid + "," + npcUt);
            XI.RaiseGMessage(new Artiad.Abandon()
            {
                Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                Genre = Card.Genre.NMB,
                SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, SingleCard = npcUt }
            }.ToMessage());
        }
        #endregion NPC Single

        #region NPC Effect Util
        private void Harm(Player py, int n, FiveElement element = FiveElement.A, long mask = 0)
        {
            Harm(null, py, n, element, HPEvoMask.FROM_NMB.Set(mask));
        }
        private void Cure(Player py, int n, FiveElement element = FiveElement.A, long mask = 0)
        {
            Cure(null, py, n, element, HPEvoMask.FROM_NMB.Set(mask));
        }
        private void Cure(IEnumerable<Player> pys, int n, FiveElement element = FiveElement.A, long mask = 0)
        {
            Cure(null, pys, n, element, HPEvoMask.FROM_NMB.Set(mask));
        }
        #endregion NPC Effect Util
    }
}
