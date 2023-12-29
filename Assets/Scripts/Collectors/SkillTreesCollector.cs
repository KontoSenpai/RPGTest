using System.Linq;
using RPGTest.Modules.SkillTree;
using RPGTest.Modules.SkillTree.Models;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class SkillTreesCollector : ICollector
    {
        public static string SkillTrees = ((TextAsset)Resources.Load("Configs/SkillTrees")).text;

        private static SkillTreeBank _bank;
        public static SkillTreeBank Bank => _bank ??= Collect<SkillTreeBank>(SkillTrees);

        public static SkillTree TryGetSkillTree(string ID)
        {
            if (Bank != null && Bank.SkillTrees != null)
            {
                var tree = Bank.SkillTrees.SingleOrDefault(x => x.Id == ID);

                if (tree != null)
                {
                    tree.SkillNodes = Bank.SkillNodes.Where(n => tree.NodeIds.Contains(n.Id));
                }
                return tree;
            }
            return null;
        }
    }
}

