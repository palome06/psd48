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

namespace PSD.ClientAo.OI
{
    /// <summary>
    /// Interaction logic for NumberPad.xaml
    /// </summary>
    public partial class NumberPad : UserControl
    {
        public AoMinami Minami { private set; get; }

        public NumberPad()
        {
            InitializeComponent();
            mSelection = 0;
            kyButtons = new Button[5];
            NeedSelect = true;
            encoding = null;
            Minami = new AoMinami(this);
        }

        private int mSelection;
        private bool mNeedSelect;
        private Button[] kyButtons;

        private IDictionary<string, string> encoding;

        public bool NeedSelect
        {
            set
            {
                if (mNeedSelect != value)
                {
                    mNeedSelect = value;
                    if (mNeedSelect)
                    {
                        opPanel0.Visibility = Visibility.Visible;
                        opPanel1.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        opPanel0.Visibility = Visibility.Hidden;
                        opPanel1.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public int CountOfSelection
        {
            set
            {
                if (mSelection != value)
                {
                    mSelection = value;
                    switch (mSelection)
                    {
                        case 1: opButton1.Visibility = Visibility.Collapsed;
                            opButton2.Visibility = Visibility.Collapsed;
                            opButton3.Visibility = Visibility.Visible;
                            opButton4.Visibility = Visibility.Collapsed;
                            opButton5.Visibility = Visibility.Collapsed;
                            kyButtons[0] = opButton3; break;
                        case 2: opButton1.Visibility = Visibility.Collapsed;
                            opButton2.Visibility = Visibility.Visible;
                            opButton3.Visibility = Visibility.Collapsed;
                            opButton4.Visibility = Visibility.Visible;
                            opButton5.Visibility = Visibility.Collapsed;
                            kyButtons[0] = opButton2; kyButtons[1] = opButton4; break;
                        case 3: opButton1.Visibility = Visibility.Visible;
                            opButton2.Visibility = Visibility.Collapsed;
                            opButton3.Visibility = Visibility.Visible;
                            opButton4.Visibility = Visibility.Collapsed;
                            opButton5.Visibility = Visibility.Visible;
                            kyButtons[0] = opButton1; kyButtons[1] = opButton3;
                            kyButtons[2] = opButton5; break;
                        case 4: opButton1.Visibility = Visibility.Visible;
                            opButton2.Visibility = Visibility.Visible;
                            opButton3.Visibility = Visibility.Collapsed;
                            opButton4.Visibility = Visibility.Visible;
                            opButton5.Visibility = Visibility.Visible;
                            kyButtons[0] = opButton1; kyButtons[1] = opButton2;
                            kyButtons[2] = opButton4; kyButtons[3] = opButton5; break;
                        case 5: opButton1.Visibility = Visibility.Visible;
                            opButton2.Visibility = Visibility.Visible;
                            opButton3.Visibility = Visibility.Visible;
                            opButton4.Visibility = Visibility.Visible;
                            opButton5.Visibility = Visibility.Visible;
                            kyButtons[0] = opButton1; kyButtons[1] = opButton2;
                            kyButtons[2] = opButton3; kyButtons[3] = opButton4;
                            kyButtons[4] = opButton5; break;
                    }
                }
            }
            get { return mSelection; }
        }

        private void OpButton1Click(object sender, RoutedEventArgs e)
        {
            SendResult(1);
        }
        private void OpButton2Click(object sender, RoutedEventArgs e)
        {
            if (mSelection == 2)
                SendResult(1);
            else if (mSelection == 4 || mSelection == 5)
                SendResult(2);
        }
        private void OpButton3Click(object sender, RoutedEventArgs e)
        {
            if (mSelection == 1)
                SendResult(1);
            else if (mSelection == 3)
                SendResult(2);
            else if (mSelection == 5)
                SendResult(3);
        }
        private void OpButton4Click(object sender, RoutedEventArgs e)
        {
            if (mSelection == 2)
                SendResult(2);
            else if (mSelection == 4)
                SendResult(3);
            else if (mSelection == 5)
                SendResult(4);
        }
        private void OpButton5Click(object sender, RoutedEventArgs e)
        {
            if (mSelection == 3)
                SendResult(3);
            else if (mSelection == 4)
                SendResult(4);
            else if (mSelection == 5)
                SendResult(5);
        }
        private void SendResult(int value)
        {
            string valueString = value.ToString();
            this.Visibility = Visibility.Hidden;
            if (input != null)
            {
                if (encoding != null && value <= encoding.Count)
                {
                    int idx = 1; bool found = false;
                    foreach (string name in encoding.Keys)
                    {
                        if (idx == value)
                        {
                            found = true;
                            input(encoding[name]);
                            break;
                        }
                        else
                            ++idx;
                    }
                    if (!found)
                        input(value.ToString());
                }
                else
                    input(value.ToString());
            }
        }

        internal void Show(int select, params string[] names)
        {
            this.encoding = null;
            NeedSelect = true;
            CountOfSelection = select;
            opTitle.Text = names[0];
            for (int i = 1; i < names.Length; ++i)
                kyButtons[i - 1].Content = names[i];
            Visibility = Visibility.Visible;
        }

        internal void ShowWithEncoding(int select, string title,
            IDictionary<string, string> encoding)
        {
            this.encoding = encoding;
            NeedSelect = true;
            CountOfSelection = select;
            opTitle.Text = title;
            int idx = 0;
            foreach (string name in encoding.Keys)
                kyButtons[idx++].Content = name;
            Visibility = Visibility.Visible;
        }

        internal void ShowTip(string tip)
        {
            NeedSelect = false;
            opTitleOnly.Text = tip;
            Visibility = Visibility.Visible;
        }

        internal void HideTip()
        {
            Visibility = Visibility.Hidden;
        }
        // Define an Event based on the above Delegate
        public event Util.InputMessageHandler input;
    }
}
