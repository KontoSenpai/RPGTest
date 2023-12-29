using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace RPGTest.Modules.SkillTree.Models
{
    /// <summary>
    /// A SkillTree defines all learnable <see cref="SkillTreeNode"/>
    /// </summary>
    public class SkillTree
    {
        public string Id { get; set; }

        public IEnumerable<string> NodeIds { get; set; }

        [YamlIgnore]
        public IEnumerable<SkillTreeNode> SkillNodes { get; set; }

        public List<string> GetUnlockedAbilities(Dictionary<string, int> unlockedNodes)
        {
            List<string> unlockIDs = new List<string>();
            foreach (var unlockedNode in unlockedNodes)
            {
                if (unlockedNode.Value <= 0)
                {
                    continue;
                }

                var skillNode = SkillNodes.SingleOrDefault(n => n.Id == unlockedNode.Key);
                if (skillNode == null)
                {
                    throw new System.Exception($"Unknown SkillNode {unlockedNode.Key} in SkillTree {Id}");
                }

                unlockIDs.AddRange(GetUnlockedSkillNodeUnlockIds(skillNode.Abilities, unlockedNode.Value));
            }
            return unlockIDs;
        }

        public List<string> GetUnlockedEffects(Dictionary<string, int> unlockedNodes)
        {
            List<string> unlockIDs = new List<string>();
            foreach (var unlockedNode in unlockedNodes)
            {
                if (unlockedNode.Value <= 0)
                {
                    continue;
                }

                var skillNode = SkillNodes.SingleOrDefault(n => n.Id == unlockedNode.Key);
                if (skillNode == null)
                {
                    throw new System.Exception($"Unknown SkillNode {unlockedNode.Key} in SkillTree {Id}");
                }

                unlockIDs.AddRange(GetUnlockedSkillNodeUnlockIds(skillNode.Effects, unlockedNode.Value));
            }
            return unlockIDs;
        }

        private List<string> GetUnlockedSkillNodeUnlockIds(List<SkillTreeNodeUnlockable> unlocks, int rank)
        {
            List<string> unlockIDs = new List<string>();
            foreach (var unlock in unlocks)
            {
                if (rank < unlock.RequiredRank)
                {
                    continue;
                }
                if (rank > unlock.MaxRank)
                {
                    rank = unlock.MaxRank;
                }
                unlockIDs.Add($"{unlock.Id}.{rank}");
            }
            return unlockIDs;
        }
    }
}
