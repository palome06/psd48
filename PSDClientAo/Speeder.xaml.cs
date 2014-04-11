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
    /// Interaction logic for Speeder.xaml
    /// </summary>
    public partial class Speeder : UserControl
    {
        public Speeder()
        {
            InitializeComponent();
            AoDisplay = null;
        }

        public AoDisplay AoDisplay { set; get; }

        private void SpderPrevClick(object sender, RoutedEventArgs e)
        {
            if (AoDisplay != null)
            {
                AoDisplay.ReplayPrev();
                Magi.Text = AoDisplay.GetMagi() + "x";
            }
        }

        private void SpderPlayClick(object sender, RoutedEventArgs e)
        {
            if (AoDisplay != null)
                AoDisplay.ReplayPlay();
            PlayIcon.Visibility = Visibility.Collapsed;
            PauseIcon.Visibility = Visibility.Visible;
        }

        private void SpderPauseClick(object sender, RoutedEventArgs e)
        {
            if (AoDisplay != null)
                AoDisplay.ReplayPause();
            PlayIcon.Visibility = Visibility.Visible;
            PauseIcon.Visibility = Visibility.Collapsed;
        }

        private void SpderNextClick(object sender, RoutedEventArgs e)
        {
            if (AoDisplay != null)
            {
                AoDisplay.ReplayNext();
                Magi.Text = AoDisplay.GetMagi() + "x";
            }
        }
    }
}
