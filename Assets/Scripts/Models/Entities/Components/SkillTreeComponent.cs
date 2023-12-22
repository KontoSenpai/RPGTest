
using RPGTest.Collectors;
using RPGTest.Models.Items;
using System.Collections.Generic;

namespace RPGTest.Models.Entity.Components
{
    // SkillTreeComponent is a component to use for internal Operations on entities skill trees, to make sure the base class is only a data handler
    public class SkillTreeComponent
    {
        public string SkillTreeID { get; set; }

        public int UnspentSkillPoints { get; set; }

        public Dictionary<string, int> SpentSkillPoints;

        private SkillTree m_SkillTree => SkillTreesCollector.TryGetSkillTree(SkillTreeID);
    }
}


