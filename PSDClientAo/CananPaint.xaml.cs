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
