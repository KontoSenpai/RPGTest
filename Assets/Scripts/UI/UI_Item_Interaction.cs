using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models.Items;
using RPGTest.Inputs;

namespace RPGTest.UI
{
    public enum InteractionType
    {
        Throw,
        Purchase,
        Sell
    }


    public class UI_Item_Interaction : MonoBehaviour
    {
        public Button OkButton;
        public TextMeshProUGUI Label_ButtonText;
        public TextMeshProUGUI Label_ItemName;
        public TextMeshProUGUI Label_PurchaseQuantity;
        public TextMeshProUGUI Label_MoneyCost;
        public Image Image_Currency;

        private Item m_item;
        private InteractionType m_interactionType;

        private int m_interactQuantity { get; set; }
        private int m_maxQuantity { get; set; }

        [HideInInspector]
        public CancelActionHandler ItemInteractionCancelled { get; set; }
        public ItemInteractionHandler ItemInteractionRequested { get; set; }
        [HideInInspector]
        public delegate void ItemInteractionHandler(Item item, int quantity);

        private InventoryManager m_inventoryManager => FindObjectOfType<InventoryManager>();

        private Controls m_playerInput;
        public virtual void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
            m_playerInput.UI.Cancel.performed += Cancel_performed;
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();


        private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.ItemInteractionCancelled();
        }

        private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var movement = obj.ReadValue<Vector2>();
            if (movement.x < 0)
            {
                TryChangePurchaseQuantity(-1);
            }
            else if (movement.x > 0)
            {
                TryChangePurchaseQuantity(1);
            }
        }

        public void Initialize(InteractionType type, Item item)
        {
            var inventoryManager = FindObjectOfType<GameManager>().InventoryManager;

            m_interactQuantity = 0;
            m_item = item;
            m_interactionType = type;
            switch (m_interactionType)
            {
                case InteractionType.Purchase:
                    m_maxQuantity = inventoryManager.GetRemainingItemSlots(m_item.Id);
                    Image_Currency.enabled = true;
                    Label_MoneyCost.enabled = true;
                    break;
                case InteractionType.Sell:
                    m_maxQuantity = inventoryManager.GetHeldItemQuantity(m_item.Id);
                    Image_Currency.enabled = true;
                    Label_MoneyCost.enabled = true;
                    break;
                case InteractionType.Throw:
                    m_maxQuantity = inventoryManager.GetHeldItemQuantity(m_item.Id);
                    Image_Currency.enabled = false;
                    Label_MoneyCost.enabled = false;
                    break;
            };
            OkButton.Select();

            Label_ItemName.text = m_item.Name;
            Label_PurchaseQuantity.text = m_interactQuantity.ToString();
            Label_MoneyCost.text = (m_interactQuantity * m_item.Value).ToString();
        }

        public void onClickChangeQuantity(int variation)
        {
            TryChangePurchaseQuantity(1);
        }

        public void onClickOk()
        {
            ItemInteractionRequested(m_item, m_interactQuantity);
            gameObject.SetActive(false);
        }

        public bool TryChangePurchaseQuantity(int variation)
        {
            if (m_interactQuantity + variation < 0 || m_interactQuantity + variation > m_maxQuantity)
            {
                return false;
            }

            switch (m_interactionType)
            {
                case InteractionType.Throw:
                    m_interactQuantity += variation;
                    Label_PurchaseQuantity.text = m_interactQuantity.ToString();
                    return true;
                case InteractionType.Purchase:
                    if (m_inventoryManager.Money >= m_item.Value * (m_interactQuantity + variation))
                    {
                        m_interactQuantity += variation;
                        Label_PurchaseQuantity.text = m_interactQuantity.ToString();
                        Label_MoneyCost.text = (m_interactQuantity * m_item.Value).ToString();
                        return true;
                    }
                    return false;
                case InteractionType.Sell:
                    m_interactQuantity += variation;
                    Label_PurchaseQuantity.text = m_interactQuantity.ToString();
                    Label_MoneyCost.text = (m_interactQuantity * m_item.Value).ToString();
                    return true;
            }

            if ((m_interactQuantity + variation >= 0 && m_interactQuantity + variation <= m_maxQuantity) &&
                m_inventoryManager.Money >= m_item.Value * (m_interactQuantity + variation))
            {
                m_interactQuantity += variation;
                Label_PurchaseQuantity.text = m_interactQuantity.ToString();
                Label_MoneyCost.text = (m_interactQuantity * m_item.Value).ToString();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class PurchaseEventArgs
    {
        public Item PurchaseItem { get; private set; }
        public int PurchaseQuantity { get; private set; }

        public PurchaseEventArgs(Item item, int quantity)
        {
            PurchaseItem = item;
            PurchaseQuantity = quantity;
        }
    }
}
