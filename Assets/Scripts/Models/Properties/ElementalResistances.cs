using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGTest.Models
{
    /// <summary>
    /// Class used to retrieve elemental resistances from an entity configuration
    /// </summary>
    public class ElementalResistances
    {
        public float Fire { get; set; } = 0.0f;

        public float Ice { get; set; } = 0.0f;

        public float Water { get; set; } = 0.0f;

        public float Lightning { get; set; } = 0.0f;

        public float Wind { get; set; } = 0.0f;

        public float Earth { get; set; } = 0.0f;

        public float Light { get; set; } = 0.0f;

        public float Dark { get; set; } = 0.0f;
    }
}
