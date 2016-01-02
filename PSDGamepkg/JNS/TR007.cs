﻿using PSD.Base;
using PSD.Base.Card;
using System;
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
            TargetPlayer(XI.Board.Rounder.Uid, player.Uid);
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
            List<ushort> invs = harms.Where(p => XI.Board.Garden[p.Who].Team == player.Team &&
                (p.Element == FiveElement.AQUA || p.Element == FiveElement.AGNI) && p.N > 0)
                .Select(p => p.Who).ToList();
            if (invs.Count > 0)
                TargetPlayer(player.Uid, invs);
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

                TargetPlayer(player.Uid, to);
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
            TargetPlayer(player.Uid, who);
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
                {
                    TargetPlayer(player.Uid, XI.Board.Supporter.Uid);
                    XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",1,1");
                }
                else if (XI.Board.Rounder.OppTeam == player.Team)
                {
                    TargetPlayer(player.Uid, XI.Board.Hinder.Uid);
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",1,1");
                }
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
                    TargetPlayer(player.Uid, ut);
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
                        TargetPlayer(player.Uid, to);
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
            if (type == 0)
                return player.TokenExcl.Count > 0 || player.Guardian != 0;
            else if (type == 1)
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
            // Always ask for select new one
            if (player.TokenExcl.Count > 0)
            {
                string input = XI.AsyncInput(player.Uid, "I1(p" +
                    string.Join("p", player.TokenExcl) + ")", "JNT0702", "0");
                ushort iNo = ushort.Parse(input);
                if (im.ContainsKey(iNo))
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + "," + im[iNo]);
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1," + isk[iNo]);
                }
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + iNo);
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
            else if (type == 3) // Loss the tag if leave without self-change
            {
                string[] g0oy = fuse.Split(',');
                for (int i = 1; i < g0oy.Length; i += 2)
                {
                    ushort utype = ushort.Parse(g0oy[i]);
                    ushort who = ushort.Parse(g0oy[i + 1]);
                    if (utype != 1 && player.ROM.ContainsKey("Away") &&
                            ((List<ushort>)player.ROM["Away"]).Contains(who))
                        return true;
                }
                return false;
            }
            else if (type == 4) // Decrease the DEX value if exists in Away
            {
                string[] g0iy = fuse.Split(',');
                for (int i = 1; i < g0iy.Length;)
                {
                    ushort utype = ushort.Parse(g0iy[i]);
                    ushort who = ushort.Parse(g0iy[i + 1]);
                    if (player.ROM.ContainsKey("Away") &&
                            ((List<ushort>)player.ROM["Away"]).Contains(who))
                        return true;
                    if (utype == 2)
                        i += 4;
                    else
                        i += 3;
                }
                return false;
            }
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
                TargetPlayer(player.Uid, list);
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
            else if (type == 3)
            {
                List<ushort> list = (List<ushort>)player.ROM["Away"];
                string[] g0oy = fuse.Split(',');
                for (int i = 1; i < g0oy.Length; i += 2)
                {
                    ushort utype = ushort.Parse(g0oy[i]);
                    ushort who = ushort.Parse(g0oy[i + 1]);
                    if (utype != 1)
                        list.Remove(who);
                }
            }
            else if (type == 4)
            {
                List<ushort> list = (List<ushort>)player.ROM["Away"];
                List<ushort> ins = new List<ushort>();
                string[] g0iy = fuse.Split(',');
                for (int i = 1; i < g0iy.Length;)
                {
                    ushort utype = ushort.Parse(g0iy[i]);
                    ushort who = ushort.Parse(g0iy[i + 1]);
                    if (list.Contains(who))
                    {
                        XI.RaiseGMessage("G0OX," + who + ",0," + player.ROMInt);
                        ins.Add(who);
                    }
                    if (utype == 2)
                        i += 4;
                    else
                        i += 3;
                }
                TargetPlayer(player.Uid, ins);
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
                        string choose = XI.AsyncInput(ut, "#请选择『水御灵』执行项目。" +
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
                ushort ut = ushort.Parse(g0iy[2]);
                string choose = XI.AsyncInput(ut, "#请选择『水御灵』执行项目。" +
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
                for (int idx = 1; idx < g1di.Length;)
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
            for (int idx = 1; idx < g1di.Length;)
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
                for (int idx = 1; idx < g1di.Length;)
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
                for (int idx = 1; idx < g1di.Length;)
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
            TargetPlayer(player.Uid, ut);
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
            return harms.Any(p => XI.Board.Garden[p.Who].Team == player.Team &&
                p.Element != FiveElement.LOVE && p.N > 0);
        }
        public void JNT0713Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<ushort> uts = harms.Where(p => XI.Board.Garden[p.Who].Team == player.Team
                && p.Element != FiveElement.LOVE && p.N > 0).Select(p => p.Who).Distinct().ToList();
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
            TargetPlayer(player.Uid, XI.Board.Garden.Values.Where(p => p.IsAlive).Select(p => p.Uid));
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
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && !p.Runes.Contains(3));
        }
        public void JNT0802Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort uq = ushort.Parse(argst.Substring(0, idx));
            ushort ut = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + uq);
            TargetPlayer(player.Uid, ut);
            XI.RaiseGMessage("G0IF," + ut + ",3");
        }
        public string JNT0802Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1(p" + string.Join("p", XI.Board.Garden
                    .Values.Where(p => p.IsTared && !p.Runes.Contains(3)).Select(p => p.Uid)) + ")";
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
                    {
                        TargetPlayer(player.Uid, ut);
                        XI.RaiseGMessage("G1XR,0," + player.Uid + "," + ut + ",1,1");
                    }
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
            XI.RaiseGMessage("G0HG," + string.Join(",", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == player.Team).Select(p => p.Uid + ",2")));
            string[] args = fuse.Split(',');
            for (int i = 1; i < args.Length; ++i)
                if (args[i] != player.Uid.ToString())
                {
                    ushort me = ushort.Parse(args[i]);
                    Player py = XI.Board.Garden[me];
                    string range = Util.SSelect(XI.Board,
                        p => p.IsAlive && p.Team == player.Team);
                    string input = XI.AsyncInput(me, "#获得补牌的,T1" + range, "G0ZW", "0");
                    XI.RaiseGMessage("G0HG," + input + ",2");
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
            XI.AsyncInput(player.Uid, "C1(" + Util.RepeatString("p0", py.Tux.Count) + ")", "JNT1003", "0");
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
            TargetPlayer(player.Uid, ut);
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

            TargetPlayer(player.Uid, ut);
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

            TargetPlayer(player.Uid, ut);
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
                {
                    TargetPlayer(player.Uid, XI.Board.Supporter.Uid);
                    XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",2");
                }
                else
                {
                    TargetPlayer(player.Uid, XI.Board.Hinder.Uid);
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",2");
                }
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
                TargetPlayer(player.Uid, XI.Board.Garden.Values.Where(p => p.IsAlive
                    && p.Team == player.Team && p.Tux.Count > 0).Select(p => p.Uid));
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
            TargetPlayer(player.Uid, invs.Select(p => p.Uid));
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
            ISet<ushort> lovers = new HashSet<ushort>();
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.IsAlive)
                {
                    Hero hro = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                    int n = hro.Spouses.Where(p => !p.StartsWith("!")).Count();
                    if (n > 0)
                    {
                        lovers.Add(py.Uid);
                        if (sel == 1)
                            g0dh.Add(py.Uid + ",0," + n);
                        else if (sel == 2)
                            g0dh.Add(py.Uid + ",1," + n);
                    }
                }
            }
            TargetPlayer(player.Uid, lovers);
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
            bool b1 = XI.Board.RestNPCPiles.Count > 0 && player.Tux.Count > 0;
            string[] g0xzs = fuse.Split(',');
            return b1 && g0xzs[2] == "2";
        }
        public void JNT2002Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort tuxCode = ushort.Parse(argst.Substring(0, idx));
            string selection = argst.Substring(idx + 1);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + tuxCode);
            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            if (selection == "1") // Put ahead
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
                return "#弃置,/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
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
                p => p.IsTared && p.Team == player.OppTeam); // won't consider whether occurs twice
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
                + namae[tux.Type] + "##对方补4牌,Y2", "JNT2101", "0");
            if (choose == "2")
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,4");
            else
            {
                XI.RaiseGMessage("G2FU,0,0,0,C," + string.Join(",", XI.Board.Garden[tar].Tux));
                List<ushort> tuxes = XI.Board.Garden[tar].Tux.Where(p =>
                    namae[XI.LibTuple.TL.DecodeTux(p).Type] != namae[tux.Type]).ToList();
                if (tuxes.Count > 0)
                    XI.RaiseGMessage("G0QZ," + tar + "," + string.Join(",", tuxes));
            }
        }
        public string JNT2101Input(Player player, int type, string fuse, string input)
        {
            if (input == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1(p" + string.Join("p", XI.Board.Garden
                    .Values.Where(p => p.IsTared && p.Team == player.OppTeam).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool JNT2102Valid(Player player, int type, string fuse)
        {
            if (player.Uid == XI.Board.Hinder.Uid && player.RAMUshort == 0)
                return true;
            else
                return false;
        }
        public void JNT2102Action(Player player, int type, string fuse, string args)
        {
            player.RAMUshort = 1;
            TargetPlayer(player.Uid, XI.Board.Rounder.Uid);
            XI.RaiseGMessage("G0IX," + player.Uid + ",2");
        }
        #endregion TR021 - LongMing
        #region TR022 - XiaChulin
        public bool JNT2201Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && player.ExCards.Count < 3 && fuse.Split(',')[2] == "2";
            else if (type == 1)
                return player.ExCards.Count > 0;
            else if (type == 2)
            {
                string[] blocks = fuse.Split(',');
                int idx = 1;
                while (idx < blocks.Length)
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    int n = int.Parse(blocks[idx + 1]);
                    if (who == player.Uid)
                    {
                        var cards = Util.TakeRange(blocks, idx + 2, idx + 2 + n)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (cards.Intersect(player.ExCards).Any())
                            return true;
                    }
                    idx += (n + 2);
                }
                return false;
            }
            else return false;
        }
        public void JNT2201Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                XI.RaiseGMessage("G0ZB," + player.Uid + ",2," + player.Uid + "," + ut);
                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0IP," + player.Team + ",1");
            }
            else if (type == 1)
                XI.RaiseGMessage("G0IP," + player.Team + "," + player.ExCards.Count);
            else if (type == 2)
            {
                string[] blocks = fuse.Split(',');
                int idx = 1; int count = 0;
                while (idx < blocks.Length)
                {
                    int n = int.Parse(blocks[idx + 1]);
                    var cards = Util.TakeRange(blocks, idx + 2, idx + 2 + n).Select(p => ushort.Parse(p));
                    count += cards.Intersect(player.ExCards).Count();
                    idx += (n + 2);
                }
                if (count > 0 && XI.Board.InFight)
                    XI.RaiseGMessage("G0OP," + player.Team + "," + count);
            }
        }
        public string JNT2201Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                var allType = player.ExCards.Select(p => XI.LibTuple.TL.DecodeTux(p).Type).ToList();
                List<ushort> avails = player.Tux.Where(p => !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup() &&
                    !allType.Contains(XI.LibTuple.TL.DecodeTux(p).Type)).ToList();
                if (avails.Count == 0)
                    return "/";
                else
                    return "/Q1(p" + string.Join("p", avails) + ")";
            }
            else return "";
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

            Tux tux = XI.LibTuple.TL.DecodeTux(eq);
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.IsAlive && py.Team == player.OppTeam)
                {
                    List<ushort> dices = py.ListOutAllEquips().Where(p =>
                        XI.LibTuple.TL.DecodeTux(p).Type == tux.Type).ToList();
                    if (dices.Count > 0)
                        XI.RaiseGMessage("G0QZ," + py.Uid + "," + string.Join(",", dices));
                }
            }
            Artiad.Procedure.LoopOfNPCUntilJoinable(XI, player);
        }
        public string JNT2202Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#装备来源,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Team == player.OppTeam && p.ListOutAllEquips().Count > 0).Select(p => p.Uid)) + ")";
            else
            {
                string[] blocks = prev.Split(',');
                if (blocks.Length == 1)
                {
                    ushort ut = ushort.Parse(prev);
                    return "#获取的装备,/C1(p" + string.Join("p", XI.Board.Garden[ut].ListOutAllEquips()) + ")";
                }
                else if (blocks.Length == 2)
                {
                    return "#交予,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                        p.Team == player.Team && p.Uid != player.Uid).Select(p => p.Uid)) + ")";
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
        #region TR023 - YueJinzhao
        public bool JNT2301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNT2301", player, fuse) && XI.Board.RestMonPiles.Count > 0;
            else if (type == 1)
            {
                Monster fighter = XI.Board.Battler as Monster;
                List<Monster> mons = player.TokenExcl.Select(p =>
                    XI.LibTuple.ML.Decode(ushort.Parse(p.Substring("M".Length)))).ToList();
                return fighter != null && mons.Any(p => p.Element == fighter.Element);
            }
            else return false;
        }
        public void JNT2301Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort[] pops = XI.Board.RestMonPiles.Dequeue(4);
                if (pops.Length > 0)
                {
                    XI.RaiseGMessage("G0YM,5," + string.Join(",", pops));
                    string mjoint = string.Join(",", pops.Select(p => "M" + p));
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + mjoint);
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + pops.Length + "," + mjoint);
                }
            }
            else if (type == 1)
            {
                Monster fighter = XI.Board.Battler as Monster;
                List<Monster> mons = player.TokenExcl.Select(p =>
                    XI.LibTuple.ML.Decode(ushort.Parse(p.Substring("M".Length)))).ToList();
                int n = mons.Count(p => p.Element == fighter.Element);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + n);
                XI.RaiseGMessage("G0IX," + player.Uid + ",1," + n);
            }
        }
        public bool JNT2302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player);
            else if (type == 1)
                return player.RAMUshort == 1;
            else
                return false;
        }
        public void JNT2302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort ut = ushort.Parse(argst.Substring(0, idx));
                string sel = argst.Substring(idx + 1);
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,M" + ut);
                XI.RaiseGMessage("G0ON," + player.Uid + ",M,1," + ut);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",M" + ut);
                if (!sel.StartsWith("2"))
                    XI.RaiseGMessage("G0IP," + player.Team + ",3");
                else
                {
                    int jdx = sel.IndexOf(',');
                    ushort target = ushort.Parse(sel.Substring(jdx + 1));
                    XI.RaiseGMessage("G0IF," + target + ",2");
                }
                player.RAMUshort = 1;
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G0LH,0," + player.Uid + "," + (player.HPb - 2));
                player.RAMUshort = 0;
            }
        }
        public string JNT2302Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                    return "/M1(p" + string.Join("p", player.TokenExcl.Select(p => p.Substring("M".Length))) + ")";
                else if (prev.IndexOf(',') < 0)
                {
                    if (XI.Board.Garden.Values.Any(p => p.IsTared))
                        return "#请选择「共鸣」执行项##战力+3##获得「神行」,/Y2";
                    else return "#请选择「共鸣」执行项##战力+3,/Y1";
                }
                else
                {
                    int idx = prev.IndexOf(',');
                    string sel = prev.Substring(idx + 1);
                    if (!sel.StartsWith("2"))
                        return "";
                    else if (sel.IndexOf(',') < 0)
                        return "#获得「神行」,T1" + AAllTareds(player);
                    else
                        return "";
                }
            }
            else return "";
        }
        #endregion TR023 - YueJinzhao
        #region TR024 - YueQi
        public bool JNT2401Valid(Player player, int type, string fuse)
        {
            if (XI.Board.Rounder.Team == player.Team)
            {
                string[] g0on = fuse.Split(',');
                for (int i = 1; i < g0on.Length;)
                {
                    string cardType = g0on[i + 1];
                    int n = int.Parse(g0on[i + 2]);
                    if (cardType == "M")
                    {
                        for (int j = i + 3; j < i + 3 + n; ++j)
                        {
                            ushort ut = ushort.Parse(g0on[j]);
                            if (Base.Card.NMBLib.IsMonster(ut))
                                return true;
                        }
                    }
                    i += (n + 3);
                }
            }
            return false;
        }
        public void JNT2401Action(Player player, int type, string fuse, string argst)
        {
            int count = 0;
            string[] g0on = fuse.Split(',');
            for (int i = 1; i < g0on.Length;)
            {
                string cardType = g0on[i + 1];
                int n = int.Parse(g0on[i + 2]);
                if (cardType == "M")
                {
                    for (int j = i + 3; j < i + 3 + n; ++j)
                    {
                        ushort ut = ushort.Parse(g0on[j]);
                        if (Base.Card.NMBLib.IsMonster(ut))
                            ++count;
                    }
                }
                i += (n + 3);
            }
            while (count > 0)
            {
                string select = XI.AsyncInput(player.Uid, "#获得标记,/T1" + AAllTareds(player) + ",#获得标记,F1(p" +
                    string.Join("p", XI.LibTuple.RL.GetFullAppendableList()) + ")", "JNT2402", "0");
                if (select.StartsWith("/") || select.StartsWith(VI.CinSentinel))
                    break;
                XI.RaiseGMessage("G0IF," + select);
                --count;
            }
        }
        public bool JNT2402Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.TuxPiles.Count > 0;
        }
        public void JNT2402Action(Player player, int type, string fuse, string argst)
        {
            ushort gayaUt = ushort.Parse(argst);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + gayaUt);
            Tux gayaTux = XI.LibTuple.TL.DecodeTux(gayaUt);
            List<ushort> table = new List<ushort>();
            while (XI.Board.TuxPiles.Count > 0 && table.Count < 2)
            {
                ushort ut = XI.DequeueOfPile(XI.Board.TuxPiles);
                XI.RaiseGMessage("G2IN,0,1");
                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    if (tux.IsTuxEqiup() && gayaTux.IsTuxEqiup() || tux.Type == gayaTux.Type)
                    {
                        XI.RaiseGMessage("G0YM,8," + ut);
                        table.Add(ut);
                    }
                    else
                        XI.RaiseGMessage("G0ON,0,C,1," + ut);
                }
            }
            if (table.Count == 2)
            {
                XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + table[0] + "," + table[1]);
                while (table.Count > 0)
                {
                    string select = XI.AsyncInput(player.Uid, "#分配的,/Q" + (table.Count == 2 ? "1~2" : "1") +
                        "(p" + string.Join("p", table) + "),/T1" + ATeammatesTared(player), "JNT2402", "0");
                    if ((select.StartsWith("/") && !select.Contains(",")) || select.StartsWith(VI.CinSentinel))
                    {
                        table.Clear(); break;
                    }
                    else if (select.StartsWith("/"))
                        continue;
                    else
                    {
                        int idx = select.LastIndexOf(',');
                        ushort to = ushort.Parse(select.Substring(idx + 1));
                        string deliver = select.Substring(0, idx);
                        ushort[] delivers = deliver.Split(',').Select(p => ushort.Parse(p)).ToArray();
                        if (to != player.Uid)
                            XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1," + delivers.Length + "," + deliver);
                        table.RemoveAll(p => delivers.Contains(p));
                    }
                }
            }
            if (table.Count != 0)
                XI.RaiseGMessage("G0ON,0,C," + table.Count + "," + string.Join(",", table));
        }
        public string JNT2402Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "/Q1(p" + string.Join("p", player.Tux) + ")" : "";
        }
        #endregion TR024 - YueQi
        #region TR025 - Liaori
        public bool JNT2501Valid(Player player, int type, string fuse)
        {
            Tux zpt1 = XI.LibTuple.TL.EncodeTuxCode("ZPT1");
            return player.Tux.Count > 0 && zpt1.Bribe(player, type, fuse) && zpt1.Valid(player, type, fuse);
        }
        public void JNT2501Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",ZPT1," + card + ";0," + fuse);
            XI.RaiseGMessage("G0CZ,0," + player.Uid);
        }
        public string JNT2501Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cands = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type != Base.Card.Tux.TuxType.ZP).ToList();
                return cands.Count > 0 ? ("/Q1(p" + string.Join("p", cands) + ")") : "/";
            }
            else
                return "";
        }
        public bool JNT2502Valid(Player player, int type, string fuse)
        {
            if (type == 0) // Watch
                return player.RAMUshort == 0 && fuse.Split(',')[2] == "2";
            else if (type == 1)
                return player.RAMUshort == 1;
            else
                return false;
        }
        public void JNT2502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                player.RAMUshort = 1;
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
            }
            else if (type == 1)
            {
                player.RAMUshort = 0;
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
            }
        }
        #endregion TR025 - Liaori
        #region TR027 - Qianye
        public bool JNT2701Valid(Player player, int type, string fuse)
        {
            if ((type == 0) || type >= 2 && type <= 5 && XI.Board.InFightThrough) // Z1 || I/OX, I/OW
            {
                if (XI.Board.IsAttendWar(player) && XI.Board.Battler != null)
                {
                    if (type == 2 || type == 3)
                    {
                        string[] blocks = fuse.Split(',');
                        if (blocks[1] != player.Uid.ToString()) { return false; }
                    }
                    else if (type == 4 || type == 5)
                    {
                        string[] blocks = fuse.Split(',');
                        if (blocks[1] != (XI.Board.Battler as Monster).DBSerial.ToString()) { return false; }
                    }
                    int now = player.DEX - XI.Board.Battler.AGL;
                    if (now < 0)
                        now = 0;
                    return now != player.RAMUshort;
                }
                return false;
            }
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
            else
                return false;
        }
        public void JNT2701Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type >= 2 && type <= 5)
            {
                int delta = player.DEX - XI.Board.Battler.AGL;
                if (delta < 0) { delta = 0; }
                if (delta < player.RAMUshort)
                    XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + "," + (player.RAMUshort - delta));
                else if (delta > player.RAMUshort)
                    XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + "," + (delta - player.RAMUshort));
                player.RAMUshort = (ushort)delta;
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
        }
        public bool JNT2702Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1) // ZB/OT
                return player.GetBaseEquipCount() != player.ROMUshort;
            else if (type == 2 || type == 3) // IS/OS
                return IsMathISOS("JNT2702", player, fuse) && player.GetBaseEquipCount() > 0;
            else if (type == 4) // Give up
                return fuse == "G0FI,O";
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
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
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
                return IsMathISOS("JNT2802", player, fuse);
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    if ((blocks[i] == "0" || blocks[i] == "2") &&
                            player.SingleTokenTar.ToString() == blocks[i + 1])
                        return true;
                }
                return false;
            }
            else if (type == 2)
            {
                if (player.SingleTokenTar != 0)
                {
                    List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                    foreach (Artiad.Cure cure in cures)
                    {
                        if (cure.Element != FiveElement.SOL && cure.Element != FiveElement.LOVE)
                        {
                            Player py = XI.Board.Garden[cure.Who];
                            if (py != null && py.Uid == player.SingleTokenTar && cure.N > 0)
                                return true;
                        }
                    }
                }
            }
            else if (type == 3)
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
                    //ushort cardId = XI.LibTuple.TL.EncodeTuxCode("WQ02").DBSerial; = 48;
                    ushort cardId = (XI.LibTuple.TL.EncodeTuxCode("WQ02") as TuxEqiup).SingleEntry;
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
        public void JNT2802Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string target = XI.AsyncInput(player.Uid, "#「天蛇杖」的,T1" + AAllTareds(player), "JNT2802", "0");
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ushort.Parse(target));
                TargetPlayer(player.Uid, player.SingleTokenTar);
                XI.SendOutUAMessage(player.Uid, "JNT2802," + target, "0");
            }
            else if (type == 1)
            {
                if (player.SingleTokenTar == XI.Board.Rounder.Uid)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
            }
            else if (type == 2)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                foreach (Artiad.Cure cure in cures)
                {
                    if (cure.Element != FiveElement.SOL && cure.Element != FiveElement.LOVE)
                    {
                        if (cure.Who == player.SingleTokenTar)
                            ++cure.N;
                    }
                }
                if (cures.Count > 0)
                    XI.InnerGMessage(Artiad.Cure.ToMessage(cures), 16);
                //List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                //foreach (Artiad.Cure cure in cures)
                //{
                //    if (cure.Element != FiveElement.SOL && cure.Element != FiveElement.LOVE)
                //    {
                //        Player py = XI.Board.Garden[cure.Who];
                //        if (py != null && py.Team == player.Team)
                //            ++cure.N;
                //    }
                //}
                //if (cures.Count > 0)
                //    XI.InnerGMessage(Artiad.Cure.ToMessage(cures), 16);
            }
            else if (type == 3)
            {
                string target = XI.AsyncInput(player.Uid, "#获得【天蛇杖】的,/T1" + ATeammatesTared(player), "JNT2802", "1");
                if (target.StartsWith("/")) return;
                ushort to = ushort.Parse(target);
                TargetPlayer(player.Uid, to);
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
        #endregion TR028 - Wuhou
        #region TR029 - Xianqing
        public bool JNT2901Valid(Player player, int type, string fuse)
        {
            var g = XI.Board.Garden;
            List<Artiad.Harm> harm = Artiad.Harm.Parse(fuse);
            return player.Tux.Count > 0 && harm.Any(p => p.Who != player.Uid && g[p.Who].IsTared &&
                 g[p.Who].Team == player.Team && Artiad.Harm.GetPropedElement().Contains(p.Element) && p.N > 0);
        }
        public void JNT2901Action(Player player, int type, string fuse, string argst)
        {
            var g = XI.Board.Garden;
            List<Artiad.Harm> harm = Artiad.Harm.Parse(fuse);
            List<ushort> invs = harm.Where(p => g[p.Who].IsTared && p.Who != player.Uid && g[p.Who].Team == player.Team &&
                 Artiad.Harm.GetPropedElement().Contains(p.Element) && p.N > 0).Select(p => p.Who).Distinct().ToList();
            while (invs.Count > 0 && player.Tux.Count > 0)
            {
                string giver = XI.AsyncInput(player.Uid, "#给予,/T1(p" + string.Join("p", invs) + ")", "JNT2901", "0");
                if (giver.StartsWith("/") || giver.Contains(VI.CinSentinel))
                    break;
                string give = XI.AsyncInput(player.Uid, "#给予,/Q1" + (player.Tux.Count > 1 ? ("~" +
                    player.Tux.Count) : "") + "(p" + string.Join("p", player.Tux) + ")", "JNT2901", "1");
                if (!give.StartsWith("/") && !give.Contains(VI.CinSentinel))
                {
                    ushort who = ushort.Parse(giver);
                    //ushort cardUt = ushort.Parse(give);
                    string[] cardUts = give.Split(',');
                    Player py = XI.Board.Garden[who];
                    TargetPlayer(player.Uid, who);
                    invs.Remove(who);

                    XI.RaiseGMessage("G0HQ,0," + who + "," + player.Uid + ",1," + cardUts.Length + "," + give);
                    List<ushort> canUse = py.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null && (XI.LibTuple.TL
                        .DecodeTux(p).Type == Tux.TuxType.JP || XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()) &&
                        XI.LibTuple.TL.DecodeTux(p).Valid(py, 0, fuse)).ToList();
                    if (canUse.Count != 0)
                    {
                        string uses = XI.AsyncInput(who, "#使用,/Q1(p" +
                            string.Join("p", canUse) + ")", "JNT2901", "2");
                        if (!uses.StartsWith("/") && !uses.Contains(VI.CinSentinel))
                        {
                            ushort useUt = ushort.Parse(uses);
                            Artiad.Procedure.UseCardDirectly(g[who], useUt, fuse, XI, who);
                        }
                    }
                    else
                        XI.AsyncInput(who, "/", "JNT2901", "2");
                    if (player.Tux.Count == 0)
                        XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                }
            }
        }
        public bool JNT2902Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.Rounder.Team == player.Team && XI.Board.Supporter.Uid != 0 && !XI.Board.SupportSucc;
            bool b2 = XI.Board.Rounder.Team == player.OppTeam && XI.Board.Hinder.Uid != 0 && !XI.Board.HinderSucc;
            return !XI.Board.IsAttendWar(player) && (b1 || b2);
        }
        public void JNT2902Action(Player player, int type, string fuse, string argst)
        {
            if (XI.Board.Rounder.Team == player.Team)
                XI.RaiseGMessage("G17F,S," + player.Uid);
            else if (XI.Board.Rounder.Team == player.OppTeam)
                XI.RaiseGMessage("G17F,H," + player.Uid);
        }
        #endregion TR029 - Xianqing
        #region TR032 - JuShifang
        public bool JNT3201Valid(Player player, int type, string fuse)
        {
            ushort op = ushort.Parse(fuse.Split(',')[2]);
            return (op & 0x2) == 0 && IsMathISOS("JNT3201", player, fuse);
        }
        public void JNT3201Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0OS," + player.Uid + ",0,JNT3201,JNT3202");
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",17033");
            XI.RaiseGMessage("G0IS," + player.Uid + ",2,JNT3301,JNT3302,JNT3303");
        }
        public bool JNT3202Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                ushort op = ushort.Parse(fuse.Split(',')[2]);
                return (op & 0x2) != 0 && IsMathISOS("JNT3202", player, fuse);
            }
            else if (type == 1 && player.SingleTokenTar != 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                return harms.Any(p => p.Who == player.Uid && p.N > 0);
            }
            else return false;
        }
        public void JNT3202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string target = XI.AsyncInput(player.Uid,
                    "#『大英雄』的,/T1" + AOthersTared(player), "JNT3202", "0");
                if (!target.StartsWith("/") && !target.Contains(VI.CinSentinel))
                {
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ushort.Parse(target));
                    TargetPlayer(player.Uid, player.SingleTokenTar);
                    XI.SendOutUAMessage(player.Uid, "JNT3202," + target, "0");
                }
            }
            else if (type == 1)
            {
                TargetPlayer(player.Uid, player.SingleTokenTar);
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Artiad.Harm myHarm = harms.First(p => p.Who == player.Uid && p.N > 0);
                if (harms.Any(p => p.Who == player.SingleTokenTar && p.N > 0 && p.Element == myHarm.Element))
                    harms.First(p => p.Who == player.SingleTokenTar && p.N > 0 && p.Element == myHarm.Element).N += myHarm.N;
                else
                    harms.Add(new Artiad.Harm(player.SingleTokenTar, myHarm.Source, myHarm.Element, myHarm.N, myHarm.Mask));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -119);
            }
        }
        #endregion TR032 - JuShifang
        #region TR033 - Doubao
        public bool JNT3301Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count < 5;
        }
        public void JNT3301Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",1," + (5 - player.Tux.Count));
        }
        public bool JNT3302Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => p.Who == player.Uid && p.Element != FiveElement.LOVE && p.N > 0) && player.Tux.Count > 0;
        }
        public void JNT3302Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            Cure(player, player, 1);
        }
        public string JNT3302Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "/Q1(p" + string.Join("p", player.Tux) + ")" : "";
        }
        public bool JNT3303Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                Func<Player, bool> spouseContains = (p) =>
                {
                    Hero hero = XI.LibTuple.HL.InstanceHero(p.SelectHero);
                    if (hero != null && hero.Spouses.Contains(player.SelectHero.ToString()))
                        return false;
                    if (p.ExSpouses.Contains(player.SelectHero.ToString()))
                        return false;
                    return true;
                };
                return XI.Board.Garden.Values.Any(p => p.HP == 0 && p.Team == player.Team && !p.Loved && spouseContains(p));
            }
            else if (type == 1)
                return player.ROM.ContainsKey("ExSpTo") && ((List<ushort>)player.ROM["ExSpTo"]).Count > 0;
            else if (type == 2)
                return fuse.Split(',').Contains(player.Uid.ToString());
            else return false;
        }
        public void JNT3303Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                List<ushort> tars = argst.Split(',').Select(p => ushort.Parse(p)).ToList();
                tars.ForEach(p => XI.Board.Garden[p].ExSpouses.Add(player.SelectHero.ToString()));
                if (!player.ROM.ContainsKey("ExSpTo"))
                    player.ROM["ExSpTo"] = new List<ushort>();
                List<ushort> exspto = (List<ushort>)player.ROM["ExSpTo"];
                exspto.AddRange(tars);
            }
            else if (type == 1)
            {
                List<ushort> exspto = (List<ushort>)player.ROM["ExSpTo"];
                exspto.ForEach(p => XI.Board.Garden[p].ExSpouses.Remove(player.SelectHero.ToString()));
                player.ROM.Remove("ExSpTo");
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
                XI.RaiseGMessage("G0OY,1," + player.Uid);
                XI.RaiseGMessage("G0OS," + player.Uid + ",0,JNT3301,JNT3302,JNT3303");
                XI.RaiseGMessage("G0IY,1," + player.Uid +
                    ",17032," + XI.LibTuple.HL.InstanceHero(17032).HP);
                XI.RaiseGMessage("G0IS," + player.Uid + ",2,JNT3201,JNT3202");

                if (zw != "")
                    XI.InnerGMessage("G0ZW" + zw, -10);
            }
        }
        public string JNT3303Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                Func<Player, bool> spouseContains = (p) =>
                {
                    Hero hero = XI.LibTuple.HL.InstanceHero(p.SelectHero);
                    if (hero != null && hero.Spouses.Contains(player.SelectHero.ToString()))
                        return false;
                    if (p.ExSpouses.Contains(player.SelectHero.ToString()))
                        return false;
                    return true;
                };
                List<ushort> list = XI.Board.Garden.Values.Where(p => p.HP == 0 && p.Team == player.Team &&
                    !p.Loved && spouseContains(p)).Select(p => p.Uid).ToList();
                return "/T1" + (list.Count > 1 ? ("~" + list.Count) : "") + "(p" + string.Join("p", list) + ")";
            }
            else return "";
        }
        #endregion TR033 - Doubao
        #region TR034 - Mingxiu
        public bool JNT3401Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
               || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            return meLose && XI.Board.Garden.Values.Where(p => p.IsTared && XI.Board.IsAttendWar(p)
                && p.Uid != player.Uid && p.Team == player.Team).Any();
        }
        public void JNT3401Action(Player player, int type, string fuse, string argst)
        {
            ushort who = ushort.Parse(argst);
            Harm(player, XI.Board.Garden[who], 1);
            TargetPlayer(player.Uid, who);
            XI.RaiseGMessage("G0IF," + who + ",1");
        }
        public string JNT3401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> uts = XI.Board.Garden.Values.Where(p => p.IsTared && XI.Board.IsAttendWar(p)
                    && p.Uid != player.Uid && p.Team == player.Team).Select(p => p.Uid).ToList();
                return "/T1(p" + string.Join("p", uts) + ")";
            }
            else return "";
        }
        public bool JNT3402Valid(Player player, int type, string fuse)
        {
            if (type == 0) // PD
            {
                return XI.Board.Garden.Values.Any(p => p.IsAlive && XI.Board.IsAttendWar(p) &&
                    XI.LibTuple.HL.InstanceHero(p.SelectHero).Bio.Contains("K"));
            }
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
                        if (XI.LibTuple.HL.InstanceHero(XI.Board.Garden[od].SelectHero).Bio.Contains("K"))
                            --zero;
                        if (XI.LibTuple.HL.InstanceHero(XI.Board.Garden[nw].SelectHero).Bio.Contains("K"))
                            ++zero;
                    }
                }
                return zero != 0;
            }
            else return false;
        }
        public void JNT3402Action(Player player, int type, string fuse, string args)
        {
            if (type == 0) // PD
            {
                int count = XI.Board.Garden.Values.Count(p => p.IsAlive && XI.Board.IsAttendWar(p) &&
                    XI.LibTuple.HL.InstanceHero(p.SelectHero).Bio.Contains("K"));
                player.RAMUshort = (ushort)count;
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + count * 2);
            }
            else if (type == 1)
            {
                int count = XI.Board.Garden.Values.Count(p => p.IsAlive && XI.Board.IsAttendWar(p) &&
                    XI.LibTuple.HL.InstanceHero(p.SelectHero).Bio.Contains("K"));
                if (count > player.RAMUshort)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + (count - player.RAMUshort) * 2);
                else if (count < player.RAMUshort)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",1," + (player.RAMUshort - count) * 2);
                player.RAMUshort = (ushort)count;
            }
        }
        public bool JNT3403Valid(Player player, int type, string fuse)
        {
            bool hasPet = XI.Board.Garden.Values.Any(p => p.IsTared &&
                p.Team == player.OppTeam && p.GetPetCount() > 0);
            bool lessPeople = XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.Team) <
                XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.OppTeam);
            return player.ROMUshort == 0 && hasPet && lessPeople;
        }
        public void JNT3403Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort who = ushort.Parse(argst.Substring(0, idx));
            ushort pet = ushort.Parse(argst.Substring(idx + 1));
            TargetPlayer(player.Uid, who);
            XI.RaiseGMessage("G0HL," + who + "," + pet);
            XI.RaiseGMessage("G0ON," + who + ",M,1," + pet);
            player.ROMUshort = 1;
            if (player.DEXh > 0)
                XI.RaiseGMessage("G0OX," + player.Uid + ",0," + player.DEXh);
        }
        public string JNT3403Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#弃置宠物的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                    p.IsTared && p.Team == player.OppTeam && p.GetPetCount() > 0).Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                return "#弃置的,/M1(p" + string.Join("p", XI.Board.Garden[who].Pets.Where(p => p != 0)) + ")";
            }
            else
                return "";
        }
        #endregion TR034 - Mingxiu
        #region TR036 - LuoMaiming
        public bool JNT3601Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNT3601", player, fuse);
            else if (type == 2)
            {
                Base.Card.Monster mon = XI.Board.Battler as Monster;
                if (mon != null && (mon.Element == FiveElement.AQUA ||
                        mon.Element == FiveElement.AGNI || mon.Element == FiveElement.SOL))
                {
                    return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.Team && XI.Board.IsAttendWar(p));
                }
            }
            return false;
        }
        public void JNT3601Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.IsAlive && py.Team == player.Team)
                        ++py.TuxLimit;
                }
            }
            else if (type == 1)
            {
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.IsAlive && py.Team == player.Team)
                        --py.TuxLimit;
                }
            }
            else if (type == 2)
            {
                XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.Team && XI.Board.IsAttendWar(p)).Select(p => p.Uid + ",0,1")));
            }
        }
        public bool JNT3602Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid);
            else if (type == 1)
            {
                string[] g1ck = fuse.Split(',');
                return g1ck[2] == "SF06" && g1ck[3] == "0";
            }
            else
                return false;
        }
        public void JNT3602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort ut = ushort.Parse(argst.Substring(0, idx));
                ushort to = ushort.Parse(argst.Substring(idx + 1));
                XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + ut);
                XI.RaiseGMessage("G0IF," + to + ",6");
            }
            else if (type == 1)
            {
                if (argst == "1")
                    Cure(player, player, 2);
                else
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
        }
        public string JNT3602Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1" + AOthersTared(player);
            else if (type == 1 && prev == "")
                return "#请选择「嗜血法阵」执行项。##HP+2##补一张牌,Y2";
            else return "";
        }
        #endregion TR036 - LuoMaiming
        #region TR037 - YingXuwei
        public bool JNT3701Valid(Player player, int type, string fuse)
        {
            string[] g0on = fuse.Split(',');
            for (int idx = 1; idx < g0on.Length;)
            {
                ushort who = ushort.Parse(g0on[idx]);
                string cm = g0on[idx + 1];
                int n = int.Parse(g0on[idx + 2]);
                if (XI.Board.Garden.ContainsKey(who) && XI.Board.Garden[who].Team == player.OppTeam && cm == "C")
                {
                    List<ushort> tuxes = Util.TakeRange(g0on, idx + 3, idx + 3 + n)
                        .Select(p => ushort.Parse(p)).Where(p => XI.Board.TuxDises.Contains(p)).ToList();
                    foreach (ushort ut in tuxes)
                    {
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux != null && tux.Type == Tux.TuxType.TP)
                            return true;
                    }
                }
                idx += (3 + n);
            }
            return false;
        }
        public void JNT3701Action(Player player, int type, string fuse, string argst)
        {
            ushort which = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            string[] g0on = fuse.Split(',');
            string ng0on = "";
            XI.RaiseGMessage(Artiad.Toxi.ToMessage(new Artiad.Toxi(player.Uid, player.Uid, FiveElement.A, 1)));
            for (int idx = 1; idx < g0on.Length;)
            {
                ushort who = ushort.Parse(g0on[idx]);
                string cm = g0on[idx + 1];
                int n = int.Parse(g0on[idx + 2]);
                if (who != player.Uid && cm == "C")
                {
                    List<ushort> tuxes = Util.TakeRange(g0on, idx + 3, idx + 3 + n)
                        .Select(p => ushort.Parse(p)).Where(p => XI.Board.TuxDises.Contains(p)).ToList();
                    if (tuxes.Contains(which))
                    {
                        XI.RaiseGMessage("G2CN,0,1");
                        //XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
                        XI.RaiseGMessage("G0ZB," + player.Uid + ",2,0," + which);
                        XI.Board.TuxDises.Remove(which);
                        tuxes.Remove(which); break;
                    }
                    if (tuxes.Count > 0)
                        ng0on += "," + who + "," + cm + "," + tuxes.Count + "," + string.Join(",", tuxes);
                }
                else
                    ng0on += "," + string.Join(",", Util.TakeRange(g0on, idx, idx + 3 + n));
                idx += (3 + n);
            }
            if (ng0on.Length > 0)
                XI.InnerGMessage("G0ON" + ng0on, 145);
        }
        public string JNT3701Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                ISet<ushort> invs = new HashSet<ushort>();
                string[] g0on = fuse.Split(',');
                for (int idx = 1; idx < g0on.Length;)
                {
                    ushort who = ushort.Parse(g0on[idx]);
                    string cm = g0on[idx + 1];
                    int n = int.Parse(g0on[idx + 2]);
                    Player py = XI.Board.Garden[who];
                    if (py.Team == player.OppTeam && cm == "C")
                    {
                        List<ushort> tuxes = Util.TakeRange(g0on, idx + 3, idx + 3 + n)
                            .Select(p => ushort.Parse(p)).Where(p => XI.Board.TuxDises.Contains(p)).ToList();
                        foreach (ushort ut in tuxes)
                        {
                            Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                            if (tux != null && tux.Type == Tux.TuxType.TP)
                                invs.Add(who);
                        }
                    }
                    idx += (3 + n);
                }
                return "/T1(p" + string.Join("p", invs) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort tar = ushort.Parse(prev);
                ISet<ushort> txs = new HashSet<ushort>();
                string[] g0on = fuse.Split(',');
                for (int idx = 1; idx < g0on.Length;)
                {
                    ushort who = ushort.Parse(g0on[idx]);
                    string cm = g0on[idx + 1];
                    int n = int.Parse(g0on[idx + 2]);
                    Player py = XI.Board.Garden[who];
                    if (who == tar && cm == "C")
                    {
                        List<ushort> tuxes = Util.TakeRange(g0on, idx + 3, idx + 3 + n)
                            .Select(p => ushort.Parse(p)).Where(p => XI.Board.TuxDises.Contains(p)).ToList();
                        foreach (ushort ut in tuxes)
                        {
                            Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                            if (tux != null && tux.Type == Tux.TuxType.TP)
                                txs.Add(ut);
                        }
                    }
                    idx += (3 + n);
                }
                return "/C1(p" + string.Join("p", txs) + ")";
            }
            else
                return "";
        }
        public void JNT3702Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                // G0ZB,A,2/3/4,from,x
                string[] blocks = fuse.Split(',');
                if (blocks[1] == player.Uid.ToString() && blocks[2] == "2")
                {
                    int n = blocks.Length - 4;
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0," + n);
                }
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                int idx = 1; int count = 0;
                while (idx < blocks.Length)
                {
                    int n = int.Parse(blocks[idx + 1]);
                    var cards = Util.TakeRange(blocks, idx + 2, idx + 2 + n).Select(p => ushort.Parse(p));
                    count += cards.Intersect(player.ExCards).Count();
                    idx += (n + 2);
                }
                if (count > 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + count);
            }
        }
        public bool JNT3702Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                // G0ZB,A,2/3/4,x
                string[] blocks = fuse.Split(',');
                if (blocks[1] == player.Uid.ToString() && blocks[2] == "2")
                    return blocks.Length > 4;
                else
                    return false;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                int idx = 1;
                while (idx < blocks.Length)
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    int n = int.Parse(blocks[idx + 1]);
                    if (who == player.Uid)
                    {
                        var cards = Util.TakeRange(blocks, idx + 2, idx + 2 + n)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (cards.Intersect(player.ExCards).Any())
                            return true;
                    }
                    idx += (n + 2);
                }
                return false;
            }
            else
                return false;
        }
        public void JNT3703Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                player.RAMUshort = 1;
                ushort ut = ushort.Parse(argst);
                Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                for (int i = 0; i < linkHeads.Length; ++i)
                {
                    int idx = linkHeads[i].IndexOf(',');
                    string pureTypeStr = linkHeads[i].Substring(idx + 1);
                    if (!pureTypeStr.Contains("!"))
                    {
                        ushort pureType = ushort.Parse(pureTypeStr);
                        XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," + tux.Code +
                            "," + argst + ";" + pureType + "," + pureFuse); // argst contains ut and won't be null
                        break;
                    }
                }
            }
            else if (type == 2)
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", player.ExCards));
        }
        public bool JNT3703Valid(Player player, int type, string fuse)
        {
            if ((type == 0 || type == 1) && player.ExCards.Count > 0)
            {
                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                // linkHeads = { "TP02,0", "TP03,0" };
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                foreach (ushort ut in player.ExCards)
                {
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    if (tux != null)
                    {
                        for (int i = 0; i < linkHeads.Length; ++i)
                        {
                            int idx = linkHeads[i].IndexOf(',');
                            string pureName = linkHeads[i].Substring(0, idx);
                            string pureTypeStr = linkHeads[i].Substring(idx + 1);
                            if (!pureTypeStr.Contains("!"))
                            {
                                ushort pureType = ushort.Parse(pureTypeStr);
                                if (tux.Code == pureName && tux.Bribe(player, pureType, pureFuse)
                                        && tux.Valid(player, pureType, pureFuse))
                                    return true;
                            }
                        }
                    }
                }
                return false;
            }
            else if (type == 2 && player.ExCards.Count > 0)
                return player.RAMUshort != 0;
            else return false;
        }
        public string JNT3703Input(Player player, int type, string fuse, string prev)
        {
            if ((type == 0 || type == 1) && prev == "")
            {
                List<ushort> candidates = new List<ushort>();
                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                foreach (ushort ut in player.ExCards)
                {
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    if (tux != null)
                    {
                        for (int i = 0; i < linkHeads.Length; ++i)
                        {
                            int idx = linkHeads[i].IndexOf(',');
                            string pureName = linkHeads[i].Substring(0, idx);
                            string pureTypeStr = linkHeads[i].Substring(idx + 1);
                            if (!pureTypeStr.Contains("!"))
                            {
                                ushort pureType = ushort.Parse(pureTypeStr);
                                if (tux.Code == pureName && tux.Bribe(player, pureType, pureFuse)
                                            && tux.Valid(player, pureType, pureFuse))
                                {
                                    candidates.Add(ut); break;
                                }
                            }
                        }
                    }
                }
                return "/Q1(p" + string.Join("p", candidates) + ")";
            }
            else return "";
        }
        #endregion TR037 - YingXuwei
        #region TR039 - BianLuohuan
        public bool JNT3901Valid(Player player, int type, string fuse)
        {
            Tux jpt6 = XI.LibTuple.TL.EncodeTuxCode("JPT6");
            return player.Tux.Count > 0 && jpt6.Valid(player, type, fuse) && jpt6.Bribe(player, type, fuse);
        }
        public void JNT3901Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JPT6," + card + ";0," + fuse);
        }
        public string JNT3901Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else return "";
        }
        public bool JNT3902Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid);
            else if (type == 1)
                return XI.Board.Garden.Values.Any(p => p.IsTared && XI.Board.IsAttendWar(p) && p.Runes.Contains(5));
            else
                return false;
        }
        public void JNT3902Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort ut = ushort.Parse(argst.Substring(0, idx));
                ushort to = ushort.Parse(argst.Substring(idx + 1));
                XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + ut);
                XI.RaiseGMessage("G0IF," + to + ",5");
            }
            else if (type == 1)
            {
                List<ushort> invs = argst.Split(',').Select(p => ushort.Parse(p)).ToList();
                if (invs.Contains(XI.Board.Rounder.Uid) && XI.Board.Rounder.Runes.Contains(5))
                    XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,1");
                if (XI.Board.Supporter.IsReal && invs.Contains(XI.Board.Supporter.Uid) && XI.Board.Supporter.Runes.Contains(5))
                {
                    XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",2");
                    XI.RaiseGMessage("G0IA," + XI.Board.Supporter.Uid + ",1,1");
                }
                if (XI.Board.Hinder.IsReal && invs.Contains(XI.Board.Hinder.Uid) && XI.Board.Hinder.Runes.Contains(5))
                {
                    XI.RaiseGMessage("G0IX," + XI.Board.Hinder.Uid + ",2");
                    XI.RaiseGMessage("G0IA," + XI.Board.Hinder.Uid + ",1,1");
                }
            }
        }
        public string JNT3902Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + "),/T1" + AOthersTared(player);
            else if (type == 1 && prev == "")
            {
                List<ushort> hass = XI.Board.Garden.Values.Where(p => p.IsTared && XI.Board.IsAttendWar(p)
                    && p.Runes.Contains(5)).Select(p => p.Uid).ToList();
                return "/T1" + (hass.Count > 1 ? ("~" + hass.Count) : "") + "(p" + string.Join("p", hass) + ")";
            }
            else return "";
        }
        public bool JNT3903Valid(Player player, int type, string fuse)
        {
            return XI.Board.RestNPCPiles.Count > 0 && player.HP == 0 && player.IsAlive;
        }
        public void JNT3903Action(Player player, int type, string fuse, string argst)
        {
            Artiad.Procedure.LoopOfNPCUntilJoinable(XI, player);
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", -20);
        }
        #endregion TR039 - BianLuohuan
        #region TR040 - QiliXiaoyuan
        public bool JNT4001Valid(Player player, int type, string fuse)
        {
            Monster mon = XI.Board.Battler as Monster;
            return mon != null && mon.Level == Monster.ClLevel.STRONG || mon.Level == Monster.ClLevel.BOSS;
        }
        public void JNT4001Action(Player player, int type, string fuse, string argst)
        {
            Monster mon = XI.Board.Battler as Monster;
            if (mon.Level == Monster.ClLevel.STRONG)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
            else if (mon.Level == Monster.ClLevel.BOSS)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
        }
        public bool JNT4002Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.Supporter.Uid != 0 && XI.Board.Rounder.Team == player.Team && !XI.Board.SupportSucc;
            bool b2 = XI.Board.Hinder.Uid != 0 && XI.Board.Rounder.Team == player.OppTeam && !XI.Board.HinderSucc;
            bool b3 = XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.Team && !XI.Board.IsAttendWar(p));
            return (b1 || b2) && b3;
        }
        public void JNT4002Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(p => p.IsTared &&
                p.Team == player.Team && !XI.Board.IsAttendWar(p)).Select(p => p.Uid + ",0,1")));
        }
        #endregion TR040 - QiliXiaoyuan
        #region TR041 - Shuoxuan
        public bool JNT4101Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid && p.Tux.Count > 0);
            else if (type == 1)
                return player.TokenExcl.Count > 0 && player.ROMUshort != 0;
            else if ((type == 2 || type == 3) && player.TokenExcl.Count > 0)
            {
                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                // linkHeads = { "TP02,0", "TP03,0" };
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                ushort ut = ushort.Parse(player.TokenExcl[0].Substring("C".Length));
                Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    for (int i = 0; i < linkHeads.Length; ++i)
                    {
                        int idx = linkHeads[i].IndexOf(',');
                        string pureName = linkHeads[i].Substring(0, idx);
                        string pureTypeStr = linkHeads[i].Substring(idx + 1);
                        if (!pureTypeStr.Contains("!"))
                        {
                            ushort pureType = ushort.Parse(pureTypeStr);
                            if (tux.Code == pureName && tux.Bribe(player, pureType, pureFuse)
                                    && tux.Valid(player, pureType, pureFuse))
                                return true;
                        }
                    }
                }
                return false;
            }
            else
                return false;
        }
        public void JNT4101Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst.Substring(0, argst.IndexOf(',')));
                List<ushort> vals = XI.Board.Garden[ut].Tux.Except(XI.Board.ProtectedTux).ToList();
                vals.Shuffle();
                XI.RaiseGMessage("G0OT," + ut + ",1," + vals[0]);
                XI.RaiseGMessage("G2TZ," + player.Uid + "," + ut + ",C" + vals[0]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,C" + vals[0]);
                player.ROMUshort = ut;
            }
            else if (type == 1)
            {
                ushort who = player.ROMUshort;
                List<string> tokens = player.TokenExcl.ToList();
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + player.TokenExcl.Count + "," + string.Join(",", player.TokenExcl));
                if (!XI.Board.Garden[who].IsAlive)
                {
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", tokens));
                    XI.RaiseGMessage("G0ON,C," + player.Uid + "," + tokens.Count +
                        "," + string.Join(",", tokens.Select(p => p.Substring("C".Length))));
                }
                else
                {
                    XI.RaiseGMessage("G0HQ,3," + player.ROMUshort + "," + player.Uid + "," + tokens.Count +
                        "," + string.Join(",", tokens.Select(p => p.Substring("C".Length))));
                    if (player.Tux.Count > 0)
                    {
                        string select = XI.AsyncInput(player.Uid, "#交还,Q1(p" + string.Join("p", player.Tux) + ")",
                            "JNT4101", "0");
                        if (select != VI.CinSentinel)
                        {
                            ushort back = ushort.Parse(select);
                            XI.RaiseGMessage("G0HQ,0," + who + "," + player.Uid + ",1,1," + back);
                        }
                    }
                }
                player.ROMUshort = 0;
            }
            else if (type == 2 || type == 3)
            {
                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                // linkHeads = { "TP02,0", "TP03,0" };
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                List<string> excl = player.TokenExcl.ToList();
                foreach (string ccode in excl)
                {
                    ushort ut = ushort.Parse(ccode.Substring("C".Length));
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    if (tux != null && !tux.IsTuxEqiup())
                        for (int i = 0; i < linkHeads.Length; ++i)
                        {
                            int idx = linkHeads[i].IndexOf(',');
                            string pureName = linkHeads[i].Substring(0, idx);
                            string pureTypeStr = linkHeads[i].Substring(idx + 1);
                            if (!pureTypeStr.Contains("!"))
                            {
                                ushort pureType = ushort.Parse(pureTypeStr);
                                if (tux.Code == pureName && tux.Bribe(player, pureType, pureFuse)
                                            && tux.Valid(player, pureType, pureFuse))
                                {
                                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,C" + ut);
                                    XI.RaiseGMessage("G2TZ,0," + player.Uid + ",C" + ut);
                                    if ((tux.IsEq[pureType] & 3) == 0)
                                        XI.RaiseGMessage("G0ON," + player.Uid + ",C,1," + ut);
                                    else
                                        XI.Board.PendingTux.Enqueue(player.Uid + ",G0CC," + ut);
                                    if (tux.Type == Base.Card.Tux.TuxType.ZP)
                                        XI.RaiseGMessage("G0CZ,0," + player.Uid);
                                    XI.InnerGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," +
                                        tux.Code + "," + ut + ";" + pureType + "," + pureFuse, 101);
                                    break;
                                }
                            }
                        }
                    else if (tux != null)
                    {
                        XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,C" + ut);
                        XI.RaiseGMessage("G0ZB," + player.Uid + ",1," + player.Uid + ",0,0," + ut);
                        break;
                    }
                }
            }
        }
        public string JNT4101Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                        p.Uid != player.Uid && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
                else if (prev.IndexOf(',') < 0)
                {
                    ushort who = ushort.Parse(prev);
                    return "C1(" + Util.RepeatString("p0", XI.Board.Garden[who].Tux.Count) + ")";
                }
                else
                    return "";
            }
            else if (type == 2 || type == 3)
            {
                if (prev == "")
                {
                    List<ushort> candidates = new List<ushort>();
                    string linkFuse = fuse;
                    int lfidx = linkFuse.IndexOf(':');
                    string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                    string pureFuse = linkFuse.Substring(lfidx + 1);

                    foreach (string ccard in player.TokenExcl)
                    {
                        ushort ut = ushort.Parse(ccard.Substring("C".Length));
                        Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux != null)
                        {
                            for (int i = 0; i < linkHeads.Length; ++i)
                            {
                                int idx = linkHeads[i].IndexOf(',');
                                string pureName = linkHeads[i].Substring(0, idx);
                                string pureTypeStr = linkHeads[i].Substring(idx + 1);
                                if (!pureTypeStr.Contains("!"))
                                {
                                    ushort pureType = ushort.Parse(pureTypeStr);
                                    if (tux.Code == pureName && tux.Bribe(player, pureType, pureFuse)
                                                && tux.Valid(player, pureType, pureFuse))
                                    {
                                        candidates.Add(ut); break;
                                    }
                                }
                            }
                        }
                    }
                    return "/C1(p" + string.Join("p", candidates) + ")";
                }
                else return "";
            }
            return "";
        }
        public bool JNT4102Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.OppTeam && p.Tux.Count > 1);
        }
        public void JNT4102Action(Player player, int type, string fuse, string argst)
        {
            int incr = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam).Max(p => p.Tux.Count) - 1;
            if (incr > 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + incr);
        }

        #endregion TR041 - Shuoxuan
        #region TR042 - Linyuan
        public bool JNT4201Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => XI.Board.Garden[p.Who].IsAlive && XI.Board.Garden[p.Who].Tux.Count <= p.N);
        }
        public void JNT4201Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Player> tos = harms.Where(p => XI.Board.Garden[p.Who].IsAlive && p.Element != FiveElement.LOVE &&
                XI.Board.Garden[p.Who].Tux.Count <= p.N).Select(p => XI.Board.Garden[p.Who]).Distinct().ToList();
            Cure(player, tos, 1);
        }
        public bool JNT4202Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0) && player.ROMUshort == 0;
        }
        public void JNT4202Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            player.ROMUshort = 1;
            XI.RaiseGMessage("G0TT," + tar);
            int result = XI.Board.DiceValue;
            Cure(player, XI.Board.Garden[tar], result);
            XI.RaiseGMessage("G0IF," + tar + ",3,4");
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", 0);
        }
        public string JNT4202Input(Player player, int type, string fuse, string prev)
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
        #endregion TR042 - Linyuan
        #region TR043 - Zhuyu
        public bool JNT4301Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public void JNT4301Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort ut = ushort.Parse(argst.Substring(0, idx));
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + ut);
            ushort tar = ushort.Parse(argst.Substring(idx + 1));
            if (tar == player.Uid)
                XI.RaiseGMessage("G0IF," + player.Uid + ",2");
            else
                XI.RaiseGMessage("G0IF," + tar + ",4");
        }
        public string JNT4301Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#获得「坚盾」或「神行」,/T1" + ATeammatesTared(player);
            else return "";
        }
        public bool JNT4302Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Runes.Count > 0);
        }
        public void JNT4302Action(Player player, int type, string fuse, string argst)
        {
            int team = int.Parse(argst);
            List<Player> teammates = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == team && p.Runes.Count > 0).ToList();
            int count = teammates.Sum(p => p.Runes.Count);
            if (count > 0)
            {
                teammates.ForEach(p => XI.RaiseGMessage("G0OF," + p.Uid + "," + string.Join(",", p.Runes)));
                if (team == player.Team)
                    Cure(player, player, count);
                else if (team == player.OppTeam)
                    Harm(player, player, count);
            }
        }
        public string JNT4302Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "S" : "";
        }
        #endregion TR043 - Zhuyu
        #region TR044 - Suhe
        public bool JNT4401Valid(Player player, int type, string fuse)
        {
            Func<string, bool> joinable = (p) =>
            {
                if (p.StartsWith("!"))
                    return false;
                int avatar = int.Parse(p);
                Hero hero = XI.LibTuple.HL.InstanceHero(avatar);
                return hero != null && Artiad.ContentRule.IsHeroJoinable(hero, XI);
                // check whether the hero is joinable or not (e.g. HL005)
            };
            return XI.Board.Garden.Values.Any(p => p.Team == player.Team && p.IsTared &&
                 (!player.ROM.ContainsKey("Changed") || !((List<ushort>)player.ROM["Changed"]).Contains(p.Uid))) &&
                XI.Board.Garden.Values.Any(p => p.IsTared && XI.LibTuple.HL.InstanceHero(p.SelectHero).Spouses.Any(q => joinable(q)));
        }
        public void JNT4401Action(Player player, int type, string fuse, string argst)
        {
            ushort[] args = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            ushort who = args[0];
            int avatar = args[2];

            if (!player.ROM.ContainsKey("Changed"))
                player.ROM["Changed"] = new List<ushort>();
            ((List<ushort>)player.ROM["Changed"]).Add(who);

            int hp = XI.Board.Garden[who].HP;
            XI.RaiseGMessage("G0OY,0," + who);
            XI.RaiseGMessage("G0IY,2," + who + "," + avatar + "," + hp);

            List<ushort> hasCard = XI.Board.Garden.Values.Where(p => p.Team == player.Team &&
                 p.IsTared && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            if (hasCard.Count > 0)
            {
                string discardAll = XI.AsyncInput(player.Uid, "#弃置手牌以回复,/T1(p" +
                    string.Join("p", hasCard) + ")", "JNT4401", "0");
                if (!discardAll.StartsWith("/") && !discardAll.Contains(VI.CinSentinel))
                {
                    ushort dwho = ushort.Parse(discardAll);
                    int dhp = XI.Board.Garden[dwho].Tux.Count;
                    XI.RaiseGMessage("G0QZ," + dwho + "," + string.Join(",", XI.Board.Garden[dwho].Tux));
                    Cure(player, XI.Board.Garden[who], dhp);
                }
            }
        }
        public string JNT4401Input(Player player, int type, string fuse, string prev)
        {
            Func<string, bool> joinable = (p) =>
            {
                if (p.StartsWith("!"))
                    return false;
                int avatar = int.Parse(p);
                Hero hero = XI.LibTuple.HL.InstanceHero(avatar);
                return hero != null && Artiad.ContentRule.IsHeroJoinable(hero, XI);
                // check whether the hero is joinable or not (e.g. HL005)
            };
            if (prev == "")
            {
                return "#进行变身,/T1" + FormatPlayers(p => p.Team == player.Team && p.IsTared && (!player.ROM.ContainsKey(
                    "Changed") || !((List<ushort>)player.ROM["Changed"]).Contains(p.Uid))) + ",#倾慕者,/T1"
                    + FormatPlayers(p => p.IsTared && XI.LibTuple.HL.InstanceHero(p.SelectHero).Spouses.Any(q => joinable(q)));
            }
            else if (prev.IndexOf(',', (prev.IndexOf(',') + 1)) < 0)
            {
                ushort target = ushort.Parse(prev.Substring(prev.IndexOf(',') + 1));
                return "/H1(p" + string.Join("p", XI.LibTuple.HL.InstanceHero(XI.Board.Garden[target].SelectHero)
                    .Spouses.Where(p => joinable(p))) + ")";
            }
            else return "";
        }
        public bool JNT4402Valid(Player player, int type, string fuse)
        {
            string[] g0iy = fuse.Split(',');
            return g0iy[1] != "1";
        }
        public void JNT4402Action(Player player, int type, string fuse, string argst)
        {
            string select = XI.AsyncInput(player.Uid, "#请选择「归墟」执行项##战力+1##命中+1,Y2", "JNT4402", "0");
            if (select == "2")
                XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
            else
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        #endregion TR044 - Suhe
        #region TR045 - Cangfeng
        public bool JNT4501Valid(Player player, int type, string fuse)
        {
            return XI.Board.Supporter.Uid == player.Uid && XI.Board.IsAttendWarSucc(XI.Board.Hinder);
        }
        public void JNT4501Action(Player player, int type, string fuse, string argst)
        {
            if (argst == "2")
                XI.RaiseGMessage("G0DH," + XI.Board.Rounder.Uid + ",0,1");
            else
                XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,2");
        }
        public string JNT4501Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "#请选择触发者执行项##战力+2##补1张牌,/Y2" : "";
        }
        public bool JNT4502BKValid(Player player, int type, string fuse, ushort owner)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return player.Tux.Count > 0 && harms.Any(p => XI.Board.Garden[owner].Team == player.Team &&
                p.Who != owner && p.Who == player.Uid && Artiad.Harm.GetPropedElement().Contains(p.Element) && p.N > 0);
        }
        public void JNT4502Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort to = ushort.Parse(argst.Substring(0, idx)); // to = TR045
            ushort card = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + card);
            string insert = XI.AsyncInput(to, "#您获取,F1(p" + string.Join("p",
                XI.LibTuple.RL.GetFullAppendableList()) + ")", "JNT4502", "0");
            if (!string.IsNullOrEmpty(insert) && !insert.StartsWith(VI.CinSentinel))
            {
                ushort rune = ushort.Parse(insert);
                XI.RaiseGMessage("G0IF," + to + "," + rune);
            }
        }
        public string JNT4502Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "";
            else if (prev.IndexOf(',') < 0)
                return "#交予的,/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNT4503Valid(Player player, int type, string fuse)
        {
            return player.Runes.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid);
        }
        public void JNT4503Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort rune = ushort.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0OF," + player.Uid + "," + rune);
            XI.RaiseGMessage("G0IF," + to + "," + rune);
        }
        public string JNT4503Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/F1(p" + string.Join("p", player.Runes) + "),/T1" + AOthersTared(player);
            else
                return "";
        }
        #endregion TR045 - Cangfeng
    }
}