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

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for PlayerBoard.xaml
    /// </summary>
    public partial class PlayerBoard : UserControl
    {
        public Base.LibGroup Tuple
        {
            set
            {
                AoPlayer = new AoPlayer(this, value)
                {
                    //Nick = "樂無異",
                    //Rank = 6,
                    //Team = 2
                };
                DataContext = AoPlayer;

                suImmobe.ToolTip = Tips.IchiDisplay.GetExspTip(value, "STU1");
                if (suImmobe.ToolTip == null)
                    suImmobe.ToolTip = "定身";
                suLoved.ToolTip = Tips.IchiDisplay.GetExspTip(value, "STU2");
                if (suLoved.ToolTip == null)
                    suLoved.ToolTip = "已倾慕";
                su0Pet.ToolTip = Tips.IchiDisplay.GetExspTip(value, "STU3");
                if (su0Pet.ToolTip == null)
                    su0Pet.ToolTip = "无宠物效果";
                //hahaha.DataContext = AoPlayer;

                //AoPlayer.setPet(3, 13);
                //AoPlayer.setPet(2, 12);
                //AoPlayer.insEscue(1001);
                //AoPlayer.Immobilized = true;
                //AoPlayer.SelectHero = 10502;
                //AoPlayer.IsAlive = true;
                //AoPlayer.HP = 4;
                //AoPlayer.HPa = 6;
                //AoPlayer.STR = 4;
                //AoPlayer.STRa = 2;
                //AoPlayer.DEX = 8;
                //AoPlayer.DEXa = 8;
                //AoPlayer.Weapon = 47;
                //AoPlayer.Armor = 54;
            }
        }

        public AoPlayer AoPlayer { private set; get; }

        public PlayerBoard()
        {
            InitializeComponent();
            SetAsWithOutXB();

            //AoPlayer ap = mainGrid.Resources["aoPlayer"] as AoPlayer;
            //ap.SetPlayerBoard(this);
            isValid = true; isActive = false;
            azureMask.Opacity = 0;
            mainGrid.Template = Resources["inactiveBdTemplate"] as ControlTemplate;

            weaponLock.Loc = Card.RubanLock.Location.WEAPON;
            weaponLock.Cat = Card.RubanLock.Category.SOUND;
            armorLock.Loc = Card.RubanLock.Location.ARMOR;
            armorLock.Cat = Card.RubanLock.Category.SOUND;
            troveLock.Loc = Card.RubanLock.Location.TROVE;
            troveLock.Cat = Card.RubanLock.Category.SOUND;
            troveBox.Loc = Card.RubanBox.Location.TROVE;
            troveBox.Cat = Card.RubanBox.Category.SOUND;

            weaponLock.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                {
                    //MessageBox.Show("AD.InsSelectedCard(weaponLock.UT)" + weaponLock.UT);
                    AD.InsSelectedCard(weaponLock.UT);
                }
            };
            weaponLock.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.DelSelectedCard(weaponLock.UT);
            };
            armorLock.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                {
                    //MessageBox.Show("AD.InsSelectedCard(weaponLock.UT)" + armorLock.UT);
                    AD.InsSelectedCard(armorLock.UT);
                }
            };
            armorLock.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.DelSelectedCard(armorLock.UT);
            };
            troveBox.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.InsSelectedCard(troveBox.UT);
            };
            troveBox.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.DelSelectedCard(troveBox.UT);
            };
            troveLock.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.InsSelectedCard(troveLock.UT);
            };
            troveLock.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.DelSelectedCard(troveLock.UT);
            };
            exEquipLock.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                {
                    //MessageBox.Show("AD.InsSelectedCard(weaponLock.UT)" + armorLock.UT);
                    AD.InsSelectedCard(exEquipLock.UT);
                }
            };
            exEquipLock.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
            {
                if (AD != null)
                    AD.DelSelectedCard(exEquipLock.UT);
            };
            enabledExcard = new List<ushort>();
            enabledPets = new List<ushort>();
            enabledFakeq = new List<ushort>();
            enabledEscue = new List<ushort>();
            enabledRune = new List<ushort>();
            //enabledExEquip = false;
            troveBox.padClickHandler += inLuggageClick;
        }

        private bool isValid;
        private bool isActive;

        public void SetTargetValid(bool value)
        {
            if (value != isValid)
            {
                isValid = value;
                if (value)
                    azureMask.Opacity = 0;
                else
                {
                    azureMask.Opacity = 0.5;
                    SetTargetActive(false);
                }
            }
        }
        public void SetTargetActive(bool value)
        {
            //mainGrid.IsM
            if (value != isActive)
            {
                isActive = value;
                Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
                SolidColorBrush brush = rec.Background as SolidColorBrush;
                if (value)
                    mainGrid.Template = Resources["activeBdTemplate"] as ControlTemplate;
                else
                    mainGrid.Template = Resources["inactiveBdTemplate"] as ControlTemplate;
                mainGrid.ApplyTemplate();
                (mainGrid.Template.FindName("goldenMask", mainGrid) as Grid).Background = brush;
            }
        }

        internal void SetTargetLock()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            SolidColorBrush brush = rec.Background as SolidColorBrush;

            mainGrid.Template = Resources["lockBdTemplate"] as ControlTemplate;
            mainGrid.ApplyTemplate();

            (mainGrid.Template.FindName("goldenMask", mainGrid) as Grid).Background = brush;
        }

        public void SetRounder()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            rec.Background = Resources["maskOfRounder"] as SolidColorBrush;
        }
        public void SetSpSuccess()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            rec.Background = Resources["maskOfSpSuccess"] as SolidColorBrush;
        }
        public void SetSpFail()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            rec.Background = Resources["maskOfSpFail"] as SolidColorBrush;
        }
        public void SetClear()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            rec.Background = new SolidColorBrush(Colors.White);
        }
        public void SetDelegate()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            rec.Background = Resources["maskOfDelegate"] as SolidColorBrush;
        }
        public void SetAsNotTrigger()
        {
            Grid rec = mainGrid.Template.FindName("goldenMask", mainGrid) as Grid;
            if (rec.Background == Resources["maskOfRounder"] as SolidColorBrush)
                rec.Background = new SolidColorBrush(Colors.White);
        }
        public void EnableWeapon()
        {
            weaponLock.Cat = Card.RubanLock.Category.ACTIVE;
        }
        public void DisableWeapon()
        {
            weaponLock.cardBody.IsChecked = false;
            weaponLock.Cat = Card.RubanLock.Category.SOUND;
        }
        internal void LockWeapon()
        {
            weaponLock.Cat = Card.RubanLock.Category.SOUND;
        }
        public void EnableArmor()
        {
            armorLock.Cat = Card.RubanLock.Category.ACTIVE;
        }
        public void DisableArmor()
        {
            armorLock.cardBody.IsChecked = false;
            armorLock.Cat = Card.RubanLock.Category.SOUND;
        }
        internal void LockArmor()
        {
            armorLock.Cat = Card.RubanLock.Category.SOUND;
        }
        public void EnableTrove()
        {
            troveBox.Cat = Card.RubanBox.Category.ACTIVE;
            troveLock.Cat = Card.RubanLock.Category.ACTIVE;
        }
        public void DisableTrove()
        {
            troveBox.cardBody.IsChecked = false;
            troveBox.Cat = Card.RubanBox.Category.SOUND;
            troveLock.cardBody.IsChecked = false;
            troveLock.Cat = Card.RubanLock.Category.SOUND;
        }
        internal void LockTrove()
        {
            troveBox.Cat = Card.RubanBox.Category.SOUND;
            troveLock.Cat = Card.RubanLock.Category.SOUND;
        }
        internal void EnableExEquip()
        {
            exEquipLock.Cat = Card.RubanLock.Category.ACTIVE;
        }
        public void DisableExEquip()
        {
            exEquipLock.cardBody.IsChecked = false;
            exEquipLock.Cat = Card.RubanLock.Category.SOUND;
        }
        internal void LockExEquip()
        {
            exEquipLock.Cat = Card.RubanLock.Category.SOUND;
        }
        public void EnableExCards(IEnumerable<ushort> vset)
        {
            enabledExcard = vset.ToList();
            if (enabledExcard.Count > 0)
                excardBorder.Visibility = Visibility.Visible;
            else
                excardBorder.Visibility = Visibility.Collapsed;
        }
        public void ResumeExCards()
        {
            enabledExcard.Clear();
            excardBorder.Visibility = Visibility.Collapsed;
        }
        //internal void LockExCards() { }
        public Card.RubanLock GetStandardRubanLock(ushort ut)
        {
            if (weaponLock != null && weaponLock.UT == ut)
                return weaponLock;
            else if (armorLock != null && armorLock.UT == ut)
                return armorLock;
            else if (troveLock != null && troveLock.UT == ut)
                return troveLock;
            else
                return null;
        }
        public void EnablePets(IEnumerable<ushort> vset)
        {
            enabledPets = vset.ToList();
            if (enabledPets.Count > 0)
                petBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            else
                petBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        public void ResumePets()
        {
            enabledPets.Clear();
            petBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        public void EnableFakeq(IEnumerable<ushort> vset)
        {
            enabledFakeq = vset.ToList();
            if (enabledFakeq.Count > 0)
                werBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            else
                werBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        public void ResumeFakeq()
        {
            enabledFakeq.Clear();
            werBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        public void EnableRune(IEnumerable<ushort> vset)
        {
            enabledRune = vset.ToList();
            if (enabledRune.Count > 0)
                runeBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            else
                runeBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        public void ResumeRune()
        {
            enabledRune.Clear();
            runeBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        public void EnableEscue(IEnumerable<ushort> vset)
        {
            enabledEscue = vset.ToList();
            if (enabledEscue.Count > 0)
                npcBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            else
                npcBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        public void ResumeEscue()
        {
            enabledEscue.Clear();
            npcBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
        internal AoDisplay AD { set; get; }

        private List<ushort> enabledExcard;
        private List<ushort> enabledPets;
        private List<ushort> enabledFakeq;
        private List<ushort> enabledEscue;
        private List<ushort> enabledRune;
        //private bool enabledExEquip;

        private void excardBarClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
            {
                //if (AoPlayer.ExEquip != 0)
                //{
                //    if (!enabledExEquip)
                //        AD.yhTV.Show("C" + AoPlayer.ExEquip, AoPlayer.Rank + "EQ");
                //    else
                //        AD.yhTV.ShowSelectableList("C" + AoPlayer.ExEquip, AoPlayer.Rank + "SEQ", "");
                //}
                if (enabledExcard == null || enabledExcard.Count == 0)
                    AD.yhTV.Show(AoPlayer.GetExCardsMatList(), AoPlayer.Rank + "EC");
                else
                {
                    //enabledExcard.Remove(44);
                    AD.yhTV.ShowSelectableList(enabledExcard.Select(p => "C" + p).ToList(),
                        AoPlayer.GetExCardsList().Except(enabledExcard).Select(p => "C" + p).ToList(),
                        AoPlayer.Rank + "SEC", "");
                }
            }
        }
        private void expeopleBarClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
                AD.yhTV.Show(AoPlayer.ExSpCards, AoPlayer.Rank + "EP");
        }
        private void folderBarClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
                AD.yhTV.Show(AoPlayer.GetMyFolderMatList(), AoPlayer.Rank + "MFD");
        }

        private void petButtonClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
            {
                if (enabledPets == null || enabledPets.Count == 0)
                    AD.yhTV.Show(AoPlayer.Pets.Select(p => "M" + p).ToList(), AoPlayer.Rank + "PT");
                else
                {
                    AD.yhTV.ShowSelectableList(enabledPets.Select(p => "M" + p).ToList(),
                        AoPlayer.Pets.Except(enabledPets).Select(p => "M" + p).ToList(),
                        AoPlayer.Rank + "SPT", "PT");
                }
            }
        }
        private void npcButtonClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
            {
                if (enabledEscue == null || enabledEscue.Count == 0)
                {
                    AD.yhTV.Show(AoPlayer.Escue.Select(p => "M" + p).ToList(), AoPlayer.Rank + "YJ");
                }
                else
                {
                    AD.yhTV.ShowSelectableList(enabledEscue.Select(p => "M" + p).ToList(),
                        AoPlayer.Escue.Except(enabledEscue).Select(p => "M" + p).ToList(),
                        AoPlayer.Rank + "SYJ", "YJ");
                }
            }
        }
        private void runeButtonClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
            {
                if (enabledRune == null || enabledRune.Count == 0)
                {
                    AD.yhTV.Show(AoPlayer.Runes.Select(p => "R" + p).ToList(), AoPlayer.Rank + "FW");
                }
                else
                {
                    AD.yhTV.ShowSelectableList(enabledRune.Select(p => "R" + p).ToList(),
                        AoPlayer.Runes.Except(enabledRune).Select(p => "R" + p).ToList(),
                        AoPlayer.Rank + "SFW", "FW");
                }
            }
        }
        private void werButtonClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
            {
                if (enabledFakeq == null || enabledFakeq.Count == 0)
                {
                    AD.yhTV.Show(AoPlayer.Fakeq.Keys.Select(p => "C" + p).ToList(), AoPlayer.Rank + "FQ");
                }
                else
                {
                    AD.yhTV.ShowSelectableList(enabledFakeq.Select(p => "C" + p).ToList(),
                        AoPlayer.Fakeq.Keys.Except(enabledFakeq).Select(p => "C" + p).ToList(),
                        AoPlayer.Rank + "SFQ", "TX");
                }
            }
        }
        private void inLuggageClick(object sender, RoutedEventArgs e)
        {
            if (AD != null)
                AD.yhTV.Show(AoPlayer.InLuggage, AoPlayer.Rank + "ILG");
        }
        //private childItem FindVisualChild<childItem>(DependencyObject obj)
        //where childItem : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(obj, i);
        //        if (child != null && child is childItem)
        //            return (childItem)child;
        //        else
        //        {
        //            childItem childOfChild = FindVisualChild<childItem>(child);
        //            if (childOfChild != null)
        //                return childOfChild;
        //        }
        //    }
        //    return null;
        //}

        #region Switch between XB or not
        private void SetEquipCount(int l, int h, int ew, int et, int wpWidth, int wpTop,
            int amWidth, int amTop, bool lg, int pbHeight, int pbWidth, int pbLeft, int pbTop)
        {
            weaponLock.Height = h; weaponLock.Width = wpWidth;
            Canvas.SetLeft(weaponLock, l);
            Canvas.SetTop(weaponLock, wpTop);

            armorLock.Height = h; armorLock.Width = amWidth;
            Canvas.SetLeft(armorLock, l);
            Canvas.SetTop(armorLock, amTop);

            troveLock.Visibility = lg ? Visibility.Visible : Visibility.Collapsed;

            expeopleBar.Height = h; expeopleBar.Width = ew;
            Canvas.SetLeft(expeopleBar, l);
            Canvas.SetTop(expeopleBar, et);
            excardBar.Height = h; excardBar.Width = ew;
            Canvas.SetLeft(excardBar, l);
            Canvas.SetTop(excardBar, et);
            exEquipLock.Height = h; exEquipLock.Width = ew;
            Canvas.SetLeft(exEquipLock, l);
            Canvas.SetTop(exEquipLock, et);
            excardBorder.Height = h; excardBorder.Width = ew;
            Canvas.SetLeft(excardBorder, l);
            Canvas.SetTop(excardBorder, et);

            playerRankB.Height = pbHeight; playerRankB.Width = pbWidth;
            Canvas.SetLeft(playerRankB, pbLeft);
            Canvas.SetTop(playerRankB, pbTop);
        }

        internal void SetAsWithXB()
        {
            SetEquipCount(-82, 23, 98, 75, 99, 3, 87, 27, true, 28, 30, -90, -21);
        }
        internal void SetAsWithOutXB()
        {
            // l(eft), h(eight), ew(idth), et(op)
            SetEquipCount(-82, 23, 98, 70, 91, 20, 86, 45, false, 40, 40, -90, -19);
        }
        #endregion Switch between XB or not
    }
}
