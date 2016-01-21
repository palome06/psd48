using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.PSDGamepkg
{
    public static class Util
    {
        public static void Shuffle<Type>(this IList<Type> list)
        {
            Random random = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Type value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static IEnumerable<Type> PickSomeInRandomOrder<Type>(
            IEnumerable<Type> someTypes, int maxCount)
        {
            Random random = new Random();
            Dictionary<double, Type> randomSortTable = new Dictionary<double, Type>();
            foreach (Type someType in someTypes)
                randomSortTable[random.NextDouble()] = someType;
            return randomSortTable.OrderBy(KVP => KVP.Key).Take(maxCount).Select(KVP => KVP.Value);
        }
        public static string SParal(Board board, Func<Player, bool> where,
            Func<Player, string> stringize, string sepeartor)
        {
            string msg = string.Join(sepeartor, board.Garden.Values.Where(where).Select(stringize));
            if (msg.Length > 0)
                return msg;
            else
                return null;
        }
        public static string Substring(string content, int start, int end)
        {
            if (start < 0)
                return "";
            else if (end < 0)
                return start < content.Length ? content.Substring(start) : "";
            else
                return start < end ? content.Substring(start, end - start) : "";
        }
        public static string[] Splits(string line, string sepeator)
        {
            return line.Split(new string[] { sepeator }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static void AddToMultiMap<K, V>(IDictionary<K, List<V>> dict, K key, V value)
        {
            if (dict.ContainsKey(key))
            {
                if (!dict[key].Contains(value))
                    dict[key].Add(value);
            }
            else
            {
                List<V> list = new List<V>();
                list.Add(value);
                dict.Add(key, list);
            }
        }
        public static void AddToUniqueMultiMap<K, V>(IDictionary<K, ISet<V>> dict, K key, V value)
        {
            if (dict.ContainsKey(key))
            {
                if (!dict[key].Contains(value))
                    dict[key].Add(value);
            }
            else
            {
                ISet<V> list = new HashSet<V>();
                list.Add(value);
                dict.Add(key, list);
            }
        }
        public static void PlusToMap<K>(IDictionary<K, int> dict, K key, int delta)
        {
            if (dict.ContainsKey(key))
                dict[key] += delta;
            else
                dict.Add(key, delta);
        }
        public static T[] TakeRange<T>(T[] blocks, int jdx, int kdx)
        {
            if (jdx <= kdx && kdx <= blocks.Length)
            {
                T[] result = new T[kdx - jdx];
                for (int i = jdx; i < kdx; ++i)
                    result[i - jdx] = blocks[i];
                return result;
            }
            else
                return new T[0];
        }
        public static string RepeatString(string @string, int times)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < times; ++i)
                sb.Append(@string);
            return sb.ToString();
        }
        public static T[] RepeatToArray<T>(T value, int count)
        {
            T[] arr = new T[count];
            for (int i = 0; i < count; ++i)
                arr[i] = value;
            return arr;
        }
        public static bool TryNotEmpty(IDictionary<string, object> map, string key)
        {
            return map.ContainsKey(key) && (string)map[key] != "";
        }
        public static void SafeExecute(Action action, Action<Exception> handler)
        {
            try { action(); }
            catch (Exception ex)
            {
                if (!(ex is System.Threading.ThreadAbortException))
                {
                    handler(ex); Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
