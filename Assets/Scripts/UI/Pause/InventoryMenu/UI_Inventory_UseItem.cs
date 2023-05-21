using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Inputs;
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
    public class UI_Inventory_UseItem : MonoBehaviour
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
        public delegate void ItemInteractionPerformedHandler(MenuItemActionType actionType, List<Item> items);


        private Item m_item;
        private Slot m_slot;
        private PlayableCharacter m_owner;

        private MenuItemActionType m_panelActionType;
        private ActionType m_actionType;

        private List<GameObject> m_partyMembers;

        private int m_memberIndex;

        //Equip Specific
        private Slot m_selectedSlot;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;
        private GameObject m_currentSelectedPartyMember => m_partyMembers[m_memberIndex];

        public bool AnyApplicableMember => m_partyMembers.Count > 0 && m_partyMembers.All(x => x.GetComponent<Button>().interactable);

        private Controls m_playerInput;
        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += OnNavigate_performed;
            m_playerInput.UI.Cancel.performed += OnCancel_performed;
            m_playerInput.UI.MouseMoved.performed += OnMouseMoved_performed;
            m_playerInput.UI.Submit.performed += OnSubmit_performed;

            PanelEquipment.EquipActionPerformed += OnEquipAction_Performed;
            PanelEquipment.EquipActionCancelled += OnEquipAction_Cancelled;
        }
        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        public void Initialize(Item item, Slot slot, PlayableCharacter owner)
        {
            this.gameObject.SetActive(true);
            m_item = item;

            InitializeCharacterList();

            switch (item.Type)
            {
                case ItemType.Consumable:
                    InitializeUse();
                    m_panelActionType = MenuItemActionType.Use;
                    m_actionType = ActionType.ItemMenu;
                    break;
                case ItemType.Equipment:
                    if (slot == Slot.None)
                    {
                        InitializeEquip();
                        m_panelActionType = MenuItemActionType.Equip;
                        m_actionType = ActionType.Equip;
                    } else
                    {
                        m_owner = owner;
                    }
                    RefreshEquipmentPanel();
                    break;
            }
        }

        public void Close()
        {
            Clear();
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Opens this UI component with the item we want to perform actions on
        /// </summary>
        /// <param name="item">Selected Item</param>
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

            if (m_panelActionType == MenuItemActionType.Equip)
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

        public void Clear()
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
        }

        #region events
        private void OnMemberPlayerWidget_Updated(Enums.Attribute attribute)
        {
            Refresh();
            ItemInteractionPerformed(m_panelActionType, new List<Item> { m_item });
        }

        public void OnMember_Selected(UIActionSelection selection, GameObject member)
        {
            UseItem(member);
        }
        
        public void OnEquipAction_Performed(Slot slot)
        {
            m_playerInput.Enable();
            var character = m_currentSelectedPartyMember.GetComponent<UI_PartyMember>().GetCharacter();

            character.TryEquip(slot, (Equipment)m_item, out List<Item> changedItems);
            changedItems.Add(m_item);
            if (ItemInteractionPerformed != null)
            {
                ItemInteractionPerformed(MenuItemActionType.Equip, changedItems);
            }
        }

        private void OnEquipAction_Cancelled()
        {
            m_currentSelectedPartyMember.GetComponent<Button>().Select();
            m_playerInput.Enable();
        }
        #endregion

        #region private methods
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
            switch (m_panelActionType)
            {
                case MenuItemActionType.Use:
                    if (((Consumable)m_item).Effects.All(x => !x.EvaluateEffect(character.GetAttributes())))
                    {
                        return;
                    }

                    var action = new EntityAction(character, m_actionType, m_item.Id, new List<Entity>() { character }, m_inventoryManager);
                    StartCoroutine(action.Execute(m_partyManager, null, null));
                    break;
                case MenuItemActionType.Equip:
                    m_playerInput.Disable();
                    m_memberIndex = m_partyMembers.IndexOf(member);
                    PanelEquipment.GetComponent<UI_EquipmentSlots>().Open();
                    break;
            }
        }
        #endregion
    }
}
