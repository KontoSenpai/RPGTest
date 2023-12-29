using RPGTest.Models;
using System.Collections.Generic;

namespace RPGTest.Modules.SkillTree.Models
{
    public class SkillTreeNode : IdObject
    {
        public int XIndex { get; set; } = 1;
        
        public int YIndex { get; set; } = 1;

        public SkillTreeNodeRequirement Requirements { get; set; }

        public string Description { get; set; }

        public int MaxRank { get; set; }

        public List<SkillTreeNodeUnlockable> Abilities { get; set; }

        public List<SkillTreeNodeUnlockable> Effects { get; set; }
    }
}
