using RPGTest.Managers;
using System.Collections.Generic;
using UnityEngine;
using RPGTest.UI.Widgets;
using RPGTest.Models.Entity;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using RPGTest.Models;

namespace RPGTest.UI
{
    public class UI_Equipment_SubMenu : UI_Pause_SubMenu
    {
        [SerializeField] private UI_Equipment_Equipment EquipmentInfo;
        [SerializeField] private UI_CharacterInfoDisplay_Widget CharacterInfo;

        private PlayableCharacter m_selectedCharacter;
        private PresetSlot m_currentPreset;

        //Items control
        private int m_currentNavigationIndex = 0;
        private bool m_actionInProgress = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Navigate.started += ctx => {
                m_navigateStarted = true;
            };
            m_playerInput.UI.Cycle.started += ctx =>
            {
                CycleCharacters_Performed(ctx);
            };
            m_playerInput.UI.Navigate.performed += ctx =>
            {
                //if (m_ActionMenuOpened) return;
                //m_performTimeStamp = Time.time + 0.3f;
                //Navigate_Performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.SecondaryNavigate.performed += ctx =>
            {
                SecondaryNavigate_Performed(ctx.ReadValue<Vector2>());
            };
            m_playerInput.UI.Navigate.canceled += ctx => {
                m_navigateStarted = false;
            };
            m_playerInput.UI.Submit.performed += ctx =>
            {
                //Submit_Performed();
            };
            m_playerInput.UI.SecondaryAction.performed += ctx =>
            {
                //SecondaryAction_Performed();
            };
            m_playerInput.UI.MouseMoved.performed += ctx =>
            {
                //MouseMoved_Performed();
            };
            m_playerInput.UI.RightClick.performed += ctx =>
            {
                //MouseRightClick_Performed();
            };
        }

        void Start()
        {

        }

        public void Update()
        {
            if (m_navigateStarted && (Time.time - m_performTimeStamp) >= WaitTimeBetweenPerforms)
            {
                m_performTimeStamp = Time.time;
                //Navigate_Performed(m_playerInput.UI.Navigate.ReadValue<Vector2>());
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            UpdateInputActions();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        #region Input Events
        private void MouseMoved_Performed()
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
            m_currentNavigationIndex = -1;
        }
        private void CycleCharacters_Performed(InputAction.CallbackContext ctx)
        {
            var movement = ctx.ReadValue<float>();
            if (movement > 0)
            {
                m_currentNavigationIndex++;
            } else
            {
                m_currentNavigationIndex--;
            }
            Initialize();
        }
        private void SecondaryNavigate_Performed(Vector2 movement)
        {
            if ((movement.x < -0.4f || movement.x > 0.04f))
            {
                m_currentPreset = m_currentPreset == PresetSlot.First ? PresetSlot.Second : PresetSlot.First;
                Initialize();
            }
        }
        #endregion

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Navigate",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name
                    }
                },
                {
                    "Change Character",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cycle.name
                    }
                },
                {
                    "Select",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                }
            };

            if (m_actionInProgress)
            {
                m_inputActions.Add("Cancel",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            }
            else
            {
                m_inputActions.Add("Exit Menu",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name
                    });
            }
            base.UpdateInputActions();
        }

        public override void OpenMenu(Dictionary<string, object> parameters)
        {
            base.OpenMenu(parameters);
            if (parameters.TryGetValue("CharacterIndex", out var value))
            {
                m_currentNavigationIndex = (int)value;
            } else
            {
                m_currentNavigationIndex = 0;
            }
            Initialize();
            UpdateInputActions();
        }

        public override void Initialize(bool refreshAll = true)
        {
            var characters = m_partyManager.GetAllPartyMembers();
            var character = characters[m_currentNavigationIndex];
            if (EquipmentInfo)
            {
                EquipmentInfo.Refresh(character, m_currentPreset);
            }
            if (CharacterInfo)
            {
                CharacterInfo.Refresh(character);
            }
        }
    }
}
