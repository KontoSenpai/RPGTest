using RPGTest.Models;
using RPGTest.Modules.Battle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RPGTest.Interactibles
{
    public class InteractibleEnemy : MonoBehaviour, IInteractible
    {
        [SerializeField] private SphereCollider BattleTriggerCollider;
        [SerializeField] private BoxCollider PlayerDetectionCollider;

        [SerializeField] private NavMeshAgent NavMeshAgent;

        [SerializeField] private List<EnemyReference> Enemies = new List<EnemyReference>();
        [SerializeField] private Enums.EncounterType EncounterType;
        [SerializeField] private string SpecialText;
        [SerializeField] private AudioClip Bgm;

        [SerializeField] private float ChaseDistanceThrehshold;
        [SerializeField] private float ChaseSpeed;

        [SerializeField] private float WanderWaitDelay;
        [SerializeField] private float WanderRadiusAllowance;
        [SerializeField] private float WanderSpeed;

        // Chase
        private GameObject m_playerController;
        private Vector3 m_chaseStartingPoint;
        private bool m_inChase;
        // Wander
        private float m_maxWanderDistance;
        private Vector3 m_wanderPoint;
        private bool m_isWandering;
        private bool m_canWander = true; // To prevent concurrent CoRoutine to start

        public event EnemyDestroyedHandler EnemyDestroyed;
        public delegate void EnemyDestroyedHandler(InteractibleEnemy interactible, float timerOverride);

        public void Update()
        {
            if (m_inChase)
            {
                var distance = Vector3.Distance(this.gameObject.transform.position, m_chaseStartingPoint);

                if (Mathf.Abs(distance) > ChaseDistanceThrehshold)
                {
                    m_inChase = false;
                    m_chaseStartingPoint = Vector3.zero;
                    m_playerController = null;
                    EnemyDestroyed(this, 1f);
                }
                else
                {
                    NavMeshAgent.destination = m_playerController.transform.position;
                }
            }
            else if (!m_isWandering && m_canWander)
            {
                m_canWander = false;
                StartCoroutine(BeginNewWandering());
            } 
            else if (m_isWandering)
            {
                var distanceToPoint = Vector3.Distance(this.gameObject.transform.position, m_wanderPoint);
                if (Mathf.Abs(distanceToPoint) < 1)
                {
                    m_isWandering = false;
                    m_canWander = true;
                }
            }
        }

        public void Initialize(List<EnemyReference> enemies, Enums.EncounterType encounterType, string specialText, AudioClip bgm, float parentRadius)
        {
            Enemies = enemies;
            EncounterType = encounterType;
            SpecialText = specialText;
            Bgm = bgm;

            m_maxWanderDistance = parentRadius;
            this.transform.position = GetPositionWithinRadius(transform.parent.transform.position, parentRadius);
        }

        public void Reset()
        {
            this.transform.position = GetPositionWithinRadius(transform.parent.transform.position, m_maxWanderDistance);
            m_canWander = true;
        }

        public void Interact()
        {
        }

        // Triger battle initiation in the BattleManager
        public void Interact(GameObject player, Collider collider)
        {
            m_isWandering = false;
            if (collider == BattleTriggerCollider)
            {
                //EnemyDestroyed(this, 0.0f);
                FindObjectOfType<BattleManager>().Initialize(this, Enemies, EncounterType, SpecialText, Bgm);
            } else if (collider == PlayerDetectionCollider) 
            {
                m_playerController = player;
                m_inChase = true;
                NavMeshAgent.speed = ChaseSpeed;
                m_chaseStartingPoint = this.gameObject.transform.position;
            }
        }

        public void Destroy()
        {
            EnemyDestroyed(this, 0.0f);
        }

        // TODO : failsafe in case of infinite recursivity
        private Vector3 GetPositionWithinRadius(Vector3 referencePoint, float radius)
        {
            var groupPosition = new Vector3(
                referencePoint.x + Random.Range(-radius, radius),
                referencePoint.y,
                referencePoint.z + Random.Range(-radius, radius)
            );
            if (Physics.Raycast(groupPosition, Vector3.down, out RaycastHit raycastHit, 5.0f))
            {
                return new Vector3(groupPosition.x, raycastHit.transform.position.y + 1, groupPosition.z);
            }
            return GetPositionWithinRadius(referencePoint, radius);
        }

        private IEnumerator BeginNewWandering()
        {
            yield return new WaitForSeconds(WanderWaitDelay);

            m_wanderPoint = GetPositionWithinRadius(transform.parent.transform.position, WanderRadiusAllowance);
            NavMeshAgent.destination = m_wanderPoint;
            NavMeshAgent.speed = WanderSpeed;
            m_isWandering = true;
            yield return null;
        }
    }
}
