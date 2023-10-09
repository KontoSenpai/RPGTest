using System.Linq;
using System.Linq;
using RPGTest.Models.Abilities;
using RPGTest.Models.Banks;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class AbilitiesCollector : ICollector
    {
        static public string AbilitiesAsset = ((TextAsset)Resources.Load("Configs/Abilities")).text;

        private static AbilityBank _bank;
        public static AbilityBank Bank => _bank ?? (_bank = Collect<AbilityBank>(AbilitiesAsset));

        public static Ability TryGetAbility(string ID)
        {
            if (Bank != null && Bank.Abilities != null)
            {
                var item = Bank.Abilities.SingleOrDefault(x => x.Id == ID);
                return item;
            }
            return null;
        }
    }
}

