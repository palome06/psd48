using PSD.Base;
using PSD.Base.Card;
using PSD.Base.Utils;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

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
            //else if (type == 1) // G0QR,90,1,1
            //{
            //    ushort who = ushort.Parse(fuse.Substring("G0QR,".Length));
            //    TargetPlayer(player.Uid, who);
            //}
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
            //else if (type == 1)
            //{
            //    ushort who = ushort.Parse(fuse.Substring("G0QR,".Length));
            //    Player py = XI.Board.Garden[who];
            //    if (py != null && py.Team == player.Team && py.Immobilized)
            //        return true;
            //}
            return false;
        }
        public void JNH0104Action(Player player, int type, string fuse, string argst)
        {
            string[] args = fuse.Split(',');
            //Board.IsTangled = true;
            ushort who = ushort.Parse(args[1]);
            ushort mon2ut = XI.Board.Monster2;
            Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            if (NMBLib.IsMonster(mon2ut))
            {
                // XI.WI.BCast("E0HZ,1," + who + "," + mon2ut);
                XI.RaiseGMessage(new Artiad.ImperialLeft()
                {
                    Zone = Artiad.ImperialLeft.ZoneType.M2,
                    Trigger = player.Uid,
                    Card = mon2ut
                }.ToMessage());
                Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2.STR >= mon1.STR)
                {
                    mon1.Curtain();
                    if (XI.Board.Mon1From != 0)
                    {
                        XI.RaiseGMessage(new Artiad.LosePet()
                        {
                            Owner = XI.Board.Mon1From,
                            SinglePet = XI.Board.Monster1,
                            Recycle = true
                        }.ToMessage());
                    }
                    else
                    {
                        XI.RaiseGMessage("G0WB," + XI.Board.Monster1);
                        XI.RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                            Genre = Card.Genre.NMB,
                            SingleUnit = new Artiad.CustomsUnit() { SingleCard = XI.Board.Monster1 }
                        }.ToMessage());
                    }
                    XI.RaiseGMessage(new Artiad.ImperialLeft()
                    {
                        Zone = Artiad.ImperialLeft.ZoneType.M1,
                        IsReset = true
                    }.ToMessage());
                    XI.Board.Monster1 = 0;

                    XI.RaiseGMessage("G1GE,W," + mon2);
                    XI.RaiseGMessage("G0WB," + XI.Board.Monster2);
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = XI.Board.Monster2 }
                    }.ToMessage());
                    XI.RaiseGMessage(new Artiad.ImperialLeft()
                    {
                        Zone = Artiad.ImperialLeft.ZoneType.M2,
                        Trigger = player.Uid,
                        IsReset = true
                    }.ToMessage());
                    XI.Board.Monster2 = 0;

                    XI.RaiseGMessage("G0IA," + player.Uid + ",2");
                    XI.RaiseGMessage(new Artiad.Goto()
                    {
                        CrossStage = false,
                        Terminal = "R" + XI.Board.Rounder.Uid + "ZN"
                    }.ToMessage());
                }
                else
                    XI.InnerGMessage(fuse, 281);
            }
            else if (NMBLib.IsNPC(mon2ut))
            {
                // XI.WI.BCast("E0HZ,2," + who + "," + mon2ut);
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = XI.Board.Monster2 }
                }.ToMessage());
                XI.RaiseGMessage(new Artiad.ImperialLeft()
                {
                    Zone = Artiad.ImperialLeft.ZoneType.M2,
                    IsReset = true
                }.ToMessage());
                XI.Board.Monster2 = 0;
                XI.InnerGMessage(fuse, 301);
            }
        }
        public bool JNH0104Valid(Player player, int type, string fuse)
        {
            string[] g0hzs = fuse.Split(',');
            ushort who = ushort.Parse(g0hzs[1]);
            ushort target = ushort.Parse(g0hzs[2]);
            ushort mon2ut = XI.Board.Monster2;
            return who == player.Uid && target != 0 && XI.Board.Monster2 != 0;
        }
        #endregion HL001 - Yanfeng
        #region HL002 - YangYue
        public bool JNH0201Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNH0201", player, fuse);
            else if (type == 1)
            {
                string[] g1ck = fuse.Split(',');
                return g1ck[1] == player.Uid.ToString() &&
                    new string[] { "JNH0203", "JNH0204", "JNH0205", "JNH0206" }.Contains(g1ck[2]);
            }
            else if (type == 2)
            {
                int idxc = fuse.IndexOf(',');
                ushort ut = ushort.Parse(fuse.Substring(idxc + 1));
                return player.SingleTokenTar == ut;
            }
            else return false;
        }
        public void JNH0201Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,2,I23,I24");
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,I23,I24");
                XI.RaiseGMessage("G0MA," + player.Uid + ",18");
                XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNH0203,JNH0204");
            }
            else if (type == 1)
            {
                string[] g1ck = fuse.Split(',');
                if (new string[] { "JNH0203", "JNH0204" }.Contains(g1ck[2]))
                {
                    XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNH0203,JNH0204");
                    XI.RaiseGMessage("G0MA," + player.Uid + ",19");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNH0205,JNH0206");
                }
                else if (new string[] { "JNH0205", "JNH0206" }.Contains(g1ck[2]))
                {
                    XI.RaiseGMessage("G0OS," + player.Uid + ",1,JNH0205,JNH0206");
                    XI.RaiseGMessage("G0MA," + player.Uid + ",18");
                    XI.RaiseGMessage("G0IS," + player.Uid + ",1,JNH0203,JNH0204");
                }
            }
            else if (type == 2)
            {
                if (player.SingleTokenTar != 0)
                {
                    XI.Board.Garden[player.SingleTokenTar].SetAllTuxDisabled("JNH0206", false);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                }
            }
        }
        public bool JNH0202Valid(Player player, int type, string fuse)
        {
            if (player.Guardian == 18)
            {
                string[] g0zw = fuse.Split(',');
                return player.IsAlive && Algo.TakeRange(g0zw, 1, g0zw.Length).Select(p => ushort.Parse(p))
                    .Any(p => XI.Board.Garden[p].Team == player.Team && p != player.Uid);
            }
            else if (player.Guardian == 19)
            {
                return player.IsAlive && !XI.Board.Garden.Values.Any(p => p.Uid != player.Uid &&
                    p.IsAlive && p.Team == player.Team);
            }
            else return false;
        }
        public void JNH0202Action(Player player, int type, string fuse, string argst)
        {
            if (player.Guardian == 18)
                XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JPT5,0;0," + fuse);
            else if (player.Guardian == 19)
            {
                List<ushort> sm = XI.Board.MonDises.Where(p => p != 0 && XI.LibTuple.ML.Decode(p) != null
                && XI.LibTuple.ML.Decode(p).STR <= 7).ToList();
                if (sm.Count > 0)
                {
                    string sl = XI.AsyncInput(player.Uid, "#获得,/M1(p" + string.Join("p", sm) + ")", "JNH0202", "0");
                    if (sl != VI.CinSentinel && !sl.StartsWith("/"))
                    {
                        ushort mon = ushort.Parse(sl);
                        XI.RaiseGMessage("G2CN,1,1");
                        XI.Board.MonDises.Remove(mon);
                        XI.RaiseGMessage(new Artiad.HarvestPet()
                        {
                            Farmer = player.Uid,
                            SinglePet = mon,
                            Plow = false
                        }.ToMessage());
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
        public bool JNH0203Valid(Player player, int type, string fuse)
        {
            return player.HP < player.HPb && player.Tux.Count > 0 && Artiad.Harm.Parse(
                fuse).Any(p => !HPEvoMask.TERMIN_AT.IsSet(p.Mask) && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask));
        }
        public void JNH0203Action(Player player, int type, string fuse, string argst)
        {
            ushort resi = ushort.Parse(argst);
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + resi);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (!HPEvoMask.CHAIN_INVAO.IsSet(harm.Mask) && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
                {
                    if (player.HP < 3) { harm.N *= 2; }
                    else { ++harm.N; }
                }
            }
            TargetPlayer(player.Uid, harms.Select(p => p.Who).Distinct());
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNH0203,0");
            XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -189);
        }
        public string JNH0203Input(Player player, int type, string fuse, string prev)
        {
            return prev == "" ? "/Q1(p" + string.Join("p", player.Tux) + ")" : "";
        }
        public bool JNH0204Valid(Player player, int type, string fuse)
        {
            if (player.Tux.Count > 0)
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("ZPT5");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH0204Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            ushort uid = player.Uid;
            XI.RaiseGMessage("G0CC," + uid + ",0," + uid + ",ZPT5," + ut + ";0," + fuse);
            XI.RaiseGMessage("G0CZ,0," + player.Uid);
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNH0204,0");
        }
        public string JNH0204Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                if (XI.Board.Rounder.Uid != player.Uid)
                {
                    List<ushort> tuxes = player.Tux.Where(p =>
                        XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.ZP).ToList();
                    return tuxes.Count > 0 ? "/Q1(p" + string.Join("p", tuxes) + ")" : "/";
                }
                else
                    return "/Q1(p" + string.Join("p", player.Tux) + ")";
            }
            else
                return "";
        }
        public void JNH0205Action(Player player, int type, string fuse, string argst)
        {
            while (XI.Board.TuxPiles.Count > 0)
            {
                bool isContinue = false;
                ushort ut = XI.DequeueOfPile(XI.Board.TuxPiles);
                XI.RaiseGMessage("G2IN,0,1");
                Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    if (Artiad.ContentRule.IsTuxUsableEveryWhere(tux) && tux.Valid(player, 0, fuse))
                    {
                        XI.RaiseGMessage(new Artiad.ImperialCentre()
                        {
                            Genre = Card.Genre.Tux,
                            SingleCard = ut
                        }.ToMessage());
                        string msg = "#是否要使用【" + tux.Name + "】##是##否,Y2";
                        string use = XI.AsyncInput(player.Uid, msg, "JNH0205", "0");
                        if (use == "1")
                            isContinue |= Artiad.Procedure.UseCardDirectly(player, ut, fuse, XI, 0);
                    }
                }
                if (!isContinue)
                {
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                        Genre = Card.Genre.Tux,
                        SingleUnit = new Artiad.CustomsUnit() { SingleCard = ut }
                    }.ToMessage());
                    break;
                }
            }
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNH0205,0");
        }
        public bool JNH0206Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && XI.Board.Garden.Values.Any(p =>
                XI.Board.IsAttendWar(p) && p.Team == player.OppTeam && p.IsTared && p.Tux.Count > 0);
        }
        public void JNH0206Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst);
            XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + ut);

            Player tar = XI.Board.Garden[ut];
            TargetPlayer(player.Uid, tar.Uid);
            string c0 = Algo.RepeatString("p0", tar.Tux.Count);
            XI.AsyncInput(player.Uid, "#展示的,C1(" + c0 + ")", "JNH0206", "0");
            List<ushort> vals = tar.Tux.ToList();
            vals.Shuffle();
            ushort randomCard = vals[0];

            XI.RaiseGMessage(new Artiad.AnnouceCard()
            {
                Action = Artiad.AnnouceCard.Type.DECLARE,
                Officer = ut,
                Genre = Card.Genre.Tux,
                SingleCard = randomCard
            }.ToMessage());
            Tux tux = XI.LibTuple.TL.DecodeTux(randomCard);
            if (tux.Type == Tux.TuxType.JP)
                Harm(player, new Player[] { player, tar }, 1);
            else if (tux.Type == Tux.TuxType.ZP)
                XI.RaiseGMessage("G0OA," + ut + ",1,2");
            else if (tux.Type == Tux.TuxType.TP)
                tar.SetAllTuxDisabled("JNH0206", true);
            else if (tux.IsTuxEqiup())
                XI.RaiseGMessage(new Artiad.EquipStandard()
                {
                    Who = ut,
                    Source = ut,
                    SingleCard = randomCard
                }.ToMessage());

            if (!tux.IsTuxEqiup())
            {
                string willAsk = string.Format("#是否弃置{0}的【{1}】##是##否,Y2",
                    XI.DisplayPlayer(ut), tux.Name);
                string willDiscard = XI.AsyncInput(player.Uid, willAsk, "JNH0206", "1");
                if (willDiscard == "1")
                {
                    if (tar.ListOutAllCards().Contains(randomCard))
                        XI.RaiseGMessage("G0QZ," + ut + "," + randomCard);
                    XI.RaiseGMessage("G0DH," + ut + ",0,1");
                }
                else
                {
                    willDiscard = XI.AsyncInput(ut, willAsk, "JNH0206", "2");
                    if (willDiscard == "1")
                    {
                        if (tar.ListOutAllCards().Contains(randomCard))
                            XI.RaiseGMessage("G0QZ," + ut + "," + randomCard);
                        XI.RaiseGMessage("G0DH," + ut + ",0,1");
                    }
                }
            }
            XI.RaiseGMessage("G1CK," + player.Uid + ",JNH0206,0");
        }
        public string JNH0206Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                        XI.Board.IsAttendWar(p) && p.Team == player.OppTeam &&
                        p.IsTared && p.Tux.Count > 0).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        #endregion HL002 - YangYue
        #region HL003 - YangTai
        public bool JNH0301Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N <= player.GetAllCardsCount() &&
                !HPEvoMask.DECR_INVAO.IsSet(p.Mask) && !HPEvoMask.TERMIN_AT.IsSet(p.Mask)) &&
                XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared);
        }
        public void JNH0301Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            string[] blocks = argst.Split(',');
            ushort to = ushort.Parse(blocks[0]);
            TargetPlayer(player.Uid, to);
            //int mValue = 0; 
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", Algo.TakeRange(blocks, 1, blocks.Length)));
            Artiad.Procedure.RotateHarm(player, XI.Board.Garden[to], false, (v) => v, ref harms);
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
                    if (harm.Who == player.Uid && !HPEvoMask.DECR_INVAO.IsSet(harm.Mask) &&
                        !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
                        n = harm.N;
                }
                return "/Q" + n + "(p" + string.Join("p", player.ListOutAllCards()) + ")";
            }
            else
                return "";
        }
        public void JNH0302Action(Player player, int type, string fuse, string argst)
        {
            System.Action<Player, bool> rotation = (py, rst) =>
            {
                int delta = py.STRh - py.DEXh;
                if (rst) { delta = -delta; }
                if (delta > 0)
                {
                    XI.RaiseGMessage("G0IX," + py.Uid + ",0," + delta);
                    XI.RaiseGMessage("G0OA," + py.Uid + ",0," + delta);
                }
                else if (delta < 0)
                {
                    XI.RaiseGMessage("G0OX," + py.Uid + ",0," + (-delta));
                    XI.RaiseGMessage("G0IA," + py.Uid + ",0," + (-delta));
                }
            };
            if (type == 0)
            {
                if (player.TokenTars.Count == 0)
                {
                    rotation(player, false);
                    // Reverse Ordered Player
                    XI.RaiseGMessage("G0HR,0,1");
                    string another = XI.AsyncInput(player.Uid, "#另一名,/T1" +
                        AOthersTared(player), "JNH0302Action", "0");
                    if (!another.StartsWith("/") && another != VI.CinSentinel)
                    {
                        ushort to = ushort.Parse(another);
                        XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + to);
                        Player py = XI.Board.Garden[to];
                        rotation(py, false);
                    }
                    else
                        XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + player.Uid);
                }
                else
                {
                    rotation(player, true);
                    // Reverse Ordered Player
                    XI.RaiseGMessage("G0HR,0,1");
                    if (player.SingleTokenTar != player.Uid)
                        rotation(XI.Board.Garden[player.SingleTokenTar], true);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                }
            }
            else if (type == 1) // Leave of myself
            {
                XI.RaiseGMessage("G0HR,0,1");
                if (player.SingleTokenTar != 0)
                {
                    rotation(XI.Board.Garden[player.SingleTokenTar], true);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                }
            }
            else if (type == 2) // Leave of the target
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + player.Uid);
            }
            else if (type == 3) // still fold the one taking same change
                rotation(XI.Board.Garden[player.SingleTokenTar], false);
            else if (type == 4)
            {
                Artiad.InnateChange ic = Artiad.InnateChange.Parse(fuse);
                if (ic.NewValue <= 0)
                    ic.NewValue = 0;
                Player py = XI.Board.Garden[ic.Who];
                if (ic.Item == Artiad.InnateChange.Prop.STR)
                {
                    int oldValue = py.STRh;
                    py.STRh = ic.NewValue;
                    if (oldValue > ic.NewValue)
                        XI.RaiseGMessage("G0OX," + ic.Who + ",0," + (oldValue - ic.NewValue));
                    else if (oldValue < ic.NewValue)
                        XI.RaiseGMessage("G0IX," + ic.Who + ",0," + (ic.NewValue - oldValue));
                }
                else if (ic.Item == Artiad.InnateChange.Prop.DEX)
                {
                    int oldValue = py.DEXh;
                    py.DEXh = ic.NewValue;
                    if (oldValue > ic.NewValue)
                        XI.RaiseGMessage("G0OA," + ic.Who + ",0," + (oldValue - ic.NewValue));
                    else if (oldValue < ic.NewValue)
                        XI.RaiseGMessage("G0IA," + ic.Who + ",0," + (ic.NewValue - oldValue));
                }
                XI.InnerGMessage(fuse, 101);
            }
        }
        public bool JNH0302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
                return !XI.Board.ClockWised && IsMathISOS("JNH0302", player, fuse);
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
            else if (type == 4 && player.SingleTokenTar != 0)
            {
                Artiad.InnateChange ic = Artiad.InnateChange.Parse(fuse);
                return (ic.Who == player.Uid || ic.Who == player.SingleTokenTar) &&
                    (ic.Item == Artiad.InnateChange.Prop.STR || ic.Item == Artiad.InnateChange.Prop.DEX);
            }
            return false;
        }
        public void JNH0303Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0TT," + player.Uid);
            if (XI.Board.DiceValue != 6)
            {
                string select = XI.AsyncInput(player.Uid, "#受伤或弃牌,T1" + AOthersTared(player) +
                    ",#请选择『千栗鬼噬』执行项##HP-2##弃所有牌,Y2", "JNH0303", "0");
                int idx = select.IndexOf(',');
                string opt = Algo.Substring(select, idx + 1, -1);
                ushort who = ushort.Parse(Algo.Substring(select, 0, idx));
                if (opt == "1")
                    Harm(player, XI.Board.Garden[who], 2, FiveElement.A, (long)HPEvoMask.TUX_INAVO);
                else
                    XI.RaiseGMessage("G0DH," + who + ",3");
            }
        }
        public bool JNH0303Valid(Player player, int type, string fuse)
        {
            return Algo.TakeRange(fuse.Split(','), 1, -1).Any(p => p == player.Uid.ToString());
        }
        #endregion HL003 - YangTai
        #region HL004 - YeFengling
        public bool JNH0401Valid(Player player, int type, string fuse)
        {
            return player.Tux.Count >= 2 && player.Team == XI.Board.Rounder.Team;
        }
        public void JNH0401Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            XI.RaiseGMessage("G0OW," + XI.Board.Monster1 + ",2");
            XI.RaiseGMessage(new Artiad.Goto()
            {
                CrossStage = false,
                Terminal = "R" + XI.Board.Rounder.Uid + "PD"
            }.ToMessage());
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
                return true;
            else if (type == 1 && player.TokenAwake) // G0CE
            {
                int idx = fuse.IndexOf(';');
                string[] g0ce = fuse.Substring(0, idx).Split(',');
                ushort who = ushort.Parse(g0ce[1]);
                Player py = XI.Board.Garden[who];
                if (py != null && py.Team == player.Team)
                    return true;
            }
            else if (type == 2 && player.TokenAwake) // G1UE
            {
                string[] g1ue = fuse.Split(',');
                ushort who = ushort.Parse(g1ue[1]);
                Player py = XI.Board.Garden[who];
                if (py != null && py.Team == player.Team)
                    return true;
            }
            else if (type == 3) // Remove the token
                return player.TokenAwake;
            return false;
        }
        public void JNH0402Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
                player.RAM.Set("+1tux", true);
                player.SetZPDisabled("JNH0402", true);
            }
            else if (type == 1 || type == 2)
            {
                player.RAM.Set("+1tux", false);
                XI.RaiseGMessage("G0IP," + player.Team + ",1");
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
                player.SetZPDisabled("JNH0402", false);
                if (player.RAM.GetBool("+1tux"))
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                player.RAM.Set("+1tux", null);
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
            string target = XI.AsyncInput(player.Uid, "#月神附身,T1" + AOthersTared(player), "JNH0403", "0");
            ushort who = ushort.Parse(target);
            // change the player as HL005
            int orgHero = XI.Board.Garden[who].SelectHero;
            XI.RaiseGMessage("G0OY,0," + who);
            XI.RaiseGMessage("G0IY,0," + who + ",19005");
            XI.RaiseGMessage("G0IV," + who + "," + orgHero);
            XI.Board.BannedHero.Add(orgHero);
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
                XI.Board.BannedHero.Remove(orgHero);
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
                XI.Board.BannedHero.Remove(orgHero);
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
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.Team
                    && (p.HasAnyEquips() || p.GetPetCount() > 0));
            else if (type == 1 && player.TokenAwake)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                return cures.Any(p => XI.Board.Garden[p.Who].IsAlive &&
                     XI.Board.Garden[p.Who].Team == player.OppTeam && p.N > 0);
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
                    XI.RaiseGMessage("G0ZI," + args[1] + "," + args[2]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
            }
            else if (type == 1)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                cures.RemoveAll(p => XI.Board.Garden[p.Who].IsAlive &&
                     XI.Board.Garden[p.Who].Team == player.OppTeam && p.N > 0);
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
            else if (type == 1 && player.TokenTars.Count > 0)
            {
                string[] g0ds = fuse.Split(',');
                return g0ds[1] == player.Uid.ToString() && g0ds[2] == "1";
            }
            else if (type == 2 && player.TokenTars.Count > 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                return harms.Any(p => p.Who == player.SingleTokenTar
                     && p.N > 0 && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
            }
            else if (type == 3 && player.TokenTars.Count > 0)
                return XI.Board.Rounder.Uid == player.SingleTokenTar;
            else if (type == 4 && player.TokenTars.Count > 0)
                return true;
            else
                return false;
        }
        public void JNH0503Action(Player player, int type, string fuse, string argst)
        {
            // R#TM,G0DS,G0OH,AskForAttender??,R#GR(Before)
            if (type == 0)
            {
                string[] args = argst.Split(',');
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + args[0]);
                XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
                ushort tar = ushort.Parse(args[1]);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",2,1," + tar);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",2,1," + player.SingleTokenTar);
            else if (type == 2)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.RemoveAll(p => p.Who == player.SingleTokenTar
                     && p.N > 0 && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -44);
            }
            else if (type == 3)
            {
                XI.RaiseGMessage(new Artiad.Goto()
                {
                    CrossStage = false,
                    Terminal = "R" + XI.Board.Rounder.Uid + "Z0"
                }.ToMessage());
            }
            else if (type == 4)
            {
                Player rd = XI.Board.Rounder;
                Player st = XI.Board.Garden[player.SingleTokenTar];
                string name = "T" + player.SingleTokenTar.ToString();
                if (rd.Team == st.Team)
                {
                    if (rd.Uid != player.SingleTokenTar)
                        XI.Board.PosSupporters.Remove(name);
                    else
                        XI.Board.PosSupporters.Clear();
                }
                else if (rd.Team == st.OppTeam)
                    XI.Board.PosHinders.Remove(name);
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
                    else if (blocks[1] == "4")
                    {
                        string me = player.Uid.ToString();
                        if (blocks[2] == me && blocks[4] != "0")
                            return XI.Board.Garden[ushort.Parse(blocks[3])].Team == player.OppTeam;
                        if (blocks[3] == me && blocks[5] != "0")
                            return XI.Board.Garden[ushort.Parse(blocks[2])].Team == player.OppTeam;
                    }
                }
                return false;
            }
            else if (type == 2)
            {
                return player.ExCards.Count > 0 && Artiad.Cure.Parse(fuse).Any(p => p.N > 0 &&
                    XI.Board.Garden[p.Who].IsTared && !HPEvoMask.TERMIN_AT.IsSet(p.Mask));
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
                        Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                        if (tux != null)
                        {
                            if (!tux.IsTuxEqiup())
                                XI.RaiseGMessage(new Artiad.EquipExCards()
                                {
                                    Who = player.Uid,
                                    FromSky = true,
                                    SingleCard = ut
                                }.ToMessage());
                            else
                                XI.RaiseGMessage(new Artiad.Abandon()
                                {
                                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                                    Genre = Card.Genre.Tux,
                                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = ut }
                                }.ToMessage());
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
                Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    if (!tux.IsTuxEqiup())
                    {
                        XI.RaiseGMessage(new Artiad.EquipExCards()
                        {
                            Who = player.Uid,
                            FromSky = true,
                            SingleCard = ut
                        }.ToMessage());
                    }
                    else
                        XI.RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                            Genre = Card.Genre.Tux,
                            SingleUnit = new Artiad.CustomsUnit() { SingleCard = ut }
                        }.ToMessage());
                }
            }
            else if (type == 2)
            {
                ushort[] args = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + args[0]);
                ushort[] invs = Algo.TakeRange(args, 2, args.Length);
                if (args[1] == 1) // Cure
                    Cure(player, invs.Select(p => XI.Board.Garden[p]), 1);
                else if (args[1] == 2) // Obtain Cards
                    XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p + ",0,1")));
            }
        }
        public string JNH0601Input(Player player, int type, string fuse, string prev)
        {
            if (type == 2 && prev == "")
            {
                string p1 = "#弃置「仁心」,/Q1(p" + string.Join("p", player.ExCards) + ")";
                string p2 = "#请选择『妙法仁心』执行项##HP+1##补1张牌,Y2";
                List<ushort> invs = Artiad.Cure.Parse(fuse).Where(p => p.N > 0 &&
                    XI.Board.Garden[p.Who].IsTared && !HPEvoMask.TERMIN_AT.IsSet(p.Mask))
                    .Select(p => p.Who).ToList();
                string p3 = "/T1" + (invs.Count > 1 ? ("~" + invs.Count) : "") + "(p" + string.Join("p", invs) + ")";
                return p1 + "," + p2 + "," + p3;
            }
            else
                return "";
        }
        public bool JNH0602Valid(Player player, int type, string fuse)
        {
            if (type == 0) // HarmResult
                return !player.TokenAwake && Artiad.Harm.Parse(fuse).Any(p => p.N > 0);
            else if (type == 1)
            {
                return player.TokenAwake && Artiad.Harm.Parse(fuse).Any(p => p.N > 0 &&
                    !HPEvoMask.DECR_INVAO.IsSet(p.N) && !HPEvoMask.TERMIN_AT.IsSet(p.N));
            }
            else if (type == 2)
                return player.TokenAwake;
            return false;
        }
        public void JNH0602Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (!HPEvoMask.DECR_INVAO.IsSet(harm.Mask) && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
                        --harm.N;
                }
                harms.RemoveAll(p => p.N == 0);
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -149);
            }
            else if (type == 2)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
        }
        public bool JNH0603Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Select(p => XI.Board.Garden[p.Who]).Any(p => p.IsTared &&
                p.HP > 0 && p.HP < 3 && p.ListOutAllCards().Count > 0);
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
                List<ushort> invs = harms.Select(p => XI.Board.Garden[p.Who]).Where(p => p.IsTared &&
                    p.HP > 0 && p.HP < 3 && p.ListOutAllCards().Count > 0).Select(p => p.Uid).Distinct().ToList();
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
            if (type == 0) // ON
            {
                Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
                return ab.Genre == Card.Genre.NMB && ab.List.Any(p => p.Cards.Any(q => NMBLib.IsNPC(q)));
            }
            else if (type == 1) // PD
            {
                bool stucken = (XI.Board.IsAttendWar(player) && player.TokenExcl.Count > 0) ||
                    player.TokenExcl.Any(p => XI.LibTuple.NL.Decode(
                    (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR >= 5);
                return stucken;
            }
            else if (type == 2 && XI.Board.PoolEnabled && player.TokenExcl.Count > 0) // FI
                return Artiad.CoachingChange.Parse(fuse).AttendOrLeave(player.Uid) != 0;
            else if (type == 3 || type == 4) // IS/OS
            {
                if (IsMathISOS("JNH0701", player, fuse) && XI.Board.PoolEnabled)
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
                ushort npcUt = ushort.Parse(argst);
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(npcUt));
                int rest = System.Math.Max(npc.STR, player.TokenExcl.Count) + 1;
                foreach (ushort ut in XI.Board.OrderedPlayer())
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
                    Artiad.Abandon abandon = Artiad.Abandon.Parse(fuse);
                    if (Artiad.CustomsHelper.RemoveCards(abandon, npcUt))
                        XI.InnerGMessage(abandon.ToMessage(), 30);
                }
                else
                    XI.InnerGMessage(fuse, 31);
            }
            else if (type == 1 || type == 2 || type == 3)
            {
                if (XI.Board.PoolEnabled)
                {
                    int upfive = player.TokenExcl.Count(p => XI.LibTuple.NL.Decode(
                        (ushort)(int.Parse(p.Substring("M".Length)) - 1000)).STR >= 5);
                    int dnfive = player.TokenExcl.Count;
                    if (XI.Board.IsAttendWar(player))
                        XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JNH0701," + dnfive);
                    else if (!XI.Board.IsAttendWar(player))
                        XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JNH0701," + upfive);
                }
            }
            else if (type == 4)
                XI.RaiseGMessage("G1WP," + player.Team + "," + player.Uid + ",JNH0701,0");
        }
        public string JNH0701Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                return "#收为「魅心」,/M1(p" + string.Join("p", Artiad.Abandon.Parse(fuse)
                    .List.SelectMany(p => p.Cards).Where(p => NMBLib.IsNPC(p))) + ")";
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
            TargetPlayer(player.Uid, tar);
            //XI.RaiseGMessage("G2CN,0,1");
            //XI.Board.TuxDises.Remove(ut);
            rest.Remove(ut); eqs.Remove(ut);
            XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
            if (blocks.Length >= 3)
            {
                ushort @return = ushort.Parse(blocks[2]);
                rest.Remove(@return); eqs.Remove(@return);
                XI.RaiseGMessage("G0HQ,2," + tar + ",0,0," + @return);
            }
            rest.AddRange(eqs);
            XI.InnerGMessage("G1DI," + tar + ",1," + rest.Count + "," +
                (rest.Count - eqs.Count) + "," + string.Join(",", rest), 31);
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
                        tuxes.AddRange(Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
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
                        tuxes.AddRange(Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
                            .Select(p => ushort.Parse(p)));
                    }
                    idx += (4 + n);
                }
                tuxes.Remove(except);
                if (tuxes.Count > 1)
                    return "#对方获得,/C1(p" + string.Join("p", tuxes) + ")";
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
            List<Player> invs = harms.Where(p => p.Element.IsPropedElement())
                .Select(p => XI.Board.Garden[p.Who]).Where(p => p.IsAlive).Distinct().ToList();
            TargetPlayer(player.Uid, invs.Select(p => p.Uid));
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
            return XI.Board.Garden.Values.Count(p => p.IsAlive && p.Immobilized && p.Team == player.OppTeam) <=
                XI.Board.Garden.Values.Count(p => p.IsAlive && p.Immobilized && p.Team == player.Team) &&
                player.Tux.Count > 0 && harms.Any(p => XI.Board.Garden[p.Who].IsAlive && p.Element.IsPropedElement());
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
            ushort who = 0;
            if (argst.StartsWith("T"))
                who = ushort.Parse(argst.Substring("T".Length));
            else if (argst.StartsWith("P"))
                who = (ushort)(ushort.Parse(argst.Substring("PT".Length)) + 1000);
            else if (argst.StartsWith("I"))
                who = (ushort)(ushort.Parse(argst.Substring("I".Length)) + 3000);
            if (who != 0)
            {
                if (XI.Board.Supporter == player)
                    XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                    {
                        Role = Artiad.CoachingHelper.PType.SUPPORTER, Coach = who
                    } }.ToMessage());
                else if (XI.Board.Hinder == player)
                    XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                    {
                        Role = Artiad.CoachingHelper.PType.HINDER, Coach = who
                    } }.ToMessage());
            }
        }
        public bool JNH0802Valid(Player player, int type, string fuse)
        {
            List<string> ps = XI.Board.Supporter == player ? XI.Board.PosSupporters :
                    (XI.Board.Hinder == player ? XI.Board.PosHinders : null);
            if (ps != null)
            {
                ps = ps.Where(p => p != "T" + player.Uid &&
                    p != "T" + XI.Board.Rounder.Uid && p != "0").ToList();
                ps.RemoveAll(p => p.StartsWith("T") &&
                    !XI.Board.Garden[ushort.Parse(p.Substring("T".Length))].IsTared);
                return ps.Any();
            }
            return false;
        }
        public string JNH0802Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<string> ps = XI.Board.Supporter == player ? XI.Board.PosSupporters :
                    (XI.Board.Hinder == player ? XI.Board.PosHinders : null);
                if (ps != null)
                {
                    ps = ps.Where(p => p != "T" + player.Uid &&
                        p != "T" + XI.Board.Rounder.Uid && p != "0").ToList();
                    ps.RemoveAll(p => p.StartsWith("T") &&
                        !XI.Board.Garden[ushort.Parse(p.Substring("T".Length))].IsTared);
                    return "#改为参战,/J1(p" + string.Join("p", ps) + ")";
                }
            }
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
                // G0CC,A,0,B,KN,x1,x2;TF
                int idx = fuse.IndexOf(';');
                string[] g0cc = fuse.Substring(0, idx).Split(',');
                string kn = g0cc[4];
                ushort[] txs = Algo.TakeRange(g0cc, 5, g0cc.Length).Select(p => ushort.Parse(p)).ToArray();
                if (kn != "TP01" && kn != "TPT3" && (txs.Any(p => p != 0)))
                {
                    Tux ktux = XI.LibTuple.TL.EncodeTuxCode(kn);
                    ushort trigger = ushort.Parse(g0cc[3]);
                    Player py = XI.Board.Garden[trigger];
                    if (py.Team == player.OppTeam && ktux != null && ktux.Type == Tux.TuxType.TP)
                        return true;
                }
            }
            return false;
        }
        public void JNH0901Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
            int hdx = fuse.IndexOf(';');
            string[] g0cc = Algo.Substring(fuse, 0, hdx).Split(',');
            // ushort ust = ushort.Parse(g0cc[1]);
            ushort[] txs = Algo.TakeRange(g0cc, 5, g0cc.Length).Select(p => ushort.Parse(p)).ToArray();
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
                string origin = Algo.Substring(fuse, kdx + 1, -1);
                //if (origin.StartsWith("G0CC"))
                //    XI.InnerGMessage(origin, 141);
                //else if (origin.StartsWith("G"))
                //    XI.RaiseGMessage(origin);
                if (origin.StartsWith("G"))
                {
                    string cardname = g0cc[4];
                    int inType = int.Parse(Algo.Substring(fuse, hdx + 1, kdx));
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
                return cands.Count >= 2 ? ("/Q2(p" + string.Join("p", cands) + ")") : "/";
            }
            else
                return "";
        }
        public bool JNH0902Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.Tux.Count > 0)
            {
                if (!string.IsNullOrEmpty(player.RAM.GetString("ZPName")) && !player.RAM.GetBool("Winded"))
                {
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(player.RAM.GetString("ZPName"));
                    return tux != null && tux.Valid(player, 0, fuse);
                }
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                ushort ut = ushort.Parse(blocks[1]);
                string cardCode = blocks[4];
                if (!player.RAM.GetBool("Winded") && ut == player.Uid)
                {
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardCode);
                    if (tux != null && tux.Type == Base.Card.Tux.TuxType.ZP)
                        return true;
                }
            }
            return false;
        }
        public void JNH0902Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort ut = ushort.Parse(argst);
                if (player.IsAlive)
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + "," +
                        player.RAM.GetString("ZPName") + "," + ut + ";0," + fuse);
                player.RAM.Set("Winded", true);
            }
            else if (type == 1)
            {
                string[] blocks = fuse.Split(',');
                player.RAM.Set("ZPName", blocks[4]);
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
                string[] g0cc = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
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
            else if (type == 2) // G1UE
            {
                string[] g1ue = fuse.Split(',');
                ushort who = ushort.Parse(g1ue[1]);
                return who == player.Uid;
            }
            else if (type == 3) // R*TM
                return player.RFM.GetInt("MemoryCount") > player.HP;
            else if (type == 4) // R*ED
                return player.TokenCount > 0;
            else return false;
        }
        public void JNH0903Action(Player player, int type, string fuse, string argst)
        {
            System.Action<int> add = n =>
            {
                int now = player.RFM.GetInt("MemoryCount") + n;
                player.RFM.Set("MemoryCount", now);
                if (now > 8 && player.TokenCount <= 8)
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",0," + (9 - player.TokenCount));
                else if (player.TokenCount <= 8)
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",0," + n);
            };

            if (type == 0)
            {
                string[] g0cc = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                int n = g0cc.Length - 5;
                if (g0cc[5] == "0")
                    --n;
                add(n);
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
                        add(n);
                    idx += (4 + n);
                }
            }
            else if (type == 2)
                add(1);
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + player.TokenCount);
                XI.RaiseGMessage("G0OY,1," + player.Uid);
                XI.RaiseGMessage("G0OS," + player.Uid + ",0,JNH0901,JNH0902,JNH0903");
                XI.RaiseGMessage("G0IY,1," + player.Uid + ",19015");
                XI.RaiseGMessage("G0IS," + player.Uid + ",0,JNH1501,JNH1502");
            }
            else if (type == 4)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",0," + player.TokenCount);
        }
        #endregion HL009 - Lingjian
        #region HL010 - ShuiLingjing
        public bool JNH1001Valid(Player player, int type, string fuse)
        {
            if (!player.IsAlive)
                return false;
            string[] g1ly = fuse.Split(',');
            for (int idx = 1; idx < g1ly.Length;)
            {
                ushort who = ushort.Parse(g1ly[idx]);
                int n = int.Parse(g1ly[idx + 1]);
                //List<ushort> cards = Algo.TakeRange(g0ot, idx + 2, idx + 2 + n)
                //    .Select(p => ushort.Parse(p)).ToList();
                Player py = XI.Board.Garden[who];
                if (py.IsTared && py.Team == player.Team && py.Tux.Count == 0 &&
                    !player.RFM.GetOrSetUshortArray("Couraged").Contains(who))
                {
                    return true;
                }
                idx += (n + 2);
            }
            return false;
        }
        public void JNH1001Action(Player player, int type, string fuse, string argst)
        {
            ushort[] whos = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            TargetPlayer(player.Uid, whos);
            player.RFM.GetOrSetUshortArray("Couraged").AddRange(whos);
            XI.RaiseGMessage("G0DH," + string.Join(",", whos.Select(p =>
                p + ",0," + (XI.Board.Garden[p].GetPetCount() >= 2 ? 2 : 1))));
        }
        public string JNH1001Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                ISet<ushort> invs = new HashSet<ushort>();
                string[] g0ot = fuse.Split(',');
                for (int idx = 1; idx < g0ot.Length;)
                {
                    ushort who = ushort.Parse(g0ot[idx]);
                    int n = int.Parse(g0ot[idx + 1]);
                    //List<ushort> cards = Algo.TakeRange(g0ot, idx + 2, idx + 2 + n)
                    //    .Select(p => ushort.Parse(p)).ToList();
                    Player py = XI.Board.Garden[who];
                    if (py.IsTared && py.Team == player.Team && py.Tux.Count == 0 &&
                        !player.RFM.GetOrSetUshortArray("Couraged").Contains(who))
                    {
                        invs.Add(who);
                    }
                    idx += (n + 2);
                }
                return "/T1" + (invs.Count > 1 ? ("~" + invs.Count) : "") + "(p" + string.Join("p", invs) + ")";
            }
            else return "";
        }
        public bool JNH1002Valid(Player player, int type, string fuse)
        {
            if (type == 0) // G0IC
            {
                return Artiad.JoinPetEffects.Parse(fuse).List.Any(p =>
                    XI.Board.Garden[p.Owner].IsAlive && XI.Board.Garden[p.Owner].Team == player.Team);
            }
            else if (type == 1) // G0OC
            {
                return Artiad.CollapsePetEffects.Parse(fuse).List.Any(p =>
                    XI.Board.Garden[p.Owner].IsAlive && XI.Board.Garden[p.Owner].Team == player.Team);
            }
            else if (type == 2 || type == 3) // G0IS, G0OS
                return IsMathISOS("JNH1002", player, fuse) && XI.Board.Garden.Values
                    .Any(p => p.IsAlive && p.Team == player.Team && p.GetActivePetCount(XI.Board) > 0);
            else
                return false;
        }
        public void JNH1002Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                Artiad.JoinPetEffects jpes = Artiad.JoinPetEffects.Parse(fuse);
                jpes.List.ForEach(p => XI.Board.Garden[p.Owner].TuxLimit += p.Pets.Length);
            }
            else if (type == 1)
            {
                Artiad.CollapsePetEffects cpes = Artiad.CollapsePetEffects.Parse(fuse);
                cpes.List.ForEach(p => XI.Board.Garden[p.Owner].TuxLimit -= p.Pets.Length);
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
        }
        public bool JNH1003Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (py.Team == player.Team && harm.N >= py.HP)
                    return true;
            }
            return false;
        }
        public void JNH1003Action(Player player, int type, string fuse, string argst)
        {
            ushort tar = ushort.Parse(argst);
            ushort mon = XI.LibTuple.ML.Encode("GSH3");
            if (mon != 0)
            {
                XI.RaiseGMessage(new Artiad.HarvestPet()
                {
                    Farmer = tar,
                    SinglePet = mon,
                    Plow = false
                }.ToMessage());
                XI.RaiseGMessage("G0ZW," + player.Uid);
            }
        }
        public string JNH1003Input(Player player, int type, string fuse, string prev)
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
                bool b2 = XI.Board.Rounder.Team == player.Team && XI.Board.Hinder.Uid == 0;
                return b1 || b2;
            }
            else
                return false;
        }
        public void JNH1101Action(Player player, int type, string fuse, string argst)
        {
            List<Player> losses = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Uid != player.Uid &&
                XI.Board.IsAttendWar(p) && !XI.Board.IsAttendWarSucc(p) && p.ListOutAllCards().Count > 0).ToList();
            TargetPlayer(player.Uid, losses.Select(p => p.Uid));
            IDictionary<ushort, string> result = XI.MultiAsyncInput(losses.ToDictionary(p => p.Uid,
                p => "#交出的,Q1(p" + string.Join("p", p.ListOutAllCards()) + ")"));
            foreach (var pair in result)
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + string.Join(",", pair.Key + ",1,1," + pair.Value));
            if (!XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && XI.Board.IsAttendWar(p)))
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
        }
        public bool JNH1102Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && XI.Board.IsAttendWar(player);
            else if (type == 1 && player.TokenFold.Count > 0 && player.RAM.GetInt("Scared") > 0) // G0CE
            {
                int idx = fuse.IndexOf(';');
                string[] g0ce = fuse.Substring(0, idx).Split(',');
                Player py = XI.Board.Garden[ushort.Parse(g0ce[1])];
                return py != null && py.Team == player.OppTeam;
            }
            else if (type == 2 && player.TokenFold.Count > 0 && player.RAM.GetInt("Scared") > 0) // G1CW, only sheild the first one
            {
                int fdx = fuse.IndexOf(';');
                string[] g1cw = fuse.Substring(0, fdx).Split(',');
                Player py = XI.Board.Garden[ushort.Parse(g1cw[1])];
                return py != null && py.Team == player.OppTeam;
            }
            else if (type == 3)
                return player.TokenFold.Count > 0 || player.RAM.GetInt("Scared") > 0;
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
                player.RAM.Set("Scared", 2);
            }
            else if (type == 1)
            {
                int sc = player.RAM.GetInt("Scared");
                if (sc == 2)
                {
                    player.RAM.Set("Scared", 1);
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
                    XI.InnerGMessage(fuse, 91);
                }
                else if (sc == 1)
                {
                    player.RAM.Set("Scared", 0);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
                    int hdx = fuse.IndexOf(';');
                    string[] g0ce = Algo.Substring(fuse, 0, hdx).Split(',');
                    int kdx = fuse.IndexOf(',', hdx);
                    string origin = Algo.Substring(fuse, kdx + 1, -1);
                    TargetPlayer(player.Uid, ushort.Parse(g0ce[1]));
                    XI.RaiseGMessage("G2CL," + ushort.Parse(g0ce[1]) + "," + g0ce[4]);
                    if (origin.StartsWith("G") && g0ce[2] != "2") // Avoid Double Computation on Copy
                    {
                        string cardname = g0ce[4];
                        int inType = int.Parse(Algo.Substring(fuse, hdx + 1, kdx));
                        Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                        int prior = tux.Priorities[inType];
                        XI.InnerGMessage(origin, prior);
                        // TODO:Fatal Error, JNT2902 will trigger the event at wrong priority
                        // e.g. card=JP01,inType=0,prior=0;while origin=G1TH
                    }
                }
            }
            else if (type == 2)
            {
                int sc = player.RAM.GetInt("Scared");
                if (sc == 2)
                {
                    player.RAM.Set("Scared", 1);
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
                    XI.InnerGMessage(fuse, 81);
                }
                else if (sc == 1)
                {
                    player.RAM.Set("Scared", 0);
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
                    int fdx = fuse.IndexOf(';');
                    int hdx = fuse.IndexOf(';', fdx + 1);
                    int idx = fuse.IndexOf(',', hdx);
                    int sktInType = int.Parse(Algo.Substring(fuse, hdx + 1, idx));
                    string sktFuse = Algo.Substring(fuse, idx + 1, -1);
                    string cdFuse = Algo.Substring(fuse, fdx + 1, -1);

                    string[] g1cw = fuse.Substring(0, fdx).Split(',');
                    ushort first = ushort.Parse(g1cw[1]);
                    ushort second = ushort.Parse(g1cw[2]);
                    ushort provider = ushort.Parse(g1cw[3]);
                    Tux tux = XI.LibTuple.TL.EncodeTuxCode(g1cw[4]);
                    ushort it = ushort.Parse(g1cw[5]);
                    // provider, second, it
                    if (Artiad.Procedure.LocustChangePendingTux(XI, provider, second, it))
                    {
                        XI.RaiseGMessage("G2CL," + first + "," + g1cw[4]);
                        XI.InnerGMessage("G0CC," + provider + ",1," + second +
                            "," + tux.Code + "," + it + ";" + sktInType + "," + sktFuse, 101);
                    }
                    XI.InnerGMessage(cdFuse, 101);
                }
            }
            else if (type == 3)
            {
                if (player.TokenFold.Count > 0)
                {
                    List<ushort> ijs = player.TokenFold.ToList();
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",4," + ijs.Count + "," + string.Join(",", ijs));
                    XI.RaiseGMessage("G0IT," + player.Uid + "," + ijs.Count + "," + string.Join(",", ijs));
                }
                player.RAM.Set("Scared", 0);
                if (player.TokenAwake)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
                // Artiad.Procedure.SetPlayerAllEqEnable(XI, new Player[] { player }, 0x1, "JNH1102");
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
                string c0 = Algo.RepeatString("p0", XI.Board.Garden[py.Uid].Tux.Count);
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
                    ushort[] tuxs = Algo.TakeRange(uts, 0, uts.Length - 1);
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
            string result = AffichePlayers(p => p.IsAlive &&
                p.Uid != player.Uid && p.Tux.Count > 0, p => p.Uid + ",1,1");
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
            if (type == 0) // Needs consider the re-shuffle of Monster cards'
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
                XI.RaiseGMessage(new Artiad.ImperialCentre()
                {
                    Genre = Card.Genre.NMB,
                    SingleCard = pop
                }.ToMessage());
                Monster mon = XI.LibTuple.ML.Decode(pop);
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
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit()
                    {
                        Source = player.Uid,
                        Cards = animals.Select(p => ushort.Parse(p.Substring("M".Length))).ToArray()
                    }
                }.ToMessage());
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + sanimals);
            }
            else if (type == 2 || type == 3)
            {
                int agl = player.TokenExcl.Select(p => XI.LibTuple.ML.Decode(
                    ushort.Parse(p.Substring("M".Length)))).Sum(p => p.AGL) / 2;
                int delta = agl - player.ROM.GetInt("aglsum");
                if (delta > 0)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",0," + delta);
                else if (delta < 0)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",0," + (-delta));
                player.ROM.Set("aglsum", agl);

                string[] blocks = fuse.Split(',');
                int n = int.Parse(blocks[3]);
                XI.RaiseGMessage((type == 2 ? "G0IA," : "G0OA,") + player.Uid + ",0," + n);
            }
        }
        public bool JNH1302Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return Artiad.CoachingChange.Parse(fuse).List.Any(p => p.Role ==
                    Artiad.CoachingHelper.PType.GIVEUP) && XI.Board.Rounder.Team == player.OppTeam;
            }
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
                // G0CE,A,T,0,KN,y,z;TF
                string[] g0ce = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                ushort who = ushort.Parse(g0ce[1]);
                string tuxName = g0ce[4];
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(tuxName);
                return who == player.Uid && tux.Type == Tux.TuxType.ZP;
            } // only shield ZP which is of general locust, so ignore special log for G1CW
            else
                return false;
        }
        public void JNH1401Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                foreach (Tux tux in XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.ZP))
                    player.AddToPrice(tux.Code, false, "JNH1401", '=', 1);
                //player.SetZPDisabled("JNH1401", true);
            }
            else if (type == 1)
            {
                foreach (Tux tux in XI.LibTuple.TL.Firsts.Where(p => p.Type == Tux.TuxType.ZP))
                    player.RemoveFromPrice(tux.Code, false, "JNH1401");
                //player.SetZPDisabled("JNH1401", false);
            }
            else if (type == 2)
            {
                int hdx = fuse.IndexOf(';');
                string[] g0cc = Algo.Substring(fuse, 0, hdx).Split(',');
                int kdx = fuse.IndexOf(',', hdx);
                string origin = Algo.Substring(fuse, kdx + 1, -1);
                if (origin.StartsWith("G"))
                {
                    string cardname = g0cc[4];
                    int inType = int.Parse(Algo.Substring(fuse, hdx + 1, kdx));
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    int prior = tux.Priorities[inType];
                    XI.InnerGMessage(origin, prior);
                }
            }
        }
        public bool JNH1402Valid(Player player, int type, string fuse)
        {
            if (Artiad.KittyHelper.IsHarvest(fuse))
            {
                Artiad.HarvestPet hvp = Artiad.HarvestPet.Parse(fuse);
                var b = XI.Board;
                return player.Uid == b.Rounder.Uid && player.Uid == hvp.Farmer && hvp.Trophy && b.Garden.Values
                    .Any(p => p.IsTared && p.Uid != player.Uid && p.Team == player.Team && p.HP > player.HP);
            }
            else return false;
        }
        public void JNH1402Action(Player player, int type, string fuse, string argst)
        {
            string giveString = XI.AsyncInput(player.Uid, "#交予宠物,T1" + FormatPlayers(p => p.IsTared &&
                p.Uid != player.Uid && p.Team == player.Team && p.HP > player.HP), "JNH1402", "0");
            if (giveString != VI.CinSentinel)
            {
                ushort give = ushort.Parse(giveString);
                TargetPlayer(player.Uid, give);
                Artiad.HarvestPet hvp = Artiad.HarvestPet.Parse(fuse);
                hvp.Farmer = give;
                XI.InnerGMessage(hvp.ToMessage(), 21);
            }
            Harm(player, player, 1);
        }
        public bool JNH1403Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.CalculatePetsScore()[player.Team] >= 30;
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
            if (type == 0 && Artiad.KittyHelper.IsHarvest(fuse))
            {
                Artiad.HarvestPet hvp = Artiad.HarvestPet.Parse(fuse);
                Player py = XI.Board.Garden[hvp.Farmer];
                if (hvp.Trophy && py.Team == player.Team &&
                    XI.Board.Garden.Values.Any(p => p.IsTared && p.Uid != hvp.Farmer))
                {
                    return hvp.Pets.Any(p => py.Pets[
                        XI.LibTuple.ML.Decode(p).Element.Elem2Index()] != 0);
                }
            }
            else if (type == 1)
            {
                string[] g0zw = fuse.Split(',');
                for (int i = 1; i < g0zw.Length; ++i)
                {
                    ushort me = ushort.Parse(g0zw[i]);
                    Player py = XI.Board.Garden[me];
                    if (py.GetPetCount() > 0)
                        return true;
                }
            }
            return false;
        }
        public void JNH1502Action(Player player, int type, string fuse, string argst)
        {
            // PetFrom, Pet, PetGivenTo
            string[] args = argst.Split(',');
            ushort from = ushort.Parse(args[0]);
            ushort pet = ushort.Parse(args[1]);
            ushort to = ushort.Parse(args[2]);
            XI.RaiseGMessage(new Artiad.HarvestPet()
            {
                Farmer = to,
                Farmland = from,
                SinglePet = pet,
                TreatyAct = type == 0 ? Artiad.HarvestPet.Treaty.KOKAN : Artiad.HarvestPet.Treaty.ACTIVE
            }.ToMessage());
        }
        public string JNH1502Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                Artiad.HarvestPet hvp = Artiad.HarvestPet.Parse(fuse);
                Player py = XI.Board.Garden[hvp.Farmer];
                return "#交出宠物,/T1(p" + hvp.Farmer + "),/M1(p" + string.Join("p",
                    py.Pets.Where(p => p != 0)) + "),#获得,/T1" + AOthersTared(py);
            }
            else if (type == 1)
            {
                if (prev == "")
                {
                    List<ushort> invs = new List<ushort>();
                    string[] g0zw = fuse.Split(',');
                    for (int i = 1; i < g0zw.Length; ++i)
                    {
                        ushort me = ushort.Parse(g0zw[i]);
                        Player py = XI.Board.Garden[me];
                        if (py.GetPetCount() > 0)
                            invs.Add(me);
                    }
                    return "/T1(p" + string.Join("p", invs) + ")";
                }
                else if (prev.IndexOf(',') < 0)
                {
                    ushort from = ushort.Parse(prev);
                    Player py = XI.Board.Garden[from];
                    return "/M1(p" + string.Join("p", py.Pets.Where(p => p != 0)) + "),#获得,/T1" + AOthersTared(py);
                }
                else return "";
            }
            else return "";
        }
        #endregion HL015 - LiJianling
        #region HL016 - WangFeixia
        public bool JNH1601Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNH1601", player, fuse);
            else if (type == 1)
                return player.Guardian != 0;
            else if (type == 2)
                return XI.Board.Rounder.Team == player.Team && player.TokenExcl.Count > 0;
            else
                return false;
        }
        public void JNH1601Action(Player player, int type, string fuse, string argst)
        {
            IDictionary<ushort, ushort> im = new Dictionary<ushort, ushort>();
            IDictionary<ushort, ushort> jm = new Dictionary<ushort, ushort>();
            IDictionary<ushort, string> isk = new Dictionary<ushort, string>();
            System.Action<int, int, int> reg = (i, l, sk) =>
            {
                ushort ui = (ushort)i, ul = (ushort)l;
                im[ui] = ul; jm[ul] = ui; isk[ui] = string.Format("JNH16{0:D2}", sk);
            };
            reg(15, 10, 3); reg(16, 11, 4); reg(17, 12, 5); reg(18, 13, 6); reg(19, 14, 7);
            reg(26, 20, 8); reg(27, 21, 9);
            reg(20, 15, 10); reg(21, 16, 11); reg(22, 17, 12);

            if (type == 0)
            {
                string part = string.Join(",", im.Keys.Select(p => "I" + p));
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1," + im.Keys.Count + "," + part);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + part);
            }
            else if (type == 1 || type == 2)
            {
                ushort iNo = player.Guardian;
                if (iNo != 0)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                    if (jm.ContainsKey(iNo))
                        XI.RaiseGMessage("G0OS," + player.Uid + ",1," + isk[jm[iNo]]);
                }
                if (type == 2)
                {
                    ushort pick = ushort.Parse(argst);
                    if (im.ContainsKey(pick))
                    {
                        XI.RaiseGMessage("G0MA," + player.Uid + "," + im[pick]);
                        XI.RaiseGMessage("G0IS," + player.Uid + ",1," + isk[pick]);
                    }
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + pick);
                }
            }
        }
        public string JNH1601Input(Player player, int type, string fuse, string prev)
        {
            if (type == 2 && prev == "")
                return "/I1(p" + string.Join("p", player.TokenExcl.Select(p => p.Substring("I".Length))) + ")";
            else
                return "";
        }
        public bool JNH1602Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.ROM.GetInt("Holy") == 0)
            {
                var hl = XI.LibTuple.HL;
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0 &&
                    (p.Uid == player.Uid || hl.InstanceHero(p.SelectHero).Bio.Contains("H")));
            }
            else if (type == 1 && player.ROM.GetInt("Holy") == 2)
                return true;
            else if (type == 2 && IsMathISOS("JNH1602", player, fuse) && player.ROM.GetInt("Holy") == 2)
                return XI.Board.PoolEnabled;
            else if (type == 3 && player.ROM.GetInt("Holy") == 1)
                return XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Sum(p => p.GetPetCount()) >= 3;
            else if (type == 4 && player.ROM.GetInt("Holy") == 2)
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
                Cure(player, XI.Board.Garden[tg], 3);
                ushort guardian = player.Guardian;
                if (player.Guardian != 0)
                {
                    XI.RaiseGMessage("G0MA," + player.Uid + ",0");
                    int ki = guardian - 10 + 3;
                    string skillName = ki < 10 ? ("JNH160" + ki) : ("JNH16" + ki);
                    XI.RaiseGMessage("G0OS," + player.Uid + ",1," + skillName);
                }
                if (player.TokenExcl.Count > 0)
                {
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + player.TokenExcl.Count
                        + "," + string.Join(",", player.TokenExcl));
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", player.TokenExcl));
                }
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.DEX,
                    Who = player.Uid,
                    NewValue = 4
                }.ToMessage());
                if (player.DEXh < 4)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",3," + (4 - player.DEXh));
                else if (player.DEXh > 4)
                    XI.RaiseGMessage("G0OX," + player.Uid + ",3," + (player.DEXh - 4));
                player.ROM.Set("Holy", 2);
                if (XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Sum(p => p.GetPetCount()) >= 3)
                {
                    player.ROM.Set("Holy", 2);
                    if (XI.Board.PoolEnabled)
                        XI.RaiseGMessage("G0IP," + player.Team + ",2");
                }
                else
                    player.ROM.Set("Holy", 1);
                XI.InnerGMessage("G0ZH,0", 0);
            }
            else if (type == 1)
                XI.RaiseGMessage("G0IP," + player.Team + ",2");
            else if (type == 2)
                XI.RaiseGMessage("G0OP," + player.Team + ",2");
            else if (type == 3)
            {
                player.ROM.Set("Holy", 2);
                if (XI.Board.PoolEnabled)
                    XI.RaiseGMessage("G0IP," + player.Team + ",2");
            }
            else if (type == 4)
            {
                player.ROM.Set("Holy", 1);
                if (XI.Board.PoolEnabled)
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
            if (type == 0)
            {
                return Artiad.JoinPetEffects.Parse(fuse).List.Any(p => p.Pets.Any(q =>
                    XI.LibTuple.ML.Decode(q) != null && XI.LibTuple.ML.Decode(q).Element == elem));
            }
            else if (type == 1)
            {
                return Artiad.CollapsePetEffects.Parse(fuse).List.Any(p => p.Pets.Any(q =>
                    XI.LibTuple.ML.Decode(q) != null && XI.LibTuple.ML.Decode(q).Element == elem));
            }
            else if (type == 2 || type == 3)
            {
                int thisEle = elem.Elem2Index();
                int advEle = adv.Elem2Index();
                return IsMathISOS(skillName, player, fuse) && XI.Board.Garden.Values.
                    Any(p => p.IsAlive && (p.Pets[thisEle] != 0 || p.Pets[advEle] != 0));
            }
            else if (type == 4 || (type == 5 && XI.Board.IsMonsterDebut))
            {
                Monster fieldMon = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                return fieldMon != null && fieldMon.Element == disadv;
            }
            else if (type == 6)
            {
                Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
                return opt.Pets.Any(p => XI.LibTuple.ML.Decode(p).Element == adv);
            }
            else if (type == 7)
            {
                Artiad.LosePet lpt = Artiad.LosePet.Parse(fuse);
                return lpt.Pets.Any(p => XI.LibTuple.ML.Decode(p).Element == adv);
            }
            else
                return false;
        }
        private void JNH16SeriesAction(Player player, int type, string fuse,
            string skillName, FiveElement elem, FiveElement adv, FiveElement disadv)
        {
            if (type == 0)
            {
                Artiad.JoinPetEffects.Parse(fuse).List.ForEach(p =>
                {
                    int cnt = p.Pets.Count(q => XI.LibTuple.ML.Decode(q) != null &&
                        XI.LibTuple.ML.Decode(q).Element == elem);
                    if (cnt > 0)
                    {
                        XI.RaiseGMessage("G0IA," + p.Owner + ",0," + cnt);
                        XI.RaiseGMessage("G0IX," + p.Owner + ",0," + cnt);
                    }
                });
            }
            else if (type == 1)
            {
                Artiad.CollapsePetEffects.Parse(fuse).List.ForEach(p =>
                {
                    int cnt = p.Pets.Count(q => XI.LibTuple.ML.Decode(q) != null &&
                        XI.LibTuple.ML.Decode(q).Element == elem);
                    if (cnt > 0)
                    {
                        XI.RaiseGMessage("G0OA," + p.Owner + ",0," + cnt);
                        XI.RaiseGMessage("G0OX," + p.Owner + ",0," + cnt);
                    }
                });
            }
            else if (type == 2)
            {
                int thisEle = elem.Elem2Index();
                int advEle = adv.Elem2Index();
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
                    XI.RaiseGMessage(new Artiad.DisableItPetEffect() { Its = actionPets.ToArray() }.ToMessage());
            }
            else if (type == 3)
            {
                int thisEle = elem.Elem2Index();
                int advEle = adv.Elem2Index();
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
                    XI.RaiseGMessage(new Artiad.EnableItPetEffect() { Its = actionPets.ToArray() }.ToMessage());
            }
            else if (type == 4)
                XI.Board.IsMonsterDebut = true;
            else if (type == 5)
                XI.RaiseGMessage("G0IW," + XI.Board.Monster1 + ",1");
            else if (type == 6)
            {
                Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
                opt.Pets.Where(p => XI.LibTuple.ML.Decode(p).Element == adv)
                    .ToList().ForEach(p => XI.Board.NotActionPets.Add(p));
            }
            else if (type == 7)
            {
                Artiad.LosePet lpt = Artiad.LosePet.Parse(fuse);
                lpt.Pets.Where(p => XI.LibTuple.ML.Decode(p).Element == adv)
                    .ToList().ForEach(p => XI.Board.NotActionPets.Add(p));
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
        public bool JNH1608Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1608",
                FiveElement.YINN, FiveElement.SOLARIS, FiveElement.SOLARIS);
        }
        public void JNH1608Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1608",
                FiveElement.YINN, FiveElement.SOLARIS, FiveElement.SOLARIS);
        }
        public bool JNH1609Valid(Player player, int type, string fuse)
        {
            return JNH16SeriesValid(player, type, fuse, "JNH1609",
                FiveElement.SOLARIS, FiveElement.YINN, FiveElement.YINN);
        }
        public void JNH1609Action(Player player, int type, string fuse, string argst)
        {
            JNH16SeriesAction(player, type, fuse, "JNH1609",
                FiveElement.SOLARIS, FiveElement.YINN, FiveElement.YINN);
        }
        public bool JNH1610BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Tux.Count > 0)
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("ZP02");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH1610Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            ushort uid = player.Uid;
            XI.RaiseGMessage("G0CC," + uid + ",0," + uid + ",ZP02," + ut + ";0," + fuse);
            XI.RaiseGMessage("G0CZ,0," + player.Uid);
        }
        public string JNH1610Input(Player player, int type, string fuse, string prev)
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
        public bool JNH1611BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Tux.Count > 0 && ((type == 0 && XI.Board.Rounder.Uid == player.Uid) || type == 1))
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("TPT1");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH1611Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            ushort uid = player.Uid;
            XI.RaiseGMessage("G0CC," + uid + ",0," + uid + ",TPT1," + ut + ";" + type + "," + fuse);
        }
        public string JNH1611Input(Player player, int type, string fuse, string prev)
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
        public bool JNH1612BKValid(Player player, int type, string fuse, ushort owner)
        {
            if (player.Uid == XI.Board.Rounder.Uid && player.Tux.Count > 0)
            {
                Tux copiee = XI.LibTuple.TL.EncodeTuxCode("JP03");
                return copiee.Bribe(player, type, fuse) && copiee.Valid(player, type, fuse);
            }
            else
                return false;
        }
        public void JNH1612Action(Player player, int type, string fuse, string argst)
        {
            ushort ut = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid + ",JP03," + ut + ";0," + fuse);
        }
        public string JNH1612Input(Player player, int type, string fuse, string prev)
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
            return player.Tux.Count >= 2 && XI.Board.Garden.Values.Count(p =>
                p.Uid != player.Uid && p.IsTared) >= 2;
        }
        public void JNH1701Action(Player player, int type, string fuse, string argst)
        {
            ushort[] args = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + args[0] + "," + args[1]);
            ushort[] tarsRaw = Algo.TakeRange(args, 2, args.Length);
            TargetPlayer(player.Uid, tarsRaw);
            int mask = 0;
            for (int i = 0; i < 2; ++i)
            {
                Base.Card.Tux tx = XI.LibTuple.TL.DecodeTux(args[i]);
                if (tx.Type == Tux.TuxType.JP)
                    mask |= 0x1;
                else if (tx.Type == Tux.TuxType.TP)
                    mask |= 0x2;
                else if (tx.Type == Tux.TuxType.ZP)
                    mask |= 0x4;
                else if (tx.IsTuxEqiup())
                    mask |= 0x8;
            }
            List<ushort> tars = XI.Board.ReOrderedPlayers(tarsRaw);
            bool anyResponse = false;
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
                string hints = uttux.Count > 0 ? "#弃置(否则HP-1),/Q1(p" +
                    string.Join("p", uttux) + ")" : "/";
                string input = XI.AsyncInput(tar, hints, "JNH1701", "0");
                if (!input.StartsWith("/") && input != VI.CinSentinel)
                {
                    XI.RaiseGMessage("G0QZ," + tar + "," + input);
                    anyResponse = true; break;
                }
            }
            if (!anyResponse)
                Harm(player, tars.Select(p => XI.Board.Garden[p]), 1, FiveElement.AQUA);
        }
        public string JNH1701Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#弃置,/Q2(p" + string.Join("p", player.Tux) + "),/T2" + AOthersTared(player);
            //{
            //    if (player.Tux.Count > 1)
            //        return "#弃置,/Q1~2(p" + string.Join("p", player.Tux) + "),!0";
            //    else
            //        return "#弃置,/Q1(p" + player.Tux[0] + "),!0";
            //}
            //else if (prev.EndsWith(",0"))
            //{
            //    string[] blocks = prev.Split(',');
            //    int tarsz = blocks.Length - 1;
            //    List<ushort> cands = XI.Board.Garden.Values.Where(p => p.IsTared &&
            //        p.Team == player.OppTeam).Select(p => p.Uid).ToList();
            //    if (tarsz > 1)
            //        return "/T1~" + tarsz + "(p" + string.Join("p", cands) + ")";
            //    else
            //        return "/T1(p" + string.Join("p", cands) + ")";
            //}
            else
                return "";
        }
        public bool JNH1702Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => p.N == 1 && !HPEvoMask.TERMIN_AT.IsSet(p.Mask));
        }
        public void JNH1702Action(Player player, int type, string fuse, string argst)
        {
            ISet<Player> normals = new HashSet<Player>();
            //ISet<Player> roars = new HashSet<Player>();
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (harm.N == 1 && !HPEvoMask.TERMIN_AT.IsSet(harm.Mask) && py.Tux.Count > 0)
                    normals.Add(py);
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
                    TargetPlayer(player.Uid, pys.Select(p => p.Uid));
                    foreach (Player py in pys)
                    {
                        if (py.Uid != player.Uid)
                        {
                            XI.AsyncInput(player.Uid, string.Format("#弃置{0},C1({1})", XI.DisplayPlayer(py.Uid),
                                Algo.RepeatString("p0", py.Tux.Count)), "JNH1702", "1");
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
            Harm(player, player, hp, FiveElement.A, (long)HPEvoMask.TERMIN_AT);
            Artiad.Procedure.ArticuloMortis(XI, XI.WI, false);
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
                if (player.ROM.GetInt("Destroy") == 0)
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
                XI.LibTuple.TL.EncodeTuxCode("TPT3").Valid(player, type, fuse) &&
                (!player.IsSKOpt || player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Code == tuxCode));
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
            if (type == 0)
            {
                if (player.ROM.GetInt("Destroy") == 0 && XI.Board.MonPiles.Count > 0)
                {
                    string[] g0hz = fuse.Split(',');
                    ushort who = ushort.Parse(g0hz[1]);
                    ushort mon = ushort.Parse(g0hz[2]);
                    return XI.Board.Garden[who].Team == player.Team && mon != 0;
                }
                return false;
            }
            else if (type == 1 || type == 2)
            {
                int destroy = player.ROM.GetInt("Destroy");
                return destroy == 1 || destroy == 2;
            }
            else
                return false;
        }
        public void JNH1803Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string[] g0hz = fuse.Split(',');
                int monCnt = XI.Board.MonPiles.Count;
                if (monCnt > 3) monCnt = 3;
                List<ushort> mons = XI.Board.MonPiles.Dequeue(monCnt).ToList();
                XI.RaiseGMessage("G2IN,1," + monCnt);
                // show the monsters to the trigger
                XI.RaiseGMessage(new Artiad.ImperialCentre()
                {
                    Genre = Card.Genre.NMB,
                    Cards = mons.ToArray()
                }.ToMessage());

                ushort newTangle = 0, bomb = 0, owner = 0;
                do
                {
                    string ans1st = XI.AsyncInput(player.Uid, "#更改为混战,/M1(p" +
                        string.Join("p", mons) + ")", "JNH1803", "0");
                    if (ans1st.StartsWith("/"))
                        break;
                    if (ans1st != VI.CinSentinel)
                    {
                        newTangle = ushort.Parse(ans1st);
                        if (Base.Card.NMBLib.IsMonster(newTangle))
                        {
                            Monster tangle = XI.LibTuple.ML.Decode(newTangle);
                            int elemIdx = tangle.Element.Elem2Index();
                            List<ushort> bombs = XI.Board.Garden.Values.Where(p => p.Team == player.Team &&
                                p.IsAlive).Select(p => p.Pets[elemIdx]).Where(p => p != 0).ToList();
                            string ans2nd;
                            if (bombs.Count > 0)
                                ans2nd = XI.AsyncInput(player.Uid, "#爆发,/M1(p" +
                                    string.Join("p", bombs) + ")", "JNH1803", "1");
                            else
                                ans2nd = XI.AsyncInput(player.Uid, "/", "JNH1803", "1");
                            if (ans2nd.StartsWith("/"))
                                break;
                            else if (ans2nd != VI.CinSentinel)
                            {
                                bomb = ushort.Parse(ans2nd);
                                owner = XI.Board.Garden.Values.Single(p => p.Pets[elemIdx] == bomb).Uid;
                                break;
                            }
                        }
                        else
                            break;
                    }
                } while (true);
                player.ROM.Set("Destroy", 1);
                if (bomb != 0)
                {
                    XI.RaiseGMessage("G0HI," + owner + "," + bomb);
                    player.ROM.Set("Destroy", 2);
                }
                if (newTangle != 0)
                {
                    mons.Remove(newTangle); mons.Add(XI.Board.Monster2);
                    XI.RaiseGMessage("G0WB," + XI.Board.Monster2);
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit() { Cards = mons.ToArray() }
                    }.ToMessage());
                    XI.Board.Monster2 = newTangle;

                    ushort who = ushort.Parse(g0hz[1]);
                    if (NMBLib.IsMonster(newTangle) || NMBLib.IsNPC(newTangle))
                        XI.WI.BCast("E0HZ,1," + who + "," + newTangle);
                    XI.RaiseGMessage(new Artiad.ImperialLeft()
                    {
                        Zone = Artiad.ImperialLeft.ZoneType.M2,
                        Trigger = who,
                        Card = newTangle
                    }.ToMessage());
                    XI.InnerGMessage("G0HZ," + who + "," + XI.Board.Monster2, 221);
                }
                else
                {
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit() { Cards = mons.ToArray() }
                    }.ToMessage());
                    XI.InnerGMessage(fuse, 221);
                }
            }
            else if (type == 1)
            {
                ushort mon = XI.Board.Monster2;
                if (NMBLib.IsMonster(mon) && player.ROM.GetInt("Destroy") == 2)
                {
                    XI.RaiseGMessage("G0IP," + player.Team + "," + XI.LibTuple.ML.Decode(mon).STR);
                    XI.InnerGMessage(fuse, 301);
                }
                else
                    XI.InnerGMessage(fuse, 291);
            }
            else if (type == 2)
            {
                XI.RaiseGMessage(new Artiad.DisablePlayerPetEffect() { SingleWho = player.Uid }.ToMessage());
                player.ROM.Set("Destroy", 3);
            }
        }
        #endregion HL018 - Xu'Nansong
        #region HL019 - Kongxiu
        public bool JNH1901Valid(Player player, int type, string fuse)
        {
            if (type == 0) // Gain the Token in 5'
                return IsMathISOS("JNH1901", player, fuse);
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
            var b = XI.Board;
            return player.TokenCount > 0 && b.IsAttendWar(player) && b.Garden.Values.Any(p => p.IsReal &&
                p.Gender == 'F' && p.Uid != player.Uid && p.Uid != b.Rounder.Uid && b.IsAttendWar(p));
        }
        public void JNH1902Action(Player player, int type, string fuse, string argst)
        {
            var b = XI.Board;
            List<ushort> invs = b.Garden.Values.Where(p => p.IsReal && p.Gender == 'F' && p.Uid != player.Uid &&
                p.Uid != b.Rounder.Uid && b.IsAttendWar(p)).Select(p => p.Uid).ToList();
            foreach (ushort ut in XI.Board.OrderedPlayer())
            {
                if (invs.Contains(ut))
                {
                    TargetPlayer(player.Uid, ut);
                    XI.RaiseGMessage("G0TT," + ut);
                    int value = b.DiceValue;
                    if (value == 1 || value == 2)
                    {
                        if (b.Garden[ut] == b.Supporter)
                            XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                            {
                                Role = Artiad.CoachingHelper.PType.SUPPORTER, Coach = 0
                            } }.ToMessage());
                        else if (b.Garden[ut] == b.Hinder)
                            XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
                            {
                                Role = Artiad.CoachingHelper.PType.HINDER, Coach = 0
                            } }.ToMessage());
                    }
                    else if (value == 5 || value == 6)
                        XI.RaiseGMessage("G0OJ," + player.Uid + ",0,1");
                }
            }
        }
        public bool JNH1903Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return (XI.Board.Supporter.Uid == player.Uid || XI.Board.Hinder.Uid == player.Uid) &&
                    player.TokenCount > 0 && XI.Board.Garden.Values.Any(p => p.IsTared &&
                    p.Team == player.OppTeam && XI.Board.IsAttendWar(p) && p.Gender == 'M');
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
                TargetPlayer(player.Uid, tar);
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
                return "/T1" + FormatPlayers(p => p.IsTared && p.Team == player.OppTeam &&
                    XI.Board.IsAttendWar(p) && p.Gender == 'M');
            }
            else return "";
        }
        #endregion HL019 - Kongxiu
        #region HL020 - Mojianke
        public bool JNH2001Valid(Player player, int type, string fuse)
        {
            if (!player.RFM.GetBool("InJNH2002"))
            {
                ushort who = 0;
                if (type == 0) // ZB
                    who = Artiad.ClothingHelper.GetWho(fuse);
                else if (type == 1) // HD
                    who = Artiad.ObtainPet.Parse(fuse).Farmer;
                else if (type == 2) // IF
                {
                    string[] g0ifs = fuse.Split(',');
                    who = ushort.Parse(g0ifs[1]);
                }
                return who != 0 && XI.Board.Garden[who].Team == player.OppTeam;
            }
            return false;
        }
        public void JNH2001Action(Player player, int type, string fuse, string argst)
        {
            ushort who = 0; int n = 0;
            if (type == 0) // ZB
            {
                who = Artiad.ClothingHelper.GetWho(fuse);
                n = 1;
            }
            else if (type == 1) // HD
            {
                who = Artiad.ObtainPet.Parse(fuse).Farmer;
                n = 1;
            }
            else if (type == 2) // IF
            {
                string[] g0ifs = fuse.Split(',');
                who = ushort.Parse(g0ifs[1]);
                n = g0ifs.Length - 2;
            }
            for (int i = 0; i < n; ++i)
            {
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
                string select = XI.AsyncInput(player.Uid, "#置为「魔灵」,Q1(p" +
                    string.Join("p", player.Tux) + ")", "JNH2001", "0");
                if (select != VI.CinSentinel)
                {
                    ushort ut = ushort.Parse(select);
                    XI.RaiseGMessage("G0OT," + player.Uid + ",1," + ut);
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",4,1," + ut);
                }
            }
        }
        public bool JNH2002Valid(Player player, int type, string fuse)
        {
            if (type == 0 && player.TokenFold.Count >= 1)
                return player.RestZP <= 0 && (player.Team == XI.Board.Rounder.Team ? XI.Board.CalculateRPool() : XI.Board.CalculateOPool()) >= 2;
            else if (type == 1 && player.TokenFold.Count >= 2)
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.GetBaseEquipCount() > 0);
            else if (type == 2 && player.TokenFold.Count >= 3)
                return XI.Board.Rounder.Team == player.Team && XI.Board.MonDises.Count > 0;
            else if (type == 3 && player.TokenFold.Count >= 4)
                return true;
            else if (type == 4 && player.TokenFold.Count >= 5)
                return XI.Board.Garden.Values.Any(p => p.IsTared && (p.Team == player.OppTeam || p.Runes.Count > 0));
            else
                return false;
        }
        public void JNH2002Action(Player player, int type, string fuse, string argst)
        {
            string[] args = argst.Split(',');
            ushort[] uargs = args.Select(p => ushort.Parse(p)).ToArray();
            player.RFM.Set("InJNH2002", true);
            if (type == 0)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,1," + argst);
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, Cards = uargs }
                }.ToMessage());
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
                ushort[] utokens = Algo.TakeRange(uargs, 2, uargs.Length);
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,2," + string.Join(",", utokens));
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, Cards = utokens }
                }.ToMessage());
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", utokens.Select(p => "C" + p)));
                XI.RaiseGMessage("G0HQ,0," + who + "," + who + ",0,1," + card);
            }
            else if (type == 2)
            {
                ushort[] utokens = Algo.TakeRange(uargs, 1, uargs.Length);
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,3," + string.Join(",", utokens));
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, Cards = utokens }
                }.ToMessage());
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", utokens.Select(p => "C" + p)));
                ushort pop = ushort.Parse(args[0]);
                XI.Board.MonDises.Remove(pop);
                XI.RaiseGMessage("G2CN,1,1");
                XI.RaiseGMessage(new Artiad.ImperialRight()
                {
                    Encrypted = false,
                    Genre = Card.Genre.NMB,
                    SingleItem = new Artiad.ImperialRightUnit() { Source = 0, SingleCard = pop }
                }.ToMessage());
            }
            else if (type == 3)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,4," + argst);
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, Cards = uargs }
                }.ToMessage());
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", args.Select(p => "C" + p)));
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
                XI.RaiseGMessage("G0IF," + player.Uid + ",2");
            }
            else if (type == 4)
            {
                XI.RaiseGMessage("G0OJ," + player.Uid + ",4,5," + argst);
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, Cards = uargs }
                }.ToMessage());
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", args.Select(p => "C" + p)));
                int total = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam)
                    .Sum(p => p.GetEquipCount() + p.Runes.Count);
                while (total > 0)
                {
                    string selTarget = XI.AsyncInput(player.Uid, "#(剩余" + total + "枚),/T1" + FormatPlayers(p =>
                        p.IsTared && (p.Team == player.OppTeam || p.Runes.Count > 0)), "JNH2002", "0");
                    if (selTarget.StartsWith("/") || selTarget == VI.CinSentinel)
                        break;
                    ushort target = ushort.Parse(selTarget);
                    TargetPlayer(player.Uid, target);
                    Player py = XI.Board.Garden[target];
                    bool bmins = py.Runes.Count > 0;
                    bool bplus = py.Team == player.OppTeam;
                    string msg = "#请选择" + XI.DisplayPlayer(target) + "执行项";
                    if (bmins)
                        msg += "##弃置标记";
                    if (bplus)
                        msg += "##获得标记";
                    msg += (bmins && bplus) ? ",Y2" : ",Y1";
                    string selAction = XI.AsyncInput(player.Uid, msg, "JNH2002", "1");
                    if (selAction == "1" && bmins)
                    {
                        int upLimit = System.Math.Min(py.Runes.Count, total);
                        string selDis = XI.AsyncInput(player.Uid, "#弃置的,/F1" + (upLimit > 1 ?
                            ("~" + upLimit) : "") + "(p" + string.Join("p", py.Runes) + ")", "JNH2002", "2");
                        if (!selDis.StartsWith("/") && selDis != VI.CinSentinel)
                        {
                            string[] runes = selDis.Split(',');
                            XI.RaiseGMessage("G0OF," + target + "," + selDis);
                            total -= runes.Length;
                        }
                    }
                    else
                    {
                        ushort[] uts = XI.LibTuple.RL.GetFullAppendableList();
                        int upLimit = System.Math.Min(uts.Length, total);
                        string selDis = XI.AsyncInput(player.Uid, "#获得的,/F1" + (upLimit > 1 ?
                            ("~" + upLimit) : "") + "(p" + string.Join("p", uts) + ")", "JNH2002", "3");
                        if (!selDis.StartsWith("/") && selDis != VI.CinSentinel)
                        {
                            string[] runes = selDis.Split(',');
                            XI.RaiseGMessage("G0IF," + target + "," + selDis);
                            total -= runes.Length;
                        }
                    }
                }
            }
            player.RFM.Set("InJNH2002", false);
        }
        public string JNH2002Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "#弃置魔灵,/C1(p" + string.Join("p", player.TokenFold) + ")";
            else if (type == 1)
            {
                if (prev == "")
                    return "/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p =>
                        p.IsTared && p.GetEquipCount() > 0).Select(p => p.Uid)) + ")";
                else if (prev.IndexOf(',') < 0)
                {
                    ushort who = ushort.Parse(prev);
                    return "/" + (who == player.Uid ? "Q" : "C") + "1(p" +
                        string.Join("p", XI.Board.Garden[who].ListOutAllBaseEquip()) +
                        "),#弃置魔灵,/C2(p" + string.Join("p", player.TokenFold) + ")";
                }
            }
            else if (type == 2 && prev == "")
                return "/M1(p" + XI.Board.MonDises.Last() + "),#弃置魔灵,/C3(p" + string.Join("p", player.TokenFold) + ")";
            else if (type == 3 && prev == "")
                return "#弃置魔灵,/C4(p" + string.Join("p", player.TokenFold) + ")";
            else if (type == 4 && prev == "")
                return "#弃置魔灵,/C5(p" + string.Join("p", player.TokenFold) + ")";
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
                int diff1 = XI.Board.PoolDelta;
                FiveElement[] props = FiveElementHelper.GetPropedElements();
                int opr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.OppTeam).Any(r => r.Pets[q.Elem2Index()] != 0));
                int rpr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == player.Team).Any(r => r.Pets[q.Elem2Index()] != 0));
                return diff1 >= 2 && opr > rpr;
            }
            else
                return false;
        }
        public void JNH2103Action(Player player, int type, string fuse, string argst)
        {
            int diff1 = XI.Board.PoolDelta;
            FiveElement[] props = FiveElementHelper.GetPropedElements();
            int opr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.OppTeam).Any(r => r.Pets[q.Elem2Index()] != 0));
            int rpr = props.Count(q => XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.Team).Any(r => r.Pets[q.Elem2Index()] != 0));
            int diff = System.Math.Min(diff1, opr - rpr);
            string rSel = XI.AsyncInput(player.Uid, "#补" + diff + "张牌,T1" +
                ATeammatesTared(player), "JNH2103", "0");
            ushort rTar = ushort.Parse(rSel);
            XI.RaiseGMessage("G0DH," + rTar + ",0," + diff);
        }
        #endregion HL021 - MurongKe

        #region E3W07 - Sitang
        public bool JNE0101Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Rounder.IsAlive && XI.Board.Rounder.Tux.Count == 0;
            else if (type == 1)
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Tux.Count == 0);
            else
                return false;
        }
        public void JNE0101Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                Harm(player, XI.Board.Rounder, 1);
            else if (type == 1)
            {
                string sel = XI.AsyncInput(player.Uid, "#HP-1,T1" + FormatPlayers(p =>
                    p.IsTared && p.Tux.Count == 0), "JNE0101", "0");
                ushort who = ushort.Parse(sel);
                Harm(player, XI.Board.Garden[who], 1);
            }
        }
        public bool JNE0102Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return Artiad.Cure.Parse(fuse).Any(p => p.N > 0 && p.Who == XI.Board.Rounder.Uid);
            else if (type == 1)
                return XI.Board.Garden.Values.Any(p => p.HP == 0 && p.IsAlive &&
                    !player.RFM.GetUshortArray("ICU").Contains(p.Uid));
            else
                return false;
        }
        public void JNE0102Action(Player player, int type, string fuse)
        {
            string sel = XI.AsyncInput(player.Uid, "#补牌,T1" + AAllTareds(player), "JNE0102", "0");
            ushort who = ushort.Parse(sel);
            XI.RaiseGMessage("G0DH," + who + ",0,1");
            if (type == 1)
            {
                player.RFM.GetUshortArray("ICU").Add(XI.Board.Garden.Values.First(p => p.HP == 0 &&
                    p.IsAlive && !player.RFM.GetUshortArray("ICU").Contains(p.Uid)).Uid);
            }
        }
        #endregion E3W07 - Sitang
        #region EX301 - JingTian
        public bool JNE0201Valid(Player player, int type, string fuse)
        {
            var v = XI.Board.Garden.Values;
            return player.Tux.Count > 0 && v.Any(p => p.IsTared);
        }
        public void JNE0201Action(Player player, int type, string fuse, string argst)
        {
            ushort[] part = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
            XI.RaiseGMessage("G0QZ," + player.Uid + "," + part[0]);
            ushort to = part[2];
            TargetPlayer(player.Uid, to);
            if (part[1] == 1) // immoraze
                XI.RaiseGMessage("G0DS," + to + ",1");
            else if (part[1] == 2)
                XI.RaiseGMessage("G0OF," + to + "," + part[3]);
        }
        public string JNE0201Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> tuxes = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.ZP).ToList();
                return tuxes.Count == 0 ? "/" : "/Q1(p" + string.Join("p", tuxes) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                if (XI.Board.Garden.Values.Any(p => p.IsTared && p.Runes.Contains(5) || p.Runes.Contains(6)))
                    return "#请选择『百无禁忌』执行项##解除横置##弃置标记,/Y2";
                else
                    return "#请选择『百无禁忌』执行项##解除横置,/Y1";
            }
            else
            {
                string[] parts = prev.Split(',');
                if (parts.Length == 2 && parts[1] == "1")
                    return "/T1" + AAllTareds(player);
                else if (parts[1] == "2")
                {
                    if (parts.Length == 2)
                        return "/T1" + FormatPlayers(p => p.IsTared && p.Runes.Contains(5) || p.Runes.Contains(6));
                    else if (parts.Length == 3)
                    {
                        ushort who = ushort.Parse(parts[2]);
                        List<ushort> runes = new ushort[] { 5, 6 }.Intersect(XI.Board.Garden[who].Runes).ToList();
                        return "/F1(p" + string.Join("p", runes) + ")";
                    }
                }
                return "";
            }
        }
        public bool JNE0202Valid(Player player, int type, string fuse)
        {
            if (XI.Board.Rounder.Team != player.OppTeam)
                return false;
            if (XI.Board.RoundIN != "R" + XI.Board.Rounder.Uid + "GR")
                return false;
            if (type == 0) // G1DI
            {
                string[] g1di = fuse.Split(',');
                for (int i = 1; i < g1di.Length;)
                {
                    ushort me = ushort.Parse(g1di[i]);
                    ushort lose = ushort.Parse(g1di[i + 1]);
                    if (lose == 0) // Get Card
                    {
                        if (XI.Board.Garden[me].Team == player.OppTeam)
                        {
                            int n = int.Parse(g1di[i + 2]);
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
                return Artiad.Abandon.Parse(fuse).Genre == Card.Genre.Tux;
            else return false;
        }
        public void JNE0202Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                List<ushort> uts = XI.Board.TuxDises.Intersect(
                    player.RAM.GetOrSetUshortArray("pledge")).ToList();
                if (uts.Count > 0)
                {
                    string second = XI.AsyncInput(player.Uid, "#请选择『蜀中巨富』获得牌,C1(p" +
                        string.Join("p", uts) + ")", "JNE0202", "0");
                    if (second != VI.CinSentinel)
                    {
                        ushort ut = ushort.Parse(second);
                        XI.RaiseGMessage("G2CN,0,1");
                        XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + ut);
                        XI.Board.TuxDises.Remove(ut);
                    }
                }
                else
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
            else if (type == 1)
            {
                player.RAM.GetOrSetUshortArray("pledge").AddRange(
                    Artiad.Abandon.Parse(fuse).List.SelectMany(p => p.Cards));
            }
        }
        public bool JNE0203Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Team == XI.Board.Rounder.Team && XI.Board.IsAttendWar(player) &&
                    XI.Board.IsBattleWin && XI.Board.PoolDelta >= 2 && player.Tux.Count > 0;
            else if (type == 1)
                return player.ExCards.Count > 0;
            else
                return false;
        }
        public void JNE0203Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort[] cards = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage(new Artiad.EquipExCards()
                {
                    Who = player.Uid,
                    Source = player.Uid,
                    Cards = cards
                }.ToMessage());
            }
            else if (type == 1)
            {
                ushort[] cards = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
                XI.RaiseGMessage("G0IP," + player.Team + "," + (2 * cards.Length));
            }
        }
        public string JNE0203Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<ushort> txs = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()).ToList();
                int n = System.Math.Min(XI.Board.PoolDelta / 2, txs.Count);
                if (n == 0)
                    return "/";
                else if (n == 1)
                    return "#装备的,/Q1(p" + string.Join("p", txs) + ")";
                else
                    return "#装备的,/Q1~" + n + "(p" + string.Join("p", txs) + ")";
            }
            else if (type == 1 && prev == "")
                return "/Q1(p" + string.Join("p", player.ExCards) + ")";
            else
                return "";
        }
        #endregion EX301 - JingTian
        #region EX408 - Chanyou
        public bool JNE0301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNE0301", player, fuse);
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
            { 
                return "#请选择调整怪物闪避方式.##闪避增加##闪避减少,/Y2,#弃置,/C1" + (player.TokenExcl.Count > 1 ?
                    ("~" + player.TokenExcl.Count) : "") + "(p" + string.Join("p", player.TokenExcl.Select(
                    p => p.Substring("C".Length))) + ")";
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
                {
                    XI.RaiseGMessage(new Artiad.InnateChange()
                    {
                        Item = Artiad.InnateChange.Prop.HP,
                        Who = player.Uid,
                        NewValue = player.HPb - 2
                    }.ToMessage());
                }
                XI.RaiseGMessage("G0ZW," + tar);
            }
            else
            {
                List<ushort> vals = py.Tux.Except(XI.Board.ProtectedTux).ToList();
                if (tar != player.Uid)
                {
                    string c0 = Algo.RepeatString("p0", vals.Count);
                    XI.AsyncInput(player.Uid, "#作为「幻」的,C1(" + c0 + ")", "JNE0303", "1");
                    vals.Shuffle();
                    XI.RaiseGMessage("G0OT," + tar + ",1," + vals[0]);
                    XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,C" + vals[0]);
                }
                else
                {
                    string select = XI.AsyncInput(player.Uid, "#作为「幻」的,Q1(p" +
                        string.Join("p", player.Tux) + ")", "JNE0303", "1");
                    if (select != VI.CinSentinel)
                    {
                        ushort card = ushort.Parse(select);
                        XI.RaiseGMessage("G0OT," + tar + ",1," + card);
                        XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,C" + card);
                    }
                }
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.HP,
                    Who = py.Uid,
                    NewValue = py.HPb - 2
                }.ToMessage());
                if (py.IsAlive)
                {
                    if (py.HP < player.HPb)
                        Cure(player, py, (player.HPb - py.HP), FiveElement.A, (long)HPEvoMask.TERMIN_AT);
                    else if (py.HP > player.HPb)
                        Harm(player, py, (py.HP - player.HPb), FiveElement.A, (long)HPEvoMask.TERMIN_AT);
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
            else if (blocks[1] == "4")
            {
                string me = player.Uid.ToString();
                if (blocks[2] == me && blocks[4] != "0")
                    return XI.Board.Garden[ushort.Parse(blocks[3])].Team == player.OppTeam;
                if (blocks[3] == me && blocks[5] != "0")
                    return XI.Board.Garden[ushort.Parse(blocks[2])].Team == player.OppTeam;
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
                string c0 = Algo.RepeatString("p0", XI.Board.Garden[from].Tux.Count);
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
                        string c0 = Algo.RepeatString("p0", py.Tux.Count);
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
        #region EX515 - Qingshi
        public bool JNE0501Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsMonsterDebut && XI.Board.IsAttendWar(player);
        }
        public void JNE0501Action(Player player, int type, string fuse, string argst)
        {
            string select = XI.AsyncInput(player.Uid, "#请选择『洞察』执行项##战力+2##出场无效,Y2",
                "JNE0501", "0");
            XI.SendOutUAMessage(player.Uid, "JNE0501," + select, "0");
            if (select == "2")
            {
                XI.Board.IsMonsterDebut = false;
                XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,1");
            }
            else
            {
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
                XI.RaiseGMessage("G2YS,M,1,T, " + player.Uid);
            }
        }
        public bool JNE0502Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return player.Tux.Count > 0 && !XI.Board.IsAttendWar(player);
            else if (type == 1)
                return player.TokenAwake && fuse.Split(',').Contains("L");
            else if (type == 2)
                return player.TokenAwake;
            else
                return false;
        }
        public void JNE0502Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + argst);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",3");
            }
            else if (type == 1)
            {
                string[] g1ge = fuse.Split(',');
                for (int i = 1; i < g1ge.Length; i += 2)
                {
                    bool? isWin = null;
                    if (g1ge[i] == "W")
                        isWin = true;
                    else if (g1ge[i] == "L")
                        isWin = false;
                    ushort monCode = ushort.Parse(g1ge[i + 1]);
                    Monster monster = XI.LibTuple.ML.Decode(monCode);
                    if (monster != null)
                    {
                        if (isWin == true)
                            monster.WinEff();
                        else if (isWin == false)
                        {
                            int elem = monster.Element.Elem2Index();
                            List<Player> others = XI.Board.Garden.Values.Where(p =>
                                p.Team == XI.Board.Rounder.OppTeam && p.Pets[elem] != 0).ToList();
                            if (others.Any() && XI.Board.Mon1From == 0)
                            {
                                string input = XI.AsyncInput(XI.Board.Opponent.Uid, "#要替换的,/M1(p" + string.Join("p",
                                    others.Select(p => p.Pets[elem])) + ")", "JNE0502", "0");
                                if (input != VI.CinSentinel && !input.StartsWith("/"))
                                {
                                    ushort mons = ushort.Parse(input);
                                    Player py = others.Single(p => p.Pets[elem] == mons);
                                    XI.RaiseGMessage(new Artiad.HarvestPet()
                                    {
                                        Farmer = py.Uid,
                                        SinglePet = monCode,
                                        Trophy = true,
                                        TreatyAct = Artiad.HarvestPet.Treaty.PASSIVE
                                    }.ToMessage());
                                    if (XI.Board.Monster1 == monCode)
                                        XI.Board.Monster1 = 0;
                                    else if (XI.Board.Monster2 == monCode)
                                        XI.Board.Monster2 = 0;
                                }
                            }
                        }
                    }
                }
            }
            else if (type == 2)
                XI.RaiseGMessage("G0OJ," + player.Uid + ",3");
        }
        public string JNE0502Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
            {
                List<ushort> tps = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p) != null &&
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP).ToList();
                return tps.Count > 0 ? "#弃置,/Q1(p" + string.Join("p", tps) + ")" : "/";
            }
            else
                return "";
        }
        #endregion EX515 - Qingshi
        #region EX119 - Jisanniang
        public bool JNE0701Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWarSucc(player) && XI.Board.Garden.Values.Any(p =>
                p.Team == player.OppTeam && p.IsTared && XI.Board.IsAttendWar(p) && p.Tux.Count > 0);
        }
        public void JNE0701Action(Player player, int type, string fuse, string argst)
        {
            string whoStr = XI.AsyncInput(player.Uid, "#弃牌,T1" + FormatPlayers(p => p.Team == player.OppTeam &&
                p.IsTared && XI.Board.IsAttendWar(p) && p.Tux.Count > 0), "JNE0701", "0");
            ushort who = ushort.Parse(whoStr);
            TargetPlayer(player.Uid, who);
            string c0 = Algo.RepeatString("p0", XI.Board.Garden[who].Tux.Count);
            XI.AsyncInput(player.Uid, "#获得的,C1(" + c0 + ")", "JNE0701", "1");
            XI.RaiseGMessage("G0DH," + who + ",2,1");
        }
        public bool JNE0702Valid(Player player, int type, string fuse)
        {
            Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
            return ab.Genre == Card.Genre.NMB && ab.List.Any(p => p.Cards.Any(q => NMBLib.IsMonster(q)));
        }
        public void JNE0702Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        #endregion EX119 - Jisanniang
        #region EX312 - ZhaoWuyan
        public bool JNE0801Valid(Player player, int type, string fuse)
        {
            if (type == 0) // G0IS/OS
                return IsMathISOS("JNE0801", player, fuse);
            else if (type == 1)
                return IsMathISOS("JNE0801", player, fuse) && player.TokenExcl.Count > 0;
            else if (type == 2)
            {
                Artiad.InnateChange ic = Artiad.InnateChange.Parse(fuse);
                return ic.Item == Artiad.InnateChange.Prop.HP && ic.Who == player.Uid &&
                    ic.NewValue != player.TokenExcl.Count;
            }
            else return false;
        }
        public void JNE0801Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort[] pops = XI.Board.RestNPCPiles.Dequeue(5);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,5," + string.Join(",", pops.Select(p => "M" + p)));
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + string.Join(",", pops.Select(p => "M" + p)));
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.HP,
                    Who = player.Uid,
                    NewValue = 5
                }.ToMessage());
                Cure(player, player, 5);
                int female = pops.Count(p => XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(p)).Gender == 'F');
                if (female > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0," + female);
            }
            else if (type == 1)
            {
                //int all = player.TokenExcl.Count;
                //XI.RaiseGMessage(new Artiad.InnateChange()
                //{
                //    Item = Artiad.InnateChange.Prop.HP,
                //    Who = player.Uid,
                //    NewValue = player.HPb - all
                //}.ToMessage());
                if (player.IsAlive)
                {
                    int female = player.TokenExcl.Count(p => XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(
                        ushort.Parse(p.Substring("M".Length)))).Gender == 'F');
                    if (female > 0)
                        XI.RaiseGMessage("G0OA," + player.Uid + ",0," + female);
                }
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", player.TokenExcl));
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + Algo.ListToString(player.TokenExcl));
            }
            else if (type == 2)
            {
                if (player.HPb != player.TokenExcl.Count)
                {
                    XI.RaiseGMessage(new Artiad.InnateChange()
                    {
                        Item = Artiad.InnateChange.Prop.HP,
                        Who = player.Uid,
                        NewValue = player.TokenExcl.Count
                    }.ToMessage());
                }
            }
        }
        public bool JNE0802Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => XI.Board.Garden[p.Who].IsAlive &&
                XI.Board.Garden[p.Who].Gender == 'F' && p.N > 0 && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask));
        }
        public void JNE0802Action(Player player, int type, string fuse, string argst)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<ushort> invs = harms.Where(p => XI.Board.Garden[p.Who].Gender == 'F' &&
                p.N > 0 && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask)).Select(p => p.Who).Distinct().ToList();
            TargetPlayer(player.Uid, invs);
            string select = XI.AsyncInput(player.Uid,
                "#请选择『生发』执行项##伤害值+1##您HP+1,Y2", "JNE0802", "0");
            bool moreHarm = select == "1";
            if (moreHarm)
            {
                harms.ForEach(p =>
                {
                    if (XI.Board.Garden[p.Who].Gender == 'F' && p.N > 0 && !HPEvoMask.CHAIN_INVAO.IsSet(p.Mask))
                        ++p.N;
                });
                TargetPlayer(player.Uid, invs);
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 92);
            }
            else
            {
                Cure(player, player, 1);
                XI.InnerGMessage(fuse, 92);
            }
        }
        public bool JNE0803Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                Artiad.ImperialLeft il = Artiad.ImperialLeft.Parse(fuse);
                return player.TokenExcl.Count > 0 && il.IsNPCRelated();
            }
            else if (type == 1)
            {
                return XI.Board.Garden.Values.Any(p => p.HP == 0 && p.IsTared) &&
                    player.TokenExcl.Any(p => Artiad.ContentRule.IsNPCJoinable(
                    XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(ushort.Parse(p.Substring("M".Length)))), XI));
            }
            else
                return false;
        }
        public void JNE0803Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                Artiad.ImperialLeft il = Artiad.ImperialLeft.Parse(fuse);
                ushort outSoul = ushort.Parse(argst);
                ushort inSoul = il.Card;
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,M" + outSoul);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,M" + outSoul);
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,1,M" + inSoul);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + ",M" + inSoul);
                int delta = 0;
                if (XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(outSoul)).Gender == 'F')
                    --delta;
                if (XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(inSoul)).Gender == 'F')
                    ++delta;

                il.Card = outSoul;
                il.Handle100(XI, XI.WI);
                XI.InnerGMessage(il.ToMessage(), 111);
                if (delta < 0)
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (-delta));
                else if (delta > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",0," + delta);
            }
            else if (type == 1)
            {
                int idx = argst.IndexOf(',');
                ushort tar = ushort.Parse(argst.Substring(0, idx));
                ushort outSoul = ushort.Parse(argst.Substring(idx + 1));
                XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,M" + outSoul);
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,M" + outSoul);
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(outSoul));
                XI.RaiseGMessage(new Artiad.InnateChange()
                {
                    Item = Artiad.InnateChange.Prop.HP,
                    Who = player.Uid,
                    NewValue = player.HPb - 1
                }.ToMessage());
                if (npc.Gender == 'F')
                    XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
                XI.RaiseGMessage("G0OY,0," + tar);
                XI.RaiseGMessage("G0IY,2," + tar + "," + npc.Hero + ",1");
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.SHOWBOARD,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = outSoul }
                }.ToMessage());
                string next = XI.AsyncInput(player.Uid,
                    "#请选择『离魂汤』执行项##获得手牌##获得宠物,Y2", "JNE0603", "0");
                Player tr = XI.Board.Garden[tar];
                if (next == "2")
                {
                    if (tr.GetPetCount() > 0)
                    {
                        string selPet = XI.AsyncInput(player.Uid, "#获得,M1(p" + string.Join(
                            "p", XI.Board.Garden[tar].Pets.Where(p => p != 0)) + ")", "JNE0603", "1");
                        ushort pet = ushort.Parse(selPet);
                        XI.RaiseGMessage(new Artiad.HarvestPet()
                        {
                            Farmer = player.Uid,
                            Farmland = tar,
                            SinglePet = pet,
                            TreatyAct = Artiad.HarvestPet.Treaty.ACTIVE
                        }.ToMessage());
                    }
                }
                else
                {
                    if (tr.Tux.Count > 0)
                        XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + tar + ",1," + tr.Tux.ListToString());
                }
                if (XI.Board.Garden.Values.Any(p => p.HP == 0 && p.IsAlive))
                    XI.RaiseGMessage("G0ZH,-20");
            }
        }
        public string JNE0803Input(Player player, int type, string fuse, string prev)
        {
            if (type == 0 && prev == "")
                return "#替换,/M1(p" + string.Join("p", player.TokenExcl.Select(p => p.Substring("M".Length))) + ")";
            else if (type == 1 && prev == "")
            {
                return "/T1" + FormatPlayers(p => p.Uid != player.Uid && p.HP == 0 && p.IsTared) + ",/M1(p" +
                    string.Join("p", player.TokenExcl.Select(p => ushort.Parse(p.Substring("M".Length))).Where(
                        p => Artiad.ContentRule.IsNPCJoinable(XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(p)), XI))) + ")";
            }
            else return "";
        }
        #endregion EX312 - ZhaoWuyan

        #region RM302 - TangXuejian
        public bool JNR0201Valid(Player player, int type, string fuse)
        {
            return JN30201Valid(player, type, fuse) && !XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0);
        }
        public void JNR0201Action(Player player, int type, string fuse, string argst)
        {
            JN30201Action(player, type, fuse, argst);
        }
        public string JNR0201Input(Player player, int type, string fuse, string prev)
        {
            return JN30201Input(player, type, fuse, prev);
        }
        public bool JNR0202Valid(Player player, int type, string fuse)
        {
            return JN30202Valid(player, type, fuse);
        }
        public void JNR0202Action(Player player, int type, string fuse, string argst)
        {
            JN30202Action(player, type, fuse, argst);
        }
        public string JNR0202Input(Player player, int type, string fuse, string prev)
        {
            return JN30202Input(player, type, fuse, prev);
        }
        public bool JNR0203Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                   || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            return player.HP >= 2 && XI.Board.IsAttendWar(player) && meLose;
        }
        public void JNR0203Action(Player player, int type, string fuse, string argst)
        {
            Harm(player, player, 2, FiveElement.A, (long)HPEvoMask.TUX_INAVO);
            if (player.IsAlive)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,2");
        }
        #endregion RM302 - TangXuejian
        #region RM202 - SuMei
        public bool JNR0301Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return JN20201Valid(player, 0, fuse);
            else if (type == 1)
                return !player.RFM.GetBool("DuoFight");
            else
                return false;
        }
        public void JNR0301Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
                JN20201Action(player, type, fuse, argst);
            else if (type == 1)
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public bool JNR0302Valid(Player player, int type, string fuse)
        {
            bool anyTP = player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP);
            bool anyJP = player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP);
            // Optimization:
            // JP + TP: Both
            // JP: ask type 1 & 2
            // TP: ask type 0
            // NIL: ask type 0
            if (type == 0)
            {
                bool tp01 = JN20202Valid(player, type, fuse) &&
                    (!player.IsSKOpt || anyTP || (!anyTP && !anyJP));
                bool tpt3 = player.Tux.Count > 0 && XI.LibTuple.TL.EncodeTuxCode("TPT3")
                    .Valid(player, 1, fuse) && (!player.IsSKOpt || anyJP);
                return tp01 || tpt3;
            }
            else if (type == 1)
            {
                Tux tpt3 = XI.LibTuple.TL.EncodeTuxCode("TPT3");
                return player.Tux.Count > 0 && tpt3.Valid(player, 0, fuse) && (!player.IsSKOpt || anyJP);
            }
            else
                return false;
        }
        public void JNR0302Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort card = ushort.Parse(argst);
                Tux tux = XI.LibTuple.TL.DecodeTux(card);
                if (tux.Type == Tux.TuxType.TP)
                    JN20202Action(player, type, fuse, argst);
                else if (tux.Type == Tux.TuxType.JP)
                    XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid +
                        ",TPT3," + card + ";1," + fuse);
            }
            else if (type == 1)
            {
                ushort card = ushort.Parse(argst);
                XI.RaiseGMessage("G0CC," + player.Uid + ",0," + player.Uid +
                    ",TPT3," + card + ";0," + fuse);
            }
        }
        public string JNR0302Input(Player player, int type, string fuse, string prev)
        {
            bool anyTP = player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP);
            bool anyJP = player.Tux.Any(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP);
            if (type == 0 && prev == "")
            {
                bool tp01 = player.Tux.Count > 0 && XI.LibTuple.TL.EncodeTuxCode("TP01")
                    .Valid(player, 0, fuse) && anyTP;
                bool tpt3 = player.Tux.Count > 0 && XI.LibTuple.TL.EncodeTuxCode("TPT3")
                    .Valid(player, 1, fuse) && anyJP;
                List<ushort> cands = new List<ushort>();
                if (tp01)
                    cands.AddRange(player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.TP));
                if (tpt3)
                    cands.AddRange(player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP));
                string hint = (tp01 || tpt3) ? "#当作【{0}】,/Q1(p" + string.Join("p", cands) + ")" : "/";
                List<string> @as = new List<string>();
                if (tp01)
                    @as.Add(XI.LibTuple.TL.EncodeTuxCode("TP01").Name);
                if (tpt3)
                    @as.Add(XI.LibTuple.TL.EncodeTuxCode("TPT3").Name);
                return string.Format(hint, string.Join("/", @as));
            }
            else if (type == 1 && prev == "")
            {
                List<ushort> cands = player.Tux.Where(p =>
                    XI.LibTuple.TL.DecodeTux(p).Type == Tux.TuxType.JP).ToList();
                return cands.Count > 0 ? "#当作【净衣咒】,/Q1(p" + string.Join("p", cands) + ")" : "/";
            }
            else return "";
        }
        #endregion RM202 - SuMei
        #region R3W01 - NangongHuang
        public bool JNR0401Valid(Player player, int type, string fuse)
        {
            return XI.Board.MonPiles.Count >= 3;
        }
        public void JNR0401Action(Player player, int type, string fuse, string args)
        {
            ushort[] first = XI.Board.MonPiles.Watch(3);
            XI.RaiseGMessage("G0XZ," + player.Uid + ",2,1,3,1");
            if (player.Tux.Count > 0 && first.Intersect(XI.Board.MonDises).Any(p => Base.Card.NMBLib.IsMonster(p)))
                XI.RaiseGMessage("G0DH," + player.Uid + ",1,1");
        }
        public bool JNR0402Valid(Player player, int type, string fuse)
        {
            Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
            List<Player> enemys = XI.Board.Garden.Values.Where(q => q.IsAlive && q.Team == player.OppTeam).ToList();
            return opt.Farmer == player.Uid && opt.Trophy && opt.Pets.Select(p => XI.LibTuple.ML.Decode(p))
                .Any(p => enemys.Any(q => q.Pets[p.Element.Elem2Index()] != 0) || enemys.Any(q =>
                q.Pets.Any(r => r != 0 && XI.LibTuple.ML.Decode(r).STR == p.STR)));
        }
        public void JNR0402Action(Player player, int type, string fuse, string args)
        {
            ushort pet = ushort.Parse(args);
            ushort who = Artiad.ContentRule.GetPetOwnership(pet, XI);
            TargetPlayer(player.Uid, who);
            XI.RaiseGMessage(new Artiad.LosePet()
            {
                Owner = who,
                SinglePet = pet
            }.ToMessage());
        }
        public string JNR0402Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                Artiad.ObtainPet opt = Artiad.ObtainPet.Parse(fuse);
                List<Player> enemys = XI.Board.Garden.Values.Where(
                    q => q.IsAlive && q.Team == player.OppTeam).ToList();
                List<ushort> candidates = new List<ushort>();
                foreach (ushort pt in opt.Pets)
                {
                    Monster monster = XI.LibTuple.ML.Decode(pt);
                    int elem = monster.Element.Elem2Index();
                    candidates.AddRange(enemys.Where(p => p.Pets[elem] != 0).Select(p => p.Pets[elem]));
                    candidates.AddRange(enemys.SelectMany(p => p.Pets).Where(p => p != 0 &&
                        XI.LibTuple.ML.Decode(p).STR == monster.STR));
                }
                return "#弃置,/M1(p" + string.Join(",", candidates.Distinct()) + ")";
            }
            else
                return "";
        }
        #endregion R3W01 - NangongHuang
        #region RM509 - Lingyin
        public bool JNR0501Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return IsMathISOS("JNR0501", player, fuse);
            else if (type == 1)
                return IsMathISOS("JNR0501", player, fuse) && player.TokenExcl.Count < 5;
            else if (type == 2)
                return JNT0403Valid(player, 0, fuse);
            else
                return false;
        }
        public void JNR0501Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                XI.RaiseGMessage("G0IJ," + player.Uid + ",1,5,I1,I2,I3,I4,I5");
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0,I1,I2,I3,I4,I5");
            }
            else if (type == 1)
            {
                if (player.TokenExcl.Count > 0)
                    XI.RaiseGMessage("G0OJ," + player.Uid + ",1," + Algo.ListToString(player.TokenExcl));
                XI.RaiseGMessage("G0OA," + player.Uid + ",0,5");
            }
            else if (type == 2)
                JNT0403Action(player, 0, fuse, argst);
        }
        public bool JNR0502Valid(Player player, int type, string fuse)
        {
            bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                   || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
            Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
            Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
            return meLose && player.RFM.GetBool("Light") == false && (JNT0402Valid(player, 0, fuse) ||
                (mon1 != null && (mon1.Element == FiveElement.YINN || mon1.Element == FiveElement.SOLARIS)) ||
                (mon2 != null && (mon2.Element == FiveElement.YINN || mon2.Element == FiveElement.SOLARIS)));
        }
        public void JNR0502Action(Player player, int type, string fuse, string argst)
        {
            player.RFM.Set("Light", true);
            int idx = argst.IndexOf(',');
            int card = int.Parse(argst.Substring(0, idx));
            ushort to = ushort.Parse(argst.Substring(idx + 1));

            TargetPlayer(player.Uid, to);
            XI.RaiseGMessage("G0OJ," + player.Uid + ",1,1,I" + card);
            XI.RaiseGMessage("G2TZ,0," + player.Uid + ",I" + card);
            if (XI.Board.Garden[to].Tux.Count > 0)
                XI.RaiseGMessage("G0DH," + to + ",1,1");
            XI.RaiseGMessage("G0DH," + to + ",0,1");
        }
        public string JNR0502Input(Player player, int type, string fuse, string prev)
        {
            if (prev == "")
            {
                List<int> sets = new List<int>();
                Monster mon1 = XI.LibTuple.ML.Decode(XI.Board.Monster1);
                if (mon1 != null && player.TokenExcl.Contains("I" + mon1.Element.Elem2Int()))
                    sets.Add(mon1.Element.Elem2Int());
                if (mon1 != null && (mon1.Element == FiveElement.YINN || mon1.Element == FiveElement.SOLARIS))
                    sets.AddRange(player.TokenExcl.Select(p => int.Parse(p.Substring("I".Length))));
                Monster mon2 = XI.LibTuple.ML.Decode(XI.Board.Monster2);
                if (mon2 != null && player.TokenExcl.Contains("I" + mon2.Element.Elem2Int()))
                    sets.Add(mon2.Element.Elem2Int());
                if (mon2 != null && (mon2.Element == FiveElement.YINN || mon2.Element == FiveElement.SOLARIS))
                    sets.AddRange(player.TokenExcl.Select(p => int.Parse(p.Substring("I".Length))));
                return "/I1(p" + string.Join("p", sets.Distinct()) + "),#弃牌补牌,/T1" + AAlls(player);
            }
            else
                return "";
        }
        public bool JNR0503Valid(Player player, int type, string fuse)
        {
            return player.RFM.GetBool("Light") == false && JNT0401Valid(player, 0, fuse);
        }
        public void JNR0503Action(Player player, int type, string fuse, string argst)
        {
            player.RFM.Set("Light", true);
            JNT0401Action(player, 0, fuse, argst);
        }
        public string JNR0503Input(Player player, int type, string fuse, string prev)
        {
            return JNT0401Input(player, 0, fuse, prev);
        }
        #endregion RM509 - Lingyin
    }
}