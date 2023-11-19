using RPGTest.Helpers;
using RPGTest.Inputs;
using RPGTest.Models.Entity;
using RPGTest.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_View_ActionSelection : UI_View
    {
        [SerializeField]
        private GameObject ActionGameObject;

        [SerializeField]
        private GridLayoutGroup LayoutGroup => GetComponent<GridLayoutGroup>();

        [SerializeField]
        private GameObject LayoutElement;

        public event CharacterEnumActionSelectionHandler CharacterEnumActionSelected;

        private PlayableCharacter m_character;

        public override void Open()
        {
            base.Open();
        }

        public override void Close()
        {
            base.Close();
        }

        public void Initialize(GameObject component, List<Enum> actions)
        {
            Clear();
            if (component != null && component.TryGetComponent(out UI_View_EntityInfos entityComponent)) {
                m_character = entityComponent.GetPlayableCharacter();
            }

            foreach(var action in actions)
            {
                var go = Instantiate(ActionGameObject, LayoutElement.transform);
                var actionComponent = go.GetComponent<UI_ActionButton>();
                actionComponent.Initialize(action);
                actionComponent.NavigationActionSelected += ActionComponent_NavigationActionSelected;

            }
            ResizeToContent();
            MoveToGameObject(component);

            UI_List_Utils.RefreshHierarchy(LayoutElement, GetComponentsInChildren<Button>().Select(t => t.gameObject).ToList());
            UI_List_Utils.SetVerticalNavigation(GetComponentsInChildren<Button>().Select(c => c.gameObject).ToList());


            GetComponentsInChildren<Button>()[0].Select();
        }

        private void ActionComponent_NavigationActionSelected(Enum action)
        {
            CharacterEnumActionSelected(m_character, action);
        }

        public void Clear()
        {
            m_character = null;
            foreach(var gameObject in GetComponentsInChildren<Button>().Select(o => o.gameObject))
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }


        public override Dictionary<string, string[]> GetInputDisplay(Controls playerInput = null)
        {
            var controls = playerInput ?? m_playerInput;
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Action",
                    new string[]
                    {
                        "UI_" + controls.UI.Navigate.name + ".vertical"
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + controls.UI.Submit.name,
                        "UI_" + controls.UI.LeftClick.name,
                    }
                },
            };

            return m_inputActions;
        }

        #region private methods

        /// <summary>
        /// https://docs.unity3d.com/2022.1/Documentation/Manual/HOWTO-UIFitContentSize.html
        /// </summary>d
        private void ResizeToContent()
        {
            if (LayoutGroup != null)
            {
                var count = GetComponentsInChildren<UI_Control_Tooltip>()
                    .Where(t => t.gameObject.activeSelf)
                    .Select(t => t.gameObject).Count();

                if (count < LayoutGroup.constraintCount)
                {
                    var transform = gameObject.GetComponent<RectTransform>();
                    transform.sizeDelta = new Vector2(transform.sizeDelta.x, count * LayoutGroup.cellSize.y);
                }
            }
        }
        #endregion
    }
}

