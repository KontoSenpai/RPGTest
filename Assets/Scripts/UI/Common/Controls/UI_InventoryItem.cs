using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
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
        public ItemSelectionConfirmedEventArgs(Item item, PresetSlot preset = PresetSlot.None, EquipmentSlot slot = EquipmentSlot.None, PlayableCharacter owner = null)
        {
            Item = item;

            Preset = preset;
            Slot = slot;
            Owner = owner;
        }

        public Item Item { get; }

        public PresetSlot Preset { get; }
        public EquipmentSlot Slot { get; }
        public PlayableCharacter Owner { get; }
    }

    public class PendingItemSelection
    {
        public Item Item { get; set; }

        public PresetSlot Preset { get; set; }
        public EquipmentSlot Slot { get; set; }
        public PlayableCharacter Owner { get; set; }
    }

    public class UI_InventoryItem : MonoBehaviour
    {
        [HideInInspector]
        public event EventHandler<ItemSelectionConfirmedEventArgs> ItemSelectionConfirmed;

        public PresetSlot Preset = PresetSlot.None;
        public EquipmentSlot Slot = EquipmentSlot.None;

        [HideInInspector]
        public Item Item { get; private set; }
        [HideInInspector]
        public int Quantity { get; private set; } = -1;
        [HideInInspector]
        public List<PlayableCharacter> Owners { get; set; } = new List<PlayableCharacter>();

        [SerializeField] protected Image ItemImage;
        [SerializeField] protected Image ItemTypeImage;
        [SerializeField] protected TextMeshProUGUI ItemName;

        [SerializeField] private bool DisplaySlot;
        [SerializeField] private GameObject SlotInformations;

        [SerializeField]
        private GameObject m_statsPanel;

        [SerializeField]
        private GameObject m_attributePillGo;

        [SerializeField]
        private GameObject m_resistancePillGo;

        [SerializeField]
        private GameObject m_effectPillGo;

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
        public void InitializeForOwner(Item item, PlayableCharacter owner, PresetSlot preset, EquipmentSlot slot)
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
            UpdateHeldQuantityInternal();
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
            return Slot != EquipmentSlot.None;
        }

        public void PreviewItem(Item item)
        {
            RefreshItemDisplayInformation(item ?? Item);
        }

        public void RefreshItemDisplayInformation(Item item)
        {
            Clean();

            SetOwnershipDisplayInternal();

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
        }

        /// <summary>
        /// Clean the UI to a blank state, with the least amount of information displayed on the control
        /// </summary>
        public void Clean()
        {
            ItemName.text = string.Empty;
            if (ItemImage != null)
            {
                ItemImage.enabled = false;
            }
            if (ItemTypeImage != null)
            {
                ItemTypeImage.gameObject.transform.parent.gameObject.SetActive(false);
            }

            var pills = GetComponentsInChildren<UI_Control_StatPill>();

            pills.ForEach((g) => Destroy(g.gameObject));
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
                        GameObject instantiatedObject = Instantiate(m_attributePillGo);
                        instantiatedObject.GetComponent<UI_Control_StatPillAttribute>().Initialize(attribute.Key, attribute.Value, true);
                        instantiatedObject.transform.SetParent(m_statsPanel.transform);
                        instantiatedObject.transform.localScale = new Vector3(1, 1, 1);
                    }
                    foreach (KeyValuePair<Element, float> element in equipment.ElementalResistances)
                    {
                        if (element.Key == Element.None)
                        {
                            continue;
                        }
                        GameObject instantiatedObject = Instantiate(m_resistancePillGo);
                        instantiatedObject.GetComponent<UI_Control_StatPillResistance>().Initialize(element.Key, Mathf.FloorToInt(element.Value * 100));
                        instantiatedObject.transform.SetParent(m_statsPanel.transform);
                        instantiatedObject.transform.localScale = new Vector3(1, 1, 1);
                    }
                    foreach (string effect in equipment.Effects)
                    {
                        GameObject instantiatedObject = Instantiate(m_effectPillGo);
                        instantiatedObject.GetComponent<UI_Control_StatPillEffect>().Initialize(EffectsCollector.TryGetEffect(effect));
                        instantiatedObject.transform.SetParent(m_statsPanel.transform);
                        instantiatedObject.transform.localScale = new Vector3(1, 1, 1);
                    }
                    break;
            }
        }

        private void UpdateHeldQuantityInternal()
        {
            var ownershipComponent = GetComponentInChildren<UI_ItemOwnership>();
            if (ownershipComponent != null)
            {
                ownershipComponent.SetHeldQuantity(Owners.Any(), Quantity);
            }
        }

        private void SetOwnershipDisplayInternal()
        {
            var ownershipComponent = GetComponentInChildren<UI_ItemOwnership>();
            if (ownershipComponent != null)
            {
                ownershipComponent.SetOwnershipDisplay(Owners, Preset, Slot);
            }
        }
        #endregion
    }
}