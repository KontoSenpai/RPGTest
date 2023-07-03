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
        Key,
        None,
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
        [SerializeField] private ItemFilterCategory m_filter;
        [SerializeField] private TextMeshProUGUI m_name;

        public void Awake()
        {
            m_name.text = m_filter.ToString();
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
