using PSD.ClientAo.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PSD.ClientAo.OI
{
    // decided only AoArena
    public class AoArena
    {
        private Arena ar;

        internal Base.LibGroup Tuple { private set; get; }

        public ushort Rank { get { return ar.Rank; } }

        internal Base.Rules.Casting Casting { set; get; }

        public AoArena(Arena ar, Base.LibGroup tuple)
        {
            this.ar = ar; this.Tuple = tuple;
            Casting = null;
        }
        #region Utils
        // Generate Ruban List for a single line of heroes
        protected List<Ruban> GenRubanList(List<int> h)
        {
            List<Ruban> hi = new List<Ruban>();
            if (h == null || h.Count == 0)
                return hi;
            foreach (int hr in h.ToList())
                hi.Add(GenRuban(hr));
            return hi;
        }
        protected Ruban GenRuban(int hr)
        {
            Ruban ruban = null;
            if (hr != 0)
            {
                Base.Card.Hero hro = Tuple.HL.InstanceHero(hr);
                if (hro != null)
                {
                    Image image = ar.TryFindResource("hroCard" + hro.Ofcode) as Image;
                    if (image != null)
                        ruban = new Ruban(image, (ushort)hr);
                }
            }
            if (ruban == null)
                ruban = new Ruban(ar.TryFindResource("hroCard000") as Image, 0);
            ruban.ToolTip = Tips.IchiDisplay.GetHeroTip(Tuple, hr);
            ruban.Loc = Ruban.Location.DEAL;
            return ruban;
        }
        #endregion Utils
        // General Show
        internal void Show()
        {
            if (Casting is Base.Rules.CastingPick)
            {
                var cp = Casting as Base.Rules.CastingPick;
                if (cp.Xuan.ContainsKey(Rank) && cp.Xuan[Rank].Count > 0)
                    ar.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        List<Ruban> x = GenRubanList(cp.Xuan[Rank]);
                        List<Ruban> h = GenRubanList(cp.Huan.ContainsKey(Rank) ? cp.Huan[Rank] : null);
                        x.AddRange(h);
                        ar.CstPick(x);
                    }));
            }
            else if (Casting is Base.Rules.CastingTable)
            {
                var ct = Casting as Base.Rules.CastingTable;
                ar.Dispatcher.BeginInvoke((Action)(() =>
                {
                    List<Ruban> x = GenRubanList(ct.Xuan);
                    List<Ruban> baka = GenRubanList(ct.BanAka);
                    List<Ruban> bo = GenRubanList(ct.BanAo);
                    ar.CstTable(x, baka, bo);
                    //foreach (var pair in ct.Ding)
                    //{
                    //    if (pair.Value != 0)
                    //        ar.Decide(pair.Key, pair.Value);
                    //}
                }));
            }
            else if (Casting is Base.Rules.CastingPublic)
            {
                var cp = Casting as Base.Rules.CastingPublic;
                ar.Dispatcher.BeginInvoke((Action)(() =>
                {
                    List<Ruban> x = GenRubanList(cp.Xuan);
                    List<Ruban> baka = GenRubanList(cp.BanAka);
                    List<Ruban> bo = GenRubanList(cp.BanAo);
                    List<Ruban> paka = GenRubanList(cp.DingAka);
                    List<Ruban> pao = GenRubanList(cp.DingAo);
                    ar.CstPublic(x, baka, bo, paka, pao);
                }));
            }
            else if (Casting is Base.Rules.CastingCongress)
            {
                var cc = Casting as Base.Rules.CastingCongress;
                ar.Dispatcher.BeginInvoke((Action)(() =>
                {
                    bool isAo = (Rank > 0 && Rank < 1000 && Rank % 2 == 0);
                    bool isAka = (Rank > 0 && Rank < 1000 && Rank % 2 == 1);
                    if (isAo)
                    {
                        if (!cc.DecidedAo)
                        {
                            List<Ruban> xme = GenRubanList(cc.XuanAo);
                            List<Ruban> xop = GenRubanList(cc.XuanAka);
                            Ruban me = cc.Ding[Rank] != 0 ? GenRuban(cc.Ding[Rank]) : null;
                            int ru = Rank + 2; if (ru > 6) ru -= 6;
                            int lu = Rank - 2; if (lu <= 0) lu += 6;
                            Ruban right = cc.Ding[(ushort)ru] != 0 ? GenRuban(cc.Ding[(ushort)ru]) : null;
                            Ruban left = cc.Ding[(ushort)lu] != 0 ? GenRuban(cc.Ding[(ushort)lu]) : null;
                            if (!cc.CaptainMode)
                                ar.CstCongress(xme, xop, me, right, left, false, true);
                            else
                            {
                                if (cc.IsCaptain(Rank))
                                    ar.CstCongress(xme, xop, me, right, left, true, true);
                                else
                                    ar.CstCongress(xme, xop, me, right, left, true, false);
                            }
                        }
                    }
                    else
                    {
                        if (!isAka)
                        {
                            List<Ruban> xme = GenRubanList(cc.XuanAka);
                            List<Ruban> xop = GenRubanList(cc.XuanAo);
                            Ruban me = cc.Ding[Rank] != 0 ? GenRuban(cc.Ding[Rank]) : null;
                            int ru = Rank + 2; if (ru > 6) ru -= 6;
                            int lu = Rank - 2; if (lu <= 0) lu += 6;
                            Ruban right = cc.Ding[(ushort)ru] != 0 ? GenRuban(cc.Ding[(ushort)ru]) : null;
                            Ruban left = cc.Ding[(ushort)lu] != 0 ? GenRuban(cc.Ding[(ushort)lu]) : null;
                            foreach (Ruban r in new Ruban[] { me, right, left })
                                if (r != null)
                                    xme.Add(r);
                            xme.Shuffle(); xop.Shuffle();
                            ar.CstCongress(xme, xop, null, null, null, false, false);
                        }
                        else if (!cc.DecidedAka)
                        {
                            List<Ruban> xme = GenRubanList(cc.XuanAka);
                            List<Ruban> xop = GenRubanList(cc.XuanAo);
                            Ruban me = cc.Ding[Rank] != 0 ? GenRuban(cc.Ding[Rank]) : null;
                            int ru = Rank + 2; if (ru > 6) ru -= 6;
                            int lu = Rank - 2; if (lu <= 0) lu += 6;
                            Ruban right = cc.Ding[(ushort)ru] != 0 ? GenRuban(cc.Ding[(ushort)ru]) : null;
                            Ruban left = cc.Ding[(ushort)lu] != 0 ? GenRuban(cc.Ding[(ushort)lu]) : null;
                            if (!cc.CaptainMode)
                                ar.CstCongress(xme, xop, me, right, left, false, true);
                            else
                            {
                                if (cc.IsCaptain(Rank))
                                    ar.CstCongress(xme, xop, me, right, left, true, true);
                                else
                                    ar.CstCongress(xme, xop, me, right, left, true, false);
                            }
                        }
                    }
                }));
            }
        }
        // Active selection, mark as active
        internal void Active(int[] jdxs)
        {
            if (Casting is Base.Rules.CastingTable || Casting is Base.Rules.CastingPublic)
            {
                ar.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ar.ActiveArena(jdxs);
                }));
            }
        }

        internal void Disactive(int[] jdxs)
        {
            if (Casting is Base.Rules.CastingTable || Casting is Base.Rules.CastingPublic)
            {
                ar.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ar.DisactiveArena(jdxs);
                }));
            }
        }
        // Switch Specific Ruban card
        internal void Switch(int from, int to)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                Ruban ruban = GenRuban(to);
                ar.Switch(from, ruban);
            }));
        }
        // Remove Ruban from the arena, e.g. is picked
        internal void Remove(int heroCode)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                ar.Remove(heroCode);
            }));
        }

        internal void BanBy(ushort puid, int heroCode)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                bool isAka = puid % 2 == 1;
                ar.BanBy(isAka, heroCode);
            }));
        }
        internal void PuckBack(int heroCode)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                Ruban ruban = GenRuban(heroCode);
                ar.PuckBack(ruban);
            }));
        }
        internal void PickBy(ushort puid, int selAva)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                bool isAka = puid % 2 == 1;
                ar.PickBy(puid, selAva);
            }));
        }
        public void CongressDing(ushort puid, int heroCode, bool captain)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                ar.CongressDing(puid, heroCode, captain);
            }));
        }
        public void CongressBack(int heroCode)
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                ar.CongressBack(heroCode);
            }));
        }

        internal void Shutdown()
        {
            ar.Dispatcher.BeginInvoke((Action)(() =>
            {
                ar.FinishArena();
            }));
        }
    }
}