using PSD.ClientAo.Card;
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
    /// Interaction logic for Television.xaml
    /// </summary>
    public partial class Television : ContentControl
    {
        internal AoTV AoTV { set; get; }

        public string TagTitle { private set; get; }

        internal AoDisplay AD { set; get; }

        public Television(string tag)
        {
            InitializeComponent();
            this.Template = Resources["TVItemTemplate"] as ControlTemplate;
            this.ApplyTemplate();
            this.TagTitle = tag;
        }

        public Ruban GetRuban(ushort ut)
        {
            foreach (UIElement child in mainBoard.Children)
            {
                Card.Ruban ruban = child as Card.Ruban;
                if (ruban.UT == ut)
                    return ruban;
            }
            return null;
        }

        internal void ShowTable(List<Ruban> hi)
        {
            mainBoard.Children.Clear();
            int usz = hi.Count;
            int idx = 0;
            foreach (Ruban ruban in hi)
            {
                mainBoard.Children.Add(ruban);
                ruban.LengthLimit = OI.DealTable.MAX_LENGTHCNT;
                ruban.SetOfIndex(idx, 0, hi.Count);
                ++idx;

                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (AD != null)
                        AD.InsSelectedCard(ruban.UT);
                };
                ruban.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
                {
                    if (AD != null)
                        AD.DelSelectedCard(ruban.UT);
                };
            }
            this.Visibility = Visibility.Visible;
        }

        internal void ShowTableMonster(List<Ruban> hi)
        {
            mainBoard.Children.Clear();
            int usz = hi.Count;
            int idx = 0;
            foreach (Ruban ruban in hi)
            {
                mainBoard.Children.Add(ruban);
                ruban.LengthLimit = OI.DealTable.MAX_LENGTHCNT;
                ruban.SetOfIndex(idx, 0, hi.Count);
                ++idx;

                ruban.cardBody.Checked += delegate(object sender, RoutedEventArgs e)
                {
                    if (AD != null)
                        AD.InsSelectedMon(ruban.UT);
                };
                ruban.cardBody.Unchecked += delegate(object sender, RoutedEventArgs e)
                {
                    if (AD != null)
                        AD.DelSelectedMon(ruban.UT);
                };
            }
            this.Visibility = Visibility.Visible;
        }

        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            if (AoTV != null)
                AoTV.Recycle(this);
            this.Visibility = Visibility.Collapsed;
        }

        internal void LockRuban()
        {
            foreach (UIElement uiem in mainBoard.Children)
            {
                Ruban ruban = uiem as Ruban;
                if (ruban != null)
                    ruban.Cat = Ruban.Category.SOUND;
            }
        }
    }
}
