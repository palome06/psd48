using PSD.Base.Card;
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

namespace PSD.ClientAo.Card
{
    /// <summary>
    /// Interaction logic for Ruban.xaml
    /// </summary>
    public partial class Ruban : ContentControl
    {
        public enum Location { NIL, BAG, WATCH, DEAL };

        public enum Category { NIL, ACTIVE, SOUND, LUMBERJACK, PISTON, AO_MASK, AKA_MASK };

        public const int HITORI_WIDTH = 90;
        public const int HITORI_HEIGHT = 130;

        private Location mLoc;
        public Location Loc
        {
            set
            {
                if (mLoc != value)
                {
                    mLoc = value;
                    UpdateLocCat(mLoc, mCat);
                }
            }
            get { return mLoc; }
        }

        private Category mCat;
        public Category Cat {
            set
            {
                if (mCat != value)
                {
                    mCat = value;
                    UpdateLocCat(mLoc, mCat);
                }
            }
            get { return mCat; }
        }

        public Image Face { private set; get; }

        public ushort UT { private set; get; }

        public Ruban(Image face, ushort ut)
        {
            this.Face = new Image() { Source = face.Source };
            this.UT = ut;

            InitializeComponent();
            cardBody.Content = Face;
            mLoc = Location.NIL; mCat = Category.NIL;
            Index = 0; Jndex = 0;
            this.Template = Resources["RubanItemTemplate"] as ControlTemplate;
            ApplyTemplate();

            ShipRule = ShipRule.DEF_SETTING;
        }
        public Ruban Clone()
        {
            Ruban ruban = new Ruban(Face, UT);
            ruban.Loc = Loc;
            ruban.Cat = Cat;
            return ruban;
        }

        public void UpdateLocCat(Location mLoc, Category mCat)
        {
            if (mCat == Category.ACTIVE)
            {
                cardBody.IsHitTestVisible = true;
                cardBody.IsEnabled = true;
                Template = Resources["MoziItemTemplate"] as ControlTemplate;
                if (mLoc == Location.BAG)
                    cardBody.Template = Resources["ActiveCardBag"] as ControlTemplate;
                else if (mLoc == Location.DEAL)
                    cardBody.Template = Resources["ActiveCardDeal"] as ControlTemplate;
                else if (mLoc == Location.WATCH)
                    cardBody.Template = Resources["ActiveCardDeal"] as ControlTemplate;
            }
            else if (mCat == Category.SOUND)
            {
                cardBody.IsHitTestVisible = true;
                cardBody.IsEnabled = false;
                Template = Resources["MoziItemTemplate"] as ControlTemplate;
                if (mLoc == Location.BAG)
                    cardBody.Template = Resources["ActiveCardBag"] as ControlTemplate;
                else if (mLoc == Location.DEAL)
                    cardBody.Template = Resources["ActiveCardDeal"] as ControlTemplate;
                else if (mLoc == Location.WATCH)
                    cardBody.Template = Resources["SoundCardWatcher"] as ControlTemplate;
            }
            else if (mCat == Category.LUMBERJACK)
            {
                cardBody.IsHitTestVisible = false;
                cardBody.IsEnabled = false;
                Template = Resources["RubanItemTemplate"] as ControlTemplate;
                if (mLoc == Location.BAG)
                    cardBody.Template = Resources["LumberjackCardBag"] as ControlTemplate;
                else if (mLoc == Location.DEAL)
                    cardBody.Template = Resources["LumberjackCardDeal"] as ControlTemplate;
                else if (mLoc == Location.WATCH)
                {
                    Template = Resources["MoziItemTemplate"] as ControlTemplate;
                    cardBody.Template = Resources["LumberjackCardDeal"] as ControlTemplate;
                }
            }
            else if (mCat == Category.PISTON)
            {
                cardBody.IsHitTestVisible = false;
                cardBody.IsEnabled = false;
                Template = Resources["RubanItemTemplate"] as ControlTemplate;
                if (mLoc == Location.DEAL)
                    cardBody.Template = Resources["PistonCardDeal"] as ControlTemplate;
            }
            else if (mCat == Category.AO_MASK)
            {
                cardBody.IsHitTestVisible = false;
                cardBody.IsEnabled = false;
                Template = Resources["MoziItemTemplate"] as ControlTemplate;
                cardBody.Template = Resources["AoMaskCard"] as ControlTemplate;
            }
            else if (mCat == Category.AKA_MASK)
            {
                cardBody.IsHitTestVisible = false;
                cardBody.IsEnabled = false;
                Template = Resources["MoziItemTemplate"] as ControlTemplate;
                cardBody.Template = Resources["AkaMaskCard"] as ControlTemplate;
            }
            if (mLoc != Location.NIL && mCat != Category.NIL)
            {
                ApplyTemplate();
                cardBody.ApplyTemplate();
            }
        }

        public int Index { set; get; }
        public int Jndex { set; get; }

        // maximum length of Ruban in a line
        public int LengthLimit { set; get; }

        public void SetOfIndex(int index, int jndex, int totalLine)
        {
            if (index >= totalLine)
                index = totalLine - 1;
            Index = index;
            Jndex = jndex;
            Canvas.SetZIndex(this, 0);
            if (totalLine * HITORI_WIDTH < LengthLimit)
                Canvas.SetLeft(this, index * HITORI_WIDTH);
            else
            {
                double each = (double)(LengthLimit - HITORI_WIDTH) / (totalLine - 1);
                Canvas.SetLeft(this, index * each);
            }
            Canvas.SetTop(this, jndex * HITORI_HEIGHT);
        }

        //private void SetOfIndex(int index, int jndex, int total, int limit, int maxLength)
        //{
        //    if (index >= total)
        //        index = total - 1;
        //    Index = index;
        //    Jndex = jndex;
        //    if (total < limit)
        //    {
        //        Canvas.SetZIndex(this, 0);
        //        Canvas.SetLeft(this, index * PersonalBag.HITORI_SIZE);
        //        Canvas.SetTop(this, jndex * OI.DealTable.HITORI_LAYER);
        //    }
        //    else
        //    {
        //        double each = (double)(maxLength - PersonalBag.HITORI_SIZE) / (total - 1);
        //        Canvas.SetZIndex(this, 0);
        //        Canvas.SetLeft(this, index * each);
        //        Canvas.SetTop(this, jndex * OI.DealTable.HITORI_LAYER);
        //    }
        //}

        //public void SetOfIndex(int index, int jndex, int total)
        //{
        //    SetOfIndex(index, jndex, total, PersonalBag.UNFOLD_LIMIT, PersonalBag.MAX_LENGTHCNT);
        //}

        //public void SetOfIndexTable(int index, int jndex, int total)
        //{
        //    SetOfIndex(index, jndex, total, OI.DealTable.UNFOLD_LIMIT, OI.DealTable.MAX_LENGTHCNT);
        //}

        #region Move Style

        public ShipRule ShipRule { set; get; }

        private void PutBack(Canvas rubanship)
        {
            foreach (ShipRule.Zone zn in ShipRule.ZoneList)
            {
                if (Index >= zn.x1 && Index <= zn.x2 && Jndex >= zn.y1 && Jndex <= zn.y2)
                {
                    if (zn.style == ShipRule.AlignStyle.ALIGN)
                    {
                        int szt = 0;
                        foreach (var elem in rubanship.Children)
                        {
                            Ruban ru = elem as Ruban;
                            if (ru.Jndex == this.Jndex)
                                ++szt;
                        }
                        SetOfIndex(Index, Jndex, szt);
                    }
                    else if (zn.style == ShipRule.AlignStyle.STAY)
                        SetOfIndex(Index, Jndex, Index + 1);
                    return;
                }
            }
        }

        internal void OnMove(int idx, int jdx)
        {
            Canvas rubanship = VisualTreeHelper.GetParent(this) as Canvas;
            int oIdx = Index, oJdx = Jndex;
            if (idx == Index && jdx == Jndex) { PutBack(rubanship); return; }
            if (ShipRule != null)
            {
                ShipRule.Zone oldzn = null, newzn = null;
                foreach (ShipRule.Zone zn in ShipRule.ZoneList)
                {
                    if (Index >= zn.x1 && Index <= zn.x2 && Jndex >= zn.y1 && Jndex <= zn.y2)
                        oldzn = zn;
                    if (idx >= zn.x1 && idx <= zn.x2 && jdx >= zn.y1 && jdx <= zn.y2)
                        newzn = zn;
                }
                if (newzn == null) { PutBack(rubanship); return; }
                // Remove the old one
                if (oldzn.style == ShipRule.AlignStyle.ALIGN)
                {
                    int sz = 0;
                    foreach (var elem in rubanship.Children)
                    {
                        Ruban ru = elem as Ruban;
                        if (ru.Jndex == this.Jndex)
                            ++sz;
                    }
                    --sz;
                    foreach (var elem in rubanship.Children)
                    {
                        Ruban ru = elem as Ruban;
                        if (ru != this && ru.Jndex == this.Jndex && ru.Index > this.Index)
                            ru.SetOfIndex(ru.Index - 1, ru.Jndex, sz);
                    }
                }
                else if (oldzn.style == ShipRule.AlignStyle.STAY) { }

                // Insert the new one
                if (newzn.style == ShipRule.AlignStyle.ALIGN)
                {
                    int sz = 0;
                    foreach (var elem in rubanship.Children)
                    {
                        Ruban ru = elem as Ruban;
                        if (ru != this && ru.Jndex == jdx)
                            ++sz;
                    }
                    ++sz;
                    // besu round operation
                    List<Ruban> behinds = new List<Ruban>();
                    foreach (var elem in rubanship.Children)
                    {
                        Ruban ru = elem as Ruban;
                        if (ru != this && ru.Jndex == jdx && ru.Index >= idx)
                        {
                            ru.SetOfIndex(ru.Index + 1, jdx, sz);
                            behinds.Add(ru);
                        }
                    }
                    rubanship.Children.Remove(this);
                    rubanship.Children.Add(this);
                    SetOfIndex(idx, jdx, sz);
                    foreach (Ruban ru in behinds)
                        rubanship.Children.Remove(ru);
                    foreach (Ruban ru in behinds)
                        rubanship.Children.Add(ru);
                }
                else if (newzn.style == ShipRule.AlignStyle.STAY)
                    SetOfIndex(idx, jdx, idx + 1);
            }
            if (moveCaller != null)
                moveCaller(oIdx, oJdx, idx, jdx);
        }

        public event Util.RubanMoveHandler moveCaller;

        #endregion Move Style

        #region Factory Utils
        private static void Image2Gray(ref Image image, bool gray)
        {
            if (gray)
            {
                FormatConvertedBitmap bitmap = new FormatConvertedBitmap();
                bitmap.BeginInit();
                bitmap.Source = image.Source as BitmapSource;
                bitmap.DestinationFormat = PixelFormats.Gray32Float;
                bitmap.EndInit();
                // Create Image Element
                image = new Image() { Width = image.Width, Height = image.Height, Source = bitmap };
            }
        }
        public static Ruban GenRubanGray(string str, FrameworkElement uc, Base.LibGroup tuple)
        {
            return GenRuban(str, uc, tuple, true);
        }
        public static Ruban GenRuban(string str, FrameworkElement uc, Base.LibGroup tuple)
        {
            return GenRuban(str, uc, tuple, false);
        }
        private static Ruban GenRuban(string str, FrameworkElement uc, Base.LibGroup tuple, bool gray)
        {
            Ruban rb = null;
            if (str == "C0")
                rb = new Ruban(uc.TryFindResource("tuxCard000") as Image, 0);
            else if (str.StartsWith("C"))
            {
                ushort ut = ushort.Parse(str.Substring(1));
                Tux tux = tuple.TL.DecodeTux(ut);
                if (tux != null)
                {
                    Image image = uc.TryFindResource("tuxCard" + tux.Code) as Image;
                    if (image != null)
                        rb = new Ruban(image, ut);
                    else
                        rb = new Ruban(uc.TryFindResource("tuxCard000") as Image, ut);
                }
                else
                    rb = new Ruban(uc.TryFindResource("tuxCard000") as Image, ut);
                rb.ToolTip = Tips.IchiDisplay.GetTuxTip(tuple, ut);
            }
            else if (str.StartsWith("M"))
            {
                ushort ut = ushort.Parse(str.Substring(1));
                NMB nmb = NMBLib.Decode(ut, tuple.ML, tuple.NL);
                if (nmb != null)
                {
                    Image image = uc.TryFindResource("monCard" + nmb.Code) as Image;
                    Image2Gray(ref image, gray);
                    if (image != null)
                        rb = new Ruban(image, ut);
                    else
                        rb = new Ruban(uc.TryFindResource("monCard000") as Image, ut);
                    if (nmb.IsMonster())
                        rb.ToolTip = Tips.IchiDisplay.GetMonTip(tuple, NMBLib.OriginalMonster(ut));
                    else if (nmb.IsNPC())
                        rb.ToolTip = Tips.IchiDisplay.GetNPCTip(tuple, NMBLib.OriginalNPC(ut));
                }
                else
                    rb = new Ruban(uc.TryFindResource("monCard000") as Image, ut);
            }
            else if (str.StartsWith("H0"))
                rb = new Ruban(uc.TryFindResource("hroCard000") as Image, 0);
            else if (str.StartsWith("H"))
            {
                ushort ut = ushort.Parse(str.Substring("H".Length));
                Hero hro = tuple.HL.InstanceHero(ut);
                if (hro != null)
                {
                    Image image = uc.TryFindResource("hroCard" + hro.Ofcode) as Image;
                    Image2Gray(ref image, gray);
                    if (image != null)
                        rb = new Ruban(image, ut);
                    else
                        rb = new Ruban(uc.TryFindResource("hroCard000") as Image, ut);
                }
                else rb = new Ruban(uc.TryFindResource("hroCard000") as Image, ut);
                rb.ToolTip = Tips.IchiDisplay.GetHeroTip(tuple, ut);
            }
            else if (str.StartsWith("D"))
            {
                ushort ut = ushort.Parse(str.Substring("D".Length));
                Image image = uc.TryFindResource("diceImg" + ut) as Image;
                if (image != null)
                    rb = new Ruban(image, ut);
                else
                    rb = new Ruban(uc.TryFindResource("diceImg000") as Image, ut);
                rb.ToolTip = null;
            }
            else if (str.StartsWith("G"))
            {
                ushort dbSerial = ushort.Parse(str.Substring("G".Length));
                Tux tux = tuple.TL.EncodeTuxDbSerial(dbSerial);
                if (tux != null)
                {
                    Image image = uc.TryFindResource("tuxCard" + tux.Code) as Image;
                    Image2Gray(ref image, gray);
                    if (image != null)
                        rb = new Ruban(image, dbSerial);
                    else
                        rb = new Ruban(uc.TryFindResource("tuxCard000") as Image, dbSerial);
                    rb.ToolTip = Tips.IchiDisplay.GetTuxDbSerialTip(tuple, dbSerial);
                }
                else
                    rb = new Ruban(uc.TryFindResource("tuxCard000") as Image, dbSerial);
            }
            else if (str.StartsWith("I"))
            {
                ushort ut = ushort.Parse(str.Substring("I".Length));
                Image image = uc.TryFindResource("exspCard" + ut) as Image;
                if (image != null)
                    rb = new Ruban(image, ut);
                else
                    rb = new Ruban(uc.TryFindResource("diceImg000") as Image, ut);
                rb.ToolTip = Tips.IchiDisplay.GetExspTip(tuple, "I" + ut);
            }
            else if (str.StartsWith("E0"))
            {
                Image image = uc.TryFindResource("eveCard000") as Image;
                image.RenderTransform = new RotateTransform(90);
                rb = new Ruban(image, 0);
            }
            else if (str.StartsWith("E"))
            {
                ushort ut = ushort.Parse(str.Substring("E".Length));
                Evenement eve = tuple.EL.DecodeEvenement(ut);
                Image image = uc.TryFindResource("eveCard" + eve.Code) as Image
                    ?? uc.TryFindResource("eveCard000") as Image;

                FormatConvertedBitmap bp = new FormatConvertedBitmap();
                bp.BeginInit();
                bp.Source = image.Source as BitmapSource;
                bp.EndInit();

                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bp;
                tb.Transform = new RotateTransform(90);
                tb.EndInit();

                image = new Image() { Source = tb };
                Image2Gray(ref image, gray);
                rb = new Ruban(image, ut);
                rb.ToolTip = Tips.IchiDisplay.GetEveTip(tuple, ut);
            }
            else if (str.StartsWith("R"))
            {
                ushort ut = ushort.Parse(str.Substring("R".Length));
                Image image = uc.TryFindResource("runeCard" + ut) as Image;
                if (image != null)
                    rb = new Ruban(image, ut);
                else
                    rb = new Ruban(uc.TryFindResource("runeCard000") as Image, ut);
                rb.ToolTip = Tips.IchiDisplay.GetRuneTip(tuple, ut);
            }
            else if (str.StartsWith("V"))
            {
                ushort ut = ushort.Parse(str.Substring("V".Length));
                Image image = uc.TryFindResource("fiveImg" + ut) as Image;
                if (image != null)
                    rb = new Ruban(image, ut);
                else
                    rb = new Ruban(uc.TryFindResource("diceImg000") as Image, ut);
                rb.ToolTip = Tips.IchiDisplay.GetFiveTip(tuple, ut);
            }
            return rb;
        }

        public static List<Ruban> GenRubanList(IEnumerable<string> s, FrameworkElement uc, Base.LibGroup tuple)
        {
            List<Ruban> hi = new List<Ruban>();
            if (s == null || s.Count() == 0)
                return hi;            
            foreach (string str in s)
            {
                Ruban ru = GenRuban(str, uc, tuple);
                if (ru != null)
                    hi.Add(ru);
            }
            return hi;
        }
        #endregion Factory Utils
    }
}
