using PSD.Base;
using PSD.Base.Card;
using PSD.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

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
            return Artiad.Harm.Parse(fuse).Any(p => XI.Board.Garden[p.Who].Team == player.Team &&
                !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && p.N > 0 &&
                (p.Element == FiveElement.AQUA || p.Element == FiveElement.AGNI));
        }
        public void JNT0102Action(Player player, int type, string fuse, string argst)
        {
            // G0OH,A,Src,p,n,...
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<ushort> invs = harms.Where(p => XI.Board.Garden[p.Who].Team == player.Team &&
                !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && p.N > 0 &&
                (p.Element == FiveElement.AQUA || p.Element == FiveElement.AGNI)).Select(p => p.Who).ToList();
            TargetPlayer(player.Uid, invs);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py.Team == player.Team && !HPEvoMask.IMMUNE_INVAO.IsSet(harm.Mask) &&
                    (harm.Element == FiveElement.AQUA || harm.Element == FiveElement.AGNI))
                {
                    if (--harm.N <= 0)
                        rvs.Add(harm);
                }
            }
            harms.RemoveAll(p => rvs.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -19);
        }
        #endregion TR001 - Suyu
        #region TR002 - XuChangqing
        public bool JNT0201Valid(Player player, int type, string fuse)
        {
            if (!player.RFM.GetBool("InJNT0202"))
            { // Not valid in JNT0202
                Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
                Func<ushort, bool> enemy = p => XI.Board.Garden[p].Team == player.OppTeam;
                return enemy(opt.Farmer) && (opt.Farmland == 0 || !enemy(opt.Farmland));
            }
            else return false;
        }
        public void JNT0201Action(Player player, int type, string fuse, string argst)
        {
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            for (int i = 0; i < opt.Pets.Length; ++i)
            {
                string input = XI.AsyncInput(player.Uid, "#获得2张补牌,T1" +
                    ATeammatesTared(player), "JNT0201", "0");
                ushort who = ushort.Parse(input);
                if (who != 0)
                    XI.RaiseGMessage("G0DH," + who + ",0,2");
            }
        }
        public bool JNT0202Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                bool self = XI.Board.Rounder.Uid == player.Uid;
                bool isWin = XI.Board.IsBattleWin;
                Monster m1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                Monster m2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                return self && !isWin && ((m2 != null && m2.Level != Monster.ClLevel.BOSS) ||
                    (m1 != null && XI.Board.Mon1From == 0 && m1.Level != Monster.ClLevel.BOSS));
            }
            else if (type == 1)
            {
                string[] g1ck = fuse.Split(',');
                return g1ck[1] == player.Uid.ToString() && g1ck[2] == "JNT0202";
            }
            else
                return false;
        }
        public void JNT0202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                Monster m1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                Monster m2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (m1 != null && m1.Level != Monster.ClLevel.BOSS)
                    XI.RaiseGMessage("G1CK," + player.Uid + ",JNT0202,0");
                if (m2 != null && m2.Level != Monster.ClLevel.BOSS)
                    XI.RaiseGMessage("G1CK," + player.Uid + ",JNT0202,1");
            }
            else if (type == 1)
            {
                string[] g1ck = fuse.Split(',');
                bool first = g1ck[3] == "0";
                ushort monFrom = first ? XI.Board.Mon1From : (ushort)0;
                ushort monIt = first ? XI.Board.Monster1 : XI.Board.Monster2;

                if (argst != "")
                {
                    player.RFM.Set("InJNT0202", true);
                    int idx = argst.IndexOf(',');
                    ushort from = ushort.Parse(argst.Substring(0, idx));
                    ushort pet = ushort.Parse(argst.Substring(idx + 1));

                    string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#获得宠物,T1" + FormatPlayers(
                        p => p.IsAlive && p.Team == player.OppTeam), "JNT0202", "0");
                    ushort to = ushort.Parse(input);
                    XI.RaiseGMessage(new Artiad.HarvestPet()
                    {
                        Farmer = to,
                        Farmland = from,
                        SinglePet = pet,
                        Reposit = true,
                        Plow = true
                    }.ToMessage());
                    player.RFM.Set("InJNT0202", false);
                }
                XI.RaiseGMessage(new Artiad.HarvestPet()
                {
                    Farmer = player.Uid,
                    Farmland = monFrom,
                    SinglePet = monIt,
                    Reposit = false,
                    Plow = true,
                    Trophy = true,
                    TreatyAct = Artiad.HarvestPet.Treaty.PASSIVE
                }.ToMessage());
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
            int idx = argst.IndexOf(',');
            ushort pt = ushort.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage(new Artiad.HarvestPet()
            {
                Farmer = to,
                Farmland = player.Uid,
                SinglePet = pt,
                TreatyAct = Artiad.HarvestPet.Treaty.PASSIVE
            }.ToMessage());
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
        }
        public string JNT0302Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/M1(p" + string.Join("p", player.Pets.Where(p => p != 0)) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#交给宠物,/T1" + AFriendsTared(player);
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
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse).Where(p => p.Who == player.Uid && p.N > 0 &&
                    !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask) &&
                    !HPEvoMask.TERMIN_AT.IsSet(p.Mask)).ToList();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Element == FiveElement.A && player.TokenExcl.Contains("I4"))
                        return true;
                    else if (harm.Element.IsStandardPropedElement() &&
                            player.TokenExcl.Contains("I" + harm.Element.Elem2Int()))
                        return true;
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
                Artiad.Procedure.RotateHarm(player, XI.Board.Garden[to], false, (v) => v - 1, ref harms);
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
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse).Where(p => p.Who == player.Uid && p.N > 0 &&
                    !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask) &&
                    !HPEvoMask.TERMIN_AT.IsSet(p.Mask)).ToList();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Element == FiveElement.A && player.TokenExcl.Contains("I4"))
                        cands.Add(4);
                    else if (harm.Element.IsStandardPropedElement() &&
                            player.TokenExcl.Contains("I" + harm.Element.Elem2Int()))
                        cands.Add(harm.Element.Elem2Int());
                }
                return "/I1(p" + string.Join("p", cands.Select(p => "I" + p)) + "),#转移,/T1" + AAllTareds(player);
            }
            else
                return "";
        }
        public bool JNT0402Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            bool has1 = mon1 != null && player.TokenExcl.Contains("I" + mon1.Element.Elem2Int());
            Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            bool has2 = mon2 != null && player.TokenExcl.Contains("I" + mon1.Element.Elem2Int());
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
                if (mon1 != null && player.TokenExcl.Contains("I" + mon1.Element.Elem2Int()))
                    sets.Add(mon1.Element.Elem2Int());
                Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2 != null && player.TokenExcl.Contains("I" + mon2.Element.Elem2Int()))
                    sets.Add(mon2.Element.Elem2Int());
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
                return "#转化为战力,/D1" + ((player.DEX == 1) ? "" : ("~" + player.DEX));
            else return "";
        }
        public void JNT0502Action(Player player, int type, string fuse, string argst)
        {
            string target = XI.AsyncInput(player.Uid, "#无宠物效果,T1" + AOthersTared(player), "JNT0502", "0");
            ushort who = ushort.Parse(target);
            TargetPlayer(player.Uid, who);
            XI.RaiseGMessage(new Artiad.DisablePlayerPetEffect() { SingleWho = who }.ToMessage());
            XI.SendOutUAMessage(player.Uid, "JNT0502," + target, "0");
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
            player.RAM.Set("TuxTypeMask", player.RAM.GetInt("TuxTypeMask") | typeCode);
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
                if ((player.RAM.GetInt("TuxTypeMask") & 0x1) != 0)
                    types.Add(Base.Card.Tux.TuxType.JP);
                else if ((player.RAM.GetInt("TuxTypeMask") & 0x2) != 0)
                    types.Add(Base.Card.Tux.TuxType.ZP);
                if ((player.RAM.GetInt("TuxTypeMask") & 0x4) != 0)
                    types.Add(Base.Card.Tux.TuxType.TP);
                if ((player.RAM.GetInt("TuxTypeMask") & 0x8) != 0)
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
            Player opy = XI.Board.Garden[owner];
            if (player.Team != opy.Team)
                return false;
            int lfidx = linkFuse.IndexOf(':');
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            foreach (string linkHeadComp in linkHeads)
            {
                int lastIdx = linkHeadComp.LastIndexOf(',');
                string linkHead = linkHeadComp.Substring(0, lastIdx);
                string rawFuse = linkHeadComp.Substring(lastIdx + 1);
                if (!Artiad.ContentRule.IsFuseMatch(rawFuse, fuse, XI.Board))
                    continue;
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JNT0701"))
                        continue;
                    int rcmIdx = rlink.IndexOf(',');
                    string rName = Algo.Substring(rlink, 0, rcmIdx);
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
                        {
                            string tfuse = tux.IsLinked(tType) ? linkFuse : fuse;
                            if (tux.Bribe(opy, tType, fuse) && tux.Valid(opy, tType, tfuse))
                                return true;
                        }
                    }
                    else
                    {
                        int tConsType = int.Parse(rlink.Substring(pdIdx + 1));
                        int tType = int.Parse(Algo.Substring(rlink, rcmIdx + 1, pdIdx));
                        foreach (ushort ut in player.ListOutAllEquips())
                        {
                            TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as TuxEqiup;
                            if (tue != null && tue.Code == rName)
                            {
                                string tfuse = tue.IsLinked(tConsType, tType) ? linkFuse : fuse;
                                if (tue.ConsumeValidHolder(player, opy, tConsType, tType, tfuse))
                                    return true;
                            }
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

            ushort targetTo, tuxTo; string rest;
            if (prev == "") { return ""; }
            else if (prev.IndexOf(',') < 0)
            {
                targetTo = ushort.Parse(prev);
                tuxTo = 0; rest = "";
            }
            else
            {
                string[] parts = prev.Split(',');
                targetTo = ushort.Parse(parts[0]);
                tuxTo = ushort.Parse(parts[1]);
                rest = string.Join(",", Algo.TakeRange(parts, 2, parts.Length));
            }
            List<ushort> usefulTux = new List<ushort>();
            string nextAsk = "";

            System.Func<ushort, Player, int, bool> tuxUsable = (ut, py, tType) =>
            {
                Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux == null) { return false; }
                string tfuse = tux.IsLinked(tType) ? linkFuse : fuse;
                return tux.Bribe(player, tType, tfuse) && tux.Valid(py, tType, tfuse);
            };
            System.Func<ushort, Player, int, int, bool> eqUsable = (ut, py, tConsume, tType) =>
            {
                TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as TuxEqiup;
                if (tue == null) { return false; }
                string tfuse = tue.IsLinked(tConsume, tType) ? linkFuse : fuse;
                return tue.ConsumeValidHolder(player, py, tConsume, tType, tfuse);
            };

            foreach (string linkHeadComp in linkHeads)
            {
                int lastIdx = linkHeadComp.LastIndexOf(',');
                string linkHead = linkHeadComp.Substring(0, lastIdx);
                string rawFuse = linkHeadComp.Substring(lastIdx + 1);
                if (!Artiad.ContentRule.IsFuseMatch(rawFuse, fuse, XI.Board))
                    continue;
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JNT0701"))
                        continue;
                    int rcmIdx = rlink.IndexOf(',');
                    string rName = Algo.Substring(rlink, 0, rcmIdx);
                    int pdIdx = rlink.IndexOf('!', rcmIdx + 1);
                    if (pdIdx < 0) // Not equip special case
                    {
                        int tType = int.Parse(rlink.Substring(rcmIdx + 1));
                        if (tuxTo == 0)
                        {
                            Player py = XI.Board.Garden[targetTo];
                            usefulTux.AddRange(player.Tux.Where(p =>
                                XI.LibTuple.TL.DecodeTux(p).Code == rName && tuxUsable(p, py, tType)));
                        }
                        else
                        {
                            Player py = XI.Board.Garden[targetTo];
                            Tux tux = XI.LibTuple.TL.DecodeTux(tuxTo);
                            if (tux != null && tux.Code == rName)
                            {
                                string tfuse = tux.IsLinked(tType) ? linkFuse : fuse;
                                nextAsk = tux.InputHolder(player, py, tType, tfuse, rest); break;
                            }
                        }
                    }
                    else
                    {
                        int tConsType = int.Parse(Algo.Substring(rlink, rcmIdx + 1, pdIdx));
                        int tType = int.Parse(rlink.Substring(pdIdx + 1));
                        if (tuxTo == 0)
                        {
                            Player py = XI.Board.Garden[targetTo];
                            usefulTux.AddRange(player.ListOutAllEquips().Where(p =>
                                XI.LibTuple.TL.DecodeTux(p).Code == rName && eqUsable(p, py, tConsType, tType)));
                        }
                        else
                        {
                            Player py = XI.Board.Garden[targetTo];
                            TuxEqiup tue = XI.LibTuple.TL.DecodeTux(tuxTo) as TuxEqiup;
                            if (tue != null && tue.Code == rName)
                            {
                                string tfuse = tue.IsLinked(tConsType, tType) ? linkFuse : fuse;
                                nextAsk = tue.ConsumeInputHolder(player, py, tConsType, tType, tfuse, rest); break;
                            }
                        }
                    }
                }
            }
            if (tuxTo == 0)
                return usefulTux.Count > 0 ? "/Q1(p" + string.Join("p", usefulTux.Distinct()) + ")" : "/";
            else
                return nextAsk;
        }
        public void JNT0701Action(Player player, int type, string linkFuse, string argst)
        {
            int ichicm = argst.IndexOf(',');
            int nicm = argst.IndexOf(',', ichicm + 1);
            ushort to = ushort.Parse(argst.Substring(0, ichicm));
            ushort ut = ushort.Parse(Algo.Substring(argst, ichicm + 1, nicm));
            string crest = nicm < 0 ? "" : ("," + argst.Substring(nicm + 1));

            int lfidx = linkFuse.IndexOf(':');
            // linkHeads = { "TP02,0", "TP03,0" };
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
            foreach (string linkHeadComp in linkHeads)
            {
                int lastIdx = linkHeadComp.LastIndexOf(',');
                string linkHead = linkHeadComp.Substring(0, lastIdx);
                string rawFuse = linkHeadComp.Substring(lastIdx + 1);
                if (!Artiad.ContentRule.IsFuseMatch(rawFuse, fuse, XI.Board))
                    continue;
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JNT0701"))
                        continue;
                    int rcmIdx = rlink.IndexOf(',');
                    string rName = Algo.Substring(rlink, 0, rcmIdx);
                    string inTypeStr = Algo.Substring(rlink, rcmIdx + 1, -1);
                    if (tux.Code == rName)
                    {
                        TargetPlayer(player.Uid, to);
                        if (tux.IsTuxEqiup() && inTypeStr.Contains('!'))
                        {
                            int sancm = inTypeStr.IndexOf('!');
                            ushort consumeCode = ushort.Parse(inTypeStr.Substring(0, sancm));
                            ushort inTypeCode = ushort.Parse(inTypeStr.Substring(sancm + 1));
                            string tfuse = (tux as TuxEqiup).IsLinked(consumeCode, inTypeCode) ? linkFuse : fuse;
                            XI.RaiseGMessage("G0ZC," + player.Uid + "," + (3 + consumeCode) + "," +
                                ut + "," + to + crest + ";" + inTypeCode + "," + tfuse);
                        }
                        else
                        {
                            string tfuse = tux.IsLinked(int.Parse(inTypeStr)) ? linkFuse : fuse;
                            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + to + "," + tux.Code
                                    + "," + ut + ";" + inTypeStr + "," + tfuse);
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
            else if (type == 3 && Artiad.ClothingHelper.IsStandard(fuse)) // ZB
            {
                Artiad.EquipStandard eis = Artiad.EquipStandard.Parse(fuse);
                return eis.Who == player.Uid && eis.Cards.Any(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.FJ);
            }
            else if (type == 4) // OT
            {
                string[] g0ot = fuse.Split(',');
                int idx = 1;
                while (idx < g0ot.Length)
                {
                    ushort ut = ushort.Parse(g0ot[idx]);
                    int n = int.Parse(g0ot[idx + 1]);
                    List<ushort> tuxs = Algo.TakeRange(g0ot, idx + 2, idx + 2 + n)
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
                Cure(player, player, 2);
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
            XI.RaiseGMessage(new Artiad.EquipStandard()
            {
                Who = to, Source = from, SingleCard = tux, SlotAssign = true
            }.ToMessage());
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
                List<ushort> cands = XI.Board.Garden.Values.Where(p => p.Uid != ut && p.IsTared &&
                    p.GetSlotCapacity(tux.Type) > p.GetCurrentEquipCount(tux.Type)).Select(p => p.Uid).ToList();
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
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNT0706Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.HP < player.HPb;
            else if (type == 1)
            {
                ushort r = XI.Board.Rounder.Uid;
                return player.Uid != r && player.ROM.GetOrSetUshortArray("Away").Contains(r);
            }
            else if (type == 2)
            {
                return IsMathISOS("JNT0706", player, fuse) &&
                    player.ROM.GetOrSetUshortArray("Away").Count > 0;
            }
            else if (type == 3) // Loss the tag if leave without self-change
            {
                string[] g0oy = fuse.Split(',');
                for (int i = 1; i < g0oy.Length; i += 2)
                {
                    ushort utype = ushort.Parse(g0oy[i]);
                    ushort who = ushort.Parse(g0oy[i + 1]);
                    if (utype != 1 && player.ROM.GetOrSetUshortArray("Away").Contains(who))
                        return true;
                }
                return false;
            }
            else if (type == 4) // Decrease the DEX value if exists in Away
            {
                string[] g0iy = fuse.Split(',');
                for (int i = 1; i < g0iy.Length; )
                {
                    ushort utype = ushort.Parse(g0iy[i]);
                    ushort who = ushort.Parse(g0iy[i + 1]);
                    if (player.ROM.GetOrSetUshortArray("Away").Contains(who))
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
                player.ROM.Set("AwayValue", player.HPb - player.HP);
                List<ushort> list = new List<ushort>();
                XI.Board.Garden.Values.Where(p => p.IsAlive && p.Uid != player.Uid).ToList().ForEach(p =>
                {
                    XI.RaiseGMessage("G0OX," + p.Uid + ",0," + player.ROM.GetInt("AwayValue"));
                    list.Add(p.Uid);
                });
                TargetPlayer(player.Uid, list);
                player.ROM.Set("Away", list);
            }
            else if (type == 1)
            {
                ushort r = XI.Board.Rounder.Uid;
                player.ROM.GetOrSetUshortArray("Away").Remove(r);
                XI.RaiseGMessage("G0IX," + r + ",0," + player.ROM.GetInt("AwayValue"));
            }
            else if (type == 2)
            {
                player.ROM.GetOrSetUshortArray("Away").ForEach(p => XI.RaiseGMessage(
                    "G0IX," + p + ",0," + player.ROM.GetInt("AwayValue")));
                player.ROM.Set("AwayValue", null);
                player.ROM.Set("Away", null);
            }
            else if (type == 3)
            {
                string[] g0oy = fuse.Split(',');
                for (int i = 1; i < g0oy.Length; i += 2)
                {
                    ushort utype = ushort.Parse(g0oy[i]);
                    ushort who = ushort.Parse(g0oy[i + 1]);
                    if (utype != 1)
                        player.ROM.GetOrSetUshortArray("Away").Remove(who);
                }
            }
            else if (type == 4)
            {
                List<ushort> ins = new List<ushort>();
                string[] g0iy = fuse.Split(',');
                for (int i = 1; i < g0iy.Length;)
                {
                    ushort utype = ushort.Parse(g0iy[i]);
                    ushort who = ushort.Parse(g0iy[i + 1]);
                    if (player.ROM.GetOrSetUshortArray("Away").Contains(who))
                    {
                        XI.RaiseGMessage("G0OX," + who + ",0," + player.ROM.GetInt("AwayValue"));
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
                Diva diva = player.ROM.GetDiva("Ice");
                if (diva != null)
                {
                    string[] g0oy = fuse.Split(',');
                    for (int i = 1; i < g0oy.Length; i += 2)
                    {
                        ushort ytype = ushort.Parse(g0oy[i]);
                        ushort ut = ushort.Parse(g0oy[i + 1]);
                        if (XI.Board.Garden[ut].Team == player.Team && ut != player.Uid
                            && (ytype == 0 || ytype == 2))
                        {
                            string us = ut.ToString();
                            if (diva.GetInt(us) == 1 || diva.GetInt(us) == 2)
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
                foreach (ushort ut in XI.Board.OrderedPlayer(player.Uid))
                {
                    string us = ut.ToString();
                    Player py = XI.Board.Garden[ut];
                    if (py.Team == player.Team && py.IsAlive)
                    {
                        string choose = XI.AsyncInput(ut, "#请选择『水御灵』执行项目。" +
                            "##战力+1##命中+1,Y2", "JNT0707", "0");
                        if (choose == "1")
                        {
                            player.ROM.GetOrSetDiva("Ice").Set(us, 1);
                            XI.RaiseGMessage("G0IA," + ut + ",0,1");
                        }
                        else
                        {
                            player.ROM.GetOrSetDiva("Ice").Set(us, 2);
                            XI.RaiseGMessage("G0IX," + ut + ",0,1");
                        }
                    }
                }
            }
            else if (type == 1 && player.ROM.GetDiva("Ice") != null)
            {
                Diva diva = player.ROM.GetDiva("Ice");
                foreach (string key in diva.GetKeys())
                {
                    ushort ut = ushort.Parse(key);
                    if (diva.GetInt(key) == 1)
                        XI.RaiseGMessage("G0OA," + ut + ",0,1");
                    else if (diva.GetInt(key) == 2)
                        XI.RaiseGMessage("G0OX," + ut + ",0,1");
                }
            }
            else if (type == 2)
            {
                string[] g0iy = fuse.Split(',');
                ushort ut = ushort.Parse(g0iy[2]);
                string us = ut.ToString();
                string choose = XI.AsyncInput(ut, "#请选择『水御灵』执行项目。" +
                            "##战力+1##命中+1,Y2", "JNT0707", "0");
                if (choose == "1")
                {
                    player.ROM.GetOrSetDiva("Ice").Set(us, 1);
                    XI.RaiseGMessage("G0IA," + ut + ",0,1");
                }
                else
                {
                    player.ROM.GetOrSetDiva("Ice").Set(us, 2);
                    XI.RaiseGMessage("G0IX," + ut + ",0,1");
                }
            }
            else if (type == 3)
            {
                Diva diva = player.ROM.GetDiva("Ice");
                if (diva != null)
                {
                    string[] g0oy = fuse.Split(',');
                    for (int i = 1; i < g0oy.Length; i += 2)
                    {
                        ushort ytype = ushort.Parse(g0oy[i]);
                        string us = g0oy[i + 1];
                        ushort ut = ushort.Parse(us);
                        if (XI.Board.Garden[ut].Team == player.Team && ut != player.Uid
                            && (ytype == 0 || ytype == 2))
                        {
                            if (diva.GetInt(us) == 1)
                                XI.RaiseGMessage("G0OA," + ut + ",0,1");
                            else if (diva.GetInt(us) == 2)
                                XI.RaiseGMessage("G0OX," + ut + ",0,1");
                            diva.Set(us, null);
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
                    List<ushort> rms = Algo.TakeRange(g1di, idx + 4, idx + 4 + n - neq)
                        .Select(p => ushort.Parse(p)).ToList();
                    List<ushort> reqs = Algo.TakeRange(g1di, idx + 4 + n - neq, idx + 4 + n)
                        .Select(p => ushort.Parse(p)).ToList();
                    rest.AddRange(rms);
                    eqs.AddRange(reqs);
                }
                else
                    n1di += "," + string.Join(",", Algo.TakeRange(g1di, idx, idx + 4 + n));
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
                        tuxes.AddRange(Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
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
            return XI.Board.InCampaign && Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                p.N > 0 && !HPEvoMask.TERMIN_AT.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
        }
        public void JNT0710Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            harms.ForEach(p =>
            {
                if (p.Who == player.Uid && p.N > 0 &&
                    !HPEvoMask.TERMIN_AT.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask))
                { --p.N; }
            });
            harms.RemoveAll(p => p.N <= 0);
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
            Harm(player, player, n, FiveElement.SOLARIS);
            XI.RaiseGMessage("G0IA," + player.Uid + ",1," + n);
        }
        public string JNT0711Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#转化为战力,/D" + ((player.HP > 1) ? ("1~" + player.HP) : "1");
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
            return Artiad.Harm.Parse(fuse).Any(p =>
                XI.Board.Garden[p.Who].Team == player.Team && p.N > 0);
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
            List<Player> alls = XI.Board.Garden.Values.Where(p => p.IsAlive).ToList();
            TargetPlayer(player.Uid, alls.Select(p => p.Uid));
            Harm(player, alls, 1, FiveElement.YINN);
        }
        #endregion TR007 - LiYiru

        #region TR008 - XiahouJinxuan
        public bool JNT0801Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Garden.Values.Any(p => p.IsTared &&
                    p.Uid != player.Uid && p.Team == player.Team);
            else if (type == 1 || type == 2)
                return player.RFM.GetUshort("Flag") == XI.Board.Rounder.Uid;
            else if (type == 3) // G0JM,R^ED
            {
                int idx = fuse.IndexOf(',');
                string stage = fuse.Substring(idx + 1);
                return stage == "R" + player.RFM.GetUshort("Flag") + "ED";
            }
            else if (type == 4) // G0OY,0/1/2,A
            {
                string[] g0oys = fuse.Split(',');
                if (g0oys[1] == "2" && player.RFM.GetUshort("Flag") != 0)
                {
                    for (int i = 2; i < g0oys.Length; ++i)
                        if (g0oys[i] == player.Uid.ToString())
                            return player.RFM.GetUshort("Flag") == XI.Board.Rounder.Uid;
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
                player.RFM.Set("Flag", tar);
                XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                {
                    Role = Artiad.CoachingHelper.PType.HORN, Coach = tar
                } }.ToMessage());
                XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + tar + "ZW" }.ToMessage());
            }
            else if (type == 1)
                XI.Board.AllowNoSupport = false;
            else if (type == 2)
            {
                player.RFM.Set("Flag", null);
                // Mark the target back.
                XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + player.Uid + "ZZ" }.ToMessage());
            }
            else if (type == 3)
                XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + player.Uid + "ZZ" }.ToMessage());
            else if (type == 4)
            {
                ushort ut = player.RFM.GetUshort("Flag");
                XI.Board.JumpTable["R" + ut + "ZZ"] =
                    "R" + player.Uid + "ED,R" + ut + "ED";
                XI.Board.JumpTable["R" + ut + "ED"] =
                    "R" + player.Uid + "ED,R" + ut + "ZZ";
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
            FiveElement[] props = FiveElementHelper.GetStandardPropedElements();
            List<Player> ops = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).ToList();
            ushort cnt = (ushort)props.Count(p => ops.Any(q => q.Pets[p.Elem2Index()] != 0));
            bool same = cnt == player.ROM.GetUshort("FivePaint");

            if (type == 0 || type == 1)
                return !same;
            else if (type == 2 || type == 3)
                return !same && IsMathISOS("JNT0803", player, fuse);
            return false;
        }
        public void JNT0803Action(Player player, int type, string fuse, string argst)
        {
            FiveElement[] props = FiveElementHelper.GetStandardPropedElements();
            List<Player> ops = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).ToList();
            ushort cnt = (ushort)props.Count(p => ops.Any(q => q.Pets[p.Elem2Index()] != 0));
            if (type == 0 || type == 1)
            {
                int ocnt = player.ROM.GetUshort("FivePaint");
                if (cnt > ocnt)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0," + (cnt - ocnt));
                else if (cnt < ocnt)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (ocnt - cnt));
                player.ROM.Set("FivePaint", cnt);
            }
            else if (type == 2)
            {
                player.ROM.Set("FivePaint", cnt);
                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + cnt);
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OA," + player.Uid + ",0," + cnt); 
                player.ROM.Set("FivePaint", null);
            }
        }
        #endregion TR008 - XiahouJinxuan
        #region TR009 - Xia
        //public void JNT0901Action(Player player, int type, string fuse, string argst)
        //{
        //    XI.RaiseGMessage("G0HQ,2," + player.Uid + ",1,1");
        //    XI.RaiseGMessage("G0PB,1,0," + player.Uid + ",1," + argst);
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
                    string input = XI.AsyncInput(me, "#获得补牌的,T1" + ATeammates(player), "G0ZW", "0");
                    XI.RaiseGMessage("G0HG," + input + ",2");
                }
            XI.InnerGMessage(fuse, 301);
        }
        public bool JNT0903Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                p.N >= XI.Board.Garden[p.Who].HP && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
        }
        public void JNT0903Action(Player player, int type, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            XI.RaiseGMessage("G0TT," + player.Uid);
            int value = XI.Board.DiceValue;
            if (value < 5)
            {
                harms.RemoveAll(p => p.Who == player.Uid &&
                    p.N >= XI.Board.Garden[p.Who].HP && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
            }
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -69);
        }
        #endregion TR009 - Xia
        #region TR010 - MuChanglan
        public bool JNT1001Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return Artiad.EqImport.Parse(fuse).Imports.Any(p => XI.Board.Garden[p.Who].Team ==
                    player.OppTeam && p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.WQ);
            }
            else if (type == 1)
            {
                return Artiad.EqExport.Parse(fuse).Exports.Any(p => XI.Board.Garden[p.Who].Team ==
                    player.OppTeam && p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.WQ);
            }
            else if (type == 2 || type == 3) // not registered yet
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
                return any && IsMathISOS("JNT1001", player, fuse);
            }
            else return false;
        }
        public void JNT1001Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int n = Artiad.EqImport.Parse(fuse).Imports.Count(p => XI.Board.Garden[p.Who].Team ==
                    player.OppTeam && p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.WQ);
                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + n);
            }
            else if (type == 1)
            {
                int n = Artiad.EqExport.Parse(fuse).Exports.Count(p => XI.Board.Garden[p.Who].Team ==
                    player.OppTeam && p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.WQ);
                XI.RaiseGMessage("G0OA," + player.Uid + ",0," + n);
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
            XI.AsyncInput(player.Uid, "C1(" + Algo.RepeatString("p0", py.Tux.Count) + ")", "JNT1003", "0");
            XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + t + ",2,1");
        }
        #endregion TR010 - MuChanglan
        #region TR011 - JiangCheng
        public bool JNT1101Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => p.Who != player.Uid && p.N > 0 && XI.Board.Garden[p.Who].IsTared &&
                XI.Board.Garden[p.Who].Team == player.Team && p.Source != p.Who && !new HPEvoMask[] {
                 HPEvoMask.TERMIN_AT, HPEvoMask.DECR_INVAO, HPEvoMask.CHAIN_INVAO }.Any(q => q.IsSet(p.Mask)));
        }
        public void JNT1101Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            ushort ut = ushort.Parse(argst);
            TargetPlayer(player.Uid, ut);
            Artiad.Procedure.RotateHarm(XI.Board.Garden[ut], player, false, (v) => v + 1, ref harms);
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -60);
        }
        public string JNT1101Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                return "#代为承受伤害,/T1(p" + string.Join("p", Artiad.Harm.Parse(fuse).Where(p => p.Who != player.Uid &&
                    p.N > 0 && XI.Board.Garden[p.Who].IsTared && XI.Board.Garden[p.Who].Team == player.Team &&
                    p.Source != p.Who && !new HPEvoMask[] { HPEvoMask.TERMIN_AT, HPEvoMask.DECR_INVAO,
                    HPEvoMask.CHAIN_INVAO }.Any(q => q.IsSet(p.Mask))).Select(p => p.Who).Distinct()) + ")";
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

                ushort gamer = ushort.Parse(XI.AsyncInput(player.Uid,
                    "T1" + ATeammates(player), "JNT1201", "0"));
                XI.RaiseGMessage("G0XZ," + gamer + ",2,0,1");

                if (XI.Board.PoolEnabled)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
                player.RFM.Set("Watched", player.RFM.GetInt("Watched") + 1);
            }
            else if (type == 2)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + player.RFM.GetInt("Watched"));
        }
        public bool JNT1201Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return player.Tux.Count > 0 && XI.Board.MonPiles.Count > 0;
            else if (type == 2)
                return player.RFM.GetInt("Watched") > 0;
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
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
        }
        #endregion TR012 - HuangfuZhuo
        #region TR013 - XieCangxing
        public bool JNT1301Valid(Player player, int type, string fuse)
        {
            bool b0 = XI.Board.IsAttendWar(player);
            bool b1 = player.HP >= player.RAM.GetInt("Sell") + 1;
            bool b2 = player.Weapon == 0;
            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(player.ExEquip);
            bool b3 = tux == null || tux.Type == Base.Card.Tux.TuxType.WQ;
            return b0 && b1 && b2 && b3;
        }
        public void JNT1301Action(Player player, int type, string fuse, string argst)
        {
            player.RAM.Set("Sell", player.RAM.GetInt("Sell") + 1);
            Harm(player, player, player.RAM.GetInt("Sell"));
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
            XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        #endregion TR013 - XieCangxing
        #region TR014 - Jieluo
        public bool JNT1401Valid(Player player, int type, string fuse) // No Action called, handled in ALIVE flag
        {
            return Artiad.Harm.Parse(fuse).Any(p => HPEvoMask.RSV_WORM.IsSet(p.Mask) && p.N > 0 &&
                XI.Board.Garden[p.Who].HP >= 2 && XI.Board.Garden[p.Who].HP - p.N < 1);
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
                        !HPEvoMask.DECR_INVAO.IsSet(harm.Mask) && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
                    thisHarm = harm;
                else if (harm.Who == ut)
                    thatHarm = harm;
            }
            if (thisHarm != null)
            {
                int a, b;
                a = thisHarm.N / 2;
                b = thisHarm.N - a;
                if (a > 0)
                    thisHarm.N = a;
                else { harms.Remove(thisHarm); }
                thisHarm.Mask = HPEvoMask.ALIVE_HARD.Set(HPEvoMask.RSV_WORM.Set(thisHarm.Mask));

                if (b > 0)
                {
                    if (thatHarm != null && thatHarm.Element == thisHarm.Element)
                        thatHarm.N += b;
                    else
                        harms.Add(thatHarm = new Artiad.Harm(ut, thisHarm.Source, thisHarm.Element,
                            b, thisHarm.Mask));
                    thatHarm.Mask = HPEvoMask.ALIVE_HARD.Set(HPEvoMask.RSV_WORM.Set(thatHarm.Mask));
                }
            }
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 91);
        }
        public bool JNT1402Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && p.Gender == 'M') &&
                Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N > 0 &&
                !HPEvoMask.DECR_INVAO.IsSet(p.Mask) && !HPEvoMask.TERMIN_AT.IsSet(p.Mask));
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
                if (!HPEvoMask.DECR_INVAO.IsSet(harm.Mask) && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
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
            totalAmount -= XI.Board.Garden.Values.Count(
                p => p.IsAlive && p.Uid != leifeng.Uid && p.Team == leifeng.Team);
            if (thisHarm != null)
            {
                thisHarm.N = totalAmount;
                thisHarm.Who = leifeng.Uid;
                thisHarm.Mask = HPEvoMask.ALIVE_HARD.Set(HPEvoMask.RSV_WORM.Set(thisHarm.Mask));
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
                    if (!HPEvoMask.DECR_INVAO.IsSet(harm.Mask) && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
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
                    if (!HPEvoMask.DECR_INVAO.IsSet(harm.Mask) && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
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
            string input = XI.AsyncInput(player.Uid, "#HP-1,/T1" + FormatPlayers(
                p => p.IsTared && p.Team == player.OppTeam), "JNT1501", "0");
            if (!input.StartsWith("/") && input != VI.CinSentinel)
                Harm(player, XI.Board.Garden[ushort.Parse(input)], 1, FiveElement.YINN);
        }
        public bool JNT1501Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.OppTeam) &&
                Artiad.Harm.Parse(fuse).Any(p => XI.Board.Garden[p.Who].Team == player.Team && p.N > 0 &&
                p.Who != player.Uid && p.Source != p.Who && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask));
        }
        public void JNT1502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0) // ST->[EP]->GR
            {
                player.RFM.Set("endEGR", 1);
                XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + player.Uid + "GS" }.ToMessage());
            }
            else if (type == 1) // GR->GE->[GF]->EV->GR->GE->...
            {
                player.RFM.Set("endEGR", 2);
                XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + player.Uid + "EV" }.ToMessage());
            }
            else if (type == 2)
                Cure(player, player, 1);
        }
        public bool JNT1502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.RFM.GetInt("endEGR") == 0;
            else if (type == 1)
                return player.RFM.GetInt("endEGR") == 1;
            else if (type == 2)
            {
                string[] g1evs = fuse.Split(',');
                return g1evs[1] == player.Uid.ToString();
            }
            else
                return false;
        }
        #endregion TR015 - Liyan
        #region TR016 - Xuanji
        public void JNT1601Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#获得2张补牌,T1" + FormatPlayers(p => p.IsTared &&
                !player.RAM.GetOrSetUshortArray("Worm").Contains(p.Uid)), "JNT1601", "0");
            ushort who = ushort.Parse(input);
            player.RAM.GetOrSetUshortArray("Worm").Add(who);
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,2");
        }
        public bool JNT1601Valid(Player player, int type, string fuse)
        {
            return Artiad.ClothingHelper.GetWho(fuse) == player.Uid && XI.Board.Garden.Values
                .Where(p => p.IsTared).Select(p => p.Uid).Except(player.RAM.GetOrSetUshortArray("Worm")).Any();
        }
        public void JNT1602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                player.RAM.Set("AquaHit", 1);
            else if (type == 1)
            {
                player.RAM.Set("AquaHit", 2);
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
                {
                    XI.RaiseGMessage(new Artiad.Goto()
                    {
                        CrossStage = false,
                        Terminal = "R" + XI.Board.Rounder.Uid + "VT"
                    }.ToMessage());
                }
            }
            else if (type == 3 || type == 5)
            {
                player.ROM.Set("AquaCaptured", true);
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
            }
            else if (type == 4 || type == 6)
            {
                player.ROM.Set("AquaCaptured", false);
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
                    return (player.Team == XI.Board.Rounder.Team && XI.Board.Supporter != null) ||
                        (player.Team == XI.Board.Rounder.OppTeam && XI.Board.Hinder != null);
                }
                return false;
            }
            else if (type == 1 && player.RAM.GetInt("AquaHit") == 1 && Artiad.PondRefresh.Parse(fuse).CheckHit)
            {
                return (player.Team == XI.Board.Rounder.Team && XI.Board.Supporter.Uid != 0) ||
                    (player.Team == XI.Board.Rounder.OppTeam && XI.Board.Hinder.Uid != 0);
            }
            else if (type == 2)
            {
                int aquaHitStatus = player.RAM.GetInt("AquaHit");
                return aquaHitStatus == 1 || aquaHitStatus == 2;
            }
            else if (type >= 3 && type <= 6)
            {
                int zidx = FiveElement.AQUA.Elem2Index();
                bool has = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.Team && p.Pets[zidx] != 0).Any();
                if (type == 3 || type == 5)
                {
                    bool b = !player.ROM.GetBool("AquaCaptured") && has;
                    if (type == 5)
                        b &= IsMathISOS("JNT1602", player, fuse);
                    return b;
                }
                else if (type == 4 || type == 6)
                {
                    bool b = player.ROM.GetBool("AquaCaptured") && has;
                    if (type == 6)
                        b &= IsMathISOS("JNT1602", player, fuse);
                    return b;
                }
            }
            return false;
        }
        #endregion TR016 - Xuanji
        #region TR017 - Huaishuo
        public void JNT1701Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                IDictionary<ushort, string> discards = new Dictionary<ushort, string>();
                TargetPlayer(player.Uid, XI.Board.Garden.Values.Where(p => p.IsAlive
                    && p.Team == player.Team && p.Tux.Count > 0).Select(p => p.Uid));
                foreach (Player py in XI.Board.Garden.Values)
                {
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
                            string.Join(",", Algo.TakeRange(parts, 2, parts.Length));
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
                if (py.Team == player.Team && py.IsTared && py.Tux.Count < py.TuxLimit && py.HP > harm.N &&
                    !HPEvoMask.TERMIN_AT.IsSet(harm.Mask) && !HPEvoMask.CHAIN_INVAO.IsSet(harm.Mask))
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
                if (py.Team == player.Team && py.IsTared && py.Tux.Count < py.TuxLimit && py.HP > harm.N &&
                    !HPEvoMask.TERMIN_AT.IsSet(harm.Mask) && !HPEvoMask.CHAIN_INVAO.IsSet(harm.Mask))
                {
                    return true;
                }
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
                    if (py.Team == player.Team && py.IsTared && py.Tux.Count < py.TuxLimit && py.HP > harm.N &&
                        !HPEvoMask.TERMIN_AT.IsSet(harm.Mask) && !HPEvoMask.CHAIN_INVAO.IsSet(harm.Mask))
                    {
                        invs.Add(harm.Who);
                    }
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
                string add = XI.AsyncInput(player.Uid, "#『弦歌问情』触发，是否令我方战力+1##是##否,Y2",
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
                    string input = XI.AsyncInput(uds[idxs], "Z1(p" +
                        string.Join("p", XI.Board.PZone) + ")", "JNT1901", "0");
                    if (!input.StartsWith("/"))
                    {
                        ushort cd = ushort.Parse(input);
                        if (XI.Board.PZone.Contains(cd))
                        {
                            XI.RaiseGMessage("G1OU," + cd);
                            XI.RaiseGMessage("G2QU,0,C,0" + cd);
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
        public bool JNT1902BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Team != XI.Board.Garden[owner].Team)
                return false;
            if (player.Tux.Count == 0)
                return false;
            if (player.IsSKOpt && !player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Code == "JPT2"))
                return false;
            string linkFuse = fuse;
            int lfidx = linkFuse.IndexOf(':');
            string pureFuse = linkFuse.Substring(lfidx + 1);
            foreach (string linkHead in linkFuse.Substring(0, lfidx).Split('&'))
            {
                if (linkHead.StartsWith("JNT1902"))
                    continue;
                string[] lh = linkHead.Split(',');
                string pureName = lh[0], pureTypeStr = lh[1], rawOc = lh[2];

                if (!pureTypeStr.Contains("!") && Artiad.ContentRule.IsFuseMatch(rawOc, pureFuse, XI.Board))
                {
                    ushort pureType = ushort.Parse(pureTypeStr);
                    Tux tux = XI.LibTuple.TL.EncodeTuxCode(pureName);
                    if (tux != null && XI.LibTuple.TL.IsTuxInGroup(tux, XI.PCS.Level))
                    {
                        if (tux.Bribe(player, pureType, pureFuse) && tux.Valid(player, pureType, pureFuse))
                            return true;
                    }
                }
            }
            return false;
        }
        public void JNT1902Action(Player player, int type, string fuse, string argst)
        {
            string[] args = argst.Split(',');
            ushort udb = ushort.Parse(args[1]);
            ushort ut = ushort.Parse(args[2]);
            Tux tux = XI.LibTuple.TL.EncodeTuxDbSerial(udb);

            string pureFuse;
            int pureType = Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                tux, player, XI.Board, out pureFuse);
            if (tux.Type == Tux.TuxType.ZP)
                XI.RaiseGMessage("G0CZ,0," + player.Uid);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," +
                tux.Code + "," + ut + ";" + pureType + "," + pureFuse);
        }
        public string JNT1902Input(Player player, int type, string fuse, string prev)
        {
            if (prev.IndexOf(',') < 0)
            {
                List<ushort> jpt2s = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Code == "JPT2").ToList();
                if (jpt2s.Count == 0)
                    return "/";
                ISet<ushort> dbs = new HashSet<ushort>();

                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                // linkHeads = { "TP02,0", "TP03,0" };
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                for (int i = 0; i < linkHeads.Length; ++i)
                {
                    if (linkHeads[i].StartsWith("JNT1902"))
                        continue;
                    string[] lh = linkHeads[i].Split(',');
                    string pureName = lh[0], pureTypeStr = lh[1], rawOc = lh[2];
                    if (!pureTypeStr.Contains("!") && Artiad.ContentRule.IsFuseMatch(rawOc, pureFuse, XI.Board))
                    {
                        ushort pureType = ushort.Parse(pureTypeStr);
                        Tux tux = XI.LibTuple.TL.EncodeTuxCode(pureName);
                        if (tux != null && XI.LibTuple.TL.IsTuxInGroup(tux, XI.PCS.Level))
                        {
                            if (tux.Bribe(player, pureType, pureFuse) && tux.Valid(player, pureType, pureFuse))
                                dbs.Add(tux.DBSerial);
                        }
                    }
                }
                return "#转化,/G1(p" + string.Join("p", dbs) + "),/Q1(p" + string.Join("p", jpt2s) + ")";
            }
            else return "";
        }
        public bool JNT1903Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNT1903", player, fuse);
            else if (type == 2)
            {
                Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
                return ab.Zone == Artiad.CustomsHelper.ZoneType.PLAYER && ab.Genre == Card.Genre.Tux &&
                    ab.List.Any(p => p.Source != player.Uid && p.Source != 0 &&
                    p.Cards.Any(q => XI.LibTuple.TL.DecodeTux(q).Code == "WQ04"));
            }
            else
                return false;
        }
        public void JNT1903Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                player.AddToPrice("WQ04", false, "JNT1903", '!', 0);
                player.AddToPrice("WQ04", true, "JNT1903", '!', 0);
            }
            else if (type == 1)
            {
                player.RemoveFromPrice("WQ04", false, "JNT1903");
                player.RemoveFromPrice("WQ04", true, "JNT1903");
            }
            else if (type == 2)
            {
                ushort ut = XI.LibTuple.TL.UniqueEquipSerial("WQ04");
                XI.RaiseGMessage("G2CN,0,1");
                XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
                XI.Board.TuxDises.Remove(ut);

                Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
                if (Artiad.CustomsHelper.RemoveCards(ab, ut))
                    XI.InnerGMessage(ab.ToMessage(), 141);
            }
        }
        public bool JNT1904Valid(Player player, int type, string fuse)
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
                return !player.ROM.GetBool("logomachy") && XI.Board.Garden.Values.Any(p =>
                    p.HP == 0 && p.Team == player.Team && p.IsAlive && !p.Loved && spouseContains(p));
            }
            else if (type == 1)
                return player.ROM.GetOrSetIntArray("ExSpFrom").Count > 0;
            else return false;
        }
        public void JNT1904Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort to = ushort.Parse(argst);
                if (player.Tux.Count > 0)
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", player.Tux));
                List<int> setInto = XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Uid != to).Select(p => p.SelectHero).ToList();
                player.ROM.Set("ExspTo", to);
                XI.Board.Garden[to].ExSpouses.AddRange(setInto.Select(p => p.ToString()));
                player.ROM.GetOrSetIntArray("ExSpFrom").AddRange(setInto);
                player.ROM.Set("logomachy", true);
            }
            else if (type == 1)
            {
                player.ROM.GetOrSetIntArray("ExSpFrom").ForEach(p => XI.Board.Garden[
                    player.ROM.GetUshort("ExspTo")].ExSpouses.Remove(p.ToString()));
                player.ROM.Set("ExSpFrom", null);
            }
        }
        public string JNT1904Input(Player player, int type, string fuse, string prev)
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
                return "/T1" + FormatPlayers(p => p.HP == 0 && p.Team == player.Team &&
                    p.IsAlive && !p.Loved && spouseContains(p));
            }
            else return "";
        }
        #endregion TR019 - JingTian
        #region TR020 - LeiYuan'ge
        public bool JNT2001Valid(Player player, int type, string fuse)
        {
            if (type == 0) // Selection
                return player.TokenExcl.Count > 0;
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
                    XI.RaiseGMessage("G0MA," + player.Uid + ",1");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNT2003");
                }
                else if (iNo == 7)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",2");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNT2004");
                }
                else if (iNo == 8)
                {
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
                return "I1(p" + string.Join("p", player.TokenExcl)　+ ")";
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
                // Consider Change it back into G0PB,0,who,...
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
            return player.Uid == XI.Board.Hinder.Uid && !player.RAM.GetBool("Hit") &&
                Artiad.PondRefresh.Parse(fuse).CheckHit;
        }
        public void JNT2102Action(Player player, int type, string fuse, string args)
        {
            player.RAM.Set("Hit", true);
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
                        var cards = Algo.TakeRange(blocks, idx + 2, idx + 2 + n)
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
                XI.RaiseGMessage(new Artiad.EquipExCards()
                {
                    Who = player.Uid, Source = player.Uid, SingleCard = ut
                }.ToMessage());
                if (XI.Board.PoolEnabled)
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
                    var cards = Algo.TakeRange(blocks, idx + 2, idx + 2 + n).Select(p => ushort.Parse(p));
                    count += cards.Intersect(player.ExCards).Count();
                    idx += (n + 2);
                }
                if (count > 0 && XI.Board.PoolEnabled)
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
            {
                XI.RaiseGMessage(new Artiad.EquipStandard()
                {
                    Who = to, Source = who, SingleCard = eq
                }.ToMessage());
            }

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
                return XI.Board.IsAttendWar(player) && player.TokenExcl.Count > 0;
            else if (type == 1)
                return player.RFM.GetBool("Resonance");
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
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, SingleCard = ut }
                }.ToMessage());
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",M" + ut);
                if (!sel.StartsWith("2"))
                    XI.RaiseGMessage("G0IP," + player.Team + ",3");
                else
                {
                    int jdx = sel.IndexOf(',');
                    ushort target = ushort.Parse(sel.Substring(jdx + 1));
                    XI.RaiseGMessage("G0IF," + target + ",2");
                }
                player.RFM.Set("Resonance", true);
            }
            else if (type == 1)
            {
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.HP,
                    Who = player.Uid,
                    NewValue = player.HPb - 2
                }.ToMessage());
                player.RFM.Set("Resonance", false);
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
            Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
            return XI.Board.Rounder.Team == player.Team && ab.Genre == Card.Genre.NMB &&
                ab.List.Any(p => p.Cards.Any(q => NMBLib.IsMonster(q)));
        }
        public void JNT2401Action(Player player, int type, string fuse, string argst)
        {
            Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
            int count = ab.List.Sum(p => p.Cards.Count(q => NMBLib.IsMonster(q)));
            while (count > 0)
            {
                string select = XI.AsyncInput(player.Uid, "#获得标记(剩余" + count + "枚),/T1" +
                    AAllTareds(player) + ",#获得标记,F1(p" + string.Join("p",
                    XI.LibTuple.RL.GetFullAppendableList()) + ")", "JNT2401", "0");
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
            List<ushort> picks = Artiad.Procedure.CardHunter(XI, Card.Genre.Tux,
                (p) => gayaTux.IsSameType(XI.LibTuple.TL.DecodeTux(p)), (a, r) => a.Count == 2, true);
            if (picks.Count > 0)
            { 
                XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + picks[0] + "," + picks[1]);
                while (picks.Count > 0)
                {
                    string select = XI.AsyncInput(player.Uid, "#分配的,/Q" + (picks.Count == 2 ? "1~2" : "1") +
                        "(p" + string.Join("p", picks) + "),/T1" + ATeammatesTared(player), "JNT2402", "0");
                    if ((select.StartsWith("/") && !select.Contains(",")) || select.StartsWith(VI.CinSentinel))
                    {
                        picks.Clear(); break;
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
                        picks.RemoveAll(p => delivers.Contains(p));
                    }
                }
            }
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
                return !player.RFM.GetBool("Watched") && fuse.Split(',')[2] == "2";
            else if (type == 1)
                return player.RFM.GetBool("Watched");
            else
                return false;
        }
        public void JNT2502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                player.RFM.Set("Watched", true);
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
                XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
            }
            else if (type == 1)
            {
                player.RFM.Set("Watched", false);
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                XI.RaiseGMessage("G0OX," + player.Uid + ",0,1");
            }
        }
        #endregion TR025 - Liaori
        #region TR026 - Huaying
        public bool JNT2601Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return Artiad.Harm.Parse(fuse).Any(p => XI.Board.Garden[p.Who].Team == player.Team &&
                    XI.Board.Garden[p.Who].IsTared && p.N > 0 && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) &&
                    player.TokenExcl.Contains("V" + p.Element.Elem2Int()));
            }
            else if (type == 1)
                return IsMathISOS("JNT2601", player, fuse);
            else return false;
        }
        public void JNT2601Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort tar = ushort.Parse(argst.Substring(0, idx));
                int elem = int.Parse(argst.Substring(idx + 1));
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,V" + elem);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",V" + elem);

                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.RemoveAll(p => p.Who == tar && p.Element.Elem2Int() == elem);
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -9);
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,5,V1,V2,V3,V4,V5");
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,V1,V2,V3,V4,V5");
            }
        }
        public string JNT2601Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                return "#免伤,/T1(p" + string.Join("p", Artiad.Harm.Parse(fuse).Where(p =>
                    XI.Board.Garden[p.Who].Team == player.Team && XI.Board.Garden[p.Who].IsTared &&
                    p.N > 0 && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) &&
                    player.TokenExcl.Contains("V" + p.Element.Elem2Int())).Select(p => p.Who).Distinct()) + ")";
            }
            else if (type == 0 && prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                return "#免疫属性,/V1(p" + string.Join("p", Artiad.Harm.Parse(fuse).Where(p => p.Who == who &&
                    p.N > 0 && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask)).Select(p => p.Element.Elem2Int()).ToList()) + ")";
            }
            else return "";
        }
        public bool JNT2602Valid(Player player, int type, string fuse)
        {
            return !player.ROM.GetBool("Po") && XI.Board.Garden.Values.Any(p => p.Team == player.Team && p.GetPetCount() > 0);
        }
        public void JNT2602Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort meFrom = ushort.Parse(argst.Substring(0, idx));
            ushort petUt = ushort.Parse(argst.Substring(idx + 1));
            TargetPlayer(player.Uid, meFrom);
            XI.RaiseGMessage(new Artiad.LosePet()
            {
                Owner = meFrom,
                SinglePet = petUt
            }.ToMessage());
            Monster pet = XI.LibTuple.ML.Decode(petUt);
            int fiveIdx = pet.Element.Elem2Index();
            if (XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && p.Pets[fiveIdx] != 0))
            {
                Player op = XI.Board.GetOpponenet(player);
                string monStr = XI.AsyncInput(op.Uid, "#弃置宠物,M1(p" + string.Join("p", XI.Board.Garden.Values
                    .Where(p => p.Team == player.OppTeam && p.Pets[fiveIdx] != 0)
                    .Select(p => p.Pets[fiveIdx])) + ")", "JNT2602", "0");
                if (monStr != VI.CinSentinel)
                {
                    ushort monUt = ushort.Parse(monStr);
                    ushort opFrom = XI.Board.Garden.Values.First(p => p.Pets[fiveIdx] == monUt).Uid;
                    XI.RaiseGMessage(new Artiad.LosePet()
                    {
                        Owner = opFrom,
                        SinglePet = monUt
                    }.ToMessage());

                    Monster mon = XI.LibTuple.ML.Decode(monUt);
                    if (mon.Level == Monster.ClLevel.WEAK)
                        Harm(player, player, 1, FiveElement.SOLARIS);
                    else if (mon.Level == Monster.ClLevel.STRONG)
                        Harm(player, player, 2, FiveElement.SOLARIS);
                    else if (mon.Level == Monster.ClLevel.BOSS)
                        Harm(player, player, 3, FiveElement.SOLARIS);
                }
            }
            player.ROM.Set("Po", true);
        }
        public string JNT2602Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#弃置宠物,/T1" + FormatPlayers(p => p.Team == player.Team && p.GetPetCount() > 0);
            else if (prev.IndexOf(',') < 0)
            {
                Player py = XI.Board.Garden[ushort.Parse(prev)];
                return "#弃置宠物,/M1(p" + string.Join("p", py.Pets.Where(p => p != 0)) + ")";
            }
            else
                return "";
        }
        #endregion TR026 - Huaying
        #region TR027 - Qianye
        public bool JNT2701Valid(Player player, int type, string fuse)
        {
            if ((type == 0) || type >= 2 && type <= 5 && XI.Board.PoolEnabled) // Z1 || I/OX, I/OW
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
                    return now != player.RAM.GetInt("Moqi");
                }
                return false;
            }
            else if (type == 1 && XI.Board.PoolEnabled && player.DEX > XI.Board.Battler.AGL) // FI
                return Artiad.CoachingChange.Parse(fuse).AttendOrLeave(player.Uid) != 0;
            else
                return false;
        }
        public void JNT2701Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type >= 2 && type <= 5)
            {
                int delta = player.DEX - XI.Board.Battler.AGL;
                if (delta < 0) { delta = 0; }
                int moqi = player.RAM.GetInt("Moqi");
                if (delta < moqi)
                    XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + "," + (moqi - delta));
                else if (delta > moqi)
                    XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + "," + (delta - moqi));
                player.RAM.Set("Moqi", delta);
            }
            else if (type == 1)
            {
                if (XI.Board.IsAttendWar(player))
                {
                    int delta = player.DEX - XI.Board.Battler.AGL;
                    if (delta <= 0)
                        player.RAM.Set("Moqi", 0);
                    else
                    {
                        player.RAM.Set("Moqi", delta);
                        XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + "," + delta);
                    }
                }
                else
                {
                    if (player.RAM.GetInt("Moqi") > 0)
                        XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + "," + player.RAM.GetInt("Moqi"));
                    player.RAM.Set("Moqi", 0);
                }
            }
        }
        public bool JNT2702Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1) // ZB/OT
                return player.GetBaseEquipCount() != player.ROM.GetInt("Cloth");
            else if (type == 2 || type == 3) // IS/OS
                return IsMathISOS("JNT2702", player, fuse) && player.GetBaseEquipCount() > 0;
            else if (type == 4) // Give up
            {
                return Artiad.CoachingChange.Parse(fuse).List.Any(
                    p => p.Role == Artiad.CoachingHelper.PType.GIVEUP);
            }
            else
                return false;
        }
        public void JNT2702Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                int now = player.GetBaseEquipCount();
                int delta = now - player.ROM.GetInt("Cloth");
                player.ROM.Set("Cloth", now);
                player.TuxLimit += delta;
            }
            else if (type == 2)
            {
                int now = player.GetBaseEquipCount();
                player.ROM.Set("Cloth", now);
                player.TuxLimit += now;
            }
            else if (type == 3)
            {
                player.TuxLimit -= player.GetBaseEquipCount();
                player.ROM.Set("Cloth", null);
            }
            else if (type == 4)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
        }
        #endregion TR027 - Qianye
        #region TR028 - Wuhou
        public bool JNT2801Valid(Player player, int type, string fuse)
        {
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            return opt.Farmland == 0 || XI.Board.Garden[opt.Farmer].Team ==
                XI.Board.Garden[opt.Farmland].OppTeam;
        }
        public void JNT2801Action(Player player, int type, string fuse, string argst)
        {
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            for (int i = 0; i < opt.Pets.Length; ++i)
            {
                string input = XI.AsyncInput(player.Uid, "#获得补牌,T1" +
                    ATeammatesTared(player), "JNT2801", "0");
                ushort who = ushort.Parse(input);
                if (who != 0)
                    XI.RaiseGMessage("G0DH," + who + ",0,1");
            }
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
                return player.SingleTokenTar != 0 && Artiad.Cure.Parse(fuse).Any(p => p.N > 0 &&
                    p.Who == player.SingleTokenTar && !HPEvoMask.TERMIN_AT.IsSet(p.Mask));
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
                    if (Artiad.ContentRule.FindCardExistance(cardId, Card.Genre.Tux, XI, true))
                        return true;
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
                    if (cure.N > 0 && cure.Who == player.SingleTokenTar && !HPEvoMask.TERMIN_AT.IsSet(cure.Mask))
                        ++cure.N;
                }
                if (cures.Count > 0)
                    XI.InnerGMessage(Artiad.Cure.ToMessage(cures), 16);
            }
            else if (type == 3)
            {
                string target = XI.AsyncInput(player.Uid, "#获得【天蛇杖】的,/T1" +
                    ATeammatesTared(player), "JNT2802", "1");
                if (target.StartsWith("/")) return;
                ushort to = ushort.Parse(target);
                TargetPlayer(player.Uid, to);
                ushort cardId = (XI.LibTuple.TL.EncodeTuxCode("WQ02") as TuxEqiup).SingleEntry;
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
                {
                    XI.RaiseGMessage(new Artiad.EquipStandard()
                    {
                        Who = to, Source = to, SingleCard = cardId
                    }.ToMessage());
                }
            }
        }
        #endregion TR028 - Wuhou
        #region TR029 - Xianqing
        public bool JNT2901Valid(Player player, int type, string fuse)
        {
            var g = XI.Board.Garden;
            List<Artiad.Harm> harm = Artiad.Harm.Parse(fuse);
            return player.Tux.Count > 0 && harm.Any(p => p.Who != player.Uid && g[p.Who].IsTared &&
                 g[p.Who].Team == player.Team && p.Element.IsPropedElement() && p.N > 0);
        }
        public void JNT2901Action(Player player, int type, string fuse, string argst)
        {
            var g = XI.Board.Garden;
            List<Artiad.Harm> harm = Artiad.Harm.Parse(fuse);
            List<ushort> invs = harm.Where(p => g[p.Who].IsTared && g[p.Who].Team == player.Team &&
                p.Who != player.Uid && p.Element.IsPropedElement() && p.N > 0).Select(p => p.Who).Distinct().ToList();
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
                XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                {
                    Role = Artiad.CoachingHelper.PType.SUPPORTER, Coach = player.Uid
                } }.ToMessage());
            else if (XI.Board.Rounder.Team == player.OppTeam)
                XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                {
                    Role = Artiad.CoachingHelper.PType.HINDER, Coach = player.Uid
                } }.ToMessage());
        }
        #endregion TR029 - Xianqing
        #region TR030 - LuoZhaoyan Male
        public bool JNT3001Valid(Player player, int type, string fuse)
        {
            var b = XI.Board;
            return b.IsAttendWar(player) && !b.IsAttendWarSucc(player) && b.Garden.Values.Any(
                p => p.IsAlive && p.Team == player.OppTeam && XI.Board.IsAttendWar(p));
        }
        public void JNT3001Action(Player player, int type, string fuse, string argst)
        {
            Harm(player, XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam && XI.Board.IsAttendWar(p)), 1);
        }
        public bool JNT3002Valid(Player player, int type, string fuse)
        {
            Func<Player, bool> cond = p => p.IsAlive && p.Uid != player.Uid && p.Tux.Count <= player.Tux.Count;
            return XI.Board.PoolEnabled && XI.Board.Garden.Values.Any(cond) == player.RAM.GetBool("Poorest");
        }
        public void JNT3002Action(Player player, int type, string fuse, string argst)
        {
            if (player.RAM.GetBool("Poorest"))
            {
                player.RAM.Set("Poorest", false);
                XI.RaiseGMessage("G0OA," + player.Uid + ",1,1");
                XI.RaiseGMessage("G0OX," + player.Uid + ",1,1");
            }
            else
            {
                player.RAM.Set("Poorest", true);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
                XI.RaiseGMessage("G0IX," + player.Uid + ",1,1");
            }
        }
        public bool JNT3003Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => p.N > 0 && XI.Board.Garden[p.Who].Team == player.Team &&
                p.Who != player.Uid && (HPEvoMask.FROM_JP.IsSet(p.Mask) || HPEvoMask.FROM_SK.IsSet(p.Mask)));
        }
        public void JNT3003Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0OS," + player.Uid + ",0,JNT3003");
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",17031");
            XI.RaiseGMessage("G0IS," + player.Uid + ",0,JNT3101");
        }
        #endregion TR030 - LuoZhaoyan Male
        #region TR031 - LuoZhaoyan
        public bool JNT3101Valid(Player player, int type, string fuse)
        {
            Tux jpt2 = XI.LibTuple.TL.EncodeTuxCode("JPT2");
            return player.Tux.Count >= 2 && jpt2 != null && jpt2.Bribe(player, type, fuse)
                && jpt2.Valid(player, type, fuse);
        }
        public void JNT3101Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JPT2," + argst + ";0," + fuse);
        }
        public string JNT3101Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q2(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        #endregion TR031 - LuoZhaoyan
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
                return harms.Any(p => p.Who == player.Uid && p.N > 0 && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask));
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
            return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N > 0) && player.Tux.Count > 0;
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
                return XI.Board.Garden.Values.Any(p => p.HP == 0 && p.Team == player.Team &&
                     p.IsAlive && !p.Loved && spouseContains(p));
            }
            else if (type == 1)
                return player.ROM.GetOrSetUshortArray("ExSpTo").Count > 0;
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
                player.ROM.GetOrSetUshortArray("ExSpTo").AddRange(tars);
            }
            else if (type == 1)
            {
                player.ROM.GetOrSetUshortArray("ExSpTo").ForEach(p =>
                    XI.Board.Garden[p].ExSpouses.Remove(player.SelectHero.ToString()));
                player.ROM.Set("ExSpTo", null);
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
                    p.IsAlive && !p.Loved && spouseContains(p)).Select(p => p.Uid).ToList();
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
            if (XI.Board.PoolEnabled)
            {
                int count = XI.Board.Garden.Values.Count(p => p.IsAlive && XI.Board.IsAttendWar(p) &&
                    XI.LibTuple.HL.InstanceHero(p.SelectHero).Bio.Contains("K"));
                if (type == 0)
                    return count > 0;
                else
                    return count != player.RAM.GetInt("Ghost");
            }
            else return false;
        }
        public void JNT3402Action(Player player, int type, string fuse, string args)
        {
            int count = XI.Board.Garden.Values.Count(p => p.IsAlive && XI.Board.IsAttendWar(p) &&
                XI.LibTuple.HL.InstanceHero(p.SelectHero).Bio.Contains("K") );
            if (type == 0)
            {
                player.RAM.Set("Ghost", count);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + count * 2);
            }
            else
            {
                int delta = count - player.RAM.GetInt("Ghost");
                player.RAM.Set("Ghost", count);
                if (delta > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + delta * 2);
                else if (delta < 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",1," + (-delta) * 2);
            }
        }
        public bool JNT3403Valid(Player player, int type, string fuse)
        {
            bool hasPet = XI.Board.Garden.Values.Any(p => p.IsTared &&
                p.Team == player.OppTeam && p.GetPetCount() > 0);
            bool lessPeople = XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.Team) <
                XI.Board.Garden.Values.Count(p => p.IsAlive && p.Team == player.OppTeam);
            return !player.ROM.GetBool("predict") && hasPet && lessPeople;
        }
        public void JNT3403Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort who = ushort.Parse(argst.Substring(0, idx));
            ushort pet = ushort.Parse(argst.Substring(idx + 1));
            TargetPlayer(player.Uid, who);
            XI.RaiseGMessage(new Artiad.LosePet()
            {
                Owner = who,
                SinglePet = pet
            }.ToMessage());
            player.ROM.Set("predict", true);
            XI.RaiseGMessage(new Artiad.InnateChange()
            {
                Item = Artiad.InnateChange.Prop.DEX,
                Who = player.Uid,
                NewValue = 0
            }.ToMessage());
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
        #region TR035 - GuHanjiang
        public bool JNT3501Valid(Player player, int type, string fuse)
        {
            if (type == 0 && !player.RAM.GetBool("InChangeFate")) // G1EV,130
            {
                bool b1 = player.DEX > 0 && XI.Board.EveDises.Count > 0 && XI.Board.Eve != 0;
                Player trigger = XI.Board.Garden[ushort.Parse(fuse.Split(',')[1])];
                bool b2 = trigger != null && trigger.Team == player.Team && trigger.IsAlive;
                return b1 && b2;
            }
            else if ((type == 1 || type == 2) && player.RAM.GetBool("InChangeFate")) // G1EV,220
                return XI.Board.Eve != 0;
            else if (type == 3) // G0I/OJ
            {
                string[] g0ij = fuse.Split(',');
                if (g0ij[1] == player.Uid.ToString() && g0ij[2] == "1")
                {
                    int n = int.Parse(g0ij[3]);
                    int delta = player.TokenExcl.Count / 2 - (player.TokenExcl.Count - n) / 2;
                    return delta > 0;
                }
                else return false;
            }
            else if (type == 4) // G0OJ
            {
                string[] g0oj = fuse.Split(',');
                if (g0oj[1] == player.Uid.ToString() && g0oj[2] == "1")
                {
                    int n = int.Parse(g0oj[3]);
                    int delta = (player.TokenExcl.Count + n) / 2 - player.TokenExcl.Count / 2;
                    return delta > 0;
                }
                else return false;
            }
            else return false;
        }
        public void JNT3501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                XI.Board.EveDises.Remove(ut);
                XI.RaiseGMessage("G2CN,2,1");

                ushort trigger = ushort.Parse(fuse.Split(',')[1]);
                string wouldChange = XI.AsyncInput(trigger, "#执行,E1(p" +
                    ut + "p" + XI.Board.Eve + ")", "JNT3501", "0");
                if (wouldChange == ut.ToString())
                {
                    XI.RaiseGMessage("G0YM,2,0,0");
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,E" + XI.Board.Eve);
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0,E" + XI.Board.Eve);

                    XI.Board.Eve = ut;
                    XI.RaiseGMessage("G0YM,2," + ut + ",0");
                }
                else
                {
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,E" + ut);
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0,E" + ut);
                }
                player.RAM.Set("InChangeFate", true);
            }
            else if (type == 1 || type == 2)
            {
                XI.AsyncInput(player.Uid, "#收入「改命」,E1(p" + XI.Board.Eve + ")", "JNT3501", "1");
                XI.RaiseGMessage("G0YM,2,0,0");
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,E" + XI.Board.Eve);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,E" + XI.Board.Eve);
                XI.Board.Eve = 0;
                player.RAM.Set("InChangeFate", null);
            }
            else if (type == 3) // G0I/OJ
            {
                string[] g0ij = fuse.Split(',');
                int n = int.Parse(g0ij[3]);
                int delta = player.TokenExcl.Count / 2 - (player.TokenExcl.Count - n) / 2;
                XI.RaiseGMessage("G0OX," + player.Uid + ",0," + delta);
            }
            else if (type == 4) // G0OJ
            {
                string[] g0oj = fuse.Split(',');
                int n = int.Parse(g0oj[3]);
                int delta = (player.TokenExcl.Count + n) / 2 - player.TokenExcl.Count / 2;
                XI.RaiseGMessage("G0IX," + player.Uid + ",0," + delta);
            }
        }
        public string JNT3501Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "#执行替换的,/E1(p" + string.Join("p", XI.Board.EveDises) + ")";
            else
                return "";
        }
        public bool JNT3502Valid(Player player, int type, string fuse)
        {
            return player.HP > 0 && XI.Board.Garden.Values.Any(
                p => p.IsTared && p.Team == player.Team && p.Uid != player.Uid);
        }
        public void JNT3502Action(Player player, int type, string fuse, string argst)
        {
            int n = int.Parse(argst);
            List<Player> pys = XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Team == player.Team && p.Uid != player.Uid).ToList();
            Artiad.Procedure.AssignCurePoint(XI, player, 2 * n, "JNT3502", pys,
                (d) => Cure(player, d.Keys.ToList(), d.Values.ToList(), FiveElement.SOLARIS));
            Harm(player, player, n, FiveElement.SOLARIS);
        }
        public string JNT3502Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/D1" + (player.HP > 1 ? ("~" + player.HP) : "");
            else
                return "";
        }
        #endregion TR035 - GuHanjiang
        #region TR036 - LuoMaiming
        public bool JNT3601Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNT3601", player, fuse);
            else if (type == 2)
            {
                Base.Card.Monster mon = XI.Board.Battler as Monster;
                return mon != null && (mon.Element == FiveElement.AQUA || mon.Element == FiveElement.AGNI ||
                    mon.Element == FiveElement.SOLARIS) && XI.Board.Garden.Values.Any(p =>
                    p.IsAlive && p.Team == player.Team && XI.Board.IsAttendWar(p));
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
                return "#请选择『嗜血法阵』执行项。##HP+2##补一张牌,Y2";
            else return "";
        }
        #endregion TR036 - LuoMaiming
        #region TR037 - Cangfeng
        public bool JNT3701Valid(Player player, int type, string fuse)
        {
            return XI.Board.Supporter.Uid == player.Uid && XI.Board.IsAttendWarSucc(XI.Board.Hinder);
        }
        public void JNT3701Action(Player player, int type, string fuse, string argst)
        {
            ushort rd = XI.Board.Rounder.Uid;
            TargetPlayer(player.Uid, rd);
            if (argst == "2")
                XI.RaiseGMessage("G0DH," + rd + ",0,1");
            else
                XI.RaiseGMessage("G0IA," + rd + ",1,2");
        }
        public string JNT3701Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "#请选择触发者执行项##战力+2##补1张牌,/Y2" : "";
        }
        public bool JNT3702BKValid(Player player, int type, string fuse, ushort owner)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return player.Tux.Count > 0 && harms.Any(p => XI.Board.Garden[owner].Team == player.Team &&
                p.Who != owner && p.Who == player.Uid && p.Element.IsPropedElement() && p.N > 0);
        }
        public void JNT3702Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort to = ushort.Parse(argst.Substring(0, idx)); // to = TR045
            ushort card = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + card);
            string insert = XI.AsyncInput(to, "#您获取,F1(p" + string.Join("p",
                XI.LibTuple.RL.GetFullAppendableList()) + ")", "JNT3702", "0");
            if (!string.IsNullOrEmpty(insert) && !insert.StartsWith(VI.CinSentinel))
            {
                ushort rune = ushort.Parse(insert);
                XI.RaiseGMessage("G0IF," + to + "," + rune);
            }
        }
        public string JNT3702Input(Player player, int type, string fuse, string prev)
        {
            if (prev.IndexOf(',') < 0)
                return "#交予的,/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNT3703Valid(Player player, int type, string fuse)
        {
            return player.Runes.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid);
        }
        public void JNT3703Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort rune = ushort.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0OF," + player.Uid + "," + rune);
            XI.RaiseGMessage("G0IF," + to + "," + rune);
        }
        public string JNT3703Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/F1(p" + string.Join("p", player.Runes) + "),/T1" + AOthersTared(player);
            else
                return "";
        }
        #endregion TR037 - Cangfeng
        #region TR038 - YingXuwei
        public bool JNT3801Valid(Player player, int type, string fuse)
        {
            Artiad.Abandon abandon = Artiad.Abandon.Parse(fuse);
            return abandon.Genre == Card.Genre.Tux && abandon.Zone == Artiad.CustomsHelper.ZoneType.PLAYER &&
                abandon.List.Any(p => XI.Board.Garden[p.Source].Team == player.OppTeam &&
                p.Cards.Any(q => XI.Board.TuxDises.Contains(q) && XI.LibTuple.TL.DecodeTux(q).Type == Tux.TuxType.TP));
        }
        public void JNT3801Action(Player player, int type, string fuse, string argst)
        {
            ushort which = ushort.Parse(argst);
            Harm(player, player, 1, FiveElement.YINN);

            if (player.ExCards.Count >= 8)
            {
                string input = XI.AsyncInput(player.Uid, "#「龙晶」中替换,C1(p" +
                    string.Join("p", player.ExCards) + ")", "JNT3801", "0");
                ushort sub = ushort.Parse(input);
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + sub);
            }
            XI.RaiseGMessage(new Artiad.EquipExCards()
            {
                Who = player.Uid,
                FromSky = true,
                SingleCard = which
            }.ToMessage());
            XI.RaiseGMessage("G2CN,0,1");
            XI.Board.TuxDises.Remove(which);
            
            Artiad.Abandon abandon = Artiad.Abandon.Parse(fuse);
            if (Artiad.CustomsHelper.RemoveCards(abandon, which))
                XI.InnerGMessage(abandon.ToMessage(), 145);
        }
        public string JNT3801Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                Artiad.Abandon abandon = Artiad.Abandon.Parse(fuse);
                ushort[] txs = abandon.List.Where(p =>
                    XI.Board.Garden[p.Source].Team == player.OppTeam).SelectMany(p => p.Cards).Where(p =>
                    XI.Board.TuxDises.Contains(p) && XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP).ToArray();
                return "#收为「龙晶」,/C1(p" + string.Join("p", txs) + ")";
            }
            else
                return "";
        }
        public void JNT3802Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 && Artiad.ClothingHelper.IsEx(fuse))
            {
                Artiad.EquipExCards eec = Artiad.EquipExCards.Parse(fuse);
                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + eec.Cards.Length);
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                int idx = 1; int count = 0;
                while (idx < blocks.Length)
                {
                    int n = int.Parse(blocks[idx + 1]);
                    var cards = Algo.TakeRange(blocks, idx + 2, idx + 2 + n).Select(p => ushort.Parse(p));
                    count += cards.Intersect(player.ExCards).Count();
                    idx += (n + 2);
                }
                if (count > 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + count);
            }
        }
        public bool JNT3802Valid(Player player, int type, string fuse)
        {
            if (type == 0 && Artiad.ClothingHelper.IsEx(fuse))
            {
                Artiad.EquipExCards eec = Artiad.EquipExCards.Parse(fuse);
                return eec.Who == player.Uid && eec.Cards.Length > 0;
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
                        var cards = Algo.TakeRange(blocks, idx + 2, idx + 2 + n)
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
        public void JNT3803Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
            {
                player.RFM.Set("UseSoul", true);
                ushort ut = ushort.Parse(argst);
                Tux tux = XI.LibTuple.TL.DecodeTux(ut);

                string pureFuse;
                int pureType = Artiad.ContentRule.GetTuxTypeFromLink(fuse, tux, player, XI.Board, out pureFuse);
                if (pureType >= 0) // argst contains ut and won't be null
                {
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," +
                        tux.Code + "," + argst + ";" + pureType + "," + pureFuse);
                }
            }
            else if (type == 2)
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", player.ExCards));
        }
        public bool JNT3803Valid(Player player, int type, string fuse)
        {
            if ((type == 0 || type == 1) && player.ExCards.Count > 0)
            {
                return player.ExCards.Any(p => Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                    XI.LibTuple.TL.DecodeTux(p), player, XI.Board) >= 0);
            }
            else if (type == 2 && player.ExCards.Count > 0)
                return player.RFM.GetBool("UseSoul");
            else return false;
        }
        public string JNT3803Input(Player player, int type, string fuse, string prev)
        {
            if ((type == 0 || type == 1) && prev == "")
            {
                List<ushort> candidates = player.ExCards.Where(p => Artiad.ContentRule.GetTuxTypeFromLink(
                    fuse, XI.LibTuple.TL.DecodeTux(p), player, XI.Board) >= 0).ToList();
                return "/Q1(p" + string.Join("p", candidates) + ")";
            }
            else return "";
        }
        #endregion TR038 - YingXuwei
        #region TR039 - GeQingfei
        public bool JNT3901Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
            {
                ushort code = (ushort)(25 + 3000);
                return XI.Board.Supporter.Uid == code || XI.Board.Hinder.Uid == code;
            }
            else if (type == 2 || type == 3)
                return IsMathISOS("JNT3901", player, fuse);
            else if (type == 4) // only notify the action, do nothing
            {
                ushort zs17 = (ushort)(3000 + 25);
                return XI.Board.Hinder.Uid == zs17 || XI.Board.Supporter.Uid == zs17;
            }
            else
                return false;
        }
        public void JNT3901Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                if (XI.Board.Rounder.Team == player.Team)
                    XI.Board.PosSupporters.Add("I25");
                else
                    XI.Board.PosHinders.Add("I25");
            }
            else if (type == 1)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            else if (type == 2)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,I25");
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,I25");
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I25");
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",I25");
                if (XI.Board.PoolEnabled)
                {
                    ushort zs17 = (ushort)(3000 + 25);
                    if (XI.Board.Supporter.Uid == zs17)
                        XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                        {
                            Role = Artiad.CoachingHelper.PType.SUPPORTER, Coach = 0
                        } }.ToMessage());
                    else if (XI.Board.Hinder.Uid == zs17)
                        XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                        {
                            Role = Artiad.CoachingHelper.PType.HINDER, Coach = 0
                        } }.ToMessage());
                }
            }
        }
        public bool JNT3902Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1)
                return IsMathISOS("JNT3902", player, fuse);
            else if (type == 2)
            {
                return Artiad.EqImport.Parse(fuse).Imports.Any(p => p.Who == player.Uid &&
                    (p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.FJ ||
                    p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.XB));
            }
            else if (type == 3)
            {
                return Artiad.EqExport.Parse(fuse).Exports.Any(p => p.Who == player.Uid &&
                    (p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.FJ ||
                    p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.XB));
            }
            else if (type == 4 && Artiad.ClothingHelper.IsStandard(fuse)) // G0ZB
            {
                Artiad.EquipStandard eis = Artiad.EquipStandard.Parse(fuse);
                return eis.Who == player.Uid && eis.Cards.Any(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.WQ);
            }
            else
                return false;
        }
        public void JNT3902Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0) // G0IS
            {
                List<Tux> wqs = XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.WQ).ToList();
                wqs.ForEach(p => player.AddToPrice(p.Code, false, "JNT3902", '=', 2));
                XI.RaiseGMessage(new Artiad.EqSlotVariation()
                {
                    Who = player.Uid, Slot = Artiad.ClothingHelper.SlotType.WQ, Increase = false
                }.ToMessage());
            }
            else if (type == 1) // G0OS
            {
                List<Tux> wqs = XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.WQ).ToList();
                wqs.ForEach(p => player.RemoveFromPrice(p.Code, false, "JNT3902"));
                XI.RaiseGMessage(new Artiad.EqSlotVariation()
                {
                    Who = player.Uid, Slot = Artiad.ClothingHelper.SlotType.WQ, Increase = true
                }.ToMessage());
            }
            else if (type == 2)
            {
                int nfj = Artiad.EqImport.Parse(fuse).Imports.Count(p => p.Who == player.Uid &&
                    p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.FJ);
                if (nfj > 0)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0," + nfj);
                int nxb = Artiad.EqImport.Parse(fuse).Imports.Count(p => p.Who == player.Uid &&
                    p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.XB);
                if (nxb > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0," + nxb);
            }
            else if (type == 3)
            {
                int nfj = Artiad.EqExport.Parse(fuse).Exports.Count(p => p.Who == player.Uid &&
                    p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.FJ);
                if (nfj > 0)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0," + nfj);
                int nxb = Artiad.EqExport.Parse(fuse).Exports.Count(p => p.Who == player.Uid &&
                    p.GetActualCardAs(XI).Type == Base.Card.Tux.TuxType.XB);
                if (nxb > 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + nxb);
            }
            // type == 4, only called.
        }
        #endregion TR039 - GeQingfei
        #region TR040 - BianLuohuan
        public bool JNT4001Valid(Player player, int type, string fuse)
        {
            Tux jpt6 = XI.LibTuple.TL.EncodeTuxCode("JPT6");
            return player.Tux.Count > 0 && jpt6.Valid(player, type, fuse) && jpt6.Bribe(player, type, fuse);
        }
        public void JNT4001Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JPT6," + card + ";0," + fuse);
        }
        public string JNT4001Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else return "";
        }
        public bool JNT4002Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid);
            else if (type == 1)
                return XI.Board.Garden.Values.Any(p => p.IsTared && XI.Board.IsAttendWar(p) && p.Runes.Contains(5));
            else
                return false;
        }
        public void JNT4002Action(Player player, int type, string fuse, string argst)
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
        public string JNT4002Input(Player player, int type, string fuse, string prev)
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
        public bool JNT4003Valid(Player player, int type, string fuse)
        {
            return XI.Board.RestNPCPiles.Count > 0 && player.HP == 0 && player.IsAlive;
        }
        public void JNT4003Action(Player player, int type, string fuse, string argst)
        {
            Artiad.Procedure.LoopOfNPCUntilJoinable(XI, player);
            if (XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                XI.InnerGMessage("G0ZH,0", -20);
        }
        #endregion TR039 - BianLuohuan
        #region TR041 - QiliXiaoyuan
        public bool JNT4101Valid(Player player, int type, string fuse)
        {
            Monster mon = XI.Board.Battler as Monster;
            return mon != null && mon.Level == Monster.ClLevel.STRONG || mon.Level == Monster.ClLevel.BOSS;
        }
        public void JNT4101Action(Player player, int type, string fuse, string argst)
        {
            Monster mon = XI.Board.Battler as Monster;
            if (mon.Level == Monster.ClLevel.STRONG)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
            else if (mon.Level == Monster.ClLevel.BOSS)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
        }
        public bool JNT4102Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.Supporter.Uid != 0 && XI.Board.Rounder.Team == player.Team && !XI.Board.SupportSucc;
            bool b2 = XI.Board.Hinder.Uid != 0 && XI.Board.Rounder.Team == player.OppTeam && !XI.Board.HinderSucc;
            bool b3 = XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.Team && !XI.Board.IsAttendWar(p));
            return (b1 || b2) && b3;
        }
        public void JNT4102Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + string.Join(",", XI.Board.Garden.Values.Where(p => p.IsTared &&
                p.Team == player.Team && !XI.Board.IsAttendWar(p)).Select(p => p.Uid + ",0,1")));
        }
        #endregion TR040 - QiliXiaoyuan
        #region TR042 - Shuoxuan
        public bool JNT4201Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid && p.Tux.Count > 0);
            else if (type == 1)
                return player.TokenExcl.Count > 0;
            else if ((type == 2 || type == 3) && player.TokenExcl.Count > 0)
                return player.TokenExcl.Select(p => ushort.Parse(p.Substring("C".Length))).Any(
                        p => Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                        XI.LibTuple.TL.DecodeTux(p), player, XI.Board) >= 0);
            else
                return false;
        }
        public void JNT4201Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                TargetPlayer(player.Uid, ut);
                Player who = XI.Board.Garden[ut];
                XI.AsyncInput(player.Uid, "C1(" + Algo.RepeatString("p0", who.Tux.Count) + ")", "JNT4201", "0");
                List<ushort> vals = who.Tux.Except(XI.Board.ProtectedTux).ToList();
                vals.Shuffle();
                XI.RaiseGMessage("G0OT," + ut + ",1," + vals[0]);
                XI.RaiseGMessage("G2TZ," + player.Uid + "," + ut + ",C" + vals[0]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,C" + vals[0]);
                player.RFM.Set("TuxFrom", ut);
            }
            else if (type == 1)
            {
                ushort who = player.RFM.GetUshort("TuxFrom");
                List<string> tokens = player.TokenExcl.ToList();
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + player.TokenExcl.Count + "," + string.Join(",", player.TokenExcl));
                if (!XI.Board.Garden[who].IsAlive)
                {
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", tokens));
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit()
                        {
                            Source = player.Uid,
                            Cards = tokens.Select(p => ushort.Parse(p.Substring("C".Length))).ToArray()
                        }
                    }.ToMessage());
                }
                else
                {
                    XI.RaiseGMessage("G0HQ,3," + who + "," + player.Uid + "," + tokens.Count +
                        "," + string.Join(",", tokens.Select(p => p.Substring("C".Length))));
                    if (player.Tux.Count > 0)
                    {
                        string select = XI.AsyncInput(player.Uid, "#交还,Q1(p" + 
                            string.Join("p", player.Tux) + ")","JNT4201", "1");
                        if (select != VI.CinSentinel)
                        {
                            ushort back = ushort.Parse(select);
                            XI.RaiseGMessage("G0HQ,0," + who + "," + player.Uid + ",1,1," + back);
                        }
                    }
                }
            }
            else if (type == 2 || type == 3)
            {
                ushort ut = ushort.Parse(argst);
                Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                string pureFuse;
                int pureType = Artiad.ContentRule.GetTuxTypeFromLink(
                    fuse, tux, player, XI.Board, out pureFuse);
                if (!tux.IsTuxEqiup())
                {
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,C" + ut);
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + ",C" + ut);
                    if (!Artiad.ContentRule.IsTuxVestige(tux.Code, pureType))
                        XI.RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                            Genre = Card.Genre.Tux,
                            SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, SingleCard = ut }
                        }.ToMessage());
                    else
                        XI.Board.PendingTux.Enqueue(player.Uid + ",G0CC," + ut);
                    if (tux.Type == Tux.TuxType.ZP)
                        XI.RaiseGMessage("G0CZ,0," + player.Uid);
                    XI.InnerGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," +
                        tux.Code + "," + ut + ";" + pureType + "," + pureFuse, 101);
                }
                else if (tux != null)
                {
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,C" + ut);
                    XI.RaiseGMessage("G1UE," + player.Uid + ",0," + ut);
                }
            }
        }
        public string JNT4201Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                        p.Uid != player.Uid && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
                else
                    return "";
            }
            else if (type == 2 || type == 3)
            {
                if (prev == "")
                {
                    List<ushort> candidates = player.TokenExcl.Select(p => ushort.Parse(
                        p.Substring("C".Length))).Where(p => Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                        XI.LibTuple.TL.DecodeTux(p), player, XI.Board) >= 0).ToList();
                    return "/C1(p" + string.Join("p", candidates) + ")";
                }
                else return "";
            }
            return "";
        }
        public bool JNT4202Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Team == player.OppTeam && p.Tux.Count > 1);
        }
        public void JNT4202Action(Player player, int type, string fuse, string argst)
        {
            int incr = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam).Max(p => p.Tux.Count) - 1;
            if (incr > 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + incr);
        }
        #endregion TR041 - Shuoxuan
        #region TR043 - Linyuan
        public bool JNT4301Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => XI.Board.Garden[p.Who].IsAlive && XI.Board.Garden[p.Who].Tux.Count <= p.N);
        }
        public void JNT4301Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            Cure(player, harms.Where(p => XI.Board.Garden[p.Who].IsAlive &&
                XI.Board.Garden[p.Who].Tux.Count <= p.N).Select(p => XI.Board.Garden[p.Who]).Distinct(), 1);
        }
        public bool JNT4302Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0) && !player.ROM.GetBool("Helped");
        }
        public void JNT4302Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            player.ROM.Set("Helped", true);
            XI.RaiseGMessage("G0TT," + tar);
            int result = XI.Board.DiceValue;
            Cure(player, XI.Board.Garden[tar], result);
            XI.RaiseGMessage("G0IF," + tar + ",3,4");
            Artiad.Procedure.ArticuloMortis(XI, XI.WI, false);
        }
        public string JNT4302Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1" + FormatPlayers(p => p.IsTared && p.HP == 0);
            else
                return "";
        }
        #endregion TR042 - Linyuan
        #region TR044 - Zhuyu
        public bool JNT4401Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public void JNT4401Action(Player player, int type, string fuse, string argst)
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
        public string JNT4401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#获得「坚盾」或「神行」,/T1" + ATeammatesTared(player);
            else return "";
        }
        public bool JNT4402Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsAlive && p.Runes.Count > 0);
        }
        public void JNT4402Action(Player player, int type, string fuse, string argst)
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
        public string JNT4402Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "S" : "";
        }
        #endregion TR043 - Zhuyu
        #region TR045 - Suhe
        public bool JNT4501Valid(Player player, int type, string fuse)
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
                !player.ROM.GetOrSetUshortArray("Changed").Contains(p.Uid)) &&
                XI.Board.Garden.Values.Any(p => p.IsTared && XI.LibTuple.HL.InstanceHero(p.SelectHero).Spouses.Any(q => joinable(q)));
        }
        public void JNT4501Action(Player player, int type, string fuse, string argst)
        {
            ushort[] args = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            ushort who = args[0];
            int avatar = args[2];

            player.ROM.GetOrSetUshortArray("Changed").Add(who);
            int hp = XI.Board.Garden[who].HP;
            XI.RaiseGMessage("G0OY,0," + who);
            XI.RaiseGMessage("G0IY,2," + who + "," + avatar + "," + hp);

            List<ushort> hasCard = XI.Board.Garden.Values.Where(p => p.Team == player.Team &&
                 p.IsTared && p.Tux.Count > 0).Select(p => p.Uid).ToList();
            if (hasCard.Count > 0)
            {
                string discardAll = XI.AsyncInput(player.Uid, "#弃置手牌以回复,/T1(p" +
                    string.Join("p", hasCard) + ")", "JNT4501", "0");
                if (!discardAll.StartsWith("/") && !discardAll.Contains(VI.CinSentinel))
                {
                    ushort dwho = ushort.Parse(discardAll);
                    int dhp = XI.Board.Garden[dwho].Tux.Count;
                    XI.RaiseGMessage("G0QZ," + dwho + "," + string.Join(",", XI.Board.Garden[dwho].Tux));
                    Cure(player, XI.Board.Garden[who], dhp);
                }
            }
        }
        public string JNT4501Input(Player player, int type, string fuse, string prev)
        {
            Func<string, List<Hero>> chain = (p) =>
            {
                if (p.StartsWith("!"))
                    return new List<Hero>();
                int avatar = int.Parse(p);
                Hero hero = XI.LibTuple.HL.InstanceHero(avatar);
                return Artiad.ContentRule.GetJoinableHeroChain(hero, XI);
            };
            Func<string, bool> joinable = (p) => chain(p).Count > 0;
            if (prev == "")
            {
                return "#进行变身,/T1" + FormatPlayers(p => p.Team == player.Team && p.IsTared &&
                    !player.ROM.GetOrSetUshortArray("Changed").Contains(p.Uid)) + ",#倾慕者,/T1"
                    + FormatPlayers(p => p.IsTared && XI.LibTuple.HL.InstanceHero(p.SelectHero).Spouses.Any(q => joinable(q)));
            }
            else if (prev.IndexOf(',', (prev.IndexOf(',') + 1)) < 0)
            {
                ushort target = ushort.Parse(prev.Substring(prev.IndexOf(',') + 1));
                Hero hero = XI.LibTuple.HL.InstanceHero(XI.Board.Garden[target].SelectHero);
                List<Hero> heros = new List<Hero>();
                foreach (string spo in hero.Spouses)
                    heros.AddRange(chain(spo));
                return "/H1(p" + string.Join("p", heros.Select(p => p.Avatar)) + ")";
            }
            else return "";
        }
        public bool JNT4502Valid(Player player, int type, string fuse)
        {
            string[] g0iy = fuse.Split(',');
            int changeType = int.Parse(g0iy[1]);
            ushort who = ushort.Parse(g0iy[2]);
            //int heroNum = int.Parse(g0iy[3]);
            return changeType != 1 && who != player.Uid;
        }
        public void JNT4502Action(Player player, int type, string fuse, string argst)
        {
            string select = XI.AsyncInput(player.Uid,
                "#请选择『归墟』执行项##战力+1##命中+1,Y2", "JNT4502", "0");
            if (select == "2")
                XI.RaiseGMessage("G0IX," + player.Uid + ",0,1");
            else
                XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        }
        #endregion TR044 - Suhe

        #region RE001 - HuangfuZhuo (R.TR012)
        public bool JNR0101Valid(Player player, int type, string fuse)
        {
            Tux tp04 = XI.LibTuple.TL.EncodeTuxCode("TP04");
            return player.Tux.Count > 0 && tp04 != null &&
                tp04.Bribe(player, type, fuse) && tp04.Valid(player, type, fuse);
        }
        public void JNR0101Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",TP04," + argst + ";0," + fuse);
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNR0101,0");
        }
        public string JNR0101Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JNR0102Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && 
                p.Runes.Intersect(XI.LibTuple.RL.GetFullNegative()).Any());
        }
        public void JNR0102Action(Player player, int type, string fuse, string argst)
        {
            ushort[] parts = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0OF," + parts[0] + "," + parts[1]);
            XI.RaiseGMessage("G0IF," + parts[0] + "," + parts[2]);
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNR0102,0");
        }
        public string JNR0102Input(Player player, int type, string fise, string prev)
        {
            if (prev == "")
            {
                return "/T1" + FormatPlayers(p => p.IsTared && p.Runes.Intersect(
                    XI.LibTuple.RL.GetFullNegative()).Any());
            }
            else if (prev.IndexOf(',') < 0)
            {
                Player py = XI.Board.Garden[ushort.Parse(prev)];
                return "#消除,/F1(p" + string.Join("p", XI.LibTuple.RL.GetFullNegative().Intersect(
                    py.Runes)) + "),#更改,/F1(p" + string.Join("p", XI.LibTuple.RL.GetFullPositive()) + ")";
            }
            else return "";
        }
        public bool JNR0103Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return JNT1202Valid(player, 0, fuse);
            else if (type == 1)
            {
                string[] g1ck = fuse.Split(',');
                ushort who = ushort.Parse(g1ck[1]);
                string skill = g1ck[2];
                if (who == player.Uid)
                {
                    if (skill == "JNR0101")
                        return !player.RFM.GetBool("Action01");
                    else if (skill == "JNR0102")
                        return !player.RFM.GetBool("Action02");
                }
                return false;
            }
            else if (type == 2)
                return player.RFM.GetBool("Action01") || player.RFM.GetBool("Action02");
            else return false;
        }
        public void JNR0103Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                JNT1202Action(player, 0, fuse, argst);
            else if (type == 1)
            {
                if (XI.Board.PoolEnabled)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
                string[] g1ck = fuse.Split(',');
                string skill = g1ck[2];
                if (skill == "JNR0101")
                    player.RFM.Set("Action01", true);
                else if (skill == "JNR0102")
                    player.RFM.Set("Action02", true);
            }
            else if (type == 2)
            {
                int value = 0;
                if (player.RFM.GetBool("Action01"))
                    value += 2;
                if (player.RFM.GetBool("Action02"))
                    value += 2;
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + value);
            }
        }
        #endregion RE001 - HuangfuZhuo (R.TR012)
    }
}