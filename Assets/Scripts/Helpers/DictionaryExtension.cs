using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGTest.Helpers
{
    public static class DictionaryExtension
    {

        public static bool TryCopy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, out Dictionary<TKey, TValue> outDictionary)
        {
            outDictionary = new Dictionary<TKey, TValue>();

            foreach(TKey key in dictionary.Keys)
            {
                outDictionary.Add(key, dictionary[key]);
            }

            return true;
        }
    }
}
