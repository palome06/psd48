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
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AoDisplay : Window
    {
        public Base.LibGroup Tuple { get { return Resources["libGroup"] as Base.LibGroup; } }

        private ZI mzi; private XIVisi mvisi;
        private XIVisi VISI
        {
            get
            {
                if (mvisi != null) return mvisi;
                else if (mzi != null) return mzi.XV;
                else return null;
            }
        }
        private Thread visiThread;

        public AoMix Mix { private set; get; }
        // Hall mode - Gamer mode
        public AoDisplay(string sv, string nick, int ava,
            bool record, bool msglog, int mode, int level, string[] trainer, int team)
        {
            InitializeComponent();
            var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            ResetGameTitle(ass.Name + " v" + ass.Version);
            Init();
            mzi = null; mvisi = null;
            visiThread = new Thread(delegate()
            {
                int port = Base.NetworkCode.HALL_PORT;
                mzi = new ZI(nick, ava, sv, port, team, mode, level, trainer, record, msglog, this);
                mzi.StartHall();
            });
            visiThread.Start();
            yhTV = new OI.AoTV(this);
            tvDict = new Dictionary<string, OI.Television>();
        }
        // Hall mode - Watcher Mode
        public AoDisplay(string sv, int room, ushort uid,
            string name, bool record, bool msglog)
        {
            InitializeComponent();
            Init();
            mzi = null; mvisi = null;
            VW.Cyvi vi = new VW.Cyvi(this, record, msglog);
            visiThread = new Thread(delegate()
            {
                int port = Base.NetworkCode.HALL_PORT + room;
                vi.Init();
                mvisi = new XIVisi(uid, name, 0, vi, sv, room, record, msglog, true, this);
                mvisi.RunAsync();
            });
            visiThread.Start();
            yhTV = new OI.AoTV(this);
            tvDict = new Dictionary<string, OI.Television>();
        }
        // Direct mode
        public AoDisplay(string sv, string nick, int ava, bool watch,
            bool record, bool msglog, int room, int team)
        {
            InitializeComponent();
            Init();
            mzi = null; mvisi = null;
            visiThread = new Thread(delegate()
            {
                int port = Base.NetworkCode.DIR_PORT + room;
                if (port >= 65535)
                    port = Base.NetworkCode.DIR_PORT;
                mvisi = XIVisi.CreateInDirectConnect(sv, port, nick,
                    ava, team, record, watch, msglog, this);
                mvisi.RunAsync();
            });
            visiThread.Start();
            yhTV = new OI.AoTV(this);
            tvDict = new Dictionary<string, OI.Television>();
        }
        // Replay mode
        public AoDisplay(string fileName)
        {
            InitializeComponent();
            Init();
            mzi = null; mvisi = null;
            yfSpeeder.Visibility = Visibility.Visible;
            visiThread = new Thread(delegate()
            {
                mvisi = new XIVisi(fileName, this);
                mvisi.RunAsync();
            });
            visiThread.Start();
            yhTV = new OI.AoTV(this);
            tvDict = new Dictionary<string, OI.Television>();
        }
        // Reconnection mode
        public AoDisplay(string nick, string sv, int room, bool record, bool msglog)
        {
            InitializeComponent();
            var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            ResetGameTitle(ass.Name + " v" + ass.Version);
            Init();
            mzi = null; mvisi = null;
            visiThread = new Thread(delegate()
            {
                int port = Base.NetworkCode.HALL_PORT;
                mzi = ZI.CreateResumeHall(nick, sv, port, record, msglog, room, this);
                mzi.ResumeHall();
            });
            visiThread.Start();
            yhTV = new OI.AoTV(this);
            tvDict = new Dictionary<string, OI.Television>();
        }

        public ushort SelfUid { get { return yfPlayerR2.AoPlayer.Rank; } }

        #region Start and Terminate

        private void Init()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
            PlayerBoard[] allBoards = new PlayerBoard[]{ yfPlayerO1,yfPlayerO2,
                yfPlayerO3, yfPlayerR1,yfPlayerR2,yfPlayerR3};

            foreach (PlayerBoard pb in allBoards)
            {
                pb.AD = this;
                pb.mainGrid.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (selectedTarget != null && !selectedTarget.Contains(pb.AoPlayer.Rank))
                        selectedTarget.Add(pb.AoPlayer.Rank);
                };
                pb.mainGrid.Unchecked += delegate(object sender, RoutedEventArgs e)
                {
                    var st = selectedTarget;
                    if (st != null)
                        st.Remove(pb.AoPlayer.Rank);
                };
            }
            yfBag.AD = this;
            yfOrchis40.Orch.AD = this;
            yfPilesBar.Field.AD = this;
            yfSpeeder.AoDisplay = this;
            yfArena.AD = this;
            Mix = new AoMix(this);
            yfDeal.Visibility = Visibility.Hidden;
            yfMinami.Visibility = Visibility.Hidden;
            //selectedTarget = new ObservableCollection<ushort>();
            //selectedQard = new ObservableCollection<ushort>();
            ResetAllSelectedList();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (visiThread != null && visiThread.IsAlive)
                visiThread.Abort();
            if (VISI != null)
                VISI.CancelThread();
            yfOrchis40.Close();
            Environment.Exit(0);
            // TODO: it leads to assert error in AoVoice, to change to a better way
        }
        //visi.RunAsync();

        public void ResetAllSelectedList()
        {
            //if (selectedTarget != null)
            //    selectedTarget.Clear();
            //if (selectedQard != null)
            //    selectedQard.Clear();
            //if (selectedMon != null)
            //    selectedMon.Clear();
            selectedTarget = null;
            selectedQard = null;
            selectedMon = null;
        }

        public void ResetGameTitle(string title)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Title = title;
            }));
        }

        public void SetRoom(int room)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                yfRoomNumber.Text = room.ToString();
            }));
        }

        #endregion Start and Terminate

        #region TV Related

        public OI.AoTV yhTV;

        public IDictionary<string, OI.Television> tvDict;

        internal OI.Television InsTVDict(string tvTag, OI.Television tv)
        {
            if (!tvDict.ContainsKey(tvTag))
            {
                tvDict.Add(tvTag, tv);
                greatCanvas.Children.Add(tv);
                return tv;
            }
            else
                return tvDict[tvTag];
        }
        internal void RmvTVDict(string tvTag)
        {
            if (tvDict.ContainsKey(tvTag))
            {
                OI.Television tv = tvDict[tvTag];
                greatCanvas.Children.Remove(tv);
                tvDict.Remove(tvTag);
            }
        }
        internal void LockTVDict(string tvTag)
        {
            if (tvDict.ContainsKey(tvTag))
            {
                OI.Television tv = tvDict[tvTag];
                tv.LockRuban();
            }
        }
        internal bool IsTVDictContains(string tvTag)
        {
            return tvDict.ContainsKey(tvTag);
        }

        #endregion TV Related

        #region Group Members and Control Panel

        private ObservableCollection<ushort> selectedTarget;

        //private bool Cancellable { set; get; }

        internal void StartSelectTarget(List<ushort> cands, int r1, int r2)
        {
            IDictionary<ushort, PlayerBoard> dict = new Dictionary<ushort, PlayerBoard>();
            dict.Add(yfPlayerO1.AoPlayer.Rank, yfPlayerO1);
            dict.Add(yfPlayerO2.AoPlayer.Rank, yfPlayerO2);
            dict.Add(yfPlayerO3.AoPlayer.Rank, yfPlayerO3);
            dict.Add(yfPlayerR1.AoPlayer.Rank, yfPlayerR1);
            dict.Add(yfPlayerR2.AoPlayer.Rank, yfPlayerR2);
            dict.Add(yfPlayerR3.AoPlayer.Rank, yfPlayerR3);

            foreach (PlayerBoard pb in dict.Values)
            {
                pb.SetTargetActive(false);
                pb.SetTargetValid(false);
            }
            //selectedTarget.Clear();
            selectedTarget = new ObservableCollection<ushort>();
            //selectedQard = null;

            foreach (ushort ut in cands)
            {
                //dict[ut].mainGrid.Checked += delegate(object sender, RoutedEventArgs e)
                //{
                //    if (!selectedTarget.Contains(ut))
                //        selectedTarget.Add(ut);
                //};
                //dict[ut].mainGrid.Unchecked += delegate(object sender, RoutedEventArgs e)
                //{
                //    selectedTarget.Remove(ut);
                //};
                dict[ut].SetTargetActive(true);
                dict[ut].SetTargetValid(true);
            }
            //EnableJoyPass(cancellable);
            selectedTarget.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedTarget != null)
                {
                    if (r1 == r2)
                    {
                        if (selectedTarget.Count == r1)
                        {
                            if (selectedTarget.Count == 0)
                                EnableJoyDecide("0");
                            else
                                EnableJoyDecide(string.Join(",", selectedTarget));
                        }
                        else if (selectedTarget.Count > r1)
                            dict[selectedTarget[0]].mainGrid.IsChecked = false;
                        else
                            DisableJoyDecide();
                    }
                    else
                    {
                        if (selectedTarget.Count >= r1 && selectedTarget.Count <= r2)
                            EnableJoyDecide(string.Join(",", selectedTarget));
                        else if (selectedTarget.Count > r2)
                            dict[selectedTarget[0]].mainGrid.IsChecked = false;
                        else
                            DisableJoyDecide();
                    }
                }
            };
        }

        internal void FinishSelectTarget()
        {
            PlayerBoard[] pbAll = new PlayerBoard[] {
                yfPlayerO1, yfPlayerO2, yfPlayerO3, yfPlayerR1, yfPlayerR2, yfPlayerR3 };
            foreach (PlayerBoard pb in pbAll)
            {
                pb.mainGrid.IsChecked = false;
                pb.SetTargetActive(false);
                pb.SetTargetValid(true);
            }
        }

        internal void LockSelectTarget()
        {
            PlayerBoard[] pbAll = new PlayerBoard[] {
                yfPlayerO1, yfPlayerO2, yfPlayerO3, yfPlayerR1, yfPlayerR2, yfPlayerR3 };
            foreach (PlayerBoard pb in pbAll)
            {
                if (selectedTarget.Contains(pb.AoPlayer.Rank))
                    pb.SetTargetLock();
                else
                {
                    pb.SetTargetActive(false);
                    pb.SetTargetValid(true);
                }
            }
        }
        //internal Image GetGivenTokenImage(ushort ut)
        //{
        //    PlayerBoard[] pbAll = new PlayerBoard[] {
        //        yfPlayerO1, yfPlayerO2, yfPlayerO3, yfPlayerR1, yfPlayerR2, yfPlayerR3 };
        //    foreach (PlayerBoard pb in pbAll)
        //    {
        //        if (pb.AoPlayer.Rank == ut)
        //            return pb.trGiven;
        //    }
        //    return null;
        //}
        internal StackPanel GetTokenStackPanel(ushort ut)
        {
            PlayerBoard[] pbAll = new PlayerBoard[] {
                yfPlayerO1, yfPlayerO2, yfPlayerO3, yfPlayerR1, yfPlayerR2, yfPlayerR3 };
            foreach (PlayerBoard pb in pbAll)
            {
                if (pb.AoPlayer.Rank == ut)
                    return pb.tokenStack;
            }
            return null;
        }

        private void EnableJoyDecide(string message)
        {
            yfJoy.DecideMessage = message;
            yfJoy.CEE.DecideValid = true;
        }
        private void DisableJoyDecide() { yfJoy.CEE.DecideValid = false; }

        #endregion Group Members and Control Panel
        #region Cards Selection and Control Panel

        private ObservableCollection<ushort> selectedQard;

        internal void InsSelectedCard(ushort ut) { if (selectedQard != null) selectedQard.Add(ut); }
        internal void DelSelectedCard(ushort ut) { if (selectedQard != null) selectedQard.Remove(ut); }
        internal void StartSelectQard(List<ushort> cands, int r1, int r2)
        {
            AoPlayer wata = yfPlayerR2.AoPlayer;
            List<ushort> bags = yfBag.Me.Tux.Intersect(cands).ToList();
            bool weaponEab = wata.Weapon != 0 && cands.Contains(wata.Weapon);
            bool armorEab = wata.Armor != 0 && cands.Contains(wata.Armor);
            bool troveEab = wata.Trove != 0 && cands.Contains(wata.Trove);
            List<ushort> excs = wata.GetExCardsList().Intersect(cands).ToList();
            bool exeqEab = wata.ExEquip != 0 && cands.Contains(wata.ExEquip);
            List<ushort> fakeq = wata.Fakeq.Keys.Intersect(cands).ToList();

            selectedQard = new ObservableCollection<ushort>();
            //selectedTarget = null;

            yfBag.EnableTux(bags);
            if (weaponEab)
                yfPlayerR2.EnableWeapon();
            if (armorEab)
                yfPlayerR2.EnableArmor();
            if (troveEab)
                yfPlayerR2.EnableTrove();
            if (excs.Count > 0)
                yfPlayerR2.EnableExCards(excs);
            if (exeqEab)
                yfPlayerR2.EnableExEquip();
            if (fakeq.Count > 0)
                yfPlayerR2.EnableFakeq(fakeq);

            selectedQard.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedQard != null)
                {
                    if (r1 == r2)
                    {
                        if (selectedQard.Count == r1)
                        {
                            if (selectedQard.Count == 0)
                                EnableJoyDecide("0");
                            else
                                EnableJoyDecide(string.Join(",", selectedQard));
                        }
                        else if (selectedQard.Count > r1)
                            RemoveDupQard(bags, weaponEab, armorEab, troveEab, excs, fakeq);
                        else
                            DisableJoyDecide();
                    }
                    else
                    {
                        if (selectedQard.Count >= r1 && selectedQard.Count <= r2)
                            EnableJoyDecide(string.Join(",", selectedQard));
                        else if (selectedQard.Count > r2)
                            RemoveDupQard(bags, weaponEab, armorEab, troveEab, excs, fakeq);
                        else
                            DisableJoyDecide();
                    }
                }
            };
        }
        private void RemoveDupQard(List<ushort> bags, bool weaponEab,
            bool armorEab, bool troveEab, List<ushort> excs, List<ushort> fakeq)
        {
            ushort ut = selectedQard[0];
            if (bags.Contains(ut))
            {
                Card.Ruban ruban = yfBag.GetRuban(selectedQard[0]);
                if (ruban != null)
                {
                    ruban.cardBody.IsChecked = false;
                    return;
                }
            }
            if (weaponEab || armorEab || troveEab)
            {
                Card.RubanLock rubanlock = yfPlayerR2.GetStandardRubanLock(ut);
                if (rubanlock != null)
                {
                    rubanlock.cardBody.IsChecked = false;
                    return;
                }
                else if (troveEab && yfPlayerR2.troveBox != null && yfPlayerR2.troveBox.UT == ut)
                {
                    yfPlayerR2.troveBox.cardBody.IsChecked = false;
                    return;
                }
            }
            if (excs.Contains(ut))
            {
                ushort rk = yfPlayerR2.AoPlayer.Rank;
                if (tvDict.ContainsKey(rk + "SEC"))
                {
                    OI.Television tv = tvDict[rk + "SEC"];
                    Card.Ruban ruban = tv.GetRuban(selectedQard[0]);
                    if (ruban != null)
                    {
                        ruban.cardBody.IsChecked = false;
                        return;
                    }
                }
            }
            if (fakeq.Contains(ut))
            {
                ushort rk = yfPlayerR2.AoPlayer.Rank;
                if (tvDict.ContainsKey(rk + "SFQ"))
                {
                    OI.Television tv = tvDict[rk + "SFQ"];
                    Card.Ruban ruban = tv.GetRuban(selectedQard[0]);
                    if (ruban != null)
                    {
                        ruban.cardBody.IsChecked = false;
                        return;
                    }
                }
            }
        }
        internal void FinishSelectQard()
        {
            yfBag.ResumeTux();
            yfPlayerR2.DisableWeapon();
            yfPlayerR2.DisableArmor();
            yfPlayerR2.DisableTrove();
            yfPlayerR2.ResumeExCards();
            yfPlayerR2.DisableExEquip();
            yfPlayerR2.ResumeFakeq();
            RmvTVDict(yfPlayerR2.AoPlayer.Rank + "SEC");
            RmvTVDict(yfPlayerR2.AoPlayer.Rank + "SFQ");
        }
        internal void LockSelectQard()
        {
            yfBag.LockTux();
            yfPlayerR2.LockWeapon();
            yfPlayerR2.LockArmor();
            yfPlayerR2.LockTrove();
            //yfPlayerR2.LockExCards();
            yfPlayerR2.LockExEquip();
        }

        internal void StartSelectTX(List<ushort> cands)
        {
            AoPlayer wata = yfPlayerR2.AoPlayer;
            List<ushort> bags = yfBag.Me.Tux.Intersect(cands).ToList();
            bool weaponEab = wata.Weapon != 0 && cands.Contains(wata.Weapon);
            bool armorEab = wata.Armor != 0 && cands.Contains(wata.Armor);
            bool troveEab = wata.Trove != 0 && cands.Contains(wata.Trove);
            List<ushort> excs = wata.GetExCardsList().Intersect(cands).ToList();
            bool exeqEab = wata.ExEquip != 0 && cands.Contains(wata.ExEquip);
            List<ushort> fakeq = wata.Fakeq.Keys.Intersect(cands).ToList();

            selectedQard = new ObservableCollection<ushort>();
            //selectedTarget = null;

            yfBag.EnableTux(bags);
            if (weaponEab)
                yfPlayerR2.EnableWeapon();
            if (armorEab)
                yfPlayerR2.EnableArmor();
            if (troveEab)
                yfPlayerR2.EnableTrove();
            if (excs.Count > 0)
                yfPlayerR2.EnableExCards(excs);
            if (exeqEab)
                yfPlayerR2.EnableExEquip();
            if (fakeq.Count > 0)
                yfPlayerR2.EnableFakeq(fakeq);

            selectedQard.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedQard != null)
                {
                    if (selectedQard.Count == 1)
                        EnableJoyDecide("TX" + selectedQard[0]);
                    else if (selectedQard.Count > 1)
                        RemoveDupQard(bags, weaponEab, armorEab, troveEab, excs, fakeq);
                    else
                        DisableJoyDecide();
                }
            };
        }

        private ObservableCollection<ushort> selectedMon;

        internal void InsSelectedMon(ushort ut) { if (selectedMon != null) selectedMon.Add(ut); }
        internal void DelSelectedMon(ushort ut) { if (selectedMon != null) selectedMon.Remove(ut); }
        internal void StartSelectPT(List<ushort> cands, bool self)
        {
            AoPlayer wata = yfPlayerR2.AoPlayer;
            //List<ushort> pets = wata.Pets.Where(p => p != 0).Intersect(cands).ToList();
            //List<ushort> opts = wata.Pets.Where(p => p != 0).Except(pets).ToList();

            selectedMon = new ObservableCollection<ushort>();
            ushort whoes = 0;
            if (self)
            {
                whoes = yfPlayerR2.AoPlayer.Rank;
                yfPlayerR2.EnablePets(wata.Pets.Intersect(cands).ToList());
            }
            else
            {
                List<ushort> pets = yfPlayerR1.AoPlayer.Pets.Intersect(cands).ToList();
                if (pets.Count > 0)
                {
                    yfPlayerR1.EnablePets(pets);
                    whoes = yfPlayerR1.AoPlayer.Rank;
                }
                pets = yfPlayerR2.AoPlayer.Pets.Intersect(cands).ToList();
                if (pets.Count > 0)
                {
                    yfPlayerR2.EnablePets(pets);
                    whoes = yfPlayerR2.AoPlayer.Rank;
                }
                pets = yfPlayerR3.AoPlayer.Pets.Intersect(cands).ToList();
                if (pets.Count > 0)
                {
                    yfPlayerR3.EnablePets(pets);
                    whoes = yfPlayerR3.AoPlayer.Rank;
                }
            }

            selectedMon.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedMon != null)
                {
                    if (selectedMon.Count == 1)
                        EnableJoyDecide("PT" + selectedMon[0]);
                    else if (selectedMon.Count > 1)
                    {
                        ushort first = selectedMon[0];
                        if (tvDict.ContainsKey(whoes + "SPT"))
                        {
                            OI.Television tv = tvDict[whoes + "SPT"];
                            Card.Ruban ruban = tv.GetRuban(selectedMon[0]);
                            if (ruban != null)
                            {
                                ruban.cardBody.IsChecked = false;
                                return;
                            }
                        }
                    }
                    else
                        DisableJoyDecide();
                }
            };
        }

        internal void FinishSelectPT()
        {
            PlayerBoard[] pbs = new PlayerBoard[] { yfPlayerR1, yfPlayerR2,
                yfPlayerR3, yfPlayerO1, yfPlayerO2, yfPlayerO3 };
            foreach (PlayerBoard pb in pbs)
            {
                pb.ResumePets();
                RmvTVDict(pb.AoPlayer.Rank + "SPT");
            }
        }

        private ObservableCollection<ushort> selectedRune;
        internal void InsSelectedRune(ushort ut) { if (selectedRune != null) selectedRune.Add(ut); }
        internal void DelSelectedRune(ushort ut) { if (selectedRune != null) selectedRune.Remove(ut); }
        internal void StartSelectSF(List<ushort> cands)
        {
            AoPlayer wata = yfPlayerR2.AoPlayer;
            //List<ushort> pets = wata.Pets.Where(p => p != 0).Intersect(cands).ToList();
            //List<ushort> opts = wata.Pets.Where(p => p != 0).Except(pets).ToList();

            selectedRune = new ObservableCollection<ushort>();
            ushort whoes = yfPlayerR2.AoPlayer.Rank;
            yfPlayerR2.EnableRune(wata.Runes.Intersect(cands).ToList());

            selectedRune.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedRune != null)
                {
                    if (selectedRune.Count == 1)
                        EnableJoyDecide("FW" + selectedRune[0]);
                    else if (selectedRune.Count > 1)
                    {
                        ushort first = selectedRune[0];
                        if (tvDict.ContainsKey(whoes + "SFW"))
                        {
                            OI.Television tv = tvDict[whoes + "SFW"];
                            Card.Ruban ruban = tv.GetRuban(selectedRune[0]);
                            if (ruban != null)
                            {
                                ruban.cardBody.IsChecked = false;
                                return;
                            }
                        }
                    }
                    else
                        DisableJoyDecide();
                }
            };
        }
        internal void FinishSelectSF()
        {
            yfPlayerR2.ResumeRune();
            RmvTVDict(yfPlayerR2.AoPlayer.Rank + "SFW");
        }

        private ObservableCollection<ushort> selectedEscue;
        internal void InsSelectedEscue(ushort ut) { if (selectedEscue != null) selectedEscue.Add(ut); }
        internal void DelSelectedEscue(ushort ut) { if (selectedEscue != null) selectedEscue.Remove(ut); }
        internal void StartSelectYJ(List<ushort> cands)
        {
            AoPlayer wata = yfPlayerR2.AoPlayer;
            //List<ushort> pets = wata.Pets.Where(p => p != 0).Intersect(cands).ToList();
            //List<ushort> opts = wata.Pets.Where(p => p != 0).Except(pets).ToList();

            selectedEscue = new ObservableCollection<ushort>();
            ushort whoes = yfPlayerR2.AoPlayer.Rank;
            yfPlayerR2.EnableEscue(wata.Escue.Intersect(cands).ToList());

            selectedEscue.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedEscue != null)
                {
                    if (selectedEscue.Count == 1)
                        EnableJoyDecide("YJ" + selectedEscue[0]);
                    else if (selectedEscue.Count > 1)
                    {
                        ushort first = selectedEscue[0];
                        if (tvDict.ContainsKey(whoes + "SYJ"))
                        {
                            OI.Television tv = tvDict[whoes + "SYJ"];
                            Card.Ruban ruban = tv.GetRuban(selectedEscue[0]);
                            if (ruban != null)
                            {
                                ruban.cardBody.IsChecked = false;
                                return;
                            }
                        }
                    }
                    else
                        DisableJoyDecide();
                }
            };
        }
        internal void FinishSelectYJ()
        {
            yfPlayerR2.ResumeEscue();
            RmvTVDict(yfPlayerR2.AoPlayer.Rank + "SYJ");
        }
        
        private ObservableCollection<ushort> selectedExsp;
        internal void InsSelectedExsp(ushort ut) { if (selectedExsp != null) selectedExsp.Add(ut); }
        internal void DelSelectedExsp(ushort ut) { if (selectedExsp != null) selectedExsp.Remove(ut); }
        internal void StartSelectExsp(List<ushort> cands)
        {
            AoPlayer wata = yfPlayerR2.AoPlayer;
            //List<ushort> pets = wata.Pets.Where(p => p != 0).Intersect(cands).ToList();
            //List<ushort> opts = wata.Pets.Where(p => p != 0).Except(pets).ToList();
            selectedExsp = new ObservableCollection<ushort>();
            ushort whoes = 0;
            Action<PlayerBoard> findOwner = (yp) =>
            {
                List<ushort> exsp = yp.AoPlayer.ExSpCards.Where(p => p.StartsWith("I"))
                    .Select(p => ushort.Parse(p.Substring("I".Length))).Intersect(cands).ToList();
                if (exsp.Count > 0)
                {
                    yp.EnableExspCards(exsp);
                    whoes = yp.AoPlayer.Rank;
                }
            };
            findOwner(yfPlayerR1);
            findOwner(yfPlayerR2);
            findOwner(yfPlayerR3);

            selectedExsp.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (selectedExsp != null)
                {
                    if (selectedExsp.Count == 1)
                        EnableJoyDecide("I" + selectedExsp[0]);
                    else if (selectedExsp.Count > 1)
                    {
                        ushort first = selectedExsp[0];
                        if (tvDict.ContainsKey(whoes + "SES"))
                        {
                            OI.Television tv = tvDict[whoes + "SES"];
                            Card.Ruban ruban = tv.GetRuban(selectedExsp[0]);
                            if (ruban != null)
                            {
                                ruban.cardBody.IsChecked = false;
                                return;
                            }
                        }
                    }
                    else
                        DisableJoyDecide();
                }
            };
        }

        internal void FinishSelectExsp()
        {
            PlayerBoard[] pbs = new PlayerBoard[] { yfPlayerR1, yfPlayerR2,
                yfPlayerR3, yfPlayerO1, yfPlayerO2, yfPlayerO3 };
            foreach (PlayerBoard pb in pbs)
            {
                pb.ResumeExsp();
                RmvTVDict(pb.AoPlayer.Rank + "SES");
            }
        }
        #endregion Cards Selection and Control Panel

        public ushort Player2Position(ushort ut)
        {
            if (yfPlayerR1.AoPlayer.Rank == ut)
                return 2;
            else if (yfPlayerR2.AoPlayer.Rank == ut)
                return 4;
            else if (yfPlayerR3.AoPlayer.Rank == ut)
                return 6;
            else if (yfPlayerO1.AoPlayer.Rank == ut)
                return 1;
            else if (yfPlayerO2.AoPlayer.Rank == ut)
                return 3;
            else if (yfPlayerO3.AoPlayer.Rank == ut)
                return 5;
            else
                return 0;
        }

        internal void ShowProgressBar(ushort ut)
        {
            if (yfPlayerO1.AoPlayer.Rank == ut)
                yfMoonlightO1.ABegin();
            else if (yfPlayerO2.AoPlayer.Rank == ut)
                yfMoonlightO2.ABegin();
            else if (yfPlayerO3.AoPlayer.Rank == ut)
                yfMoonlightO3.ABegin();
            else if (yfPlayerR1.AoPlayer.Rank == ut)
                yfMoonlightR1.ABegin();
            else if (yfPlayerR2.AoPlayer.Rank == ut)
                yfMoonlightR2.ABegin();
            else if (yfPlayerR3.AoPlayer.Rank == ut)
                yfMoonlightR3.ABegin();
        }

        internal void HideProgressBar(ushort ut)
        {
            if (yfPlayerO1.AoPlayer.Rank == ut)
                yfMoonlightO1.AStop();
            else if (yfPlayerO2.AoPlayer.Rank == ut)
                yfMoonlightO2.AStop();
            else if (yfPlayerO3.AoPlayer.Rank == ut)
                yfMoonlightO3.AStop();
            else if (yfPlayerR1.AoPlayer.Rank == ut)
                yfMoonlightR1.AStop();
            else if (yfPlayerR2.AoPlayer.Rank == ut)
                yfMoonlightR2.AStop();
            else if (yfPlayerR3.AoPlayer.Rank == ut)
                yfMoonlightR3.AStop();
        }

        internal void SetPlayerXBSlot(bool enabled)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (enabled)
                {
                    yfPlayerO1.SetAsWithXB();
                    yfPlayerO2.SetAsWithXB();
                    yfPlayerO3.SetAsWithXB();
                    yfPlayerR1.SetAsWithXB();
                    yfPlayerR2.SetAsWithXB();
                    yfPlayerR3.SetAsWithXB();
                }
                else
                {
                    yfPlayerO1.SetAsWithOutXB();
                    yfPlayerO2.SetAsWithOutXB();
                    yfPlayerO3.SetAsWithOutXB();
                    yfPlayerR1.SetAsWithOutXB();
                    yfPlayerR2.SetAsWithOutXB();
                    yfPlayerR3.SetAsWithOutXB();
                }
            }));
        }
        #region Replay Part
        internal void ReplayPrev() { mvisi.ReplayPrev(); }
        internal void ReplayPlay() { mvisi.ReplayPlay(); }
        internal void ReplayPause() { mvisi.ReplayPause(); }
        internal void ReplayNext() { mvisi.ReplayNext(); }
        internal string GetMagi() { return mvisi.GetMagi(); }
        #endregion Replay Part

        //private void PlayerBoard_KeyDown_1(object sender, KeyEventArgs e)
        //{
        //    MessageBox.Show("e^");
        //    MessageBox.Show(e.Key.ToString());

        //public void DisplayChat(ushort who, string chatText)
        //{
        //    Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        string nick = VISI.A0P[who].Nick;
        //        int selHero = VISI.A0P[who].SelectHero;
        //        string hero = selHero == 0 ? null : VISI.zd.Hero(selHero);
        //        yfMigi.DisplayChat(nick, hero, chatText);
        //    }));
        //}

        internal void DisplayChat(string nick, string chatText)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                yfMigi.DisplayChat(nick, null, chatText);
            }));
        }
        internal void SetCanan(CananPaint.CananSignal signal)
        {
            yfCanan.SetCanan(signal);
        }
    }
}
