using RPGTest.Models.Interactibles;
using System.Linq;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class NpcCollector : ICollector
    {
        static public string NPCAsset = ((TextAsset)Resources.Load("Configs/NPCs")).text;

        private static NpcBank _bank;
        public static NpcBank Bank => _bank ?? (_bank = Collect<NpcBank>(NPCAsset));

        public static Merchant TryGetMerchant(string ID)
        {
            if (Bank != null && Bank.Merchants != null)
            {
                var npc = Bank.Merchants.SingleOrDefault(x => x.Id == ID);
                return npc;
            }
            return null;
        }
    }
}
