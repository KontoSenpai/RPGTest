using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_Item : MonoBehaviour
    {
        [SerializeField] private Image ItemImage;
        [SerializeField] private TextMeshProUGUI ItemName;
        [SerializeField] private TextMeshProUGUI QuantityHeld;
        [SerializeField] private TextMeshProUGUI Owner;
        [SerializeField] private GameObject IsEquipped;

        [HideInInspector]
        public ItemSelectedHandler ItemSelected { get; set; }
        [HideInInspector]
        public delegate void ItemSelectedHandler(Item item);

        private Item m_item;
        private PlayableCharacter m_owner;
        private Slot m_slot;

        #region public Methods
        public void Initialize(Item item, int count)
        {
            m_item = item;
            Refresh(count);
        }

        public void Refresh(int count = 0)
        {
            ItemName.text = m_item.Name;
            if(m_owner != null)
            {
                QuantityHeld.text = "1";
            }
            else
            {
                QuantityHeld.text = count.ToString();
            }
        }

        public Item GetItem()
        {
            return m_item;
        }

        public void UseItem()
        {
            ItemSelected(m_item);
        }

        public void SetOWner(PlayableCharacter owner, Slot slot)
        {
            if(owner != null)
            {
                m_owner = owner;
                m_slot = slot;
                Owner.text = $"{m_owner.Name} - {m_slot.ToString()}";
                IsEquipped.SetActive(true);
            }
        }

        public Slot GetSlot()
        {
            if(GetOwner() != null)
            {
                return m_slot;
            }
            else
            {
                return Slot.None;
            }           
        }

        public PlayableCharacter GetOwner()
        {
            return m_owner;
        }

        public bool GetIsEquipped()
        {
            return GetSlot() != Slot.None;
        }
        #endregion
    }
}