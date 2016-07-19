using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public static class Card
    {
        private static Random randomSeed = new Random();
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
                pkgs = new int[] { 1, 2, 4 };
            else if (pkgCode == 4)
                pkgs = new int[] { 1, 2, 4, 5, 7 };
            else if (pkgCode == 5)
                pkgs = new int[] { 1, 2, 3, 4, 5, 6, 7 };
            return pkgs;
        }

        public enum Genre { NIL, Tux, NMB, Eve, TuxSerial, Rune, Five, Exsp, Hero, NPC }
        public static char Genre2Char(this Genre genre)
        {
            return new char[] { ' ', 'C', 'M', 'E', 'G', 'F', 'V', 'I', 'H', 'N' }[(int)genre];
        }
        public static Genre Char2Genre(this char @char)
        {
            switch (@char)
            {
                case 'C': return Genre.Tux;
                case 'M': return Genre.NMB;
                case 'E': return Genre.Eve;
                case 'G': return Genre.TuxSerial;
                case 'F': return Genre.Rune;
                case 'V': return Genre.Five;
                case 'I': return Genre.Exsp;
                case 'H': return Genre.Hero;
                case 'N': return Genre.NPC; // npc specified only
                default: return Genre.NIL;
            }
        }
        // genre of piles/dices
        public enum PileGenre { Tux, NMB, Eve, UH, UM, UN }
    }
}
