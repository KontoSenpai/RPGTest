using MyBox;
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

        public Dictionary<Element, int> Elements { get; set; } = new Dictionary<Element, int>() { { Element.None, 100 } };

        public virtual Range PowerRange { get; set; }

        public bool IsWeapon => ((int)EquipmentType).InRange(0,7);
        public bool IsHeadArmor => ((int)EquipmentType).InRange(8, 9);
        public bool IsBodyArmor => ((int)EquipmentType).InRange(10, 12);
        public bool IsAccessory => ((int)EquipmentType).InRange(13, 13);
    }

}
