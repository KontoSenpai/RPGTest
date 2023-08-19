using RPGTest.Inputs;
using RPGTest.Enums;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public partial class TargettingSystem : MonoBehaviour
    {
        private bool m_targetMode = false;

        private List<TargetType> m_possibleTargetsTypes;

        private List<PlayableCharacter> m_party;
        private List<Enemy> m_enemies;

        private Entity m_requester;

        //Target navigation
        private bool m_targetEnemy = false;
        private bool m_targetSide = false;
        private bool m_targetAll = false;
        private int m_targetIndex = -1;

        private Controls m_playerInput;


        public event TargetingValidatedHandler PlayerTargettingDone;
        public delegate void TargetingValidatedHandler(Entity requester, bool submit, List<Entity> targets);


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
                if (movement.y != 0 && CanSelectSingleTarget())
                {
                    SelectSingleTarget(movement.y > 0);
                }
                else if (movement.x != 0)
                {
                    SelectOtherSide();
                }
                UpdateInformationState();
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
                    m_targetEnemy = true;
                    m_targetSide = true;
                    break;
                case TargetType.SingleEnemy:
                    m_targetEnemy = true;
                    m_targetSide = false;
                    m_targetIndex = m_enemies.GetFirstAliveIndex();
                    break;
                case TargetType.Allies:
                    m_targetEnemy = false;
                    m_targetSide = true;
                    break;
                case TargetType.SingleAlly:
                case TargetType.Self:
                    m_targetEnemy = false;
                    m_targetIndex = m_party.GetIndexOfAlly(playableCharacter);
                    break;
                case TargetType.All:
                    m_targetAll = true;
                    break;
            }

            m_requester = playableCharacter;
            if (targetTypes != null && targetTypes.Count > 0)
            {
                m_possibleTargetsTypes = targetTypes;
            }
            else
            {
                m_possibleTargetsTypes = new List<TargetType> { defaultTargetType };
            }

            UpdateInformationState();

            m_targetMode = true;
        }

        private void ValidateTargeting(bool submit)
        {
            List<Entity> targets = new List<Entity>();
            if (submit)
            {
                if(m_targetAll)
                {
                    targets.AddRange(m_enemies.Where(en => en.IsAlive));
                    targets.AddRange(m_party.Where(en => en.IsAlive));
                }
                else if (m_targetEnemy && m_targetSide)
                {
                    targets.AddRange(m_enemies.Where(en => en.IsAlive));
                }
                else if (m_targetEnemy)
                {
                    targets.Add(m_enemies[m_targetIndex]);
                }
                else if (!m_targetEnemy && m_targetSide)
                {
                    targets.AddRange(m_party);
                }
                else if (!m_targetEnemy)
                {
                    targets.Add(m_party[m_targetIndex]);
                }
            }
          
            m_targetAll = false;
            m_targetSide = false;
            m_targetIndex = -1;

            UpdateInformationState();

            m_targetMode = false;
            PlayerTargettingDone(m_requester, submit, submit ? targets : null);
        }


        private void UpdateInformationState()
        {
            for (int i = 0; i < m_enemies.Where(e => e.IsAlive).Count(); i++)
            {
                m_enemies.Where(e => e.IsAlive).ToList()[i].BattleModel.GetComponent<BattleEntity>().ToggleCursor((m_targetIndex == i && m_targetEnemy) || m_targetAll || m_targetSide);
            }
            for (int i = 0; i < m_party.Count; i++)
            {
                m_party[i].BattleModel.GetComponent<BattleEntity>().ToggleCursor((m_targetIndex == i && !m_targetEnemy) || m_targetAll || m_targetSide);
            }
        }
    }
}
