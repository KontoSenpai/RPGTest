using RPGTest.Models;

namespace RPGTest.Modules.SkillTree.Models
{
    /// <summary>
    /// A SkillTreeNodeUnlockable represents an Object unlocked via a <see cref="SkillTreeNode"/>
    /// It contains information 
    /// </summary>
    public class SkillTreeNodeUnlockable : IdObject
    {
        public int MaxRank { get; set; } = -1;

        public int RequiredRank { get; set; } = -1;
    }
}
