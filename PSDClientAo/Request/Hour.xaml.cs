using PSD.Base.Card;
using PSD.ClientAo.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PSD.ClientAo.Request
{
    /// <summary>
    /// Interaction logic for PSDRequest.xaml
    /// </summary>
    public partial class Hour : UserControl
    {
        private PSD.Base.LibGroup lg;
        private WrapPanel[] wrapPanels;
        private bool IsGenreNotAvailable(int group)
        {
            //if (group == 0)
            //    return genre == 3 || genre == 4 || genre == 7 || genre == 9;
            //else
            //    return group == 0 || group == 3 || group > 5;
            if (group == 0)
                return false;
            else
                return !(group >= 1 && group <= 7);
        }
        private void AddContent(string prefix, int avatar, int group, int genre, bool isInTest)
        {
            AddContent(prefix + avatar, group, wrapPanels[genre], isInTest);
        }
        private void AddContent(string code, int group, WrapPanel wp, bool isInTest)
        {
            Grid grid = new Grid() { Width = 100, Height = 130 };
            Ruban ruban = null;
            if (!isInTest && IsGenreNotAvailable(group))
                ruban = Ruban.GenRubanGray(code, this, lg);
            else
                ruban = Ruban.GenRuban(code, this, lg);
            ruban.Loc = Ruban.Location.WATCH; ruban.Cat = Ruban.Category.SOUND;
            ruban.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            ruban.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            grid.Children.Add(ruban);
            if (isInTest)
            {
                Image img = new Image()
                {
                    Source = (TryFindResource("HeroInTestStamp") as Image).Source,
                    Width = 70,
                    Height = 70,
                    VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    IsHitTestVisible = false
                };
                grid.Children.Add(img);
            }
            wp.Children.Add(grid);
        }
        public Hour()
        {
            lg = new Base.LibGroup();
            InitializeComponent();
            string[] genreName = new string[] { "稻草人", "标准包", "凤鸣玉誓", "SP", "EX",
                "三世轮回", "云来奇缘", "含笑九泉", "界限突破", "宿命篇" };
            int[] genreIndex = new int[] { 1, 2, 3, 5, 6, 4, 7, 9 };
            wrapPanels = new WrapPanel[genreName.Length];
            foreach (int index in genreIndex)
            {
                GroupBox gb = new GroupBox()
                {
                    Header = new TextBlock()
                    {
                        FontSize = 24,
                        FontFamily = new FontFamily("Lisu"),
                        Text = genreName[index]
                    }
                };
                gb.Content = wrapPanels[index] = new WrapPanel();
                heroStackPanel.Children.Add(gb);
            }
            foreach (Hero hero in lg.HL.ListAllHeros(0))
                AddContent("H", hero.Avatar, hero.Group, hero.Genre, hero.AvailableTestPkg != 0);

            genreIndex = new int[] { 1, 5, 6, 7, 9 };
            foreach (int index in genreIndex)
            {
                GroupBox gb = new GroupBox()
                {
                    Header = new TextBlock()
                    {
                        FontSize = 24,
                        FontFamily = new FontFamily("Lisu"),
                        Text = genreName[index]
                    }
                };
                gb.Content = wrapPanels[index] = new WrapPanel();
                tuxStackPanel.Children.Add(gb);
            }
            foreach (Tux tux in lg.TL.ListAllTuxs(0))
                AddContent("G", tux.DBSerial, tux.Package.All(p => !IsGenreNotAvailable(p)) ? 0 : 8, tux.Genre, false);

            genreIndex = new int[] { 1, 5, 6, 7, 9 };
            foreach (int index in genreIndex)
            {
                GroupBox gb = new GroupBox()
                {
                    Header = new TextBlock()
                    {
                        FontSize = 24,
                        FontFamily = new FontFamily("Lisu"),
                        Text = genreName[index]
                    }
                };
                gb.Content = wrapPanels[index] = new WrapPanel();
                npcStackPanel.Children.Add(gb);
            }
            foreach (ushort npcCode in lg.NL.ListAllSeleable(0))
            {
                NPC npc = lg.NL.Decode(npcCode);
                AddContent("M", NMBLib.CodeOfNPC(npcCode), npc.Group, npc.Genre, false);
            }

            genreIndex = new int[] { 1, 5, 6, 7, 9 };
            foreach (int index in genreIndex)
            {
                GroupBox gb = new GroupBox()
                {
                    Header = new TextBlock()
                    {
                        FontSize = 24,
                        FontFamily = new FontFamily("Lisu"),
                        Text = genreName[index]
                    }
                };
                gb.Content = wrapPanels[index] = new WrapPanel();
                monStackPanel.Children.Add(gb);
            }
            //ushort counter = 1;
            foreach (Monster mon in lg.ML.ListAllMonster(0))
                AddContent("M", mon.DBSerial, mon.Group, mon.Genre, false);

            genreIndex = new int[] { 1, 6, 7, 9 };
            foreach (int index in genreIndex)
            {
                GroupBox gb = new GroupBox()
                {
                    Header = new TextBlock()
                    {
                        FontSize = 24,
                        FontFamily = new FontFamily("Lisu"),
                        Text = genreName[index]
                    }
                };
                gb.Content = wrapPanels[index] = new WrapPanel();
                eveStackPanel.Children.Add(gb);
            }
            ISet<Evenement> eveSet = new HashSet<Evenement>();
            foreach (ushort eveCode in lg.EL.ListAllSeleable(0))
            {
                Evenement eve = lg.EL.DecodeEvenement(eveCode);
                if (eveSet.Add(eve))
                    AddContent("E", eveCode, eve.Group, eve.Genre, false);
            }

            IDictionary<int, WrapPanel> iCardDict = new Dictionary<int, WrapPanel>();

            GroupBox gbOfRune = new GroupBox()
            {
                Header = new TextBlock()
                {
                    FontSize = 24,
                    FontFamily = new FontFamily("Lisu"),
                    Text = "标准标记"
                }
            };
            WrapPanel wpOfRune = new WrapPanel();
            gbOfRune.Content = wpOfRune;
            iCardStackPanel.Children.Add(gbOfRune);
            foreach (Base.Rune rune in lg.RL.Firsts)
            {
                ushort rnCode = lg.RL.GetSingleIndex(rune);
                AddContent("R" + rnCode, 0, wpOfRune, false);
            }
            foreach (Exsp exsp in lg.ESL.Firsts.Where(p => p.Type == 3))
            {
                string[] codes = exsp.Code.Split(',');
                if (!codes.Any(p => p.StartsWith("I")))
                    continue;
                Hero hero = lg.HL.InstanceHero(exsp.Hero);
                string code = codes.First(p => p.StartsWith("I"));
                if (!iCardDict.ContainsKey(exsp.Hero))
                {
                    GroupBox gb = new GroupBox()
                    {
                        Header = new TextBlock()
                        {
                            FontSize = 24,
                            FontFamily = new FontFamily("Lisu"),
                            Text = hero.Name
                        }
                    };
                    gb.Content = iCardDict[exsp.Hero] = new WrapPanel();
                    iCardStackPanel.Children.Add(gb);
                }
                AddContent(code, hero.Group, iCardDict[exsp.Hero], false);
            }
        }
    }
}