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
using System.Collections.Specialized;
using PSD.ClientAo.Card;

namespace PSD.ClientAo.OI
{
    /// <summary>
    /// Interaction logic for DealTable.xaml
    /// </summary>
    public partial class DealTable : UserControl
    {
        public const int UNFOLD_LIMIT = 4;

        public const int HITORI_SIZE = 90;
        public const int HITORI_LAYER = 130;

        public const int MAX_LENGTHCNT = UNFOLD_LIMIT * HITORI_SIZE + 10;

        public Base.LibGroup Tuple
        {
            set
            {
                Deal = new AoDeal(this, value);

                //List<string> ls1 = (new string[] { "T0", "T0", "T0", "T0", "T0", "T0", "T0" }).ToList();
                //List<string> ls2 = new List<string>();
                ////List<string> ls2 = (new string[] { "T53", "T54" }).ToList();
                //Deal.Show(ls1, ls2);
            }
        }

        public AoDeal Deal { get; set; }

        private bool Cancelable { set; get; }

        private bool Keep { set; get; }

        private string TableType { set; get; }
        // record those selected list
        private ObservableCollection<Ruban> selectedList;

        public DealTable()
        {
            InitializeComponent();
            selectedList = new ObservableCollection<Ruban>();
        }

        /// <summary>
        /// Show Deal Table with Arrangement
        /// </summary>
        /// <param name="uphi">original cards</param>
        /// <param name="rest">number of rest cards</param>
        /// <param name="put">number of put cards</param>
        /// <param name="singleLine">whether singleLine can handle it</param>
        /// <param name="cancellable"></param>
        /// <param name="keep"></param>
        internal void ShowXArrageTable(List<Ruban> uphi, int r1, int r2,
            bool cancellable, bool keep)
        {
            TableType = "XArrage" + r1 + "," + r2;
            Keep = keep;
            mainBoard.Children.Clear();
            int usz = uphi.Count;
            int idx = 0;
            foreach (Ruban ruban in uphi)
            {
                mainBoard.Children.Add(ruban);
                ruban.LengthLimit = MAX_LENGTHCNT;
                ruban.SetOfIndex(idx, 0, uphi.Count);
                ++idx;
            }
            if (r1 != r2 || r1 != uphi.Count)
            {
                mainGrid.Height = 274;
                mainGrid.Background = FindResource("dt01Bg") as ImageBrush;
                mainBoard.Height = 240;
                //sepLine.Visibility = Visibility.Visible;
            }
            else
            {
                mainGrid.Height = 166;
                mainGrid.Background = FindResource("dt02Bg") as ImageBrush;
                mainBoard.Height = 120;
                //sepLine.Visibility = Visibility.Collapsed;
            }
            okButton.Visibility = Visibility.Visible;
            closeButton.Visibility = cancellable ? Visibility.Visible : Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Show Deal Table
        /// </summary>
        /// <param name="uphi">First rows</param>
        /// <param name="dnhi">Second rows</param>
        /// <param name="r1">Needed Element left range</param>
        /// <param name="r2">Needed Element right range</param>
        /// <param name="cancelable">Whether cancellable</param>
        internal void ShowTable(List<Ruban> uphi, List<Ruban> dnhi,
            int r1, int r2, bool cancelable, bool keep)
        {
            TableType = "Table";
            Keep = keep;
            mainBoard.Children.Clear();
            selectedList = new ObservableCollection<Ruban>();
            int usz = uphi.Count;
            int idx = 0;
            foreach (Ruban ruban in uphi)
            {
                mainBoard.Children.Add(ruban);
                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (!selectedList.Contains(ruban))
                        selectedList.Add(ruban);
                };
                ruban.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
                {
                    selectedList.Remove(ruban);
                };
                ruban.LengthLimit = OI.DealTable.MAX_LENGTHCNT;
                ruban.SetOfIndex(idx, 0, uphi.Count);
                ++idx;
            }
            if (dnhi != null && dnhi.Count > 0)
            {
                mainGrid.Height = 274;
                mainGrid.Background = FindResource("dt01Bg") as ImageBrush;

                int dsz = dnhi.Count;
                //double deach = dsz <= UNFOLD_LIMIT ? HITORI_SIZE :
                //    (double)(MAX_LENGTHCNT - HITORI_SIZE) / (dsz - 1);
                idx = 0;
                foreach (Ruban ruban in dnhi)
                {
                    mainBoard.Children.Add(ruban);
                    ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                    {
                        if (!selectedList.Contains(ruban))
                            selectedList.Add(ruban);
                    };
                    ruban.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
                    {
                        selectedList.Remove(ruban);
                    };
                    ruban.LengthLimit = OI.DealTable.MAX_LENGTHCNT;
                    ruban.SetOfIndex(idx, 1, dnhi.Count);
                    ++idx;
                }
                //sepLine.Visibility = Visibility.Visible;
                mainBoard.Height = 240;
            }
            else
            {
                mainGrid.Height = 166;
                mainGrid.Background = FindResource("dt02Bg") as ImageBrush;
                mainBoard.Height = 120;
                //sepLine.Visibility = Visibility.Collapsed;
            }
            this.Cancelable = cancelable;
            selectedList.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (r1 == 1 && r2 == 1 && selectedList.Count == r1)
                {
                    if (keep)
                        LockTable();
                    else
                        FinishTable();
                    if (input != null)
                        input(string.Join(",", selectedList.Select(p => p.UT)));
                }
                else if (r1 != 1 || r2 != 1)
                {
                    if (selectedList.Count >= r1 && selectedList.Count <= r2)
                        okButton.Visibility = Visibility.Visible;
                    else
                        okButton.Visibility = Visibility.Hidden;
                }
            };
            closeButton.Visibility = cancelable ? Visibility.Visible : Visibility.Hidden;
            okButton.Visibility = r1 == 0 ? Visibility.Visible : Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }

        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            if (input != null && Cancelable)
                input("0");
        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
            {
                if (TableType == "Table")
                {
                    if (!Keep)
                        FinishTable();
                    okButton.Visibility = Visibility.Collapsed;
                    if (selectedList.Count > 0)
                        input(string.Join(",", selectedList.Select(p => p.UT)));
                    else
                        input("0");
                }
                else if (TableType.Contains("XArrage"))
                {
                    int cidx = TableType.IndexOf(',');
                    int r1 = int.Parse(TableType.Substring("XArrage".Length, cidx - "XArrage".Length));
                    int r2 = int.Parse(TableType.Substring(cidx + 1));

                    string output = "";

                    IDictionary<int, Ruban> mop = new Dictionary<int, Ruban>();
                    foreach (var elem in mainBoard.Children)
                    {
                        Ruban ruban = elem as Ruban;
                        if (ruban.Jndex == 1)
                            mop.Add(ruban.Index, ruban);
                    }
                    for (int i = 0; i < mop.Count; ++i)
                        output += "," + mop[i].UT;

                    IDictionary<int, Ruban> map = new Dictionary<int, Ruban>();
                    foreach (var elem in mainBoard.Children)
                    {
                        Ruban ruban = elem as Ruban;
                        if (ruban.Jndex == 0)
                            map.Add(ruban.Index, ruban);
                    }
                    for (int i = 0; i < map.Count; ++i)
                        output += "," + map[i].UT;
                    if (map.Count >= r1 && map.Count <= r2)
                    {
                        FinishTable();
                        input(string.IsNullOrEmpty(output) ? "0" : output.Substring(1));
                    }
                }
            }
        }

        internal void FinishTable()
        {
            //tuxBoard.Children.Clear();
            //eqBoard.Children.Clear();
            mainBoard.Children.Clear();
            Visibility = Visibility.Collapsed;
        }

        internal void LockTable()
        {
            foreach (UIElement uie in mainBoard.Children)
            {
                Ruban ruban = uie as Ruban;
                ruban.Cat = Ruban.Category.SOUND;
            }
        }

        public event Util.InputMessageHandler input;
    }
}
