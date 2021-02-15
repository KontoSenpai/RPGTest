using RPGTest.Enums;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Items
{
    public class Item : IdObject
    {
        [YamlMember(Alias = "ITEM_TYPE")]
        public ItemType Type { get; set; }

        public string Description { get; set; } = "Default item description";

        public int Value { get; set; } = 0;
    }

    public class KeyItem : Item
    {
    }
}
