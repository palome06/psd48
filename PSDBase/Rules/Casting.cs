using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.Base.Rules
{
    public abstract class Casting
    {
        public const int playerCapacity = 6;
        //public abstract string SentCurrent();
    }

    public class CastingPick : Casting
    {
        // Candidates
        public IDictionary<ushort, List<int>> Xuan { private set; get; }
        public IDictionary<ushort, List<int>> Huan { private set; get; }
        // Selected Character
        public IDictionary<ushort, int> Ding { private set; get; }

        public CastingPick()
        {
            Xuan = new Dictionary<ushort, List<int>>();
            Huan = new Dictionary<ushort, List<int>>();
            Ding = new Dictionary<ushort, int>();
            for (ushort i = 1; i <= playerCapacity; ++i)
                Ding[i] = 0;
        }

        public void Init(ushort ut, List<int> xuan, List<int> huan = null)
        {
            List<int> nxuan = new List<int>(); nxuan.AddRange(xuan);
            Xuan[ut] = nxuan;
            List<int> nhuan = new List<int>();
            if (huan != null)
            {
                nhuan.AddRange(huan);
                Huan[ut] = nhuan;
            }
            //Ding[ut] = ding;
        }

        public bool Pick(ushort ut, int which)
        {
            if (Xuan.ContainsKey(ut) && Xuan[ut].Contains(which))
            {
                Xuan[ut].Remove(which); Ding[ut] = which; return true;
            }
            else if (!Xuan.ContainsKey(ut))
            {
                Ding[ut] = which; return true;
            }
            else return false;
        }

        public int Switch(ushort ut, int which)
        {
            if (Xuan.ContainsKey(ut) && Huan.ContainsKey(ut))
            {
                if (Xuan[ut].Contains(which) && Huan[ut].Count > 0)
                {
                    int value = Huan[ut].First();
                    Huan[ut].RemoveAt(0);
                    Xuan[ut].Remove(which); Xuan[ut].Add(value);
                    return value;
                }
            }
            return 0;
        }
        public int SwitchAt(ushort ut, int idx)
        {
            if (Xuan.ContainsKey(ut) && Huan.ContainsKey(ut))
            {
                if (idx <= Xuan[ut].Count && Huan[ut].Count > 0)
                {
                    int value = Huan[ut].First();
                    Huan[ut].RemoveAt(0);
                    Xuan[ut][idx] = value;
                    return value;
                }
            }
            return 0;
        }
        public int SwitchTo(ushort ut, int which, int to)
        {
            if (Xuan.ContainsKey(ut) && Huan.ContainsKey(ut))
            {
                if (Xuan[ut].Contains(which) && Huan[ut].Count > 0)
                {
                    Huan[ut].RemoveAt(0);
                    Xuan[ut].Remove(which); Xuan[ut].Add(to);
                    return to;
                }
            }
            return 0;
        }
        public string ToMessage(ushort ut)
        {
            return string.Join(",", Xuan[ut]) +
                (Huan.ContainsKey(ut) && Huan[ut].Count > 0 ? ",0" : "");
        }
    }

    public class CastingTable : Casting
    {
        public List<int> Xuan { private set; get; }
        public List<int> BanAka { private set; get; }
        public List<int> BanAo { private set; get; }
        public IDictionary<ushort, int> Ding { private set; get; }

        public CastingTable(List<int> xuan, List<int> bAka = null, List<int> bAo = null)
        {
            Xuan = new List<int>(); Xuan.AddRange(xuan);
            BanAka = new List<int>();
            if (bAka != null) { BanAka.AddRange(bAka); }
            BanAo = new List<int>();
            if (bAo != null) { BanAo.AddRange(bAo); }
            Ding = new Dictionary<ushort, int>();
            for (ushort i = 1; i <= playerCapacity; ++i)
                Ding[i] = 0;
        }
        //public void Init(ushort ut, int ding)
        //{
        //    Ding[ut] = ding;
        //}
        public bool Pick(ushort ut, int which)
        {
            if (Xuan.Contains(which))
            {
                Xuan.Remove(which); Ding[ut] = which; return true;
            }
            else return false;
        }
        public bool Ban(ushort ut, int which)
        {
            bool aka = (ut % 2 == 1);
            if (Xuan.Contains(which))
            {
                Xuan.Remove(which);
                (aka ? BanAka : BanAo).Add(which); return true;
            }
            else
                return false;
        }
        public bool PutBack(int which)
        {
            if (Xuan.Contains(which))
                return false;
            Xuan.Add(which);
            return true;
        }
        public string ToMessage()
        {
            List<int>[] lists = new List<int>[] { Xuan, BanAka, BanAo };
            List<string> answers = new List<string>();
            foreach (List<int> list in lists)
            {
                if (list.Count > 0)
                    answers.Add(list.Count + "," + string.Join(",", list));
                else
                    answers.Add("0");
            }
            return string.Join(",", answers);
        }
    }

    public class CastingPublic : Casting
    {
        // Heroes that is to be selected
        public List<int> Xuan { private set; get; }
        // Heroes selected by Rounder or Opponent
        public List<int> DingAka { private set; get; }
        public List<int> DingAo { private set; get; }
        // Banned or Decided Picked, Shown in Xuan in different style
        public List<int> BanAka { private set; get; }
        public List<int> BanAo { private set; get; }
        // Silenced Random Characters
        public List<int> Secrets { private set; get; }
        public int SilencedIdx { private set; get; }

        public CastingPublic(List<int> xuan, List<int> pAka = null, List<int> pAo = null,
            List<int> bAka = null, List<int> bAo = null,
            List<int> secrets = null, int silencedIdx = 0)
        {
            Xuan = new List<int>(); Xuan.AddRange(xuan);
            DingAka = new List<int>(); if (pAka != null) DingAka.AddRange(pAka);
            DingAo = new List<int>(); if (pAo != null) DingAo.AddRange(pAo);
            BanAka = new List<int>(); if (bAka != null) BanAka.AddRange(bAka);
            BanAo = new List<int>(); if (bAo != null) BanAo.AddRange(bAo);
            Secrets = new List<int>(); if (secrets != null) Secrets.AddRange(secrets);
            SilencedIdx = silencedIdx;
        }
        public bool Ban(bool aka, int which)
        {
            if (Xuan.Contains(which))
            {
                Xuan.Remove(which);
                (aka ? BanAka : BanAo).Add(which);
                return true;
            }
            else return false;
        }
        // Used only in Server side, which = 0 is the secrets one
        public int Pick(bool aka, int which)
        {
            if (Xuan.Contains(which))
            {
                Xuan.Remove(which);
                (aka ? DingAka : DingAo).Add(which);
                return which;
            }
            else if (which == 0 && SilencedIdx < Secrets.Count)
            {
                int wh = Secrets[SilencedIdx++];
                Xuan.Remove(wh);
                (aka ? DingAka : DingAo).Add(wh);
                return wh;
            }
            else return 0;
        }
        // Used only in Client side, given a certain results, returns whether real
        public bool PickReport(bool aka, int which)
        {
            if (Xuan.Contains(which))
            {
                Xuan.Remove(which);
                (aka ? DingAka : DingAo).Add(which);
                return true;
            }
            else
            {
                Xuan.Remove(0);
                (aka ? DingAka : DingAo).Add(which);
                return false;
            }
        }
        public string ToMessage()
        {
            List<int>[] lists = new List<int>[] { Xuan, DingAka, DingAo, BanAka, BanAo };
            List<string> answers = new List<string>();
            foreach (List<int> list in lists)
            {
                if (list.Count > 0)
                {
                    answers.Add(list.Count.ToString());
                    foreach (int iv in list)
                    {
                        if (Secrets.Contains(iv))
                            answers.Add("0");
                        else
                            answers.Add(iv.ToString());
                    }
                }
                else
                    answers.Add("0");
            }
            return string.Join(",", answers);
        }
        public void ToHint(ushort ut, Base.VW.IVI vi,
            Func<IEnumerable<int>, string> withCode, Func<IEnumerable<int>, string> withoutCode)
        {
            bool ao = (ut > 0 && ut < 1000 && ut % 2 == 0);
            bool aka = (ut > 0 && ut < 1000 && ut % 2 == 1);
            List<int> dingR = (ao ? DingAo : DingAka).ToList();
            List<int> dingO = (ao ? DingAka : DingAo).ToList();
            List<int> banR = (ao ? BanAo : BanAka).ToList();
            List<int> banO = (ao ? BanAka : BanAo).ToList();

            if (!ao)
            {
                List<int> hinds = dingO.Intersect(Secrets).ToList();
                foreach (int h in hinds)
                {
                    dingO.Remove(h); dingO.Add(0);
                }
            }
            if (!aka)
            {
                List<int> hinds = dingR.Intersect(Secrets).ToList();
                foreach (int h in hinds)
                {
                    dingR.Remove(h); dingR.Add(0);
                }
            }

            List<int> x = Xuan.ToList();
            List<int> xhinds = x.Intersect(Secrets).ToList();
            foreach (int xh in xhinds)
            {
                x.Remove(xh); x.Add(0);
            }
            vi.Cout(ut, "当前可选角色-{0}.", withCode(x));
            if (dingR.Count > 0)
                vi.Cout(ut, "我方已选角色-{0}.", withoutCode(dingR));
            if (dingO.Count > 0)
                vi.Cout(ut, "对方已选角色-{0}.", withoutCode(dingO));
            if (banR.Count > 0)
                vi.Cout(ut, "我方已禁角色-{0}.", withoutCode(banR));
            if (banO.Count > 0)
                vi.Cout(ut, "对方已禁角色-{0}.", withoutCode(banO));
        }
    }

    public class CastingCongress : Casting
    {
        public List<int> XuanAka { private set; get; }
        public List<int> XuanAo { private set; get; }
        public IDictionary<ushort, int> Ding { private set; get; }

        public bool DecidedAka { set; get; }
        public bool DecidedAo { set; get; }

        public bool CaptainMode { set; get; }
        // e.g. CP false, SS2 true
        public List<int> Secrets { private set; get; }
        //public bool Viewable { set; get; }

        public CastingCongress(List<int> xuanAka, List<int> xuanAo,
            List<int> secrets)
        {
            XuanAka = new List<int>(); XuanAka.AddRange(xuanAka);
            XuanAo = new List<int>(); XuanAo.AddRange(xuanAo);
            Secrets = new List<int>(); Secrets.AddRange(secrets);
            Ding = new Dictionary<ushort, int>();
            for (ushort i = 1; i <= playerCapacity; ++i)
                Ding[i] = 0;
            DecidedAka = false;
            DecidedAo = false;
            CaptainMode = false;
        }
        public void Init(ushort ut, int selAva)
        {
            Ding[ut] = selAva;
        }
        public int Set(ushort to, int which)
        {
            ushort putBackTo;
            return Set(to, which, out putBackTo);
        }
        // return type : -1->invalid, >=0->the one put back, 0 no retrieve
        public int Set(ushort to, int which, out ushort putBackTo)
        {
            if (to == 0)
            {
                putBackTo = 0;
                foreach (var pr in Ding)
                {
                    if (pr.Value == which)
                    {
                        if (pr.Key % 2 == 0) // Ao
                            XuanAo.Add(which);
                        else
                            XuanAka.Add(which);
                        Ding[pr.Key] = 0; return which;
                    }
                }
                return -1;
            }
            else
            {
                if (to % 2 == 0)
                { // Ao
                    if (XuanAo.Contains(which))
                    {
                        XuanAo.Remove(which);
                        int putBack = Ding[to];
                        if (putBack != 0)
                            XuanAo.Add(putBack);
                        Ding[to] = which;
                        putBackTo = 0;
                        return putBack;
                    }
                }
                else
                {
                    if (XuanAka.Contains(which))
                    {
                        XuanAka.Remove(which);
                        int putBack = Ding[to];
                        if (putBack != 0)
                            XuanAka.Add(putBack);
                        Ding[to] = which;
                        putBackTo = 0;
                        return putBack;
                    }
                }
                foreach (var pair in Ding)
                {
                    if (pair.Key != to && pair.Key % 2 == to % 2 && pair.Value == which)
                    {
                        int putBack = Ding[to];
                        Ding[to] = which;
                        Ding[pair.Key] = putBack;
                        putBackTo = pair.Key;
                        return putBack;
                    }
                }
                putBackTo = 0;
                return -1;
            }
        }
        public bool IsDecide(ushort ut)
        {
            return !Ding.Any(p => (p.Key % 2 == ut % 2) && p.Value == 0);
        }
        public string ToMessage(bool akaTeam, bool watch = false)
        {
            bool aka = !watch && akaTeam;
            bool ao = !watch && !akaTeam;
            List<int> allAka = new List<int>();
            List<int> allAo = new List<int>();
            allAka.AddRange(XuanAka);
            allAka.AddRange(Ding.Where(p => p.Key % 2 == 1).Select(p => p.Value).Where(p => p != 0));
            allAo.AddRange(XuanAo);
            allAo.AddRange(Ding.Where(p => p.Key % 2 == 0).Select(p => p.Value).Where(p => p != 0));
            if (!aka)
            {
                for (int i = 0; i < allAka.Count; ++i)
                    if (Secrets.Contains(allAka[i]))
                        allAka[i] = 0;
            }
            if (!ao)
            {
                for (int i = 0; i < allAo.Count; ++i)
                    if (Secrets.Contains(allAo[i]))
                        allAo[i] = 0;
            }
            List<string> answers = new List<string>();
            List<int>[] lists = watch ? new List<int>[] { allAka, allAo } :
                (aka ? new List<int>[] { XuanAka, allAo } : new List<int>[] { allAka, XuanAo });
            foreach (List<int> list in lists)
            {
                if (list.Count > 0)
                    answers.Add(list.Count + "," + string.Join(",", list));
                else
                    answers.Add("0");
            }
            // true if aka but i % 2 == 1, or ao and i % 2 == 0
            for (ushort i = 1; i <= 6; ++i)
                answers.Add(i + "," + ((!watch && ((i % 2 == 0) ^ akaTeam) ? Ding[i].ToString() : "0")));
            return string.Join(",", answers);
        }

        public void ToHint(ushort ut, Base.VW.IVI vi,
            Func<IEnumerable<int>, string> withCode, Func<int, string> withoutCode)
        {
            bool isAo = (ut > 0 && ut < 1000 && ut % 2 == 0);
            List<int> xuanR = isAo ? XuanAo : XuanAka;
            List<int> xuanO = isAo ? XuanAka : XuanAo;
            vi.Cout(ut, "对方可选角色-{0}.", withCode(xuanO));
            vi.Cout(ut, "我方可选角色-{0}.", withCode(xuanR));
            if (Ding.Any(p => p.Value != 0))
                vi.Cout(ut, "当前已选角色-{0}.", string.Join(",", Ding.Where(
                    p => p.Value != 0).Select(p => p.Key + ":" + withoutCode(p.Value))));
        }

        public void ToInputRequire(ushort ut, Base.VW.IVI vi)
        {
            vi.Cout(ut, "===> 选择您的角色{0}{1}.", Ding[ut] != 0 ? "，0为退回" : "",
                 IsCaptain(ut) ? "，X为选将确定" : "");
        }

        public bool IsCaptain(ushort ut) { return ut == 3 || ut == 4; }
        public ushort[] GetCaptainLoop() { return new ushort[] { 4, 3, 3, 4 }; }
    }
}
