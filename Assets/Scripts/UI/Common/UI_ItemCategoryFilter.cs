using System;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public enum ItemFilterCategory
    {
        All,
        Weapons,
        Armors,
        Accessories,
        Consumables,
        Materials,
        Valuable,
        Key
    }

    public class CategoryFilterSelected : EventArgs
    {
        public CategoryFilterSelected(ItemFilterCategory filterCategory)
        {
            FilterCategory = filterCategory;
        }

        public ItemFilterCategory FilterCategory { get; }
    }

    public class UI_ItemCategoryFilter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_name;
        private ItemFilterCategory m_filter;

        public void Initialize(ItemFilterCategory filter)
        {
            m_filter = filter;
            m_name.text = filter.ToString();
        }

        public ItemFilterCategory GetFilter() 
        {
            return m_filter;
        }

        public void OnCategory_Selected()
        {
            CategoryFilterSelected.Invoke(this, new CategoryFilterSelected(m_filter));
        }

        [HideInInspector]
        public event EventHandler<CategoryFilterSelected> CategoryFilterSelected;
    }
}
