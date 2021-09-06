using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Collectors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RPGTest.Models.Items;
using static RPGTest.UI.UI_Category_Button;

namespace RPGTest.UI
{
    public class UI_Shop : UI_Base
    {
        private InventoryManager m_inventoryManager => FindObjectOfType<InventoryManager>();

        public Scrollbar Scrollbar;

        public GameObject PurchaseWindow;
        public GameObject ShopList;
        public GameObject ItemMenuInstantiate;
        public Text CurrentMoney;
        public Button[] CategoryButtons;
        private int m_maxItemsPerPage = 10;

        public Color UnselectedCategoryColor;
        public Color SelectedCategoryColor;
        public Color UnselectedItemColor;
        public Color SelectedItemColor;
        //UI control
        bool m_initialized = false;

        //Category control
        private int m_currentCategoryIndex = 0;

        //Items control
        private List<GameObject> m_allItems;
        private IEnumerable<UI_ShopItem> m_shopItems => m_allItems.Select(x => x.GetComponent<UI_ShopItem>());
        private int m_currentSelectedItemIndex = 0;
        bool m_purchaseMode = true;

        //Input control
        private bool m_menuNavigationPressed = false;

        public void Awake()
        {
            
        }

        // Start is called before the first frame update
        void Start()
        {
            PurchaseWindow.GetComponent<UI_Item_Interaction>().ItemInteractionRequested += OnPurchaseRequested;
            foreach(var CategoryButton in CategoryButtons)
            {
                CategoryButton.GetComponent<UI_Category_Button>().CategoryFilterRequested += OnCategoryFilterRequested;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isActiveAndEnabled && m_initialized && !PurchaseWindow.activeSelf)
            {
                if (UnityEngine.Input.GetButton("Menu-Category-Left") && !m_menuNavigationPressed)
                {
                    m_menuNavigationPressed = true;
                    ChangeCategory(-1);
                }
                else if (UnityEngine.Input.GetButton("Menu-Category-Right") && !m_menuNavigationPressed)
                {
                    m_menuNavigationPressed = true;
                    ChangeCategory(1);
                }

                if(UnityEngine.Input.GetButton("Menu-Navigation-Up") && !m_menuNavigationPressed)
                {
                    m_menuNavigationPressed = true;
                    ChangeSelectedItem(-1);
                }
                else if(UnityEngine.Input.GetButton("Menu-Navigation-Down") && !m_menuNavigationPressed)
                {
                    m_menuNavigationPressed = true;
                    ChangeSelectedItem(1);
                }

                if (UnityEngine.Input.GetButton("Submit") && !m_menuNavigationPressed)
                {
                    m_menuNavigationPressed = true;
                    var itemId = m_allItems[m_currentSelectedItemIndex].GetComponent<UI_ShopItem>().GetItemId();
                    OpenPurchaseWindow(ItemCollector.TryGetItem(itemId));
                }

                if (UnityEngine.Input.GetButton("Cancel") && !m_menuNavigationPressed)
                {
                    m_menuNavigationPressed = true;
                    EmptyItemList();
                    UIClosed(this, null);
                }

                if (!UnityEngine.Input.GetButton("Menu-Category-Left") && !UnityEngine.Input.GetButton("Menu-Category-Right") && !UnityEngine.Input.GetButton("Menu-Navigation-Up") && !UnityEngine.Input.GetButton("Menu-Navigation-Down") && !UnityEngine.Input.GetButton("Submit") && !UnityEngine.Input.GetButton("Cancel"))
                { 
                    m_menuNavigationPressed = false;
                }
            }

            else if(!isActiveAndEnabled)
            {
                m_initialized = false;
            }
        }

        public IEnumerable<string> GetItemsId()
        {
            return m_shopItems.Select(x => x.GetItemId());
        }

        public void Initialize(IEnumerable<Item> items, bool purchaseMode = true)
        {
            m_purchaseMode = purchaseMode;

            m_allItems = new List<GameObject>();
            foreach (var item in items)
            {
                var uiItem = Instantiate(ItemMenuInstantiate);
                uiItem.transform.SetParent(ShopList.transform);
                uiItem.name = item.Id;
                uiItem.transform.localScale = new Vector3(1, 1, 1);
                uiItem.GetComponent<UI_ShopItem>().Initialize(item, m_inventoryManager.GetHeldItemQuantity(item.Id));
                uiItem.GetComponent<UI_ShopItem>().ItemClicked += OnItemClicked;
                m_allItems.Add(uiItem);
            }

            Scrollbar.numberOfSteps = m_allItems.Count;
            if(m_allItems.Count > m_maxItemsPerPage)
            {
                //Scrollbar.size = 1 / m_allItems.Count;
            }

            CurrentMoney.text = m_inventoryManager.Money.ToString();
            m_currentCategoryIndex = 0;
            m_currentSelectedItemIndex = 0;
            ChangeCategory(0);
            ChangeSelectedItem(0);
            m_initialized = true;
            m_menuNavigationPressed = true;
            UIOpened(this, null);
        }

        public void SetSelectedCategory(ItemType filteredCategory)
        {
            var matchingElement = CategoryButtons.FirstOrDefault(x => x.GetComponent<UI_Category_Button>().ItemType == filteredCategory);
            m_currentCategoryIndex = System.Array.IndexOf(CategoryButtons, CategoryButtons.FirstOrDefault(x => x.GetComponent<UI_Category_Button>().ItemType == filteredCategory));
        }

        public void SetSelectedItem(GameObject sender)
        {
            var index = System.Array.IndexOf(m_allItems.ToArray(), m_allItems.FirstOrDefault(x => x == sender));
            if(index > m_currentSelectedItemIndex)
            {
                ChangeSelectedItem(index - m_currentSelectedItemIndex);
            }
            else if(index < m_currentSelectedItemIndex)
            {
                ChangeSelectedItem(m_currentSelectedItemIndex - index);
            }
        }

        /// <summary>
        /// Chang the visibility of items affected by the filter
        /// </summary>
        /// <param name="filter">Item category to filter with</param>
        public void SetFilter(ItemType filter)
        {
            foreach(var shopItem in m_shopItems)
            {
                shopItem.SetVisibility(filter);
            }
        }

        public void OpenPurchaseWindow(Item selectedItem)
        {
            PurchaseWindow.SetActive(true);
            PurchaseWindow.GetComponent<UI_Item_Interaction>().Initialize(InteractionType.Purchase, selectedItem);
        }

        public bool TryExecuteTransaction(Item item, int quantity)
        {
            if (m_inventoryManager.TryUpdateMoney((m_purchaseMode ? -1 : 1) * (item.Value * quantity)))
            {
                CurrentMoney.text = m_inventoryManager.Money.ToString();
            }
            else
            {
                return false;
            }
            if(m_inventoryManager.TryAddItem(item.Id, quantity) == 0)
            {
                m_allItems.SingleOrDefault(x => x.GetComponent<UI_ShopItem>().GetItemId() == item.Id).GetComponent<UI_ShopItem>().UpdateHeldQuantity(m_inventoryManager.GetHeldItemQuantity(item.Id));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ChangeCategory(int indexChange)
        {
            if(m_currentCategoryIndex + indexChange >= 0 && m_currentCategoryIndex + indexChange < CategoryButtons.Count())
            {
                var color = CategoryButtons[m_currentCategoryIndex].colors;

                //Restore unselected state on previous category
                color.normalColor = UnselectedCategoryColor;
                CategoryButtons[m_currentCategoryIndex].colors = color;

                m_currentCategoryIndex += indexChange;

                //Set selected state on current category
                color.normalColor = SelectedCategoryColor;
                CategoryButtons[m_currentCategoryIndex].colors = color;

                SetFilter(CategoryButtons[m_currentCategoryIndex].GetComponent<UI_Category_Button>().ItemType);
                m_currentSelectedItemIndex = 0;
            }
        }

        private void ChangeSelectedItem(int indexChange)
        {
            if ( m_currentSelectedItemIndex + indexChange >= 0 && m_currentSelectedItemIndex + indexChange < m_allItems.Count)
            {
                var color = m_allItems[m_currentSelectedItemIndex].GetComponent<Button>().colors;
                //Restore unselected state on previous item
                color.normalColor = UnselectedItemColor;
                m_allItems[m_currentSelectedItemIndex].GetComponent<Button>().colors = color;

                m_currentSelectedItemIndex += indexChange;

                //Set selected state on current item
                color.normalColor = SelectedItemColor;
                m_allItems[m_currentSelectedItemIndex].GetComponent<Button>().colors = color;
            }
        }

        private void EmptyItemList()
        {
            foreach (var uiItem in m_allItems)
            {
                Destroy(uiItem);
            }
        }


        public void OnItemClicked(ItemClickedEventArgs icea)
        {
            SetSelectedItem(icea.UIItem);
            OpenPurchaseWindow(icea.Item);
        }

        public void OnPurchaseRequested(Item item, int quantity)
        {
            TryExecuteTransaction(item, quantity);
        }

        public void OnCategoryFilterRequested(ItemType itemType)
        {
            SetSelectedCategory(itemType);
            SetFilter(itemType);
        }

    }
}

