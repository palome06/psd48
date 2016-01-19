﻿using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.ClientZero
{
    public static class Util
    {
        private static Random randomSeed = new Random();

        public static void Shuffle<Type>(this IList<Type> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = randomSeed.Next(n + 1);
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
        public static string SatoString<T>(IEnumerable<T> array)
        {
            return Sato(array.Select(p => p.ToString()), ",");
        }
        public static string SatoWithBracket<T>(IEnumerable<T> array, string separator, string prefix, string suffix)
        {
            string str = Sato(array, separator);
            return str == "" ? "" : prefix + str + suffix;
        }
        public static string Sato<T>(IEnumerable<T> array, string separator)
        {
            string str = "";
            foreach (T obj in array)
                str += separator + obj.ToString();
            return str.Length > 1 ? str.Substring(separator.Length) : "";
        }
        public static string SatoBool(bool[] array, int start, int end)
        {
            string str = "";
            for (int i = start; i < end;++i )
                if (array[i])
                    str += "," + i;
            return str.Length > 1 ? str.Substring(1) : "";
        }
        public static string CombineArray<T>(T[] array, int start, int end)
        {
            string str = "";
            for (int i = start; i < end; ++i)
                str += "," + array[i];
            return str.Length > 1 ? str.Substring(1) : "";
        }
        public static string SSelect(Board board, Func<Player, bool> func)
        {
            var v = board.Garden.Values.Where(func).Select(p => p.Uid.ToString());
            string msg = Sato(v, "p");
            if (msg.Length > 0)
                return "(p" + msg + ")";
            else
                return null;
        }
        public static string SParal(Board board, Func<Player, bool> where,
            Func<Player, string> stringize, string sepeartor)
        {
            var v = board.Garden.Values.Where(where).Select(stringize);
            string msg = Sato(v, sepeartor);
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
        public static void SafeExecute(Action action, Action<Exception> handler)
        {
            //try { action(); }
            //catch (Exception ex) { handler(ex);
            //while (true)
            //{
            //    int c = 2; c += 4;
            //    System.Threading.Thread.Sleep(1000);
            //}
            //}
            action();
        }
    }
}
