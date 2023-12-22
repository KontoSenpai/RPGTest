using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGTest.Helpers
{
    public static class FloatExtensions
    {
        public static bool IsInRange(this float floatToCheck, float min, float max)
        {
            return floatToCheck >= min && floatToCheck <= max;
        }
    }
}
