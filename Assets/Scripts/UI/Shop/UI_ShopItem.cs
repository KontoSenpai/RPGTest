using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Items;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_ShopItem : MonoBehaviour
    {
        public GameObject PurchaseWindow;
        public Text UI_ItemName;
        public Text UI_ItemType;
        public Text UI_ItemPrice;
        public Text UI_HeldItem;

        public event Action<ItemClickedEventArgs> ItemClicked = delegate { };

        private Item m_item;

        private int m_held = 0;
        private int m_purchase_QTY = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize(Item item, int held)
        {
            m_item = item;

            UI_ItemName.text = m_item.Name;

            switch(m_item.Type)
            {
                case ItemType.Equipment:
                    UI_ItemType.text = ((Equipment)m_item).EquipmentType.ToString();
                    break;
                case ItemType.Consumable:
                    UI_ItemType.text = ((Consumable)m_item).ConsumableType.ToString();
                    break;
                default:
                    UI_ItemType.text = string.Empty;
                    break;
            }

            UI_ItemPrice.text = m_item.Value.ToString();

            UpdateHeldQuantity(held);
        }

        /// <summary>
        /// Set visibility of the item depending of the selected filter.
        /// </summary>
        /// <param name="filter">Selected Item Type to filter</param>
        public void SetVisibility(ItemType filter)
        {
            if(filter == ItemType.None)
            {               
                this.enabled = true;
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(m_item.Type == filter);
            }
        }

        public string GetItemId()
        {
            return m_item.Id;
        }

        public void UpdateHeldQuantity(int newQuantity)
        {
            m_held = newQuantity;
            UI_HeldItem.text = m_held.ToString();
        }

        public int GetPurchasedQuantity()
        {
            return m_purchase_QTY;
        }

        /// <summary>
        /// Get the total price of the selected item
        /// ie. purchase quantity * item value
        /// </summary>
        /// <returns></returns>
        public int GetTotalPrice()
        {
            return m_purchase_QTY * m_item.Value;
        }

        public void onClickShopItem()
        {
            ItemClicked(new ItemClickedEventArgs( this.gameObject, m_item));
        }
    }

    public class ItemClickedEventArgs
    {
        public GameObject UIItem { get; private set; }

        public Item Item { get; private set; }

        public ItemClickedEventArgs(GameObject uiitem, Item item)
        {
            UIItem = uiitem;

            Item = item;
        }
    }
}