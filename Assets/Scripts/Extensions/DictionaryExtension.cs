using System.Collections.Generic;

namespace RPGTest.Extensions
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
