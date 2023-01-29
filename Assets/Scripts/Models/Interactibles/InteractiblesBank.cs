using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RPGTest.Models.Interactibles
{
    public class InteractiblesBank
    {
        [YamlMember(Alias = "Chests")]
        public List<Chest> Chests { get; set; }

        [YamlMember(Alias = "Merchants")]
        public List<Merchant> NPCs { get; set; }
    }
}
