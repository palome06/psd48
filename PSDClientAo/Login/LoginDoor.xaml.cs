using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PSD.ClientAo.Login
{
    /// <summary>
    /// Interaction logic for LoginDoor.xaml
    /// </summary>
    public partial class LoginDoor : Window
    {
        public const int PORT_DEF = 40201;

        private MediaPlayer mp;

        public LoginDoor()
        {
            InitializeComponent();
            var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            Title = (ass.Name + " v" + ass.Version) + " 登陆界面";
            DataContext = this;

            //portTextBox.Text = TextBox_TextChanged_1"0";
            //new Window1().Show();
            IsHallMode = true;
            ResetOptions();
            IsRoomGained = false;

            mp = new MediaPlayer();
            //mp.Open(new Uri(@"pack://application:,,,/PSDRisoLib;component/Resources/Sound/MainLogin.mp3"));
            //string mp3Path = @"D:\Gradming\psd48\PSDRisoLib\Resources\Sound\MainLogin.wav";
            //string mp3Path = @"pack://application:,,,/PSDRisoLib;component/Resources/Sound/MainLogin.mp3";
            string mp3Path = @"pack://siteoforigin:,,,/Resources/fmxy.mp3";
            Uri uri = new Uri(mp3Path, UriKind.RelativeOrAbsolute);
            mp.Open(uri);
            mp.Play();

            md_openDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Multiselect = false, RestoreDirectory = true,
                Filter = "PSG|*.psg"
            };
        }

        private ZI zi;
        private Thread ziThread;
        private bool IsRoomGained { set; get; }
        private int selectedRoom;
        public int DecidedRoom { private set; get; }

        public void ReportRoom(List<int> rooms)
        {
            RoomListBox.UnselectAll();
            RoomListBox.Items.Clear();
            for (int i = 0; i < rooms.Count; ++i)
                RoomListBox.Items.Add(new Label() { Content = rooms[i].ToString() });
        }
        // type = 0, empty hall; type = 1, request fails
        public void ReportWatchFail(int type)
        {
            if (type == 0)
                MessageBox.Show("抱歉，当前没有闲置的房间.");
            else if (type == 1)
                MessageBox.Show("抱歉，连接失败.");
        }

        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            string addr = addrTextBox.Text;
            if ("本机" == addr)
                addr = "127.0.0.1";
            string nick = userTextBox.Text;
            int ava = 0;
            if (IsRoomGained)
            {
                bool record = IsHallRecord;
                DecidedRoom = selectedRoom;
                //MessageBox.Show("Waiting!");
                return;
            }
            if (IsReplayMode)
            {
                AoDisplay a0d = new AoDisplay(VideoPathBox.Text);
                a0d.Show();
                this.Close();
            }
            else
            {
                // int ava = portial.GetUid();
                if (IsHallMode)
                {
                    bool watch = IsHallWatched;
                    bool record = IsHallRecord;
                    bool resume = IsHallReconnect;
                    bool msglog = true;
                    int mode = IsHallSelModeEnabled ? SelMode : Base.Rules.RuleCode.DEF_CODE;
                    //int pkg = GetPkgCode();
                    int level = IsHallLevelEnabled == false ? 0 :
                         ((PkgMode << 1) | (LvTestCheckBox.IsChecked == true ? 1 : 0));
                    int team = IsHallTeamEnabled ? HallTeamMode : Base.Rules.RuleCode.DEF_CODE;
                    string[] trainer = (LvTestCheckBox.IsChecked == true && LvRingText.Text.Length > 0) ?
                        LvRingText.Text.Split(',') : null;
                    if (resume)
                    {
                        int room;
                        if (!int.TryParse(roomInputTextBox.Text, out room))
                            room = 0;
                        AoDisplay a0d = new AoDisplay(nick, addr, room, record, msglog);
                        a0d.Show();
                        this.Close();
                    }
                    else if (!watch)
                    {
                        AoDisplay a0d = new AoDisplay(addr, nick, ava, record, msglog, mode, level, trainer, team);
                        a0d.Show();
                        this.Close();
                    }
                    else
                    {
                        int port = Base.NetworkCode.HALL_PORT;
                        zi = new ZI(nick, addr, port, record, this);
                        ziThread = new Thread(delegate() { zi.StartWatchHall(); });
                        ziThread.Start();
                        IsRoomGained = true;
                    }
                }
                else
                {
                    bool watch = IsDirWatched;
                    bool record = IsDirRecord;
                    bool msglog = true;
                    int room;
                    if (!int.TryParse(DirRoomTextBox.Text, out room))
                        room = 0;
                    int team = IsDirTeamEnabled ? DirTeamMode : 0;
                    AoDisplay a0d = new AoDisplay(addr, nick, ava, watch, record, msglog, room, team);
                    a0d.Show();
                    this.Close();
                }
            }
        }
        private void WatchListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Label label = RoomListBox.SelectedItem as Label;
            if (label != null)
            {
                string text = label.Content as string;
                int idx = text.IndexOf(' ');
                selectedRoom = int.Parse(Util.Substring(text, 0, idx));
            }
            else
                selectedRoom = 0;
        }
        private void ReconnectRoomInputChecked(object sender, RoutedEventArgs e)
        {
            roomInputTextBox.IsEnabled = true;
        }
        private void ReconnectRoomInputUnchecked(object sender, RoutedEventArgs e)
        {
            roomInputTextBox.IsEnabled = false;
        }

        private void ButtonResetClick(object sender, RoutedEventArgs e) { ResetOptions(); }
        private void ResetOptions()
        {
            addrTextBox.Text = "本机";
            userTextBox.Text = GetRandomHeroName();
            //portTextBox.Text = PORT_DEF.ToString();
            //(teamRadioPanel.Children[0] as RadioButton).IsChecked = true;
            HallSelModeCB.IsChecked = null;
            HallPkgCB.IsChecked = null;
            HallTeamCB.IsChecked = null;
            DirTeamCB.IsChecked = null;
            HallLevelCB.IsChecked = null;

            HallSelModeCB.IsChecked = true;
            HallPkgCB.IsChecked = false;
            HallTeamCB.IsChecked = false;
            DirTeamCB.IsChecked = false;
            HallLevelCB.IsChecked = true;
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            //Environment.Exit(0);
            if (ziThread != null && ziThread.IsAlive)
                ziThread.Abort();
            if (mp != null)
                mp.Close();
        }

        private Random random = new Random();

        private string GetRandomHeroName()
        {
            string[] names = new string[] {"凤天凌","瑚月","姬亭","迦兰多","蓉霜","白王",
                "魂", "左殇", "银翎", "蝶", "瓷儿", "玄鱼", "冷荼", "雷当", "沐小葵", "长鸿",
                "闻人羽","乐无异","夏夷则","阿阮","沈夜",
                "夏侯仪","冰璃","封铃笙","慕容璇玑","古德伦",
                "楚歌","海棠","甄瑶","韩靖","沈嫣","杜晏","夏侯翎",
                "南宫飞云","燕若雪","柴嵩","赵无双","唐影","秋依水",
                "越今朝","越祈","闲卿","洛昭言","扁络桓","葛清霏","绮里小媛","嬴旭危"
            };
            int idx = random.Next(names.Length);
            return names[idx];
        }
        #region Basis Checkboxes
        private bool IsReplayMode { set; get; }
        private bool IsHallMode { set; get; }

        private bool IsHallWatched { set; get; }
        private bool IsHallReconnect { set; get; }

        private bool IsHallRecord { set; get; }
        private bool IsHallSelModeEnabled { set; get; }
        private bool IsHallPkgEnabled { set; get; }
        private bool IsHallLevelEnabled { set; get; }
        private bool IsHallTeamEnabled { set; get; }
        private bool IsHallMsgLogEnabled { set; get; }

        private void HallPlayChecked(object sender, RoutedEventArgs e)
        {
            IsHallWatched = false; IsHallReconnect = false;
            if (SelDetailGrid != null)
                SelDetailGrid.Visibility = Visibility.Visible;
            if (RoomListGrid != null)
                RoomListGrid.Visibility = Visibility.Collapsed;
            if (RoomInputGrid != null)
                RoomInputGrid.Visibility = Visibility.Collapsed;
        }
        private void HallWatchChecked(object sender, RoutedEventArgs e)
        {
            IsHallWatched = true; IsHallReconnect = false;
            SelDetailGrid.Visibility = Visibility.Collapsed;
            RoomListGrid.Visibility = Visibility.Visible;
            RoomInputGrid.Visibility = Visibility.Collapsed;
        }
        private void HallReconnectChecked(object sender, RoutedEventArgs e)
        {
            IsHallWatched = false; IsHallReconnect = true;
            //SelDetailGrid.IsEnabled = false;
            SelDetailGrid.Visibility = Visibility.Collapsed;
            RoomListGrid.Visibility = Visibility.Collapsed;
            RoomInputGrid.Visibility = Visibility.Visible;
        }

        private void HallRecordChecked(object sender, RoutedEventArgs e)
        {
            IsHallRecord = true;
        }
        private void HallRecordUnChecked(object sender, RoutedEventArgs e)
        {
            IsHallRecord = false;
        }
        private void HallSelModeEnabled(object sender, RoutedEventArgs e)
        {
            HallSelModePanel.IsEnabled = true;
            IsHallSelModeEnabled = true;
        }
        private void HallSelModeDisabled(object sender, RoutedEventArgs e)
        {
            HallSelModePanel.IsEnabled = false;
            IsHallSelModeEnabled = false;
        }
        private void HallPkgEnabled(object sender, RoutedEventArgs e)
        {
            HallPkgPanel.IsEnabled = true;
            IsHallPkgEnabled = true;
        }
        private void HallPkgDisabled(object sender, RoutedEventArgs e)
        {
            HallPkgPanel.IsEnabled = false;
            IsHallPkgEnabled = false;
        }
        private void HallLevelEnabled(object sender, RoutedEventArgs e)
        {
            HallLevelPanel.IsEnabled = true;
            IsHallLevelEnabled = true;
        }
        private void HallLevelDisabled(object sender, RoutedEventArgs e)
        {
            HallLevelPanel.IsEnabled = false;
            IsHallLevelEnabled = false;
        }
        private void HallTeamEnabled(object sender, RoutedEventArgs e)
        {
            HallTeamPanel.IsEnabled = true;
            IsHallTeamEnabled = true;
        }
        private void HallTeamDisabled(object sender, RoutedEventArgs e)
        {
            HallTeamPanel.IsEnabled = false;
            IsHallTeamEnabled = false;
        }
        private void HallMsgLogChecked(object sender, RoutedEventArgs e)
        {
            IsHallMsgLogEnabled = true;
        }
        private void HallMsgLogUnChecked(object sender, RoutedEventArgs e)
        {
            IsHallMsgLogEnabled = false;
        }
        #endregion Basis Checkboxes
        #region Options
        private int SelMode { set; get; }
        private void Sel31Decided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_31; }
        private void SelNMDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_NM; }
        private void SelRMDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_RM; }
        private void SelBPDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_BP; }
        private void SelRDDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_RD; }
        private void SelZYDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_ZY; }
        private void SelCMDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_CM; }
        private void SelCPDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_CP; }
        private void SelINDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_IN; }
        private void SelSSDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_SS; }
        private void SelCJDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_CJ; }
        private void SelTCDecided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_TC; }
        private void Sel00Decided(object sender, RoutedEventArgs e) { SelMode = Base.Rules.RuleCode.MODE_00; }

        private int PkgMode { set; get; }
        private void Lv0Decided(object sender, RoutedEventArgs e) { PkgMode = 1; }
        private void Lv1Decided(object sender, RoutedEventArgs e) { PkgMode = 2; }
        private void Lv2Decided(object sender, RoutedEventArgs e) { PkgMode = 3; }
        private void Lv3Decided(object sender, RoutedEventArgs e) { PkgMode = 4; }
        private void Lv4Decided(object sender, RoutedEventArgs e) { PkgMode = 5; }

        private int GetPkgCode()
        {
            if (!IsHallPkgEnabled)
                return 0;
            int result = 0;
            CheckBox[] cbs = new CheckBox[] { Pkg1CheckBox, Pkg2CheckBox, Pkg3CheckBox,
                Pkg4CheckBox, Pkg5CheckBox, Pkg7CheckBox, Pkg6CheckBox };
            int besu = 1;
            for (int i = 0; i < cbs.Length; ++i, besu <<= 1)
            {
                if (cbs[i].IsChecked == true)
                    result |= besu;
            }
            return result;
        }
        private void HallPkgAllSelClick(object sender, RoutedEventArgs e)
        {
            CheckBox[] cbs = new CheckBox[] { Pkg1CheckBox, Pkg2CheckBox, Pkg3CheckBox,
                Pkg4CheckBox, Pkg5CheckBox, Pkg7CheckBox, Pkg6CheckBox };
            foreach (CheckBox cb in cbs)
                cb.IsChecked = true;
            HallPkgCB.IsChecked = true;
        }

        private int HallTeamMode { set; get; }
        private void HallTeamNoDecided(object sender, RoutedEventArgs e)
        {
            HallTeamMode = Base.Rules.RuleCode.HOPE_NO;
        }
        private void HallTeamYesDecided(object sender, RoutedEventArgs e)
        {
            HallTeamMode = Base.Rules.RuleCode.HOPE_YES;
        }
        private void HallTeamAkaDecided(object sender, RoutedEventArgs e)
        {
            HallTeamMode = Base.Rules.RuleCode.HOPE_AKA;
        }
        private void HallTeamAoDecided(object sender, RoutedEventArgs e)
        {
            HallTeamMode = Base.Rules.RuleCode.HOPE_AO;
        }
        private void HallTeamIPDecided(object sender, RoutedEventArgs e)
        {
            HallTeamMode = Base.Rules.RuleCode.HOPE_IP;
        }
        #endregion Options
        #region Dir Checkbox
        private bool IsDirWatched { set; get; }
        private bool IsDirRecord { set; get; }
        private bool IsDirTeamEnabled { set; get; }
        private bool IsDirMsgLogEnabled { set; get; }

        private void DirWatchChecked(object sender, RoutedEventArgs e)
        {
            IsDirWatched = true;
            WatchSelDetailGrid.IsEnabled = false;
        }
        private void DirWatchUnChecked(object sender, RoutedEventArgs e)
        {
            IsDirWatched = false;
            WatchSelDetailGrid.IsEnabled = true;
        }
        private void DirRecordChecked(object sender, RoutedEventArgs e)
        {
            IsDirRecord = true;
        }
        private void DirRecordUnChecked(object sender, RoutedEventArgs e)
        {
            IsDirWatched = false;
        }
        private void DirMsgLogChecked(object sender, RoutedEventArgs e)
        {
            IsDirMsgLogEnabled = true;
        }
        private void DirMsgLogUnChecked(object sender, RoutedEventArgs e)
        {
            IsDirMsgLogEnabled = false;
        }
        private void DirTeamEnabled(object sender, RoutedEventArgs e)
        {
            IsDirTeamEnabled = true;
            DirTeamPanel.IsEnabled = true;
        }
        private void DirTeamDisabled(object sender, RoutedEventArgs e)
        {
            IsDirTeamEnabled = false;
            DirTeamPanel.IsEnabled = false;
        }
        private int DirTeamMode { set; get; }
        private void DirTeamAkaDecided(object sender, RoutedEventArgs e)
        {
            DirTeamMode = Base.Rules.RuleCode.HOPE_AKA;
        }
        private void DirTeamAoDecided(object sender, RoutedEventArgs e)
        {
            DirTeamMode = Base.Rules.RuleCode.HOPE_AO;
        }
        private void DirTeamIPDecided(object sender, RoutedEventArgs e)
        {
            DirTeamMode = Base.Rules.RuleCode.HOPE_IP;
        }
        #endregion Dir Checkbox

        #region Replay
        private System.Windows.Forms.OpenFileDialog md_openDialog;

        private void VideoBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            if (md_openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                VideoPathBox.Text = md_openDialog.FileName;
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as TabControl;
            var selected = item.SelectedItem as TabItem;

            if (selected == RequestHourTab)
                mainBoard.Opacity = 0.95;
            else
                mainBoard.Opacity = 0.8;

            if (selected == HallTab) // Hall mode
            {
                IsHallMode = true;
                IsReplayMode = false;
            }
            else if (selected == DirTab)
            {
                IsHallMode = false;
                IsReplayMode = false;
            }
            else if (selected == VideoTab)
                IsReplayMode = true;
        }
        #endregion Replay

        private void Mode00Gate(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "AKB48Show!")
            {
                Sel00Radio.IsEnabled = true;
                Lv4Radio.IsEnabled = true;
            }
            else
            {
                Sel00Radio.IsEnabled = false;
                Lv4Radio.IsEnabled = false;
            }
        }
    }
}
