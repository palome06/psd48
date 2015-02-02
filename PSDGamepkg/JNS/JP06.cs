using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg.JNS
{
    public class TuxCottage
    {
        #region Base Operations
        private Base.VW.IVI VI { set; get; }
        //private VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public TuxCottage(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }

        public IDictionary<string, Tux> RegisterDelegates(TuxLib lib, int pkgCode)
        {
            TuxCottage tc = this;
            IDictionary<string, Tux> tx01 = new Dictionary<string, Tux>();
            List<Tux> tuxes = lib.ListAllTuxs(pkgCode);
            foreach (Tux tux in tuxes)
            {
                tx01.Add(tux.Code, tux);
                string cardCode = tux.Code;
                var methodAction = tc.GetType().GetMethod(cardCode + "Action");
                if (methodAction != null)
                {
                    tux.Action += delegate(Player player, int type, string fuse, string argst)
                    {
                        methodAction.Invoke(tc, new object[] { player, type, fuse, argst });
                    };
                }
                var methodBribe = tc.GetType().GetMethod(cardCode + "Bribe");
                if (methodBribe != null)
                    tux.Bribe += new Tux.ValidDelegate(delegate(Player player, int type, string fuse)
                    {
                        return (bool)methodBribe.Invoke(tc, new object[] { player, type, fuse });
                    });
                else
                    tux.Bribe += new Tux.ValidDelegate(delegate(Player player, int type, string fuse)
                    {
                        return (bool)GeneralTuxBride(player);
                    });
                var methodValid = tc.GetType().GetMethod(cardCode + "Valid");
                if (methodValid != null)
                    tux.Valid += new Tux.ValidDelegate(delegate(Player player, int type, string fuse)
                    {
                        return (bool)methodValid.Invoke(tc, new object[] { player, type, fuse });
                    });
                var methodInput = tc.GetType().GetMethod(cardCode + "Input");
                if (methodInput != null)
                {
                    tux.Input += new Tux.InputDelegate(delegate(Player player, int type, string fuse, string prev)
                    {
                        return (string)methodInput.Invoke(tc, new object[] { player, type, fuse, prev });
                    });
                }
                else
                {
                    var methodInputAli = tc.GetType().GetMethod(cardCode + "InputHolder");
                    if (methodInputAli != null)
                    {
                        tux.Input += new Tux.InputDelegate(delegate(Player player, int type, string fuse, string prev)
                        {
                            return (string)methodInputAli.Invoke(tc, new object[] { player, player, type, fuse, prev });
                        });
                    }
                }
                var metholdInputHolder = tc.GetType().GetMethod(cardCode + "InputHolder");
                if (metholdInputHolder != null)
                {
                    tux.InputHolder += new Tux.InputHolderDelegate(delegate(Player provider,
                        Player user, int type, string fuse, string prev)
                    {
                        return (string)metholdInputHolder.Invoke(tc, new object[] { provider, user, type, fuse, prev });
                    });
                }
                else
                {
                    var methodInputAli = tc.GetType().GetMethod(cardCode + "Input");
                    if (methodInputAli != null)
                    {
                        tux.InputHolder += new Tux.InputHolderDelegate(delegate(Player provider,
                            Player user, int type, string fuse, string prev)
                        {
                            return (string)methodInputAli.Invoke(tc, new object[] { provider, type, fuse, prev });
                        });
                    }
                }
                //var methodEncrypt = tc.GetType().GetMethod(cardCode + "Encrypt");
                if (tux.IsTuxEqiup())
                {
                    TuxEqiup tue = (TuxEqiup)tux;
                    var methodConsumeAction = tc.GetType().GetMethod(cardCode + "ConsumeAction");
                    if (methodConsumeAction != null)
                    {
                        tue.ConsumeAction += new TuxEqiup.CsActionDelegate(delegate(Player player,
                            int consumeType, int type, string fuse, string argst)
                        {
                            methodConsumeAction.Invoke(tc, new object[] { player, consumeType, type, fuse, argst });
                        });
                    }
                    else
                    {
                        var methodConsumeActionAli = tc.GetType().GetMethod(cardCode + "ConsumeActionHolder");
                        if (methodConsumeActionAli != null)
                        {
                            tue.ConsumeAction += new TuxEqiup.CsActionDelegate(delegate(Player player,
                                int consumeType, int type, string fuse, string argst)
                            {
                                methodConsumeActionAli.Invoke(tc, new object[] { player, player,
                                    consumeType, type, fuse, argst });
                            });
                        }
                    }
                    var methodConsumeActionHolder = tc.GetType().GetMethod(cardCode + "ConsumeActionHolder");
                    if (methodConsumeActionHolder != null)
                    {
                        tue.ConsumeActionHolder += new TuxEqiup.CsActionHolderDelegate(delegate(Player provider,
                            Player user, int consumeType, int type, string fuse, string argst)
                        {
                            methodConsumeActionHolder.Invoke(tc, new object[] { provider, user,
                                consumeType, type, fuse, argst });
                        });
                    }
                    else
                    {
                        var methodConsumeActionAli = tc.GetType().GetMethod(cardCode + "ConsumeAction");
                        if (methodConsumeActionAli != null)
                            tue.ConsumeActionHolder += new TuxEqiup.CsActionHolderDelegate(delegate(Player provider,
                                Player user, int consumeType, int type, string fuse, string prev)
                            {
                                methodConsumeActionAli.Invoke(tc, new object[] { provider, consumeType, type, fuse, prev });
                            });
                    }
                    var methodIncrAction = tc.GetType().GetMethod(cardCode + "IncrAction");
                    if (methodIncrAction != null)
                        tue.IncrAction += new TuxEqiup.CrActionDelegate(delegate(Player player)
                        {
                            methodIncrAction.Invoke(tc, new object[] { player });
                        });
                    tue.IncrAction += new TuxEqiup.CrActionDelegate(delegate(Player player)
                    {
                        EquipGeneralIncrAction(tue, player);
                    });
                    var methodDecrAction = tc.GetType().GetMethod(cardCode + "DecrAction");
                    if (methodDecrAction != null)
                        tue.DecrAction += new TuxEqiup.CrActionDelegate(delegate(Player player)
                        {
                            methodDecrAction.Invoke(tc, new object[] { player });
                        });
                    tue.DecrAction += new TuxEqiup.CrActionDelegate(delegate(Player player)
                    {
                        EquipGeneralDecrAction(tue, player);
                    });
                    var methodConsumeValid = tc.GetType().GetMethod(cardCode + "ConsumeValid");
                    if (methodConsumeValid != null)
                    {
                        tue.ConsumeValid += new TuxEqiup.CsValidDelegate(delegate(Player player,
                            int consumeType, int type, string fuse)
                        {
                            return (bool)methodConsumeValid.Invoke(tc, new object[] { player, consumeType, type, fuse });
                        });
                    }
                    else
                    {
                        var methodConsumeValidAli = tc.GetType().GetMethod(cardCode + "ConsumeValidHolder");
                        if (methodConsumeValidAli != null)
                            tue.ConsumeValid += new TuxEqiup.CsValidDelegate(delegate(Player player,
                                int consumeType, int type, string fuse)
                            {
                                return (bool)methodConsumeValidAli.Invoke(tc, new object[] { player, player,
                                    consumeType, type, fuse });
                            });
                    }
                    var methodConsumeValidHolder = tc.GetType().GetMethod(cardCode + "ConsumeValidHolder");
                    if (methodConsumeValidHolder != null)
                    {
                        tue.ConsumeValidHolder += new TuxEqiup.CsValidHolderDelegate(delegate(Player provider,
                            Player user, int consumeType, int type, string fuse)
                        {
                            return (bool)methodConsumeValidHolder.Invoke(tc, new object[] { provider, user,
                                consumeType, type, fuse });
                        });
                    }
                    else
                    {
                        var methodConsumeValidAli = tc.GetType().GetMethod(cardCode + "ConsumeValid");
                        if (methodConsumeValidAli != null)
                            tue.ConsumeValidHolder += new TuxEqiup.CsValidHolderDelegate(delegate(Player provider,
                            Player user, int consumeType, int type, string fuse)
                            {
                                return (bool)methodConsumeValidAli.Invoke(tc, new object[] { provider,
                                    consumeType, type, fuse });
                            });
                    }
                    var methodConsumeInput = tc.GetType().GetMethod(cardCode + "ConsumeInput");
                    if (methodConsumeInput != null)
                    {
                        tue.ConsumeInput += new TuxEqiup.CsInputDelegate(delegate(Player player,
                            int consumeType, int type, string fuse, string prev)
                        {
                            return (string)methodConsumeInput.Invoke(tc, new object[] { player,
                                consumeType, type, fuse, prev });
                        });
                    }
                    else
                    {
                        var methodConsumeInputAli = tc.GetType().GetMethod(cardCode + "ConsumeInputHolder");
                        if (methodConsumeInputAli != null)
                            tue.ConsumeInput += new TuxEqiup.CsInputDelegate(delegate(Player player,
                                int consumeType, int type, string fuse, string prev)
                            {
                                return (string)methodConsumeInputAli.Invoke(tc, new object[] { player, player,
                                    consumeType, type, fuse, prev });
                            });
                    }
                    var methodConsumeInputHolder = tc.GetType().GetMethod(cardCode + "ConsumeInputHolder");
                    if (methodConsumeInputHolder != null)
                    {
                        tue.ConsumeInputHolder += new TuxEqiup.CsInputHolderDelegate(delegate(Player provider,
                            Player user, int consumeType, int type, string fuse, string prev)
                        {
                            return (string)methodConsumeInputHolder.Invoke(tc, new object[] { provider,
                                user, consumeType, type, fuse, prev });
                        });
                    }
                    else
                    {
                        var methodConsumeInputAli = tc.GetType().GetMethod(cardCode + "ConsumeInput");
                        if (methodConsumeInputAli != null)
                            tue.ConsumeInputHolder += new TuxEqiup.CsInputHolderDelegate(delegate(Player provider,
                                Player user, int consumeType, int type, string fuse, string prev)
                            {
                                return (string)methodConsumeInputAli.Invoke(tc, new object[] { provider,
                                    consumeType, type, fuse, prev });
                            });
                    }
                    var methodUseAction = tc.GetType().GetMethod(cardCode + "UseAction");
                    if (methodUseAction != null)
                        tue.UseAction += new TuxEqiup.CsUseActionDelegate(delegate(ushort cardUt, Player player)
                        {
                            methodUseAction.Invoke(tc, new object[] { cardUt, player });
                        });
                    else
                        tue.UseAction += new TuxEqiup.CsUseActionDelegate(delegate(ushort cardUt, Player player)
                        {
                            EquipGeneralUseAction(cardUt, player);
                        });
                    var methodInsAction = tc.GetType().GetMethod(cardCode + "InsAction");
                    if (methodInsAction != null)
                        tue.InsAction += new TuxEqiup.CrActionDelegate(delegate(Player player)
                        {
                            methodInsAction.Invoke(tc, new object[] { player });
                        });
                    var methodDelAction = tc.GetType().GetMethod(cardCode + "DelAction");
                    if (methodDelAction != null)
                        tue.DelAction += new TuxEqiup.CrActionDelegate(delegate(Player player)
                        {
                            methodDelAction.Invoke(tc, new object[] { player });
                        });
                }
                var methodLocust = tc.GetType().GetMethod(cardCode + "Locust");
                if (methodLocust != null)
                    tux.Locust += new Tux.LocustActionDelegate(delegate(Player player, int type, string fuse,
                        string argst, string cdFuse, Player locuster, Tux locus)
                    {
                        methodLocust.Invoke(tc, new object[] { player, type,
                                fuse, argst, cdFuse, locuster, locus });
                    });
                else
                    tux.Locust += new Tux.LocustActionDelegate(delegate(Player player, int type, string fuse,
                        string argst, string cdFuse, Player locuster, Tux locus)
                    {
                        GeneralLocustAction(player, type, fuse, argst, cdFuse, locuster, locus);
                    });
            }
            return tx01;
        }
        #endregion Base Operations

        #region JP
        // Tou Dao
        public void JP01Action(Player player, int type, string fuse, string argst)
        {
            //ushort from = ushort.Parse(argst);
            string msg = Util.SSelect(XI.Board, p => p != player && p.IsTared &&
                p.Tux.Except(XI.Board.ProtectedTux).Any());
            string inputFormat = (msg != null) ? "#获得其1张手牌,T1" + msg : "/";
            var ai = XI.AsyncInput(player.Uid, inputFormat, "JP01", "0");
            if (!ai.StartsWith("/"))
            {
                ushort from = ushort.Parse(ai);
                VI.Cout(0, "{0}对{1}使用「偷盗」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(from));
                string c0 = Util.RepeatString("p0",
                    XI.Board.Garden[from].Tux.Except(XI.Board.ProtectedTux).Count());
                XI.AsyncInput(player.Uid, "#获得的,C1(" + c0 + ")", "JP01," + from, "0");
                XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + from + ",2,1");
            }
            else
                VI.Cout(0, "{0}使用的「偷盗」无效.", XI.DisplayPlayer(player.Uid));
        }
        public bool JP01Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Where(p => p != player &&
                p.IsTared && p.Tux.Count > 0).Any();
        }
        // Kui Ce Tian Ji
        public void JP02Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "{0}使用「窥测天机」.", XI.DisplayPlayer(player.Uid));
            XI.RaiseGMessage("G0XZ," + player.Uid + ",2,1,2");
        }
        public bool JP02Valid(Player player, int type, string fuse)
        {
            return XI.Board.MonPiles.Count >= 2;
        }
        // Wu Qi Chao Yuan
        public void JP03Action(Player player, int type, string fuse, string argst)
        {
            VI.Cout(0, "{0}使用「五气朝元」.", player.Uid);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(XI.Board.Garden.Values.Where(p => p.IsAlive &&
                p.Team == player.Team).Select(p => new Artiad.Cure(p.Uid, player.Uid, FiveElement.AQUA, 1))));
        }
        // Shu Er Guo
        public void JP04Action(Player player, int type, string fuse, string argst)
        {
            ushort to = ushort.Parse(XI.AsyncInput(player.Uid,
                "#获得2张补牌,T1" + Util.SSelect(XI.Board, p => p.IsTared), "JP04", "0"));
            VI.Cout(0, "{0}对{1}使用「鼠儿果」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
            XI.RaiseGMessage("G0DH," + to + ",0,2");
        }
        public void JP05Action(Player player, int type, string fuse, string argst)
        {
            //ushort to = ushort.Parse(argst.Substring(argst.IndexOf(',') + 1));
            int maskFromJP = Artiad.IntHelper.SetMask(0, GiftMask.FROM_TUX, true);
            if (type == 0)
            {
                ushort to = ushort.Parse(XI.AsyncInput(
                    player.Uid, "#攻击,T1" + Util.SSelect(XI.Board, p => p.IsTared), "JP05", "0"));
                //ushort to = ushort.Parse(argst);
                VI.Cout(0, "{0}对{1}使用「天雷破」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
                XI.RaiseGMessage(Artiad.Harm.ToMessage(
                    new Artiad.Harm(to, player.Uid, FiveElement.THUNDER, 2, maskFromJP)));
            }
            else if (type == 1)
            {
                ushort to = ushort.Parse(fuse.Substring("R#EV,".Length));
                VI.Cout(0, "{0}对{1}使用「天雷破」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
                XI.RaiseGMessage(Artiad.Harm.ToMessage(
                    new Artiad.Harm(to, player.Uid, FiveElement.THUNDER, 2, maskFromJP)));
            }
        }
        public void JP06Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> targets = XI.Board.Garden.Values.Where(p => p.IsTared &&
                p.ListOutAllCards().Except(XI.Board.ProtectedTux).Any()).Select(p => p.Uid).ToList();
            string first = XI.AsyncInput(player.Uid, targets.Count > 0 ?
                ("#弃置,T1(p" + string.Join("p", targets) + ")") : "/", "JP06", "0");
            bool valid = false;
            if (!first.StartsWith("/"))
            {
                ushort owner = ushort.Parse(first);
                Player py = XI.Board.Garden[owner];
                string secondFormat = string.Join("p",
                    py.ListOutAllCardsWithEncrypt(XI.Board.ProtectedTux));
                if (secondFormat != "")
                    secondFormat = "#弃置的,C1(p" + secondFormat + ")";
                else
                    secondFormat = "/";

                string second = XI.AsyncInput(player.Uid, secondFormat, "JP06," + first, "0");
                if (!second.StartsWith("/"))
                {
                    ushort card = ushort.Parse(second);
                    if (card == 0)
                    {
                        VI.Cout(0, "{0}对{1}使用「铜钱镖」，弃置其一张手牌.",
                            XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(owner));
                        XI.RaiseGMessage("G0DH," + owner + ",2,1");
                    }
                    else
                    {
                        VI.Cout(0, "{0}对{1}使用「铜钱镖」，弃置卡牌{2}.",
                            XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(owner), XI.DisplayTux(card));
                        XI.RaiseGMessage("G0QZ," + owner + "," + card);
                    }
                    valid = true;
                }
            }
            if (!valid)
                VI.Cout(0, "{0}使用的「铜钱镖」无效.", XI.DisplayPlayer(player.Uid));
        }
        public bool JP06Valid(Player player, int type, string fuse)
        {
            if (XI.Board.Garden.Values.Where(p => p != player && p.IsTared && p.HasAnyCards()).Any())
                return true;
            else
                return player.Tux.Count > 0 || player.HasAnyEquips();
        }
        #endregion JP
        #region TP
        // Bing Xin Jue
        public void TP01Action(Player player, int type, string fuse, string args)
        {
            // fuse = G0CD,A,KN,x1,x2;...
            int idx1 = "G0CD".Length;
            int idx2 = fuse.IndexOf(',', idx1 + 1);
            int idx3 = fuse.IndexOf(',', idx2 + 1);
            int jdx = fuse.IndexOf(';');
            // VI.Cout(0, "{0}使用了「冰心诀」.", XI.DisplayPlayer(player.Uid));
            XI.RaiseGMessage("G2CL," + Util.Substring(fuse, idx1 + 1, idx2) + "," +
                Util.Substring(fuse, idx3 + 1, jdx));

            int kdx = fuse.IndexOf(',', jdx);
            if (kdx >= 0)
            {
                string origin = Util.Substring(fuse, kdx + 1, -1);
                if (origin.StartsWith("G0CD"))
                    XI.InnerGMessage(origin, 1);
                else if (origin.StartsWith("G"))
                {
                    int hdx = fuse.IndexOf(';');
                    string[] argv = Util.Substring(fuse, 0, hdx).Split(',');
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(argv[3]);
                    int inType = int.Parse(Util.Substring(fuse, hdx + 1, fuse.IndexOf(',', hdx)));
                    int prior = tux.Priorities[inType];
                    XI.InnerGMessage(origin, prior);
                }
            }
        }
        public bool TP01Valid(Player player, int type, string fuse)
        {
            string[] blocks = fuse.Split(',');
            bool cardValid = blocks[2] != "1";
            bool teammate = XI.Board.Garden[ushort.Parse(blocks[1])].Team == player.Team;
            return cardValid && (!player.IsTPOpt || !teammate);
        }
        // Ling Hu Xian Dan
        public bool TP02Bribe(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                Player r = XI.Board.Rounder;
                return r != null && r.Uid == player.Uid && !r.DrTuxDisabled;
            }
            else
                return !player.DrTuxDisabled;
        }
        public void TP02Action(Player player, int type, string fuse, string args)
        {
            if (type == 0)
            { // Use as JP
                VI.Cout(0, "{0}对自己使用「灵葫仙丹」.", XI.DisplayPlayer(player.Uid));
                XI.RaiseGMessage(Artiad.Cure.ToMessage(
                    new Artiad.Cure(player.Uid, player.Uid, FiveElement.A, 2)));
            }
            else if (type == 1)
            {
                List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).Select(p => p.Uid).ToList();
                string ic = invs.Count > 0 ? "T1(p" + string.Join("p", invs) + ")" : "/";
                ushort tg = ushort.Parse(XI.AsyncInput(player.Uid, ic, "「灵葫仙丹」", "1"));
                if (invs.Contains(tg))
                {
                    VI.Cout(0, "{0}对{1}使用「灵葫仙丹」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(tg));
                    XI.RaiseGMessage(Artiad.Cure.ToMessage(new Artiad.Cure(tg, player.Uid, FiveElement.A, 2)));
                }
                XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        public bool TP02Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
                return XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).Any();
            else
                return false;
        }
        public void TP02Locust(Player player, int type, string fuse,
            string argst, string cdFuse, Player locuster, Tux locust)
        {
            if (type == 0)
                GeneralLocustAction(player, type, fuse, argst, cdFuse, locuster, locust);
            else if (type == 1)
            {
                string[] argv = cdFuse.Split(',');
                ushort host = ushort.Parse(argv[1]);
                XI.RaiseGMessage("G0CE,1," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
                List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).Select(p => p.Uid).ToList();
                string ic = invs.Count > 0 ? "T1(p" + string.Join("p", invs) + ")" : "/";
                ushort tg = ushort.Parse(XI.AsyncInput(player.Uid, ic, "「灵葫仙丹」", "1"));
                if (invs.Contains(tg))
                {
                    VI.Cout(0, "{0}对{1}使用「灵葫仙丹」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(tg));
                    XI.RaiseGMessage(Artiad.Cure.ToMessage(new Artiad.Cure(tg, player.Uid, FiveElement.A, 2)));
                }

                invs = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
                ushort owner = locuster.Uid;
                string last = null; bool locusSucc = false;
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
                    ushort locustee = ushort.Parse(last.Split(',')[2]);
                    XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                    if (invs.Count > 0)
                    {
                        string newFuse = "G0ZH,0";
                        if (locuster.IsAlive && locuster.IsAlive && locust.Valid(locuster, type, newFuse))
                        {
                            XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                                "," + locust.Code + "," + locustee + ";" + type + "," + newFuse, 101);
                            locusSucc = true;
                        }
                    }
                }
                invs = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
                if (!locusSucc && invs.Count > 0)
                    XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        // Yin Gu
        public bool TP03Valid(Player player, int type, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.IsAvoidable())
                    return true;
            }
            return false;
        }
        public void TP03Action(Player player, int type, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.IsAvoidable())
                {
                    VI.Cout(0, "{0}使用「隐蛊」免疫本次伤害.", XI.DisplayPlayer(player.Uid));
                    rvs.Add(harm);
                }
            }
            harms.RemoveAll(p => rvs.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 0);
        }
        public void TP03Locust(Player player, int type, string fuse, string argst, string cdFuse, Player locuster, Tux locust)
        {
            string[] argv = cdFuse.Split(',');
            ushort host = ushort.Parse(argv[1]);
            XI.RaiseGMessage("G0CE,1," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == player.Uid && harm.IsAvoidable())
                {
                    VI.Cout(0, "{0}使用「隐蛊」免疫本次伤害.", XI.DisplayPlayer(player.Uid));
                    rvs.Add(harm);
                }
            }

            harms.RemoveAll(p => rvs.Contains(p));
            bool locusSucc = false;
            ushort owner = locuster.Uid;
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
                ushort locustee = ushort.Parse(last.Split(',')[2]);
                XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                if (harms.Count > 0)
                {
                    string newFuse = Artiad.Harm.ToMessage(harms);
                    if (locuster.IsAlive && locuster.IsAlive && locust.Valid(locuster, type, newFuse))
                    {
                        XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                            "," + locust.Code + "," + locustee + ";" + type + "," + newFuse, 101);
                        locusSucc = true;
                    }
                }
            }
            if (!locusSucc && harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 0);
        }
        public void TP04Action(Player player, int type, string fuse, string args)
        {
            ushort gamer = ushort.Parse(XI.AsyncInput(player.Uid,
                "T1" + Util.SSelect(XI.Board, p => p.Team == player.Team && p.IsTared), "「洞冥宝镜」", "0"));
            VI.Cout(0, "{0}对{1}使用「洞冥宝镜」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(gamer));
            XI.RaiseGMessage("G0XZ," + gamer + ",2,0,1");
            //XI.InnerGMessage("G1SG,0", 0);
        }
        public bool TP04Valid(Player player, int type, string fuse) {
            return XI.Board.MonPiles.Count > 0;
        }
        #endregion TP
        #region WQ
        public bool WQ02ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                // fuse = G0IH,A,Src,p,n...
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                foreach (Artiad.Cure cure in cures)
                {
                    if (cure.Who == player.Uid &&
                            cure.Element != FiveElement.SOL && cure.Element != FiveElement.LOVE)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void WQ02ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                // G0IH,A,Src,p,n,...
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                foreach (Artiad.Cure cure in cures)
                {
                    if (cure.Who == player.Uid &&
                            cure.Element != FiveElement.SOL && cure.Element != FiveElement.LOVE)
                    {
                        VI.Cout(0, "{0}触发「天蛇杖」,本次HP回复数值+1.", XI.DisplayPlayer(player.Uid));
                        ++cure.N;
                    }
                }
                if (cures.Count > 0)
                    XI.InnerGMessage(Artiad.Cure.ToMessage(cures), 11);
            }
        }
        #endregion WQ
        #region FJ
        public bool FJ01ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
                return player.IsAlive && player.HP == 0;
            else return false;
        }
        public void FJ01ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                VI.Cout(0, "{0}爆发「五彩霞衣」，脱离濒死状态且HP+2.", XI.DisplayPlayer(player.Uid));
                XI.RaiseGMessage(Artiad.Cure.ToMessage(
                    new Artiad.Cure(player.Uid, player.Uid, FiveElement.A, 2)));
                List<Player> zeros = XI.Board.Garden.Values.Where(p => p.IsAlive && p.HP == 0).ToList();
                if (zeros.Count > 0)
                    XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        public bool FJ02ConsumeValidHolder(Player provider, Player user, int consumeType, int type, string fuse)
        {
            if (consumeType == 0 && provider.Tux.Count > 0)
            {
                int lfidx = fuse.IndexOf(':');
                string pureFuse = fuse.Substring(lfidx + 1);
                return XI.LibTuple.TL.EncodeTuxCode("TP03").Valid(user, type, pureFuse);
            }
            else
                return false;
        }
        public void FJ02ConsumeActionHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string argst)
        {
            if (consumeType == 0)
            {
                ushort card = ushort.Parse(argst);
                int lfidx = fuse.IndexOf(':');
                string pureFuse = fuse.Substring(lfidx + 1);
                if (card != 0)
                    XI.RaiseGMessage("G0CC," + provider.Uid + ",0," + provider.Uid + ",TP03," + card + ";0," + pureFuse);
            }
        }
        public string FJ02ConsumeInputHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + Util.Sato(provider.Tux, "p") + ")";
            else
                return "";
        }
        public bool FJ03ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                // fuse = G0OH,A,Src,p,n...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid &&
                            harm.Element != FiveElement.SOL && harm.Element != FiveElement.LOVE)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void FJ03ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                // G0OH,A,Src,p,n,...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid &&
                            harm.Element != FiveElement.SOL && harm.Element != FiveElement.LOVE)
                    {
                        VI.Cout(0, "{0}触发「龙魂战铠」,本次伤害数值-1.", XI.DisplayPlayer(player.Uid));
                        {
                            if (--harm.N <= 0)
                                rvs.Add(harm);
                        }
                    }
                }
                harms.RemoveAll(p => rvs.Contains(p));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -9);
            }
        }
        //public void FJ04IncrAction(Player player)
        //{
        //    XI.RaiseGMessage("G0IA," + player.Uid + ",0,1");
        //}
        //public void FJ04DecrAction(Player player)
        //{
        //    XI.RaiseGMessage("G0OA," + player.Uid + ",0,1");
        //}
        public bool FJ04ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.FROM_TUX))
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void FJ04ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> iHarms = new List<Artiad.Harm>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.FROM_TUX))
                        iHarms.Add(harm);
                }
                harms.RemoveAll(p => iHarms.Contains(p));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(iHarms), -49);
                VI.Cout(0, "{0}触发「乾坤道袍」,本次伤害无效.", XI.DisplayPlayer(player.Uid));
            }
        }
        public bool FJ05ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE)
                        return true;
                }
            }
            return false;
        }
        public void FJ05ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                // G0OH,A,Src,p,n,...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<Artiad.Harm> rvs = new List<Artiad.Harm>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE)
                    {
                        VI.Cout(0, "{0}爆发「踏云靴」,免疫本次伤害，且HP+1.", XI.DisplayPlayer(player.Uid));
                        rvs.Add(harm);
                        XI.RaiseGMessage(Artiad.Cure.ToMessage(
                            new Artiad.Cure(player.Uid, player.Uid, FiveElement.A, 1)));
                    }
                }
                harms.RemoveAll(p => rvs.Contains(p));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 0);
            }
        }
        #endregion FJ
        #region ZP
        public bool ZP01Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public bool ZP01Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void ZP01Action(Player player, int type, string fuse, string argst)
        {
            string rwho = fuse.Substring(0, 2);
            XI.RaiseGMessage("G0JM," + rwho + "Z2");
        }
        // Tiangangzhanqi
        public bool ZP02Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public bool ZP02Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWarSucc(player);
        }
        public void ZP02Action(Player player, int type, string fuse, string argst)
        {
            if (player.STRa > 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + player.STRa);
        }
        // Jincanwang
        public bool ZP03Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public bool ZP03Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWarSucc(player);
        }
        public void ZP03Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
        }
        // Tianxuanwuyin
        public bool ZP04Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public void ZP04Action(Player player, int type, string fuse, string argst)
        {
            ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "S", "ZP04", "0"));
            XI.RaiseGMessage("G0IP," + side + ",2");
        }
        #endregion ZP

        #region Package of 4
        public void JPT1Action(Player player, int type, string fuse, string argst)
        {
            string[] args = argst.Split(',');
            bool b1 = XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0).Any();
            bool b2 = player.Tux.Count > 0 && XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0 &&
                XI.Board.Garden.Values.Where(q => q.IsTared && q.Team == p.Team && p.Uid != q.Uid).Any()).Any();
            
            string costr;
            if (b1 && b2)
                costr = "#请选择「驯化」执行项。##开牌##驯化,Y2";
            else if (b1)
                costr = "#请选择「驯化」执行项。##开牌,Y1";
            else if (b2)
                costr = "#请选择「驯化」执行项。##驯化,Y1";
            else
                costr = "";
            costr = XI.AsyncInput(player.Uid, costr, "JPT1", "0");
            if (costr == "2" || (costr == "1" && b2 && !b1))
            {
                if (player.Tux.Count < 0)
                    XI.AsyncInput(player.Uid, "/", "JPT1", "0");
                else
                {
                    string i0 = "#弃置的,Q1(p" + string.Join("p", player.Tux) + ")";
                    string qzStr = XI.AsyncInput(player.Uid, i0, "JPT1", "0");
                    ushort ut = ushort.Parse(qzStr);
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + ut);

                    string i1 = "#交出宠物的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                        p => p.IsTared && p.Pets.Where(q => q != 0).Any() &&
                        XI.Board.Garden.Values.Where(q => q.IsAlive &&
                            q.Uid != p.Uid && q.Team == p.Team).Any()).Select(p => p.Uid)) + ")";
                    string fromStr = XI.AsyncInput(player.Uid, i1, "JPT1", "0");
                    Player from = XI.Board.Garden[ushort.Parse(fromStr)];
                    do
                    {
                        string i2 = "#交予宠物的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                            p => p.IsTared && p.Uid != from.Uid && p.Team == from.Team).Select(p => p.Uid))
                            + "),/M1(p" + string.Join("p", from.Pets.Where(p => p != 0)) + ")";
                        string toStr = XI.AsyncInput(player.Uid, i2, "JPT1", "0");
                        if (!toStr.StartsWith("/"))
                        {
                            string[] tos = toStr.Split(',');
                            ushort to = ushort.Parse(tos[0]);
                            ushort pet = ushort.Parse(tos[1]);
                            XI.RaiseGMessage("G0HC,1," + to + "," + from.Uid + ",0," + pet);
                            break;
                        }
                    } while (true);
                }
            }
            else
            {
                int sumR = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.Team)
                    .Sum(p => p.GetPetCount());
                int sumO = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam)
                    .Sum(p => p.GetPetCount());

                Player op = XI.Board.GetOpponenet(player);
                List<ushort> pops = XI.DequeueOfPile(XI.Board.TuxPiles, sumO + sumR).ToList();
                XI.RaiseGMessage("G2IN,0," + (sumO + sumR));
                XI.RaiseGMessage("G1IU," + string.Join(",", pops));
                //XI.RaiseGMessage("G2FU,0," + string.Join(",", pops));

                string range1 = Util.SSelect(XI.Board, p => p.Team == player.Team && p.IsAlive);
                string range2 = Util.SSelect(XI.Board, p => p.Team == player.OppTeam && p.IsAlive);

                ushort[] uds = { player.Uid, op.Uid };
                string[] ranges = { Util.SSelect(XI.Board, p => p.Team == player.Team && p.IsAlive),
                        Util.SSelect(XI.Board, p => p.Team == player.OppTeam && p.IsAlive) };
                string input; string[] ips;

                int sumTR = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.Team)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
                int sumTO = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Team == player.OppTeam)
                    .Sum(p => p.Pets.Where(q => q != 0 && XI.LibTuple.ML.Decode(q) != null)
                    .Sum(q => XI.LibTuple.ML.Decode(q).STR));
                int idxs = sumTR <= sumTO ? 0 : 1;

                do
                {
                    //XI.RaiseGMessage("G2FU,1,1," + uds[idxs] + "," + string.Join(",", pops));
                    XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0," + string.Join(",", pops));
                    string pubTux = Util.SatoWithBracket(XI.Board.PZone, "p", "(p", ")");
                    input = XI.AsyncInput(uds[idxs], "+Z1" + pubTux + ",#获得卡牌的,/T1" + ranges[idxs], "JPT1", "0");
                    if (!input.StartsWith("/"))
                    {
                        ips = input.Split(',');
                        ushort cd;
                        if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                        {
                            ushort ut = ushort.Parse(ips[1]);
                            XI.RaiseGMessage("G1OU," + cd);
                            XI.RaiseGMessage("G2QU,0,0," + cd);
                            XI.RaiseGMessage("G0HQ,2," + ut + ",0,0," + cd);
                            pops.Remove(cd);
                            idxs = (idxs + 1) % 2;
                        }
                    }
                    XI.RaiseGMessage("G2FU,3");
                } while (pops.Count > 0);
            }
        }
        public bool JPT1Valid(Player player, int type, string fuse)
        {
            bool b1 = XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0).Any();
            bool b2 = player.Tux.Count > 1 && XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0 &&
                XI.Board.Garden.Values.Where(q => q.IsTared && q.Team == p.Team && p.Uid != q.Uid).Any()).Any();
            return b1 || b2;
        }
        public void JPT2Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> list = XI.Board.OrderedPlayer();
            List<Player> pys = list.Select(p => XI.Board.Garden[p]).Where(p =>
                p != null && p.IsAlive && p.Team == player.Team).ToList();
            if (pys.Count > 0)
                XI.RaiseGMessage("G0DH," + string.Join(",", pys.Select(p => p.Uid + ",0,1")));
        }
        public bool ZPT1Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public void ZPT1Action(Player player, int type, string fuse, string argst)
        {
            int val = (XI.Board.IsAttendWarSucc(player) || !XI.Board.IsAttendWar(player)) ? 1 : 4;
            ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "S", "ZPT1", "0"));
            XI.RaiseGMessage("G0IP," + side + "," + val);
        }
        public bool TPT1Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                if (XI.Board.Monster1 != 0)
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
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    bool tr = Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.TERMIN);
                    if (harm.N > 1 && harm.Element != FiveElement.YIN &&
                            harm.Element != FiveElement.LOVE && !tr && XI.Board.Garden[harm.Who].IsTared)
                        return true;
                }
            }
            return false;
        }
        public void TPT1Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                while (true)
                {
                    List<ushort> targets = XI.Board.Garden.Values.Where(
                        p => p.IsTared && p.Team == player.OppTeam && p.GetPetCount() > 0)
                        .Select(p => p.Uid).Except(XI.Board.PetProtecedPlayer).ToList();
                    string whoStr = XI.AsyncInput(player.Uid, "T1(p" +
                        string.Join("p", targets) + ")", "JPT1Action", "0");
                    ushort who = ushort.Parse(whoStr);
                    string monStr = XI.AsyncInput(player.Uid, "/M1(p" + string.Join("p", XI.Board.Garden[who]
                        .Pets.Where(p => p != 0)) + ")", "JPT1Action", "0");
                    if (monStr == VI.CinSentinel)
                        break;
                    if (!monStr.StartsWith("/"))
                    {
                        ushort mon = ushort.Parse(monStr);
                        XI.Board.Mon1From = who;
                        XI.Board.Monster1 = mon;
                        XI.RaiseGMessage("G0YM,0," + mon + "," + who);
                        XI.Board.AllowNoSupport = false;
                        break;
                    }
                }
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                ISet<ushort> invs = new HashSet<ushort>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.N > 1 && harm.Element != FiveElement.LOVE && XI.Board.Garden[harm.Who].IsTared)
                        invs.Add(harm.Who);
                }
                if (invs.Count > 0)
                {
                    string whoStr = XI.AsyncInput(player.Uid, "T1(p" + string.Join("p", invs)
                        + ")", "JPT1Action", "0");
                    ushort who = ushort.Parse(whoStr);
                    Artiad.Harm solHarm = null;
                    foreach (Artiad.Harm harm in harms)
                    {
                        bool tr = Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.TERMIN);
                        if (harm.Who == who && harm.N > 1 && harm.Element != FiveElement.YIN &&
                            harm.Element != FiveElement.LOVE && !tr)
                        {
                            if (harm.Element != FiveElement.SOL)
                                harm.N = 1;
                            else
                                solHarm = harm;
                        }
                    }
                    if (solHarm != null)
                    {
                        harms.Remove(solHarm);
                        foreach (Artiad.Harm harm in harms)
                        {
                            if (harm.Who == solHarm.Who && harm.Element == FiveElement.A)
                            {
                                harm.N += 1; solHarm = null;
                                break;
                            }
                            else if (harm.Who == solHarm.Who && harm.Element == FiveElement.SOL)
                            {
                                solHarm = null;
                                break;
                            }
                        }
                        if (solHarm != null)
                            harms.Add(new Artiad.Harm(solHarm.Who, solHarm.Source, FiveElement.A, 1, solHarm.Mask));
                    }
                }
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 85);
            }
        }
        public void TPT1Locust(Player player, int type, string fuse, string argst, string cdFuse, Player locuster, Tux locust)
        {
            if (type == 1)
            {
                string[] argv = cdFuse.Split(',');
                ushort host = ushort.Parse(argv[1]);
                XI.RaiseGMessage("G0CE,1," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                ISet<ushort> invs = new HashSet<ushort>();
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.N > 1 && harm.Element != FiveElement.LOVE && XI.Board.Garden[harm.Who].IsTared)
                        invs.Add(harm.Who);
                }
                if (invs.Count > 0)
                {
                    string whoStr = XI.AsyncInput(player.Uid, "T1(p" + string.Join("p", invs)
                        + ")", "JPT1Action", "0");
                    ushort who = ushort.Parse(whoStr);
                    Artiad.Harm solHarm = null;
                    foreach (Artiad.Harm harm in harms)
                    {
                        bool tr = Artiad.IntHelper.IsMaskSet(harm.Mask, GiftMask.TERMIN);
                        if (harm.Who == who && harm.N > 1 && harm.Element != FiveElement.YIN &&
                            harm.Element != FiveElement.LOVE && !tr)
                        {
                            if (harm.Element != FiveElement.SOL)
                                harm.N = 1;
                            else
                                solHarm = harm;
                        }
                    }
                    if (solHarm != null)
                    {
                        harms.Remove(solHarm);
                        foreach (Artiad.Harm harm in harms)
                        {
                            if (harm.Who == solHarm.Who && harm.Element == FiveElement.A)
                            {
                                harm.N += 1; solHarm = null;
                                break;
                            }
                            else if (harm.Who == solHarm.Who && harm.Element == FiveElement.SOL)
                            {
                                solHarm = null;
                                break;
                            }
                        }
                        if (solHarm != null)
                            harms.Add(new Artiad.Harm(solHarm.Who, solHarm.Source, FiveElement.A, 1, solHarm.Mask));
                    }
                }
                ushort owner = locuster.Uid;
                string last = null; bool locusSucc = false;
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
                    ushort locustee = ushort.Parse(last.Split(',')[2]);
                    XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                    if (harms.Any(p => p.N > 1 && p.Element != FiveElement.LOVE && XI.Board.Garden[p.Who].IsTared))
                    {
                        string newFuse = Artiad.Harm.ToMessage(harms);
                        if (locuster.IsAlive && locuster.IsAlive && locust.Valid(locuster, type, newFuse))
                        {
                            XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                                "," + locust.Code + "," + locustee + ";" + type + "," + newFuse, 101);
                            locusSucc = true;
                        }
                    }
                }
                if (!locusSucc && harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 85);
            }
        }
        public bool TPT2Bribe(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                Player r = XI.Board.Rounder;
                return r != null && r.Uid == player.Uid && !r.DrTuxDisabled;
            }
            else
                return !player.DrTuxDisabled;
        }
        public void TPT2Action(Player player, int type, string fuse, string argst)
        {
            string whoStr = XI.AsyncInput(player.Uid, "#获得补牌的,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")", "JPT2Action", "0");
            ushort who = ushort.Parse(whoStr);
            XI.RaiseGMessage("G0DH," + who + ",0,1");
        }
        public bool TPT2Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared);
        }
        public bool WQT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
                return XI.Board.IsAttendWar(player) && player.STR > 0;
            return false;
        }
        public void WQT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                VI.Cout(0, "{0}爆发「羲和」,令其强制命中.", XI.DisplayPlayer(player.Uid));
                XI.RaiseGMessage("G0IX," + player.Uid + ",2");
            }
        }
        public bool WQT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
                return XI.Board.IsAttendWar(player) && player.STR > 0;
            return false;
        }
        public void WQT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                VI.Cout(0, "{0}爆发「望舒」,令其战力加倍.", XI.DisplayPlayer(player.Uid));
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + player.STRa);
            }
        }
        public bool FJT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                if (player.Tux.Count > 0)
                {
                    // fuse = G0OH,A,Src,p,n...
                    List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                    foreach (Artiad.Harm harm in harms)
                    {
                        if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE)
                        {
                            if (XI.Board.Garden.Values.Any(p => p.Team ==
                                    XI.Board.Garden[harm.Who].OppTeam && p.IsTared))
                                return true;
                        }
                    }
                }
                return false;
            }
            else return false;
        }
        public void FJT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                int idx = argst.IndexOf(',');
                ushort ut = ushort.Parse(argst.Substring(0, idx));
                ushort to = ushort.Parse(argst.Substring(idx + 1));
                // G0OH,A,Src,p,n,...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Artiad.Harm rotation = null;
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE)
                    {
                        VI.Cout(0, "{0}触发「烟月神镜」,将伤害转移给{1}.",
                            XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
                        XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + ut);
                        rotation = harm;
                    }
                }
                if (rotation != null)
                {
                    harms.Remove(rotation);
                    if (rotation.Element == FiveElement.SOL)
                        rotation.Element = FiveElement.A;
                    foreach (Artiad.Harm harm in harms)
                    {
                        if (harm.Who == to)
                        {
                            if (harm.Element == FiveElement.SOL
                                && rotation.Element == FiveElement.A)
                            {
                                rotation = null; break;
                            }
                            if (harm.Element == rotation.Element)
                            {
                                ++harm.N;
                                rotation = null;
                                break;
                            }
                        }
                    }
                }
                if (rotation != null)
                {
                    rotation.Who = to;
                    rotation.N = 1;
                    //rotation.N = XI.Board.Garden[to].HP - (player.HP - rotation.N);
                    if (rotation.N > 0)
                        harms.Add(rotation);
                }
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -109);
            }
        }
        public string FJT1ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#交给对方,/Q1(p" + string.Join("p", player.Tux) + ")";
            else if (prev.IndexOf(',') < 0)
                return "#交给的,/T1(p" + string.Join("p", XI.Board.Garden.Values.Where(p => p.IsTared &&
                            p.Team == player.OppTeam).Select(p => p.Uid)) + ")";
            else
                return "";
        }
        public bool FJT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
            {
                // fuse = G0OH,A,Src,p,n...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE)
                        return true;
                }
                return false;
            }
            else return false;
        }
        public void FJT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                // G0OH,A,Src,p,n,...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == player.Uid && harm.Element != FiveElement.LOVE)
                    {
                        VI.Cout(0, "{0}爆发「寿葫芦」.", XI.DisplayPlayer(player.Uid));
                        harm.N = 1;
                    }
                }
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 85);
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
        }
        public void FJT2UseAction(ushort cardUt, Player player)
        {
            string tarStr = XI.AsyncInput(player.Uid, "#装备的,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")", "FJT2UseAction", "0");
            if (tarStr != VI.CinSentinel)
            {
                ushort tar = ushort.Parse(tarStr);
                if (tar != player.Uid)
                    XI.RaiseGMessage("G0ZB," + tar + ",1," + player.Uid +
                        ",0," + player.Uid + "," + cardUt);
                else
                    XI.RaiseGMessage("G0ZB," + player.Uid + ",0," + cardUt);
            }
        }
        public void FJT2InsAction(Player player)
        {
            Artiad.Cure cure = new Artiad.Cure(player.Uid, player.Uid, FiveElement.A, 2);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(cure));
        }
        #endregion Package of 4
        #region Package of 5 - Others
        public void JPT3Action(Player player, int type, string fuse, string argst)
        {
            var g = XI.Board.Garden; var v = g.Values;
            bool xgTuxValid = v.Where(p => p.IsTared &&
                p.Team == player.Team && p.Tux.Count > 0).Count() >= 2;
            bool xgPetValid = v.Where(p => p.IsTared &&
                p.Team == player.Team && p.GetPetCount() > 0).Count() >= 2;
            string hint = "#请选择执行项##触发事件";
            if (xgTuxValid)
                hint += "##交换手牌";
            if (xgPetValid)
                hint += "##交换宠物";
            int cnt = 1 + (xgTuxValid ? 1 : 0) + (xgPetValid ? 1 : 0);
            string input = XI.AsyncInput(player.Uid, hint + ",Y" + cnt, "JPT3", "0");
            if (input == "2")
            {
                string targets = XI.AsyncInput(player.Uid, "T2(p" + string.Join("p", v.Where(
                    p => p.IsTared && p.Team == player.Team && p.Tux.Count > 0).Select(p => p.Uid))
                    + ")", "JPT3", "0");
                int cmidx = targets.IndexOf(',');
                ushort iv = ushort.Parse(targets.Substring(0, cmidx));
                ushort jv = ushort.Parse(targets.Substring(cmidx + 1));
                int mn = Math.Min(3, Math.Min(g[iv].Tux.Count, g[jv].Tux.Count));
                XI.RaiseGMessage("G1XR,2," + iv + "," + jv + ",0," + mn);
            }
            else if (input == "3")
            {
                string targets = XI.AsyncInput(player.Uid, "T2(p" + string.Join("p", v.Where(
                    p => p.IsTared && p.Team == player.Team && p.GetPetCount() > 0).Select(p => p.Uid))
                    + ")", "JPT3", "0");
                int cmidx = targets.IndexOf(',');
                ushort iv = ushort.Parse(targets.Substring(0, cmidx));
                ushort jv = ushort.Parse(targets.Substring(cmidx + 1));
                int mn = Math.Min(g[iv].Tux.Count, g[jv].Tux.Count);

                List<ushort> imon = g[iv].Pets.Where(p => p != 0).ToList();
                string iipt = XI.AsyncInput(iv, "M1(p" + string.Join("p", imon), "JPT3", "0");
                XI.RaiseGMessage("G0HC,1," + jv + "," + iv + ",1," + iipt);
                List<ushort> vdMons = g[jv].Pets.Where(p => p != 0 && !imon.Contains(p)).ToList();
                if (vdMons.Count() > 0)
                {
                    string jipt = XI.AsyncInput(jv, "M1(p" + string.Join("p", vdMons), "JPT3", "0");
                    XI.RaiseGMessage("G0HC,1," + iv + "," + jv + ",1," + jipt);
                }
                Harm(null, player, 1);
            }
            else // if input == "1"
                XI.RaiseGMessage("G1EV," + player.Uid + ",1");
        }
        public void JPT4Action(Player player, int type, string fuse, string argst)
        {
            int maskFromJP = Artiad.IntHelper.SetMask(0, GiftMask.FROM_TUX, true);
            if (type == 0)
            {
                ushort to = ushort.Parse(XI.AsyncInput(
                    player.Uid, "T1" + Util.SSelect(XI.Board, p => p.IsTared), "JPT4", "0"));
                VI.Cout(0, "{0}对{1}使用「锁魂钉」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
                int harm = Math.Max(XI.Board.Garden[to].GetPetCount(), 1);
                XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(to, player.Uid,
                    FiveElement.A, harm, maskFromJP)));
            }
        }
        public void JPT5Action(Player player, int type, string fuse, string argst)
        {
            ushort to = ushort.Parse(XI.AsyncInput(
                    player.Uid, "T1" + Util.SSelect(XI.Board, p => p.IsTared), "JPT5", "0"));

            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
            XI.RaiseGMessage("G0YM,3," + pop + ",0");
            XI.Board.Monster1 = pop;
            UEchoCode r5ed = XI.HandleWithNPCEffect(XI.Board.Garden[to], npc, false);
            if (r5ed == UEchoCode.END_ACTION)
                XI.RaiseGMessage("G1YP," + player.Uid + "," + pop);
            
            if (XI.Board.Monster1 != 0) // In case the NPC has been taken away
            {
                XI.Board.Monster1 = 0;
                XI.RaiseGMessage("G0ON,0,M,1," + pop);
            }
            // XI.Board.RestNPCDises.Add(pop);
            XI.RaiseGMessage("G0YM,3,0,0");
            //}
        }
        public void ZPT2Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#请选择执行项##命中+3##战力+2,Y2", "ZPT2", "0");
            if (input == "1")
                XI.RaiseGMessage("G0IX," + player.Uid + ",1,3");
            else
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
        }
        public bool ZPT2Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public bool ZPT2Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void ZPT3Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#请选择执行项##"
                + "任意命中+1##自战力加成##自命中加成,Y3", "ZPT3", "0");
            if (input == "1")
            {
                string target = XI.AsyncInput(player.Uid, "T1" + AAllTareds(player), "ZPT3", "1");
                XI.RaiseGMessage("G0IX," + target + ",1,1");
            }
            else if (input == "2")
            {
                if (player.DEXa > 0)
                    XI.RaiseGMessage("G0IA," + player.Uid + ",1," + player.DEXa);
            }
            else if (input == "3")
            {
                if (player.STRa > 0)
                    XI.RaiseGMessage("G0IX," + player.Uid + ",1," + player.STRa);
            }
        }
        public bool ZPT3Bribe(Player player, int type, string fuse)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.DrTuxDisabled;
        }
        public bool TPT3Valid(Player player, int type, string fuse)
        {
            string[] kekkaiA = new string[] { "TP01,0", "TPT3,0", "TPT3,1" };
            string[] kekkaiB = new string[] { "ZP01,0", "TPT1,0" };
            // G0CD,A,T,KN,x..;TF
            if (type == 0)
            {
                int idx = fuse.IndexOf(';');
                string[] blocks = fuse.Substring(0, idx).Split(',');
                string tuxType = Util.Substring(fuse, idx + 1, fuse.IndexOf(',', idx + 1));
                Player py = XI.Board.Garden[ushort.Parse(blocks[1])];
                bool typeMatch = blocks[2] != "1";
                string tuxCode = blocks[3];
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(tuxCode);
                return py.Team == player.OppTeam && typeMatch && tux != null &&
                    !kekkaiA.Contains(tuxCode + "," + tuxType) && !kekkaiB.Contains(tuxCode + "," + tuxType);
            }
            else if (type == 1)
            {
                int idx = fuse.IndexOf(';');
                string[] blocks = fuse.Substring(0, idx).Split(',');
                string tuxType = Util.Substring(fuse, idx + 1, fuse.IndexOf(',', idx + 1));
                Player py = XI.Board.Garden[ushort.Parse(blocks[1])];
                bool typeMatch = blocks[2] != "1";
                string tuxCode = blocks[3];
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(tuxCode);
                return py.Team == player.OppTeam && typeMatch && tux != null &&
                    kekkaiA.Contains(tuxCode + "," + tuxType);
            }
            else
                return false;
        }
        public void TPT3Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                if (!XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.Team))
                {
                    XI.AsyncInput(player.Uid, "/", "TPT3", "0"); return;
                }
                string tar = XI.AsyncInput(player.Uid, "#【净衣咒】使用,T1" + ATaredTeammates(player), "TPT3", "0");
                Player locuster = XI.Board.Garden[ushort.Parse(tar)];
                int idx = fuse.IndexOf(';');
                string[] blocks = fuse.Substring(0, idx).Split(',');
                Tux tux = XI.LibTuple.TL.EncodeTuxCode(blocks[3]);
                int jdx = fuse.IndexOf(',', idx + 1);
                ushort sktType = ushort.Parse(Util.Substring(fuse, idx + 1, jdx));
                string sktFuse = Util.Substring(fuse, jdx + 1, -1);
                string cdFuse = Util.Substring(fuse, 0, idx);
                // Warning: self might be more than two tuxes
                tux.Locust(player, sktType, sktFuse, argst, cdFuse, locuster, tux);
            }
            else if (type == 1)
                TP01Action(player, 0, fuse, "");
        }
        #endregion Package of 5 - Others
        #region Package of 5 - XB
        public void XBT1InsAction(Player player)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public void XBT1DelAction(Player player)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT1") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT1");
            if (lug != null && lug.Capacities.Count > 0) {
                lug.Pull = true;
                List<string> cap = lug.Capacities.ToList();
                XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                    + ",1," + string.Join(",", cap));
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                XI.RaiseGMessage("G0ON," + player.Uid + ",M," + cap.Count + ","
                    + string.Join(",", cap.Select(p => p.Substring("M".Length))));
                lug.Pull = false;
            }
        }
        public bool XBT1ConsumeValid(Player player, int consumeType, int type, string fuse) {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT1") as Base.Card.Luggage;
            if (lug != null && consumeType == 0 && !lug.Pull)
            {
                if (type == 0) {
                    if (XI.Board.InFightThrough && XI.Board.Rounder.Team == player.Team) {
                        string[] g0on = fuse.Split(',');
                        for (int i = 1; i < g0on.Length; ) {
                            string cardType = g0on[i + 1];
                            int n = int.Parse(g0on[i + 2]);
                            if (cardType == "M") {
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
                } else if (type == 1)
                    return lug.Capacities.Sum(p => XI.LibTuple.ML.Decode(
                        ushort.Parse(p.Substring("M".Length))).STR) >= 4;
            }
            return false;
        }
        public void XBT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT1") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT1");
            if (lug != null && lugCode != 0 && consumeType == 0)
            {
                if (type == 0) {
                    List<ushort> sns = new List<ushort>();
                    string[] g0on = fuse.Split(',');
                    string n0on = "";
                    for (int i = 1; i < g0on.Length; ) {
                        string fromZone = g0on[i];
                        string cardType = g0on[i + 1];
                        int n = int.Parse(g0on[i + 2]);
                        if (cardType == "M") {
                            List<ushort> keeps = new List<ushort>();
                            for (int j = i + 3; j < i + 3 + n; ++j)
                            {
                                ushort ut = ushort.Parse(g0on[j]);
                                if (Base.Card.NMBLib.IsMonster(ut))
                                    sns.Add(ut);
                                else
                                    keeps.Add(ut);
                            }
                            if (keeps.Count > 0)
                                n0on += "," + fromZone + ",M," + keeps.Count + "," + string.Join(",", keeps);
                        } else
                            n0on += "," + string.Join(",", Util.TakeRange(g0on, i, i + 3 + n));
                        i += (n + 3);
                    }
                    if (sns.Count > 0)
                    {
                        string ss = string.Join(",", sns.Select(p => "M" + p));
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",0," + ss);
                        XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + ss);
                    }
                    if (n0on.Length > 0)
                        XI.InnerGMessage("G0ON" + n0on, 81);
                } else if (type == 1) {
                    int total = lug.Capacities.Sum(p => XI.LibTuple.ML.Decode(
                        ushort.Parse(p.Substring("M".Length))).STR) / 4;
                    IDictionary<Player, int> sch = new Dictionary<Player, int>();
                    List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsTared).ToList();
                    while (total > 0)
                    {
                        if (invs.Count == 0)
                            break;
                        if (invs.Count == 1)
                        {
                            string word = "#补牌,T1(p" + invs[0].Uid + "),#补牌数,D" + total;
                            XI.AsyncInput(XI.Board.Rounder.Uid, word, "XBT1", "0");
                            sch[invs[0]] = total;
                            total = 0; invs.Clear();
                        }
                        else
                        {
                            string ichi = total == 1 ? "/D1" : ("/D1~" + total);
                            string word = "#补牌,T1(p" + string.Join("p",
                                invs.Select(p => p.Uid)) + "),#补牌数," + ichi;
                            string input = XI.AsyncInput(XI.Board.Rounder.Uid, word, "XBT1", "0");
                            if (!input.Contains("/"))
                            {
                                string[] ips = input.Split(',');
                                ushort ut = ushort.Parse(ips[0]);
                                int zn = int.Parse(ips[1]);
                                Player py = XI.Board.Garden[ut];
                                if (!sch.ContainsKey(py))
                                    sch[py] = 0;
                                sch[py] += zn;
                                total -= zn;
                            }
                        }
                    }
                    List<string> cap = lug.Capacities.ToList();
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                        + ",1," + string.Join(",", cap));
                    XI.RaiseGMessage("G0ON," + player.Uid + ",M," + cap.Count +
                        "," + string.Join(",", cap.Select(p => p.Substring("M".Length))));
                    if (sch.Count > 0)
                        XI.RaiseGMessage("G0DH," + string.Join(",", sch.Select(
                            p => p.Key.Uid + ",0," + p.Value)));
                }
            }
        }
        public void XBT2InsAction(Player player)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public void XBT2DelAction(Player player)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT2") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT2");
            if (lug != null && lug.Capacities.Count > 0)
            {
                List<string> cap = lug.Capacities.ToList();
                XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                    + ",1," + string.Join(",", cap));
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                XI.RaiseGMessage("G0ON," + player.Uid + ",C," + cap.Count + ","
                    + string.Join(",", cap.Select(p => p.Substring("C".Length))));
            }
        }
        public bool XBT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT2") as Base.Card.Luggage;
            if (lug != null && consumeType == 0)
            {
                if (type == 0)
                {
                    string[] g1di = fuse.Split(',');
                    for (int idx = 1; idx < g1di.Length;) {
                        ushort who = ushort.Parse(g1di[idx]);
                        bool drIn = g1di[idx + 1] == "0";
                        int n = int.Parse(g1di[idx + 2]);
                        if (who == player.Uid && !drIn && n > 0)
                            return true;
                        idx += (4 + n);
                    }
                }
                else if (type == 1)
                {
                    bool isRd = XI.Board.RoundIN == "R" + XI.Board.Rounder.Uid + "QR";
                    if (lug.Capacities.Count >= 2 && isRd)
                    {
                        string[] g1di = fuse.Split(',');
                        for (int idx = 1; idx < g1di.Length; )
                        {
                            ushort who = ushort.Parse(g1di[idx]);
                            bool drIn = g1di[idx + 1] == "0";
                            int n = int.Parse(g1di[idx + 2]);
                            if (who != player.Uid && !drIn && n > 0)
                            {
                                if (Util.TakeRange(g1di, idx + 4, idx + 4 + n).Select(p =>
                                        ushort.Parse(p)).Any(p => XI.Board.TuxDises.Contains(p)))
                                    return true;
                            }
                            idx += (4 + n);
                        }
                    }
                }
            }
            return false;
        }
        public void XBT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT2") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT2");
            if (lug != null && lugCode != 0 && consumeType == 0)
            {
                if (type == 0)
                {
                    string n1di = "";
                    List<ushort> tuxes = new List<ushort>();
                    List<ushort> eqs = new List<ushort>();
                    string[] g1di = fuse.Split(',');
                    for (int idx = 1; idx < g1di.Length; )
                    {
                        ushort who = ushort.Parse(g1di[idx]);
                        bool drIn = g1di[idx + 1] == "0";
                        int n = int.Parse(g1di[idx + 2]);
                        if (who == player.Uid && !drIn && n > 0)
                        {
                            int neq = int.Parse(g1di[idx + 3]);
                            List<ushort> rms = Util.TakeRange(g1di, idx + 4, idx + 4 + n - neq)
                                .Select(p => ushort.Parse(p)).ToList();
                            List<ushort> reqs = Util.TakeRange(g1di, idx + 4 + n - neq, idx + 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            tuxes.AddRange(rms);
                            eqs.AddRange(reqs);
                        }
                        else
                            n1di += "," + string.Join(",", Util.TakeRange(g1di, idx, idx + 4 + n));
                        idx += (4 + n);
                    }
                    ushort[] revs = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    if (revs.Length + lug.Capacities.Count <= 4)
                    {
                        string ss = string.Join(",", revs.Select(p => "C" + p));
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",0," + ss);
                        XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + ss);
                    }
                    else
                    {
                        int dif = revs.Length + lug.Capacities.Count - 4;
                        string dhead = "#「炼蛊皿」中替换,C" + dif + "(p";
                        string dinput = XI.AsyncInput(player.Uid, dhead + string.Join("p", lug.Capacities
                            .Select(p => p.Substring("C".Length))) + ")", "XBT2Consume", "1");
                        ushort[] subs = dinput.Split(',').Select(p => ushort.Parse(p)).ToArray();
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                            + ",1," + string.Join(",", subs.Select(p => "C" + p)));
                        XI.RaiseGMessage("G2TZ,0," + player.Uid + ","
                            + string.Join(",", subs.Select(p => "C" + p)));
                        XI.RaiseGMessage("G0ON," + player.Uid + ",C," + subs.Length + "," + string.Join(",", subs));
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                            + ",0," + string.Join(",", revs.Select(p => "C" + p)));
                        XI.RaiseGMessage("G2TZ," + player.Uid + ",0,"
                            + string.Join(",", revs.Select(p => "C" + p)));
                    }
                    tuxes.RemoveAll(p => revs.Contains(p));
                    eqs.RemoveAll(p => revs.Contains(p));
                    tuxes.AddRange(eqs);
                    if (tuxes.Count > 0)
                        n1di += "," + player.Uid + ",1," + tuxes.Count + "," + (tuxes.Count - eqs.Count) +
                            "," + string.Join(",", tuxes);
                    if (n1di.Length > 0)
                        XI.InnerGMessage("G1DI" + n1di, 41);
                }
                else if (type == 1)
                {
                    string n1di = "";
                    IDictionary<ushort, List<ushort>> tuxes = new Dictionary<ushort, List<ushort>>();
                    IDictionary<ushort, List<ushort>> eqs = new Dictionary<ushort, List<ushort>>();
                    IDictionary<ushort, ushort> belongs = new Dictionary<ushort, ushort>();
                    string[] g1di = fuse.Split(',');
                    for (int idx = 1; idx < g1di.Length; )
                    {
                        ushort who = ushort.Parse(g1di[idx]);
                        bool drIn = g1di[idx + 1] == "0";
                        int n = int.Parse(g1di[idx + 2]);
                        if (who != player.Uid && !drIn && n > 0)
                        {
                            int neq = int.Parse(g1di[idx + 3]);
                            for (int j = idx + 4; j < idx + 4 + n - neq; ++j)
                            {
                                ushort tx = ushort.Parse(g1di[j]);
                                Util.AddToMultiMap(tuxes, who, tx);
                                belongs[tx] = who;
                            }
                            for (int j = idx + 4 + n - neq; j < idx + 4 + n; ++j)
                            {
                                ushort tx = ushort.Parse(g1di[j]);
                                Util.AddToMultiMap(eqs, who, tx);
                                belongs[tx] = who;
                            }
                        }
                        else
                            n1di += "," + string.Join(",", Util.TakeRange(g1di, idx, idx + 4 + n));
                        idx += (4 + n);
                    }
                    ushort[] blocks = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    string ss = "C" + blocks[1] + ",C" + blocks[2];
                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1," + ss);
                    XI.RaiseGMessage("G0ON," + player.Uid + ",C,2," + blocks[1] + "," + blocks[2]);
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + ss);
                    XI.RaiseGMessage("G2CN,0,1");
                    XI.Board.TuxDises.Remove(blocks[0]);
                    XI.RaiseGMessage("G0HQ,2," + player.Uid + ",0,0," + blocks[0]);
                    ushort belonger = belongs[blocks[0]];
                    if (tuxes.ContainsKey(belonger))
                    {
                        tuxes[belonger].Remove(blocks[0]);
                        if (tuxes[belonger].Count == 0)
                            tuxes.Remove(belonger);
                    }
                    if (eqs.ContainsKey(belonger))
                    {
                        eqs[belonger].Remove(blocks[0]);
                        if (eqs[belonger].Count == 0)
                            eqs.Remove(belonger);
                    }
                    foreach (ushort put in XI.Board.Garden.Keys)
                    {
                        int ntx = 0, neq = 0;
                        List<ushort> allList = new List<ushort>();
                        if (tuxes.ContainsKey(put)) { ntx += tuxes[put].Count; allList.AddRange(tuxes[put]); }
                        if (eqs.ContainsKey(put)) { neq += eqs[put].Count; allList.AddRange(eqs[put]); }
                        if (ntx + neq > 0)
                            n1di += "," + put + ",1," + (ntx + neq) + "," + neq + "," + string.Join(",", allList);
                    }
                    if (n1di.Length > 0)
                        XI.InnerGMessage("G1DI" + n1di, 141);
                }
            }
        }
        public string XBT2ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT2") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT2");
            if (lug != null && lugCode != 0 && consumeType == 0)
            {
                if (type == 0 && prev == "")
                {
                    List<ushort> tuxes = new List<ushort>();
                    string[] g1di = fuse.Split(',');
                    for (int idx = 1; idx < g1di.Length; )
                    {
                        ushort who = ushort.Parse(g1di[idx]);
                        bool drIn = g1di[idx + 1] == "0";
                        int n = int.Parse(g1di[idx + 2]);
                        if (who == player.Uid && !drIn && n > 0)
                            tuxes.AddRange(Util.TakeRange(g1di, idx + 4, idx + 4 + n)
                                .Select(p => ushort.Parse(p)));
                        idx += (4 + n);
                    }
                    int tcnt = tuxes.Count > 4 ? 4 : tuxes.Count;
                    string head = (tcnt > 1) ? ("/C1~" + tcnt) : "/C1";
                    return "#置入「炼蛊皿」," + head + "(p" + string.Join("p", tuxes) + ")";
                }
                else if (type == 1 && prev == "")
                {
                    List<ushort> tuxes = new List<ushort>();
                    string[] g1di = fuse.Split(',');
                    for (int idx = 1; idx < g1di.Length; )
                    {
                        ushort who = ushort.Parse(g1di[idx]);
                        bool drIn = g1di[idx + 1] == "0";
                        int n = int.Parse(g1di[idx + 2]);
                        if (who != player.Uid && !drIn && n > 0)
                            tuxes.AddRange(Util.TakeRange(g1di, idx + 4, idx + 4 + n)
                                .Select(p => ushort.Parse(p)));
                        idx += (4 + n);
                    }
                    return "#获得的,/C1(p" + string.Join("p", tuxes) + "),#弃置的,/C2(p"
                        + string.Join("p", lug.Capacities.Select(p => p.Substring("C".Length))) + ")";
                }
            }
            return "";
        }
        public void XBT3InsAction(Player player)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public void XBT3DelAction(Player player)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT3");
            if (lug != null && lug.Capacities.Count > 0)
            {
                List<string> cap = lug.Capacities.ToList();
                XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                    + ",1," + string.Join(",", lug.Capacities));
                if (player.IsAlive)
                    XI.RaiseGMessage("G0HQ,3," + player.Uid + "," + player.Uid + ","
                        + cap.Count + "," + string.Join(",", cap.Select(p => p.Substring("C".Length))));
                else
                {
                    string ss = string.Join(",", cap.Select(p => p.Substring("C".Length)));
                    XI.RaiseGMessage("G0ON," + player.Uid + ",C," + cap.Count + "," + ss);
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + ss);
                }
            }
        }
        public void XBT3ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT3");
            if (lug != null && consumeType == 0)
            {
                if (type == 0)
                {
                    if (lug.Capacities.Count > 0)
                    {
                        List<string> cap = lug.Capacities.ToList();
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                            + ",1," + string.Join(",", lug.Capacities));
                        XI.RaiseGMessage("G0HQ,3," + player.Uid + "," + player.Uid + ","
                            + cap.Count + "," + string.Join(",", cap.Select(p => p.Substring("C".Length))));
                    }
                    if (player.Tux.Count > 0)
                    {
                        int tcnt = player.Tux.Count > 2 ? 2 : player.Tux.Count;
                        string head = (tcnt > 1) ? ("/Q1~" + tcnt) : "/Q1";
                        string input = XI.AsyncInput(player.Uid, "#置入「梦见樽」," + head +
                            "(p" + string.Join("p", player.Tux) + ")", "XBT3ConsumeAction", "0");
                        if (input != VI.CinSentinel && !input.StartsWith("/"))
                        {
                            ushort[] cards = input.Split(',').Select(p => ushort.Parse(p)).ToArray();
                            XI.RaiseGMessage("G0OT," + player.Uid + "," +
                                cards.Length + "," + string.Join(",", cards));
                            string ss = string.Join(",", cards.Select(p => "C" + p));
                            XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",0," + ss);
                            XI.RaiseGMessage("G2TZ," + player.Uid + "," + player.Uid + "," + ss);
                        }
                    }
                    XI.InnerGMessage(fuse, 271);
                }
                else if (type == 1 || type == 2)
                {
                    string linkFuse = fuse;
                    int lfidx = linkFuse.IndexOf(':');
                    // linkHeads = { "TP02,0", "TP03,0" };
                    string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                    string pureFuse = linkFuse.Substring(lfidx + 1);

                    ushort ut = ushort.Parse(argst);
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
                                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1,C" + ut);
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
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1,C" + ut);
                        XI.RaiseGMessage("G0ZB," + player.Uid + ",1," + player.Uid + ",0,0," + ut);
                    }
                }
            }
        }
        public bool XBT3ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Base.Card.Luggage;
            if (lug != null && consumeType == 0)
            {
                if (type == 0)
                {
                    string[] blocks = fuse.Split(',');
                    int idx = 1;
                    while (idx < blocks.Length)
                    {
                        ushort n = ushort.Parse(blocks[idx + 2]);
                        if (blocks[idx + 1] == "0")
                        {
                            ushort who = ushort.Parse(blocks[idx]);
                            Player py = XI.Board.Garden[who];
                            if (who == player.Uid && n > 0)
                                return true;
                        }
                        idx += (n + 4);
                    }
                }
                else if (type == 1 || type == 2)
                {
                    string linkFuse = fuse;
                    int lfidx = linkFuse.IndexOf(':');
                    // linkHeads = { "TP02,0", "TP03,0" };
                    string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                    string pureFuse = linkFuse.Substring(lfidx + 1);

                    foreach (string ccard in lug.Capacities)
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
                                //if (pureTypeStr.Contains("!"))
                                //{
                                //    int jdx = pureTypeStr.IndexOf('!');
                                //    ushort pureType = ushort.Parse(pureTypeStr.Substring(0, jdx));
                                //    ushort pureConsu = ushort.Parse(pureTypeStr.Substring(jdx + 1));
                                //    if (tux.Code == pureName && tux.IsTuxEqiup())
                                //    {
                                //        Base.Card.TuxEqiup tue = tux as Base.Card.TuxEqiup;
                                //        if (tue.ConsumeValid(player, pureConsu, pureType, pureFuse))
                                //            return true;
                                //    }
                                //} else
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
                }
            }
            return false;
        }
        public string XBT3ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Base.Card.Luggage;
            if (lug != null && consumeType == 0 && (type == 1 || type == 2) && prev == "")
            {
                List<ushort> candidates = new List<ushort>();

                string linkFuse = fuse;
                int lfidx = linkFuse.IndexOf(':');
                string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                string pureFuse = linkFuse.Substring(lfidx + 1);

                foreach (string ccard in lug.Capacities)
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
        public void XBT4InsAction(Player player)
        {
            XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
        }
        public void XBT4DelAction(Player player)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT4") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT4");
            if (lug != null && lug.Capacities.Count > 0)
            {
                if (player.IsAlive)
                {
                    string revInput = XI.AsyncInput(player.Uid, "#保留,/C1(p" +
                        string.Join("p", lug.Capacities.Select(p => p.Substring("C".Length))), "XBT4DelAction", "0");
                    if (revInput != VI.CinSentinel && !revInput.StartsWith("/"))
                    {
                        ushort ut = ushort.Parse(revInput);
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1,C" + ut);
                        XI.RaiseGMessage("G0HQ,3," + player.Uid + "," + player.Uid + ",1," + ut);
                    }
                }
                List<string> cap = lug.Capacities.ToList();
                if (cap.Count > 0)
                {
                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1," + string.Join(",", cap));
                    XI.RaiseGMessage("G0ON," + player.Uid + ",C," + cap.Count + ","
                        + string.Join(",", cap.Select(p => p.Substring("C".Length))));
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                }
            }
        }
        public bool XBT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT4") as Base.Card.Luggage;
            if (lug != null && consumeType == 0)
            {
                if (type == 0 && XI.Board.InFight)
                {
                    // G0CC,A,0,B,TP02,17,36
                    string[] args = fuse.Split(',');
                    ushort ust = ushort.Parse(args[1]);
                    string cardname = args[4];
                    int hdx = fuse.IndexOf(';');
                    int idx = fuse.IndexOf(',', hdx);

                    Base.Card.Tux tuxBase = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    if (tuxBase.Type == Tux.TuxType.ZP)
                    {
                        string[] argv = fuse.Substring(0, hdx).Split(',');
                        List<Base.Card.Tux> cards = Util.TakeRange(argv, 5, argv.Length).Select(p =>
                            ushort.Parse(p)).Where(p => p > 0).Select(
                            p => XI.LibTuple.TL.DecodeTux(p)).ToList();
                        if (XI.Board.Garden[ust].Team == player.Team &&
                                cards.Count > 0 && !cards.Any(p => p.Type != Tux.TuxType.ZP))
                            return true;
                    }
                }
                //else if (type == 1 && XI.Board.InFight)
                //{
                //    string[] g1di = fuse.Split(',');
                //    for (int idx = 1; idx < g1di.Length; )
                //    {
                //        ushort who = ushort.Parse(g1di[idx]);
                //        bool drIn = g1di[idx + 1] == "0";
                //        int n = int.Parse(g1di[idx + 2]);
                //        if (XI.Board.Garden[who].Team == player.Team && !drIn && n > 0)
                //        {
                //            List<Base.Card.Tux> cards = Util.TakeRange(g1di, idx + 3, idx + 3 + n)
                //                .Select(p => XI.LibTuple.TL.DecodeTux(ushort.Parse(p))).ToList();
                //            if (cards.Count > 0 && !cards.Any(p => p.Type != Tux.TuxType.ZP))
                //                return true;
                //        }
                //        idx += (3 + n);
                //    }
                //}
                else if (type == 1)
                {
                    bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                        || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
                    if (meLose)
                    {
                        List<string> rms = new List<string>();
                        foreach (string tuxInfo in XI.Board.PendingTux)
                        {
                            List<ushort> accu = new List<ushort>();
                            string[] parts = tuxInfo.Split(',');
                            if (parts[1] == "XBT4Consume")
                                return true;
                        }
                    }
                }
                else if (type == 2)
                {
                    List<string> rms = new List<string>();
                    foreach (string tuxInfo in XI.Board.PendingTux)
                    {
                        List<ushort> accu = new List<ushort>();
                        string[] parts = tuxInfo.Split(',');
                        if (parts[1] == "XBT4Consume")
                            return true;
                    }
                }
                else if (type == 3)
                    return lug.Capacities.Count > 0;
            }
            return false;
        }
        public void XBT4ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT4") as Base.Card.Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT4");
            if (lug != null && type == 0)
            {
                string[] args = fuse.Split(',');
                // G0CC,A,0,B,TP02,17,36
                ushort ust = ushort.Parse(args[1]);
                string cardname = args[4];
                int hdx = fuse.IndexOf(';');
                int idx = fuse.IndexOf(',', hdx);

                int sktInType = int.Parse(Util.Substring(fuse, hdx + 1, idx));
                string sktFuse = Util.Substring(fuse, idx + 1, -1);
                Base.Card.Tux tuxBase = XI.LibTuple.TL.EncodeTuxCode(cardname);
                string[] argv = fuse.Substring(0, hdx).Split(',');

                List<ushort> cards = Util.TakeRange(argv, 5, argv.Length).Select(p =>
                    ushort.Parse(p)).Where(p => p > 0).ToList();

                ushort hst = ushort.Parse(args[3]);
                foreach (ushort p in cards)
                    XI.Board.PendingTux.Remove(hst + ",G0CC," + p);
                XI.Board.PendingTux.Enqueue(player.Uid + ",XBT4Consume," + string.Join(",", cards));
                XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + string.Join(",", cards.Select(p => "C" + p)));
                XI.InnerGMessage(fuse, 201);
            }
            //else if (lug != null && type == 1)
            //{
            //    string n1di = "";
            //    List<ushort> tuxes = new List<ushort>();
            //    string[] g1di = fuse.Split(',');
            //    for (int idx = 1; idx < g1di.Length; )
            //    {
            //        ushort who = ushort.Parse(g1di[idx]);
            //        bool drIn = g1di[idx + 1] == "0";
            //        int n = int.Parse(g1di[idx + 2]);
            //        if (XI.Board.Garden[who].Team == player.Team && !drIn && n > 0)
            //        {
            //            List<ushort> cds = Util.TakeRange(g1di, idx + 3, idx + 3 + n)
            //                .Select(p => ushort.Parse(p)).ToList();
            //            if (cds.Count > 0 && !cds.Any(p => XI.LibTuple.TL.DecodeTux(p).Type != Tux.TuxType.ZP))
            //            {
            //                XI.RaiseGMessage("G0OT," + who + "," + cds.Count + "," + string.Join(",", cds));
            //                tuxes.AddRange(cds);
            //            }
            //            else
            //                n1di += "," + string.Join(",", Util.TakeRange(g1di, idx, idx + 3 + n));
            //        }
            //        else
            //            n1di += "," + string.Join(",", Util.TakeRange(g1di, idx, idx + 3 + n));
            //        idx += (3 + n);
            //    }
            //    if (tuxes.Count > 0)
            //        XI.Board.PendingTux.Enqueue(player.Uid + "," + "XBT4Consume," + string.Join(",", tuxes));
            //    if (n1di.Length > 0)
            //        XI.InnerGMessage("G1DI" + n1di, 51);
            //}
            else if (lug != null && (type == 1 || type == 2))
            {
                List<ushort> tuxes = new List<ushort>();
                List<string> rms = new List<string>();
                foreach (string tuxInfo in XI.Board.PendingTux)
                {
                    List<ushort> accu = new List<ushort>();
                    string[] parts = tuxInfo.Split(',');
                    string utstr = parts[0];
                    if (parts[1] == "XBT4Consume")
                    {
                        tuxes.AddRange(Util.TakeRange(parts, 2, parts.Length).Select(p => ushort.Parse(p)));
                        rms.Add(tuxInfo);
                    }
                }
                foreach (string rm in rms)
                    XI.Board.PendingTux.Remove(rm);
                if (type == 1)
                {
                    string ss = string.Join(",", tuxes.Select(p => "C" + p));
                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",0," + ss);
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + ss);
                }
                else
                    XI.RaiseGMessage("G0ON,10,C," + tuxes.Count + "," + string.Join(",", tuxes));
            }
            else if (lug != null && type == 3)
            {
                List<string> cap = lug.Capacities.ToList();
                string ss = string.Join(",", cap);
                XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1," + ss);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + ss);
                XI.RaiseGMessage("G0ON," + player.Uid + ",C," + cap.Count +
                    "," + string.Join(",", cap.Select(p => p.Substring("C".Length))));
                XI.RaiseGMessage("G0IP," + player.Team + "," + cap.Count);
            }
        }
        #endregion Package of 5 - XB

        #region Equip Util
        public void EquipGeneralIncrAction(TuxEqiup te, Player player)
        {
            if (te.IncrOfSTR > 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + te.IncrOfSTR);
            else if (te.IncrOfSTR < 0)
                XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (-te.IncrOfSTR));
            if (te.IncrOfDEX > 0)
                XI.RaiseGMessage("G0IX," + player.Uid + ",0," + te.IncrOfDEX);
            else if (te.IncrOfDEX < 0)
                XI.RaiseGMessage("G0OA," + player.Uid + ",0," + (-te.IncrOfDEX));
        }
        public void EquipGeneralDecrAction(TuxEqiup te, Player player)
        {
            if (te.IncrOfSTR > 0)
                XI.RaiseGMessage("G0OA," + player.Uid + ",0," + te.IncrOfSTR);
            else if (te.IncrOfSTR < 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + (-te.IncrOfSTR));
            if (te.IncrOfDEX > 0)
                XI.RaiseGMessage("G0OX," + player.Uid + ",0," + te.IncrOfDEX);
            else if (te.IncrOfDEX < 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",0," + (-te.IncrOfDEX));
        }
        private void EquipGeneralUseAction(ushort cardUt, Player player)
        {
            XI.RaiseGMessage("G0ZB," + player.Uid + ",0," + cardUt);
        }
        #endregion Equip Util

        #region Tux Util
        private string AOthers(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Uid != py.Uid).Select(p => p.Uid)) + ")";
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
        private string ATaredTeammates(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Team == py.Team).Select(p => p.Uid)) + ")";
        }
        private string AEnemy(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == py.OppTeam).Select(p => p.Uid)) + ")";
        }
        private void Harm(Player src, Player py, int n, FiveElement five = FiveElement.A, int mask = 0)
        {
            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                new Artiad.Harm(py.Uid, src == null ? 0 : src.Uid, five, n, mask)));
        }
        private bool GeneralTuxBride(Player player)
        {
            return !player.DrTuxDisabled;
        }
        private void GeneralLocustAction(Player player, int type, string fuse,
            string argst, string cdFuse, Player locuster, Tux locus)
        {
            // G0CD,A,T,JP02,17,36;1,G0OH,...
            string[] argv = cdFuse.Split(',');
            if ((locus.IsEq[type] & 1) == 0)
                XI.RaiseGMessage("G0CE,1," + argv[1] + "," + argv[2] + ",0," + argv[3] +
                    ";" + type + "," + fuse);
            else
                XI.RaiseGMessage("G0CE,1," + argv[1] + "," + argv[2] + ",1," + argv[3] +
                    "," + argv[4] + ";" + type + "," + fuse);
            ushort owner = locuster.Uid;
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
                ushort locustee = ushort.Parse(last.Split(',')[2]);
                bool b1 = locuster.IsAlive && player.IsAlive && locus.Valid(player, type, fuse);
                if (!b1)
                    XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                else
                {
                    if ((locus.IsEq[type] & 3) == 0)
                        XI.RaiseGMessage("G0ON,10,C,1," + locustee);
                    //else if ((locus.IsEq[type] & 1) != 0)
                    //    XI.Board.PendingTux.Enqueue(locuster.Uid + ",G0ZB," + locus.Code + "," + locustee);
                    //else
                    //    XI.Board.PendingTux.Enqueue(locuster.Uid + ",G0CC," + locustee);
                    XI.Board.PendingTux.Enqueue(locuster.Uid + ",G0CC," + locustee);
                }
                XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                    "," + locus.Code + "," + locustee + ";" + type + "," + fuse, 101);
            }
            XI.InnerGMessage(cdFuse + ";" + type + "," + fuse, 106);
        }
        #endregion Tux Util
        //private bool IfFuseSatisfy(Player player, string prev, string target)
        //{
        //    if (target.Contains('#'))
        //        return prev.Contains(target.Replace("#", player.Uid.ToString()));
        //    else if (target.Contains('$'))
        //    {
        //        foreach (ushort ut in XI.Board.Garden.Keys)
        //        {
        //            if (ut != player.Uid)
        //            {
        //                if (prev.Contains(target.Replace("$", ut.ToString())))
        //                    return true;
        //            }
        //        }
        //        return false;
        //    }
        //    else if (target.Contains('*'))
        //    {
        //        foreach (ushort ut in XI.Board.Garden.Keys)
        //        {
        //            if (prev.Contains(target.Replace("*", ut.ToString())))
        //                return true;
        //        }
        //        return false;
        //    }
        //    else
        //        return prev.Contains(target);
        //}
    }
}
