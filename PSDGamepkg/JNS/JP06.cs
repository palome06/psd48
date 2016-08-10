using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.JNS
{
    public class TuxCottage : JNSBase
    {
        #region Base Operations
        public TuxCottage(XI xi, Base.VW.IVI vi) : base(xi, vi) { }

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
                    tux.Action += delegate(Player player, int type, string fuse, string argst)
                    {
                        methodAction.Invoke(tc, new object[] { player, type, fuse, argst });
                    };
                var methodVestige = tc.GetType().GetMethod(cardCode + "Vestige");
                if (methodVestige != null)
                {
                    tux.Vestige += delegate(Player player, int type, string fuse, ushort it)
                    {
                        methodVestige.Invoke(tc, new object[] { player, type, fuse, it });
                    };
                }
                var methodBribe = tc.GetType().GetMethod(cardCode + "Bribe");
                if (methodBribe != null)
                    tux.Bribe += new Tux.ValidDelegate(delegate(Player player, int type, string fuse)
                    {
                        return (bool)methodBribe.Invoke(tc, new object[] { player, type, fuse });
                    });
                else if (tux.Type == Tux.TuxType.ZP)
                    tux.Bribe += new Tux.ValidDelegate(delegate(Player player, int type, string fuse)
                    {
                        return (bool)GeneralZPBribe(player);
                    });
                else if (tux.IsTuxEqiup())
                    tux.Bribe += delegate(Player player, int type, string fuse)
                    {
                        return (bool)GeneralEquipmentBribe(player, tux.Type);
                    };
                else
                    tux.Bribe += new Tux.ValidDelegate(delegate (Player player, int type, string fuse)
                    {
                        return (bool)GeneralTuxBribe(player);
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
                        tue.UseAction += delegate(ushort cardUt, Player player, ushort source)
                        {
                            methodUseAction.Invoke(tc, new object[] { cardUt, player, source });
                        };
                    else
                        tue.UseAction += delegate (ushort cardUt, Player player, ushort source)
                        {
                            EquipGeneralUseAction(cardUt, player, source);
                        };
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
                tux.Locust += new Tux.LocustActionDelegate(delegate(Player player, int type, string fuse,
                    string cdFuse, Player locuster, Tux locus, ushort it)
                {
                    var methodLocust = tc.GetType().GetMethod(cardCode + "Locust");
                    if (methodLocust != null)
                        methodLocust.Invoke(tc, new object[] { player, type, fuse, cdFuse, locuster, locus, it });
                    else
                        GeneralLocust(player, type, fuse, cdFuse, locuster, locus, it);
                });
            }
            return tx01;
        }
        #endregion Base Operations

        #region JP
        // Tou Dao
        public void JP01Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> invs = XI.Board.Garden.Values.Where(p => p.Uid != player.Uid &&
                p.IsTared && p.Tux.Except(XI.Board.ProtectedTux).Any()).Select(p => p.Uid).ToList();
            string inputFormat = (invs.Count > 0) ? "#获得其手牌,T1(p" + string.Join("p", invs) + ")" : "/";
            var ai = XI.AsyncInput(player.Uid, inputFormat, "JP01", "0");
            if (!ai.StartsWith("/"))
            {
                ushort from = ushort.Parse(ai);
                TargetPlayer(player.Uid, from);
                string c0 = Algo.RepeatString("p0",
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
            XI.RaiseGMessage("G0XZ," + player.Uid + ",2,1,2");
        }
        public bool JP02Valid(Player player, int type, string fuse)
        {
            return XI.Board.MonPiles.Count >= 2;
        }
        // Wu Qi Chao Yuan
        public void JP03Action(Player player, int type, string fuse, string argst)
        {
            List<Player> friends = XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == player.Team).ToList();
            TargetPlayer(player.Uid, friends.Select(p => p.Uid));
            Cure(player, friends, 1, FiveElement.AQUA, (long)HPEvoMask.FROM_JP);
        }
        // Shu Er Guo
        public void JP04Action(Player player, int type, string fuse, string argst)
        {
            ushort to = ushort.Parse(XI.AsyncInput(player.Uid,
                "#获得2张补牌,T1" + FormatPlayers(p => p.IsTared), "JP04", "0"));
            TargetPlayer(player.Uid, to);
            VI.Cout(0, "{0}对{1}使用「鼠儿果」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
            XI.RaiseGMessage("G0DH," + to + ",0,2");
        }
        public void JP05Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                ushort to = ushort.Parse(XI.AsyncInput(player.Uid, 
                    "#攻击,T1" + FormatPlayers(p => p.IsTared), "JP05", "0"));
                TargetPlayer(player.Uid, to);
                XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(to, player.Uid,
                    FiveElement.THUNDER, 2, (long)HPEvoMask.FROM_JP)));
            }
            else if (type == 1)
            {
                ushort to = ushort.Parse(fuse.Substring("R#EV,".Length));
                TargetPlayer(player.Uid, to);
                VI.Cout(0, "{0}对{1}使用「天雷破」.", XI.DisplayPlayer(player.Uid), XI.DisplayPlayer(to));
                XI.RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(to, player.Uid,
                    FiveElement.THUNDER, 2, (long)HPEvoMask.FROM_JP)));
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
                    TargetPlayer(player.Uid, owner);
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
            XI.RaiseGMessage("G2CL," + Algo.Substring(fuse, idx1 + 1, idx2) + "," +
                Algo.Substring(fuse, idx3 + 1, jdx));

            int kdx = fuse.IndexOf(',', jdx);
            if (kdx >= 0)
            {
                string origin = Algo.Substring(fuse, kdx + 1, -1);
                if (origin.StartsWith("G0CD"))
                    XI.InnerGMessage(origin, 1);
                else if (origin.StartsWith("G"))
                {
                    int hdx = fuse.IndexOf(';');
                    string[] argv = Algo.Substring(fuse, 0, hdx).Split(',');
                    Base.Card.Tux tux = XI.LibTuple.TL.EncodeTuxCode(argv[3]);
                    int inType = int.Parse(Algo.Substring(fuse, hdx + 1, fuse.IndexOf(',', hdx)));
                    int prior = tux.Priorities[inType];
                    if (tux.IsTermini[inType])
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
                return r != null && r.Uid == player.Uid && !r.AllTuxDisabled;
            }
            else
                return !player.AllTuxDisabled;
        }
        public void TP02Action(Player player, int type, string fuse, string args)
        {
            if (type == 0) // Use as JP
                Cure(player, player, 2);
            else if (type == 1)
            {
                List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).Select(p => p.Uid).ToList();
                string ic = invs.Count > 0 ? "T1(p" + string.Join("p", invs) + ")" : "/";
                ushort tg = ushort.Parse(XI.AsyncInput(player.Uid, ic, "TP02", "0"));
                if (invs.Contains(tg))
                {
                    TargetPlayer(player.Uid, tg);
                    Cure(player, XI.Board.Garden[tg], 2);
                }
                XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        public bool TP02Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return true;
            else if (type == 1)
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.HP == 0);
            else
                return false;
        }
        public void TP02Locust(Player player, int type, string fuse, string cdFuse, Player locuster, Tux locust, ushort locustee)
        {
            if (type == 0)
                GeneralLocust(player, 0, fuse, cdFuse, locuster, locust, locustee);
            else if (type == 1)
            {
                string[] argv = cdFuse.Split(',');
                ushort host = ushort.Parse(argv[1]);
                XI.RaiseGMessage("G0CE," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
                List<ushort> invs = XI.Board.Garden.Values.Where(p => p.IsTared && p.HP == 0).Select(p => p.Uid).ToList();
                string ic = invs.Count > 0 ? "#HP回复,T1(p" + string.Join("p", invs) + ")" : "/";
                ushort tg = ushort.Parse(XI.AsyncInput(host, ic, "TP01", "0"));
                if (invs.Contains(tg))
                {
                    TargetPlayer(player.Uid, tg);
                    VI.Cout(0, "{0}对{1}使用「灵葫仙丹」.", XI.DisplayPlayer(host), XI.DisplayPlayer(tg));
                    Cure(XI.Board.Garden[host], XI.Board.Garden[tg], 2);
                }
                bool locusSucc = false;
                if (Artiad.Procedure.LocustChangePendingTux(XI, player.Uid, locuster.Uid, locustee))
                {
                    string newFuse = "G0ZH,0";
                    if (player.IsAlive && locuster.IsAlive && locust.Valid(locuster, 0, newFuse))
                    {
                        XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                            "," + locust.Code + "," + locustee + ";0," + newFuse, 101);
                        locusSucc = true;
                    }
                }
                if (!locusSucc && XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                    XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        // Yin Gu
        public bool TP03Valid(Player player, int type, string fuse)
        {
            return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                p.N > 0 && !HPEvoMask.TUX_INAVO.IsSet(p.Mask));
        }
        public void TP03Action(Player player, int type, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            harms.RemoveAll(p => p.Who == player.Uid && !HPEvoMask.TUX_INAVO.IsSet(p.Mask));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 0);
        }
        public void TP03Locust(Player player, int type, string fuse, string cdFuse, Player locuster, Tux locust, ushort locustee)
        {
            string[] argv = cdFuse.Split(',');
            ushort host = ushort.Parse(argv[1]);
            XI.RaiseGMessage("G0CE," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            harms.RemoveAll(p => p.Who == host && !HPEvoMask.TUX_INAVO.IsSet(p.Mask));

            bool locusSucc = false;
            if (Artiad.Procedure.LocustChangePendingTux(XI, player.Uid, locuster.Uid, locustee))
            {
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
                "T1" + ATeammatesTared(player), "TP04", "0"));
            TargetPlayer(player.Uid, gamer);
            XI.RaiseGMessage("G0XZ," + gamer + ",2,0,1");
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
                return Artiad.Cure.Parse(fuse).Any(p => p.Who == player.Uid &&
                    p.N > 0 && !HPEvoMask.TERMIN_AT.IsSet(p.Mask));
            }
            else return false;
        }
        public void WQ02ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Cure> cures = Artiad.Cure.Parse(fuse);
                cures.ForEach(p => { if (p.Who == player.Uid &&
                    p.N > 0 && !HPEvoMask.TERMIN_AT.IsSet(p.Mask)) { ++p.N; } });
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
                Cure(player, player, 2);
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
                    XI.RaiseGMessage("G0CC," + provider.Uid + ",0," + user.Uid + ",TP03," + card + ";0," + pureFuse);
            }
        }
        public string FJ02ConsumeInputHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string prev)
        {
            if (prev == "")
                return "/Q1(p" + string.Join("p", provider.Tux) + ")";
            else
                return "";
        }
        public bool FJ03ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N > 0 &&
                    !HPEvoMask.TERMIN_AT.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
            }
            else return false;
        }
        public void FJ03ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
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
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -9);
            }
        }
        public bool FJ04ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N > 0 &&
                    HPEvoMask.FROM_JP.IsSet(p.Mask) && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
            }
            else return false;
        }
        public void FJ04ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.RemoveAll(p => p.Who == player.Uid && p.N > 0 &&
                    HPEvoMask.FROM_JP.IsSet(p.Mask) && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -49);
            }
        }
        public bool FJ05ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
            {
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && p.N > 0 &&
                       !HPEvoMask.DECR_INVAO.IsSet(p.Mask) && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
            }
            return false;
        }
        public void FJ05ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.RemoveAll(p => p.Who == player.Uid && p.N > 0 &&
                    !HPEvoMask.DECR_INVAO.IsSet(p.Mask) && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask));
                Cure(player, player, 1);
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 0);
            }
        }
        #endregion FJ
        #region ZP
        public bool ZP01Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void ZP01Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage(new Artiad.Goto()
            {
                CrossStage = false,
                Terminal = "R" + XI.Board.Rounder.Uid + "Z2"
            }.ToMessage());
        }
        // Tiangangzhanqi
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
        public bool ZP03Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWarSucc(player);
        }
        public void ZP03Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0IA," + player.Uid + ",1,3");
        }
        // Tianxuanwuyin
        public void ZP04Action(Player player, int type, string fuse, string argst)
        {
            ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "S", "ZP04", "0"));
            XI.RaiseGMessage("G0IP," + side + ",2");
        }
        #endregion ZP

        #region Package of 4
        public void JPT1Action(Player player, int type, string fuse, string argst)
        {
            JPT1FullOperation(player, true, type == 1);
        }
        private void JPT1FullOperation(Player player, bool needDiscard, bool domestOnly)
        {
            bool b1 = XI.Board.Garden.Values.Any(p => p.IsAlive && p.GetPetCount() > 0) && !domestOnly;
            bool b2 = ((!needDiscard || player.Tux.Count > 0) && XI.Board.Garden.Values.Any(p => p.IsAlive && p.GetPetCount() > 0 &&
                XI.Board.Garden.Values.Any(q => q.IsTared && q.Team == p.Team && p.Uid != q.Uid)));
            
            string costr;
            if (b1 && b2)
                costr = "#请选择【驯化】执行项。##开牌##驯化,Y2";
            else if (b1)
                costr = "#请选择【驯化】执行项。##开牌,Y1";
            else if (b2)
                costr = "#请选择【驯化】执行项。##驯化,Y1";
            else
                costr = "";
            costr = XI.AsyncInput(player.Uid, costr, "JPT1", "0");
            if (costr == "2" || (costr == "1" && b2 && !b1))
            {
                if (player.Tux.Count < 0)
                    XI.AsyncInput(player.Uid, "/", "JPT1", "0");
                else
                {
                    if (needDiscard)
                    {
                        string i0 = "#弃置的,Q1(p" + string.Join("p", player.Tux) + ")";
                        string qzStr = XI.AsyncInput(player.Uid, i0, "JPT1", "0");
                        ushort ut = ushort.Parse(qzStr);
                        XI.RaiseGMessage("G0QZ," + player.Uid + "," + ut);
                    }

                    string i1 = "#交出宠物的,T1(p" + string.Join("p", XI.Board.Garden.Values.Where(
                        p => p.IsTared && p.Pets.Where(q => q != 0).Any() &&
                        XI.Board.Garden.Values.Where(q => q.IsAlive &&
                            q.Uid != p.Uid && q.Team == p.Team).Any()).Select(p => p.Uid)) + ")";
                    string fromStr = XI.AsyncInput(player.Uid, i1, "JPT1", "0");
                    Player from = XI.Board.Garden[ushort.Parse(fromStr)];
                    TargetPlayer(player.Uid, from.Uid);
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
                            XI.RaiseGMessage(new Artiad.HarvestPet()
                            {
                                Farmer = to,
                                Farmland = from.Uid,
                                SinglePet = pet,
                                TreatyAct = Artiad.HarvestPet.Treaty.KOKAN
                            }.ToMessage());
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

                ushort[] uds = { player.Uid, op.Uid };
                string[] ranges = { ATeammates(player), AEnemy(player) };
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
                    XI.RaiseGMessage("G2FU,0," + uds[idxs] + ",0,C," + string.Join(",", pops));
                    input = XI.AsyncInput(uds[idxs], "+Z1(p" + string.Join("p", XI.Board.PZone) +
                        "),#获得卡牌的,/T1" + ranges[idxs], "JPT1", "0");
                    if (!input.StartsWith("/"))
                    {
                        ips = input.Split(',');
                        ushort cd;
                        if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                        {
                            ushort ut = ushort.Parse(ips[1]);
                            XI.RaiseGMessage("G1OU," + cd);
                            XI.RaiseGMessage("G2QU,0,C,0," + cd);
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
            return (type == 0 && b1) || b2;
        }
        public void JPT2Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> list = XI.Board.OrderedPlayer();
            List<Player> pys = list.Select(p => XI.Board.Garden[p]).Where(p =>
                p != null && p.IsAlive && p.Team == player.Team).ToList();
            if (pys.Count > 0)
            {
                TargetPlayer(player.Uid, pys.Select(p => p.Uid));
                XI.RaiseGMessage("G0DH," + string.Join(",", pys.Select(p => p.Uid + ",0,1")));
            }
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
                return Artiad.Harm.Parse(fuse).Any(p => p.N > 1 && XI.Board.Garden[p.Who].IsTared &&
                    !HPEvoMask.TUX_INAVO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
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
                    string whoStr = XI.AsyncInput(player.Uid, "#夺宠,T1(p" +
                        string.Join("p", targets) + ")", "TPT1", "0");
                    ushort who = ushort.Parse(whoStr);
                    string monStr = XI.AsyncInput(player.Uid, "#夺宠,/M1(p" + string.Join("p",
                        XI.Board.Garden[who] .Pets.Where(p => p != 0)) + ")", "TPT1", "0");
                    if (monStr == VI.CinSentinel)
                        break;
                    if (!monStr.StartsWith("/"))
                    {
                        TargetPlayer(player.Uid, who);
                        ushort mon = ushort.Parse(monStr);
                        XI.RaiseGMessage(new Artiad.ImperialLeft()
                        {
                            Zone = Artiad.ImperialLeft.ZoneType.M1,
                            Trigger = player.Uid,
                            Source = who,
                            Card = mon
                        }.ToMessage());
                        XI.Board.AllowNoSupport = false;
                        break;
                    }
                }
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Func<Artiad.Harm, bool> yes = (p) => p.N > 1 && XI.Board.Garden[p.Who].IsTared &&
                    !HPEvoMask.TUX_INAVO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask);
                List<ushort> invs = harms.Where(p => yes(p)).Select(p => p.Who).Distinct().ToList();

                string whoStr = XI.AsyncInput(player.Uid, "T1(p" + string.Join("p", invs) + ")", "TPT1", "0");
                ushort who = ushort.Parse(whoStr);
                TargetPlayer(player.Uid, who);

                foreach (Artiad.Harm harm in harms)
                {
                    if (yes(harm) && harm.Who == who)
                    {
                        harm.N = 1;
                        if (HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
                            harm.Mask = HPEvoMask.FINAL_MASK.Reset(harm.Mask);
                    }
                }
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 85);
            }
        }
        public void TPT1Locust(Player player, int type, string fuse, string cdFuse, Player locuster, Tux locust, ushort locustee)
        {
            if (type == 1)
            {
                string[] argv = cdFuse.Split(',');
                ushort host = ushort.Parse(argv[1]);
                XI.RaiseGMessage("G0CE," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                Func<Artiad.Harm, bool> yes = (p) => p.N > 1 && XI.Board.Garden[p.Who].IsTared &&
                    !HPEvoMask.TUX_INAVO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask);
                List<ushort> invs = harms.Where(p => yes(p)).Select(p => p.Who).Distinct().ToList();

                string whoStr = XI.AsyncInput(host, "T1(p" + string.Join("p", invs) + ")", "TPT1", "0");
                ushort who = ushort.Parse(whoStr);
                TargetPlayer(player.Uid, who);

                foreach (Artiad.Harm harm in harms)
                {
                    if (yes(harm) && harm.Who == who)
                    {
                        harm.N = 1;
                        if (HPEvoMask.TERMIN_AT.IsSet(harm.Mask))
                            harm.Mask = HPEvoMask.FINAL_MASK.Reset(harm.Mask);
                    }
                }
                bool locusSucc = false;
                if (Artiad.Procedure.LocustChangePendingTux(XI, player.Uid, locuster.Uid, locustee))
                {
                    if (harms.Any(p => yes(p)))
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
                return r != null && r.Uid == player.Uid && !r.AllTuxDisabled;
            }
            else
                return !player.AllTuxDisabled;
        }
        public void TPT2Vestige(Player player, int type, string fuse, ushort it)
        {
            if (type == 0 && it != 0)
            {
                XI.RaiseGMessage(new Artiad.EquipFakeq()
                {
                    Who = player.Uid, Source = player.Uid, Card = it, CardAs = "TPT2"
                }.ToMessage());
            }
        }
        public void TPT2Action(Player player, int type, string fuse, string argst)
        {
            if (type == 1)
            {
                string whoStr = XI.AsyncInput(player.Uid,
                    "#获得补牌的,T1" + AAllTareds(player), "TPT2Action", "0");
                ushort who = ushort.Parse(whoStr);
                XI.RaiseGMessage("G0DH," + who + ",0,1");
            }
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
                VI.Cout(0, "{0}爆发【羲和】,令其强制命中.", XI.DisplayPlayer(player.Uid));
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
                VI.Cout(0, "{0}爆发【望舒】,令其战力加倍.", XI.DisplayPlayer(player.Uid));
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + player.STRa);
            }
        }
        public bool FJT1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                return player.Tux.Count > 0 && Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid &&
                    p.N > 0 && !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask)) &&
                    XI.Board.Garden.Values.Any(p => p.Team == player.OppTeam && p.IsTared);
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
                Player py = XI.Board.Garden[to];
                // G0OH,A,Src,p,n,...
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                XI.RaiseGMessage("G0HQ,0," + to + "," + player.Uid + ",1,1," + ut);
                Artiad.Procedure.RotateHarm(player, py, true, (v) => 1, ref harms);
                if (harms.Count > 0)
                     XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -109);
            }
        }
        public string FJT1ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (prev == "")
                return "#交给对方,/Q1(p" + string.Join("p", player.Tux) + "),#交给的,/T1" + AEnemyTared(player);
            else
                return "";
        }
        public bool FJT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 1)
                return Artiad.Harm.Parse(fuse).Any(p => p.Who == player.Uid && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
            else return false;
        }
        public void FJT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                harms.ForEach(p =>
                {
                    if (p.Who == player.Uid && !HPEvoMask.DECR_INVAO.IsSet(p.Mask))
                    {
                        p.N = 1;
                        if (HPEvoMask.TERMIN_AT.IsSet(p.Mask))
                            p.Mask = HPEvoMask.FINAL_MASK.Reset(p.Mask);
                    }
                });
                if (harms.Count > 0)
                    XI.InnerGMessage(Artiad.Harm.ToMessage(harms), 85);
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            }
        }
        public void FJT2UseAction(ushort cardUt, Player player, ushort source)
        {
            TuxEqiup me = XI.LibTuple.TL.EncodeTuxCode("FJT2") as TuxEqiup;
            string tarStr = XI.AsyncInput(player.Uid, "#装备的,T1" + FormatPlayers(p =>
                p.IsTared && !me.RFM.GetOrSetUshortArray("Used").Contains(p.Uid)), "FJT2", "0");
            if (tarStr != VI.CinSentinel)
            {
                ushort tar = ushort.Parse(tarStr);
                XI.RaiseGMessage(new Artiad.EquipStandard()
                {
                    Who = tar, Source = source, SingleCard = cardUt
                }.ToMessage());
                me.RFM.GetOrSetUshortArray("Used").Add(tar);
            }
        }
        public void FJT2InsAction(Player player)
        {
            Cure(player, player, 2);
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
            if (input == "2" && xgTuxValid)
            {
                string targets = XI.AsyncInput(player.Uid, "#交换手牌,T2(p" + string.Join("p", v.Where(
                    p => p.IsTared && p.Team == player.Team && p.Tux.Count > 0).Select(p => p.Uid))
                    + ")", "JPT3", "1");
                int cmidx = targets.IndexOf(',');
                ushort iv = ushort.Parse(targets.Substring(0, cmidx));
                ushort jv = ushort.Parse(targets.Substring(cmidx + 1));
                int mn = Math.Min(3, Math.Min(g[iv].Tux.Count, g[jv].Tux.Count));
                TargetPlayer(player.Uid, new ushort[] { iv, jv });
                XI.RaiseGMessage("G1XR,2," + iv + "," + jv + ",0," + mn);
            }
            else if (input == "2" || input == "3")
            {
                string targets = XI.AsyncInput(player.Uid, "#交换宠物,T2(p" + string.Join("p", v.Where(
                    p => p.IsTared && p.Team == player.Team && p.GetPetCount() > 0).Select(p => p.Uid))
                    + ")", "JPT3", "0");
                int cmidx = targets.IndexOf(',');
                ushort iv = ushort.Parse(targets.Substring(0, cmidx));
                ushort jv = ushort.Parse(targets.Substring(cmidx + 1));
                
                string ipt = XI.AsyncInput(player.Uid, "M1(p" + string.Join("p", g[iv].Pets.Where(p => p != 0)) + ")," +
                    "M1(p" + string.Join("p", g[jv].Pets.Where(p => p != 0)) + ")", "JPT3", "2");
                ushort[] uipt = ipt.Split(',').Select(p => ushort.Parse(p)).ToArray();
                XI.RaiseGMessage(new Artiad.TradePet()
                {
                    A = iv, ASinglePet = uipt[0], B = jv, BSinglePet = uipt[1]
                }.ToMessage());
                Harm(player, player, 1, FiveElement.A, (long)HPEvoMask.FROM_JP);
            }
            else // if input == "1"
                XI.RaiseGMessage("G1EV," + player.Uid + ",1");
        }
        private void JPT4FullOperation(Player player, Func<Player, int> harmValue)
        {
            ushort to = ushort.Parse(XI.AsyncInput(player.Uid, "T1" + FormatPlayers(p => p.IsTared), "JPT4", "0"));
            TargetPlayer(player.Uid, to);
            Player py = XI.Board.Garden[to];
            Harm(player, py, harmValue(py), FiveElement.A, (long)HPEvoMask.FROM_JP);
        }
        public void JPT4Action(Player player, int type, string fuse, string argst)
        {
            JPT4FullOperation(player, (p) => Math.Max(p.GetPetCount(), 1));
        }
        public void JPT5Action(Player player, int type, string fuse, string argst)
        {
            string jpt5Name = XI.LibTuple.TL.EncodeTuxCode("JPT5").Name;
            ushort to = ushort.Parse(XI.AsyncInput(player.Uid,
                "#【" + jpt5Name + "】作用,T1" + AAllTareds(player), "JPT5", "0"));
            ushort pop = XI.Board.RestNPCPiles.Dequeue();
            XI.RaiseGMessage(new Artiad.ImperialLeft()
            {
                Zone = Artiad.ImperialLeft.ZoneType.W,
                Trigger = to,
                Card = pop
            }.ToMessage());
            ushort wang = XI.Board.Wang.Peek();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(wang));
            UEchoCode r5ed = XI.HandleWithNPCEffect(XI.Board.Garden[to], npc, "JPT5");
            if (r5ed == UEchoCode.NO_OPTIONS)
                XI.AsyncInput(to, "#无法执行,//", "JPT5", "1");
            else if (r5ed == UEchoCode.END_ACTION)
                XI.RaiseGMessage("G1YP," + player.Uid + "," + wang);

            if (XI.Board.Wang.Count > 0 && XI.Board.Wang.Peek() == wang)
            {
                XI.Board.Wang.Pop();
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = wang }
                }.ToMessage());
            }
            // XI.Board.RestNPCDises.Add(pop);
            XI.RaiseGMessage(new Artiad.ImperialLeft()
            {
                Zone = Artiad.ImperialLeft.ZoneType.W,
                IsReset = true
            }.ToMessage());
        }
        public void ZPT2Action(Player player, int type, string fuse, string argst)
        {
            string input = XI.AsyncInput(player.Uid, "#请选择执行项##命中+3##战力+2,Y2", "ZPT2", "0");
            if (input == "1")
                XI.RaiseGMessage("G0IX," + player.Uid + ",1,3");
            else
                XI.RaiseGMessage("G0IA," + player.Uid + ",1,2");
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
        public bool TPT3Valid(Player player, int type, string fuse)
        {
            string[] kekkaiA = new string[] { "TP01,0", "TPT3,0", "TPT3,1", "ZPT4,0", "TPH2,0" };
            string[] kekkaiB = new string[] { "ZP01,0", "TPT1,0", "JPH4,0" };
            // G0CD,A,T,KN,x..;TF
            if (type == 0)
            {
                int idx = fuse.IndexOf(';');
                string[] blocks = fuse.Substring(0, idx).Split(',');
                string tuxType = Algo.Substring(fuse, idx + 1, fuse.IndexOf(',', idx + 1));
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
                string tuxType = Algo.Substring(fuse, idx + 1, fuse.IndexOf(',', idx + 1));
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
        public void TPT3Vestige(Player player, int type, string fuse, ushort it)
        {
            if (type == 0)
            {
                if (!XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.Team))
                {
                    XI.AsyncInput(player.Uid, "/", "TPT3", "0"); return;
                }
                string tar = XI.AsyncInput(player.Uid, "#【净衣咒】使用,T1" + ATeammatesTared(player), "TPT3", "0");
                Player locuster = XI.Board.Garden[ushort.Parse(tar)];
                TargetPlayer(player.Uid, locuster.Uid);

                string cdFuse = Algo.Substring(fuse, 0, fuse.IndexOf(';'));
                string[] g0cd = cdFuse.Split(',');
                // Warning: self might be more than two tuxes
                // G1CW,A[1st:Org],B[2nd:Target],C[2nd:Provider],JP04;cdFuse;TF
                XI.RaiseGMessage("G1CW," + g0cd[1] + "," + tar + "," +
                    player.Uid + "," + g0cd[3] + "," + it + ";" + fuse);
            }
        }
        public void TPT3Action(Player player, int type, string fuse, string argst)
        {
            if (type == 1)
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
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit()
                    {
                        Source = player.Uid,
                        Cards = cap.Select(p => ushort.Parse(p.Substring("M".Length))).ToArray()
                    }
                }.ToMessage());
                lug.Pull = false;
            }
        }
        public bool XBT1ConsumeValid(Player player, int consumeType, int type, string fuse) {
            Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT1") as Luggage;
            if (lug != null && consumeType == 0 && !lug.Pull)
            {
                if (type == 0 && XI.Board.InCampaign && XI.Board.Rounder.Team == player.Team)
                {
                    Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
                    return ab.Genre == Card.Genre.NMB &&
                        ab.List.Any(p => p.Cards.Any(q => NMBLib.IsMonster(q)));
                }
                else if (type == 1)
                    return lug.Capacities.Sum(p => XI.LibTuple.ML.Decode(
                        ushort.Parse(p.Substring("M".Length))).STR) >= 4;
            }
            return false;
        }
        public void XBT1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT1") as Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT1");
            if (lug != null && lugCode != 0 && consumeType == 0)
            {
                if (type == 0)
                {
                    Artiad.Abandon ab = Artiad.Abandon.Parse(fuse);
                    ushort[] sns = ab.List.SelectMany(p => p.Cards)
                        .Where(p => NMBLib.IsMonster(p)).ToArray();
                    string ss = string.Join(",", sns.Select(p => "M" + p));
                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",0," + ss);
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + ss);
                    if (Artiad.CustomsHelper.RemoveCards(ab, sns))
                        XI.InnerGMessage(ab.ToMessage(), 81);
                }
                else if (type == 1)
                {
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
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new Artiad.CustomsUnit()
                        {
                            Source = player.Uid,
                            Cards = cap.Select(p => ushort.Parse(p.Substring("M".Length))).ToArray()
                        }
                    }.ToMessage());
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
        private void XBT2FullDelAction(Player player, string cardName)
        {
            Luggage lug = XI.LibTuple.TL.EncodeTuxCode(cardName) as Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial(cardName);
            if (lug != null && lug.Capacities.Count > 0)
            {
                List<string> cap = lug.Capacities.ToList();
                XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                    + ",1," + string.Join(",", cap));
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit()
                    {
                        Source = player.Uid,
                        Cards = cap.Select(p => ushort.Parse(p.Substring("C".Length))).ToArray()
                    }
                }.ToMessage());
            }
        }
        public void XBT2DelAction(Player player)
        {
            XBT2FullDelAction(player, "XBT2");
        }
        private bool XBT2FullConsumeValid(Player player, int consumeType,
            int type, string fuse, bool eqOnly, string cardName)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode(cardName) as Base.Card.Luggage;
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
                        {
                            ushort[] uts = Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
                                .Select(p => ushort.Parse(p)).ToArray();
                            if (uts.Any(p => (!eqOnly || !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup())))
                                return true;
                        }
                        idx += (4 + n);
                    }
                }
                else if (type == 1)
                {
                    bool isRd = eqOnly || (XI.Board.RoundIN == "R" + XI.Board.Rounder.Uid + "QR");
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
                                ushort[] uts = Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
                                    .Select(p => ushort.Parse(p)).ToArray();
                                uts = uts.Intersect(XI.Board.TuxDises).ToArray();
                                if (uts.Any(p => !eqOnly || !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()))
                                    return true;
                            }
                            idx += (4 + n);
                        }
                    }
                }
            }
            return false;
        }
        public bool XBT2ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            return XBT2FullConsumeValid(player, consumeType, type, fuse, false, "XBT2");
        }
        private void XBT2FullConsumeAction(Player player, int consumeType,
            int type, string fuse, string argst, string cardName)
        {
            Luggage lug = XI.LibTuple.TL.EncodeTuxCode(cardName) as Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial(cardName);
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
                            List<ushort> rms = Algo.TakeRange(g1di, idx + 4, idx + 4 + n - neq)
                                .Select(p => ushort.Parse(p)).ToList();
                            List<ushort> reqs = Algo.TakeRange(g1di, idx + 4 + n - neq, idx + 4 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            tuxes.AddRange(rms);
                            eqs.AddRange(reqs);
                        }
                        else
                            n1di += "," + string.Join(",", Algo.TakeRange(g1di, idx, idx + 4 + n));
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
                        string dhead = "#【炼蛊皿】中替换,C" + dif + "(p";
                        string dinput = XI.AsyncInput(player.Uid, dhead + string.Join("p", lug.Capacities
                            .Select(p => p.Substring("C".Length))) + ")", cardName + "Consume", "1");
                        ushort[] subs = dinput.Split(',').Select(p => ushort.Parse(p)).ToArray();
                        XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode
                            + ",1," + string.Join(",", subs.Select(p => "C" + p)));
                        XI.RaiseGMessage("G2TZ,0," + player.Uid + ","
                            + string.Join(",", subs.Select(p => "C" + p)));
                        XI.RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                            Genre = Card.Genre.Tux,
                            SingleUnit = new Artiad.CustomsUnit() { Source = player.Uid, Cards = subs }
                        }.ToMessage());
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
                                Algo.AddToMultiMap(tuxes, who, tx);
                                belongs[tx] = who;
                            }
                            for (int j = idx + 4 + n - neq; j < idx + 4 + n; ++j)
                            {
                                ushort tx = ushort.Parse(g1di[j]);
                                Algo.AddToMultiMap(eqs, who, tx);
                                belongs[tx] = who;
                            }
                        }
                        else
                            n1di += "," + string.Join(",", Algo.TakeRange(g1di, idx, idx + 4 + n));
                        idx += (4 + n);
                    }
                    ushort[] blocks = argst.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    string ss = "C" + blocks[1] + ",C" + blocks[2];
                    XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1," + ss);
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Genre.Tux,
                        SingleUnit = new Artiad.CustomsUnit()
                        {
                            Source = player.Uid,
                            Cards = new ushort[] { blocks[1], blocks[2] }
                        }
                    }.ToMessage());
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
        public void XBT2ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            XBT2FullConsumeAction(player, consumeType, type, fuse, argst, "XBT2");
        }
        private string XBT2FullConsumeInput(Player player, int consumeType, int type,
            string fuse, string prev, bool eqOnly, string cardName)
        {
            Luggage lug = XI.LibTuple.TL.EncodeTuxCode(cardName) as Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial(cardName);
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
                        {
                            ushort[] uts = Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
                               .Select(p => ushort.Parse(p)).ToArray();
                            tuxes.AddRange(uts.Where(p => !eqOnly ||
                                !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()));
                        }
                        idx += (4 + n);
                    }
                    int tcnt = tuxes.Count > 4 ? 4 : tuxes.Count;
                    string head = (tcnt > 1) ? ("/C1~" + tcnt) : "/C1";
                    return "#置入【炼蛊皿】," + head + "(p" + string.Join("p", tuxes) + ")";
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
                        {
                            ushort[] uts = Algo.TakeRange(g1di, idx + 4, idx + 4 + n)
                                  .Select(p => ushort.Parse(p)).ToArray();
                            tuxes.AddRange(uts.Where(p => !eqOnly ||
                                !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup()));
                        }
                        idx += (4 + n);
                    }
                    return "#获得的,/C1(p" + string.Join("p", tuxes) + "),#弃置的,/C2(p"
                        + string.Join("p", lug.Capacities.Select(p => p.Substring("C".Length))) + ")";
                }
            }
            return "";
        }
        public string XBT2ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            return XBT2FullConsumeInput(player, consumeType, type, fuse, prev, false, "XBT2");
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
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Genre.Tux,
                        SingleUnit = new Artiad.CustomsUnit()
                        {
                            Source = player.Uid,
                            Cards = cap.Select(p => ushort.Parse(p.Substring("C".Length))).ToArray()
                        }
                    }.ToMessage());
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                }
            }
        }
        public void XBT3ConsumeActionHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string argst)
        {
            Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Luggage;
            ushort lugCode = XI.LibTuple.TL.UniqueEquipSerial("XBT3");
            if (lug != null && consumeType == 0)
            {
                if (type == 0)
                {
                    Player player = provider;
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
                        string input = XI.AsyncInput(player.Uid, "#置入【梦见樽】," + head +
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
                    string pureFuse;
                    ushort ut = ushort.Parse(argst);
                    Tux tux = XI.LibTuple.TL.DecodeTux(ut);
                    if (!tux.IsTuxEqiup())
                    {
                        int pt = Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                            tux, provider, user, XI.Board, out pureFuse);
                        if (pt >= 0)
                        {
                            XI.RaiseGMessage("G0SN," + provider.Uid + "," + lugCode + ",1,C" + ut);
                            XI.RaiseGMessage("G2TZ,0," + provider.Uid + ",C" + ut);
                            if (!Artiad.ContentRule.IsTuxVestige(tux.Code, pt))
                                XI.RaiseGMessage(new Artiad.Abandon()
                                {
                                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                                    Genre = Card.Genre.Tux,
                                    SingleUnit = new Artiad.CustomsUnit() { Source = provider.Uid, SingleCard = ut }
                                }.ToMessage());
                            else
                                XI.Board.PendingTux.Enqueue(user.Uid + ",G0CC," + ut);
                            if (tux.Type == Base.Card.Tux.TuxType.ZP)
                                XI.RaiseGMessage("G0CZ,0," + provider.Uid);
                            XI.InnerGMessage("G0CC," + provider.Uid + ",0," + user.Uid + "," +
                                tux.Code + "," + ut + ";" + pt + "," + pureFuse, 101);
                        }
                    }
                    else
                    {
                        XI.RaiseGMessage("G0SN," + provider.Uid + "," + lugCode + ",1,C" + ut);
                        XI.RaiseGMessage("G1UE," + user.Uid + ",0," + ut);
                    }
                }
            }
        }
        public bool XBT3ConsumeValidHolder(Player provider, Player user, int consumeType, int type, string fuse)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Base.Card.Luggage;
            if (lug != null && consumeType == 0)
            {
                if (type == 0)
                {
                    Player player = provider;
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
                    return lug.Capacities.Select(p => ushort.Parse(p.Substring("C".Length))).Any(
                        p => Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                        XI.LibTuple.TL.DecodeTux(p), provider, user, XI.Board) >= 0);
                }
            }
            return false;
        }
        public string XBT3ConsumeInputHolder(Player provider, Player user, int consumeType,
            int type, string fuse, string prev)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT3") as Base.Card.Luggage;
            if (lug != null && consumeType == 0 && (type == 1 || type == 2) && prev == "")
            {
                List<ushort> candidates = lug.Capacities.Select(p => ushort.Parse(p.Substring("C".Length)))
                    .Where(p => Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                    XI.LibTuple.TL.DecodeTux(p), provider, user, XI.Board) >= 0).ToList();
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
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Genre.Tux,
                        SingleUnit = new Artiad.CustomsUnit()
                        {
                            Source = player.Uid,
                            Cards = cap.Select(p => ushort.Parse(p.Substring("C".Length))).ToArray()
                        }
                    }.ToMessage());
                    XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + string.Join(",", cap));
                }
            }
        }
        public bool XBT4ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            Base.Card.Luggage lug = XI.LibTuple.TL.EncodeTuxCode("XBT4") as Base.Card.Luggage;
            if (lug != null && consumeType == 0)
            {
                if (type == 0 && XI.Board.PlayerPoolEnabled)
                {
                    // G0CC,A,0,B,TP02,17,36
                    string[] args = fuse.Split(',');
                    ushort hst = ushort.Parse(args[3]);
                    string cardname = args[4];
                    int hdx = fuse.IndexOf(';');

                    Tux tuxBase = XI.LibTuple.TL.EncodeTuxCode(cardname);
                    if (tuxBase.Type == Tux.TuxType.ZP)
                    {
                        string[] argv = fuse.Substring(0, hdx).Split(',');
                        List<Tux> cards = Algo.TakeRange(argv, 5, argv.Length).Select(p =>
                            ushort.Parse(p)).Where(p => p > 0).Select(
                            p => XI.LibTuple.TL.DecodeTux(p)).ToList();
                        if (XI.Board.Garden[hst].Team == player.Team &&
                                cards.Count > 0 && !cards.Any(p => p.Type != Tux.TuxType.ZP))
                            return true;
                    }
                }
                else if (type == 1)
                {
                    bool meLose = (player.Team == XI.Board.Rounder.Team && !XI.Board.IsBattleWin)
                        || (player.Team == XI.Board.Rounder.OppTeam && XI.Board.IsBattleWin);
                    return meLose && XI.Board.PendingTux.Any(p => p.Split(',')[1] == "XBT4Consume");
                }
                else if (type == 2)
                    return XI.Board.PendingTux.Any(p => p.Split(',')[1] == "XBT4Consume");
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
                string[] argv = Algo.Substring(fuse, 0, fuse.IndexOf(';')).Split(',');
                ushort hst = ushort.Parse(args[3]);
                List<ushort> cards = Algo.TakeRange(argv, 5, argv.Length).Select(p =>
                    ushort.Parse(p)).Where(p => p > 0).ToList();

                cards.RemoveAll(p => !XI.Board.PendingTux.Contains(hst + ",G0CC," + p));
                if (cards.Count > 0)
                {
                    cards.ForEach(p => XI.Board.PendingTux.Remove(hst + ",G0CC," + p));
                    XI.Board.PendingTux.Enqueue(player.Uid + ",XBT4Consume," + string.Join(",", cards));
                    XI.RaiseGMessage("G2TZ," + player.Uid + ",0," + string.Join(",", cards.Select(p => "C" + p)));
                }
            }
            else if (lug != null && (type == 1 || type == 2))
            {
                List<ushort> tuxes = new List<ushort>();
                List<string> rms = new List<string>();
                foreach (string tuxInfo in XI.Board.PendingTux)
                {
                    string[] parts = tuxInfo.Split(',');
                    if (parts[1] == "XBT4Consume")
                    {
                        tuxes.AddRange(Algo.TakeRange(parts, 2, parts.Length).Select(p => ushort.Parse(p)));
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
                    XI.RaiseGMessage(new Artiad.Abandon()
                    {
                        Zone = Artiad.CustomsHelper.ZoneType.IMPLICIT,
                        Genre = Card.Genre.Tux,
                        SingleUnit = new Artiad.CustomsUnit() { Cards = tuxes.ToArray() }
                    }.ToMessage());
            }
            else if (lug != null && type == 3)
            {
                List<string> cap = lug.Capacities.ToList();
                string ss = string.Join(",", cap);
                XI.RaiseGMessage("G0SN," + player.Uid + "," + lugCode + ",1," + ss);
                XI.RaiseGMessage("G2TZ,0," + player.Uid + "," + ss);
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new Artiad.CustomsUnit()
                    {
                        Source = player.Uid,
                        Cards = cap.Select(p => ushort.Parse(p.Substring("C".Length))).ToArray()
                    }
                }.ToMessage());
                XI.RaiseGMessage("G0IP," + player.Team + "," + cap.Count);
            }
        }
        #endregion Package of 5 - XB
        #region Package of 6
        public void JPT6Action(Player player, int type, string fuse, string argst)
        {
            var ai = XI.AsyncInput(player.Uid, "#进行判定的,T1(p" + string.Join("p",
                XI.Board.Garden.Values.Where(p => p.IsTared).Select(p => p.Uid)) + ")", "JPT6", "0");
            if (!ai.StartsWith("/"))
            {
                ushort tar = ushort.Parse(ai);
                TargetPlayer(player.Uid, tar);
                XI.RaiseGMessage("G0TT," + tar);
                int val = (XI.Board.DiceValue + 1) / 2;
                Player py = XI.Board.Garden[tar];
                if (py.Team == player.Team)
                    Cure(player, py, val, FiveElement.YINN, (long)HPEvoMask.FROM_JP);
                else
                    Harm(player, py, val, FiveElement.YINN, (long)HPEvoMask.FROM_JP);
            }
        }
        public bool TPT4Valid(Player player, int type, string fuse)
        {
            if (type == 0)
            {
                return XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.Team) &&
                    XI.Board.Garden.Values.Any(p => p.IsTared && p.Team == player.OppTeam);
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                return harms.Any(p => XI.Board.Garden[p.Who].IsTared && XI.Board.Garden[p.Who].HP <= p.N);
            }
            else if (type == 2)
                return XI.Board.Garden.Values.Any(p => p.IsTared);
            else return false;
        }
        public void TPT4Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                List<string> results = new List<string>();
                int[] teams = new int[] { player.Team, player.OppTeam };
                string[] teamName = new string[] { "#弃置我方", "#弃置对方" };
                for (int i = 0; i < 2; ++i)
                {
                    List<ushort> pys = XI.Board.Garden.Values.Where(p => p.IsTared &&
                        p.Team == teams[i] && p.Runes.Count > 0).Select(p => p.Uid).ToList();
                    if (pys.Count > 0)
                    {
                        string person = XI.AsyncInput(player.Uid, teamName[i] + ",/T1(p" +
                            string.Join("p", pys) + ")", "TPT4", "0");
                        if (!string.IsNullOrEmpty(person) && !person.StartsWith("/"))
                        {
                            ushort usr = ushort.Parse(person);
                            string target = XI.AsyncInput(player.Uid, teamName[i] + ",F1(p" +
                                string.Join("p", XI.Board.Garden[usr].Runes) + ")", "TPT4", "1");
                            results.Add(usr + "," + target);
                        }
                    }
                }
                results.ForEach(p => XI.RaiseGMessage("G0OF," + p));
            }
            else if (type == 1)
            {
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<ushort> targets = harms.Where(p => XI.Board.Garden[p.Who].IsTared &&
                    XI.Board.Garden[p.Who].HP <= p.N).Select(p => p.Who).Distinct().ToList();
                string result = XI.AsyncInput(player.Uid, "#回复,T1(p" +
                    string.Join("p", targets) + ")", "TPT4", "2");
                if (!string.IsNullOrEmpty(result) && !result.StartsWith("/"))
                {
                    ushort who = ushort.Parse(result);
                    XI.RaiseGMessage("G0TT," + who);
                    int value = XI.Board.DiceValue;
                    Cure(player, XI.Board.Garden[who], value);
                }
            }
            else if (type == 2)
            {
                string result = XI.AsyncInput(player.Uid, "#回复,T1" + AAllTareds(player), "TPT4", "3");
                if (!string.IsNullOrEmpty(result) && !result.StartsWith("/"))
                {
                    ushort who = ushort.Parse(result);
                    XI.RaiseGMessage("G0TT," + who);
                    int value = XI.Board.DiceValue;
                    Cure(player, XI.Board.Garden[who], value);
                }
            }
        }
        public void TPT4Locust(Player player, int type, string fuse, string cdFuse, Player locuster, Tux locust, ushort locustee)
        {
            if (type == 0)
                GeneralLocust(player, 0, fuse, cdFuse, locuster, locust, locustee);
            else if (type == 1)
            {
                string[] argv = cdFuse.Split(',');
                ushort host = ushort.Parse(argv[1]);
                XI.RaiseGMessage("G0CE," + host + ",2,0," + argv[3] + ";" + type + "," + fuse);
                List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
                List<ushort> targets = harms.Where(p => XI.Board.Garden[p.Who].IsTared &&
                    XI.Board.Garden[p.Who].HP <= p.N).Select(p => p.Who).Distinct().ToList();
                string result = XI.AsyncInput(host, "#回复,T1(p" + string.Join("p", targets) + ")", "TPT4", "2");
                if (!string.IsNullOrEmpty(result) && !result.StartsWith("/"))
                {
                    ushort who = ushort.Parse(result);
                    XI.RaiseGMessage("G0TT," + who);
                    int value = XI.Board.DiceValue;
                    Cure(XI.Board.Garden[host], XI.Board.Garden[who], value);
                }
                bool locusSucc = false;
                if (Artiad.Procedure.LocustChangePendingTux(XI, player.Uid, locuster.Uid, locustee))
                {
                    if (player.IsAlive && locuster.IsAlive && locust.Valid(locuster, 2, fuse))
                    {
                        XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                            "," + locust.Code + "," + locustee + ";2," + fuse, 101);
                        locusSucc = true;
                    }
                }
                if (!locusSucc && XI.Board.Garden.Values.Any(p => p.IsAlive && p.HP == 0))
                    XI.InnerGMessage("G0ZH,0", 0);
            }
        }
        public void ZPT4Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0TT," + player.Uid);
            int val = XI.Board.DiceValue;
            if (val == 1 || val == 6)
                XI.RaiseGMessage("G0OA," + player.Uid + ",2");
            else
                XI.RaiseGMessage("G0IP," + player.Team + "," + val);
            XI.RaiseGMessage(new Artiad.Goto()
            {
                CrossStage = false,
                Terminal = "R" + XI.Board.Rounder.Uid + "ZN"
            }.ToMessage());
        }
        public bool ZPT4Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void ZPT5Action(Player player, int type, string fuse, string argst)
        {
            int diff = player.HPb - player.HP;
            if (diff > 0)
                XI.RaiseGMessage("G0IA," + player.Uid + ",1," + diff);
        }
        public bool ZPT5Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWarSucc(player) && player.HP < player.HPb;
        }
        public void XBT5DecrAction(Player player)
        {
            GeneralClearIllusion(player, "XBT5");
        }
        public bool XBT5ConsumeValidHolder(Player provider, Player user, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Illusion xbt5 = XI.LibTuple.TL.EncodeTuxCode("XBT5") as Illusion;
                if (type == 0 && provider.Tux.Count > 0 && xbt5.ILAS == null)
                {
                    Tux zp04 = XI.LibTuple.TL.EncodeTuxCode("ZP04");
                    return zp04 != null && zp04.Bribe(provider, type, fuse) && zp04.Valid(user, type, fuse);
                }
                else if ((type == 1 || type == 2) && xbt5.ILAS != null)
                    return true;
                else if (type == 3 && xbt5.ILAS == "FJT1" && !user.ArmorDisabled)
                {
                    TuxEqiup fjt1 = XI.LibTuple.TL.EncodeTuxCode("FJT1") as TuxEqiup;
                    return fjt1.ConsumeValid(user, 0, 0, fuse);
                }
                else
                    return false;
            }
            else
                return false;
        }
        public void XBT5ConsumeActionHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string argst)
        {
            if (consumeType == 0)
            {
                if (type == 0)
                {
                    XI.RaiseGMessage("G0CC," + provider.Uid + ",0," + user.Uid + ",ZP04," + argst + ";0," + fuse);
                    XI.RaiseGMessage("G0CZ,0," + provider.Uid);
                }
                else if (type == 1 || type == 2)
                    GeneralClearIllusion(user, "XBT5");
                else if (type == 3)
                {
                    TuxEqiup fjt1 = XI.LibTuple.TL.EncodeTuxCode("FJT1") as TuxEqiup;
                    fjt1.ConsumeAction(user, 0, 0, fuse, argst);
                }
            }
        }
        public string XBT5ConsumeInputHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string prev)
        {
            if (consumeType == 0)
            {
                if (type == 0 && prev == "")
                    return "/Q1(p" + string.Join("p", provider.Tux) + ")";
                else if (type == 3)
                {
                    TuxEqiup fjt1 = XI.LibTuple.TL.EncodeTuxCode("FJT1") as TuxEqiup;
                    return fjt1.ConsumeInput(user, 0, 0, fuse, prev);
                }
                else
                    return "";
            }
            else return "";
        }
        public void XBT6DecrAction(Player player)
        {
            GeneralClearIllusion(player, "XBT6");
        }
        public bool XBT6ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Illusion xbt6 = XI.LibTuple.TL.EncodeTuxCode("XBT6") as Illusion;
                if (type == 0 && xbt6.ILAS == null)
                {
                    return XI.Board.RestNPCPiles.Count > 0 && Artiad.Harm.Parse(fuse).Any(p => p.N > 0 &&
                        p.Who != player.Uid && XI.Board.Garden[p.Who].Team == player.Team);
                }
                else if (type == 1 && xbt6.ILAS == null)
                {
                    string[] g0zw = fuse.Split(',');
                    return Algo.TakeRange(g0zw, 1, g0zw.Length).Any(p =>
                        XI.Board.Garden[ushort.Parse(p)].IsAlive);
                }
                else if ((type == 2 || type == 3) && xbt6.ILAS != null)
                    return true;
                else
                    return false;
            }
            else if (consumeType == 1)
            {
                Illusion xbt6 = XI.LibTuple.TL.EncodeTuxCode("XBT6") as Illusion;
                if (type == 0 && xbt6.ILAS == "FJ01" && !player.ArmorDisabled)
                {
                    TuxEqiup fj01 = XI.LibTuple.TL.EncodeTuxCode("FJ01") as TuxEqiup;
                    return fj01.ConsumeValid(player, 1, 0, fuse);
                }
                else
                    return false;
            }
            else
                return false;
        }
        public void XBT6ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                if (type == 0 || type == 1)
                {
                    ushort pop = XI.Board.RestNPCPiles.Dequeue();
                    XI.RaiseGMessage(new Artiad.ImperialLeft()
                    {
                        Zone = Artiad.ImperialLeft.ZoneType.W,
                        Trigger = player.Uid,
                        Card = pop
                    }.ToMessage());
                    ushort wang = XI.Board.Wang.Peek();
                    NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(wang));
                    UEchoCode r5ed = XI.HandleWithNPCEffect(player, npc, "XBT6");
                    if (r5ed == UEchoCode.NO_OPTIONS)
                        XI.AsyncInput(player.Uid, "#无法执行,//", "XBT6ConsumeAction", "0");
                    if (r5ed == UEchoCode.END_ACTION)
                        XI.RaiseGMessage("G1YP," + player.Uid + "," + wang);

                    if (XI.Board.Wang.Count > 0 && XI.Board.Wang.Peek() == wang)
                    { // In case the NPC has been taken away
                        XI.RaiseGMessage(new Artiad.ImperialLeft()
                        {
                            Zone = Artiad.ImperialLeft.ZoneType.W,
                            IsReset = true
                        }.ToMessage());
                        XI.RaiseGMessage(new Artiad.Abandon()
                        {
                            Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                            Genre = Card.Genre.NMB,
                            SingleUnit = new Artiad.CustomsUnit() { SingleCard = wang }
                        }.ToMessage());
                    }
                }
                else if (type == 2 || type == 3)
                    GeneralClearIllusion(player, "XBT6");
            }
            else if (consumeType == 1)
            {
                if (type == 0)
                {
                    TuxEqiup fj01 = XI.LibTuple.TL.EncodeTuxCode("FJ01") as TuxEqiup;
                    fj01.ConsumeAction(player, 1, 0, fuse, argst);
                }
            }
        }
        public string XBT6ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 1)
            {
                if (type == 0)
                {
                    TuxEqiup fj01 = XI.LibTuple.TL.EncodeTuxCode("FJ01") as TuxEqiup;
                    return fj01.ConsumeInput(player, 1, 0, fuse, prev);
                }
                else
                    return "";
            }
            else return "";
        }
        public void XBT7DecrAction(Player player)
        {
            GeneralClearIllusion(player, "XBT7");
        }
        public bool XBT7ConsumeValidHolder(Player provider, Player user, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
            {
                Illusion xbt7 = XI.LibTuple.TL.EncodeTuxCode("XBT7") as Illusion;
                if ((type == 0 || type == 1) && provider.Tux.Count >= 2 && 
                    !provider.RFM.GetOrSetDiva("XBT7").GetBool("Used") && xbt7.ILAS == null)
                {
                    string linkFuse = fuse;
                    int lfidx = linkFuse.IndexOf(':');
                    string pureFuse = linkFuse.Substring(lfidx + 1);
                    foreach (string linkHead in linkFuse.Substring(0, lfidx).Split('&'))
                    {
                        if (linkHead.StartsWith("XBT7"))
                            continue;
                        string[] lh = linkHead.Split(',');
                        string pureName = lh[0], pureTypeStr = lh[1], rawOc = lh[2];

                        if (!pureTypeStr.Contains("!") && Artiad.ContentRule.IsFuseMatch(rawOc, pureFuse, XI.Board))
                        {
                            ushort pureType = ushort.Parse(pureTypeStr);
                            Tux tux = XI.LibTuple.TL.EncodeTuxCode(pureName);
                            if (tux != null && XI.LibTuple.TL.IsTuxInGroup(tux, XI.PCS.Level) &&
                                tux.Bribe(provider, pureType, pureFuse) && tux.Valid(user, pureType, pureFuse))
                                return true;
                        }
                    }
                }
                else if ((type == 2 || type == 3) && xbt7.ILAS != null)
                    return true;
                return false;
            }
            else if (consumeType == 1)
            {
                Illusion xbt7 = XI.LibTuple.TL.EncodeTuxCode("XBT7") as Illusion;
                if (type == 0 && xbt7.ILAS == "FJ05" && !user.ArmorDisabled)
                {
                    TuxEqiup fj05 = XI.LibTuple.TL.EncodeTuxCode("FJ05") as TuxEqiup;
                    return fj05.ConsumeValid(user, 1, 0, fuse);
                }
                else return false;
            }
            else return false;
        }
        public void XBT7ConsumeActionHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string argst)
        {
            if (consumeType == 0)
            {
                if (type == 0 || type == 1)
                {
                    string[] args = argst.Split(',');
                    ushort udb = ushort.Parse(args[0]);
                    string uts = args[1] + "," + args[2];
                    Tux tux = XI.LibTuple.TL.EncodeTuxDbSerial(udb);

                    string pureFuse;
                    int pureType = Artiad.ContentRule.GetTuxTypeFromLink(fuse,
                        tux, provider, user, XI.Board, out pureFuse);
                    if (tux.Type == Tux.TuxType.ZP)
                        XI.RaiseGMessage("G0CZ,0," + provider.Uid);
                    XI.RaiseGMessage("G0CC," + provider.Uid + ",0," + user.Uid + "," +
                        tux.Code + "," + uts + ";" + pureType + "," + pureFuse);
                    provider.RFM.GetOrSetDiva("XBT7").Set("Used", true);
                }
                else if (type == 2 || type == 3)
                    GeneralClearIllusion(user, "XBT7");
            }
            else if (consumeType == 1)
            {
                if (type == 0)
                {
                    TuxEqiup fj05 = XI.LibTuple.TL.EncodeTuxCode("FJ05") as TuxEqiup;
                    fj05.ConsumeAction(user, 1, 0, fuse, argst);
                }
            }
        }
        public string XBT7ConsumeInputHolder(Player provider, Player user, int consumeType,
            int type, string fuse, string prev)
        {
            if (consumeType == 0)
            {
                if ((type == 0 || type == 1) && prev == "")
                {
                    ISet<ushort> dbs = new HashSet<ushort>();

                    string linkFuse = fuse;
                    int lfidx = linkFuse.IndexOf(':');
                    // linkHeads = { "TP02,0", "TP03,0" };
                    string[] linkHeads = linkFuse.Substring(0, lfidx).Split('&');
                    string pureFuse = linkFuse.Substring(lfidx + 1);

                    for (int i = 0; i < linkHeads.Length; ++i)
                    {
                        if (linkHeads[i].StartsWith("XBT7"))
                            continue;
                        string[] lh = linkHeads[i].Split(',');
                        string pureName = lh[0], pureTypeStr = lh[1], rawOc = lh[2];
                        if (!pureTypeStr.Contains("!") && Artiad.ContentRule.IsFuseMatch(rawOc, pureFuse, XI.Board))
                        {
                            ushort pureType = ushort.Parse(pureTypeStr);
                            Tux tux = XI.LibTuple.TL.EncodeTuxCode(pureName);
                            if (tux != null && XI.LibTuple.TL.IsTuxInGroup(tux, XI.PCS.Level))
                            {
                                if (tux.Bribe(provider, pureType, pureFuse) && tux.Valid(user, pureType, pureFuse))
                                    dbs.Add(tux.DBSerial);
                            }
                        }
                    }
                    return "#转化,/G1(p" + string.Join("p", dbs) + ")";
                }
                else if ((type == 0 || type == 1) && prev.IndexOf(',') < 0)
                {
                    ushort db = ushort.Parse(prev);
                    Tux tux = XI.LibTuple.TL.EncodeTuxDbSerial(db);
                    List<ushort> hands = provider.Tux.Where(p =>
                        tux.IsSameType(XI.LibTuple.TL.DecodeTux(p))).ToList();
                    if (hands.Count >= 2)
                        return "/Q2(p" + string.Join("p", hands) + ")";
                    else
                        return "/";
                }
                else return "";
            }
            else if (consumeType == 1)
            {
                if (type == 0 && prev == "")
                {
                    TuxEqiup fj05 = XI.LibTuple.TL.EncodeTuxCode("FJ05") as TuxEqiup;
                    return fj05.ConsumeInput(user, 1, 0, fuse, prev);
                }
                else return "";
            }
            else return "";
        }
        #endregion Package of 6

        #region Package of HL
        public void JPH1Action(Player player, int type, string fuse, string argst)
        {
            string whoStr = XI.AsyncInput(player.Uid, "#攻击,T1" + AAllTareds(player), "JPH1", "0");
            ushort who = ushort.Parse(whoStr);
            Player py = XI.Board.Garden[who];
            TargetPlayer(player.Uid, who);
            if (!py.HasAnyCards())
                return;
            int n = py.GetEquipCount();
            string select = XI.AsyncInput(who, "#请响应【风卷尘生】##弃置" + (n + 1) + "张牌" +
                (n == 0 ? ",Y1" : "##弃置装备,Y2"), "JPH1", "1");
            if (select == "2")
            {
                XI.RaiseGMessage("G0QZ," + who + "," + string.Join(",", py.ListOutAllEquips()));
                XI.RaiseGMessage("G0DH," + who + ",0," + n);
            }
            else
            {
                int k = Math.Min(n + 1, py.GetAllCardsCount());
                string ts = XI.AsyncInput(who, "#弃置的,Q" + k + "(p" + string.Join(
                    "p", py.ListOutAllCards()), "JPH1", "2");
                XI.RaiseGMessage("G0QZ," + who + "," + ts);
            }
        }
        public bool JPH1Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.HasAnyCards());
        }
        public void JPH2Action(Player player, int type, string fuse, string argst)
        {
            string whoStr = XI.AsyncInput(player.Uid, "#攻击,T1" +
                FormatPlayers(p => p.IsTared && p.HasAnyEquips()), "JPH2", "0");
            ushort who = ushort.Parse(whoStr);
            Player py = XI.Board.Garden[who];
            TargetPlayer(player.Uid, who);
            int n = Math.Min(py.GetEquipCount() + 2, 5);
            string select = XI.AsyncInput(who, string.Format("#请响应【{0}】##HP-{1}##弃置装备,Y2",
                XI.LibTuple.TL.EncodeTuxCode("JPH2").Name, n), "JPH2", "0");
            if (select == "1")
                Harm(player, py, n, FiveElement.AERO, (long)HPEvoMask.FROM_JP);
            else if (py.GetEquipCount() > 3)
            {
                string eqSel = XI.AsyncInput(who, "#弃置,Q3(p" + string.Join("p", 
                    py.ListOutAllEquips()) + ")", "JPH2", "1");
                XI.RaiseGMessage("G0QZ," + who + "," + eqSel);
            }
            else if (py.HasAnyEquips())
                XI.RaiseGMessage("G0QZ," + who + "," + string.Join(",", py.ListOutAllEquips()));
        }
        public bool JPH2Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && p.HasAnyEquips());
        }
        public void JPH3Action(Player player, int type, string fuse, string argst)
        {
            string whoStr = XI.AsyncInput(player.Uid, "V1(p" + string.Join("p", FiveElementHelper.GetPropedElements()
                .Select(p => p.Elem2Int())) + "),#请选择【七光御阵】执行项##伤害弃牌##回复补牌,Y2", "JPH3", "0");
            int idx = whoStr.IndexOf(',');
            FiveElement five = FiveElementHelper.Int2Elem(int.Parse(whoStr.Substring(0, idx)));
            int elemIdx = five.Elem2Index();
            XI.RaiseGMessage(new Artiad.AnnouceCard()
            {
                Action = Artiad.AnnouceCard.Type.FLASH,
                Officer = player.Uid,
                Genre = Card.Genre.Five,
                SingleCard = (ushort)five.Elem2Int()
            }.ToMessage());
            string selection = whoStr.Substring(idx + 1);
            if (selection == "2")
            {
                List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Pets[elemIdx] == 0).ToList();
                if (invs.Count > 0)
                {
                    Cure(player, invs, 1, five, (long)HPEvoMask.FROM_JP);
                    XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p.Uid + ",0,1")));
                }
            }
            else
            {
                List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive && p.Pets[elemIdx] != 0).ToList();
                if (invs.Count > 0)
                    Harm(player, invs, invs.Select(p => p.GetPetCount()), five, (long)HPEvoMask.FROM_JP);
                invs = invs.Where(p => p.Tux.Count > 0).ToList();
                if (invs.Count > 0)
                    XI.RaiseGMessage("G0DH," + string.Join(",", invs.Select(p => p.Uid + ",1," + p.GetPetCount())));
            }
        }
        public void JPH4Action(Player player, int type, string fuse, string argst)
        {
            List<ushort> picks = Artiad.Procedure.CardHunter(XI, Card.PileGenre.NMB,
                (p) => NMBLib.IsMonster(p), (a, r) => a.Count == 3, true);
            if (picks.Count > 0)
                XI.RaiseGMessage("G1IU," + string.Join(",", picks));
            Player nx = XI.Board.GetOpponenet(player);
            while (XI.Board.PZone.Count > 0)
            {
                XI.RaiseGMessage("G2FU,0," + nx.Uid + ",0,M," + string.Join(",", XI.Board.PZone));
                string input = XI.AsyncInput(nx.Uid, "+M1(p" + string.Join("p", XI.Board.PZone) +
                    "),#获得宠物的,/T1" + ATeammates(nx), "JPH4", "0");
                if (!input.Contains(VI.CinSentinel) && !input.StartsWith("/"))
                {
                    string[] ips = input.Split(',');
                    ushort cd;
                    if (ushort.TryParse(ips[0], out cd) && XI.Board.PZone.Contains(cd))
                    {
                        ushort ut = ushort.Parse(ips[1]);
                        XI.RaiseGMessage("G1OU," + cd);
                        XI.RaiseGMessage("G2QU,0,M,0," + cd);
                        XI.RaiseGMessage(new Artiad.HarvestPet()
                        {
                            Farmer = ut,
                            SinglePet = cd,
                            Plow = false,
                        }.ToMessage());
                    }
                }
                XI.RaiseGMessage("G2FU,3");
            }
            //while (popCount < 3)
            //{
            //    List<ushort> pys = XI.Board.Garden.Values.Where(p =>
            //        p.Team == player.Team && p.GetPetCount() > 0).Select(p => p.Uid).ToList();
            //    if (pys.Count == 0)
            //        break;
            //    string inputWho = XI.AsyncInput(player.Uid, "#弃置宠物,T1(p" + string.Join("p", pys), "JPH4", "1");
            //    if (!inputWho.Contains(VI.CinSentinel))
            //    {
            //        ushort who = ushort.Parse(inputWho);
            //        List<ushort> pts = XI.Board.Garden[who].Pets.Where(p => p != 0).ToList();
            //        string inputMon = XI.AsyncInput(player.Uid, "#弃置宠物,/M1" + (pts.Count > 1 ?
            //            ("~" + pts.Count) : "") + "(p" + string.Join("p", pts) + ")", "JPH4", "2");
            //        if (!inputMon.Contains(VI.CinSentinel) && !inputMon.StartsWith("/"))
            //        {
            //            ushort[] pets = inputMon.Split(',').Select(p => ushort.Parse(p)).ToArray();
            //            XI.RaiseGMessage(new Artiad.LosePet()
            //            {
            //                Owner = who,
            //                Pets = pets
            //            }.ToMessage());
            //            popCount += pets.Length;
            //        }
            //    }
            //}
            XI.RaiseGMessage("G1WJ,0");
        }
        public bool JPH4Valid(Player player, int type, string fuse)
        {
            return XI.Board.MonPiles.Count > 0;
        }
        public void TPH1Action(Player player, int type, string fuse, string argst)
        {
            if (type == 0)
            {
                string whoStr = XI.AsyncInput(player.Uid,
                    "#获得「神算」的,T1" + AAllTareds(player), "TPH1", "0");
                ushort who = ushort.Parse(whoStr);
                TargetPlayer(player.Uid, who);
                XI.RaiseGMessage("G0IF," + who + ",7");
            }
            else if (type == 1)
            {
                XI.RaiseGMessage("G2YS,T," + player.Uid + ",M,1");
                string sel = XI.AsyncInput(player.Uid, "#调整怪物闪避##-1##+1,Y2", "TPH1", "1");
                if (sel == "1")
                    XI.RaiseGMessage("G0OW," + XI.Board.Monster1 + ",1");
                else if (sel == "2")
                    XI.RaiseGMessage("G0IW," + XI.Board.Monster1 + ",1");
            }
        }
        public bool TPH1Valid(Player player, int type, string fuse)
        {
            if (type == 0)
                return XI.Board.Garden.Values.Any(p => p.IsTared);
            else if (type == 1)
                return true;
            return false;
        }
        public void TPH2Action(Player player, int type, string fuse, string argst)
        {
            Artiad.HarvestPet hpt = Artiad.HarvestPet.Parse(fuse);
            ushort which;
            if (hpt.Pets.Length > 1)
            {
                string whichSel = XI.AsyncInput(player.Uid, "#弃置,M1(p" +
                    string.Join("p", hpt.Pets) + ")", "TPH2", "0");
                which = ushort.Parse(whichSel);
            }
            else
                which = hpt.Pets[0];
            TargetPlayer(player.Uid, hpt.Farmer);
            Monster monster = XI.LibTuple.ML.Decode(which);
            int pts = 0;
            if (monster.Level == Monster.ClLevel.WEAK) pts = 2;
            else if (monster.Level == Monster.ClLevel.STRONG) pts = 4;
            else if (monster.Level == Monster.ClLevel.BOSS) pts = 6;
            // monster1/2 -> remove it direclty
            // from other places -> discard it
            if (hpt.Farmland != 0)
                XI.RaiseGMessage(new Artiad.LosePet() { Owner = hpt.Farmland, SinglePet = which }.ToMessage());
            else
                XI.RaiseGMessage(new Artiad.Abandon()
                {
                    Zone = Artiad.CustomsHelper.ZoneType.EXPLICIT,
                    Genre = Card.Genre.NMB,
                    SingleUnit = new Artiad.CustomsUnit() { SingleCard = which }
                }.ToMessage());
            while (pts > 0)
            {
                string incrSel = XI.AsyncInput(hpt.Farmer, "#获得标记(剩余" + pts + "枚),/T1" +
                    AAlls(player) + "),#获得标记(剩余" + pts + "枚),/F1" + StdRunes(), "TPH2", "1");
                if (incrSel.StartsWith("/"))
                    break;
                else if (incrSel != VI.CinSentinel)
                {
                    int idx = incrSel.IndexOf(',');
                    ushort hptTar = ushort.Parse(incrSel.Substring(0, idx));
                    ushort hptRune = ushort.Parse(incrSel.Substring(idx + 1));
                    TargetPlayer(hpt.Farmer, hptTar);
                    XI.RaiseGMessage("G0IF," + hptTar + "," + hptRune);
                    --pts;
                }
            }
            List<ushort> rests = hpt.Pets.Where(p => p != which).ToList();
            if (rests.Count > 0)
            {
                hpt.Pets = rests.ToArray();
                XI.InnerGMessage(hpt.ToMessage(), 70);
            }
        }
        public bool TPH2Valid(Player player, int type, string fuse)
        {
            if (Artiad.KittyHelper.IsHarvest(fuse))
            {
                Artiad.HarvestPet hpt = Artiad.HarvestPet.Parse(fuse);
                return XI.Board.Garden[hpt.Farmer].IsTared && hpt.Trophy;
            }
            else return false;
        }
        public void TPH3Action(Player player, int type, string fuse, string argst)
        {
            Player rd = XI.Board.Rounder;
            if (player.Uid != rd.Uid)
            {
                TargetPlayer(player.Uid, rd.Uid);
                int ptc = player.Tux.Count, rtc = rd.Tux.Count;
                XI.RaiseGMessage("G0HQ,4," + player.Uid + "," + rd.Uid + "," + ptc + "," + rtc +
                    (ptc > 0 ? ("," + string.Join(",", player.Tux)) : "") +
                    (rtc > 0 ? ("," + string.Join(",", rd.Tux)) : ""));
                if (player.Team != rd.Team && ptc <= rtc - 3)
                {
                    XI.RaiseGMessage("G0DS," + player.Uid + ",0,1");
                    Harm(player, player, 2);
                }
            }
        }
        public void TPH4Action(Player player, int type, string fuse, string argst)
        {
            XI.RaiseGMessage("G0XZ," + player.Uid + ",3,1,2,1");
        }
        public bool TPH4Valid(Player player, int type, string fuse)
        {
            return XI.Board.EvePiles.Count >= 2;
        }
        public void ZPH1Action(Player player, int type, string fuse, string argst)
        {
            string ai = XI.AsyncInput(player.Uid, "#命中-2,T1" + AAllTareds(player), "ZPH1", "0");
            ushort to = ushort.Parse(ai);
            TargetPlayer(player.Uid, to);
            XI.RaiseGMessage("G0OX," + to + ",1,2");
        }
        public bool ZPH1Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared);
        }
        public void ZPH2Action(Player player, int type, string fuse, string argst)
        {
            string ai = XI.AsyncInput(player.Uid, "#额外参战,T1" + FormatPlayers(
                p => p.IsTared && !XI.Board.IsAttendWar(p)), "ZPH2", "0");
            ushort attender = ushort.Parse(ai);
            TargetPlayer(player.Uid, attender);
            XI.RaiseGMessage(new Artiad.CoachingSign() { SingleUnit = new Artiad.CoachingSignUnit()
            {
                Role = Artiad.CoachingHelper.PType.EX_ENTER, Coach = attender
            } }.ToMessage());
        }
        public bool ZPH2Valid(Player player, int type, string fuse)
        {
            return XI.Board.IsAttendWar(player) && XI.Board.Garden.Values.Any(
                p => p.IsTared && !XI.Board.IsAttendWar(p));
        }
        public void ZPH3Action(Player player, int type, string fuse, string argst)
        {
            string tuxName = XI.LibTuple.TL.EncodeTuxCode("ZPH3").Name;
            string ai = XI.AsyncInput(player.Uid, "#【" + tuxName + "】作用,T1" +
                FormatPlayers(p => p.IsTared && XI.Board.IsAttendWar(p) && p.GetPetCount() > 0), "ZPH3", "0");
            ushort to = ushort.Parse(ai);
            TargetPlayer(player.Uid, to);
            Player py = XI.Board.Garden[to];
            int nt = Math.Min(py.GetPetCount(), py.Tux.Count);
            if (nt > 0)
            {
                string sel = XI.AsyncInput(to, "#弃置的,Q" + nt + "(p" + string.Join("p", py.Tux) + ")," +
                    string.Format("#请选择【{0}】执行项##战力+{1}##命中+{1}##HP+{1},Y3", tuxName, nt), "ZPH3", "1");
                int idx = sel.LastIndexOf(',');
                string tuxes = sel.Substring(0, idx);
                XI.RaiseGMessage("G0QZ," + to + "," + tuxes);
                ushort usel = ushort.Parse(sel.Substring(idx + 1));
                if (usel == 3)
                    Cure(player, py, nt);
                else if (usel == 2)
                    XI.RaiseGMessage("G0IX," + to + ",1," + nt);
                else if (usel == 1)
                    XI.RaiseGMessage("G0IA," + to + ",1," + nt);
            }
        }
        public bool ZHP3Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Any(p => p.IsTared && XI.Board.IsAttendWar(p) && p.GetPetCount() > 0);
        }

        public bool WQH1ConsumeValidHolder(Player provider, Player user, int consumeType, int type, string fuse)
        {
            if (consumeType == 0 && provider.Tux.Count >= 2)
            {
                int lfidx = fuse.IndexOf(':');
                string pureFuse = fuse.Substring(lfidx + 1);
                Tux zp03 = XI.LibTuple.TL.EncodeTuxCode("ZP03");
                return zp03 != null && zp03.Bribe(provider, type, pureFuse) && zp03.Valid(user, type, pureFuse);
            }
            else
                return false;
        }
        public void WQH1ConsumeActionHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string argst)
        {
            if (consumeType == 0)
            {
                int lfidx = fuse.IndexOf(':');
                string pureFuse = fuse.Substring(lfidx + 1);
                XI.RaiseGMessage("G0CC," + provider.Uid + ",0," + user.Uid + ",ZP03," + argst + ";0," + pureFuse);
                XI.RaiseGMessage("G0CZ,0," + provider.Uid);
            }
        }
        public string WQH1ConsumeInputHolder(Player provider, Player user, int consumeType, int type,
            string fuse, string prev)
        {
            if (consumeType == 0 && prev == "")
                return "/Q2(p" + string.Join("p", provider.Tux) + ")";
            else return "";
        }
        public bool FJH1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            return player.Uid == XI.Board.Rounder.Uid && fuse.Split(',').Contains("L");
        }
        public void FJH1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
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
                        XI.RaiseGMessage(Artiad.Harm.ToMessage(
                            new Artiad.Harm(player.Uid, (monCode + 1000), monster.Element, 1, 0)));
                    }
                }
            }
        }
        public void FJH1UseAction(ushort cardUt, Player player, ushort source)
        {
            TuxEqiup me = XI.LibTuple.TL.EncodeTuxCode("FJH1") as TuxEqiup;
            string tarStr = XI.AsyncInput(player.Uid, "#装备的,T1" + FormatPlayers(p =>
                p.IsTared && !me.RFM.GetOrSetUshortArray("Used").Contains(p.Uid)), "FJH1", "0");
            if (tarStr != VI.CinSentinel)
            {
                ushort tar = ushort.Parse(tarStr);
                XI.RaiseGMessage(new Artiad.EquipStandard()
                {
                    Who = tar, Source = source, SingleCard = cardUt
                }.ToMessage());
                me.RFM.GetOrSetUshortArray("Used").Add(tar);
            }
        }
        public void FJH1DelAction(Player player)
        {
            if (player.IsAlive)
                Cure(player, player, 1);
        }
        public bool XBH1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            if (consumeType == 0)
                return XI.Board.Garden.Values.Any(p => p.Uid != player.Uid && p.IsTared) && XI.Board.Battler != null;
            return false;
        }
        public void XBH1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            if (consumeType == 0)
            {
                ushort selfCode = XI.LibTuple.TL.UniqueEquipSerial("XBH1");
                ushort tar = ushort.Parse(argst);
                Player py = XI.Board.Garden[tar];
                TargetPlayer(player.Uid, tar);
                if (py.Tux.Count > 0)
                {
                    string sel = XI.AsyncInput(tar, "#交出的,Q1(p" + string.Join("p", py.Tux) + ")", "XBH1", "0");
                    if (!sel.Contains(VI.CinSentinel))
                    {
                        ushort tux = ushort.Parse(sel);
                        XI.RaiseGMessage("G0HQ,0," + player.Uid + "," + tar + ",1,1," + tux);
                    }
                }
                XI.RaiseGMessage("G0HQ,0," + tar + "," + player.Uid + ",0,1," + selfCode);
            }
        }
        public string XBH1ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            if (consumeType == 0 && prev == "")
                return "#交予的,/T1" + FormatPlayers(p => p.IsTared && p.Uid != player.Uid);
            return "";
        }

        #endregion Package of HL

        #region Renewed
        public bool TPR1Valid(Player player, int type, string fuse)
        {
           return XI.Board.Garden.Values.Any(p => p.IsTared);
        }
        public void TPR1Action(Player player, int type, string fuse, string argst)
        {
           if (type == 0)
           {
               string whoStr = XI.AsyncInput(player.Uid, "#获得标记的,T1" + FormatPlayers(p => p.IsTared) +
                    ",F1(p" + string.Join("p", XI.LibTuple.RL.GetFullPositive()) + ")", "TPR1Action", "0");
               XI.RaiseGMessage("G0IF," + whoStr);
           }
           else if (type == 1)
           {
               string whoStr = XI.AsyncInput(player.Uid, "#获得补牌的,T1" +
                   FormatPlayers(p => p.IsTared), "TPR1Action", "1");
               ushort who = ushort.Parse(whoStr);
               XI.RaiseGMessage("G0DH," + who + ",0,1");
           }
        }
        public bool JPR1Valid(Player player, int type, string fuse)
        {
            return XI.Board.Garden.Values.Where(p => p.IsAlive && p.GetPetCount() > 0).Any();
        }
        public void JPR1Action(Player player, int type, string fuse, string argst)
        {
            JPT1FullOperation(player, false, type == 1);
        }
        public void JPR2Action(Player player, int type, string fuse, string argst)
        {
            JPT4FullOperation(player, (p) => Math.Max(1, Math.Max(p.GetPetCount(), p.GetBaseEquipCount())));
        }
        public void XBR1InsAction(Player player)
        {
            XBT2InsAction(player);
        }
        public void XBR1DelAction(Player player)
        {
            XBT2FullDelAction(player, "XBR1");
        }
        public bool XBR1ConsumeValid(Player player, int consumeType, int type, string fuse)
        {
            return XBT2FullConsumeValid(player, consumeType, type, fuse, true, "XBR1");
        }
        public void XBR1ConsumeAction(Player player, int consumeType, int type, string fuse, string argst)
        {
            XBT2FullConsumeAction(player, consumeType, type, fuse, argst, "XBR1");
        }
        public string XBR1ConsumeInput(Player player, int consumeType, int type, string fuse, string prev)
        {
            return XBT2FullConsumeInput(player, consumeType, type, fuse, prev, true, "XBR1");
        }
        #endregion Renewed

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
        private void EquipGeneralUseAction(ushort cardUt, Player player, ushort source)
        {
            XI.RaiseGMessage(new Artiad.EquipStandard()
            {
                Who = player.Uid, Source = source, SingleCard = cardUt
            }.ToMessage());
        }
        #endregion Equip Util

        #region Tux Util
        private bool GeneralTuxBribe(Player player)
        {
            return !player.AllTuxDisabled;
        }
        private bool GeneralZPBribe(Player player)
        {
            return player.RestZP > 0 && !player.ZPDisabled && !player.AllTuxDisabled;
        }
        private bool GeneralEquipmentBribe(Player player, Tux.TuxType tuxType)
        {
            return Artiad.ClothingHelper.IsEquipable(player, tuxType);
        }
        private void GeneralLocust(Player player, int type, string fuse, string cdFuse,
            Player locuster, Tux locus, ushort locustee)
        {
            // G0CD,A,T,JP02,17,36;1,G0OH,...
            string[] argv = cdFuse.Split(',');
            string cardName = argv[3];
            if (!Artiad.ContentRule.IsTuxVestige(cardName, type))
                XI.RaiseGMessage("G0CE," + argv[1] + "," + argv[2] + ",0," + cardName +
                    ";" + type + "," + fuse);
            else
                XI.RaiseGMessage("G0CE," + argv[1] + "," + argv[2] + ",1," + cardName +
                    "," + argv[4] + ";" + type + "," + fuse);

            if (Artiad.Procedure.LocustChangePendingTux(XI, player.Uid, locuster.Uid, locustee))
            {
                XI.InnerGMessage("G0CC," + player.Uid + ",1," + locuster.Uid +
                    "," + locus.Code + "," + locustee + ";" + type + "," + fuse, 101);
            }
            XI.InnerGMessage(cdFuse + ";" + type + "," + fuse, 106);
        }
        // private void GeneralIncrIllusion(Player player, string illCode, ushort callUt, ushort asDbSerial)
        // {
        //     XI.RaiseGMessage("G0QZ," + player.Uid + "," + callUt);
        //     TuxEqiup tue = XI.LibTuple.TL.EncodeTuxDbSerial(asDbSerial) as TuxEqiup;
        //     Illusion ill = XI.LibTuple.TL.EncodeTuxCode(illCode) as Illusion;
        //     ill.ILAS = tue.Code; // TODO: change into a standard NGT(G0UL)
        //     XI.RaiseGMessage(new Artiad.EqImport()
        //     {
        //         SingleUnit = new Artiad.CardAsUnit()
        //         {
        //             Who = player.Uid,
        //             Card = ill.SingleEntry,
        //             CardAs = ill.ILAS
        //         }
        //     }.ToMessage());
        // }
        // private void GeneralIncrIllusion(Player player, string illCode, string argst)
        // {
        //     int idx = argst.IndexOf(',');
        //     GeneralIncrIllusion(player, illCode, ushort.Parse(argst.Substring(0, idx)),
        //         ushort.Parse(argst.Substring(idx + 1)));
        // }
        private void GeneralClearIllusion(Player player, string illCode)
        {
            Illusion ill = XI.LibTuple.TL.EncodeTuxCode(illCode) as Illusion;
            if (ill != null && ill.ILAS != null)
            {
                XI.RaiseGMessage(new Artiad.EqExport()
                {
                    SingleUnit = new Artiad.CardAsUnit()
                    {
                        Who = player.Uid,
                        Card = ill.SingleEntry,
                        CardAs = ill.ILAS
                    }
                }.ToMessage());
            }
            ill.ILAS = null;
        }
        // private bool GeneralIllusionValid(Player player, string illCode, string[] candidates)
        // {
        //     if (player.Tux.Count == 0)
        //         return false;
        //     foreach (string tuxCode in candidates)
        //     {
        //         TuxEqiup tue = XI.LibTuple.TL.EncodeTuxCode(tuxCode) as TuxEqiup;
        //         if (tue == null)
        //             continue;
        //         if (XI.Board.Garden.Values.Any(p => p.ListOutAllEquips().Contains(tue.SingleEntry)))
        //             continue;
        //         if (player.GetSlotCapacity(tue.Type) <= player.GetCurrentEquipCount(tue.Type))
        //             continue;
        //         return true;
        //     }
        //     return false;
        // }
        // private string GeneralIllusionInput(Player player, string illCode, string[] candidates)
        // {
        //     List<ushort> invs = new List<ushort>();
        //     foreach (string tuxCode in candidates)
        //     {
        //         TuxEqiup tue = XI.LibTuple.TL.EncodeTuxCode(tuxCode) as TuxEqiup;
        //         if (tue == null)
        //             continue;
        //         if (XI.Board.Garden.Values.Any(p => p.ListOutAllEquips().Contains(tue.SingleEntry)))
        //             continue;
        //         if (player.GetSlotCapacity(tue.Type) <= player.GetCurrentEquipCount(tue.Type))
        //             continue;
        //         invs.Add(tue.DBSerial);
        //     }
        //     return "#幻化弃置,/Q1(p" + string.Join("p", player.Tux) + "),#幻化,/G1(p" + string.Join("p", invs) + ")";
        // }
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