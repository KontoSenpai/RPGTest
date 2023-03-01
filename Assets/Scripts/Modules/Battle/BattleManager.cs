using RPGTest.Collectors;
using RPGTest.Controllers;
using RPGTest.Interactibles;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Effects;
using RPGTest.Models.Entity;
using RPGTest.Modules.Battle.Action;
using RPGTest.Modules.Battle.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public enum EntityFilter
    {
        Enemy,
        Party,
        All
    }
    public class BattleManager : MonoBehaviour 
    {
#if UNITY_EDITOR
        public EntityFilter ActionEntityFilter = EntityFilter.All;
        public string ActionEntityIDFilter;
#endif

        public Camera mainCamera;
        public GameObject SpriteDamage;
        public GameObject Character;
        public GameObject BattleEntityTemplate;
        public List<GameObject> EnemyModels;
        public bool m_waitingForUserInput = false;

        public TargettingSystem TargettingSystem => GetComponent<TargettingSystem>();

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        private Dictionary<Enemy, int> m_enemiesMap;

        private List<PlayableCharacter> m_party;
        private List<Enemy> m_enemies;
        private List<Entity> m_entities;

        private List<ActionSequence> m_actionQueue = new List<ActionSequence>();

        private UI_BattleOverlay m_battleOverlay => FindObjectOfType<GameManager>().UIManager.GetUIComponent<UI_BattleOverlay>();

        private Coroutine m_battleCoroutine;

        private InteractibleEnemy m_interactibleEnemy;

        void Start()
        {
            if(m_battleOverlay != null)
            {
                m_battleOverlay.Initialize(TargettingSystem);
                m_battleOverlay.RewardPanelClosed += RewardPanel_OnPanelClosed;
            }
        }

        public void Initialize(InteractibleEnemy interactibleEnemy, List<EnemyReference> enemies, Enums.EncounterType encounterType, string specialText, AudioClip BGM)
        {
            m_waitingForUserInput = false;

            Character.SetActive(false);
            m_interactibleEnemy = interactibleEnemy;
            m_interactibleEnemy.transform.parent.gameObject.SetActive(false); // Disable the spawner

            m_enemiesMap = EnemiesCollector.EnemyReferencesToEnemies(enemies);
            m_party = m_partyManager.GetActivePartyMembers();

            InitializeEntities();

            var position = m_interactibleEnemy.transform.parent.position;
            // Move camera to appropriate position
            Camera camera = FindObjectOfType<Camera>();
            camera.transform.position = new Vector3(position.x - 5, position.y + 2, position.z - 1);
            camera.transform.localEulerAngles = new Vector3(20, 80, 0);
            camera.gameObject.GetComponent<NoClippingCameraController>().enabled = false;

            foreach (var enemy in m_enemies)
            {
                enemy.BattleModel = Instantiate(BattleEntityTemplate, this.transform);
                enemy.BattleModel.GetComponent<BattleEntity>().Initialize(enemy, EnemyModels.SingleOrDefault(x => x.name == enemy.Id), position, (1 + (enemy.FrontRow ? 0 : 1)), m_enemies.Count > 1 ? m_enemies.IndexOf(enemy) - 1 : -2);
            }

            foreach (var partyMember in m_party)
            {
                var mesh = FindObjectOfType<GameManager>().PartyManager.BattleModels.SingleOrDefault(x => x.name == partyMember.Id);
                
                partyMember.BattleModel = Instantiate(BattleEntityTemplate, this.transform);
                partyMember.BattleModel.GetComponent<BattleEntity>().Initialize(partyMember, mesh, position, -1 * (1 + (partyMember.FrontRow ? 0 : 1)), m_party.Count > 1 ? m_party.IndexOf(partyMember) - 1 : -2, true);
                
                partyMember.CurrentStamina = partyMember.BaseAttributes.MaxStamina;
                partyMember.PlayerInputRequested += PartyMember_OnPlayerInputRequested;
            }

            InitializeBattleUI();
            DisplayEncounterString(encounterType, specialText);
            TargettingSystem.Initialize(m_party, m_enemies);
            
            m_battleCoroutine = StartCoroutine(BattleLogic());
        }

        public void ExitCombat(bool victory)
        {
            StopCoroutine(m_battleCoroutine);
            CleanBattleUI();

            foreach(ActionSequence actionSequence in m_actionQueue)
            {
                actionSequence.InterruptSequence();
            }
            foreach (Enemy enemy in m_enemies)
            {
                Destroy(enemy.BattleModel);
            }
            foreach (PlayableCharacter partyMember in m_party)
            {
                Destroy(partyMember.BattleModel);
            }

            if (victory)
            {
                int totalExp = 0;
                Dictionary<string, int> items = new Dictionary<string, int>();
                int totalGold = 0;
                foreach(var enemy in m_enemies)
                {
                    totalExp += enemy.ExperienceBounty;
                    totalGold += enemy.GoldBounty;
                    var loot = enemy.LootTable.GetLoot();
                    foreach(var item in loot)
                    {
                        if (items.ContainsKey(item.Key))
                        {
                            items[item.Key] += item.Value;
                        }
                        else
                        {
                            items.Add(item.Key, item.Value);
                        }
                    }
                }

                m_battleOverlay.DisplayRewards(totalExp, items, totalGold);
            }
            else // TODO, eventually
            {

            }
        }

        /// <summary>
        /// Coroutine to handle the ATB charge and Action delay
        /// </summary>
        /// <returns></returns>
        private IEnumerator BattleLogic()
        {
            do
            {
                bool actionInProgress = m_actionQueue.Any(x => x.SequenceState == ActionState.Executing);
                if (!m_waitingForUserInput && !actionInProgress)
                {
                    foreach (var actionSequence in m_actionQueue)
                    {
                        if (actionSequence.SequenceState == ActionState.Pending)
                        {
                            StartCoroutine(actionSequence.BeginSequence());
                        }
                        else if (actionSequence.SequenceState == ActionState.Ready)
                        {
                            StartCoroutine(actionSequence.ExecuteSequence(this, m_party, m_enemies));
                            break;
                        }
                    }

                    actionInProgress = m_actionQueue.Any(x => x.SequenceState == ActionState.Executing);

                    if (!actionInProgress)
                    {
                        foreach (Entity entity in m_entities.Where(e => e.IsAlive))
                        {
#if UNITY_EDITOR
                            if (!FilterApplies(entity)) continue;
#endif
                            if (!m_waitingForUserInput && !m_actionQueue.Any(x => x.Caster.Name == entity.Name) && entity.FillATB())
                            {
                                entity.RefillResources();
                                entity.ReduceStatusDurations();
                                StartCoroutine(entity.SelectAction(this, m_party, m_enemies, selectedActions => QueueActionSequence(selectedActions)));
                            }
                        }
                    }
                }
                yield return null;
            }
            while (true);
        }

#if UNITY_EDITOR
        private bool FilterApplies(Entity entity)
        {
            if (ActionEntityFilter == EntityFilter.All) return true;
            if (entity.Id.StartsWith("PC") && ActionEntityFilter != EntityFilter.Party)
            {
                return false;

            } else if (!entity.Id.StartsWith("PC") && ActionEntityFilter != EntityFilter.Enemy)
            {
                return false;
            }

            return string.IsNullOrEmpty(ActionEntityIDFilter) || entity.Id == ActionEntityIDFilter;
        }
#endif

        private void QueueActionSequence(ActionSequence actionSequence)
        {
            foreach(EntityAction action in actionSequence.Actions)
            {
                action.ActionLogged += Action_OnActionLogged;
                action.EffectApplied += Action_OnEffectApplied;
                action.ActionCompleted += Action_OnActionCompleted;
            }

            actionSequence.ActionSequenceCompleted += ActionSequence_OnSequenceCompleted;
            m_actionQueue.Add(actionSequence);
        }

        private void ClearActionQueue()
        {
            foreach(var action in m_actionQueue)
            {
                action.CancelSequence();
            }
            m_actionQueue.Clear();

            foreach(var partyMember in m_party)
            {
                partyMember.ResetATB();
            }
        }
        
        #region Events
        private void PartyMember_OnPlayerInputRequested(PlayableCharacter character, bool waitStatus)
        {
            if (waitStatus)
            {
                foreach (ActionSequence actionSequence in m_actionQueue)
                {
                    actionSequence.InterruptSequence();
                }
                m_battleOverlay.ShowActionSelection(character);
            }
            else
            {
                foreach (ActionSequence actionSequence in m_actionQueue)
                {
                    actionSequence.ResumeSequence();
                }
            }
        }

        private void ActionSequence_OnSequenceCompleted(ActionSequence actionSequence)
        {
            foreach(EntityAction action in actionSequence.Actions)
            {
                action.ActionLogged -= Action_OnActionLogged;
                action.EffectApplied -= Action_OnEffectApplied;
                action.ActionCompleted-= Action_OnActionCompleted;
            }
            actionSequence.ActionSequenceCompleted -= ActionSequence_OnSequenceCompleted;
            m_actionQueue.Remove(actionSequence);

            if (m_enemies.TrueForAll(x => !x.IsAlive))
            {
                ClearActionQueue();
                ExitCombat(true);
            }
            else if (m_party.TrueForAll(x => !x.IsAlive))
            {
                ClearActionQueue();
                Debug.Log("LOSE");
                ExitCombat(false);
            }
            else
            {
                foreach (var act in m_actionQueue)
                {
                    act.ResumeSequence();
                }
            }
        }

        private void Action_OnActionLogged(string actionString)
        {
            StartCoroutine(m_battleOverlay.DisplayMessage(actionString));
        }

        private void Action_OnEffectApplied(EffectEvaluation effect, int value)
        {
            if (effect.EffectType == Enums.EffectType.Damage || effect.EffectType == Enums.EffectType.Heal)
            {
                var targetTransform = effect.Target.BattleModel.transform;
                var sprite = Instantiate(SpriteDamage, targetTransform);
                sprite.GetComponent<BattleSprite>().Initialize(targetTransform.position, value);
            }
        }

        private void Action_OnActionCompleted(IEnumerable<Entity> entities)
        {
            foreach(var entity in entities)
            {
                if (!entity.IsAlive)
                {
                    foreach (ActionSequence actionSequence in m_actionQueue.Where(a => a.Caster == entity))
                    {
                        actionSequence.InterruptSequence();
                    }
                    m_actionQueue.RemoveAll(a => a.Caster == entity);
                    entity.ResetATB();
                }
            }
        }

        private void RewardPanel_OnPanelClosed()
        {
            m_battleOverlay.Close();

            Camera camera = FindObjectOfType<Camera>();
            Character.SetActive(true);
            m_interactibleEnemy.transform.parent.gameObject.SetActive(true);
            m_interactibleEnemy.GetComponent<InteractibleEnemy>().Destroy();
            camera.gameObject.GetComponent<NoClippingCameraController>().enabled = true;
        }
        #endregion

        private void InitializeBattleUI()
        {
            m_battleOverlay.Initialize(m_party);
        }

        private void InitializeEntities()
        {
            m_enemies = GetEnemies().ToList();
            m_entities = new List<Entity>();
            m_entities.AddRange(m_enemies);
            m_entities.AddRange(m_party);
        }

        private IEnumerable<Enemy> GetEnemies()
        {
            foreach (KeyValuePair<Enemy, int> pair in m_enemiesMap)
            {
                for(int i = 0; i < pair.Value; i++)
                {
                    char letter = (char)('A' + (char)(i % 27));
                    yield return new Enemy(pair.Key, pair.Value == 1 ? null : letter.ToString());
                }
            }
        }

        private void DisplayEncounterString(Enums.EncounterType encounterType, string specialText)
        {
            StringBuilder builder = new StringBuilder();
            bool plural = false;
            foreach (KeyValuePair<Enemy, int> pair in m_enemiesMap)
            {
                if(pair.Value == 1)
                {
                    if (builder.ToString() != "") { // Already added an entry
                        plural = true;
                    }
                    builder.Append($"A {pair.Key.Name} ");
                } else
                {
                    builder.Append($"{pair.Value} {pair.Key.Name}s ");
                    plural = true;
                }

            }

            switch (encounterType) {
                case Enums.EncounterType.Normal:
                    if(specialText != null)
                    {
                        builder.Append($"made {( plural ? "their" : "its")} apparition");
                    }
                    break;
                default:
                    break;
            }

            StartCoroutine(m_battleOverlay.DisplayMessage(builder.ToString(), 1.5f));
        }

        /// <summary>
        /// Unplug and delete UI Widgets
        /// </summary>
        private void CleanBattleUI()
        {
            foreach(var partyMember in m_party)
            {
                partyMember.PlayerInputRequested -= PartyMember_OnPlayerInputRequested;
            }
            m_battleOverlay.Clean();
        }
    }
}
