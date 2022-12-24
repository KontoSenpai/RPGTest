using UnityEngine;

namespace RPGTest.Models
{
    public class Range
    {
        public float Min { get; set; }

        public float Max { get; set; }

        public float GetValue()
        {
            return Min == 0 && Max == 0 ? 1.0f : Random.Range(Min, Max);
        }
    }
}
