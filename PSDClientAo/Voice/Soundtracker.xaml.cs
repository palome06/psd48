using System.Windows;
using System.Windows.Controls;

namespace PSD.ClientAo.Voice
{
    /// <summary>
    /// Interaction logic for Soundtracker.xaml
    /// </summary>
    public partial class Soundtracker : UserControl
    {
        public Soundtracker()
        {
            InitializeComponent();
        }

        public void Mute()
        {
            iconMuteButton.Visibility = Visibility.Visible;
            iconPlayButton.Visibility = Visibility.Collapsed;
            // mute
        }

        public void Play()
        {
            iconMuteButton.Visibility = Visibility.Collapsed;
            iconPlayButton.Visibility = Visibility.Visible;
            // play
        }

        private void iconPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Mute();
        }

        private void iconMuteButton_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }
    }
}
