using RPGTest.Models.Interactibles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class InteractiblesCollector : ICollector
    {
        static public string InteractiblesAsset = ((TextAsset)Resources.Load("Configs/Interactibles")).text;

        private static InteractiblesBank _bank;
        public static InteractiblesBank Bank => _bank ?? (_bank = Collect<InteractiblesBank>(InteractiblesAsset));

        /*
        public static Interactible TryGetInteractibleObject(string ID)
        {
            if (Bank != null && Bank.Interactibles != null)
            {
                var interactible = Bank.Interactibles.SingleOrDefault(x => x.Id == ID);
                return interactible;
            }
            return null;
        }
        */

        public static Chest TryGetChest(string ID)
        {
            if (Bank != null && Bank.Chests != null)
            {
                var chest = Bank.Chests.SingleOrDefault(x => x.Id == ID);
                return chest;
            }
            return null;
        }

        public static void TryUpdateChestInventory(string ID, Dictionary<string, int> newInventory)
        {
            if (Bank != null && Bank.Chests != null)
            {
                Bank.Chests.SingleOrDefault(x => x.Id == ID).Inventory = newInventory;
            }
        }
    }
}