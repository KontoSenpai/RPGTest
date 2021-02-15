using RPGTest.Collectors;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Interactibles
{
    public class InteractibleEnemy : MonoBehaviour, IInteractible
    {
        private List<EnemyReference> m_enemies = new List<EnemyReference>();

        public void Initialize(List<EnemyReference> enemies)
        {
            m_enemies = enemies;
;        }

        public void Interact(List<EnemyReference> enemies)
        {

        }

        public void Interact()
        {
            FindObjectOfType<BattleManager>().Initialize(GetEnemies().ToList(), this.transform.parent.transform.position);
            Destroy();
        }

        private IEnumerable<Enemy> GetEnemies()
        {
            Dictionary<string, int> sameCount = new Dictionary<string, int>();
            foreach(var enemy in m_enemies)
            {
                var model = EnemiesCollector.TryGetEnemy(enemy.EnemyID);

                if(sameCount.ContainsKey(enemy.EnemyID))
                {
                    sameCount[enemy.EnemyID] +=1;
                }
                else
                {
                    sameCount.Add(enemy.EnemyID, 0);
                }

                char letter = (char)('A' + (char)(sameCount[enemy.EnemyID] % 27));
                yield return new Enemy(model, m_enemies.Where(x => x.EnemyID == enemy.EnemyID).Count() == 1 ? null : letter.ToString());
            }
        }

        private void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}
