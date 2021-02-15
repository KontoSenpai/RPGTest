using System;
using System.Collections.Generic;

namespace RPGTest.Models
{
    public class GrowthTable
    {
        public string CharacterId { get; set; }
        public List<int> XPToNextLevel { get; set; }
    }
}
