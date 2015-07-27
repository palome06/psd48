using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public class Card
    {
        private static Random randomSeed = new Random();
        /// <summary>
        /// Generate Random Piles
        /// </summary>
        /// <param name="except">except for some codes that exists</param>
        /// <param name="ranges">range of possible value, (e.g) [1,100,400,500]</param>
        /// <returns>Queue of the piles</returns>
        public static List<ushort> GeneratePiles(List<ushort> except, ushort[] ranges)
        {
            List<ushort> nums = new List<ushort>();
            if (except != null && except.Count > 0)
            {
                except.Sort();
                int expIdx = 0;
                for (int i = 0; i < ranges.Length; i += 2)
                    for (ushort j = ranges[i]; j <= ranges[i + 1]; ++j)
                    {
                        while (j >= except[expIdx] && expIdx < except.Count)
                            ++expIdx;
                        if (expIdx < except.Count && j != except[expIdx])
                            nums.Add(j);
                    }
            }
            else
            {
                for (int i = 0; i < ranges.Length; i += 2)
                    for (ushort j = ranges[i]; j <= ranges[i + 1]; ++j)
                        nums.Add(j);
            }
            return nums;
        }

        public static IEnumerable<Type> PickSomeInRandomOrder<Type>(
            IEnumerable<Type> someTypes, int maxCount)
        {
            Dictionary<double, Type> randomSortTable = new Dictionary<double, Type>();
            foreach (Type someType in someTypes)
                randomSortTable[randomSeed.NextDouble()] = someType;
            return randomSortTable.OrderBy(KVP => KVP.Key).Take(maxCount).Select(KVP => KVP.Value);
        }

        public static IEnumerable<Type> PickSomeInGivenProbability<Type>(
            IEnumerable<Type> someTypes, double propbability)
        {
            List<Type> result = new List<Type>();
            foreach (Type type in someTypes) {
                double decide = randomSeed.NextDouble();
                if (decide < propbability)
                    result.Add(type);
            }
            return result;
        }

        public static int[] Level2Pkg(int level)
        {
            int[] pkgs = null;
            int pkgCode = level >> 1;
            if (pkgCode == 1)
                pkgs = new int[] { 1 };
            else if (pkgCode == 2)
                pkgs = new int[] { 1, 2 };
            else if (pkgCode == 3)
                pkgs = new int[] { 1, 2, 4, 6 };
            else if (pkgCode == 4)
                pkgs = new int[] { 1, 2, 3, 4, 5, 6, 7 };
            return pkgs;
        }
    }
}
