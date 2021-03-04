using MyBox;
using RPGTest.Inputs;
using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RPGTest.Helpers;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_ActionSelection : MonoBehaviour
    {
        [Separator("Buttons")]
        public Button ActionButton;
        public Button ThrowButton;
        public TextMeshProUGUI ActionString;
        public GameObject UsePanel;
        public GameObject ThrowPanel;
        public GameObject UsePanelListContent;
        [Separator("Prefabs to instantiate")]
        public GameObject UseItemInstantiate;
        public GameObject EquipItemInstantiate;

        [HideInInspector]
        public ItemUsedHandler ItemActionSelected { get; set; }
        [HideInInspector]
        public delegate void ItemUsedHandler(MenuItemActionType actionType, List<Item> items);

        private Item m_item;

        private PlayableCharacter m_owner;
        private Slot m_slot;

        private MenuItemActionType m_actionType;
        private ActionType m_action;
        
        private Controls m_playerInput;

        private bool m_inSubMenu = false;
        private int m_memberIndex;
        private bool m_actionInProgress = false;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        #region public Methods
        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Cancel.performed += OnCancelPerformed;
        }


        private List<GameObject> m_InstantiatedItems;

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        /// <summary>
        /// Opens this UI component with the item we want to perform actions on
        /// </summary>
        /// <param name="item">Selected Item</param>
        public void InitializeUse(Item item)
        {
            m_inSubMenu = false;
            m_item = item;

            m_actionType = MenuItemActionType.Use;
            ActionString.text = "Use";

            InitializeGui();
        }

        public void InitializeEquip(Item item)
        {
            m_inSubMenu = false;
            m_item = item;

            m_actionType = MenuItemActionType.Equip;
            ActionString.text = "Equip";

            InitializeGui();
        }

        public void InitializeUnequip(Item item, Slot slot, PlayableCharacter owner)
        {
            m_inSubMenu = false;
            m_item = item;
            m_slot = slot;
            m_owner = owner;

            m_actionType = MenuItemActionType.Unequip;
            ActionString.text = "Unequip";

            InitializeGui();
        }

        private void InitializeGui()
        {
            InitializeUsePanel();
            ActionButton.Select();

            if (m_InstantiatedItems.All(x => !x.GetComponent<Button>().interactable))
            {
                ActionButton.interactable = false;
                var nav = ThrowButton.navigation;
                nav.selectOnUp = null;
                ThrowButton.navigation = nav;
                ThrowButton.Select();
            }

            UsePanel.SetActive(false);
            ThrowPanel.SetActive(false);

            ThrowPanel.GetComponent<UI_ItemInteraction>().ItemInteractionRequested += OnItemInteractionRequested;
        }

        /// <summary>
        /// Will initiate the closing of this UI component
        /// </summary>
        public void Close()
        {
            if (m_InstantiatedItems != null)
            {
                foreach (var item in m_InstantiatedItems)
                {
                    item.GetComponent<UI_Member_Widget>().MemberSelected -= onMemberSelected;
                    Destroy(item);
                }
                m_InstantiatedItems.Clear();
            }

            foreach (var member in m_partyManager.GetAllPartyMembers())
            {
                member.PlayerWidgetUpdated -= Member_PlayerWidgetUpdated;
            }

            ThrowPanel.GetComponent<UI_ItemInteraction>().ItemInteractionRequested -= OnItemInteractionRequested;
            gameObject.SetActive(false);
        }

        #endregion


        #region private Methods
        private void InitializeUsePanel()
        {
            m_InstantiatedItems = new List<GameObject>();
            if (m_actionType == MenuItemActionType.Use)
            {
                m_action = ActionType.ItemMenu;
                foreach (var member in m_partyManager.GetAllPartyMembers())
                {
                    var uiMember = CreateInstantiateItem(UseItemInstantiate);

                    var uiMemberScript = uiMember.GetComponent<UI_Member_Widget>();
                    uiMemberScript.Initialize(member);
                    member.PlayerWidgetUpdated += Member_PlayerWidgetUpdated;
                    uiMemberScript.MemberSelected += onMemberSelected;

                    m_InstantiatedItems.Add(uiMember);
                    
                    if (((Consumable)m_item).Effects.All(x => !x.EvaluateEffect(member.GetAttributes())))
                    {
                        uiMember.GetComponent<Button>().interactable = false;
                    }
                }
            }
            else
            {
                m_action = ActionType.Equip;
                foreach (var member in m_partyManager.GetAllPartyMembers())
                {
                    var uiMember = CreateInstantiateItem(EquipItemInstantiate);

                    var uiMemberScript = uiMember.GetComponent<UI_Member_Widget>();
                    uiMemberScript.Initialize(member);
                    member.PlayerWidgetUpdated += Member_PlayerWidgetUpdated;
                    uiMemberScript.MemberSelected += onMemberSelected;
                    uiMemberScript.EquipmentSlotSelected += onEquipmentSlotSelected;

                    m_InstantiatedItems.Add(uiMember);
                }
            }

            //Expliciting navigation
            foreach (var member in m_InstantiatedItems)
            {
                var index = m_InstantiatedItems.IndexOf(member);

                member.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? m_InstantiatedItems[index - 1].GetComponent<Button>() : null,
                    Down: index < m_InstantiatedItems.Count - 1 ? m_InstantiatedItems[index + 1].GetComponent<Button>() : null);
            }
        }

        private GameObject CreateInstantiateItem(GameObject prefab)
        {
            var uiMember = Instantiate(prefab);
            UseItemInstantiate.GetComponent<RectTransform>();
            uiMember.transform.SetParent(UsePanelListContent.transform);
            uiMember.transform.localScale = new Vector3(1, 1, 1);
            return uiMember;
        }

        private void EvaluateItemValidity()
        {

        }

        /// <summary>
        /// Will refresh all instantiated components
        /// </summary>
        private void Refresh()
        {
            foreach(var item in m_InstantiatedItems)
            {
                item.GetComponent<UI_Member_Widget>().Refresh();
            }
        }
        #endregion

        #region Events
        private void OnCancelPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            onClickCancel();
        }

        public void onClickOk()
        {
            switch(m_actionType)
            {
                case MenuItemActionType.Use:
                case MenuItemActionType.Equip:
                    Refresh();
                    UsePanel.SetActive(true);

                    var instantiatedItem = m_InstantiatedItems.FirstOrDefault(x => x.GetComponent<Button>().interactable);
                    if (instantiatedItem != null)
                    {
                        instantiatedItem.GetComponent<Button>().Select();
                    }
                    else
                    {
                        m_InstantiatedItems.FirstOrDefault().GetComponent<Button>().Select();
                    }

                    var rectTransform = m_InstantiatedItems.FirstOrDefault().GetComponent<RectTransform>();
                    var height = rectTransform.sizeDelta.y * m_InstantiatedItems.Count;

                    UsePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);

                    var rect = UsePanel.GetComponent<RectTransform>();
                    rect.position = new Vector3(Screen.width / 2, Screen.height / 2, 10);
                    m_inSubMenu = true;
                    break;
                case MenuItemActionType.Unequip:
                    m_owner.TryUnequip(m_slot, out List<Item> removedItems);
                    ItemActionSelected(m_actionType, removedItems);
                    break;
            }

        }

        private void Member_PlayerWidgetUpdated(Enums.Attribute attribute)
        {
            Refresh();
            if (m_inventoryManager.GetHeldItemQuantity(m_item.Id) == 0)
            {
                Close();
            }
        }

        public void onClickThrow()
        {
            ThrowPanel.SetActive(true);
            ThrowPanel.GetComponent<UI_ItemInteraction>().Initialize(InteractionType.Throw, m_item);
            m_inSubMenu = true;
        }

        public void onClickCancel()
        {
            if(m_inSubMenu)
            {
                if(m_actionInProgress)
                {
                    m_InstantiatedItems[m_memberIndex].GetComponent<Button>().Select();
                    m_actionInProgress = false;
                }
                else
                {
                    if (UsePanel.activeInHierarchy)
                    {
                        ActionButton.GetComponent<Button>().Select();
                        UsePanel.SetActive(false);
                    }
                    else if (ThrowPanel.activeInHierarchy)
                    {
                        ThrowButton.GetComponent<Button>().Select();
                        ThrowPanel.SetActive(false);
                    }

                    m_inSubMenu = false;
                }
            }
            else
            {
                ItemActionSelected(MenuItemActionType.Cancel, null);
                Close();
            }
        }

        public void OnItemInteractionRequested(Item item, int quantity)
        {
            m_inventoryManager.RemoveItem(item.Id, quantity);
            ItemActionSelected(MenuItemActionType.Throw, new List<Item>{ m_item });
            Close();
        }

        public void onMemberSelected(GameObject member)
        {
            var character = member.GetComponent<UI_Member_Widget>().GetCharacter();
            switch (m_actionType)
            {
                case MenuItemActionType.Use:
                    var action = new EntityAction(character, m_action, m_item.Id, new List<Entity>() { character }, m_inventoryManager);
                    StartCoroutine(action.Execute(null,null));
                    ItemActionSelected(m_actionType, new List<Item> { m_item });
                    break;
                case MenuItemActionType.Equip:
                    if (((Equipment)m_item).IsWeapon)
                    {
                        if (m_actionInProgress == false)
                        {
                            m_memberIndex = m_InstantiatedItems.IndexOf(member);
                            m_actionInProgress = true;
                        }
                    }
                    else
                    {
                        character.TryEquip(m_slot, (Equipment)m_item, out List<Item> removedItems);
                        removedItems.Add(m_item);
                        ItemActionSelected(m_actionType, removedItems);
                    }
                    break;
            }
        }

        public void onEquipmentSlotSelected(GameObject member, Slot slot)
        {
            var character = member.GetComponent<UI_Member_Widget>().GetCharacter();

            character.TryEquip(slot, (Equipment)m_item, out List<Item> removedItems);
            removedItems.Add(m_item);
            ItemActionSelected(MenuItemActionType.Equip, removedItems);
        }
        #endregion
    }
}
