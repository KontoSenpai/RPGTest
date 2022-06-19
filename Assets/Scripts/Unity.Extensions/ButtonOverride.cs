using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Extensions.UI
{
    /// <summary>
    /// A standard button that sends an event when clicked.
    /// </summary>
    [AddComponentMenu("UI/ButtonOverride", 30)]
    public class ButtonOverride : Button
    {
        [Serializable]
        public class ButtonSelectedEvent : UnityEvent { }
        [Serializable]
        public class ButtonDeselectedEvent : UnityEvent { }

        public bool Selected = false;

        [SerializeField]
        private ButtonSelectedEvent m_OnSelect = new ButtonSelectedEvent();
        [SerializeField]
        private ButtonDeselectedEvent m_OnDeselect = new ButtonDeselectedEvent();
        public ButtonSelectedEvent onSelect
        {
            get { return m_OnSelect; }
            set { m_OnSelect = value; }
        }
        public ButtonDeselectedEvent onDeselect
        {
            get { return m_OnDeselect; }
            set { m_OnDeselect = value; }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onSelect", this);
            base.OnSelect(eventData);

            Selected = true;
            m_OnSelect.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onDeselect", this);
            base.OnDeselect(eventData);

            Selected = false;
            m_OnDeselect.Invoke();
        }
    }
}
