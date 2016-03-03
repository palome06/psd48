using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base
{
    public class SKBranch
    {
        public string Occur { set; get; }
        public int Priority { set; get; }
        // 0x1, only once in a trigger or not
        public bool Once { set; get; }
        // 0x2, trigger one by one or altogether
        public bool Serial { set; get; }
        // 0x4, whether hind or not
        public bool Hind { set; get; }
        // 0x8, change/raise new Mint enviormment
        public bool Demiurgic { set; get; }
        public int MixCode
        {
            set
            {
                Once = (value & 0x1) != 0;
                Demiurgic = (value & 0x2) != 0;
                Serial = (value & 0x4) != 0;
                Hind = (value & 0x8) != 0;
            }
            get
            {
                return (Once ? 0x1 : 0) | (Demiurgic ? 0x2 : 0) |
                       (Serial ? 0x4 : 0) | (Hind ? 0x8 : 0);
            }
        }
        // Lock == null means locked when optimized and whereas
        public bool? Lock { set; get; }

        public bool Linked { get { return Occur.Contains("&"); } }

        public static SKBranch[] ParseFromStrings(string occurStr, string priortyStr, string mixCodeStr)
        {
            string[] occurs = occurStr.Split(',');
            string[] priorties = priortyStr.Split(',');
            string[] mixCodes = mixCodeStr.Split(',');
            int sz = occurs.Length;
            SKBranch[] skbs = new SKBranch[sz];
            for (int i = 0; i < sz; ++i)
            {
                if (occurs[i] == "" || occurs[i] == "^") { skbs[i] = null; continue; }
                SKBranch skb = new SKBranch()
                {
                    Priority = int.Parse(priorties[i]),
                    MixCode = int.Parse(mixCodes[i])
                };
                if (occurs[i].StartsWith("!"))
                {
                    skb.Occur = occurs[i].Substring(1); skb.Lock = true;
                }
                else if (occurs[i].StartsWith("?"))
                {
                    skb.Occur = occurs[i].Substring(1); skb.Lock = null;
                }
                else
                {
                    skb.Occur = occurs[i]; skb.Lock = false;
                }
                skbs[i] = skb;
            }
            return skbs;
        }

        public static SKBranch[] ParseFromString(string @string)
        {
            if (string.IsNullOrEmpty(@string)) return new SKBranch[0];

            string[] strs = @string.Split(';');
            int sz = strs.Length;
            SKBranch[] skbs = new SKBranch[sz];
            for (int i = 0; i <sz; ++i)
            {
                if (strs[i] == "" ||strs[i] =="^"){skbs[i]=null;continue;}
                string[] parts = strs[i].Split(',');
                SKBranch skb = new SKBranch()
                {
                    Priority = int.Parse(parts[1]),
                    MixCode = int.Parse(parts[2])
                };
                string occur = parts[0];
                if (occur.StartsWith("!"))
                {
                    skb.Occur = occur.Substring(1); skb.Lock = true;
                }
                else if (occur.StartsWith("?"))
                {
                    skb.Occur = occur.Substring(1); skb.Lock = null;
                }
                else
                {
                    skb.Occur = occur; skb.Lock = false;
                }
                skbs[i] = skb;
            }
            return skbs;
        }

        public static string GetAas(int aliasSerial)
        {
            System.Data.DataRowCollection data = new Utils.ReadonlySQL("psd.db3")
                .Query(new string[] { "AVAL" }, "AAs", "AKEY = " + aliasSerial);
            if (data.Count == 1)
                return (string)data[0]["AVAL"];
            else
                return "";
        }
    }
}