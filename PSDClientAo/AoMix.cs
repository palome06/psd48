using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.ClientAo
{
    public class AoMix
    {
        public AoDisplay AD { private set; get; }

        public AoMix(AoDisplay ad) { this.AD = ad; }

        public void StartSelectTarget(List<ushort> cands, int r1, int r2)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                AD.StartSelectTarget(cands, r1, r2);
            }));
        }
        public void FinishSelectTarget()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.FinishSelectTarget(); }));
        }
        public void LockSelectTarget()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.LockSelectTarget(); }));
        }

        public void StartSelectQard(List<ushort> cands, int r1, int r2)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                AD.StartSelectQard(cands, r1, r2);
            }));
        }
        public void FinishSelectQard()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.FinishSelectQard(); }));
        }
        public void LockSelectQard()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.LockSelectQard(); }));
        }

        public void StartSelectTX(List<ushort> cands)
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.StartSelectTX(cands); }));
        }
        public void StartSelectPT(List<ushort> cands, bool self)
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.StartSelectPT(cands, self); }));
        }
        public void StartSelectExsp(List<ushort> cands)
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.StartSelectExsp(cands); }));
        }
        public void StartSelectSF(List<ushort> cands)
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.StartSelectSF(cands); }));
        }
        public void StartSelectYJ(List<ushort> cands)
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.StartSelectYJ(cands); }));
        }
        public void FinishSelectPT()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.FinishSelectPT(); }));
        }
        public void FinishSelectSF()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.FinishSelectSF(); }));
        }
        public void FinishSelectYJ()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.FinishSelectYJ(); }));
        }
        public void FinishSelectExsp()
        {
            AD.Dispatcher.BeginInvoke((Action)(() => { AD.FinishSelectExsp(); }));
        }
    }
}
