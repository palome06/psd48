using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.Base.Utils
{
    public static class Algo
    {
        private static Random randomSeed = new Random();

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
            Dictionary<double, Type> randomSortTable = new Dictionary<double, Type>();
            foreach (Type someType in someTypes)
                randomSortTable[randomSeed.NextDouble()] = someType;
            return randomSortTable.OrderBy(KVP => KVP.Key).Take(maxCount).Select(KVP => KVP.Value);
        }

        public static bool Include(object @object, params object[] options)
        {
            return options.Contains(@object);
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
        public static int CountItemFromComma(string line)
        {
            return string.IsNullOrEmpty(line) ? 0 : (line.Count(p => p == ',') + 1);
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
        public static T[] TakeRange<T>(T[] blocks, int start)
        {
            return TakeRange(blocks, start, blocks.Length);
        }
        public static T[] TakeRange<T>(T[] blocks, int jdx, int kdx)
        {
            if (kdx == -1)
                kdx = blocks.Length;
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
        public static ushort[] TakeArrayWithSize(ushort[] blocks, int start, out int next)
        {
            int n = (int)blocks[start];
            next = start + n + 1;
            return TakeRange(blocks, start + 1, start + 1 + n);
        }
        // rate means the size of each entry, e.g. (2, 4, -4, 5, -5) then rate = 2
        public static ushort[] TakeArrayWithSize(string[] blocks, int start, out int next, int rate = 1)
        {
            int n = int.Parse(blocks[start]);
            next = start + n * rate + 1;
            return TakeRange(blocks, start + 1, start + 1 + n * rate).Select(p => ushort.Parse(p)).ToArray();
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
        public static bool Equals<T>(T[][] arrays, int idx, int jdx, T expected)
        {
            if (idx >= arrays.Length)
                return false;
            if (jdx >= arrays[idx].Length)
                return false;
            return arrays[idx][jdx].Equals(expected);
        }
        public static bool TryNotEmpty(IDictionary<string, object> map, string key)
        {
            return map.ContainsKey(key) && (string)map[key] != "";
        }
        public static string ListToString<T>(this ICollection<T> list)
        {
            return list.Count != 0 ? (list.Count + "," + string.Join(",", list)) : "0";
        }
        public static void RemoveFromMultiMap<K, T>(IDictionary<K, List<T>> map, K key, T value)
        {
            if (map.ContainsKey(key))
            {
                List<T> list = map[key];
                list.Remove(value);
                if (list.Count == 0)
                    map.Remove(key);
            }
        }
        public static void RemoveFromMultiMap<K, T>(IDictionary<K, List<T>> map,
            K key, Predicate<T> match)
        {
            if (map.ContainsKey(key))
            {
                List<T> list = map[key];
                list.RemoveAll(match);
                if (list.Count == 0)
                    map.Remove(key);
            }
        }
        public static bool IsSubSet<T>(IEnumerable<T> subset, IEnumerable<T> set)
        {
            return subset.Intersect(set).Count() == subset.Count();
        }

        public static void LongMessageParse(string[] lines, Action<ushort> setWho,
            Action<ushort, string, object> assign, string[] keys)
        {
            int idx = 1;
            while (idx < lines.Length)
            {
                ushort who = ushort.Parse(lines[idx++]);
                setWho(who);
                for (int i = 0; i < keys.Length; ++i)
                {
                    int serp = keys[i].IndexOf(',');
                    string ktype = keys[i].Substring(0, serp);
                    string kname = keys[i].Substring(serp + 1);
                    if (ktype == "LA") // array of string
                    {
                        int n = int.Parse(lines[idx++]);
                        string[] values = TakeRange(lines, idx, idx + n);
                        assign(who, kname, values);
                        idx += n;
                    }
                    else if (ktype == "LU") // array of ushort
                        assign(who, kname, TakeArrayWithSize(lines, idx, out idx));
                    else if (ktype == "LI") // array of int
                    {
                        int n = int.Parse(lines[idx++]);
                        string[] values = TakeRange(lines, idx, idx + n);
                        assign(who, kname, values.Select(p => int.Parse(p)).ToArray());
                        idx += n;
                    }
                    else if (ktype.StartsWith("LC")) // array of ushort with size appended
                    {
                        int n = int.Parse(ktype.Substring("LC".Length));
                        string[] values = TakeRange(lines, idx, idx + n);
                        assign(who, kname, values.Select(p => ushort.Parse(p)).ToArray());
                        idx += n;
                    }
                    else if (ktype == "LD") // array of string with double size indicator
                    {
                        int n = int.Parse(lines[idx++]) * 2;
                        string[] values = TakeRange(lines, idx, idx + n);
                        assign(who, kname, values);
                        idx += n;
                    }
                    else if (ktype == "U") // single ushort
                    {
                        ushort value = ushort.Parse(lines[idx++]);
                        assign(who, kname, value);
                    }
                    else if (ktype == "I") // single integer
                    {
                        int value = int.Parse(lines[idx++]);
                        assign(who, kname, value);
                    }
                    else if (ktype == "A") // single string
                    {
                        string value = lines[idx++];
                        assign(who, kname, value);
                    }
                }
            }
        }
    }
}
