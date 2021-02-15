using RPGTest.Enums;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Items
{
    public class Consumable : Item
    {
        [YamlMember(Alias = "CONSUMABLE_TYPE")]
        public ConsumableType ConsumableType { get; set; }

        public List<Effect> Effects { get; set; }

        public TargetType DefaultTarget { get; set; }
        public List<TargetType> TargetTypes { get; set; }
    }
}
