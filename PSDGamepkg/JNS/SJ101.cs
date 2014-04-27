using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg.JNS
{
    public class EveCottage
    {
        private Base.VW.IVI VI { set; get; }
        //private VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public EveCottage(XI XI, Base.VW.IVI vi)
        {
            this.XI = XI; this.VI = vi;
        }
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
                    eve.Action += delegate() { method.Invoke(ec, new object[] { }); };

                var methodPers = ec.GetType().GetMethod(cardCode + "Pers");
                if (methodPers != null)
                    eve.Pers += new Evenement.ActionDelegate(delegate()
                    { methodPers.Invoke(ec, new object[] { }); });
                var methodPersValid = ec.GetType().GetMethod(cardCode + "PersValid");
                if (methodPersValid != null)
                    eve.PersValid += new Evenement.ValidDelegate(delegate()
                        { return (bool)methodPersValid.Invoke(ec, new object[] { }); });
            }
            return ev01;
        }
        #region Eve Of Pal1
        public void SJ101()
        {
            Player rd = XI.Board.Rounder;
            if (rd.Gender == 'M')
            {
                XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
                XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(rd.Uid, 0, FiveElement.THUNDER, 1, 0)));
                XI.VI.Cout(0, "SJ101,0");
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
        public void SJ102()
        {
            string msg = Util.SParal(XI.Board, p => p.IsAlive && p.GetPetCount() == 0,
                p => p.Uid + ",0,1", ",");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void SJ103()
        {
            Player rd = XI.Board.Rounder;
            string range1 = Util.SSelect(XI.Board, p => p.Team == rd.Team && p.IsAlive);
            string range2 = Util.SSelect(XI.Board, p => p.Team == rd.OppTeam && p.IsAlive);
            string input = XI.AsyncInput(rd.Uid, "#获得2张手牌的,T1" + range1 + ",T1" + range2, "SJ103", "0");
            string[] ips = input.Split(',');
            XI.RaiseGMessage("G0DH," + ips[0] + ",0,2," + ips[1] + ",0,2");
        }
        public void SJ104()
        {
            Player rd = XI.Board.Rounder, nx = XI.Board.Opponent;
            List<ushort> pops = XI.DequeueOfPile(XI.Board.TuxPiles, 4).ToList();
            XI.RaiseGMessage("G2IN,0,4");
            XI.RaiseGMessage("G1IU," + string.Join(",", pops));
            //XI.RaiseGMessage("G2FU,0," + string.Join(",", pops));
            
            string range1 = Util.SSelect(XI.Board, p => p.Team == rd.Team && p.IsAlive);
            string range2 = Util.SSelect(XI.Board, p => p.Team == rd.OppTeam && p.IsAlive);

            ushort[] uds = { rd.Uid, nx.Uid };
            string[] ranges = { Util.SSelect(XI.Board, p => p.Team == rd.Team && p.IsAlive),
                        Util.SSelect(XI.Board, p => p.Team == rd.OppTeam && p.IsAlive) };
            string input; string[] ips;
            int idxs = 1;

            do 
            {
                //XI.RaiseGMessage("G2FU,1,1," + uds[idxs] + "," + string.Join(",", pops));
                XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0," + string.Join(",", pops));
                string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                input = XI.AsyncInput(uds[idxs], "+Z1" + pubTux + ",#获得卡牌的,/T1" + ranges[idxs], "SJ104", "0");
                if (!input.StartsWith("/"))
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
        public void SJ201()
        {
            string msg = Util.SParal(XI.Board, p => p.IsAlive && p.HP <= 3,
                p => p.Uid + ",0,1", ",");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }

        public void SJ202()
        {
            string range = Util.SSelect(XI.Board, p => p.IsAlive && p.HP >= 2 && p.Team == XI.Board.Rounder.Team);
            if (range != null)
            {
                string input = XI.AsyncInput(XI.Board.Rounder.Uid,"#HP将为1的,T1" + range, "SJ202", "0");
                ushort target = ushort.Parse(input);
                Player targetPy = XI.Board.Garden[target];
                int dHp = targetPy.HP - 1;
                int maskDuel = Artiad.IntHelper.SetMask(0, GiftMask.INCOUNTABLE, true);
                XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(target, 0, FiveElement.SOL, dHp, maskDuel)));
            }
            string msg = Util.SParal(XI.Board, p => p.IsAlive && p.Team.Equals(XI.Board.Rounder.Team),
                p => p.Uid + ",0,1", ",");
            if (msg != null)
                XI.RaiseGMessage("G0DH," + msg);
        }
        #endregion Eve Of Pal2
        #region Eve Of Pal3

        public void SJ301()
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
        public void SJ302()
        {
            int minHp = XI.Board.Garden.Values.Where(p => p.IsAlive).Min(p => p.HP);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.HP == minHp).Select(p => new Artiad.Cure(p.Uid, 0, FiveElement.A, 2))));
        }
        public void SJ303()
        {
            int countOfPet = XI.Board.Rounder.GetPetCount();
            if (countOfPet > 0)
                XI.RaiseGMessage("G0DH," + XI.Board.Rounder.Uid + ",1," + countOfPet);
            XI.RaiseGMessage("G0DH," + XI.Board.Rounder.Uid + ",0,1");
        }
        #endregion Eve Of Pal3
        #region Eve Of Pal3A
        public void S3W01()
        {
            int maxSTR = XI.Board.Garden.Values.Where(p => p.IsAlive).Max(p => p.STR);
            string msgP = Util.SParal(XI.Board, p => p.IsAlive && p.STR == maxSTR,
                p => p.Uid + ",1,2", ",");

            int minSTR = XI.Board.Garden.Values.Where(p => p.IsAlive).Min(p => p.STR);
            string msgN = Util.SParal(XI.Board, p => p.IsAlive && p.STR == minSTR,
                p => p.Uid + ",0,2", ",");

            XI.RaiseGMessage("G0DH," + msgP + "," + msgN);
        }

        public void S3W02()
        {
            int countOfPet = XI.Board.Garden.Values.Where(
                p => p.Team == XI.Board.Rounder.OppTeam).Sum(p => p.GetPetCount());
            if (countOfPet > 0)
            {
                string range = Util.SSelect(XI.Board, p => p.IsAlive && p.Team == XI.Board.Rounder.Team);
                string input = XI.AsyncInput(XI.Board.Rounder.Uid, "#回复HP的,T1" + range, "S3W02", "0");
                ushort target = ushort.Parse(input);
                XI.RaiseGMessage(Artiad.Cure.ToMessage(new Artiad.Cure(target, 0, FiveElement.A, countOfPet)));
            }
        }
        #endregion Eve Of Pal3A
        #region Eve Of Pal4
        public void SJ401()
        {
            Player rd = XI.Board.Rounder;
            if (rd.Tux.Count > 0)
                XI.RaiseGMessage("G0DH," + rd.Uid + ",2," + rd.Tux.Count);
            XI.RaiseGMessage("G0DH," + rd.Uid + ",0,2");
        }

        public void SJ402()
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
        public void SJ501()
        {
            int maxTux = XI.Board.Garden.Values.Where(p => p.IsAlive).Max(p => p.Tux.Count);
            var v = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count == maxTux);
            if (v.Count() == 1)
                XI.RaiseGMessage("G0DS," + v.Single().Uid + ",0,1");
        }
        #endregion Eve Of Pal5

        #region Package 5#
        public void SJT01()
        {
            Player rd = XI.Board.Rounder;
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
        public void SJT02()
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
        public void SJT03()
        {
            Player rd = XI.Board.Rounder, od = XI.Board.Opponent;
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
                XI.RaiseGMessage("G2FU,0," + rd.Uid + "," + rps.Count + "," + rg + "," + string.Join(",", pops));
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
                XI.RaiseGMessage("G2FU,0," + od.Uid + "," + ops.Count + "," + og + "," + string.Join(",", pops));
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
        public void SJT04()
        {
            int sumMe = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
            int sumOe = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam)
                .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                .Sum(q => XI.LibTuple.ML.Decode(q).STR));

            Player py;
            if (sumMe < sumOe)
                py = XI.Board.Rounder;
            else if (sumMe > sumOe)
                py = XI.Board.Opponent;
            else if (XI.Board.Rounder.Team == 1)
                py = XI.Board.Rounder;
            else
                py = XI.Board.Opponent;
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
                    XI.RaiseGMessage("G2FU,0," + py.Uid + ",0," + string.Join(",", uts));
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
        public void SJT05()
        {
            string result = Util.SParal(XI.Board, p => p.IsAlive &&
                p.Tux.Count > 0, p => p.Uid + ",1,1", ",");
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH," + result);

            int sumTR = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
            int sumTO = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam)
                .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                .Sum(q => XI.LibTuple.ML.Decode(q).STR));

            if (sumTR > sumTO)
                result = Util.SParal(XI.Board, p => p.IsAlive &&
                    p.Team == XI.Board.Rounder.OppTeam, p => p.Uid + ",0,1", ",");
            else if (sumTR < sumTO)
                result = Util.SParal(XI.Board, p => p.IsAlive &&
                    p.Team == XI.Board.Rounder.Team, p => p.Uid + ",0,1", ",");
            else
                result = XI.Board.Rounder.Uid + ",0,1";
            XI.RaiseGMessage("G0DH," + result);
        }
        public void SJT06()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.GetPetCount() == 0).ToList();
            if (invs.Any())
                XI.RaiseGMessage("G1XR,1,0,2," + string.Join(",", invs.Select(p => p.Uid)));
        }
        public void SJT07()
        {
            var gv = XI.Board.Garden.Values;
            int lr = gv.Count(p => p.IsAlive && p.Team == XI.Board.Rounder.Team);
            int or = gv.Count(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam);
            Player py = (lr <= or) ? XI.Board.Rounder : XI.Board.Opponent;

            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
            XI.RaiseGMessage("G0YM,3," + pop + ",0");

            UEchoCode r5ed = XI.HandleWithNPCEffect(py, npc, false);
            //XI.Board.RestNPCDises.Add(pop);
            XI.RaiseGMessage("G0ON,0,M,1," + pop);
            XI.RaiseGMessage("G0YM,3,0,0");
        }
        public void SJT08()
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
        public void SJT09()
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
        public void SJT10()
        {
            if (XI.Board.RestNPCPiles.Count > 0)
            {
                ushort pop = XI.Board.RestNPCPiles.Dequeue();
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
                XI.RaiseGMessage("G0YM,3," + pop + ",0");
                Player rd = XI.Board.Rounder;
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

        public void SJ001()
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(XI.Board.Garden.Values.Where(p => p.IsAlive)
                .Select(p => new Artiad.Harm(p.Uid, 0, FiveElement.AQUA, 12, 0))));
        }

        public void SJ002()
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(1, 0, FiveElement.SATURN, 24, 0)));
        }

        public void SJ003()
        {
            XI.RaiseGMessage("G0HD,1,2,0,15");
            XI.RaiseGMessage("G0HD,1,4,0,6");
            XI.RaiseGMessage("G0HD,1,6,0,17");
        }
    }
}
