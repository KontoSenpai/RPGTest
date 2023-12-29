using System.Collections.Generic;

using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace RPGTest.Modules.SkillTree.Models
{
    /// <summary>
    /// A SkillTreeNodeRequirement represents the requirements for a <see cref="SkillTreeNode"/> to be learnt or acquired
    /// </summary>
    public class SkillTreeNodeRequirement
    {
        public List<string> NodeIds { get; set; }

        public List<string> QuestIds { get; set; }
    }
}