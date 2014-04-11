using PSD.ClientAo.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PSD.ClientAo.OI
{
    public class AoDeal
    {
        private DealTable dt;

        public Base.LibGroup Tuple { private set; get; }

        public List<string> Up { private set; get; }

        public List<string> Dn { private set; get; }
        // if exceed, then unfold the card
        public const int UNFOLD_LIMIT = 5;

        private List<Ruban> GenRubanList(IEnumerable<string> s)
        {
            return Ruban.GenRubanList(s, dt, Tuple);
        }

        public void Show(IEnumerable<string> ups, IEnumerable<string> dns,
            int r1, int r2, bool cancellable, bool keep)
        {
            Up.Clear(); Dn.Clear();
            Up.AddRange(ups);
            if (dns != null)
                Dn.AddRange(dns);
            
            dt.Dispatcher.BeginInvoke((Action)(() =>
            {
                List<Ruban> uphi = GenRubanList(Up);
                foreach (Ruban ruban in uphi)
                {
                    ruban.Loc = Ruban.Location.DEAL;
                    ruban.Cat = Ruban.Category.ACTIVE;
                }
                List<Ruban> dnhi = GenRubanList(Dn);
                foreach (Ruban ruban in dnhi)
                {
                    ruban.Loc = Ruban.Location.DEAL;
                    ruban.Cat = Ruban.Category.ACTIVE;
                }
                dt.ShowTable(uphi, dnhi, r1, r2, cancellable, keep);
            }));
        }

        public void ShowXArrage(IEnumerable<string> ups, int r1,
            int r2, bool cancellable, bool keep)
        {
            Up.Clear(); Dn.Clear();
            Up.AddRange(ups);
            dt.Dispatcher.BeginInvoke((Action)(() =>
            {
                List<Ruban> uphi = GenRubanList(Up);
                foreach (Ruban ruban in uphi)
                {
                    ruban.Loc = Ruban.Location.DEAL;
                    ruban.Cat = Ruban.Category.PISTON;
                }
                dt.ShowXArrageTable(uphi, r1, r2, cancellable, keep);
            }));
        }

        public void FinishTable()
        {
            dt.Dispatcher.BeginInvoke((Action)(() =>
            {
                dt.FinishTable();
            }));
        }

        public AoDeal(DealTable dt, Base.LibGroup libGroup)
        {
            this.dt = dt;

            Tuple = libGroup;
            Up = new List<string>();
            Dn = new List<string>();
        }
    }
}