using PSD.Base;
using PSD.Base.Card;
using PSD.Base.Utils;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.JNS
{
    public partial class SkillCottage : JNSBase
    {
        public SkillCottage(XI xi, Base.VW.IVI vi) : base(xi, vi) { }
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
                    sk.Action += new Skill.ActionDelegate(delegate (Player player, int type, string fuse, string argst)
                    {
                        methodAction.Invoke(sc, new object[] { player, type, fuse, argst });
                    });
                var methodValid = sc.GetType().GetMethod(skCode + "Valid");
                if (methodValid != null)
                    sk.Valid += new Skill.ValidDelegate(delegate (Player player, int type, string fuse)
                    {
                        return (bool)methodValid.Invoke(sc, new object[] { player, type, fuse });
                    });
                var methodInput = sc.GetType().GetMethod(skCode + "Input");
                if (methodInput != null)
                    sk.Input += new Skill.InputDelegate(delegate (Player player, int type, string fuse, string prev)
                    {
                        return (string)methodInput.Invoke(sc, new object[] { player, type, fuse, prev });
                    });
                var methodEncrypt = sc.GetType().GetMethod(skCode + "Encrypt");
                if (methodEncrypt != null)
                    sk.Encrypt += new Skill.EncryptDelegate(delegate (string argst)
                    {
                        return (string)methodEncrypt.Invoke(sc, new object[] { argst });
                    });
                if (sk.IsBK)
                {
                    Bless bs = (Bless)sk;
                    var methodBKValid = sc.GetType().GetMethod(skCode + "BKValid");
                    if (methodBKValid != null)
                        bs.BKValid += new Bless.BKValidDelegate(delegate (Player player, int type, string fuse, ushort owner)
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
            TargetPlayer(XI.Board.Rounder.Uid, player.Uid);
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
            TargetPlayer(player.Uid, XI.Board.Hinder.Uid);
            XI.AsyncInput(player.Uid, "#获得的,T1(p" + XI.Board.Hinder.Uid + ")", "JN10102", "0");
            string c0 = Algo.RepeatString("p0", XI.Board.Garden[XI.Board.Hinder.Uid].Tux.Count);
            XI.AsyncInput(player.Uid, "#获得的,C1(" + c0 + ")", "JN10102", "0");
            XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + XI.Board.Hinder.Uid + ",2,1");
        }
        #endregion XJ101 - LiXiaoyao
        #region XJ102 - ZhaoLing'er
        public bool JN10201Valid(Player player, int type, string fuse)
        {
            return IsMathISOS("JN10201", player, fuse);
        }
        public void JN10201Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage(new Artiad.EqSlotVariation()
            {
                Who = player.Uid,
                Slot = Artiad.ClothingHelper.SlotType.WQ,
                Increase = (type == 0) ? true : false
            }.ToMessage());
        }
        public bool JN10202Valid(Player player, int type, string fuse)
        {
            if (player.IsAlive && XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Select(p => p.GetPetCount()).Sum() >= 3)
            {
                if (type == 0)
                    return true;
                else if (type == 1)
                    return IsMathISOS("JN10202", player, fuse);
            }
            return false;
        }
        public void JN10202Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0OS," + player.Uid + ",0,JN10202");
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10103");
            XI.RaiseGMessage("G0IS," + player.Uid + ",0,JN10302,JN10303");
        }
        #endregion XJ102 - ZhaoLing'er
        #region XJ103 - Mengshe
        public bool JN10302Valid(Player player, int type, string fuse) // Others are the same as ZhaoLing'er
        {
            if (player.IsAlive && XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Select(p => p.GetPetCount()).Sum() < 3)
            {
                if (type == 0)
                    return true;
                else if (type == 1)
                    return IsMathISOS("JN10302", player, fuse);
            }
            return false;
        }
        public void JN10302Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0OS," + player.Uid + ",2,JN10302,JN10303");
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10102");
            XI.RaiseGMessage("G0IS," + player.Uid + ",2,JN10202");
        }
        public bool JN10303Valid(Player player, int type, string fuse)
        {
            if (type == 0) return true;
            else if ((type == 1 || type == 2) && XI.Board.PoolEnabled)
                return IsMathISOS("JN10303", player, fuse);
            else
                return false;
        }
        public void JN10303Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1)
                XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JN10303,2");
            else if (type == 2)
                XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JN10303,0");
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
                return (player.Tux.Count > 1 ? ("/+Q1~" + player.Tux.Count) : "/+Q1") + "(p" + string.Join("p",
                    player.Tux) + "),/T1" + FormatPlayers(p => p != player && p.IsTared && p.Team == player.Team);
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
            XI.RaiseGMessage("G0DH," + AffichePlayers(p => p.IsAlive && p.Team == player.Team, p => p.Uid + ",0,1"));
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
        public bool JN10602Valid(Player player, int type, string fuse) // Set as Lock'in
        {
            // Ushort: 0->normal pass, 1->second battle, 2->give up second battle
            if (type == 0)
                return XI.Board.Battler != null && player.RFM.GetInt("DuoFight") == 0 && XI.Board.MonPiles.Count > 0;
            else if (type == 1)
            {
                if (player.RFM.GetInt("DuoFight") == 2)
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
            else if (type == 2)
                return player.RFM.GetInt("DuoFight") == 1;
            else
                return false;
        }
        public void JN10602Action(Player player, int type, string fuse, string args)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G1SG,0");
                string yes = XI.AsyncInput(player.Uid,
                    "#是否进行第二次战斗？##不进行##进行,Y2", "JN10602", "0");
                if (yes.Equals("2"))
                {
                    player.RFM.Set("DuoFight", 1);
                    XI.RaiseGMessage("G17F,U," + player.Uid);
                    XI.Board.CleanBattler();
                    XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + player.Uid + "ZW" }.ToMessage());
                }
                else
                    player.RFM.Set("DuoFight", 2);
            }
            else if (type == 1)
            {
                string[] g0ht = fuse.Split(',');
                for (int i = 1; i < g0ht.Length; i += 2)
                {
                    ushort ut = ushort.Parse(g0ht[i]);
                    int n = int.Parse(g0ht[i + 1]);
                    if (ut == player.Uid && n > 0)
                        g0ht[i + 1] = (n + 1).ToString();
                }
                XI.InnerGMessage(string.Join(",", g0ht), 41);
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
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
                XI.RaiseGMessage("G2YS,T," + player.Uid + ",P," + (ushort)player.Team);
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
            return !player.RFM.GetBool("DuoFight") && XI.Board.MonPiles.Count > 0;
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
                player.RFM.Set("DuoFight", true);
                XI.RaiseGMessage(new Artiad.Goto() { Terminal = "R" + player.Uid + "ZM" }.ToMessage());
            }
        }
        public bool JN20202Valid(Player player, int type, string fuse)
        {
            Tux tp01 = XI.LibTuple.TL.EncodeTuxCode("TP01");
            return player.Tux.Count > 0 && tp01 != null && tp01.Valid(player, type, fuse);
        }
        public void JN20202Action(Player player, int type, string fuse, string argst)
        {
            ushort card = ushort.Parse(argst);
            VI.Cout(0, "苏媚使用「拒绝」.");
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",TP01," + card + ";0," + fuse);
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

            if (type == 0 || (type == 1 && XI.Board.PoolEnabled))
            {
                if (!player.RAM.GetBool("STR+3") && notin)
                    return true;
                else if (player.RAM.GetBool("STR+3") && !notin)
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
                    player.RAM.Set("STR+3", true);
                    TargetPlayer(player.Uid, XI.Board.Rounder.Uid);
                    XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,3");
                }
                else if (player.Team == XI.Board.Rounder.OppTeam)
                {
                    if (NMBLib.IsMonster(XI.Board.Monster1))
                    {
                        VI.Cout(0, "沈欺霜触发「仙霞五奇」，怪物灵力+3.");
                        player.RAM.Set("STR+3", true);
                        XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,1");
                        XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + ",3");
                    }
                }
            }
            else if (type == 1)
            {
                if (player.Team == XI.Board.Rounder.Team)
                {
                    if (!player.RAM.GetBool("STR+3"))
                    {
                        player.RAM.Set("STR+3", true);
                        TargetPlayer(player.Uid, XI.Board.Rounder.Uid);
                        XI.RaiseGMessage("G0IA," + XI.Board.Rounder.Uid + ",1,3");
                    }
                    else
                    {
                        player.RAM.Set("STR+3", false);
                        TargetPlayer(player.Uid, XI.Board.Rounder.Uid);
                        XI.RaiseGMessage("G0OA," + XI.Board.Rounder.Uid + ",1,3");
                    }
                }
                else if (player.Team == XI.Board.Rounder.OppTeam)
                {
                    if (NMBLib.IsMonster(XI.Board.Monster1))
                    {
                        if (!player.RAM.GetBool("STR+3"))
                        {
                            player.RAM.Set("STR+3", true);
                            XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,1");
                            XI.RaiseGMessage("G0IB," + XI.Board.Monster1 + ",3");
                        }
                        else
                        {
                            player.RAM.Set("STR+3", false);
                            XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,1");
                            XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + ",3");
                        }
                    }
                }
            }
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
            player.RAM.GetOrSetUshortArray("harmed").Add(who);
        }
        public string JN20601Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> v1 = XI.Board.Garden.Values.Where(p => p.IsTared && p.Gender == 'F' &&
                    !player.RAM.GetOrSetUshortArray("harmed").Contains(p.Uid)).Select(p => p.Uid).ToList();
                return "/T1(p" + string.Join("p", v1) + ")";
            }
            else
                return "";
        }
        public bool JN20601Valid(Player player, int type, string fuse)
        {
            return player.HP >= 2 && XI.Board.Garden.Values.Any(p => p.IsTared &&
                p.Gender == 'F' && !player.RAM.GetOrSetUshortArray("harmed").Contains(p.Uid));
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
                XI.InnerGMessage("G0ZW" + zw, -10);
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
            return player.Tux.Count > 0 && Artiad.Harm.Parse(fuse).Any(p => XI.Board.Garden[p.Who].IsAlive &&
                p.N > 0 && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask) && (!player.IsSKOpt || p.Who != player.Uid));
        }
        public void JN30201Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            Harm(player, Artiad.Harm.Parse(fuse).Where(p => XI.Board.Garden[p.Who].IsAlive && p.N > 0 &&
                !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask)).Select(p => XI.Board.Garden[p.Who]).Distinct(), 1);
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
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
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
            return player.Tux.Count > 0;
        }
        public bool JN30203Valid(Player player, int type, string fuse)
        {
            return player.HP >= 2 && XI.Board.IsAttendWar(player);
        }
        public void JN30203Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "唐雪见发动「好胜」.");
            Harm(player, player, 2, FiveElement.A, (long)HPEvoMask.TUX_INAVO);
            if (player.IsAlive)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
        }
        #endregion XJ302 - TangXuejian
        #region XJ303 - LongKui Blue
        public void JN30301Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0OY,1," + player.Uid);
            XI.RaiseGMessage("G0OS," + player.Uid + ",0,JN30301,JN30302,JN30303");
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10304");
            XI.RaiseGMessage("G0IS," + player.Uid + ",2,JN30401,JN30402,JN30403");
        }
        public bool JN30302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                string jpname = player.RAM.GetString("JPName");
                if (jpname != null && !player.RAM.GetBool("Melt"))
                {
                    Tux tux = XI.LibTuple.TL.EncodeTuxCode(jpname);
                    if (tux != null && tux.Valid(player, 0, fuse))
                        return true;
                }
                return false;
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                ushort ut = ushort.Parse(blocks[1]);
                if (!player.RAM.GetBool("Melt") && ut == player.Uid)
                {
                    string cardCode = blocks[4];
                    Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardCode);
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
                {
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0," +
                        player.Uid + "," + player.RAM.GetString("JPName") + ",0;0," + fuse);
                }
                player.RAM.Set("Melt", true);
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                string jpname = blocks[4];
                player.RAM.Set("JPName", jpname);
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
                XI.RaiseGMessage("G0ZI," + player.Uid + "," + card);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
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
            XI.RaiseGMessage("G0OS," + player.Uid + ",0,JN30401,JN30402,JN30403");
            XI.RaiseGMessage("G0IY,1," + player.Uid + ",10303");
            XI.RaiseGMessage("G0IS," + player.Uid + ",2,JN30301,JN30302,JN30303");
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
                XI.RaiseGMessage("G0ZI," + player.Uid + "," + card);
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
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            System.Func<ushort, bool> teammate = p => XI.Board.Garden[p].Team == player.Team;
            return teammate(opt.Farmer) && (opt.Farmland == 0 || !teammate(opt.Farmland));
        }
        public void JN30501Action(Player player, int type, string fuse, string argst)
        {
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            for (int i = 0; i < opt.Pets.Length; ++i)
            {
                string input = XI.AsyncInput(player.Uid,
                    "#获得2张补牌,T1" + ATeammatesTared(player), "JN30501", "0");
                ushort who = ushort.Parse(input);
                if (who != 0)
                    XI.RaiseGMessage("G0DH," + who + ",0,2");
            }
        }
        public bool JN30502Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            ushort x = ushort.Parse(blocks[1]);
            return player.Pets.Contains(x);
        }
        public void JN30502Action(Player player, int type, string fuse, string argst)
        {
            string[] g0wb = fuse.Split(',');
            ushort x = ushort.Parse(g0wb[1]);
            if (player.Pets.Contains(x))
                XI.RaiseGMessage("G0IB," + x + ",3");
        }
        #endregion XJ305 - Zixuan
        #region XJ306 - ChongLou
        public bool JN30601Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid) &&
                player.Tux.Count >= player.RAM.GetInt("DuelCount") + 1;
        }
        public string JN30601Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> all = XI.Board.Garden.Values.Where(p => p.IsTared
                    && p.Uid != player.Uid).Select(p => p.Uid).ToList();
                int dlc = player.RAM.GetInt("DuelCount") + 1;
                if (all.Count >= 2)
                    return "/Q" + dlc + "(p" + string.Join("p", player.Tux) + ")"
                        + ",/T1~2(p" + string.Join("p", all) + ")";
                else
                    return "/Q" + dlc + "(p" + string.Join("p", player.Tux) + ")"
                        + ",/T1(p" + string.Join("p", all) + ")";
            }
            else
                return "";
        }
        public void JN30601Action(Player player, int type, string fuse, string args)
        {
            string[] blocks = args.Split(',');
            List<ushort> restCards = Algo.TakeRange(blocks, 0, player.RAM.GetInt("DuelCount") + 1)
                .Select(p => ushort.Parse(p)).ToList();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", restCards));
            List<ushort> tos = Algo.TakeRange(blocks, player.RAM.GetInt("DuelCount") + 1, blocks.Length)
                .Select(p => ushort.Parse(p)).ToList();
            TargetPlayer(player.Uid, tos);
            player.RAM.Set("DuelCount", player.RAM.GetInt("DuelCount") + 1);
            long maskDuel = HPEvoMask.ALIVE.Set(HPEvoMask.TUX_INAVO.Set((long)HPEvoMask.RSV_DUEL));
            foreach (ushort to in tos)
            {
                XI.RaiseGMessage("G0TT," + player.Uid);
                int myDice = XI.Board.DiceValue;
                XI.RaiseGMessage("G0TT," + to);
                int toDice = XI.Board.DiceValue;
                if (myDice > toDice)
                    Harm(player, XI.Board.Garden[to], 3, FiveElement.A, maskDuel);
                else if (myDice < toDice)
                    Harm(player, player, 3, FiveElement.A, maskDuel);
                else
                    Harm(player, new Player[] { XI.Board.Garden[to], player }, 2, FiveElement.A, maskDuel);
            }
        }
        public bool JN30602Valid(Player player, int type, string fuse) // No Action called, handled in ALIVE flag
        {
            return Artiad.Harm.Parse(fuse).Any(p => HPEvoMask.RSV_DUEL.IsSet(p.Mask) && p.N > 0 &&
                XI.Board.Garden[p.Who].HP >= 2 && XI.Board.Garden[p.Who].HP - p.N < 1);
        }
        public bool JN30603Valid(Player player, int type, string fuse)
        {
            return XI.Board.Rounder.Team == player.OppTeam && XI.Board.IsAttendWar(player);
        }
        public void JN30603Action(Player player, int type, string fuse, string argst)
        {
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
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            return opt.Farmer == player.Uid && opt.Trophy && opt.Pets.Select(p => XI.LibTuple.ML.Decode(p).Element
                .Elem2Index()).Any(p => XI.Board.Garden.Values.Any(q => q.IsAlive && q.Team == q.OppTeam && q.Pets[p] != 0));
        }
        public void JN40102Action(Player player, int type, string fuse, string args)
        {
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            foreach (ushort pt in opt.Pets)
            {
                Monster monster = XI.LibTuple.ML.Decode(pt);
                int elem = monster.Element.Elem2Index();
                if (!XI.Board.Garden.Values.Any(q => q.IsAlive && q.Team == q.OppTeam && q.Pets[elem] != 0))
                    continue;
                List<ushort> mons = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam && p.Pets[elem] != 0).Select(p => p.Pets[elem]).ToList();
                string input = XI.AsyncInput(player.Uid, "#弃置的,/M1(p" +
                    string.Join("p", mons) + ")", "JN40102", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    ushort pet = ushort.Parse(input);
                    ushort who = Artiad.ContentRule.GetPetOwnership(pet, XI);
                    TargetPlayer(player.Uid, who);
                    XI.RaiseGMessage(new Artiad.LosePet()
                    {
                        Owner = who,
                        SinglePet = pet
                    }.ToMessage());
                }
            }
        }
        #endregion X3W01 - Nan'gongHuang
        #region X3W02 - WenHui
        public bool JN40201Valid(Player player, int type, string fuse)
        {
            if (type == 0 || (type == 1 && XI.Board.Rounder.Uid == player.Uid))
            { // Trigger Support Status in self round'
                if (!player.RAM.GetBool("STR+3") && XI.Board.SupportSucc)
                    return true;
                else if (player.RAM.GetBool("STR+3") && !XI.Board.SupportSucc)
                    return true;
            }
            return false;
        }
        public void JN40201Action(Player player, int type, string fuse, string argst)
        {
            if (!player.RAM.GetBool("STR+3"))
            {
                player.RAM.Set("STR+3", true);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
            }
            else
            {
                player.RAM.Set("STR+3", false);
                XI.RaiseGMessage("G0OA," + player.Uid + ",1,3");
            }
        }
        public bool JN40202Valid(Player player, int type, string fuse)
        {
            return !XI.Board.IsBattleWin && XI.Board.Garden.Values
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
                return XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).Any();
            else
                return false;
        }
        public void JN40301Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "星璇使用「烹饪」.");
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",TP02," + argst + ";" + type + "," + fuse);
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
            string getGroup = string.Join(",", XI.Board.Garden.Values.Where(
                p => p.Uid != player.Uid && p.IsAlive && p.Team == player.Team && p.Tux.Count > 0).Select(
                p => p.Uid + ",2," + p.Tux.Count));
            if (!string.IsNullOrEmpty(getGroup))
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + getGroup);
            do
            {
                string targ = "/T1" + FormatPlayers(p => p.IsTared && p.Uid != player.Uid && p.Team == player.Team);
                string carg = "(p" + string.Join("p", player.Tux) + ")";
                carg = (player.Tux.Count > 1) ? ("/+Q1~" + player.Tux.Count + carg) : ("/+Q1" + carg);
                string select = XI.AsyncInput(player.Uid, carg + "," + targ, "JN40302", "0");
                if (select == "0" || select == "/0" || select == "")
                    break; // On Finish of Catching
                else if (select.Contains(VI.CinSentinel))
                    continue;
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
                            + cards.Count() + "," + string.Join(",", cards));
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
            ushort[] cards = args.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage(new Artiad.EquipExCards()
            {
                Who = player.Uid, Source = player.Uid, Cards = cards
            }.ToMessage());
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
                    // G0CD,A,0,KN,x1,x2;TF
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    return (blocks[1] != player.Uid.ToString()) &&
                        (blocks[3] == "JP01" || blocks[3] == "JP06");
                }
                else
                    return true;
            }
            else
            {
                if (type == 0 || type == 2)
                    return true;
                if (type == 1)
                {// Ask for CC
                    Base.Card.Monster mon = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                    return mon != null && mon.IsTuxInvolved(XI.Board.Rounder.Uid == player.Uid,
                        XI.Board.IsAttendWar(player), true, false, XI.Board.Rounder.Team == player.Team);
                }
                else if (type == 3) // G0QR
                    return fuse.Substring("G0QR,".Length) == player.Uid.ToString();
                else if (type == 4)
                {
                    Base.Card.Evenement eve = XI.LibTuple.EL.DecodeEvenement(XI.Board.Eve);
                    return eve.IsTuxInvolved(true);
                }
                else if (type == 5)
                {// Ask for WN/LS
                    Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                    Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                    bool b1 = XI.Board.Rounder.Uid == player.Uid;
                    bool b2 = XI.Board.IsAttendWar(player);
                    bool b3 = false;
                    bool b4 = XI.Board.IsBattleWin;
                    bool b5 = XI.Board.Rounder.Team == player.Team;
                    return (mon1 != null && mon1.IsTuxInvolved(b1, b2, b3, b4, b5)) ||
                        (mon2 != null && mon2.IsTuxInvolved(b1, b2, b3, b4, b5));
                }
                else if (type == 6)
                {
                    // G0CD,A,0,KN,x1,x2;TF
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    ushort user = ushort.Parse(blocks[1]);
                    return user != 0 && user != player.Uid && XI.Board.Garden[user].Team == player.OppTeam
                        && (blocks[3] == "JP01" || blocks[3] == "JP06");
                }
            }
            return basecon;
        }
        public void JN40403Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
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
                //XI.InnerGMessage(fuse, 91);
            }
        }
        public bool JN40403Valid(Player player, int type, string fuse)
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
        #endregion X3W04 - WangPengxu
        #region XJ401 - YunTianhe
        public bool JN50101Valid(Player player, int type, string fuse)
        {
            if (type == 0 || type == 1 || type == 4)
            {
                return XI.Board.IsAttendWar(player) && XI.Board.Battler != null &&
                    player.DEX - XI.Board.Battler.AGL >= 4 && !player.RAM.GetBool("STR+2");
            }
            else if (type == 2 || type == 3)
            {
                return XI.Board.IsAttendWar(player) && XI.Board.Battler != null &&
                    player.DEX - XI.Board.Battler.AGL < 4 && player.RAM.GetBool("STR+2");
            }
            else return false;
        }
        public void JN50101Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1 || type == 4)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
                player.RAM.Set("STR+2", true);
            }
            else if (type == 2 || type == 3)
            {
                XI.RaiseGMessage("G0OA," + player.Uid + ",1,2");
                player.RAM.Set("STR+2", false);
            }
        }
        public void JN50102Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IP," + player.Team + ",8");
                player.ROM.Set("REC", 1); // 1: mark as triggered
            }
            else if (type == 1)
            {
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.DEX,
                    Who = player.Uid,
                    NewValue = 0
                }.ToMessage());
                player.ROM.Set("REC", 2); // 2: Blind
            }
        }
        public bool JN50102Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.ROM.GetInt("REC") == 0;
            else if (type == 1)
                return player.ROM.GetInt("REC") == 1;
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
                ushort db = ushort.Parse(args[1]);
                string cardCode = XI.LibTuple.TL.EncodeTuxDbSerial(db).Code;
                XI.RaiseGMessage("G0CC," + player.Uid + ",0," +
                    player.Uid + "," + cardCode + "," + card + ";0," + fuse);
            }
        }
        public string JN50201Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                ushort jp01 = XI.LibTuple.TL.EncodeTuxCode("JP01").DBSerial;
                ushort jp06 = XI.LibTuple.TL.EncodeTuxCode("JP06").DBSerial;
                bool jp01Valid = XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.Tux.Count > 0);
                return "/Q1(p" + string.Join("p", player.Tux) + "),#请选择『搜囊探宝』执行项,/G1(p"
                    + jp06 + (jp01Valid ? ("p" + jp01) : "") + ")";
            }
            else
                return "";
        }
        public void JN50202Action(Player player, int type, string fuse, string argst)
        {
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
                string targ = "/T1" + FormatPlayers(p => p.IsTared && p.Uid != player.Uid);
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
                        XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1," +
                            cards.Count() + "," + string.Join(",", cards));
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
            if (type == 0)
            {
                Artiad.JoinPetEffects jpes = Artiad.JoinPetEffects.Parse(fuse);
                jpes.List.ForEach(p => XI.RaiseGMessage("G0IA," + p.Owner + ",0," + p.Pets.Length));
            }
            else if (type == 1)
            {
                Artiad.CollapsePetEffects cpes = Artiad.CollapsePetEffects.Parse(fuse);
                cpes.List.ForEach(p => XI.RaiseGMessage("G0OA," + p.Owner + ",0," + p.Pets.Length));
            }
            else if (type == 2 || type == 3)
            {
                string title = (type == 2) ? "G0IA" : "G0OA";
                foreach (Player py in XI.Board.Garden.Values)
                    if (py.IsAlive && py.Team == player.Team && !py.PetDisabled)
                    {
                        int count = py.GetActivePetCount(XI.Board);
                        if (count > 0)
                            XI.RaiseGMessage(title + "," + py.Uid + ",0," + count);
                    }
            }
        }
        public bool JN50301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return Artiad.JoinPetEffects.Parse(fuse).List.Any(p =>
                    XI.Board.Garden[p.Owner].Team == player.Team);
            }
            else if (type == 1)
            {
                return Artiad.CollapsePetEffects.Parse(fuse).List.Any(p =>
                   XI.Board.Garden[p.Owner].Team == player.Team);
            }
            else if (type == 2 || type == 3)
                return IsMathISOS("JN50301", player, fuse) && XI.Board.Garden.Values.Any(p =>
                    p.Team == player.Team && p.GetActivePetCount(XI.Board) > 0);
            else return false;
        }
        public void JN50302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.DEX,
                    Who = player.Uid,
                    NewValue = 5
                }.ToMessage());
                List<string> otherSkill = player.Skills.Where(p => p != "JN50302").ToList();
                if (otherSkill.Count > 0)
                    XI.RaiseGMessage("G0OS," + player.Uid + ",0," + string.Join(",", otherSkill));
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
                    XI.InnerGMessage(string.Join(",", Algo.TakeRange(blocks, 0, blocks.Length - 1)), 351);
                }
            }
            else if (type == 1)
            {
                ushort ut = (ushort)(fuse[1] - '0');
                Player rp = XI.Board.Garden[ut];
                if (rp.Team == player.Team)
                    XI.Board.PosSupporters.Add("T" + player.Uid);
                else
                    XI.Board.PosHinders.Add("T" + player.Uid);
            }
            else if (type == 2)
                XI.RaiseGMessage("G0OY,2," + player.Uid);
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
            else
                return false;
        }
        #endregion XJ403 - LiuMengli
        #region XJ404 - MurongZiying
        public bool JN50401Valid(Player player, int type, string fuse)
        {
            return player.ListOutAllEquips().Count > 0 && XI.Board.Garden.Values.Any(p => p.IsTared &&
                p.Uid != player.Uid && !player.RAM.GetOrSetUshortArray("Present").Contains(p.Uid));
        }
        public void JN50401Action(Player player, int type, string fuse, string argst)
        {
            int idx = argst.IndexOf(',');
            ushort card = ushort.Parse(argst.Substring(0, idx));
            ushort who = ushort.Parse(argst.Substring(idx + 1));
            if (player.Fakeq.ContainsKey(card))
                XI.RaiseGMessage(new Artiad.EquipFakeq()
                {
                    Who = who, Source = player.Uid, Card = card, CardAs = player.Fakeq[card]
                }.ToMessage());
            else
                XI.RaiseGMessage(new Artiad.EquipStandard()
                {
                    Who = who, Source = player.Uid, SingleCard = card, Coach = player.Uid
                }.ToMessage());
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
            player.RAM.GetOrSetUshortArray("Present").Add(who);
        }
        public string JN50401Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                string c = "/Q1(p" + string.Join("p", player.ListOutAllEquips()) + ")";
                string t = "/T1" + FormatPlayers(p => p.IsTared && p.Uid != player.Uid &&
                    !player.RAM.GetOrSetUshortArray("Present").Contains(p.Uid));
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
            return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && p.N > 0 &&
                (p.Element == FiveElement.AQUA || p.Element == FiveElement.AGNI));
        }
        public void JN50501Action(Player player, int type, string fuse, string argst)
        {
            // G0OH,A,Src,p,n,...
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            harms.RemoveAll(p => p.Who == player.Uid &&
                !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) &&
                (p.Element == FiveElement.AQUA || p.Element == FiveElement.AGNI));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -39);
        }
        public void JN50502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string target = XI.AsyncInput(player.Uid,
                    "#『结拜』的,/T1" + AOthersTared(player), "JN50502", "0");
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ushort.Parse(target));
                TargetPlayer(player.Uid, player.SingleTokenTar);
                XI.SendOutUAMessage(player.Uid, "JN50502," + target, "0");
            }
            else if (type == 1)
            {
                TargetPlayer(player.Uid, player.SingleTokenTar);
                VI.Cout(0, "玄霄触发「结拜」，对玩家{0}的命中+1.", XI.DisplayPlayer(player.SingleTokenTar));
                XI.RaiseGMessage("G0IX," + player.Uid + ",1,1");
            }
            else if (type == 2)
            {
                if (player.SingleTokenTar == XI.Board.Rounder.Uid)
                {
                    XI.RaiseGMessage("G0OX," + player.Uid + ",1,1");
                    VI.Cout(0, "玄霄失去「结拜」目标.");
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                }
                //XI.InnerGMessage(fuse, 111);
            }
        }
        public bool JN50502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JN50502", player, fuse);
            else if (type == 1)
                return player.TokenTars.Count > 0 && player.SingleTokenTar == XI.Board.Rounder.Uid;
            else if (type == 2)
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
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," + tux.Code + "," + card + ";0," + fuse);
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
                Player oy = XI.Board.Garden[owner];
                if (!oy.IsAlive || owner == player.Uid)
                    return false;
                // G1DI,A,Ty=0,n,m,x
                string[] blocks = fuse.Split(',');
                int idx = 1;
                while (idx < blocks.Length)
                {
                    ushort n = ushort.Parse(blocks[idx + 2]);
                    if (blocks[idx + 1] == "0")
                    {
                        ushort who = ushort.Parse(blocks[idx]);
                        Player py = XI.Board.Garden[who];
                        if (who == player.Uid && py.Team == oy.Team && who != owner &&
                                n > 0 && !oy.RAM.GetOrSetUshortArray("fortress").Contains(who))
                            return true;
                    }
                    idx += (n + 4);
                }
            }
            else if (type == 1)
                return XI.Board.Garden[owner].RAM.GetOrSetUshortArray("fortress").Contains(player.Uid);
            return false;
        }
        public void JN60102Action(Player player, int type, string fuse, string args)
        {
            if (type == 0)
            {
                bool isSet = false;
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
                        oy.RAM.GetOrSetUshortArray("fortress").Add(player.Uid);
                        string[] blocks = fuse.Split(',');
                        string nfuse = "";
                        int jdx = 1;
                        while (jdx < blocks.Length)
                        {
                            ushort who = ushort.Parse(blocks[jdx]);
                            ushort n = ushort.Parse(blocks[jdx + 2]);
                            if (who != player.Uid)
                                nfuse += "," + string.Join(",", Algo.TakeRange(blocks, jdx, jdx + 4 + n));
                            else
                            {
                                List<ushort> rests = Algo.TakeRange(blocks, jdx + 4, jdx + 4 + n)
                                    .Select(p => ushort.Parse(p)).Where(p => p != card).ToList();
                                if (rests.Count > 0)
                                    nfuse += "," + blocks[jdx] + "," + blocks[jdx + 1] + "," + rests.Count +
                                        "," + rests.Count + "," + string.Join(",", rests);
                            }
                            jdx += (n + 4);
                        }
                        if (nfuse.Length > 0)
                        {
                            isSet = true;
                            XI.InnerGMessage(blocks[0] + nfuse, 120);
                        }
                    }
                }
                if (!isSet)
                    XI.InnerGMessage(fuse, 121);
            }
            else if (type == 1)
            {
                ushort to = ushort.Parse(args);
                Player oy = XI.Board.Garden[to];
                oy.RAM.Set("fortress", null);
                XI.InnerGMessage(fuse, 122);
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
                                jdx = idx + 4; kdx = idx + 4 + n; break;
                            }
                        }
                        idx += (n + 4);
                    }
                    if (jdx >= 0 && kdx > jdx)
                        return "/Q1(p" + string.Join("p", Algo.TakeRange(blocks, jdx, kdx)) + ")";
                    else
                        return "";
                }
                else
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
        //        string occur = Algo.Substring(fuse, 0, fuse.IndexOf(','));
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
                    string rName = Algo.Substring(rlink, 0, rcmIdx);
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
                        int tType = int.Parse(Algo.Substring(rlink, rcmIdx + 1, pdIdx));
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
                                        if (tux.ConsumeValidHolder(player, py, tConsType, tType, linkFuse))
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
                        string rName = Algo.Substring(rlink, 0, rcmIdx);
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
                            int tType = int.Parse(Algo.Substring(rlink, rcmIdx + 1, pdIdx));
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
                                            if (tux.ConsumeValidHolder(player, py, tConsType, tType, linkFuse))
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
                        string rName = Algo.Substring(rlink, 0, rcmIdx);
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
                            int tConsType = int.Parse(Algo.Substring(rlink, rcmIdx + 1, pdIdx));
                            int tType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                            foreach (ushort ut in player.ListOutAllEquips())
                            {
                                Base.Card.TuxEqiup tux = XI.LibTuple.TL.DecodeTux(ut) as Base.Card.TuxEqiup;
                                if (tux != null && tux.Code == rName)
                                {
                                    Player py = XI.Board.Garden[who];
                                    if (tux.ConsumeValidHolder(player, py, tConsType, tType, linkFuse))
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
                ushort who = ushort.Parse(Algo.Substring(prev, 0, ichicm));
                ushort ut = ushort.Parse(Algo.Substring(prev, ichicm + 1, nicm));
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
                        string rName = Algo.Substring(rlink, 0, rcmIdx);
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
                            int tConsType = int.Parse(Algo.Substring(rlink, rcmIdx + 1, pdIdx));
                            int tType = (pdIdx < 0) ? -1 : int.Parse(rlink.Substring(pdIdx + 1));
                            Base.Card.TuxEqiup te = tux as Base.Card.TuxEqiup;
                            if (te != null && te.Code == rName)
                            {
                                Player py = XI.Board.Garden[who];
                                return te.ConsumeInputHolder(player, py, tConsType, tType, linkFuse, rest);
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
            ushort ut = ushort.Parse(Algo.Substring(argst, ichicm + 1, nicm));
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
                    XI.RaiseGMessage(new Artiad.AnnouceCard()
                    {
                        Action = Artiad.AnnouceCard.Type.DISCOVERY,
                        Officer = py.Uid,
                        Genre = Card.Genre.NMB,
                        SingleCard = mon
                    }.ToMessage());
                    if (NMBLib.IsMonster(mon))
                    {
                        XI.RaiseGMessage(new Artiad.HarvestPet()
                        {
                            Farmer = who,
                            SinglePet = mon,
                            Plow = false,
                        }.ToMessage());
                    }
                    else
                        XI.RaiseGMessage("G0ON,0,M,1," + mon);
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
            if (XI.Board.PoolEnabled)
            {
                int count = XI.Board.Garden.Values.Count(p => p.IsAlive &&
                    p.Gender == 'F' && XI.Board.IsAttendWar(p));
                if (type == 0)
                    return count > 0;
                else
                    return count != player.RAM.GetInt("Girlfriend");
            }
            return false;
        }
        public void JN60301Action(Player player, int type, string fuse, string args)
        {
            int count = XI.Board.Garden.Values.Count(p => p.IsAlive &&
                p.Gender == 'F' && XI.Board.IsAttendWar(p));
            if (type == 0)
            {
                player.RAM.Set("Girlfriend", count);
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + count);
            }
            else
            {
                int delta = count - player.RAM.GetInt("Girlfriend");
                player.RAM.Set("Girlfriend", count);
                if (delta > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + delta);
                else if (delta < 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",1," + (-delta));
            }
        }
        public bool JN60302Valid(Player player, int type, string fuse)
        {
            return player.Uid == XI.Board.Rounder.Uid && XI.Board.Supporter.Uid != 0 && !player.RAM.GetBool("Hit");
        }
        public void JN60302Action(Player player, int type, string fuse, string args)
        {
            player.RAM.Set("Hit", true);
            TargetPlayer(player.Uid, XI.Board.Supporter.Uid);
            XI.RaiseGMessage("G0IX," + XI.Board.Supporter.Uid + ",2");
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
            Tux tux = XI.LibTuple.TL.DecodeTux(card);
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," +
                player.Uid + "," + tux.Code + "," + card + ";0," + fuse);
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
                if (type == 0 || type == 2)
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
                else if (type == 3) // G0QR
                    return fuse.Substring("G0QR,".Length) == player.Uid.ToString();
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
                    bool b4 = XI.Board.IsBattleWin;
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
                else if (type == 7) // G0ZH,0
                    return true;
            }
            return basecon;
        }
        public void JN60403Action(Player player, int type, string fuse, string args)
        {
            string[] blocks = args.Split(',');
            List<ushort> cards = Algo.TakeRange(blocks, 0, blocks.Length - 1).Select(p => ushort.Parse(p)).ToList();
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
            bool self = (XI.Board.Rounder.Uid == player.Uid);
            if (type == 0)
                return XI.Board.IsAttendWar(player) && !self && player.HP < player.HPb;
            else if (type == 1)
            {
                bool b1 = XI.Board.RoundIN[0] == 'R' && XI.Board.RoundIN.Substring(2, 2) == "ZD";
                bool b2 = XI.Board.IsAttendWar(player);
                if (b1 && b2 && !self)
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
                if (b1 && b2 && !self)
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
            {
                return IsMathISOS("JN60501", player, fuse) &&
                     XI.Board.IsAttendWar(player) && !self && player.HP < player.HPb;
            }
            else if (type == 5 && XI.Board.PoolEnabled && player.HP < player.HPb)
            {
                string[] g0fi = fuse.Split(',');
                if (g0fi[1] == "O" || g0fi[1] == "U")
                    return false;
                int result = 0;
                for (int i = 1; i < g0fi.Length; i += 3)
                {
                    char ch = g0fi[i][0];
                    ushort old = ushort.Parse(g0fi[i + 1]);
                    ushort to = ushort.Parse(g0fi[i + 2]);
                    if (ch == 'S' || ch == 'H')
                    {
                        if (old == player.Uid)
                            --result;
                        if (to == player.Uid)
                            ++result;
                    }
                    else if (ch == 'T' || ch == 'W')
                    {
                        result = 0; break;
                    }
                }
                return result != 0;
            }
            return false;
        }
        public void JN60501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0 || type == 1 || type == 2 || type == 4)
            {
                int point = player.HPb - player.HP;
                XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JN60501," + point);
            }
            else if (type == 3)
                XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JN60501,0");
            else if (type == 5)
            {
                string[] g0fi = fuse.Split(',');
                if (g0fi[1] == "O" || g0fi[1] == "U")
                    return;
                int result = 0;
                for (int i = 1; i < g0fi.Length; i += 3)
                {
                    char ch = g0fi[i][0];
                    ushort old = ushort.Parse(g0fi[i + 1]);
                    ushort to = ushort.Parse(g0fi[i + 2]);
                    if (ch == 'S' || ch == 'H')
                    {
                        if (old == player.Uid)
                            --result;
                        if (to == player.Uid)
                            ++result;
                    }
                    else if (ch == 'T' || ch == 'W')
                    {
                        result = 0; break;
                    }
                }
                if (result < 0)
                    XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JN60501,0");
                else if (result > 0)
                {
                    int point = player.HPb - player.HP;
                    XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JN60501," + point);
                }
            }
        }
        public bool JN60502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player);
            else if (type == 1)
                return player.ROM.GetBool("Sacrified");
            else
                return false;
        }
        public void JN60502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",2");
                player.ROM.Set("Sacrified", true);
                XI.RaiseGMessage(new Artiad.Goto()
                {
                    CrossStage = false,
                    Terminal = "R" + XI.Board.Rounder.Uid + "ZN"
                }.ToMessage());
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
                return player.TokenExcl.Count > 0;
            else if (type == 2)
            {
                string[] g0oj = fuse.Split(',');
                if (g0oj[1] == player.Uid.ToString() && g0oj[2] == "1")
                    return ushort.Parse(g0oj[3]) > 0;
                else
                    return false;
            }
            else
                return false;
        }
        public void JN60601Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] blocks = fuse.Split(',');
                List<int> heros = Algo.TakeRange(blocks, 1, blocks.Length)
                    .Select(p => ushort.Parse(p)).Where(p => p != player.Uid)
                    .Select(p => XI.Board.Garden[p].SelectHero).ToList();
                string hs = string.Join(",", heros.Select(p => "H" + p));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + heros.Count + "," + hs);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + hs);
                XI.Board.BannedHero.AddRange(heros);
                if (XI.Board.PoolEnabled)
                    XI.RaiseGMessage("G0IP," + player.Team + "," + heros.Count);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0IP," + player.Team + "," + player.TokenExcl.Count);
            else if (type == 2)
            {
                string[] g0oj = fuse.Split(',');
                int n = int.Parse(g0oj[3]);
                List<int> souls = Algo.TakeRange(g0oj, 4, 4 + n).Select(
                    p => int.Parse(p.Substring("H".Length))).ToList();
                XI.Board.BannedHero.RemoveAll(p => souls.Contains(p));
                XI.Board.HeroDises.AddRange(souls);
                if (XI.Board.PoolEnabled)
                    XI.RaiseGMessage("G0OP," + player.Team + "," + n);
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
            ushort[] pets = player.Pets.Where(p => p != 0).ToArray();
            if (pets.Length > 0)
                XI.RaiseGMessage(new Artiad.LosePet() { Owner = player.Uid, Pets = pets }.ToMessage());
            List<int> souls = player.TokenExcl.Where(p => p.StartsWith("H"))
                .Select(p => int.Parse(p.Substring("H".Length))).ToList();
            if (souls.Count > 0)
            {
                string hs = string.Join(",", souls.Select(p => "H" + p));
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + souls.Count + "," + hs);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + hs);
            }
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
                int od = player.TokenCount;
                if (player.TokenCount < 4)
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",0,1");
                int delta = (player.TokenCount / 2) - (od / 2);
                if (delta > 0)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0," + delta);
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                int telta = 0;
                for (int idx = 1; idx < blocks.Length;)
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
                if (telta > player.TokenCount)
                    telta = player.TokenCount;
                int zelta = (player.TokenCount / 2) - ((player.TokenCount - telta) / 2);
                if (zelta > 0)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0," + zelta);
                //XI.InnerGMessage(fuse, 81);
            }
        }
        public bool JN60801Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.IsAttendWar(player) && player.TokenCount < 4;
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                for (int idx = 1; idx < blocks.Length;)
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
            return player.TokenCount > 0 && Artiad.Harm.Parse(fuse).Any(p => XI.Board.Garden[p.Who].IsTared &&
                p.N > 0 && !HPEvoMask.TERMIN_AT.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
        }
        public void JN60802Action(Player player, int type, string fuse, string argst)
        {
            string[] parts = argst.Split(',');
            ushort who = ushort.Parse(parts[0]); int point = int.Parse(parts[1]);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == who && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask) &&
                    !HPEvoMask.DECR_INVAO.IsSet(harm.Mask))
                {
                    TargetPlayer(player.Uid, who);
                    if ((harm.N -= point) <= 0)
                        rvs.Add(harm);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + point);
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
                return "#「雷屏」作用,/T1(p" + string.Join("p", Artiad.Harm.Parse(fuse).Where(p => p.N > 0 && 
                    XI.Board.Garden[p.Who].IsTared && !HPEvoMask.TERMIN_AT.IsSet(p.Mask) &&
                    !HPEvoMask.DECR_INVAO.IsSet(p.Mask)).Select(p => p.Who).Distinct()) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort tar = ushort.Parse(prev);
                int point = 0;
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == tar) { point = harm.N; break; }
                }
                if (point > player.TokenCount)
                    point = player.TokenCount;
                return point == 0 ? "" : ("#屏蔽伤害,/D1" + ((point == 1) ? "" : ("~" + point)));
            }
            else
                return "";
        }
        public bool JN60803Valid(Player player, int type, string fuse)
        {
            return player.TokenCount >= 2;
        }
        public void JN60803Action(Player player, int type, string fuse, string argst)
        {
            int count = player.TokenCount / 2;
            if (count > 0)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + player.TokenCount);
                Harm(player, XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam), count, FiveElement.THUNDER);
            }
        }
        #endregion XJ508 - OuyangHui

        #region SP101 - LiXiaoyao
        public void JNS0101Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                player.ROM.GetOrSetDiva("@15001").Set("incr", 12 - player.HPb);
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.HP,
                    Who = player.Uid,
                    NewValue = 12
                }.ToMessage());
            }
            else if (type == 1)
            {
                int incr = player.ROM.GetOrSetDiva("@15001").GetInt("incr");
                if (incr != 0)
                {
                    XI.RaiseGMessage(new Artiad.InnateChange()
                    {
                        Item = Artiad.InnateChange.Prop.HP,
                        Who = player.Uid,
                        NewValue = 12 - incr
                    }.ToMessage());
                    player.ROM.GetDiva("@15001").Set("incr", null);
                }
            }
        }
        public bool JNS0101Valid(Player player, int type, string fuse)
        {
            return IsMathISOS("JNS0101", player, fuse);
        }
        public bool JNS0102Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1 || type == 2)
                return IsMathISOS("JNS0102", player, fuse);
            else
                return false;
        }
        public void JNS0102Action(Player player, int type, string fuse, string argst)
        {
            Diva diva = player.ROM.GetDiva("15001");
            if (player.Coss.Count > 0 && diva != null)
            {
                int hero = player.ROM.GetOrSetDiva("@15001").GetInt("hero");
                if (hero != 0)
                {
                    Hero heroin = XI.LibTuple.HL.InstanceHero(hero);
                    if (heroin != null)
                        Artiad.Procedure.DecrGuestPlayer(player, heroin, XI, 0x1);
                    int peek = player.Coss.Peek();
                    XI.RaiseGMessage("G0OV," + player.Uid + "," + peek);
                    Hero peekHero = XI.LibTuple.HL.InstanceHero(peek);
                    if (peekHero != null)
                        XI.Board.HeroDises.Add(peek);
                    diva.Set("hero", null);
                }
            }
            if (type == 0 || type == 1)
            {
                XI.AsyncInput(player.Uid, "//", "JNS0102", "0");
                int next = XI.Board.HeroPiles.Dequeue();
                XI.RaiseGMessage("G0IV," + player.Uid + "," + next);
                player.ROM.GetOrSetDiva("@15001").Set("hero", next);
                Hero nextHero = XI.LibTuple.HL.InstanceHero(next);
                if (nextHero != null)
                    Artiad.Procedure.IncrGuestPlayer(player, nextHero, XI, 0x1);
            }
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
        #region SP102 - ZhaoLing'er
        public bool JNS0201Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.Tux.Count > 0)
            {
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != player.Uid &&
                    player.RFM.GetOrSetDiva("@15002").GetOrSetUshortArray("Dreamed").Contains(p.Uid));
            }
            else if (type == 1 || type == 2)
            {
                bool b = player.Coss.Count > 0 && player.RFM.GetOrSetDiva("@15002").GetInt("Hero") != 0;
                if (type == 2)
                    b &= IsMathISOS("JNS0201", player, fuse);
                return b;
            }
            else
                return false;
        }
        public void JNS0201Action(Player player, int type, string fuse, string argst)
        {
            //Diva diva = player.RFM.GetDiva("15002");
            //List<ushort> dreams = diva != null ? diva.GetUshortArray("Dreamed") : new List<ushort>();
            if (player.Coss.Count > 0)
            {
                int hero = player.RFM.GetDiva("@15002").GetInt("Hero");
                if (hero != 0)
                {
                    Hero heroin = XI.LibTuple.HL.InstanceHero(hero);
                    if (heroin != null)
                        Artiad.Procedure.DecrGuestPlayer(player, heroin, XI, 0x3);
                    int peek = player.Coss.Peek();
                    XI.RaiseGMessage("G0OV," + player.Uid + "," + peek);
                }
            }
            if (type == 0)
            {
                int idx = argst.IndexOf(',');
                ushort discardUt = ushort.Parse(argst.Substring(0, idx));
                ushort who = ushort.Parse(argst.Substring(idx + 1));
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + discardUt);
                Player py = XI.Board.Garden[who];
                XI.RaiseGMessage("G0IV," + player.Uid + "," + py.SelectHero);
                player.RFM.GetOrSetDiva("@15002").Set("Hero", py.SelectHero);
                player.RFM.GetOrSetDiva("@15002").GetOrSetUshortArray("Dreamed").Add(who);
                Hero nextHero = XI.LibTuple.HL.InstanceHero(py.SelectHero);
                if (nextHero != null)
                    Artiad.Procedure.IncrGuestPlayer(player, nextHero, XI, 0x3);
            }
        }
        public string JNS0201Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<ushort> obs = XI.Board.Garden.Values.Where(p => p.IsTared && p.Uid != player.Uid)
                    .Select(p => p.Uid).Except(player.RFM.GetOrSetDiva("@15002")
                    .GetOrSetUshortArray("Dreamed")).ToList();
                if (obs.Count > 0)
                {
                    return "/Q1(p" + string.Join("p", player.Tux) + "),#获取技能,/T1(p" +
                        string.Join("p", obs) + ")";
                }
                else
                    return "/";
            }
            else return "";
        }
        public void JNS0202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort card = ushort.Parse(argst);
                XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JP03," + card + ";0," + fuse);
            }
            else if (type == 1)
            {
                foreach (Tux tux in XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.JP))
                    player.AddToPrice(tux.Code, false, "JNS0202", '=', 1);
            }
            else if (type == 2)
            {
                foreach (Tux tux in XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.JP))
                    player.RemoveFromPrice(tux.Code, false, "JNS0202");
            }
        }
        public bool JNS0202Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                if (player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP))
                {
                    Tux copiee = XI.LibTuple.TL.EncodeTuxCode("JP03");
                    return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
                }
                else return false;
            }
            else if (type == 1 || type == 2)
                return IsMathISOS("JNS0202", player, fuse);
            else
                return false;
        }
        public string JNS0202Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "" && type == 0)
            {
                return "/Q1(p" + string.Join("p", player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP)) + ")";
            }
            else return "";
        }
        #endregion SP102 - ZhaoLing'er
        #region SP103 - Linyueru
        public void JNS0301Action(Player player, int type, string fuse, string argst)
        { // Harm Value is Combined' from Homeland and Taiwan version, (1 + 3) / 2 = 2
            ushort[] uts = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + uts[0]);
            TargetPlayer(player.Uid, uts);
            XI.RaiseGMessage("G0TT," + uts[1]);
            int va = XI.Board.DiceValue;
            XI.RaiseGMessage("G0TT," + uts[2]);
            int vb = XI.Board.DiceValue;

            if (va < vb)
                player.RAM.Set("AWin", 2);
            else if (va > vb)
                player.RAM.Set("AWin", 1);
            else
                player.RAM.Set("AWin", 0);
            player.RAM.GetOrSetUshortArray("Duel").AddRange(new ushort[] { uts[1], uts[2] });
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNS0302,0");
            if (player.RAM.GetInt("AWin") == 2)
                Harm(player, XI.Board.Garden[uts[1]], 2);
            else if (player.RAM.GetInt("AWin") == 1)
                Harm(player, XI.Board.Garden[uts[2]], 2);
        }
        public bool JNS0301Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsTared &&
                    !player.RAM.GetOrSetUshortArray("Duel").Contains(p.Uid)).ToList();
                return pys.Any(p => p.Gender == 'M') && pys.Any(p => p.Gender == 'F');
            }
            else
                return false;
        }
        public string JNS0301Input(Player player, int type, string fuse, string prev)
        {
            List<Player> pys = XI.Board.Garden.Values.Where(p => p.IsTared &&
                !player.RAM.GetOrSetUshortArray("Duel").Contains(p.Uid)).ToList();
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
            List<ushort> uts = player.RAM.GetOrSetUshortArray("Duel");
            ushort a = uts[uts.Count - 2], b = uts[uts.Count - 1];
            XI.RaiseGMessage("G0TT," + a);
            int va = XI.Board.DiceValue;
            XI.RaiseGMessage("G0TT," + b);
            int vb = XI.Board.DiceValue;

            if (va < vb)
                player.RAM.Set("AWin", 2);
            else if (va > vb)
                player.RAM.Set("AWin", 1);
            else
                player.RAM.Set("AWin", 0);
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
                for (int i = 1; i < args.Length;)
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
                for (int i = 1; i < args.Length;)
                {
                    ushort me = ushort.Parse(args[i]);
                    ushort lose = ushort.Parse(args[i + 1]);
                    int n = int.Parse(args[i + 2]);
                    if (lose == 0) // Get Card
                    {
                        if (me == player.Uid && n > 0)
                        {
                            List<ushort> cards = Algo.TakeRange(args, i + 4, i + 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            string cardstr = XI.AsyncInput(player.Uid,
                                "#弃置的,Q1(p" + string.Join("p", cards) + ")", "JNS0801", "0");
                            ushort card = ushort.Parse(cardstr);
                            XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                            cards.Remove(card);
                            int cn = cards.Count;
                            if (cn > 0)
                                g1di += "," + me + "," + lose + "," + cn + "," + cn + "," + string.Join(",", cards);
                        }
                        else
                            g1di += "," + string.Join(",", Algo.TakeRange(args, i, i + 4 + n));
                    }
                    else
                        g1di += "," + string.Join(",", Algo.TakeRange(args, i, i + 4 + n));
                    i += (4 + n);
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
                for (int i = 1; i < args.Length;)
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
                for (int i = 1; i < args.Length;)
                {
                    ushort me = ushort.Parse(args[i]);
                    ushort lose = ushort.Parse(args[i + 1]);
                    int n = int.Parse(args[i + 2]);
                    if (lose == 0) // Get Card
                    {
                        if (me == player.Uid && n > 0)
                            return true;
                    }
                    i += (4 + n);
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
                XI.RaiseGMessage(new Artiad.AnnouceCard()
                {
                    Action = Artiad.AnnouceCard.Type.SHOW,
                    Officer = who,
                    Genre = Card.Genre.Five,
                    SingleCard = card
                }.ToMessage());
                player.RFM.Set("protected", card);
            }
            else if (type == 1)
            {
                XI.Board.ProtectedTux.Remove(player.RFM.GetUshort("protected"));
                player.RFM.Set("protected", null);
            }
        }
        public bool JNS0802Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                // G0CD,A,T,KN,x1,x2;TF
                bool basecon = player.Tux.Count > 0 || XI.Board.Garden.Values.Where(p => p.IsTared &&
                    p.Team == player.Team && p.HasAnyEquips()).Any();
                if (basecon)
                {
                    string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                    return blocks[3] == "JP01" || blocks[3] == "JP06";
                }
                else
                    return false;
            }
            else if (type == 1)
            {
                // G0CE,A,T,0,KN,y,z;TF
                string[] blocks = fuse.Substring(0, fuse.IndexOf(';')).Split(',');
                Player py = XI.Board.Garden[ushort.Parse(blocks[1])];
                return py.Team == player.OppTeam && blocks[3] == "0" && (blocks[4] == "JP01" || blocks[4] == "JP06");
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
                XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,1");
                XI.RaiseGMessage("G0OB," + XI.Board.Monster1 + ",2");
                if (XI.Board.Hinder.IsTared)
                {
                    TargetPlayer(player.Uid, XI.Board.Hinder.Uid);
                    XI.RaiseGMessage("G0OX," + XI.Board.Hinder.Uid + ",1,1");
                }
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,2");
                XI.RaiseGMessage("G0OB," + XI.Board.Monster2 + ",2");
            }
        }
        public bool JNS0902Valid(Player player, int type, string fuse)
        {
            return IsMathISOS("JNS0902", player, fuse);
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
                return XI.Board.PoolEnabled && XI.Board.IsAttendWar(player) && IsMathISOS("JNS0401", player, fuse);
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
            XI.RaiseGMessage(new Artiad.Goto()
            {
                CrossStage = false,
                Terminal = "R" + XI.Board.Rounder.Uid + "VT"
            }.ToMessage());
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
                return ut == player.SingleTokenTar;
            }
            return false;
        }
        public void JNS0501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort target = ushort.Parse(argst);
                TargetPlayer(player.Uid, target);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + target);
                XI.Board.Garden[target].ZPDisabled = true;
            }
            else if (type == 1)
            {
                if (player.SingleTokenTar != 0)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
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
            TargetPlayer(player.Uid, new ushort[] { t1, t2 });
            XI.RaiseGMessage(new Artiad.TradePet()
            {
                A = t1,
                AGoods = XI.Board.Garden[t1].Pets.Where(p => p != 0).ToArray(),
                B = t2,
                BGoods = XI.Board.Garden[t2].Pets.Where(p => p != 0).ToArray(),
            }.ToMessage());
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
            XI.RaiseGMessage("G2FU,0,0,0,C," + string.Join(",", XI.Board.Garden[target].Tux));
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
            TargetPlayer(player.Uid, who);
            XI.RaiseGMessage(new Artiad.LosePet() { Owner = who, SinglePet = pet }.ToMessage());
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

        #region Override JNSBase Util Functions
        protected new void Harm(Player src, Player py, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            base.Harm(src, py, n, five, HPEvoMask.FROM_SK.Set(mask));
        }
        protected new void Harm(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            base.Harm(src, invs, n, five, HPEvoMask.FROM_SK.Set(mask));
        }
        protected new void Harm(Player src, IEnumerable<Player> invs,
            IEnumerable<int> ns, FiveElement five = FiveElement.A, long mask = 0)
        {
            base.Harm(src, invs, ns, five, HPEvoMask.FROM_SK.Set(mask));
        }

        protected new void Cure(Player src, Player py, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            base.Cure(src, py, n, five, HPEvoMask.FROM_SK.Set(mask));
        }
        protected new void Cure(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            base.Cure(src, invs, n, five, HPEvoMask.FROM_SK.Set(mask));
        }
        protected new void Cure(Player src, IEnumerable<Player> invs,
            IEnumerable<int> ns, FiveElement five = FiveElement.A, long mask = 0)
        {
            base.Cure(src, invs, ns, five, HPEvoMask.FROM_SK.Set(mask));
        }
        #endregion Override JNSBase Util Functions
    }
}