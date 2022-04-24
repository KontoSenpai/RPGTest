using RPGTest.Inputs;
using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Widgets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using RPGTest.Helpers;

namespace RPGTest.UI.Battle 
{
    public class UI_ActionSelection_Widget : MonoBehaviour
    {
        private enum MenuType
        {
            Root = 0,
            Abilities = 1,
            Items = 2
        }

        [SerializeField] private List<GameObject> RootButtons;

        [SerializeField] private GameObject InstantiableActionButton;
        [SerializeField] private GameObject ActionsViewport;
        [SerializeField] private GameObject ActionsViewportContent;
        [SerializeField] private Button DummyButton;
        

        private Controls m_playerInput;

        private List<GameObject> m_instantiatedButtons { get; set; } = new List<GameObject>();

        //Input control
        private bool m_isActive = false;
        private MenuType m_currentMenuType = MenuType.Root;
        private bool m_waitingForTarget;

        private Dictionary<MenuType, int> m_menuIndexes = new Dictionary<MenuType, int>()
        {
            { MenuType.Root, 1 },
            { MenuType.Abilities, 1 },
            { MenuType.Items, 0 }
        };

        private ActionType m_selectedActionType;
        private string m_selectedActionId;

        private const int ActionButtonWidth = 300;

        public event TargetingRequestHandler PlayerTargetingRequested;
        public delegate void TargetingRequestHandler(PlayableCharacter playableCharacter, TargetType defaultTarget, List<TargetType> targetType);

        public event MultiCastCountChangedHandler MultiCastCountChanged;
        public delegate void MultiCastCountChangedHandler(int count);

        public event MultiCastActionSelectedHandler MultiCastActionSelected;
        public delegate void MultiCastActionSelectedHandler(int index, string actionName);

        public event ResetMultiCastStateHandler ResetMultiCastStateRequested;
        public delegate void ResetMultiCastStateHandler();

        private PlayableCharacter m_playableCharacter;
        private List<EntityAction> m_selectedActions;
        private int m_actionCount = 1;
        private const int M_MAXACTIONCOUNT = 4;
        private bool m_eatOneInput = false;

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        public void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
            //m_playerInput.UI.CycleMenus.performed += PresetSwap_performed;
            m_playerInput.UI.CycleMenus.performed += ActionMulticast_performed;
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        private void Cancel_performed(InputAction.CallbackContext obj)
        {
            if(!m_waitingForTarget && m_isActive)
            {
                switch (m_currentMenuType)
                {
                    case MenuType.Root:
                        if(m_selectedActions.Count > 0)
                        {
                            int lastIndex = m_selectedActions.Count - 1;
                            m_selectedActions.RemoveAt(lastIndex);
                            MultiCastActionSelected(lastIndex, null);
                        }
                        break;
                    case MenuType.Abilities:
                    case MenuType.Items:
                        Debug.Log("Cancel");
                        DepopulateActionViewport();
                        m_currentMenuType = MenuType.Root;
                        SetCurrentButtonInteractable(RootButtons, true);
                        SelectButton(RootButtons);
                        break;
                }
            }
        }

        private void PresetSwap_performed(InputAction.CallbackContext obj)
        {
            if(!m_waitingForTarget && m_isActive)
            {
                m_playableCharacter.EquipmentSlots.ChangePreset();
            }
        }

        private void ActionMulticast_performed(InputAction.CallbackContext obj)
        {
            if (!m_waitingForTarget && m_isActive && m_selectedActions.Count == 0)
            {
                var movement = obj.ReadValue<float>();

                if(movement > 0 && m_actionCount < M_MAXACTIONCOUNT)
                {
                    m_actionCount++;
                    MultiCastCountChanged(m_actionCount);
                }
                else if(movement < 0 && m_actionCount > 1)
                {
                    m_actionCount--;
                    MultiCastCountChanged(m_actionCount);
                }
            }
        }

        private void Navigate_performed(InputAction.CallbackContext obj)
        {
            if (!m_waitingForTarget && m_isActive)
            {          
                var movement = obj.ReadValue<Vector2>();
                if (movement.y < 0)
                {
                    ChangeMenuItemIndex(+1);
                }
                else if (movement.y > 0)
                {
                    ChangeMenuItemIndex(-1);
                }
                else if (movement.x < 0 && m_currentMenuType == MenuType.Root)
                {
                    ChangeStance(false);
                }
                else if (movement.x > 0 && m_currentMenuType == MenuType.Root)
                {
                    ChangeStance(true);
                }
            }
        }

        public void Initialize(PlayableCharacter playableCharacter)
        {
            m_actionCount = 1;
            GetComponent<Canvas>().enabled = true;
            m_playableCharacter = playableCharacter;
            m_playableCharacter.CreateNewActionSequence();
            m_selectedActions = new List<EntityAction>();
            //WeaponPresetWidget.Initialize(m_playableCharacter.EquipmentSlots.CurrentPreset == PresetSlot.First);
            //StancesWidget.ChangeStanceImage((int)m_playableCharacter.Stance.GetCurrentStance());
            m_currentMenuType = MenuType.Root;

            ExplicitRootNavigation();
            
            ActionsViewport.SetActive(false);
            ScopeMainRoot();
            ChangeMenuItemIndex(0);
            RootButtons[1].GetComponent<Button>().Select();

            m_isActive = true;
            m_waitingForTarget = false;
        }

        private void CleanWidget()
        {
            ResetButtonState();
            ResetMultiCastStateRequested();
            DepopulateActionViewport();
        }

        private void ResetButtonState()
        {
            SetCurrentButtonInteractable(RootButtons, true);

            m_menuIndexes[MenuType.Root] = 1;
            m_menuIndexes[MenuType.Abilities] = 0;
            m_menuIndexes[MenuType.Items] = 0;
        }

        private void ReselectButton()
        {
            switch(m_currentMenuType)
            {
                case MenuType.Root:
                    SelectButton(RootButtons);
                    break;
                case MenuType.Abilities:
                case MenuType.Items:
                    SelectButton(m_instantiatedButtons);
                    break;
            }
        }

        public void ValidateTargetInformation(PlayableCharacter sender, bool submit, List<Entity> targets = null)
        {
            if (sender != m_playableCharacter)
                return;

            if(submit)
            {
                var newAction = new EntityAction(m_playableCharacter, m_selectedActionType, m_selectedActionId, targets, FindObjectOfType<GameManager>().InventoryManager);
                m_selectedActions.Add(newAction);
                MultiCastActionSelected(m_selectedActions.IndexOf(newAction), newAction.GetActionName());
                if(m_selectedActions.Count == m_actionCount)
                {
                    m_isActive = false;
                    m_playableCharacter.SetSelectedActions(m_selectedActions);
                    CleanWidget();
                    return;
                }
                m_eatOneInput = true;
            }
            GetComponent<Canvas>().enabled = true;
            m_playerInput.Enable();
            ReselectButton();
            m_isActive = true;
            m_waitingForTarget = false;
        }

        private void ActionButton_PlayerActionSelected(ActionType type, string actionId)
        {
            SelectAction(type, actionId);
        }

        public void SelectAction(ActionType type, string actionId)
        {
            if(m_eatOneInput)
            {
                m_eatOneInput = false;
                return;
            }
            if (!m_waitingForTarget && m_isActive)
            {
                m_waitingForTarget = true;
                m_selectedActionId = actionId;
                TargetType defaultTargetting = TargetType.SingleEnemy;
                List<TargetType> availableTargetting = new List<TargetType>();
                switch (type)
                {
                    case ActionType.Ability:
                        m_selectedActionType = ActionType.Ability;
                        var ability = AbilitiesCollector.TryGetAbility(actionId);
                        if (ability.DefaultTarget != TargetType.None)
                        {
                            defaultTargetting = ability.DefaultTarget;
                        }

                        availableTargetting = ability.TargetTypes;
                        break;
                    case ActionType.Item:
                        m_selectedActionType = ActionType.Item;
                        var item = (Consumable)ItemCollector.TryGetItem(actionId);
                        defaultTargetting = item.DefaultTarget;
                        if (item.DefaultTarget != TargetType.None)
                        {
                            defaultTargetting = item.DefaultTarget;
                        }
                        availableTargetting = item.TargetTypes;
                        break;
                }
                m_playerInput.Disable();
                PlayerTargetingRequested(m_playableCharacter, defaultTargetting, availableTargetting);

                if(DummyButton != null)
                    DummyButton.Select();

                GetComponent<Canvas>().enabled = false;
            }
        }

        #region Button Events
        public void SelectAttack()
        {
            SelectAction(ActionType.Ability, m_playableCharacter.DefaultAttack);
        }

        public void ScopeMainRoot()
        {
            m_currentMenuType = MenuType.Root;

            SelectButton(RootButtons);
        }

        public void ScopeAbilities()
        {
            m_currentMenuType = MenuType.Abilities;
            m_menuIndexes[m_currentMenuType] = 0;
            PopulateAbilities();

            SelectButton(m_instantiatedButtons);
        }

        public void ScopeItems()
        {
            m_currentMenuType = MenuType.Items;
            m_menuIndexes[m_currentMenuType] = 0;
            PopulateItems();
            SelectButton(m_instantiatedButtons);
        }

        public void Flee()
        {

        }
        #endregion

        private void ExplicitRootNavigation()
        {
            if (m_playableCharacter.LimitReady)
            {
                RootButtons[0].SetActive(true);
            }
            if (m_playableCharacter.Abilities == null || m_playableCharacter.Abilities.Count == 0)
            {
                RootButtons[2].GetComponent<Button>().interactable = false;
            }
            foreach (var button in RootButtons)
            {
                var index = RootButtons.IndexOf(button);
                Button up = null;
                Button down = null;
                if(index > 0)
                {
                    up = RootButtons.GetRange(0, index).LastOrDefault(x => x.activeInHierarchy)?.GetComponent<Button>();
                }
                if(index < RootButtons.Count -1)
                {
                    down = RootButtons.GetRange(index + 1, (RootButtons.Count - 1) - index).FirstOrDefault(x => x.activeInHierarchy)?.GetComponent<Button>();
                }
                button.GetComponent<Button>().ExplicitNavigation(Up: up,Down: down);
            }
        }

        private void ChangeMenuItemIndex(int variation)
        {
            switch(m_currentMenuType)
            {
                case MenuType.Root:
                    HandleItemsNavigation(RootButtons, variation);
                    break;
                case MenuType.Abilities:
                case MenuType.Items:
                    HandleItemsNavigation(m_instantiatedButtons, variation);
                    ActionsViewport.GetComponent<UI_ViewportBehavior>().StepChange(variation);
                    break;
            }
        }

        private void HandleItemsNavigation(List<GameObject> buttons, int variation)
        {
            var oldIndex = m_menuIndexes[m_currentMenuType];
            var newIndex = oldIndex + variation;

            if (newIndex < 0 || newIndex > buttons.Count -1)
            {
                return;
            }

            while (!buttons[newIndex].activeInHierarchy || !buttons[newIndex].GetComponent<Button>().interactable)
            {
                if(newIndex < buttons.Count - 1 && newIndex > 0)
                {
                    newIndex += variation;
                }
                else
                {
                    return;
                }
            }

            m_menuIndexes[m_currentMenuType] = newIndex;
        }

        private void SelectButton(List<GameObject> buttons)
        {
            buttons[m_menuIndexes[m_currentMenuType]].GetComponent<Button>().Select();
        }

        #region AbilityViewport
        private void PopulateAbilities()
        {
            PopulateActionViewport(ActionType.Ability,
                m_playableCharacter.GetAbilitiesOfType(AbilityType.Default).Select( a => (a.ability.Id, a.usable)),
                2);
        }
        #endregion

        #region ItemViewport
        private void PopulateItems()
        {
            PopulateActionViewport(ActionType.Item,
                FindObjectOfType<GameManager>().InventoryManager.GetConsumables().Select(c => (c.Id, true)),
                1);
        }
        #endregion

        #region viewport
        private void PopulateActionViewport(ActionType actionType, IEnumerable<(string id, bool interactable)> actions, int coeff)
        {
            foreach (var action in actions)
            {
                var uiItem = Instantiate(InstantiableActionButton);
                uiItem.transform.SetParent(ActionsViewportContent.transform);
                uiItem.name = action.id;
                uiItem.transform.localScale = new Vector3(1, 1, 1);
                var actionButton = uiItem.GetComponent<UI_ActionButton>();
                actionButton.InitializeAction(actionType, action);
                actionButton.PlayerActionSelected += ActionButton_PlayerActionSelected;
                m_instantiatedButtons.Add(uiItem);
            }
            foreach (var instantiatedButton in m_instantiatedButtons)
            {
                var index = m_instantiatedButtons.IndexOf(instantiatedButton);
                instantiatedButton.GetComponent<Button>().ExplicitNavigation(Up: index > 0 ? m_instantiatedButtons[index - 1].GetComponent<Button>() : null,
                    Down: index < m_instantiatedButtons.Count - 1 ? m_instantiatedButtons[index + 1].GetComponent<Button>() : null);
            }

            ActionsViewport.SetActive(true);
            ActionsViewport.GetComponent<UI_ViewportBehavior>().Initialize(m_instantiatedButtons.Count, coeff);
        }

        private void DepopulateActionViewport()
        {
            ActionsViewport.SetActive(false);
            foreach (var button in m_instantiatedButtons)
            {
                var actionButton = button.GetComponent<UI_ActionButton>();
                actionButton.PlayerActionSelected -= ActionButton_PlayerActionSelected;
                Destroy(button);
            }
            m_instantiatedButtons.Clear();
        }
        #endregion

        private void SetCurrentButtonInteractable(List<GameObject> buttons, bool selectable)
        {
            buttons[m_menuIndexes[m_currentMenuType]].GetComponent<Button>().interactable = selectable;
        }

        private void ChangeStance(bool cycleRight)
        {
            m_playableCharacter.Stance.ChangeCurrentStance(cycleRight);
            //StancesWidget.ChangeStanceImage((int)m_playableCharacter.Stance.GetCurrentStance());
        }
    }
}
