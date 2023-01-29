using System.Collections.Generic;
using System.Linq;
using RPGTest.Models.Effects;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class EffectsCollector : ICollector
    {
        static public string EffectsAsset = ((TextAsset)Resources.Load("Configs/Effects")).text;

        private static EffectsBank _bank;
        public static EffectsBank Bank => _bank ??= Collect<EffectsBank>(EffectsAsset);

        public static Effect TryGetEffect(string ID)
        {
            if (Bank != null && Bank.Effects != null)
            {
                var item = Bank.Effects.SingleOrDefault(x => x.Id == ID);
                return item;
            }
            return null;
        }

        public static List<Effect> TryGetEffects(List<string> IDs)
        {
            if (Bank != null && Bank.Effects != null)
            {
                var items = Bank.Effects.Where(x => IDs.Contains(x.Id));
                return items.ToList();
            }
            return null;
        }
    }
}

