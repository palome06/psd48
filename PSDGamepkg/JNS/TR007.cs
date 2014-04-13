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
                bool isWin = XI.Board.IsRounderBattleWin();
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
            if (type == 0)
            {
                if (player.ROMCards.Count > 0)
                {
                    bool anyOther = XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid);
                    List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                    foreach (Artiad.Harm harm in harms)
                    {
                        if (harm.Who == player.Uid)
                        {
                            int prop = Artiad.IntHelper.Elem2Int(harm.Element);
                            foreach (FiveElement five in Artiad.Harm.GetPropedElement())
                                if (harm.Element == five && player.ROMCards.Contains("I" + prop))
                                    return anyOther;
                            if (!Artiad.Harm.GetPropedElement().Contains(harm.Element))
                            {
                                if (harm.Element != FiveElement.LOVE && player.ROMCards.Contains("I4"))
                                    return anyOther;
                            }
                        }
                    }
                }
                return false;
            }
            else if (type == 1)
            {
                string[] parts = fuse.Split(',');
                if (parts[1] == player.Uid.ToString())
                {
                    for (int i = 3; i < parts.Length; ++i)
                        if (parts[i] == "JNT0401")
                            return true;
                }
                return false;
            }
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
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -99);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,5,I1,I2,I3,I4,I5");
        }
        public string JNT0401Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                {
                    ISet<int> cands = new HashSet<int>();
                    List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                    foreach (Artiad.Harm harm in harms)
                    {
                        if (harm.Who == player.Uid)
                        {
                            int prop = Artiad.IntHelper.Elem2Int(harm.Element);
                            foreach (FiveElement five in Artiad.Harm.GetPropedElement())
                                if (harm.Element == five && player.ROMCards.Contains("I" + prop))
                                    cands.Add(prop);
                            if (!Artiad.Harm.GetPropedElement().Contains(harm.Element))
                            {
                                if (harm.Element != FiveElement.LOVE &&
                                        harm.Element != FiveElement.SOL && player.ROMCards.Contains("I4"))
                                    cands.Add(4);
                            }
                        }
                    }
                    return "/I1(p" + string.Join("p", cands) + ")";
                }
                else if (prev.IndexOf(',') < 0)
                    return "#承受伤害的,/T1(p" + string.Join("p", XI.Board.Garden.Values
                        .Where(p => p.IsTared).Select(p => p.Uid)) + ")";
                else
                    return "";
            }
            else
                return "";
        }
        public bool JNT0402Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            bool has1 = mon1 != null && player.ROMCards.Contains("I" + (Util.GetFiveElementId(mon1.Element) + 1));
            Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            bool has2 = mon2 != null && player.ROMCards.Contains("I" + (Util.GetFiveElementId(mon2.Element) + 1));
            return XI.Board.IsAttendWar(player) && meLose && (has1 || has2);
        }
        public void JNT0402Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            int card = int.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));

            XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + card);
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
                if (mon1 != null && player.ROMCards.Contains("I" + (Util.GetFiveElementId(mon1.Element) + 1)))
                    sets.Add(Util.GetFiveElementId(mon1.Element) + 1);
                Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2 != null && player.ROMCards.Contains("I" + (Util.GetFiveElementId(mon2.Element) + 1)))
                    sets.Add(Util.GetFiveElementId(mon2.Element) + 1);
                return "/I1(p" + string.Join("p", sets) + ")";
            }
            else if (prev.IndexOf(',') < 0)
                return "#获得补牌,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsAlive)
                    .Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public void JNT0403Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = fuse.Split(',');
            int count = 0;
            for (int i = 1; i < blocks.Length; i += 3)
            {
                if (blocks[i] == player.Uid.ToString() && blocks[i + 1] == "1")
                    ++count;
            }
            XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        public bool JNT0403Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; i += 3)
            {
                if (blocks[i] == player.Uid.ToString() && blocks[i + 1] == "1")
                    return true;
            }
            return false;
        }
        #endregion TR004 - Lingyin
        #region TR005 - Lingbo
        public void JNT0501Action(Player player, int type, string fuse, string argst)
        {
            int val = int.Parse(argst);
            if (val > 0 && val <= player.DEX)
            {
                XI.RaiseGMessage("G0OX," + player.Uid + ",2," + val);
                XI.RaiseGMessage("G0IA," + player.Uid + ",2," + val);
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
            XI.RaiseGMessage("G0OE," + who);
            VI.Cout(0, "TR凌波对玩家{0}发动了「梦缘」.", XI.DisplayPlayer(who));
            XI.SendOutUAMessage(player.Uid, "JNT0502," + target, "0");
            //XI.InnerGMessage("G0OY,2," + player.Uid, 81);
        }
        public bool JNT0502Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; ++i)
                if (blocks[i] == player.Uid.ToString())
                    return true;
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
        public void JNT0602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] args = fuse.Split(',');
                for (int i = 1; i < args.Length; )
                {
                    ushort me = ushort.Parse(args[i]);
                    ushort lose = ushort.Parse(args[i + 1]);
                    if (lose == 0) // Get Card
                    {
                        if (XI.Board.Rounder.Uid == me)
                        {
                            int n = int.Parse(args[i + 2]);
                            args[i + 2] = (n + 1).ToString();
                        }
                        i += 3;
                    }
                    else if (lose == 1)
                        i += 3;
                    else if (lose == 2)
                        i += 3;
                    else if (lose == 3)
                        i += 2;
                    else
                        break;
                }
                XI.InnerGMessage(string.Join(",", args), 61);
            }
            else if (type == 1)
            {
                string g1di = "";
                string[] args = fuse.Split(',');
                for (int i = 1; i < args.Length; )
                {
                    ushort me = ushort.Parse(args[i]);
                    ushort lose = ushort.Parse(args[i + 1]);
                    int n = int.Parse(args[i + 2]);
                    if (lose == 0) // Get Card
                    {
                        if (XI.Board.Rounder.Uid == me && XI.Board.Rounder.Tux.Count > 0)
                        {
                            List<ushort> cards = Util.TakeRange(args, i + 3, i + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            string cardstr = XI.AsyncInput(XI.Board.Rounder.Uid, "#弃置的,Q1(p" +
                                string.Join("p", XI.Board.Rounder.Tux) + ")", "JNT0602", "0");
                            ushort card = ushort.Parse(cardstr);
                            XI.RaiseGMessage("G0QZ," + me + "," + card);
                            if (cards.Contains(card))
                                cards.Remove(card);
                            if (cards.Count > 0)
                                g1di += "," + me + "," + lose + "," + cards.Count + "," + string.Join(",", cards);
                        }
                        else
                            g1di += "," + string.Join(",", Util.TakeRange(args, i, i + 3 + n));
                    }
                    else
                        g1di += "," + string.Join(",", Util.TakeRange(args, i, i + 3 + n));
                    i += (3 + n);
                }
                if (g1di.Length > 0)
                    XI.InnerGMessage("G1DI" + g1di, 151);
            }
        }
        public bool JNT0602Valid(Player player, int type, string fuse)
        {
            string roundIn = XI.Board.RoundIN;
            ushort inr = (ushort)(roundIn[1] - '0');
            if (XI.Board.Rounder == null || !XI.Board.Rounder.IsAlive)
                return false;
            if (XI.Board.Rounder.Team != player.Team)
                return false;
            if (roundIn[0] != 'R' || roundIn.Substring(2) != "BC")
                return false;

            if (type == 0)
            {
                string[] args = fuse.Split(',');
                for (int i = 1; i < args.Length; )
                {
                    ushort me = ushort.Parse(args[i]);
                    ushort lose = ushort.Parse(args[i + 1]);
                    if (lose == 0) // Get Card
                    {
                        if (XI.Board.Rounder.Uid == me)
                        {
                            int n = int.Parse(args[i + 2]);
                            if (n > 0)
                                return true;
                        }
                        i += 3;
                    }
                    else if (lose == 1)
                        i += 3;
                    else if (lose == 2)
                        i += 3;
                    else if (lose == 3)
                        i += 2;
                    else
                        break;
                }
                return false;
            }
            else if (type == 1)
            {
                string[] args = fuse.Split(',');
                for (int i = 1; i < args.Length; )
                {
                    ushort me = ushort.Parse(args[i]);
                    ushort lose = ushort.Parse(args[i + 1]);
                    int n = int.Parse(args[i + 2]);
                    if (lose == 0) // Get Card
                    {
                        if (XI.Board.Rounder.Uid == me && XI.Board.Rounder.Tux.Count > 0)
                            return true;
                    }
                    i += (3 + n);
                }
                return false;
            }
            else
                return false;
        }
        #endregion TR006 - OuyangQian
        #region TR007 - LiYiru (EMPTY)
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
                XI.RaiseGMessage("G2KI," + player.Uid + ",0," + tar + ",4");
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
                    .Where(p => p.IsTared).Select(p => p.Uid).Except(player.ROMPlayerTar).Any();
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    Player py = XI.Board.Garden[harm.Who];
                    if (player.ROMPlayerTar.Contains(harm.Who) &&
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
                    if (player.ROMPlayerTar.Contains(harm.Who) &&
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
            //else if (type == 2)
            //{
            //    ushort ut = player.ROMPlayerTar;
            //    XI.RaiseGMessage("G0OJ," + player.Uid + ",2," + ut);
            //    Player py = XI.Board.Garden[ut];
            //    string tux;
            //    if (py.Tux.Count > 0)
            //    {
            //        tux = XI.AsyncInput(ut, "#弃置(取消则HP-1),/Q1(p" +
            //            string.Join("p", py.Tux) + ")", "JNT0802", "0");
            //    }
            //    else
            //        tux = XI.AsyncInput(ut, "/", "JNT0802", "0");

            //    if (tux.StartsWith("/") || tux == "0")
            //        Harm(player, py, 1);
            //    else
            //        XI.RaiseGMessage("G0QZ," + ut + "," + tux);
            //}
        }
        public string JNT0802Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1(p" + string.Join("p",
                    XI.Board.Garden.Values.Where(p => p.IsTared && !player.ROMPlayerTar.Contains(p.Uid))
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
                if (blocks[idx + 1] == "0")
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    ushort n = ushort.Parse(blocks[idx + 2]);
                    Player py = XI.Board.Garden[who];
                    if (who != player.Uid && py.IsAlive && py.Team == player.Team && n > 0)
                        invs.Add(who);
                    idx += (n + 3);
                }
                else
                {
                    ushort n = ushort.Parse(blocks[idx + 2]);
                    idx += (n + 3);
                }
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
            if (!player.IsAlive)
                return false;
            //if (XI.Board.RoundIN == "R" + XI.Board.Rounder.Uid + "EV")
            //    return false;
            if (player.Tux.Count <= 0)
                return false;
            HashSet<ushort> invs = new HashSet<ushort>();
            // G0DI,A,0,n,B,1,m
            string[] blocks = fuse.Split(',');
            int idx = 1;
            while (idx < blocks.Length)
            {
                if (blocks[idx + 1] == "0")
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    ushort n = ushort.Parse(blocks[idx + 2]);
                    Player py = XI.Board.Garden[who];
                    if (who != player.Uid && py.IsAlive && py.Team == player.Team && n > 0)
                        return true;
                    idx += (n + 3);
                }
                else
                {
                    ushort n = ushort.Parse(blocks[idx + 2]);
                    idx += (n + 3);
                }
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
        //public bool JNT1001Valid(Player player, int type, string fuse)
        //{
        //    bool b1 = XI.Board.IsAttendWar(player);
        //    if (player.IsSKOpt)
        //    {
        //        bool any = false;
        //        foreach (Player py in XI.Board.Garden.Values)
        //            if (py.IsAlive && py.Team == player.OppTeam)
        //            {
        //                if (py.Weapon != 0) { any = true; break; }
        //                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(py.ExEquip);
        //                if (tux != null && tux.Type == Tux.TuxType.WQ)
        //                    { any = true; break; }
        //            }
        //        if (!any) return false;
        //    }
        //    return XI.Board.IsAttendWar(player) && player.Tux.Count > 0;
        //}
        //public void JNT1001Action(Player player, int type, string fuse, string argst)
        //{
        //    int totalAmount = 0;
        //    foreach (Player py in XI.Board.Garden.Values)
        //        if (py.IsAlive && py.Team == player.OppTeam)
        //        {
        //            if (py.Weapon != 0)
        //                ++totalAmount;
        //            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(py.ExEquip);
        //            if (tux != null && tux.Type == Tux.TuxType.WQ)
        //                ++totalAmount;
        //        }
        //    XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
        //    if (totalAmount > 0)
        //        XI.RaiseGMessage("G0IA," + player.Uid + ",2," + totalAmount);
        //}
        //public string JNT1001Input(Player player, int type, string fuse, string prev)
        //{
        //    if (prev == "")
        //        return "/Q1(p" + string.Join("p", player.Tux) + ")";
        //    else
        //        return "";
        //}
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
                int amount = int.Parse(g0xzs[4]);
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
                if (py != player && py.Team == player.Team && py.IsTared &&
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
                if (thatHarm != null)
                {
                    thatHarm.N += (thisHarm.N + 1);
                    harms.Remove(thisHarm);
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
                    if (py != player && py.Team == player.Team && py.IsTared &&
                            harm.Element != FiveElement.LOVE && harm.Source != harm.Who && harm.N > 0)
                        uts.Add(py.Uid);
                }
                return "#代为承受伤害,/T1(p" + string.Join("p", uts);
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
                //XI.RaiseGMessage("G0CC," + player.Uid + ",0,TP04," + card + ";0," + fuse);

                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",2,1");
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
            XI.RaiseGMessage("G0IA," + player.Uid + ",2,1");
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
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            int count = harms.Count(p => p.Who != player.Uid && p.Source != p.Who
                && XI.Board.Garden[p.Who].Team == player.Team && p.Element != FiveElement.LOVE);
            IDictionary<Player, int> fengs = new Dictionary<Player, int>();
            for (int i = 0; i < count; ++i)
            {
                string input = XI.AsyncInput(player.Uid, "#HP-1,/T1(p" + string.Join("p",
                   XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")", "JNT1501", "0");
                if (input == "0" || input.StartsWith("/") || input == VI.CinSentinel)
                    break;
                Player py = XI.Board.Garden[ushort.Parse(input)];
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
            return harms.Any(p => p.Who != player.Uid && p.Source != p.Who
                && XI.Board.Garden[p.Who].Team == player.Team && p.Element != FiveElement.LOVE);
        }
        public void JNT1502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0) // ST->[EP]->GR
            {
                player.ROMUshort |= 0x1;
                XI.RaiseGMessage("G0JM,R" + player.Uid + "GR");
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
                XI.Board.Garden.Values.Where(p => p.IsAlive && !player.RAMPeoples.Contains(p.Uid))
                .Select(p => p.Uid)) + ")", "JNT1601", "0");
            ushort who = ushort.Parse(input);
            player.RAMPeoples.Add(who);
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,2");
        }
        public bool JNT1601Valid(Player player, int type, string fuse)
        {
            string[] g0zbs = fuse.Split(',');
            return g0zbs[1] == player.Uid.ToString() && XI.Board.Garden.Values
                .Where(p => p.IsAlive).Select(p => p.Uid).Except(player.RAMPeoples).Any();
        }
        public void JNT1602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                player.RAMUshort = 1;
            else if (type == 1)
            {
                player.RAMUshort = 2;
                if (player.Team == XI.Board.Rounder.Team)
                    XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",3");
                else
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",3");
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
            if (type == 1)
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            XI.RaiseGMessage("G0IP," + player.Team + ",1");
        }
        public bool JNT1801Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                // G0CC,{A,0/A,B},KN,x1,x2;TF
                string[] g0cc = fuse.Split(',');
                string kn = g0cc[3];
                Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(kn);
                if (tux != null && tux.Type == Base.Card.Tux.TuxType.ZP)
                {
                    if (g0cc[2] == "0" && g0cc[1] == player.Uid.ToString())
                        return true;
                    else if (g0cc[2] == player.Uid.ToString())
                        return true;
                }
            }
            else if (type == 1)
                return player.Tux.Count > 0;
            return false;
        }
        public string JNT1801Input(Player player, int type, string fuse, string prev)
        {
            if (type == 1 && prev == "")
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
        #endregion TR016 - Qinji

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
            XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + cnt);
            player.ROMUshort = 0;
            XI.RaiseGMessage("G0IP," + player.Team + "," + (cnt + 2));
            XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
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
                    XI.RaiseGMessage("G2YM,1," + mon2ut + ",0");
                    if (mon2.STR >= mon1.STR)
                    {
                        mon1.Curtain();
                        if (XI.Board.Mon1From != 0)
                        {
                            XI.RaiseGMessage("G0HL," + XI.Board.Mon1From + "," + XI.Board.Monster1);
                            XI.RaiseGMessage("G0WB," + XI.Board.Monster1);
                            XI.Board.MonDises.Add(XI.Board.Monster1);
                            XI.RaiseGMessage("G2ON,1," + XI.Board.Monster1);
                        }
                        else
                        {
                            XI.RaiseGMessage("G0WB," + XI.Board.Monster1);
                            XI.Board.MonDises.Add(XI.Board.Monster1);
                            XI.RaiseGMessage("G2ON,1," + XI.Board.Monster1);
                        }
                        XI.RaiseGMessage("G2YM,0,0,0");
                        XI.Board.Monster1 = 0;

                        mon2.WinEff();
                        XI.RaiseGMessage("G0WB," + XI.Board.Monster2);
                        XI.Board.MonDises.Add(XI.Board.Monster2);
                        XI.RaiseGMessage("G2ON,1," + XI.Board.Monster2);
                        XI.RaiseGMessage("G2YM,1,0,0");
                        XI.Board.Monster2 = 0;

                        XI.RaiseGMessage("G0IA," + player.Uid + ",3");
                        XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "ZN");
                    }
                    else
                        XI.RaiseGMessage("G0IP," + XI.Board.Rounder.OppTeam + "," + mon2.STR);
                }
                else if (NMBLib.IsNPC(mon2ut))
                {
                    NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(mon2ut));
                    //bool doubled = false;
                    //Hero nho = XI.LibTuple.HL.InstanceHero(npc.Hero);
                    //foreach (Player py in XI.Board.Garden.Values.Where(
                    //    p => p.IsAlive && p.Team == XI.Board.Rounder.Team))
                    //{
                    //    Hero pero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                    //    if (pero != null)
                    //    {
                    //        if (pero.Spouses.Contains(npc.Hero.ToString()))
                    //        {
                    //            doubled = true; break;
                    //        }
                    //        else if (nho != null && pero.Spouses.Contains(nho.Archetype.ToString()))
                    //        {
                    //            doubled = true; break;
                    //        }
                    //        Hero par = XI.LibTuple.HL.InstanceHero(pero.Archetype);
                    //        if (par != null && par.Spouses.Contains(npc.Hero.ToString()))
                    //        {
                    //            doubled = true; break;
                    //        }
                    //    }
                    //}
                    //if (doubled)
                    //    XI.WI.BCast("E0HZ,3," + who + "," + mon2ut);
                    //else
                    //    XI.WI.BCast("E0HZ,2," + who + "," + mon2ut);
                    //XI.RaiseGMessage("G2YM,1," + mon2ut + ",0");
                    //int nSTR = doubled ? (2 * npc.STR) : npc.STR;
                    XI.WI.BCast("E0HZ,2," + who + "," + mon2ut);
                    int nSTR = npc.STR;
                    if (nSTR >= mon1.STR)
                    {
                        mon1.Curtain();
                        if (XI.Board.Mon1From != 0)
                        {
                            XI.RaiseGMessage("G0HL," + XI.Board.Mon1From + "," + XI.Board.Monster1);
                            XI.RaiseGMessage("G0WB," + XI.Board.Monster1);
                            XI.Board.MonDises.Add(XI.Board.Monster1);
                            XI.RaiseGMessage("G2ON,1," + XI.Board.Monster1);
                        }
                        else
                        {
                            XI.RaiseGMessage("G0WB," + XI.Board.Monster1);
                            XI.Board.MonDises.Add(XI.Board.Monster1);
                            XI.RaiseGMessage("G2ON,1," + XI.Board.Monster1);
                        }
                        XI.RaiseGMessage("G2YM,0,0,0");
                        XI.Board.Mon1From = 0;
                        XI.Board.Monster1 = 0;

                        XI.HandleWithNPCEffect(player, npc, false);
                        XI.Board.MonDises.Add(XI.Board.Monster2);
                        XI.RaiseGMessage("G2ON,1," + XI.Board.Monster2);
                        XI.RaiseGMessage("G2YM,1,0,0");
                        XI.Board.Monster2 = 0;

                        XI.RaiseGMessage("G0IA," + player.Uid + ",3");
                        XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "ZN");
                    }
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
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",0,1");
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
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",0,1");
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
        #region HL008 - Yingyu
        public void JNH0801Action(Player player, int type, string fuse, string argst)
        {
            string[] parts = argst.Split(',');
            ushort ut = ushort.Parse(parts[0]);
            Player py = XI.Board.Garden[ut];
            int mask = 0;
            List<ushort> metux = Util.TakeRange(parts, 1, parts.Length)
                .Select(p => ushort.Parse(p)).ToList();
            foreach (ushort mut in metux)
            {
                Base.Card.Tux tx = XI.LibTuple.TL.DecodeTux(mut);
                if (tx.Type == Tux.TuxType.JP)
                    mask |= 0x1;
                else if (tx.Type == Tux.TuxType.TP)
                    mask |= 0x2;
                else if (tx.Type == Tux.TuxType.ZP)
                    mask |= 0x4;
                else if (tx.Type == Tux.TuxType.WQ || tx.Type == Tux.TuxType.FJ || tx.Type == Tux.TuxType.XB)
                    mask |= 0x8;
            }
            XI.RaiseGMessage("G2FU,2," + player.Uid + "," + string.Join(",", metux));
            List<ushort> uttux = new List<ushort>();
            foreach (ushort mut in py.Tux)
            {
                Base.Card.Tux tx = XI.LibTuple.TL.DecodeTux(mut);
                if (tx.Type == Tux.TuxType.JP && ((mask & 0x1) == 0))
                    uttux.Add(mut);
                else if (tx.Type == Tux.TuxType.TP && ((mask & 0x2) == 0))
                    uttux.Add(mut);
                else if (tx.Type == Tux.TuxType.ZP && ((mask & 0x4) == 0))
                    uttux.Add(mut);
                else if ((tx.Type == Tux.TuxType.WQ || tx.Type == Tux.TuxType.FJ
                    || tx.Type == Tux.TuxType.WQ) && ((mask & 0x8) == 0))
                    uttux.Add(mut);
            }
            string hints = uttux.Count > 0 ? "#弃置(取消则补牌后定身,/Q1(p" + string.Join("p", uttux) + ")" : "/";
            string input = XI.AsyncInput(ut, hints, "JNH0801", "0");
            if (!input.StartsWith("/") && input != VI.CinSentinel)
            {
                XI.RaiseGMessage("G0QZ," + ut + "," + input);
                XI.RaiseGMessage("G0DH," + ut + ",0,1");
            }
            else
            {
                XI.RaiseGMessage("G0DH," + ut + ",0," + metux.Count);
                XI.RaiseGMessage("G0DS," + ut + ",0,1");
            }
        }
        public bool JNH0801Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return player.Tux.Count > 0 && harms.Any(p => p.Who != p.Source
                && XI.Board.Garden[p.Who].IsTared && p.Element != FiveElement.LOVE);
        }
        public string JNH0801Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<ushort> uts = harms.Where(p => p.Who != p.Source
                    && XI.Board.Garden[p.Who].IsTared && p.Element != FiveElement.LOVE)
                    .Select(p => p.Who).Distinct().ToList();
                return "/T1(p" + string.Join("p", uts) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                if (player.Tux.Count < 2)
                    return "/Q1(p" + string.Join("p", player.Tux) + ")";
                else
                    return "/Q1~2(p" + string.Join("p", player.Tux) + ")";
            }
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
            {
                XI.Board.Hinder = XI.Board.Garden[ut];
                XI.RaiseGMessage("G2HS,2," + ut);
            }
            else if (XI.Board.Supporter == player)
            {
                XI.Board.Supporter = XI.Board.Garden[ut];
                XI.RaiseGMessage("G2HS,1," + ut);
            }
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
        #region HL018 - Xu'Nansong
        public bool JNH1801Valid(Player player, int type, string fuse)
        {
            bool meWin = (player.Team == XI.Board.Rounder.Team && XI.Board.IsBattleWin);
            if (meWin && XI.Board.Hinder.IsTared)
            {
                int diff = XI.Board.CalculateRPool() - XI.Board.CalculateOPool();
                return diff >= 2;
            }
            else
                return false;
        }
        public void JNH1801Action(Player player, int type, string fuse, string argst)
        {
            string hSel = XI.AsyncInput(player.Uid, "#进行「凝元诀」选择,T1(p" +
                XI.Board.Hinder.Uid + ")", "JNH1801Action", "0");
            ushort otar = ushort.Parse(hSel);
            int diff = (XI.Board.CalculateRPool() - XI.Board.CalculateOPool()) / 2;
            string oSel = XI.AsyncInput(otar, "#请选择HP-" + diff +
                "或对方HP+2##HP-" + diff + "##对方HP+2,Y2", "JNH1801Action", "1");
            if (oSel != "2")
                Harm(player, XI.Board.Garden[otar], diff);
            else
            {
                string rSel = XI.AsyncInput(player.Uid, "#HP+2,T1" +
                    ATeammatesTared(player), "JNH1801Action", "2");
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
                int diff = XI.Board.CalculateOPool() - XI.Board.CalculateRPool();
                int diff2 = dicts[player.OppTeam] - dicts[player.Team];
                return diff >= 2 && diff2 >= 5;
            }
            else
                return false;
        }
        public void JNH1802Action(Player player, int type, string fuse, string argst)
        {
            IDictionary<int, int> dicts = XI.CalculatePetsScore();
            int diff1 = XI.Board.CalculateOPool() - XI.Board.CalculateRPool();
            int diff2 = dicts[player.OppTeam] - dicts[player.Team];
            int diff = System.Math.Min(diff1 / 2, diff2 / 5);
            string rSel = XI.AsyncInput(player.Uid, "#补" + diff + "张牌,T1" +
                ATeammatesTared(player), "JNH1802Action", "0");
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
                        point += (1 + XI.LibTuple.ML.Decode(pet).STR);
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
                Base.Card.Hero hero = XI.LibTuple.HL.InstanceHero(player.SelectHero);
                if (hero != null)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + hero.STR);
                else
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,4");
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
    }
}
