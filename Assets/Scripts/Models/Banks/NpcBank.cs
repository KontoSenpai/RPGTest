using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Banks
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
