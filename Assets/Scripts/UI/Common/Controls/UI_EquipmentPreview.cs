using RPGTest.Models.Items;
using Unity.Extensions.UI;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentPreview : MonoBehaviour
    {
        [SerializeField] private UI_InventoryItem m_inventoryItem;
        [SerializeField] private ButtonOverride m_equipmentButton;

        private Equipment m_previewItem;

        #region public Methods
        public void Awake()
        {
            m_equipmentButton.onSelect.AddListener(On_Select);
            m_equipmentButton.onDeselect.AddListener(On_Deselect);
        }

        public void Initialize(Equipment previewItem = null)
        {
            m_previewItem = previewItem;
        }

        public void Enable(bool value)
        {
            m_equipmentButton.interactable = value;
        }

        public ButtonOverride GetButton()
        {
            return m_equipmentButton;
        }

        public void PreviewItem(bool state)
        {
            m_inventoryItem.PreviewItem(state ? m_previewItem : null);
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the button gets selected
        /// </summary>
        public void On_Select()
        {
            if (m_previewItem != null)
            {
                PreviewItem(true);
            }

            // Todo : replace stat information with preview item
        }

        /// <summary>
        /// Triggered when the button gets deselected
        /// </summary>
        public void On_Deselect()
        {
            if (m_previewItem != null)
            {
                PreviewItem(false);
            }
            // Todo : replace stat information with equiped item
        }
        #endregion
    }
}