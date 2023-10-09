using RPGTest.Helpers;
using System.Collections.Generic;

namespace RPGTest.Models.Items
{
    public class LootTable
    {
        public List<LootDrop> LootDrops;

        public Dictionary<string,int> GetLoot()
        {
            var loot = new Dictionary<string, int>();

            foreach(var drop in LootDrops)
            {
                var rng = IntExtensions.GetNumberInRangeWithRetries(0,100,1);
                if (rng / 100 < drop.Probability)
                {
                    loot.Add(drop.Id, drop.Quantity);
                }
            }

            return loot;
        }
    }

    public class LootDrop
    {
        public string Id { get; set; }

        public int Quantity { get; set; }

        public float Probability { get; set; }
    }
}
