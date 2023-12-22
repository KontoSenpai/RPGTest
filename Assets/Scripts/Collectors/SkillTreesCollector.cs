using System.Linq;
using RPGTest.Models.Items;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class SkillTreesCollector : ICollector
    {
        static public string SkillTrees = ((TextAsset)Resources.Load("Configs/SkillTrees")).text;

        private static SkillTreeBank _bank;
        public static SkillTreeBank Bank => _bank ?? (_bank = Collect<SkillTreeBank>(SkillTrees));

        public static SkillTree TryGetSkillTree(string ID)
        {
            if (Bank != null && Bank.SkillTrees != null)
            {
                var item = Bank.SkillTrees.SingleOrDefault(x => x.Id == ID);
                return item;
            }
            return null;
        }
    }
}

