using RPGTest.Enums;
using RPGTest.Inputs;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class ItemUsedEventArgs : EventArgs
    {
        public ItemUsedEventArgs(UIActionSelection selection, Item item, Slot slot, PlayableCharacter owner)
        {
            Selection = selection;
            Item = item;
            Slot = slot;
            Owner = owner;
        }

        public UIActionSelection Selection { get; }
        public Item Item { get; }
        public Slot Slot { get; }
        public PlayableCharacter Owner { get; }
    }

    public class UI_InventoryItem : AdvancedButton
    {
        [SerializeField] private Image ItemImage;
        [SerializeField] private TextMeshProUGUI ItemName;
        [SerializeField] private TextMeshProUGUI QuantityHeld;
        [SerializeField] private TextMeshProUGUI Owner;
        [SerializeField] private GameObject IsEquipped;

        [HideInInspector]
        public event EventHandler<ItemUsedEventArgs> ItemUsed;

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

        #region Input Events
        // Select for swap
        public void PrimaryAction_Selected()
        {
            ItemUsed(this, new ItemUsedEventArgs(UIActionSelection.Primary, m_item, m_slot, m_owner));
        }

        // Select for SubMenu
        public void SecondaryAction_Selected()
        {
            ItemUsed(this, new ItemUsedEventArgs(UIActionSelection.Secondary, m_item, m_slot, m_owner));
        }
        #endregion

        public Item GetItem()
        {
            return m_item;
        }

        public void UseItem()
        {
            ItemUsed(this, new ItemUsedEventArgs(UIActionSelection.Primary, m_item, m_slot, m_owner));
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