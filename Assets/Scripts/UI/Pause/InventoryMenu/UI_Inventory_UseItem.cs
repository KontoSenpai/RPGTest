using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.Modules.Battle.Action;
using RPGTest.Modules.Party;
using RPGTest.UI.Common;
using RPGTest.UI.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_Inventory_UseItem : UI_Dialog
    {
        [SerializeField] private TextMeshProUGUI Header;

        [SerializeField] private GameObject PartyList;
        [SerializeField] private GameObject PartyMemberGo;

        [SerializeField] private UI_EquipmentSet PanelEquipment;

        [HideInInspector]
        public CancelActionHandler ItemInteractionCancelled { get; set; }
        [HideInInspector]
        public ItemInteractionPerformedHandler ItemInteractionPerformed { get; set; }
        [HideInInspector]
        public delegate void ItemInteractionPerformedHandler(MenuActionType actionType, List<Item> items);

        private Item m_item;

        // Navigation
        private List<GameObject> m_partyMembers;
        private int m_memberIndex;

        //Equip Specific
        private PlayableCharacter m_owner; // character currently using selected piece of gear

        private PresetSlot m_preset; // preset where the piece of gear is currently equipped on current owner
        private EquipmentSlot m_slot; // slot where the piece of gear is equipped on current owner

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        private GameObject m_currentSelectedPartyMember => m_partyMembers[m_memberIndex];

        //public bool AnyApplicableMember => m_partyMembers.Count > 0 && m_partyMembers.All(x => x.GetComponent<Button>().interactable);

        public override void Awake()
        {
            base.Awake();

            m_playerInput.UI.Navigate.performed += OnNavigate_performed;

            m_playerInput.UI.SecondaryAction.performed += OnSecondaryAction_performed;

            m_playerInput.UI.Cancel.performed += OnCancel_performed;
            m_playerInput.UI.RightClick.performed += OnCancel_performed;

            m_playerInput.UI.MouseMoved.performed += OnMouseMoved_performed;

            PanelEquipment.SlotConfirmed += OnEquipAction_Performed;
            PanelEquipment.EquipActionCancelled += OnEquipAction_Cancelled;
        }

        /// <summary>
        /// Behaviour when opening the window for a consummable
        /// </summary>
        /// <param name="item">Consummable to use</param>
        public void Open(Item item)
        {
            base.Open();
            Initialize(item);
            InitializeUse();

            EnableControls();
            UpdateInputActions();
        }

        /// <summary>
        /// Behaviour when opening the window for a piece of equipment
        /// </summary>
        /// <param name="item">Equipement to equip</param>
        /// <param name="owner">Character who is currently using the piece of equipment. Can be null</param>
        /// <param name="slot">Slot where the equipment is equipped on the current owner</param>
        public void Open(Item item, PlayableCharacter owner, PresetSlot preset, EquipmentSlot slot)
        {
            base.Open();
            Initialize(item);

            if (owner == null)
            {
                InitializeEquip();
            } 
            else
            {
                m_owner = owner;
                m_preset = preset;
                m_slot = slot;
            }

            EnableControls();
            UpdateInputActions();
        }

        /// <summary>
        /// Behaviour when closing the window
        /// Clear all lists and reset the state of the window
        /// </summary>
        public override void Close()
        {
            Clear();
            EventSystemEvents.OnSelectionUpdated -= OnSelection_Updated;
            base.Close();
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Character",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name  + ".vertical"
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                },
                {
                    "Cancel",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                        "UI_" + m_playerInput.UI.RightClick.name,
                    }
                }
            };

            switch (m_item.Type)
            {
                case ItemType.Equipment:
                    m_inputActions.Add(
                    "Change Preset",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.SecondaryAction.name,
                    });
                    break;
            }
            base.UpdateInputActions();
        }

        #region input events      
        private void OnNavigate_performed(InputAction.CallbackContext ctx)
        {
            if (FindObjectOfType<EventSystem>().currentSelectedGameObject == null)
            {
                m_partyMembers.First().GetComponent<Button>().Select();
            }
        }

        private void OnSecondaryAction_performed(InputAction.CallbackContext ctx)
        {
            switch (m_item.Type) {
                case ItemType.Equipment:
                    PanelEquipment.ChangePreset();
                    break;
                default:
                    Debug.LogError("Unsupported action for item type!");
                    break;
            }
        }

        private void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            CancelAction();
        }

        private void OnMouseMoved_performed(InputAction.CallbackContext ctx)
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
            m_memberIndex = -1;
        }
        #endregion

        #region 
        public void OnMember_Selected(UIActionSelection selection, GameObject member)
        {
            UseItem(member);
        }

        private void OnEquipAction_Performed(PresetSlot preset, EquipmentSlot slot)
        {
            var character = m_currentSelectedPartyMember.GetComponent<UI_View_EntityInfos>().GetPlayableCharacter();

            if (m_owner != null)
            {
                m_owner.EquipmentComponent.TryUnequip(m_preset, m_slot, out var _);
            }

            character.EquipmentComponent.TryEquip(preset, slot, (Equipment)m_item, out List<Item> changedItems);
            changedItems.Add(m_item);
            if (ItemInteractionPerformed != null)
            {
                ItemInteractionPerformed(MenuActionType.Equip, changedItems);
            }
        }

        private void OnEquipAction_Cancelled()
        {
            m_currentSelectedPartyMember.GetComponent<Button>().Select();
            EnableControls();
        }

        /// <summary>
        /// Triggered everytime we perform a consumption that would trigger a change in the player state (HP/Mana/Status Effect)
        /// </summary>
        /// <param name="attribute"></param>
        private void OnMemberPlayerWidget_Updated(Attribute attribute)
        {
            Refresh();
            ItemInteractionPerformed(MenuActionType.Use, new List<Item> { m_item });
        }

        private void OnSelection_Updated(GameObject currentSelection, GameObject previousSelection)
        {
            if (m_item.Type != ItemType.Equipment || currentSelection == null)
            {
                return;
            }

            if (currentSelection.TryGetComponent<UI_View_EntityInfos>(out var compoment))
            {
                m_memberIndex = m_partyMembers.IndexOf(currentSelection);
                RefreshEquipmentPanel(compoment.GetPlayableCharacter());
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Enable the gameObject
        /// Initialize the characterList
        /// </summary>
        /// <param name="item"></param>
        private void Initialize(Item item)
        {
            m_item = item;

            InitializeCharacterList();
            EventSystemEvents.OnSelectionUpdated += OnSelection_Updated;
        }

        /// <summary>
        /// Instiantiate Selector for all party members.
        /// </summary>
        private void InitializeCharacterList()
        {
            m_partyMembers = new List<GameObject>();

            foreach (var member in m_partyManager.GetAllExistingPartyMembers())
            {
                var uiMember = CreateInstantiateItem(PartyMemberGo);
                var uiMemberScript = uiMember.GetComponent<UI_View_EntityInfos>();
                uiMember.transform.name = member.Name;

                uiMemberScript.Initialize(member, m_preset);
                uiMemberScript.MemberSelected += OnMember_Selected;

                member.PlayerWidgetUpdated += OnMemberPlayerWidget_Updated;

                m_partyMembers.Add(uiMember);
            }

            //Expliciting navigation
            foreach (var member in m_partyMembers)
            {
                var index = m_partyMembers.IndexOf(member);

                member.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? m_partyMembers[index - 1].GetComponent<Button>() : null,
                    Down: index < m_partyMembers.Count - 1 ? m_partyMembers[index + 1].GetComponent<Button>() : null);
            }
            m_partyMembers.First().GetComponent<Button>().Select();
        }

        private void InitializeUse()
        {
            Header.text = "Use Item";
            PanelEquipment.Close();
        }

        private void InitializeEquip()
        {
            Header.text = "Equip Item";
            PanelEquipment.Open();
        }

        /// <summary>
        /// Will refresh all instantiated components
        /// </summary>
        private void Refresh()
        {
            foreach (var item in m_partyMembers)
            {
                item.GetComponent<UI_View_EntityInfos>().Refresh();
            }
        }

        /// <summary>
        /// Clean everything that have been instantiated
        /// Reset properties state
        /// </summary>
        private void Clear()
        {
            PanelEquipment.GetComponent<UI_EquipmentSet>().Clean();
            PanelEquipment.gameObject.SetActive(false);

            if (m_partyMembers != null)
            {
                foreach (var item in m_partyMembers)
                {
                    item.GetComponent<UI_View_EntityInfos>().MemberSelected -= OnMember_Selected;
                    Destroy(item);
                }
                m_partyMembers.Clear();
            }

            foreach (var member in m_partyManager.GetAllExistingPartyMembers())
            {
                member.PlayerWidgetUpdated -= OnMemberPlayerWidget_Updated;
            }

            m_owner = null;
            m_preset = PresetSlot.None;
            m_slot = EquipmentSlot.None;
        }

        private void RefreshEquipmentPanel(PlayableCharacter character)
        {
            PanelEquipment.Initialize(character, m_item as Equipment);
        }

        private GameObject CreateInstantiateItem(GameObject prefab)
        {
            var uiMember = Instantiate(prefab);
            PartyMemberGo.GetComponent<RectTransform>();
            uiMember.transform.SetParent(PartyList.transform);
            uiMember.transform.localScale = new Vector3(1, 1, 1);
            return uiMember;
        }

        private void CancelAction()
        {
            ItemInteractionCancelled();
        }
        
        private void UseItem(GameObject member)
        {
            var character = member.GetComponent<UI_View_EntityInfos>().GetEntity();
            switch (m_item.Type)
            {
                case ItemType.Consumable:
                    // Todo : Evaluate status effects (death, poison etc... ) (need to determine if they linger after battle)
                    //if (((Consumable)m_item).Effects.All(x => !x.EvaluateEffectAttributes(character.GetAttributes())))
                    //{
                    //    // Todo : play meepmerp sound
                    //    return;
                    //}

                    var action = new EntityAction(character, ActionType.Item, m_item.Id, new List<Entity>() { character }, m_inventoryManager);
                    StartCoroutine(action.Execute(m_partyManager, null, null));
                    break;
                case ItemType.Equipment:
                    DisableControls();
                    m_memberIndex = m_partyMembers.IndexOf(member);
                    PanelEquipment.Select();
                    break;
            }
        }
        #endregion
    }
}
