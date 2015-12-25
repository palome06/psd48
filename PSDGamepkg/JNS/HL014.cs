using PSD.Base;
using PSD.Base.Card;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.JNS
{
    public partial class SkillCottage
    {
        #region HL001 - Yanfeng
        public void JNH0101Action(Player player, int type, string fuse, string argst)
        {
            ushort cnt = 0;
            var tl = XI.LibTuple.TL;
            List<Base.Card.Tux> list = player.Tux.Select(
                p => tl.DecodeTux(p)).Distinct().ToList();
            if (list.Any(p => p.Type == Tux.TuxType.JP))
                ++cnt;
            if (list.Any(p => p.Type == Tux.TuxType.TP))
                ++cnt;
            if (list.Any(p => p.Type == Tux.TuxType.ZP))
                ++cnt;
            if (list.Any(p => p.Type == Tux.TuxType.FJ || p.Type == Tux.TuxType.WQ || p.Type == Tux.TuxType.XB))
                ++cnt;

            XI.RaiseGMessage("G0DH," + player.Uid + ",2," + player.Tux.Count);
            if (cnt + player.TokenCount > 8)
                cnt = (ushort)(8 - player.TokenCount);
            if (cnt > 0)
                XI.RaiseGMessage("G0IJ," + player.Uid + ",0," + cnt);
        }
        public bool JNH0101Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && player.Tux.Count > 0;
        }
        public void JNH0102Action(Player player, int type, string fuse, string argst)
        {
            int cnt = player.TokenCount;
            XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + cnt);
            if (XI.Board.IsAttendWarSucc(player))
                XI.RaiseGMessage("G0IP," + player.Team + "," + (cnt + 2));
            else
                XI.RaiseGMessage("G0IP," + player.Team + "," + cnt);
        }
        public bool JNH0102Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.IsAttendWar(player);
            var mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            var mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            bool b2 = mon1 != null && mon1.Level == Monster.ClLevel.BOSS;
            bool b3 = mon2 != null && mon2.Level == Monster.ClLevel.BOSS;
            bool b4 = player.TokenCount >= 2;
            return b1 && (b2 || b3) && b4;
        }
        public void JNH0103Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0DS," + player.Uid + ",1");
            else if (type == 1)
            {
                ushort who = ushort.Parse(fuse.Substring("G0QR,".Length));
                TargetPlayer(player.Uid, who);
            }
        }
        public bool JNH0103Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.Immobilized)
            {
                string[] g0ds = fuse.Split(',');
                for (int i = 1; i < g0ds.Length;)
                {
                    if (g0ds[i + 1] == "0") // A,0,n
                    {
                        if (g0ds[i] == player.Uid.ToString())
                            return true;
                        else
                            i += 3;
                    }
                    else // A,1
                        i += 2;
                }
            }
            else if (type == 1)
            {
                ushort who = ushort.Parse(fuse.Substring("G0QR,".Length));
                Player py = XI.Board.Garden[who];
                if (py != null && py.Team == player.Team && py.Immobilized)
                    return true;
            }
            return false;
        }
        public void JNH0104Action(Player player, int type, string fuse, string argst)
        {
            string[] args = fuse.Split(',');
            //Board.IsTangled = true;
            ushort who = ushort.Parse(args[1]);
            ushort mon2ut = XI.Board.Monster2;
            if (mon2ut != 0)
            {
                Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                if (NMBLib.IsMonster(mon2ut))
                {
                    Monster mon2 = XI.LibTuple.ML.Decode(mon2ut);
                    XI.WI.BCast("E0HZ,1," + who + "," + mon2ut);
                    XI.RaiseGMessage("G0YM,1," + mon2ut + ",0");
                    if (mon2.STR >= mon1.STR)
                    {
                        mon1.Curtain();
                        if (XI.Board.Mon1From != 0)
                            XI.RaiseGMessage("G0HL," + XI.Board.Mon1From + "," + XI.Board.Monster1);
                        XI.RaiseGMessage("G0WB," + XI.Board.Monster1);
                        XI.RaiseGMessage("G0ON," + XI.Board.Mon1From + ",M,1," + XI.Board.Monster1);
                        XI.RaiseGMessage("G0YM,0,0,0");
                        XI.Board.Monster1 = 0;

                        mon2.WinEff();
                        XI.RaiseGMessage("G0WB," + XI.Board.Monster2);
                        XI.RaiseGMessage("G0ON,0,M,1," + XI.Board.Monster2);
                        XI.RaiseGMessage("G0YM,1,0,0");
                        XI.Board.Monster2 = 0;

                        XI.RaiseGMessage("G0IA," + player.Uid + ",2");
                        XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "ZN");
                    }
                    else
                        XI.RaiseGMessage("G0IP," + XI.Board.Rounder.OppTeam + "," + mon2.STR);
                }
                else if (NMBLib.IsNPC(mon2ut))
                {
                    XI.WI.BCast("E0HZ,2," + who + "," + mon2ut);
                    XI.RaiseGMessage("G0ON,0,M,1," + XI.Board.Monster2);
                    XI.RaiseGMessage("G0YM,1,0,0");
                    XI.Board.Monster2 = 0;
                }
                XI.InnerGMessage(fuse, 201);
            }
            else
                XI.InnerGMessage(fuse, 191);
        }
        public bool JNH0104Valid(Player player, int type, string fuse)
        {
            string[] g0hzs = fuse.Split(',');
            return g0hzs[1] == player.Uid.ToString() && g0hzs[2] != "0";
        }
        #endregion HL001 - Yanfeng
        #region HL002 - YangYue
        public bool JNH0201Valid(Player player, int type, string fuse)
        {
            if (player.HP < 7)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                bool anyHarm = player.HP < 3 && harms.Any(p => p.Element != FiveElement.SOL
                    && p.Element != FiveElement.LOVE);
                bool hasIncr = harms.Any(p => p.Element != FiveElement.SOL
                    && p.Element != FiveElement.LOVE && p.N >= 2);
                return anyHarm || hasIncr;
            }
            else return false;
        }
        public void JNH0201Action(Player player, int type, string fuse, string argst)
        {
            float factor = (player.HP < 3) ? 2.0f : 1.5f;
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Element != FiveElement.SOL && harm.Element != FiveElement.LOVE)
                    harm.N = (int)(harm.N * factor);
            }
            XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNH0202");
            XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -189);
        }
        public bool JNH0202Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player) && XI.Board.Garden.Values.Any(p =>
                    XI.Board.IsAttendWar(p) && p.Team == player.OppTeam && p.IsTared && p.Tux.Count > 0);
            else if (type == 1)
            {
                int idxc = fuse.IndexOf(',');
                ushort ut = ushort.Parse(fuse.Substring(idxc + 1));
                return player.SingleTokenTar == ut;
            }
            return false;
        }
        public void JNH0202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ut);

                Player tar = XI.Board.Garden[ut];
                string c0 = Util.RepeatString("p0", tar.Tux.Count);
                XI.AsyncInput(player.Uid, "#弃置的,C1(" + c0 + ")", "JNH0202", "0");
                List<ushort> vals = tar.Tux.ToList();
                vals.Shuffle();
                ushort randomCard = vals[0];

                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(randomCard);
                if (tux.IsTuxEqiup())
                {
                    XI.RaiseGMessage("G0ZB," + ut + ",0," + randomCard);
                    Cure(player, tar, 2);
                }
                else
                {
                    XI.RaiseGMessage("G0QZ," + ut + "," + randomCard);
                    if (tux.Type == Tux.TuxType.JP)
                        Harm(player, new Player[] { player, tar }, 1);
                    else if (tux.Type == Tux.TuxType.ZP)
                    {
                        int delta = tar.STR - tar.STR / 2;
                        if (delta > 0)
                            XI.RaiseGMessage("G0OA," + ut + ",1," + delta);
                    }
                    else if (tux.Type == Tux.TuxType.TP)
                        tar.DrTuxDisabled = true;
                }
                XI.RaiseGMessage("G0DH," + ut + ",0,1");
                XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNH0201");
            }
            else if (type == 1)
            {
                if (player.SingleTokenTar != 0)
                {
                    XI.Board.Garden[player.SingleTokenTar].DrTuxDisabled = false;
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                }
            }
        }
        public string JNH0202Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                        XI.Board.IsAttendWar(p) && p.Team == player.OppTeam &&
                        p.IsTared && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNH0203Valid(Player player, int type, string fuse)
        {
            bool solo = !XI.Board.Garden.Values.Where(p => p.Uid != player.Uid &&
                p.IsAlive && p.Team == player.Team).Any();
            bool case1 = player.Skills.Contains("JNH0201");
            bool case2 = player.Skills.Contains("JNH0202");
            return solo && (case1 || case2);
        }
        public void JNH0203Action(Player player, int type, string fuse, string argst)
        {
            if (player.Skills.Contains("JNH0201"))
                XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JPT5,0;0," + fuse);
            if (player.Skills.Contains("JNH0202"))
            {
                List<ushort> sm = XI.Board.MonDises.Where(p => p != 0 && XI.LibTuple.ML.Decode(p) != null
                && XI.LibTuple.ML.Decode(p).STR <= 7).ToList();
                if (sm.Count > 0)
                {
                    string sl = XI.AsyncInput(player.Uid, "/获得,M1(p" + string.Join("p", sm) + ")", "JNH0203", "0");
                    if (sl != VI.CinSentinel && !sl.StartsWith("/"))
                    {
                        ushort mon = ushort.Parse(sl);
                        XI.RaiseGMessage("G2CN,1,1");
                        XI.Board.MonDises.Remove(mon);
                        XI.RaiseGMessage("G0HC,1," + player.Uid + ",0,1," + mon);
                    }
                }
                bool apable = !XI.Board.BannedHero.Contains(10107) && !XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.SelectHero == 10107).Any();
                if (apable)
                {
                    XI.RaiseGMessage("G0OY,0," + player.Uid);
                    XI.RaiseGMessage("G0IY,0," + player.Uid + ",10107");
                }
            }
        }
        #endregion HL002 - YangYue
        #region HL003 - YangTai
        public bool JNH0301Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE &&
                        harm.Element != FiveElement.SOL && harm.N <= player.GetAllCardsCount())
                    return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
            }
            return false;
        }
        public void JNH0301Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            Artiad.Harm thisHarm = null, thatHarm = null;
            string[] blocks = argst.Split(',');
            ushort to = ushort.Parse(blocks[0]);
            TargetPlayer(player.Uid, to);
            //int mValue = 0; 
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", Util.TakeRange(blocks, 1, blocks.Length)));
            bool action = false;
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                {
                    thisHarm = harm;
                    //XI.RaiseGMessage("G0TT," + to);
                    //int value = XI.Board.DiceValue;
                    //int m = value / 2;
                    //if (harm.N > m)
                    //{
                    //    string format = player.Tux.Count < (harm.N - m) ? "/" :
                    //        "/Q" + (harm.N - m) + "(p" + string.Join("p", player.Tux) + ")";
                    //    string input = XI.AsyncInput(player.Uid, format, "JNH0301", "0");
                    //    if (!input.StartsWith("/"))
                    //    {
                    //        List<ushort> uts = input.Split(',').Select(p => ushort.Parse(p)).ToList();
                    //        XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", uts));
                    //        mValue = m; action = true;
                    //    }
                    //    else { mValue = 0; action = false; }
                    //}
                    //else { mValue = m; action = true; }
                    action = true;
                }
                else if (harm.Who == to)
                    thatHarm = harm;
            }
            if (action)
            {
                if (thatHarm != null && thatHarm.Element == thisHarm.Element)
                {
                    thatHarm.N += thisHarm.N;
                    harms.Remove(thisHarm);
                }
                else
                    thisHarm.Who = to;
            }
            XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -54);
            if (XI.Board.Garden[to].IsAlive)
            {
                XI.RaiseGMessage("G0TT," + to);
                int m = XI.Board.DiceValue / 2;
                if (m > 0)
                    XI.RaiseGMessage("G0DH," + to + ",0," + m);
            }
        }
        public string JNH0301Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1" + AOthersTared(player);
            else if (prev.IndexOf(',') < 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                int n = 0;
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                        n = harm.N;
                }
                return "/Q" + n + "(p" + string.Join("p", player.ListOutAllCards()) + ")";
            }
            else
                return "";
        }
        public void JNH0302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                if (player.TokenTars.Count == 0)
                {
                    Base.Card.Hero hero = XI.LibTuple.HL.InstanceHero(player.SelectHero);
                    if (hero != null)
                    {
                        if (hero.STR > hero.DEX)
                        {
                            XI.RaiseGMessage("G0IX," + player.Uid + ",0," + (hero.STR - hero.DEX));
                            XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (hero.STR - hero.DEX));
                        }
                        else if (hero.STR < hero.DEX)
                        {
                            XI.RaiseGMessage("G0OX," + player.Uid + ",0," + (hero.DEX - hero.STR));
                            XI.RaiseGMessage("G0IA," + player.Uid + ",0," + (hero.DEX - hero.STR));
                        }
                    }
                    // Reverse Ordered Player
                    XI.RaiseGMessage("G0HR,0,1");
                    string another = XI.AsyncInput(player.Uid, "#另一名,/T1" + AOthersTared(player), "JNH0302Action", "0");
                    if (!another.StartsWith("/") && another != VI.CinSentinel)
                    {
                        ushort to = ushort.Parse(another);
                        XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + to);
                        Player py = XI.Board.Garden[to];
                        Base.Card.Hero pyHero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                        if (pyHero != null)
                        {
                            if (pyHero.STR > pyHero.DEX)
                            {
                                XI.RaiseGMessage("G0IX," + py.Uid + ",0," + (pyHero.STR - pyHero.DEX));
                                XI.RaiseGMessage("G0OA," + py.Uid + ",0," + (pyHero.STR - pyHero.DEX));
                            }
                            else if (pyHero.STR < pyHero.DEX)
                            {
                                XI.RaiseGMessage("G0OX," + py.Uid + ",0," + (pyHero.DEX - pyHero.STR));
                                XI.RaiseGMessage("G0IA," + py.Uid + ",0," + (pyHero.DEX - pyHero.STR));
                            }
                        }
                    }
                    else
                        XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + player.Uid);
                }
                else
                {
                    Base.Card.Hero hero = XI.LibTuple.HL.InstanceHero(player.SelectHero);
                    if (hero != null)
                    {
                        if (hero.STR > hero.DEX)
                        {
                            XI.RaiseGMessage("G0OX," + player.Uid + ",0," + (hero.STR - hero.DEX));
                            XI.RaiseGMessage("G0IA," + player.Uid + ",0," + (hero.STR - hero.DEX));
                        }
                        else if (hero.STR < hero.DEX)
                        {
                            XI.RaiseGMessage("G0IX," + player.Uid + ",0," + (hero.DEX - hero.STR));
                            XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (hero.DEX - hero.STR));
                        }
                    }
                    // Reverse Ordered Player
                    XI.RaiseGMessage("G0HR,0,1");
                    if (player.SingleTokenTar != player.Uid)
                    {
                        Player py = XI.Board.Garden[player.SingleTokenTar];
                        Base.Card.Hero pyHero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                        if (pyHero != null)
                        {
                            if (pyHero.STR > pyHero.DEX)
                            {
                                XI.RaiseGMessage("G0OX," + py.Uid + ",0," + (pyHero.STR - pyHero.DEX));
                                XI.RaiseGMessage("G0IA," + py.Uid + ",0," + (pyHero.STR - pyHero.DEX));
                            }
                            else if (pyHero.STR < pyHero.DEX)
                            {
                                XI.RaiseGMessage("G0IX," + py.Uid + ",0," + (pyHero.DEX - pyHero.STR));
                                XI.RaiseGMessage("G0OA," + py.Uid + ",0," + (pyHero.DEX - pyHero.STR));
                            }
                        }
                    }
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                }
            }
            else if (type == 1)
                XI.RaiseGMessage("G0HR,0,1");
            else if (type == 2)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + player.Uid);
            }
            else if (type == 3)
            {
                Player py = XI.Board.Garden[player.SingleTokenTar];
                Base.Card.Hero pyHero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                if (pyHero != null)
                {
                    if (pyHero.STR > pyHero.DEX)
                    {
                        XI.RaiseGMessage("G0IX," + py.Uid + ",0," + (pyHero.STR - pyHero.DEX));
                        XI.RaiseGMessage("G0OA," + py.Uid + ",0," + (pyHero.STR - pyHero.DEX));
                    }
                    else if (pyHero.STR < pyHero.DEX)
                    {
                        XI.RaiseGMessage("G0OX," + py.Uid + ",0," + (pyHero.DEX - pyHero.STR));
                        XI.RaiseGMessage("G0IA," + py.Uid + ",0," + (pyHero.DEX - pyHero.STR));
                    }
                }
            }
        }
        public bool JNH0302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNH0302")
                            //return (player.ROMUshort != 0);
                            return !XI.Board.ClockWised;
                }
            }
            else if (type == 2)
            {
                string[] parts = fuse.Split(',');
                ushort thype = ushort.Parse(parts[1]);
                ushort who = ushort.Parse(parts[2]);
                if (player.SingleTokenTar != player.Uid && who == player.SingleTokenTar && thype != 1)
                    return true;
            }
            else if (type == 3)
            {
                string[] parts = fuse.Split(',');
                ushort thype = ushort.Parse(parts[1]);
                ushort who = ushort.Parse(parts[2]);
                if (player.SingleTokenTar != player.Uid && who == player.SingleTokenTar && thype == 1)
                    return true;
            }
            return false;
        }
        public void JNH0303Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0TT," + player.Uid);
            if (XI.Board.DiceValue != 6)
            {
                int total = player.GetPetCount() + player.ListOutAllCards().Count;
                string range = Util.SSelect(XI.Board, p => p != player && p.IsTared);
                string target = XI.AsyncInput(player.Uid, "#HP-" + total + ",T1" + range, "JNH0303", "0");
                ushort who = ushort.Parse(target);
                Harm(player, XI.Board.Garden[who], total, FiveElement.YIN);
                XI.SendOutUAMessage(player.Uid, "JNH0303," + who, "0");
            }
            //XI.InnerGMessage("G0OY,2," + player.Uid, 81);
        }
        public bool JNH0303Valid(Player player, int type, string fuse)
        {
            int total = player.GetPetCount() + player.ListOutAllCards().Count;
            if (total == 0)
                return false;
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; ++i)
                if (blocks[i] == player.Uid.ToString())
                    return true;
            return false;
        }
        #endregion HL003 - YangTai
        #region HL004 - YeFengling
        public bool JNH0401Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && player.Tux.Count >= 2 &&
                player.Team == XI.Board.Rounder.Team;
        }
        public void JNH0401Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            XI.RaiseGMessage("G0OW," + XI.Board.Monster1 + ",2");
            XI.RaiseGMessage("G0JM,R" + player.Uid + "PD");
        }
        public string JNH0401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q2(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNH0402Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player);
            else if (type == 1 && player.TokenAwake)
            { // G0CE
                int idx = fuse.IndexOf(';');
                string[] g0ce = fuse.Substring(0, idx).Split(',');
                ushort who = ushort.Parse(g0ce[2]);
                Player py = XI.Board.Garden[who];
                if (py != null && py.Team == player.Team)
                    return true;
            }
            // TODO: Equip is also valid here
            else if (type == 2)
                return player.TokenAwake;
            return false;
        }
        public void JNH0402Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
                player.RAMUshort = 1;
                player.RestZP = 0;
            }
            else if (type == 1)
            {
                player.RAMUshort = 0;
                XI.RaiseGMessage("G0IP," + player.Team + ",1");
            }
            else if (type == 2)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
                if (player.RAMUshort == 1)
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                player.RAMUshort = 0;
            }
        }
        public bool JNH0403Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; ++i)
                if (blocks[i] == player.Uid.ToString())
                    return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
            return false;
        }
        public void JNH0403Action(Player player, int type, string fuse, string argst)
        {
            string range = Util.SSelect(XI.Board, p => p != player && p.IsTared);
            string target = XI.AsyncInput(player.Uid, "#月神附身,T1" + range, "JNH0403", "0");
            ushort who = ushort.Parse(target);
            // TODO: change the player as HL005
            int orgHero = XI.Board.Garden[who].SelectHero;
            XI.RaiseGMessage("G0OY,0," + who);
            XI.RaiseGMessage("G0IY,0," + who + ",19005");
            XI.RaiseGMessage("G0IV," + who + "," + orgHero);
        }
        #endregion HL004 - YeFengling
        #region HL005 - Lunar Deity
        public bool JNH0501Valid(Player player, int type, string fuse)
        {
            if (type == 0) // G0IS
                return IsMathISOS("JNH0501", player, fuse);
            else if (type == 1) // G0IY
                return XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.Team) >= 3;
            else if (type == 2) // G0ZW, hind
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; ++i)
                    if (blocks[i] == player.Uid.ToString())
                        return true;
                return false;
            }
            return false;
        }
        public void JNH0501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,3");
            else if (type == 1)
            {
                int orgHero = player.Coss.Peek();
                XI.RaiseGMessage("G0OV," + player.Uid + "," + orgHero);
                XI.RaiseGMessage("G0OY,0," + player.Uid);
                XI.RaiseGMessage("G0IY,0," + player.Uid + "," + orgHero);
            }
            else if (type == 2)
            {
                string[] blocks = fuse.Split(',');
                string zw = "";
                for (int i = 1; i < blocks.Length; ++i)
                {
                    if (blocks[i] != player.Uid.ToString())
                        zw += "," + blocks[i];
                }
                int orgHero = player.Coss.Peek();
                XI.RaiseGMessage("G0OV," + player.Uid + "," + orgHero);
                XI.RaiseGMessage("G0OY,0," + player.Uid);
                XI.RaiseGMessage("G0IY,0," + player.Uid + "," + orgHero);
                if (zw != "")
                    XI.InnerGMessage("G0ZW" + zw, -8);
            }
        }
        public bool JNH0502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.Team
                    && (p.HasAnyEquips() || p.GetPetCount() > 0));
            else if (type == 1 && player.TokenAwake)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                return cures.Any(p => XI.Board.Garden[p.Who].IsAlive &&
                     XI.Board.Garden[p.Who].Team == player.OppTeam &&
                     p.N > 0 && p.Element != FiveElement.LOVE);
            }
            else if (type == 2)
                return player.TokenAwake;
            else
                return false;
        }
        public void JNH0502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] args = argst.Split(',');
                if (args[0] == "1") // Discard Pets
                    XI.RaiseGMessage("G0HI," + args[1] + "," + args[2]);
                else if (args[0] == "2") // Discard Equips
                    XI.RaiseGMessage("G0ZC," + args[1] + ",2," + args[2] + ";" + fuse);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
            }
            else if (type == 1)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                cures.RemoveAll(p => XI.Board.Garden[p.Who].IsAlive &&
                     XI.Board.Garden[p.Who].Team == player.OppTeam &&
                     p.N > 0 && p.Element != FiveElement.LOVE);
                if (cures.Count > 0)
                    XI.InnerGMessage(Artiad.Cure.ToMessage(cures), 51);
            }
            else if (type == 2)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
        }
        public string JNH0502Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                    return "#请选择爆发项目##宠物##装备,/Y2";
                else if (prev.IndexOf(',') < 0)
                {
                    ushort sel = ushort.Parse(prev);
                    if (sel == 1)
                    {
                        List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                             p.Team == player.Team && p.GetPetCount() > 0).ToList();
                        if (pys.Count > 0)
                            return "#爆发宠物,/T1(p" + string.Join("p", pys.Select(p => p.Uid)) + ")";
                        else
                            return "/";
                    }
                    else
                    {
                        List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                             p.Team == player.Team && p.HasAnyEquips()).ToList();
                        if (pys.Count > 0)
                            return "#爆发装备,/T1(p" + string.Join("p", pys.Select(p => p.Uid)) + ")";
                        else
                            return "/";
                    }
                }
                else if (prev.IndexOf(',', prev.IndexOf(',') + 1) < 0)
                {
                    ushort[] uts = prev.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    if (uts[0] == 1)
                    {
                        return "#爆发宠物,/M1(p" + string.Join("p",
                             XI.Board.Garden[uts[1]].Pets.Where(p => p != 0)) + ")";
                    }
                    else if (uts[1] == player.Uid)
                    {
                        return "#爆发装备,/Q1(p" + string.Join("p",
                             XI.Board.Garden[uts[1]].ListOutAllEquips()) + ")";
                    }
                    else
                    {
                        return "#爆发装备,/C1(p" + string.Join("p",
                             XI.Board.Garden[uts[1]].ListOutAllEquips()) + ")";
                    }
                }
                else
                    return "";
            }
            else
                return "";
        }
        public bool JNH0503Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared);
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                return harms.Any(p => p.Who != p.Source && p.Who == player.SingleTokenTar
                     && p.N > 0 && p.Element != FiveElement.LOVE);
            }
            else if (type == 2)
                return player.TokenTars.Count > 0;
            else if (type == 3 && player.TokenTars.Count > 0)
                return player.TokenTars.Count > 0 && IsMathISOS("JNH0503", player, fuse);
            else
                return false;
        }
        public void JNH0503Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] args = argst.Split(',');
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + args[0]);
                ushort tar = ushort.Parse(args[1]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + tar);
                Player py = XI.Board.Garden[tar];
                player.ROMUshort = (ushort)(py.DEXh);
                if (player.ROMUshort > 0)
                {
                    XI.RaiseGMessage("G0OX," + tar + ",0," + player.ROMUshort);
                    player.DEXh = player.ROMUshort;
                }
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.RemoveAll(p => p.Who != p.Source && p.Who == player.SingleTokenTar
                     && p.N > 0 && p.Element != FiveElement.LOVE);
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -44);
            }
            else if (type == 2)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
            else if (type == 3)
            {
                if (player.ROMUshort > 0)
                {
                    XI.RaiseGMessage("G0IX," + player.SingleTokenTar + ",1," + player.ROMUshort);
                    XI.Board.Garden[player.SingleTokenTar].DEXh = player.ROMUshort;
                    player.ROMUshort = 0;
                }
                XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
            }
        }
        public string JNH0503Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1" + AAllTareds(player);
            else
                return "";
        }
        #endregion HL005 - Lunar Deity
        #region HL006 - ZhaoWen
        public bool JNH0601Valid(Player player, int type, string fuse)
        {
            if (!player.IsAlive)
                return false;
            // G1DI,G0HQ,[G0IH]
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                for (int jdx = 1; jdx < blocks.Length;)
                {
                    ushort who = ushort.Parse(blocks[jdx]);
                    ushort inOut = ushort.Parse(blocks[jdx + 1]);
                    ushort n = ushort.Parse(blocks[jdx + 2]);
                    ushort ntx = ushort.Parse(blocks[jdx + 3]);
                    if (who == player.Uid && inOut == 1 && ntx > 0)
                        return true;
                    jdx += (n + 4);
                }
            }
            else if (type == 1)
            {
                // G0HQ,0,A,B,n/G0HQ,1,A,B
                if (player.IsAlive)
                {
                    string[] blocks = fuse.Split(',');
                    if (blocks[1] == "0")
                    {
                        ushort to = ushort.Parse(blocks[2]);
                        ushort from = ushort.Parse(blocks[3]);
                        int n = int.Parse(blocks[5]);
                        return from == player.Uid && XI.Board.Garden[to].Team == player.OppTeam && n > 0;
                    }
                    //else if (blocks[1] == "1" && blocks[3] == player.Uid.ToString())
                    //{
                    //    ushort to = ushort.Parse(blocks[2]);
                    //    System.Func<Base.Card.Tux, bool> isEq = p => p.Type == Tux.TuxType.WQ ||
                    //        p.Type == Tux.TuxType.FJ || p.Type == Tux.TuxType.XB;
                    //    if (XI.Board.Garden[to].Team == player.OppTeam)
                    //        return player.ListOutAllEquips().Any(p => isEq(XI.LibTuple.TL.DecodeTux(p)));
                    //}
                }
                return false;
            }
            else if (type == 2)
            {
                if (player.ExCards.Count > 0)
                {
                    List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                    foreach (Artiad.Cure cure in cures)
                    {
                        if (XI.Board.Garden[cure.Who].IsAlive && cure.N > 0 &&
                                cure.Element != FiveElement.LOVE)
                            return true;
                    }
                    return false;
                }
            }
            return false;
        }
        public void JNH0601Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                for (int jdx = 1; jdx < blocks.Length;)
                {
                    ushort who = ushort.Parse(blocks[jdx]);
                    ushort inOut = ushort.Parse(blocks[jdx + 1]);
                    ushort n = ushort.Parse(blocks[jdx + 2]);
                    ushort ntx = ushort.Parse(blocks[jdx + 3]);
                    if (who == player.Uid && inOut == 1 && ntx > 0)
                    {
                        ushort ut = XI.DequeueOfPile(XI.Board.TuxPiles);
                        XI.RaiseGMessage("G2IN,0,1");
                        Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux != null)
                        {
                            if (!tux.IsTuxEqiup())
                                XI.RaiseGMessage("G0ZB," + player.Uid + ",2,0," + ut);
                            else
                                XI.RaiseGMessage("G0ON,0,C,1," + ut);
                        }
                    }
                    jdx += (n + 4);
                }
            }
            else if (type == 1)
            {
                // G0HQ,0,A,B,...
                ushort ut = XI.DequeueOfPile(XI.Board.TuxPiles);
                XI.RaiseGMessage("G2IN,0,1");
                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    if (!tux.IsTuxEqiup())
                        XI.RaiseGMessage("G0ZB," + player.Uid + ",2,0," + ut);
                    else
                        XI.RaiseGMessage("G0ON,0,C,1," + ut);
                }
            }
            else if (type == 2)
            {
                ushort card = ushort.Parse(argst);
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                ISet<Player> invs = new HashSet<Player>();
                foreach (Artiad.Cure cure in cures)
                {
                    if (XI.Board.Garden[cure.Who].IsAlive && cure.N > 0
                            && cure.Element != FiveElement.LOVE)
                        invs.Add(XI.Board.Garden[cure.Who]);
                }
                if (invs.Count > 0)
                {
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                    Cure(player, invs, 1);
                }
            }
        }
        public string JNH0601Input(Player player, int type, string fuse, string prev)
        {
            if (type == 2 && prev == "")
                return "/Q1(p" + string.Join("p", player.ExCards) + ")";
            else
                return "";
        }
        public bool JNH0602Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                if (!player.TokenAwake)
                {
                    List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                    return harms.Any(p => p.Element != FiveElement.LOVE);
                }
            }
            else if (type == 1)
            {
                if (player.TokenAwake)
                {
                    List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                    return harms.Any(p => p.Element != FiveElement.LOVE
                        && p.Element != FiveElement.SOL);
                }
            }
            else if (type == 2)
                return player.TokenAwake;
            return false;
        }
        public void JNH0602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
                //List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                //bool twoEnemy = harms.Where(p => p.Element != FiveElement.LOVE &&
                //        XI.Board.Garden[p.Who].Team == player.OppTeam)
                //        .Select(p => p.Who).Distinct().Count() >= 2;
                //bool noFriend = !harms.Any(p => p.Element != FiveElement.LOVE &&
                //        XI.Board.Garden[p.Who].Team == player.Team && p.N > 0);
                //if (twoEnemy)
                //    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> nHarms = new List<Artiad.Harm>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                    {
                        if (--harm.N > 0)
                            nHarms.Add(harm);
                    }
                }
                if (nHarms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(nHarms), -149);
            }
            else if (type == 2)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
        }
        public bool JNH0603Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Select(p => XI.Board.Garden[p.Who]).Any(p => p.IsTared && p.HP > 0 &&
                p.HP * 3 <= p.HPb && p.ListOutAllCards().Count > 0);
        }
        public void JNH0603Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort tar = ushort.Parse(argst.Substring(0, idx));
            ushort ut = ushort.Parse(argst.Substring(idx + 1));
            if (ut != 0)
                XI.RaiseGMessage("G0QZ," + tar + "," + ut);
            else
                XI.RaiseGMessage("G0DH," + tar + ",2,1");
            Cure(player, XI.Board.Garden[tar], 1);
        }
        public string JNH0603Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<ushort> invs = harms.Select(p => XI.Board.Garden[p.Who]).Where(p => p.IsTared && p.HP > 0 &&
                    p.HP * 3 <= p.HPb && p.ListOutAllCards().Count > 0).Select(p => p.Uid).Distinct().ToList();
                return "/T1(p" + string.Join("p", invs) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort tar = ushort.Parse(prev);
                Player py = XI.Board.Garden[tar];
                if (tar == player.Uid)
                    return "/Q1(p" + string.Join("p", py.ListOutAllCards()) + ")";
                else
                    return "C1(p" + string.Join("p", py.ListOutAllCardsWithEncrypt()) + ")";
            }
            else
                return "";
        }
        #endregion HL006 - ZhaoWen
        #region HL007 - Yingyue
        public bool JNH0701Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                for (int idx = 1; idx < blocks.Length;)
                {
                    //string fromZone = blocks[idx];
                    string cardType = blocks[idx + 1];
                    int cnt = int.Parse(blocks[idx + 2]);
                    if (cardType == "M" && cnt > 0)
                    {
                        for (int i = idx + 3; i < idx + 3 + cnt; ++i)
                        {
                            ushort ut = ushort.Parse(blocks[i]);
                            if (NMBLib.IsNPC(ut))
                                return true;
                        }
                    }
                    idx += (3 + cnt);
                }
            }
            else if (type == 1) // Z1
            {
                bool stucken = (XI.Board.IsAttendWar(player) && player.TokenExcl.Count > 0) ||
                    player.TokenExcl.Any(p => XI.LibTuple.NL.Decode(
                    (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR >= 5);
                return stucken;
            }
            else if (type == 2) // AF
            {
                bool waken = player.TokenExcl.Any(p => XI.LibTuple.NL.Decode(
                    (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR < 5);
                string[] g0af = fuse.Split(',');
                if (g0af[1] != "0" && XI.Board.InFight && waken)
                    for (int i = 1; i < g0af.Length; i += 2)
                    {
                        ushort ut = ushort.Parse(g0af[i + 1]);
                        if (ut == player.Uid)
                        {
                            ushort delta = ushort.Parse(g0af[i]);
                            if (delta > 4 && player.RAMInt > 0)
                                return true;
                            else if (delta <= 4 && player.RAMInt == 0)
                                return true;
                        }
                    }
                return false;
            }
            else if (type == 3 || type == 4) // IS/OS
            {
                if (IsMathISOS("JNH0801", player, fuse) && XI.Board.InFight)
                {
                    return (XI.Board.IsAttendWar(player) && player.TokenExcl.Count > 0) ||
                    player.TokenExcl.Any(p => XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(
                        ushort.Parse(p.Substring("M".Length)))).STR >= 5);
                }
                else return false;
            }
            return false;
        }
        public void JNH0701Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                IDictionary<ushort, List<ushort>> dict = new Dictionary<ushort, List<ushort>>();
                string n0on = "";
                string[] blocks = fuse.Split(',');
                for (int idx = 1; idx < blocks.Length;)
                {
                    ushort fromZone = ushort.Parse(blocks[idx]);
                    string cardType = blocks[idx + 1];
                    int cnt = int.Parse(blocks[idx + 2]);
                    if (cardType == "M" && cnt > 0)
                    {
                        for (int i = idx + 3; i < idx + 3 + cnt; ++i)
                            Util.AddToMultiMap(dict, fromZone, ushort.Parse(blocks[i]));
                    }
                    else
                        n0on += "," + string.Join(",", Util.TakeRange(blocks, idx, idx + 3 + cnt));
                    idx += (3 + cnt);
                }

                ushort npcUt = ushort.Parse(argst);
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(npcUt));
                int rest = npc.STR + 1;
                foreach (ushort ut in XI.Board.OrderedPlayer(player.Uid))
                {
                    Player py = XI.Board.Garden[ut];
                    if (py.IsAlive && py.Team == player.Team && py.Tux.Count > 0)
                    {
                        if (rest <= 0)
                            break;
                        int mn = System.Math.Min(rest, py.Tux.Count);
                        string hint = "#弃置(还需" + rest + "张),/Q1" +
                            ((mn > 1) ? ("~" + mn) : "") + "(p" + string.Join("p", py.Tux) + ")";
                        string input = XI.AsyncInput(py.Uid, hint, "JNH0701", "0");
                        if (input != VI.CinSentinel && !input.StartsWith("/"))
                        {
                            string[] parts = input.Split(',');
                            XI.RaiseGMessage("G0QZ," + ut + "," + input);
                            rest -= parts.Length;
                        }
                    }
                }
                if (rest == 0)
                {
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,M" + npcUt);
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0,M" + npcUt);
                    foreach (var pair in dict)
                        pair.Value.Remove(npcUt);
                }
                string mAdd = string.Join(",", dict.Where(p => p.Value.Count > 0)
                    .Select(p => p.Key + ",M," + p.Value.Count + "," + string.Join(",", p.Value)));
                if (mAdd.Length > 0)
                    n0on += "," + mAdd;
                if (n0on.Length > 0)
                {
                    if (rest == 0)
                        XI.InnerGMessage("G0ON" + n0on, 30);
                    else
                        XI.InnerGMessage("G0ON" + n0on, 31);
                }
            }
            else if (type == 1)
            {
                int upfive = player.TokenExcl.Count(p => XI.LibTuple.NL.Decode(
                    (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR >= 5);
                int dnfive = player.TokenExcl.Count; int delta;
                if (XI.Board.IsAttendWar(player))
                    delta = dnfive - player.RAMInt;
                else
                    delta = upfive - player.RAMInt;
                player.RAMInt = dnfive;
                if (delta > 0)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + delta);
                else if (delta < 0)
                    XI.RaiseGMessage("G0OP," + player.Team + "," + (-delta));
            }
            else if (type == 2)
            {
                int dnfive = player.TokenExcl.Count(p => XI.LibTuple.NL.Decode(
                    (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR < 5);
                if (XI.Board.IsAttendWar(player))
                {
                    player.RAMInt += dnfive;
                    XI.RaiseGMessage("G0IP," + player.Team + "," + dnfive);
                }
                else
                {
                    player.RAMInt -= dnfive;
                    XI.RaiseGMessage("G0OP," + player.Team + "," + dnfive);
                }
            }
            else if (type == 3)
            {
                int upfive = player.TokenExcl.Count(p => XI.LibTuple.NL.Decode(
                    (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR >= 5);
                int dnfive = player.TokenExcl.Count;
                int delta = XI.Board.IsAttendWar(player) ? dnfive : upfive;
                player.RAMInt = delta;
                if (delta > 0)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + delta);
            }
            else if (type == 4)
            {
                if (player.RAMInt > 0)
                {
                    XI.RaiseGMessage("G0OP," + player.Team + "," + player.RAMInt);
                    player.RAMInt = 0;
                }
            }
        }
        public string JNH0701Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<ushort> mons = new List<ushort>();
                string[] blocks = fuse.Split(',');
                for (int idx = 1; idx < blocks.Length;)
                {
                    //string fromZone = blocks[idx];
                    string cardType = blocks[idx + 1];
                    int cnt = int.Parse(blocks[idx + 2]);
                    if (cardType == "M" && cnt > 0)
                    {
                        for (int i = idx + 3; i < idx + 3 + cnt; ++i)
                        {
                            ushort ut = ushort.Parse(blocks[i]);
                            if (NMBLib.IsNPC(ut))
                                mons.Add(ut);
                        }
                    }
                    idx += (3 + cnt);
                }
                if (mons.Count > 0)
                    return "#收为「魅心」,/M1(p" + string.Join("p", mons) + ")";
                else
                    return "/";
            }
            else
                return "";
        }

        public bool JNH0702Valid(Player player, int type, string fuse)
        {
            string[] g1di = fuse.Split(',');
            for (int idx = 1; idx < g1di.Length;)
            {
                ushort who = ushort.Parse(g1di[idx]);
                bool drIn = g1di[idx + 1] == "0";
                int n = int.Parse(g1di[idx + 2]);
                if (who != player.Uid && XI.Board.Garden[who].IsTared && !drIn && n >= 2)
                    return true;
                idx += (4 + n);
            }
            return false;
        }
        public void JNH0702Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = argst.Split(',');
            string[] g1di = fuse.Split(',');
            string n1di = "";
            List<ushort> rest = new List<ushort>();
            List<ushort> eqs = new List<ushort>();
            ushort tar = ushort.Parse(blocks[0]);
            ushort ut = ushort.Parse(blocks[1]);
            for (int idx = 1; idx < g1di.Length;)
            {
                ushort who = ushort.Parse(g1di[idx]);
                bool drIn = g1di[idx + 1] == "0";
                int n = int.Parse(g1di[idx + 2]);
                if (who == tar && XI.Board.Garden[who].IsTared && !drIn && n >= 2)
                {
                    int neq = int.Parse(g1di[idx + 3]);
                    List<ushort> rms = Util.TakeRange(g1di, idx + 4, idx + 4 + n - neq)
                        .Select(p => ushort.Parse(p)).ToList();
                    List<ushort> reqs = Util.TakeRange(g1di, idx + 4 + n - neq, idx + 4 + n)
                        .Select(p => ushort.Parse(p)).ToList();
                    rest.AddRange(rms);
                    eqs.AddRange(reqs);
                }
                else
                    n1di += "," + string.Join(",", Util.TakeRange(g1di, idx, idx + 4 + n));
                idx += (4 + n);
            }
            //XI.RaiseGMessage("G2CN,0,1");
            //XI.Board.TuxDises.Remove(ut);
            rest.Remove(ut); eqs.Remove(ut);
            XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
            if (blocks.Length >= 3)
            {
                ushort didAbandon = ushort.Parse(blocks[2]);
                List<ushort> jointResult = new List<ushort>();
                jointResult.AddRange(rest); jointResult.AddRange(eqs);
                jointResult.Remove(didAbandon);
                if (jointResult.Count > 0)
                    XI.RaiseGMessage("G0HQ,2," + tar + ",0,0," + string.Join(",", jointResult));
                if (rest.Contains(didAbandon))
                    n1di += "," + tar + ",1,1,1," + didAbandon;
                else if (eqs.Contains(didAbandon))
                    n1di += "," + tar + ",1,1,0," + didAbandon;
            }
            else // no other inputs enters
            {
                rest.AddRange(eqs);
                n1di += "," + tar + ",1," + rest.Count + "," + (rest.Count - eqs.Count) + "," + string.Join(",", rest);
            }
            if (n1di.Length > 0)
                XI.InnerGMessage("G1DI" + n1di, 31);
        }
        public string JNH0702Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                ISet<ushort> invs = new HashSet<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length;)
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who != player.Uid && XI.Board.Garden[who].IsTared && !drIn && n >= 2)
                        invs.Add(who);
                    idx += (4 + n);
                }
                return "#获得,/T1(p" + string.Join("p", invs) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort ut = ushort.Parse(prev);
                List<ushort> tuxes = new List<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length;)
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who == ut && !drIn && n >= 2)
                    {
                        tuxes.AddRange(Util.TakeRange(g1di, idx + 4, idx + 4 + n)
                            .Select(p => ushort.Parse(p)));
                    }
                    idx += (4 + n);
                }
                return "#获得,/C1(p" + string.Join("p", tuxes) + ")";
            }
            else if (prev.IndexOf(',', prev.IndexOf(',') + 1) < 0)
            {
                int serp = prev.IndexOf(',');
                ushort ut = ushort.Parse(prev.Substring(0, serp));
                ushort except = ushort.Parse(prev.Substring(serp + 1));
                List<ushort> tuxes = new List<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length;)
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who == ut && !drIn && n >= 2)
                    {
                        tuxes.AddRange(Util.TakeRange(g1di, idx + 4, idx + 4 + n)
                            .Select(p => ushort.Parse(p)));
                    }
                    idx += (4 + n);
                }
                tuxes.Remove(except);
                if (tuxes.Count > 1)
                    return "#排除,/C1(p" + string.Join("p", tuxes) + ")";
                else
                    return "";
            }
            else
                return "";
        }
        public bool JNH0703Valid(Player player, int type, string fuse)
        {
            return !player.Immobilized && player.Tux.Count > 0 && XI.Board.Garden.Values.Any(
                p => p.IsTared && p.Team == player.Team && p.Uid != player.Uid);
        }
        public void JNH0703Action(Player player, int type, string fuse, string argst)
        {
            while (player.Tux.Count > 0)
            {
                string select = XI.AsyncInput(player.Uid, "/Q" + (player.Tux.Count > 1 ? ("1~" + player.Tux.Count) : "1") +
                    "(p" + string.Join("p", player.Tux) + "),/T1" + FormatPlayers(p => p.IsTared &&
                    p.Team == player.Team && p.Uid != player.Uid), "JNH0703", "0");
                if (!select.StartsWith("/") && !select.StartsWith(VI.CinSentinel))
                {
                    int idx = select.LastIndexOf(',');
                    ushort to = ushort.Parse(select.Substring(idx + 1));
                    string deliver = select.Substring(0, idx);
                    ushort[] delivers = deliver.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1," + delivers.Length + "," + deliver);
                }
            }
            XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
        }
        #endregion HL007 - Yingyue
        #region HL008 - Yingyu
        public void JNH0801Action(Player player, int type, string fuse, string argst)
        {
            ushort discard = ushort.Parse(argst);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + discard);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Player> invs = harms.Where(p => p.Element != FiveElement.LOVE)
                .Select(p => XI.Board.Garden[p.Who]).Where(p => p.IsAlive).Distinct().ToList();
            XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p.Uid + ",0,1")));

            List<Player> above = invs.Where(p => p.IsAlive &&
                p.Tux.Count > 0 && p.Tux.Count > p.TuxLimit).ToList();
            if (above.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",",
                    above.Select(p => p.Uid + ",1," + (p.Tux.Count - p.TuxLimit))));
            List<Player> below = invs.Where(p => p.IsAlive &&
                p.Tux.Count <= p.TuxLimit).Except(above).ToList();
            foreach (Player py in below)
                XI.RaiseGMessage("G0DS," + py.Uid + ",0,1");
        }
        public bool JNH0801Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return player.Tux.Count > 0 && harms.Any(p =>
                XI.Board.Garden[p.Who].IsAlive && p.Element != FiveElement.LOVE);
        }
        public string JNH0801Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#弃置的,/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public void JNH0802Action(Player player, int type, string fuse, string argst)
        {
            int cmidx = argst.IndexOf(',');
            ushort tux = ushort.Parse(argst.Substring(0, cmidx));
            ushort ut = ushort.Parse(argst.Substring(cmidx + 1));
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + tux);
            if (XI.Board.Hinder == player)
                XI.RaiseGMessage("G0AF," + player.Uid + ",6," + ut + ",2");
            else if (XI.Board.Supporter == player)
                XI.RaiseGMessage("G0AF," + player.Uid + ",5," + ut + ",1");
            XI.RaiseGMessage("G09P,0");
        }
        public bool JNH0802Valid(Player player, int type, string fuse)
        {
            if ((XI.Board.Hinder == player || XI.Board.Supporter == player) && player.Tux.Count > 0)
            {
                if (XI.Board.Supporter == player)
                {
                    List<ushort> psTars = XI.Board.PosSupporters.Where(p => p.StartsWith("T"))
                        .Select(p => ushort.Parse(p.Substring("T".Length))).ToList();
                    psTars = psTars.Where(p => XI.Board.Garden[p].IsTared &&
                        p != XI.Board.Rounder.Uid && p != player.Uid).ToList();
                    return psTars.Any();
                }
                else if (XI.Board.Hinder == player)
                {
                    List<ushort> psTars = XI.Board.PosHinders.Where(p => p.StartsWith("T"))
                        .Select(p => ushort.Parse(p.Substring("T".Length))).ToList();
                    psTars = psTars.Where(p => XI.Board.Garden[p].IsTared && p != player.Uid).ToList();
                    return psTars.Any();
                }
                return false;
            }
            return false;
        }
        public string JNH0802Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> tuxes = player.Tux.ToList();
                if (tuxes.Count > 0)
                    return "/Q1(p" + string.Join("p", tuxes) + ")";
                else
                    return "/";
            }
            else if (prev.IndexOf(',') < 0)
            {
                if (XI.Board.Supporter == player)
                {
                    List<ushort> psTars = XI.Board.PosSupporters.Where(p => p.StartsWith("T"))
                        .Select(p => ushort.Parse(p.Substring("T".Length))).ToList();
                    psTars = psTars.Where(p => XI.Board.Garden[p].IsTared &&
                        p != XI.Board.Rounder.Uid && p != player.Uid).ToList();
                    return "/T1(p" + string.Join("p", psTars) + ")";
                }
                else if (XI.Board.Hinder == player)
                {
                    List<ushort> psTars = XI.Board.PosHinders.Where(p => p.StartsWith("T"))
                        .Select(p => ushort.Parse(p.Substring("T".Length))).ToList();
                    psTars = psTars.Where(p => XI.Board.Garden[p].IsTared && p != player.Uid).ToList();
                    return "/T1(p" + string.Join("p", psTars) + ")";
                }
                return "";
            }
            else
                return "";
        }
        #endregion HL008 - Yingyu
        #region HL009 - Lingjian
        public bool JNH0901Valid(Player player, int type, string fuse)
        {
            List<ushort> cands = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type != Base.Card.Tux.TuxType.TP).ToList();
            if (cands.Count >= 2)
            {
                string[] kekkai = new string[] { "TP01,0", "TPT3,0", "TPT3,1", "ZP01,0", "TPT1,0" };
                // G0CC,A,0,B,KN,x1,x2;TF
                int idx = fuse.IndexOf(';');
                string[] g0cc = fuse.Substring(0, idx).Split(',');
                string tuxType = Util.Substring(fuse, idx + 1, fuse.IndexOf(',', idx + 1));
                string kn = g0cc[4];
                Tux ktux = XI.LibTuple.TL.EncodeTuxCode(kn);
                ushort who = ushort.Parse(g0cc[1]);
                if (who != player.Uid && ktux != null && ktux.Type == Tux.TuxType.TP
                        && !kekkai.Contains(kn + "," + tuxType))
                    return true;
            }
            return false;
        }
        public void JNH0901Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            int hdx = fuse.IndexOf(';');
            string[] g0cc = Util.Substring(fuse, 0, hdx).Split(',');
            ushort ust = ushort.Parse(g0cc[1]);
            ushort[] txs = Util.TakeRange(g0cc, 5, g0cc.Length).Select(p => ushort.Parse(p)).ToArray();
            // List<ushort> txis = txs.Where(p => XI.Board.TuxDises.Contains(p)).ToList();
            if (txs.Length > 0)
            {
                ushort hst = ushort.Parse(g0cc[3]);
                foreach (ushort p in txs)
                    XI.Board.PendingTux.Remove(hst + ",G0CC," + p);
                XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + string.Join(",", txs));
            }

            int kdx = fuse.IndexOf(',', hdx);
            if (kdx >= 0)
            {
                string origin = Util.Substring(fuse, kdx + 1, -1);
                //if (origin.StartsWith("G0CC"))
                //    XI.InnerGMessage(origin, 141);
                //else if (origin.StartsWith("G"))
                //    XI.RaiseGMessage(origin);
                if (origin.StartsWith("G"))
                {
                    string cardname = g0cc[4];
                    int inType = int.Parse(Util.Substring(fuse, hdx + 1, kdx));
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    int prior = tux.Priorities[inType];
                    XI.InnerGMessage(origin, prior);
                }
            }
        }
        public string JNH0901Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cands = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type != Base.Card.Tux.TuxType.TP).ToList();
                return "/Q2(p" + string.Join("p", cands) + ")";
            }
            else
                return "";
        }
        public bool JNH0902Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                if (Util.TryNotEmpty(player.RAM, "ZPName") &&
                    player.RAMUshort == 0 && player.Tux.Count > 0)
                {
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode((string)player.RAM["ZPName"]);
                    if (tux != null && tux.Valid(player, 0, fuse))
                        return true;
                }
                return false;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                if (player.RAMUshort == 0 && blocks[1].Equals(player.Uid.ToString()))
                {
                    string cardCode = blocks[4];
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardCode);
                    if (tux != null && tux.Type == Base.Card.Tux.TuxType.ZP)
                        return true;
                }
            }
            else if (type == 2)
                return Util.TryNotEmpty(player.RAM, "ZPName") || player.RAMUshort != 0;
            return false;
        }
        public void JNH0902Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                if (player.IsAlive)
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0," +
                        player.Uid + "," + (string)player.RAM["ZPName"] + "," + ut + ";0," + fuse);
                player.RAMUshort = 1;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                player.RAM["ZPName"] = blocks[4];
            }
            else if (type == 2)
            {
                player.RAMUshort = 0;
                player.RAM["ZPName"] = "";
            }
        }
        public string JNH0902Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNH0903Valid(Player player, int type, string fuse)
        {
            if (type == 0) // G0CC
            {
                string[] g0cc = Util.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                int n = g0cc.Length - 5;
                if (g0cc[5] == "0")
                    --n;
                return g0cc[1] == player.Uid.ToString() && n > 0;
            }
            else if (type == 1) // G1DI
            {
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length;)
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who == player.Uid && !drIn && n > 0)
                        return true;
                    idx += (4 + n);
                }
                return false;
            }
            else if (type == 2) // G0ZB
            {
                string[] g0zb = fuse.Split(',');
                if (g0zb[2] == "0" && g0zb[1] == player.Uid.ToString())
                    return true;
                else if (g0zb[2] == "1" && g0zb[3] == player.Uid.ToString())
                    return true;
                else if (g0zb[2] == "4" && g0zb[1] == player.Uid.ToString())
                    return true;
                else
                    return false;
            }
            else if (type == 3) // R*TM
                return player.RAMInt > player.HP;
            else return false;
        }
        public void JNH0903Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] g0cc = Util.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                int n = g0cc.Length - 5;
                if (g0cc[5] == "0")
                    --n;
                player.RAMInt += n;
            }
            else if (type == 1)
            {
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length;)
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who == player.Uid && !drIn && n > 0)
                        player.RAMInt += n;
                    idx += (4 + n);
                }
            }
            else if (type == 2)
            {
                string[] g0zb = fuse.Split(',');
                if (g0zb[2] == "0" && g0zb[1] == player.Uid.ToString())
                    player.RAMInt += (g0zb.Length - 3);
                else if (g0zb[2] == "1" && g0zb[3] == player.Uid.ToString())
                    player.RAMInt += (g0zb.Length - 4);
                else if (g0zb[2] == "4" && g0zb[1] == player.Uid.ToString())
                    player.RAMInt += (g0zb.Length - 3);
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OY,1," + player.Uid);
                XI.RaiseGMessage("G0OS," + player.Uid + ",0,JNH0901,JNH0902,JNH0903");
                XI.RaiseGMessage("G0IY,1," + player.Uid + ",19015");
                XI.RaiseGMessage("G0IS," + player.Uid + ",0,JNH1501,JNH1502");
            }
        }
        #endregion HL009 - Lingjian
        #region HL010 - ShuiLingjing
        public bool JNH1001Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1) // G0IC, G0OC, G0IS, G0OS, R*BC
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    Player py = XI.Board.Garden[who];
                    if (player.IsAlive && py.Team == player.Team)
                        return true;
                }
                return false;
            }
            else if (type == 2 || type == 3)
                return IsMathISOS("JNH1001", player, fuse) && XI.Board.Garden.Values
                    .Any(p => p.IsAlive && p.Team == player.Team && p.GetActivePetCount(XI.Board) > 0);
            else if (type == 4) // G0HT,A,n
            {
                string[] g0ht = fuse.Split(',');
                Player py = XI.Board.Garden[ushort.Parse(g0ht[1])];
                return py.Team == player.Team && py.GetPetCount() > 0;
            }
            else
                return false;
        }
        public void JNH1001Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                string[] blocks = fuse.Split(',');
                IDictionary<ushort, int> table = new Dictionary<ushort, int>();
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    if (table.ContainsKey(who))
                        ++table[who];
                    else
                        table.Add(who, 1);
                }
                foreach (var pair in table)
                {
                    if (type == 0)
                        XI.Board.Garden[pair.Key].TuxLimit += pair.Value;
                    else if (type == 1)
                        XI.Board.Garden[pair.Key].TuxLimit -= pair.Value;
                }
            }
            else if (type == 2)
            {
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.Team)
                    {
                        int count = py.GetActivePetCount(XI.Board);
                        py.TuxLimit += count;
                    }
            }
            else if (type == 3)
            {
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.Team)
                    {
                        int count = py.GetActivePetCount(XI.Board);
                        py.TuxLimit -= count;
                    }
            }
            else if (type == 4)
            {
                string[] g0ht = fuse.Split(',');
                Player py = XI.Board.Garden[ushort.Parse(g0ht[1])];
                TargetPlayer(player.Uid, py.Uid);
                bool yesAction = false;
                if (py.Tux.Count > 0)
                {
                    string ques = XI.AsyncInput(py.Uid, "#您是否弃置所有手牌？##是##否,Y2", "JNH1001", "0");
                    if (ques == "1")
                    {
                        XI.RaiseGMessage("G0QZ," + py.Uid + "," + string.Join(",", py.Tux));
                        yesAction = true;
                    }
                    else
                        yesAction = false;
                }
                else yesAction = true;
                if (yesAction)
                {
                    int n = int.Parse(g0ht[2]);
                    XI.InnerGMessage("G0HT," + py.Uid + "," + (n + py.GetPetCount()), 51);
                }
                else
                    XI.InnerGMessage(fuse, 51);
            }
        }
        public bool JNH1002Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py.Team == player.Team && harm.N >= py.HP && harm.Element != FiveElement.LOVE)
                    return true;
            }
            return false;
        }
        public void JNH1002Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            ushort mon = XI.LibTuple.ML.Encode("GSH3");
            if (mon != 0)
            {
                XI.RaiseGMessage("G0HC,1," + tar + ",0,1," + mon);
                XI.RaiseGMessage("G0ZW," + player.Uid);
            }
        }
        public string JNH1002Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> tars = XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Uid != player.Uid && p.Team == player.Team).Select(p => p.Uid).ToList();
                return tars.Count > 0 ? ("#获得宠物「水菱晶」,/T1(p" + string.Join("p", tars) + ")") : "/";
            }
            else
                return "";
        }
        #endregion HL010 - ShuiLingjing
        #region HL011 - ShuiGang
        public bool JNH1101Valid(Player player, int type, string fuse)
        {
            if (XI.Board.IsAttendWar(player))
            {
                bool b1 = XI.Board.Garden.Values.Any(p => p.IsAlive && p.Uid != player.Uid
                    && XI.Board.IsAttendWar(p) && !XI.Board.IsAttendWarSucc(p) && p.ListOutAllCards().Count > 0);
                bool b2 = !XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && XI.Board.IsAttendWar(p));
                return b1 || b2;
            }
            else
                return false;
        }
        public void JNH1101Action(Player player, int type, string fuse, string argst)
        {
            List<Player> losses = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Uid != player.Uid
                    && XI.Board.IsAttendWar(p) && !XI.Board.IsAttendWarSucc(p)).ToList();
            TargetPlayer(player.Uid, losses.Select(p => p.Uid));
            IDictionary<ushort, string> dict = new Dictionary<ushort, string>();
            foreach (Player py in losses)
                dict.Add(py.Uid, "#交出的,Q1(p" + string.Join("p", py.ListOutAllCards()) + ")");
            IDictionary<ushort, string> result = XI.MultiAsyncInput(dict);
            foreach (var pair in result)
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + string.Join(",", pair.Key + ",1,1," + pair.Value));
            if (!XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && XI.Board.IsAttendWar(p)))
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
        }
        public bool JNH1102Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.IsAttendWar(player);
            else if (type == 1 && player.TokenFold.Count > 0 && player.RAMUshort == 1) // G0CE
            {
                int idx = fuse.IndexOf(';');
                string[] g0ce = fuse.Substring(0, idx).Split(',');
                Player py = XI.Board.Garden[ushort.Parse(g0ce[1])];
                return py != null && py.Team == player.OppTeam;
            }
            else if (type == 2 && player.TokenFold.Count > 0 && player.RAMUshort == 1) // G1CW, only sheild the first one
            {
                int fdx = fuse.IndexOf(';');
                string[] g1cw = fuse.Substring(0, fdx).Split(',');
                Player py = XI.Board.Garden[ushort.Parse(g1cw[1])];
                return py != null && py.Team == player.OppTeam;
            }
            else if (type == 3 || type == 4)
                return player.TokenFold.Count > 0 || player.RAMUshort != 0;
            else
                return false;
        }
        public void JNH1102Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                List<ushort> ijs = player.Tux.ToList();
                int n = player.Tux.Count;
                XI.RaiseGMessage("G0OT," + player.Uid + "," + n + "," + string.Join(",", ijs));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",4," + n + "," + string.Join(",", ijs));
                player.RAMUshort = 1;
            }
            else if (type == 1)
            {
                player.RAMUshort = 0;
                int hdx = fuse.IndexOf(';');
                string[] g0ce = Util.Substring(fuse, 0, hdx).Split(',');
                int kdx = fuse.IndexOf(',', hdx);
                string origin = Util.Substring(fuse, kdx + 1, -1);
                XI.RaiseGMessage("G2CL," + ushort.Parse(g0ce[1]) + "," + g0ce[4]);
                if (origin.StartsWith("G") && g0ce[2] != "2") // Avoid Double Computation on Copy
                {
                    string cardname = g0ce[4];
                    int inType = int.Parse(Util.Substring(fuse, hdx + 1, kdx));
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    int prior = tux.Priorities[inType];
                    XI.InnerGMessage(origin, prior);
                }
            }
            else if (type == 2)
            {
                player.RAMUshort = 0;
                int fdx = fuse.IndexOf(';');
                int hdx = fuse.IndexOf(';', fdx + 1);
                int idx = fuse.IndexOf(',', hdx);
                int sktInType = int.Parse(Util.Substring(fuse, hdx + 1, idx));
                string sktFuse = Util.Substring(fuse, idx + 1, -1);
                string cdFuse = Util.Substring(fuse, fdx + 1, -1);

                string[] g1cw = fuse.Substring(0, fdx).Split(',');
                ushort first = ushort.Parse(g1cw[1]);
                ushort second = ushort.Parse(g1cw[2]);
                ushort provider = ushort.Parse(g1cw[3]);
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(g1cw[4]);

                string last = null;
                foreach (string tuxInfo in XI.Board.PendingTux)
                {
                    List<ushort> accu = new List<ushort>();
                    string[] parts = tuxInfo.Split(',');
                    string utstr = parts[0];
                    if (parts[1] == "G0CC")
                        last = tuxInfo;
                }
                if (last != null)
                {
                    XI.Board.PendingTux.Remove(last);
                    Player locuster = XI.Board.Garden[provider];
                    ushort locustee = ushort.Parse(last.Split(',')[2]);
                    bool b1 = locuster.IsAlive && player.IsAlive && tux.Valid(player, type, sktFuse);
                    if (!b1)
                        XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                    else
                    {
                        if ((tux.IsEq[sktInType] & 3) == 0)
                            XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                        XI.Board.PendingTux.Enqueue(locuster.Uid + ",G0CC," + locustee);
                    }
                    XI.RaiseGMessage("G2CL," + first + "," + g1cw[4]);
                    XI.InnerGMessage("G0CC," + provider + ",1," + second +
                        "," + tux.Code + "," + locustee + ";" + sktInType + "," + sktFuse, 101);
                }
                XI.InnerGMessage(cdFuse, 106);
            }
            else if (type == 3 || type == 4)
            {
                if (player.TokenFold.Count > 0)
                {
                    List<ushort> ijs = player.TokenFold.ToList();
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",4," + ijs.Count + "," + string.Join(",", ijs));
                    XI.RaiseGMessage("G0IT," + player.Uid + "," + ijs.Count + "," + string.Join(",", ijs));
                }
                player.RAMUshort = 0;
            }
        }
        #endregion HL011 - ShuiGang
        #region HL012 - LiuYing'er
        public void JNH1201Action(Player player, int type, string fuse, string argst)
        {
            List<Player> pys = new List<Player>();
            foreach (ushort put in XI.Board.OrderedPlayer())
            {
                Player py = XI.Board.Garden[put];
                if (py.IsAlive && py.Team == player.OppTeam && py.Tux.Count > 0)
                    pys.Add(py);
            }
            //List<Player> pys = XI.Board.Garden.Values.Where(p =>
            //    p.IsAlive && p.Team == player.OppTeam && p.Tux.Count > 0).ToList();
            List<ushort> cards = new List<ushort>();
            foreach (Player py in pys)
            {
                List<ushort> myTux = player.Tux.ToList();
                string c0 = Util.RepeatString("p0", XI.Board.Garden[py.Uid].Tux.Count);
                XI.AsyncInput(player.Uid, "#获得" + XI.LibTuple.HL.InstanceHero(py.SelectHero).Name
                    + "的,C1(" + c0 + ")", "JNH1201", "0");
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + py.Uid + ",2,1");
                cards.AddRange(player.Tux.Except(myTux));
            }
            pys = XI.Board.Garden.Values.Where(p =>
                p.IsTared && p.Team == player.OppTeam).ToList();
            while (cards.Count > 0)
            {
                string title = (cards.Count == 1) ? "Q1" : ("Q1~" + cards.Count);
                title += "(p" + string.Join("p", cards) + ")";
                string input = XI.AsyncInput(player.Uid, "#分配的," + title + ",#分配的,/T1(p" +
                    string.Join("p", pys.Select(p => p.Uid)) + ")", "JNH1201", "0");

                if (!input.StartsWith("/"))
                {
                    ushort[] uts = input.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    ushort tar = uts[uts.Length - 1];
                    ushort[] tuxs = Util.TakeRange(uts, 0, uts.Length - 1);
                    XI.RaiseGMessage("G0HQ,0," + tar + "," + player.Uid
                            + ",1," + tuxs.Length + "," + string.Join(",", tuxs));
                    cards.RemoveAll(p => tuxs.Contains(p));
                }
            }
        }
        public bool JNH1201Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Team == player.OppTeam && p.Tux.Count > 0).Any();
        }
        public void JNH1202Action(Player player, int type, string fuse, string prev)
        {
            TargetPlayer(player.Uid, XI.Board.Garden.Values
                .Where(p => p.IsAlive && p.Uid != player.Uid).Select(p => p.Uid));
            string result = Util.SParal(XI.Board, p => p.IsAlive &&
                p.Uid != player.Uid && p.Tux.Count > 0, p => p.Uid + ",1,1", ",");
            if (!string.IsNullOrEmpty(result))
                XI.RaiseGMessage("G0DH," + result);
            Cure(player, XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.Team), 1);
        }
        public bool JNH1202Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count >= player.TuxLimit;
        }
        #endregion HL012 - LiuYing'er
        #region HL013 - Xiongshanjun
        public bool JNH1301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player) && (
                    XI.Board.RestMonPiles.Count + XI.Board.RestMonDises.Count > 0);
            else if (type == 1)
                return player.TokenExcl.Count >= 3;
            else if (type == 2 || type == 3)
            {
                string[] blocks = fuse.Split(',');
                return (blocks[1] == player.Uid.ToString() && blocks[2] == "1" && blocks[3] != "0");
            }
            return false;
        }
        public void JNH1301Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort pop = XI.Board.RestMonPiles.Dequeue();
                Monster mon = XI.LibTuple.ML.Decode(pop);
                XI.RaiseGMessage("G0YM,5," + pop);
                int delta = mon.STR / 2;
                if (delta > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + delta);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,M" + pop);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,M" + pop);
            }
            else if (type == 1)
            {
                List<string> animals = player.TokenExcl.ToList();
                string sanimals = string.Join(",", animals);
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + animals.Count + "," + sanimals);
                XI.RaiseGMessage("G0ON," + player.Uid + ",M," + animals.Count +
                     "," + string.Join(",", animals.Select(p => p.Substring("M".Length))));
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + sanimals);
            }
            else if (type == 2 || type == 3)
            {
                int agl = player.TokenExcl.Select(p => XI.LibTuple.ML.Decode(
                    ushort.Parse(p.Substring("M".Length)))).Sum(p => p.AGL) / 2;
                int delta = agl - player.ROMUshort;
                if (delta > 0)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0," + delta);
                else if (delta < 0)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0," + (-delta));
                player.ROMUshort = (ushort)agl;

                string[] blocks = fuse.Split(',');
                int n = int.Parse(blocks[3]);
                XI.RaiseGMessage((type == 2 ? "G0IA," : "G0OA,") + player.Uid + ",0," + n);
            }
        }
        public bool JNH1302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return fuse == "G0FI,O" && XI.Board.Rounder.Team == player.OppTeam;
            else if (type == 1)
            {
                string[] g1yp = fuse.Split(',');
                for (int i = 1; i < g1yp.Length; i += 2)
                {
                    Player py = XI.Board.Garden[ushort.Parse(g1yp[i])];
                    if (py.Team == player.OppTeam)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void JNH1302Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == player.Team).Select(p => p.Uid + ",0,1")));
        }
        #endregion HL013 - Xiongshanjun
        #region HL014 - Moxiang
        public bool JNH1401Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNH1401", player, fuse);
            else if (type == 2)
            {
                // :[ Mainly to solve now
                // G0CE,A,T,0,KN,y,z;TF
                string[] g0ce = Util.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                ushort who = ushort.Parse(g0ce[1]);
                string tuxName = g0ce[4];
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(tuxName);
                return who == player.Uid && tux.Type == Tux.TuxType.ZP;
            }
            else
                return false;
        }
        public void JNH1401Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                foreach (Tux tux in XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.ZP))
                    player.cz01PriceDict[tux.Code] = 1;
            }
            else if (type == 1)
            {
                foreach (Tux tux in XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.ZP))
                    player.cz01PriceDict.Remove(tux.Code);
            }
            else if (type == 2)
            {
                int hdx = fuse.IndexOf(';');
                string[] g0cc = Util.Substring(fuse, 0, hdx).Split(',');
                int kdx = fuse.IndexOf(',', hdx);
                string origin = Util.Substring(fuse, kdx + 1, -1);
                if (origin.StartsWith("G"))
                {
                    string cardname = g0cc[4];
                    int inType = int.Parse(Util.Substring(fuse, hdx + 1, kdx));
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    int prior = tux.Priorities[inType];
                    XI.InnerGMessage(origin, prior);
                }
            }
        }
        public bool JNH1402Valid(Player player, int type, string fuse)
        {
            int meScore = (player.Team == 1) ? XI.Board.FinalAkaScore : XI.Board.FinalAoScore;
            int opScore = XI.Board.FinalAkaScore + XI.Board.FinalAoScore - meScore;

            if (meScore < opScore || (meScore == opScore && player.Team == 1))
            {
                int peopleDelta = XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.Team)
                    - XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.OppTeam);
                if (peopleDelta > 0)
                    meScore += (5 * peopleDelta);
                return meScore >= opScore;
            }
            else
                return false;
        }
        public void JNH1402Action(Player player, int type, string fuse, string argst)
        {
            if (player.Team == 1)
                XI.RaiseGMessage("G0WN,1");
            else if (player.Team == 2)
                XI.RaiseGMessage("G0WN,2");
        }
        public bool JNH1403Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                IDictionary<int, int> dicts = XI.CalculatePetsScore();
                if (dicts[player.Team] >= 30)
                    return true;
                else
                    return false;
            }
            else if (type == 1)
                return player.TokenAwake;
            else
                return false;
        }
        public void JNH1403Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
            else if (type == 1)
                XI.RaiseGMessage("G1WJ,0");
        }
        #endregion HL014 - Moxiang
        #region HL015 - LiJianling
        public bool JNH1501Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
            {
                if (IsMathISOS("JNH1501", player, fuse))
                {
                    foreach (Player py in XI.Board.Garden.Values)
                    {
                        if (py.Uid != player.Uid && py.IsAlive && py.Team == player.Team)
                        {
                            Hero hero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                            if (hero != null && !hero.Bio.Contains("K"))
                                return true;
                        }
                    }
                }
            }
            else if (type == 2)
            { // GOIY,0/1,A,S
                string[] blocks = fuse.Split(',');
                ushort who = ushort.Parse(blocks[2]);
                if (who != player.Uid && (who + player.Uid) % 2 == 0)
                {
                    int heroCode = int.Parse(blocks[3]);
                    var hero = XI.LibTuple.HL.InstanceHero(heroCode);
                    if (hero != null && !hero.Bio.Contains("K"))
                        return true;
                }
            }
            else if (type == 3)
            { // GOOY,0/1,A
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    if (who != player.Uid && (who + player.Uid) % 2 == 0)
                    {
                        int heroCode = XI.Board.Garden[who].SelectHero;
                        var hero = XI.LibTuple.HL.InstanceHero(heroCode);
                        if (hero != null && !hero.Bio.Contains("K"))
                            return true;
                    }
                }
            }
            return false;
        }
        public void JNH1501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                string title = (type == 0) ? "G0IX," : "G0OX,";
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.Uid != player.Uid && py.IsAlive && py.Team == player.Team)
                    {
                        Hero hero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                        if (hero != null && !hero.Bio.Contains("K"))
                            XI.RaiseGMessage(title + py.Uid + ",0,2");
                    }
                }
            }
            else if (type == 2)
            {
                string[] blocks = fuse.Split(',');
                ushort who = ushort.Parse(blocks[2]);
                if (who != player.Uid && (who + player.Uid) % 2 == 0)
                {
                    int heroCode = int.Parse(blocks[3]);
                    var hero = XI.LibTuple.HL.InstanceHero(heroCode);
                    if (hero != null && !hero.Bio.Contains("K"))
                        XI.RaiseGMessage("G0IX," + who + ",0,2");
                }
            }
            else if (type == 3)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    if (who != player.Uid && (who + player.Uid) % 2 == 0)
                    {
                        int heroCode = XI.Board.Garden[who].SelectHero;
                        var hero = XI.LibTuple.HL.InstanceHero(heroCode);
                        if (hero != null && !hero.Bio.Contains("K"))
                            XI.RaiseGMessage("G0OX," + who + ",0,2");
                    }
                }
            }
        }
        public bool JNH1502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] g0hcs = fuse.Split(',');
                ushort to = ushort.Parse(g0hcs[2]);
                ushort from = ushort.Parse(g0hcs[3]);
                return XI.Board.Garden[to].Team == player.Team && (from == 0 ||
                    XI.Board.Garden[to].Team != XI.Board.Garden[from].Team) &&
                    XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.Team && p.GetPetCount() > 0);
            }
            else if (type == 1)
                return player.TokenExcl.Count > 0;
            else
                return false;
        }
        public void JNH1502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort[] uts = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage("G0HL," + uts[0] + "," + uts[1]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,M" + uts[1]);
                XI.RaiseGMessage("G2TZ," + uts[0] + "," + player.Uid + ",M" + uts[1]);
                Cure(player, player, 1);
            }
            else if (type == 1)
            {
                int delta = player.TokenExcl.Sum(p => XI.LibTuple.ML.Decode(
                        ushort.Parse(p.Substring("M".Length))).STR);
                if (player.Team == 1)
                    XI.Board.FinalAkaScore += delta;
                else
                    XI.Board.FinalAoScore += delta;
            }
        }
        public string JNH1502Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                    return "#放逐宠物,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                        p => p.IsAlive && p.Team == player.Team && p.GetPetCount() > 0).Select(p => p.Uid)) + ")";
                else if (prev.IndexOf(',') < 0)
                {
                    Player py = XI.Board.Garden[ushort.Parse(prev)];
                    return "#放逐宠物,/M1(p" + string.Join("p", py.Pets.Where(p => p != 0)) + ")";
                }
                else return "";
            }
            else
                return "";
        }
        #endregion HL015 - LiJianling
        #region HL016 - WangFeixia
        public bool JNH1601Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.TokenExcl.Count > 0 || player.Guardian != 0;
            else if (type == 1)
                return IsMathISOS("JNH1601", player, fuse);
            else
                return false;
        }
        public void JNH1601Action(Player player, int type, string fuse, string argst)
        {
            IDictionary<ushort, ushort> im = new Dictionary<ushort, ushort>();
            IDictionary<ushort, ushort> jm = new Dictionary<ushort, ushort>();
            IDictionary<ushort, string> isk = new Dictionary<ushort, string>();
            for (ushort i = 0; i < 8; ++i)
            {
                im[(ushort)(15 + i)] = (ushort)(10 + i);
                jm[(ushort)(10 + i)] = (ushort)(15 + i);
                isk[(ushort)(15 + i)] = i < 7 ? ("JNH160" + (3 + i)) : ("JNH16" + (3 + i));
            }

            if (type == 0)
            {
                ushort iNo = player.Guardian;
                if (iNo != 0)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                    if (jm.ContainsKey(iNo))
                        XI.RaiseGMessage("G0OS," + player.Uid + ",1," + isk[jm[iNo]]);
                }
            }
            else if (type == 1)
            {
                string part = string.Join(",", im.Keys.Select(p => "I" + p));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + im.Keys.Count + "," + part);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + part);
            }
            if (player.TokenExcl.Count > 0)
            {
                string input = XI.AsyncInput(player.Uid, "I1(p" +
                    string.Join("p", player.TokenExcl) + ")", "JNH1601", "0");
                ushort iNo = ushort.Parse(input);
                if (im.ContainsKey(iNo))
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + "," + im[iNo]);
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1," + isk[iNo]);
                }
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + iNo);
            }
        }
        public bool JNH1602Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.ROMUshort == 0)
            {
                var hl = XI.LibTuple.HL;
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0 &&
                    (p.Uid == player.Uid || hl.InstanceHero(p.SelectHero).Bio.Contains("H")));
            }
            else if (type == 1 && player.ROMUshort == 2)
                return true;
            else if (type == 2 && IsMathISOS("JNH1602", player, fuse) && player.ROMUshort == 2)
                return XI.Board.InFight;
            else if (type == 3 && player.ROMUshort == 1)
                return XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Sum(p => p.GetPetCount()) >= 3;
            else if (type == 4 && player.ROMUshort == 2)
                return XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Sum(p => p.GetPetCount()) < 3;
            else
                return false;
        }
        public void JNH1602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort tg = ushort.Parse(argst);
                XI.RaiseGMessage(Artiad.Cure.ToMessage(new Artiad.Cure(
                    tg, player.Uid, FiveElement.A, 3)));
                ushort guardian = player.Guardian;
                if (player.Guardian != 0)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                    int ki = guardian - 10 + 3;
                    string skillName = ki < 10 ? (",JNH160" + ki) : (",JNH16" + ki);
                    XI.RaiseGMessage("G0OS,1," + player.Uid + skillName);
                }
                if (player.TokenExcl.Count > 0)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + player.TokenExcl.Count
                        + "," + string.Join(",", player.TokenExcl));
                Base.Card.Hero hero = XI.LibTuple.HL.InstanceHero(player.SelectHero);
                if (hero != null)
                {
                    if (hero.DEX < 4)
                        XI.RaiseGMessage("G0IX," + player.Uid + ",0," + (4 - hero.DEX));
                    else if (hero.DEX > 4)
                        XI.RaiseGMessage("G0OX," + player.Uid + ",0," + (hero.DEX - 4));
                }
                else
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0,2");
                player.ROMUshort = 2;
                if (XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Sum(p => p.GetPetCount()) >= 3)
                {
                    player.ROMUshort = 2;
                    if (XI.Board.InFight)
                        XI.RaiseGMessage("G0IP," + player.Team + ",2");
                }
                else
                    player.ROMUshort = 1;
                XI.InnerGMessage("G0ZH,0", 0);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0IP," + player.Team + ",2");
            else if (type == 2)
                XI.RaiseGMessage("G0OP," + player.Team + ",2");
            else if (type == 3)
            {
                player.ROMUshort = 2;
                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0IP," + player.Team + ",2");
            }
            else if (type == 4)
            {
                player.ROMUshort = 1;
                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0OP," + player.Team + ",2");
            }
        }
        public string JNH1602Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsTared && p.HP == 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        private bool JNH16SeriesValid(Player player, int type, string fuse,
            string skillName, FiveElement elem, FiveElement adv, FiveElement disadv)
        {
            if (type == 0 || type == 1)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    ushort pt = ushort.Parse(blocks[i + 2]);
                    Player py = XI.Board.Garden[who];
                    Monster mon = XI.LibTuple.ML.Decode(pt);
                    if (player.IsAlive && mon != null && mon.Element == elem)
                        return true;
                }
                return false;
            }
            else if (type == 2 || type == 3)
            {
                int thisEle = Util.GetFiveElementId(elem);
                int advEle = Util.GetFiveElementId(adv);
                return IsMathISOS(skillName, player, fuse) && XI.Board.Garden.Values.
                    Any(p => p.IsAlive && (p.Pets[thisEle] != 0 || p.Pets[advEle] != 0));
            }
            else if (type == 4)
            {
                Monster fieldMon = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                return fieldMon != null && fieldMon.Element == disadv;
            }
            else if (type == 5)
            {
                string[] blocks = fuse.Split(',');
                ushort card = ushort.Parse(blocks[4]);
                Base.Card.Monster monster = XI.LibTuple.ML.Decode(card);
                return monster != null && monster.Element == adv;
            }
            else if (type == 6)
            {
                string[] blocks = fuse.Split(',');
                ushort card = ushort.Parse(blocks[2]);
                Base.Card.Monster monster = XI.LibTuple.ML.Decode(card);
                return monster != null && monster.Element == adv;
            }
            else
                return false;
        }
        private void JNH16SeriesAction(Player player, int type, string fuse,
            string skillName, FiveElement elem, FiveElement adv, FiveElement disadv)
        {
            if (type == 0 || type == 1)
            {
                string[] blocks = fuse.Split(',');
                IDictionary<ushort, int> table = new Dictionary<ushort, int>();
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    ushort pt = ushort.Parse(blocks[i + 2]);
                    Monster mon = XI.LibTuple.ML.Decode(pt);
                    if (mon != null && mon.Element == elem)
                    {
                        XI.RaiseGMessage((type == 0 ? "G0IA" : "G0OA") + "," + who + ",0,1");
                        XI.RaiseGMessage((type == 0 ? "G0IX" : "G0OX") + "," + who + ",0,1");
                    }
                }
            }
            else if (type == 2)
            {
                int thisEle = Util.GetFiveElementId(elem);
                int advEle = Util.GetFiveElementId(adv);
                List<ushort> actionPets = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsAlive && !p.PetDisabled))
                {
                    if (py.Pets[thisEle] != 0 && !XI.Board.NotActionPets.Contains(py.Pets[thisEle]))
                    {
                        XI.RaiseGMessage("G0IA," + py.Uid + ",0,1");
                        XI.RaiseGMessage("G0IX," + py.Uid + ",0,1");
                    }
                    if (py.Pets[advEle] != 0)
                        actionPets.Add(py.Pets[advEle]);
                }
                if (actionPets.Count > 0)
                    XI.RaiseGMessage("G0OE,1," + string.Join(",", actionPets));
            }
            else if (type == 3)
            {
                int thisEle = Util.GetFiveElementId(elem);
                int advEle = Util.GetFiveElementId(adv);
                List<ushort> actionPets = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsAlive && !p.PetDisabled))
                {
                    if (py.Pets[thisEle] != 0 && !XI.Board.NotActionPets.Contains(py.Pets[thisEle]))
                    {
                        XI.RaiseGMessage("G0OA," + py.Uid + ",0,1");
                        XI.RaiseGMessage("G0OX," + py.Uid + ",0,1");
                    }
                    if (py.Pets[advEle] != 0)
                        actionPets.Add(py.Pets[advEle]);
                }
                if (actionPets.Count > 0)
                    XI.RaiseGMessage("G0IE,1," + string.Join(",", actionPets));
            }
            else if (type == 4)
                XI.RaiseGMessage("G0IW," + XI.Board.Monster1 + ",1");
            else if (type == 5)
            {
                string[] blocks = fuse.Split(',');
                ushort card = ushort.Parse(blocks[4]);
                Base.Card.Monster monster = XI.LibTuple.ML.Decode(card);
                if (monster.Element == adv)
                    XI.Board.NotActionPets.Add(card);
            }
            else if (type == 6)
            {
                string[] blocks = fuse.Split(',');
                ushort card = ushort.Parse(blocks[2]);
                Base.Card.Monster monster = XI.LibTuple.ML.Decode(card);
                if (monster.Element == adv)
                    XI.Board.NotActionPets.Remove(card);
            }
        }
        public bool JNH1603Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1603",
                FiveElement.AQUA, FiveElement.AGNI, FiveElement.SATURN);
        }
        public void JNH1603Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1603",
                FiveElement.AQUA, FiveElement.AGNI, FiveElement.SATURN);
        }
        public bool JNH1604Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1604",
                FiveElement.AGNI, FiveElement.THUNDER, FiveElement.AQUA);
        }
        public void JNH1604Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1604",
                FiveElement.AGNI, FiveElement.THUNDER, FiveElement.AQUA);
        }
        public bool JNH1605Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1605",
                FiveElement.THUNDER, FiveElement.AERO, FiveElement.AGNI);
        }
        public void JNH1605Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1605",
                FiveElement.THUNDER, FiveElement.AERO, FiveElement.AGNI);
        }
        public bool JNH1606Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1606",
                FiveElement.AERO, FiveElement.SATURN, FiveElement.THUNDER);
        }
        public void JNH1606Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1606",
                FiveElement.AERO, FiveElement.SATURN, FiveElement.THUNDER);
        }
        public bool JNH1607Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1607",
                FiveElement.SATURN, FiveElement.AQUA, FiveElement.AERO);
        }
        public void JNH1607Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1607",
                FiveElement.SATURN, FiveElement.AQUA, FiveElement.AERO);
        }
        public bool JNH1608BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Tux.Count > 0)
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("ZP02");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH1608Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            ushort uid = player.Uid;
            XI.RaiseGMessage("G0CC," + uid + ",0," + uid + ",ZP02," + ut + ";0," + fuse);
            XI.RaiseGMessage("G0CZ,0," + player.Uid);
        }
        public string JNH1608Input(Player player, int type, string fuse, string prev)
        {
            if (prev.IndexOf(',') < 0)
            {
                List<ushort> tuxes = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.ZP).ToList();
                if (tuxes.Count > 0)
                    return "/Q1(p" + string.Join("p", tuxes) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JNH1609BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Tux.Count > 0 && ((type == 0 && XI.Board.Rounder.Uid == player.Uid) || type == 1))
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("TPT1");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH1609Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            ushort uid = player.Uid;
            XI.RaiseGMessage("G0CC," + uid + ",0," + uid + ",TPT1," + ut + ";" + type + "," + fuse);
        }
        public string JNH1609Input(Player player, int type, string fuse, string prev)
        {
            if (prev.IndexOf(',') < 0)
            {
                List<ushort> tuxes = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP).ToList();
                if (tuxes.Count > 0)
                    return "/Q1(p" + string.Join("p", tuxes) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JNH1610BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Uid == XI.Board.Rounder.Uid && player.Tux.Count > 0)
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("JP03");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH1610Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            ushort uid = player.Uid;
            XI.RaiseGMessage("G0CC," + uid + ",0," + uid + ",JP03," + ut + ";0," + fuse);
        }
        public string JNH1610Input(Player player, int type, string fuse, string prev)
        {
            if (prev.IndexOf(',') < 0)
            {
                List<ushort> tuxes = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP).ToList();
                if (tuxes.Count > 0)
                    return "/Q1(p" + string.Join("p", tuxes) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        #endregion HL016 - WangFeixia
        #region HL017 - Lian'er
        public bool JNH1701Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(
                p => p.IsTared && p.Team == player.OppTeam);
        }
        public void JNH1701Action(Player player, int type, string fuse, string argst)
        {
            int spIdx = argst.IndexOf(",0,");
            ushort[] tuxs = argst.Substring(0, spIdx).Split(',')
                .Select(p => ushort.Parse(p)).ToArray();
            ushort[] tars = argst.Substring(spIdx + ",0,".Length).Split(',')
                .Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", tuxs));
            TargetPlayer(player.Uid, tars);
            int mask = 0;
            foreach (ushort mut in tuxs)
            {
                Base.Card.Tux tx = XI.LibTuple.TL.DecodeTux(mut);
                if (tx.Type == Tux.TuxType.JP)
                    mask |= 0x1;
                else if (tx.Type == Tux.TuxType.TP)
                    mask |= 0x2;
                else if (tx.Type == Tux.TuxType.ZP)
                    mask |= 0x4;
                else if (tx.IsTuxEqiup())
                    mask |= 0x8;
            }
            List<Player> invs = new List<Player>();
            foreach (ushort tar in tars)
            {
                Player py = XI.Board.Garden[tar];
                List<ushort> uttux = new List<ushort>();
                foreach (ushort mut in py.Tux)
                {
                    Base.Card.Tux tx = XI.LibTuple.TL.DecodeTux(mut);
                    if (tx.Type == Tux.TuxType.JP && ((mask & 0x1) != 0))
                        uttux.Add(mut);
                    else if (tx.Type == Tux.TuxType.TP && ((mask & 0x2) != 0))
                        uttux.Add(mut);
                    else if (tx.Type == Tux.TuxType.ZP && ((mask & 0x4) != 0))
                        uttux.Add(mut);
                    else if (tx.IsTuxEqiup() && ((mask & 0x8) != 0))
                        uttux.Add(mut);
                }
                string hints = uttux.Count > 0 ? "#弃置(取消则HP-1),/Q1(p" + string.Join("p", uttux) + ")" : "/";
                string input = XI.AsyncInput(tar, hints, "JNH1701", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                    XI.RaiseGMessage("G0QZ," + tar + "," + input);
                else
                    invs.Add(py);
            }
            if (invs.Count > 0)
                Harm(player, invs, 1, FiveElement.AQUA);
        }
        public string JNH1701Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                if (player.Tux.Count > 1)
                    return "#弃置,/Q1~2(p" + string.Join("p", player.Tux) + "),!0";
                else
                    return "#弃置,/Q1(p" + player.Tux[0] + "),!0";
            }
            else if (prev.EndsWith(",0"))
            {
                string[] blocks = prev.Split(',');
                int tarsz = blocks.Length - 1;
                List<ushort> cands = XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Team == player.OppTeam).Select(p => p.Uid).ToList();
                if (tarsz > 1)
                    return "/T1~" + tarsz + "(p" + string.Join("p", cands) + ")";
                else
                    return "/T1(p" + string.Join("p", cands) + ")";
            }
            else
                return "";
        }
        public bool JNH1702Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Element != FiveElement.SOL &&
                        harm.Element != FiveElement.LOVE && harm.N == 1)
                    return true;
            }
            return false;
        }
        public void JNH1702Action(Player player, int type, string fuse, string argst)
        {
            ISet<Player> normals = new HashSet<Player>();
            //ISet<Player> roars = new HashSet<Player>();
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (harm.N == 1 && harm.Element != FiveElement.SOL &&
                         harm.Element != FiveElement.LOVE && py.Tux.Count > 0)
                    normals.Add(py);
                //if (harm.N == 1 && (harm.Element == FiveElement.YIN || (harm.Element == FiveElement.A &&
                //         Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.INCOUNTABLE))))
                //    roars.Add(py);
            }
            if (normals.Count > 0)
            {
                string tarHint = normals.Count > 1 ? ("/T1~" + normals.Count) : "/T1";
                string input = XI.AsyncInput(player.Uid, "#弃1张手牌(取消则您补1牌)," + tarHint + "(p" +
                     string.Join("p", normals.Select(p => p.Uid)) + ")", "JNH1702", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    List<Player> pys = input.Split(',').Select(
                        p => XI.Board.Garden[ushort.Parse(p)]).ToList();
                    foreach (Player py in pys)
                    {
                        if (py.Uid != player.Uid)
                        {
                            XI.AsyncInput(player.Uid, "#弃置的,C1(" +
                                Util.RepeatString("p0", py.Tux.Count) + ")", "JNH1702", "1");
                            XI.RaiseGMessage("G0DH," + py.Uid + ",2,1");
                        }
                    }
                    if (player.Tux.Count > 0)
                        XI.RaiseGMessage("G0DH," + player.Uid + ",1,1");
                }
                else
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
            else
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            //if (roars.Count > 0)
            //    Harm(player, roars, 4, FiveElement.AQUA);
        }
        public bool JNH1703Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0 && player.HP > 0)
                return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared && p.HP == 0);
            else
                return false;
        }
        public void JNH1703Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            List<ushort> eqs = player.ListOutAllEquips().ToList();
            if (eqs.Count > 0)
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", eqs));
            int hp = player.HP;
            XI.RaiseGMessage("G0HQ,0," + tar + "," + player.Uid + ",2," + player.Tux.Count);
            Cure(player, XI.Board.Garden[tar], hp);
            int maskDuel = Artiad.IntHelper.SetMask(0, GiftMask.INCOUNTABLE, true);
            Harm(player, player, hp, FiveElement.SOL, maskDuel);
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", 0);
        }
        public string JNH1703Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<Player> invs = XI.Board.Garden.Values.Where(p => p.Uid != player.Uid
                     && p.IsTared && p.HP == 0).ToList();
                return "/T1(p" + string.Join("p", invs.Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        #endregion HL017 - Lian'er
        #region HL018 - Xu'Nansong
        public bool JNH1801Valid(Player player, int type, string fuse)
        {
            return XI.Board.Supporter.Uid == player.Uid && XI.Board.Hinder.IsReal;
        }
        public void JNH1801Action(Player player, int type, string fuse, string argst)
        {
            if (XI.Board.Hinder.Uid != 0)
            {
                TargetPlayer(player.Uid, XI.Board.Hinder.Uid);
                if (player.ROMUshort != 2)
                    XI.RaiseGMessage("G0OX," + XI.Board.Hinder.Uid + ",1,4");
                else
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",1,4");
            }
        }
        public void JNH1802Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",TPT3," + card + ";" + type + "," + fuse);
        }
        public bool JNH1802Valid(Player player, int type, string fuse)
        {
            int idx = fuse.IndexOf(';');
            string[] blocks = fuse.Substring(0, idx).Split(',');
            string tuxCode = blocks[3];
            return player.Tux.Count > 0 && XI.LibTuple.TL.EncodeTuxCode(tuxCode).Type == Tux.TuxType.ZP &&
                XI.LibTuple.TL.EncodeTuxCode("TPT3").Valid(player, type, fuse);
        }
        public string JNH1802Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                int idx = fuse.IndexOf(';');
                string[] blocks = fuse.Substring(0, idx).Split(',');
                string tuxCode = blocks[3];
                List<ushort> cands = player.Tux.Where(p =>
                       XI.LibTuple.TL.DecodeTux(p).Code == tuxCode).ToList();
                return (cands.Count > 0) ? ("/Q1(p" + string.Join("p", cands) + ")") : "/";
            }
            else
                return "";
        }
        public bool JNH1803Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.ROMUshort == 0)
            {
                return XI.Board.MonPiles.Count > 0;
                // List<ushort> equips = new List<ushort>();
                // foreach (Player py in XI.Board.Garden.Values)
                //     if (py.Team == player.Team && py.IsAlive)
                //         equips.AddRange(py.Pets.Where(p => p != 0));
                // foreach (string ce in XI.Board.CsPets)
                // {
                //     int idx = ce.IndexOf(',');
                //     ushort pet = ushort.Parse(ce.Substring(idx + 1));
                //     equips.Remove(pet);
                // }
                // return equips.Count > 0;
            }
            else if (type == 1)
                return player.ROMUshort == 1;
            else
                return false;
        }
        public void JNH1803Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int monCnt = XI.Board.MonPiles.Count;
                if (monCnt > 3) monCnt = 3;
                List<ushort> mons = XI.Board.MonPiles.Dequeue(monCnt).ToList();
                XI.RaiseGMessage("G2IN,1," + monCnt);
                //List<ushort> fmons = XI.Board.MonPiles.Dequeue(monCnt).ToList();
                //int finalPoint = 0;
                //IDictionary<ushort, ushort> bombmon = new Dictionary<ushort, ushort>();
                //IDictionary<ushort, ushort> bombnpc = new Dictionary<ushort, ushort>();
                // show the monsters to the trigger
                XI.RaiseGMessage("G0YM,5," + string.Join(",", mons));
                while (mons.Count > 0)
                {
                    IDictionary<ushort, List<ushort>> dict = new Dictionary<ushort, List<ushort>>();
                    XI.RaiseGMessage("G2FU,0," + player.Uid + ",0,M," + string.Join(",", mons));
                    foreach (ushort mon in mons)
                    {
                        List<ushort> subSelection = new List<ushort>();
                        Monster monster = XI.LibTuple.ML.Decode(mon);
                        if (monster != null)
                        {
                            int five = Util.GetFiveElementId(monster.Element);
                            if (five != -1)
                            {
                                subSelection.AddRange(XI.Board.Garden.Values.Where(p => p.Team == player.Team &&
                                    p.IsAlive && p.Pets[five] != 0).Select(p => p.Pets[five]));
                            }
                        }
                        else if (Base.Card.NMBLib.IsNPC(mon))
                        {
                            foreach (Player py in XI.Board.Garden.Values)
                            {
                                if (py.Team == player.Team && py.IsAlive)
                                    subSelection.AddRange(py.Escue);
                            }
                        }
                        foreach (string ce in XI.Board.CsPets)
                        {
                            int idx = ce.IndexOf(',');
                            ushort pet = ushort.Parse(ce.Substring(idx + 1));
                            subSelection.Remove(pet);
                        }
                        // subSelection.RemoveAll(p => bombmon.ContainsKey(p) || bombnpc.ContainsKey(p));
                        if (subSelection.Count > 0)
                            dict[mon] = subSelection;
                    }
                    if (dict.Count > 0)
                    {
                        string input = XI.AsyncInput(player.Uid, "#增加战力,/M1(p" +
                            string.Join("p", dict.Keys) + ")", "JNH1803", "0");
                        if (!input.Contains(VI.CinSentinel) && !input.StartsWith("/"))
                        {
                            ushort which = ushort.Parse(input);
                            string input2 = XI.AsyncInput(player.Uid, "#爆发的,/M1(p" +
                                string.Join("p", dict[which]) + ")", "JNH1803", "1");
                            if (!input2.Contains(VI.CinSentinel) && !input2.StartsWith("/"))
                            {
                                ushort bomb = ushort.Parse(input2);
                                Monster monster = XI.LibTuple.ML.Decode(which);
                                if (monster != null)
                                {
                                    ushort who = XI.Board.Garden.Values.Where(p =>
                                        p.Pets.Contains(bomb)).Select(p => p.Uid).Single();
                                    XI.RaiseGMessage("G0HI," + who + "," + bomb);
                                    if (monster.STR > 0)
                                        XI.RaiseGMessage("G0IP," + player.Team + "," + monster.STR);
                                }
                                else if (Base.Card.NMBLib.IsNPC(which))
                                {
                                    NPC npc = XI.LibTuple.NL.Decode(Base.Card.NMBLib.OriginalNPC(which));
                                    ushort who = XI.Board.Garden.Values.Where(p =>
                                        p.Escue.Contains(bomb)).Select(p => p.Uid).Single();
                                    XI.Board.Garden[who].Escue.Remove(bomb);
                                    XI.RaiseGMessage("G2OL," + who + "," + bomb);
                                    XI.RaiseGMessage("G0ON," + who + ",M,1," + bomb);
                                    if (npc.STR > 0)
                                        XI.RaiseGMessage("G0IP," + player.Team + "," + npc.STR);
                                }
                                XI.Board.MonDises.Add(which);
                                XI.RaiseGMessage("G0ON,0,M,1," + which);
                                mons.Remove(which);
                            }
                        }
                        else
                            break;
                    }
                    else
                    {
                        XI.AsyncInput(player.Uid, "/", "JNH1803", "0");
                        break;
                    }
                }
                // if (!anySet)
                //     finalPoint = 2;
                // if (bombmon.Count > 0)
                //     XI.RaiseGMessage("G0HI," + string.Join(",", bombmon.Select(p => p.Value + "," + p.Key)));
                // if (bombnpc.Count > 0)
                // {
                //     foreach (var pair in bombnpc)
                //         XI.Board.Garden[pair.Value].Escue.Remove(pair.Key);
                //     XI.RaiseGMessage("G2OL," + string.Join(",", bombnpc.Select(p => p.Value + "," + p.Key)));
                // }
                // if (finalPoint > 0)
                //     XI.RaiseGMessage("G0IP," + player.Team + "," + finalPoint);
                if (mons.Count > 0)
                {
                    XI.Board.MonDises.AddRange(mons);
                    XI.RaiseGMessage("G0ON,0,M," + mons.Count + "," + string.Join(",", mons));
                }

                // int point = 0;
                // ushort[] pets = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                // string g0hi = "";
                // foreach (ushort pet in pets)
                // {
                //     ushort who = 0;
                //     foreach (Player py in XI.Board.Garden.Values)
                //     {
                //         if (py.Pets.Contains(pet))
                //         {
                //             who = py.Uid;
                //             break;
                //         }
                //     }
                //     if (who != 0)
                //     {
                //         g0hi += "," + who + "," + pet;
                //         point += (XI.LibTuple.ML.Decode(pet).STR + 1);
                //     }
                // }
                // if (g0hi.Length > 0)
                //     XI.RaiseGMessage("G0HI" + g0hi);
                // if (point > 0)
                //     XI.RaiseGMessage("G0IP," + player.Team + "," + point);
                player.ROMUshort = 1;
            }
            else if (type == 1)
            {
                // VI.Cout(0, "TR徐南松基础战力数值变为0.");
                // if (player.STRh > 0)
                //     XI.RaiseGMessage("G0OA," + player.Uid + ",0," + player.STRh);
                XI.RaiseGMessage("G0OE,0," + player.Uid);
                player.ROMUshort = 2;
            }
        }
        // public string JNH1803Input(Player player, int type, string fuse, string prev)
        // {
        //     if (type == 0 && prev == "")
        //     {
        //         List<ushort> equips = new List<ushort>();
        //         foreach (Player py in XI.Board.Garden.Values)
        //             if (py.Team == player.Team && py.IsAlive)
        //                 equips.AddRange(py.Pets.Where(p => p != 0));
        //         foreach (string ce in XI.Board.CsPets)
        //         {
        //             int idx = ce.IndexOf(',');
        //             ushort pet = ushort.Parse(ce.Substring(idx + 1));
        //             equips.Remove(pet);
        //         }
        //         if (equips.Count > 1)
        //             return "#爆发,/M1~" + equips.Count + "(p" + string.Join("p", equips) + ")";
        //         else
        //             return "#爆发,/M1(p" + string.Join("p", equips) + ")";
        //     }
        //     else return "";
        // }
        #endregion HL018 - Xu'Nansong
        #region HL019 - Kongxiu
        public bool JNH1901Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNH1901")
                            return true;
                }
                return false;
            }
            else if (type == 1 || type == 2)
                return player.TokenCount == 0;
            else
                return false;
        }
        public void JNH1901Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IJ," + player.Uid + ",0,5");
            else if (type == 1 || type == 2)
                XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        public bool JNH1902Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return player.RAMUshort == 0 && player.TokenCount >= 2 &&
                    XI.Board.Garden.Values.Where(p => p.IsTared && p.Gender == 'M').Any();
            }
            else if (type == 1)
            {
                if (player.RAMUshort != 0)
                {
                    Player py = XI.Board.Garden[player.RAMUshort];
                    return player.Tux.Count > 0 && py.IsAlive && py.TuxLimit > 0;
                }
                else return false;
            }
            else
                return false;
        }
        public void JNH1902Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                int txCount = XI.Board.Garden[ut].Tux.Count;
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0,2");
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + ut + ",2," + txCount);
                player.RAMUshort = ut;
            }
            else if (type == 1)
            {
                ushort ut = player.RAMUshort;
                int txLmt = System.Math.Min(player.Tux.Count, XI.Board.Garden[ut].TuxLimit);
                string given = XI.AsyncInput(player.Uid, "#交还的,Q" + txLmt + "(p" +
                    string.Join("p", player.Tux) + ")", "JNH1902", "1");
                if (given != VI.CinSentinel)
                    XI.RaiseGMessage("G0HQ,0," + ut + "," + player.Uid + ",1," + txLmt + "," + given);
            }
        }
        public string JNH1902Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "#获得全部手牌的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsTared && p.Gender == 'M').Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNH1903Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                if (XI.Board.IsAttendWar(player) && player.TokenCount > 0)
                {
                    if (XI.Board.Rounder.Team == player.Team)
                        return XI.Board.Hinder.IsTared && XI.Board.Hinder.Gender == 'M';
                    else
                        return (XI.Board.Supporter.IsTared && XI.Board.Supporter.Gender == 'M') ||
                            (XI.Board.Rounder.IsTared && XI.Board.Rounder.Gender == 'M');
                }
                else return false;
            }
            else if (type == 1 || type == 2)
            {
                if (player.SingleTokenTar != 0)
                {
                    // G0IA,A,0,n/3
                    string[] blocks = fuse.Split(',');
                    ushort incrType = ushort.Parse(blocks[2]);
                    ushort who = ushort.Parse(blocks[1]);
                    ushort tar = player.SingleTokenTar;
                    if (incrType != 3 && (who == player.Uid || who == tar))
                        return player.STR != XI.Board.Garden[tar].STR;
                }
                return false;
            }
            else if (type == 3)
            {
                int idxc = fuse.IndexOf(',');
                ushort ut = ushort.Parse(fuse.Substring(idxc + 1));
                return ut == player.SingleTokenTar;
            }
            else return false;
        }
        public void JNH1903Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort tar = ushort.Parse(argst);
                Player py = XI.Board.Garden[tar];
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0,1");
                if (py.STR > player.STR)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + (py.STR - player.STR));
                else if (py.STR < player.STR)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",1," + (player.STR - py.STR));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + tar);
            }
            else if (type == 1)
            {
                Player py = XI.Board.Garden[player.SingleTokenTar];
                if (py.STR > player.STR)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + (py.STR - player.STR));
                else if (py.STR < player.STR)
                    XI.RaiseGMessage("G0IA," + py.Uid + ",1," + (player.STR - py.STR));
            }
            else if (type == 2)
            {
                Player py = XI.Board.Garden[player.SingleTokenTar];
                if (py.STR > player.STR)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",1," + (py.STR - player.STR));
                else if (py.STR < player.STR)
                    XI.RaiseGMessage("G0OA," + py.Uid + ",1," + (player.STR - py.STR));
            }
            else if (type == 3)
            {
                if (player.SingleTokenTar != 0)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
            }
        }
        public string JNH1903Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                if (XI.Board.Rounder.Team == player.Team)
                    return "/T1(p" + XI.Board.Hinder.Uid + ")";
                else
                {
                    List<ushort> invs = new List<ushort>();
                    if (XI.Board.Rounder.IsTared && XI.Board.Rounder.Gender == 'M')
                        invs.Add(XI.Board.Rounder.Uid);
                    if (XI.Board.Supporter.IsTared && XI.Board.Supporter.Gender == 'M')
                        invs.Add(XI.Board.Supporter.Uid);
                    return "/T1(p" + string.Join("p", invs) + ")";
                }
            }
            else return "";
        }
        #endregion HL019 - Kongxiu
        #region HL020 - Mojianke
        public bool JNH2001Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                if (blocks[1] == "0" || blocks[1] == "1")
                    return true;
            }
            else if (type == 2)
                return true;
            return false;
        }
        public void JNH2001Action(Player player, int type, string fuse, string argst)
        {
            bool self = true; int n = 0;
            if (type == 0)
            {
                string[] g0zbs = fuse.Split(',');
                ushort who = ushort.Parse(g0zbs[1]);
                self = XI.Board.Garden[who].Team == player.Team;
                n = 1;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                ushort who = ushort.Parse(blocks[2]);
                self = XI.Board.Garden[who].Team == player.Team;
                n = 1;
            }
            else if (type == 2)
            {
                string[] g0ifs = fuse.Split(',');
                ushort who = ushort.Parse(g0ifs[1]);
                self = XI.Board.Garden[who].Team == player.Team;
                n = g0ifs.Length - 2;
            }
            for (int i = 0; i < n; ++i)
            {
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                if (self)
                {
                    if (player.Tux.Count > 0)
                        XI.RaiseGMessage("G0DH," + player.Uid + ",1,1");
                }
                else
                {
                    string select = XI.AsyncInput(player.Uid, "#置为「魔灵」,Q1(p" + string.Join("p", player.Tux) + ")", "JNH1903Action", "0");
                    if (select != VI.CinSentinel)
                    {
                        ushort ut = ushort.Parse(select);
                        XI.RaiseGMessage("G0OT," + player.Uid + "," + 1 + "," + ut);
                        XI.RaiseGMessage("G0IJ," + player.Uid + ",4,1," + ut);
                    }
                }
            }
        }
        public bool JNH2002Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.TokenFold.Count >= 1)
                return player.RestZP <= 0 && (player.Team == XI.Board.Rounder.Team ? XI.Board.CalculateRPool() : XI.Board.CalculateOPool()) >= 2;
            else if (type == 1 && player.TokenFold.Count >= 2)
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.GetEquipCount() > 0);
            else if (type == 2 && player.TokenFold.Count >= 3)
                return XI.Board.Rounder.Team == player.Team && XI.Board.MonDises.Count > 0;
            else if (type == 3 && player.TokenFold.Count >= 4)
                return true;
            else if (type == 4 && player.TokenFold.Count >= 5)
                return XI.Board.Rounder.GetEquipCount() > 0;
            else
                return false;
        }
        public void JNH2002Action(Player player, int type, string fuse, string argst)
        {
            string[] args = argst.Split(',');
            if (type == 0)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,1," + argst);
                XI.RaiseGMessage("G0ON," + player.Uid + ",C,1," + argst);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",C" + argst);
                //XI.RaiseGMessage("G1XR,1,1,1," + player.Uid);
                XI.RaiseGMessage("G0OP," + player.Team + ",2");
                ++player.RestZP;
            }
            else if (type == 1)
            {
                ushort who = ushort.Parse(args[0]);
                ushort card = ushort.Parse(args[1]);
                TargetPlayer(player.Uid, who);
                string[] tokens = Util.TakeRange(args, 2, args.Length);
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,2," + string.Join(",", tokens));
                XI.RaiseGMessage("G0ON," + player.Uid + ",C,2," + string.Join(",", tokens));
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", tokens.Select(p => "C" + p)));
                XI.RaiseGMessage("G0HQ,0," + who + "," + who + ",0,1," + card);
            }
            else if (type == 2)
            {
                string[] tokens = Util.TakeRange(args, 1, args.Length);
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,3," + string.Join(",", tokens));
                XI.RaiseGMessage("G0ON," + player.Uid + ",C,3," + string.Join(",", tokens));
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", tokens.Select(p => "C" + p)));
                ushort pop = ushort.Parse(args[0]);
                XI.Board.MonDises.Remove(pop);
                XI.RaiseGMessage("G2CN,1,1");
                XI.Board.MonPiles.PushBack(pop);
                XI.RaiseGMessage("G0PB,1," + player.Uid + ",1," + pop);
                XI.RaiseGMessage("G0YM,5," + pop);
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,4," + argst);
                XI.RaiseGMessage("G0ON," + player.Uid + ",C,4," + argst);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", args.Select(p => "C" + p)));
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
            }
            else if (type == 4)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,5," + argst);
                XI.RaiseGMessage("G0ON," + player.Uid + ",C,5," + argst);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", args.Select(p => "C" + p)));
                Player rd = XI.Board.Rounder;
                XI.RaiseGMessage("G0QZ," + rd.Uid + "," + string.Join(",", rd.ListOutAllEquips()));
            }
        }
        public string JNH2002Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                    return "#弃置魔灵,/C1(p" + string.Join("p", player.TokenFold) + ")";
                else
                    return "";
            }
            else if (type == 1)
            {
                if (prev == "")
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                        p.IsTared && p.GetBaseEquipCount() > 0).Select(p => p.Uid)) + ")";
                else if (prev.IndexOf(',') < 0)
                {
                    ushort who = ushort.Parse(prev);
                    return "/" + (who == player.Uid ? "Q" : "C") + "1(p" + string.Join("p", XI.Board.Garden[who].ListOutAllEquips())
                        + "),#弃置魔灵,/C2(p" + string.Join("p", player.TokenFold) + ")";
                }
                else
                    return "";
            }
            else if (type == 2)
            {
                if (prev == "")
                    return "/M1(p" + XI.Board.MonDises.Last() + "),#弃置魔灵,/C3(p" + string.Join("p", player.TokenFold) + ")";
                else
                    return "";
            }
            else if (type == 3)
            {
                if (prev == "")
                    return "#弃置魔灵,/C4(p" + string.Join("p", player.TokenFold) + ")";
                else
                    return "";
            }
            else if (type == 4)
            {
                if (prev == "")
                    return "#弃置魔灵,/C5(p" + string.Join("p", player.TokenFold) + ")";
                else
                    return "";
            }
            else
                return "";
        }
        #endregion HL020 - Mojianke
        #region HL021 - MurongKe
        public void JNH2101Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        public bool JNH2102Valid(Player player, int type, string fuse)
        {
            bool meWin = (player.Team == XI.Board.Rounder.Team && XI.Board.IsBattleWin);
            if (meWin && XI.Board.Hinder.IsTared)
                return XI.Board.PoolDelta > 0;
            else
                return false;
        }
        public void JNH2102Action(Player player, int type, string fuse, string argst)
        {
            TargetPlayer(player.Uid, XI.Board.Hinder.Uid);
            int diff = XI.Board.PoolDelta;
            string oSel = XI.AsyncInput(XI.Board.Hinder.Uid, "#请进行『阳核』选择##HP-" +
                diff + "##对方HP+1,Y2", "JNH2102", "0");
            if (oSel != "2")
                Harm(player, XI.Board.Hinder, diff);
            else
            {
                List<Player> cands = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.HP < p.HPb && p.Team == player.Team).ToList();
                if (cands.Count > 0)
                {
                    int maxLoss = cands.Max(p => p.HPb - p.HP);
                    Cure(player, cands.Where(p => p.HPb - p.HP == maxLoss), 1);
                }
            }
        }
        public bool JNH2103Valid(Player player, int type, string fuse)
        {
            bool meWin = (player.Team == XI.Board.Rounder.OppTeam && !XI.Board.IsBattleWin);
            if (meWin)
            {
                IDictionary<int, int> dicts = XI.CalculatePetsScore();
                int diff1 = XI.Board.PoolDelta;

                ushort[] props = new ushort[] { 0, 1, 2, 3, 4 };
                int opr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Any(r => r.Pets[q] != 0));
                int rpr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.Team).Any(r => r.Pets[q] != 0));
                return diff1 >= 2 && opr > rpr;
            }
            else
                return false;
        }
        public void JNH2103Action(Player player, int type, string fuse, string argst)
        {
            IDictionary<int, int> dicts = XI.CalculatePetsScore();
            int diff1 = XI.Board.PoolDelta;
            ushort[] props = new ushort[] { 0, 1, 2, 3, 4 };
            int opr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Any(r => r.Pets[q] != 0));
            int rpr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.Team).Any(r => r.Pets[q] != 0));
            int diff = System.Math.Min(diff1, opr - rpr);
            string rSel = XI.AsyncInput(player.Uid, "#补" + diff + "张牌,T1" +
                ATeammatesTared(player), "JNH2103", "0");
            ushort rTar = ushort.Parse(rSel);
            XI.RaiseGMessage("G0DH," + rTar + ",0," + diff);
        }
        #endregion HL021 - MurongKe

        #region EX301 - JingTian
        public bool JNE0201Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
                return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
            else
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Tux.Count > 0);
        }
        public void JNE0201Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            TargetPlayer(player.Uid, ut);
            Player py = XI.Board.Garden[ut];
            IDictionary<ushort, string> reservation = new Dictionary<ushort, string>();
            if (player.Tux.Count > 1)
                reservation[player.Uid] = "#保留的,Q1(p" + string.Join("p", player.Tux) + ")";
            if (py.Tux.Count > 1)
                reservation[ut] = "#保留的,Q1(p" + string.Join("p", py.Tux) + ")";
            IDictionary<ushort, string> reAns = XI.MultiAsyncInput(reservation);
            List<ushort> ot1 = reAns.ContainsKey(player.Uid) ? player.Tux.Except(reAns[player.Uid]
                .Split(',').Select(p => ushort.Parse(p))).ToList() : new List<ushort>();
            List<ushort> ot2 = reAns.ContainsKey(ut) ? py.Tux.Except(reAns[ut]
                .Split(',').Select(p => ushort.Parse(p))).ToList() : new List<ushort>();
            string g0ot = "";
            if (ot1.Count > 0)
                g0ot += "," + player.Uid + "," + ot1.Count + "," + string.Join(",", ot1);
            if (ot2.Count > 0)
                g0ot += "," + py.Uid + "," + ot2.Count + "," + string.Join(",", ot2);
            if (g0ot.Length > 0)
            {
                XI.RaiseGMessage("G0OT" + g0ot);
                ot1.AddRange(ot2);
                XI.RaiseGMessage("G1IU," + string.Join(",", ot1));

                ushort[] uds = { player.Uid, ut };
                int idxs = 0;
                do
                {
                    XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0,C," + string.Join(",", ot1));
                    string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                    string input = XI.AsyncInput(uds[idxs], "Z1" + pubTux, "JNT1901", "0");
                    if (!input.StartsWith("/"))
                    {
                        ushort cd = ushort.Parse(input);
                        if (XI.Board.PZone.Contains(cd))
                        {
                            XI.RaiseGMessage("G1OU," + cd);
                            XI.RaiseGMessage("G2QU,0,0" + cd);
                            XI.RaiseGMessage("G0HQ,2," + uds[idxs] + ",0,0," + cd);
                            ot1.Remove(cd);
                            idxs = (idxs + 1) % 2;
                        }
                    }
                    XI.RaiseGMessage("G2FU,3");
                } while (ot1.Count > 0);
            }
        }
        public string JNE0201Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                if (player.Tux.Count > 0)
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                         p.Uid != player.Uid && p.IsTared).Select(p => p.Uid)) + ")";
                else
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                         p.Uid != player.Uid && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        public bool JNE0202Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNE0202", player, fuse);
            else if (type == 1)
            { // G0ON
                if (!player.ROM.ContainsKey("Weapon"))
                    return false;
                string[] g0on = fuse.Split(',');
                for (int idx = 1; idx < g0on.Length;)
                {
                    ushort who = ushort.Parse(g0on[idx]);
                    string cm = g0on[idx + 1];
                    int n = int.Parse(g0on[idx + 2]);
                    if (who != player.Uid && cm == "C")
                    {
                        List<ushort> tuxes = Util.TakeRange(g0on, idx + 3, idx + 3 + n)
                            .Select(p => ushort.Parse(p)).Where(p => XI.Board.TuxDises.Contains(p)).ToList();
                        foreach (ushort ut in tuxes)
                        {
                            Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                            if (tux != null && tux.DBSerial == (ushort)player.ROM["Weapon"])
                                return true;
                        }
                    }
                    idx += (3 + n);
                }
                return false;
            }
            else
                return false;
        }
        public void JNE0202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                List<ushort> wqs = XI.LibTuple.TL.Firsts.Where(p =>
                    p.Type == Tux.TuxType.WQ).Select(p => p.DBSerial).ToList();
                string input = XI.AsyncInput(player.Uid, "#始终获得的,G1(p"
                     + string.Join("p", wqs) + ")", "JNT1902", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    ushort dbSerial = ushort.Parse(input);
                    player.ROM["Weapon"] = dbSerial;
                    XI.RaiseGMessage("G2FU,4," + player.Uid + "," + dbSerial);
                }
            }
            else if (type == 1)
            {
                string[] g0on = fuse.Split(',');
                string ng0on = "";
                for (int idx = 1; idx < g0on.Length;)
                {
                    ushort who = ushort.Parse(g0on[idx]);
                    string cm = g0on[idx + 1];
                    int n = int.Parse(g0on[idx + 2]);
                    if (who != player.Uid && cm == "C")
                    {
                        List<ushort> tuxes = Util.TakeRange(g0on, idx + 3, idx + 3 + n)
                            .Select(p => ushort.Parse(p)).Where(p => XI.Board.TuxDises.Contains(p)).ToList();
                        foreach (ushort ut in tuxes)
                        {
                            Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                            if (tux != null && tux.DBSerial == (ushort)player.ROM["Weapon"])
                            {
                                XI.RaiseGMessage("G2CN,0,1");
                                XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
                                XI.Board.TuxDises.Remove(ut);
                                tuxes.Remove(ut); break;
                            }
                        }
                        if (tuxes.Count > 0)
                            ng0on += "," + who + "," + cm + "," + tuxes.Count + "," + string.Join(",", tuxes);
                    }
                    else
                        ng0on += "," + string.Join(",", Util.TakeRange(g0on, idx, idx + 3 + n));
                    idx += (3 + n);
                }
                if (ng0on.Length > 0)
                    XI.InnerGMessage("G0ON" + ng0on, 141);
            }
        }
        public bool JNE0203Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Team == XI.Board.Rounder.Team && XI.Board.IsBattleWin
                     && XI.Board.PoolDelta > 0 && player.Tux.Count >= 2;
            else if (type == 1)
                return player.TokenCount > 0;
            else
                return false;
        }
        public void JNE0203Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
                if (player.TokenCount < 7)
                {
                    int delta = System.Math.Min(XI.Board.PoolDelta, 7 - player.TokenCount);
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",0," + delta);
                }
            }
            else if (type == 1)
            {
                int z = player.TokenCount;
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + z);
                XI.RaiseGMessage("G0IP," + player.Team + "," + z);
            }
        }
        public string JNE0203Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "#弃置的,/Q2(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        #endregion EX301 - JingTian
        #region EX408 - Chanyou
        public bool JNE0301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNE0301")
                            return true;
                }
                return false;
            }
            else if (type == 1)
                return true;
            else
                return false;
        }
        public void JNE0301Action(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                ushort[] uts = XI.DequeueOfPile(XI.Board.TuxPiles, 2);
                XI.RaiseGMessage("G2IN,0,2");
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,2,C" + uts[0] + ",C" + uts[1]);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,C" + uts[0] + ",C" + uts[1]);
            }
            else if (type == 1)
            {
                ushort ut = XI.DequeueOfPile(XI.Board.TuxPiles);
                XI.RaiseGMessage("G2IN,0,1");
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,C" + ut);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,C" + ut);
            }
        }
        public bool JNE0302Valid(Player player, int type, string fuse)
        {
            return player.TokenExcl.Count > 0;
        }
        public void JNE0302Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            bool incr = argst.Substring(0, idx) == "1";
            List<string> uts = argst.Substring(idx + 1).Split(',').Select(p => "C" + p).ToList();
            XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + uts.Count + "," + string.Join(",", uts));
            XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", uts));
            if (incr)
                XI.RaiseGMessage("G0IW," + XI.Board.Monster1 + "," + uts.Count);
            else
                XI.RaiseGMessage("G0OW," + XI.Board.Monster1 + "," + uts.Count);
        }
        public string JNE0302Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#请选择调整怪物闪避方式.##闪避增加##闪避减少,/Y2";
            else if (prev.IndexOf(',') < 0)
            {
                string head = "/I1" + (player.TokenExcl.Count > 1 ? ("~" + player.TokenExcl.Count) : "");
                return head + "(p" + string.Join("p", player.TokenExcl) + ")";
            }
            else
                return "";
        }
        public bool JNE0303Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0);
        }
        public void JNE0303Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            Player py = XI.Board.Garden[tar];
            bool survive = false;
            TargetPlayer(player.Uid, tar);
            if (py.Tux.Count > 0)
            {
                string sel1 = XI.AsyncInput(tar, "#请选择是否立即阵亡.##否##是,Y2", "JNE0303", "0");
                survive = (sel1 != "2");
            }
            if (!survive)
            {
                if (player.IsAlive)
                    XI.RaiseGMessage("G0LH,0," + player.Uid + "," + (player.HPb - 2));
                XI.RaiseGMessage("G0ZW," + tar);
            }
            else
            {
                List<ushort> vals = py.Tux.Except(XI.Board.ProtectedTux).ToList();
                string c0 = Util.RepeatString("p0", vals.Count);
                XI.AsyncInput(player.Uid, "#作为「幻」的,C1(" + c0 + ")", "JNE0303", "1");
                vals.Shuffle();
                XI.RaiseGMessage("G0OT," + tar + ",1," + vals[0]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,C" + vals[0]);

                XI.RaiseGMessage("G0LH,0," + py.Uid + "," + (py.HPb - 2));
                if (py.IsAlive)
                {
                    int maskSol = Artiad.IntHelper.SetMask(0, GiftMask.INCOUNTABLE, true);
                    if (py.HP < player.HPb)
                        Cure(player, py, (player.HPb - py.HP), FiveElement.SOL);
                    else if (py.HP > player.HPb)
                        Harm(player, py, (py.HP - player.HPb), FiveElement.SOL, maskSol);
                }
                if (py.IsAlive)
                    XI.RaiseGMessage("G0DS," + tar + ",0,1");
            }
            if (XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", -30);
        }
        public string JNE0303Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).ToList();
                return "/T1(p" + string.Join("p", invs.Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        #endregion EX408 - Chanyou
        #region EX506 - Kumu
        public bool JNE0401Valid(Player player, int type, string fuse)
        {
            // G0HQ,0,A,B,utype,n
            string[] blocks = fuse.Split(',');
            if (blocks[1] == "0")
            {
                ushort to = ushort.Parse(blocks[2]);
                ushort from = ushort.Parse(blocks[3]);
                int n = int.Parse(blocks[5]);
                return from != player.Uid && to != player.Uid && XI.Board.Garden[from].Tux.Count > 0 && n > 0;
            }
            return false;
        }
        public void JNE0401Action(Player player, int type, string fuse, string argst)
        {
            ushort who = ushort.Parse(argst.Substring(0, argst.IndexOf(',')));
            XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + who + ",2,1");
        }
        public string JNE0401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                string[] g0hq = fuse.Split(',');
                ushort from = ushort.Parse(g0hq[3]);
                return "/T1(p" + from + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort from = ushort.Parse(prev);
                string c0 = Util.RepeatString("p0", XI.Board.Garden[from].Tux.Count);
                return "#获得的,C1(" + c0 + ")";
            }
            else
                return "";
        }
        public bool JNE0402Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] g0hg = fuse.Split(',');
                for (int i = 1; i < g0hg.Length; i += 2)
                {
                    int n = int.Parse(g0hg[i + 1]);
                    if (n > 0)
                        return true;
                }
                return false;
            }
            else if (type == 1)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (player.TokenTars.Contains(ut) && n > 0)
                        return true;
                }
                return false;
            }
            else if (type == 2)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (player.TokenTars.Contains(ut) && XI.Board.Garden[ut].Tux.Count > 0 && n > 0)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void JNE0402Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] g0hg = fuse.Split(',');
                ISet<ushort> sets = new HashSet<ushort>();
                for (int i = 1; i < g0hg.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0hg[i]);
                    sets.Add(ut);
                }
                TargetPlayer(player.Uid, sets);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2," +
                    sets.Count + "," + string.Join(",", sets));
            }
            else if (type == 1)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    Player py = XI.Board.Garden[ut];
                    if (player.TokenTars.Contains(ut) && n > 0)
                        g0ht[i + 1] = (n + 1).ToString();
                }
                XI.InnerGMessage(string.Join(",", g0ht), 56);
            }
            else if (type == 2)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    Player py = XI.Board.Garden[ut];
                    if (player.TokenTars.Contains(ut) && n > 0 && py.Tux.Count > 0 && ut != player.Uid)
                    {
                        string c0 = Util.RepeatString("p0", py.Tux.Count);
                        XI.AsyncInput(player.Uid, "#获取的,C1(" + c0 + ")", "JNE0402", "1");
                        XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + ut + ",2,1");
                    }
                }
            }
        }
        public bool JNE0403Valid(Player player, int type, string fuse)
        {
            ushort who = ushort.Parse(fuse.Substring(fuse.IndexOf(',') + 1));
            return XI.Board.Garden[who].Team == player.OppTeam && player.Tux.Count > 0;
        }
        public void JNE0403Action(Player player, int type, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
            int dv = XI.Board.DiceValue;
            XI.RaiseGMessage("G0T7," + player.Uid + "," + dv + "," + (7 - dv));
        }
        public string JNE0403Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        #endregion EX506 - Kumu
    }
}