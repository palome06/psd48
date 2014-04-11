using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PSD.ClientAo
{
    public class AoMe
    {
        private PersonalBag pb;

        public Base.LibGroup Tuple { private set; get; }

        public List<ushort> Tux { private set; get; }
        // if exceed, then unfold the card
        public const int UNFOLD_LIMIT = 5;

        public void insTux(ushort ut)
        {
            Tux.Add(ut);
            Base.Card.Tux tux = Tuple.TL.DecodeTux(ut);
            Card.Ruban ruban = null;
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (tux != null)
                {
                    Image image = pb.TryFindResource("tuxCard" + tux.Code) as Image;
                    if (image != null)
                        ruban = new Card.Ruban(image, ut);
                }
                if (ruban == null)
                    ruban = new Card.Ruban(pb.TryFindResource("tuxCard000") as Image, ut);
                ruban.ToolTip = Tips.IchiDisplay.GetTuxTip(Tuple, ut);
                ruban.Loc = Card.Ruban.Location.BAG;
                ruban.Cat = Card.Ruban.Category.ACTIVE;
                pb.InsTux(ruban);
            }));
        }

        public void delTux(ushort ut)
        {
            Tux.Remove(ut);
            pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                pb.DelTux(ut);
            }));
        }

        public void insTux(IEnumerable<ushort> uts)
        {
            foreach (ushort ut in uts)
                insTux(ut);
        }
        public void delTux(IEnumerable<ushort> uts)
        {
            foreach (ushort ut in uts)
                delTux(ut);
        }

        public void ResumeTux()
        {
            pb.Dispatcher.BeginInvoke((Action)(() => { pb.ResumeTux(); }));
        }

        public AoMe(PersonalBag pb, Base.LibGroup libGroup)
        {
            this.pb = pb;
            Tux = new List<ushort>();

            Tuple = libGroup;
        }
    }
}
