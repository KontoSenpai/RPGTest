using System.Collections.Generic;
using System.Linq;
using RPGTest.Modules.SkillTree.Models;
using YamlDotNet.Serialization;

namespace RPGTest.Modules.SkillTree
{
    public class SkillTreeBank
    {
        public List<Models.SkillTree> SkillTrees { get; set; }

        public List<SkillTreeNode> SkillNodes { get; set; }
    }
}
