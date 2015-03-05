using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base
{
	public class SKBranch
	{
		public int Type { set; get; }

		public string Occur { set; get; }

		public int Priority { set; get; }
		// 0x1, only once in a trigger or not
		public bool Once { set; get; }
		// 0x2, trigger one by one or altogether
		public bool Serial { set; get; }
		// 0x4, whether hind or not
		public bool Hind { set; get; }
		// public bool Termin { set; get; }
		public int MixCode
		{
			set
			{
				Once = (MixCode & 0x1) == 0;
				Serial = (MixCode & 0x2) == 0;
				Hind = (MixCode & 0x4) == 0;
			}
			get { return (Once ? 0x1 : 0 | MixCode ? 0x2 : 0 | Hind ? 0x4 : 0); }
		}
		// Lock == null means locked when optimized and whereas
		public bool? Lock { set; get; }

		public bool Linked { get { return Occur.Contains("&"); }

		public static SKBranch[] ParseFromStrings(string occurStr, string priortyStr, string mixCodeStr)
		{
			string[] occurs = occurStr.Split(',');
            string[] priorties = priortyStr.Split(',');
            string[] mixCodes = mixedCodeStr.Split(',');
            int sz = occurs.Length;
			SKBranch[] skbs = new SKBranch[sz];
            for (int i = 0; i < sz; ++i)
            {
            	if (occurs[i] == "^") { skbs[i] = null; continue; }
                SKBranch skb = new SKBranch()
                {
                    Type = i,
                    Prioritiy = int.Parse(priorties[i]),
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
	}
}