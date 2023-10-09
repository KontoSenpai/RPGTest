using RPGTest.Enums;
using System.Collections.Generic;

namespace RPGTest.Modules.Localization.Models
{
    public class LocalizationLine
    {
        public string Id { get; set; }

        public Dictionary<Language, string> LocalizedValues { get; set; }
    }
}

