using System.Collections.Generic;
using UnityEngine;
using RPGTest.Models.Entity;

namespace RPGTest.Managers
{
    public class AbilityManager : MonoBehaviour
    {
        public void Start()
        {

        }

        /// <summary>
        /// Attempt to add multiple items in the inventory
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<string> TryAddAbilites(PlayableCharacter pc, List<string> abilitiesIds)
        {
            var leftovers = new List<string>();
            foreach (var abilityId in abilitiesIds)
            {
                var remaining = TryAddAbility(pc, abilityId);
            }
            return leftovers;
        }

        /// <summary>
        /// Attempt to add an item in the inventory.
        /// If the item is already present we simply increase the owned quantity.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public bool TryAddAbility(PlayableCharacter pc, string abilityId)
        {
            if (pc.Abilities.Contains(abilityId))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
