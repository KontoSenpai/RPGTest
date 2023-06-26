using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.UI
{
    public class UI_EquipmentList : UI_ItemList
    {
        private List<UI_InventoryItem> m_items = new List<UI_InventoryItem>(); // List of available guiItems.

        /// <summary>
        /// Initialize the display information of each given gui item and filters.
        /// Clears both items and filters list before affectin the new objects
        /// </summary>
        /// <param name="items">Instantiated GUI items</param>
        /// <param name="availableFilters">Available filters</param>
        public override void Initialize(List<UIItemDisplay> items, List<ItemFilterCategory> availableFilters)
        {
            m_items.ForEach((i) => Destroy(i));
            m_items.Clear();
            m_filters.ForEach((f) => Destroy(f));
            m_filters.Clear();

            // InitializeItems(items);
            // InitializeFilters(availableFilters);
        }
    }
}
