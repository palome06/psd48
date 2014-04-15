using PSD.Base;
using System.Windows.Controls;
using PSD.Base.Card;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;
using System.Collections.Generic;

namespace PSD.ClientAo.Tips
{
    public class IchiDisplay
    {
        private const int LSIZE = 18, SSIZE = 12;

        public static ToolTip GetHeroTip(LibGroup Tuple, int heroCode)
        {
            Hero hero = Tuple.HL.InstanceHero(heroCode);
            if (hero == null)
                return null;

            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            //trb.
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            pr.Inlines.Add(new Run(hero.Name)
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontSize = LSIZE
            });
            pr.Inlines.Add(new Run(" "));
            pr.Inlines.Add(new Run(string.Format("HP {0} 战力 {1} 命中 {2}", hero.HP, hero.STR, hero.DEX))
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = SSIZE
            });
            foreach (string skillstr in hero.Skills)
            {
                Skill skill = Tuple.SL.EncodeSkill(skillstr);
                if (skill != null)
                {
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(skill.Name)
                    {
                        Foreground = new SolidColorBrush(Colors.LawnGreen),
                        FontSize = SSIZE
                    });
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(skill.Descripe)
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = SSIZE
                    });
                }
            }
            List<string> spouses = new List<string>();
            foreach (string sp in hero.Spouses)
            {
                int hro;
                if (int.TryParse(sp, out hro))
                {
                    Hero hr = Tuple.HL.InstanceHero(hro);
                    if (hr != null)
                        spouses.Add(hr.Name);
                }
                else if (sp == "!1")
                    spouses.Add("场上任意一人");
                else if (sp == "!2")
                    spouses.Add("水魔兽");
                else if (sp == "!3")
                    spouses.Add("全体正式蜀山弟子");
                else if (sp == "!4")
                    spouses.Add("全体琼华弟子");
                else if (sp == "!5")
                    spouses.Add("指定场上一名女性");
                else if (sp == "!6")
                    spouses.Add("指定场上一人男性");
            }

            if (spouses.Count > 0)
            {
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new Run("倾慕者：")
                {
                    Foreground = new SolidColorBrush(Colors.DeepSkyBlue),
                    FontSize = SSIZE
                });
                pr.Inlines.Add(new Run(string.Join(",", spouses))
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = SSIZE
                });
            }

            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 300 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }

        public static ToolTip GetTuxTip(LibGroup Tuple, ushort tuxCode)
        {
            Tux tux = Tuple.TL.DecodeTux(tuxCode);
            if (tux == null)
                return null;

            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            //trb.
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            Color color;
            switch (tux.Type)
            {
                case Tux.TuxType.JP: color = Colors.YellowGreen; break;
                case Tux.TuxType.TP: color = Colors.LightGray; break;
                case Tux.TuxType.ZP: color = Colors.Red; break;
                case Tux.TuxType.FJ:
                case Tux.TuxType.WQ:
                case Tux.TuxType.XB: color = Colors.DeepSkyBlue; break;
                default: color = Colors.White; break;
            }
            pr.Inlines.Add(new Run(tux.Name)
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                FontSize = LSIZE
            });
            if (!string.IsNullOrEmpty(tux.Description))
            {
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new Run(tux.Description)
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = SSIZE
                });
            }
            if (!string.IsNullOrEmpty(tux.Special))
            {
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new Run("特效")
                {
                    Foreground = new SolidColorBrush(Colors.LawnGreen),
                    FontSize = SSIZE
                });
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new Run(tux.Special)
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = SSIZE
                });
            }
            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 300 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }

        public static ToolTip GetEveTip(LibGroup Tuple, ushort eveCode)
        {
            Evenement eve = Tuple.EL.DecodeEvenement(eveCode);
            if (eve == null)
                return null;

            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            pr.Inlines.Add(new Run(eve.Name)
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontSize = LSIZE
            });
            pr.Inlines.Add(new LineBreak());
            pr.Inlines.Add(new LineBreak());
            if (!string.IsNullOrEmpty(eve.Description))
            {
                pr.Inlines.Add(new Run(eve.Description)
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = SSIZE
                });
            }
            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 300 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }

        public static ToolTip GetMonTip(LibGroup Tuple, ushort monCode)
        {
            Monster mon = Tuple.ML.Decode(monCode);
            if (mon == null)
                return null;

            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            Color color; string fiveText;
            switch (mon.Element)
            {
                case FiveElement.AQUA: color = Colors.DeepSkyBlue; fiveText = "水"; break;
                case FiveElement.AGNI: color = Colors.Red;fiveText = "火";  break;
                case FiveElement.THUNDER: color = Colors.SlateBlue;fiveText = "雷";  break;
                case FiveElement.AERO: color = Colors.SpringGreen;fiveText = "风";  break;
                case FiveElement.SATURN: color = Colors.Yellow;fiveText = "土";  break;
                default: color = Colors.White;fiveText = "无属性"; break;
            }
            pr.Inlines.Add(new Run(mon.Name)
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                FontSize = LSIZE
            });
            pr.Inlines.Add(new Run(" "));
            string levelText;
            switch (mon.Level)
            {
                case Monster.ClLevel.WEAK: levelText = "弱"; break;
                case Monster.ClLevel.STRONG: levelText = "强"; break;
                case Monster.ClLevel.BOSS: levelText = "BOSS"; break;
                default: levelText = ""; break;
            }
            pr.Inlines.Add(new Run(string.Format("战力 {0} 闪避 {1} {2}", mon.STR, mon.AGL, levelText))
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = SSIZE
            });
            pr.Inlines.Add(new Run(" "));
            pr.Inlines.Add(new Run(fiveText)
            {
                Foreground = new SolidColorBrush(color),
                FontSize = SSIZE
            });
            string[] des = new string[] { mon.PetText, mon.DebutText, mon.WinText, mon.LoseText };
            string[] dis = new string[] { "宠物效果", "出场效果", "胜利效果", "失败效果" };
            for (int i = 0; i < 4; ++i)
            {
                if (!string.IsNullOrEmpty(des[i]))
                {
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(dis[i])
                    {
                        Foreground = new SolidColorBrush(Colors.LawnGreen),
                        FontSize = SSIZE
                    });
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(des[i])
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = SSIZE
                    });
                }
            }
            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 320 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }

        public static ToolTip GetNPCTip(LibGroup Tuple, ushort npcCode)
        {
            NPC npc = Tuple.NL.Decode(npcCode);
            if (npc == null)
                return null;

            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            //trb.
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            pr.Inlines.Add(new Run(npc.Name)
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontSize = LSIZE
            });
            pr.Inlines.Add(new Run(" "));
            pr.Inlines.Add(new Run(string.Format("战力 {0}", npc.STR))
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = SSIZE
            });
            bool entered = false;
            foreach (string skillstr in npc.Skills)
            {
                NCAction action = Tuple.NJL.EncodeNCAction(skillstr);
                if (action != null)
                {
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(action.Name)
                    {
                        Foreground = new SolidColorBrush(Colors.LawnGreen),
                        FontSize = SSIZE
                    });
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(action.Intro)
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = SSIZE
                    });
                    if (action.Code == "NJ01")
                        entered = true;
                }
            }
            if (entered)
            {
                Hero hero = Tuple.HL.InstanceHero(npc.Hero);
                if (hero != null)
                {
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(string.Format("加入角色属性：HP {0} 战力 {1} 命中 {2}",
                        hero.HP, hero.STR, hero.DEX))
                    {
                        Foreground = new SolidColorBrush(Colors.Gray),
                        FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                        FontSize = SSIZE
                    });
                    foreach (string skillstr in hero.Skills)
                    {
                        Skill skill = Tuple.SL.EncodeSkill(skillstr);
                        if (skill != null)
                        {
                            pr.Inlines.Add(new LineBreak());
                            pr.Inlines.Add(new LineBreak());
                            pr.Inlines.Add(new Run(skill.Name)
                            {
                                Foreground = new SolidColorBrush(Colors.DeepSkyBlue),
                                FontSize = SSIZE
                            });
                            pr.Inlines.Add(new LineBreak());
                            pr.Inlines.Add(new Run(skill.Descripe)
                            {
                                Foreground = new SolidColorBrush(Colors.White),
                                FontSize = SSIZE
                            });
                        }
                    }
                    List<string> spouses = new List<string>();
                    foreach (string sp in hero.Spouses)
                    {
                        int hro;
                        if (int.TryParse(sp, out hro))
                        {
                            Hero hr = Tuple.HL.InstanceHero(hro);
                            if (hr != null)
                                spouses.Add(hr.Name);
                        }
                        else if (sp == "!1")
                            spouses.Add("场上任意一人");
                        else if (sp == "!2")
                            spouses.Add("水魔兽");
                        else if (sp == "!3")
                            spouses.Add("全体正式蜀山弟子");
                        else if (sp == "!4")
                            spouses.Add("全体琼华弟子");
                        else if (sp == "!5")
                            spouses.Add("指定场上一名女性");
                        else if (sp == "!6")
                            spouses.Add("指定场上一人男性");
                    }
                    if (spouses.Count > 0)
                    {
                        pr.Inlines.Add(new LineBreak());
                        pr.Inlines.Add(new LineBreak());
                        pr.Inlines.Add(new Run("倾慕者：")
                        {
                            Foreground = new SolidColorBrush(Colors.IndianRed),
                            FontSize = SSIZE
                        });
                        pr.Inlines.Add(new Run(string.Join(",", spouses))
                        {
                            Foreground = new SolidColorBrush(Colors.White),
                            FontSize = SSIZE
                        });
                    }
                }
            }

            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 320 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }

        public static ToolTip GetSkillTip(Skill skill)
        {
            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            //trb.
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            pr.Inlines.Add(new Run(skill.Name)
            {
                Foreground = new SolidColorBrush(Colors.LawnGreen),
                FontSize = SSIZE
            });
            pr.Inlines.Add(new LineBreak());
            pr.Inlines.Add(new Run(skill.Descripe)
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = SSIZE
            });

            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 300 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }

        public static ToolTip GetExspTip(LibGroup Tuple, string code)
        {
            Exsp exsp = Tuple.ESL.Encode(code);
            if (exsp == null)
                return null;

            Grid mainGrid = new Grid();
            Grid gd1 = new Grid()
            {
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.6
            };
            gd1.Margin = new Thickness(-10);
            mainGrid.Children.Add(gd1);

            RichTextBox trb = new RichTextBox()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                FontFamily = new System.Windows.Media.FontFamily("SimSun"),
                BorderThickness = new Thickness(0)
            };
            //trb.
            Paragraph pr = new Paragraph() { Margin = new Thickness(0.0) };
            pr.Inlines.Add(new Run(exsp.Name)
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontSize = LSIZE
            });
            foreach (string skillstr in exsp.Skills)
            {
                Skill skill = Tuple.SL.EncodeSkill(skillstr);
                if (skill != null)
                {
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(skill.Name)
                    {
                        Foreground = new SolidColorBrush(Colors.LawnGreen),
                        FontSize = SSIZE
                    });
                    pr.Inlines.Add(new LineBreak());
                    pr.Inlines.Add(new Run(skill.Descripe)
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = SSIZE
                    });
                }
            }
            foreach (var pair in exsp.Description)
            {
                pr.Inlines.Add(new LineBreak());
                pr.Inlines.Add(new LineBreak());
                if (!string.IsNullOrEmpty(pair.Key))
                {
                    pr.Inlines.Add(new Run(pair.Key)
                    {
                        Foreground = new SolidColorBrush(Colors.LawnGreen),
                        FontSize = SSIZE
                    });
                    pr.Inlines.Add(new LineBreak());
                }
                pr.Inlines.Add(new Run(pair.Value)
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = SSIZE
                });
            }

            trb.Document = new FlowDocument();
            trb.Document.Blocks.Add(pr);

            Grid gd2 = new Grid() { Width = 300 };
            gd2.Children.Add(trb);
            mainGrid.Children.Add(gd2);

            ToolTip tt = new ToolTip();
            tt.Content = mainGrid;
            return tt;
        }
    }
}
