using RPGTest.Enums;
using RPGTest.Inputs;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class ItemSelectionConfirmedEventArgs : EventArgs
    {
        public ItemSelectionConfirmedEventArgs(UIActionSelection selection, Item item, Slot slot, PlayableCharacter owner)
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
        public event EventHandler<ItemSelectionConfirmedEventArgs> ItemSelectionConfirmed;

        private Item m_item;

        private List<PlayableCharacter> m_owners = new List<PlayableCharacter>();
        private PresetSlot m_preset;
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
            QuantityHeld.text = count.ToString();
        }

        #region Input Events
        // Select for swap
        public void PrimaryAction_Selected()
        {
            ItemSelectionConfirmed(this, new ItemSelectionConfirmedEventArgs(UIActionSelection.Primary, m_item, m_slot, m_owners.Count > 1 ? m_owners[0] : null));
        }

        // Select for SubMenu
        public void SecondaryAction_Selected()
        {
            ItemSelectionConfirmed(this, new ItemSelectionConfirmedEventArgs(UIActionSelection.Secondary, m_item, m_slot, m_owners.Count > 1 ? m_owners[0] : null));
        }
        #endregion

        public Item GetItem()
        {
            return m_item;
        }

        public void SetOwners(List<PlayableCharacter> owners)
        {
            m_owners = owners;
        }

        public List<PlayableCharacter> GetOwners()
        {
            return m_owners;
        }

        public void SetOwner(PlayableCharacter owner, PresetSlot preset, Slot slot)
        {
            if(owner != null)
            {
                m_owners = new List<PlayableCharacter> { owner };
                m_preset = preset;
                m_slot = slot;
                Owner.text = $"{m_owners[0].Name}";
                IsEquipped.SetActive(true);
            }
        }

        public PlayableCharacter GetOwner()
        {
            return m_owners.Count >= 1 ? m_owners[0] : null;
        }

        public PresetSlot GetPreset()
        {
            if (GetOwner() != null)
            {
                return m_preset;
            }
            return PresetSlot.None;
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


        public bool GetIsEquipped()
        {
            return GetSlot() != Slot.None;
        }
        #endregion
    }
}