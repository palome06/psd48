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
    /// Interaction logic for PersonalBag.xaml
    /// </summary>
    public partial class PersonalBag : UserControl
    {
        public Base.LibGroup Tuple {
            set
            {
                AoMe me = new AoMe(this, value); Me = me;
                DataContext = me;

                //me.insTux(33);
                ////me.insTux(34);
                ////me.insTux(35);
                //me.insTux(36);
                ////me.insTux(37);
                ////me.insTux(52);
                //me.insTux(6);
                ////me.insTux(39);
                //me.insTux(18);
            }
        }

        public AoMe Me { get; private set; }

        public AoDisplay AD { get; set; }
        // if exceed, then unfold the card
        public const int UNFOLD_LIMIT = 5;

        public const int HITORI_SIZE = 90;

        public const int MAX_LENGTHCNT = UNFOLD_LIMIT * HITORI_SIZE + 10;

        public PersonalBag()
        {
            InitializeComponent();
        }

        internal void InsTux(Card.Ruban ruban)
        {
            int target = mainCanvas.Children.Count;
            ruban.Index = target;
            if (target < UNFOLD_LIMIT)
            {
                Canvas.SetLeft(ruban, target * HITORI_SIZE);
                mainCanvas.Children.Add(ruban);
            }
            else
            {
                double each = (double)(MAX_LENGTHCNT - HITORI_SIZE) / target;
                int idx = 0;
                foreach (var child in mainCanvas.Children)
                {
                    Card.Ruban hit = child as Card.Ruban;
                    if (hit != null)
                    {
                        Canvas.SetLeft(hit, idx * each);
                        ++idx;
                    }
                }
                Canvas.SetLeft(ruban, idx * each);
                mainCanvas.Children.Add(ruban);
            }
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
        internal void DelTux(ushort ut)
        {
            Card.Ruban ruban = null;
            foreach (var child in mainCanvas.Children)
            {
                Card.Ruban hi = child as Card.Ruban;
                if (hi != null && hi.UT == ut)
                {
                    ruban = hi; break;
                }
            }
            if (ruban != null)
            {
                int rdx = ruban.Index;
                mainCanvas.Children.Remove(ruban);
                int sz = mainCanvas.Children.Count;
                if (sz <= UNFOLD_LIMIT)
                {
                    foreach (var child in mainCanvas.Children)
                    {
                        Card.Ruban hi = child as Card.Ruban;
                        if (hi.Index > ruban.Index)
                            --hi.Index;
                        Canvas.SetLeft(hi, hi.Index * HITORI_SIZE);
                    }
                }
                else
                {
                    double each = (double)(MAX_LENGTHCNT - HITORI_SIZE) / (sz - 1);
                    foreach (var child in mainCanvas.Children)
                    {
                        Card.Ruban hit = child as Card.Ruban;
                        if (hit.Index > ruban.Index)
                            --hit.Index;
                        Canvas.SetLeft(hit, hit.Index * each);
                    }
                }
            }
        }

        internal void EnableTux(IEnumerable<ushort> vset)
        {
            foreach (UIElement child in mainCanvas.Children)
            {
                Card.Ruban ruban = child as Card.Ruban;
                if (vset.Contains(ruban.UT))
                    ruban.Cat = Card.Ruban.Category.ACTIVE;
                else
                    ruban.Cat = Card.Ruban.Category.LUMBERJACK;
            }
        }
        internal void ResumeTux()
        {
            foreach (UIElement child in mainCanvas.Children)
            {
                Card.Ruban ruban = child as Card.Ruban;
                ruban.cardBody.IsChecked = false;
                ruban.Cat = Card.Ruban.Category.SOUND;
            }
        }
        internal void LockTux()
        {
            foreach (UIElement child in mainCanvas.Children)
            {
                Card.Ruban ruban = child as Card.Ruban;
                ruban.Cat = Card.Ruban.Category.SOUND;
            }
        }

        public Card.Ruban GetRuban(ushort ut)
        {
            foreach (UIElement child in mainCanvas.Children)
            {
                Card.Ruban ruban = child as Card.Ruban;
                if (ruban.UT == ut)
                    return ruban;
            }
            return null;
        }
    }
}
