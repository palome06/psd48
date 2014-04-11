using PSD.ClientAo.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PSD.ClientAo
{
    public class AoOrchis
    {
        public List<ushort> Tux { private set; get; }

        public List<ushort> Nmb { private set; get; }

        public List<ushort> Eve { private set; get; }

        private Orchis40 orchis40;

        public AoDisplay AD { get; set; }

        public Base.LibGroup Tuple { private set; get; }
        // if exceed, then unfold the card
        public const int UNFOLD_LIMIT = 5;

        //public void InsTux(ushort ut)
        //{
        //    Base.Card.Tux tux = Tuple.TL.DecodeTux(ut);
        //    Card.Mystic mystic;
        //    if (tux != null)
        //    {
        //        Image image = orchis.Resources["tuxCard" + tux.Code] as Image;
        //        if (image != null)
        //            mystic = new Card.Mystic(image, ut);
        //        else
        //            mystic = new Card.Mystic(orchis.Resources["tuxCard000"] as Image, ut);
        //    } else
        //        mystic = new Card.Mystic(orchis.Resources["tuxCard000"] as Image, ut);
        //    mystic.IsEnabled = false;
        //    orchis.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        orchis.InsTux(mystic);
        //    }));
        //}
        //public void ClearTux() {
        //    orchis.ClearTux();
        //}

        //public void InsNmb(ushort ut)
        //{
        //    Base.Card.NMB nmb = Base.Card.NMBLib.Decode(ut, Tuple.ML, Tuple.NL);
        //    Card.Mystic mystic;
        //    if (nmb != null)
        //    {
        //        Image image = orchis.Resources["monCard" + nmb.Code] as Image;
        //        if (image != null)
        //            mystic = new Card.Mystic(image, ut);
        //        else
        //            mystic = new Card.Mystic(orchis.Resources["monCard000"] as Image, ut);
        //    }
        //    else
        //        mystic = new Card.Mystic(orchis.Resources["monCard000"] as Image, ut);
        //    orchis.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        orchis.InsNmb(mystic);
        //    }));
        //}
        //public void ClearNmb()
        //{
        //    orchis.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        orchis.ClearNmb();
        //    }));
        //}

        //public void InsEve(ushort ut)
        //{
        //    Base.Card.Evenement eve = Tuple.EL.DecodeEvenement(ut);
        //    Card.MysticV mystic;
        //    if (eve != null)
        //    {
        //        Image image = orchis.Resources["eveCard" + eve.Code] as Image;
        //        if (image != null)
        //            mystic = new Card.MysticV(image, ut);
        //        else
        //            mystic = new Card.MysticV(orchis.Resources["eveCard000"] as Image, ut);
        //    }
        //    else
        //        mystic = new Card.MysticV(orchis.Resources["eveCard000"] as Image, ut);
        //    orchis.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        orchis.InsEve(mystic);
        //    }));
        //}
        //public void ClearEve()
        //{
        //    orchis.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        orchis.ClearEve();
        //    }));
        //}

        public AoOrchis(Orchis40 orchis, Base.LibGroup libGroup)
        {
            this.orchis40 = orchis;
            Tux = new List<ushort>();

            Tuple = libGroup;
        }

        #region Orchis40

        public void FlyingGet(string card, ushort from, ushort to, bool isLong = false)
        {
            List<string> l = new List<string>(); l.Add(card);
            FlyingGet(l, from, to, isLong);
        }
        public void FlyingGet(List<string> cards, ushort from, ushort to, bool isLong = false)
        {
            if (orchis40 == null)
                return;
            ushort sfrom = AD.Player2Position(from);
            ushort sto = AD.Player2Position(to);
            orchis40.Dispatcher.BeginInvoke((Action)(() =>
            {
                List<Ruban> hi = Ruban.GenRubanList(cards, orchis40, Tuple);
                foreach (Ruban ruban in hi)
                {
                    ruban.Cat = Ruban.Category.SOUND;
                    ruban.Loc = Ruban.Location.DEAL;
                }
                orchis40.ShowFlyingGet(hi, sfrom, sto, isLong);
            }));
        }

        #endregion Orchis40
    }
}
