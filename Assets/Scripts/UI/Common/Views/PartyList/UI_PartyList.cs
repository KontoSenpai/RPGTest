using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.Models.Entity;
using RPGTest.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// Component that manage the display of a party list.
    /// </summary>
    public class UI_PartyList : UI_View
    {
        [HideInInspector]
        public GameObjectSelectionHandler CharacterSelectionChanged { get; set; }
        [HideInInspector]
        public GameObjectActionSelectionHandler CharacterSelectionConfirmed { get; set; }
        [HideInInspector]
        public event EventHandler SecondaryActionPerformed;

        [SerializeField] protected GameObject ActivePartyList;

        [SerializeField] private GameObject GuestSlot;
        [SerializeField] private bool DisplayGuestSlot;

        [SerializeField] private bool DisplayInactiveList;
        [SerializeField] protected GameObject InactivePartyList;

        private IEnumerable<UI_View_EntityInfos> m_activeCharactersControls => ActivePartyList.GetComponentsInChildren<UI_View_EntityInfos>();
        private IEnumerable<UI_View_EntityInfos> m_inactiveCharactersControls => InactivePartyList.GetComponentsInChildren<UI_View_EntityInfos>();

        public override void Awake()
        {
            base.Awake();

            foreach (var control in GetControls().ToList())
            {
                control.MemberSelected += OnMember_selected;
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            EventSystemEvents.OnSelectionUpdated += OnSelection_updated;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            EventSystemEvents.OnSelectionUpdated -= OnSelection_updated;
        }

        public override void Select()
        {
            base.Select();

            m_playerInput.UI.SecondaryAction.performed += SecondaryAction_performed;
            m_activeCharactersControls.First(c => c.gameObject.activeSelf).GetComponent<Button>().Select();
        }

        public void Select(GameObject control)
        {
            if (!this.isActiveAndEnabled) {
                return;
            }

            base.Select();
            m_playerInput.UI.SecondaryAction.performed += SecondaryAction_performed;

            GameObject matchingControl = null;
            if (control != null && control.TryGetComponent(out UI_View_EntityInfos component))
            {
                var id = component.GetPlayableCharacter()?.Id ?? string.Empty;
                matchingControl = GetComponentsInChildren<UI_View_EntityInfos>()
                    .FirstOrDefault(c => c.GetPlayableCharacter() != null && c.GetPlayableCharacter().Id == id)?.gameObject;
            }
            

            if (matchingControl == null) {
                matchingControl = m_activeCharactersControls.First(c => c.gameObject.activeSelf).gameObject;
            }

            matchingControl.GetComponent<Button>().Select();
        }

        public void DisableSecondaryAction()
        {
            m_playerInput.UI.SecondaryAction.performed -= SecondaryAction_performed;
        }

        public override void Deselect()
        {
            base.Deselect();

            DisableSecondaryAction();
        }

        public override Dictionary<string, string[]> GetInputDisplay(Controls playerInput = null)
        {
            var controls = playerInput ?? m_playerInput;
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Character",
                    new string[]
                    {
                        "UI_" + controls.UI.Navigate.name
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + controls.UI.Submit.name,
                        "UI_" + controls.UI.LeftClick.name,
                    }
                },
            };

            return m_inputActions;
        }

        /// <summary>
        /// Initialize the EntityInfo components of both the active and inactive party lists and explicit the navigation between the controls
        /// </summary>
        /// <param name="activeMembers"></param>
        /// <param name="inactiveMembers"></param>
        public void Initialize(IEnumerable<PlayableCharacter> activeMembers, IEnumerable<PlayableCharacter> inactiveMembers, PlayableCharacter guestCharacter)
        {
            if (!TryInitializeList(m_activeCharactersControls, activeMembers))
            {
                return;
            }
            if (!TryInitializeList(m_inactiveCharactersControls, inactiveMembers))
            {
                return;
            }

            UpdateControl(GuestSlot.GetComponent<UI_View_EntityInfos>(), guestCharacter);

            ExplicitNavigationForSelection();
        }

        /// <summary>
        /// Return all entity controls
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UI_View_EntityInfos> GetControls()
        {
            return m_activeCharactersControls
                .Concat(m_inactiveCharactersControls)
                .Append(GuestSlot.GetComponent<UI_View_EntityInfos>());
        }

        public int GetControlIndex(GameObject control)
        {
            var controls = GetControls().ToList();
            for(int i = 0; i < controls.Count; i++)
            {
                if (controls[i].gameObject == control)
                {
                    return i;
                }
            }
            return -1;
        }

        #region Input Events
        /// <summary>
        /// Raise an event to the hosting View for execution of a context specific action.
        /// </summary>
        private void SecondaryAction_performed(InputAction.CallbackContext ctx)
        {
            SecondaryActionPerformed(this, null);
        }
        #endregion

        #region Events
        /// <summary>
        /// Handled from the UIEventSystem, whenever a different <see cref="Selectable"/> changes
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <param name="previousSelection"></param>
        public void OnSelection_updated(GameObject currentSelection, GameObject previousSelection)
        {
            if (currentSelection != null && 
                (m_activeCharactersControls.Any(c => c.gameObject == currentSelection) ||
                m_inactiveCharactersControls.Any(c => c.gameObject == currentSelection) ||
                GuestSlot == currentSelection
                ))
            {
                CharacterSelectionChanged(currentSelection);

                if (currentSelection.GetComponent<UI_View_EntityInfos>().GetPlayableCharacter() == null)
                {
                    DisableSecondaryAction();
                } else
                {
                    m_playerInput.UI.SecondaryAction.performed += SecondaryAction_performed;
                }
            }
        }

        public void OnMember_selected(UIActionSelection action, GameObject member)
        {
            CharacterSelectionConfirmed.Invoke(member, UIActionSelection.Primary);
        }
        #endregion

        #region Private Methods

        private bool TryInitializeList(IEnumerable<UI_View_EntityInfos> controls, IEnumerable<PlayableCharacter> characters)
        {
            if (controls.Count() != characters.Count())
            {
                Debug.LogError($"Missmatch! controls length : {controls.Count()} is different than characters length: {characters.Count()}");
                return false;
            }
            UpdateControls(controls.ToList(), characters.ToList());
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateControls(List<UI_View_EntityInfos> controls, List<PlayableCharacter> characters)
        {
            for(int i = 0; i < controls.Count(); i++)
            {
                UpdateControl(controls[i], characters[i]);
            }
        }

        private void UpdateControl(UI_View_EntityInfos control, PlayableCharacter character)
        {
            control.Initialize(character, character?.EquipmentSlots?.CurrentPreset ?? Models.PresetSlot.First);
        }

        /// <summary>
        /// Set the navigation for the selection stage of the menu.
        /// </summary>
        private void ExplicitNavigationForSelection()
        {
            UI_List_Utils.SetVerticalNavigation(m_activeCharactersControls.Select(c => c.gameObject).ToList());

            var layoutGroup = InactivePartyList.GetComponent<GridLayoutGroup>();
            var existingInactiveCount = m_inactiveCharactersControls.Count(c => c.GetPlayableCharacter() != null);
            existingInactiveCount = existingInactiveCount < m_inactiveCharactersControls.Count() ? existingInactiveCount + 1 : existingInactiveCount;

            var itemsPerRow = UI_List_Utils.GetAmountOfObjectsPerGridRow(m_inactiveCharactersControls.Count(), layoutGroup.constraint, layoutGroup.constraintCount);

            UI_List_Utils.SetGridNavigation(m_inactiveCharactersControls.Take(existingInactiveCount).Select(c => c.gameObject).ToList(), itemsPerRow);

            var guestWidget = GuestSlot.GetComponent<UI_View_EntityInfos>();
            var lastActiveButton = m_activeCharactersControls.Last().GetComponent<Button>();
            var firstInactiveButton = m_inactiveCharactersControls.First().GetComponent<Button>();

            lastActiveButton.UpdateExplicitNavigation(new Dictionary<NavigationSlot, Button> { { NavigationSlot.Down, guestWidget.GetPlayableCharacter() != null ? guestWidget.GetComponent<Button>() : firstInactiveButton } });

            m_inactiveCharactersControls
                .Take(itemsPerRow)
                .Select(c => c.GetComponent<Button>())
                .ForEach(b => b.UpdateExplicitNavigation(new Dictionary<NavigationSlot, Button> { { NavigationSlot.Up, guestWidget.GetPlayableCharacter() != null ? guestWidget.GetComponent<Button>() : lastActiveButton } }));
        }
        #endregion
    }
}