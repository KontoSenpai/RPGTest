using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.Modules.Battle.Action;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_UseControl : MonoBehaviour
    {
        public GameObject PanelList;
        public GameObject PanelListContent;
        public GameObject ItemInstantiate;

        [SerializeField]
        private GameObject m_PanelEquipment;

        [HideInInspector]
        public CancelActionHandler ItemInteractionCancelled { get; set; }
        [HideInInspector]
        public ActionFinshedHandler ActionFinished { get; set; }
        [HideInInspector]
        public delegate void ActionFinshedHandler(MenuItemActionType actionType, List<Item> items);


        private Item m_item;

        private MenuItemActionType m_panelActionType;
        private ActionType m_actionType;

        private List<GameObject> m_InstantiatedItems;


        private bool m_actionInProgress = false;
        private int m_memberIndex;


        //Equip Specific
        private Slot m_selectedSlot;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;
        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        public bool AnyApplicableMember => m_InstantiatedItems.Count > 0 && m_InstantiatedItems.All(x => x.GetComponent<Button>().interactable);

        private Controls m_playerInput;
        public virtual void Awake()
        {
            Debug.Log("awake");
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
            m_playerInput.UI.Cancel.performed += Cancel_performed;
            m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().SlotSelected += onEquipmentSlotSelected;
        }
        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        public void InitializePanel(MenuItemActionType type, Item selectedItem)
        {
            m_panelActionType = type;
            m_InstantiatedItems = new List<GameObject>();
            m_memberIndex = 0;
            m_item = selectedItem;

            if (m_panelActionType == MenuItemActionType.Use)
                m_actionType = ActionType.ItemMenu;
            else if (m_panelActionType == MenuItemActionType.Equip)
            {
                m_actionType = ActionType.Equip;
                m_PanelEquipment.gameObject.SetActive(true);
                m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().Initialize(m_item as Equipment);
            }             

            foreach (var member in m_partyManager.GetAllExistingPartyMembers())
            {
                var uiMember = CreateInstantiateItem(ItemInstantiate);
                var uiMemberScript = uiMember.GetComponent<UI_Member_Widget>();
                uiMemberScript.Initialize(member);
                member.PlayerWidgetUpdated += Member_PlayerWidgetUpdated;

                uiMemberScript.MemberSelected += Member_Selected;
                m_InstantiatedItems.Add(uiMember);
            }

            //Expliciting navigation
            foreach (var member in m_InstantiatedItems)
            {
                var index = m_InstantiatedItems.IndexOf(member);

                member.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? m_InstantiatedItems[index - 1].GetComponent<Button>() : null,
                    Down: index < m_InstantiatedItems.Count - 1 ? m_InstantiatedItems[index + 1].GetComponent<Button>() : null);
            }

            Resize();
        }

        public void Focus()
        {
            Refresh();

            var instantiatedItem = m_InstantiatedItems.FirstOrDefault(x => x.GetComponent<Button>().interactable);

            if (instantiatedItem != null)
            {
                instantiatedItem.GetComponent<Button>().Select();
            }
            else
            {
                m_InstantiatedItems.FirstOrDefault().GetComponent<Button>().Select();
            }

            if (m_actionType == ActionType.Equip)
            {
                RefreshEquipmentPanel();
            }
        }


        public void Clean()
        {
            m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().Clean();
            this.m_PanelEquipment.gameObject.SetActive(false);

            if (m_InstantiatedItems != null)
            {
                foreach (var item in m_InstantiatedItems)
                {
                    item.GetComponent<UI_Member_Widget>().MemberSelected -= Member_Selected;
                    Destroy(item);
                }
                m_InstantiatedItems.Clear();
            }

            foreach (var member in m_partyManager.GetAllExistingPartyMembers())
            {
                member.PlayerWidgetUpdated -= Member_PlayerWidgetUpdated;
            }
        }


        #region event handlers
        private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var movement = obj.ReadValue<Vector2>();
            if (!m_actionInProgress)
            {
                if (movement.y > 0 && m_memberIndex > 0)
                {
                    m_memberIndex -= 1;
                }
                else if (movement.y < 0 && m_memberIndex < m_InstantiatedItems.Count - 1)
                {
                    m_memberIndex += 1;
                }

                if(m_panelActionType == MenuItemActionType.Equip)
                {
                    RefreshEquipmentPanel();
                }
            }
        }

        private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CancelAction();
        }
        private void Equip_Cancelled()
        {
            CancelAction();
        }

        private void Member_PlayerWidgetUpdated(Enums.Attribute attribute)
        {
            Refresh();
            ActionFinished(m_panelActionType, new List<Item> { m_item });
        }

        public void Member_Selected(GameObject member)
        {
            var character = member.GetComponent<UI_Member_Widget>().GetCharacter();
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
                    if (m_actionInProgress == false)
                    {
                        m_memberIndex = m_InstantiatedItems.IndexOf(member);
                        m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().Focus();
                        m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().ItemInteractionCancelled += Equip_Cancelled;
                        m_actionInProgress = true;
                    }
                    break;
            }
        }
        public void onEquipmentSlotSelected(Slot slot)
        {
            m_actionInProgress = false;
            var character = m_InstantiatedItems[m_memberIndex].GetComponent<UI_Member_Widget>().GetCharacter();

            character.TryEquip(slot, (Equipment)m_item, out List<Item> removedItems);
            removedItems.Add(m_item);
            ActionFinished(MenuItemActionType.Equip, removedItems);
        }
        #endregion

        #region private methods
        private void Resize()
        {
            var rect = this.GetComponent<RectTransform>();

            float width = 0.0f;
            Vector2 toto = new Vector2();
            toto.y = rect.sizeDelta.y;

            var listRect = this.PanelList.GetComponent<RectTransform>();
            width = listRect.sizeDelta.x;

            if (m_PanelEquipment.gameObject.activeSelf)
            {
                var listEquip = this.m_PanelEquipment.GetComponent<RectTransform>();
                width += listEquip.sizeDelta.x;
            }
            toto.x = width;
            rect.sizeDelta = toto;
        }

        /// <summary>
        /// Will refresh all instantiated components
        /// </summary>
        private void Refresh()
        {
            m_memberIndex = 0;
            foreach (var item in m_InstantiatedItems)
            {
                item.GetComponent<UI_Member_Widget>().Refresh();
            }
        }

        private void RefreshEquipmentPanel()
        {
            var character = m_InstantiatedItems[m_memberIndex].GetComponent<UI_Member_Widget>().GetCharacter();
            m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().UpdateContent(character);
        }

        private GameObject CreateInstantiateItem(GameObject prefab)
        {
            var uiMember = Instantiate(prefab);
            ItemInstantiate.GetComponent<RectTransform>();
            uiMember.transform.SetParent(PanelListContent.transform);
            uiMember.transform.localScale = new Vector3(1, 1, 1);
            return uiMember;
        }

        private void CancelAction()
        {
            if (m_actionInProgress)
            {
                m_PanelEquipment.GetComponent<UI_SubMenu_Inventory_Equipment>().ItemInteractionCancelled -= Equip_Cancelled;
                m_InstantiatedItems[m_memberIndex].GetComponent<Button>().Select();
                m_actionInProgress = false;
            }
            else
            {
                this.ItemInteractionCancelled();
            }
        }
        #endregion
    }
}
