using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PSD.ClientAo.Voice
{
    /// <summary>
    /// Interaction logic for Soundtracker.xaml
    /// </summary>
    public partial class Soundtracker : UserControl
    {
        public AoVoice AV { private set; get; }

        public Soundtracker()
        {
            InitializeComponent();

            Process[] pname = Process.GetProcessesByName("PSDClientAo");
            bool hasOther = pname.Length > 1;
            if (hasOther)
            {
                iconPlayButton.Visibility = Visibility.Collapsed;
                iconMuteButton.Visibility = Visibility.Visible;
            }
            AV = new AoVoice(hasOther);
            AV.Init();
        }

        public void Mute()
        {
            iconMuteButton.Visibility = Visibility.Visible;
            iconPlayButton.Visibility = Visibility.Collapsed;
            // mute
            if (AV != null)
                AV.Mute();
        }

        public void Play()
        {
            iconMuteButton.Visibility = Visibility.Collapsed;
            iconPlayButton.Visibility = Visibility.Visible;
            // play
            if (AV != null)
                AV.Resume();
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
