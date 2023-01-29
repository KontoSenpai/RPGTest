using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Interactibles
{
    public class NpcBank
    {

        [YamlIgnore]
        public List<Npc> Npcs
        {
            get
            {
                var npcList = new List<Npc>();
                npcList.AddRange(Merchants);
                npcList.AddRange(Talkers);
                return npcList;
            }
        }


        public List<Merchant> Merchants { get; set; }
        public List<Talker> Talkers { get; set; }
    }
}
