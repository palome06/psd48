using PSD.Base;
using PSD.Base.Card;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.JNS
{
    public partial class SkillCottage
    {
        #region TR001 - Suyu
        public bool JNT0101Valid(Player player, int type, string fuse)
        {
            Player r = XI.Board.Rounder;
            if (r.Uid != player.Uid)
            {
                if (r.Team == player.Team && r.Gender == 'M')
                    return true;
                if (r.Team == player.OppTeam && r.Gender == 'F')
                    return true;
            }
            return false;
        }
        public void JNT0101Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "TR夙玉发动「望舒剑」.");
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
            XI.RaiseGMessage("G0IX," + player.Uid + ",1,1");
        }
        public bool JNT0102Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py.Team == player.Team && (harm.Element == FiveElement.AQUA ||
                        harm.Element == FiveElement.AGNI))
                    return true;
            }
            return false;
        }
        public void JNT0102Action(Player player, int type, string fuse, string argst)
        {
            // G0OH,A,Src,p,n,...
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py.Team == player.Team && (harm.Element == FiveElement.AQUA ||
                        harm.Element == FiveElement.AGNI))
                {
                    if (--harm.N <= 0)
                        rvs.Add(harm);
                }
            }
            harms.RemoveAll(p => rvs.Contains(p));
            VI.Cout(0, "TR夙玉发动「灵光藻玉」.");
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -19);
        }
        #endregion TR001 - Suyu
        #region TR002 - XuChangqing
        public bool JNT0201Valid(Player player, int type, string fuse)
        {
            // G0HD,0,A,B,x
            string[] blocks = fuse.Split(',');
            if (player.ROMUshort == 1) // Not valid in JNT0202
                return false;
            if (blocks[1] == "0")
            {
                ushort who = ushort.Parse(blocks[2]);
                if (XI.Board.Garden[who].Team == player.OppTeam)
                    return true;
            }
            else if (blocks[1] == "1")
            {
                ushort who = ushort.Parse(blocks[2]);
                ushort where = ushort.Parse(blocks[3]);
                if (XI.Board.Garden[who].Team != player.OppTeam)
                    return false;
                else if (where != 0 && XI.Board.Garden[who].Team == XI.Board.Garden[where].Team)
                    return false;
                else
                    return true;
            }
            return false;
        }
        public void JNT0201Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#获得2张补牌,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Team == player.Team).Select(p => p.Uid)) + ")", "JNT0201", "0");
            ushort who = ushort.Parse(input);
            VI.Cout(0, "TR徐长卿发动「侠义」，指定我方补牌2张.");
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,2");
            //XI.InnerGMessage(fuse, 136);
        }
        public bool JNT0202Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                bool self = XI.Board.Rounder.Uid == player.Uid;
                bool isWin = XI.Board.IsBattleWin;
                Base.Card.Monster m1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                Base.Card.Monster m2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                return self && !isWin && ((m2 != null && m2.Level != Base.Card.Monster.ClLevel.BOSS) ||
                    (m1 != null && XI.Board.Mon1From == 0 && m1.Level != Base.Card.Monster.ClLevel.BOSS));
            }
            else if (type == 1)
            {
                string[] args = fuse.Split(',');
                return args[1] == player.Uid.ToString();
            }
            else
                return false;
        }
        public void JNT0202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                //string res = "G1JG," + player.Uid;
                Base.Card.Monster m1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                Base.Card.Monster m2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (m1 != null && m1.Level != Base.Card.Monster.ClLevel.BOSS)
                    XI.RaiseGMessage("G1JG," + player.Uid + "," + XI.Board.Mon1From + "," + XI.Board.Monster1);
                if (m2 != null && m2.Level != Base.Card.Monster.ClLevel.BOSS)
                    XI.RaiseGMessage("G1JG," + player.Uid + ",0," + XI.Board.Monster2);
            }
            else if (type == 1)
            {
                string[] fuseargs = fuse.Split(',');
                ushort monFrom = ushort.Parse(fuseargs[2]);
                List<ushort> monGet = Util.TakeRange(fuseargs, 3, fuseargs.Length).Select(p => ushort.Parse(p)).ToList();

                if (argst != "")
                {
                    player.ROMUshort = 1;
                    int idx = argst.IndexOf(',');
                    ushort from = ushort.Parse(argst.Substring(0, idx));
                    ushort pet = ushort.Parse(argst.Substring(idx + 1));

                    string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#获得宠物,T1(p" + string.Join(
                        "p", XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam)
                        .Select(p => p.Uid)) + ")", "JNT0202", "0");
                    ushort to = ushort.Parse(input);
                    XI.RaiseGMessage("G0HC,1," + to + "," + from + ",1," + pet);
                    player.ROMUshort = 0;
                }
                XI.RaiseGMessage("G0HC,1," + player.Uid + "," + monFrom + ",1," + string.Join(",", monGet));
            }
        }
        public string JNT0202Input(Player player, int type, string fuse, string prev)
        {
            if (type == 1)
            {
                if (prev == "")
                {
                    List<ushort> pys = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                        p.Team == player.Team && p.Pets.Where(q => q != 0).Any()).Select(p => p.Uid).ToList();
                    if (pys.Count == 0)
                        return "";
                    return "#交出宠物,/T1(p" + string.Join("p", pys) + ")";
                }
                else if (prev.IndexOf(',') < 0)
                {
                    ushort who = ushort.Parse(prev);
                    List<ushort> pts = XI.Board.Garden[who].Pets.Where(p => p != 0).ToList();
                    return "#交给对方,/M1(p" + string.Join("p", pts) + ")";
                }
                else
                    return "";
            }
            else return "";
        }
        #endregion TR002 - XuChangqing
        #region TR003 - YunTianqing
        public bool JNT0301Valid(Player player, int type, string fuse)
        {
            Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            return XI.Board.IsAttendWar(player) && mon != null && (mon.Element !=
                Base.Card.FiveElement.AQUA && mon.Element != Base.Card.FiveElement.AGNI);
        }
        public void JNT0301Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "TR云天青发动「仁义剑仙」.");
            XI.RaiseGMessage("G0IX," + player.Uid + ",1,3");
        }
        public bool JNT0302Valid(Player player, int type, string fuse)
        {
            return player.Pets.Where(p => p != 0).Any() && XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Uid != player.Uid && p.Team == player.Team).Any();
        }
        public void JNT0302Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "TR云天青发动「转轮镜台」将宠物交给队友.");
            string[] args = argst.Split(',');
            ushort pt = ushort.Parse(args[0]);
            ushort to = ushort.Parse(args[1]);
            XI.RaiseGMessage("G0HC,1," + to + "," + player.Uid + ",1," + pt);
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
        }
        public string JNT0302Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/M1(p" + string.Join("p", player.Pets.Where(p => p != 0)) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#交给宠物,/T1(p" + string.Join("p", XI.Board.Garden.Values
                    .Where(p => p.IsTared && p.Uid != player.Uid && p.Team == player.Team)
                    .Select(q => q.Uid)) + ")";
            else
                return "";
        }
        #endregion TR003 - YunTianqing
        #region TR004 - Lingyin
        public bool JNT0401Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.TokenExcl.Count > 0 &&
                XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid))
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid)
                    {
                        int prop = Artiad.IntHelper.Elem2Int(harm.Element);
                        foreach (FiveElement five in Artiad.Harm.GetPropedElement())
                            if (harm.Element == five && player.TokenExcl.Contains("I" + prop))
                                return true;
                        if (!Artiad.Harm.GetPropedElement().Contains(harm.Element))
                        {
                            if (harm.Element != FiveElement.LOVE &&
                                    harm.Element != FiveElement.SOL && player.TokenExcl.Contains("I4"))
                                return true;
                        }
                    }
                }
                return false;
            }
            else if (type == 1)
                return IsMathISOS("JNT0401", player, fuse);
            else
                return false;
        }
        public void JNT0401Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                int card = int.Parse(argst.Substring(0, idx));
                ushort to = ushort.Parse(argst.Substring(idx + 1));

                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                Artiad.Harm rotation = null;
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid)
                    {
                        int prop = Artiad.IntHelper.Elem2Int(harm.Element);
                        foreach (FiveElement five in Artiad.Harm.GetPropedElement())
                        {
                            if (harm.Element == five && card == prop)
                            {
                                rotation = harm;
                                rotation.N = harm.N - 1;
                            }
                        }
                        //if (harm.Element == FiveElement.SOL)
                        //{
                        //    rotation = harm;
                        //    rotation.N = player.HP - harm.N;
                        //}
                        if (!Artiad.Harm.GetPropedElement().Contains(harm.Element) &&
                            harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL && card == 4)
                        {
                            rotation = harm;
                            rotation.N = harm.N - 1;
                        }
                    }
                }
                if (rotation != null)
                    harms.Remove(rotation);
                if (rotation != null && rotation.N > 0)
                {
                    foreach (Artiad.Harm harm in harms)
                    {
                        if (harm.Who == to && harm.Element == rotation.Element)
                        {
                            //if (rotation.Element == FiveElement.SOL)
                            //{
                            //    int nlast = (XI.Board.Garden[to].HP - harm.N);
                            //    int last = nlast < rotation.N ? nlast : rotation.N;
                            //    int v = XI.Board.Garden[to].HP - last;
                            //    if (v > 0)
                            //        harm.N = v;
                            //    else
                            //        rvs.Add(harm);
                            //}
                            //else
                            //    harm.N += rotation.N;
                            harm.N += rotation.N;
                            rotation = null;
                            break;
                        }
                    }
                    harms.RemoveAll(p => rvs.Contains(p));
                    if (rotation != null)
                    {
                        rotation.Who = to;
                        //if (rotation.Element == FiveElement.SOL)
                        //{
                        //    rotation.N = XI.Board.Garden[to].HP - rotation.N;
                        //    if (rotation.N > 0)
                        //        harms.Add(rotation);
                        //}
                        //else
                        //    harms.Add(rotation);
                        harms.Add(rotation);
                    }
                }
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + card);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",I" + card);
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -99);
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,5,I1,I2,I3,I4,I5");
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,I1,I2,I3,I4,I5");
            }
        }
        public string JNT0401Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                ISet<int> cands = new HashSet<int>();
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid)
                    {
                        int prop = Artiad.IntHelper.Elem2Int(harm.Element);
                        foreach (FiveElement five in Artiad.Harm.GetPropedElement())
                            if (harm.Element == five && player.TokenExcl.Contains("I" + prop))
                                cands.Add(prop);
                        if (!Artiad.Harm.GetPropedElement().Contains(harm.Element))
                        {
                            if (harm.Element != FiveElement.LOVE &&
                                    harm.Element != FiveElement.SOL && player.TokenExcl.Contains("I4"))
                                cands.Add(4);
                        }
                    }
                }
                return "/I1(p" + string.Join("p", cands.Select(p => "I" + p)) + "),#承受伤害的,/T1(p" +
                    string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        public bool JNT0402Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            bool has1 = mon1 != null && player.TokenExcl.Contains("I" + (Util.GetFiveElementId(mon1.Element) + 1));
            Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            bool has2 = mon2 != null && player.TokenExcl.Contains("I" + (Util.GetFiveElementId(mon2.Element) + 1));
            return XI.Board.IsAttendWar(player) && meLose && (has1 || has2);
        }
        public void JNT0402Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            int card = int.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));

            XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + card);
            XI.RaiseGMessage("G2TZ,0," + player.Uid + ",I" + card);
            XI.RaiseGMessage("G0DH," + to + ",0,1");
            if (XI.Board.Garden[to].Tux.Count >= 2)
                XI.RaiseGMessage("G0DH," + to + ",1,1");
        }
        public string JNT0402Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<int> sets = new List<int>();
                Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                if (mon1 != null && player.TokenExcl.Contains("I" + (Util.GetFiveElementId(mon1.Element) + 1)))
                    sets.Add(Util.GetFiveElementId(mon1.Element) + 1);
                Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2 != null && player.TokenExcl.Contains("I" + (Util.GetFiveElementId(mon2.Element) + 1)))
                    sets.Add(Util.GetFiveElementId(mon2.Element) + 1);
                return "/I1(p" + string.Join("p", sets.Select(p => "I" + p)) + "),#获得补牌,/T1(p" +
                    string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        public void JNT0403Action(Player player, int type, string fuse, string argst)
        {
            string[] g0oj = fuse.Split(',');
            int count = int.Parse(g0oj[3]);
            XI.RaiseGMessage("G0IA," + player.Uid + ",0," + count);
        }
        public bool JNT0403Valid(Player player, int type, string fuse)
        {
            string[] g0oj = fuse.Split(',');
            return (g0oj[1] == player.Uid.ToString() && g0oj[2] == "1");
        }
        #endregion TR004 - Lingyin
        #region TR005 - Lingbo
        public void JNT0501Action(Player player, int type, string fuse, string argst)
        {
            int val = int.Parse(argst);
            if (val > 0 && val <= player.DEX)
            {
                XI.RaiseGMessage("G0OX," + player.Uid + ",1," + val);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + val);
            }
        }
        public bool JNT0501Valid(Player player, int type, string fuse)
        {
            return player.DEX > 0 && XI.Board.IsAttendWar(player) && XI.Board.Rounder.Uid != player.Uid;
        }
        public string JNT0501Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#转化为战力的,/D1" + ((player.DEX == 1) ? "" : ("~" + player.DEX));
            else return "";
        }
        public void JNT0502Action(Player player, int type, string fuse, string argst)
        {
            string range = Util.SSelect(XI.Board, p => p != player && p.IsTared);
            string target = XI.AsyncInput(player.Uid, "#无法获得宠物效果,T1" + range, "JNT0502", "0");
            ushort who = ushort.Parse(target);
            XI.RaiseGMessage("G0OE,0," + who);
            VI.Cout(0, "TR凌波对玩家{0}发动了「梦缘」.", XI.DisplayPlayer(who));
            XI.SendOutUAMessage(player.Uid, "JNT0502," + target, "0");
            //XI.InnerGMessage("G0OY,2," + player.Uid, 81);
        }
        public bool JNT0502Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; ++i)
                if (blocks[i] == player.Uid.ToString())
                    return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
            return false;
        }
        #endregion TR005 - Lingbo
        #region TR006 - OuyangQian
        public bool JNT0601Valid(Player player, int type, string fuse)
        {
            bool b1 = player.Tux.Count > 0;
            bool b2 = XI.Board.Hinder.Uid != 0 && XI.Board.Rounder.OppTeam == player.Team;
            bool b3 = XI.Board.Supporter.Uid != 0 && XI.Board.Rounder.Team == player.Team;
            return b1 && (b2 || b3);
        }
        public void JNT0601Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            Base.Card.Tux card = XI.LibTuple.TL.DecodeTux(ut);
            int typeCode = 0;
            switch (card.Type)
            {
                case Base.Card.Tux.TuxType.JP: typeCode = 0x1; break;
                case Base.Card.Tux.TuxType.ZP: typeCode = 0x2; break;
                case Base.Card.Tux.TuxType.TP: typeCode = 0x4; break;
                case Base.Card.Tux.TuxType.FJ:
                case Base.Card.Tux.TuxType.WQ:
                case Base.Card.Tux.TuxType.XB: typeCode = 0x8; break;
            }
            player.RAMInt |= typeCode;
            VI.Cout(0, "TR欧阳倩发动「痴情」.");
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + ut);
            Harm(player, player, 1);
            if (player.IsAlive)
            {
                if (XI.Board.Rounder.Team == player.Team)
                    XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",1,1");
                else if (XI.Board.Rounder.OppTeam == player.Team)
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",1,1");
            }
        }
        public string JNT0601Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<Base.Card.Tux.TuxType> types = new List<Base.Card.Tux.TuxType>();
                if ((player.RAMInt & 0x1) != 0)
                    types.Add(Base.Card.Tux.TuxType.JP);
                else if ((player.RAMInt & 0x2) != 0)
                    types.Add(Base.Card.Tux.TuxType.ZP);
                if ((player.RAMInt & 0x4) != 0)
                    types.Add(Base.Card.Tux.TuxType.TP);
                if ((player.RAMInt & 0x8) != 0)
                {
                    types.Add(Base.Card.Tux.TuxType.FJ);
                    types.Add(Base.Card.Tux.TuxType.WQ);
                    types.Add(Base.Card.Tux.TuxType.XB);
                }

                var v1 = player.Tux.Where(p => !types.Contains(
                    XI.LibTuple.TL.DecodeTux(p).Type)).ToList();
                if (v1.Any())
                    return "/Q1(p" + string.Join("p", v1) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JNT0602Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (XI.Board.Garden[ut].Team == player.Team && n > 0)
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
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.Team && py.Tux.Count > 0)
                        return true;
                }
                return false;
            }
            return false;
        }
        public void JNT0602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.Team && n > 0)
                        g0ht[i + 1] = (n + 1).ToString();
                }
                XI.InnerGMessage(string.Join(",", g0ht), 61);
            }
            else if (type == 1)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.Team && n > 0 && py.Tux.Count > 0)
                    {
                        string backCard = XI.AsyncInput(py.Uid, "#弃置的,Q1(p" +
                             string.Join("p", py.Tux) + ")", "JNT0602", "1");
                        ushort card = ushort.Parse(backCard);
                        XI.RaiseGMessage("G0QZ," + py.Uid + "," + card);
                    }
                }
            }
        }
        #endregion TR006 - OuyangQian
        #region TR007 - LiYiru
        public bool JNT0701BKValid(Player player, int type, string linkFuse, ushort owner)
        {
            if (player.Uid == owner)
                return false;
            int lfidx = linkFuse.IndexOf(':');
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);
            Player opy = XI.Board.Garden[owner];
            if (player.Team != opy.Team)
                return false;

            foreach (string linkHead in linkHeads)
            {
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JNT0701"))
                        continue;
                    int rcmIdx = rlink.IndexOf(',');
                    string rName = Util.Substring(rlink, 0, rcmIdx);
                    int pdIdx = rlink.IndexOf('!', rcmIdx + 1);
                    if (pdIdx < 0) // Not equip special case
                    {
                        int tType = int.Parse(rlink.Substring(rcmIdx + 1));
                        //foreach (ushort ut in player.Tux)
                        //{
                        //    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        //    if (tux != null && tux.Code == rName)
                        //        if (tux.Bribe(opy, tType, fuse) && tux.Valid(opy, tType, fuse))
                        //            return true;
                        //}
                        Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(rName);
                        if (player.Tux.Count > 0 && tux != null)
                            if (tux.Bribe(opy, tType, fuse) && tux.Valid(opy, tType, fuse))
                                return true;
                    }
                    else
                    {
                        int tType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
                        int tConsType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                        foreach (ushort ut in player.ListOutAllEquips())
                        {
                            Base.Card.TuxEqiup tux = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
                            if (tux != null && tux.Code == rName)
                                if (tux.ConsumeValidHolder(player, opy, tConsType, tType, linkFuse))
                                    return true;
                        }
                    }
                }
            }
            return false;
        }
        public string JNT0701Input(Player player, int type, string linkFuse, string prev)
        {
            int lfidx = linkFuse.IndexOf(':');
            // linkHeads = { "TP02,0", "TP03,0" };
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            if (prev == "")
                return "";
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                Player opy = XI.Board.Garden[who];
                ISet<ushort> usefulTux = new HashSet<ushort>();
                foreach (string linkHead in linkHeads)
                {
                    List<string> relateds = XI.Sk03[linkHead].ToList();
                    relateds.Add(linkHead);
                    // relateds = { "TP03,0", "FJ02,0!0" };
                    foreach (string rlink in relateds)
                    {
                        if (rlink.StartsWith("JNT0701"))
                            continue;
                        int rcmIdx = rlink.IndexOf(',');
                        string rName = Util.Substring(rlink, 0, rcmIdx);
                        int pdIdx = rlink.IndexOf('!', rcmIdx + 1);
                        if (pdIdx < 0) // Not equip special case
                        {
                            int tType = int.Parse(rlink.Substring(rcmIdx + 1));
                            foreach (ushort ut in player.Tux)
                            {
                                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                                if (tux != null && tux.Code == rName)
                                    if (tux.Bribe(opy, tType, fuse) && tux.Valid(opy, tType, fuse))
                                        usefulTux.Add(ut);
                            }
                        }
                        else
                        {
                            int tConsType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
                            int tType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                            foreach (ushort ut in player.ListOutAllEquips())
                            {
                                Base.Card.TuxEqiup tux = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
                                if (tux != null && tux.Code == rName)
                                    if (tux.ConsumeValidHolder(player, opy, tConsType, tType, linkFuse))
                                        usefulTux.Add(ut);
                            }
                        }
                    }
                }
                if (usefulTux.Count > 0)
                    return "/Q1(p" + string.Join("p", usefulTux) + ")";
                else
                    return "/";
            }
            else
            {
                int ichicm = prev.IndexOf(',');
                int nicm = prev.IndexOf(',', ichicm + 1);
                ushort who = ushort.Parse(Util.Substring(prev, 0, ichicm));
                ushort ut = ushort.Parse(Util.Substring(prev, ichicm + 1, nicm));
                string rest = nicm < 0 ? "" : prev.Substring(nicm + 1);
                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                Player opy = XI.Board.Garden[who];

                foreach (string linkHead in linkHeads)
                {
                    List<string> relateds = XI.Sk03[linkHead].ToList();
                    relateds.Add(linkHead);
                    // relateds = { "TP03,0", "FJ02,0!0" };
                    foreach (string rlink in relateds)
                    {
                        if (rlink.StartsWith("JNT0701"))
                            continue;
                        int rcmIdx = rlink.IndexOf(',');
                        string rName = Util.Substring(rlink, 0, rcmIdx);
                        int pdIdx = rlink.IndexOf('!', rcmIdx + 1);
                        if (pdIdx < 0) // Not equip special case
                        {
                            int tType = int.Parse(rlink.Substring(rcmIdx + 1));
                            if (tux != null && tux.Code == rName)
                                if (tux.Bribe(opy, tType, fuse) && tux.Valid(opy, tType, fuse))
                                    return tux.InputHolder(player, opy, tType, fuse, rest);
                        }
                        else
                        {
                            int tConsType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
                            int tType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                            Base.Card.TuxEqiup te = tux as Base.Card.TuxEqiup;
                            if (te != null && te.Code == rName)
                                return te.ConsumeInputHolder(player, opy, tConsType, tType, linkFuse, rest);
                        }
                    }
                }
                return "";
            }
        }
        public void JNT0701Action(Player player, int type, string linkFuse, string argst)
        {
            int ichicm = argst.IndexOf(',');
            int nicm = argst.IndexOf(',', ichicm + 1);
            ushort to = ushort.Parse(argst.Substring(0, ichicm));
            ushort ut = ushort.Parse(Util.Substring(argst, ichicm + 1, nicm));
            string crest = nicm < 0 ? "" : ("," + argst.Substring(nicm + 1));

            int lfidx = linkFuse.IndexOf(':');
            // linkHeads = { "TP02,0", "TP03,0" };
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
            foreach (string linkHead in linkHeads)
            {
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JNT0701"))
                        continue;
                    int rcmIdx = rlink.IndexOf(',');
                    string rName = Util.Substring(rlink, 0, rcmIdx);
                    string inTypeStr = Util.Substring(rlink, rcmIdx + 1, -1);
                    if (tux.Code == rName)
                    {
                        if (tux.IsTuxEqiup() && inTypeStr.Contains('!'))
                        {
                            int sancm = inTypeStr.IndexOf('!');
                            ushort consumeCode = ushort.Parse(inTypeStr.Substring(0, sancm));
                            ushort inTypeCode = ushort.Parse(inTypeStr.Substring(sancm + 1));
                            XI.RaiseGMessage("G0ZC," + player.Uid + "," + (3 + consumeCode) + "," +
                                ut + "," + to + crest + ";" + inTypeCode + "," + fuse);
                        }
                        else
                        {
                            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + to + "," + tux.Code
                                    + "," + ut + ";" + inTypeStr + "," + fuse);
                        }
                        return;
                    }
                }
            }
        }
        public bool JNT0702Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return player.TokenExcl.Count > 0 || player.Guardian != 0;
            else if (type == 2)
                return IsMathISOS("JNT0702", player, fuse);
            else
                return false;
        }
        public void JNT0702Action(Player player, int type, string fuse, string argst)
        {
            IDictionary<ushort, ushort> im = new Dictionary<ushort, ushort>();
            IDictionary<ushort, ushort> jm = new Dictionary<ushort, ushort>();
            IDictionary<ushort, string> isk = new Dictionary<ushort, string>();
            im[9] = 4; jm[4] = 9; isk[9] = "JNT0703,JNT0704";
            im[10] = 5; jm[5] = 10; isk[10] = "JNT0705,JNT0706";
            im[11] = 6; jm[6] = 11; isk[11] = "JNT0707,JNT0708";
            im[12] = 7; jm[7] = 12; isk[12] = "JNT0709,JNT0710";
            im[13] = 8; jm[8] = 13; isk[13] = "JNT0711,JNT0712";
            im[14] = 9; jm[9] = 14; isk[14] = "JNT0713,JNT0714";

            if (type == 0 || type == 1)
            {
                ushort iNo = player.Guardian;
                if (iNo != 0)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                    if (jm.ContainsKey(iNo))
                        XI.RaiseGMessage("G0OS," + player.Uid + ",1," + isk[jm[iNo]]);
                }
                if (player.TokenExcl.Count > 0)
                {
                    string input = XI.AsyncInput(player.Uid, "I1(p" +
                        string.Join("p", player.TokenExcl) + ")", "JNT0702", "0");
                    iNo = ushort.Parse(input);
                    if (im.ContainsKey(iNo))
                    {
                        XI.RaiseGMessage("G0MA," + player.Uid + "," + im[iNo]);
                        XI.RaiseGMessage("G0IS," + player.Uid + ",1," + isk[iNo]);
                    }
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + iNo);
                }
            }
            else if (type == 2)
            {
                string part = string.Join(",", im.Keys.Select(p => "I" + p));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + im.Keys.Count + "," + part);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + part);
            }
        }
        //0:G0IS,120;1:G0OS,80;2:G0ZH,0
        public bool JNT0703Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNT0703", player, fuse) && player.Armor == 0;
            else if (type == 2)
                return player.IsAlive && player.HP == 0 && player.Armor == 0;
            else if (type == 3) // ZB
            {
                string[] g0zb = fuse.Split(',');
                if (g0zb[1] == player.Uid.ToString() && g0zb[2] == "0")
                {
                    for (int i = 3; i < g0zb.Length; ++i)
                    {
                        ushort ut = ushort.Parse(g0zb[i]);
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux != null && tux.Type == Tux.TuxType.FJ)
                            return true;
                    }
                }
                else if (g0zb[1] == player.Uid.ToString() && g0zb[2] == "1")
                {
                    for (int i = 6; i < g0zb.Length; ++i)
                    {
                        ushort ut = ushort.Parse(g0zb[i]);
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux != null && tux.Type == Tux.TuxType.FJ)
                            return true;
                    }
                }
                return false;
            }
            else if (type == 4) // OT
            {
                string[] g0ot = fuse.Split(',');
                int idx = 1;
                while (idx < g0ot.Length)
                {
                    ushort ut = ushort.Parse(g0ot[idx]);
                    int n = int.Parse(g0ot[idx + 1]);
                    List<ushort> tuxs = Util.TakeRange(g0ot, idx + 2, idx + 2 + n)
                        .Select(p => ushort.Parse(p)).ToList();
                    if (ut == player.Uid && tuxs.Contains(player.Armor))
                        return true;
                    idx += (idx + 2 + n);
                }
                return false;
            }
            else
                return false;
        }
        public void JNT0703Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
            else if (type == 1)
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            else if (type == 2)
            {
                XI.RaiseGMessage(Artiad.Cure.ToMessage(
                    new Artiad.Cure(player.Uid, player.Uid, FiveElement.A, 2)));
                XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNT0703");
                List<Player> zeros = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).ToList();
                if (zeros.Count > 0)
                    XI.InnerGMessage("G0ZH,0", 0);
            }
            else if (type == 3)
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            else if (type == 4)
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        //0:R#GR,0
        public bool JNT0704Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count >= 2 && XI.Board.Garden.Values.Any(
                p => p.IsTared && p.GetBaseEquipCount() > 0);
        }
        public void JNT0704Action(Player player, int type, string fuse, string argst)
        {
            ushort[] uts = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + uts[0] + "," + uts[1]);
            ushort from = uts[2], tux = uts[3], to = uts[4];
            XI.RaiseGMessage("G0ZB," + to + ",1," + player.Uid + ",1," + from + "," + tux);
        }
        public string JNT0704Input(Player player, int type, string fuse, string prev)
        {
            string[] parts = prev.Split(',');
            if (prev == "")
                return "/Q2(p" + string.Join("p", player.Tux) + ")";
            else if (parts.Length <= 2)
                return "#交出装备,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                    p.IsTared && p.GetBaseEquipCount() > 0).Select(p => p.Uid)) + ")";
            else if (parts.Length <= 3)
            {
                ushort ut = ushort.Parse(parts[2]);
                Player py = XI.Board.Garden[ut];
                ushort[] cands = new ushort[] { py.Weapon, py.Armor, py.Trove, py.ExEquip };
                string head = (ut == player.Uid) ? "/Q1" : "/C1";
                return head + "(p" + string.Join("p", cands.Where(p => p != 0)) + ")";
            }
            else if (parts.Length <= 4)
            {
                ushort who = ushort.Parse(parts[2]);
                ushort ut = ushort.Parse(parts[3]);
                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                List<ushort> cands = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.Uid != ut && py.IsTared)
                    {
                        bool b1 = py.Weapon == 0 || (py.ExEquip == 0 && ((py.ExMask & 0x1) != 0));
                        bool b2 = py.Armor == 0 || (py.ExEquip == 0 && ((py.ExMask & 0x2) != 0));
                        bool b3 = py.Trove == 0 || (py.ExEquip == 0 && ((py.ExMask & 0x4) != 0));
                        if (tux.Type == Tux.TuxType.WQ && b1) { cands.Add(py.Uid); continue; }
                        if (tux.Type == Tux.TuxType.FJ && b2) { cands.Add(py.Uid); continue; }
                        if (tux.Type == Tux.TuxType.XB && b3) { cands.Add(py.Uid); continue; }
                    }
                }
                if (cands.Count > 0)
                    return "#交予的,/T1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else return "";
        }
        public bool JNT0705Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public void JNT0705Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid
                + ",JP03," + card + ";0," + fuse);
        }
        public string JNT0705Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "") return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else return "";
        }
        public bool JNT0706Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.HP < player.HPb;
            else if (type == 1)
            {
                ushort r = XI.Board.Rounder.Uid;
                return player.Uid != r && player.ROM.ContainsKey("Away") &&
                    ((List<ushort>)player.ROM["Away"]).Contains(r);
            }
            else if (type == 2)
                return IsMathISOS("JNT0706", player, fuse) && player.ROM.ContainsKey("Away") &&
                    ((List<ushort>)player.ROM["Away"]).Count > 0;
            else
                return false;
        }
        public void JNT0706Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                player.ROMInt = player.HPb - player.HP;
                List<ushort> list = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Uid != player.Uid)
                    {
                        XI.RaiseGMessage("G0OX," + py.Uid + ",0," + player.ROMInt);
                        list.Add(py.Uid);
                    }
                player.ROM["Away"] = list;
            }
            else if (type == 1)
            {
                ushort r = XI.Board.Rounder.Uid;
                ((List<ushort>)player.ROM["Away"]).Remove(r);
                XI.RaiseGMessage("G0IX," + r + ",0," + player.ROMInt);
            }
            else if (type == 2)
            {
                List<ushort> list = (List<ushort>)player.ROM["Away"];
                foreach (ushort ut in list)
                    XI.RaiseGMessage("G0IX," + ut + ",0," + player.ROMInt);
                player.ROMInt = 0;
                player.ROM.Remove("Away");
            }
        }
        public bool JNT0707Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNT0707", player, fuse);
            else if (type == 2) //G0IY
            {
                string[] g0iy = fuse.Split(',');
                ushort ytype = ushort.Parse(g0iy[1]);
                ushort ut = ushort.Parse(g0iy[2]);
                return XI.Board.Garden[ut].Team == player.Team && (ytype == 0 || ytype == 2);
            }
            else if (type == 3) // G0OY
            {
                if (player.ROM.ContainsKey("Ice"))
                {
                    IDictionary<ushort, bool> dict = (IDictionary<ushort, bool>)player.ROM["Ice"];
                    string[] g0oy = fuse.Split(',');
                    for (int i = 1; i < g0oy.Length; i += 2)
                    {
                        ushort ytype = ushort.Parse(g0oy[i]);
                        ushort ut = ushort.Parse(g0oy[i + 1]);
                        if (XI.Board.Garden[ut].Team == player.Team && ut != player.Uid
                            && (ytype == 0 || ytype == 2))
                        {
                            if (dict.ContainsKey(ut))
                                return true;
                        }
                    }
                }
                return false;
            }
            else
                return false;
        }
        public void JNT0707Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                IDictionary<ushort, bool> dict = new Dictionary<ushort, bool>();
                foreach (ushort ut in XI.Board.OrderedPlayer(player.Uid))
                {
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.Team && py.IsAlive)
                    {
                        string choose = XI.AsyncInput(ut, "#请选择「水御灵」执行项目。" +
                            "##战力+1##命中+1,Y2", "JNT0707", "0");
                        if (choose == "1")
                        {
                            dict[ut] = true;
                            XI.RaiseGMessage("G0IA," + ut + ",0,1");
                        }
                        else
                        {
                            dict[ut] = false;
                            XI.RaiseGMessage("G0IX," + ut + ",0,1");
                        }
                    }
                }
                player.ROM["Ice"] = dict;
            }
            else if (type == 1)
            {
                if (player.ROM.ContainsKey("Ice"))
                {
                    IDictionary<ushort, bool> dict = (IDictionary<ushort, bool>)player.ROM["Ice"];
                    foreach (var pair in dict)
                    {
                        if (pair.Value)
                            XI.RaiseGMessage("G0OA," + pair.Key + ",0,1");
                        else
                            XI.RaiseGMessage("G0OX," + pair.Key + ",0,1");
                    }
                    player.ROM.Remove("Ice");
                }
            }
            else if (type == 2)
            {
                IDictionary<ushort, bool> dict = player.ROM.ContainsKey("Ice") ? new Dictionary<ushort, bool>()
                    : (IDictionary<ushort, bool>)player.ROM["Ice"];
                string[] g0iy = fuse.Split(',');
                ushort ytype = ushort.Parse(g0iy[1]);
                ushort ut = ushort.Parse(g0iy[2]);
                string choose = XI.AsyncInput(ut, "#请选择「水御灵」执行项目。" +
                            "##战力+1##命中+1,Y2", "JNT0707", "0");
                if (choose == "1")
                {
                    dict[ut] = true;
                    XI.RaiseGMessage("G0IA," + ut + ",0,1");
                }
                else
                {
                    dict[ut] = false;
                    XI.RaiseGMessage("G0IX," + ut + ",0,1");
                }
                player.ROM["Ice"] = dict;
            }
            else if (type == 3)
            {
                IDictionary<ushort, bool> dict = (IDictionary<ushort, bool>)player.ROM["Ice"];
                string[] g0oy = fuse.Split(',');
                for (int i = 1; i < g0oy.Length; i += 2)
                {
                    ushort ytype = ushort.Parse(g0oy[i]);
                    ushort ut = ushort.Parse(g0oy[i + 1]);
                    if (XI.Board.Garden[ut].Team == player.Team && ut != player.Uid
                        && (ytype == 0 || ytype == 2))
                    {
                        if (dict.ContainsKey(ut))
                        {
                            if (dict[ut])
                                XI.RaiseGMessage("G0OA," + ut + ",0,1");
                            else
                                XI.RaiseGMessage("G0OX," + ut + ",0,1");
                            dict.Remove(ut);
                        }
                    }
                }
            }
        }
        public bool JNT0708Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length; )
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who != player.Uid && XI.Board.Garden[who].IsAlive && !drIn)
                        return true;
                    idx += (4 + n);
                }
            }
            return false;
        }
        public void JNT0708Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = argst.Split(',');
            string[] g1di = fuse.Split(',');
            string n1di = "";
            List<ushort> rest = new List<ushort>();
            List<ushort> eqs = new List<ushort>();
            ushort sub = ushort.Parse(blocks[0]);
            ushort tar = ushort.Parse(blocks[1]);
            ushort ut = ushort.Parse(blocks[2]);
            for (int idx = 1; idx < g1di.Length; )
            {
                ushort who = ushort.Parse(g1di[idx]);
                bool drIn = g1di[idx + 1] == "0";
                int n = int.Parse(g1di[idx + 2]);
                if (who == tar && XI.Board.Garden[who].IsAlive && !drIn)
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
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + sub);
            XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
            rest.Remove(ut); eqs.Remove(ut);
            rest.AddRange(eqs);
            if (rest.Count > 0)
                n1di += "," + tar + ",1," + rest.Count + "," + (rest.Count - eqs.Count) + "," + string.Join(",", rest);
            if (n1di.Length > 0)
                XI.InnerGMessage("G1DI" + n1di, 21);
        }
        public string JNT0708Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ISet<ushort> invs = new HashSet<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length; )
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who != player.Uid && XI.Board.Garden[who].IsTared && !drIn && n > 0)
                        invs.Add(who);
                    idx += (4 + n);
                }
                return "/T1(p" + string.Join("p", invs) + ")";
            }
            else if (prev.IndexOf(',', prev.IndexOf(',') + 1) < 0)
            {
                ushort ut = ushort.Parse(prev.Substring(prev.IndexOf(',') + 1));
                List<ushort> tuxes = new List<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length; )
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who == ut && !drIn && n > 0)
                    {
                        tuxes.AddRange(Util.TakeRange(g1di, idx + 4, idx + 4 + n)
                            .Select(p => ushort.Parse(p)));
                    }
                    idx += (4 + n);
                }
                return "/C1(p" + string.Join("p", tuxes) + ")";
            }
            else
                return "";
        }
        public bool JNT0709Valid(Player player, int type, string fuse)
        {
            return IsMathISOS("JNT0709", player, fuse);
        }
        public void JNT0709Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,3");
                XI.RaiseGMessage("G0OX," + player.Uid + ",0,2");
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,3");
                XI.RaiseGMessage("G0IX," + player.Uid + ",0,2");
            }
        }
        public bool JNT0710Valid(Player player, int type, string fuse)
        {
            if (XI.Board.InFightThrough)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                return harms.Any(p => p.Who == player.Uid && p.N > 0 &&
                    p.Element != FiveElement.SOL && p.Element != FiveElement.LOVE);
            }
            else
                return false;
        }
        public void JNT0710Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rmv = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
                if (harm.Who == player.Uid && harm.N > 0 &&
                        harm.Element != FiveElement.SOL && harm.Element != FiveElement.LOVE)
                {
                    if (--harm.N <= 0)
                        rmv.Add(harm);
                }
            harms.RemoveAll(p => rmv.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -12);
        }
        public bool JNT0711Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWarSucc(player);
        }
        public void JNT0711Action(Player player, int type, string fuse, string argst)
        {
            ushort n = ushort.Parse(argst);
            XI.RaiseGMessage(Artiad.Toxi.ToMessage(new Artiad.Toxi(player.Uid,
                player.Uid, FiveElement.A, n)));
            XI.RaiseGMessage("G0IA," + player.Uid + ",1," + n);
        }
        public string JNT0711Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#流失的HP数,/D" + ((player.HP > 1) ? ("1~" + player.HP) : "1");
            else
                return "";
        }
        public bool JNT0712Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public void JNT0712Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            XI.RaiseGMessage("G0TT," + player.Uid);
            int va = XI.Board.DiceValue;
            XI.RaiseGMessage("G0TT," + ut);
            int vb = XI.Board.DiceValue;

            if (va < vb)
                XI.RaiseGMessage("G0DH," + player.Uid + ",1,2");
            else if (va > vb)
                XI.RaiseGMessage("G0DH," + ut + ",1,2");
            else
                XI.RaiseGMessage("G0DH," + player.Uid + ",1,1," + ut + ",1,1");
        }
        public string JNT0712Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Uid != player.Uid && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNT0713Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => XI.Board.Garden[p.Who].Team == player.Team && p.N > 0);
        }
        public void JNT0713Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<ushort> uts = harms.Where(p => XI.Board.Garden[p.Who].Team == player.Team
                && p.N > 0).Select(p => p.Who).Distinct().ToList();
            XI.RaiseGMessage("G0DH," + string.Join(",", uts.Select(p => p + ",0,1")));
        }
        public bool JNT0714Valid(Player player, int type, string fuse)
        {
            string[] g0ht = fuse.Split(',');
            return g0ht[1] == player.Uid.ToString() && int.Parse(g0ht[2]) > 0;
        }
        public void JNT0714Action(Player player, int type, string fuse, string argst)
        {
            string[] g0ht = fuse.Split(',');
            int n = int.Parse(g0ht[2]) - 1;
            if (n > 0)
                XI.InnerGMessage("G0HT," + g0ht[1] + "," + n, 86);
            List<Artiad.Toxi> toxis = XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p =>
                new Artiad.Toxi(p.Uid, player.Uid, FiveElement.A, 1)).ToList();
            XI.RaiseGMessage(Artiad.Toxi.ToMessage(toxis));
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(p =>
                p.IsAlive && p.Team == player.Team).Select(p => p.Uid + ",0,1")));
        }
        #endregion TR007 - LiYiru

        #region TR008 - XiahouJinxuan
        public bool JNT0801Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Garden.Values.Any(p => p.IsTared &&
                    p.Uid != player.Uid && p.Team == player.Team);
            else if (type == 1 || type == 2)
                return player.RAMUshort == XI.Board.Rounder.Uid;
            else if (type == 3) // G0JM,R^ED
            {
                int idx = fuse.IndexOf(',');
                string stage = fuse.Substring(idx + 1);
                return player.RAMUshort == XI.Board.Rounder.Uid
                    && stage == "R" + player.RAMUshort + "ED";
            }
            else if (type == 4) // G0OY,0/1/2,A
            {
                string[] g0oys = fuse.Split(',');
                if (g0oys[1] == "2" && player.RAMUshort != 0)
                {
                    for (int i = 2; i < g0oys.Length; ++i)
                        if (g0oys[i] == player.Uid.ToString())
                            return player.RAMUshort == XI.Board.Rounder.Uid;
                }
                return false;
            }
            else
                return false;
        }
        public void JNT0801Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort tar = ushort.Parse(argst);
                player.RAMUshort = tar;
                XI.RaiseGMessage("G17F,W," + tar);
                XI.RaiseGMessage("G0JM,R" + tar + "ZW");
            }
            else if (type == 1)
                XI.Board.AllowNoSupport = false;
            else if (type == 2)
            {
                player.RAMUshort = 0;
                // TODO:Mark the target back.
                XI.RaiseGMessage("G0JM,R" + player.Uid + "ZZ");
            }
            else if (type == 3)
                XI.RaiseGMessage("G0JM,R" + player.Uid + "ZZ");
            else if (type == 4)
            {
                XI.Board.JumpTable["R" + player.RAMUshort + "ZZ"] =
                    "R" + player.Uid + "ED,R" + player.RAMUshort + "ED";
                XI.Board.JumpTable["R" + player.RAMUshort + "ED"] =
                    "R" + player.Uid + "ED,R" + player.RAMUshort + "ZZ";
            }
        }
        public string JNT0801Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Team == player.Team && p.Uid != player.Uid).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNT0802Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.Garden.Values
                    .Where(p => p.IsTared).Select(p => p.Uid).Except(player.TokenTars).Any();
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    if (player.TokenTars.Contains(harm.Who) &&
                            Artiad.Harm.GetPropedElement().Contains(harm.Element))
                        return true;
                }
            }
            //else if (type == 2)
            //{
            //    ushort ut = player.ROMPlayerTar;
            //    return ut != 0 && XI.Board.Garden[ut].IsAlive;
            //}
            return false;
        }
        public void JNT0802Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort uq = ushort.Parse(argst.Substring(0, idx));
                ushort ut = ushort.Parse(argst.Substring(idx + 1));
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + uq);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ut);
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                IDictionary<ushort, List<Artiad.Harm>> dict = new Dictionary<ushort, List<Artiad.Harm>>();
                foreach (Artiad.Harm harm in harms)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    if (player.TokenTars.Contains(harm.Who) &&
                            Artiad.Harm.GetPropedElement().Contains(harm.Element))
                        Util.AddToMultiMap(dict, harm.Who, harm);
                }
                IDictionary<ushort, string> input = new Dictionary<ushort, string>();
                foreach (var pair in dict)
                    input[pair.Key] = "#请选择是否使用「坚盾」状态##使用##不使用,Y2";
                IDictionary<ushort, string> reply = XI.MultiAsyncInput(input);
                foreach (var pair in reply)
                {
                    if (pair.Value == "1")
                        harms.RemoveAll(p => dict[pair.Key].Contains(p));
                }
                List<ushort> repu = reply.Where(p => p.Value == "1").Select(p => p.Key).ToList();
                if (repu.Count > 0)
                {
                    harms.RemoveAll(p => repu.Contains(p.Who));
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2," + repu.Count + "," + string.Join(",", repu));
                }
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -17);
            }
        }
        public string JNT0802Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1(p" + string.Join("p",
                    XI.Board.Garden.Values.Where(p => p.IsTared && !player.TokenTars.Contains(p.Uid))
                    .Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNT0803Valid(Player player, int type, string fuse)
        {
            ushort[] props = new ushort[] { 0, 1, 2, 3, 4 };
            List<Player> ops = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).ToList();
            ushort cnt = (ushort)props.Count(p => ops.Any(q => q.Pets[p] != 0));
            bool same = cnt == player.ROMUshort;

            if (type == 0 || type == 1)
                return !same;
            else if (type == 2 || type == 3)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNT0803")
                            return !same;
                }
            }
            return false;
        }
        public void JNT0803Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                ushort[] props = new ushort[] { 0, 1, 2, 3, 4 };
                List<Player> ops = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).ToList();
                ushort cnt = (ushort)props.Count(p => ops.Any(q => q.Pets[p] != 0));
                if (cnt > player.ROMUshort)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0," + (cnt - player.ROMUshort));
                else if (cnt < player.ROMUshort)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (player.ROMUshort - cnt));
                player.ROMUshort = cnt;
            }
            else if (type == 2 || type == 3)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNT0803")
                        {
                            ushort[] props = new ushort[] { 0, 1, 2, 3, 4 };
                            List<Player> ops = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                                p.Team == player.OppTeam).ToList();
                            ushort cnt = (ushort)props.Count(p => ops.Any(q => q.Pets[p] != 0));
                            if (type == 2)
                            {
                                player.ROMUshort = cnt;
                                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + cnt);
                            }
                            else if (type == 3)
                            {
                                XI.RaiseGMessage("G0OA," + player.Uid + ",0," + cnt);
                                player.ROMUshort = 0;
                            }

                        }
                }
            }
        }
        #endregion TR008 - XiahouJinxuan
        #region TR009 - Xia
        //public void JNT0901Action(Player player, int type, string fuse, string argst)
        //{
        //    XI.RaiseGMessage("G0HQ,2," + player.Uid + ",1,1");
        //    XI.RaiseGMessage("G0PB,0," + player.Uid + ",1," + argst);
        //}
        //public bool JNT0901Valid(Player player, int type, string fuse)
        //{
        //    if (player.Tux.Count > 0 && XI.Board.RoundIN != "R" + XI.Board.Rounder.Uid + "EV")
        //    {
        //        string[] args = fuse.Split(',');
        //        for (int i = 1; i < args.Length; )
        //        {
        //            ushort me = ushort.Parse(args[i]);
        //            ushort lose = ushort.Parse(args[i + 1]);
        //            if (lose == 0) // Get Card
        //            {
        //                Player py = XI.Board.Garden[me];
        //                if ((py.Team == player.Team) && (py.Uid != player.Uid || !player.IsSKOpt))
        //                {
        //                    int n = int.Parse(args[i + 2]);
        //                    if (n > 0)
        //                        return true;
        //                }
        //                i += 3;
        //            }
        //            else if (lose == 1)
        //                i += 3;
        //            else if (lose == 2)
        //                i += 3;
        //            else if (lose == 3)
        //                i += 2;
        //            else
        //                break;
        //        }
        //    }
        //    return false;
        //}
        //public string JNT0901Input(Player player, int type, string fuse, string prev)
        //{
        //    if (prev == "")
        //        return "#交换,/Q1(p" + string.Join("p", player.Tux) + ")";
        //    else
        //        return "";
        //}
        public void JNT0901Action(Player player, int type, string fuse, string argst)
        {
            HashSet<ushort> invs = new HashSet<ushort>();
            // G0DI,A,0,n,B,1,m
            string[] blocks = fuse.Split(',');
            int idx = 1;
            while (idx < blocks.Length)
            {
                ushort n = ushort.Parse(blocks[idx + 2]);
                if (blocks[idx + 1] == "0")
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    Player py = XI.Board.Garden[who];
                    if (who != player.Uid && py.IsAlive && py.Team == player.Team && n > 0)
                        invs.Add(who);
                }
                idx += (n + 4);
            }
            List<ushort> invl = new List<ushort>();
            foreach (ushort ut in XI.Board.OrderedPlayer())
            {
                if (invs.Contains(ut))
                    invl.Add(ut);
            }
            foreach (ushort ut in invs)
            {
                Player py = XI.Board.Garden[ut];
                if (py.Tux.Count > 0 && player.Tux.Count > 0)
                {
                    string target = XI.AsyncInput(player.Uid, "/T1(p" + ut + ")", "JNT0901", "0");
                    if (target == ut.ToString())
                        XI.RaiseGMessage("G1XR,0," + player.Uid + "," + ut + ",1,1");
                    //{
                    //    IDictionary<ushort, string> dicts = new Dictionary<ushort, string>();
                    //    dicts.Add(player.Uid, "Q1(p" + string.Join("p", player.Tux) + ")");
                    //    dicts.Add(ut, "Q1(p" + string.Join("p", py.Tux) + ")");
                    //    IDictionary<ushort, string> q = XI.MultiAsyncInput(dicts);
                    //    XI.RaiseGMessage("G0HQ,0," + ut + "," + player.Uid + ",1,1," + q[player.Uid]);
                    //    XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + ut + ",1,1," + q[ut]);
                    //}
                }
            }
            XI.InnerGMessage(fuse, 221);
        }
        public bool JNT0901Valid(Player player, int type, string fuse)
        {
            if (!player.IsAlive || player.Tux.Count <= 0)
                return false;
            // G0DI,A,0,n,B,1,m
            string[] blocks = fuse.Split(',');
            int idx = 1;
            while (idx < blocks.Length)
            {
                ushort n = ushort.Parse(blocks[idx + 2]);
                if (blocks[idx + 1] == "0")
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    Player py = XI.Board.Garden[who];
                    if (who != player.Uid && py.IsAlive && py.Team == player.Team && n > 0)
                        return true;
                }
                idx += (n + 4);
            }
            return false;
        }
        public bool JNT0902Valid(Player player, int type, string fuse)
        {
            string[] g0zws = fuse.Split(',');
            for (int i = 1; i < g0zws.Length; ++i)
                if (g0zws[i] == player.Uid.ToString())
                    return true;
            return false;
        }
        public void JNT0902Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == player.Team).Select(p => p.Uid + ",0,2")));
            string[] args = fuse.Split(',');
            for (int i = 1; i < args.Length; ++i)
                if (args[i] != player.Uid.ToString())
                {
                    ushort me = ushort.Parse(args[i]);
                    Player py = XI.Board.Garden[me];
                    string range = Util.SSelect(XI.Board,
                        p => p.IsAlive && p.Team == player.Team);
                    string input = XI.AsyncInput(me, "#获得补牌的,T1" + range, "G0ZW", "0");
                    XI.RaiseGMessage("G0DH," + input + ",0,2");
                }
            XI.InnerGMessage(fuse, 301);
        }
        public bool JNT0903Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.N >= player.HP && harm.Element != FiveElement.LOVE)
                    return true;
            }
            return false;
        }
        public void JNT0903Action(Player player, int type, string fuse, string args)
        {
            Artiad.Harm thisHarm = null;
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.N >= player.HP && harm.Element != FiveElement.LOVE)
                    thisHarm = harm;
            }
            XI.RaiseGMessage("G0TT," + player.Uid);
            int value = XI.Board.DiceValue;
            if (value < 5)
                harms.Remove(thisHarm);
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -69);
        }
        #endregion TR009 - Xia
        #region TR010 - MuChanglan
        public bool JNT1001Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0 || type == 1) // "G1IZ"
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    Player py = XI.Board.Garden[who];
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (py.Team == player.OppTeam &&
                            XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        return true;
                }
                return false;
            }
            else if (type == 2 || type == 3)
            {
                bool any = false;
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.OppTeam)
                    {
                        if (py.Weapon != 0) { any = true; break; }
                        Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(py.ExEquip);
                        if (tux != null && tux.Type == Tux.TuxType.WQ)
                        { any = true; break; }
                    }
                if (any)
                {
                    if (blocks[1] == player.Uid.ToString())
                    {
                        for (int i = 3; i < blocks.Length; ++i)
                            if (blocks[i] == "JNT1001")
                                return true;
                    }
                }
                return false;
            }
            else return false;
        }
        public void JNT1001Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0 || type == 1)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    Player py = XI.Board.Garden[who];
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (py.Team == player.OppTeam &&
                            XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                    {
                        if (type == 0)
                            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                        else if (type == 1)
                            XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                    }
                }
            }
            else if (type == 2 || type == 3)
            {
                int totalAmount = 0;
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.OppTeam)
                    {
                        if (py.Weapon != 0)
                            ++totalAmount;
                        Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(py.ExEquip);
                        if (tux != null && tux.Type == Tux.TuxType.WQ)
                            ++totalAmount;
                    }
                if (totalAmount > 0)
                {
                    if (type == 2)
                        XI.RaiseGMessage("G0IA," + player.Uid + ",0," + totalAmount);
                    else if (type == 3)
                        XI.RaiseGMessage("G0OA," + player.Uid + ",0," + totalAmount);
                }
            }
        }
        public bool JNT1002Valid(Player player, int type, string fuse)
        {
            string[] g0xzs = fuse.Split(',');
            return g0xzs[1] != player.Uid.ToString() && g0xzs[2] == "2";
        }
        public void JNT1002Action(Player player, int type, string fuse, string argst)
        {
            // G0XZ,A,0,M,n,[m]
            string[] g0xzs = fuse.Split(',');
            if (g0xzs[1] != player.Uid.ToString() && g0xzs[2] == "2")
            {
                int amount = g0xzs.Length > 5 ? int.Parse(g0xzs[5]) : int.Parse(g0xzs[4]);
                XI.RaiseGMessage("G0XZ," + player.Uid + ",2,0," + amount);
            }
        }
        public bool JNT1003Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            return XI.Board.IsAttendWar(player) && meLose && XI.Board.Garden.Values.Where(p => p.IsTared &&
                XI.Board.IsAttendWar(p) && p.Team == player.OppTeam && p.Tux.Count > 0).Any();
        }
        public void JNT1003Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> opps = XI.Board.Garden.Values.Where(p => p.IsTared &&
                XI.Board.IsAttendWar(p) && p.Team == player.OppTeam && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            string t = XI.AsyncInput(player.Uid, "T1(p" + string.Join("p", opps)
                + ")", "JNT1003", "0");
            Player py = XI.Board.Garden[ushort.Parse(t)];
            XI.AsyncInput(player.Uid, "C1(" + Util.RepeatString("p0", py.Tux.Count)
                + ")", "JNT1003", "0");
            XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + t + ",2,1");
        }
        #endregion TR010 - MuChanglan
        #region TR011 - JiangCheng
        public bool JNT1101Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py != player && py.Team == player.Team && py.IsTared && harm.Element != FiveElement.SOL &&
                        harm.Element != FiveElement.LOVE && harm.Source != harm.Who && harm.N > 0)
                    return true;
            }
            return false;
        }
        public void JNT1101Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            Artiad.Harm thisHarm = null, thatHarm = null;
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == ut && harm.N > 0)
                    thisHarm = harm;
                else if (harm.Who == player.Uid)
                    thatHarm = harm;
            }
            if (thisHarm != null)
            {
                //if (thisHarm.Element == FiveElement.SOL) {
                //    int hpLeft = XI.Board.Garden[thisHarm.Who].HP - thisHarm.N;
                //    if (thatHarm != null) {
                //        if (thatHarm.Element == FiveElement.SOL) {
                //            int ahpLeft = player.HP - thatHarm.N;
                //            if (hpLeft < ahpLeft)
                //                thatHarm.N = (player.HP - hpLeft);
                //            harms.Remove(thisHarm);
                //        } else {
                //            thisHarm.Who = player.Uid;
                //            thisHarm.N = player.HP - hpLeft;
                //            harms.Remove(thatHarm);
                //        }
                //    } else {
                //        thisHarm.Who = player.Uid;
                //        thisHarm.N = player.HP - hpLeft;
                //    }
                //} else {
                if (thatHarm != null)
                {
                    if (thatHarm.Element == FiveElement.SOL)
                        harms.Remove(thisHarm);
                    else
                    {
                        thatHarm.N += (thisHarm.N + 1);
                        harms.Remove(thisHarm);
                    }
                }
                else
                {
                    ++thisHarm.N;
                    thisHarm.Who = player.Uid;
                }
            }
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -60);
        }
        public string JNT1101Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                HashSet<ushort> uts = new HashSet<ushort>();
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    if (py != player && py.Team == player.Team && py.IsTared && harm.Element != FiveElement.SOL &&
                            harm.Element != FiveElement.LOVE && harm.Source != harm.Who && harm.N > 0)
                        uts.Add(py.Uid);
                }
                return "#代为承受伤害,/T1(p" + string.Join("p", uts) + ")";
            }
            else
                return "";
        }
        public bool JNT1102Valid(Player player, int type, string fuse)
        {
            return XI.Board.Supporter.Uid == player.Uid;
        }
        public void JNT1102Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",1,2");
        }
        public bool JNT1103Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.PCS.ListAllHeros().Any(p => p.Ofcode == "XJ505");
            var v = XI.Board.Garden.Values;
            var h = XI.LibTuple.HL;
            bool b2 = v.Any(p => p.Team == player.OppTeam && (
                h.InstanceHero(p.SelectHero).Bio.Contains("A") || h.InstanceHero(p.SelectHero).Bio.Contains("E")));
            bool b3 = v.Any(p => p.Team == player.Team && p.Uid != player.Uid &&
                h.InstanceHero(p.SelectHero).Bio.Contains("K"));
            return b1 && (b2 || b3);
        }
        public void JNT1103Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,0," + player.Uid);
            XI.RaiseGMessage("G0IY,0," + player.Uid + ",10605");
        }
        #endregion TR011 - JiangCheng
        #region TR012 - HuangfuZhuo
        public void JNT1201Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                ushort card = ushort.Parse(argst);
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);

                ushort gamer = ushort.Parse(XI.AsyncInput(player.Uid, "T1" + Util.SSelect(
                    XI.Board, p => p.Team == player.Team && p.IsAlive), "JNT1201", "0"));
                VI.Cout(0, "{0}对{1}发动「天循两仪」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(gamer));
                XI.RaiseGMessage("G0XZ," + gamer + ",2,0,1");

                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
                ++player.RAMUshort;
            }
            else if (type == 2)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + player.RAMUshort);
        }
        public bool JNT1201Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return player.Tux.Count > 0 && XI.Board.MonPiles.Count > 0;
            else if (type == 2)
                return player.RAMUshort > 0;
            return false;
        }
        public string JNT1201Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "" && (type == 0 || type == 1))
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNT1202Valid(Player player, int type, string fuse)
        {
            return XI.Board.Supporter == null || !XI.Board.SupportSucc;
        }
        public void JNT1202Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,3");
        }
        #endregion TR012 - HuangfuZhuo
        #region TR013 - XieCangxing
        public bool JNT1301Valid(Player player, int type, string fuse)
        {
            bool b0 = XI.Board.IsAttendWar(player);
            bool b1 = player.HP >= player.RAMUshort + 1;
            bool b2 = player.Weapon == 0;
            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(player.ExEquip);
            bool b3 = tux == null || tux.Type == Base.Card.Tux.TuxType.WQ;
            return b0 && b1 && b2 && b3;
        }
        public void JNT1301Action(Player player, int type, string fuse, string argst)
        {
            ++player.RAMUshort;
            Harm(player, player, player.RAMUshort);
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
        }
        public bool JNT1302Valid(Player player, int type, string fuse)
        {
            return player.HP < 5;
        }
        public void JNT1302Action(Player player, int type, string fuse, string argst)
        {
            int total = player.HP * 2;
            bool done = false;
            while (!done)
            {
                List<ushort> allPets = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.IsAlive && py.Team == player.OppTeam)
                        allPets.AddRange(py.Pets.Where(p => p != 0));
                }
                int na = allPets.Count;
                if (na == 0)
                    break;
                string ts = XI.AsyncInput(XI.Board.Opponent.Uid, "#弃置累积战力" + total + "及以上的,M1" +
                    (na > 1 ? "~" + na : "") + "(p" + string.Join("p", allPets), "JNT1302", "0");
                List<ushort> pick = ts.Split(',').Select(p => ushort.Parse(p)).ToList();
                int sum = pick.Sum(p => XI.LibTuple.ML.Decode(p).STR);
                if (sum >= total || pick.Count == na)
                {
                    foreach (ushort ut in pick)
                    {
                        foreach (Player py in XI.Board.Garden.Values)
                        {
                            if (py.Pets.Contains(ut))
                            {
                                XI.RaiseGMessage("G0HL," + py.Uid + "," + ut);
                                break;
                            }
                        }
                    }
                    done = true;
                }
            }
            XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        #endregion TR013 - XieCangxing
        #region TR014 - Jieluo
        public void JNT1401Action(Player player, int type, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.ALIVE_PIS)
                        && py.Team == player.OppTeam && py.HP - harm.N < 1)
                    harm.N = py.HP - 1;
                if (harm.N <= 0)
                    rvs.Add(harm);
            }
            harms.RemoveAll(p => rvs.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 96);
        }
        public bool JNT1401Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.ALIVE_PIS)
                        && py.Team == player.OppTeam && py.HP - harm.N < 1)
                    return true;
            }
            return false;
        }
        public void JNT1402Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort tux = ushort.Parse(argst.Substring(0, idx));
            ushort ut = ushort.Parse(argst.Substring(idx + 1));

            XI.RaiseGMessage("G0QZ," + player.Uid + "," + tux);
            Artiad.Harm thisHarm = null, thatHarm = null;
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.N > 0 &&
                        harm.Element != FiveElement.SOL && harm.Element != FiveElement.LOVE)
                    thisHarm = harm;
                else if (harm.Who == ut)
                    thatHarm = harm;
            }
            if (thisHarm != null)
            {
                int a, b;
                a = thisHarm.N / 2;
                b = thisHarm.N - a;
                //if (thisHarm.Element != FiveElement.SOL)
                //{
                //    a = thisHarm.N / 2;
                //    b = thisHarm.N - a;
                //}
                //else
                //{
                //    a = thisHarm.N;
                //    b = XI.Board.Garden[ut].HP - (player.HP - thisHarm.N);
                //}
                if (a > 0)
                    thisHarm.N = a;
                else { harms.Remove(thisHarm); }
                thisHarm.Mask = Artiad.IntHelper.SetMask(thisHarm.Mask, GiftMask.ALIVE_PIS, true);

                if (b > 0)
                {
                    if (thatHarm != null && thatHarm.Element == thisHarm.Element)
                        thatHarm.N += b;
                    else
                        harms.Add(thatHarm = new Artiad.Harm(ut, thisHarm.Source, thisHarm.Element,
                            b, thisHarm.Mask));
                    thatHarm.Mask = Artiad.IntHelper.SetMask(thatHarm.Mask, GiftMask.ALIVE_PIS, true);
                }
            }
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 91);
        }
        public bool JNT1402Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.N > 0 && harm.Who == player.Uid &&
                            harm.Element != FiveElement.SOL && harm.Element != FiveElement.LOVE)
                        return player.Tux.Count > 0 &&
                            XI.Board.Garden.Values.Where(p => p.Gender == 'M' && p.IsTared).Any();
                }
            }
            return false;
        }
        public string JNT1402Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                return "#弃置,/Q1(p" + string.Join("p", player.Tux) + "),#平分伤害,/T1(p"
                    + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Gender == 'M' && p.IsTared).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        public void JNT1403Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort tux = ushort.Parse(argst.Substring(0, idx));
            ushort ut = ushort.Parse(argst.Substring(idx + 1));

            XI.RaiseGMessage("G0QZ," + player.Uid + "," + tux);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            Player leifeng = XI.Board.Garden[ut];
            List<Artiad.Harm> ncnts = new List<Artiad.Harm>();

            int totalAmount = 0;
            Artiad.Harm thisHarm = null;
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    if (py.Team != leifeng.Team)
                        ncnts.Add(harm);
                    else if (py.Uid != leifeng.Uid)
                    {
                        totalAmount += harm.N;
                        thisHarm = harm;
                    }
                    else
                    {
                        totalAmount += harm.N;
                        thisHarm = harm;
                    }
                }
                else
                    ncnts.Add(harm);
            }
            totalAmount -= (XI.Board.Garden.Values.Count(
                p => p.IsAlive && p.Team == leifeng.Team) - 1);
            if (thisHarm != null)
            {
                thisHarm.N = totalAmount;
                thisHarm.Who = leifeng.Uid;
                thisHarm.Mask = Artiad.IntHelper.SetMask(thisHarm.Mask, GiftMask.ALIVE_PIS, true);
                ncnts.Add(thisHarm);
            }
            if (ncnts.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(ncnts), -74);
        }
        public bool JNT1403Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                HashSet<int> teams = new HashSet<int>();
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    if (harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                    {
                        if (!teams.Contains(py.Team))
                            teams.Add(py.Team);
                        else
                            return true;
                    }
                }
            }
            return false;
        }
        public string JNT1403Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                IDictionary<int, HashSet<ushort>> dict = new Dictionary<int, HashSet<ushort>>();
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                    {
                        Player py = XI.Board.Garden[harm.Who];
                        if (!dict.ContainsKey(py.Team))
                        {
                            HashSet<ushort> hs = new HashSet<ushort>();
                            hs.Add(py.Uid);
                            dict[py.Team] = hs;
                        }
                        else
                            dict[py.Team].Add(py.Uid);
                    }
                }
                List<ushort> lists = new List<ushort>();
                foreach (var pair in dict)
                {
                    if (pair.Value.Count > 1)
                        lists.AddRange(pair.Value.Where(p => XI.Board.Garden[p].IsTared));
                }
                return "#弃置,/Q1(p" + string.Join("p", player.Tux) + "),#承担全部伤害,/T1(p"
                       + string.Join("p", lists) + ")";
            }
            else
                return "";
        }

        #endregion TR014 - Jieluo
        #region TR015 - Liyan
        public void JNT1501Action(Player player, int type, string fuse, string argst)
        {
            var g = XI.Board.Garden;
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            int count = harms.Count(p => p.Who != player.Uid && p.Source != p.Who
                && g[p.Who].Team == player.Team && p.Element != FiveElement.LOVE);
            IDictionary<Player, int> fengs = new Dictionary<Player, int>();
            for (int i = 0; i < count; ++i)
            {
                string input = XI.AsyncInput(player.Uid, "#HP-1,/T1(p" + string.Join("p",
                   g.Values.Where(p => p.IsTared && p.Team == player.OppTeam).Select(p => p.Uid)) + ")", "JNT1501", "0");
                if (input == "0" || input.StartsWith("/") || input == VI.CinSentinel)
                    break;
                Player py = g[ushort.Parse(input)];
                if (fengs.ContainsKey(py))
                    ++fengs[py];
                else
                    fengs[py] = 1;
            }
            if (fengs.Count > 0)
                Harm(player, fengs.Keys.ToList(), fengs.Values.ToList());
        }
        public bool JNT1501Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => p.Who != player.Uid && p.Source != p.Who && XI.Board.Garden[p.Who].Team == player.Team
                && p.Element != FiveElement.LOVE) && XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.OppTeam);
        }
        public void JNT1502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0) // ST->[EP]->GR
            {
                player.ROMUshort |= 0x1;
                XI.RaiseGMessage("G0JM,R" + player.Uid + "GS");
            }
            else if (type == 1) // GR->GE->[GF]->EV->GR->GE->...
            {
                player.ROMUshort = (ushort)(player.ROMUshort & ~0x1);
                XI.RaiseGMessage("G0JM,R" + player.Uid + "EV");
            }
            else if (type == 2)
                player.ROMUshort |= 0x2;
            else if (type == 3)
            {
                player.ROMUshort = (ushort)(player.ROMUshort & ~0x2);
                //XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                Cure(player, player, 1);
            }
        }
        public bool JNT1502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return (player.ROMUshort & 0x1) == 0;
            else if (type == 1)
                return (player.ROMUshort & 0x1) != 0;
            else if (type == 2)
            {
                string[] g2ym = fuse.Split(',');
                return XI.Board.Rounder == player && g2ym[1] == "2";
            }
            else if (type == 3)
            {
                bool b1 = (player.ROMUshort & 0x2) != 0;
                string[] g1evs = fuse.Split(',');
                bool b2 = g1evs[1] == player.Uid.ToString();
                return b1 && b2;
            }
            else
                return false;
        }
        #endregion TR015 - Liyan
        #region TR016 - Xuanji
        public void JNT1601Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#获得2张补牌,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsAlive && !player.RAMUtList.Contains(p.Uid))
                .Select(p => p.Uid)) + ")", "JNT1601", "0");
            ushort who = ushort.Parse(input);
            player.RAMUtList.Add(who);
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,2");
        }
        public bool JNT1601Valid(Player player, int type, string fuse)
        {
            string[] g0zbs = fuse.Split(',');
            return g0zbs[1] == player.Uid.ToString() && XI.Board.Garden.Values
                .Where(p => p.IsAlive).Select(p => p.Uid).Except(player.RAMUtList).Any();
        }
        public void JNT1602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                player.RAMUshort = 1;
            else if (type == 1)
            {
                player.RAMUshort = 2;
                if (player.Team == XI.Board.Rounder.Team)
                    XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",2");
                else
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",2");
            }
            else if (type == 2)
            {
                bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                    || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
                if (meLose)
                    Harm(player, player, 2, FiveElement.AQUA);
                else
                    XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "VT");
            }
            else if (type == 3 || type == 5)
            {
                player.ROMUshort = 1;
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            }
            else if (type == 4 || type == 6)
            {
                player.ROMUshort = 0;
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
            }
        }
        public bool JNT1602Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                Base.Card.Monster mon = XI.Board.Battler as Monster;
                if (mon != null && mon.Element == FiveElement.AQUA)
                {
                    if (player.Team == XI.Board.Rounder.Team && XI.Board.Supporter != null)
                        return true;
                    else if (player.Team == XI.Board.Rounder.OppTeam && XI.Board.Hinder != null)
                        return true;
                }
                return false;
            }
            else if (type == 1)
            {
                if (player.RAMUshort == 1)
                {
                    if (player.Team == XI.Board.Rounder.Team && XI.Board.Supporter != null)
                        return true;
                    else if (player.Team == XI.Board.Rounder.OppTeam && XI.Board.Hinder != null)
                        return true;
                }
            }
            else if (type == 2)
                return player.RAMUshort == 1 || player.RAMUshort == 2;
            else if (type >= 3 && type <= 6)
            {
                int zidx = Util.GetFiveElementId(FiveElement.AQUA);
                bool has = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.Team && p.Pets[zidx] != 0).Any();
                if (type == 3)
                    return player.ROMUshort == 0 && has;
                else if (type == 4)
                    return player.ROMUshort == 1 && !has;
                else if (type == 5 || type == 6)
                {
                    string[] parts = fuse.Split(',');
                    if (parts[1] == player.Uid.ToString())
                    {
                        for (int i = 3; i < parts.Length; ++i)
                            if (parts[i] == "JNT1602")
                            {
                                if (type == 5)
                                    return player.ROMUshort == 0 && has;
                                else if (type == 6)
                                    return player.ROMUshort == 1 && !has;
                            }
                    }
                }
            }
            return false;
        }
        #endregion TR016 - Xuanji
        #region TR017 - Huaishuo
        public void JNT1701Action(Player player, int type, string fuse, string argst)
        {
            // !G0IS,!G0OS,!G0IY,!G0OY,G1EV,!G1EV
            // 1,1,1,1,1,1
            // 0,0,0,0,0,0

            //if (type == 0)
            //{
            //    foreach (Player py in XI.Board.Garden.Values)
            //    {
            //        if (py.IsAlive && py.Team == player.Team)
            //            py.TuxLimit += 1;
            //    }
            //}
            //else if (type == 1)
            //{
            //    foreach (Player py in XI.Board.Garden.Values)
            //    {
            //        if (py.IsAlive && py.Team == player.Team)
            //            py.TuxLimit -= 1;
            //    }
            //}
            //else if (type == 2)
            //{
            //    string[] g0iys = fuse.Split(',');
            //    if (g0iys[1] == "0" || g0iys[1] == "2") // Reset OR Attend
            //    {
            //        ushort ut = ushort.Parse(g0iys[2]);
            //        Player py = XI.Board.Garden[ut];
            //        if (py.IsAlive && py.Team == player.Team)
            //            py.TuxLimit += 1;
            //    }
            //}
            //else if (type == 3)
            //{
            //    if (player.IsAlive)
            //    {
            //        string[] g0iys = fuse.Split(',');
            //        for (int i = 1; i < g0iys.Length; i += 2)
            //        {
            //            if (g0iys[i] == "0" || g0iys[i] == "2") // Reset OR Attend
            //            {
            //                ushort ut = ushort.Parse(g0iys[i + 1]);
            //                Player py = XI.Board.Garden[ut];
            //                if (py.IsAlive && py.Team == player.Team)
            //                    py.TuxLimit -= 1;
            //            }
            //        }
            //    }
            //}
            if (type == 0)
            {
                IDictionary<ushort, string> discards = new Dictionary<ushort, string>();
                foreach (Player py in XI.Board.Garden.Values)
                {
                    //if (py.IsAlive && py.Team == player.Team)
                    //{
                    //    if (py.Tux.Count == 1)
                    //        discards.Add(py.Uid, "/Q1(p" + py.Tux[0] + ")");
                    //    else if (py.Tux.Count > 1)
                    //        discards.Add(py.Uid, "/Q1~2(p" + string.Join("p", py.Tux) + ")");
                    //}
                    if (py.IsAlive && py.Team == player.Team && py.Tux.Count > 0)
                        discards.Add(py.Uid, "#临时移出的,/Q1(p" + string.Join("p", py.Tux) + ")");
                }
                if (discards.Count > 0)
                {
                    IDictionary<ushort, string> result = XI.MultiAsyncInput(discards);
                    string g2rn = "";
                    foreach (var pair in result)
                    {
                        if (!pair.Value.StartsWith("/") && pair.Value != VI.CinSentinel && pair.Value != "0")
                        {
                            List<ushort> cards = pair.Value.Split(',').Select(p => ushort.Parse(p)).ToList();
                            XI.RaiseGMessage("G0OT," + pair.Key + "," + cards.Count() + "," + string.Join(",", cards));
                            XI.Board.PendingTux.Enqueue(pair.Key + "," + "JNT1701," + string.Join(",", cards));
                            g2rn += "," + pair.Key + ",0," + cards.Count + "," + string.Join(",", cards);
                        }
                    }
                    if (g2rn != "")
                        XI.RaiseGMessage("G2RN" + g2rn);
                }
            }
            else if (type == 1)
            {
                List<string> rms = new List<string>();
                string g2rn = "";
                foreach (string tuxInfo in XI.Board.PendingTux)
                {
                    List<ushort> accu = new List<ushort>();
                    string[] parts = tuxInfo.Split(',');
                    string utstr = parts[0];
                    if (parts[1] == "JNT1701")
                    {
                        string tails = (parts.Length - 2) + "," +
                            string.Join(",", Util.TakeRange(parts, 2, parts.Length));
                        XI.RaiseGMessage("G0IT," + utstr + "," + tails);
                        g2rn += ",0," + utstr + "," + tails;
                        rms.Add(tuxInfo);
                    }
                }
                foreach (string rm in rms)
                    XI.Board.PendingTux.Remove(rm);
                if (g2rn != "")
                    XI.RaiseGMessage("G2RN" + g2rn);
            }
        }
        public bool JNT1701Valid(Player player, int type, string fuse)
        {
            //if (type == 0 || type == 1)
            //{
            //    string[] parts = fuse.Split(',');
            //    if (parts[1] == player.Uid.ToString())
            //    {
            //        for (int i = 3; i < parts.Length; ++i)
            //            if (parts[i] == "JNT1701")
            //                return true;
            //    }
            //    return false;
            //}
            //else if (type == 2)
            //{
            //    string[] g0iys = fuse.Split(',');
            //    if (g0iys[1] == "0" || g0iys[1] == "2") // Reset OR Attend
            //    {
            //        ushort ut = ushort.Parse(g0iys[2]);
            //        Player py = XI.Board.Garden[ut];
            //        if (py.IsAlive && py.Team == player.Team)
            //            return true;
            //    }
            //}
            //else if (type == 3)
            //{
            //    if (player.IsAlive)
            //    {
            //        string[] g0iys = fuse.Split(',');
            //        for (int i = 1; i < g0iys.Length; i += 2)
            //        {
            //            if (g0iys[i] == "0" || g0iys[i] == "2") // Reset OR Attend
            //            {
            //                ushort ut = ushort.Parse(g0iys[i + 1]);
            //                Player py = XI.Board.Garden[ut];
            //                if (py.IsAlive && py.Team == player.Team)
            //                    return true;
            //            }
            //        }
            //    }
            //}
            if (type == 0)
            {
                bool b0 = XI.Board.Garden.Values.Any(p => p.IsAlive
                    && p.Team == player.Team && p.Tux.Count > 0);
                if (player.IsSKOpt)
                {
                    Base.Card.Evenement eve = XI.LibTuple.EL.DecodeEvenement(XI.Board.Eve);
                    b0 &= eve.IsTuxInvolved(false);
                }
                return b0;
            }
            else if (type == 1)
                return true;
            return false;
        }
        public void JNT1702Action(Player player, int type, string fuse, string argst)
        {
            List<Player> invs = argst.Split(',').Select(
                p => XI.Board.Garden[ushort.Parse(p)]).ToList();
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py.Team == player.Team && harm.Element != FiveElement.LOVE &&
                    py.Tux.Count < py.TuxLimit && py.HP > harm.N && py.IsTared)
                {
                    if (invs.Contains(py))
                        ++harm.N;
                }
            }
            if (invs.Count > 0)
            {
                XI.RaiseGMessage("G0DH," + string.Join(",",
                    invs.Select(p => p.Uid + ",0," + (p.TuxLimit - p.Tux.Count))));
            }
            XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 93);
        }
        public bool JNT1702Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                bool countable = harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL;
                if (py.Team == player.Team && countable &&
                        py.Tux.Count < py.TuxLimit && py.HP > harm.N && py.IsTared)
                    return true;
            }
            return false;
        }
        public string JNT1702Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                HashSet<ushort> invs = new HashSet<ushort>();
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    bool countable = harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL;
                    if (py.Team == player.Team && countable &&
                            py.Tux.Count < py.TuxLimit && py.HP > harm.N && py.IsTared)
                        invs.Add(harm.Who);
                }
                if (invs.Count > 1)
                    return "/T1~" + invs.Count + "(p" + string.Join("p", invs) + ")";
                else
                    return "/T1(p" + string.Join("p", invs) + ")";
            }
            else
                return "";
        }
        #endregion TR017 - Huaishuo
        #region TR018 - Qinji
        public void JNT1801Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
                XI.RaiseGMessage("G0IP," + player.Team + ",1");
            }
            else if (type == 1)
            {
                string add = XI.AsyncInput(player.Uid, "#「弦歌问情」触发，是否令我方战力+1##是##否,Y2",
                    "JNT1801", "0");
                if (add != "2")
                    XI.RaiseGMessage("G0IP," + player.Team + ",1");
            }
        }
        public bool JNT1801Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Any(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Base.Card.Tux.TuxType.ZP);
            else if (type == 1)
            {
                // G0CC,A,0,B,KN,x1,x2;TF
                string[] g0cc = fuse.Split(',');
                string kn = g0cc[4];
                Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(kn);
                if (tux != null && tux.Type == Base.Card.Tux.TuxType.ZP)
                {
                    if (g0cc[3] == player.Uid.ToString())
                        return true;
                }
            }
            return false;
        }
        public string JNT1801Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<ushort> cands = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type == Base.Card.Tux.TuxType.ZP).ToList();
                if (cands.Count > 0)
                    return "/Q1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public void JNT1802Action(Player player, int type, string fuse, string argst)
        {
            int cmidx = argst.IndexOf(',');
            ushort ut = ushort.Parse(argst.Substring(0, cmidx));
            ushort sel = ushort.Parse(argst.Substring(cmidx + 1));
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + ut);
            List<string> g0dh = new List<string>();
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.IsAlive)
                {
                    Hero hro = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                    int n = hro.Spouses.Where(p => !p.StartsWith("!")).Count();
                    if (n > 0)
                    {
                        if (sel == 1)
                            g0dh.Add(py.Uid + ",0," + n);
                        else if (sel == 2)
                            g0dh.Add(py.Uid + ",1," + n);
                    }
                }
            }
            if (g0dh.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", g0dh));
        }
        public bool JNT1802Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public string JNT1802Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#请选择「情殇」执行项##补牌##弃牌,/Y2";
            else
                return "";
        }
        #endregion TR018 - Qinji

        #region TR019 - JingTian
        public bool JNT1901Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
                return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
            else
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Tux.Count > 0);
        }
        public void JNT1901Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
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
                    XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0," + string.Join(",", ot1));
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
        public string JNT1901Input(Player player, int type, string fuse, string prev)
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
        public bool JNT1902Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNT1902", player, fuse);
            else if (type == 1)
            { // G0ON
                if (!player.ROM.ContainsKey("Weapon"))
                    return false;
                string[] g0on = fuse.Split(',');
                for (int idx = 1; idx < g0on.Length; )
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
        public void JNT1902Action(Player player, int type, string fuse, string argst)
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
                for (int idx = 1; idx < g0on.Length; )
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
        public bool JNT1903Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Team == XI.Board.Rounder.Team && XI.Board.IsBattleWin
                     && XI.Board.PoolDelta > 0 && player.Tux.Count >= 2;
            else if (type == 1)
                return player.TokenCount > 0;
            else
                return false;
        }
        public void JNT1903Action(Player player, int type, string fuse, string argst)
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
        public string JNT1903Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "#弃置的,/Q2(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        #endregion TR019 - JingTian
        #region TR020 - LeiYuan'ge
        public bool JNT2001Valid(Player player, int type, string fuse)
        {
            if (type == 0) // Selection
                return player.ROMUshort < 7;
            else if (type == 1) // Remove the current at the starting round stage
                return player.Guardian != 0;
            else if (type == 2) // Debut
                return IsMathISOS("JNT2001", player, fuse);
            return false;
        }
        public void JNT2001Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort iNo = ushort.Parse(argst);
                if (iNo == 6)
                {
                    player.ROMUshort += 1;
                    XI.RaiseGMessage("G0MA," + player.Uid + ",1");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNT2003");
                }
                else if (iNo == 7)
                {
                    player.ROMUshort += 2;
                    XI.RaiseGMessage("G0MA," + player.Uid + ",2");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNT2004");
                }
                else if (iNo == 8)
                {
                    player.ROMUshort += 4;
                    XI.RaiseGMessage("G0MA," + player.Uid + ",3");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNT2005");
                }
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + iNo);
            }
            else if (type == 1)
            {
                ushort iNo = player.Guardian;
                XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                if (iNo == 1)
                    XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNT2003");
                else if (iNo == 2)
                    XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNT2004");
                else if (iNo == 3)
                    XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNT2005");
            }
            else if (type == 2)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,3,I6,I7,I8");
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,I6,I7,I8");
            }
        }
        public string JNT2001Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<string> cands = new List<string>();
                if ((player.ROMUshort & (0x1)) == 0)
                    cands.Add("I6");
                if ((player.ROMUshort & (0x2)) == 0)
                    cands.Add("I7");
                if ((player.ROMUshort & (0x4)) == 0)
                    cands.Add("I8");
                return "I1(p" + string.Join("p", cands) + ")";
            }
            else
                return "";
        }
        public bool JNT2002Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.RestNPCPiles.Count > 0 && player.Tux.Count >= 2;
            string[] g0xzs = fuse.Split(',');
            return b1 && g0xzs[2] == "2";
        }
        public void JNT2002Action(Player player, int type, string fuse, string argst)
        {
            ushort[] dist = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + dist[0] + "," + dist[1]);
            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
            if (dist[2] == 1) // Put ahead
            {
                XI.Board.MonPiles.PushBack(pop);
                XI.RaiseGMessage("G0YM,6,0,0");
            }
            else // Put at next position
            {
                List<ushort> list = new List<ushort>();
                ushort first = XI.Board.MonPiles.Dequeue();
                list.Add(first); list.Add(pop);
                XI.Board.MonPiles.PushBack(list);
                XI.RaiseGMessage("G0YM,6,1,0");
            }
        }
        public string JNT2002Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#弃置的,/Q2(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',', prev.IndexOf(',') + 1) < 0)
            {
                if (XI.Board.MonPiles.Count > 0)
                    return "#请选择新NPC放置位置##牌堆顶##堆顶下一张,Y2";
                else
                    return "#请选择新NPC放置位置##牌堆顶,Y1";
            }
            else
                return "";
        }
        public bool JNT2003Valid(Player player, int type, string fuse)
        {
            return IsMathISOS("JNT2003", player, fuse);
        }
        public void JNT2003Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,3");
            else if (type == 1)
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,3");
        }
        public bool JNT2004Valid(Player player, int type, string fuse)
        {
            return IsMathISOS("JNT2004", player, fuse);
        }
        public void JNT2004Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IX," + player.Uid + ",0,3");
            else if (type == 1)
                XI.RaiseGMessage("G0OX," + player.Uid + ",0,3");
        }
        public bool JNT2005Valid(Player player, int type, string fuse)
        {
            return true;
        }
        public void JNT2005Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        #endregion TR020 - LeiYuan'ge
        #region TR021 - LongMing
        public bool JNT2101Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(
                p => p.IsTared && p.Team == player.OppTeam && !player.RAMUtList.Contains(p.Uid));
        }
        public void JNT2101Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort ut = ushort.Parse(argst.Substring(0, idx));
            ushort tar = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0HQ,0," + tar + "," + player.Uid + ",0,1," + ut);
            Tux tux = XI.LibTuple.TL.DecodeTux(ut);
            IDictionary<Tux.TuxType, string> namae = new Dictionary<Tux.TuxType, string>();
            namae[Tux.TuxType.JP] = "技牌";
            namae[Tux.TuxType.TP] = "特殊牌";
            namae[Tux.TuxType.ZP] = "战牌";
            namae[Tux.TuxType.WQ] = namae[Tux.TuxType.FJ] = namae[Tux.TuxType.XB] = "装备牌";

            string choose = XI.AsyncInput(tar, "#请选择「弈局」执行项目。##弃非"
                + namae[tux.Type] + "##对方补3牌,Y2", "JNT2101", "0");
            if (choose == "2")
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,3");
            else
            {
                XI.RaiseGMessage("G2FU,0,0,0," + string.Join(",", XI.Board.Garden[tar].Tux));
                List<ushort> tuxes = XI.Board.Garden[tar].Tux.Where(p =>
                    namae[XI.LibTuple.TL.DecodeTux(p).Type] != namae[tux.Type]).ToList();
                if (tuxes.Count > 0)
                    XI.RaiseGMessage("G0QZ," + tar + "," + string.Join(",", tuxes));
            }
            player.RAMUtList.Add(tar);
        }
        public string JNT2101Input(Player player, int type, string fuse, string input)
        {
            if (input == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1(p" + string.Join("p",
                    XI.Board.Garden.Values.Where(p => p.IsTared && p.Team == player.OppTeam &&
                     !player.RAMUtList.Contains(p.Uid)).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNT2102Valid(Player player, int type, string fuse)
        {
            return XI.Board.Rounder.Team == player.Team && XI.Board.IsAttendWar(player)
                && XI.Board.Hinder.Uid > 0 && XI.Board.Hinder.Uid < 1000;
        }
        public void JNT2102Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OX," + XI.Board.Hinder.Uid + ",1,4");
        }
        #endregion TR021 - LongMing
        #region TR022 - XiaChulin
        public bool JNT2201Valid(Player player, int type, string fuse)
        {
            if (type == 0) // Hold
                return !XI.Board.InFight && player.RAMUshort == 0;
            else if (type == 1) // start, release Hold and increase
                return XI.Board.IsAttendWar(player) && player.RAMUshort == 1;
            else if (type == 2) // in battle and watched, increase directly
                return XI.Board.InFight && XI.Board.IsAttendWar(player) && player.RAMUshort == 0;
            else if (type == 3) // in battle and attender change
            {
                if (player.RAMUshort == 1 && XI.Board.InFight)
                {
                    string[] e0fi = fuse.Split(',');
                    for (int i = 1; i < e0fi.Length; i += 3)
                    {
                        char c = e0fi[i][0];
                        if ((c == 'S' || c == 'H' || c == 'W') && e0fi[i + 1] == player.Uid.ToString())
                            return true;
                    }
                }
                return false;
            }
            else if (type == 4)
                return player.RAMUshort == 2;
            else
                return false;
        }
        public void JNT2201Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                player.RAMUshort = 1;
            else if (type == 1)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
                player.RAMUshort = 2;
            }
            else if (type == 2 || type == 3)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
                player.RAMUshort = 2;
            }
            else if (type == 4)
            {
                player.RAMUshort = 1;
                Harm(player, player, 1);
            }
        }
        public bool JNT2202Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.OppTeam
                && p.ListOutAllEquips().Count > 0);
        }
        public void JNT2202Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = argst.Split(',');
            bool eqed = (blocks.Length == 4 && blocks[3] == "1");
            ushort who = ushort.Parse(blocks[0]);
            ushort eq = ushort.Parse(blocks[1]);
            ushort to = ushort.Parse(blocks[2]);
            if (!eqed)
                XI.RaiseGMessage("G0HQ,0," + to + "," + who + ",0,1," + eq);
            else
                XI.RaiseGMessage("G0ZB," + to + ",1," + player.Uid + ",0," + who + "," + eq);
            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
            XI.RaiseGMessage("G0YM,3," + pop + ",0");
            XI.RaiseGMessage("G0DH," + player.Uid + ",3");
            List<ushort> pets = player.Pets.Where(p => p != 0).ToList();
            foreach (ushort ut in pets)
                XI.RaiseGMessage("G0HL," + player.Uid + "," + ut);
            if (pets.Count > 0)
                XI.RaiseGMessage("G0ON," + player.Uid + ",M," + pets.Count + "," + string.Join(",", pets));
            XI.AsyncInput(player.Uid, "//", "JNT2202", "0");
            //XI.Board.RestNPCDises.Add(pop);
            XI.RaiseGMessage("G0ON,0,M,1," + pop);
            XI.RaiseGMessage("G0YM,3,0,0");
            if (npc != null && Artiad.ContentRule.IsNPCJoinable(npc, XI.LibTuple.HL, XI.Board))
            {
                XI.RaiseGMessage("G0OY,0," + player.Uid);
                XI.RaiseGMessage("G0IY,2," + player.Uid + "," + npc.Hero + ",3");
            }
            else
                XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        public string JNT2202Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Team == player.OppTeam && p.ListOutAllEquips().Count > 0).Select(p => p.Uid)) + ")";
            else
            {
                string[] blocks = prev.Split(',');
                if (blocks.Length == 1)
                {
                    ushort ut = ushort.Parse(prev);
                    return "/C1(p" + string.Join("p", XI.Board.Garden[ut].ListOutAllEquips()) + ")";
                }
                else if (blocks.Length == 2)
                {
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                        p.Team == player.Team).Select(p => p.Uid)) + ")";
                }
                else if (blocks.Length == 3)
                {
                    ushort tx = ushort.Parse(blocks[1]);
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(tx);
                    if (tux.IsTuxEqiup())
                        return "#请选择是否直接装备##是##否,Y2";
                    else
                        return "";
                }
                else
                    return "";
            }
        }
        #endregion TR022 - XiaChulin
        #region TR024 - Chanyou
        public bool JNT2401Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNT2401")
                            return true;
                }
                return false;
            }
            else if (type == 1)
                return true;
            else
                return false;
        }
        public void JNT2401Action(Player player, int type, string fuse, string prev)
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
        public bool JNT2402Valid(Player player, int type, string fuse)
        {
            return player.TokenExcl.Count > 0;
        }
        public void JNT2402Action(Player player, int type, string fuse, string argst)
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
        public string JNT2402Input(Player player, int type, string fuse, string prev)
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
        public bool JNT2403Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0);
        }
        public void JNT2403Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            Player py = XI.Board.Garden[tar];
            bool survive = false;
            if (py.Tux.Count > 0)
            {
                string sel1 = XI.AsyncInput(tar, "#请选择是否立即阵亡.##否##是,Y2", "JNT2403", "0");
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
                XI.AsyncInput(player.Uid, "#作为「幻」的,C1(" + c0 + ")", "JNT2403", "1");
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
        public string JNT2403Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).ToList();
                return "/T1(p" + string.Join("p", invs.Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        #endregion TR024 - Chanyou
        #region TR025 - Liaori
        public bool JNT2501Valid(Player player, int type, string fuse)
        {
            if (type == 0) {
                return player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP) &&
                    XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0 &&
                    XI.Board.Garden.Values.Where(q => q.IsTared && q.Team == p.Team && p.Uid != q.Uid).Any()).Any();
            }
            else if (type == 1) {
                if (XI.Board.Monster1 != 0)
                    return false;
                if (!player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP))
                    return false;
                IDictionary<int, int> dicts = XI.CalculatePetsScore();
                List<Player> targets = XI.Board.Garden.Values.Where(
                        p => p.IsTared && p.Team == player.OppTeam && p.GetPetCount() > 0)
                        .Where(p => !XI.Board.PetProtecedPlayer.Contains(p.Uid)).ToList();
                if (!targets.Any())
                    return false;
                if (dicts[player.Team] <= dicts[player.OppTeam])
                    return true;
            }
            return true;
        }
        public void JNT2501Action(Player player, int type, string fuse, string argv)
        {
            ushort[] uts = argv.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + uts[0]);
            if (type == 0)
            {
                XI.RaiseGMessage("G0HC,1," + uts[2] + "," + uts[1] + ",0," + uts[3]);
            }
            else if (type == 1)
            {
                ushort who = uts[1], mon = uts[2];
                XI.Board.Mon1From = who;
                XI.Board.Monster1 = mon;
                XI.RaiseGMessage("G0YM,0," + mon + "," + who);
                XI.Board.AllowNoSupport = false;
            }
        }
        public string JNT2501Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                var tl = XI.LibTuple.TL;
                if (prev == "")
                    return "#弃置的,/Q1(p" + string.Join("p", player.Tux.Where(p => tl.DecodeTux(p).Type == Tux.TuxType.JP)) + ")";
                else if (prev.IndexOf(',') < 0)
                    return "#交出宠物的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0 &&
                    XI.Board.Garden.Values.Where(q => q.IsTared && q.Team == p.Team && p.Uid != q.Uid).Any()).Select(p => p.Uid)) + ")";
                else if (prev.IndexOf(',', prev.IndexOf(',') + 1) < 0)
                {
                    Player from = XI.Board.Garden[ushort.Parse(prev.Substring(prev.IndexOf(',') + 1))];
                    return "#交予宠物的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                            p => p.IsTared && p.Uid != from.Uid && p.Team == from.Team).Select(p => p.Uid))
                            + "),/M1(p" + string.Join("p", from.Pets.Where(p => p != 0)) + ")";
                }
                else
                    return "";
            }
            else if (type == 1)
            {
                var tl = XI.LibTuple.TL;
                if (prev == "")
                    return "#弃置的,/Q1(p" + string.Join("p", player.Tux.Where(p => tl.DecodeTux(p).Type == Tux.TuxType.TP)) + ")";
                else if (prev.IndexOf(',') < 0) {
                    List<ushort> targets = XI.Board.Garden.Values.Where(
                        p => p.IsTared && p.Team == player.OppTeam && p.GetPetCount() > 0)
                        .Select(p => p.Uid).Except(XI.Board.PetProtecedPlayer).ToList();
                    return "#夺宠角色,/T1(p" + string.Join("p", targets) + ")";
                }
                else if (prev.IndexOf(',', prev.IndexOf(',') + 1) < 0)
                {
                    ushort who = ushort.Parse(prev.Substring(prev.IndexOf(',') + 1));
                    return "/M1(p" + string.Join("p", XI.Board.Garden[who].Pets.Where(p => p != 0)) + ")";
                }
                else
                    return "";
            }
            else
                return "";
        }
        public bool JNT2502Valid(Player player, int type, string fuse)
        {
            string[] g0xzs = fuse.Split(',');
            return g0xzs[2] == "2";
        }
        public void JNT2502Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        #endregion TR025 - Liaori
        #region TR027 - Qianye
        public bool JNT2701Valid(Player player, int type, string fuse)
        {
            if (type == 0) // Z1
                return XI.Board.IsAttendWar(player) && XI.Board.Battler != null;
            else if (type == 1 && XI.Board.InFight) // FI
            {
                string[] g0fi = fuse.Split(',');
                int zero = 0;
                for (int i = 1; i < g0fi.Length; i += 3)
                {
                    char ch = g0fi[i][0];
                    ushort od = ushort.Parse(g0fi[i + 1]);
                    ushort nw = ushort.Parse(g0fi[i + 2]);
                    if (g0fi[i] == "S" || g0fi[i] == "H")
                    {
                        if (od == player.Uid)
                            --zero;
                        if (nw == player.Uid)
                            ++zero;
                    }
                }
                return zero != 0 && (player.DEX > XI.Board.Battler.AGL);
            }
            else if (type >= 2 && type <= 5) // I/OX,I/OW
            {
                if (XI.Board.IsAttendWar(player) && XI.Board.Battler != null)
                {
                    int now = player.DEX - XI.Board.Battler.AGL;
                    if (now < 0)
                        now = 0;
                    return now != player.RAMUshort;
                }
                return false;
            }
            else
                return false;
        }
        public void JNT2701Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int delta = player.DEX - XI.Board.Battler.AGL;
                player.RAMUshort = (delta < 0) ? (ushort)0 : (ushort)delta;
                if (player.RAMUshort > 0)
                    XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + "," + player.RAMUshort);
            }
            else if (type == 1)
            {
                if (XI.Board.IsAttendWar(player))
                {
                    int delta = player.DEX - XI.Board.Battler.AGL;
                    player.RAMUshort = (delta < 0) ? (ushort)0 : (ushort)delta;
                    if (player.RAMUshort > 0)
                        XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + "," + player.RAMUshort);
                }
                else
                {
                    if (player.RAMUshort > 0)
                        XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + "," + player.RAMUshort);
                    player.RAMUshort = 0;
                }
            }
            else if (type >= 2 && type <= 5)
            {
                int now = player.DEX - XI.Board.Battler.AGL;
                if (now < 0) now = 0;
                int delta = now - player.RAMUshort;
                if (delta < 0)
                    XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + "," + (-delta));
                else if (delta > 0)
                    XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + "," + delta);
                player.RAMUshort = (ushort)now;
            }
        }
        public bool JNT2702Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1) // ZB/OT
                return player.GetBaseEquipCount() != player.ROMUshort;
            else if (type == 2 || type == 3) // IS/OS
                return IsMathISOS("JNT2702", player, fuse) && player.GetBaseEquipCount() > 0;
            else if (type == 4) // DH
            {
                Player r = XI.Board.Rounder;
                return r.Uid == player.Uid && r.GetEquipCount() >= 2
                     && XI.Board.RoundIN == "R" + r.Uid + "BC";
            }
            else
                return false;
        }
        public void JNT2702Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                int now = player.GetBaseEquipCount();
                int delta = now - player.ROMUshort;
                player.ROMUshort = (ushort)now;
                player.TuxLimit += delta;
            }
            else if (type == 2)
                player.TuxLimit += (player.ROMUshort = (ushort)player.GetBaseEquipCount());
            else if (type == 3)
            {
                player.TuxLimit -= player.GetBaseEquipCount();
                player.ROMUshort = 0;
            }
            else if (type == 4)
            {
                string[] blocks = fuse.Split(',');
                string g0dh = "";
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort ut = ushort.Parse(blocks[i]);
                    int gtype = int.Parse(blocks[i + 1]);
                    int n = int.Parse(blocks[i + 2]);
                    if (ut == XI.Board.Rounder.Uid && gtype == 0 && n > 0)
                        g0dh += "," + ut + ",0," + (n + 1);
                    else
                        g0dh += "," + ut + "," + gtype + "," + n;
                }
                if (g0dh.Length > 0)
                    XI.InnerGMessage("G0DH" + g0dh, 56);
            }
        }
        #endregion TR027 - Qianye
        #region TR028 - Wuhou
        public bool JNT2801Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            if (blocks[1] == "0")
                return true;
            else if (blocks[1] == "1")
            {
                ushort who = ushort.Parse(blocks[2]);
                ushort where = ushort.Parse(blocks[3]);
                return !(where != 0 && XI.Board.Garden[who].Team == XI.Board.Garden[where].Team);
            }
            return false;
        }
        public void JNT2801Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#获得1张补牌,T1(p" + string.Join(
                "p", XI.Board.Garden.Values.Where(p => p.IsTared && p.Team == player.Team)
                .Select(p => p.Uid)) + ")", "JNT2801", "0");
            ushort who = ushort.Parse(input);
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,1");
        }
        public bool JNT2802Valid(Player player, int type, string fuse)
        {
            if (type == 0)
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
            else if (type == 1)
            {
                bool death = false;
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; ++i)
                    if (blocks[i] == player.Uid.ToString())
                        if (XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.Team == player.Team && p.IsTared))
                        {
                            death = true; break;
                        }
                if (death)
                {
                    //ushort cardId = XI.LibTuple.TL.EncodeTuxCode("WQ02").DBSerial;
                    ushort cardId = 48;
                    if (XI.Board.TuxDises.Contains(cardId))
                        return true;
                    foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsTared))
                    {
                        foreach (ushort eq in py.ListOutAllEquips())
                        {
                            if (eq == cardId)
                                return true;
                            Tux tux = XI.LibTuple.TL.DecodeTux(eq);
                            if (tux.IsTuxEqiup())
                            {
                                TuxEqiup tue = tux as TuxEqiup;
                                if (tue.IsLuggage())
                                {
                                    Luggage lg = tue as Luggage;
                                    if (lg.Capacities.Contains("C" + cardId))
                                        return true;
                                }
                            }
                        }
                        if (py.TokenExcl.Contains("C" + cardId))
                            return true;
                    }
                }
            }
            return false;
        }
        public void JNT2802Action(Player player, int type, string fuse, string argv)
        {
            if (type == 0)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                foreach (Artiad.Cure cure in cures)
                {
                    if (cure.Who == player.Uid &&
                            cure.Element != FiveElement.SOL && cure.Element != FiveElement.LOVE)
                    {
                        ++cure.N;
                    }
                }
                if (cures.Count > 0)
                    XI.InnerGMessage(Artiad.Cure.ToMessage(cures), 16);
            }
            else if (type == 1)
            {
                string target = XI.AsyncInput(player.Uid, "#获得【天蛇杖】的,/T1" + ATeammatesTared(player), "JNT2802", "1");
                if (target.StartsWith("/")) return;
                ushort to = ushort.Parse(target);
                //ushort cardId = XI.LibTuple.TL.EncodeTuxCode("WQ02").DBSerial;
                ushort cardId = 48;
                if (XI.Board.TuxDises.Contains(cardId))
                    XI.RaiseGMessage("G0HQ,2," + to + ",0,0," + cardId);
                else
                {
                    foreach (Player py in XI.Board.Garden.Values.Where(p => p.IsTared))
                    {
                        foreach (ushort eq in py.ListOutAllEquips())
                        {
                            if (eq == cardId)
                                XI.RaiseGMessage("G0HQ,0," + to + "," + py.Uid + ",0,1," + cardId);
                            Tux tux = XI.LibTuple.TL.DecodeTux(eq);
                            if (tux.IsTuxEqiup())
                            {
                                TuxEqiup tue = tux as TuxEqiup;
                                if (tue.IsLuggage())
                                {
                                    Luggage lg = tue as Luggage;
                                    if (lg.Capacities.Contains("C" + cardId))
                                    {
                                        XI.RaiseGMessage("G0SN," + py.Uid + "," + eq + ",1,C" + cardId);
                                        XI.RaiseGMessage("G0HQ,3," + to + "," + py.Uid + ",1," + cardId);
                                    }
                                }
                            }
                        }
                        if (py.TokenExcl.Contains("C" + cardId))
                        {
                            XI.RaiseGMessage("G0OJ," + py.Uid + ",1,1,C" + cardId);
                            XI.RaiseGMessage("G0HQ,3," + to + "," + py.Uid + ",1," + cardId);
                        }
                    }
                }
                string os = XI.AsyncInput(to, "#您是否要立即装备？##是##否,Y2", "JNT2802", "0");
                if (os == "1")
                    XI.RaiseGMessage("G0ZB," + to + ",0," + cardId);
            }
        }
        #endregion

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
            if (cnt + player.ROMUshort > 8)
                cnt = (ushort)(8 - player.ROMUshort);
            if (cnt > 0)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",0," + cnt);
                player.ROMUshort += cnt;
            }
        }
        public bool JNH0101Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && player.Tux.Count > 0;
        }
        public void JNH0102Action(Player player, int type, string fuse, string argst)
        {
            int cnt = player.ROMUshort;
            XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
            XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + cnt);
            player.ROMUshort = 0;
            XI.RaiseGMessage("G0IP," + player.Team + "," + (cnt + 2));
        }
        public bool JNH0102Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.IsAttendWarSucc(player);
            var mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            var mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            bool b2 = mon1 != null && mon1.Level == Monster.ClLevel.BOSS;
            bool b3 = mon2 != null && mon2.Level == Monster.ClLevel.BOSS;
            bool b4 = player.ROMUshort >= 2;
            return b1 && (b2 || b3) && b4;
        }
        public void JNH0103Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DS," + player.Uid + ",1");
        }
        public bool JNH0103Valid(Player player, int type, string fuse)
        {
            if (player.Immobilized)
            {
                string[] g0ds = fuse.Split(',');
                for (int i = 1; i < g0ds.Length; )
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
                if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                    return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
            }
            return false;
        }
        public void JNH0301Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            Artiad.Harm thisHarm = null, thatHarm = null;
            ushort to = ushort.Parse(argst);
            int mValue = 0; bool action = false;
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE && harm.Element != FiveElement.SOL)
                {
                    thisHarm = harm;
                    XI.RaiseGMessage("G0TT," + to);
                    int value = XI.Board.DiceValue;
                    int m = value / 2;
                    if (harm.N > m)
                    {
                        string format = player.Tux.Count < (harm.N - m) ? "/" :
                            "/Q" + (harm.N - m) + "(p" + string.Join("p", player.Tux) + ")";
                        string input = XI.AsyncInput(player.Uid, format, "JNH0301", "0");
                        if (!input.StartsWith("/"))
                        {
                            List<ushort> uts = input.Split(',').Select(p => ushort.Parse(p)).ToList();
                            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", uts));
                            mValue = m; action = true;
                        }
                        else { mValue = 0; action = false; }
                    }
                    else { mValue = m; action = true; }
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
            if (XI.Board.Garden[to].IsAlive && mValue > 0)
                XI.RaiseGMessage("G0DH," + to + ",0," + mValue);
        }
        public string JNH0301Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1" + AOthersTared(player);
            else
                return "";
        }
        public void JNH0302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                if (player.ROMUshort == 0)
                {
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
                    player.ROMUshort = 1;
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
                }
                else if (player.ROMUshort == 1)
                {
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
                    player.ROMUshort = 0;
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
                }
            }
            else if (type == 1)
                XI.RaiseGMessage("G0HR,0,1");
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
            return false;
        }
        public void JNH0303Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0TT," + player.Uid);
            if (XI.Board.DiceValue != 6)
            {
                int total = player.GetPetCount() + player.ListOutAllCards().Count;
                string range = Util.SSelect(XI.Board, p => p != player && p.IsTared);
                string target = XI.AsyncInput(player.Uid, "#HP-" + 0 + ",T1" + range, "JNH0303", "0");
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
                string tuxType = Util.Substring(fuse, idx + 1, fuse.IndexOf(',', idx + 1));
                ushort who = ushort.Parse(g0ce[2]);
                Player py = XI.Board.Garden[who];
                if (py != null && py.Team == player.Team)
                    return true;
            }
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
                for (int jdx = 1; jdx < blocks.Length; )
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
                    if (blocks[1] == "0" && blocks[3] == player.Uid.ToString())
                    {
                        ushort to = ushort.Parse(blocks[2]);
                        int n = int.Parse(blocks[4]);
                        return XI.Board.Garden[to].Team == player.OppTeam && n > 0;
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
                for (int jdx = 1; jdx < blocks.Length; )
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
                    return "Q1(p" + string.Join("p", py.ListOutAllCards()) + ")";
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
                for (int idx = 1; idx < blocks.Length; )
                {
                    string fromZone = blocks[idx];
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
                for (int idx = 1; idx < blocks.Length; )
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
                int rest = npc.STR;
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
                for (int idx = 1; idx < blocks.Length; )
                {
                    string fromZone = blocks[idx];
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
            if (XI.Board.Garden.Values.Where(p => p.IsTared && p.Team == player.Team &&
                !p.Immobilized).Select(p => p.Uid).Any() && player.RAMUshort == 0)
            {
                string[] g1ev = fuse.Split(',');
                Player trigger = XI.Board.Garden[ushort.Parse(g1ev[1])];
                return trigger != null && trigger.Team == player.OppTeam;
            }
            else
                return false;
        }
        public void JNH0702Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            XI.RaiseGMessage("G0DS," + ut + ",0,1");
            player.RAMUshort = 1;

            XI.RaiseGMessage("G0ON,10,E,1," + XI.Board.Eve);
            XI.RaiseGMessage("G0YM,2,0,0");
            XI.Board.Eve = 0;

            string[] g1ev = fuse.Split(',');
            XI.RaiseGMessage("G1EV," + g1ev[1] + "," + g1ev[2]);
        }
        public string JNH0702Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#横置,/T1(p" + string.Join("p", XI.Board.Garden.Values
                    .Where(p => p.IsTared && p.Team == player.Team && !p.Immobilized)
                    .Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNH0703Valid(Player player, int type, string fuse)
        {
            string[] g1di = fuse.Split(',');
            for (int idx = 1; idx < g1di.Length; )
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
        public void JNH0703Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = argst.Split(',');
            string[] g1di = fuse.Split(',');
            string n1di = "";
            List<ushort> rest = new List<ushort>();
            List<ushort> eqs = new List<ushort>();
            ushort tar = ushort.Parse(blocks[0]);
            ushort ut = ushort.Parse(blocks[1]);
            for (int idx = 1; idx < g1di.Length; )
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
            if (blocks.Length >= 3)
                XI.RaiseGMessage("G0HQ,0," + tar + "," + player.Uid + ",1,1," + blocks[2]);
            //XI.RaiseGMessage("G2CN,0,1");
            //XI.Board.TuxDises.Remove(ut);
            XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
            rest.Remove(ut); eqs.Remove(ut);
            rest.AddRange(eqs);
            if (rest.Count > 0)
                n1di += "," + tar + ",1," + rest.Count + "," + (rest.Count - eqs.Count) + "," + string.Join(",", rest);
            if (n1di.Length > 0)
                XI.InnerGMessage("G1DI" + n1di, 31);
        }
        public string JNH0703Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                ISet<ushort> invs = new HashSet<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length; )
                {
                    ushort who = ushort.Parse(g1di[idx]);
                    bool drIn = g1di[idx + 1] == "0";
                    int n = int.Parse(g1di[idx + 2]);
                    if (who != player.Uid && XI.Board.Garden[who].IsTared && !drIn && n >= 2)
                        invs.Add(who);
                    idx += (4 + n);
                }
                return "/T1(p" + string.Join("p", invs) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort ut = ushort.Parse(prev);
                List<ushort> tuxes = new List<ushort>();
                string[] g1di = fuse.Split(',');
                for (int idx = 1; idx < g1di.Length; )
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
                string p1 = "/C1(p" + string.Join("p", tuxes) + ")";
                if (XI.Board.Garden[ut].Team == player.OppTeam && player.Tux.Count > 0)
                    p1 += ",#交予对方的,/Q1(p" + string.Join("p", player.Tux) + ")";
                return p1;
            }
            else
                return "";
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
                for (int idx = 1; idx < g1di.Length; )
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
                for (int idx = 1; idx < g1di.Length; )
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
                    .Any(p => p.IsAlive && p.Team == player.Team && p.GetActionPetCount(XI.Board) > 0);
            else if (type == 4)
            {
                Player r = XI.Board.Rounder;
                return r.Team == player.Team && r.GetPetCount() > 0 && r.Tux.Count == 0
                     && XI.Board.RoundIN == "R" + r.Uid + "BC";
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
                        int count = py.GetActionPetCount(XI.Board);
                        py.TuxLimit += count;
                    }
            }
            else if (type == 3)
            {
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.Team)
                    {
                        int count = py.GetActionPetCount(XI.Board);
                        py.TuxLimit -= count;
                    }
            }
            else if (type == 4)
            {
                string[] blocks = fuse.Split(',');
                string g0dh = "";
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort ut = ushort.Parse(blocks[i]);
                    int gtype = int.Parse(blocks[i + 1]);
                    int n = int.Parse(blocks[i + 2]);
                    if (ut == XI.Board.Rounder.Uid && gtype == 0 && n > 0)
                    {
                        int petCount = XI.Board.Garden[ut].GetPetCount();
                        g0dh += "," + ut + ",0," + (n + petCount);
                    }
                    else
                        g0dh += "," + ut + "," + gtype + "," + n;
                }
                if (g0dh.Length > 0)
                    XI.InnerGMessage("G0DH" + g0dh, 46);
            }
        }
        public bool JNH1002Valid(Player player, int type, string fuse)
        {
            return player.IsAlive && player.HP == 0 && XI.Board.Garden.Values.Any(
                p => p.Uid != player.Uid && p.IsTared);
        }
        public void JNH1002Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            ushort mon = XI.LibTuple.ML.Encode("GSH2");
            if (mon != 0)
            {
                XI.RaiseGMessage("G0HC,1," + tar + ",0,1," + mon);
                XI.RaiseGMessage("G0ZW," + player.Uid);
            }
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", -50);
        }
        public string JNH1002Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1" + AAllTareds(player);
            else
                return "";
        }
        #endregion HL010 - ShuiLingjing
        #region HL011 - ShuiGang
        public bool JNH1101Valid(Player player, int type, string fuse)
        {
            if (XI.Board.IsAttendWar(player))
            {
                bool b1 = XI.Board.Garden.Values.Any(p => p.IsAlive
                    && XI.Board.IsAttendWar(p) && !XI.Board.IsAttendWarSucc(p));
                bool b2 = !XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && XI.Board.IsAttendWar(p))
                    && XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && p.IsTared && p.Tux.Count > 0);
                return b1 || b2;
            }
            else
                return false;
        }
        public void JNH1101Action(Player player, int type, string fuse, string argst)
        {
            List<Player> losses = XI.Board.Garden.Values.Where(p => p.IsAlive
                    && XI.Board.IsAttendWar(p) && !XI.Board.IsAttendWarSucc(p)).ToList();
            List<ushort> qzs = losses.Where(p => p.Tux.Count > 0).Select(p => p.Uid).ToList();
            List<Player> hms = losses.Where(p => p.Tux.Count == 0).ToList();
            if (qzs.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", qzs.Select(p => p + ",1,1")));
            if (hms.Count > 0)
                Harm(player, hms, 2);
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
                if (origin.StartsWith("G") && g0ce[2] != "2") // Avoid Double Computation on Copy
                {
                    string cardname = g0ce[4];
                    int inType = int.Parse(Util.Substring(fuse, hdx + 1, kdx));
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    int prior = tux.Priorities[inType];
                    XI.InnerGMessage(origin, prior);
                }
                Harm(player, player, 1);
            } else if (type == 2) {
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
                XI.RaiseGMessage("G0YM,5," + pop + ",0");
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
            string input = XI.AsyncInput(player.Uid, "#获得2张补牌,T1(p" + string.Join(
                "p", XI.Board.Garden.Values.Where(p => p.IsTared && p.Team == player.Team)
                .Select(p => p.Uid)) + ")", "JNH1302", "0");
            ushort who = ushort.Parse(input);
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,2");
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
            if (type == 0 || type == 1)
                return player.TokenExcl.Count > 0 || player.Guardian != 0;
            else if (type == 2)
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

            if (type == 0 || type == 1)
            {
                ushort iNo = player.Guardian;
                if (iNo != 0)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                    if (jm.ContainsKey(iNo))
                        XI.RaiseGMessage("G0OS," + player.Uid + ",1," + isk[jm[iNo]]);
                }
                if (player.TokenExcl.Count > 0)
                {
                    string input = XI.AsyncInput(player.Uid, "I1(p" +
                        string.Join("p", player.TokenExcl) + ")", "JNH1601", "0");
                    iNo = ushort.Parse(input);
                    if (im.ContainsKey(iNo))
                    {
                        XI.RaiseGMessage("G0MA," + player.Uid + "," + im[iNo]);
                        XI.RaiseGMessage("G0IS," + player.Uid + ",1," + isk[iNo]);
                    }
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + iNo);
                }
            }
            else if (type == 2)
            {
                string part = string.Join(",", im.Keys.Select(p => "I" + p));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + im.Keys.Count + "," + part);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + part);
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
                string input = XI.AsyncInput(player.Uid, "#弃1张手牌(取消则您补1牌),/T1(p" +
                     string.Join("p", normals.Select(p => p.Uid)) + ")", "JNH1702", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    Player tpy = XI.Board.Garden[ushort.Parse(input)];
                    if (tpy.Uid != player.Uid)
                    {
                        XI.AsyncInput(player.Uid, "#弃置的,C1(" +
                            Util.RepeatString("p0", tpy.Tux.Count) + ")", "JNH1702", "1");
                        XI.RaiseGMessage("G0DH," + input + ",2,1");
                    }
                    else
                    {
                        string mytux = XI.AsyncInput(player.Uid, "#弃置的,Q1(p" +
                            string.Join("p", player.Tux) + ")", "JNH1702", "1");
                        XI.RaiseGMessage("G0QZ," + player.Uid + "," + mytux);
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
            bool meWin = (player.Team == XI.Board.Rounder.Team && XI.Board.IsBattleWin);
            if (meWin && XI.Board.Hinder.IsTared)
                return XI.Board.PoolDelta >= 2;
            else
                return false;
        }
        public void JNH1801Action(Player player, int type, string fuse, string argst)
        {
            string hSel = XI.AsyncInput(player.Uid, "#进行「凝元劲」选择,T1(p" +
                XI.Board.Hinder.Uid + ")", "JNH1801", "0");
            ushort otar = ushort.Parse(hSel);
            int diff = XI.Board.PoolDelta / 2;
            string oSel = XI.AsyncInput(otar, "#请选择HP-" + diff +
                "或对方HP+2##HP-" + diff + "##对方HP+2,Y2", "JNH1801", "1");
            if (oSel != "2")
                Harm(player, XI.Board.Garden[otar], diff);
            else
            {
                string rSel = XI.AsyncInput(player.Uid, "#HP+2,T1" +
                    ATeammatesTared(player), "JNH1801", "2");
                ushort rtar = ushort.Parse(rSel);
                Cure(player, XI.Board.Garden[rtar], 2);
            }
        }
        public bool JNH1802Valid(Player player, int type, string fuse)
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
        public void JNH1802Action(Player player, int type, string fuse, string argst)
        {
            IDictionary<int, int> dicts = XI.CalculatePetsScore();
            int diff1 = XI.Board.PoolDelta;
            ushort[] props = new ushort[] { 0, 1, 2, 3, 4 };
            int opr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Any(r => r.Pets[q] != 0));
            int rpr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.Team).Any(r => r.Pets[q] != 0));
            int diff = System.Math.Min(diff1 / 2, opr - rpr);
            string rSel = XI.AsyncInput(player.Uid, "#补" + diff + "张牌,T1" +
                ATeammatesTared(player), "JNH1802", "0");
            ushort rTar = ushort.Parse(rSel);
            XI.RaiseGMessage("G0DH," + rTar + ",0," + diff);
        }
        public bool JNH1803Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.ROMUshort == 0)
            {
                List<ushort> equips = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.Team == player.Team && py.IsAlive)
                        equips.AddRange(py.Pets.Where(p => p != 0));
                foreach (string ce in XI.Board.CsPets)
                {
                    int idx = ce.IndexOf(',');
                    ushort pet = ushort.Parse(ce.Substring(idx + 1));
                    equips.Remove(pet);
                }
                return equips.Count > 0;
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
                int point = 0;
                ushort[] pets = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                string g0hi = "";
                foreach (ushort pet in pets)
                {
                    ushort who = 0;
                    foreach (Player py in XI.Board.Garden.Values)
                    {
                        if (py.Pets.Contains(pet))
                        {
                            who = py.Uid;
                            break;
                        }
                    }
                    if (who != 0)
                    {
                        g0hi += "," + who + "," + pet;
                        point += (XI.LibTuple.ML.Decode(pet).STR + 1);
                    }
                }
                if (g0hi.Length > 0)
                    XI.RaiseGMessage("G0HI" + g0hi);
                if (point > 0)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + point);
                player.ROMUshort = 1;
            }
            else if (type == 1)
            {
                VI.Cout(0, "TR徐南松基础战力数值变为0.");
                if (player.STRh > 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + player.STRh);
                XI.RaiseGMessage("G0OE,0," + player.Uid);
                player.ROMUshort = 2;
            }
        }
        public string JNH1803Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<ushort> equips = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.Team == player.Team && py.IsAlive)
                        equips.AddRange(py.Pets.Where(p => p != 0));
                foreach (string ce in XI.Board.CsPets)
                {
                    int idx = ce.IndexOf(',');
                    ushort pet = ushort.Parse(ce.Substring(idx + 1));
                    equips.Remove(pet);
                }
                if (equips.Count > 1)
                    return "#爆发,/M1~" + equips.Count + "(p" + string.Join("p", equips) + ")";
                else
                    return "#爆发,/M1(p" + string.Join("p", equips) + ")";
            }
            else return "";
        }
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
    }
}
