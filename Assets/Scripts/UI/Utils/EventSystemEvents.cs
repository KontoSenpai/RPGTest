using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGTest.UI.Utils
{
    /// <summary>
    /// Allows to capture events when a Selectable selection is updated
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class EventSystemEvents : MonoBehaviour
    {
        public static event Action<GameObject, GameObject> OnSelectionUpdated;
        private static void DispatchSelectionUpdated(GameObject newSelectedGameObject, GameObject previousSelectedGameObject)
        {
            if (OnSelectionUpdated != null)
            {
                OnSelectionUpdated.Invoke(newSelectedGameObject, previousSelectedGameObject);
            }
        }

        [SerializeField]
        private EventSystem eventSystem;

        private GameObject m_LastSelectedGameObject;

        private void Update()
        {
            var currentSelectedGameObject = eventSystem.currentSelectedGameObject;
            if (currentSelectedGameObject != m_LastSelectedGameObject)
            {
                DispatchSelectionUpdated(currentSelectedGameObject, m_LastSelectedGameObject);
                m_LastSelectedGameObject = currentSelectedGameObject;
            }
        }
    }
}