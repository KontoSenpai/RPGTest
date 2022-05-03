using RPGTest.Collectors;
using RPGTest.Interactibles;
using RPGTest.Models;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Managers
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private string SpawnerId;
        [SerializeField] private GameObject enemyObject;
        [SerializeField] private int MaxUnits = 3;
        [SerializeField] private int SpawnRadius = 10;

        [SerializeField] private Enums.EncounterType EncounterType;
        [SerializeField] private string SpecialText;
        [SerializeField] private AudioClip BGM;

        private List<GameObject> m_spawnedGroups = new List<GameObject>();
        private EnemySpawner m_enemySpawner;
        //private int m_cooldownBeforeRespawn = 60;
        private bool m_init = false;

        // Start is called before the first frame update
        void Start()
        {
            this.GetComponent<MeshRenderer>().enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_spawnedGroups.Count == 0 && !m_init)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            m_enemySpawner = EnemiesCollector.TryGetEnemySpawner(SpawnerId);
            while (m_spawnedGroups.Count < MaxUnits)
            {
                var obj = SpawnObject();
                obj.transform.parent = this.transform;
                m_spawnedGroups.Add(obj);
            }
            m_init = true;
        }

        private GameObject SpawnObject()
        {
            var group = Instantiate(enemyObject);
            group.GetComponent<InteractibleEnemy>().Initialize(GetGroup(), EncounterType, SpecialText, BGM);
            var groupPosition = new Vector3(this.transform.position.x + Random.Range(-SpawnRadius, SpawnRadius), this.transform.position.y, this.transform.position.z + Random.Range(-SpawnRadius, SpawnRadius));
            if (Physics.Raycast(groupPosition, Vector3.down, out RaycastHit raycastHit, 5.0f))
            {
                group.transform.position = new Vector3(groupPosition.x, raycastHit.transform.position.y + 1, groupPosition.z);
            }
            
            return group;
        }

        private List<EnemyReference> GetGroup()
        {
            return m_enemySpawner.GetGroup();
        }
    }
}

