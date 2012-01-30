using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterXml
{
    internal class XIRDataObject
    {
        private const int BASE64 = 64;
        private const int VERBATIM = 65;
        private Dictionary<string, KeyValuePair<int, string>> elements = new Dictionary<string, KeyValuePair<int, string>>();

        public XIRDataObject(string type, string subtype)
        {
            SetVerbatim("xir.type", type);
            SetVerbatim("xir.subtype", string.IsNullOrEmpty(subtype) ? "unused" : subtype);
        }

        internal void SetVerbatim(string key, string value)
        {
            value = string.IsNullOrEmpty(value) ? "None" : value;
            KeyValuePair<int, string> p = new KeyValuePair<int, string>(VERBATIM, value);
            if (elements.ContainsKey(key))
            {
                elements[key] = p;
            }
            else
            {
                elements.Add(key, p);
            }
        }

        internal void SetBase64(string key, string value)
        {
            string encoded = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(value));
            KeyValuePair<int, string> p = new KeyValuePair<int, string>(BASE64, encoded);
            if (elements.ContainsKey(key))
            {
                elements[key] = p;
            }
            else
            {
                elements.Add(key, p);
            }
        }

        private string GetTypeString(int type)
        {
            if (type == BASE64)
            {
                return "base64=";
            }
            else if (type == VERBATIM)
            {
                return "verbatim=";
            }
            return string.Empty;
        }

        internal void Print()
        {
            foreach (var iter in elements.Where(e => e.Key.IndexOf("xir.") > -1).OrderBy(e => e.Key))
            {
                Console.WriteLine(iter.Key + ":" + GetTypeString(iter.Value.Key) + iter.Value.Value);
            }

            foreach (var iter in elements.Where(e => e.Key.IndexOf("xir.") == -1).OrderBy(e => e.Key))
            {
                Console.WriteLine(iter.Key + ":" + GetTypeString(iter.Value.Key) + iter.Value.Value);
            }

            Console.WriteLine();
        }
    }
}
