﻿using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg.JNS
{
    public partial class SkillCottage
    {
        private Base.VW.IVI VI { set; get; }
        //public VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public SkillCottage(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }
        // Action class, 0 thread receives and maintain data base.
        // Input class, player.Uid thread receives and maintain data base.
        public IDictionary<string, Skill> RegisterDelegates(SkillLib lib)
        {
            SkillCottage sc = this;
            IDictionary<string, Skill> sk01 = new Dictionary<string, Skill>();
            foreach (Skill sk in lib.Firsts)
            {
                sk01.Add(sk.Code, sk);
                string skCode = sk.Code;
                var methodAction = sc.GetType().GetMethod(skCode + "Action");
                if (methodAction != null)
                    sk.Action += new Skill.ActionDelegate(delegate(Player player, int type, string fuse, string argst)
                    {
                        methodAction.Invoke(sc, new object[] { player, type, fuse, argst });
                    });
                var methodValid = sc.GetType().GetMethod(skCode + "Valid");
                if (methodValid != null)
                    sk.Valid += new Skill.ValidDelegate(delegate(Player player, int type, string fuse)
                    {
                        return (bool)methodValid.Invoke(sc, new object[] { player, type, fuse });
                    });
                var methodInput = sc.GetType().GetMethod(skCode + "Input");
                if (methodInput != null)
                    sk.Input += new Skill.InputDelegate(delegate(Player player, int type, string fuse, string prev)
                    {
                        return (string)methodInput.Invoke(sc, new object[] { player, type, fuse, prev });
                    });
                var methodEncrypt = sc.GetType().GetMethod(skCode + "Encrypt");
                if (methodEncrypt != null)
                    sk.Encrypt += new Skill.EncryptDelegate(delegate(string argst)
                    {
                        return (string)methodEncrypt.Invoke(sc, new object[] { argst });
                    });
                if (sk.IsBK)
                {
                    Bless bs = (Bless)sk;
                    var methodBKValid = sc.GetType().GetMethod(skCode + "BKValid");
                    if (methodBKValid != null)
                        bs.BKValid += new Bless.BKValidDelegate(delegate(Player player, int type, string fuse, ushort owner)
                        {
                            return (bool)methodBKValid.Invoke(sc, new object[] { player, type, fuse, owner });
                        });
                }
            }
            return sk01;
        }

        #region XJ101 - LiXiaoyao
        public bool JN10101Valid(Player player, int type, string fuse)
        {
            return XI.Board.Rounder.Gender == 'F' && XI.Board.Rounder.Team == player.Team;
        }
        public void JN10101Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "李逍遥触发「侠骨柔肠」，对{0}的命中+1.", XI.DisplayPlayer(XI.Board.Rounder.Uid));
            XI.RaiseGMessage("G0IX," + player.Uid + ",1,1");
        }
        public bool JN10102Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && XI.Board.Rounder.Team == player.Team
                && XI.Board.Battler != null && XI.Board.Battler.AGL <= 2
                && XI.Board.Hinder.IsTared && XI.Board.Hinder.Tux.Count > 0;
        }
        public void JN10102Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(XI.AsyncInput(player.Uid, "#获得的,T1(p" +
                XI.Board.Hinder.Uid + ")", "JN10102", "0"));
            VI.Cout(0, "李逍遥触发「飞龙探云手」，获得妨碍者{0}一张牌.", XI.DisplayPlayer(tar));
            string c0 = Util.RepeatString("p0", XI.Board.Garden[XI.Board.Hinder.Uid].Tux.Count);
            XI.AsyncInput(player.Uid, "#获得的,C1(" + c0 + ")", "JN10102" , "0");
            XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + XI.Board.Hinder.Uid + ",2,1");
        }
        #endregion XJ101 - LiXiaoyao
        #region XJ102 - ZhaoLing'er
        public bool JN10201Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                int zbtype = int.Parse(blocks[2]);
                if (zbtype == 0)
                {
                    // G0ZB,A,0,x
                    ushort me = ushort.Parse(blocks[1]);
                    ushort card = ushort.Parse(blocks[3]);
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(card);
                    return (me == player.Uid && player.Tux.Contains(card) && (player.Weapon != 0 ||
                        player.ExEquip != 0) && tux.Type == Base.Card.Tux.TuxType.WQ);
                }
                else if (zbtype == 1)
                {
                    // G0ZB,A,1,B,x
                    ushort to = ushort.Parse(blocks[1]);
                    ushort card = ushort.Parse(blocks[4]);
                    Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(card);
                    return (to == player.Uid && (player.Weapon != 0 ||
                        player.ExEquip != 0) && tux.Type == Base.Card.Tux.TuxType.WQ);
                }
            }
            else if (type == 1 && (player.Weapon != 0 || player.ExEquip != 0))
            {
                string[] blocks = fuse.Split(',');
                int idx = 1;
                while (idx < blocks.Length)
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    int n = int.Parse(blocks[idx + 1]);
                    if (who == player.Uid)
                    {
                        List<ushort> lu = Util.TakeRange(blocks, idx + 2, idx + 2 + n)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (lu.Contains(player.ExEquip) && player.ExEquip != 0)
                            return true;
                        //bool w1 = player.Weapon != 0 && lu.Contains(player.Weapon);
                        //bool w2 = player.ExCards.Count > 0 && lu.Contains(player.ExCards.First());
                        ////if (w1 || w2)
                        //if (w2)
                            //return true;
                    }
                    idx += (n + 2);
                }
            }
            //else if (type == 2)
            //{
            //    string linkFuse = fuse;
            //    int lfidx = linkFuse.IndexOf(':');
            //    // linkHeads = { "TP02,0", "TP03,0" };
            //    string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            //    fuse = linkFuse.Substring(lfidx + 1);

            //    foreach (string linkHead in linkHeads)
            //    {
            //        List<string> relateds = XI.Sk03[linkHead].ToList();
            //        relateds.Add(linkHead);
            //        // relateds = { "TP03,0", "FJ02,0!0" };
            //        foreach (string rlink in relateds)
            //        {
            //            if (rlink.StartsWith("JN10201") || rlink.StartsWith("JN10301"))
            //                continue;
            //            int rcmIdx = rlink.IndexOf(',');
            //            string rName = Util.Substring(rlink, 0, rcmIdx);
            //            int pdIdx = rlink.IndexOf('!', rcmIdx + 1);

            //            int tConsType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
            //            int tType = int.Parse(rlink.Substring(pdIdx + 1));
            //            foreach (ushort ut in player.ExCards)
            //            {
            //                Base.Card.TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
            //                if (tue != null && tue.Code == rName &&
            //                        tue.ConsumeValid(player, tConsType, tType, fuse))
            //                    return true;
            //            }
            //        }
            //    }
            //    return false;
            //}
            return false;
        }
        public void JN10201Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                int zbtype = int.Parse(blocks[2]);
                // G0ZB,A,0,x/G0ZB,A,1,B,x
                ushort card = (zbtype == 0) ? ushort.Parse(blocks[3]) : ushort.Parse(blocks[4]);
                ushort inv = (zbtype == 0) ? player.Uid : ushort.Parse(blocks[3]);
                var tux = XI.LibTuple.TL.DecodeTux(card) as TuxEqiup;
                XI.RaiseGMessage("G0OT," + inv + ",1," + card);
                string pos = "";
                int count = 0;
                if (player.Weapon != 0) { pos += "p" + player.Weapon; ++count; }
                if (player.ExEquip > 0) { pos += "p" + player.ExEquip; ++count; }
                string hole = count < 2 ? "#要替换的(取消不替换),/" : "#要替换的,";
                string input = XI.AsyncInput(inv, hole + "C1(" + pos + ")", "JN10201", "0");
                if (input == "/0") input = "0";
                ushort sel = ushort.Parse(input);
                if (sel != 0)
                {
                    if (sel == player.Weapon || sel == player.ExEquip)
                        XI.RaiseGMessage("G0QZ," + player.Uid + "," + sel);
                }
                if (player.Weapon == 0)
                {
                    player.Weapon = card;
                    XI.RaiseGMessage("G1IZ," + player.Uid + "," + card);
                    XI.WI.BCast("E0ZB," + player.Uid + ",0,1," + card);
                    if (!player.WeaponDisabled)
                        tux.InsAction(player);
                }
                else
                {
                    player.ExEquip = card;
                    XI.RaiseGMessage("G1IZ," + player.Uid + "," + card);
                    XI.WI.BCast("E0ZB," + player.Uid + ",0,5," + card);
                    if (!player.WeaponDisabled)
                        tux.InsAction(player);
                }
                XI.InnerGMessage(fuse, 101);
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
                        List<ushort> lu = Util.TakeRange(blocks, idx + 2, idx + 2 + n)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (lu.Contains(player.ExEquip) && player.ExEquip != 0)
                            XI.RaiseGMessage("G1OZ," + who + "," + player.ExEquip);
                    }
                    idx += (n + 2);
                }
                //XI.InnerGMessage(fuse, 81);
            }
            //else if (type == 2)
            //{
            //    int idxc = argst.IndexOf(',');
            //    ushort utc;
            //    if (idxc >= 0)
            //        utc = ushort.Parse(argst.Substring(0, idxc));

            //    string linkFuse = fuse;
            //    int lfidx = linkFuse.IndexOf(':');
            //    // linkHeads = { "TP02,0", "TP03,0" };
            //    string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            //    fuse = linkFuse.Substring(lfidx + 1);

            //    foreach (string linkHead in linkHeads)
            //    {
            //        List<string> relateds = XI.Sk03[linkHead].ToList();
            //        relateds.Add(linkHead);
            //        // relateds = { "TP03,0", "FJ02,0!0" };
            //        foreach (string rlink in relateds)
            //        {
            //            if (rlink.StartsWith("JN10201") || rlink.StartsWith("JN10301"))
            //                continue;
            //            int rcmIdx = rlink.IndexOf(',');
            //            string rName = Util.Substring(rlink, 0, rcmIdx);
            //            int pdIdx = rlink.IndexOf('!', rcmIdx + 1);

            //            int tType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
            //            int tConsType = int.Parse(rlink.Substring(pdIdx + 1));
            //            foreach (ushort ut in player.ExCards)
            //            {
            //                Base.Card.TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
            //                if (tue != null && tue.Code == rName &&
            //                        tue.ConsumeValid(player, tConsType, tType, fuse))
            //                    tue.ConsumeAction(player, tConsType, tType, fuse, argst);
            //            }
            //        }
            //    }
            //}
        }
        //public string JN10201Input(Player player, int type, string fuse, string prev)
        //{
        //    if (type == 2)
        //    {
        //        if (prev == "")
        //        {
        //            List<ushort> posWeapons = new List<ushort>();

        //            string linkFuse = fuse;
        //            int lfidx = linkFuse.IndexOf(':');
        //            // linkHeads = { "TP02,0", "TP03,0" };
        //            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
        //            fuse = linkFuse.Substring(lfidx + 1);

        //            foreach (string linkHead in linkHeads)
        //            {
        //                List<string> relateds = XI.Sk03[linkHead].ToList();
        //                relateds.Add(linkHead);
        //                // relateds = { "TP03,0", "FJ02,0!0" };
        //                foreach (string rlink in relateds)
        //                {
        //                    if (rlink.StartsWith("JN10201") || rlink.StartsWith("JN10301"))
        //                        continue;
        //                    int rcmIdx = rlink.IndexOf(',');
        //                    string rName = Util.Substring(rlink, 0, rcmIdx);
        //                    int pdIdx = rlink.IndexOf('!', rcmIdx + 1);

        //                    int tType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
        //                    int tConsType = int.Parse(rlink.Substring(pdIdx + 1));
        //                    foreach (ushort ut in player.ExCards)
        //                    {
        //                        Base.Card.TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
        //                        if (tue != null && tue.Code == rName &&
        //                                tue.ConsumeValid(player, tConsType, tType, fuse))
        //                            posWeapons.Add(ut);
        //                    }
        //                }
        //            }

        //            if (posWeapons.Count > 0)
        //                return "/Q1(p" + string.Join("p", posWeapons) + ")";
        //            else
        //                return "/";
        //        }
        //        else
        //        {
        //            int idxc = prev.IndexOf(',');
        //            ushort utc = ushort.Parse(Util.Substring(prev, 0, idxc));
        //            string uprev = Util.Substring(prev, idxc + 1, -1);

        //            string linkFuse = fuse;
        //            int lfidx = linkFuse.IndexOf(':');
        //            // linkHeads = { "TP02,0", "TP03,0" };
        //            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
        //            fuse = linkFuse.Substring(lfidx + 1);

        //            foreach (string linkHead in linkHeads)
        //            {
        //                List<string> relateds = XI.Sk03[linkHead].ToList();
        //                relateds.Add(linkHead);
        //                // relateds = { "TP03,0", "FJ02,0!0" };
        //                foreach (string rlink in relateds)
        //                {
        //                    if (rlink.StartsWith("JN10201") || rlink.StartsWith("JN10301"))
        //                        continue;
        //                    int rcmIdx = rlink.IndexOf(',');
        //                    string rName = Util.Substring(rlink, 0, rcmIdx);
        //                    int pdIdx = rlink.IndexOf('!', rcmIdx + 1);

        //                    int tType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
        //                    int tConsType = int.Parse(rlink.Substring(pdIdx + 1));
        //                    foreach (ushort ut in player.ExCards)
        //                    {
        //                        Base.Card.TuxEqiup tue = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
        //                        if (utc == ut && tue != null && tue.Code == rName &&
        //                                tue.ConsumeValid(player, tConsType, tType, fuse))
        //                            return tue.ConsumeInput(player, tConsType, tType, fuse, uprev);
        //                    }
        //                }
        //            }
        //            return "";
        //        }
        //    }
        //    else return "";
        //}
        public bool JN10202Valid(Player player, int type, string fuse)
        {
            return player.IsAlive && XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Select(p => p.GetPetCount()).Sum() >= 3;
        }
        public void JN10202Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10103");
            //XI.InnerGMessage(fuse, 121);
        }
        #endregion XJ102 - ZhaoLing'er
        #region XJ103 - Mengshe
        //public bool JN10301Valid(Player player, int type, string fuse)
        //{
        //    return JN10201Valid(player, type, fuse);
        //}
        //public void JN10301Action(Player player, int type, string fuse, string argst)
        //{
        //    JN10201Action(player, type, fuse, argst);
        //}
        public bool JN10302Valid(Player player, int type, string fuse)
        {
            return player.IsAlive && XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Select(p => p.GetPetCount()).Sum() < 3;
        }
        public void JN10302Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10102");
            //XI.InnerGMessage(fuse, 121);
        }
        public bool JN10303Valid(Player player, int type, string fuse)
        {
            if (type == 0) return true;
            else if ((type == 1 || type == 2) && XI.Board.InFight)
                return IsMathISOS("JN10303", player, fuse);
            else
                return false;
        }
        public void JN10303Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IP," + player.Team + ",2");
            else if (type == 1)
            {
                XI.RaiseGMessage("G0IP," + player.Team + ",2");
                //XI.InnerGMessage(fuse, 121);
            }
            else if (type == 2)
            {
                XI.RaiseGMessage("G0OP," + player.Team + ",2");
                //XI.InnerGMessage(fuse, 81);
            }
        }
        #endregion XJ103 - Mengshe
        #region XJ104 - LinYueru
        public bool JN10401Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0 || type == 1)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void JN10401Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        XI.RaiseGMessage("G0IA," + who + ",0,1");
                }
                //XI.InnerGMessage(fuse, 121);
            }
            else if (type == 1)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        XI.RaiseGMessage("G0OA," + who + ",0,1");
                }
                //XI.InnerGMessage(fuse, 71);
            }
        }
        public bool JN10402Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            return XI.Board.IsAttendWar(player) && meLose && XI.Board.Garden.Values.Where(
                p => p.IsAlive && XI.Board.IsAttendWar(p) && p.Team == player.OppTeam).Any();
        }
        public void JN10402Action(Player player, int type, string fuse, string argst)
        {
            var opps = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                XI.Board.IsAttendWar(p) && p.Team == player.OppTeam).ToList();
            if (opps.Any())
                Harm(player, opps, 1);
        }
        #endregion XJ104 - LinYueru
        #region XJ105 - A'Nu
        public bool JN10501Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Any(
                p => p != player && p.Team == player.Team && p.IsTared);
        }
        public void JN10501Action(Player player, int type, string fuse, string argst)
        {
            if (argst != "0")
            {
                int idx = argst.LastIndexOf(',');
                ushort to = ushort.Parse(argst.Substring(idx + 1));
                if (to != 0)
                {
                    string cardss = argst.Substring(0, idx);
                    ushort[] cards = cardss.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    VI.Cout(0, "阿奴发动「鬼精灵」,将{0}交予{1}.", XI.DisplayTux(cards), XI.DisplayPlayer(to));
                    XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1," + cards.Length + "," + cardss);
                }
            }
        }
        public string JN10501Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                string c = (player.Tux.Count > 1 ? "/+Q1~" + player.Tux.Count : "Q1") +
                    Util.SatoWithBracket(player.Tux, "p", "(p", ")");
                string t = "/T1" + Util.SSelect(XI.Board, p => p != player && p.IsTared && p.Team == player.Team);
                return c + "," + t;
            }
            else return "";
        }
        public string JN10501Encrypt(string args)
        {
            string[] splits = args.Split(',');
            if (splits.Length >= 2)
                return "0," + (splits.Length - 1) + "," + splits[splits.Length - 1];
            else
                return args;
        }
        public bool JN10502Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count == 0;
        }
        public void JN10502Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "阿奴发动「万蛊蚀天」.");
            XI.RaiseGMessage("G0DH," + Util.SParal(XI.Board,
                p => p.IsAlive && p.Team == player.Team, p => p.Uid + ",0,1", ","));
            Harm(player, XI.Board.Garden.Values.Where(p => p.IsAlive && p != player), 1);
        }
        #endregion XJ105 - A'Nu
        #region XJ106 - Jiujianxian
        public bool JN10601Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0 || type == 1)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void JN10601Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        XI.RaiseGMessage("G0IX," + who + ",0,1");
                }
                //XI.InnerGMessage(fuse, 121);
            }
            else if (type == 1)
            {
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    ushort weq = ushort.Parse(blocks[i + 1]);
                    if (who == player.Uid && XI.LibTuple.TL.DecodeTux(weq).Type == Base.Card.Tux.TuxType.WQ)
                        XI.RaiseGMessage("G0OX," + who + ",0,1");
                }
                //XI.InnerGMessage(fuse, 71);
            }
        }
        public bool JN10602Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.RAMUshort == 0 && XI.Board.MonPiles.Count > 0;
            else if (type == 1)
                return XI.Board.RoundIN == ("R" + player.Uid + "BC") && player.RAMUshort == 0;
            else if (type == 2)
                return player.RAMUshort == 1;
            else
                return false;
        }
        public void JN10602Action(Player player, int type, string fuse, string args)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G1SG,0");
                string yes = XI.AsyncInput(player.Uid, "#是否进行第二次战斗？##不进行##进行,Y2", "JN10602", "0");
                if (yes.Equals("2"))
                    XI.RaiseGMessage("G0JM,R" + player.Uid + "ZW");
                else
                    player.RAMUshort = 1;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                string g0dh = "";
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort ut = ushort.Parse(blocks[i]);
                    int gtype = int.Parse(blocks[i + 1]);
                    int n = int.Parse(blocks[i + 2]);
                    if (ut == player.Uid && gtype == 0 && n > 0)
                        g0dh += "," + ut + ",0," + (n + 1);
                    else
                        g0dh += "," + ut + "," + gtype + "," + n;
                }
                if (g0dh.Length > 0)
                    XI.InnerGMessage("G0DH" + g0dh, 41);
            }
            else if (type == 2)
                XI.Board.AllowNoSupport = false;
        }
        #endregion XJ106 - Jiujianxian
        #region XJ107 - Baiyue Lord
        public bool JN10701Valid(Player player, int type, string fuse)
        {
            if (XI.Board.IsAttendWar(player) && XI.Board.Battler.IsMonster())
            {
                Base.Card.Monster b = (Base.Card.Monster)(XI.Board.Battler);
                return b != null && (b.Element == Base.Card.FiveElement.AQUA ||
                    b.Element == Base.Card.FiveElement.AGNI);
            }
            return false;
        }
        public void JN10701Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "拜月教主发动「水魔兽合体」，战力+2.");
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
        }
        public bool JN10702Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && player.Tux.Count >= 2 && player.RestZP > 0;
        }
        public void JN10702Action(Player player, int type, string fuse, string argst)
        {
            if (argst != "0")
            {
                player.RestZP = 0;
                VI.Cout(0, "拜月教主发动「召唤水魔兽」，战力池+5.");
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
                XI.RaiseGMessage("G0IP," + player.Team + ",5");
            }
        }
        public string JN10702Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q2(p" + string.Join("p", player.Tux) + ")";
            else return "";
        }
        #endregion XJ107 - Baiyue Lord
        #region XJ201 - WangXiaohu
        public bool JN20101Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void JN20101Action(Player player, int type, string fuse, string args)
        {
            XI.RaiseGMessage("G0TT," + player.Uid);
            int value = XI.Board.DiceValue;
            if (value == 1 || value == 6)
                value = 0;
            player.RAMUshort = (ushort)value;
            if (value > 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + value);
        }
        public bool JN20102Valid(Player player, int type, string fuse)
        {
            ushort who = ushort.Parse(fuse.Substring(fuse.IndexOf(',') + 1));
            return player.Tux.Count > 0 && player.Uid == who;
        }
        public void JN20102Action(Player player, int type, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            if (player.Tux.Contains(card))
            {
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                XI.RaiseGMessage("G0TT," + player.Uid);
            }
            //XI.InnerGMessage(fuse, 111);
        }
        public string JN20102Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        #endregion XJ201 - Wangxiaohu
        #region XJ202 - SuMei
        public bool JN20201Valid(Player player, int type, string fuse)
        {
            return player.RAMUshort == 0 && XI.Board.MonPiles.Count > 0;
        }
        public void JN20201Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G1SG,0");
            string yes = XI.AsyncInput(player.Uid, "#是否放弃此怪，翻出新怪？##不翻出##翻出,Y2", "JN20201", "0");
            if (yes.Equals("2"))
            {
                VI.Cout(0, "苏媚触发「狡猾」.");
                if (XI.Board.Mon1From == 0)
                    XI.RaiseGMessage("G0ON,0,M,1," + XI.Board.Monster1);
                else
                    XI.Board.Mon1From = 0;
                XI.Board.Monster1 = 0; XI.Board.Battler = null;
                player.RAMUshort = 1;
                XI.RaiseGMessage("G0JM,R" + player.Uid + "ZM");
            }
        }
        public bool JN20202Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public void JN20202Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            VI.Cout(0, "苏媚使用「拒绝」.");
            XI.RaiseGMessage("G0CC," + player.Uid + ",0,TP01," + card + ";0," + fuse);
        }
        public string JN20202Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cands = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Base.Card.Tux.TuxType.TP).ToList();
                if (cands.Count > 0)
                    return "/Q1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        #endregion XJ202 - SuMei
        #region XJ203 - ShenQishuang
        public bool JN20301Valid(Player player, int type, string fuse)
        {
            bool notin = false;
            if (player.Team == XI.Board.Rounder.Team)
                notin = XI.Board.Supporter == null || !XI.Board.SupportSucc;
            else if (player.Team == XI.Board.Rounder.OppTeam)
                notin = XI.Board.Hinder == null || !XI.Board.HinderSucc;

            if (type == 0 || (type == 1 && XI.Board.InFight))
            {
                if (player.RAMUshort == 0 && notin)
                    return true;
                else if (player.RAMUshort == 1 && !notin)
                    return true;
            }
            return false;
        }
        public void JN20301Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                if (player.Team == XI.Board.Rounder.Team)
                {
                    VI.Cout(0, "沈欺霜触发「仙霞五奇」，触发者灵力+3.");
                    player.RAMUshort = 1;
                    XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,3");
                }
                else if (player.Team == XI.Board.Rounder.OppTeam)
                {
                    if (Base.Card.NMBLib.IsMonster(XI.Board.Monster1))
                    {
                        VI.Cout(0, "沈欺霜触发「仙霞五奇」，怪物灵力+3.");
                        player.RAMUshort = 1;
                        XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + ",3");
                    }
                }
            }
            else if (type == 1)
            {
                if (player.Team == XI.Board.Rounder.Team)
                {
                    if (player.RAMUshort == 0)
                    {
                        player.RAMUshort = 1;
                        XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,3");
                    }
                    else if (player.RAMUshort == 1)
                    {
                        player.RAMUshort = 0;
                        XI.RaiseGMessage("G0OA," + XI.Board.Rounder.Uid + ",1,3");
                    }
                }
                else if (player.Team == XI.Board.Rounder.OppTeam)
                {
                    if (Base.Card.NMBLib.IsMonster(XI.Board.Monster1))
                    {
                        if (player.RAMUshort == 0)
                        {
                            player.RAMUshort = 1;
                            XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + ",3");
                        }
                        else if (player.RAMUshort == 1)
                        {
                            player.RAMUshort = 0;
                            XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + ",3");
                        }
                    }
                }
            }
            //XI.RaiseGMessage("G0IP," + player.Team + ",3");
        }
        public void JN20302Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort card = ushort.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));
            VI.Cout(0, "沈欺霜对{0}使用「元灵归心术」.", XI.DisplayPlayer(to));
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
            Cure(player, XI.Board.Garden[to], 2);
        }
        public string JN20302Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cands = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Base.Card.Tux.TuxType.JP).ToList();
                if (cands.Count > 0)
                    return "/+Q1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else if (!prev.Contains(","))
                return "/T1" + AAllTareds(player);
            else
                return "";
        }
        #endregion XJ203 - ShenQishuang
        #region XJ206 - KongLin
        public void JN20601Action(Player player, int type, string fuse, string args)
        {
            ushort who = ushort.Parse(args);
            Harm(player, new Player[] { player, XI.Board.Garden[who] }, 1);
            player.RAMPeoples.Add(who);
        }
        public string JN20601Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> v1 = XI.Board.Garden.Values.Where(p => p.IsTared && p.Gender == 'F' &&
                    !player.RAMPeoples.Contains(p.Uid)).Select(p => p.Uid).ToList();
                if (v1.Any())
                    return "/T1(p" + string.Join("p", v1) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JN20601Valid(Player player, int type, string fuse)
        {
            return player.HP >= 2;
        }
        public void JN20602Action(Player player, int type, string fuse, string args)
        {
            // G0ZW,A,B...
            string[] blocks = fuse.Split(',');
            string zw = "";
            for (int i = 1; i < blocks.Length; ++i)
            {
                if (blocks[i] != player.Uid.ToString())
                    zw += "," + blocks[i];
            }
            XI.RaiseGMessage("G0OY,0," + player.Uid);
            XI.RaiseGMessage("G0IY,0," + player.Uid + ",10207");
            if (zw != "")
                XI.InnerGMessage("G0ZW" + zw, -9);
        }
        public bool JN20602Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; ++i)
                if (blocks[i] == player.Uid.ToString())
                    return true;
            return false;
        }
        #endregion XJ206 - KongLin
        #region XJ207 - Mozun
        public void JN20701Action(Player player, int type, string fuse, string args)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public void JN20702Action(Player player, int type, string fuse, string args)
        {
            Harm(player, player, 1);
        }
        #endregion XJ207 - Mozun
        #region XJ302 - TangXuejian
        public bool JN30201Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (XI.Board.Garden[harm.Who].IsAlive && harm.N > 0 &&
                        harm.Element != FiveElement.LOVE &&
                        !Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.TERMIN))
                    {
                        if (!player.IsSKOpt)
                            return true;
                        else if (player.IsSKOpt && harm.Who != player.Uid)
                            return true;
                    }
                }
                return false;
            }
            return false;
        }
        public void JN30201Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            ISet<Player> invs = new HashSet<Player>();
            foreach (Artiad.Harm harm in harms)
            {
                if (XI.Board.Garden[harm.Who].IsAlive && harm.N > 0 && harm.Element != FiveElement.LOVE
                        && !Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.TERMIN))
                    invs.Add(XI.Board.Garden[harm.Who]);
            }
            if (invs.Count > 0)
            {
                VI.Cout(0, "唐雪见发动「追打」.");
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                Harm(player, invs, 1);
            }
            //XI.InnerGMessage(fuse, 151);
        }
        public string JN30201Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public void JN30202Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            VI.Cout(0, "唐雪见发动「连击」.");
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
            XI.RaiseGMessage("G0IA," + player.Uid + ",2,2");
        }
        public string JN30202Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cands = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type != Base.Card.Tux.TuxType.ZP).ToList();
                if (cands.Count > 0)
                    return "/Q1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JN30202Valid(Player player, int type, string fuse)
        {
            if (player.IsSKOpt && !XI.Board.IsAttendWarSucc(player))
                return false;
            return true;
        }
        public bool JN30203Valid(Player player, int type, string fuse)
        {
            return player.HP >= 2 && XI.Board.IsAttendWar(player);
        }
        public void JN30203Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "唐雪见发动「好胜」.");
            Harm(player, player, 2, FiveElement.YIN);
            if (player.IsAlive)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
        }
        #endregion XJ302 - TangXuejian
        #region XJ303 - LongKui Blue
        public void JN30301Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10304");
        }
        public bool JN30302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                if (player.RAMString != "" && player.RAMUshort == 0)
                {
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(player.RAMString);
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
                    string cardCode = blocks[3];
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardCode);
                    if (tux != null && tux.Type == Base.Card.Tux.TuxType.JP)
                        return true;
                }
            }
            return false;
        }
        public void JN30302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                Harm(player, player, 1);
                if (player.IsAlive)
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.RAMString + ",0;0," + fuse);
                player.RAMUshort = 1;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                player.RAMString = blocks[3];
                //XI.InnerGMessage(fuse, 121);
            }
        }
        public bool JN30303Valid(Player player, int type, string fuse)
        {
            if (player.IsSKOpt && !XI.Board.IsAttendWarSucc(player))
                return false;

            List<ushort> equips = player.ListOutAllEquips().ToList();
            foreach (string ce in XI.Board.CsEqiups)
            {
                int idx = ce.IndexOf(',');
                ushort who = ushort.Parse(ce.Substring(0, idx));
                ushort card = ushort.Parse(ce.Substring(idx + 1));
                if (who == player.Uid)
                    equips.Remove(card);
            }
            return equips.Count > 0;
        }
        public void JN30303Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            if (card != 0)
            {
                XI.RaiseGMessage("G0ZC," + player.Uid + ",2," + card + ";" + fuse);
                XI.RaiseGMessage("G0IA," + player.Uid + ",2,3");
            }
        }
        public string JN30303Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> equips = player.ListOutAllEquips().ToList();
                foreach (string ce in XI.Board.CsEqiups)
                {
                    int idx = ce.IndexOf(',');
                    ushort who = ushort.Parse(ce.Substring(0, idx));
                    ushort card = ushort.Parse(ce.Substring(idx + 1));
                    if (who == player.Uid)
                        equips.Remove(card);
                }
                return "/Q1(p" + string.Join("p", equips) + ")";
            }
            else return "";
        }
        #endregion XJ303 - LongKui Blue
        #region XJ304 - LongKui Red
        public void JN30401Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10303");
        }
        public bool JN30402Valid(Player player, int type, string fuse)
        {
            foreach (Player py in XI.Board.Garden.Values)
            {
                if (py.Uid != player.Uid && py.IsAlive && py.Team == player.Team)
                {
                    if (py.HasAnyEquips())
                        return true;
                }
            }
            return false;
        }
        public void JN30402Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort from = ushort.Parse(argst.Substring(0, idx));
            ushort card = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + from + ",0,1," + card);
        }
        public string JN30402Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.Uid != player.Uid &&
                    p.IsAlive && p.Team == player.Team && p.HasAnyEquips()).Select(p => p.Uid)) + ")";
            else if (!prev.Contains(','))
            {
                ushort who = ushort.Parse(prev);
                Player py = XI.Board.Garden[who];
                return "/C1(p" + string.Join("p", py.ListOutAllEquips()) + ")";
            }
            else return "";
        }
        public bool JN30403Valid(Player player, int type, string fuse)
        {
            return player.HasAnyEquips();
        }
        public void JN30403Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort card = ushort.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));
            if (card != 0)
            {
                XI.RaiseGMessage("G0ZC," + player.Uid + ",2," + card + ";" + fuse);
                Harm(player, XI.Board.Garden[to], 3);
            }
        }
        public string JN30403Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.ListOutAllEquips()) + ")";
            else if (!prev.Contains(","))
                return "/T1" + AAllTareds(player);
            else return "";
        }
        #endregion XJ304 - LongKui Red
        #region XJ305 - ZiXuan
        public bool JN30501Valid(Player player, int type, string fuse)
        {
            // G0HD,0,A,B,x
            string[] blocks = fuse.Split(',');
            if (blocks[1] == "0")
            {
                ushort who = ushort.Parse(blocks[2]);
                if (XI.Board.Garden[who].Team == player.Team)
                    return true;
            }
            else if (blocks[1] == "1")
            {
                ushort who = ushort.Parse(blocks[2]);
                ushort where = ushort.Parse(blocks[3]);
                if (where != 0 && XI.Board.Garden[who].Team == XI.Board.Garden[where].Team)
                    return false;
                else if (XI.Board.Garden[who].Team != player.Team)
                    return false;
                else
                    return true;
            }
            return false;
        }
        public void JN30501Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#获得2张补牌,T1(p" + string.Join(
                "p", XI.Board.Garden.Values.Where(p => p.IsTared && p.Team == player.Team)
                .Select(p => p.Uid)) + ")", "JN30501", "0");
            ushort who = ushort.Parse(input);
            if (who != 0)
                XI.RaiseGMessage("G0DH," + who + ",0,2");
            //XI.InnerGMessage(fuse, 131);
        }
        public bool JN30502Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            //if (type == 0)
            //{
            //    // G0IC,A,x
            //    ushort who = ushort.Parse(blocks[1]);
            //    return who == player.Uid;
            //}
            //else if (type == 1)
            //{
            //    // G0OY,0/1,A
            //    ushort who = ushort.Parse(blocks[2]);
            //    return who == player.Uid && player.Pets.Where(p => p != 0).Any();
            //}
            //if (type == 0)
            //{
            //    // G0HD,0,A,B,x/G0HD,1,A,B,x
            //    ushort who = ushort.Parse(blocks[2]);
            //    //ushort pt = ushort.Parse(blocks[1] == "0" ? blocks[3] : blocks[4]);
            //    return who == player.Uid;
            //}
            //else if (type == 1)
            //{
            //    // G0HL,A,x
            //    ushort who = ushort.Parse(blocks[1]);
            //    ushort pt = ushort.Parse(blocks[2]);
            //    return who == player.Uid && XI.Board.Garden[who].Pets.Contains(pt);
            //}
            //else if (type == 2)
            //{
            ushort x = ushort.Parse(blocks[1]);
            return player.Pets.Contains(x);
            //}
            //return false;
        }
        public void JN30502Action(Player player, int type, string fuse, string argst)
        {
            // G0I/OC,A,x
            //string[] blocks = fuse.Split(',');
            //if (type == 0)
            //{
            //    ushort x = ushort.Parse(blocks[2]);
            //    if (x != 0)
            //        XI.RaiseGMessage("G0IB," + x + ",3");
            //}
            //else if (type == 1)
            //{
            //    foreach (ushort pt in player.Pets)
            //        if (pt != 0)
            //            XI.RaiseGMessage("G0OB," + pt + ",3");
            //}
            //XI.InnerGMessage(fuse, 121);
            string[] blocks = fuse.Split(',');
            //if (type == 0)
            //{
            //    ushort x = ushort.Parse(blocks[1] == "0" ? blocks[3] : blocks[4]);
            //    if (x != 0)
            //        XI.RaiseGMessage("G0IB," + x + ",3");
            //}
            //else if (type == 1)
            //{
            //    ushort x = ushort.Parse(blocks[2]);
            //    if (x != 0)
            //        XI.RaiseGMessage("G0OB," + x + ",3");
            //}
            //else if (type == 2)
            //{
            ushort x = ushort.Parse(blocks[1]);
            if (player.Pets.Contains(x))
                XI.RaiseGMessage("G0IB," + x + ",3");
            //}
            //XI.InnerGMessage(fuse, 111);
        }
        #endregion XJ305 - Zixuan
        #region XJ306 - ChongLou
        public bool JN30601Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared
                    && p.Uid != player.Uid) && player.Tux.Count >= player.RAMUshort + 1;
        }
        public string JN30601Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> all = XI.Board.Garden.Values.Where(p => p.IsTared
                    && p.Uid != player.Uid).Select(p => p.Uid).ToList();
                if (all.Count >= 2)
                    return "/Q" + (player.RAMUshort + 1) + "(p" + string.Join("p", player.Tux) + ")"
                        + ",/T1~2(p" + string.Join("p", all) + ")";
                else
                    return "/Q" + (player.RAMUshort + 1) + "(p" + string.Join("p", player.Tux) + ")"
                        + ",/T1(p" + string.Join("p", all) + ")";
            }
            else
                return "";
        }
        public void JN30601Action(Player player, int type, string fuse, string args)
        {
            string[] blocks = args.Split(',');
            List<ushort> restCards = Util.TakeRange(blocks, 0, player.RAMUshort + 1)
                .Select(p => ushort.Parse(p)).ToList();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", restCards));
            List<ushort> tos = Util.TakeRange(blocks, player.RAMUshort + 1, blocks.Length)
                .Select(p => ushort.Parse(p)).ToList();
            ++player.RAMUshort;
            int maskDuel = Artiad.IntHelper.SetMask(0, GiftMask.ALIVE_DUEL, true);
            foreach (ushort to in tos)
            {
                XI.RaiseGMessage("G0TT," + player.Uid);
                int myDice = XI.Board.DiceValue;
                XI.RaiseGMessage("G0TT," + to);
                int toDice = XI.Board.DiceValue;
                if (myDice > toDice)
                    Harm(player, XI.Board.Garden[to], 3, FiveElement.YIN, maskDuel);
                else if (myDice < toDice)
                    Harm(player, player, 3, FiveElement.YIN, maskDuel);
                else
                    Harm(player, new Player[] { XI.Board.Garden[to], player }, 2, FiveElement.YIN, maskDuel);
            }
        }
        public bool JN30602Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.ALIVE_DUEL)
                        && harm.N > 0 && py.HP >= 2 && py.HP - harm.N < 1)
                    return true;
            }
            return false;
        }
        public void JN30602Action(Player player, int type, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.ALIVE_DUEL)
                        && harm.N > 0 && py.HP >= 2 && py.HP - harm.N < 1)
                    harm.N = py.HP - 1;
            }
            XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 96);
        }
        public bool JN30603Valid(Player player, int type, string fuse)
        {
            return XI.Board.Rounder.Team == player.OppTeam && XI.Board.IsAttendWar(player);
        }
        public void JN30603Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "重楼触发「降临」，对{0}的妨碍命中+2.", XI.DisplayPlayer(XI.Board.Rounder.Uid));
            XI.RaiseGMessage("G0IX," + player.Uid + ",1,2");
        }
        #endregion XJ306 - ChongLou
        #region X3W01 - Nan'gongHuang
        public bool JN40101Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.MonPiles.Count >= 3;
        }
        public void JN40101Action(Player player, int type, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
            XI.RaiseGMessage("G0XZ," + player.Uid + ",2,1,3,1");
            ushort[] pops = XI.Board.MonPiles.Dequeue(2);
            XI.RaiseGMessage("G2IN,1,2");
            XI.RaiseGMessage("G0ON,0,M,2," + string.Join(",", pops));
        }
        public string JN40101Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else
                return "";
        }
        public bool JN40102Valid(Player player, int type, string fuse)
        {
            //G0HC,0,A,x
            string[] blocks = fuse.Split(',');
            if (blocks[1] == "0" && blocks[2] == player.Uid.ToString())
            {
                ushort card = ushort.Parse(blocks[4]);
                Base.Card.Monster monster = XI.LibTuple.ML.Decode(card);
                int element = Util.GetFiveElementId(monster.Element);
                if (element >= 0)
                {
                    return XI.Board.Garden.Values.Where(p => p.IsAlive &&
                        p.Team == player.OppTeam && p.Pets[element] != 0).Any();
                }
            }
            return false;
        }
        public void JN40102Action(Player player, int type, string fuse, string args)
        {
            int idx = args.IndexOf(',');
            ushort who = ushort.Parse(args.Substring(0, idx));
            ushort pet = ushort.Parse(args.Substring(idx + 1));
            XI.RaiseGMessage("G0HL," + who + "," + pet);
            XI.RaiseGMessage("G0ON," + who + ",M,1," + pet);
            //XI.InnerGMessage(fuse, 141);
        }
        public string JN40102Input(Player player, int type, string fuse, string prev)
        {
            string[] blocks = fuse.Split(',');
            ushort card = ushort.Parse(blocks[4]);
            Base.Card.Monster monster = XI.LibTuple.ML.Decode(card);
            int element = Util.GetFiveElementId(monster.Element);
            if (prev == "")
                return "#弃置宠物的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsAlive && p.Team == player.OppTeam && p.Pets[element] != 0)
                    .Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                return "#弃置的,/M1(p" + XI.Board.Garden[who].Pets[element] + ")";
            }
            else
                return "";
        }
        #endregion X3W01 - Nan'gongHuang
        #region X3W02 - WenHui
        public bool JN40201Valid(Player player, int type, string fuse)
        {
            if (XI.Board.Rounder.Uid == player.Uid)
            {
                if (player.RAMUshort == 0 && XI.Board.SupportSucc)
                    return true;
                else if (player.RAMUshort == 1 && !XI.Board.SupportSucc)
                    return true;
            }
            return false;
        }
        public void JN40201Action(Player player, int type, string fuse, string argst)
        {
            if (player.RAMUshort == 0)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
                player.RAMUshort = 1;
            }
            else if (player.RAMUshort == 1)
            {
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,3");
                player.RAMUshort = 0;
            }
        }
        public bool JN40202Valid(Player player, int type, string fuse)
        {
            return !XI.Board.IsRounderBattleWin() && XI.Board.Garden.Values
                .Any(p => p.IsTared && p.Uid != player.Uid);
        }
        public void JN40202Action(Player player, int type, string fuse, string argst)
        {
            ushort who = ushort.Parse(argst);
            Harm(player, XI.Board.Garden[who], 2);
        }
        public string JN40202Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1" + AAllTareds(player);
            else
                return "";
        }
        #endregion X3W02 - WenHui
        #region X3W03 - Xingxuan
        public bool JN40301Valid(Player player, int type, string fuse)
        {
            bool b1 = player.Tux.Count >= 2;
            if (type == 0)
                return b1;
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                return b1 && Util.TakeRange(blocks, 1, blocks.Length).Select(
                    p => XI.Board.Garden[ushort.Parse(p)]).Where(p => p.IsTared && p.HP == 0).Any();
            } else
                return false;
        }
        public void JN40301Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "星璇使用「烹饪」.");
            XI.RaiseGMessage("G0CC," + player.Uid + ",0,TP02," + argst + ";" + type + "," + fuse);
        }
        public string JN40301Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                if (player.Tux.Count >= 2)
                    return "/Q2(p" + string.Join("p", player.Tux) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JN40302Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Select(p => p.Uid != player.Uid &&
                p.IsAlive && p.Team == player.Team && p.Tux.Count > 0).Any();
        }
        public void JN40302Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "星璇预定发动「兄弟」.");
            string stage1 = "G0HQ,0," + player.Uid + "," + string.Join(",", XI.Board.Garden.Values.Where(
                p => p.Uid != player.Uid && p.IsAlive && p.Team == player.Team && p.Tux.Count > 0).Select(
                p => p.Uid + ",2," + p.Tux.Count));
            XI.RaiseGMessage(stage1);
            do
            {
                string targ = "/T1" + Util.SSelect(XI.Board, p => p.IsTared &&
                    p.Uid != player.Uid && p.Team == player.Team);
                string carg = "(p" + string.Join("p", player.Tux) + ")";
                carg = (player.Tux.Count > 1) ? ("/+Q1~" + player.Tux.Count + carg) : ("/+Q1" + carg);
                string select = XI.AsyncInput(player.Uid, carg + "," + targ, "JN40302", "0");
                if (select == "0" || select == "/0" || select == "")
                    break; // On Finish of Catching
                else
                {
                    int idx = select.LastIndexOf(',');
                    ushort to = ushort.Parse(select.Substring(idx + 1));
                    string cardsString = select.Substring(0, idx);
                    if (to == 0)
                        continue;
                    else
                    {
                        List<ushort> cards = cardsString.Split(',')
                            .Select(p => ushort.Parse(p)).ToList();
                        VI.Cout(0, "星璇发动「兄弟」将{0}交给了{1}.",
                            XI.DisplayTux(cards), XI.DisplayPlayer(to));
                        XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,"
                            + cards.Count() + "," + Util.Sato(cards, ","));
                    }
                }
            } while (player.Tux.Count > 0);
        }
        #endregion X3W03 - Xingxuan
        #region X3W04 - WangPengxu
        public void JN40401Action(Player player, int type, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            if (card != 0 && player.ListOutAllCards().Contains(card))
            {
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                Cure(player, player, 2);
            }
        }
        public bool JN40401Valid(Player player, int type, string fuse)
        {
            return player.HasAnyCards();
        }
        public string JN40401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.ListOutAllCards()) + ")";
            else
                return "";
        }
        public void JN40402Action(Player player, int type, string fuse, string args)
        {
            List<ushort> cards = args.Split(',').Select(p => ushort.Parse(p)).ToList();
            XI.RaiseGMessage("G0ZB," + player.Uid + ",2," + string.Join(",", cards));
            //foreach (ushort ut in cards)
            //    XI.RaiseGMessage("G0ZB," + player.Uid + ",2," + ut);
            //XI.RaiseGMessage("G0IA," + player.Uid + ",0," + cards.Count);
            if (type == 6)
                XI.InnerGMessage(fuse, 50);
        }
        public string JN40402Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cds = player.Tux.Where(p =>
                    !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()).ToList();
                int count = (cds.Count + player.ExCards.Count <= 5) ?
                    cds.Count : (5 - player.ExCards.Count);
                if (cds.Count == 0)
                    return "/";
                else
                    return "/Q1" + ((count > 1) ? ("~" + count) : "") + "(p" + string.Join("p", cds) + ")";
            }
            else
                return "";
        }
        public bool JN40402Valid(Player player, int type, string fuse)
        {
            //bool basecon = player.ExCards.Count < 5 && player.Tux.Where(p =>
            //    !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()).Any();
            bool basecon = player.ExCards.Count < 5 && player.Tux.Count > 0;
            if (!basecon)
                return false;

            if (!player.IsSKOpt)
            {
                if (type == 6)
                {
                    // G0CD,A,KN,x1,x2;TF
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    return (blocks[1] != player.Uid.ToString()) &&
                        (blocks[2] == "JP01" || blocks[2] == "JP06");
                }
                else
                    return true;
            }
            else
            {
                if (type == 0 || type == 2 || type == 3)
                    return true;
                if (type == 1)
                {// Ask for CC
                    Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                    return mon != null && mon.IsTuxInvolved(XI.Board.Rounder.Uid == player.Uid,
                        XI.Board.IsAttendWar(player), true, false, XI.Board.Rounder.Team == player.Team);
                }
                else if (type == 4)
                {
                    Base.Card.Evenement eve = XI.LibTuple.EL.DecodeEvenement(XI.Board.Eve);
                    return eve.IsTuxInvolved(true);
                }
                else if (type == 5)
                {// Ask for WN/LS
                    Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                    Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                    bool b1 = XI.Board.Rounder.Uid == player.Uid;
                    bool b2 = XI.Board.IsAttendWar(player);
                    bool b3 = false;
                    bool b4 = XI.Board.IsRounderBattleWin();
                    bool b5 = XI.Board.Rounder.Team == player.Team;
                    return mon1 != null && mon2 != null && mon1.IsTuxInvolved(b1, b2, b3,
                        b4, b5) && mon2.IsTuxInvolved(b1, b2, b3, b4, b5);
                }
                else if (type == 6)
                {
                    // G0CD,A,KN,x1,x2;TF
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    ushort user = ushort.Parse(blocks[1]);
                    return user != 0 && user != player.Uid && XI.Board.Garden[user].Team == player.OppTeam
                        && (blocks[2] == "JP01" || blocks[2] == "JP06");
                }
            }
            return basecon;
        }
        public void JN40403Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                // G0ZB,A,2/3/4,x
                string[] blocks = fuse.Split(',');
                if (blocks[1] == player.Uid.ToString() && blocks[2] == "2")
                {
                    int n = blocks.Length - 3;
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
                //XI.InnerGMessage(fuse, 91);
            }
        }
        public bool JN40403Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                // G0ZB,A,2/3/4,x
                string[] blocks = fuse.Split(',');
                if (blocks[1] == player.Uid.ToString() && blocks[2] == "2")
                    return blocks.Length > 3;
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
        #endregion X3W04 - WangPengxu
        #region XJ401 - YunTianhe
        public bool JN50101Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return XI.Board.IsAttendWar(player) && XI.Board.Battler != null &&
                    player.DEX - XI.Board.Battler.AGL >= 4 && (player.RAMUshort == 0);
            }
            else if (type == 1)
            {
                if (XI.Board.IsAttendWar(player) && XI.Board.Battler != null && XI.Board.InFight)
                {
                    if (player.DEX - XI.Board.Battler.AGL >= 4 && (player.RAMUshort == 0))
                        return true;
                }
            }
            else if (type == 2)
            {
                if (XI.Board.IsAttendWar(player) && XI.Board.Battler != null && XI.Board.InFight)
                {
                    if (player.DEX - XI.Board.Battler.AGL < 4 && (player.RAMUshort == 1))
                        return true;
                }
            }
            return false;
        }
        public void JN50101Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                VI.Cout(0, "云天河发动「天河剑」.");
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
                player.RAMUshort = 1;
            }
            else if (type == 1)
            {
                VI.Cout(0, "云天河触发「天河剑」.");
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
                player.RAMUshort = 1;
            }
            else if (type == 2)
            {
                VI.Cout(0, "云天河触发「天河剑」.");
                XI.RaiseGMessage("G0OA," + player.Uid + ",1,2");
                player.RAMUshort = 0;
            }
        }
        public void JN50102Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                VI.Cout(0, "云天河发动「后羿射日弓」.");
                XI.RaiseGMessage("G0IP," + player.Team + ",8");
                player.ROMUshort = 1;
            }
            else if (type == 1)
            {
                VI.Cout(0, "云天河基础命中数值变为0.");
                Base.Card.Hero hero = XI.LibTuple.HL.InstanceHero(player.SelectHero);
                if (hero != null)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0," + hero.DEX);
                else
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0,6");
                player.ROMUshort = 2;
            }
        }
        public bool JN50102Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.ROMUshort == 0;
            else if (type == 1)
                return player.ROMUshort == 1;
            else
                return false;
        }
        #endregion XJ401 - YunTianhe
        #region XJ402 - HanLingsha
        public bool JN50201Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Where(
                p => p.Uid != player.Uid && p.HasAnyCards()).Any();
        }
        public void JN50201Action(Player player, int type, string fuse, string argst)
        {
            string[] args = argst.Split(',');
            ushort card = ushort.Parse(args[0]);
            if (card != 0)
            {
                ushort choose = ushort.Parse(args[1]);
                if (choose == 1)
                {
                    VI.Cout(0, "韩菱纱发动「搜囊探宝」.");
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0,JP06," + card + ";0," + fuse);
                }
                else if (choose == 2)
                {
                    VI.Cout(0, "韩菱纱发动「搜囊探宝」.");
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0,JP01," + card + ";0," + fuse);
                }
            }
        }
        public string JN50201Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                bool tp01 = false;
                foreach (Player p in XI.Board.Garden.Values)
                {
                    if (p != player && (p.Tux.Count > 0))
                    {
                        tp01 |= true; break;
                    }
                }
                string cp = "/Q1(p" + Util.Sato(player.Tux, "p") + ")";
                return cp + (tp01 ? ",#请选择「搜囊探宝」执行项。##铜钱镖##偷盗,/Y2" :
                    ",#请选择「搜囊探宝」执行项。##铜钱镖,/Y1");
            }
            else
                return "";
        }
        public void JN50202Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "韩菱纱发动「劫富济贫」.");
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            if (player.Tux.Count >= 2)
                XI.RaiseGMessage("G0DH," + player.Uid + ",1,1");
        }
        public bool JN50203Valid(Player player, int type, string fuse)
        {
            // G0ZW,A,B,C
            string[] args = fuse.Split(',');
            for (int i = 1; i < args.Length; ++i)
            {
                ushort who = ushort.Parse(args[i]);
                if (who == player.Uid)
                    return false;
                else
                {
                    Player p = XI.Board.Garden[who];
                    if (p.HasAnyCards())
                        return true;
                }
            }
            return false;
        }
        public void JN50203Action(Player player, int type, string fuse, string argst)
        {
            // G0ZW,A,B,C
            string[] args = fuse.Split(',');
            int people = 0;
            List<ushort> possiCards = new List<ushort>();
            VI.Cout(0, "韩菱纱预定发动「盗墓」.");
            for (int i = 1; i < args.Length; ++i)
            {
                ushort who = ushort.Parse(args[i]);
                Player fromPlayer = XI.Board.Garden[who];
                if (fromPlayer.Uid != player.Uid && fromPlayer.HasAnyCards())
                {
                    possiCards.AddRange(fromPlayer.ListOutAllCards());
                    XI.RaiseGMessage("G0HQ,1," + player.Uid + "," + who);
                    ++people;
                }
            }
            string select = null;
            do
            {
                string targ = "/T1" + Util.SSelect(XI.Board, p => p.IsTared && p.Uid != player.Uid);
                string carg = "(p" + string.Join("p", possiCards) + ")";
                carg = (possiCards.Count > 1) ? ("/+Q1~" + possiCards.Count + carg) : ("/+Q1" + carg);
                select = XI.AsyncInput(player.Uid, carg + "," + targ, "JN50203", "0");
                if (select == "0" || select == "/0" || select == "")
                    break; // On Finish of Catching
                else
                {
                    int idx = select.LastIndexOf(',');
                    ushort to = ushort.Parse(select.Substring(idx + 1));
                    string cardsString = select.Substring(0, idx);
                    if (to == 0)
                        continue;
                    else
                    {
                        List<ushort> cards = cardsString.Split(',').Select(p => ushort.Parse(p)).ToList();
                        VI.Cout(0, "韩菱纱将「盗墓」获得的卡牌{0}交给了{1}.", XI.DisplayTux(cards), XI.DisplayPlayer(to));
                        foreach (ushort cd in cards)
                            possiCards.Remove(cd);
                        XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1," + cards.Count() + "," + Util.Sato(cards, ","));
                    }
                }
            } while (possiCards.Count > 0);
            VI.Cout(0, "韩菱纱因发动「盗墓」受到1点伤害.");
            Harm(player, player, 1);
            XI.InnerGMessage(fuse, 191);
        }
        #endregion XJ402 - HanLingsha
        #region XJ403 - LiuMengli
        public void JN50301Action(Player player, int type, string fuse, string argst)
        {
            string[] blocks = fuse.Split(',');
            VI.Cout(0, "柳梦璃发动了「妖王」({0}).", type);
            if (type == 0 || type == 1)
            {
                IDictionary<ushort, int> table = new Dictionary<ushort, int>();
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    if (table.ContainsKey(who))
                        ++table[who];
                    else
                        table.Add(who, 1);
                }
                string title = (type == 0) ? "G0IA" : "G0OA";
                foreach (var pair in table)
                    XI.RaiseGMessage(title + "," + pair.Key + ",0," + pair.Value);
                //XI.InnerGMessage(fuse, 81);
            }
            else if (type == 2)
            {
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.Team && !py.PetDisabled)
                    {
                        int count = py.Pets.Where(p => p != 0).Count();
                        if (count > 0)
                            XI.RaiseGMessage("G0IA," + py.Uid + ",0," + count);
                    }
                //XI.InnerGMessage(fuse, 121);
            }
            else if (type == 3)
            {
                if (player.IsAlive)
                {
                    foreach (Player py in XI.Board.Garden.Values)
                        if (py.IsAlive && py.Team == player.Team && !py.PetDisabled)
                        {
                            int count = py.Pets.Where(p => p != 0).Count();
                            if (count > 0)
                                XI.RaiseGMessage("G0OA," + py.Uid + ",0," + count);
                        }
                    //XI.InnerGMessage(fuse, 91);
                }
            }
            else if (type == 4)
            {
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.Team && !py.PetDisabled)
                    {
                        int count = py.Pets.Where(p => p != 0).Count();
                        if (count > 0)
                            XI.RaiseGMessage("G0OA," + py.Uid + ",0," + count);
                    }
                //XI.InnerGMessage(fuse, 91);
            }
        }
        public bool JN50301Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            if (type == 0 || type == 1)
            {
                for (int i = 1; i < blocks.Length; i += 3)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    Player py = XI.Board.Garden[who];
                    if (player.IsAlive && py.Team == player.Team)
                        return true;
                }
                return false;
            }
            else if (type == 2)
            { // GOIY,0/1,A,S
                int heroCode = int.Parse(blocks[3]);
                var hero = XI.LibTuple.HL.InstanceHero(heroCode);
                if (hero != null && hero.Avatar == 10503)
                    return true;
                else return false;
            }
            else if (type == 3)
            { // GOOY,0/1,A
                for (int i = 1; i < blocks.Length; i += 2)
                    if (blocks[i + 1] == player.Uid.ToString())
                        return true;
                return false;
            }
            else if (type == 4)
            { // G0ZW,[A]
                for (int i = 1; i < blocks.Length; ++i)
                {
                    ushort who = ushort.Parse(blocks[i]);
                    if (who == player.Uid)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void JN50302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IX," + player.Uid + ",0," + (5 - 3));
                string[] blocks = fuse.Split(','); int idx = 0;
                for (int i = 1; i < blocks.Length; ++i)
                {
                    if (blocks[i].Equals(player.Uid.ToString()))
                    {
                        idx = i; break;
                    }
                }
                if (idx > 0)
                {
                    blocks[idx] = blocks[blocks.Length - 1];
                    XI.InnerGMessage(string.Join(",", Util.TakeRange(blocks, 0, blocks.Length - 1)), 351);
                }
            }
            else if (type == 1)
            {
                ushort ut = (ushort)(fuse[1] - '0');
                Player rp = XI.Board.Garden[ut];
                if (rp.Team == player.Team)
                    XI.Board.PosSupporters.Add(player.Uid.ToString());
                else
                    XI.Board.PosHinders.Add(player.Uid.ToString());
            }
            else if (type == 2)
            {
                XI.RaiseGMessage("G0OY,2," + player.Uid);
                //XI.InnerGMessage(fuse, 91);
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OX," + player.Uid + ",0," + (5 - 3));
                //XI.InnerGMessage(fuse, 81);
            }
        }
        public bool JN50302Valid(Player player, int type, string fuse)
        {
            if (type == 0 && !player.IsAlive)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; ++i)
                {
                    if (blocks[i].Equals(player.Uid.ToString()))
                        return true;
                }
                return false;
            }
            else if (type == 1 && !player.IsAlive)
                return true;
            else if (type == 2)
            {
                // GOIY,0/1,A,S
                string[] blocks = fuse.Split(',');
                int heroCode = int.Parse(blocks[3]);
                var hero = XI.LibTuple.HL.InstanceHero(heroCode);
                if (hero != null && hero.Avatar == 10503)
                    return true;
                else return false;
            }
            else if (type == 3)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 2)
                    if (blocks[i + 1] == player.Uid.ToString())
                        return true;
                return false;
            }
            else
                return false;
        }
        #endregion XJ403 - LiuMengli
        #region XJ404 - MurongZiying
        public bool JN50401Valid(Player player, int type, string fuse)
        {
            return player.ListOutAllEquips().Count > 0 && XI.Board.Garden.Values.Any(
                    p => p.IsTared && p.Uid != player.Uid && !player.RAMPeoples.Contains(p.Uid));
            //return player.Weapon != 0 || player.Armor != 0 || player.ExEquip != 0;
        }
        public void JN50401Action(Player player, int type, string fuse, string argst)
        {
            if (argst != "0" && !argst.EndsWith(",0"))
            {
                int idx = argst.IndexOf(',');
                ushort card = ushort.Parse(argst.Substring(0, idx));
                ushort who = ushort.Parse(argst.Substring(idx + 1));
                //XI.RaiseGMessage("G0QY,0," + player.Uid + "," + card);
                XI.RaiseGMessage("G0ZB," + who + ",1," + player.Uid + "," + card);
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
                player.RAMPeoples.Add(who);
            }
        }
        public string JN50401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                string c = "/Q1(p" + string.Join("p", player.ListOutAllEquips()) + ")";
                //string c = "/Q1(" + (player.Weapon != 0 ? ("p" + player.Weapon) : "")
                //    + (player.Armor != 0 ? ("p" + player.Armor) : "") + ")";
                string t = "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsTared && p.Uid != player.Uid && !player.RAMPeoples.Contains(p.Uid)
                    ).Select(p => p.Uid)) + ")";
                return "#装备," + c + "," + t;
            }
            else return "";
        }
        public bool JN50402Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return fuse.Substring("G0QR,".Length).Equals(player.Uid.ToString()) &&
                    player.Tux.Count > (player.TuxLimit - 2); // Just show!!
            else if (type == 1)
                return IsMathISOS("JN50402", player, fuse);
            else
                return false;
        }
        public void JN50402Action(Player player, int type, string fuse, string argst)
        {
            if (type == 1)
                player.TuxLimit += 2;
        }
        #endregion XJ404 - MurongZiying
        #region XJ405 - Xuanxiao
        public bool JN50501Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && (harm.Element == FiveElement.AQUA ||
                        harm.Element == FiveElement.AGNI))
                    return true;
            }
            return false;
        }
        public void JN50501Action(Player player, int type, string fuse, string argst)
        {
            // G0OH,A,Src,p,n,...
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid)
                {
                    if (harm.Element == FiveElement.AQUA)
                    {
                        VI.Cout(0, "玄霄发动「凝冰焚炎」,免疫本次水属性伤害.");
                        rvs.Add(harm);
                    }
                    else if (harm.Element == FiveElement.AGNI)
                    {
                        VI.Cout(0, "玄霄发动「凝冰焚炎」,免疫本次火属性伤害.");
                        rvs.Add(harm);
                    }
                }
            }
            harms.RemoveAll(p => rvs.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -39);
        }
        public void JN50502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string range = "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                    p => p.IsTared && p.Uid != player.Uid).Select(p => p.Uid)) + ")";
                string target = XI.AsyncInput(player.Uid, "#「结拜」的,T1" + range, "JN50502", "0");
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ushort.Parse(target));
                VI.Cout(0, "玄霄对玩家{0}发动了「结拜」.", XI.DisplayPlayer(player.ROMPlayerTar));
                XI.SendOutUAMessage(player.Uid, "JN50502," + target, "0");
                //if (type == 3)
                //    XI.InnerGMessage(fuse, 111);
            }
            else if (type == 1)
            {
                VI.Cout(0, "玄霄触发「结拜」，对玩家{0}的命中+1.", XI.DisplayPlayer(player.ROMPlayerTar));
                XI.RaiseGMessage("G0IX," + player.Uid + ",1,1");
            }
            else if (type == 2)
            {
                if (player.ROMPlayerTar.Count > 0 && XI.Board.Rounder.Uid == player.ROMPlayerTar[0])
                {
                    XI.RaiseGMessage("G0OX," + player.Uid + ",1,1");
                    VI.Cout(0, "玄霄失去「结拜」目标.");
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.ROMPlayerTar[0]);
                }
                //XI.InnerGMessage(fuse, 111);
            }
        }
        public bool JN50502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JN50502", player, fuse);
            else if (type == 1)
                return player.ROMPlayerTar.Count > 0 && player.ROMPlayerTar[0] == XI.Board.Rounder.Uid;
            else if (type == 2)
            {
                if (player.ROMPlayerTar.Count > 0)
                {
                    string[] blocks = fuse.Split(',');
                    for (int i = 1; i < blocks.Length; i += 2)
                    {
                        if ((blocks[i] == "0" || blocks[i] == "2") &&
                                player.ROMPlayerTar[0].ToString() == blocks[i + 1])
                            return true;
                    }
                }
                return false;
            }
            return false;
        }
        #endregion XJ405 - Xuanxiao
        #region XJ501 - JiangYunfan
        public bool JN60101Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && (player.RestZP == 0 || player.ZPDisabled);
        }
        public void JN60101Action(Player player, int type, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(card);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + tux.Code + "," + card + ";0," + fuse);
        }
        public string JN60101Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                //bool uzp = player.UsedZP;
                //player.UsedZP = false;
                List<ushort> v1 = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p).Type == Base.Card.Tux.TuxType.ZP
                    && XI.LibTuple.TL.DecodeTux(p).Valid(player, type, fuse)).ToList();
                //player.UsedZP = uzp;
                if (v1.Any())
                    return "/Q1(p" + string.Join("p", v1) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JN60102BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (type == 0)
            {
                // G0DI,A,0,n,B,1,m
                string[] blocks = fuse.Split(',');
                int idx = 1;
                Player oy = XI.Board.Garden[owner];
                if (!oy.IsAlive)
                    return false;
                while (idx < blocks.Length)
                {
                    if (blocks[idx + 1] == "0")
                    {
                        ushort who = ushort.Parse(blocks[idx]);
                        ushort n = ushort.Parse(blocks[idx + 2]);
                        Player py = XI.Board.Garden[who];
                        if (who == player.Uid && py.Team == oy.Team &&
                                who != owner && n > 0 && !oy.RAMPeoples.Contains(player.Uid))
                            return true;
                        idx += (n + 3);
                    }
                    else
                    {
                        ushort n = ushort.Parse(blocks[idx + 2]);
                        idx += (n + 3);
                    }
                }
            }
            else if (type == 1)
                return true;
            return false;
        }
        public void JN60102Action(Player player, int type, string fuse, string args)
        {
            if (type == 0)
            {
                int idx = args.IndexOf(',');
                if (idx >= 0)
                {
                    ushort to = ushort.Parse(args.Substring(0, idx)); // to = XJ501
                    string cdstr = args.Substring(idx + 1);
                    Player oy = XI.Board.Garden[to];
                    if (cdstr != "" && cdstr != "0" && !cdstr.StartsWith("/"))
                    {
                        ushort card = ushort.Parse(args.Substring(idx + 1));
                        XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + card);
                        oy.RAMPeoples.Add(player.Uid);
                        string[] blocks = fuse.Split(',');
                        string nfuse = "";
                        int jdx = 1;
                        while (jdx < blocks.Length)
                        {
                            ushort who = ushort.Parse(blocks[jdx]);
                            ushort n = ushort.Parse(blocks[jdx + 2]);
                            if (who != player.Uid)
                                nfuse += "," + string.Join(",", Util.TakeRange(blocks, jdx, jdx + 3 + n));
                            else
                            {
                                var rests = Util.TakeRange(blocks, jdx + 3, jdx + 3 + n).Where(p => p != card.ToString());
                                int rstCnt = rests.Count();
                                if (rstCnt > 0)
                                    nfuse += "," + blocks[jdx] + "," + blocks[jdx + 1] + "," + rstCnt + "," + string.Join(",", rests);
                            }
                            jdx += (n + 3);
                        }
                        if (nfuse.Length > 0)
                            XI.InnerGMessage(blocks[0] + nfuse, 120);
                        //while (jdx < blocks.Length)
                        //{
                        //    ushort who = ushort.Parse(blocks[jdx]);
                        //    ushort n = ushort.Parse(blocks[jdx + 2]);
                        //    if (who != player.Uid)
                        //        nfuse += "," + string.Join(",", Util.TakeRange(blocks, jdx, jdx + 3 + n));
                        //    else
                        //    {
                        //        var rests = Util.TakeRange(blocks, jdx + 3, jdx + 3 + n).Where(p => p != card.ToString());
                        //        int rstCnt = rests.Count();
                        //        if (rstCnt > 0)
                        //            mfuse += "," + blocks[jdx] + "," + blocks[jdx + 1] + "," + rstCnt + "," + string.Join(",", rests);
                        //    }
                        //    jdx += (n + 3);
                        //}
                        //if (mfuse.Length > 0)
                        //    XI.InnerGMessage(blocks[0] + mfuse, 121);
                        //if (nfuse.Length > 0)
                        //    XI.InnerGMessage(blocks[0] + nfuse, 120);
                    }
                    else
                        XI.InnerGMessage(fuse, 151);
                }
                else
                    XI.InnerGMessage(fuse, 151);
            }
            else if (type == 1)
            {
                ushort to = ushort.Parse(args);
                Player oy = XI.Board.Garden[to];
                oy.RAMPeoples.Clear();
                XI.InnerGMessage(fuse, 152);
            }
        }
        public string JN60102Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                ushort to;
                if (prev == "")
                    return "";
                else if (ushort.TryParse(prev, out to))
                {
                    string[] blocks = fuse.Split(',');
                    int idx = 1;
                    int jdx = -1, kdx = -1;
                    while (idx < blocks.Length)
                    {
                        ushort n = ushort.Parse(blocks[idx + 2]);
                        if (blocks[idx + 1] == "0")
                        {
                            ushort who = ushort.Parse(blocks[idx]);
                            if (who == player.Uid && n > 0)
                            {
                                jdx = idx + 3; kdx = idx + 3 + n; break;
                            }
                        }
                        idx += (n + 3);
                    }
                    if (jdx >= 0 && kdx > jdx)
                        return "/Q1(p" + string.Join("p", Util.TakeRange(blocks, jdx, kdx)) + ")";
                    else
                        return "";
                } else
                    return "";
            }
            else if (type == 1)
                return "";
            else
                return "";
        }
        public string JN60102Encrypt(string args)
        {
            string[] splits = args.Split(',');
            if (splits.Length >= 2)
                return splits[0] + "," + (splits.Length - 1);
            else
                return args;
        }
        #endregion
        #region XJ502 - TangYurou
        //public bool JN60201BKValid(Player player, int type, string fuse, ushort owner)
        //{
        //    if (XI.Board.Garden[owner].Tux.Count > 0 && player.Team == XI.Board.Garden[owner].Team)
        //    {
        //        string occur = Util.Substring(fuse, 0, fuse.IndexOf(','));
        //        string[] cardList = new string[] { "JP01", "JP02", "JP03", "JP04", "JP05", "JP06",
        //            "ZP01", "ZP02", "ZP03", "ZP04", "TP02", "TP03" };
        //        int[] typeList = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //        for (int i = 0; i < cardList.Length; ++i)
        //        {
        //            Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardList[i]);
        //            int priority;
        //            if (XI.IsOccurIncluded(occur, player.Uid, cardList[i], SKTType.TX, typeList[i], 0, 0, out priority)
        //                && tux != null && tux.Valid(player, typeList[i], fuse))
        //                return true;
        //        }
        //    }
        //    return false;
        //}
        public bool JN60201Valid(Player player, int type, string linkFuse)
        {
            int lfidx = linkFuse.IndexOf(':');
            // linkHeads = { "TP02,0", "TP03,0" };
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            foreach (string linkHead in linkHeads)
            {
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JN60201"))
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
                            if (tux.Code == rName)
                            {
                                var vs = XI.Board.Garden.Values.Where(p => p.IsTared).ToList();
                                foreach (Player py in vs)
                                {
                                    if (py.Uid != player.Uid && tux.Targets[tType] == '#' &&
                                        tux.Bribe(player, tType, fuse) && tux.Valid(py, tType, fuse))
                                        return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        int tType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
                        int tConsType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                        foreach (ushort ut in player.ListOutAllEquips())
                        {
                            Base.Card.TuxEqiup tux = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
                            if (tux != null && tux.Code == rName)
                            {
                                var vs = XI.Board.Garden.Values.Where(p => p.IsTared).ToList();
                                foreach (Player py in vs)
                                {
                                    if (py.Uid != player.Uid)
                                        if (tux.ConsumeValidHolder(player, py, tConsType, tType, fuse))
                                            return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        public string JN60201Input(Player player, int type, string linkFuse, string prev)
        {
            int lfidx = linkFuse.IndexOf(':');
            // linkHeads = { "TP02,0", "TP03,0" };
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            if (prev == "")
            {
                ISet<ushort> usefulPlayer = new HashSet<ushort>();
                foreach (string linkHead in linkHeads)
                {
                    List<string> relateds = XI.Sk03[linkHead].ToList();
                    relateds.Add(linkHead);
                    // relateds = { "TP03,0", "FJ02,0!0" };
                    foreach (string rlink in relateds)
                    {
                        if (rlink.StartsWith("JN60201"))
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
                                if (tux.Code == rName)
                                {
                                    var vs = XI.Board.Garden.Values.Where(p => p.IsTared).ToList();
                                    foreach (Player py in vs)
                                    {
                                        if (py.Uid != player.Uid && tux.Targets[tType] == '#' &&
                                            tux.Bribe(player, tType, fuse) && tux.Valid(py, tType, fuse))
                                            usefulPlayer.Add(py.Uid);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int tType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
                            int tConsType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                            foreach (ushort ut in player.ListOutAllEquips())
                            {
                                Base.Card.TuxEqiup tux = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
                                if (tux != null && tux.Code == rName)
                                {
                                    var vs = XI.Board.Garden.Values.Where(p => p.IsTared).ToList();
                                    foreach (Player py in vs)
                                    {
                                        if (py.Uid != player.Uid)
                                            if (tux.ConsumeValidHolder(player, py, tConsType, tType, fuse))
                                                usefulPlayer.Add(py.Uid);
                                    }
                                }
                            }
                        }
                    }
                }
                if (usefulPlayer.Count > 0)
                    return "/T1(p" + string.Join("p", usefulPlayer) + ")";
                else
                    return "/";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort who = ushort.Parse(prev);
                ISet<ushort> usefulTux = new HashSet<ushort>();
                foreach (string linkHead in linkHeads)
                {
                    List<string> relateds = XI.Sk03[linkHead].ToList();
                    relateds.Add(linkHead);
                    // relateds = { "TP03,0", "FJ02,0!0" };
                    foreach (string rlink in relateds)
                    {
                        if (rlink.StartsWith("JN60201"))
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
                                if (tux.Code == rName)
                                {
                                    Player py = XI.Board.Garden[who];
                                    if (tux.Targets[tType] == '#' &&
                                            tux.Bribe(player, tType, fuse) && tux.Valid(py, tType, fuse))
                                        usefulTux.Add(ut);
                                }
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
                                {
                                    Player py = XI.Board.Garden[who];
                                    if (tux.ConsumeValidHolder(player, py, tConsType, tType, fuse))
                                        usefulTux.Add(ut);
                                }
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

                foreach (string linkHead in linkHeads)
                {
                    List<string> relateds = XI.Sk03[linkHead].ToList();
                    relateds.Add(linkHead);
                    // relateds = { "TP03,0", "FJ02,0!0" };
                    foreach (string rlink in relateds)
                    {
                        if (rlink.StartsWith("JN60201"))
                            continue;
                        int rcmIdx = rlink.IndexOf(',');
                        string rName = Util.Substring(rlink, 0, rcmIdx);
                        int pdIdx = rlink.IndexOf('!', rcmIdx + 1);
                        if (pdIdx < 0) // Not equip special case
                        {
                            int tType = int.Parse(rlink.Substring(rcmIdx + 1));
                            if (tux.Code == rName)
                            {
                                Player py = XI.Board.Garden[who];
                                if (tux.Targets[tType] == '#' &&
                                        tux.Bribe(player, tType, fuse) && tux.Valid(py, tType, fuse))
                                    return tux.InputHolder(player, py, tType, fuse, rest);
                            }
                        }
                        else
                        {
                            int tConsType = int.Parse(Util.Substring(rlink, rcmIdx + 1, pdIdx));
                            int tType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                            Base.Card.TuxEqiup te = tux as Base.Card.TuxEqiup;
                            if (te != null && te.Code == rName)
                            {
                                Player py = XI.Board.Garden[who];
                                return te.ConsumeInputHolder(player, py, tConsType, tType, fuse, rest);
                            }
                        }
                    }
                }
                return "";
            }
        }
        public void JN60201Action(Player player, int type, string linkFuse, string argst)
        {
            int ichicm = argst.IndexOf(',');
            int nicm = argst.IndexOf(',', ichicm + 1);
            ushort to = ushort.Parse(argst.Substring(0, ichicm));
            ushort ut = ushort.Parse(Util.Substring(argst, ichicm + 1, nicm));
            string crest = nicm < 0 ? "" : ("," + argst.Substring(nicm + 1));

            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(ut);
            if (tux.Type == Base.Card.Tux.TuxType.ZP)
                XI.RaiseGMessage("G0CZ,0," + player.Uid);

            int lfidx = linkFuse.IndexOf(':');
            // linkHeads = { "TP02,0", "TP03,0" };
            string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
            string fuse = linkFuse.Substring(lfidx + 1);

            foreach (string linkHead in linkHeads)
            {
                List<string> relateds = XI.Sk03[linkHead].ToList();
                relateds.Add(linkHead);
                // relateds = { "TP03,0", "FJ02,0!0" };
                foreach (string rlink in relateds)
                {
                    if (rlink.StartsWith("JN60201"))
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
                            XI.RaiseGMessage("G0CC," + player.Uid + "," + to + "," + tux.Code
                                    + "," + ut + ";" + inTypeStr + "," + fuse);
                        }
                        return;
                    }
                }
            }
        }
        public void JN60202Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> list = XI.Board.OrderedPlayer();
            foreach (ushort who in list)
            {
                Player py = XI.Board.Garden[who];
                if (who != player.Uid && py.IsAlive && py.Team == player.Team)
                {
                    ushort mon = XI.Board.MonPiles.Dequeue();
                    XI.RaiseGMessage("G2IN,1,1");
                    if (mon == 0)
                        break;
                    if (Base.Card.NMBLib.IsMonster(mon))
                    {
                        XI.RaiseGMessage("G2SW," + py.Uid + ",1," + mon);
                        XI.RaiseGMessage("G0HC,1," + who + ",0,1," + mon);
                    }
                    else
                    {
                        XI.RaiseGMessage("G2SW," + py.Uid + ",1," + mon);
                        XI.RaiseGMessage("G0ON,0,M,1," + mon);
                    }
                    if (XI.Board.MonPiles.Count <= 0)
                        break;
                }
            }
            XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        #endregion XJ502 - TangYurou
        #region XJ503 - LongYou
        public bool JN60301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return XI.Board.Rounder.Gender == 'F' || XI.Board.Supporter.Gender == 'F'
                    || XI.Board.Hinder.Gender == 'F';
            }
            else if (type == 1)
            { // GOIY,0/1,A,S
                string[] blocks = fuse.Split(',');
                ushort who = ushort.Parse(blocks[2]);
                Player py = XI.Board.Garden[who];
                return py.Gender == 'F' && XI.Board.IsAttendWar(py) && XI.Board.InFight;
            }
            else if (type == 3)
            { // GOOY,0/1,A
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; i += 2)
                {
                    ushort who = ushort.Parse(blocks[i + 1]);
                    Player py = XI.Board.Garden[who];
                    if (py.Gender == 'F' && XI.Board.IsAttendWar(py) && XI.Board.InFight)
                        return true;
                }
                return false;
            }
            return false;
        }
        public void JN60301Action(Player player, int type, string fuse, string args)
        {
            if (type == 0)
            {
                int count = XI.Board.Rounder.Gender == 'F' ? 1 : 0;
                count += (XI.Board.Supporter.Gender == 'F' ? 1 : 0);
                count += (XI.Board.Hinder.Gender == 'F' ? 1 : 0);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + count);
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,1");
                //XI.InnerGMessage(fuse, 121);
            }
            else
            {
                XI.RaiseGMessage("G0OA," + player.Uid + ",1,1");
                //XI.InnerGMessage(fuse, 91);
            }
        }
        public bool JN60302Valid(Player player, int type, string fuse)
        {
            if (player.Uid == XI.Board.Rounder.Uid &&
                    XI.Board.Supporter != null && player.RAMUshort == 0)
                return true;
            else
                return false;
            //{
            //    if (type == 0)
            //        return true;
            //    else if (type == 1)
            //    {
            //        string[] blocks = fuse.Split(',');
            //        if (blocks[1] == "0")
            //            return true;
            //    }
            //}
            //return false;
        }
        public void JN60302Action(Player player, int type, string fuse, string args)
        {
            player.RAMUshort = 1;
            XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",3");
        }
        #endregion XJ503 - LongYou
        #region XJ504 - XiaoMan
        public bool JN60401Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0;
        }
        public void JN60401Action(Player player, int type, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            Base.Card.Tux tux = XI.LibTuple.TL.DecodeTux(card);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + tux.Code + "," + card + ";0," + fuse);
        }
        public string JN60401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                var v1 = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Base.Card.Tux.TuxType.JP &&
                    XI.LibTuple.TL.DecodeTux(p).Valid(player, type, fuse)).ToList();
                if (v1.Any())
                    return "/Q1(p" + string.Join("p", v1) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        public bool JN60402Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void JN60402Action(Player player, int type, string fuse, string args)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public bool JN60403Valid(Player player, int type, string fuse)
        {
            bool basecon = player.IsAlive && player.Tux.Count >= 2;
            if (!basecon)
                return false;
            if (!player.IsSKOpt)
            {
                if (type == 6)
                {
                    // G0CD,A,KN,x1,x2;TF
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    return (blocks[1] != player.Uid.ToString()) && (blocks[2] == "JP01" || blocks[2] == "JP06");
                }
                return basecon;
            }
            else
            {
                if (type == 0 || type == 2 || type == 3)
                    return true;
                if (type == 1)
                {// Ask for CC
                    Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                    bool b1 = XI.Board.Rounder.Uid == player.Uid;
                    bool b2 = XI.Board.IsAttendWar(player);
                    bool b3 = true;
                    bool b4 = false;
                    bool b5 = XI.Board.Rounder.Team == player.Team;
                    return mon != null && (mon.IsTuxInvolved(b1, b2, b3, b4, b5) ||
                        mon.IsHarmInvolved(b1, b2, b3, b4, b5));
                }
                else if (type == 4)
                {
                    Base.Card.Evenement eve = XI.LibTuple.EL.DecodeEvenement(XI.Board.Eve);
                    return eve.IsTuxInvolved(true) || eve.IsHarmInvolved();
                }
                else if (type == 5)
                {// Ask for WN/LS
                    Base.Card.Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                    Base.Card.Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                    bool b1 = XI.Board.Rounder.Uid == player.Uid;
                    bool b2 = XI.Board.IsAttendWar(player);
                    bool b3 = false;
                    bool b4 = XI.Board.IsRounderBattleWin();
                    bool b5 = XI.Board.Rounder.Team == player.Team;
                    return (mon1 != null && (mon1.IsTuxInvolved(b1, b2, b3, b4, b5) ||
                        mon1.IsHarmInvolvedTeam(b3, b4, b5))) || (mon2 != null &&
                        (mon2.IsTuxInvolved(b1, b2, b3, b4, b5) || mon2.IsHarmInvolvedTeam(b3, b4, b5)));
                }
                else if (type == 6)
                {
                    // G0CD,A,KN,x1,x2;TF
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    ushort user = ushort.Parse(blocks[1]);
                    return user != 0 && user != player.Uid && XI.Board.Garden[user].Team == player.OppTeam
                        && (blocks[2] == "JP01" || blocks[2] == "JP06");
                }
            }
            return basecon;
        }
        public void JN60403Action(Player player, int type, string fuse, string args)
        {
            string[] blocks = args.Split(',');
            List<ushort> cards = Util.TakeRange(blocks, 0, blocks.Length - 1).Select(p => ushort.Parse(p)).ToList();
            ushort target = ushort.Parse(blocks[blocks.Length - 1]);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", cards));
            XI.RaiseGMessage("G0DH," + target + ",0,1");
            if (type == 6)
                XI.InnerGMessage(fuse, 50);
        }
        public string JN60403Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/Q2(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') == prev.LastIndexOf(','))
                return "/T1" + AAllTareds(player);
            else
                return "";
        }
        #endregion XJ504 - XiaoMan
        #region XJ505 - JiangShili
        public bool JN60501Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player) && XI.Board.Rounder.Uid != player.Uid;
            else if (type == 1)
            {
                bool b1 = XI.Board.RoundIN[0] == 'R' && XI.Board.RoundIN.Substring(2, 2) == "ZD";
                bool b2 = XI.Board.IsAttendWar(player);
                bool b3 = XI.Board.Rounder.Uid != player.Uid;
                if (b1 && b2 && b3)
                {
                    List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                    foreach (Artiad.Cure cure in cures)
                    {
                        if (cure.Who == player.Uid && cure.N > 0)
                            return true;
                    }
                }
            }
            else if (type == 2)
            {
                bool b1 = XI.Board.RoundIN[0] == 'R' && XI.Board.RoundIN.Substring(2, 2) == "ZD";
                bool b2 = XI.Board.IsAttendWar(player);
                bool b3 = XI.Board.Rounder.Uid != player.Uid;
                if (b1 && b2 && b3)
                {
                    List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                    foreach (Artiad.Harm harm in harms)
                    {
                        if (harm.Who == player.Uid && harm.N > 0)
                            return true;
                    }
                }
            }
            else if (type == 3 || type == 4)
                return IsMathISOS("JN60501", player, fuse) && player.HP < player.HPb;
            else if (type == 5)
            {
                string[] g0af = fuse.Split(',');
                if (g0af[1] != "0" && XI.Board.InFight && player.HP < player.HPb)
                    for (int i = 1; i < g0af.Length; i += 2)
                    {
                        ushort ut = ushort.Parse(g0af[i + 1]);
                        if (ut == player.Uid) {
                            ushort delta = ushort.Parse(g0af[i]);
                            if (delta > 4 && player.RAMInt > 0)
                                return true;
                            else if (delta <= 4 && player.RAMInt == 0)
                                return true;
                        }
                    }
                return false;
            }
            return false;
        }
        public void JN60501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                player.RAMInt = player.HPb - player.HP;
                if (player.RAMInt > 0)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + player.RAMInt);
            }
            else if (type == 1 || type == 2)
            {
                int curValue = player.HPb - player.HP;
                int delta = curValue - player.RAMInt;
                player.RAMInt = player.HPb - player.HP;
                if (delta > 0)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + delta);
                else if (delta < 0)
                    XI.RaiseGMessage("G0OP," + player.Team + "," + (-delta));
                //XI.InnerGMessage(fuse, 121);
            }
            else if (type == 3)
            {
                player.RAMInt = player.HPb - player.HP;
                if (player.RAMInt > 0)
                    XI.RaiseGMessage("G0OP," + player.Team + "," + player.RAMInt);
                //XI.InnerGMessage(fuse, 81);
            }
            else if (type == 4)
            {
                if (player.RAMInt > 0)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + player.RAMInt);
                player.RAMInt = 0;
                //XI.InnerGMessage(fuse, 121);
            }
            else if (type == 5)
            {
                if (XI.Board.IsAttendWar(player))
                {
                    player.RAMInt = player.HPb - player.HP;
                    if (player.RAMInt > 0)
                        XI.RaiseGMessage("G0IP," + player.Team + "," + player.RAMInt);
                }
                else
                {
                    if (player.RAMInt > 0)
                        XI.RaiseGMessage("G0OP," + player.Team + "," + player.RAMInt);
                    player.RAMInt = 0;
                }
            }
        }
        public bool JN60502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player);
            else if (type == 1)
                return player.RAMUshort == 1;
            else
                return false;
        }
        public void JN60502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                //if (player.Team == XI.Board.Rounder.Team)
                //    XI.Board.RPool = 10000;
                //else if (player.Team == XI.Board.Rounder.OppTeam)
                //    XI.Board.OPool = 10000;
                XI.RaiseGMessage("G0IA," + player.Uid + ",3");
                player.RAMUshort = 1;
                XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "ZN");
            }
            else if (type == 1)
                XI.RaiseGMessage("G0ZW," + player.Uid);
        }
        #endregion XJ505 - JiangShili
        #region XJ506 - Moyi
        public bool JN60601Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                for (int i = 1; i < blocks.Length; ++i)
                    if (blocks[i] != player.Uid.ToString())
                        return true;
                return false;
            }
            else if (type == 1)
                return player.ROMCards.Count > 0;
            else if (type == 2)
                return XI.Board.InFight && IsMathISOS("JN60601", player, fuse);
            else
                return false;
        }
        public void JN60601Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                List<int> heros = Util.TakeRange(blocks, 1, blocks.Length)
                    .Select(p => ushort.Parse(p)).Where(p => p != player.Uid)
                    .Select(p => XI.Board.Garden[p].SelectHero).ToList();
                string hs = string.Join(",", heros.Select(p => "H" + p));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + heros.Count + "," + hs);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + hs);
                XI.Board.BannedHero.AddRange(heros);
                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + heros.Count);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0IP," + player.Team + "," + player.ROMCards.Count);
            else if (type == 2)
            {
                if (XI.Board.InFight)
                    XI.RaiseGMessage("G0OP," + player.Team + "," + player.ROMCards.Count);
                //XI.InnerGMessage(fuse, 81);
            }
        }
        public bool JN60602Valid(Player player, int type, string fuse)
        {
            return !XI.Board.Garden.Values.Where(p => p.Uid != player.Uid &&
                p.IsAlive && p.Team == player.Team).Any();
        }
        public void JN60602Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",3");
            List<ushort> pets = player.Pets.Where(p => p != 0).ToList();
            foreach (ushort ut in pets)
                XI.RaiseGMessage("G0HL," + player.Uid + "," + ut);
            if (pets.Count > 0)
                XI.RaiseGMessage("G0ON," + player.Uid + ",M," + pets.Count + "," + string.Join(",", pets));
            List<int> souls = player.ROMCards.Where(p => p.StartsWith("H"))
                .Select(p => int.Parse(p.Substring("H".Length))).ToList();
            if (souls.Count > 0)
            {
                string hs = string.Join(",", souls.Select(p => "H" + p));
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + souls.Count + "," + hs);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + hs);
            }
            XI.Board.BannedHero.RemoveAll(p => souls.Contains(p));
            XI.RaiseGMessage("G0OY,0," + player.Uid);
            XI.RaiseGMessage("G0IY,0," + player.Uid + ",10607");
            XI.InnerGMessage(fuse, 341);
        }
        #endregion XJ506 - Moyi
        #region XJ507 - Yanshiqiongbing
        public void JN60701Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
        }
        public void JN60702Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0TT," + player.Uid);
            if (XI.Board.DiceValue >= 5)
                Harm(player, XI.Board.Garden.Values.Where(p => p.Uid != player.Uid && p.IsAlive), 2);
        }
        #endregion XJ507 - Yanshiqiongbing
        #region XJ508 - OuyangHui
        public void JN60801Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int od = player.ROMToken;
                if (player.ROMToken < 4)
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",0,1");
                int delta = (player.ROMToken / 2) - (od / 2);
                if (delta > 0)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0," + delta);
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                int telta = 0;
                for (int idx = 1; idx < blocks.Length; )
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    ushort utype = ushort.Parse(blocks[idx + 1]);
                    if (utype == 0)
                    {
                        ushort delta = ushort.Parse(blocks[idx + 2]);
                        if (who == player.Uid && delta > 0)
                            telta += delta;
                        idx += 3;
                    }
                    else
                        break;
                }
                if (telta > player.ROMToken)
                    telta = player.ROMToken;
                int zelta = (player.ROMToken / 2) - ((player.ROMToken - telta) / 2);
                if (zelta > 0)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0," + zelta);
                //XI.InnerGMessage(fuse, 81);
            }
        }
        public bool JN60801Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player) && player.ROMToken < 4;
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                for (int idx = 1; idx < blocks.Length; )
                {
                    ushort who = ushort.Parse(blocks[idx]);
                    ushort utype = ushort.Parse(blocks[idx + 1]);
                    if (utype == 0)
                    {
                        ushort delta = ushort.Parse(blocks[idx + 2]);
                        if (who == player.Uid && delta > 0)
                            return true;
                    }
                    else
                        break;
                }
                return false;
            }
            else
                return false;
        }
        public bool JN60802Valid(Player player, int type, string fuse)
        {
            if (player.ROMToken > 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (XI.Board.Garden[harm.Who].IsTared && harm.Element != FiveElement.SOL &&
                            harm.Element != FiveElement.LOVE && harm.N > 0)
                        return true;
                }
            }
            return false;
        }
        public void JN60802Action(Player player, int type, string fuse, string argst)
        {
            ushort who = ushort.Parse(argst);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == who && harm.Element != FiveElement.SOL &&
                        harm.Element != FiveElement.LOVE)
                {
                    if (--harm.N <= 0)
                        rvs.Add(harm);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",0,1");
                }
            }
            harms.RemoveAll(p => rvs.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 70);
        }
        public string JN60802Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                ISet<ushort> cands = new HashSet<ushort>();
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (XI.Board.Garden[harm.Who].IsTared && harm.Element != FiveElement.SOL &&
                            harm.Element != FiveElement.LOVE && harm.N > 0)
                        cands.Add(harm.Who);
                }
                return "/T1(p" + string.Join("p", cands) + ")";
            }
            else
                return "";
        }
        public bool JN60803Valid(Player player, int type, string fuse)
        {
            return player.ROMToken >= 2;
        }
        public void JN60803Action(Player player, int type, string fuse, string argst)
        {
            int count = player.ROMToken / 2;
            if (count > 0)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + player.ROMToken);
                Harm(player, XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam), count, FiveElement.THUNDER);
            }
        }
        #endregion XJ508 - OuyangHui

        #region SP101 - LiXiaoyao
        public void JNS0101Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0LH,1," + player.Uid + ",12");
        }
        public bool JNS0101Valid(Player player, int type, string fuse)
        {
            string[] parts = fuse.Split(',');
            if (parts[1] == player.Uid.ToString())
            {
                for (int i = 3; i < parts.Length; ++i)
                    if (parts[i] == "JNS0101")
                        return true;
            }
            return false;
        }
        public void JNS0102Action(Player player, int type, string fuse, string argst)
        {
            if (player.Coss.Count > 0)
            {
                int peek = player.Coss.Peek();
                XI.RaiseGMessage("G0OV," + player.Uid + "," + peek);
                XI.Board.HeroDises.Add(peek);
            }
            string order = XI.AsyncInput(player.Uid, "//", "JNS0102", "0");
            int next = XI.Board.HeroPiles.Dequeue();
            XI.RaiseGMessage("G0IV," + player.Uid + "," + next);
        }
        public void JNS0103Action(Player player, int type, string fuse, string argst)
        {
            Cure(player, player, 2);
        }
        public bool JNS0103Valid(Player player, int type, string fuse)
        {
            string[] parts = fuse.Split(',');
            if (parts[1] == player.Uid.ToString())
            {
                int hroCode = int.Parse(parts[2]);
                Base.Card.Hero hr = XI.LibTuple.HL.InstanceHero(hroCode);
                if (hr != null && hr.Gender == 'F')
                    return true;                    
            }
            return false;
        }
        #endregion SP101 - LiXiaoyao
        #region SP103 - Linyueru
        public void JNS0301Action(Player player, int type, string fuse, string argst)
        {
            ushort[] uts = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + uts[0]);
            XI.RaiseGMessage("G0TT," + uts[1]);
            int va = XI.Board.DiceValue;
            XI.RaiseGMessage("G0TT," + uts[2]);
            int vb = XI.Board.DiceValue;

            if (va < vb)
                player.RAMUshort = 2;
            else if (va > vb)
                player.RAMUshort = 1;
            else
                player.RAMUshort = 0;
            player.RAMPeoples.Add(uts[1]);
            player.RAMPeoples.Add(uts[2]);
            player.RAMString = uts[1] + "," + uts[2];

            XI.RaiseGMessage("G1CK," + player.Uid + ",JNS0302,0");
            if (player.RAMUshort == 2)
                Harm(player, XI.Board.Garden[uts[1]], 1); 
            else
                Harm(player, XI.Board.Garden[uts[2]], 1);
        }
        public bool JNS0301Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                List<Player> pys = XI.Board.Garden.Values.Where(p => p != player && p.IsTared)
                    .Where(p => !player.RAMPeoples.Contains(p.Uid)).ToList();
                return pys.Any(p => p.Gender == 'M') && pys.Any(p => p.Gender == 'F');
            }
            else
                return false;
        }
        public string JNS0301Input(Player player, int type, string fuse, string prev)
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p != player && p.IsTared)
                .Where(p => !player.RAMPeoples.Contains(p.Uid)).ToList();
            List<ushort> uts = pys.Select(p => p.Uid).ToList();
            if (prev == "")
                return "/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
                return "/T1(p" + string.Join("p", uts) + ")";
            else if (prev.Count(p => p == ',') == 1)
            {
                string[] pvs = prev.Split(',');
                char ch = XI.Board.Garden[ushort.Parse(pvs[1])].Gender;
                uts = pys.Where(p => p.Gender != ch).Select(p => p.Uid).ToList();
                return "/T1(p" + string.Join("p", uts) + ")";
            }
            else
                return "";
        }
        public void JNS0302Action(Player player, int type, string fuse, string argst)
        {
            ushort[] uts = player.RAMString.Split(',')
                .Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0TT," + uts[0]);
            int va = XI.Board.DiceValue;
            XI.RaiseGMessage("G0TT," + uts[1]);
            int vb = XI.Board.DiceValue;

            if (va < vb)
                player.RAMUshort = 2;
            else if (va > vb)
                player.RAMUshort = 1;
            else
                player.RAMUshort = 0;
        }
        public bool JNS0302Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            return blocks[1] == player.Uid.ToString() &&
                blocks[2] == "JNS0302" && blocks[3] == type.ToString();
        }
        #endregion SP103 - Linyueru
        #region SP301 - Jingtian
        public void JNS0801Action(Player player, int type, string fuse, string argst)
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
                        if (me == player.Uid)
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
                XI.InnerGMessage(string.Join(",", args), 91);
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
                        if (me == player.Uid && n > 0)
                        {
                            List<ushort> cards = Util.TakeRange(args, i + 3, i + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            string cardstr = XI.AsyncInput(player.Uid,
                                "#弃置的,Q1(p" + string.Join("p", cards) + ")", "JNS0801", "0");
                            ushort card = ushort.Parse(cardstr);
                            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                            cards.Remove(card);
                            if (cards.Count > 0)
                                g1di += "," + me + "," + lose + "," + cards.Count + "," + string.Join(",", cards);
                        }
                        else
                            g1di += "," + string.Join(",", Util.TakeRange(args, i, i + 3 + n));
                    } else
                        g1di += "," + string.Join(",", Util.TakeRange(args, i, i + 3 + n));
                    i += (3 + n);
                }
                if (g1di.Length > 0)
                    XI.InnerGMessage("G1DI" + g1di, 171);
            }
        }
        public bool JNS0801Valid(Player player, int type, string fuse)
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
                        if (me == player.Uid)
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
                        if (me == player.Uid && n > 0)
                            return true;
                    }
                    i += (3 + n);
                }
                return false;
            }
            else
                return false;
        }
        public void JNS0802Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort who = ushort.Parse(argst.Substring(0, idx));
                ushort card = ushort.Parse(argst.Substring(idx + 1));
                XI.Board.ProtectedTux.Add(card);
                XI.RaiseGMessage("G2FU,2," + who + "," + card);
                player.RAMUshort = card;
                //XI.InnerGMessage(fuse, 91);
            }
            else if (type == 1)
            {
                if (player.RAMUshort != 0)
                    XI.Board.ProtectedTux.Remove(player.RAMUshort);
                player.RAMUshort = 0;
                //XI.InnerGMessage(fuse, 141);
            }
        }
        public bool JNS0802Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                // G0CD,A,KN,x1,x2;TF
                bool basecon = true;
                basecon = basecon && (player.GetEquipCount() > 0 || player.Tux.Count > 0);
                basecon = basecon && XI.Board.Garden.Values.Where(p => p.Uid != player.Uid &&
                        p.IsTared && p.Team == player.Team).Any();
                if (basecon)
                {
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    return blocks[2] == "JP01" || blocks[2] == "JP06";
                }
                else
                    return false;
            }
            else if (type == 1)
            {
                // G0CE,A,0,KN,y,z;TF
                string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                return blocks[2] == "0" && (blocks[3] == "JP01" || blocks[3] == "JP06");
            }
            else return false;
        }
        public string JNS0802Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0)
            {
                if (prev == "")
                {
                    List<ushort> vals = new List<ushort>();
                    if (player.HasAnyCards())
                        vals.Add(player.Uid);
                    vals.AddRange(XI.Board.Garden.Values.Where(p => p.Uid != player.Uid &&
                        p.IsTared && p.Team == player.Team && p.HasAnyEquips()).Select(p => p.Uid));
                    if (vals.Count > 0)
                        return "/T1(p" + string.Join("p", vals) + ")";
                    else
                        return "/";
                }
                else if (prev.IndexOf(',') < 0)
                {
                    ushort who = ushort.Parse(prev);
                    if (who == player.Uid)
                        return "/Q1(p" + string.Join("p", player.ListOutAllCards()) + ")";
                    else
                        return "/C1(p" + string.Join("p", XI.Board.Garden[who].ListOutAllEquips()) + ")";
                }
                else return "";
            }
            else
                return "";
        }
        #endregion SP301 - Jingtian
        #region SP306 - Chonglou
        public bool JNS0901Valid(Player player, int type, string fuse)
        {
            bool b1 = (XI.Board.Rounder == player || XI.Board.Supporter == player);
            if (type == 0)
                return b1;
            else if (type == 1)
            {
                string[] g0hzs = fuse.Split(',');
                bool b2 = g0hzs[2] != "0";
                return b1 && b2 && XI.Board.Monster2 != 0;
            }
            return false;
        }
        public void JNS0901Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + ",2");
                if (XI.Board.Hinder.IsTared)
                    XI.RaiseGMessage("G0OX," + XI.Board.Hinder.Uid + ",1,1");
            }
            else if (type == 1)
                XI.RaiseGMessage("G0OB," + XI.Board.Monster2 + ",2");
        }
        public bool JNS0902Valid(Player player, int type, string fuse)
        {
            // G0IS/G0OS
            string[] parts = fuse.Split(',');
            if (parts[1] == player.Uid.ToString())
            {
                for (int i = 3; i < parts.Length; ++i)
                    if (parts[i] == "JNS0902")
                        return true;
            }
            return false;
        }
        public void JNS0902Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                player.IsTared = false;
            else if (type == 1)
                player.IsTared = true;
        }
        #endregion SP306 - Chonglou
        #region SP501 - Jiangyunfan
        public bool JNS0401Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player);
            else if (type == 1)
                return XI.Board.InFight && XI.Board.IsAttendWar(player) && IsMathISOS("JNS0401", player, fuse);
            else
                return false;
        }
        public void JNS0401Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IP," + player.Team + ",1");
            else if (type == 1)
                XI.RaiseGMessage("G0OP," + player.Team + ",1");
        }
        public bool JNS0402Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count >= 2;
        }
        public void JNS0402Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            XI.RaiseGMessage("G0JM,R" + XI.Board.Rounder.Uid + "VT");
        }
        public string JNS0402Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                if (player.Tux.Count >= 2)
                    return "/Q2(p" + string.Join("p", player.Tux) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        #endregion SP501 - Jiangyunfan
        #region SP502 - TangYurou
        public bool JNS0501Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player) && XI.Board.Garden.Values
					.Any(p => p.IsTared);
            else if (type == 1)
            {
                int idxc = fuse.IndexOf(',');
                ushort ut = ushort.Parse(fuse.Substring(idxc + 1));
                return player.ROMPlayerTar.Count > 0 && ut == player.ROMPlayerTar[0];
            }
            return false;
        }
        public void JNS0501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort target = ushort.Parse(argst);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + target);
                XI.Board.Garden[target].ZPDisabled = true;
            }
            else if (type == 1)
            {
                if (player.ROMPlayerTar.Count > 0)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.ROMPlayerTar[0]);
            }
        }
        public string JNS0501Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared)
                    .Select(p => p.Uid)) + ")";
            else
                return "";
        }        
        public bool JNS0502Valid(Player player, int type, string fuse)
        {
            if (!XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Uid != player.Uid && p.GetPetCount() > 0).Any())
                return false;
            string[] blocks = fuse.Split(',');
            for (int i = 1; i < blocks.Length; ++i)
                if (blocks[i] == player.Uid.ToString())
                    return true;
            return false;
        }
        public void JNS0502Action(Player player, int type, string fuse, string argst)
        {
            int idxc = argst.IndexOf(',');
            ushort t1 = ushort.Parse(argst.Substring(0, idxc));
            ushort t2 = ushort.Parse(argst.Substring(idxc + 1));
            XI.RaiseGMessage("G0HC,2," + t1 + "," + t2);
        }
        public string JNS0502Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared
                    && p.Uid != player.Uid && p.GetPetCount() > 0).Select(p => p.Uid)) + ")";
            else if (prev.IndexOf(',') < 0)
            {
                ushort t1 = ushort.Parse(prev);
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared
                    && p.Uid != player.Uid && p.Uid != t1).Select(p => p.Uid)) + ")";
            }
            else
                return "";
        }
        #endregion SP502 - Tangyurou
        #region SP503 - Longyou
        public void JNS0601Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort card = ushort.Parse(argst.Substring(0, idx));
            ushort target = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
            XI.RaiseGMessage("G2FU,0,0,0," + string.Join(",", XI.Board.Garden[target].Tux));
        }
        public bool JNS0601Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count > 0 && XI.Board.Garden.Values.Where(
				p => p.IsTared && p.Tux.Count > 0).Any();
        }
        public string JNS0601Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#弃置的,/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#展示手牌的,/T1(p" + string.Join("p", XI.Board.Garden.Values
                    .Where(p => p.IsTared && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public void JNS0602Action(Player player, int type, string fuse, string argst)
        {
            int val = int.Parse(argst);
            if (val > 0 && val <= player.STR)
            {
                XI.RaiseGMessage("G0OA," + player.Uid + ",1," + val);
                XI.RaiseGMessage("G0IX," + player.Uid + ",1," + val);
            }
        }
        public bool JNS0602Valid(Player player, int type, string fuse)
        {
            if (player.IsSKOpt)
                return player.STR > 0 && XI.Board.IsAttendWar(player);
            else
                return player.STR > 0;
        }
        public string JNS0602Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#转化为命中的,/D1" + ((player.STR == 1) ? "" : ("~" + player.STR));
            else return "";
        }
        #endregion SP503 - Longyou
        #region SP504 - Xiaoman
        public void JNS0701Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            XI.RaiseGMessage("G0DH," + ut + ",0,1");
        }
        public bool JNS0701Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public string JNS0701Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
					p => p.IsTared).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public void JNS0702Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort who = ushort.Parse(argst.Substring(0, idx));
            ushort pet = ushort.Parse(argst.Substring(idx + 1));
            XI.RaiseGMessage("G0HL," + who + "," + pet);
            XI.RaiseGMessage("G0ON," + who + ",M,1," + pet);
            //XI.InnerGMessage("G1WJ,0", 91);
        }
        public bool JNS0702Valid(Player player, int type, string fuse)
        {
            if (player.IsAlive)
            {
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.Team == player.OppTeam)
                    {
                        foreach (ushort pt in py.Pets)
                        {
                            Base.Card.Monster mon = XI.LibTuple.ML.Decode(pt);
                            if (mon != null && mon.Level != Base.Card.Monster.ClLevel.BOSS)
                                return true;
                        }
                    }
                }
            }
            return false;
        }
        public string JNS0702Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> cands = new List<ushort>();
                foreach (Player py in XI.Board.Garden.Values)
                {
                    if (py.Team == player.OppTeam)
                    {
                        foreach (ushort pt in py.Pets)
                        {
                            Base.Card.Monster mon = XI.LibTuple.ML.Decode(pt);
                            if (mon != null && mon.Level != Base.Card.Monster.ClLevel.BOSS)
                            {
                                cands.Add(py.Uid);
                                break;
                            }
                        }
                    }
                }
                if (cands.Count > 0)
                    return "/T1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else if (prev.IndexOf(',') < 0)
            {
                List<ushort> cands = new List<ushort>();
                ushort who = ushort.Parse(prev);
                Player py = XI.Board.Garden[who];
                foreach (ushort pt in py.Pets)
                {
                    Base.Card.Monster mon = XI.LibTuple.ML.Decode(pt);
                    if (mon != null && mon.Level != Base.Card.Monster.ClLevel.BOSS)
                        cands.Add(pt);
                }
                if (cands.Count > 0)
                    return "/M1(p" + string.Join("p", cands) + ")";
                else
                    return "/";
            }
            else
                return "";
        }
        #endregion SP504 - Xiaoman

        #region Skill Util

        private static bool Equal(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;
            else if (obj1 == null && obj2 != null)
                return false;
            else if (obj1 != null && obj2 == null)
                return false;
            else
                return obj1.Equals(obj2);
        }

        private bool IsMathISOS(string skillName, Player player, string fuse)
        {
            string[] parts = fuse.Split(',');
            if (parts[1] == player.Uid.ToString())
            {
                for (int i = 3; i < parts.Length; ++i)
                    if (parts[i] == skillName)
                        return true;
            }
            return false;
        }
        private void Harm(Player src, Player py, int n, FiveElement five = FiveElement.A, int mask = 0)
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                new Artiad.Harm(py.Uid, src == null ? 0 : src.Uid, five, n, mask)));
        }

        private void Harm(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A, int mask = 0)
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(invs.Select(p =>
                new Artiad.Harm(p.Uid, src.Uid, five, n, mask))));
        }

        private void Harm(Player src, List<Player> invs,
            List<int> ns, List<int> mask = null, FiveElement five = FiveElement.A)
        {
            int sz = invs.Count;
            XI.RaiseGMessage(Artiad.Harm.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Artiad.Harm(invs[p].Uid, src.Uid, five, ns[p], mask == null ? 0 : mask[p]))));
        }

        private void Cure(Player src, Player py, int n, FiveElement five = FiveElement.A)
        {
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                new Artiad.Cure(py.Uid, src.Uid, five, n)));
        }

        private void Cure(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A)
        {
            XI.RaiseGMessage(Artiad.Cure.ToMessage(invs.Select(p => new Artiad.Cure(
                p.Uid, src.Uid, five, n))));
        }

        private void Cure(Player src, List<Player> invs,
            List<int> ns, FiveElement five = FiveElement.A)
        {
            int sz = invs.Count;
            XI.RaiseGMessage(Artiad.Cure.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Artiad.Cure(invs[p].Uid, src.Uid, five, ns[p]))));
        }

        private string AOthers(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Uid != py.Uid).Select(p => p.Uid)) + ")";
        }
        private string AOthersTared(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Uid != py.Uid).Select(p => p.Uid)) + ")";
        }
        private string AAlls(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive).Select(p => p.Uid)) + ")";
        }
        private string AAllTareds(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared).Select(p => p.Uid)) + ")";
        }
        private string ATeammates(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == py.Team).Select(p => p.Uid)) + ")";
        }
        private string ATeammatesTared(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Team == py.Team).Select(p => p.Uid)) + ")";
        }
        private string AEnemy(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == py.OppTeam).Select(p => p.Uid)) + ")";
        }

        #endregion Skill Util
    }
}