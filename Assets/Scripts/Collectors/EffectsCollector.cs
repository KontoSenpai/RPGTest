using System.Collections.Generic;
using System.Linq;
using RPGTest.Models.Effects;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class EffectsCollector : ICollector
    {
        public static string EffectsAsset = ((TextAsset)Resources.Load("Configs/Effects")).text;

        private static EffectsBank _bank;
        public static EffectsBank Bank => _bank ??= Collect<EffectsBank>(EffectsAsset);

        public static Effect TryGetEffect(string ID)
        {
            if (Bank == null || Bank.Effects == null) return null;

            return Bank.Effects.SingleOrDefault(x => x.Id == ID);
        }

        public static List<Effect> TryGetEffects(IEnumerable<string> IDs)
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

