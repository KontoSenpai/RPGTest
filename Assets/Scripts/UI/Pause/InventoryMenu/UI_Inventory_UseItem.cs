using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.Modules.Battle.Action;
using RPGTest.UI.Common;
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

        [SerializeField] private UI_EquipmentSlots PanelEquipment;

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
        private Slot m_slot; // where the piece of gear is equipped on current owner
        private Slot m_selectedSlot; // where the selected piece of gear will be equipped

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        private GameObject m_currentSelectedPartyMember => m_partyMembers[m_memberIndex];

        //public bool AnyApplicableMember => m_partyMembers.Count > 0 && m_partyMembers.All(x => x.GetComponent<Button>().interactable);

        public override void Awake()
        {
            base.Awake();

            m_playerInput.UI.Navigate.performed += OnNavigate_performed;

            //m_playerInput.UI.Submit.performed += OnSubmit_performed;

            m_playerInput.UI.Cancel.performed += OnCancel_performed;
            m_playerInput.UI.RightClick.performed += OnCancel_performed;

            m_playerInput.UI.MouseMoved.performed += OnMouseMoved_performed;

            PanelEquipment.EquipActionPerformed += OnEquipAction_Performed;
            PanelEquipment.EquipActionCancelled += OnEquipAction_Cancelled;
        }

        /// <summary>
        /// Behaviour when opening the window for a consummable
        /// </summary>
        /// <param name="item">Consummable to use</param>
        public void Open(Item item)
        {
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
        public void Open(Item item, PlayableCharacter owner, Slot slot)
        {
            Initialize(item);

            if (owner == null)
            {
                InitializeEquip();
            } 
            else
            {
                m_owner = owner;
                m_slot = slot;
            }
            RefreshEquipmentPanel();

            EnableControls();
            UpdateInputActions();
        }

        /// <summary>
        /// Behaviour when closing the window
        /// Clear all lists and reset the state of the window
        /// </summary>
        public void Close()
        {
            Clear();
            gameObject.SetActive(false);
            DisableControls();
        }

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Change Character",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name  + ".vertical"
                    }
                },
                {
                    "Validate Selection",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                },
                {
                    "Cancel Selection",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                        "UI_" + m_playerInput.UI.RightClick.name,
                    }
                }
            };
            base.UpdateInputActions();
        }

        #region input events
        private void OnSubmit_performed(InputAction.CallbackContext ctx)
        {
            UseItem(m_currentSelectedPartyMember);
        }
        
        private void OnNavigate_performed(InputAction.CallbackContext ctx)
        {
            var movement = ctx.ReadValue<Vector2>();
            if (m_memberIndex == -1)
            {
                m_memberIndex = 0;
                m_partyMembers[m_memberIndex].GetComponent<Button>().Select();
            } 
            else if (movement.y > 0 && m_memberIndex > 0)
            {
                m_memberIndex -= 1;
            }
            else if (movement.y < 0 && m_memberIndex < m_partyMembers.Count - 1)
            {
                m_memberIndex += 1;
            }

            if (m_item.Type == ItemType.Equipment)
            {
                RefreshEquipmentPanel();
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
        
        public void OnEquipAction_Performed(Slot slot)
        {
            var character = m_currentSelectedPartyMember.GetComponent<UI_PartyMember>().GetCharacter();

            character.TryEquip(slot, (Equipment)m_item, out List<Item> changedItems);
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
        private void OnMemberPlayerWidget_Updated(Enums.Attribute attribute)
        {
            Refresh();
            ItemInteractionPerformed(MenuActionType.Use, new List<Item> { m_item });
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
            this.gameObject.SetActive(true);
            m_item = item;

            InitializeCharacterList();
        }

        /// <summary>
        /// Instiantiate Selector for all party members.
        /// </summary>
        private void InitializeCharacterList()
        {
            m_partyMembers = new List<GameObject>();
            m_memberIndex = 0;

            foreach (var member in m_partyManager.GetAllExistingPartyMembers())
            {
                var uiMember = CreateInstantiateItem(PartyMemberGo);
                var uiMemberScript = uiMember.GetComponent<UI_PartyMember>();
                uiMember.transform.name = member.Name;

                uiMemberScript.Initialize(member);
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
            PanelEquipment.gameObject.SetActive(false);
        }

        private void InitializeEquip()
        {
            Header.text = "Equip Item";
            PanelEquipment.gameObject.SetActive(true);
        }

        /// <summary>
        /// Will refresh all instantiated components
        /// </summary>
        private void Refresh()
        {
            m_memberIndex = 0;
            foreach (var item in m_partyMembers)
            {
                item.GetComponent<UI_PartyMember>().Refresh();
            }
        }

        /// <summary>
        /// Clean everything that have been instantiated
        /// Reset properties state
        /// </summary>
        private void Clear()
        {
            PanelEquipment.GetComponent<UI_EquipmentSlots>().Clean();
            PanelEquipment.gameObject.SetActive(false);

            if (m_partyMembers != null)
            {
                foreach (var item in m_partyMembers)
                {
                    item.GetComponent<UI_PartyMember>().MemberSelected -= OnMember_Selected;
                    Destroy(item);
                }
                m_partyMembers.Clear();
            }

            foreach (var member in m_partyManager.GetAllExistingPartyMembers())
            {
                member.PlayerWidgetUpdated -= OnMemberPlayerWidget_Updated;
            }

            m_owner = null;
            m_slot = Slot.None;
        }

        private void RefreshEquipmentPanel()
        {
            var character = m_partyMembers[m_memberIndex].GetComponent<UI_PartyMember>().GetCharacter();
            PanelEquipment.GetComponent<UI_EquipmentSlots>().Initialize(character, m_item as Equipment);
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
            var character = member.GetComponent<UI_PartyMember>().GetCharacter();
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
                    PanelEquipment.GetComponent<UI_EquipmentSlots>().Open();
                    break;
            }
        }
        #endregion
    }
}
