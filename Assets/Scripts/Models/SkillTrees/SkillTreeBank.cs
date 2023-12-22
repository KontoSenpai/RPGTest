using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Items
{
    public class SkillTreeBank
    {
        [YamlIgnore]
        public List<SkillTree> Trees
        {
            get
            {
                var treesList = new List<SkillTree>();
                foreach (var tree  in SkillTrees)
                {
                    tree.SkillNodes = SkillNodes.Where(x => tree.Nodes.Contains(x.Id));
                }
                return treesList;
            }
        }

        public List<SkillTree> SkillTrees { get; set; }

        public List<SkillNode> SkillNodes { get; set; }
    }
}
