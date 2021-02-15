using RPGTest.Models;
using RPGTest.Models.Banks;
using RPGTest.Models.Entity;
using System.Linq;
using UnityEngine;

namespace RPGTest.Collectors
{
    public class EnemiesCollector : ICollector
    {
        static public string EnemiesAsset = ((TextAsset)Resources.Load("Configs/Enemies")).text;

        private static EnemyBank _bank;
        public static EnemyBank Bank => _bank ?? (_bank = Collect<EnemyBank>(EnemiesAsset));

        public static EnemySpawner TryGetEnemySpawner(string ID)
        {
            if(Bank != null && Bank.EnemySpawners != null)
            {
                var enemySpawner = Bank.EnemySpawners.SingleOrDefault(x => x.Id == ID);
                return enemySpawner;
            }
            return null;
        }

        public static Enemy TryGetEnemy(string ID)
        {
            if (Bank != null && Bank.Enemies != null)
            {
                var enemy = Bank.Enemies.SingleOrDefault(x => x.Id == ID);
                return enemy;
            }
            return null;
        }
    }
}

