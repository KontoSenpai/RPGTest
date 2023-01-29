using RPGTest.Models.Entity;
using System.Collections.Generic;

namespace RPGTest.Models.Banks
{
    public class EnemyBank
    {
        public List<EnemySpawner> EnemySpawners { get; set; }

        public List<Enemy> Enemies { get; set; }
    }
}
