using PSD.Base;
using PSD.Base.Card;
using PSD.Base.Utils;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.JNS
{
    public class EveCottage : JNSBase
    {
        public EveCottage(XI XI, Base.VW.IVI vi) : base(XI, vi) { }
        public IDictionary<string, Evenement> RegisterDelegates(EvenementLib lib)
        {
            EveCottage ec = this;
            IDictionary<string, Evenement> ev01 = new Dictionary<string, Evenement>();
            foreach (Evenement eve in lib.ListAllEves(0))
            {
                string cardCode = string.Format(eve.Code) + "";
                ev01.Add(cardCode, eve);
                var method = ec.GetType().GetMethod(cardCode);
                if (method != null)
                    eve.Action += delegate(Player player) { method.Invoke(ec, new object[] { player }); };

                var methodPers = ec.GetType().GetMethod(cardCode + "Pers");
                if (methodPers != null)
                    eve.Pers += new Evenement.ActionDelegate(delegate(Player player)
                    { methodPers.Invoke(ec, new object[] { player }); });
                var methodPersValid = ec.GetType().GetMethod(cardCode + "PersValid");
                if (methodPersValid != null)
                    eve.PersValid += new Evenement.ValidDelegate(delegate()
                        { return (bool)methodPersValid.Invoke(ec, new object[] { }); });
            }
            return ev01;
        }
        #region Eve Of Pal1
        public void SJ101(Player rd)
        {
            if (rd.Gender == 'M')
            {
                XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
                Harm(null, rd, 1, FiveElement.THUNDER);
            }
            else if (rd.Gender == 'F')
            {
                if (rd.Armor != 0)
                    XI.RaiseGMessage("G0QZ," + rd.Uid + "," + rd.Armor);
                if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.Gender == 'M'))
                {
                    string input = XI.AsyncInput(rd.Uid, "#「天雷破」的,/T1" + 
                        FormatPlayers(p => p.IsAlive && p.Gender == 'M'), "SJ101", "0");
                    if (!input.StartsWith("/") && input != VI.CinSentinel)
                    {
                        ushort target = ushort.Parse(input);
                        XI.RaiseGMessage("G0CC," + rd.Uid + ",0," + rd.Uid + ",JP05,0;1,R" + rd.Uid + "EV," + target);
                    }
                }
            }
        }
        public void SJ102(Player rd)
        {
            string msg = AffichePlayers(p => p.IsAlive && p.GetPetCount() == 0, p => p.Uid + ",0,1");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void SJ103(Player rd)
        {
            string input = XI.AsyncInput(rd.Uid, "#获得2张手牌的,T1" +
                ATeammates(rd) + ",T1" + AEnemy(rd), "SJ103", "0");
            string[] ips = input.Split(',');
            XI.RaiseGMessage("G0DH," + ips[0] + ",0,2," + ips[1] + ",0,2");
        }
        public void SJ104(Player rd)
        {
            Player nx = XI.Board.GetOpponenet(rd);
            List<ushort> pops = XI.DequeueOfPile(XI.Board.TuxPiles, 4).ToList();
            XI.RaiseGMessage("G2IN,0,4");
            XI.RaiseGMessage("G1IU," + string.Join(",", pops));
            
            ushort[] uds = { rd.Uid, nx.Uid };
            string[] ranges = { ATeammates(rd), AEnemy(rd) };
            string input; string[] ips;
            int idxs = 1;

            do 
            {
                XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0,C," + string.Join(",", pops));
                input = XI.AsyncInput(uds[idxs], "+Z1(p" + string.Join("p", XI.Board.PZone) +
                    "),#获得卡牌的,/T1" + ranges[idxs], "SJ104", "0");
                if (!input.Contains(VI.CinSentinel) && !input.StartsWith("/"))
                {
                    ips = input.Split(',');
                    ushort cd;
                    if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                    {
                        ushort ut = ushort.Parse(ips[1]);
                        XI.RaiseGMessage("G1OU," + cd);
                        XI.RaiseGMessage("G2QU,0,C,0," + cd);
                        XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                        pops.Remove(cd);
                        idxs = (idxs + 1) % 2;
                    }
                }
                XI.RaiseGMessage("G2FU,3");
            } while (pops.Count > 0);
        }
        #endregion Eve Of Pal1
        #region Eve Of Pal2
        public void SJ201(Player rd)
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP <= 3).ToList();
            if (invs.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p.Uid + ",0,1")));
        }

        public void SJ202(Player rd)
        {
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.HP >= 2 && p.Team == rd.Team).Select(p => p.Uid).ToList();
            if (invs.Count > 0)
            {
                string input = XI.AsyncInput(rd.Uid,"#HP将为1的,T1(p" + string.Join("p", invs) + ")", "SJ202", "0");
                ushort target = ushort.Parse(input);
                Player leifeng = XI.Board.Garden[target];
                Harm(null, leifeng, leifeng.HP - 1, FiveElement.A, (int)HPEvoMask.TERMIN_AT);
            }
            string msg = AffichePlayers(p => p.IsAlive && p.Team == XI.Board.Rounder.Team, p => p.Uid + ",0,1");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }
        #endregion Eve Of Pal2
        #region Eve Of Pal3

        public void SJ301(Player rd)
        {
            string msg = string.Join(",", XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Tux.Count != 3).Select(p => p.Tux.Count > 3 ?
                (p.Uid + ",1," + (p.Tux.Count - 3)) : (p.Uid + ",0," + (3 - p.Tux.Count))));
            if (!string.IsNullOrEmpty(msg))
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void SJ302(Player rd)
        {
            int minHp = XI.Board.Garden.Values.Where(p => p.IsAlive).Min(p => p.HP);
            Cure(null, XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == minHp), 2);
        }
        public void SJ303(Player rd)
        {
            int countOfPet = rd.GetPetCount();
            if (countOfPet > 0)
                XI.RaiseGMessage("G0DH," + rd.Uid + ",1," + countOfPet);
            XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
        }
        #endregion Eve Of Pal3
        #region Eve Of Pal3A
        public void S3W01(Player rd)
        {
            int maxSTR = XI.Board.Garden.Values.Where(p => p.IsAlive).Max(p => p.STR);
            string msgP = AffichePlayers(p => p.IsAlive && p.STR == maxSTR, p => p.Uid + ",1,2");

            int minSTR = XI.Board.Garden.Values.Where(p => p.IsAlive).Min(p => p.STR);
            string msgN = AffichePlayers(p => p.IsAlive && p.STR == minSTR, p => p.Uid + ",0,2");

            XI.RaiseGMessage("G0DH," + msgP + "," + msgN);
        }

        public void S3W02(Player rd)
        {
            int countOfPet = XI.Board.Garden.Values.Where(
                p => p.Team == rd.OppTeam).Sum(p => p.GetPetCount());
            if (countOfPet > 0)
            {
                string input = XI.AsyncInput(rd.Uid, "#回复HP的,T1" + ATeammates(rd), "S3W02", "0");
                ushort target = ushort.Parse(input);
                Cure(null, XI.Board.Garden[target], countOfPet);
            }
        }
        #endregion Eve Of Pal3A
        #region Eve Of Pal4
        public void SJ401(Player rd)
        {
            if (rd.Tux.Count > 0)
                XI.RaiseGMessage("G0DH," + rd.Uid + ",2," + rd.Tux.Count);
            XI.RaiseGMessage("G0DH," + rd.Uid + ",0,2");
        }

        public void SJ402(Player rd)
        {
            //string result = Algo.SParal(XI.Board, p => p.IsAlive && p.GetPetCount() > 0,
            //    p => p.Uid + ",8," + p.GetPetCount(), ",");
            //if (result != null && result != "")
            //    XI.RaiseGMessage("G0OH," + result);
            var lst = XI.Board.Garden.Values.Where(p => p.IsAlive &&p.GetPetCount() > 0).ToList();
            if (lst.Any())
                XI.RaiseGMessage(Artiad.Harm.ToMessage(lst.Select(
                    p => new Artiad.Harm(p.Uid, 0, FiveElement.A, p.GetPetCount(), 0))));
        }
        #endregion Eve Of Pal4
        #region Eve Of Pal5
        public void SJ501(Player rd)
        {
            int maxTux = XI.Board.Garden.Values.Where(p => p.IsAlive).Max(p => p.Tux.Count);
            var v = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count == maxTux);
            if (v.Count() == 1)
                XI.RaiseGMessage("G0DS," + v.Single().Uid + ",0,1");
        }
        #endregion Eve Of Pal5

        #region Package 5#
        public void SJT01(Player rd)
        {
            Player hd = XI.Board.Facer(rd);
            if (hd != null && hd.IsAlive)
            {
                XI.RaiseGMessage("G0HQ,4," + rd.Uid + "," + hd.Uid + "," + rd.Tux.Count + "," + hd.Tux.Count +
                    (rd.Tux.Count > 0 ? ("," + string.Join(",", rd.Tux)) : "") +
                    (hd.Tux.Count > 0 ? ("," + string.Join(",", hd.Tux)) : ""));
            }
        }
        public void SJT02(Player rd)
        {
            List<Artiad.Harm> harms = new List<Artiad.Harm>();
            foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsAlive))
            {
                int acc = (py.Weapon != 0 ? 1 : 0) + (py.Armor != 0 ? 1 : 0) +
                    (py.Trove != 0 ? 1 : 0) + (py.ExEquip != 0 ? 1 : 0);
                if (acc > 0)
                    harms.Add(new Artiad.Harm(py.Uid, 0, FiveElement.A, acc, 0));
            }
            if (harms.Count > 0)
                XI.RaiseGMessage(Artiad.Harm.ToMessage(harms));
        }
        public void SJT03(Player rd)
        {
            Artiad.Procedure.ObtainAndAllocateTux(XI, VI, rd, XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Team == rd.OppTeam).Sum(p => p.GetPetCount()) + 1, "SJT03", "0");
            Player od = XI.Board.GetOpponenet(rd);
            Artiad.Procedure.ObtainAndAllocateTux(XI, VI, od, XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Team == rd.Team).Sum(p => p.GetPetCount()) + 1, "SJT03", "1");
        }
        public void SJT04(Player rd)
        {
            Player op = XI.Board.GetOpponenet(rd);
            int sumMe = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
            int sumOe = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam)
                .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                .Sum(q => XI.LibTuple.ML.Decode(q).STR));

            Player py;
            if (sumMe < sumOe)
                py = rd;
            else if (sumMe > sumOe)
                py = op;
            else if (rd.Team == 1)
                py = rd;
            else
                py = op;
            List<ushort> uts = XI.Board.TuxDises.Where(p =>
                XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.WQ ||
                XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.FJ).ToList();
            if (uts.Count > 0)
            {
                string ranges = "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Team == py.Team).Select(p => p.Uid)) + ")";
                XI.RaiseGMessage("G1IU," + string.Join(",", uts));
                do
                {
                    XI.RaiseGMessage("G2FU,0," + py.Uid + ",0,C," + string.Join(",", uts));
                    string input = XI.AsyncInput(py.Uid, "+Z1(p" + string.Join("p",
                        XI.Board.PZone) + ")" + ",#获得卡牌的,/T1" + ranges, "SJT04", "0");
                    if (!input.StartsWith("/"))
                    {
                        string[] ips = input.Split(',');
                        ushort cd;
                        if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                        {
                            ushort ut = ushort.Parse(ips[1]);
                            XI.RaiseGMessage("G1OU," + cd);
                            uts.Remove(cd);
                            XI.RaiseGMessage("G2QU,0,C,0," + cd);
                            // CongQIPaiDuiLiQiDiao
                            XI.RaiseGMessage("G2FU,3");
                            XI.RaiseGMessage("G2CN,0,1");
                            XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                            XI.Board.TuxDises.Remove(cd);
                            string os = XI.AsyncInput(ut, "#您是否要立即装备？##是##否,Y2", "SJT04", "0");
                            if (os == "1")
                            XI.RaiseGMessage(new Artiad.EquipStandard()
                            {
                                Who = ut, Source = ut, SingleCard = cd
                            }.ToMessage());
                            break;
                        }
                    }
                } while (true);
                if (uts.Count > 0)
                    XI.RaiseGMessage("G1OU," + string.Join(",", uts));
            }
        }
        //public void SJT04Pers()
        //{
        //    int sumAka = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == 1)
        //            .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
        //            .Sum(q => XI.LibTuple.ML.Decode(q).STR));
        //    int sumAo = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == 2)
        //        .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
        //        .Sum(q => XI.LibTuple.ML.Decode(q).STR));

        //    if (sumAka <= sumAo)
        //        XI.RaiseGMessage("G0IP,1,3");
        //    else
        //        XI.RaiseGMessage("G0IP,2,3");
        //}
        public void SJT05(Player rd)
        {
            string result = AffichePlayers(p => p.IsAlive && p.Tux.Count > 0, p => p.Uid + ",1,1");
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH," + result);

            int sumTR = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
            int sumTO = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam)
                .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                .Sum(q => XI.LibTuple.ML.Decode(q).STR));

            if (sumTR > sumTO)
                result = AffichePlayers(p => p.IsAlive && p.Team == rd.OppTeam, p => p.Uid + ",0,1");
            else if (sumTR < sumTO)
                result = AffichePlayers(p => p.IsAlive && p.Team == rd.Team, p => p.Uid + ",0,1");
            else
                result = rd.Uid + ",0,1";
            XI.RaiseGMessage("G0DH," + result);
        }
        public void SJT06(Player rd)
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.GetPetCount() == 0).ToList();
            if (invs.Any())
                XI.RaiseGMessage("G1XR,1,0,2," + string.Join(",", invs.Select(p => p.Uid)));
        }
        public void SJT07(Player rd)
        {
            var gv = XI.Board.Garden.Values;
            int lr = gv.Count(p => p.IsAlive && p.Team == rd.Team);
            int or = gv.Count(p => p.IsAlive && p.Team == rd.OppTeam);
            Player py = (lr <= or) ? rd : XI.Board.GetOpponenet(rd);

            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
            XI.RaiseGMessage("G0YM,3,0," + pop);
            XI.RaiseGMessage("G1NI," + rd.Uid + "," + pop);
            XI.Board.Wang.Push(pop);
            UEchoCode r5ed = XI.HandleWithNPCEffect(py, npc, "SJT07");
            if (r5ed == UEchoCode.NO_OPTIONS)
                XI.AsyncInput(rd.Uid, "//", "SJT07", "1");
            if (r5ed == UEchoCode.END_ACTION)
                XI.RaiseGMessage("G1YP," + py.Uid + "," + pop);
            
            if (XI.Board.Wang.Count > 0 && XI.Board.Wang.Peek() == pop)
            { // In case the NPC has been taken away
                XI.Board.Wang.Pop();
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = pop }
                }.ToMessage());
            }
            if (XI.Board.Wang.Count > 0)
                XI.RaiseGMessage("G0YM,3,1," + XI.Board.Wang.Pop());
            else
                XI.RaiseGMessage("G0YM,3,1,0"); // actual just remove one Wang, it should show the rests
        }
        public void SJT08(Player rd)
        {
            string g0dh = "";
            List<Player> ovs = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.GetEquipCount() > 2).ToList();
            if (ovs.Count > 0)
                g0dh += "," + string.Join(",", ovs.Select(p => p.Uid + ",1," + (p.GetEquipCount() - 2)));
            List<Player> ivs = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.GetEquipCount() < 2).ToList();
            if (ivs.Count > 0)
                g0dh += "," + string.Join(",", ivs.Select(p => p.Uid + ",0," + (2 - p.GetEquipCount())));
            if (g0dh.Length > 0)
                XI.RaiseGMessage("G0DH" + g0dh);
        }
        public void SJT09(Player rd)
        {
            List<Artiad.Cure> cures = XI.Board.Garden.Values
                .Where(p => p.IsAlive && p.Tux.Count < 3).Select(p =>
                    new Artiad.Cure(p.Uid, 0, FiveElement.A, 3 - p.Tux.Count, 0)).ToList();
            List<Artiad.Harm> harms = XI.Board.Garden.Values
                .Where(p => p.IsAlive && p.Tux.Count > 3).Select(p =>
                    new Artiad.Harm(p.Uid, 0, FiveElement.A, p.Tux.Count - 3, 0)).ToList();
            if (cures.Count > 0)
                XI.RaiseGMessage(Artiad.Cure.ToMessage(cures));
            if (harms.Count > 0)
                XI.RaiseGMessage(Artiad.Harm.ToMessage(harms));
        }
        public void SJT10(Player rd)
        {
            if (XI.Board.RestNPCPiles.Count > 0)
            {
                ushort pop = XI.Board.RestNPCPiles.Dequeue();
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
                XI.RaiseGMessage("G0YM,3,0," + pop);
                XI.RaiseGMessage("G1NI," + rd.Uid + "," + pop);
                XI.Board.Wang.Push(pop);
                int sr = npc.STR < 5 ? npc.STR : 5;
                if (rd.Tux.Count > sr)
                    XI.RaiseGMessage("G0DH," + rd.Uid + ",1," + (rd.Tux.Count - sr));
                else if (rd.Tux.Count < sr)
                    XI.RaiseGMessage("G0DH," + rd.Uid + ",0," + (sr - rd.Tux.Count));
                //XI.Board.RestNPCDises.Add(pop);
                if (XI.Board.Wang.Count > 0 && XI.Board.Wang.Peek() == pop)
                {
                    XI.Board.Wang.Pop();
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = pop }
                    }.ToMessage());
                }
                if (XI.Board.Wang.Count > 0)
                    XI.RaiseGMessage("G0YM,3,1," + XI.Board.Wang.Pop());
                else
                    XI.RaiseGMessage("G0YM,3,1,0");
            }
        }
        #endregion Package 5#
        #region Package 6#
        public void SJT11(Player rd)
        {
            foreach (ushort ut in XI.Board.OrderedPlayer(rd.Uid))
            {
                Player py = XI.Board.Garden[ut];
                if (py.IsAlive && py.Team == rd.OppTeam)
                {
                    XI.RaiseGMessage("G2YS,T," + rd.Uid + ",T," + py.Uid);
                    XI.RaiseGMessage("G0TT," + rd.Uid);
                    int valR = XI.Board.DiceValue;
                    XI.RaiseGMessage("G0TT," + py.Uid);
                    int valP = XI.Board.DiceValue;

                    if (valR == valP && rd.STR == py.STR)
                        continue;
                    bool win = valR > valP || (valR == valP && rd.STR < py.STR);
                    Player winner = win ? rd : py;
                    Player losser = win ? py : rd;
                    int select = 0;
                    if (losser.Tux.Count > 0 && losser.HasAnyEquips())
                    {
                        string sel = XI.AsyncInput(winner.Uid, "#请选择一项##获得手牌##弃置装备,Y2", "SJT11", "0");
                        if (sel == "1") select = 1;
                        else if (sel == "2") select = 2;
                    }
                    else if (losser.Tux.Count > 0)
                    {
                        XI.AsyncInput(winner.Uid, "#请选择一项##获得手牌,Y1", "SJT11", "0");
                        select = 1;
                    }
                    else if (losser.HasAnyEquips())
                    {
                        XI.AsyncInput(winner.Uid, "#请选择一项##弃置装备,Y1", "SJT11", "0");
                        select = 2;
                    }
                    if (select == 1)
                    {
                        string c0 = Algo.RepeatString("p0", XI.Board.Garden[losser.Uid].Tux.Count);
                        XI.AsyncInput(winner.Uid, "#获得的,C1(" + c0 + ")", "SJT11", "1");
                        XI.RaiseGMessage("G0HQ,0," + winner.Uid + "," + losser.Uid + ",2,1");
                    }
                    else if (select == 2)
                    {
                        string c0 = "p" + string.Join("p", losser.ListOutAllEquips());
                        string which = XI.AsyncInput(winner.Uid, "#弃置的,C1(" + c0 + ")", "SJT11", "1");
                        if (!which.Contains(VI.CinSentinel))
                            XI.RaiseGMessage("G0QZ," + losser.Uid + "," + which);
                    }
                }
            }
        }
        public void SJT12(Player rd)
        {
            var pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0).ToList();
            if (pys.Any())
                XI.RaiseGMessage("G1XR,1,0,0," + string.Join(",", pys.Select(p => p.Uid)));
        }
        public void SJT13(Player rd)
        {
            List<Artiad.Harm> harms = XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p =>
                new Artiad.Harm(p.Uid, p.Uid, FiveElement.A, XI.LibTuple.HL.InstanceHero(p.SelectHero)
                .Spouses.Count(q => !q.StartsWith("!")), (long)HPEvoMask.TUX_INAVO)).ToList();
            if (harms.Count > 0)
                XI.RaiseGMessage(Artiad.Harm.ToMessage(harms));
            List<ushort> zeros = harms.Where(p => p.N == 0).Select(p => p.Who).ToList();
            var ans = XI.MultiAsyncInput(zeros.Select(p => XI.Board.Garden[p]).Where(
                p =>p.GetEquipCount() > 0).ToDictionary(p => p.Uid, p =>
                "#弃置(否则获得负面标记),/Q1(p" + string.Join("p", p.ListOutAllEquips()) + ")"));
            ans.Where(p => !p.Value.StartsWith("/") && p.Value != VI.CinSentinel).ToList().ForEach(p =>
            {
                XI.RaiseGMessage("G0QZ," + p.Key + "," + p.Value);
                zeros.Remove(p.Key);
            });
            zeros.ForEach(p => XI.RaiseGMessage("G0IF," + p + ",5,6"));
        }
        public void SJT14(Player rd)
        {
            int hpR = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.Team).Sum(p => p.HP);
            int hpO = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam).Sum(p => p.HP);
            Player lesser = null;
            if (hpR < hpO) { lesser = rd; }
            else if (hpR > hpO) { lesser = XI.Board.GetOpponenet(rd); }
            if (lesser != null)
            {
                int result = 0;
                XI.RaiseGMessage("G0TT," + lesser.Uid);
                result += XI.Board.DiceValue;
                XI.RaiseGMessage("G0TT," + lesser.Uid);
                result += XI.Board.DiceValue;
                Artiad.Procedure.AssignCurePointToTeam(XI, lesser, result, "SJT14",
                    p => Cure(null, p.Keys.ToList(), p.Values.ToList()));
            }
        }
        public void SJT15(Player rd)
        {
            int n = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam)
                .Sum(p => p.Runes.Intersect(XI.LibTuple.RL.GetFullPositive()).Count());
            if (n > 0)
            {
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.Team == rd.OppTeam && py.IsAlive)
                    {
                        List<ushort> runes = py.Runes.Intersect(XI.LibTuple.RL.GetFullPositive()).ToList();
                        if (runes.Count > 0)
                            XI.RaiseGMessage("G0OF," + py.Uid + "," + string.Join(",", runes));
                    }
                }
                string whoStr = XI.AsyncInput(rd.Uid, "#承担伤害,T1" + ATeammates(rd), "SJT15", "0");
                if (!whoStr.Contains(VI.CinSentinel))
                {
                    ushort who = ushort.Parse(whoStr);
                    XI.RaiseGMessage("G2YS,T," + rd.Uid + ",T," + who);
                    Harm(null, XI.Board.Garden[who], n, FiveElement.SOLARIS);
                }
            }
        }
        public void SJT16(Player rd)
        {
            ushort cardUt = XI.DequeueOfPile(XI.Board.TuxPiles);
            XI.RaiseGMessage("G2IN,0,1");
            Tux tux = XI.LibTuple.TL.DecodeTux(cardUt);
            XI.RaiseGMessage(new Artiad.Abandon()
            {
                Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                Genre = Card.Genre.Tux,
                SingleUnit = new Artiad.CustomsUnit() { SingleCard = cardUt }
            }.ToMessage());

            List<ushort> list = XI.Board.OrderedPlayer();
            foreach (ushort ut in list)
            {
                Player py = XI.Board.Garden[ut];
                if (py.IsAlive && py.Tux.Count > 0)
                {
                    XI.AsyncInput(ut, "//", "SJT16", "0");
                    XI.RaiseGMessage("G2FU,0," + ut + ",0,C," + string.Join(",", py.Tux));
                    List<ushort> nots = py.Tux.Where(p => !tux.IsSameType(XI.LibTuple.TL.DecodeTux(p))).ToList();
                    if (nots.Count > 0)
                        XI.RaiseGMessage("G0QZ," + ut + "," + string.Join(",", nots));
                }
            }
            List<ushort> rest = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Tux.Count <= 1).Select(p => p.Uid).ToList();
            if (rest.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", rest.Select(p => p + ",0,1")));
        }
        public void SJT17(Player rd)
        {
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.IsAlive && py.Runes.Count > 0)
                    XI.RaiseGMessage("G0OF," + py.Uid + "," + string.Join(",", py.Runes));
            }
            ushort[] fuseMap = new ushort[] { 0, 1, 2, 3, 4, 5, 6 };
            foreach (ushort ut in XI.Board.OrderedPlayer(rd.Uid))
            {
                if (XI.Board.Garden[ut].IsAlive)
                {
                    XI.RaiseGMessage("G0TT," + ut);
                    int result = XI.Board.DiceValue;
                    XI.RaiseGMessage("G0IF," + ut + "," + fuseMap[result]);
                }
            }
        }
        public void SJT18(Player rd)
        {
            List<ushort> pops = new List<ushort>();
            string g0pq = "";
            foreach (Player player in XI.Board.Garden.Values)
            {
                if (player.IsAlive && (player.Tux.Count > 0 || player.GetBaseEquipCount() > 0))
                {
                    List<ushort> ts = player.Tux.Union(player.ListOutAllBaseEquip()).ToList();
                    pops.AddRange(ts);
                    g0pq += "," + player.Uid + "," + ts.Count + "," + string.Join(",", ts);
                }
            }
            if (pops.Count > 0)
            {
                XI.RaiseGMessage("G0PQ" + g0pq);
                pops.Shuffle();
                int rx = XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == rd.Team);
                int ox = XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == rd.OppTeam);
                int idx = 0;
                while (idx < pops.Count)
                {
                    foreach (ushort pyu in XI.Board.OrderedPlayer(rd.Uid))
                    {
                        Player player = XI.Board.Garden[pyu];
                        if (player.IsAlive)
                        {
                            bool less = (player.Team == rd.Team && rx < ox) || (player.Team == rd.OppTeam && rx > ox);
                            if (less && idx + 1 < pops.Count)
                                XI.RaiseGMessage("G0HQ,2," + pyu + ",2," + pops[idx++] + "," + pops[idx++]);
                            else
                                XI.RaiseGMessage("G0HQ,2," + pyu + ",2," + pops[idx++]);
                        }
                        if (idx >= pops.Count)
                            break;
                    }
                }
            }
            // TODO: substitide IT with HQ, OT with new guy (PQ indicates anomoys flying and OT)
        }
        public void SJT19(Player rd)
        {
            var lst = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0).ToList();
            if (lst.Any())
                XI.RaiseGMessage(Artiad.Harm.ToMessage(lst.Select(
                    p => new Artiad.Harm(p.Uid, 0, FiveElement.YINN, p.Tux.Count, 0))));
        }
        public void SJT20(Player rd)
        {
            List<Player> zeros = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count <= 1).ToList();
            List<Player> ones = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 1).ToList();
            List<Player> fours = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count >= 4).ToList();
            if (ones.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", ones.Select(p => p.Uid + ",2," + (p.Tux.Count - 1))));
            fours.ForEach(p => XI.RaiseGMessage("G0IF," + p.Uid + ",6"));
            zeros.ForEach(p => XI.RaiseGMessage("G0IF," + p.Uid + ",4"));
        }
        #endregion Package 6#
        #region Holiday
        public void SJH01(Player rd)
        {
            List<ushort> greater = XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.STR > rd.STR && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            string askHint = "/";
            if (greater.Count == 1)
                askHint = "#获取手牌,/T1(p" + greater[0] + ")";
            else if (greater.Count >= 2)
                askHint = "#获取手牌,/T1~2(p" + string.Join("p", greater) + ")";
            string select = XI.AsyncInput(rd.Uid, askHint, "SJH01", "0");
            if (select != VI.CinSentinel && !select.StartsWith("/"))
            {
                ushort[] tars = select.Split(',').Select(p => ushort.Parse(p)).ToArray();
                TargetPlayer(rd.Uid, tars);
                foreach (ushort tar in tars)
                {
                    XI.AsyncInput(rd.Uid, string.Format("#获得{0}的,C1({1})", XI.DisplayPlayer(tar),
                        Algo.RepeatString("p0", XI.Board.Garden[tar].Tux.Count)), "SJH01", "1");
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + tar + ",2,1");
                }
            }
            XI.RaiseGMessage(new Artiad.Goto()
            {
                Terminal = "R" + XI.Board.Rounder.Uid + "ED"
            }.ToMessage());
        }
        public void SJH02(Player rd)
        {
            List<ushort> list = XI.Board.OrderedPlayer();
            List<ushort> showList = new List<ushort>(), notShowList = new List<ushort>();
            foreach (ushort ut in list)
            {
                Player py = XI.Board.Garden[ut];
                if (py.IsAlive)
                {
                    bool show = false;
                    if (!py.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP))
                    {
                        string select = XI.AsyncInput(ut, "#是否展示您的手牌？##是##否,Y2", "SJH02", "0");
                        if (select == "1")
                            show = true;
                    }
                    else
                        XI.AsyncInput(ut, "#是否展示您的手牌？##否,Y1", "SJH02", "0");
                    if (show)
                    {
                        if (py.Tux.Count > 0)
                            XI.RaiseGMessage("G2FU,0," + ut + ",0,C," + string.Join(",", py.Tux));
                        showList.Add(ut);
                    } else
                        notShowList.Add(ut);
                }
            }
            string result = "";
            if (notShowList.Count > 0)
                result += "," + string.Join(",", notShowList.Select(p => p + ",1,2"));
            if (showList.Count > 0)
                result += "," + string.Join(",", showList.Select(p => p + ",0,2"));
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH" + result);
        }
        public void SJH03(Player rd)
        {
            IDictionary<ushort, string> requires = new Dictionary<ushort, string>();
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.GetEquipCount() > 0)
                    requires.Add(py.Uid, "#须弃置,Q1(p" + string.Join("p", py.ListOutAllEquips()) + ")");
            }
            IDictionary<ushort, string> inputs = XI.MultiAsyncInput(requires);
            foreach (var pair in inputs)
                XI.RaiseGMessage("G0QZ," + pair.Key + "," + pair.Value);

            requires.Clear();
            foreach (Player py in XI.Board.Garden.Values)
            {
                List<ushort> special = py.ListOutAllEquips()
                    .Where(p => !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()).ToList();
                if (special.Count >= 2)
                    requires.Add(py.Uid, "#须弃置,Q2(p" + string.Join("p", special) + ")");
                else if (special.Count == 1)
                    requires.Add(py.Uid, "#须弃置,Q1(p" + string.Join("p", special) + ")");
            }
            inputs = XI.MultiAsyncInput(requires);
            foreach (var pair in inputs)
                XI.RaiseGMessage("G0QZ," + pair.Key + "," + pair.Value);

            List<Player> hasEscues = XI.Board.Garden.Values.Where(p => p.Escue.Count > 0).ToList();
            if (hasEscues.Count > 0)
            {
                XI.RaiseGMessage("G2OL," + string.Join(",", hasEscues.Select(p => string.Join(",",
                    p.Escue.Select(q => p.Uid + "," + q)))));
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.NMB,
                    List = hasEscues.Select(p => new Artiad.CustomsUnit()
                    {
                        Source = p.Uid,
                        Cards = p.Escue.ToArray()
                    }).ToList()
                }.ToMessage());
                hasEscues.ForEach(p => p.Escue.Clear());
            }
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.Runes.Count > 0)
                    XI.RaiseGMessage("G0OF," + py.Uid + "," + string.Join(",", py.Runes));
            }
        }

        public void SJH04(Player rd)
        {
            ushort pop = XI.Board.RestMonPiles.Dequeue();
            Monster mon = XI.LibTuple.ML.Decode(pop);
            XI.RaiseGMessage("G0YM,5," + pop);
            XI.RaiseGMessage("G0TT," + rd.Uid);
            if (XI.Board.DiceValue + rd.STR > mon.STR)
            {
                bool done = false;
                while (!done)
                {
                    List<Player> commer = XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > rd.GetPetCount() &&
                    p.Pets.Any(q => q != 0 && XI.LibTuple.ML.Decode(q).STR <= mon.STR)).ToList();
                    string selT = XI.AsyncInput(rd.Uid, commer.Count > 0 ? ("#弃置宠物,/T1(p" + string.Join("p",
                        commer.Select(p => p.Uid)) + ")") : "/", "SJH04", "0");
                    if (!selT.StartsWith("/") && !selT.Contains(VI.CinSentinel))
                    {
                        ushort who = ushort.Parse(selT);
                        List<ushort> pets = XI.Board.Garden[who].Pets.Where(p => p != 0 &&
                            XI.LibTuple.ML.Decode(p).STR <= mon.STR).ToList();
                        string selM = XI.AsyncInput(rd.Uid, pets.Count > 0 ?
                            ("#弃置宠物,/M1(p" + string.Join("p", pets) + ")") : "/", "SJH04", "1");
                        if (!selM.StartsWith("/") && !selM.Contains(VI.CinSentinel))
                        {
                            XI.RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = who,
                                SinglePet = ushort.Parse(selM)
                            }.ToMessage());
                            done = true;
                        }
                    }
                    else if (selT.StartsWith("/"))
                        done = true;
                }
            }
            else
            {
                XI.RaiseGMessage(Artiad.Harm.ToMessage(
                    new Artiad.Harm(rd.Uid, (pop + 1000), mon.Element, rd.GetPetCount() + 1, 0)));
            }
        }

        public void SJH05(Player rd)
        {
            if (rd.Tux.Count > 0)
            {
                Player nx = XI.Board.GetOpponenet(rd);
                XI.RaiseGMessage("G2FU,0," + nx.Uid + ",0,C," + string.Join(",", rd.Tux));
                string select = XI.AsyncInput(nx.Uid, "C1(p" + string.Join("p", rd.Tux) + ")", "SJH05", "0");
                if (select != VI.CinSentinel)
                {
                    ushort ut = ushort.Parse(select);
                    Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    if (!Artiad.ContentRule.IsTuxUsableEveryWhere(tux))
                        XI.RaiseGMessage("G0QZ," + rd.Uid + "," + ut);
                    else
                    {
                        string hint = string.Format("#成为【{0}】使用者的,T1{1}", tux.Name, AAllTareds(nx));
                        string tarStr = XI.AsyncInput(nx.Uid, hint, "SJH05", "1");
                        if (tux.IsTuxEqiup())
                            XI.RaiseGMessage("G1UE," + tarStr + "," + rd.Uid + "," + ut);
                        else
                        {
                            ushort tar = ushort.Parse(tarStr);
                            if (tux.Valid(XI.Board.Garden[tar], 0, "G1EV," + rd.Uid))
                            {
                                XI.RaiseGMessage("G0CC," + rd.Uid + ",0," + tarStr +
                                    "," + tux.Code + "," + ut + ";0,G1EV," + rd.Uid);
                            }
                            else
                                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + ut);
                        }
                    }
                }
            }
            XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
        }
        public void SJH06(Player rd)
        {
            foreach (ushort ut in XI.Board.OrderedPlayer(rd.Uid))
            {
                if (rd.Tux.Count == 0) { break; }
                Player py = XI.Board.Garden[ut];
                if (ut == rd.Uid || !py.IsAlive) { continue; }
                TargetPlayer(rd.Uid, ut);
                string second = XI.AsyncInput(rd.Uid, string.Format("#交予{0}的,Q1(p{1})",
                    XI.DisplayPlayer(ut), string.Join("p", rd.ListOutAllCards())), "SJH06", "0");
                ushort card = ushort.Parse(second);
                if (card == 0)
                    XI.RaiseGMessage("G0HQ,0," + ut + "," + rd.Uid + ",2,1");
                else
                    XI.RaiseGMessage("G0HQ,0," + ut + "," + rd.Uid + ",0,1," + card);
            }
            if (!XI.Board.Garden.Values.Any(p => p.IsAlive && p.Uid != rd.Uid && p.HP <= rd.HP) ||
                !XI.Board.Garden.Values.Any(p => p.IsAlive && p.Uid != rd.Uid && p.STR <= rd.STR) ||
                !XI.Board.Garden.Values.Any(p => p.IsAlive && p.Uid != rd.Uid && p.DEX <= rd.DEX))
            {
                Cure(null, rd, XI.Board.Garden.Values.Count(p => p.IsAlive));
            }
        }
        public void SJH07(Player rd)
        {
            int count = 0;
            foreach (ushort ut in XI.Board.OrderedPlayer(rd.Uid))
            {
                Player py = XI.Board.Garden[ut];
                if (ut == rd.Uid || !py.IsAlive || py.GetAllCardsCount() == 0) { continue; }
                TargetPlayer(rd.Uid, ut);
                string second = XI.AsyncInput(rd.Uid, string.Format("#获得{0}的,C1(p{1})",
                    XI.DisplayPlayer(ut), string.Join("p", py.ListOutAllCardsWithEncrypt())), "SJH07", "0");
                ushort card = ushort.Parse(second);
                if (card == 0)
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + ut + ",2,1");
                else
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + ut + ",0,1," + card);
                if (py.Team == rd.OppTeam)
                    ++count;
            }
            if (count > 0)
                Harm(null, rd, count);
        }
        public void SJH10(Player rd)
        {
            List<ushort> possible = new List<ushort>();
            if (rd.Tux.Count > 0)
                possible.Add(0);
            if (rd.GetEquipCount() > 0)
                possible.Add(1);
            if (rd.Runes.Count > 0)
                possible.Add(2);
            if (rd.Escue.Count > 0)
                possible.Add(3);
            string[] name = { "手牌", "装备牌", "标记", "助战NPC" };
            if (possible.Count > 0)
            {
                string select = XI.AsyncInput(rd.Uid, "#请选择要全部弃置的牌类型##" +
                    string.Join("##", possible.Select(p => name[p])) + ",Y" + possible.Count, "SJH10", "0");
                if (select != VI.CinSentinel)
                {
                    ushort pick = possible[int.Parse(select) - 1];
                    if (pick == 0)
                    {
                        XI.RaiseGMessage("G0DH," + rd.Uid + ",2," + rd.Tux.Count);
                        XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
                    }
                    if (pick == 1)
                    {
                        XI.RaiseGMessage("G0QZ," + rd.Uid + "," + string.Join(",", rd.ListOutAllEquips()));
                        System.Func<ushort, bool> isEq = (p) => XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup();
                        List<ushort> picks = Artiad.Procedure.CardHunter(XI, Card.Genre.Tux,
                            (p) => isEq(p), (a, r) => a.Any(p => isEq(p)), false);
                        if (picks.Count > 0)
                            XI.RaiseGMessage("G0HQ,2," + rd.Uid + ",0,0," + picks.Single());
                    }
                    else if (pick == 2)
                    {
                        XI.RaiseGMessage("G0OF," + rd.Uid + "," + string.Join(",", rd.Runes));
                        string obtain = XI.AsyncInput(rd.Uid, "#获得标记,F1(p" + string.Join("p",
                            XI.LibTuple.RL.GetFullAppendableList()) +")", "SJH10", "1");
                        if (!obtain.StartsWith(VI.CinSentinel))
                            XI.RaiseGMessage("G0IF," + rd.Uid + "," + obtain);
                    }
                    else if (pick == 3)
                    {
                        XI.RaiseGMessage("G2OL," + string.Join(",", rd.Escue.Select(p => rd.Uid + "," + p)));
                        XI.RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                            Genre = Card.Genre.NMB,
                            SingleUnit = new Artiad.CustomsUnit()
                            {
                                Source = rd.Uid,
                                Cards = rd.Escue.ToArray()
                            }
                        }.ToMessage());
                        rd.Escue.Clear();

                        ushort pop = XI.Board.RestNPCPiles.Dequeue();
                        NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
                        XI.RaiseGMessage("G0YM,3,0," + pop);
                        XI.RaiseGMessage("G1NI," + rd.Uid + "," + pop);
                        XI.Board.Wang.Push(pop);
                        UEchoCode r5ed = XI.HandleWithNPCEffect(rd, npc, "SJH10");
                        if (r5ed == UEchoCode.NO_OPTIONS)
                            XI.AsyncInput(rd.Uid, "//", "SJH10", "2");
                        if (r5ed == UEchoCode.END_ACTION)
                            XI.RaiseGMessage("G1YP," + rd.Uid + "," + pop);

                        if (XI.Board.Wang.Count > 0 && XI.Board.Wang.Peek() == pop)
                        { // In case the NPC has been taken away
                            XI.Board.Wang.Pop();
                            XI.RaiseGMessage(new Artiad.Abandon()
                            {
                                Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                                Genre = Card.Genre.NMB,
                                SingleUnit = new Artiad.CustomsUnit() { SingleCard = pop }
                            }.ToMessage());
                        }
                        if (XI.Board.Wang.Count > 0)
                            XI.RaiseGMessage("G0YM,3,1," + XI.Board.Wang.Pop());
                        else
                            XI.RaiseGMessage("G0YM,3,1,0"); // actual just remove one Wang, it should show the rests
                    }
                }
            }
        }
        public void SJH11(Player rd)
        {
            IDictionary<ushort, string> dict = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == rd.OppTeam && p.Tux.Count > 3 && XI.Board.Facer(p).IsAlive).ToDictionary(p => p.Uid,
                p => ("#交予对方的,Q" + (p.Tux.Count - 3) + "(p" + string.Join("p", p.Tux) + ")"));
            List<Player> lesss = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == rd.OppTeam && p.Tux.Count < 3 && p.Tux.Count > 0).ToList();
            if (dict.Count > 0)
            {
                IDictionary<ushort, string> ans = XI.MultiAsyncInput(dict);
                ans.ToList().ForEach(p => XI.RaiseGMessage("G0HQ,0," + XI.Board.Facer(XI.Board.Garden[p.Key]).Uid +
                    "," + string.Join(",", p.Key + ",1," + p.Value.Split(',').Length + "," + p.Value)));
                XI.RaiseGMessage("G0DH," + string.Join(",", ans.Select(p => p.Key + ",0," + (2 * p.Value.Split(',').Length))));
            }
            if (lesss.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", lesss.Select(p => p.Uid + ",1," + (3 - p.Tux.Count))));
        }
        #endregion Holiday

        public void SJ001(Player rd)
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(XI.Board.Garden.Values.Where(p => p.IsAlive)
                .Select(p => new Artiad.Harm(p.Uid, 0, FiveElement.AQUA, 12, 0))));
        }

        public void SJ002(Player rd)
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(5, 0, FiveElement.SATURN, 12, 0)));
            XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(1, 0, FiveElement.SATURN, 12, 0)));
        }

        public void SJ003(Player rd)
        {
            XI.RaiseGMessage(new Artiad.ObtainPet() { Farmer = 2, SinglePet = 15 }.ToMessage());
            XI.RaiseGMessage(new Artiad.ObtainPet() { Farmer = 4, SinglePet = 6 }.ToMessage());
            XI.RaiseGMessage(new Artiad.ObtainPet() { Farmer = 6, SinglePet = 7 }.ToMessage());
        }
    }
}
