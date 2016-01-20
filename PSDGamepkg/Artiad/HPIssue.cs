using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.Card;

namespace PSD.PSDGamepkg.Artiad
{
    // Harm Entry, HarmResult shares the format and most of logic exception message head
    public class Harm
    {
        public ushort Who { set; get; }

        public FiveElement Element { set; get; }

        public int N { set; get; }
        // Nowadays, set 0 = unknown source, 1~6 = player, 1001~ = monster/NPC(2000+)
        public int Source { set; get; }
        // Attached Property
        public long Mask { set; get; }

        public Harm(ushort who, int source, FiveElement elem, int n, long mask)
        {
            Who = who; Element = elem;
            N = n; Source = source;
            if (elem == FiveElement.YINN)
                mask |= ((long)HPEvoMask.TUX_INAVO | (long)HPEvoMask.DECR_INVAO | (long)HPEvoMask.CHAIN_INVAO);
            else if (elem == FiveElement.SOLARIS)
                mask |= ((long)HPEvoMask.TUX_INAVO | (long)HPEvoMask.DECR_INVAO | (long)HPEvoMask.IMMUNE_INVAO);
            Mask = mask;
        }
        internal static string ToRawMessage(Harm harm)
        {
            return harm.Who + "," + harm.Source + "," + harm.Element.Elem2Int() + "," + harm.N + "," + harm.Mask;
        }
        public static string ToMessage(Harm harm) { return "G0OH," + ToRawMessage(harm); }
        public static string ToMessage(IEnumerable<Harm> harms)
        {
            return "G0OH," + string.Join(",", harms.Select(p => ToRawMessage(p)));
        }

        public static List<Harm> Parse(string line)
        {
            List<Harm> list = new List<Harm>();
            string[] blocks = line.Split(',');
            for (int i = 1; i < blocks.Length; i += 5)
            {
                ushort who = ushort.Parse(blocks[i]);
                int src = int.Parse(blocks[i + 1]);
                FiveElement elem = FiveElementHelper.Int2Elem(int.Parse(blocks[i + 2]));
                int n = int.Parse(blocks[i + 3]);
                int mask = int.Parse(blocks[i + 4]);
                list.Add(new Harm(who, src, elem, n, mask));
            }
            return list;
        }
    }
    // only for the result, just give the covertion
    public static class HarmResult
    {
        public static string ToMessage(Harm harm) { return "G1TH," + Harm.ToRawMessage(harm); }
        public static string ToMessage(IEnumerable<Harm> harms)
        {
            return "G1TH," + string.Join(",", harms.Select(p => Harm.ToRawMessage(p)));
        }
    }

    public class Cure
    {
        public ushort Who { set; get; }

        public FiveElement Element { set; get; }

        public int N { set; get; }
        // Nowadays, set 0 = unknown source, 1~6 = player, 1001~ = monster/NPC(2000+)
        public int Source { set; get; }

        public long Mask { set; get; }

        public Cure(ushort who, int source, FiveElement elem, int n, long mask)
        {
            Who = who; Element = elem;
            N = n; Source = source; Mask = mask;
        }

        public static string ToMessage(Cure cure)
        {
            return "G0IH," + cure.Who + "," + cure.Source + "," +
                cure.Element.Elem2Int() + "," + cure.N + "," + cure.Mask;
        }
        public static string ToMessage(IEnumerable<Cure> cures)
        {
            string op = string.Join(",", cures.Select(p => p.Who + "," +
                    p.Source + "," + p.Element.Elem2Int()+ "," + p.N + "," + p.Mask));
            if (!string.IsNullOrEmpty(op))
                return "G0IH," + op;
            else
                return "";
        }

        public static List<Cure> Parse(string line)
        {
            List<Cure> list = new List<Cure>();
            string[] blocks = line.Split(',');
            for (int i = 1; i < blocks.Length; i += 5)
            {
                ushort who = ushort.Parse(blocks[i]);
                int src = int.Parse(blocks[i + 1]);
                FiveElement elem = FiveElementHelper.Int2Elem(int.Parse(blocks[i + 2]));
                int n = int.Parse(blocks[i + 3]);
                int mask = int.Parse(blocks[i + 4]);
                list.Add(new Cure(who, src, elem, n, mask));
            }
            return list;
        }
    }

    public class Love
    {
        public ushort Princess { set; get; }
        // set 1~6 = player, 0 = other source (Monster, NPC, etc)
        public List<string> Prince { set; get; }
        // Count always equals 1
        public Love(ushort princess, IEnumerable<string> prince)
        {
            Princess = princess; Prince = prince.ToList();;
        }
        private string ToRawMessage()
        {
            return Princess + "," + Prince.Count + "," + string.Join(",", Prince);
        }
        public static string ToMessage(Love love) { return "G0LV," + love.ToRawMessage(); }
        public static string ToMessage(IEnumerable<Love> loves)
        {
            return "G0LV," + string.Join(",", loves.Select(p => p.ToRawMessage()));
        }
        public static List<Love> Parse(string line)
        {
            List<Love> list = new List<Love>();
            string[] g0lv = line.Split(',');
            for (int idx = 1; idx < g0lv.Length;)
            {
                ushort princess = ushort.Parse(g0lv[idx]);
                int n = int.Parse(g0lv[idx + 1]);
                string[] prince = Util.TakeRange(g0lv, idx + 2, idx + 2 + n);
                list.Add(new Love(princess, prince));
                idx += (2 + n);
            }
            return list;
        }
    }

    public class HpIssueSemaphore
    {
        private ushort Who;
        private bool IsLove;
        private FiveElement Element;
        private int Delta;
        private int HP;

        public HpIssueSemaphore(ushort who, bool isLove, FiveElement? element, int delta, int hp)
        {
            Who = who; IsLove = isLove;
            Element = element ?? FiveElement.A;
            Delta = delta; HP = hp;
        }
        private string ToRawTelegraph()
        {
            return Who + "," + (!IsLove ? ("0," + Element.Elem2Int()) : "1,0") + "," + Math.Abs(Delta) + "," + HP;
        }
        public static void Telegraph(Action<string> send, HpIssueSemaphore his)
        {
            send((his.Delta > 0 ? "E0IH," : "E0OH,") + his.ToRawTelegraph());
        }
        public static void Telegraph(Action<string> send, IEnumerable<HpIssueSemaphore> hises)
        {
            List<HpIssueSemaphore> e0ih = hises.Where(p => p.Delta > 0).ToList();
            List<HpIssueSemaphore> e0oh = hises.Where(p => p.Delta < 0).ToList();
            if (e0ih.Count > 0)
                send("E0IH," + string.Join(",", e0ih.Select(p => p.ToRawTelegraph())));
            if (e0oh.Count > 0)
                send("E0OH," + string.Join(",", e0oh.Select(p => p.ToRawTelegraph())));
        }
    }
}