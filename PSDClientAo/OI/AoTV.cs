using PSD.ClientAo.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PSD.ClientAo.OI
{
    public class AoTV
    {
        //public Television TV { private set; get; }
        public AoDisplay AD { private set; get; }
        public Base.LibGroup Tuple { private set; get; }

        //public List<string> Sets { private set; get; }

        private List<Ruban> GenRubanList(IEnumerable<string> s, Television tv)
        {
            return Ruban.GenRubanList(s, tv, Tuple);
        }

        public void Show(string single, string tag) { Show(new string[] { single }, tag); }
        public void Show(IEnumerable<string> sets, string tag)
        {
            //Sets.Clear();
            //Sets.AddRange(sets);
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                Television tv = new Television(tag) { AD = AD, AoTV = this };
                tv = AD.InsTVDict(tag, tv);
                //Canvas.SetZIndex(tv, 5);
                Canvas.SetLeft(tv, 300);
                Canvas.SetTop(tv, 170);
                List<Ruban> hi = GenRubanList(sets, tv);
                foreach (Ruban ruban in hi)
                {
                    ruban.Loc = Ruban.Location.WATCH;
                    ruban.Cat = Ruban.Category.SOUND;
                }
                tv.ShowTableCard(hi);
            }));
        }
        public void ShowSelectableList(string single, string tag, string prefix)
        { ShowSelectableList(new string[] { single }, new string[] { }, tag, prefix); }
        public void ShowSelectableList(IEnumerable<string> valSets,
            IEnumerable<string> invalSets, string tag, string prefix)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                Television tv = new Television(tag) { AD = AD, AoTV = this };
                tv = AD.InsTVDict(tag, tv);
                Canvas.SetLeft(tv, 300);
                Canvas.SetTop(tv, 170);
                List<Ruban> vals = GenRubanList(valSets, tv);
                List<Ruban> ivls = GenRubanList(invalSets, tv);
                foreach (Ruban ruban in vals)
                {
                    ruban.Loc = Ruban.Location.WATCH;
                    ruban.Cat = Ruban.Category.ACTIVE;
                }
                foreach (Ruban ruban in ivls)
                {
                    ruban.Loc = Ruban.Location.WATCH;
                    ruban.Cat = Ruban.Category.LUMBERJACK;
                }
                vals.AddRange(ivls);
                if (prefix == "PT")
                    tv.ShowTableMonster(vals);
                else if (prefix == "FW")
                    tv.ShowTableRune(vals);
                else if (prefix == "YJ")
                    tv.ShowTableEscue(vals);
                else if (prefix == "I")
                    tv.ShowTableExsp(vals);
                //else if (prefix == "TX") { }
                else
                    tv.ShowTableCard(vals);
            }));
        }

        public AoTV(AoDisplay ad)
        {
            AD = ad;
            Tuple = AD.Tuple;
        }

        internal void Recycle(Television tv)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                AD.RmvTVDict(tv.TagTitle);
            }));
        }
        internal void Recycle(string title)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                AD.RmvTVDict(title);
            }));
        }
    }
}
