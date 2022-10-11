using RPGTest.Inputs;
using RPGTest.Enums;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public class TargettingSystem : MonoBehaviour
    {
        private bool m_targetMode = false;

        private List<TargetType> m_possibleTargets;

        private List<Enemy> m_enemies;
        private List<PlayableCharacter> m_party;

        private PlayableCharacter m_requester;

        //Target navigation
        private bool m_targetEnemy = false;
        private bool m_targetAll = false;
        private int m_targetIndex = -1;

        private Controls m_playerInput;


        public event TargetingValidatedHandler PlayerTargettingDone;
        public delegate void TargetingValidatedHandler(PlayableCharacter requester, bool submit, List<Entity> targets);


        public void OnEnable() => m_playerInput.Enable();

        public void OnDisable() => m_playerInput.Disable();

        public void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
            m_playerInput.UI.Submit.performed += Submit_performed;
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        private void Submit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(m_targetMode)
                ValidateTargeting(true);
        }

        private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (m_targetMode)
                ValidateTargeting(false);
        }

        private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(m_targetMode)
            {
                var movement = obj.ReadValue<Vector2>();
                if(movement.y > 0)
                {
                    if (m_targetAll)
                    {
                        ChangeAllTargeting(false, 0);
                    }
                    else if (m_targetEnemy && m_targetIndex < m_enemies.Where(e => e.IsAlive).Count() - 1)
                    {
                        ChangeTargeting(1);
                    }
                    else if (!m_targetEnemy && m_targetIndex < m_party.Count() - 1)
                    {
                        ChangeTargeting(1);
                    }
                }
                else if(movement.y < 0)
                {
                    if (m_targetAll)
                    {
                        ChangeAllTargeting(false, m_targetEnemy ? m_enemies.Count - 1 : m_party.Count - 1);
                    }
                    else if (m_targetIndex > 0)
                    {
                        ChangeTargeting(-1);
                    }
                }
                else if(movement.x < 0)
                {
                    if (m_targetEnemy && !m_targetAll && m_possibleTargets.Contains(TargetType.Enemies))
                    {
                        ChangeAllTargeting(true);
                    }
                    else if (!m_targetEnemy && m_targetAll && m_possibleTargets.Contains(TargetType.SingleAlly))
                    {
                        ChangeAllTargeting(false);
                    }
                    else if (!m_targetEnemy && m_possibleTargets.Contains(TargetType.SingleEnemy))
                    {
                        ChangeTargetingSide(true);
                    }
                    else if (!m_targetEnemy && m_possibleTargets.Contains(TargetType.Enemies))
                    {
                        ChangeTargetingSide(true, true);
                    }
                }
                else if(movement.x > 0)
                {
                    if (m_targetEnemy && m_targetAll && m_possibleTargets.Contains(TargetType.SingleEnemy))
                    {
                        ChangeAllTargeting(false);
                    }
                    else if (!m_targetEnemy && m_possibleTargets.Contains(TargetType.Allies))
                    {
                        ChangeAllTargeting(true);
                    }
                    else if (m_targetEnemy && m_targetAll && m_possibleTargets.Contains(TargetType.SingleEnemy))
                    {
                        ChangeAllTargeting(false);
                    }
                    else if (m_targetEnemy && m_possibleTargets.Contains(TargetType.SingleAlly))
                    {
                        ChangeTargetingSide(false);
                    }
                    else if (m_targetEnemy && m_possibleTargets.Contains(TargetType.Allies))
                    {
                        ChangeTargetingSide(false, true);
                    }
                }
            }
        }

        public void Initialize(List<PlayableCharacter> party, List<Enemy> enemies)
        {
            m_party = party;
            m_enemies = enemies;
        }

        public void UI_BattleOverlay_PlayerTargetingRequested(PlayableCharacter playableCharacter, TargetType defaultTargetType, List<TargetType> targetTypes)
        {
            switch (defaultTargetType)
            {
                case TargetType.Enemies:
                    ChangeTargetingSide(true, true);
                    break;
                case TargetType.SingleEnemy:
                    ChangeTargetingSide(true);
                    break;
                case TargetType.Allies:
                    ChangeTargetingSide(false, true);
                    break;
                case TargetType.SingleAlly:
                case TargetType.Self:
                    ChangeTargetingSide(false);
                    break;
            }

            m_possibleTargets = targetTypes;
            m_requester = playableCharacter;
            m_targetMode = true;
        }

        private void ValidateTargeting(bool submit)
        {
            List<Entity> targets = new List<Entity>();
            if (submit)
            {
                if (m_targetEnemy && m_targetAll)
                {
                    targets.AddRange(m_enemies.Where(en => en.IsAlive));
                }
                else if (m_targetEnemy)
                {
                    targets.Add(m_enemies[m_targetIndex]);
                }
                else if (!m_targetEnemy && m_targetAll)
                {
                    targets.AddRange(m_party);
                }
                else if (!m_targetEnemy)
                {
                    targets.Add(m_party[m_targetIndex]);
                }
            }

            ToggleTargetsCursor(false);
            PlayerTargettingDone(m_requester, submit, submit ? targets : null);
            m_requester = null;
            m_targetIndex = -1;
            m_possibleTargets = null;
            m_targetMode = false;
        }

        private void ChangeTargeting(int indexVariation = 0)
        {
            ToggleTargetCursor(false);
            m_targetIndex += indexVariation;
            ToggleTargetCursor(true);
        }

        private void ChangeTargetingSide(bool targetEnemy, bool toAll = false)
        {
            ToggleTargetsCursor(false);

            m_targetEnemy = targetEnemy;
            m_targetIndex = 0;
            m_targetAll = toAll;

            if (m_targetAll)
            {
                ToggleTargetsCursor(true);
            }
            else
            {
                ToggleTargetCursor(true);
            }

        }

        private void ChangeAllTargeting(bool toAll, int newIndex = -1)
        {
            if (m_targetAll == toAll)
                return;

            if (newIndex != -1)
                m_targetIndex = newIndex;

            m_targetAll = toAll;
            if (m_targetAll)
            {
                ToggleTargetsCursor(true);
            }
            else
            {
                ToggleTargetsCursor(false);
                ToggleTargetCursor(true);
            }

        }

        private void ToggleTargetsCursor(bool visibility)
        {
            if (m_targetEnemy)
            {
                foreach (var enemy in m_enemies)
                {
                    enemy.BattleModel.GetComponent<BattleModel>().ToggleCursor(visibility);
                }
            }
            else
            {
                foreach (var partyMember in m_party)
                {
                    partyMember.BattleModel.GetComponent<BattleModel>().ToggleCursor(visibility);
                }
            }
        }

        private void ToggleTargetCursor(bool visibility)
        {
            if (m_targetEnemy)
            {
                m_enemies[m_targetIndex].BattleModel.GetComponent<BattleModel>().ToggleCursor(visibility);
            }
            else
            {
                m_party[m_targetIndex].BattleModel.GetComponent<BattleModel>().ToggleCursor(visibility);
            }
        }
    }
}
