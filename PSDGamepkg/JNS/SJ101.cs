using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                string range = Util.SSelect(XI.Board, p => p.IsAlive && p.Gender == 'M');
                if (range != null)
                {
                    string input = XI.AsyncInput(rd.Uid, "#「天雷破」的,/T1" + range, "SJ101", "0");
                    if (input != "0" && input != "" && input != "/0")
                    {
                        ushort target = ushort.Parse(input);
                        XI.VI.Cout(0, "{0}对{1}预定使用「天雷破」.", XI.DisplayPlayer(rd.Uid), XI.DisplayPlayer(target));
                        XI.RaiseGMessage("G0CC," + rd.Uid + ",0," + rd.Uid + ",JP05,0;1,R" + rd.Uid + "EV," + target);
                    }
                    else
                        XI.VI.Cout(0, "{0}放弃使用「天雷破」.", XI.DisplayPlayer(rd.Uid));
                }
            }
        }
        public void SJ102(Player rd)
        {
            string msg = Util.SParal(XI.Board, p => p.IsAlive && p.GetPetCount() == 0,
                p => p.Uid + ",0,1", ",");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void SJ103(Player rd)
        {
            string range1 = Util.SSelect(XI.Board, p => p.Team == rd.Team && p.IsAlive);
            string range2 = Util.SSelect(XI.Board, p => p.Team == rd.OppTeam && p.IsAlive);
            string input = XI.AsyncInput(rd.Uid, "#获得2张手牌的,T1" + range1 + ",T1" + range2, "SJ103", "0");
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
            string[] ranges = { Util.SSelect(XI.Board, p => p.Team == rd.Team && p.IsAlive),
                        Util.SSelect(XI.Board, p => p.Team == rd.OppTeam && p.IsAlive) };
            string input; string[] ips;
            int idxs = 1;

            do 
            {
                XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0,C," + string.Join(",", pops));
                string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                input = XI.AsyncInput(uds[idxs], "+Z1" + pubTux + ",#获得卡牌的,/T1" + ranges[idxs], "SJ104", "0");
                if (!input.Contains(VI.CinSentinel) && !input.StartsWith("/"))
                {
                    ips = input.Split(',');
                    ushort cd;
                    if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                    {
                        ushort ut = ushort.Parse(ips[1]);
                        XI.RaiseGMessage("G1OU," + cd);
                        XI.RaiseGMessage("G2QU,0,0," + cd);
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
            string msg = Util.SParal(XI.Board, p => p.IsAlive && p.HP <= 3,
                p => p.Uid + ",0,1", ",");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }

        public void SJ202(Player rd)
        {
            string range = Util.SSelect(XI.Board, p => p.IsAlive && p.HP >= 2 && p.Team == rd.Team);
            if (range != null)
            {
                string input = XI.AsyncInput(XI.Board.Rounder.Uid,"#HP将为1的,T1" + range, "SJ202", "0");
                ushort target = ushort.Parse(input);
                Player targetPy = XI.Board.Garden[target];
                int dHp = targetPy.HP - 1;
                int maskDuel = Artiad.IntHelper.SetMask(0, GiftMask.INCOUNTABLE, true);
                XI.RaiseGMessage(Artiad.Harm.ToMessage(
                    new Artiad.Harm(target, 0, FiveElement.SOL, dHp, maskDuel)));
            }
            string msg = Util.SParal(XI.Board, p => p.IsAlive && p.Team.Equals(XI.Board.Rounder.Team),
                p => p.Uid + ",0,1", ",");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }
        #endregion Eve Of Pal2
        #region Eve Of Pal3

        public void SJ301(Player rd)
        {
            string msgP = Util.SParal(XI.Board, p => p.IsAlive && p.Tux.Count < 3,
                p => p.Uid + ",0," + (3 - p.Tux.Count), ",");
            string msgN = Util.SParal(XI.Board, p => p.IsAlive && p.Tux.Count > 3,
                p => p.Uid + ",1," + (p.Tux.Count - 3), ",");
            string msg;
            if (msgN != null && msgP != null)
                msg = msgP + "," + msgN;
            else if (msgN != null && msgP == null)
                msg = msgN;
            else if (msgN == null && msgP != null)
                msg = msgP;
            else
                msg = "";
            if (msg != "")
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void SJ302(Player rd)
        {
            int minHp = XI.Board.Garden.Values.Where(p => p.IsAlive).Min(p => p.HP);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.HP == minHp).Select(p => new Artiad.Cure(p.Uid, 0, FiveElement.A, 2))));
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
            string msgP = Util.SParal(XI.Board, p => p.IsAlive && p.STR == maxSTR,
                p => p.Uid + ",1,2", ",");

            int minSTR = XI.Board.Garden.Values.Where(p => p.IsAlive).Min(p => p.STR);
            string msgN = Util.SParal(XI.Board, p => p.IsAlive && p.STR == minSTR,
                p => p.Uid + ",0,2", ",");

            XI.RaiseGMessage("G0DH," + msgP + "," + msgN);
        }

        public void S3W02(Player rd)
        {
            int countOfPet = XI.Board.Garden.Values.Where(
                p => p.Team == rd.OppTeam).Sum(p => p.GetPetCount());
            if (countOfPet > 0)
            {
                string range = Util.SSelect(XI.Board, p => p.IsAlive && p.Team == rd.Team);
                string input = XI.AsyncInput(rd.Uid, "#回复HP的,T1" + range, "S3W02", "0");
                ushort target = ushort.Parse(input);
                XI.RaiseGMessage(Artiad.Cure.ToMessage(
                    new Artiad.Cure(target, 0, FiveElement.A, countOfPet)));
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
            //string result = Util.SParal(XI.Board, p => p.IsAlive && p.GetPetCount() > 0,
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
            ushort uhd = (ushort)(((rd.Uid + 1) / 2 * 4) - 1 - rd.Uid);
            Player hd = XI.Board.Garden[uhd];
            if (hd != null && hd.IsAlive)
            {
                string rtx = rd.Tux.Count > 0 ? string.Join(",", rd.Tux) : "";
                string htx = hd.Tux.Count > 0 ? string.Join(",", hd.Tux) : "";
                int rtxn = rd.Tux.Count, htxn = hd.Tux.Count;
                if (rtx != "")
                    XI.RaiseGMessage("G0HQ,0," + hd.Uid + "," + rd.Uid + ",1," + rtxn + "," + rtx);
                if (htx != "")
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + hd.Uid + ",1," + htxn + "," + htx);
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
            Player od = XI.Board.GetOpponenet(rd);
            int rn = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam)
                .Sum(p => p.GetPetCount()) + 1;
            int on = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.Team)
                .Sum(p => p.GetPetCount()) + 1;
            List<ushort> rps = XI.Board.Garden.Values.Where(p => p.IsAlive
                && p.Team == rd.Team).Select(p => p.Uid).ToList();
            List<ushort> ops = XI.Board.Garden.Values.Where(p => p.IsAlive
                && p.Team == rd.OppTeam).Select(p => p.Uid).ToList();
            ushort[] rpsa = XI.Board.Garden.Values.Where(p => p.Team == rd.Team).Select(p => p.Uid).ToArray();
            ushort[] opsa = XI.Board.Garden.Values.Where(p => p.Team == rd.OppTeam).Select(p => p.Uid).ToArray();
            string rg = string.Join(",", rps), rf = "(p" + string.Join("p", rps) + ")";
            string og = string.Join(",", ops), of = "(p" + string.Join("p", ops) + ")";

            List<ushort> pops = XI.DequeueOfPile(XI.Board.TuxPiles, rn).ToList();
            XI.RaiseGMessage("G2IN,0," + rn);
            XI.RaiseGMessage("G1IU," + string.Join(",", pops));
            do 
            {
                XI.RaiseGMessage("G2FU,0," + rd.Uid + "," + rps.Count + "," + rg + ",C," + string.Join(",", pops));
                string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                int pubSz = XI.Board.PZone.Count;
                string pubDig = (pubSz > 1) ? ("+Z1~" + pubSz) : "+Z1";
                string input = XI.AsyncInput(rd.Uid, pubDig + pubTux + ",#获得卡牌的,/T1" + rf, "SJT03", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    string[] ips = input.Split(',');
                    List<ushort> getxs = Util.TakeRange(ips, 0, ips.Length - 1).Select(p => ushort.Parse(p))
                        .Where(p => XI.Board.PZone.Contains(p)).ToList();
                    ushort to = ushort.Parse(ips[ips.Length - 1]);
                    if (getxs.Count > 0)
                    {
                        XI.RaiseGMessage("G1OU," + string.Join(",", getxs));
                        XI.RaiseGMessage("G2QU,0," + rpsa.Length + "," +
                             string.Join(",", rpsa) + "," + string.Join(",", getxs));
                        XI.RaiseGMessage("G0HQ,2," + to + ",0," + rpsa.Length + "," +
                            string.Join(",", rpsa) + "," + string.Join(",", getxs));
                        foreach (ushort getx in getxs)
                            pops.Remove(getx);
                    }
                }
                XI.RaiseGMessage("G2FU,3");
            } while (pops.Count > 0);
            
            pops = XI.DequeueOfPile(XI.Board.TuxPiles, on).ToList();
            XI.RaiseGMessage("G2IN,0," + on);
            XI.RaiseGMessage("G1IU," + string.Join(",", pops));
            do
            {
                XI.RaiseGMessage("G2FU,0," + od.Uid + "," + ops.Count + "," + og + ",C," + string.Join(",", pops));
                string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                int pubSz = XI.Board.PZone.Count;
                string pubDig = (pubSz > 1) ? ("+Z1~" + pubSz) : "+Z1";
                string input = XI.AsyncInput(od.Uid, pubDig + pubTux + ",#获得卡牌的,/T1" + of, "SJT03", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    string[] ips = input.Split(',');
                    List<ushort> getxs = Util.TakeRange(ips, 0, ips.Length - 1).Select(p => ushort.Parse(p))
                        .Where(p => XI.Board.PZone.Contains(p)).ToList();
                    ushort to = ushort.Parse(ips[ips.Length - 1]);
                    if (getxs.Count > 0)
                    {
                        XI.RaiseGMessage("G1OU," + string.Join(",", getxs));
                        XI.RaiseGMessage("G2QU,0," + opsa.Length + "," +
                             string.Join(",", opsa) + "," + string.Join(",", getxs));
                        XI.RaiseGMessage("G0HQ,2," + to + ",0," + opsa.Length + "," +
                            string.Join(",", opsa) + "," + string.Join(",", getxs));
                        foreach (ushort getx in getxs)
                            pops.Remove(getx);
                    }
                }
                XI.RaiseGMessage("G2FU,3");
            } while (pops.Count > 0);
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
                    string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                    string input = XI.AsyncInput(py.Uid, "+Z1" + pubTux + ",#获得卡牌的,/T1" + ranges, "SJT04", "0");
                    if (!input.StartsWith("/"))
                    {
                        string[] ips = input.Split(',');
                        ushort cd;
                        if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                        {
                            ushort ut = ushort.Parse(ips[1]);
                            XI.RaiseGMessage("G1OU," + cd);
                            uts.Remove(cd);
                            XI.RaiseGMessage("G2QU,0,0," + cd);
                            // CongQIPaiDuiLiQiDiao
                            XI.RaiseGMessage("G2FU,3");
                            XI.RaiseGMessage("G2CN,0,1");
                            XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                            XI.Board.TuxDises.Remove(cd);
                            string os = XI.AsyncInput(ut, "#您是否要立即装备？##是##否,Y2", "SJT04", "0");
                            if (os == "1")
                                XI.RaiseGMessage("G0ZB," + ut + ",0," + cd);
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
            string result = Util.SParal(XI.Board, p => p.IsAlive &&
                p.Tux.Count > 0, p => p.Uid + ",1,1", ",");
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH," + result);

            int sumTR = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
            int sumTO = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam)
                .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                .Sum(q => XI.LibTuple.ML.Decode(q).STR));

            if (sumTR > sumTO)
                result = Util.SParal(XI.Board, p => p.IsAlive &&
                    p.Team == rd.OppTeam, p => p.Uid + ",0,1", ",");
            else if (sumTR < sumTO)
                result = Util.SParal(XI.Board, p => p.IsAlive &&
                    p.Team == rd.Team, p => p.Uid + ",0,1", ",");
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
            XI.RaiseGMessage("G0YM,3," + pop + ",0");
            XI.RaiseGMessage("G1NI," + rd.Uid + "," + pop);
            XI.Board.Monster1 = pop;
            UEchoCode r5ed = XI.HandleWithNPCEffect(py, npc, false);
            if (r5ed == UEchoCode.NO_OPTIONS)
                XI.AsyncInput(rd.Uid, "//", "SJT07", "1");
            if (r5ed == UEchoCode.END_ACTION)
                XI.RaiseGMessage("G1YP," + XI.Board.Rounder.Uid + "," + pop);
            
            if (XI.Board.Monster1 != 0) // In case the NPC has been taken away
            {
                XI.Board.Monster1 = 0;
                XI.RaiseGMessage("G0ON,0,M,1," + pop);
            }
            XI.RaiseGMessage("G0YM,3,0,0");
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
                    new Artiad.Cure(p.Uid, 0, FiveElement.A, 3 - p.Tux.Count)).ToList();
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
                XI.RaiseGMessage("G0YM,3," + pop + ",0");
                XI.RaiseGMessage("G1NI," + rd.Uid + "," + pop);
                int sr = npc.STR < 5 ? npc.STR : 5;
                if (rd.Tux.Count > sr)
                    XI.RaiseGMessage("G0DH," + rd.Uid + ",1," + (rd.Tux.Count - sr));
                else if (rd.Tux.Count < sr)
                    XI.RaiseGMessage("G0DH," + rd.Uid + ",0," + (sr - rd.Tux.Count));
                //XI.Board.RestNPCDises.Add(pop);
                XI.RaiseGMessage("G0ON,0,M,1," + pop);
                XI.RaiseGMessage("G0YM,3,0,0");
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
                        string c0 = Util.RepeatString("p0", XI.Board.Garden[losser.Uid].Tux.Count);
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
            int sumMe = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
            int sumOe = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == rd.OppTeam)
                .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                .Sum(q => XI.LibTuple.ML.Decode(q).STR));

            if (sumMe == sumOe) return;
            Player py = sumMe < sumOe ? rd : XI.Board.GetOpponenet(rd);
            string whoStr = XI.AsyncInput(py.Uid, "#执行翻怪,/T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")", "SJT14", "0");
            if (!whoStr.Contains(VI.CinSentinel) && !whoStr.StartsWith("/"))
            {
                ushort who = ushort.Parse(whoStr);
                XI.RaiseGMessage("G2YS,T," + rd.Uid + ",T," + who);
                // show the monster
                ushort pop = XI.Board.RestMonPiles.Dequeue();
                Monster mon = XI.LibTuple.ML.Decode(pop);
                XI.RaiseGMessage("G0YM,5," + pop);
                int str = mon.STR;
                string toStr = XI.AsyncInput(who, "#获得此怪物,T1(p" + string.Join("p",
                    XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")", "SJT14", "1");
                if (!toStr.Contains(VI.CinSentinel))
                {
                    ushort to = ushort.Parse(toStr);
                    XI.RaiseGMessage("G0HC,1," + to + ",0,1," + pop);
                    if (str > 0)
                        XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(who, 0, FiveElement.YIN, str, 0)));
                }
            }
        }
        public void SJT17(Player rd)
        {
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.Runes.Count > 0)
                    XI.RaiseGMessage("G0OF," + py.Uid + "," + string.Join(",", py.Runes));
            }
            ushort[] map = new ushort[] { 0, 1, 2, 3, 4, 5, 6 };
            foreach (ushort ut in XI.Board.OrderedPlayer(rd.Uid))
            {
                XI.RaiseGMessage("G0TT," + ut);
                int result = XI.Board.DiceValue;
                XI.RaiseGMessage("G0IF," + ut + "," + map[result]);
            }
        }
        #endregion Package 6#
            #region Holiday
        public void SJH01(Player rd)
        {
            XI.AsyncInput(rd.Uid, "//", "SJH01", "0");
            XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "ED");
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

            if (XI.Board.Garden.Values.Any(p => p.Escue.Count > 0))
            {
                string g2ol = string.Join(",", XI.Board.Garden.Values.Where(p => p.Escue.Count > 0)
                    .Select(p => string.Join(",", p.Escue.Select(q => p.Uid + "," + q))));
                XI.RaiseGMessage("G2OL," + g2ol);
                string g0on = string.Join(",", XI.Board.Garden.Values.Where(p => p.Escue.Count > 0)
                    .Select(p => p.Uid + ",M," + p.Escue.Count + "," + string.Join(",", p.Escue)));
                XI.RaiseGMessage("G0ON," + g0on);
                XI.Board.Garden.Values.ToList().ForEach(p => p.Escue.Clear());
            }
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.Runes.Count > 0)
                    XI.RaiseGMessage("G0OF," + py.Uid + "," + string.Join(",", py.Runes));
            }
        }

        public void SJH04(Player rd)
        {
            int n = rd.GetPetCount();
            bool done = false;
            while (!done)
            {
                List<Player> commer = XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > n).ToList();
                string selT = XI.AsyncInput(rd.Uid, commer.Count > 0 ? ("/T1(p" + string.Join("p",
                    commer.Select(p => p.Uid)) + ")") : "/", "SJH04", "0");
                if (!selT.StartsWith("/") && !selT.Contains(VI.CinSentinel))
                {
                    ushort st = ushort.Parse(selT);
                    Player py = XI.Board.Garden[st];

                    string selM = XI.AsyncInput(rd.Uid, "/M1(p" + string.Join(
                        "p", py.Pets.Where(p => p != 0)) + ")", "SJH04", "1");
                    if (!selM.StartsWith("/") && !selM.Contains(VI.CinSentinel))
                    {
                        ushort sm = ushort.Parse(selM);
                        Monster pet = XI.LibTuple.ML.Decode(sm);
                        XI.RaiseGMessage("G0TT," + rd.Uid);
                        if (XI.Board.DiceValue + rd.STR > pet.STR) {
                            XI.RaiseGMessage("G0HL," + st + "," + sm);
                            XI.RaiseGMessage("G0ON," + st + ",M,1," + sm);
                        } else {
                            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                                new Artiad.Harm(rd.Uid, (sm + 1000), pet.Element, n + 2, 0)));
                        }
                        done = true;
                    }
                }
                else if (selT.StartsWith("/"))
                {
                    if (n > 0)
                        XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(rd.Uid, 0, FiveElement.A, n, 0)));
                    done = true;
                }
            }
        }

        public void SJH05(Player rd)
        {
            if (rd.Tux.Count > 0)
            {
                Player nx = XI.Board.GetOpponenet(rd);
                XI.RaiseGMessage("G2FU,0," + nx.Uid + ",0,C," + string.Join(",", rd.Tux));
                string select = XI.AsyncInput(nx.Uid, "C1(p" + string.Join("p", rd.Tux) + ")", "SJH05", "0");
                if (!select.Contains(VI.CinSentinel))
                {
                    ushort ut = ushort.Parse(select);
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    if (tux.Type == Tux.TuxType.ZP || new string[] { "TP01", "TP03", "TPT1", "TPT3", "TPH4" }.Contains(tux.Code))
                        XI.RaiseGMessage("G0QZ," + rd.Uid + "," + ut);
                    else if (tux.IsTuxEqiup())
                    {
                        string tarStr = XI.AsyncInput(nx.Uid, "#装备的,T1(p" + string.Join("p",
                            XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")", "SJH05", "1");
                        if (tarStr != VI.CinSentinel)
                        {
                            ushort tar = ushort.Parse(tarStr);
                            if (tar != rd.Uid)
                                XI.RaiseGMessage("G0ZB," + tar + ",1," + rd.Uid +
                                    ",0," + rd.Uid + "," + ut);
                            else
                                XI.RaiseGMessage("G0ZB," + rd.Uid + ",0," + ut);
                        }
                    }
                    else
                    {
                        string tarStr = XI.AsyncInput(nx.Uid, "#成为此牌使用者的,T1(p" + string.Join("p",
                            XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")", "SJH05", "1");
                        XI.RaiseGMessage("G0CC," + rd.Uid + ",0," + tarStr + "," + tux.Code + "," + ut + ";0,G1EV," + rd.Uid);
                    }
                }
            }
        }
        public void SJH06(Player rd)
        {
            foreach (ushort ut in XI.Board.OrderedPlayer())
            {
                if (rd.Tux.Count == 0) { break; }
                Player py = XI.Board.Garden[ut];
                if (ut == rd.Uid || !py.IsAlive) { continue; }
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
            foreach (ushort ut in XI.Board.OrderedPlayer())
            {
                Player py = XI.Board.Garden[ut];
                if (ut == rd.Uid || !py.IsAlive || py.GetAllCardsCount() == 0) { continue; }
                string second = XI.AsyncInput(rd.Uid, string.Format("#获得{0}的,C1(p{1})",
                    XI.DisplayPlayer(ut), string.Join("p", py.ListOutAllCardsWithEncrypt())), "SJH07", "0");
                ushort card = ushort.Parse(second);
                if (card == 0)
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + ut + ",2,1");
                else
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + ut + ",0,1," + card);
                ++count;
            }
            if (count > 0)
                Harm(null, rd, count);
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
            XI.RaiseGMessage("G0HD,1,2,0,15");
            XI.RaiseGMessage("G0HD,1,4,0,6");
            XI.RaiseGMessage("G0HD,1,6,0,17");
        }
    }
}
