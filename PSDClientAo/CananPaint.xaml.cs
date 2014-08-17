using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for CananPaint.xaml
    /// </summary>
    public partial class CananPaint : UserControl
    {
        public CananPaint()
        {
            InitializeComponent();
        }

        public enum CananSignal
        {
            NORMAL = 0x0,
            ISWIN = 0x1, ISLOSE = 0x2,

            FAIL_CONNECTION = 0x10,
            LOSE_CONNECTION = 0x11,
            LOSE_COUNTDOWN_48 = 0x12, LOSE_COUNTDOWN_12 = 0x13,

            CRASH = 0x12,
        }

        internal void SetCanan(CananSignal signal)
        {
            if (signal == CananSignal.NORMAL)
            {
                Dispatcher.BeginInvoke((Action)(() => this.Visibility = Visibility.Collapsed));
            }
            else if (signal == CananSignal.ISWIN || signal == CananSignal.ISLOSE)
            {
                new Thread(delegate()
                {
                    Thread.Sleep(1100);
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (signal == CananSignal.ISWIN)
                        {
                            mainImg.Source = TryFindResource("cananWinGamepaint") as ImageSource;
                            this.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            mainImg.Source = TryFindResource("cananLoseGamepaint") as ImageSource;
                            this.Visibility = Visibility.Visible;
                        }
                    }));
                }).Start();
            }
            else
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    switch (signal)
                    {
                        case CananSignal.FAIL_CONNECTION:
                            mainImg.Source = TryFindResource("cananFatalpaint") as ImageSource; break;
                        case CananSignal.LOSE_CONNECTION:
                            mainImg.Source = TryFindResource("cananLosepaint") as ImageSource; break;
                        case CananSignal.LOSE_COUNTDOWN_48:
                            mainImg.Source = TryFindResource("cananCountdown48paint") as ImageSource; break;
                        case CananSignal.LOSE_COUNTDOWN_12:
                            mainImg.Source = TryFindResource("cananCountdown12paint") as ImageSource; break;
                    }
                    this.Visibility = Visibility.Visible;
                }));
            }
        }

        internal void SetCanan(bool isWin, bool loseConnection)
        {
            if (loseConnection)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this.Visibility != Visibility.Visible)
                    {
                        mainImg.Source = TryFindResource("canan03paints") as ImageSource;
                        this.Visibility = Visibility.Visible;
                    }
                }));
            }
            else if (this.Visibility != Visibility.Visible)
            {
                new Thread(delegate()
                {
                    Thread.Sleep(1100);
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (isWin)
                        {
                            mainImg.Source = TryFindResource("canan01paints") as ImageSource;
                            this.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            mainImg.Source = TryFindResource("canan02paints") as ImageSource;
                            this.Visibility = Visibility.Visible;
                        }
                    }));
                }).Start();
            }
        }
    }
}
