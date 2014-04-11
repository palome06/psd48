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
    /// Interaction logic for RubanLock.xaml
    /// </summary>
    public partial class RubanLock : UserControl
    {
        public enum Location { NIL, WEAPON, ARMOR };

        public enum Category { NIL, ACTIVE, SOUND };

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
        public Category Cat
        {
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

        private ImageBrush mFaceBrush;
        public ImageBrush Face
        {
            set
            {
                if (mFaceBrush != value)
                {
                    mFaceBrush = value;
                    cardFace.Fill = value;
                }
            }
            get { return mFaceBrush; }
        }
        public ushort UT { set; get; }

        public RubanLock()
        {
            InitializeComponent();
            mLoc = Location.NIL; mCat = Category.NIL;
        }

        public void UpdateLocCat(Location mLoc, Category mCat)
        {
            if (mCat == Category.ACTIVE)
            {
                cardBody.Template = Resources["activeEqiup"] as ControlTemplate;
                cardBody.ApplyTemplate();
                cardBody.IsEnabled = true;
            }
            else if (mCat == Category.SOUND)
            {
                cardBody.Template = Resources["soundEqiup"] as ControlTemplate;
                cardBody.ApplyTemplate();
                cardBody.IsEnabled = false;
            }
            Border gb = cardBody.Template.FindName("goldenBorder", cardBody) as Border;
            if (gb != null)
            {
                if (mLoc == Location.WEAPON)
                    gb.Width = 88;
                else if (mLoc == Location.ARMOR)
                    gb.Width = 76;
            }
        }
    }
}
