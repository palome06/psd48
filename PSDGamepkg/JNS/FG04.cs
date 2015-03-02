using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base.Card;
using PSD.Base;
using PSD.PSDGamepkg.Artiad;

namespace PSD.PSDGamepkg.JNS
{
    public class MonsterCottage
    {
        private Base.VW.IVI VI { set; get; }
        //private VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public MonsterCottage(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }

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
                    mon.Debut += new Monster.DebutDelegate(delegate()
                    {
                        methodDebut.Invoke(mc, new object[] { });
                    });
                var methodCurtain = mc.GetType().GetMethod(monCode + "Curtain");
                if (methodCurtain != null)
                    mon.Curtain += new Monster.DebutDelegate(delegate()
                    {
                        methodCurtain.Invoke(mc, new object[] { });
                    });
                var methodWin = mc.GetType().GetMethod(monCode + "WinEff");
                if (methodWin != null)
                    mon.WinEff += new Monster.WLDelegate(delegate()
                    {
                        methodWin.Invoke(mc, new object[] { });
                    });
                var methodLose = mc.GetType().GetMethod(monCode + "LoseEff");
                if (methodLose != null)
                    mon.LoseEff += new Monster.WLDelegate(delegate()
                    {
                        methodLose.Invoke(mc, new object[] { });
                    });
                var methodConsumeAction = mc.GetType().GetMethod(monCode + "ConsumeAction");
                if (methodConsumeAction != null)
                    mon.ConsumeAction += new Monster.CsActionDelegate(delegate(Player player, int consumeType, int type, string fuse, string argst)
                    {
                        methodConsumeAction.Invoke(mc, new object[] { player, consumeType, type, fuse, argst });
                    });
                var methodIncrAction = mc.GetType().GetMethod(monCode + "IncrAction");
                if (methodIncrAction != null)
                    mon.IncrAction += new Monster.CrActionDelegate(delegate(Player player)
                    {
                        methodIncrAction.Invoke(mc, new object[] { player });
                    });
                var methodDecrAction = mc.GetType().GetMethod(monCode + "DecrAction");
                if (methodDecrAction != null)
                    mon.DecrAction += new Monster.CrActionDelegate(delegate(Player player)
                    {
                        methodDecrAction.Invoke(mc, new object[] { player });
                    });
                var methodConsumeValid = mc.GetType().GetMethod(monCode + "ConsumeValid");
                if (methodConsumeValid != null)
                    mon.ConsumeValid += new Monster.CsValidDelegate(delegate(Player player, int consumeType, int type, string fuse)
                    {
                        return (bool)methodConsumeValid.Invoke(mc, new object[] { player, consumeType, type, fuse });
                    });
                var methodConsumeInput = mc.GetType().GetMethod(monCode + "ConsumeInput");
                if (methodConsumeInput != null)
                    mon.ConsumeInput += new Monster.CsInputDelegate(delegate(Player player, int consumeType, int type, string fuse, string prev)
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
            if (hd.IsAlive && hd.Uid != 0)
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
                string input = XI.AsyncInput(rd.Uid, "#须弃置的,C1(p" + string.Join("p",
                    rd.ListOutAllEquips()) + ")", "GS02LoseEff", "0");
                ushort which = ushort.Parse(input);
                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + which);
            }
        }
        public void GS02IncrAction(Player player)
        {
            ushort x = XI.LibTuple.ML.Encode("GS02");
            if (x != 0)
            {
                Monster monster = XI.LibTuple.ML.Decode(x);
                XI.RaiseGMessage("G0IB," + x + "," + (8 - monster.STR));
            }
        }
        public void GS02DecrAction(Player player)
        {
            ushort x = XI.LibTuple.ML.Encode("GS02");
            if (x != 0)
                XI.RaiseGMessage("G0WB," + x);
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
            if (hd.IsAlive && hd.Uid != 0 && hd.HasAnyCards())
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
            if (hd.IsAlive && hd.Uid != 0 && rd.IsAlive && rd.Uid != 0 && rd.HasAnyCards())
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
                for (int i = 1; i < blocks.Length; )
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
                for (int i = 1; i < blocks.Length; )
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
                ushort tg = ushort.Parse(XI.AsyncInput(player.Uid, ic, "GF04ConsumeAction", "1"));

                if (zeros.Contains(tg))
                {
                    VI.Cout(0, "{0}爆发「彩依」，使得{1}满HP复活.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(tg));
                    Player tgp = XI.Board.Garden[tg];
                    Cure("GF04", tgp, tgp.HPb, FiveElement.SOL);
                }
                zeros = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
                if (zeros.Count > 0)
                    XI.InnerGMessage("G0ZH,0", 0);
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
                ushort ut = (ushort)(fuse[1] - '0');
                Player rp = XI.Board.Garden[ut];
                if (rp.Team == player.Team)
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
            var lv = XI.Board.Garden.Values;
            List<Player> others = lv.Where(p => p.Team == XI.Board.Rounder.OppTeam && p.Pets[4] != 0).ToList();
            if (others.Any() && XI.Board.Mon1From == 0)
            {
                string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#要替换的,/M1(p" + string.Join("p",
                    others.Select(p => p.Pets[4])) + ")", "GT04LoseEff", "0");
                if (input != "0" && input != "" && !input.StartsWith("/"))
                {
                    ushort mons = ushort.Parse(input);
                    var sg = others.Where(p => p.Pets[4] == mons);
                    if (sg.Any())
                    {
                        Player py = sg.Single();
                        XI.RaiseGMessage("G0HL," + py.Uid + "," + mons);
                        XI.RaiseGMessage("G0ON," + py.Uid + ",M,1," + mons);
                        ushort gt04code = XI.LibTuple.ML.Encode("GT04");
                        XI.RaiseGMessage("G0HD,1," + py.Uid + ",0," + gt04code);
                        if (XI.Board.Monster1 == gt04code)
                            XI.Board.Monster1 = 0;
                        else if (XI.Board.Monster2 == gt04code)
                            XI.Board.Monster2 = 0;
                    }
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
                    .Select(p => p.Pets[1]).Where(p => p != 0 && XI.LibTuple.ML.Decode(p) != null)
                    .Sum(p => XI.LibTuple.ML.Decode(p).STR);
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
                    mon.RAMUshort = (ushort)player.Team;
                    XI.RaiseGMessage("G0IP," + player.Team + ",3");
                }
            }
        }
        public bool GHT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT1"));
                return mon != null && mon.RAMUshort != player.Team;
            }
            return false;
        }
        public void GHT1WinEff()
        {
            if (XI.Board.Hinder.IsValidPlayer())
                Harm("GHT1", XI.Board.Hinder, 2);
            bool anyAgni = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == XI.Board.Rounder.Team)
                    .Select(p => p.Pets[1]).Where(p => p != 0).Any();
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
                if (mon.RAMUshort == 0)
                {
                    string hint = "#请选择「镇狱明王」宠物效果。##战-1，命+2##命-1，战+2,/Y2";
                    string option = XI.AsyncInput(player.Uid, hint, "GHT2IncrAction", "0");
                    if (option == "1")
                        mon.RAMUshort = 1;
                    else if (option == "2")
                        mon.RAMUshort = 2;
                }
                if (mon.RAMUshort == 1)
                {
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0,2");
                }
                else if (mon.RAMUshort == 2)
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
                if (mon.RAMUshort == 1)
                {
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0,2");
                }
                else if (mon.RAMUshort == 2)
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
            string result = Util.SParal(XI.Board, p => p.IsAlive && p.Tux.Count > 0, p => p.Uid + ",1,1", ",");
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH," + result);
        }
        public void GFT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE &&
                            !Artiad.Harm.GetPropedElement().Contains(harm.Element) && harm.Source != harm.Who)
                        rvs.Add(harm);
                }
                harms.RemoveAll(p => rvs.Contains(p));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -9);
            }
        }
        public bool GFT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE &&
                        !Artiad.Harm.GetPropedElement().Contains(harm.Element) && harm.Source != harm.Who)
                        return true;
                }
                return false;
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
                    if (ut == player.Uid && gtype == 0 && n >= 1)
                        g0dh += "," + ut + ",0," + (n - 1);
                    else
                        g0dh += "," + ut + "," + gtype + "," + n;
                }
                if (g0dh.Length > 0)
                    XI.InnerGMessage("G0DH" + g0dh, 91);

                string st = XI.AsyncInput(player.Uid, "#获得手牌的,T1(p" + string.Join("p", XI.Board.Garden.
                    Values.Where(p => p.Uid != player.Uid && p.Tux.Count > 0).Select(p => p.Uid)) + ")",
                    "GTT1", "0");
                ushort from = ushort.Parse(st);
                string c0 = Util.RepeatString("p0", XI.Board.Garden[from].Tux.Count);
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
                    foreach (ushort ut in pick)
                    {
                        foreach (Player py in XI.Board.Garden.Values)
                        {
                            if (py.Pets.Contains(ut))
                            {
                                XI.RaiseGMessage("G0HL," + py.Uid + "," + ut);
                                XI.RaiseGMessage("G0ON," + py.Uid + ",M,1," + ut);
                                break;
                            }
                        }
                    }
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
                    for (int i = 1; i < parts.Length; )
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
                    for (int i = 1; i < parts.Length; )
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
            string ques = XI.AsyncInput(h.Uid, "#您是否弃掉所有手牌解除定身？##是##否,Y2", "GST4LoseEff", "0");
            if (ques == "1")
            {
                if (h.Tux.Count > 0)
                    XI.RaiseGMessage("G0DH," + h.Uid + ",2," + h.Tux.Count);
                XI.RaiseGMessage("G0DS," + h.Uid + ",1");
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
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT2"));
            if (mon != null)
            {
                if (mon.RAMUshort == 0)
                {
                    string hint = "#请选择「炎舞」效果免疫伤害的属性。##水##火##雷##风##土,Y5";
                    string option = XI.AsyncInput(player.Uid, hint, "GHT4IncrAction", "0");
                    mon.RAMUshort = ushort.Parse(option);
                }
            }
        }
        public void GHT4DecrAction(Player player)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT2"));
            if (mon != null)
                mon.RAMUshort = 0;
        }
        public void GHT4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT2"));
                if (mon != null)
                {
                    List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                    foreach (Artiad.Harm harm in harms)
                    {
                        int elemCode = Util.GetFiveElementId(harm.Element) + 1;
                        if (harm.Who == player.Uid && elemCode == mon.RAMUshort)
                            rvs.Add(harm);
                    }
                    harms.RemoveAll(p => rvs.Contains(p));
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
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GHT2"));
                if (mon != null)
                {
                    foreach (Artiad.Harm harm in harms)
                    {
                        int elemCode = Util.GetFiveElementId(harm.Element) + 1;
                        if (harm.Who == player.Uid && elemCode == mon.RAMUshort)
                            return true;
                    }
                }
                return false;
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
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE &&
                            Artiad.Harm.GetPropedElement().Contains(harm.Element))
                        rvs.Add(harm);
                }
                harms.RemoveAll(p => rvs.Contains(p));
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
        }
        public bool GLT3ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE &&
                            Artiad.Harm.GetPropedElement().Contains(harm.Element))
                        return true;
                }
                return false;
            }
            return false;
        }
        public void GLT3WinEff()
        {
            string target = XI.AsyncInput(XI.Board.Rounder.Uid, "#补牌,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")", "GLT3WinEff", "0");
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
                    mon.RAMUshort = (ushort)player.Team;
                string[] g1ev = fuse.Split(',');
                XI.InnerGMessage("G1EV," + g1ev[1] + "," + g1ev[2], 201);
            }
        }
        public bool GLT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.LibTuple.ML.Encode("GLT4"));
                return mon != null && mon.RAMUshort != player.Team;
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
                Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                if (mon1 != null && mon1.Code == "GLT4")
                    XI.Board.Mon1Catchable = false;
                Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
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
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.Team).ToList();
            int total = 4;
            IDictionary<Player, int> sch = new Dictionary<Player, int>();
            while (total > 0)
            {
                if (invs.Count == 1)
                {
                    string word = "#HP回复,T1(p" + invs[0].Uid + "),#回复数值,D" + total;
                    XI.AsyncInput(XI.Board.Rounder.Uid, word, "GFT3WinEff", "0");
                    sch[invs[0]] = total;
                    total = 0; invs.Clear();
                }
                else
                {
                    string ichi = total == 1 ? "/D1" : ("/D1~" + total);
                    string word = "#HP回复,T1(p" + string.Join("p",
                        invs.Select(p => p.Uid)) + "),#回复数值," + ichi;
                    string input = XI.AsyncInput(XI.Board.Rounder.Uid, word, "GFT3WinEff", "0");
                    if (!input.Contains("/"))
                    {
                        string[] ips = input.Split(',');
                        ushort ut = ushort.Parse(ips[0]);
                        int zn = int.Parse(ips[1]);
                        Player py = XI.Board.Garden[ut];
                        sch[py] = zn;
                        total -= zn;
                        invs.Remove(py);
                    }
                }
            }
            //List<Player> invs = new List<Player>();
            //foreach (ushort ut in XI.Board.OrderedPlayer())
            //{
            //    Player py = XI.Board.Garden[ut];
            //    if (py.IsAlive && py.Team == XI.Board.Rounder.Team)
            //        invs.Add(py);
            //}
            //int total = 4;
            //IDictionary<Player, int> sch = new Dictionary<Player, int>();
            //for (int i = 0; i < invs.Count; ++i)
            //{
            //    Player py = invs[i];
            //    if (total <= 0)
            //        break;
            //    string word;
            //    if (i == invs.Count - 1)
            //        word = "T1";
            //    else
            //        word = "/T1";
            //    string zero = XI.AsyncInput(XI.Board.Rounder.Uid,
            //        "#HP回复," + word + "(p" + py.Uid + ")", "GFT3WinEff", "0");
            //    if (!zero.StartsWith("/"))
            //    {
            //        if (i == invs.Count - 1)
            //            word = "D" + total;
            //        else if (total > 1)
            //            word = "/D1~" + total;
            //        else
            //            word = "/D1";

            //        string alloc = XI.AsyncInput(XI.Board.Rounder.Uid,
            //            "#HP回复," + word, "GFT3WinEff", "0");
            //        if (!alloc.StartsWith("/"))
            //        {
            //            int n = int.Parse(alloc);
            //            sch[py] = n;
            //            total -= n;
            //        }
            //    }
            //}
            Cure("GFT3", sch.Keys.ToList(), sch.Values.ToList());
        }
        public void GFT3LoseEff()
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == XI.Board.Rounder.OppTeam).ToList();
            int total = 4;
            IDictionary<Player, int> sch = new Dictionary<Player, int>();
            while (total > 0)
            {
                if (invs.Count == 1)
                {
                    string word = "#HP回复,T1(p" + invs[0].Uid + "),#回复数值,D" + total;
                    XI.AsyncInput(XI.Board.Opponent.Uid, word, "GFT3WinEff", "0");
                    sch[invs[0]] = total;
                    total = 0; invs.Clear();
                }
                else
                {
                    string ichi = total == 1 ? "/D1" : ("/D1~" + total);
                    string word = "#HP回复,T1~" + invs.Count + "(p" + string.Join("p",
                        invs.Select(p => p.Uid)) + "),#回复数值," + ichi;
                    string input = XI.AsyncInput(XI.Board.Opponent.Uid, word, "GFT3WinEff", "0");
                    if (!input.Contains("/"))
                    {
                        string[] ips = input.Split(',');
                        ushort ut = ushort.Parse(ips[0]);
                        int zn = int.Parse(ips[1]);
                        Player py = XI.Board.Garden[ut];
                        sch[py] = zn;
                        total -= zn;
                        invs.Remove(py);
                    }
                }
            }
            Cure("GFT3", sch.Keys.ToList(), sch.Values.ToList());
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
            XI.RaiseGMessage("G0LH,1," + player.Uid + "," + (player.HPb + 2));
        }
        public void GFT4DecrAction(Player player)
        {
            XI.RaiseGMessage("G0LH,0," + player.Uid + "," + (player.HPb - 2));
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
                    string.Join("p", invs.Select(p => p.Uid)) + ")", "GTT4Win", "0");
                ushort opc = ushort.Parse(opcs);
                Player o = XI.Board.Garden[opc];

                string rtx = o.Tux.Count > 0 ? string.Join(",", o.Tux) : "";
                string htx = h.Tux.Count > 0 ? string.Join(",", h.Tux) : "";
                int rtxn = o.Tux.Count, htxn = h.Tux.Count;
                if (rtx != "")
                    XI.RaiseGMessage("G0HQ,0," + h.Uid + "," + o.Uid + ",1," + rtxn + "," + rtx);
                if (htx != "")
                    XI.RaiseGMessage("G0HQ,0," + o.Uid + "," + h.Uid + ",1," + htxn + "," + htx);
            }
        }
        #endregion Package 5#

        #region Package HL
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
                foreach (Player py in invs) {
                    for (int i = 0; i < 5; ++i) {
                        if (py.Pets[i] != 0) props.Add(i);
                    }
                }
                if (type == 0)
                    XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",GSH3," + props.Count);
                else if (type == 1) { // FI
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
                        else {
                            Player po = XI.Board.Garden[old];
                            if (po != null && po.GetPetCount() > 0) { leaver = true; break; }
                        }
                        if (to == player.Uid)
                            ++playerIn;
                        else {
                            Player pt = XI.Board.Garden[to];
                            if (pt != null && pt.GetPetCount() > 0) { leaver = true; break; }
                        }
                    }
                    return leaver || playerIn != 0;
                }
                else if (type == 2 || type == 3) // IC/OC
                {
                    if (XI.Board.InFight && XI.Board.IsAttendWar(player) && yesIncr)
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
        #endregion Package HL

        #region Monster Effect Util

        private void Harm(string monCode, Player py, int n, int mask = 0)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                new Harm(py.Uid, (monValue + 1000), mon.Element, n, mask)));
        }

        private void Harm(string monCode, IEnumerable<Player> invs, int n, int mask = 0)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(invs.Select(p => new Harm(
                p.Uid, (monValue + 1000), mon.Element, n, mask))));
        }

        private void Harm(string monCode, List<Player> invs, List<int> ns, List<int> mask = null)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            int sz = invs.Count;
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Harm(invs[p].Uid, (monValue + 1000), mon.Element, ns[p], mask == null ? 0 : mask[p]))));
        }

        private void Cure(string monCode, Player py, int n)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                new Cure(py.Uid, (monValue + 1000), mon.Element, n)));
        }

        private void Cure(string monCode, Player py, int n, FiveElement five)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                new Cure(py.Uid, (monValue + 1000), five, n)));
        }

        private void Cure(string monCode, IEnumerable<Player> invs, int n)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(invs.Select(p => new Cure(
                p.Uid, (monValue + 1000), mon.Element, n))));
        }

        private void Cure(string monCode, List<Player> invs, List<int> ns)
        {
            ushort monValue = XI.LibTuple.ML.Encode(monCode);
            int sz = invs.Count;
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(monValue);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Cure(invs[p].Uid, (monValue + 1000), mon.Element, ns[p]))));
        }

        #endregion Monster Effect Util
    }
}
