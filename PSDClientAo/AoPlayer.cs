using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSD.ClientAo
{
    public class AoPlayer : INotifyPropertyChanged
    {
        private PlayerBoard pb;

        public Base.LibGroup Tuple { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public AoPlayer(PlayerBoard playerBoard, Base.LibGroup libGroup)
        {
            this.pb = playerBoard;
            this.Tuple = libGroup;

            Nick = "";
            Pets = new List<ushort>();

            TuxCount = 0;
            Weapon = 0;
            Armor = 0;
            Trove = 0;
            mExCards = new List<ushort>();
            Fakeq = new Dictionary<ushort, string>();
            Escue = new List<ushort>();
            Runes = new List<ushort>();
            mInLuggage = new List<string>();
            FolderCount = 0;

            Token = 0;
            //Peoples = new List<int>();
            mExSpCards = new List<string>();
            mPlayerTars = new List<ushort>();
            mMyFolder = new List<ushort>();
            mSkills = new List<string>();

            IsLoved = false;
            IsAlive = true;
            Immobilized = false;
            PetDisabled = false;
        }

        public void ParseFromHeroLib()
        {
            Base.Card.Hero hero = Tuple.HL.InstanceHero(SelectHero);
            if (hero != null)
            {
                Team = (Rank == 0) ? 0 : (Rank % 2 == 1 ? 1 : 2);
                HP = HPa = hero.HP;
                STR = STRa = hero.STR;
                DEX = DEXa = hero.DEX;
                IsAlive = true;
                IsLoved = false;
                Immobilized = false;
                PetDisabled = false;
            }
        }
        // use when recovering the room
        public void Reset()
        {
            DelPet(Pets.ToList());
            TuxCount = 0;
            DelExCards(mExCards.ToList());
            Fakeq.Keys.ToList().ForEach(DelFakeq);
            Escue.ToList().ForEach(DelEscue);
            Runes.ToList().ForEach(DelRune);
            if (Trove != 0)
                DelIntoLuggage(Trove, mInLuggage.ToList());
            FolderCount = 0;

            Token = 0;
            DelExSpCard(ExSpCards.ToList());
            DelPlayerTar(PlayerTars.ToList());
            DelMyFolder(mMyFolder.ToList());
            ClearSkill();

            Weapon = Armor = Trove = 0;
            IsLoved = false;
            IsAlive = true;
            Immobilized = false;
            PetDisabled = false;
        }
        #region Player Property
        private string mNick;
        public string Nick
        {
            set { mNick = value; NotifyPropertyChanged("Nick"); }
            get { return mNick; }
        }

        private ushort mRank = 0;        
        public ushort Rank
        {
            [STAThread]
            set
            {
                if (mRank != value)
                {
                    mRank = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Image img = null;
                        switch (mRank)
                        {
                            case 1:
                                img = pb.TryFindResource("rankGIchi") as Image; break;
                            case 2:
                                img = pb.TryFindResource("rankGNi") as Image; break;
                            case 3:
                                img = pb.TryFindResource("rankGSan") as Image; break;
                            case 4:
                                img = pb.TryFindResource("rankGYong") as Image; break;
                            case 5:
                                img = pb.TryFindResource("rankGGo") as Image; break;
                            case 6:
                                img = pb.TryFindResource("rankGRoku") as Image; break;
                            default: break;
                        }
                        if (img != null)
                            pb.playerRankB.Child = img;
                    }));
                }
            }
            get { return mRank; }
        }

        private int mTeam;
        public int Team
        {
            //[STAThread]
            //set
            //{
            //    if (mTeam != value)
            //    {
            //        mTeam = value;
            //        pb.Dispatcher.BeginInvoke((Action)(() =>
            //        {
            //            pb.tuxCount.Background = new SolidColorBrush(
            //                mTeam == 1 ? Colors.Red : Colors.Blue);
            //        }));
            //    }
            //}
            set { mTeam = value; NotifyPropertyChanged("Team"); }
            get { return mTeam; }
        }// 1 - Aka, 2 - Ao
        public int OppTeam { get { return 3 - mTeam; } }

        private int mHero;
        public int SelectHero
        {
            set { if (mHero != value) { OnHeroChanged(value); } }
            get { return mHero; }
        }
        [STAThread]
        private void OnHeroChanged(int value)
        {
            if (value != 0)
            {
                Base.Card.Hero hero = Tuple.HL.InstanceHero(value);
                if (hero != null)
                {
                    //ImageBrush ib = pb.Resources["heroHead" + hero.Avatar] as ImageBrush;
                    //pb.portrait.Fill = ib ?? pb.Resources["heroHead000"] as ImageBrush;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Image img = pb.TryFindResource("heroHead" + hero.Avatar) as Image;
                        if (img == null)
                            img = pb.TryFindResource("heroHead000") as Image;
                        if (!IsAlive)
                        {
                            FormatConvertedBitmap bitmap = new FormatConvertedBitmap();
                            bitmap.BeginInit();
                            bitmap.Source = img.Source as BitmapSource;
                            bitmap.DestinationFormat = PixelFormats.Gray32Float;
                            bitmap.EndInit();
                            pb.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                pb.portrait.Fill = new ImageBrush(bitmap);
                            }));
                        }
                        else
                            pb.portrait.Fill = new ImageBrush(img.Source);

                        ImageBrush imgb = pb.TryFindResource("nameBar" + hero.Ofcode) as ImageBrush;
                        if (imgb == null)
                        {
                            Base.Card.Hero aHero = Tuple.HL.InstanceHero(hero.Archetype);
                            if (aHero != null)
                                imgb = pb.TryFindResource("nameBar" + aHero.Ofcode) as ImageBrush;
                        }
                        if (imgb == null)
                            imgb = pb.TryFindResource("nameBar000Brush") as ImageBrush;
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pb.nameBarBrush.Fill = imgb;
                        }));

                        pb.portrait.ToolTip = Tips.IchiDisplay.GetHeroTip(Tuple, value);
                    }));

                    // Update Skills when change hero directly, reserved now.
                    Base.Card.Hero oldHero = Tuple.HL.InstanceHero(mHero);
                    if (oldHero != null)
                    {
                        foreach (string skstr in oldHero.Skills)
                        {
                            Base.Skill sk = Tuple.SL.EncodeSkill(skstr);
                            if (sk.IsBK)
                                pb.AD.yfJoy.CEE.LoseBKSkill(sk.Code);
                        }
                        if (Rank == pb.AD.SelfUid)
                            pb.AD.yfJoy.CEE.ResetSkill();
                    }
                    foreach (string skstr in hero.Skills)
                    {
                        Base.Skill sk = Tuple.SL.EncodeSkill(skstr);
                        if (sk != null)
                        {
                            if (sk.IsBK)
                                pb.AD.yfJoy.CEE.SetNewBKSkill(sk, Rank);
                            else if (Rank == pb.AD.SelfUid)
                                pb.AD.yfJoy.CEE.SetNewSkill(sk);
                        }
                    }
                }
            }
            else
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.portrait.Fill = new ImageBrush((pb.TryFindResource("heroHead000") as Image).Source);
                }));
            mHero = value;
        }
        #endregion Player Property

        #region Cards Property
        public List<ushort> Pets { private set; get; }
        [STAThread]
        public void InsPet(ushort pet) { InsPet(new ushort[] { pet }); }
        [STAThread]
        public void InsPet(IEnumerable<ushort> pets)
        {
            foreach (ushort pet in pets)
            {
                Base.Card.Monster mon = Tuple.ML.Decode(pet);
                if (mon != null && !Pets.Contains(pet))
                {
                    string code = mon.Code;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        pb.petStack.Children.Add(pb.TryFindResource("petsnap" + code) as Image);
                    }));
                }
                Pets.Add(pet);
            }
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.petButton.Visibility = Pets.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }));
        }
        [STAThread]
        public void DelPet(ushort pet) { DelPet(new ushort[] { pet }); }
        [STAThread]
        public void DelPet(IEnumerable<ushort> pets)
        {
            foreach (ushort pet in pets)
            {
                if (Pets.Contains(pet))
                {
                    Base.Card.Monster mon = Tuple.ML.Decode(pet);
                    if (mon != null)
                    {
                        string code = mon.Code;
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pb.petStack.Children.Remove(pb.TryFindResource("petsnap" + code) as Image);
                        }));
                    }
                    Pets.Remove(pet);
                }
            }
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.petButton.Visibility = Pets.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        public List<ushort> Escue { set; get; }
        [STAThread]
        public void InsEscue(ushort npcCd)
        {
            Base.Card.NPC npc = Base.Card.NMBLib.Decode(npcCd, Tuple.ML, Tuple.NL) as Base.Card.NPC;
            if (npc != null)
            {
                string code = npc.Code;
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.npcspStack.Children.Add(pb.TryFindResource("npcsnap" + code) as Image);
                }));
                Escue.Add(npcCd);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.npcButton.Visibility = System.Windows.Visibility.Visible;
                }));
            }
        }
        [STAThread]
        public void DelEscue(ushort npcCd)
        {
            Base.Card.NPC npc = Base.Card.NMBLib.Decode(npcCd, Tuple.ML, Tuple.NL) as Base.Card.NPC;
            if (npc != null)
            {
                string code = npc.Code;
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.npcspStack.Children.Remove(pb.TryFindResource("npcsnap" + code) as Image);
                }));
                Escue.Remove(npcCd);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (Escue.Count > 0)
                        pb.npcButton.Visibility = System.Windows.Visibility.Visible;
                    else
                        pb.npcButton.Visibility = System.Windows.Visibility.Collapsed;
                }));
            }
        }
        public List<ushort> Runes { set; get; }
        [STAThread]
        public void InsRune(ushort runeCd)
        {
            Base.Rune rune = Tuple.RL.Decode(runeCd);
            if (rune != null)
            {
                string code = rune.Code;
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Image bsimg = pb.TryFindResource("bufferIcon" + code) as Image;
                    if (bsimg != null)
                    {
                        Image nImg = new Image() { Source = bsimg.Source, ToolTip = bsimg.ToolTip };
                        pb.runeStack.Children.Add(nImg);
                    }
                }));
                Runes.Add(runeCd);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.runeButton.Visibility = System.Windows.Visibility.Visible;
                }));
            }
        }
        [STAThread]
        public void DelRune(ushort runeCd)
        {
            Base.Rune rune = Tuple.RL.Decode(runeCd);
            if (rune != null)
            {
                string code = rune.Code;
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Image bsimg = pb.TryFindResource("bufferIcon" + code) as Image;
                    foreach (var elem in pb.runeStack.Children)
                    {
                        Image img = elem as Image;
                        if (img != null && img.Source == bsimg.Source)
                        {
                            pb.runeStack.Children.Remove(img);
                            break;
                        }
                    }
                }));
                Runes.Remove(runeCd);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (Runes.Count > 0)
                        pb.runeButton.Visibility = System.Windows.Visibility.Visible;
                    else
                        pb.runeButton.Visibility = System.Windows.Visibility.Collapsed;
                }));
            }
        }

        public IDictionary<ushort, string> Fakeq { set; get; }
        [STAThread]
        public void InsFakeq(ushort werCd, string asCode)
        {
            Base.Card.Tux tux = Tuple.TL.DecodeTux(werCd);
            if (tux != null)
            {
                Fakeq[werCd] = asCode;
                string code = (asCode == "0") ? tux.Code : asCode;
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Image bsimg = pb.TryFindResource("wersnap" + asCode) as Image;
                    if (bsimg != null)
                    {
                        Image nImg = new Image() { Source = bsimg.Source, ToolTip = bsimg.ToolTip };
                        pb.werspStack.Children.Add(nImg);
                    }
                    //pb.werspStack.Children.Add(pb.TryFindResource("wersnap" + tux.Code) as Image);
                    if (Fakeq.Count > 0)
                        pb.werButton.Visibility = System.Windows.Visibility.Visible;
                    else
                        pb.werButton.Visibility = System.Windows.Visibility.Collapsed;
                }));
            }
        }
        [STAThread]
        public void DelFakeq(ushort werCd)
        {
            Base.Card.Tux tux = Tuple.TL.DecodeTux(werCd);
            if (tux != null)
            {
                string asCdoe = Fakeq[werCd];
                Fakeq.Remove(werCd);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Image bsimg = pb.TryFindResource("wersnap" + asCdoe) as Image;
                    foreach (var elem in pb.werspStack.Children)
                    {
                        Image img = elem as Image;
                        if (img != null && img.Source == bsimg.Source)
                        {
                            pb.werspStack.Children.Remove(img);
                            break;
                        }
                    }
                    //pb.werspStack.Children.Remove(pb.TryFindResource("wersnap" + tux.Code) as Image);
                    if (Fakeq.Count > 0)
                        pb.werButton.Visibility = System.Windows.Visibility.Visible;
                    else
                        pb.werButton.Visibility = System.Windows.Visibility.Collapsed;
                }));
            }
        }
        private ushort mWeapon;
        public ushort Weapon
        {
            [STAThread]
            set
            {
                if (mWeapon != value)
                {
                    if (value != 0)
                    {
                        Base.Card.Tux wp = Tuple.TL.DecodeTux(value);
                        if (wp != null)
                        {
                            string code = wp.Code;
                            pb.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                ImageBrush ib = pb.TryFindResource("staEp" + code) as ImageBrush;
                                pb.weaponLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.weaponLock.ToolTip = Tips.IchiDisplay.GetTuxTip(Tuple, value);
                            }));
                        }
                    }
                    else
                    {
                        //ImageBrush ib = pb.Resources["staEp01"] as ImageBrush;
                        //pb.weaponBar.Fill = ib ?? pb.Resources["staEp00"] as ImageBrush;
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            ImageBrush ib = pb.TryFindResource("staEpWQ00") as ImageBrush;
                            pb.weaponLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                            pb.weaponLock.ToolTip = null;
                        }));
                    }
                    mWeapon = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        pb.weaponLock.UT = mWeapon;
                    }));
                }
            }
            get { return mWeapon; }
        }

        private ushort mArmor;
        public ushort Armor
        {
            [STAThread]
            set
            {
                if (mArmor != value)
                {
                    if (value != 0)
                    {
                        Base.Card.Tux wp = Tuple.TL.DecodeTux(value);
                        if (wp != null)
                        {
                            string code = wp.Code;
                            pb.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                ImageBrush ib = pb.TryFindResource("staEp" + code) as ImageBrush;
                                //pb.armorBar.Fill = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.armorLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.armorLock.ToolTip = Tips.IchiDisplay.GetTuxTip(Tuple, value);
                            }));
                        }
                    }
                    else
                    {
                        //ImageBrush ib = pb.Resources["staEp02"] as ImageBrush;
                        //pb.weaponBar.Fill = ib ?? pb.Resources["staEp00"] as ImageBrush;
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            //pb.armorBar.Fill = new SolidColorBrush(Colors.BlueViolet);
                            ImageBrush ib = pb.TryFindResource("staEpFJ00") as ImageBrush;
                            pb.armorLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                            pb.armorLock.ToolTip = null;
                        }));
                    }
                    mArmor = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        pb.armorLock.UT = mArmor;
                    }));
                }
            }
            get { return mArmor; }
        }

        private ushort mTrove;
        public ushort Trove
        {
            [STAThread]
            set
            {
                if (mTrove != value)
                {
                    if (value != 0)
                    {
                        Base.Card.TuxEqiup tr = Tuple.TL.DecodeTux(value) as Base.Card.TuxEqiup;
                        if (tr != null && tr.IsLuggage())
                        {
                            string code = tr.Code;
                            pb.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                pb.troveLock.Visibility = System.Windows.Visibility.Collapsed;
                                pb.troveBox.Visibility = System.Windows.Visibility.Visible;
                                ImageBrush ib = pb.TryFindResource("staEp" + code) as ImageBrush;
                                //pb.armorBar.Fill = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.troveBox.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.troveBox.ToolTip = Tips.IchiDisplay.GetTuxTip(Tuple, value);
                            }));
                        }
                        else if (tr != null && !tr.IsLuggage())
                        {
                            string code = tr.Code;
                            pb.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                pb.troveLock.Visibility = System.Windows.Visibility.Visible;
                                pb.troveBox.Visibility = System.Windows.Visibility.Collapsed;
                                ImageBrush ib = pb.TryFindResource("staEp" + code) as ImageBrush;
                                //pb.armorBar.Fill = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.troveLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.troveLock.ToolTip = Tips.IchiDisplay.GetTuxTip(Tuple, value);
                            }));
                        }
                    }
                    else
                    {
                        //ImageBrush ib = pb.Resources["staEp02"] as ImageBrush;
                        //pb.weaponBar.Fill = ib ?? pb.Resources["staEp00"] as ImageBrush;
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pb.troveBox.Visibility = System.Windows.Visibility.Collapsed;
                            pb.troveLock.Visibility = System.Windows.Visibility.Visible;
                            //pb.armorBar.Fill = new SolidColorBrush(Colors.BlueViolet);
                            ImageBrush ib = pb.TryFindResource("staEpXB00") as ImageBrush;
                            pb.troveLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                            pb.troveLock.ToolTip = null;
                        }));
                    }
                    mTrove = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Base.Card.TuxEqiup tr = Tuple.TL.DecodeTux(value) as Base.Card.TuxEqiup;
                        if (tr != null && tr.IsLuggage())
                        {
                            pb.troveLock.UT = 0;
                            pb.troveBox.UT = mTrove;
                        }
                        else if (tr != null && !tr.IsLuggage())
                        {
                            pb.troveLock.UT = mTrove;
                            pb.troveBox.UT = 0;
                        }
                    }));
                }
            }
            get { return mTrove; }
        }

        private List<string> mInLuggage;
        public List<string> InLuggage { get { return mInLuggage; } }
        public void InitToLuggage(IEnumerable<string> codes)
        {
            if (codes.Any())
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    mInLuggage.AddRange(codes);
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    pb.troveBox.cardPad.Content = "(" + mInLuggage.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "ILG"))
                        pb.AD.yhTV.Show(mInLuggage, Rank + "ILG");
                }));
            }
        }
        public void InsIntoLuggage(ushort ut, IEnumerable<string> codes)
        {
            mInLuggage.AddRange(codes);
            if (ut == pb.troveBox.UT)
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    pb.troveBox.cardPad.Content = "(" + mInLuggage.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "ILG"))
                        pb.AD.yhTV.Show(mInLuggage, Rank + "ILG");
                }));
            }
        }
        public void DelIntoLuggage(ushort ut, IEnumerable<string> codes)
        {
            mInLuggage.RemoveAll(p => codes.Contains(p));
            if (ut == pb.troveBox.UT)
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    pb.troveBox.cardPad.Content = "(" + mInLuggage.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "ILG"))
                        pb.AD.yhTV.Show(mInLuggage, Rank + "ILG");
                }));
            }
        }
        public bool IsLuggageContain(ushort ut, string code)
        {
            return Trove == ut && mInLuggage.Contains(code);
        }

        private List<ushort> mExCards;
        public void InsExCards(ushort ut) { InsExCards(new ushort[] { ut }); }
        public void InsExCards(IEnumerable<ushort> uts)
        {
            if (uts.Any())
            {
                mExCards.AddRange(uts);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.excardBar.Visibility = System.Windows.Visibility.Visible;
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = (hro != null) ? (hro.ExCardsAlias ?? "特殊装备") : "特殊装备";
                    pb.excardText.Text = alias + " (" + mExCards.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "EC"))
                        pb.AD.yhTV.Show(GetExCardsMatList(), Rank + "EC");
                }));
            }
        }
        public void DelExCards(ushort ut) { DelExCards(new ushort[] { ut }); }
        public void DelExCards(IEnumerable<ushort> uts)
        {
            mExCards.RemoveAll(p => uts.Contains(p));
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (mExCards.Count > 0)
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = (hro != null) ? (hro.ExCardsAlias ?? "特殊装备") : "特殊装备";
                    pb.excardText.Text = alias + " (" + mExCards.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "EC"))
                        pb.AD.yhTV.Show(GetExCardsMatList(), Rank + "EC");
                }
                else
                {
                    pb.excardBar.Visibility = System.Windows.Visibility.Hidden;
                    pb.excardText.Text = "";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "EC"))
                        pb.AD.yhTV.Show(GetExCardsMatList(), Rank + "EC");
                }
            }));
        }
        public bool IsExCardsContain(ushort ut)
        {
            return mExCards.Contains(ut);
        }
        public List<ushort> GetExCardsList()
        {
            return mExCards.ToList();
        }
        public List<string> GetExCardsMatList()
        {
            return mExCards.Select(p => "C" + p.ToString()).ToList();
        }
        public ushort mExEquip;
        public ushort ExEquip
        {
            [STAThread]
            set
            {
                if (mExEquip != value)
                {
                    if (value != 0)
                    {
                        Base.Card.Tux wp = Tuple.TL.DecodeTux(value);
                        if (wp != null)
                        {
                            string code = wp.Code;
                            pb.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                ImageBrush ib = pb.TryFindResource("staEp" + code) as ImageBrush;
                                //pb.armorBar.Fill = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.exEquipLock.Face = ib ?? pb.TryFindResource("staEp00") as ImageBrush;
                                pb.exEquipLock.ToolTip = Tips.IchiDisplay.GetTuxTip(Tuple, value);
                                pb.exEquipLock.Visibility = System.Windows.Visibility.Visible;
                            }));
                        }
                    }
                    else
                    {
                        //ImageBrush ib = pb.Resources["staEp02"] as ImageBrush;
                        //pb.weaponBar.Fill = ib ?? pb.Resources["staEp00"] as ImageBrush;
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pb.exEquipLock.ToolTip = null;
                            pb.exEquipLock.Visibility = System.Windows.Visibility.Hidden;
                        }));
                    }
                    mExEquip = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        pb.exEquipLock.UT = mExEquip;
                    }));
                }
            }
            get { return mExEquip; }
        }

        private int mFolderCount;
        public int FolderCount
        {
            [STAThread]
            set
            {
                if (mFolderCount != value)
                {
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (mFolderCount > 0)
                        {
                            pb.folderRect.Visibility = System.Windows.Visibility.Visible;
                            Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                            string alias = "盖牌";
                            if (hro != null)
                            {
                                if (hro.FolderAlias != null)
                                    alias = hro.FolderAlias;
                                Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                                if (guest != null && guest.FolderAlias != null)
                                    alias = guest.FolderAlias;
                            }
                            pb.folderRText.Text = alias + " (" + mFolderCount + ")";
                        }
                        else
                            pb.folderRect.Visibility = System.Windows.Visibility.Collapsed;
                    }));
                    mFolderCount = value;
                }
            }
            get { return mFolderCount; }
        }
        private List<ushort> mMyFolder;
        public void InsMyFolder(ushort ut) { InsMyFolder(new ushort[] { ut }.ToList()); }
        public void InsMyFolder(List<ushort> uts)
        {
            if (uts.Count > 0)
            {
                mMyFolder.AddRange(uts);
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.folderBar.Visibility = System.Windows.Visibility.Visible;
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = "盖牌";
                    if (hro != null)
                    {
                        if (hro.FolderAlias != null)
                            alias = hro.FolderAlias;
                        Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                        if (guest != null && guest.FolderAlias != null)
                            alias = guest.FolderAlias;
                    }
                    pb.folderBText.Text = alias + " (" + mMyFolder.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "MFD"))
                        pb.AD.yhTV.Show(GetMyFolderMatList(), Rank + "MFD");
                }));
            }
        }
        public void DelMyFolder(ushort ut) { DelMyFolder(new ushort[] { ut }.ToList()); }
        public void DelMyFolder(List<ushort> uts)
        {
            mMyFolder.RemoveAll(p => uts.Contains(p));
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (mMyFolder.Count > 0)
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = "盖牌";
                    if (hro != null)
                    {
                        if (hro.FolderAlias != null)
                            alias = hro.FolderAlias;
                        Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                        if (guest != null && guest.FolderAlias != null)
                            alias = guest.FolderAlias;
                    }
                    pb.folderBText.Text = alias + " (" + mMyFolder.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "MFD"))
                        pb.AD.yhTV.Show(GetMyFolderMatList(), Rank + "MFD");
                }
                else
                {
                    pb.folderBar.Visibility = System.Windows.Visibility.Hidden;
                    pb.folderBText.Text = "";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "MFD"))
                        pb.AD.yhTV.Show(GetMyFolderMatList(), Rank + "MFD");
                }
            }));
        }
        public List<ushort> GetMyFolderList()
        {
            return mMyFolder.ToList();
        }
        public List<string> GetMyFolderMatList()
        {
            return mMyFolder.Select(p => "C" + p.ToString()).ToList();
        }

        private int mGuardian;
        public int Guardian
        {
            set { if (mGuardian != value) { OnGuardChanged(value); } }
            get { return mGuardian; }
        }
        [STAThread]
        private void OnGuardChanged(int value)
        {
            if (value != 0)
            {
                Base.Card.Exsp exsp = Tuple.ESL.Encode("L" + value);
                if (exsp != null)
                {
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ImageBrush img = pb.TryFindResource("loadGuard" + value) as ImageBrush;
                        if (img == null)
                            img = pb.TryFindResource("loadGuard000Brush") as ImageBrush;
                        pb.guardTing.Visibility = System.Windows.Visibility.Visible;
                        pb.guardTing.Fill = img;
                        pb.guardTing.ToolTip = Tips.IchiDisplay.GetExspTip(Tuple, "L" + value);
                    }));
                }
            }
            else
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.guardTing.Visibility = System.Windows.Visibility.Collapsed;
                }));
            mGuardian = value;
        }

        private int mCoss;
        public int Coss
        {
            set { if (mCoss != value) { OnCossChanged(value); } }
            get { return mCoss; }
        }
        [STAThread]
        private void OnCossChanged(int value)
        {
            if (value != 0)
            {
                Base.Card.Hero hero = Tuple.HL.InstanceHero(value);
                if (hero != null)
                {
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Image img = pb.TryFindResource("heroHead" + hero.Avatar) as Image;
                        if (img == null)
                            img = pb.TryFindResource("heroHead000") as Image;
                        pb.cossTing.Visibility = System.Windows.Visibility.Visible;
                        pb.cossTing.Fill = new ImageBrush(img.Source);
                        pb.cossTing.ToolTip = Tips.IchiDisplay.GetHeroTip(Tuple, value);
                    }));
                }
            }
            else
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.cossTing.Visibility = System.Windows.Visibility.Collapsed;
                }));
            mCoss = value;
        }

        #endregion Cards Property

        #region 3-points and Status Property
        private int mHP, mHPa;
        public int HP
        {
            set { if (mHP != value) OnHpChanged(value); }
            get { return mHP; }
        }
        public int HPa
        {
            set { if (mHPa != value) { mHPa = value; OnHpChanged(HP); } }
            get { return mHPa; }
        }
        [STAThread]
        private void OnHpChanged(int value)
        {
            mHP = value;
            float rate = (HPa == 0) ? 0.0f : (float)mHP / HPa;
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.hpBar.Width = rate * pb.hpBase.Width;
                pb.hpBarCt.Fill = pb.TryFindResource("hpBrush") as SolidColorBrush;
                pb.hpText.Text = "HP:" + mHP + "/" + HPa;
            }));
        }

        private int mSTR, mSTRa;
        public int STR
        {
            set { if (mSTR != value) OnSTRChanged(value); }
            get { return mSTR; }
        }
        public int STRa
        {
            set { if (mSTRa != value) { mSTRa = value; OnSTRChanged(STR); } }
            get { return mSTRa; }
        }
        [STAThread]
        private void OnSTRChanged(int value)
        {
            mSTR = value;
            if (mSTR <= 8)
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.strAppBar.Width = 0;
                    float rate = (float)mSTR / 8;
                    pb.strBar.Width = rate * pb.strBase.Width;
                    pb.strBarCt.Fill = pb.TryFindResource("strBrush") as SolidColorBrush;
                }));
            }
            else
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.strBar.Width = pb.strBase.Width;
                    pb.strBarCt.Fill = pb.TryFindResource("strBrush") as SolidColorBrush;
                    float rate = (float)(mSTR % 8) / 8;
                    pb.strAppBar.Width = rate * pb.strBase.Width;
                    pb.strAppBarCt.Fill = pb.TryFindResource("strAppBrush") as SolidColorBrush;
                }));
            }
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.strText.Text = "战力:" + mSTR + "/" + STRa;
            }));
        }

        private int mDEX, mDEXa;
        public int DEX
        {
            set { if (mDEX != value) OnDEXChanged(value); }
            get { return mDEX; }
        }
        public int DEXa
        {
            set { if (mDEXa != value) { mDEXa = value; OnDEXChanged(DEX); } }
            get { return mDEXa; }
        }
        [STAThread]
        private void OnDEXChanged(int value)
        {
            mDEX = value;
            if (mDEX <= 6)
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.dexAppBar.Width = 0;
                    float rate = (float)mDEX / 6;
                    pb.dexBar.Width = rate * pb.dexBase.Width;
                    pb.dexBarCt.Fill = pb.TryFindResource("dexBrush") as SolidColorBrush;
                }));
            }
            else
            {
                float rate = (float)(mDEX % 6) / 6;
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.dexBar.Width = pb.dexBase.Width;
                    pb.dexBarCt.Fill = pb.TryFindResource("dexBrush") as SolidColorBrush;
                    pb.dexAppBar.Width = rate * pb.dexBase.Width;
                    pb.dexAppBarCt.Fill = pb.TryFindResource("dexAppBrush") as SolidColorBrush;
                }));
            }
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.dexText.Text = "命中:" + mDEX + "/" + DEXa;
            }));
        }

        private int mTuxCount;
        public int TuxCount
        {
            set { mTuxCount = value; NotifyPropertyChanged("TuxCount"); }
            get { return mTuxCount; }
        }

        private bool mbIsLoved, mbIsAlive, mbImmobilized, mbPetDisabled;
        public bool IsLoved {
            set
            {
                //if (mbIsLoved != value)
                //{
                //    mbIsLoved = value;
                //    pb.Dispatcher.BeginInvoke((Action)(() =>
                //    {
                //        pb.suLoved.Visibility = value ?
                //            System.Windows.Visibility.Visible :
                //            System.Windows.Visibility.Collapsed;
                //    }));
                //}
                mbIsLoved = value;
                NotifyPropertyChanged("IsLoved");
            }
            get { return mbIsLoved; }
        }
        public bool IsAlive
        {
            set { if (mbIsAlive != value) { mbIsAlive = value; OnHeroChanged(SelectHero); } }
            get { return mbIsAlive; }
        }
        public bool Immobilized
        {
            set
            {
                //if (mbImmobilized != value)
                //{
                //    mbImmobilized = value;
                //    pb.Dispatcher.BeginInvoke((Action)(() =>
                //    {
                //        pb.suImmobe.Visibility = value ?
                //            System.Windows.Visibility.Visible :
                //            System.Windows.Visibility.Collapsed;
                //    }));
                //}
                mbImmobilized = value;
                NotifyPropertyChanged("Immobilized");
            }
            get { return mbImmobilized; }
        }
        public bool PetDisabled
        {
            set
            {
                mbPetDisabled = value;
                NotifyPropertyChanged("PetDisabled");
            }
            get { return mbPetDisabled; }
        }

        private int mToken;
        public int Token
        {
            set
            {
                if (mToken != value)
                {
                    mToken = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (mToken > 0)
                        {
                            pb.tkTake.Visibility = System.Windows.Visibility.Visible;
                            Base.Card.Hero hero = Tuple.HL.InstanceHero(SelectHero);
                            if (string.IsNullOrEmpty(hero.TokenAlias) && Coss != 0)
                            {
                                Base.Card.Hero cossHero = Tuple.HL.InstanceHero(Coss);
                                if (cossHero != null)
                                    hero = cossHero;
                            }
                            if (hero != null)
                            {
                                string rename = "snapTK" + hero.Ofcode + "_" + mToken;
                                pb.tkTake.ToolTip = Tips.IchiDisplay.GetExspTip(Tuple, "TK" + hero.Ofcode);
                                if (pb.tkTake.ToolTip == null)
                                    pb.tkTake.ToolTip = hero.TokenAlias;
                                pb.tkTake.Source = pb.TryFindResource(rename) as ImageSource;
                            }
                        }
                        else
                            pb.tkTake.Visibility = System.Windows.Visibility.Collapsed;
                    }));
                }
            }
            get { return mToken; }
        }
        private bool mAwake;
        public bool Awake
        {
            set
            {
                if (mAwake != value)
                {
                    mAwake = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (mAwake)
                        {
                            pb.awTake.Visibility = System.Windows.Visibility.Visible;
                            Base.Card.Hero hero = Tuple.HL.InstanceHero(SelectHero);
                            if (string.IsNullOrEmpty(hero.AwakeAlias) && Coss != 0)
                            {
                                Base.Card.Hero cossHero = Tuple.HL.InstanceHero(Coss);
                                if (cossHero != null)
                                    hero = cossHero;
                            }
                            if (hero != null)
                            {
                                string rename = "snapTA" + hero.Ofcode;
                                pb.awTake.ToolTip = Tips.IchiDisplay.GetExspTip(Tuple, "TA" + hero.Ofcode);
                                if (pb.awTake.ToolTip == null)
                                    pb.awTake.ToolTip = hero.AwakeAlias;
                                pb.awTake.Source = pb.TryFindResource(rename) as ImageSource;
                            }
                        }
                        else
                            pb.awTake.Visibility = System.Windows.Visibility.Collapsed;
                    }));
                }
            }
            get { return mAwake; }
        }
        private List<string> mExSpCards;
        public List<string> ExSpCards { get { return mExSpCards; } }
        public void InsExSpCard(string code) { InsExSpCard(new string[] { code }); }
        public void InsExSpCard(IEnumerable<string> codes)
        {
            mExSpCards.AddRange(codes);
            if (mExSpCards.Count > 0)
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    pb.expeopleBar.Visibility = System.Windows.Visibility.Visible;
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = "特殊牌";
                    if (hro != null)
                    {
                        if (hro.PeopleAlias != null)
                            alias = hro.PeopleAlias;
                        Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                        if (guest != null && guest.PeopleAlias != null)
                            alias = guest.PeopleAlias;
                    }
                    pb.expeopleText.Text = alias + " (" + mExSpCards.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "EP"))
                        pb.AD.yhTV.Show(mExSpCards, Rank + "EP");
                }));
            }
        }
        public void DelExSpCard(string code) { DelExSpCard(new List<string> { code }); }
        public void DelExSpCard(List<string> codes)
        {
            mExSpCards.RemoveAll(p => codes.Contains(p));
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (mExSpCards.Count > 0)
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = "特殊牌";
                    if (hro != null)
                    {
                        if (hro.PeopleAlias != null)
                            alias = hro.PeopleAlias;
                        Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                        if (guest != null && guest.PeopleAlias != null)
                            alias = guest.PeopleAlias;
                    }
                    pb.expeopleText.Text = alias + " (" + mExSpCards.Count + ")";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "EP"))
                        pb.AD.yhTV.Show(mExSpCards, Rank + "EP");
                }
                else
                {
                    pb.expeopleBar.Visibility = System.Windows.Visibility.Hidden;
                    pb.expeopleText.Text = "";
                    if (pb.AD != null && pb.AD.IsTVDictContains(Rank + "EP"))
                        pb.AD.yhTV.Show(mExSpCards, Rank + "EP");
                }
            }));
        }
        public bool IsExSpCardContain(string code)
        {
            return mExSpCards.Contains(code);
        }

        private List<ushort> mPlayerTars;
        public List<ushort> PlayerTars { get { return mPlayerTars; } }
        public void InsPlayerTar(ushort ut) { InsPlayerTar(new ushort[] { ut }); }
        public void InsPlayerTar(IEnumerable<ushort> uts)
        {
            Base.Card.Hero hero = Tuple.HL.InstanceHero(SelectHero);
            if (hero != null && string.IsNullOrEmpty(hero.PlayerTarAlias) && Coss != 0)
            {
                Base.Card.Hero cossHero = Tuple.HL.InstanceHero(Coss);
                if (cossHero != null)
                    hero = cossHero;
            }
            foreach (ushort ut in uts)
            {
                if (!mPlayerTars.Contains(ut) && hero != null)
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ImageSource imgsrc = pb.TryFindResource("snapTR" + hero.Ofcode) as ImageSource;
                        if (ut > 0)
                        {
                            StackPanel spNewTarget = pb.AD.GetTokenStackPanel(ut);
                            if (spNewTarget != null)
                            {
                                Image img = new Image() { Source = imgsrc, Height = 18, Width = 18 };
                                img.ToolTip = Tips.IchiDisplay.GetExspTip(Tuple, "TR" + hero.Ofcode);
                                if (img.ToolTip == null)
                                    img.ToolTip = hero.PlayerTarAlias;
                                spNewTarget.Children.Add(img);
                            }
                        }
                        mPlayerTars.Add(ut);
                    }));
            }
        }
        public void DelPlayerTar(IEnumerable<ushort> uts)
        {
            Base.Card.Hero hero = Tuple.HL.InstanceHero(SelectHero);
            if (string.IsNullOrEmpty(hero.PlayerTarAlias) && Coss != 0)
            {
                Base.Card.Hero cossHero = Tuple.HL.InstanceHero(Coss);
                if (cossHero != null)
                    hero = cossHero;
            }
            foreach (ushort ut in uts)
            {
                if (mPlayerTars.Contains(ut) && hero != null)
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ImageSource imgsrc = pb.TryFindResource("snapTR" + hero.Ofcode) as ImageSource;
                        if (ut > 0)
                        {
                            StackPanel spOldTarget = pb.AD.GetTokenStackPanel(ut);
                            foreach (var elem in spOldTarget.Children)
                            {
                                if (elem is Image)
                                {
                                    Image oldImg = elem as Image;
                                    if (oldImg.Source == imgsrc)
                                    {
                                        spOldTarget.Children.Remove(oldImg);
                                        break;
                                    }
                                }
                            }
                        }
                        mPlayerTars.Remove(ut);
                    }));
            }
        }
        public bool IsPlayerTarContain(ushort ut)
        {
            return mPlayerTars.Contains(ut);
        }

        private List<string> mSkills;
        public List<string> Skills { get { return mSkills; } }
        public void GainSkill(string code) { GainSkill(new List<string> { code }); }
        public void GainSkill(List<string> codes)
        {
            mSkills.AddRange(codes);
            foreach (string skstr in codes)
            {
                Base.Skill sk = Tuple.SL.EncodeSkill(skstr);
                if (sk != null)
                {
                    if (sk.IsBK)
                        pb.AD.yfJoy.CEE.SetNewBKSkill(sk, Rank);
                    else if (Rank == pb.AD.SelfUid)
                        pb.AD.yfJoy.CEE.SetNewSkill(sk);
                }
            }
        }
        public void LoseSkill(string code) { LoseSkill(new List<string> { code }); }
        public void LoseSkill(List<string> codes)
        {
            mSkills.RemoveAll(p => codes.Contains(p));
            foreach (string skstr in codes)
            {
                Base.Skill sk = Tuple.SL.EncodeSkill(skstr);
                if (sk != null)
                {
                    if (sk.IsBK)
                        pb.AD.yfJoy.CEE.LoseBKSkill(sk.Code);
                    else if (Rank == pb.AD.SelfUid)
                        pb.AD.yfJoy.CEE.LoseSkill(sk.Code);
                }
            }
        }
        public void ClearSkill()
        {
            foreach (string skstr in mSkills)
            {
                Base.Skill sk = Tuple.SL.EncodeSkill(skstr);
                if (sk != null && sk.IsBK)
                    pb.AD.yfJoy.CEE.LoseBKSkill(sk.Code);
            }
            if (Rank == pb.AD.SelfUid)
                pb.AD.yfJoy.CEE.ResetSkill();
            mSkills.Clear();
        }

        public void UpdateExCardSpTitle()
        {
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (mExCards.Count > 0)
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = "特殊装备";
                    if (hro != null)
                    {
                        if (hro.ExCardsAlias != null)
                            alias = hro.ExCardsAlias;
                        Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                        if (guest != null && guest.ExCardsAlias != null)
                            alias = guest.ExCardsAlias;
                    }
                    pb.excardText.Text = alias + " (" + mExCards.Count + ")";
                }
                if (mExSpCards.Count > 0)
                {
                    Base.Card.Hero hro = Tuple.HL.InstanceHero(mHero);
                    string alias = "特殊牌";
                    if (hro != null)
                    {
                        if (hro.PeopleAlias != null)
                            alias = hro.PeopleAlias;
                        Base.Card.Hero guest = Tuple.HL.InstanceHero(Coss);
                        if (guest != null && guest.PeopleAlias != null)
                            alias = guest.PeopleAlias;
                    }
                    pb.expeopleText.Text = alias + " (" + mExSpCards.Count + ")";
                }
            }));
        }

        #endregion 3-points and Status Property

        #region Battle Issue
        public void ClearStatus()
        {
            IsLoved = false;
            Immobilized = false;
            PetDisabled = false;
        }
        public void SetAsRounder()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.SetRounder(); }));
        }
        public void SetAsSpSucc()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.SetSpSuccess(); }));
        }
        public void SetAsSpFail()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.SetSpFail(); }));
        }
        public void SetAsClear()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.SetClear(); }));
        }
        public void SetAsDelegate()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.SetDelegate(); }));
        }
        public void SetAsNotTrigger()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.SetAsNotTrigger(); }));
        }
        public void DisableWeapon()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.DisableWeapon(); }));
        }
        public void DisableArmor()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.DisableArmor(); }));
        }
        public void DisableTrove()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.DisableTrove(); }));
        }
        public void DisableExEquip()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.DisableExEquip(); }));
        }
        public void ResumeExCards()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.ResumeExCards(); }));
        }
        public void ResumePets()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.ResumePets(); }));
        }
        public void ResumeRunes()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.ResumeRune(); }));
        }
        public void ResumeEscues()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.ResumeEscue(); }));
        }
        #endregion Battle Issue

        internal void SetAsLoser()
        {
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.loserIcon.Visibility = System.Windows.Visibility.Visible;
            }));
        }
        internal void SetAsBacker()
        {
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.loserIcon.Visibility = System.Windows.Visibility.Collapsed;
            }));
        }
    }
}