using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace PSD.ClientAo
{
    public class AoField : INotifyPropertyChanged
    {
        private PilesBar pb;
        public AoDisplay AD { set; get; }

        public Base.LibGroup Tuple { private set; get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #region /p zone

        private int mTuxCount;
        public int TuxCount
        {
            set
            {
                mTuxCount = value;
                if (mTuxCount < 0)
                {
                    mTuxCount += TuxDises;
                    TuxDises = 0;
                }
                NotifyPropertyChanged("TuxCount");
            }
            get { return mTuxCount; }
        }

        private int mMonCount;
        public int MonCount {
            set
            {
                mMonCount = value;
                if (mMonCount < 0)
                    mMonCount = 0;
                NotifyPropertyChanged("MonCount");
            }
            get { return mMonCount; }
        }

        private int mEveCount;
        public int EveCount
        {
            set
            {
                mEveCount = value;
                if (mEveCount < 0)
                {
                    mEveCount += EveDises;
                    EveDises = 0;
                }
                NotifyPropertyChanged("EveCount");
            }
            get { return mEveCount; }
        }

        private int mTuxDises, mEveDises, mMonDises;
        public int TuxDises
        {
            set
            {
                mTuxDises = value;
                NotifyPropertyChanged("TuxDises");
            }
            get { return mTuxDises; }
        }
        public int EveDises
        {
            set
            {
                mEveDises = value;
                NotifyPropertyChanged("EveDises");
            }
            get { return mEveDises; }
        }
        public int MonDises {
            set
            {
                mMonDises = value;
                NotifyPropertyChanged("MonDises");
            }
            get { return mMonDises; }
        }

        private int mScoreAka, mScoreAo;
        public int ScoreAka
        {
            set
            {
                mScoreAka = value;
                NotifyPropertyChanged("ScoreAka");
            }
            get { return mScoreAka; }
        }
        public int ScoreAo {
            set
            {
                mScoreAo = value;
                NotifyPropertyChanged("ScoreAo");
            }
            get { return mScoreAo; }
        }

        #endregion /p zone
        #region /f zone

        private int mPoolAka, mPoolAo;
        public int PoolAka
        {
            set
            {
                if (mPoolAka != value)
                {
                    mPoolAka = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        pb.fieldVS.Text = mPoolAka + ":" + mPoolAo;
                        if (mPoolAka == 0 && mPoolAo == 0)
                            pb.redPoolBar.Width = pb.bluePoolBar.Width = 200;
                        else
                        {
                            pb.redPoolBar.Width = ((float)mPoolAka / (mPoolAka + mPoolAo)) * 400;
                            pb.bluePoolBar.Width = 400 - pb.redPoolBar.Width;
                        }
                    }));
                }
            }
            get { return mPoolAka; }
        }
        public int PoolAo
        {
            set
            {
                if (mPoolAo != value)
                {
                    mPoolAo = value;
                    pb.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        pb.fieldVS.Text = mPoolAka + ":" + mPoolAo;
                        if (mPoolAka == 0 && mPoolAo == 0)
                            pb.redPoolBar.Width = pb.bluePoolBar.Width = 200;
                        else
                        {
                            pb.redPoolBar.Width = ((float)mPoolAka / (mPoolAka + mPoolAo)) * 400;
                            pb.bluePoolBar.Width = 400 - pb.redPoolBar.Width;
                        }
                    }));
                }
            }
            get { return mPoolAo; }
        }

        private ushort mMon1, mMon2, mEve1;
        private void SetMonster(ushort value, int rank, ushort from)
        {
            Image boardImage = (rank == 1) ? pb.cornerMon1 : pb.cornerMon2;
            Base.Card.NMB nmb = Base.Card.NMBLib.Decode(value, Tuple.ML, Tuple.NL);
            if (nmb != null)
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Image img = pb.TryFindResource("monCard" + nmb.Code) as Image;
                    if (img == null)
                        img = pb.TryFindResource("monCard000") as Image;
                    boardImage.Source = img.Source;
                    Card.Ruban ruban = new Card.Ruban(img, value);
                    if (nmb.IsMonster())
                        boardImage.ToolTip = Tips.IchiDisplay.GetMonTip(Tuple, Base.Card.NMBLib.OriginalMonster(value));
                    else if (nmb.IsNPC())
                        boardImage.ToolTip = Tips.IchiDisplay.GetNPCTip(Tuple, Base.Card.NMBLib.OriginalNPC(value));
                    if (rank == 1)
                        AD.yfOrchis40.FlashMon1(ruban, from);
                    else if (rank == 2)
                        AD.yfOrchis40.FlashMon2(ruban, from);
                }));
            }
            else
            {
                pb.Dispatcher.BeginInvoke((Action)(() =>
                {
                    boardImage.Source = (pb.TryFindResource("monCard000") as Image).Source;
                    boardImage.ToolTip = null;
                }));
            }
        }
        public ushort Monster1
        {
            set
            {
                if (mMon1 != value)
                {
                    mMon1 = value;
                    SetMonster(value, 1, Mon1From);
                }
            }
            get { return mMon1; }
        }
        public ushort Mon1From { set; get; }
        public ushort Monster2
        {
            set
            {
                if (mMon2 != value)
                {
                    mMon2 = value;
                    SetMonster(value, 2, Mon2From);
                }
            }
            get { return mMon2; }
        }
        public ushort Mon2From { set; get; }
        public ushort Eve1
        {
            set
            {
                if (mEve1 != value)
                {
                    mEve1 = value;
                    Base.Card.Evenement eve = Tuple.EL.DecodeEvenement(mEve1);
                    if (eve != null)
                    {
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Image img = pb.TryFindResource("eveCard" + eve.Code) as Image;
                            if (img == null)
                                img = pb.TryFindResource("eveCard000") as Image;
                            pb.cornerEve1.Source = img.Source;
                            Card.Suban suban = new Card.Suban(img, mMon1);
                            pb.cornerEve1.ToolTip = Tips.IchiDisplay.GetEveTip(Tuple, mEve1);
                            AD.yfOrchis40.FlashEve1(suban, Eve1From);
                        }));
                    }
                    else
                    {
                        pb.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            pb.cornerEve1.Source = (pb.TryFindResource("eveCard000") as Image).Source;
                            pb.cornerEve1.ToolTip = null;
                        }));
                    }
                }
            }
            get { return mEve1; }
        }
        public ushort Eve1From { set; get; }

        public ushort Supporter { set; get; }
        public ushort Hinder { set; get; }

        #endregion /f zone

        public AoField(PilesBar pb, Base.LibGroup libGroup)
        {
            this.pb = pb;
            // TODO: Tuple only initialize once.
            Tuple = libGroup;

            TuxCount = 0; MonCount = 0; EveCount = 0;
            TuxDises = 0; MonDises = 0; EveDises = 0;
            ScoreAka = 0; ScoreAo = 0;

            Monster1 = 0; Monster2 = 0;
            PoolAka = 0; PoolAo = 0;
            Supporter = 0; Hinder = 0;
            Eve1 = 0;
        }
    }
}
