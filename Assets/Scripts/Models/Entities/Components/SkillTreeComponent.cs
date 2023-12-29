using RPGTest.Collectors;
using System.Collections.Generic;
using RPGTest.Modules.SkillTree.Models;

namespace RPGTest.Models.Entity.Components
{
    // SkillTreeComponent is a component to use for internal Operations on entities skill trees, to make sure the base class is only a data handler
    public class SkillTreeComponent
    {
        public string SkillTreeId { get; set; }

        public int UnspentSkillPoints { get; set; }

        public Dictionary<string, int> SpentSkillPoints { get; set; } = new();

        private SkillTree m_SkillTree => SkillTreesCollector.TryGetSkillTree(SkillTreeId);
    }
}


