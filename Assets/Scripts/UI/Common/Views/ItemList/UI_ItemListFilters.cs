using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// Component that manage the display of an item list.
    /// </summary>
    public class UI_ItemListFilters : MonoBehaviour
    {
        [SerializeField] private List<UI_ItemCategoryFilter> Filters;
        private ItemFilterCategory m_currentFilter;

        [HideInInspector]
        public FilterChangedHandler FilterChanged { get; set; }
        [HideInInspector]
        public delegate void FilterChangedHandler(ItemFilterCategory filter);

        #region Public Methods
        public void Awake() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        /// <summary>
        /// </summary>
        public void Initialize()
        {
            SelectDefault();
            FilterInternal(Filters[0].GetFilter());
        }

        public void SelectDefault()
        {
            for (int i = 0; i < Filters.Count; i++)
            {
                Filters[i].GetComponent<Button>().interactable = i != 0;
            }
        }

        public void Filter(ItemFilterCategory filter)
        {
            if (m_currentFilter == filter) return;

            FilterInternal(filter);
        }

        public void CycleFilters()
        {
            var index = Array.IndexOf(Filters.ToArray(), Filters.SingleOrDefault(f => f.GetFilter() == m_currentFilter));
            if (index != -1 && index == Filters.Count -1)
            {
                Filter(Filters[0].GetFilter());
            } else if (index != -1)
            {
                Filter(Filters[index + 1].GetFilter());
            }
        }
        #endregion

        #region private methods
        private void FilterInternal(ItemFilterCategory filter)
        {
            m_currentFilter = filter;
            foreach (var filterComponent in Filters)
            {
                filterComponent.GetComponent<Button>().interactable = filterComponent.GetFilter() != m_currentFilter;
            }
            FilterChanged(m_currentFilter);
        }
        #endregion
    }
}
