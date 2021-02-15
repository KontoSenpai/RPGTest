using RPGTest.Models.Items;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Banks
{
    public class ItemBank
    {
        [YamlIgnore]
        public List<Item> Items
        {
            get
            {
                var itemList = new List<Item>(); 
                itemList.AddRange(Consumables);
                itemList.AddRange(Equipments);
                itemList.AddRange(KeyItems);
                return itemList;
            }
        }

        public List<Consumable> Consumables { get; set; }

        public List<Equipment> Equipments { get; set; }

        public List<KeyItem> KeyItems { get; set; }
    }
}
