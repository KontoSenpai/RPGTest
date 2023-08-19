using RPGTest.Helpers;
using RPGTest.Enums;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Items
{
    public class Equipment : Item
    {
        [YamlMember(Alias = "EQUIPMENT_TYPE")]
        public EquipmentType EquipmentType { get; set; }

        public Dictionary<Attribute, int> Attributes { get; set; }

        public bool TwoHanded { get; set; } = false;

        // Elements used when using physical attacks or abilities
        public Dictionary<Element, float> Elements { get; set; } = new Dictionary<Element, float>() { { Element.None, 0.0f } };

        // Elemental resistances granted by piece of equipment
        public Dictionary<Element, float> ElementResistances { get; set; } = new Dictionary<Element, float>() { { Element.None, 0.0f } };

        // Status Effects inflicted with physical attacks or abilities
        public Dictionary<StatusEffect, float> StatusEffects { get; set; } = new Dictionary<StatusEffect, float>() { { StatusEffect.None, 0.0f } };

        // Status Effect resistances granted by piece of equipmewnt
        public Dictionary<StatusEffect, float> StatusResistances { get; set; } = new Dictionary<StatusEffect, float>() { { StatusEffect.None, 0.0f } };

        public virtual Range PowerRange { get; set; }

        public bool IsWeapon => ((int)EquipmentType).InRange(0,7);
        public bool IsHeadArmor => ((int)EquipmentType).InRange(8, 9);
        public bool IsBodyArmor => ((int)EquipmentType).InRange(10, 12);
        public bool IsAccessory => ((int)EquipmentType).InRange(13, 13);
    }

}
