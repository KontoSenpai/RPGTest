using System;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Items;
using TMPro;
using Unity.Extensions.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    /// <summary>
    /// Generic Script to help with the display of an equipment information
    /// </summary>
    public class UI_EquipmentSlot: MonoBehaviour
    {
        public PresetSlot Preset;
        public Slot Slot;

        [SerializeField]
        private ButtonOverride m_equipmentButton;
        [SerializeField]
        private Image m_EquipmentIcon;
        [SerializeField]
        private TextMeshProUGUI m_EquipmentName;
        [SerializeField]
        private GameObject m_statsPanel;

        private Equipment m_equipedItem;
        private Equipment m_previewItem;

        public void Awake()
        {
            m_equipmentButton.onSelect.AddListener(On_Select);
            m_equipmentButton.onDeselect.AddListener(On_Deselect);
        }

        public void Initialize(Equipment equipedItem, Equipment previewItem)
        {
            m_equipedItem = equipedItem;
            m_previewItem = previewItem;
        
            m_EquipmentName.text = m_equipedItem == null ? "Empty" : m_equipedItem.Name;
        }

        public void Close()
        {
            m_equipmentButton.onSelect.RemoveAllListeners();
            m_equipmentButton.onDeselect.RemoveAllListeners();
        }

        public void On_Select()
        {
            m_EquipmentName.text = m_previewItem.Name;
            // Todo : replace content with preview item
        }

        public void On_Deselect()
        {
            m_EquipmentName.text = m_equipedItem == null ? "Empty" : m_equipedItem.Name;
            // Todo : replace content with equiped item
        }

        public void OnSlot_Selected()
        {

        }

        public void Enable(bool value)
        {
            m_equipmentButton.interactable = value;
        }

        public ButtonOverride GetButton()
        {
            return m_equipmentButton;
        }

    }
}
