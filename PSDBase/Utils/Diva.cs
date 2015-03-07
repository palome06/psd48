using System;
using System.Collections.Generic;
using System.Text;

namespace PSD.Base.Utils
{
    public class Diva
    {
        private IDictionary<string, object> dict = new Dictionary<string, object>();

        public Diva Set(string key, object value)
        {
            if (value == null)
                dict.Remove(key);
            else
                dict[key] = value;
            return this;
        }
        private T GetValue<T>(string key, T defVal) { return dict.ContainsKey(key) ? (T)dict[key] : defVal; }
        private List<T> GetArray<T>(string key)
        {
            return dict.ContainsKey(key) ? dict[key] as List<T> : new List<T>();
        }
        // public void SetUshort(string key, ushort value) { dict[key] = value; }
        // public void SetInt(string key, int value) { dict[key] = value; }
        // public void SetFloat(String key, float value) { dict[key] = value; }
        // public void SetString(string key, string value) { dict[key] = value; }
        // public void SetBool(string key, bool value) { dict[key] = value; }
        // public void SetMint(string key, Mint value) { dict[key] = value; }
        // public void SetArray<Type>(String key, List<Type> value) { dict[key] = value; }

        public ushort GetUshort(string key) { return GetValue<ushort>(key, (ushort)0); }
        public int GetInt(string key) { return GetValue<int>(key, (int)0); }
        //public float? GetFloat(string key) { return dict.ContainsKey(key) ? (float) dict[key] : null; }
        public string GetString(string key) { return GetValue<string>(key, null); }
        public bool? GetBool(string key) { return GetValue<bool?>(key, null); }
        public Diva GetDiva(string key) { return GetValue<Diva>(key, null); }
        public object GetObject(string key) { return GetValue<object>(key, null); }

        public List<ushort> GetUshortArray(string key) { return GetArray<ushort>(key); }
        public List<int> GetIntArray(string key) { return GetArray<int>(key); }
        // public List<float> GetFloatArray(String key) { return GetArray<flaot>(key); }
        public List<string> GetStringArray(string key) { return GetArray<string>(key); }
        public List<bool> GetBoolArray(string key) { return GetArray<bool>(key); }
        public List<Diva> GetDivaArray(string key) { return GetArray<Diva>(key); }
        public void Clear() { dict.Clear(); }
        public Diva() { }
        public Diva(params object[] pairs)
        {
            for (int i = 0; i < pairs.Length; i += 2)
            {
                string key = pairs[i] as string;
                object value = pairs[i + 1];
                Set(key, value);
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (var pair in dict)
            {
                sb.Append(pair.Key);
                sb.Append(":");
                object value = pair.Value;
                if (value is string)
                    sb.Append("\"" + value.ToString() + "\"");
                else if (value.GetType().GetGenericTypeDefinition() == typeof(List<>))
                    sb.Append("[" + value.ToString() + "]");
                else
                    sb.Append(value.ToString());
            }
            sb.Append("}\n");
            return sb.ToString();
        }
        public ICollection<string> GetKeys() { return dict.Keys; }
    }
}
