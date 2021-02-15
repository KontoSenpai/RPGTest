using System.Collections.Generic;
using System.Linq;

namespace RPGTest.Models
{
    public class EnemySpawner : IdObject
    {
        public List<EnemyGroup> Groups { get; set; }

        public List<EnemyReference> GetGroup()
        {
            //Random toto = new Random();
            //toto.Next();
            return Groups.FirstOrDefault().EnemyReferences;
        }
    }

    public class EnemyGroup
    {
        public double Probability { get; set; }
        public List<EnemyReference> EnemyReferences { get; set; }
    }

    public class EnemyReference
    {
        public string EnemyID { get; set; }
        public bool FrontRow { get; set; }
    }
}
