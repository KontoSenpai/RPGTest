using System;
using System.Linq;
using UnityEngine;

namespace RPGTest.Helpers
{
    public static class IntExtensions
    {
        private static int m_tries = 3;
        public static int GetNumberInRangeWithRetries(int min = 0, int max = 100, int retries = -1)
        {
            int[] results = new int[3];
            if(retries == -1)
            {
                retries = m_tries;
            }

            for (int i = 0; i < retries; i++)
            {
                System.Random rng = new System.Random();
                results[i] = rng.Next(min, max + 1);
            }

            return results.Max();
        }

        public static bool InRange(this int num, int min, int max)
        {
            return num >= min && num <= max;
        }
    }
}
