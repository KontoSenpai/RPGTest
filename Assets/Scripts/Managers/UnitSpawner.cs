using RPGTest.Collectors;
using RPGTest.Interactibles;
using RPGTest.Models;
using System.Collections;
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

        [SerializeField] float TimeBeforeRespawn = 30f;

        private List<GameObject> m_spawnedGroups = new List<GameObject>();
        private EnemySpawner m_enemySpawner;


        // Start is called before the first frame update
        void Start()
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            Initialize();
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void Initialize()
        {
            m_enemySpawner = EnemiesCollector.TryGetEnemySpawner(SpawnerId);
            while (m_spawnedGroups.Count < MaxUnits)
            {
                var obj = SpawnObject();
                m_spawnedGroups.Add(obj);
            }
        }

        private IEnumerator RespawnObject(InteractibleEnemy interactible, float respawnTimer)
        {
            yield return new WaitForSeconds(respawnTimer);
            interactible.gameObject.SetActive(true);
            interactible.Reset();
            yield return null;
        }

        private GameObject SpawnObject()
        {
            var interactible = Instantiate(enemyObject);
            interactible.transform.parent = this.transform;
            var interactibleEnemy = interactible.GetComponent<InteractibleEnemy>();
            interactibleEnemy.Initialize(GetGroup(), EncounterType, SpecialText, BGM, SpawnRadius);
            interactibleEnemy.EnemyDestroyed += OnEnemyDestroyed;
            
            return interactible;
        }

        private void OnEnemyDestroyed(InteractibleEnemy interactible, float respawnOverride)
        {
            interactible.gameObject.SetActive(false);
            StartCoroutine(RespawnObject(interactible, respawnOverride > 0.0f ? respawnOverride : TimeBeforeRespawn));
        }

        private List<EnemyReference> GetGroup()
        {
            return m_enemySpawner.GetGroup();
        }
    }
}

