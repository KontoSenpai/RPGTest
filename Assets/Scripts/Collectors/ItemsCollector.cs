using RPGTest.Models.Banks;
using RPGTest.Models.Items;
using System.Linq;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class ItemCollector : ICollector
    {
        static public string ItemsAsset = ((TextAsset)Resources.Load("Configs/Items")).text;

        private static ItemBank _bank;
        public static ItemBank Bank => _bank ?? (_bank = Collect<ItemBank>(ItemsAsset));

        public static Item TryGetItem(string ID)
        {
            if(Bank != null && Bank.Items != null)
            {
                var item = Bank.Items.SingleOrDefault(x => x.Id == ID);
                return item;
            }
            return null;
        }

        public static Consumable TryGetConsumable(string ID)
        {
            if(Bank != null && Bank.Consumables != null)
            {
                var consumable = Bank.Consumables.SingleOrDefault(x => x.Id == ID);
                return consumable;
            }
            return null;
        }

        public static Equipment TryGetEquipment(string ID)
        {
            if (Bank != null && Bank.Equipments != null)
            {
                var equipment = Bank.Equipments.SingleOrDefault(x => x.Id == ID);
                return equipment;
            }
            return null;
        }
    }
}

