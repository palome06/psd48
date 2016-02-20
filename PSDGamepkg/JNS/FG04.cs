using System.Collections.Generic;
using System.Linq;
using PSD.Base.Card;
using PSD.Base;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.JNS
{
    public class MonsterCottage : JNSBase
    {
        public MonsterCottage(XI xi, Base.VW.IVI vi) : base(xi, vi) { }

        public IDictionary<string, Monster> RegisterDelegates(MonsterLib lib)
        {
            MonsterCottage mc = this;
            IDictionary<string, Monster> mt01 = new Dictionary<string, Monster>();
            foreach (Monster mon in lib.Firsts)
            {
                string monCode = mon.Code;
                mt01.Add(monCode, mon);
                var methodDebut = mc.GetType().GetMethod(monCode + "Debut");
                if (methodDebut != null)
                    mon.Debut += new Monster.DebutDelegate(delegate ()
                    {
                        methodDebut.Invoke(mc, new object[] { });
                    });
                var methodCurtain = mc.GetType().GetMethod(monCode + "Curtain");
                if (methodCurtain != null)
                    mon.Curtain += new Monster.DebutDelegate(delegate ()
                    {
                        methodCurtain.Invoke(mc, new object[] { });
                    });
                var methodWin = mc.GetType().GetMethod(monCode + "WinEff");
                if (methodWin != null)
                    mon.WinEff += new Monster.WLDelegate(delegate ()
                    {
                        methodWin.Invoke(mc, new object[] { });
                    });
                var methodLose = mc.GetType().GetMethod(monCode + "LoseEff");
                if (methodLose != null)
                    mon.LoseEff += new Monster.WLDelegate(delegate ()
                    {
                        methodLose.Invoke(mc, new object[] { });
                    });
                var methodConsumeAction = mc.GetType().GetMethod(monCode + "ConsumeAction");
                if (methodConsumeAction != null)
                    mon.ConsumeAction += new Monster.CsActionDelegate(delegate (Player player, int consumeType, int type, string fuse, string argst)
                    {
                        methodConsumeAction.Invoke(mc, new object[] { player, consumeType, type, fuse, argst });
                    });
                var methodIncrAction = mc.GetType().GetMethod(monCode + "IncrAction");
                if (methodIncrAction != null)
                    mon.IncrAction += new Monster.CrActionDelegate(delegate (Player player)
                    {
                        methodIncrAction.Invoke(mc, new object[] { player });
                    });
                var methodDecrAction = mc.GetType().GetMethod(monCode + "DecrAction");
                if (methodDecrAction != null)
                    mon.DecrAction += new Monster.CrActionDelegate(delegate (Player player)
                    {
                        methodDecrAction.Invoke(mc, new object[] { player });
                    });
                var methodConsumeValid = mc.GetType().GetMethod(monCode + "ConsumeValid");
                if (methodConsumeValid != null)
                    mon.ConsumeValid += new Monster.CsValidDelegate(delegate (Player player, int consumeType, int type, string fuse)
                    {
                        return (bool)methodConsumeValid.Invoke(mc, new object[] { player, consumeType, type, fuse });
                    });
                var methodConsumeInput = mc.GetType().GetMethod(monCode + "ConsumeInput");
                if (methodConsumeInput != null)
                    mon.ConsumeInput += new Monster.CsInputDelegate(delegate (Player player, int consumeType, int type, string fuse, string prev)
                    {
                        return (string)methodConsumeInput.Invoke(mc, new object[] { player, consumeType, type, fuse, prev });
                    });
            }
            return mt01;
        }

        #region Aqua
        public void GS01Debut()
        {
            Player rd = XI.Board.Rounder, hd = XI.Board.Hinder;
            if (hd != null && hd.IsAlive)
            {
                XI.RaiseGMessage("G0HQ,4," + rd.Uid + "," + hd.Uid + "," + rd.Tux.Count + "," + hd.Tux.Count +
                    (rd.Tux.Count > 0 ? ("," + string.Join(",", rd.Tux)) : "") +
                    (hd.Tux.Count > 0 ? ("," + string.Join(",", hd.Tux)) : ""));
            }
        }
        public void GS01LoseEff()
        {
            XI.RaiseGMessage("G0DS," + XI.Board.Rounder.Uid + ",0,1");
        }
        public void GS01IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GS01DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }

        public void GS02WinEff()
        {
            Player rd = XI.Board.Rounder;
            var vl = XI.Board.Garden.Values;

            Harm("GS02", vl.Where(p => p.IsAlive && p.Team == rd.OppTeam), 1);
            string input = XI.AsyncInput(rd.Uid, "#额外HP-2,T1(p" + string.Join("p", vl.Where(
                p => p.IsAlive && p.Team == rd.OppTeam).Select(p => p.Uid)) + ")", "GS02WinEff", "0");
            ushort who = ushort.Parse(input);
            Harm("GS02", XI.Board.Garden[who], 2);
        }
        public void GS02LoseEff()
        {
            Player rd = XI.Board.Rounder;
            Harm("GS02", rd, 2);
            if (rd.GetEquipCount() > 0)
            {
                string input = XI.AsyncInput(rd.Uid, "#须弃置的,Q1(p" + string.Join("p",
                    rd.ListOutAllEquips()) + ")", "GS02LoseEff", "0");
                ushort which = ushort.Parse(input);
                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + which);
            }
        }
        public void GS02ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                ushort gs02 = XI.LibTuple.ML.Encode("GS02");
                if (gs02 != 0)
                {
                    Monster monster = XI.LibTuple.ML.Decode(gs02);
                    int delta = 8 - monster.STRb;
                    if (delta > 0)
                        XI.RaiseGMessage("G0IB," + gs02 + "," + delta);
                }
            }
        }
        public void GS03WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GS03", XI.Board.Hinder, 4);
        }
        public void GS03LoseEff()
        {
            Harm("GS03", XI.Board.Rounder, 4);
        }
        public void GS04WinEff()
        {
            Player rd = XI.Board.Rounder, hd = XI.Board.Hinder;
            var vl = XI.Board.Garden.Values;
            Harm("GS04", vl.Where(p => p.IsAlive && p.Team == rd.OppTeam), 1);
            if (hd.IsValidPlayer() && hd.HasAnyCards())
            {
                string second = XI.AsyncInput(rd.Uid, string.Format("#获得{0}的,C1(p{1})",
                    XI.DisplayPlayer(hd.Uid), string.Join("p", hd.ListOutAllCardsWithEncrypt())),
                    "GS04WinEff", "0");
                ushort card = ushort.Parse(second);
                if (card == 0)
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + hd.Uid + ",2,1");
                else
                    XI.RaiseGMessage("G0HQ,0," + rd.Uid + "," + hd.Uid + ",0,1," + card);
            }
        }
        public void GS04LoseEff()
        {
            Player rd = XI.Board.Rounder, hd = XI.Board.Hinder;
            Harm("GS04", rd, 2);
            if (hd.IsValidPlayer() && rd.IsAlive && rd.Uid != 0 && rd.HasAnyCards())
            {
                string second = XI.AsyncInput(hd.Uid, string.Format("#获得{0}的,C1(p{1})",
                    XI.DisplayPlayer(rd.Uid), string.Join("p", rd.ListOutAllCardsWithEncrypt())),
                    "GS04LoseEff", "0");
                ushort card = ushort.Parse(second);
                if (card == 0)
                    XI.RaiseGMessage("G0HQ,0," + hd.Uid + "," + rd.Uid + ",2,1");
                else
                    XI.RaiseGMessage("G0HQ,0," + hd.Uid + "," + rd.Uid + ",0,1," + card);
            }
        }
        public void GS04IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GS04DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        #endregion Aqua

        #region Agni
        public void GH01WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GH01", XI.Board.Hinder, 2);
        }
        public void GH01LoseEff()
        {
            Harm("GH01", XI.Board.Rounder, 3);
        }
        public void GH01IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GH01DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }

        public void GH02WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GH02", XI.Board.Hinder, 3);
        }
        public void GH02LoseEff()
        {
            Harm("GH02", XI.Board.Rounder, 3);
        }
        public void GH02ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
                XI.RaiseGMessage("G0IP," + player.Team + ",3");
        }

        public void GH03Debut()
        {
            if (XI.Board.Supporter.IsValidPlayer())
            {
                int n = XI.Board.Rounder.STR - 1;
                if (n > 0)
                    Harm("GH03", XI.Board.Supporter, n);
            }
        }
        public void GH03WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GH03", XI.Board.Hinder, 3);
        }
        public void GH03LoseEff()
        {
            Player op = XI.Board.Opponent;
            var pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team);
            if (pys.Count() >= 2)
            {
                string whostr = XI.AsyncInput(op.Uid, "#受到3点火属性伤害,T2(p" +
                    string.Join("p", pys.Select(p => p.Uid)) + ")", "GH03LoseEff", "0");
                int idx = whostr.IndexOf(',');
                if (whostr != XI.VI.CinSentinel)
                {
                    ushort p1 = ushort.Parse(whostr.Substring(0, idx));
                    ushort p2 = ushort.Parse(whostr.Substring(idx + 1));
                    Harm("GH03", new Player[] { XI.Board.Garden[p1], XI.Board.Garden[p2] }, 3);
                }
            }
            else if (pys.Count() == 1)
            {
                string whostr = XI.AsyncInput(op.Uid, "#受到3点火属性伤害,T1(p" +
                    string.Join("p", pys.Select(p => p.Uid)) + ")", "GH03LoseEff", "0");
                if (whostr != XI.VI.CinSentinel)
                {
                    ushort p1 = ushort.Parse(whostr);
                    Harm("GH03", XI.Board.Garden[p1], 3);
                }
            }
        }
        public void GH03IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,2");
        }
        public void GH03DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,2");
        }

        public void GH04Debut()
        {
            Harm("GH04", XI.Board.Garden.Values.Where(p => p.IsAlive), 2);
        }
        public void GH04WinEff()
        {
            Harm("GH04", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam), 2);
        }
        public void GH04LoseEff()
        {
            if (XI.Board.Supporter.IsValidPlayer())
                Harm("GH04", new Player[] { XI.Board.Rounder, XI.Board.Supporter }, 2);
            else
                Harm("GH04", XI.Board.Rounder, 2);
        }
        public void GH04IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,2");
        }
        public void GH04DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,2");
        }
        #endregion Agni

        #region Thunder
        public void GL01WinEff()
        {
            string whostr = XI.AsyncInput(XI.Board.Rounder.Uid, "#HP+2,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")", "GL01WinEff", "0");
            ushort who = ushort.Parse(whostr);
            Cure("GL01", XI.Board.Garden[who], 2);
        }
        public void GL01LoseEff()
        {
            Harm("GL01", XI.Board.Rounder, 3);
        }

        public void GL02Debut()
        {
            if (XI.Board.Supporter.IsValidPlayer())
                XI.RaiseGMessage("G0IA," + XI.Board.Supporter.Uid + ",1,2");
        }
        public void GL02WinEff()
        {
            string whostr = XI.AsyncInput(XI.Board.Rounder.Uid, "#获得2张牌,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")", "GL02WinEff", "0");
            ushort who = ushort.Parse(whostr);
            XI.RaiseGMessage("G0DH," + who + ",0,2");
        }
        public void GL02LoseEff()
        {
            var rd = XI.Board.Rounder;
            Harm("GL02", rd, 2);
            List<ushort> loses = rd.ListOutAllEquips();
            if (loses.Count > 0)
            {
                int count = loses.Count;
                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + string.Join(",", loses));
                XI.RaiseGMessage("G0DH," + rd.Uid + ",0," + count);
            }
        }
        public void GL02ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (ut == player.Uid && n > 0)
                        g0ht[i + 1] = (n + 1).ToString();
                }
                XI.InnerGMessage(string.Join(",", g0ht), 51);
            }
        }
        public bool GL02ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (ut == player.Uid && n > 0)
                        return true;
                }
            }
            return false;
        }

        public void GL03WinEff()
        {
            var rd = XI.Board.Rounder;
            int txCount = rd.ListOutAllCards().Count;
            if (txCount == 1)
            {
                string cardsstr = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置以获得每张HP+2效果,/Q1(p"
                    + string.Join("p", rd.ListOutAllCards()) + ")", "GL03WinEff", "0");
                if (!string.IsNullOrEmpty(cardsstr) && cardsstr != "0" && !cardsstr.StartsWith("/"))
                {
                    XI.RaiseGMessage("G0QZ," + rd.Uid + "," + cardsstr);
                    Cure("GL03", rd, 2);
                }
            }
            else if (txCount > 1)
            {
                string cardsstr = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置以获得每张HP+2效果,/Q1~" + txCount
                    + "(p" + string.Join("p", rd.ListOutAllCards()) + ")", "GL03WinEff", "0");
                if (!string.IsNullOrEmpty(cardsstr) && cardsstr != "0" && !cardsstr.StartsWith("/"))
                {
                    string[] cards = cardsstr.Split(',');
                    XI.RaiseGMessage("G0QZ," + rd.Uid + "," + cardsstr);
                    Cure("GL03", rd, 2 * cards.Length);
                }
            }
        }
        public void GL03LoseEff()
        {
            var rd = XI.Board.Rounder;
            List<ushort> loses = rd.ListOutAllEquips();
            if (loses.Count > 0)
            {
                string cardsstr = XI.AsyncInput(XI.Board.Rounder.Uid, "#须弃置,Q1(p" +
                    string.Join("p", loses) + ")", "GL03LoseEff", "0");
                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + cardsstr);
            }
            int max = XI.Board.Garden.Values.Max(p => p.GetEquipCount());
            if (max > 0)
            {
                var kaos = XI.Board.Garden.Values.Where(p => (p.GetEquipCount() == max));
                IDictionary<ushort, string> requires = new Dictionary<ushort, string>();
                foreach (Player py in kaos)
                    requires.Add(py.Uid, "#须弃置,Q1(p" + string.Join("p", py.ListOutAllEquips()) + ")");
                IDictionary<ushort, string> inputs = XI.MultiAsyncInput(requires);
                foreach (var pair in inputs)
                    XI.RaiseGMessage("G0QZ," + pair.Key + "," + pair.Value);
            }
        }
        public void GL03IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,2");
        }
        public void GL03DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,2");
        }

        public void GL04WinEff()
        {
            string p1 = string.Join(",", XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.Team && p.GetEquipCount() > 0).Select(p => p.Uid + ",0," + p.GetEquipCount()));
            if (p1 != "")
                XI.RaiseGMessage("G0DH," + p1);
        }
        public void GL04LoseEff()
        {
            Harm("GL04", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team), 2);
        }
        public void GL04IncrAction(Player player)
        {
            string g0zl = "";
            List<Player> involved = new List<Player>();
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.Team == player.OppTeam && !py.WeaponDisabled)
                {
                    involved.Add(py);
                    List<ushort> equips = py.ListOutAllEquips().Where(
                        p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.WQ).ToList();
                    if (equips.Count > 0)
                        g0zl += "," + string.Join(",", equips.Select(p => py.Uid + "," + p));
                }
            }
            if (g0zl.Length > 0)
                XI.RaiseGMessage("G0ZL" + g0zl);
            foreach (Player py in involved)
                py.SetWeaponDisabled("GL04", true);
        }
        public void GL04DecrAction(Player player)
        {
            string g0zs = "";
            List<Player> involved = new List<Player>();
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.Team == player.OppTeam && py.WeaponDisabled)
                {
                    involved.Add(py);
                    List<ushort> equips = py.ListOutAllEquips().Where(
                        p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.WQ).ToList();
                    if (equips.Count > 0)
                        g0zs += "," + string.Join(",", equips.Select(p => py.Uid + "," + p));
                }
            }
            if (g0zs.Length > 0)
                XI.RaiseGMessage("G0ZS" + g0zs);
            foreach (Player py in involved)
                py.SetWeaponDisabled("GL04", false);
        }
        public void GL04ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length;)
                {
                    int gtype = int.Parse(blocks[i]);
                    ushort ut = ushort.Parse(blocks[i + 1]);
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.OppTeam)
                        XI.Board.Garden[ut].SetWeaponDisabled("GL04", true);
                    if (gtype == 0 || gtype == 1)
                        i += 3;
                    else if (gtype == 2)
                        i += 4;
                    else
                        break;
                }
                //XI.InnerGMessage(fuse, 51);
            }
        }
        public bool GL04ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length;)
                {
                    int gtype = int.Parse(blocks[i]);
                    ushort ut = ushort.Parse(blocks[i + 1]);
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.OppTeam)
                        return true;
                    if (gtype == 0 || gtype == 1)
                        i += 3;
                    else if (gtype == 2)
                        i += 4;
                }
            }
            return false;
        }
        #endregion Thunder

        #region Aero
        public void GF01WinEff()
        {
            XI.RaiseGMessage("G0DH," + XI.Board.Rounder.Uid + ",0,1");
        }
        public void GF01LoseEff()
        {
            Harm("GF01", XI.Board.Rounder, 2);
        }
        public void GF02WinEff()
        {
            Cure("GF02", XI.Board.Rounder, 2);
        }
        public void GF02LoseEff()
        {
            Cure("GF02", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam), 2);
        }
        public void GF02IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GF02DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }
        public void GF03WinEff()
        {
            if (XI.Board.Supporter.IsValidPlayer())
                Cure("GF03", new Player[] { XI.Board.Rounder, XI.Board.Supporter }, 2);
            else
                Cure("GF03", XI.Board.Rounder, 2);
        }
        public void GF03LoseEff()
        {
            var pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0
                && p.Team == XI.Board.Rounder.Team).ToList();
            if (pys.Any())
                XI.RaiseGMessage("G1XR,1,0,0," + string.Join(",", pys.Select(p => p.Uid)));
            //if (pys.Any())
            //{
            //    IDictionary<ushort, int> tuxCount = new Dictionary<ushort, int>();
            //    foreach (var py in pys)
            //        tuxCount.Add(py.Uid, py.Tux.Count);
            //    XI.RaiseGMessage("G0DH," + string.Join(",", tuxCount.Select(p => p.Key + ",2," + p.Value)));
            //    XI.RaiseGMessage("G0DH," + string.Join(",", tuxCount.Select(p => p.Key + ",0," + p.Value)));
            //}
            Harm("GF03", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team), 2);
        }
        public void GF03ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                var pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0
                    && p.Team == player.Team);
                XI.RaiseGMessage("G1XR,1,0,0," + string.Join(",", pys.Select(p => p.Uid)));
                //IDictionary<ushort, int> tuxCount = new Dictionary<ushort, int>();
                //foreach (var py in pys)
                //    tuxCount.Add(py.Uid, py.Tux.Count);
                //XI.RaiseGMessage("G0DH," + string.Join(",", tuxCount.Select(p => p.Key + ",2," + p.Value)));
                //XI.RaiseGMessage("G0DH," + string.Join(",", tuxCount.Select(p => p.Key + ",0," + p.Value)));
            }
        }
        public bool GF03ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
            {
                var rd = XI.Board.Rounder;
                if (type == 0)
                    return XI.Board.Garden.Values.Where(p => p.IsAlive &&
                        p.Team == rd.Team && p.Tux.Count > 0).Any() && player.Team == rd.Team;
                else if (type == 1)
                    return XI.Board.Garden.Values.Where(p => p.IsAlive &&
                        p.Team == rd.Team && p.Tux.Count > 0).Any();
            }
            return false;
        }
        public void GF04WinEff()
        {
            Player op = XI.Board.Opponent;
            string whostr = XI.AsyncInput(op.Uid, "#HP+2,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == op.Team).Select(p => p.Uid)) + ")", "GF04WinEff", "0");
            ushort who = ushort.Parse(whostr);
            Cure("GF04", XI.Board.Garden[who], 2);
        }
        public bool GF04ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0);
        }
        public void GF04ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                List<ushort> zeros = XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
                string ic = zeros.Count > 0 ? "#复活,T1(p" + string.Join("p", zeros) + ")" : "/";
                ushort tg = ushort.Parse(XI.AsyncInput(player.Uid, ic, "GF04", "1"));

                if (zeros.Contains(tg))
                {
                    Player tgp = XI.Board.Garden[tg];
                    Cure("GF04", tgp, tgp.HPb - tgp.HP, (long)HPEvoMask.TERMIN_AT);
                }
                Artiad.Procedure.ArticuloMortis(XI, XI.WI, false);
            }
        }
        #endregion Aero

        #region Saturn
        public void GT01WinEff()
        {
            Player sd = XI.Board.Supporter;
            if (sd.IsValidPlayer())
                Harm("GT01", new Player[] { XI.Board.Rounder, sd }, 3);
            else
                Harm("GT01", XI.Board.Rounder, 3);
        }
        public void GT01LoseEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GT01", XI.Board.Hinder, 3);
        }
        public void GT02Debut()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                !XI.Board.IsAttendWar(p) && p.Tux.Count > 0).ToList();
            if (pys.Any())
                Harm("GT02", pys, pys.Select(p => p.Tux.Count).ToList());
        }
        public void GT02IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GT02DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }

        public void GT03Debut()
        {
            ushort x = XI.LibTuple.ML.Encode("GT03");
            if (x != 0)
                XI.RaiseGMessage("G0IB," + x + ",3");
        }
        public void GT03DecrAction(Player player)
        {
            ushort rk = (ushort)(1000 + XI.LibTuple.ML.Encode("GT03"));
            if (XI.Board.InFightThrough)
            {
                if (XI.Board.Supporter.Uid == rk)
                    XI.RaiseGMessage("G17F,S,0");
                else if (XI.Board.Hinder.Uid == rk)
                    XI.RaiseGMessage("G17F,H,0");
            }
        }
        public void GT03WinEff()
        {
            Harm("GT03", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam), 1);
        }
        public void GT03LoseEff()
        {
            Harm("GT03", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team), 2);
        }
        public void GT03ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                if (XI.Board.Rounder.Team == player.Team)
                    XI.Board.PosSupporters.Add("PT19");
                else
                    XI.Board.PosHinders.Add("PT19");
            }
        }

        public void GT04IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,2");
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GT04DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,2");
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GT04LoseEff()
        {
            int saturnElem = FiveElement.SATURN.Elem2Index();
            List<Player> others = XI.Board.Garden.Values.Where(p =>
                p.Team == XI.Board.Rounder.OppTeam && p.Pets[saturnElem] != 0).ToList();
            if (others.Any() && XI.Board.Mon1From == 0)
            {
                string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#要替换的,/M1(p" + string.Join("p",
                    others.Select(p => p.Pets[saturnElem])) + ")", "GT04LoseEff", "0");
                if (input != VI.CinSentinel && !input.StartsWith("/"))
                {
                    ushort mons = ushort.Parse(input);
                    ushort gt04code = XI.LibTuple.ML.Encode("GT04");
                    Player py = others.Single(p => p.Pets[saturnElem] == mons);
                    XI.RaiseGMessage(new Artiad.HarvestPet()
                    {
                        Farmer = py.Uid,
                        SinglePet = gt04code,
                        Trophy = true,
                        TreatyAct = Artiad.HarvestPet.Treaty.PASSIVE
                    }.ToMessage());
                    if (XI.Board.Monster1 == gt04code)
                        XI.Board.Monster1 = 0;
                    else if (XI.Board.Monster2 == gt04code)
                        XI.Board.Monster2 = 0;
                }
            }
        }
        #endregion Saturn

        #region Package 4#
        public void GST1Debut()
        {
            ushort ut = XI.Board.Supporter.Uid;
            if (ut > 0 && ut < 1000)
                XI.RaiseGMessage("G0OX," + ut + ",1,4");
        }
        public void GST1IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GST1DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GST1WinEff()
        {
            Player h = XI.Board.Hinder;
            if (h.IsValidPlayer())
            {
                bool opt2 = false;
                int hls = h.ListOutAllCards().Count;
                if (hls >= 2)
                {
                    string ts = XI.AsyncInput(h.Uid, "#弃置(取消则补满牌后横置),/Q" + hls + "(p" +
                        string.Join("p", h.ListOutAllCards()) + ")", "GST1WinEff", "0");
                    if (ts != "/0")
                    {
                        opt2 = true;
                        XI.RaiseGMessage("G0QZ," + h.Uid + "," + ts);
                    }
                }
                if (!opt2)
                {
                    if (h.Tux.Count < h.TuxLimit)
                        XI.RaiseGMessage("G0DH," + h.Uid + ",0," + (h.TuxLimit - h.Tux.Count));
                    XI.RaiseGMessage("G0DS," + h.Uid + ",0,1");
                }
            }
        }
        public void GST1LoseEff()
        {
            Harm("GST1", XI.Board.Rounder, 3);
        }

        public void GST2IncrAction(Player player)
        {
            //Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GST2"));
            //if (mon != null)
            //    mon.RAMUshort = 1;
            foreach (ushort ut in player.ListOutAllEquips())
            {
                var tx = XI.LibTuple.TL.DecodeTux(ut);
                if (tx.IsTuxEqiup())
                {
                    Base.Card.TuxEqiup te = tx as Base.Card.TuxEqiup;
                    if (te != null && !((te.Type == Tux.TuxType.FJ && player.ArmorDisabled) || (
                        te.Type == Tux.TuxType.WQ && player.WeaponDisabled) || (
                        te.Type == Tux.TuxType.XB && player.LuggageDisabled)))
                    {
                        if (te.IncrOfSTR > 0)
                            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                        if (te.IncrOfDEX > 0)
                            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
                    }
                }
            }
        }
        public void GST2DecrAction(Player player)
        {
            //Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GST2"));
            //if (mon != null)
            //    mon.RAMUshort = 0;
            foreach (ushort ut in player.ListOutAllEquips())
            {
                var tx = XI.LibTuple.TL.DecodeTux(ut);
                if (tx.IsTuxEqiup())
                {
                    Base.Card.TuxEqiup te = tx as Base.Card.TuxEqiup;
                    if (te != null && !((te.Type == Tux.TuxType.FJ && player.ArmorDisabled) || (
                        te.Type == Tux.TuxType.WQ && player.WeaponDisabled) || (
                        te.Type == Tux.TuxType.XB && player.LuggageDisabled)))
                    {
                        if (te.IncrOfSTR > 0)
                            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                        if (te.IncrOfDEX > 0)
                            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
                    }
                }
            }
        }
        public void GST2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0 && (type == 0 || type == 1))
            {
                // G0ZS/L,[A,y]*
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort ut = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && player.ListOutAllEquips().Contains(ut))
                    {
                        Base.Card.TuxEqiup te = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
                        if (te != null)
                        {
                            if (te.IncrOfSTR > 0)
                            {
                                if (type == 0)
                                    XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                                else
                                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                            }
                            if (te.IncrOfDEX > 0)
                            {
                                if (type == 0)
                                    XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
                                else
                                    XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
                            }
                        }
                    }
                }
                //XI.InnerGMessage(fuse, 121);
            }
        }
        public bool GST2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                if (type == 0 || type == 1)
                {
                    // G0ZS/L,[A,y]*
                    string[] blocks = fuse.Split(',');
                    for (int i = 1; i < blocks.Length; i += 2)
                    {
                        ushort who = ushort.Parse(blocks[i]);
                        if (who == player.Uid)
                            return true;
                    }
                }
            }
            return false;
        }
        public void GST2WinEff()
        {
            Player h = XI.Board.Hinder;
            if (h.IsValidPlayer())
            {
                Harm("GST2", h, (h.GetEquipCount() + 2));
                if (h.GetEquipCount() > 0)
                {
                    string ts = XI.AsyncInput(h.Uid, "#弃置的,Q1(p" + string.Join(
                        "p", h.ListOutAllEquips()) + ")", "GST2WinEff", "0");
                    ushort ut = ushort.Parse(ts);
                    XI.RaiseGMessage("G0QZ," + h.Uid + "," + ut);
                }
            }
        }
        public void GST2LoseEff()
        {
            Player r = XI.Board.Rounder;
            Harm("GST2", r, (r.GetEquipCount() + 2));
            if (r.GetEquipCount() > 0)
            {
                string ts = XI.AsyncInput(r.Uid, "#弃置的,Q1(p" + string.Join(
                    "p", r.ListOutAllEquips()) + ")", "GST2LoseEff", "0");
                ushort ut = ushort.Parse(ts);
                XI.RaiseGMessage("G0QZ," + r.Uid + "," + ut);
            }
        }

        public void GHT1Debut()
        {
            ushort x = XI.LibTuple.ML.Encode("GHT1");
            if (x != 0)
            {
                int inc = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team)
                    .Select(p => p.Pets[FiveElement.AGNI.Elem2Index()]).Where(p => p != 0 &&
                     XI.LibTuple.ML.Decode(p) != null).Sum(p => XI.LibTuple.ML.Decode(p).STR);
                if (inc > 0)
                    XI.RaiseGMessage("G0IB," + x + "," + inc);
            }
        }
        public void GHT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT1"));
                if (mon != null)
                {
                    mon.ROMUshort = (ushort)player.Team;
                    XI.RaiseGMessage("G0IP," + player.Team + ",3");
                }
            }
        }
        public bool GHT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT1"));
                return mon != null && mon.ROMUshort != player.Team;
            }
            return false;
        }
        public void GHT1WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GHT1", XI.Board.Hinder, 2);
            bool anyAgni = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team)
                    .Select(p => p.Pets[FiveElement.AGNI.Elem2Index()]).Where(p => p != 0).Any();
            if (!anyAgni)
            {
                Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                if (mon1 != null && mon1.Code == "GHT1")
                    XI.Board.Mon1Catchable = false;
                Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2 != null && mon2.Code == "GHT1")
                    XI.Board.Mon2Catchable = false;
            }
        }
        public void GHT1LoseEff()
        {
            List<Player> pys = new List<Player> { XI.Board.Rounder };
            if (XI.Board.Supporter.IsValidPlayer())
                pys.Add(XI.Board.Supporter);
            Harm("GHT1", pys, 2);
        }
        public void GHT2IncrAction(Player player)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT2"));
            if (mon != null)
            {
                if (mon.ROMUshort == 0)
                {
                    string hint = "#请选择「镇狱明王」宠物效果。##战-1，命+2##命-1，战+2,Y2";
                    string option = XI.AsyncInput(player.Uid, hint, "GHT2IncrAction", "0");
                    if (option == "1")
                        mon.ROMUshort = 1;
                    else if (option == "2")
                        mon.ROMUshort = 2;
                }
                if (mon.ROMUshort == 1)
                {
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0,2");
                }
                else if (mon.ROMUshort == 2)
                {
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0,2");
                }
            }
        }
        public void GHT2DecrAction(Player player)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT2"));
            if (mon != null)
            {
                if (mon.ROMUshort == 1)
                {
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0,2");
                }
                else if (mon.ROMUshort == 2)
                {
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,2");
                }
            }
        }
        public void GHT2WinEff()
        {
            Harm("GHT2", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam), 3);
        }
        public void GHT2LoseEff()
        {
            Harm("GHT2", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team), 3);
        }

        public void GLT1IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GLT1DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }
        public void GLT1WinEff()
        {
            Player r = XI.Board.Rounder;
            if (r.ListOutAllEquips().Count > 0)
            {
                bool done = false;
                while (!done)
                {
                    string ts = XI.AsyncInput(r.Uid, "#弃置以令任意一人补2张牌的,/Q1(p" +
                            string.Join("p", r.ListOutAllEquips()) + ")", "GSL1WinEff", "0");
                    if (ts != "/0")
                    {
                        string tr = XI.AsyncInput(r.Uid, "#获得2张牌的,/T1(p" +
                            string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid))
                            + ")", "GSL1WinEff", "0");
                        if (tr != "/0")
                        {
                            XI.RaiseGMessage("G0QZ," + r.Uid + "," + ts);
                            XI.RaiseGMessage("G0DH," + tr + ",0,2");
                            done = true;
                        }
                    }
                    else
                        done = true;
                }
            }
        }
        public void GLT1LoseEff()
        {
            Harm("GLT1", XI.Board.Rounder, 2);
        }

        public void GLT2Debut()
        {
            // Possible Case: G07F causes the new valid/invalid
            var b = XI.Board;
            string g0zl = "";
            foreach (Player py in new Player[] { b.Rounder, b.Hinder, b.Supporter })
            {
                if (py.IsValidPlayer())
                {
                    //List<ushort> cards = new List<ushort>();
                    foreach (ushort ut in py.ListOutAllEquips())
                    {
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux.Type == Tux.TuxType.WQ && !py.WeaponDisabled)
                            g0zl += "," + py.Uid + "," + ut;
                        if (tux.Type == Tux.TuxType.FJ && !py.ArmorDisabled)
                            g0zl += "," + py.Uid + "," + ut;
                    }
                    py.SetArmorDisabled("GLT2", true);
                    py.SetWeaponDisabled("GLT2", true);
                    //foreach (ushort ut in cards)
                    //{
                    //    Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    //    if (tux.Type == Tux.TuxType.WQ && !py.WeaponDisabled)
                    //        g0zl += "," + py.Uid + "," + ut;
                    //    if (tux.Type == Tux.TuxType.FJ && !py.ArmorDisabled)
                    //        g0zl += "," + py.Uid + "," + ut;
                    //}
                }
            }
            if (g0zl.Length > 0)
                XI.RaiseGMessage("G0ZL" + g0zl);
        }
        public void GLT2Curtain()
        {
            var b = XI.Board;
            string g0zs = "";
            //foreach (Player py in new Player[] { b.Rounder, b.Hinder, b.Supporter })
            foreach (Player py in b.Garden.Values)
            {
                if (py.IsValidPlayer())
                {
                    List<ushort> cards = new List<ushort>();
                    foreach (ushort ut in py.ListOutAllEquips())
                    {
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux.Type == Tux.TuxType.WQ && py.WeaponDisabled)
                            cards.Add(ut);
                        if (tux.Type == Tux.TuxType.FJ && py.ArmorDisabled)
                            cards.Add(ut);
                    }
                    py.SetArmorDisabled("GLT2", false);
                    py.SetWeaponDisabled("GLT2", false);
                    foreach (ushort ut in cards)
                    {
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux.Type == Tux.TuxType.WQ && !py.WeaponDisabled)
                            g0zs += "," + py.Uid + "," + ut;
                        if (tux.Type == Tux.TuxType.FJ && !py.ArmorDisabled)
                            g0zs += "," + py.Uid + "," + ut;
                    }
                }
            }
            if (g0zs.Length > 0)
                XI.RaiseGMessage("G0ZS" + g0zs);
        }
        public void GLT2IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GLT2DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GLT2WinEff()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.OppTeam && p.ListOutAllEquips().Count > 0).ToList();
            if (pys.Count > 0)
            {
                bool done = false;
                while (!done)
                {
                    string ts = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置装备的,/T1(p" +
                        string.Join("p", pys.Select(p => p.Uid)) + ")", "GLT2WinEff", "0");
                    if (ts != "/0")
                    {
                        ushort who = ushort.Parse(ts);
                        string tu = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置的,/C1(p" +
                            string.Join("p", XI.Board.Garden[who].ListOutAllEquips()) + ")", "GLT2WinEff", "0");
                        if (tu != "/0")
                        {
                            XI.RaiseGMessage("G0QZ," + who + "," + tu);
                            done = true;
                        }
                    }
                    else
                        done = true;
                }
            }
        }
        public void GLT2LoseEff()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.Team && p.ListOutAllEquips().Count > 0).ToList();
            if (pys.Count > 0)
            {
                bool done = false;
                while (!done)
                {
                    string ts = XI.AsyncInput(XI.Board.Opponent.Uid, "#弃置装备的,/T1(p" +
                        string.Join("p", pys.Select(p => p.Uid)) + ")", "GLT2LoseEff", "0");
                    if (ts != "/0")
                    {
                        ushort who = ushort.Parse(ts);
                        string tu = XI.AsyncInput(XI.Board.Opponent.Uid, "#弃置的,/C1(p" +
                            string.Join("p", XI.Board.Garden[who].ListOutAllEquips()) + ")", "GLT2LoseEff", "0");
                        if (tu != "/0")
                        {
                            XI.RaiseGMessage("G0QZ," + who + "," + tu);
                            done = true;
                        }
                    }
                    else
                        done = true;
                }
            }
        }

        public void GFT1Debut()
        {
            Cure("GFT1", XI.Board.Garden.Values.Where(p => p.IsAlive), 1);
        }
        public void GFT1IncrAction(Player player)
        {
            ++player.TuxLimit;
        }
        public void GFT1DecrAction(Player player)
        {
            --player.TuxLimit;
        }
        public void GFT1WinEff()
        {
            string tr = XI.AsyncInput(XI.Board.Rounder.Uid, "#HP+3,T1(p" +
                string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid))
                + ")", "GFT1WinEff", "0");
            Cure("GFT1", XI.Board.Garden[ushort.Parse(tr)], 3);
        }
        public void GFT1LoseEff()
        {
            Player r = XI.Board.Rounder, s = XI.Board.Supporter;

            if (s.IsValidPlayer())
                Harm("GFT1", new Player[] { r, s }, 2);
            else
                Harm("GFT1", r, 2);

            string g0dh = "";
            if (s.IsValidPlayer())
            {
                if (s.Tux.Count > 1)
                    g0dh += "," + s.Uid + ",1," + (s.Tux.Count - 1);
            }
            if (r.Tux.Count > 1)
                g0dh += "," + r.Uid + ",1," + (r.Tux.Count - 1);
            if (g0dh != "")
                XI.RaiseGMessage("G0DH" + g0dh);
        }
        public void GFT2Debut()
        {
            string result = AffichePlayers(p => p.IsAlive && p.Tux.Count > 0, p => p.Uid + ",1,1");
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH," + result);
        }
        public void GFT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.RemoveAll(p => p.Who == player.Uid && p.Source != p.Who &&
                    p.N > 0 && p.Element == FiveElement.A && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -9);
            }
        }
        public bool GFT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.Source != p.Who &&
                    p.N > 0 && p.Element == FiveElement.A && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
            }
            return false;
        }
        public void GFT2WinEff()
        {
            Cure("GFT2", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team), 2);
        }
        public void GFT2LoseEff()
        {
            Cure("GFT2", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam), 1);
        }

        public void GTT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                string[] blocks = fuse.Split(',');
                string g0dh = "";
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort ut = ushort.Parse(blocks[i]);
                    int gtype = int.Parse(blocks[i + 1]);
                    int n = int.Parse(blocks[i + 2]);
                    if (ut == player.Uid && gtype == 0)
                    {
                        if (n > 1)
                            g0dh += "," + ut + ",0," + (n - 1);
                    }
                    else
                        g0dh += "," + ut + "," + gtype + "," + n;
                }
                if (g0dh.Length > 0)
                    XI.InnerGMessage("G0DH" + g0dh, 61);

                string st = XI.AsyncInput(player.Uid, "#获得手牌的,T1(p" + string.Join("p", XI.Board.Garden.
                    Values.Where(p => p.Uid != player.Uid && p.Tux.Count > 0).Select(p => p.Uid)) + ")",
                    "GTT1", "0");
                ushort from = ushort.Parse(st);
                string c0 = Algo.RepeatString("p0", XI.Board.Garden[from].Tux.Count);
                XI.AsyncInput(player.Uid, "#获得的,C1(" + c0 + ")", "GTT1", "0");
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + from + ",2,1");
            }
            //XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public bool GTT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                if (XI.Board.RoundIN == ("R" + player.Uid + "BC"))
                    return XI.Board.Garden.Values.Where(p => p.Uid != player.Uid && p.Tux.Count > 0).Any();
            }
            return false;
        }
        public void GTT1WinEff()
        {
            Player r = XI.Board.Rounder, s = XI.Board.Supporter;
            int rc = r.ListOutAllCards().Count;
            if (rc >= 2) rc = 2;
            if (rc > 0)
            {
                string ts = XI.AsyncInput(r.Uid, "#弃置的,Q" + rc + "(p" + string.Join(
                    "p", r.ListOutAllCards()), "GTT1WinEff", "0");
                XI.RaiseGMessage("G0QZ," + r.Uid + "," + ts);
            }
            if (s.IsValidPlayer())
            {
                int sc = s.ListOutAllCards().Count;
                if (sc >= 2) sc = 2;
                if (sc > 0)
                {
                    string ts = XI.AsyncInput(s.Uid, "#弃置的,Q" + sc + "(p" + string.Join(
                        "p", s.ListOutAllCards()) + ")", "GTT1WinEff", "0");
                    XI.RaiseGMessage("G0QZ," + s.Uid + "," + ts);
                }
            }
        }
        public void GTT1LoseEff()
        {
            Player h = XI.Board.Hinder;
            if (h.IsValidPlayer())
            {
                int hc = h.ListOutAllCards().Count;
                if (hc >= 2) hc = 2;
                if (hc > 0)
                {
                    string ts = XI.AsyncInput(h.Uid, "#弃置的,Q" + hc + "(p" + string.Join(
                        "p", h.ListOutAllCards()), "GTT1LoseEff", "0");
                    XI.RaiseGMessage("G0QZ," + h.Uid + "," + ts);
                }
            }
        }
        public void GTT2Debut()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive).ToList();
            Harm("GTT2", invs, invs.Select(p => (p.HP + 2) / 3).ToList());
        }
        public void GTT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                string st = XI.AsyncInput(player.Uid, "#指定战力-2的,/T1(p" + string.Join("p", XI.Board.Garden.
                        Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")", "GTT2ConsumeAction", "0");
                if (st != "/0")
                {
                    ushort to = ushort.Parse(st);
                    XI.RaiseGMessage("G0OA," + to + ",1,2");
                }
            }
            //XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public bool GTT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
                return XI.Board.IsAttendWar(player);
            return false;
        }
        public void GTT2LoseEff()
        {
            bool done = false;
            while (!done)
            {
                List<ushort> allPets = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.IsAlive && py.Team == XI.Board.Rounder.Team)
                        allPets.AddRange(py.Pets.Where(p => p != 0));
                }
                int na = allPets.Count;
                if (na == 0)
                    break;
                string ts = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置累积战力4及以上的,M1" +
                    (na > 1 ? "~" + na : "") + "(p" + string.Join("p", allPets), "GTT2LoseEff", "0");
                List<ushort> pick = ts.Split(',').Select(p => ushort.Parse(p)).ToList();
                int sum = pick.Sum(p => XI.LibTuple.ML.Decode(p).STR);
                if (sum >= 4 || pick.Count == na)
                {
                    Artiad.ContentRule.GetPetOwnershipTable(pick, XI).ToList().ForEach(p =>
                    {
                        if (p.Key != 0)
                        {
                            XI.RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = p.Key,
                                Pets = p.ToArray()
                            }.ToMessage());
                        }
                    });
                    done = true;
                }
            }
        }
        #endregion Package 4#
        #region Package 5#
        public void GST3WinEff()
        {
            Player h = XI.Board.Hinder;
            if (h.IsValidPlayer())
            {
                if (h.Tux.Count > 0)
                    XI.RaiseGMessage("G0DH," + h.Uid + ",2," + h.Tux.Count);
                XI.RaiseGMessage("G0DH," + h.Uid + ",0,1");
            }
        }
        public void GST3LoseEff()
        {
            Player r = XI.Board.Rounder;
            if (r.Tux.Count > 0)
                XI.RaiseGMessage("G0DH," + r.Uid + ",2," + r.Tux.Count);
            XI.RaiseGMessage("G0DH," + r.Uid + ",0,1");
        }
        public void GST3IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GST3DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }
        public void GST3ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 2)
            {
                string[] parts = fuse.Split(',');
                if (type == 0) // G1DI,A,1,n,x...
                {
                    string g0dh = "";
                    for (int i = 1; i < parts.Length;)
                    {
                        ushort ut = ushort.Parse(parts[i]);
                        int discard = int.Parse(parts[i + 1]);
                        int n = int.Parse(parts[i + 2]);

                        if (discard == 1 && XI.Board.Garden[ut].Tux.Count > 0)
                            for (int j = i + 4; j < i + 4 + n; ++j)
                            {
                                ushort cd = ushort.Parse(parts[j]);
                                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(cd);
                                if (tux.Type != Tux.TuxType.ZP)
                                {
                                    g0dh += "," + ut + ",2," + XI.Board.Garden[ut].Tux.Count;
                                    break;
                                }
                            }
                        i += (4 + n);
                    }
                    if (g0dh.Length > 0)
                        XI.RaiseGMessage("G0DH" + g0dh);
                }
                else if (type == 1) // G0CC,A,0,A,KN,x1,x2;TF
                {
                    int cmidx = fuse.IndexOf(';');
                    string[] blocks = fuse.Substring(0, cmidx).Split(',');
                    ushort ut = ushort.Parse(blocks[1]);
                    for (int i = 5; i < blocks.Length; ++i)
                    {
                        ushort cd = ushort.Parse(blocks[i]);
                        Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(cd);
                        if (tux.Type != Tux.TuxType.ZP)
                        {
                            XI.RaiseGMessage("G0DH," + ut + ",2," + XI.Board.Garden[ut].Tux.Count);
                            break;
                        }
                    }
                }
            }
        }
        public bool GST3ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 2) // No G0DS event occurs anymore
            {
                string[] parts = fuse.Split(',');
                if (type == 0) // G1DI,A,1,n,x...
                {
                    for (int i = 1; i < parts.Length;)
                    {
                        ushort ut = ushort.Parse(parts[i]);
                        int discard = int.Parse(parts[i + 1]);
                        int n = int.Parse(parts[i + 2]);

                        if (discard == 1 && XI.Board.Garden[ut].Tux.Count > 0)
                            for (int j = i + 4; j < i + 4 + n; ++j)
                            {
                                ushort cd = ushort.Parse(parts[j]);
                                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(cd);
                                if (tux.Type != Tux.TuxType.ZP)
                                    return true;
                            }
                        i += (4 + n);
                    }
                }
                else if (type == 1) // G0CC,A,0,A,KN,x1,x2;TF
                {
                    int cmidx = fuse.IndexOf(';');
                    string[] blocks = fuse.Substring(0, cmidx).Split(',');
                    ushort ut = ushort.Parse(blocks[1]);
                    if (XI.Board.Garden[ut].Tux.Count <= 0)
                        return false;
                    for (int i = 5; i < blocks.Length; ++i)
                    {
                        ushort cd = ushort.Parse(blocks[i]);
                        Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(cd);
                        if (tux.Type != Tux.TuxType.ZP)
                            return true;
                    }
                }
            }
            return false;
        }

        public void GST4Debut()
        {
            foreach (Player py in XI.Board.Garden.Values)
                if (py.IsAlive)
                    XI.RaiseGMessage("G0DS," + py.Uid + ",0,1");
        }
        public void GST4WinEff()
        {
            Player r = XI.Board.Rounder, h = XI.Board.Hinder;
            if (h.IsValidPlayer())
                Harm("GST4", h, 3);
            string ques = XI.AsyncInput(r.Uid, "#您是否弃掉所有手牌解除定身？##是##否,Y2", "GST4WinEff", "0");
            if (ques == "1")
            {
                if (r.Tux.Count > 0)
                    XI.RaiseGMessage("G0DH," + r.Uid + ",2," + r.Tux.Count);
                XI.RaiseGMessage("G0DS," + r.Uid + ",1");
            }
        }
        public void GST4LoseEff()
        {
            Player r = XI.Board.Rounder, h = XI.Board.Hinder;
            Harm("GST4", r, 3);
            if (h.IsValidPlayer())
            {
                string ques = XI.AsyncInput(h.Uid, "#您是否弃掉所有手牌解除定身？##是##否,Y2", "GST4LoseEff", "0");
                if (ques == "1")
                {
                    if (h.Tux.Count > 0)
                        XI.RaiseGMessage("G0DH," + h.Uid + ",2," + h.Tux.Count);
                    XI.RaiseGMessage("G0DS," + h.Uid + ",1");
                }
            }
        }
        public void GST4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0) { } // No G0DS event occurs anymore
        }
        public bool GST4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0) // No G0DS event occurs anymore
            {
                string[] g0dss = fuse.Split(',');
                return g0dss[1] == player.Uid.ToString() && g0dss[2] == "0";
            }
            return false;
        }
        public void GST4IncrAction(Player player)
        {
            XI.Board.PetProtecedPlayer.Add(player.Uid);
        }
        public void GST4DecrAction(Player player)
        {
            XI.Board.PetProtecedPlayer.Remove(player.Uid);
        }

        public void GHT3WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GHT3", XI.Board.Hinder, 2);
        }
        public void GHT3LoseEff() { Harm("GHT3", XI.Board.Rounder, 2); }

        public void GHT4Debut()
        {
            Harm("GH04", XI.Board.Garden.Values.Where(p => p.IsAlive), 1);
        }
        public void GHT4WinEff()
        {
            List<Player> py = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam).ToList();
            if (py.Count >= 2)
            {
                string input = XI.AsyncInput(XI.Board.Rounder.Uid, "#依次进行拼点,T2(p" +
                    string.Join("p", py.Select(p => p.Uid)), "GHT4WinEff", "0");
                ushort[] ip = input.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage("G0TT," + ip[0]);
                int v0 = XI.Board.DiceValue;
                XI.RaiseGMessage("G0TT," + ip[1]);
                int v1 = XI.Board.DiceValue;
                if (v0 < v1)
                    Harm("GH04", XI.Board.Garden[ip[0]], 3);
                else if (v0 > v1)
                    Harm("GH04", XI.Board.Garden[ip[1]], 3);
                else
                    Harm("GH04", new Player[] { XI.Board.Garden[ip[0]], XI.Board.Garden[ip[1]] }, 2);
            }
            else if (py.Count == 1)
                Harm("GH04", py[0], 3);
        }
        public void GHT4LoseEff()
        {
            List<Player> py = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == XI.Board.Rounder.Team).ToList();
            if (py.Count >= 2)
            {
                string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#依次进行拼点,T2(p" +
                    string.Join("p", py.Select(p => p.Uid)), "GHT4WinEff", "0");
                ushort[] ip = input.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage("G0TT," + ip[0]);
                int v0 = XI.Board.DiceValue;
                XI.RaiseGMessage("G0TT," + ip[1]);
                int v1 = XI.Board.DiceValue;
                if (v0 < v1)
                    Harm("GH04", XI.Board.Garden[ip[0]], 3);
                else if (v0 > v1)
                    Harm("GH04", XI.Board.Garden[ip[1]], 3);
                else
                    Harm("GH04", new Player[] { XI.Board.Garden[ip[0]], XI.Board.Garden[ip[1]] }, 2);
            }
            else if (py.Count == 1)
                Harm("GH04", py[0], 3);
        }
        public void GHT4IncrAction(Player player)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT4"));
            if (mon != null && mon.ROMUshort == 0)
            {
                string opt = XI.AsyncInput(player.Uid, "#免疫伤害,V1(p" + string.Join("p",
                    FiveElementHelper.GetPropedElements().Select(p => p.Elem2Int())) + ")", "GHT4IncrAction", "0");
                ushort five = ushort.Parse(opt);
                mon.ROMUshort = five;
                XI.RaiseGMessage(new Artiad.AnnouceCard()
                {
                    Action = Artiad.AnnouceCard.Type.DECLARE,
                    Officer = player.Uid,
                    Genre = Card.Genre.Five,
                    SingleCard = five
                }.ToMessage());
            }
        }
        public void GHT4DecrAction(Player player)
        {
            Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT4"));
            if (mon != null)
                mon.ROMUshort = 0;
        }
        public void GHT4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT4"));
                if (mon != null)
                {
                    harms.RemoveAll(p => p.Element.Elem2Int() == mon.ROMUshort &&
                        p.Who == player.Uid && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
                }
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -9);
            }
        }
        public bool GHT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT4"));
                return mon != null && harms.Any(p => p.Element.Elem2Int() == mon.ROMUshort &&
                    p.Who == player.Uid && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
            }
            return false;
        }

        public void GLT3Debut()
        {
            List<Player> invs = new Player[] { XI.Board.Rounder, XI.Board.Supporter, XI.Board.Hinder }
                .Where(p => p.IsValidPlayer() && p.Tux.Count > 0).ToList();
            int n = invs.Max(p => p.Tux.Count);
            XI.RaiseGMessage("G1XR,1,2," + n + "," + string.Join(",", invs.Select(p => p.Uid)));
        }
        public void GLT3ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public bool GLT3ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                    p.N > 0 && p.Element.IsPropedElement());
            }
            return false;
        }
        public void GLT3WinEff()
        {
            string target = XI.AsyncInput(XI.Board.Rounder.Uid, "#补牌,T1" + AAlls(null), "GLT3WinEff", "0");
            XI.RaiseGMessage("G0DH," + target + ",0,1");
        }
        public void GLT3LoseEff()
        {
            Harm("GLT3", XI.Board.Rounder, 2);
        }
        public void GLT4Debut()
        {
            ushort x = XI.LibTuple.ML.Encode("GLT4");
            if (x != 0)
            {
                int inc = XI.Board.Garden.Values.Where(p => p.IsAlive).Sum(p => p.GetPetCount());
                if (inc > 0)
                    XI.RaiseGMessage("G0IB," + x + "," + inc);
            }
        }
        public void GLT4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GLT4"));
                if (mon != null)
                    mon.ROMUshort = (ushort)player.Team;
                string[] g1ev = fuse.Split(',');
                XI.InnerGMessage("G1EV," + g1ev[1] + "," + g1ev[2], 201);
            }
        }
        public bool GLT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GLT4"));
                return mon != null && mon.ROMUshort != player.Team;
            }
            return false;
        }
        public void GLT4WinEff()
        {
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team ==
                XI.Board.Rounder.Team).Select(p => p.Uid + ",0,1")));

            bool next = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team)
                    .Sum(p => p.GetPetCount()) < XI.Board.Garden.Values.Where(p => p.IsAlive &&
                        p.Team == XI.Board.Rounder.OppTeam).Sum(p => p.GetPetCount());
            if (!next)
            {
                Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                if (mon1 != null && mon1.Code == "GLT4")
                    XI.Board.Mon1Catchable = false;
                Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2 != null && mon2.Code == "GLT4")
                    XI.Board.Mon2Catchable = false;
            }
        }
        public void GLT4LoseEff()
        {
            List<Player> py = XI.Board.Garden.Values.Where(p => p.IsAlive).ToList();
            if (py.Count > 0)
            {
                string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#受到伤害,T1(p" +
                    string.Join("p", py.Select(p => p.Uid)) + ")", "GLT4LoseEff", "0");
                if (!input.StartsWith("/"))
                {
                    ushort ut = ushort.Parse(input);
                    if (XI.Board.Garden[ut].GetPetCount() > 0)
                        Harm("GLT4", XI.Board.Garden[ut], XI.Board.Garden[ut].GetPetCount());
                }
            }
        }

        public void GFT3Debut()
        {
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values
                .Where(p => p.IsAlive).Select(p => p.Uid + ",0,1")));
        }
        public void GFT3WinEff()
        {
            Artiad.Procedure.AssignCurePointToTeam(XI, XI.Board.Rounder, 4, "GFT3WinEff",
                p => Cure("GFT3", p.Keys.ToList(), p.Values.ToList()));
        }
        public void GFT3LoseEff()
        {
            Artiad.Procedure.AssignCurePointToTeam(XI, XI.Board.Opponent, 4, "GFT3LoseEff",
                p => Cure("GFT3", p.Keys.ToList(), p.Values.ToList()));
        }
        public bool GFT3ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0);
        }
        public void GFT3ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                List<ushort> zeros = XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
                string ic = zeros.Count > 0 ? "#HP+2,T1(p" + string.Join("p", zeros) + ")" : "/";
                ushort tg = ushort.Parse(XI.AsyncInput(player.Uid, ic, "GFT3ConsumeAction", "1"));

                if (zeros.Contains(tg))
                {
                    VI.Cout(0, "{0}爆发「流萤」，令{1}HP+2.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(tg));
                    Player tgp = XI.Board.Garden[tg];
                    Cure("GFT3", tgp, 2);
                }
                zeros = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
                if (zeros.Count > 0)
                    XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        public void GFT4Debut()
        {
            foreach (Player py in XI.Board.Garden.Values)
                if (py.IsAlive)
                    ++py.RestZP;
        }
        public void GFT4IncrAction(Player player)
        {
            XI.RaiseGMessage(new Artiad.InnateChange()
            {
                Item = Artiad.InnateChange.Prop.HP,
                Who = player.Uid,
                NewValue = player.HPb + 2
            }.ToMessage());
        }
        public void GFT4DecrAction(Player player)
        {
            XI.RaiseGMessage(new Artiad.InnateChange()
            {
                Item = Artiad.InnateChange.Prop.HP,
                Who = player.Uid,
                NewValue = player.HPb - 2
            }.ToMessage());
        }
        public void GFT4WinEff()
        {
            List<Player> invs = new List<Player>();
            if (XI.Board.Rounder.Tux.Count > 0)
                invs.Add(XI.Board.Rounder);
            if (XI.Board.Supporter.IsValidPlayer() && XI.Board.Supporter.Tux.Count > 0)
                invs.Add(XI.Board.Supporter);
            if (invs.Count > 0)
                Cure("GFT4", invs, invs.Select(p => p.Tux.Count).ToList());
        }
        public void GFT4LoseEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0
                && p.Team == XI.Board.Rounder.Team).ToList();
            if (invs.Count > 0)
                Harm("GFT4", invs, invs.Select(p => p.Tux.Count).ToList());
        }

        public void GTT3IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GTT3DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GTT3WinEff()
        {
            List<Player> invs = new List<Player> { XI.Board.Rounder };
            if (XI.Board.Supporter.IsValidPlayer())
                invs.Add(XI.Board.Supporter);
            if (XI.Board.Hinder.IsValidPlayer())
                invs.Add(XI.Board.Hinder);
            Harm("GTT3", invs, 2);
        }
        public void GTT3LoseEff()
        {
            List<Player> invs = new List<Player> { XI.Board.Rounder };
            if (XI.Board.Supporter.IsValidPlayer())
                invs.Add(XI.Board.Supporter);
            if (XI.Board.Hinder.IsValidPlayer())
                invs.Add(XI.Board.Hinder);
            invs = XI.Board.Garden.Values.Where(p => p.IsAlive).Except(invs).ToList();
            if (invs.Count > 0)
                Harm("GTT3", invs, 2);
        }
        public void GTT4Debut()
        {
            var g = XI.Board.Garden;
            Player r = XI.Board.Rounder, o = XI.Board.Opponent;
            IDictionary<ushort, string> ques = new Dictionary<ushort, string>();
            ques[r.Uid] = "#HP-2,T1(p" + string.Join("p", g.Values.Where(
                p => p.IsAlive && p.Team == r.OppTeam).Select(p => p.Uid)) + ")";
            ques[o.Uid] = "#HP-2,T1(p" + string.Join("p", g.Values.Where(
                p => p.IsAlive && p.Team == r.Team).Select(p => p.Uid)) + ")";
            IDictionary<ushort, string> ans = XI.MultiAsyncInput(ques);
            Harm("GTT4", ans.Values.Select(p => g[ushort.Parse(p)]).ToList(), 2);
        }
        public void GTT4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
                Cure("GTT4", player, 1);
        }
        public bool GTT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
                return XI.Board.IsAttendWar(player);
            return false;
        }
        public void GTT4WinEff()
        {
            var h = XI.Board.Hinder;
            if (h.IsValidPlayer())
            {
                List<Player> invs = new List<Player> { XI.Board.Rounder };
                if (XI.Board.Supporter.IsValidPlayer())
                    invs.Add(XI.Board.Supporter);
                string opcs = XI.AsyncInput(h.Uid, "#交换手牌的,T1(p" +
                    string.Join("p", invs.Select(p => p.Uid)) + ")", "GTT4WinEff", "0");
                ushort opc = ushort.Parse(opcs);
                Player o = XI.Board.Garden[opc];

                XI.RaiseGMessage("G0HQ,4," + h.Uid + "," + o.Uid + "," + h.Tux.Count + "," + o.Tux.Count +
                    (h.Tux.Count > 0 ? ("," + string.Join(",", h.Tux)) : "") +
                    (o.Tux.Count > 0 ? ("," + string.Join(",", o.Tux)) : ""));
            }
        }
        #endregion Package 5#

        #region Package 6# - YINN
        public void GIT1Debut()
        {
            if (XI.Board.Supporter.IsValidPlayer())
                XI.RaiseGMessage("G0IA," + XI.Board.Supporter.Uid + ",1,1");
        }
        public void GIT1WinEff()
        {
            List<ushort> friends = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team ==
                XI.Board.Rounder.Team && p.Uid != XI.Board.Rounder.Team).Select(p => p.Uid).ToList();
            if (friends.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", friends.Select(p => p + ",0,1")));
        }
        public void GIT1LoseEff()
        {
            Player rd = XI.Board.Rounder;
            Harm("GIT1", rd, 3);
            if (rd.HasAnyEquips())
            {
                string input = XI.AsyncInput(rd.Uid, "#须弃置的,Q1(p" + string.Join("p",
                    rd.ListOutAllEquips()) + ")", "GIT1LoseEff", "0");
                ushort which = ushort.Parse(input);
                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + which);
                XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
            }
        }
        public bool GIT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                if (type == 0 || (type == 1 && player.Tux.Count > 0))
                {
                    string[] g0ht = fuse.Split(',');
                    for (int i = 1; i < g0ht.Length; i += 2)
                    {
                        ushort ut = ushort.Parse(g0ht[i]);
                        if (ut == player.Uid)
                            return true;
                    }
                }
            }
            return false;
        }
        public void GIT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (player.Uid == ut)
                        g0ht[i + 1] = (n + 1).ToString();
                }
                XI.InnerGMessage(string.Join(",", g0ht), 71);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0DH," + player.Uid + ",1,1");
        }
        public void GIT2WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GIT2", XI.Board.Hinder, 3);
        }
        public void GIT2LoseEff()
        {
            Harm("GIT2", XI.Board.Rounder, 3);
        }
        public bool GIT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0 && player.Tux.Count >= 2)
            {
                Tux tp01 = XI.LibTuple.TL.EncodeTuxCode("TP01");
                return tp01.Bribe(player, type, fuse) && tp01.Valid(player, type, fuse);
            }
            else return false;
        }
        public void GIT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid +
                ",TP01," + argst + ";" + type + "," + fuse);
        }
        public string GIT2ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 0 && prev == "")
            {
                List<ushort> tuxes = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP).ToList();
                if (tuxes.Count >= 2)
                    return "/Q2(p" + string.Join("p", tuxes) + ")";
                else
                    return "/";
            }
            else return "";
        }
        public void GIT3IncrAction(Player player)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GIT3"));
            if (mon != null)
            {
                if (mon.ROMUshort == 0)
                {
                    string hint = "#请选择「魔骨」宠物效果。##战力+1##命中+1,Y2";
                    string option = XI.AsyncInput(player.Uid, hint, "GIT3IncrAction", "0");
                    if (option == "1")
                        mon.ROMUshort = 1;
                    else if (option == "2")
                        mon.ROMUshort = 2;
                }
                if (mon.ROMUshort == 1)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                else if (mon.ROMUshort == 2)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
            }
        }
        public void GIT3DecrAction(Player player)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GIT3"));
            if (mon != null)
            {
                if (mon.ROMUshort == 1)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                else if (mon.ROMUshort == 2)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
            }
        }
        public void GIT3Debut()
        {
            Player rd = XI.Board.Rounder, hd = XI.Board.Hinder;
            if (hd.IsValidPlayer())
            {
                List<ushort> rl = rd.ListOutAllBaseEquip();
                List<ushort> hl = hd.ListOutAllBaseEquip();
                if (rl.Count > 0)
                {
                    XI.RaiseGMessage("G0OT," + rd.Uid + "," + rl.Count + "," + string.Join(",", rl));
                    rl.ForEach(p => XI.Board.PendingTux.Enqueue(rd.Uid + ",G0ZB," + p));
                }
                if (hl.Count > 0)
                {
                    XI.RaiseGMessage("G0OT," + hd.Uid + "," + hl.Count + "," + string.Join(",", hl));
                    hl.ForEach(p => XI.Board.PendingTux.Enqueue(hd.Uid + ",G0ZB," + p));
                    XI.RaiseGMessage(new Artiad.EquipStandard()
                    {
                        Who = rd.Uid, Source = hd.Uid, SlotAssign = true, Cards = hl.ToArray()
                    }.ToMessage());
                }
                if (rl.Count > 0)
                {
                    XI.RaiseGMessage(new Artiad.EquipStandard()
                    {
                        Who = hd.Uid, Source = rd.Uid, SlotAssign = true, Cards = rl.ToArray()
                    }.ToMessage());
                }
            }
        }
        public void GIT3WinEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.OppTeam && p.GetBaseEquipCount() > 0).ToList();
            if (invs.Count > 0)
                Harm("GIT3", invs, invs.Select(p => p.GetBaseEquipCount()).ToList());
        }
        public void GIT3LoseEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.Team && p.GetBaseEquipCount() > 0).ToList();
            if (invs.Count > 0)
                Harm("GIT3", invs, invs.Select(p => p.GetBaseEquipCount()).ToList());
        }
        public void GIT4IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GIT4DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }
        public void GIT4Debut()
        {
            XI.RaiseGMessage("G0TT," + XI.Board.Rounder.Uid);
            int val = (XI.Board.DiceValue + 1) / 2;
            ushort x = XI.LibTuple.ML.Encode("GIT4");
            if (x != 0)
                XI.RaiseGMessage("G0IB," + x + "," + val);
        }
        public void GIT4WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GIT4", XI.Board.Hinder, 2);
        }
        public void GIT4LoseEff()
        {
            if (XI.Board.Supporter.IsValidPlayer())
                Harm("GIT4", new Player[] { XI.Board.Rounder, XI.Board.Supporter }, 2);
            else
                Harm("GIT4", XI.Board.Rounder, 2);
        }
        public void GIT5Debut()
        {
            ushort git5 = XI.LibTuple.ML.Encode("GIT5");
            int n = XI.Board.Garden.Values.Count(p => p.IsAlive);
            if (git5 != 0)
                XI.RaiseGMessage("G0IB," + git5 + "," + n);
        }
        public void GIT5ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                ushort git5 = XI.LibTuple.ML.Encode("GIT5");
                int n = XI.Board.Garden.Values.Count(p => p.IsAlive);
                if (git5 != 0)
                    XI.RaiseGMessage("G0IB," + git5 + "," + n);
            }
        }
        public void GIT5WinEff()
        {
            Harm("GIT5", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.OppTeam), 1);
        }
        public void GIT5LoseEff()
        {
            Harm("GIT5", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team), 1);
        }
        public bool GIT6ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GIT6"));
                return mon != null && XI.Board.IsAttendWar(player) && mon.RAMInt == 0;
            }
            else if (consumeType == 2)
            {
                string[] g0cc = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                string cardname = g0cc[4];
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                return tux != null && tux.Type == Tux.TuxType.ZP;
            }
            return false;
        }
        public void GIT6ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GIT6"));
                mon.RAMInt = 1;
                XI.RaiseGMessage("G0IX," + player.Uid + ",2");
            }
            else if (consumeType == 2)
            {
                string[] g0cc = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                ushort trigger = ushort.Parse(g0cc[3]);
                Harm("GIT6", XI.Board.Garden[trigger], 2);
            }
        }
        public void GIT6WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
            {
                XI.RaiseGMessage("G0TT," + XI.Board.Hinder.Uid);
                Harm("GIT6", XI.Board.Hinder, XI.Board.DiceValue);
            }
        }
        public void GIT6LoseEff()
        {
            XI.RaiseGMessage("G0TT," + XI.Board.Rounder.Uid);
            Harm("GIT6", XI.Board.Rounder, XI.Board.DiceValue);
        }
        public bool GIT7ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
                return XI.Board.IsAttendWar(player) && player.Tux.Count != player.TuxLimit;
            return false;
        }
        public void GIT7ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                int delta = System.Math.Min(System.Math.Abs(player.Tux.Count - player.TuxLimit), 3);
                if (argst == "2")
                    XI.RaiseGMessage("G0IX," + player.Uid + ",1," + delta);
                else
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + delta);
            }
        }
        public string GIT7ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 0)
            {
                if (prev == "")
                {
                    int delta = System.Math.Min(System.Math.Abs(player.Tux.Count - player.TuxLimit), 3);
                    return "#请选择【戾枭】宠物效果##战力+" + delta + "##命中+" + delta + ",/Y2";
                }
                else
                    return "";
            }
            return "";
        }
        public void GIT7WinEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Tux.Count > 0 && p.Team == XI.Board.Rounder.OppTeam).ToList();
            if (invs.Count > 0)
                Harm("GIT7", invs, invs.Select(p => p.Tux.Count).ToList());
        }
        public void GIT7LoseEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Tux.Count > 0 && p.Team == XI.Board.Rounder.Team).ToList();
            if (invs.Count > 0)
                Harm("GIT7", invs, invs.Select(p => p.Tux.Count).ToList());
        }
        public void GIT8Debut()
        {
            ushort git8 = XI.LibTuple.ML.Encode("GIT8");
            XI.RaiseGMessage("G0TT," + XI.Board.Rounder.Uid);
            int n = XI.Board.DiceValue / 2;
            if (git8 != 0 && n > 0)
                XI.RaiseGMessage("G0IW," + git8 + "," + n);
        }
        public bool GIT8ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0 && XI.Board.RoundIN == "R" + player.Uid + "GR")
            {
                if (type == 0) // G0CC
                {
                    string[] g0cc = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                    int n = g0cc.Length - 5;
                    if (g0cc[5] == "0")
                        --n;
                    return g0cc[1] == player.Uid.ToString() && n > 0;
                }
                else if (type == 1) // G1UE
                {
                    string[] g1ue = fuse.Split(',');
                    return g1ue[2] == player.Uid.ToString() && g1ue[3] != "0";
                }
                else if (type == 2)
                {
                    Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GIT8"));
                    string[] g1ck = fuse.Split(',');
                    return mon != null && mon.RAMInt >= 2 && g1ck[1] == player.Uid.ToString() &&
                        g1ck[2] == "GT08Consume" && g1ck[3] == "0";
                }
                else return false;
            }
            else return false;
        }
        public void GIT8ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GIT8"));
            if (mon != null && consumeType == 0)
            {
                if (type == 0 || type == 1)
                {
                    ++mon.RAMInt;
                    XI.RaiseGMessage("G1CK," + player.Uid + ",GT08Consume,0");
                }
                else if (type == 2) // G1CK
                {
                    mon.RAMInt = 0;
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                }
            }
        }
        public void GIT8WinEff()
        {
            Player py = XI.Board.Rounder;
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.Team == py.OppTeam &&
                p.IsAlive && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            if (invs.Count > 0)
            {
                string opt = XI.AsyncInput(py.Uid, "#抽取,T1(p" +
                    string.Join("p", invs) + ")", "GIT8WinEff", "0");
                if (opt != VI.CinSentinel)
                {
                    ushort tar = ushort.Parse(opt);
                    TargetPlayer(py.Uid, tar);
                    XI.AsyncInput(py.Uid, "C1(" + Algo.RepeatString("p0",
                        XI.Board.Garden[tar].Tux.Count) + ")", "GIT8WinEff", "1");
                    XI.RaiseGMessage("G0HQ,0," + py.Uid + "," + tar + ",2,1");
                    Harm("GIT8", XI.Board.Garden[tar], 2);
                }
            }
        }
        public void GIT8LoseEff()
        {
            Player py = XI.Board.Hinder;
            if (!py.IsValidPlayer())
                return;
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.Team == py.OppTeam &&
                p.IsAlive && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            if (invs.Count > 0)
            {
                string opt = XI.AsyncInput(py.Uid, "#抽取,T1(p" +
                    string.Join("p", invs) + ")", "GIT8LoseEff", "0");
                if (opt != VI.CinSentinel)
                {
                    ushort tar = ushort.Parse(opt);
                    TargetPlayer(py.Uid, tar);
                    XI.AsyncInput(py.Uid, "C1(" + Algo.RepeatString("p0",
                        XI.Board.Garden[tar].Tux.Count) + ")", "GIT8LoseEff", "1");
                    XI.RaiseGMessage("G0HQ,0," + py.Uid + "," + tar + ",2,1");
                    Harm("GIT8", XI.Board.Garden[tar], 2);
                }
            }
        }
        #endregion Package 6# - YINN
        #region Package 6# - SOLARIS
        public void GYT1IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,2");
        }
        public void GYT1DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,2");
        }
        public void GYT1Debut()
        {
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Tux.Count < p.TuxLimit).Select(p => p.Uid).ToList();
            if (invs.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p + ",0,2")));
        }
        public void GYT1WinEff()
        {
            Player rd = XI.Board.Rounder;
            // int n = Math.Min(3, XI.Board.Garden.Values.Count(p => p.IsAlive));
            // string opt = XI.AsyncInput(rd.Uid, "#获得,/F1" + StdRunes() + ",#获得标记,/T1" +
            //     (n > 1 ? ("~" + n) : "") + AAlls(rd), "GYT1WinEff", "0");
            // if (opt != VI.CinSentinel && !opt.StartsWith("/"))
            // {
            //     string[] parts = opt.Split(',');
            //     ushort rune = ushort.Parse(parts[0]);
            //     for (int i = 1; i < parts.Length; ++i)
            //         XI.RaiseGMessage("G0IF," + parts[i] + "," + rune);
            // }
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid).ToList();
            int pts = 3;
            while (pts > 0 && invs.Count > 0)
            {
                string optTar = XI.AsyncInput(rd.Uid, "#获得标记,/T1(p" +
                    string.Join("p", invs) + ")", "GYT1WinEff", "0");
                if (optTar.StartsWith("/"))
                    break;
                else if (optTar != VI.CinSentinel)
                {
                    string optRune = XI.AsyncInput(rd.Uid, "#获得标记,/F1" + StdRunes(), "GYT1WinEff", "1");
                    if (optRune != VI.CinSentinel && !optRune.StartsWith("/"))
                    {
                        ushort tar = ushort.Parse(optTar);
                        TargetPlayer(rd.Uid, tar);
                        invs.Remove(tar); --pts;
                        XI.RaiseGMessage("G0IF," + optTar + "," + optRune);
                    }
                }
            }
        }
        public void GYT1LoseEff()
        {
            if (!XI.Board.Hinder.IsValidPlayer())
                return;
            Player op = XI.Board.Hinder;
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid).ToList();
            int pts = 3;
            while (pts > 0 && invs.Count > 0)
            {
                string optTar = XI.AsyncInput(op.Uid, "#获得标记,/T1(p" +
                    string.Join("p", invs) + ")", "GYT1LoseEff", "0");
                if (optTar.StartsWith("/"))
                    break;
                else if (optTar != VI.CinSentinel)
                {
                    string optRune = XI.AsyncInput(op.Uid, "#获得标记,/F1" + StdRunes(), "GYT1LoseEff", "1");
                    if (optRune != VI.CinSentinel && !optRune.StartsWith("/"))
                    {
                        ushort tar = ushort.Parse(optTar);
                        TargetPlayer(op.Uid, tar);
                        invs.Remove(tar); --pts;
                        XI.RaiseGMessage("G0IF," + optTar + "," + optRune);
                    }
                }
            }
        }
        public bool GYT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                return XI.Board.Facer(player).IsAlive && Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                    p.Who != p.Source && p.Element == FiveElement.A && !HPEvoMask.TUX_INAVO.IsSet(p.Mask) &&
                    !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask) && p.N > 0);
            }
            else return false;
        }
        public void GYT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Artiad.Procedure.RotateHarm(player, XI.Board.Facer(player), true, v => 1, ref harms);
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -114);
                XI.RaiseGMessage("G0IF," + player.Uid + ",6");
            }
        }
        public void GYT2WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                XI.RaiseGMessage("G0IF," + XI.Board.Hinder.Uid + ",5");
        }
        public void GYT2LoseEff()
        {
            XI.RaiseGMessage("G0IF," + XI.Board.Rounder.Uid + ",5");
        }
        public void GYT3IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
            ++player.TuxLimit;
        }
        public void GYT3DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
            --player.TuxLimit;
        }
        public void GYT3Debut()
        {
            string msg = string.Join(",", XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Tux.Count != 3).Select(p => p.Tux.Count > 3 ?
                (p.Uid + ",1," + (p.Tux.Count - 3)) : (p.Uid + ",0," + (3 - p.Tux.Count))));
            if (!string.IsNullOrEmpty(msg))
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void GYT3WinEff()
        {
            Artiad.Procedure.ObtainAndAllocateTux(XI, VI, XI.Board.Rounder, 3, "GYT3WinEff", "0");
        }
        public void GYT3LoseEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Artiad.Procedure.ObtainAndAllocateTux(XI, VI, XI.Board.Hinder, 3, "GYT3LoseEff", "0");
        }
        public void GYT4Debut()
        {
            var pos = XI.LibTuple.RL.GetFullPositive();
            foreach (Player player in XI.Board.Garden.Values)
            {
                if (player.IsAlive)
                {
                    List<ushort> invs = pos.Intersect(player.Runes).ToList();
                    if (invs.Count > 0)
                        XI.RaiseGMessage("G0OF," + player.Uid + "," + string.Join(",", invs));
                }
            }
        }
        public void GYT4WinEff()
        {
            Player rd = XI.Board.Rounder;
            List<ushort> enes = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.HasAnyCards() && p.Team == rd.OppTeam).Select(p => p.Uid).ToList();
            if (enes.Count > 0)
            {
                string first = XI.AsyncInput(rd.Uid, "#弃牌,T1(p" +
                    string.Join("p", enes) + ")", "GYT4WinEff", "0");
                ushort who = ushort.Parse(first);
                Player tar = XI.Board.Garden[who];
                TargetPlayer(rd.Uid, who);
                string second = XI.AsyncInput(rd.Uid, string.Format("#弃置{0}的,C1(p{1})", XI.DisplayPlayer(
                    who), string.Join("p", tar.ListOutAllCardsWithEncrypt())), "GYT4WinEff", "1");
                ushort card = ushort.Parse(second);
                if (card == 0)
                    XI.RaiseGMessage("G0DH," + who + ",2,1");
                else
                    XI.RaiseGMessage("G0QZ," + who + "," + card);
                string third = XI.AsyncInput(rd.Uid, string.Format("#令{0}获得的,F1(p{1})", XI.DisplayPlayer(
                    who), string.Join("p", XI.LibTuple.RL.GetFullNegative())), "GYT4WinEff", "2");
                ushort fw = ushort.Parse(third);
                XI.RaiseGMessage("G0IF," + who + "," + fw);
            }
        }
        public void GYT4LoseEff()
        {
            Player rd = XI.Board.Hinder;
            if (rd.IsValidPlayer())
            {
                List<ushort> enes = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.HasAnyCards() && p.Team == rd.OppTeam).Select(p => p.Uid).ToList();
                if (enes.Count > 0)
                {
                    string first = XI.AsyncInput(rd.Uid, "#弃牌,T1(p" +
                        string.Join("p", enes) + ")", "GYT4LoseEff", "0");
                    ushort who = ushort.Parse(first);
                    TargetPlayer(rd.Uid, who);
                    Player tar = XI.Board.Garden[who];
                    string second = XI.AsyncInput(rd.Uid, string.Format("#弃置{0}的,C1(p{1})", XI.DisplayPlayer(
                        who), string.Join("p", tar.ListOutAllCardsWithEncrypt())), "GYT4LoseEff", "1");
                    ushort card = ushort.Parse(second);
                    if (card == 0)
                        XI.RaiseGMessage("G0DH," + who + ",2,1");
                    else
                        XI.RaiseGMessage("G0QZ," + who + "," + card);
                    string third = XI.AsyncInput(rd.Uid, string.Format("#您获得的,F1(p{0})",
                        string.Join("p", XI.LibTuple.RL.GetFullPositive())), "GYT4LoseEff", "2");
                    ushort fw = ushort.Parse(third);
                    XI.RaiseGMessage("G0IF," + rd.Uid + "," + fw);
                }
            }
        }
        public bool GYT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                    || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
                return XI.Board.IsAttendWar(player) && meLose;
            }
            return false;
        }
        public void GYT4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IF," + player.Uid + "," + argst);
        }
        public string GYT4ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 0 && prev == "")
                return "/F1(p" + string.Join("p", XI.LibTuple.RL.GetFullPositive()) + ")";
            else
                return "";
        }
        public void GYT5IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GYT5DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GYT5Debut()
        {
            string msg = string.Join(",", XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Tux.Count > 0).Select(p => p.Uid + ",2,1"));
            if (!string.IsNullOrEmpty(msg))
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void GYT5WinEff()
        {
            Player py = XI.Board.Rounder;
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.Team == py.OppTeam &&
                p.IsAlive && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            if (invs.Count > 0)
            {
                string target = XI.AsyncInput(py.Uid, "#弃置手牌,T1(p" +
                    string.Join("p", invs) + ")", "GYT5WinEff", "0");
                if (target != VI.CinSentinel)
                {
                    ushort who = ushort.Parse(target);
                    TargetPlayer(py.Uid, who);
                    XI.RaiseGMessage("G0DH," + who + ",2,1");
                }
            }
        }
        public void GYT5LoseEff()
        {
            Player py = XI.Board.Hinder;
            if (py.IsValidPlayer())
            {
                List<ushort> invs = XI.Board.Garden.Values.Where(p => p.Team == py.OppTeam &&
                    p.IsAlive && p.Tux.Count > 0).Select(p => p.Uid).ToList();
                if (invs.Count > 0)
                {
                    string target = XI.AsyncInput(py.Uid, "#弃置手牌,T1(p" +
                        string.Join("p", invs) + ")", "GYT5LoseEff", "0");
                    if (target != VI.CinSentinel)
                    {
                        ushort who = ushort.Parse(target);
                        TargetPlayer(py.Uid, who);
                        XI.RaiseGMessage("G0DH," + who + ",2,1");
                    }
                }
            }
        }
        public void GYT6IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public void GYT6DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        }
        public void GYT6Debut()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                !XI.Board.IsAttendWar(p) && p.GetEquipCount() > 0).ToList();
            IDictionary<ushort, string> requires = new Dictionary<ushort, string>();
            foreach (Player py in pys)
                requires.Add(py.Uid, "#须弃置,Q1(p" + string.Join("p", py.ListOutAllEquips()) + ")");
            IDictionary<ushort, string> inputs = XI.MultiAsyncInput(requires);
            foreach (var pair in inputs)
                XI.RaiseGMessage("G0QZ," + pair.Key + "," + pair.Value);
        }
        public void GYT6WinEff()
        {
            Player op = XI.Board.Opponent;
            if (XI.Board.Garden.Values.Any(p => p.Team == op.Team && p.IsAlive && p.GetEquipCount() > 0))
            {
                string optTar = XI.AsyncInput(op.Uid, "#弃置装备,T1" + FormatPlayers(p => p.IsAlive &&
                    p.Team == op.Team && p.GetEquipCount() > 0), "GYT6WinEff", "0");
                if (optTar != VI.CinSentinel)
                {
                    ushort tar = ushort.Parse(optTar);
                    TargetPlayer(op.Uid, tar);
                    char cq = (tar == op.Uid ? 'Q' : 'C');
                    string optUt = XI.AsyncInput(op.Uid, "#弃置装备," + cq + "1(p" + string.Join("p",
                        XI.Board.Garden[tar].ListOutAllEquips()) + ")", "GYT6WinEff", "1");
                    if (optUt != VI.CinSentinel)
                        XI.RaiseGMessage("G0QZ," + tar + "," + optUt);
                }
            }
            else if (XI.Board.Hinder.IsValidPlayer())
                XI.RaiseGMessage("G0IF," + XI.Board.Hinder.Uid + ",5");
        }
        public void GYT6LoseEff()
        {
            Player rd = XI.Board.Rounder;
            if (XI.Board.Garden.Values.Any(p => p.Team == rd.Team && p.IsAlive && p.GetEquipCount() > 0))
            {
                string optTar = XI.AsyncInput(rd.Uid, "#弃置装备,T1" + FormatPlayers(p => p.IsAlive &&
                    p.Team == rd.Team && p.GetEquipCount() > 0), "GYT6LoseEff", "0");
                if (optTar != VI.CinSentinel)
                {
                    ushort tar = ushort.Parse(optTar);
                    TargetPlayer(rd.Uid, tar);
                    char cq = (tar == rd.Uid ? 'Q' : 'C');
                    string optUt = XI.AsyncInput(rd.Uid, "#弃置装备," + cq + "1(p" + string.Join("p",
                        XI.Board.Garden[tar].ListOutAllEquips()) + ")", "GYT6LoseEff", "1");
                    if (optUt != VI.CinSentinel)
                        XI.RaiseGMessage("G0QZ," + tar + "," + optUt);
                }
            }
            else
                XI.RaiseGMessage("G0IF," + rd.Uid + ",5");
        }
        public void GYT7IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GYT7DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GYT7Debut()
        {
            string msg = string.Join(",", XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Tux.Count > p.TuxLimit).Select(p => p.Uid + ",2," + (p.Tux.Count - p.TuxLimit)));
            if (!string.IsNullOrEmpty(msg))
                XI.RaiseGMessage("G0DH," + msg);
        }
        public void GYT7WinEff()
        {
            Player py = XI.Board.Rounder;
            string target = XI.AsyncInput(py.Uid, "#补牌,T1" + ATeammates(py), "GYT7WinEff", "0");
            if (target != VI.CinSentinel)
            {
                ushort who = ushort.Parse(target);
                int delta = XI.Board.Garden[who].TuxLimit - XI.Board.Garden[who].Tux.Count;
                if (delta > 0)
                    XI.RaiseGMessage("G0DH," + who + ",0," + delta);
            }
        }
        public void GYT7LoseEff()
        {
            Player py = XI.Board.Hinder;
            if (py.IsValidPlayer())
            {
                string target = XI.AsyncInput(py.Uid, "#补牌,T1" + ATeammates(py), "GYT7LoseEff", "0");
                if (target != VI.CinSentinel)
                {
                    ushort who = ushort.Parse(target);
                    int delta = XI.Board.Garden[who].TuxLimit - XI.Board.Garden[who].Tux.Count;
                    if (delta > 0)
                        XI.RaiseGMessage("G0DH," + who + ",0," + delta);
                }
            }
        }
        public void GYT8Debut()
        {
            XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid).ToList()
                .ForEach(p => XI.RaiseGMessage("G0IF," + p + ",6"));
        }
        public void GYT8WinEff()
        {
            string optTar = XI.AsyncInput(XI.Board.Rounder.Uid,
                "#掷骰判定,T1" + FormatPlayers(p => p.IsAlive), "GYT8WinEff", "0");
            if (optTar != VI.CinSentinel)
            {
                ushort who = ushort.Parse(optTar);
                TargetPlayer(XI.Board.Rounder.Uid, who);
                XI.RaiseGMessage("G0TT," + who);
                int value = XI.Board.DiceValue;
                if (value > 4)
                    XI.RaiseGMessage("G0DH," + who + ",0," + (value - 4));
                else if (value < 4)
                    XI.RaiseGMessage("G0DH," + who + ",1," + (4 - value));
            }
        }
        public void GYT8LoseEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
            {
                string optTar = XI.AsyncInput(XI.Board.Hinder.Uid,
                    "#掷骰判定,T1" + FormatPlayers(p => p.IsAlive), "GYT8LoseEff", "0");
                if (optTar != VI.CinSentinel)
                {
                    ushort who = ushort.Parse(optTar);
                    TargetPlayer(XI.Board.Hinder.Uid, who);
                    XI.RaiseGMessage("G0TT," + who);
                    int value = XI.Board.DiceValue;
                    if (value > 4)
                        XI.RaiseGMessage("G0DH," + who + ",0," + (value - 4));
                    else if (value < 4)
                        XI.RaiseGMessage("G0DH," + who + ",1," + (4 - value));
                }
            }
        }
        #endregion Package 6# - SOLARIS

        #region Package HL
        public void GSH1IncrAction(Player player)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
        }
        public void GSH1DecrAction(Player player)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
        }
        public void GSH1Debut()
        {
            ushort gsh1 = XI.LibTuple.ML.Encode("GSH1");
            ushort owner = Artiad.ContentRule.GetPetOwnership(gsh1, XI);
            if (owner != 0)
            {
                int inc = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == XI.Board.Garden[owner].Team).Sum(p => p.GetPetCount());
                if (inc > 0)
                    XI.RaiseGMessage("G0IB," + gsh1 + "," + (inc * 2));
            }
        }
        public void GSH1WinEff()
        {
            Player rd = XI.Board.Rounder;
            if (rd.GetAllCardsCount() > 0)
            {
                int n = rd.GetAllCardsCount();
                string dises = XI.AsyncInput(rd.Uid, "#弃置以补牌,/Q1" + (n > 1 ? ("~" + n) : "") + "(p" + 
                    string.Join("p", rd.ListOutAllCards()) + "),#补牌,/T1" + AOthers(rd), "GSH1WinEff", "0");
                if (!dises.StartsWith("/") && !dises.Contains(VI.CinSentinel))
                {
                    ushort[] uts = dises.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    XI.RaiseGMessage("G0QZ," + rd.Uid + "," +
                        string.Join(",", Algo.TakeRange(uts, 0, uts.Length - 1)));
                    XI.RaiseGMessage("G0DH," + uts[uts.Length - 1] + ",0," + (uts.Length - 1));
                }
            }
        }
        public void GSH1LoseEff()
        {
            Harm("GSH1", XI.Board.Rounder, 2);
            if (XI.Board.Hinder.IsValidPlayer())
                Cure("GSH1", XI.Board.Hinder, 2);
        }
        public void GSH1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            ushort me = XI.LibTuple.ML.Encode("GSH1");
            XI.Board.Mon1From = player.Uid;
            XI.Board.Monster1 = me;
            XI.RaiseGMessage("G0YM,0," + me + "," + player.Uid);
        }
        public void GSH2Debut()
        {
            foreach (Player player in XI.Board.Garden.Values)
            {
                if (player.IsAlive && !XI.Board.IsAttendWar(player))
                    player.DrTuxDisabled = true;
            }
            XI.Board.EscueBanned.Add("GSH2Debut");
        }
        public void GSH2Curtain()
        {
            foreach (Player player in XI.Board.Garden.Values)
            {
                if (player.IsAlive && !XI.Board.IsAttendWar(player))
                    player.DrTuxDisabled = false;
            }
            XI.Board.EscueBanned.Remove("GSH2Debut");
        }
        public bool GSH2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0 && player.Tux.Count >= 2)
            {
                List<Player> invs = Artiad.Harm.Parse(fuse).Where(p => p.N > 0).Select(
                    p => XI.Board.Garden[p.Who]).Where(p => p.IsAlive).Distinct().ToList();
                return invs.Any(p => XI.Board.Garden.Values.Any(q => !invs.Contains(q) && q.IsAlive &&
                    q.HP == p.HP && q.Tux.Count == p.Tux.Count && q.Gender == p.Gender));
            }
            else if (consumeType == 2)
            {
                if (XI.Board.InFightThrough)
                {
                    // G0FI,
                    string[] g0fi = fuse.Split(',');
                    if (g0fi[1] == "O" || g0fi[1] == "U")
                        return false;
                    for (int i = 1; i < g0fi.Length; i += 3)
                    {
                        char ch = g0fi[i][0];
                        if (ch == 'S' || ch == 'H')
                            return true;
                    }
                }
            }
            return false;
        }
        public void GSH2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            // Cancel the harm directly
            if (consumeType == 0)
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            else if (consumeType == 2)
            {
                string[] g0fi = fuse.Split(',');
                for (int i = 1; i < g0fi.Length; i += 3)
                {
                    char ch = g0fi[i][0];
                    ushort old = ushort.Parse(g0fi[i + 1]);
                    ushort to = ushort.Parse(g0fi[i + 2]);
                    if (ch == 'S' || ch == 'H')
                    {
                        if (old != 0)
                            XI.Board.Garden[old].DrTuxDisabled = true;
                        if (to != 0)
                            XI.Board.Garden[to].DrTuxDisabled = false;
                    }
                }
            }
        }
        public string GSH2ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 0 && prev == "")
                return "/Q2(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public void GSH2WinEff()
        {
            Player py = XI.Board.Opponent;
            string target = XI.AsyncInput(py.Uid, "#受伤,T1" + ATeammates(py), "GSH2WinEff", "0");
            if (target != VI.CinSentinel)
            {
                ushort who = ushort.Parse(target);
                Harm("GSH2", XI.Board.Garden[who], 4, (long)HPEvoMask.TUX_INAVO);
            }
        }
        public void GSH2LoseEff()
        {
            Player py = XI.Board.Rounder;
            string target = XI.AsyncInput(py.Uid, "#受伤,T1" + ATeammates(py), "GSH2LoseEff", "0");
            if (target != VI.CinSentinel)
            {
                ushort who = ushort.Parse(target);
                Harm("GSH2", XI.Board.Garden[who], 4, (long)HPEvoMask.TUX_INAVO);
            }
        }
        public void GSH3IncrAction(Player player)
        {
            XI.Board.PetProtecedPlayer.Add(player.Uid);
        }
        public void GSH3DecrAction(Player player)
        {
            XI.Board.PetProtecedPlayer.Remove(player.Uid);
        }
        public void GSH3ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                     p.Uid != player.Uid && XI.Board.IsAttendWar(p) && p.GetPetCount() > 0).ToList();
                ISet<int> props = new HashSet<int>();
                foreach (Player py in invs)
                {
                    for (int i = 0; i < FiveElementHelper.PropCount; ++i)
                    {
                        if (py.Pets[i] != 0) props.Add(i);
                    }
                }
                if (type == 0)
                    XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",GSH3," + props.Count);
                else if (type == 1)
                { // FI
                    if (XI.Board.IsAttendWar(player))
                        XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",GSH3," + props.Count);
                    else
                        XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",GSH3,0");
                }
                else if (type == 2)
                    XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",GSH3," + props.Count);
                else if (type == 3)
                    XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",GSH3,0");
            }
        }
        public bool GSH3ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                bool yesIncr = XI.Board.Garden.Values.Any(p =>
                    p.Uid != player.Uid && XI.Board.IsAttendWar(p) && p.GetPetCount() > 0);
                if (type == 0) // Z1
                    return XI.Board.IsAttendWar(player) && yesIncr;
                else if (type == 1 && XI.Board.InFight) // FI
                {
                    string[] g0fi = fuse.Split(',');
                    if (g0fi[1] == "O" || g0fi[1] == "U")
                        return false;
                    int playerIn = 0;
                    bool leaver = false;
                    for (int i = 1; i < g0fi.Length; i += 3)
                    {
                        char ch = g0fi[i][0];
                        ushort old = ushort.Parse(g0fi[i + 1]);
                        ushort to = ushort.Parse(g0fi[i + 2]);
                        if (old == player.Uid)
                            --playerIn;
                        else
                        {
                            Player po = XI.Board.Garden[old];
                            if (po != null && po.GetPetCount() > 0) { leaver = true; break; }
                        }
                        if (to == player.Uid)
                            ++playerIn;
                        else
                        {
                            Player pt = XI.Board.Garden[to];
                            if (pt != null && pt.GetPetCount() > 0) { leaver = true; break; }
                        }
                    }
                    return leaver || playerIn != 0;
                }
                else if (type == 2 || type == 3) // IC/OC
                {
                    if (XI.Board.InFightThrough && XI.Board.IsAttendWar(player) && yesIncr)
                    {
                        string[] iocs = fuse.Split(',');
                        for (int idx = 1; idx < iocs.Length; idx += 3)
                        {
                            ushort who = ushort.Parse(iocs[idx + 1]);
                            ushort petUt = ushort.Parse(iocs[idx + 2]);
                            Monster pet = XI.LibTuple.ML.Decode(petUt);
                            if (who == player.Uid && pet != null && pet.Code == "GSH3")
                                return true;
                        }
                    }
                }
            }
            return false;
        }
        public void GHH1WinEff()
        {
            Harm("GHH1", XI.Board.Garden.Values.Where(p => p.Team == XI.Board.Rounder.OppTeam && p.IsAlive), 2);
            if (XI.Board.Hinder.IsValidPlayer() && XI.Board.Hinder.Tux.Count > 0)
            {
                string ns = XI.Board.Hinder.Tux.Count > 1 ? ("1~" + XI.Board.Hinder.Tux.Count) : "1";
                string sel = XI.AsyncInput(XI.Board.Hinder.Uid, "#弃置以回复HP,/Q" + ns +
                    "(p" + string.Join("p", XI.Board.Hinder.Tux) + ")", "GHH1WinEff", "0");
                if (!sel.Contains(VI.CinSentinel) && !sel.StartsWith("/"))
                {
                    ushort[] tuxes = sel.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    XI.RaiseGMessage("G0QZ," + XI.Board.Hinder.Uid + "," + sel);
                    Cure("GHH1", XI.Board.Garden.Values.Where(p => p.Team == XI.Board.Rounder.OppTeam && p.IsAlive), tuxes.Length);
                }
            }
        }
        public void GHH1LoseEff()
        {
            Harm("GHH1", XI.Board.Garden.Values.Where(p => p.Team == XI.Board.Rounder.Team && p.IsAlive), 2);
            if (XI.Board.Rounder.IsAlive && XI.Board.Rounder.Tux.Count > 0)
            {
                string ns = XI.Board.Rounder.Tux.Count > 1 ? ("1~" + XI.Board.Rounder.Tux.Count) : "1";
                string sel = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置以回复HP,/Q" + ns +
                    "(p" + string.Join("p", XI.Board.Rounder.Tux) + ")", "GHH1LoseEff", "0");
                if (!sel.Contains(VI.CinSentinel) && !sel.StartsWith("/"))
                {
                    ushort[] tuxes = sel.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    XI.RaiseGMessage("G0QZ," + XI.Board.Rounder.Uid + "," + sel);
                    Cure("GHH1", XI.Board.Garden.Values.Where(p => p.Team == XI.Board.Rounder.Team && p.IsAlive), tuxes.Length);
                }
            }
        }
        public void GHH1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                string[] uts = argst.Split(',');
                ushort card = ushort.Parse(uts[0]);
                ushort to = ushort.Parse(uts[1]);
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                Harm("GHH1", XI.Board.Garden[to], 2);
            }
        }
        public bool GHH1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
                return player.Tux.Count > 0;
            return false;
        }
        public string GHH1ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 0)
            {
                if (prev == "")
                    return "/Q1(p" + string.Join("p", player.Tux) + "),/T1(p" +
                        string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")";
                else
                    return "";
            }
            return "";
        }
        public void GHH2WinEff()
        {
            if (XI.Board.Rounder.Tux.Count > 0)
                XI.RaiseGMessage("G0QZ," + XI.Board.Rounder.Uid + "," + string.Join(",", XI.Board.Rounder.Tux));
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GHH2", XI.Board.Hinder, 2);
        }
        public void GHH2LoseEff()
        {
            Harm("GHH2", XI.Board.Garden.Values.Where(p => p.IsAlive), 1);
        }
        public void GHH2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
                Harm("GHH2", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam), 2);
            else if (consumeType == 2)
            {
                Monster ghh2 = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHH2"));
                if (type == 0) // G09P,95;Hack the round result?
                {
                    // if (args[1] == "0")
                    // {
                    //     Board.HinderSucc = Board.Hinder.DEXi > 0 ||
                    //         (Board.Hinder.DEXi == 0 && (Board.Hinder.DEX >= Board.Battler.AGL));
                    //     Board.SupportSucc = Board.Supporter.DEXi > 0 ||
                    //         (Board.Supporter.DEXi == 0 && (Board.Supporter.DEX >= Board.Battler.AGL));
                    //     WI.BCast("E09P,0," + Board.Supporter.Uid + (Board.SupportSucc ? ",1," : ",0,")
                    //             + Board.Hinder.Uid + (Board.HinderSucc ? ",1" : ",0"));
                    // }
                    // if (args[1] == "0" || args[1] == "1")
                    // {
                    //     int rpool = RPool;
                    //     if (XI.Board.Rounder.STR)
                    //     int rpool = Math.Max(Rounder.STR + RPool + (SupportSucc ? Supporter.STR : 0), 0)
                    //     WI.BCast("E09P,1," + Board.Rounder.Team + "," + Board.CalculateRPool()
                    //             + "," + Board.Rounder.OppTeam + "," + Board.CalculateOPool());
                    // }
                }
                else if (type == 1) // G0CC,110
                {
                    string[] g0cc = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                    ushort who = ushort.Parse(g0cc[1]);
                    // return !ghh2.RAMUtList.Contains(who);
                }
            }
        }
        public bool GHH2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N > 0 && p.Element == FiveElement.AGNI);
            else if (consumeType == 2)
                return true;
            return false;
        }
        public void GLH1Debut()
        {
            ushort glh1 = XI.LibTuple.ML.Encode("GLH1");
            int cnt = XI.Board.Garden.Values.Count(p => !p.IsAlive || p.HP < p.HPb);
            if (cnt > 0)
                XI.RaiseGMessage("G0OW," + glh1 + "," + cnt);
        }
        public void GLH1WinEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.Team && p.GetPetCount() > 0).ToList();
            if (invs.Any())
                XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p.Uid + ",0," + p.GetPetCount())));
        }
        public void GLH1LoseEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.Team && p.Tux.Count > 0 && XI.Board.Facer(p).IsAlive).ToList();
            if (invs.Any())
            {
                IDictionary<ushort, string> ans = XI.MultiAsyncInput(invs.ToDictionary(p => p.Uid,
                    p => "#交给对面的,Q1(p" + string.Join("p", p.Tux) + ")"));
                ans.ToList().ForEach(p => XI.RaiseGMessage("G0HQ,0," + XI.Board.Facer(
                    XI.Board.Garden[p.Key]).Uid + "," + string.Join(",", p.Key + ",1,1," + p.Value)));
            }
        }
        public void GLH1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                if (type == 0)
                {
                    player.RFM.GetOrSetDiva("GLH1Consume").Set("endEGR", 1);
                    XI.RaiseGMessage("G0JM,R" + player.Uid + "GS");
                }
                else if (type == 1)
                {
                    player.RFM.GetOrSetDiva("GLH1Consume").Set("endEGR", 2);
                    XI.RaiseGMessage("G0JM,R" + player.Uid + "QR");
                }
            }
        }
        public bool GLH1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                if (type == 0)
                    return player.RFM.GetOrSetDiva("GLH1Consume").GetInt("endEGR") == 0;
                else if (type == 1)
                    return player.RFM.GetOrSetDiva("GLH1Consume").GetInt("endEGR") == 1;
            }
            return false;
        }
        public void GLH2WinEff()
        {
            var rd = XI.Board.Rounder;
            while (rd.Tux.Count >= 2)
            {
                string cardsstr = XI.AsyncInput(rd.Uid, "#弃置以令1人横置,/Q2(p" + string.Join("p", rd.Tux) +
                    "),#横置,/T1" + FormatPlayers(p => p.IsAlive), "GLH2WinEff", "0");
                if (!string.IsNullOrEmpty(cardsstr) && !cardsstr.Contains(VI.CinSentinel) && !cardsstr.StartsWith("/"))
                {
                    string[] parts = cardsstr.Split(',');
                    XI.RaiseGMessage("G0QZ," + rd.Uid + "," + parts[0] + "," + parts[1]);
                    XI.RaiseGMessage("G0DS," + parts[2] + ",0,1");
                }
                else break;
            }
        }
        public void GLH2LoseEff()
        {
            string select = XI.AsyncInput(XI.Board.Opponent.Uid, "#不被横置的,T1" +
                AAlls(XI.Board.Opponent), "GLH2LoseEff", "0");
            if (!string.IsNullOrEmpty(select) && !select.Contains(VI.CinSentinel))
            {
                ushort who = ushort.Parse(select);
                XI.Board.Garden.Values.Where(p => p.IsAlive && p.Uid != who).ToList().ForEach(
                    p => XI.RaiseGMessage("G0DS," + p.Uid + ",0,1"));
            }
        }
        public void GLH2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                ushort ut = ushort.Parse(argst);
                int value = XI.Board.Garden[ut].STR;
                XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1," + value);
            }
        }
        public bool GLH2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1 && XI.Board.Rounder.DEX >= XI.Board.Battler.AGL)
                return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team != player.Team && !XI.Board.IsAttendWar(p));
            return false;
        }
        public string GLH2ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 1)
            {
                if (prev == "")
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive &&
                        p.Team != player.Team && !XI.Board.IsAttendWar(p)).Select(p => p.Uid)) + ")";
                else
                    return "";
            }
            return "";
        }
        public void GFH1Debut()
        {
            XI.Board.FightTangled = true;
        }
        private void GFH1GeneralEffAction(Player decider, string prev)
        {
            IDictionary<ushort, List<ushort>> ans = new Dictionary<ushort, List<ushort>>();
            List<Player> opps = XI.Board.Garden.Values.Where(p => p.Team == decider.OppTeam && p.IsAlive).ToList();
            List<ushort> allEnemy = opps.SelectMany(p => p.Pets).Where(p => p != 0).ToList();
            foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == decider.Team))
            {
                for (int i = 0; i < py.Pets.Length; ++i)
                {
                    if (py.Pets[i] != 0)
                    {
                        int str = XI.LibTuple.ML.Decode(py.Pets[i]).STR;
                        if (opps.Any(p => p.Pets[i] != 0) || allEnemy.Any(p => XI.LibTuple.ML.Decode(p).STR == str))
                            Algo.AddToMultiMap(ans, py.Uid, py.Pets[i]);
                    }
                }
            }
            ushort mySide = 0, myChange = 0;
            if (ans.Count > 0)
            {
                do
                {
                    string opt1 = XI.AsyncInput(decider.Uid, "#我方交换宠物,/T1(p" +
                        string.Join("p", ans.Keys) + ")", prev, "0");
                    if (opt1 != VI.CinSentinel && !opt1.StartsWith("/"))
                    {
                        mySide = ushort.Parse(opt1);
                        string opt2 = XI.AsyncInput(decider.Uid, "#我方交换宠物,/M1(p" +
                            string.Join("p", ans[mySide]) + ")", prev, "1");
                        if (opt2 != VI.CinSentinel && !opt2.StartsWith("/"))
                            myChange = ushort.Parse(opt2);
                    }
                    else
                        mySide = 0;
                } while (myChange == 0 && mySide != 0);
            }
            if (mySide != 0 && myChange != 0)
            {
                ushort aySide = 0, ayChange = 0;
                Monster pet = XI.LibTuple.ML.Decode(myChange);
                int elemIdx = pet.Element.Elem2Index();
                IDictionary<ushort, List<ushort>> bns = new Dictionary<ushort, List<ushort>>();
                foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == decider.OppTeam))
                {
                    if (py.Pets[elemIdx] != 0)
                        Algo.AddToMultiMap(bns, py.Uid, py.Pets[elemIdx]);
                    foreach (ushort pt in py.Pets)
                    {
                        if (pt != 0 && XI.LibTuple.ML.Decode(pt).STR == pet.STR)
                            Algo.AddToMultiMap(bns, py.Uid, pt);
                    }
                }
                do
                {
                    string opt3 = XI.AsyncInput(decider.Uid, "#交换对方宠物,T1(p" +
                        string.Join("p", bns.Keys) + ")", prev, "2");
                    if (opt3 != VI.CinSentinel)
                    {
                        aySide = ushort.Parse(opt3);
                        string opt4 = XI.AsyncInput(decider.Uid, "#交换对方宠物,/M1(p" +
                            string.Join("p", bns[aySide].Distinct()) + ")", prev, "3");
                        if (opt4 != VI.CinSentinel && !opt4.StartsWith("/"))
                            ayChange = ushort.Parse(opt4);
                    }
                } while (ayChange == 0);
                if (aySide != 0 && ayChange != 0)
                {
                    XI.RaiseGMessage(new Artiad.TradePet()
                    {
                        A = mySide, ASinglePet = myChange, B = aySide, BSinglePet = ayChange
                    }.ToMessage());
                }
            }
        }
        public void GFH1WinEff()
        {
            Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            if (mon1 != null && mon1.Code == "GFH1")
                XI.Board.Mon1Catchable = false;
            Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            if (mon2 != null && mon2.Code == "GFH1")
                XI.Board.Mon2Catchable = false;
            GFH1GeneralEffAction(XI.Board.Rounder, "GFH1WinEff");
        }
        public void GFH1LoseEff()
        {
            GFH1GeneralEffAction(XI.Board.Opponent, "GFH1LoseEff");
        }
        public void GFH2WinEff()
        {
            Player rd = XI.Board.Rounder;
            List<Player> pms = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.HP < p.HPb && p.Team == rd.Team).ToList();
            if (pms.Count > 0)
            {
                int minValue = pms.Min(p => p.HP);
                Cure("GFH2", pms.Where(p => p.HP == minValue), 2);
            }
        }
        public void GFH2LoseEff()
        {
            Player rd = XI.Board.Rounder;
            List<Player> pms = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == rd.Team).ToList();
            if (pms.Count > 0)
            {
                int maxValue = pms.Max(p => p.HP);
                Harm("GFH2", pms.Where(p => p.HP == maxValue), 2);
            }
        }
        public void GFH2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                if (type == 0)
                {
                    XI.RaiseGMessage("G0TT," + player.Uid);
                    if (XI.Board.DiceValue % 2 == 0)
                    {
                        string choose = XI.AsyncInput(player.Uid, "#请选择【大眼蛙】宠物效果。" +
                            "##战力+1##命中+1,Y2", "GFH2Consume", "0");
                        if (choose == "2")
                            XI.RaiseGMessage("G0IX," + player.Uid + ",1,1");
                        else
                            XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
                    }
                }
                else if (type == 1 || type == 2)
                {
                    if (player.DEX > 0)
                        XI.RaiseGMessage("G0OX," + player.Uid + ",1," + player.DEX);
                }
            }
        }
        public bool GFH2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                if (type == 0)
                    return XI.Board.IsAttendWar(player);
                else if (type == 1 || type == 2)
                {
                    Monster mon = XI.LibTuple.ML.Decode(type == 1 ? XI.Board.Monster1 : XI.Board.Monster2);
                    return mon != null && mon.Level == Monster.ClLevel.BOSS;
                }
            }
            return false;
        }
        public void GTH1Debut()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetEquipCount() > 0).ToList();
            IDictionary<ushort, string> requires = new Dictionary<ushort, string>();
            foreach (Player py in pys)
                requires.Add(py.Uid, "#须弃置,Q1(p" + string.Join("p", py.ListOutAllEquips()) + ")");
            IDictionary<ushort, string> inputs = XI.MultiAsyncInput(requires);
            foreach (var pair in inputs)
                XI.RaiseGMessage("G0QZ," + pair.Key + "," + pair.Value);
        }
        public void GTH1WinEff()
        {
            Player rd = XI.Board.Rounder;
            XI.RaiseGMessage("G0DH," + rd.Uid + ",0,1");
            int saturn = FiveElement.SATURN.Elem2Index();
            if (rd.Pets[saturn] != 0)
            {
                ushort gth1code = XI.LibTuple.ML.Encode("GTH1");
                XI.RaiseGMessage(new Artiad.HarvestPet()
                {
                    Farmer = rd.Uid,
                    SinglePet = gth1code,
                    Trophy = true,
                    TreatyAct = Artiad.HarvestPet.Treaty.PASSIVE
                }.ToMessage());
                if (XI.Board.Monster1 == gth1code)
                    XI.Board.Monster1 = 0;
                else if (XI.Board.Monster2 == gth1code)
                    XI.Board.Monster2 = 0;
            }
        }
        public void GTH1LoseEff()
        {
            if (XI.Board.Supporter.IsValidPlayer())
                Harm("GTH1", XI.Board.Supporter, 3);
            var lv = XI.Board.Garden.Values;
            int saturn = FiveElement.SATURN.Elem2Index();
            List<Player> others = lv.Where(p =>
                p.Team == XI.Board.Rounder.Team && p.Pets[saturn] != 0).ToList();
            if (others.Any() && XI.Board.Mon1From == 0)
            {
                string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#要替换的,/M1(p" + string.Join("p",
                    others.Select(p => p.Pets[saturn])) + ")", "GTH1LoseEff", "0");
                if (input != VI.CinSentinel && !input.StartsWith("/"))
                {
                    ushort mons = ushort.Parse(input);
                    ushort gth1code = XI.LibTuple.ML.Encode("GTH1");
                    Player py = others.Single(p => p.Pets[saturn] == mons);
                    XI.RaiseGMessage(new Artiad.HarvestPet()
                    {
                        Farmer = py.Uid,
                        SinglePet = gth1code,
                        Trophy = true,
                        TreatyAct = Artiad.HarvestPet.Treaty.PASSIVE
                    }.ToMessage());
                    if (XI.Board.Monster1 == gth1code)
                        XI.Board.Monster1 = 0;
                    else if (XI.Board.Monster2 == gth1code)
                        XI.Board.Monster2 = 0;
                }
            }
        }
        public void GTH1ConsumeAction(Player player, int consumeType, int type, string fuse, string args)
        {
            if (consumeType == 0)
            {
                ushort me = XI.LibTuple.ML.Encode("GTH1");
                Monster monster = XI.LibTuple.ML.Decode(me) as Monster;
                if (type == 0)
                {
                    if (player.Pets.Any(p => p != 0 && p != me))
                    {
                        XI.RaiseGMessage("G0OB," + me + "," + monster.ROMUshort);
                        monster.ROMUshort = 0;
                    }
                    else
                    {
                        int delta = player.STR - (int)monster.ROMUshort;
                        if (delta < 0)
                            XI.RaiseGMessage("G0OB," + me + "," + (-delta));
                        else if (delta > 0)
                            XI.RaiseGMessage("G0IB," + me + "," + delta);
                        monster.ROMUshort = (ushort)player.STR;
                    }
                }
                else if (type == 1)
                {
                    XI.RaiseGMessage("G0IB," + me + "," + player.STR);
                    monster.ROMUshort = (ushort)player.STR;
                }
                else if (type == 2 || type == 3 || type == 4)
                {
                    int delta = player.STR - (int)monster.ROMUshort;
                    if (delta < 0)
                        XI.RaiseGMessage("G0OB," + me + "," + (-delta));
                    else if (delta > 0)
                        XI.RaiseGMessage("G0IB," + me + "," + delta);
                    monster.ROMUshort = (ushort)player.STR;
                }//g0ax
            }
        }
        public bool GTH1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                ushort me = XI.LibTuple.ML.Encode("GTH1");
                Monster monster = XI.LibTuple.ML.Decode(me) as Monster;
                bool anyOther = player.Pets.Any(p => p != 0 && p != me);
                if (type == 0) // HD, case 1: obtain others then eliminate gth1; case 2: obtain gth1
                {
                    Artiad.ObtainPet otp = Artiad.ObtainPet.Parse(fuse);
                    return (anyOther && monster.ROMUshort != 0) || (!anyOther && player.STR > 0 &&
                        otp.Farmer == player.Uid && otp.SinglePet == me);
                }
                else if (type == 1 && monster.ROMUshort == 0 && player.STR > 0)
                    return !anyOther && player.STR > 0;
                else if (type == 2 || type == 3 || type == 4)
                    return !anyOther && monster.ROMUshort != player.STR;
            }
            return false;
        }
        public void GTH2Debut()
        {
            // 5 -> 3 -> 1 -> 2 -> 4 -> 6
        }
        public void GTH2WinEff()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0).ToList();
            if (pys.Any())
                XI.RaiseGMessage("G1XR,1,0,0," + string.Join(",", pys.Select(p => p.Uid)));
            pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count != 3).ToList();
            if (pys.Any())
            {
                XI.RaiseGMessage("G0DH," + string.Join(",", pys.Select(p => p.Tux.Count > 3 ? (
                    p.Uid + ",1," + (p.Tux.Count - 3)) : (p.Uid + ",0," + (3 - p.Tux.Count)))));
            }
        }
        public void GTH2LoseEff()
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count != 3).ToList();
            if (pys.Any())
            {
                XI.RaiseGMessage("G0DH," + string.Join(",", pys.Select(p => p.Tux.Count > 3 ? (
                    p.Uid + ",1," + (p.Tux.Count - 3)) : (p.Uid + ",0," + (3 - p.Tux.Count)))));
            }
            pys = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Tux.Count > 0).ToList();
            if (pys.Any())
                XI.RaiseGMessage("G1XR,1,0,0," + string.Join(",", pys.Select(p => p.Uid)));
        }
        #endregion Package HL

        #region Monster Effect Util

        private void Harm(string monCode, Player py, int n, long mask = 0)
        {
            mask = HPEvoMask.FROM_NMB.Set(mask);
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                new Artiad.Harm(py.Uid, (monValue + 1000), mon.Element, n, mask)));
        }

        private void Harm(string monCode, IEnumerable<Player> invs, int n, long mask = 0)
        {
            mask = HPEvoMask.FROM_NMB.Set(mask);
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(invs.Select(p =>
                new Artiad.Harm(p.Uid, (monValue + 1000), mon.Element, n, mask))));
        }

        private void Harm(string monCode, List<Player> invs, List<int> ns, long mask = 0)
        {
            mask = HPEvoMask.FROM_NMB.Set(mask);
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            int sz = invs.Count;
            Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Artiad.Harm(invs[p].Uid, (monValue + 1000), mon.Element, ns[p], mask))));
        }

        private void Cure(string monCode, Player py, int n, long mask = 0)
        {
            mask = HPEvoMask.FROM_NMB.Set(mask);
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                new Artiad.Cure(py.Uid, (monValue + 1000), mon.Element, n, mask)));
        }
        private void Cure(string monCode, IEnumerable<Player> invs, int n, long mask = 0)
        {
            mask = HPEvoMask.FROM_NMB.Set(mask);
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(invs.Select(p =>
                new Artiad.Cure(p.Uid, (monValue + 1000), mon.Element, n, mask))));
        }

        private void Cure(string monCode, List<Player> invs, List<int> ns, long mask = 0)
        {
            mask = HPEvoMask.FROM_NMB.Set(mask);
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            int sz = invs.Count;
            Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Artiad.Cure(invs[p].Uid, (monValue + 1000), mon.Element, ns[p], mask))));
        }

        #endregion Monster Effect Util
    }
}
