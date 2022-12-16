using RPGTest.Collectors;
using RPGTest.Controllers;
using RPGTest.Models;
using RPGTest.Models.Action;
using RPGTest.Models.Entity;
using RPGTest.Modules.Battle;
using RPGTest.UI.Battle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace RPGTest.Managers
{
    public class BattleManager : MonoBehaviour 
    {
        public Camera mainCamera;
        public GameObject SpriteDamage;
        public GameObject Character;
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

        void Start()
        {
            if(m_battleOverlay != null)
            {
                m_battleOverlay.Initialize(TargettingSystem);
                m_battleOverlay.RewardPanelClosed += RewardPanel_OnPanelClosed;
            }
        }

        public void Initialize(List<EnemyReference> enemies, Vector3 position, Enums.EncounterType encounterType, string specialText, AudioClip BGM)
        {
            m_waitingForUserInput = false;

            m_enemiesMap = EnemiesCollector.EnemyReferencesToEnemies(enemies);
            m_party = m_partyManager.GetActivePartyMembers();

            InitializeEntities();

            Camera camera = FindObjectOfType<Camera>();
            Character.SetActive(false);
            camera.transform.position = new Vector3(position.x - 5, position.y + 2, position.z - 1);
            camera.transform.localEulerAngles = new Vector3(20, 80, 0);
            camera.gameObject.GetComponent<NoClippingCameraController>().enabled = false;

            foreach (var enemy in m_enemies)
            {
                enemy.BattleModel = InstantiateModel(enemy, position, (1 + (enemy.FrontRow ? 0 : 1)), m_enemies.Count > 1 ? m_enemies.IndexOf(enemy) - 1 : -2);
            }

            foreach (var partyMember in m_party)
            {
                partyMember.BattleModel = InstantiateModel(partyMember, position, -1 * (1 + (partyMember.FrontRow ? 0 : 1)), m_party.Count > 1 ? m_party.IndexOf(partyMember) - 1 : -2, true);
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
            else
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
                            if (!m_waitingForUserInput && !m_actionQueue.Any(x => x.Caster.Name == entity.Name) && entity.FillATB())
                            {
                                entity.RefillResources();
                                Debug.Log(entity.Name);
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

        private void QueueActionSequence(ActionSequence actionSequence)
        {
            foreach(EntityAction action in actionSequence.Actions)
            {
                action.ActionLogged += Action_OnActionLogged;
                action.ActionExecuted += Action_OnActionExecuted;
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

        private void Action_OnActionLogged(string actionString)
        {
            StartCoroutine(m_battleOverlay.DisplayMessage(actionString));
        }

        private void ActionSequence_OnSequenceCompleted(ActionSequence actionSequence)
        {
            foreach(EntityAction action in actionSequence.Actions)
            {
                action.ActionLogged -= Action_OnActionLogged;
                action.ActionExecuted -= Action_OnActionExecuted;
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

        private void Action_OnActionExecuted(Entity entity, Enums.EffectType effectType, Enums.Attribute attribute, int value)
        {
            if (effectType == Enums.EffectType.Damage || effectType == Enums.EffectType.Heal)
            {
                var sprite = Instantiate(SpriteDamage, entity.BattleModel.transform);
                sprite.GetComponent<BattleSprite>().Initialize(entity.BattleModel.transform.position, value);
            }         

            if(!entity.IsAlive)
            {
                foreach(ActionSequence actionSequence in m_actionQueue.Where(a => a.Caster == entity))
                {
                    actionSequence.InterruptSequence();
                }
                m_actionQueue.RemoveAll(a => a.Caster == entity);
                entity.ResetATB();
            }
        }

        private void RewardPanel_OnPanelClosed()
        {
            m_battleOverlay.Close();

            Camera camera = FindObjectOfType<Camera>();
            Character.SetActive(true);
            camera.gameObject.GetComponent<NoClippingCameraController>().enabled = true;
        }
        #endregion

        /// <summary>
        /// Instantiate the character model for each participating entity
        /// </summary>
        /// <param name="entity">Owner of the model</param>
        /// <param name="position">Position of the model</param>
        /// <param name="offset">Offset for front and back row</param>
        /// <param name="count">position index</param>
        /// <param name="party">Weither or not this is a party member or an enemy</param>
        /// <returns></returns>
        private GameObject InstantiateModel(Entity entity, Vector3 position, float offset, int count = -2, bool party = false)
        {
            GameObject en = null;
            bool invertZaxis = false;
            if (!party)
            {
                en = Instantiate(EnemyModels.SingleOrDefault(x => x.name == entity.Id), this.transform);
            }
            else
            {
                en = Instantiate(FindObjectOfType<GameManager>().PartyManager.BattleModels.SingleOrDefault(x => x.name == entity.Id), this.transform);
                invertZaxis = true;
            }
            en.transform.position = new Vector3(position.x + (count > -2 ? 1 * count : 0), position.y -0.5f, position.z + offset - (count * (invertZaxis ? -0.4f : 0.4f)));
            en.transform.localEulerAngles = new Vector3(0, offset > 0 ? 195 : -15, 0);
            en.name = entity.Name;
            return en;
        }

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
