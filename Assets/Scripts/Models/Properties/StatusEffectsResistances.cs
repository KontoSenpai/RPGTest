using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGTest.Models
{
    /// <summary>
    /// Class used to retrieve negative status effect resistances from an entity configuration
    /// </summary>
    public class StatusEffectsResistances
    {
        public float Blind { get; set; } = 0.0f;

        public float Bleed { get; set; } = 0.0f;

        public float Poison { get; set; } = 0.0f;

        public float Paralysis { get; set; } = 0.0f;

        public float Silence { get; set; } = 0.0f;

        public float Confusion { get; set; } = 0.0f;

        public float Slow { get; set; } = 0.0f;

        public float Freeze { get; set; } = 0.0f;
    }
}
