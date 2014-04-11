using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.ClientAo.OI
{
    public class AoMinami
    {
        private NumberPad np;

        public AoMinami(NumberPad np) { this.np = np; }

        public void Show(int select, params string[] names)
        {
            np.Dispatcher.BeginInvoke((Action)(() =>
            {
                np.Show(select, names);
            }));
        }

        public void ShowWithEncoding(int select, string title,
            IDictionary<string, string> encoding)
        {
            np.Dispatcher.BeginInvoke((Action)(() =>
            {
                np.ShowWithEncoding(select, title, encoding);
            }));
        }

        public void ShowTip(string tip)
        {
            np.Dispatcher.BeginInvoke((Action)(() =>
            {
                np.ShowTip(tip);
            }));
        }

        public void HideTip()
        {
            np.Dispatcher.BeginInvoke((Action)(() =>
            {
                np.HideTip();
            }));
        }
    }
}
