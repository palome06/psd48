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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace PSD.ClientAo.OI
{
    /// <summary>
    /// Interaction logic for Arena.xaml
    /// </summary>
    public partial class Arena : UserControl
    {
        private Base.LibGroup mTuple;
        public Base.LibGroup Tuple
        {
            set { AoArena = new AoArena(this, mTuple = value); }
            get { return mTuple; }
        }

        public AoDisplay AD { set; get; }
        public AoArena AoArena { private set; get; }
        public ushort Rank { get { return AD.SelfUid; } }

        public Arena()
        {
            InitializeComponent();
        }

        public void FinishArena()
        {
            mainBoard.Children.Clear();
            Visibility = Visibility.Collapsed;
        }

        private int Line { set; get; }

        // Cst :[1-4]DT02(4x1),[5-8]DT01(4x2),[9-16]DT03(8x2)
        internal void CstPick(List<Ruban> cands)
        {
            //AoArena = new AoArenaPick(this, mTuple);
            mainBoard.Children.Clear();
            //selectedList = new ObservableCollection<Ruban>();
            int csz = cands.Count;
            if (csz <= 4)
                Line = 4;
            else if (csz <= 8)
                Line = 4;
            else if (csz <= 16)
                Line = 8;
            else
                Line = (csz + 1) / 2;

            int idx = 0, jdx = 0;
            if (csz <= 4)
            {
                //mainGrid.Height = 188; mainGrid.Width = 520;
                mainGrid.Height = 170; mainGrid.Width = 400;
                mainGrid.Background = FindResource("dt02Bg") as ImageBrush;
            }
            else if (csz <= 8)
            {
                mainGrid.Height = 285; mainGrid.Width = 400;
                mainGrid.Background = FindResource("dt01Bg") as ImageBrush;
            }
            else
            {
                mainGrid.Height = 285; mainGrid.Width = 760;
                mainGrid.Background = FindResource("dt03Bg") as ImageBrush;
            }
            foreach (Ruban ruban in cands)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.ACTIVE;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (input != null)
                        input(ruban.UT.ToString());
                    ruban.cardBody.IsChecked = false;
                };
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            okButton.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }

        internal bool Switch(int from, Ruban to)
        {
            List<Ruban> list = new List<Ruban>();
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null)
                    list.Add(ru);
            }
            foreach (Ruban ru in list)
                mainBoard.Children.Remove(ru);
            Ruban ichi = list.Find(p => p.UT == to.UT);
            if (ichi == null)
                ichi = list.Find(p => p.UT == 0);
            if (ichi != null)
                list.Remove(ichi);

            Ruban ni = list.Find(p => p.UT == from);
            if (ni == null)
                return false;
            list.Remove(ni);
            // Update $to
            to.Loc = Ruban.Location.DEAL;
            to.Cat = Ruban.Category.ACTIVE;
            to.LengthLimit = (int)(mainGrid.Width - 30);
            to.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (input != null)
                    input(to.UT.ToString());
                to.cardBody.IsChecked = false;
            };
            to.Index = ni.Index; to.Jndex = ni.Jndex;
            list.Add(to);

            foreach (Ruban ru in list)
            {
                mainBoard.Children.Add(ru);
                if (ichi != null && ru.Jndex == ichi.Jndex && ru.Index >= ichi.Index)
                    --ru.Index;
                int count = list.Count(p => p.Jndex == ru.Jndex);
                ru.SetOfIndex(ru.Index, ru.Jndex, count);
            }
            return true;
        }

        internal void CstTable(List<Ruban> x, List<Ruban> baka, List<Ruban> bao)
        {
            mainBoard.Children.Clear();
            //selectedList = new ObservableCollection<Ruban>();
            int csz = x.Count + baka.Count + bao.Count;
            if (csz <= 4)
                Line = 4;
            else if (csz <= 8)
                Line = 4;
            else if (csz <= 16)
                Line = 8;
            else
                Line = (csz + 1) / 2;
            int idx = 0, jdx = 0;
            if (csz <= 4)
            {
                //mainGrid.Height = 188; mainGrid.Width = 520;
                mainGrid.Height = 170; mainGrid.Width = 400;
                mainGrid.Background = FindResource("dt02Bg") as ImageBrush;
            }
            else if (csz <= 8)
            {
                mainGrid.Height = 285; mainGrid.Width = 400;
                mainGrid.Background = FindResource("dt01Bg") as ImageBrush;
            }
            else
            {
                mainGrid.Height = 285; mainGrid.Width = 760;
                mainGrid.Background = FindResource("dt03Bg") as ImageBrush;
            }
            foreach (Ruban ruban in x)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.SOUND;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (input != null)
                        input(ruban.UT.ToString());
                    ruban.cardBody.IsChecked = false;
                };
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            foreach (Ruban ruban in baka)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.AKA_MASK;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            foreach (Ruban ruban in bao)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.AO_MASK;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            okButton.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }
        //internal void Decide(ushort p1, int p2)
        //{
        //    throw new NotImplementedException();
        //}
        internal void Remove(int heroCode)
        {
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null && ru.UT == heroCode)
                {
                    mainBoard.Children.Remove(ru);
                    break;
                }
            }
        }
        internal void ActiveArena(int[] jdxs)
        {
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null && ru.Cat == Ruban.Category.SOUND)
                    if (jdxs == null || jdxs.Contains(ru.Jndex))
                        ru.Cat = Ruban.Category.ACTIVE;
            }
        }
        internal void DisactiveArena(int[] jdxs)
        {
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null && ru.Cat == Ruban.Category.ACTIVE)
                    if (jdxs == null || jdxs.Contains(ru.Jndex))
                        ru.Cat = Ruban.Category.SOUND;
            }
        }
        internal void BanBy(bool isAka, int heroCode)
        {
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null && ru.UT == heroCode)
                {
                    if (isAka)
                        ru.Cat = Ruban.Category.AKA_MASK;
                    else
                        ru.Cat = Ruban.Category.AO_MASK;
                    break;
                }
            }
        }
        internal void PuckBack(Ruban ruban)
        {
            int ti = 0, tj = 0;
            // Jndex, List of Index
            IDictionary<int, List<int>> dict = new Dictionary<int, List<int>>();
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null)
                {
                    if (!dict.ContainsKey(ru.Jndex))
                        dict.Add(ru.Jndex, new List<int>());
                    dict[ru.Jndex].Add(ru.Index);
                }
            }
            int shortIndex = -1, shortLen = 48;
            foreach (var pair in dict)
            {
                List<int> lst = pair.Value;
                lst.Sort();
                for (int i = 1; i < lst.Count; ++i)
                    if (lst[i] - lst[i - 1] != 1)
                    {
                        ti = i; tj = pair.Key;
                        goto handle;
                    }
                if (lst.Count < shortLen)
                {
                    shortIndex = pair.Key; shortLen = lst.Count;
                }
            }
            if (shortLen < 48) { ti = shortLen; tj = shortIndex; }
            else { ti = 0; tj = 0; }
        handle:
            ruban.Loc = Ruban.Location.DEAL;
            ruban.Cat = Ruban.Category.ACTIVE;
            ruban.LengthLimit = (int)(mainGrid.Width - 30);
            mainBoard.Children.Add(ruban);
            ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (input != null)
                    input(ruban.UT.ToString());
                ruban.cardBody.IsChecked = false;
            };
            ruban.SetOfIndex(ti, tj, Line);
            return;
        }

        internal void CstPublic(List<Ruban> x, List<Ruban> baka,
            List<Ruban> bao, List<Ruban> paka, List<Ruban> pao)
        {
            mainBoard.Children.Clear();
            int csz = x.Count + baka.Count + bao.Count;
            Line = 9;
            mainGrid.Height = 565; mainGrid.Width = 850;
            mainGrid.Background = FindResource("dt04Bg") as ImageBrush;
            int idx = 0, jdx = 1;
            foreach (Ruban ruban in x)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.SOUND;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (input != null)
                        input(ruban.UT.ToString());
                    //ruban.cardBody.IsChecked = false;
                };
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            foreach (Ruban ruban in baka)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.AKA_MASK;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            foreach (Ruban ruban in bao)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.AO_MASK;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx, jdx, Line);
                if (++idx >= Line) { idx -= Line; ++jdx; }
            }
            bool isAo = Rank > 0 && Rank < 1000 && Rank % 2 == 0;
            idx = 0; jdx = isAo ? 0 : 3;
            foreach (Ruban ruban in paka)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.SOUND;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx++, jdx, Line);
            }
            idx = 0; jdx = isAo ? 3 : 0;
            foreach (Ruban ruban in pao)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.SOUND;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx++, jdx, Line);
            }
            okButton.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }
        // operatable
        internal void CstCongress(List<Ruban> xme, List<Ruban> xop, Ruban me,
            Ruban right, Ruban left, bool captain, bool operatable)
        {
            mainBoard.Children.Clear();
            //selectedList = new ObservableCollection<Ruban>();
            int csz = xme.Count;
            Line = 9;
            int idx = 0;
            mainGrid.Height = 422; mainGrid.Width = 850;
            mainGrid.Background = FindResource("dt05Bg") as ImageBrush;

            ShipRule shipRule = new ShipRule();
            shipRule.ZoneList.Add(new ShipRule.Zone(0, Line, 2, 2, ShipRule.AlignStyle.ALIGN));
            shipRule.ZoneList.Add(new ShipRule.Zone(4, 4, 1, 1, ShipRule.AlignStyle.STAY));
            if (captain)
            {
                shipRule.ZoneList.Add(new ShipRule.Zone(2, 2, 1, 1, ShipRule.AlignStyle.STAY));
                shipRule.ZoneList.Add(new ShipRule.Zone(6, 6, 1, 1, ShipRule.AlignStyle.STAY));
            }
            foreach (Ruban ruban in xme)
            {
                ruban.Loc = Ruban.Location.DEAL;
                if (operatable)
                    ruban.Cat = Ruban.Category.PISTON;
                else
                    ruban.Cat = Ruban.Category.SOUND;
                ruban.ShipRule = shipRule;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (input != null)
                        input(ruban.UT.ToString());
                    ruban.cardBody.IsChecked = false;
                };
                ruban.moveCaller += delegate(int oI, int oJ, int nI, int nJ)
                {
                    if (nJ == 1)
                    {
                        if (captain)
                        {
                            if (nI == 2 || nI == 4 || nI == 6)
                            {
                                int target = Rank + (nI - 4);
                                if (target <= 0) target += 6;
                                if (target > 6) target -= 6;
                                if (input != null)
                                    input(target + "," + ruban.UT.ToString());
                            }
                        }
                        else
                        {
                            if (nI == 4 && input != null)
                                input(ruban.UT.ToString());
                        }
                    }
                    else if (oJ == 1 && nJ == 2)
                    {
                        if (captain)
                        {
                            if (nI == 2 || nI == 4 || nI == 6)
                            {
                                int target = Rank + (nI - 4);
                                if (input != null)
                                    input(target + ",0");
                            }
                        }
                        else
                        {
                            if (input != null)
                                input("0");
                        }
                    }
                };
                ruban.SetOfIndex(idx++, 2, Line);
            }
            idx = 0;
            foreach (Ruban ruban in xop)
            {
                ruban.Loc = Ruban.Location.DEAL;
                ruban.Cat = Ruban.Category.SOUND;
                ruban.LengthLimit = (int)(mainGrid.Width - 30);
                mainBoard.Children.Add(ruban);
                ruban.SetOfIndex(idx++, 0, Line);
            }
            Ruban[] friends = new Ruban[] { me, right, left };
            int[] fridx = new int[] { 4, 6, 2 };
            for (int i = 0; i < 3; ++i)
            {
                Ruban ruban = friends[i];
                if (ruban != null)
                {
                    ruban.Loc = Ruban.Location.DEAL;
                    ruban.Cat = Ruban.Category.PISTON;
                    ruban.LengthLimit = (int)(mainGrid.Width - 30);
                    ruban.ShipRule = shipRule;
                    mainBoard.Children.Add(ruban);
                    ruban.SetOfIndex(fridx[i], 1, Line);
                    ruban.moveCaller += delegate(int oI, int oJ, int nI, int nJ)
                    {
                        if (nJ == 1)
                        {
                            if (captain)
                            {
                                if (nI == 2 || nI == 4 || nI == 6)
                                {
                                    int target = Rank + (nI - 4);
                                    if (target <= 0) target += 6;
                                    if (target > 6) target -= 6;
                                    if (input != null)
                                        input(target + "," + ruban.UT.ToString());
                                }
                            }
                            else
                            {
                                if (nI == 4 && input != null)
                                    input(ruban.UT.ToString());
                            }
                        }
                        else if (oJ == 1 && nJ == 2)
                        {
                            if (captain)
                            {
                                if (nI == 2 || nI == 4 || nI == 6)
                                {
                                    int target = Rank + (nI - 4);
                                    if (input != null)
                                        input(target + ",0");
                                }
                            }
                            else
                            {
                                if (input != null)
                                    input("0");
                            }
                        }
                    };
                }
            }
            if (operatable)
                okButton.Visibility = Visibility.Visible;
            else
                okButton.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }

        internal void PickBy(ushort puid, int selAva)
        {
            bool found = false;
            if (selAva != 0)
            {
                foreach (var elem in mainBoard.Children)
                {
                    Ruban ru = elem as Ruban;
                    if (ru != null && ru.UT == selAva)
                    {
                        bool self = Rank > 0 && Rank < 1000 && (puid % 2 == Rank % 2);
                        self = self || (Rank < 0 && Rank >= 1000 && puid % 2 == 1);
                        if (self)
                        {
                            int count = 0;
                            foreach (var elem2 in mainBoard.Children)
                            {
                                Ruban ru2 = elem2 as Ruban;
                                if (ru2 != null && ru2.Jndex == 3)
                                    ++count;
                            }
                            ru.SetOfIndex(count, 3, 9);
                        }
                        else
                        {
                            int count = 0;
                            foreach (var elem2 in mainBoard.Children)
                            {
                                Ruban ru2 = elem2 as Ruban;
                                if (ru2 != null && ru2.Jndex == 0)
                                    ++count;
                            }
                            ru.SetOfIndex(count, 0, 9);
                        }
                        ru.cardBody.IsChecked = false;
                        ru.Cat = Ruban.Category.SOUND;
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                bool self = Rank > 0 && Rank < 1000 && (puid % 2 == Rank % 2);
                self = self || (Rank < 0 && Rank >= 1000 && puid % 2 == 1);
                Ruban oldRuban = null;
                List<Ruban> cands = new List<Ruban>();
                foreach (var elem in mainBoard.Children)
                {
                    Ruban ru = elem as Ruban;
                    if (ru != null && ru.UT == 0 && (ru.Jndex == 1 || ru.Jndex == 2))
                    {
                        cands.Add(ru);
                        if (ru.cardBody.IsChecked == true)
                        {
                            oldRuban = ru; break;
                        }
                    }
                }
                cands.Shuffle();
                if (oldRuban == null)
                    oldRuban = cands.First();
                if (selAva != 0)
                {
                    mainBoard.Children.Remove(oldRuban);
                    Ruban nru = Ruban.GenRuban("H" + selAva, this, Tuple);
                    nru.Loc = Ruban.Location.DEAL;
                    nru.Cat = Ruban.Category.SOUND;
                    nru.LengthLimit = (int)(mainGrid.Width - 30);
                    int count = 0;
                    int jtag = self ? 3 : 0;
                    foreach (var elem2 in mainBoard.Children)
                    {
                        Ruban ru2 = elem2 as Ruban;
                        if (ru2 != null && ru2.Jndex == jtag)
                            ++count;
                    }
                    nru.SetOfIndex(count, jtag, 9);
                    mainBoard.Children.Add(nru);
                    nru.cardBody.IsChecked = false;
                }
                else
                {
                    int count = 0;
                    int jtag = self ? 3 : 0;
                    foreach (var elem2 in mainBoard.Children)
                    {
                        Ruban ru2 = elem2 as Ruban;
                        if (ru2 != null && ru2.Jndex == jtag)
                            ++count;
                    }
                    oldRuban.SetOfIndex(count, jtag, 9);
                    oldRuban.cardBody.IsChecked = false;
                    oldRuban.Cat = Ruban.Category.SOUND;
                }
            }
        }

        internal void CongressDing(ushort puid, int heroCode, bool captain)
        {
            int ii = (puid - Rank + 6 + 4) % 6;
            if (ii == 0) ii = 6;
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null && ru.UT == heroCode)
                {
                    if (ru.Jndex == 2)
                    {
                        foreach (var elem2 in mainBoard.Children)
                        {
                            Ruban rub = elem2 as Ruban;
                            if (rub != null && rub.Jndex == 2 && rub.Index > ru.Index)
                                rub.SetOfIndex(rub.Index - 1, 2, 8);
                        }
                        ru.SetOfIndex(ii, 1, 8);
                    }
                    else if (ru.Jndex == 1)
                        ru.SetOfIndex(ii, 1, 8);
                    if (!captain && puid != Rank)
                        ru.Cat = Ruban.Category.SOUND;
                    ru.cardBody.IsChecked = false;
                    return;
                }
            }
        }
        internal void CongressBack(int heroCode)
        {
            int count = 0;
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru.Jndex == 2)
                    ++count;
            }
            foreach (var elem in mainBoard.Children)
            {
                Ruban ru = elem as Ruban;
                if (ru != null && ru.UT == heroCode)
                {
                    if (ru.Jndex != 2)
                    {
                        ru.SetOfIndex(count, 2, count + 1);
                        ru.Cat = Ruban.Category.PISTON;
                    } // else if ru.Jndex, then uid has adjusted itself.
                    return;
                }
            }
        }
        //// logic : input("10501")->Raise("H0SL,1,10501")->Return("H0DC,1,10501")
        //internal bool DoPickSwitch(Ruban ruban, int current)
        //{
        //    if (ruban != null)
        //    {
        //        foreach (var elem in mainGrid.Children)
        //        {
        //            Ruban ru = elem as Ruban;
        //            if (ru != null && ru.UT == current)
        //            {
        //                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
        //                {
        //                    if (input != null)
        //                        input(ruban.UT.ToString());
        //                    ruban.cardBody.IsChecked = false;
        //                };
        //                ruban.SetOfIndex(ru.Index, ru.Jndex, mainGrid.Children.Count);
        //                mainGrid.Children.Add(ruban);
        //                mainGrid.Children.Remove(ru);
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        //private void CstPick(ushort ut)
        //{
        //    if (input != null)
        //        input(ut.ToString());
        //    AoArenaPick aap = AoArena as AoArenaPick;
        //    if (aap != null)
        //        aap.OnPick(ut);
        //    FinishArena();
        //}
        //private void CstBP(Ruban ruban)
        //{
        //    if (input != null)
        //        input(ruban.UT.ToString());
        //    AoArenaPublic aap = AoArena as AoArenaPublic;
        //    if (aap != null)
        //    {
        //        bool isPick = aap.BP(ruban.UT);
        //        if (!isPick)
        //            ruban.Cat = Ruban.Category.LUMBERJACK;
        //        else
        //        {
        //            ruban.SetOfIndex()
        //        }
        //    }
        //}

        //internal void CstPublic(List<Ruban> pool, List<Ruban> bans,
        //    List<Ruban> rs, List<Ruban> os)
        //{
        //    //AoArena = new AoArenaPublic(this, mTuple);
        //    mainBoard.Children.Clear();
        //    int csz = pool.Count + bans.Count + rs.Count + os.Count;
        //    Line = csz / 2; 
        //    int idx = 0, jdx = 1;
        //    foreach (Ruban ruban in pool)
        //    {
        //        ruban.Loc = Ruban.Location.DEAL;
        //        ruban.Cat = Ruban.Category.PISTON;
        //        mainBoard.Children.Add(ruban);
        //        ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
        //        {
        //            if (input != null)
        //                input(ruban.UT.ToString());
        //            ruban.cardBody.IsChecked = false;
        //        };
        //        ruban.SetOfIndex(idx, jdx, Line);
        //        if (++idx >= Line) { idx -= Line; ++jdx; }
        //    }
        //    foreach (Ruban ruban in bans)
        //    {
        //        ruban.Loc = Ruban.Location.DEAL;
        //        ruban.Cat = Ruban.Category.LUMBERJACK;
        //        mainBoard.Children.Add(ruban);
        //        ruban.SetOfIndex(idx, jdx, Line);
        //        if (++idx >= Line) { idx -= Line; ++jdx; }
        //    }
        //    idx = 0;
        //    foreach (Ruban ruban in os)
        //    {
        //        ruban.Loc = Ruban.Location.DEAL;
        //        ruban.Cat = Ruban.Category.SOUND;
        //        mainBoard.Children.Add(ruban);
        //        ruban.SetOfIndex(idx, 0, Line);
        //        ++idx;
        //    }
        //    idx = 0;
        //    foreach (Ruban ruban in rs)
        //    {
        //        ruban.Loc = Ruban.Location.DEAL;
        //        ruban.Cat = Ruban.Category.SOUND;
        //        mainBoard.Children.Add(ruban);
        //        ruban.SetOfIndex(idx, 3, Line);
        //        ++idx;
        //    }
        //    mainGrid.Height = 655; mainGrid.Width = 850;
        //    mainGrid.Background = FindResource("dt04Bg") as ImageBrush;
        //    this.Visibility = Visibility.Visible;
        //}

        //public void DoPublicBan(ushort ut)
        //{
        //    foreach (var elem in mainGrid.Children)
        //    {
        //        Ruban ru = elem as Ruban;
        //        if (ru != null && ru.UT == ut)
        //        {
        //            ru.Cat = Ruban.Category.LUMBERJACK;
        //            break;
        //        }
        //    }
        //}
        //public void DoPublicPick(ushort ut, bool self)
        //{
        //    Ruban ruban = null;
        //    int jdx = self ? 3 : 0;
        //    int idx = 0;
        //    foreach (var elem in mainGrid.Children)
        //    {
        //        Ruban ru = elem as Ruban;
        //        if (ru != null && ru.Jndex == jdx)
        //            ++idx;
        //        if (ru != null && ru.UT == ut)
        //            ruban = ru;
        //    }
        //    if (ruban != null)
        //    {
        //        ruban.Cat = Ruban.Category.LUMBERJACK;
        //        ruban.SetOfIndex(idx, jdx, Line);
        //    }
        //}
        //public void DoPublicMarkPick(int which)
        //{
        //    foreach (var elem in mainGrid.Children)
        //    {
        //        Ruban ru = elem as Ruban;
        //        if (ru != null && ru.UT == which)
        //        {
        //            mainGrid.Children.Remove(ru);
        //            return;
        //        }
        //    }
        //}

        //private int pLSel, p0Sel, pRSel;

        //internal void CstCong(List<Ruban> rrs,
        //    List<Ruban> ors, Ruban pL, Ruban p0, Ruban pR)
        //{
        //    //AoArena = new AoArenaCong(this, mTuple);
        //    mainBoard.Children.Clear();
        //    int csz = rrs.Count;
        //    int idx = 0;
        //    foreach (Ruban ruban in rrs)
        //    {
        //        ruban.Loc = Ruban.Location.DEAL;
        //        ruban.Cat = Ruban.Category.PISTON;
        //        mainBoard.Children.Add(ruban);
        //        ruban.moveCaller += delegate(int oI, int oJ, int nI, int nJ)
        //        {
        //            if (nJ == 1) // TODO: set as centre position
        //            {
        //                AoArenaCong aac = AoArena as AoArenaCong;
        //                bool finished;
        //                if (nI == 3)
        //                    finished = aac.SetPlayer(2, ruban.UT);
        //                else
        //                    finished = false;
        //                if (finished)
        //                    okButton.Visibility = Visibility.Visible;
        //            }
        //            if (oJ == 1)
        //            {

        //            }
        //        };
        //        ruban.SetOfIndex(idx++, 2, Line);
        //    }
        //    idx = 0;
        //    foreach (Ruban ruban in ors)
        //    {
        //        ruban.Loc = Ruban.Location.DEAL;
        //        ruban.Cat = Ruban.Category.SOUND;
        //        mainBoard.Children.Add(ruban);
        //        ruban.SetOfIndex(idx++, 0, Line);
        //    }
        //    if (pL != null)
        //    {
        //        pL.Loc = Ruban.Location.DEAL;
        //        pL.Cat = Ruban.Category.PISTON;
        //        mainBoard.Children.Add(pL);
        //        pL.SetOfIndex(1, 1, Line);
        //    }
        //    if (p0 != null)
        //    {
        //        p0.Loc = Ruban.Location.DEAL;
        //        p0.Cat = Ruban.Category.PISTON;
        //        mainBoard.Children.Add(p0);
        //        p0.SetOfIndex(3, 1, Line);
        //    }
        //    if (pR != null)
        //    {
        //        pR.Loc = Ruban.Location.DEAL;
        //        pR.Cat = Ruban.Category.PISTON;
        //        mainBoard.Children.Add(pR);
        //        pR.SetOfIndex(5, 1, Line);
        //    }
        //    mainGrid.Height = 445; mainGrid.Width = 850;
        //    mainGrid.Background = FindResource("dt05Bg") as ImageBrush;
        //    this.Visibility = Visibility.Visible;
        //}

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
            {
                if (AoArena.Casting is Base.Rules.CastingCongress)
                    input("X");
            }
        }

        public event Util.InputMessageHandler input;
    }
}
//        public const int UNFOLD_LIMIT = 8;
//        public const int HI_SIZE = 90, HI_LAYER = 130;
//        public const int MAX_LENGTHCNT = UNFOLD_LIMIT * HI_SIZE + 10;

//        public Base.LibGroup Tuple
//        {
//            set
//            {
//                //Deal = new AoDeal(this, value);

//                //List<string> ls1 = (new string[] { "T0", "T0", "T0", "T0", "T0", "T0", "T0" }).ToList();
//                //List<string> ls2 = new List<string>();
//                ////List<string> ls2 = (new string[] { "T53", "T54" }).ToList();
//                //Deal.Show(ls1, ls2);
//            }
//        }

//        //public AoArena AoArena {set; get;}

//        ////private string TableType { set; get; }

//        //// record those selected list
//        //private ObservableCollection<Ruban> selectedList;

//        //public DealTable()
//        //{
//        //    InitializeComponent();
//        //    selectedList = new ObservableCollection<Ruban>();
//        //}

//        internal void Select(List<Ruban> rubans, bool side)
//        {
//            mainBoard.Children.Clear();
//            int usz = rubans.Count;
//            foreach (Ruban ruban in rubans)
//            {
//                mainBoard.Children.Add(ruban);
//                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
//                {
//                    //
//                    // Report select of cardBody
//                    if (input != null)
//                        input(ruban.UT.ToString());
//                    ruban.cardBody.IsChecked = false;
//                };
//            }
//        }

//        internal void ShowTable(List<Ruban> rubans)
//        {
//            mainBoard.Children.Clear();            
//            List<Ruban> uphi, List<Ruban> dnhi,
//            int r1, int r2, bool cancelable, bool keep)
//        {
//            TableType = "Table";
//            Keep = keep;
//            mainBoard.Children.Clear();
//            selectedList = new ObservableCollection<Ruban>();
//            int usz = uphi.Count;
//            int idx = 0;
//            foreach (Ruban ruban in uphi)
//            {
//                mainBoard.Children.Add(ruban);
//                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
//                {
//                    if (!selectedList.Contains(ruban))
//                        selectedList.Add(ruban);
//                };
//                ruban.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
//                {
//                    selectedList.Remove(ruban);
//                };
//                ruban.SetOfIndexTable(idx, 0, uphi.Count);
//                ++idx;
//            }
//            if (dnhi != null && dnhi.Count > 0)
//            {
//                mainGrid.Height = 274;
//                mainGrid.Background = FindResource("dt01Bg") as ImageBrush;

//                int dsz = dnhi.Count;
//                //double deach = dsz <= UNFOLD_LIMIT ? HITORI_SIZE :
//                //    (double)(MAX_LENGTHCNT - HITORI_SIZE) / (dsz - 1);
//                idx = 0;
//                foreach (Ruban ruban in dnhi)
//                {
//                    mainBoard.Children.Add(ruban);
//                    ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
//                    {
//                        if (!selectedList.Contains(ruban))
//                            selectedList.Add(ruban);
//                    };
//                    ruban.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
//                    {
//                        selectedList.Remove(ruban);
//                    };
//                    ruban.SetOfIndexTable(idx, 1, dnhi.Count);
//                    ++idx;
//                }
//                //sepLine.Visibility = Visibility.Visible;
//                mainBoard.Height = 240;
//            }
//            else
//            {
//                mainGrid.Height = 166;
//                mainGrid.Background = FindResource("dt02Bg") as ImageBrush;
//                mainBoard.Height = 120;
//                //sepLine.Visibility = Visibility.Collapsed;
//            }
//            this.Cancelable = cancelable;
//            selectedList.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
//            {
//                if (r1 == 1 && r2 == 1 && selectedList.Count == r1)
//                {
//                    if (keep)
//                        LockTable();
//                    else
//                        FinishTable();
//                    if (input != null)
//                        input(string.Join(",", selectedList.Select(p => p.UT)));
//                }
//                else if (r1 != 1 || r2 != 1)
//                {
//                    if (selectedList.Count >= r1 && selectedList.Count <= r2)
//                        okButton.Visibility = Visibility.Visible;
//                    else
//                        okButton.Visibility = Visibility.Hidden;
//                }
//            };
//            closeButton.Visibility = cancelable ? Visibility.Visible : Visibility.Hidden;
//            okButton.Visibility = r1 == 0 ? Visibility.Visible : Visibility.Hidden;
//            this.Visibility = Visibility.Visible;
//        }

//        private void closeButtonClick(object sender, RoutedEventArgs e)
//        {
//            this.Visibility = Visibility.Collapsed;
//            if (input != null && Cancelable)
//                input("0");
//        }

//        private void okButtonClick(object sender, RoutedEventArgs e)
//        {
//            if (input != null)
//            {
//                if (TableType == "Table")
//                {
//                    if (Keep)
//                        FinishTable();
//                    okButton.Visibility = Visibility.Collapsed;
//                    if (selectedList.Count > 0)
//                        input(string.Join(",", selectedList.Select(p => p.UT)));
//                    else
//                        input("0");
//                }
//                else if (TableType.Contains("XArrage"))
//                {
//                    int cidx = TableType.IndexOf(',');
//                    int r1 = int.Parse(TableType.Substring("XArrage".Length, cidx - "XArrage".Length));
//                    int r2 = int.Parse(TableType.Substring(cidx + 1));

//                    string output = "";

//                    IDictionary<int, Ruban> mop = new Dictionary<int, Ruban>();
//                    foreach (var elem in mainBoard.Children)
//                    {
//                        Ruban ruban = elem as Ruban;
//                        if (ruban.Jndex == 1)
//                            mop.Add(ruban.Index, ruban);
//                    }
//                    for (int i = 0; i < mop.Count; ++i)
//                        output += "," + mop[i].UT;

//                    IDictionary<int, Ruban> map = new Dictionary<int, Ruban>();
//                    foreach (var elem in mainBoard.Children)
//                    {
//                        Ruban ruban = elem as Ruban;
//                        if (ruban.Jndex == 0)
//                            map.Add(ruban.Index, ruban);
//                    }
//                    for (int i = 0; i < map.Count; ++i)
//                        output += "," + map[i].UT;
//                    if (map.Count >= r1 && map.Count <= r2)
//                    {
//                        FinishTable();
//                        input(string.IsNullOrEmpty(output) ? "0" : output.Substring(1));
//                    }
//                }
//            }
//        }

//        internal void FinishTable()
//        {
//            //tuxBoard.Children.Clear();
//            //eqBoard.Children.Clear();
//            mainBoard.Children.Clear();
//            Visibility = Visibility.Collapsed;
//        }

//        internal void LockTable()
//        {
//            foreach (UIElement uie in mainBoard.Children)
//            {
//                Ruban ruban = uie as Ruban;
//                ruban.Cat = Ruban.Category.SOUND;
//            }
//        }

//        public event Util.InputMessageHandler input;
//    }
//}
