using System;
using RPGTest.Managers;
using RPGTest.Models.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public enum ItemQuantitySuperiorLimit
    {
        NONE,
        OWNED,
        PROVIDED,
        CAP,
    }

    // Special EventArgs class to hold info about Shapes.
    public class ItemQuantitySelectedEventArgs : EventArgs
    {
        public int Quantity { get; }
        public ItemQuantitySelectedEventArgs(int quantity)
        {
            Quantity = quantity;
        }
    }

    public class UI_Dialog_ItemQuantity : UI_Dialog
    {
        [SerializeField] private Image ItemImage;
        [SerializeField] private TextMeshProUGUI ItemName;
        
        [SerializeField] private TextMeshProUGUI HeldQuantity;
        [SerializeField] private TextMeshProUGUI SelectedQuantity;

        [SerializeField] private GameObject CostPanel;
        [SerializeField] private TextMeshProUGUI IndividualCost;
        [SerializeField] private TextMeshProUGUI TotalCost;

        private InventoryManager m_inventoryManager => FindObjectOfType<GameManager>().InventoryManager;

        private int m_currentQuantity = 1;
        private int m_ownedQuantity = 0;
        private int m_providedQuantity = -1;
        private ItemQuantitySuperiorLimit m_limitType;

        #region Public Methods
        public override void Awake()
        {
            base.Awake();
            
            m_playerInput.UI.Cancel.performed += ctx => OnCancel_Performed();

            m_playerInput.UI.Navigate.performed += ctx =>
            {
                var value = ctx.ReadValue<Vector2>();

                if (value.y > 0.4f)
                {
                    OnIncreaseQuantity_Performed();
                }
                else if (value.y < -0.4f)
                {
                    OnDecreaseQuantity_Performed();
                }
            };
        }

        public override void Open()
        {
            base.Open();
            UpdateInputActions();

            FindObjectOfType<EventSystem>().SetSelectedGameObject(ConfirmButton);
        }

        /// <summary>
        /// Prepare the dialog with selected item and various quantities
        /// </summary>
        /// <param name="item">Item in Question</param>
        /// <param name="quantityProvided">Quantity owned by potential seller</param>
        /// <param name="limitType">Define what is the limit for the quantity selection</param>
        /// <param name="displayCost">Hide/Display the cost panel based on the value</param>
        public void Initialize(
            Item item,
            ItemQuantitySuperiorLimit limitType = ItemQuantitySuperiorLimit.NONE,
            int quantityProvided = 0,
            bool displayCost = false
        ){
            m_currentQuantity = 1;

            m_ownedQuantity = m_inventoryManager.GetHeldItemQuantity(item.Id);

            m_providedQuantity = quantityProvided;
            m_limitType = limitType;

            ItemName.text = item.Name;

            HeldQuantity.text = m_ownedQuantity.ToString();
            SelectedQuantity.text = m_currentQuantity.ToString();

            InitializeInitialCost(item, displayCost);
        }
        #endregion

        #region EventHandlers
        public void OnSubmit_Performed()
        {
            DialogActionConfirmed(this, new ItemQuantitySelectedEventArgs(m_currentQuantity));
        }

        public void OnCancel_Performed()
        {
            DialogActionCancelled(this, null);
            Close();
        }

        public void OnIncreaseQuantity_Performed()
        {
            switch (m_limitType)
            {
                case ItemQuantitySuperiorLimit.NONE:
                    m_currentQuantity++;
                    break;
                case ItemQuantitySuperiorLimit.OWNED:
                    if (m_currentQuantity < m_ownedQuantity)
                    {
                        m_currentQuantity++;
                    }
                    break;
                case ItemQuantitySuperiorLimit.PROVIDED:
                    if (m_currentQuantity < m_providedQuantity)
                    {
                        m_currentQuantity++;
                    }
                    break;
                case ItemQuantitySuperiorLimit.CAP:
                    if (m_currentQuantity < 99)
                    {
                        m_currentQuantity++;
                    }
                    break;
            }
            UpdateQuantities();
        }

        public void OnDecreaseQuantity_Performed()
        {
            if (m_currentQuantity > 1)
            {
                m_currentQuantity--;
            }
            UpdateQuantities();
        }
        #endregion

        private void InitializeInitialCost(Item item, bool enabled)
        {
            IndividualCost.text = enabled ? item.Value.ToString() : "";
            CostPanel.SetActive(enabled);
        }

        private void UpdateQuantities()
        {
            SelectedQuantity.text = m_currentQuantity.ToString();
        }
    }
}
