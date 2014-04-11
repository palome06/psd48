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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for Moonlight.xaml
    /// </summary>
    public partial class Moonlight : UserControl
    {
        private Storyboard sb;

        public Moonlight()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            InitStoryBoard();
        }

        private void InitStoryBoard()
        {
            DoubleAnimation aniG = new DoubleAnimation() { From = 200, To = 0, Duration = HALFMIN };
            aniG.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(aniG, goldenRec);
            Storyboard.SetTargetProperty(aniG, new PropertyPath(Rectangle.WidthProperty));

            DoubleAnimation aniW = new DoubleAnimation() { From = 0, To = 200, Duration = HALFMIN };
            aniW.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(aniW, whiteRec);
            Storyboard.SetTargetProperty(aniW, new PropertyPath(Rectangle.WidthProperty));

            sb = new Storyboard();
            sb.Children.Add(aniG);
            sb.Children.Add(aniW);
        }

        public readonly TimeSpan HALFMIN = TimeSpan.FromSeconds(30);

        public void Begin() { this.Visibility = Visibility.Visible; sb.Begin(); }
        public void Stop() { this.Visibility = Visibility.Hidden; sb.Stop(); }

        public void ABegin() { Dispatcher.BeginInvoke((Action)(() => { Begin(); })); }
        public void AStop() { Dispatcher.BeginInvoke((Action)(() => { Stop(); })); }
    }
}
