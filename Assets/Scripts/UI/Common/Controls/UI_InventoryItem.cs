using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.UI.Common
{
    public class ItemSelectionConfirmedEventArgs : EventArgs
    {
        public ItemSelectionConfirmedEventArgs(Item item, PresetSlot preset = PresetSlot.None, Slot slot = Slot.None, PlayableCharacter owner = null)
        {
            Item = item;

            Preset = preset;
            Slot = slot;
            Owner = owner;
        }

        public Item Item { get; }

        public PresetSlot Preset { get; }
        public Slot Slot { get; }
        public PlayableCharacter Owner { get; }
    }

    public class PendingItemSelection
    {
        public Item Item { get; set; }

        public PresetSlot Preset { get; set; }
        public Slot Slot { get; set; }
        public PlayableCharacter Owner { get; set; }
    }

    public class UI_InventoryItem : MonoBehaviour
    {
        [HideInInspector]
        public event EventHandler<ItemSelectionConfirmedEventArgs> ItemSelectionConfirmed;

        public PresetSlot Preset = PresetSlot.None;
        public Slot Slot = Slot.None;

        [HideInInspector]
        public Item Item { get; private set; }
        [HideInInspector]
        public int Quantity { get; private set; } = -1;
        [HideInInspector]
        public List<PlayableCharacter> Owners { get; set; } = new List<PlayableCharacter>();

        [SerializeField] protected Image ItemImage;
        [SerializeField] protected Image ItemTypeImage;
        [SerializeField] protected TextMeshProUGUI ItemName;
        [SerializeField] protected TextMeshProUGUI QuantityHeld;

        [SerializeField] private UI_ItemOwner OwnersPanel;

        [SerializeField] private bool DisplayQuantity;

        [SerializeField]
        private GameObject m_statsPanel;
        [SerializeField]
        private GameObject m_statGo;

        private List<GameObject> m_instantiatedStatGo = new List<GameObject>();

        #region public Methods
        /// <summary>
        /// Default initializer
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        public void Initialize(Item item, int count)
        {
            InitializeInternal(item, count);
        }

        /// <summary>
        /// Initialize for single Owner
        /// </summary>
        /// <param name="item"></param>
        /// <param name="owner"></param>
        /// <param name="preset"></param>
        /// <param name="slot"></param>
        public void InitializeForOwner(Item item, PlayableCharacter owner, PresetSlot preset, Slot slot)
        {
            Owners = new List<PlayableCharacter>() { owner };
            Preset = preset;
            Slot = slot;

            InitializeInternal(item, 1);
        }

        /// <summary>
        /// Initialize for multiple Owners
        /// </summary>
        /// <param name="item"></param>
        /// <param name="owners"></param>
        public void InitializeForOwners(Item item, List<PlayableCharacter> owners)
        {
            Owners = owners;
            InitializeInternal(item, owners.Count);
        }

        public void UpdateHeldQuantity(int count)
        {
            Quantity = count > 0 ? count : -1;
            QuantityHeld.text = DisplayQuantity ? $"X {Quantity}" : String.Empty;
        }

        public PlayableCharacter GetOwner()
        {
            return Owners?.Count >= 1 ? Owners[0] : null;
        }

        public void SetOwners(List<PlayableCharacter> owners)
        {
            Owners = owners;
            UpdateHeldQuantity(owners.Count);
        }

        public bool GetIsEquipped()
        {
            return Slot != Slot.None;
        }

        public void PreviewItem(Item item)
        {
            RefreshItemDisplayInformation(item ?? Item);
        }

        public void RefreshItemDisplayInformation(Item item)
        {
            Clean();
            if (item == null)
            {
                return;
            }
            ItemName.text = item.Name;

            // TODO : IconLibrary/Bank or a Resource.Load based on the item name
            // ItemImage.sprite = m_equipedItem.
            // ItemTypeImage.sprite = m_equipedItem

            if (m_statsPanel && m_statsPanel.activeSelf)
            {
                InitializeItemStats(item);
            }
            if (OwnersPanel != null)
            {
                OwnersPanel.SetOwnerDisplay(Owners, Preset, Slot);
            }
        }

        /// <summary>
        /// Clean the UI to a blank state, with the least amount of information displayed on the control
        /// </summary>
        public void Clean()
        {
            ItemName.text = String.Empty;
            if (ItemImage != null)
            {
                ItemImage.enabled = false;
            }
            if (ItemTypeImage != null)
            {
                ItemTypeImage.gameObject.transform.parent.gameObject.SetActive(false);
            }

            m_instantiatedStatGo.ForEach((g) => Destroy(g));
            m_instantiatedStatGo.Clear();
        }
        #endregion

        #region Events
        // Select for swap
        public virtual void OnItem_Selected()
        {
            ItemSelectionConfirmed(this, new ItemSelectionConfirmedEventArgs(Item));
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Set the value in all appropriate components
        /// </summary>
        private void InitializeInternal(Item item, int quantity)
        {
            Item = item;
            UpdateHeldQuantity(quantity);
            Clean();

            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() =>
            {
                ItemSelectionConfirmed(this, new ItemSelectionConfirmedEventArgs(Item, Preset, Slot, GetOwner()));
            });

            RefreshItemDisplayInformation(item);
        }

        private void InitializeItemStats(Item item)
        {
            switch (item.Type)
            {
                case ItemType.Consumable:
                    var consummable = (Consumable)item;
                    // TODO : attributes
                    break;
                case ItemType.Equipment:
                    var equipment = (Equipment)item;
                    foreach (KeyValuePair<Attribute, int> attribute in equipment.Attributes)
                    {
                        GameObject instantiatedObject = Instantiate(m_statGo);
                        instantiatedObject.GetComponent<UI_StatWidget>().Refresh(attribute.Key.GetDescription(), attribute.Value, true);
                        instantiatedObject.transform.SetParent(m_statsPanel.transform);
                        instantiatedObject.transform.localScale = new Vector3(1, 1, 1);
                        m_instantiatedStatGo.Add(instantiatedObject);
                    }
                    break;
            }
        }
        #endregion
    }
}