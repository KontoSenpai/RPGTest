using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.UI.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.UI.Common
{
    public class UI_View_EffectTooltip : UI_View
    {
        [SerializeField]
        private UI_Control_Tooltip ToolTipGameObject;

        [SerializeField]
        private GridLayoutGroup LayoutGroup => GetComponent<GridLayoutGroup>();

        [SerializeField]
        private GameObject LayoutElement;

        public override void Open()
        {
            base.Open();
        }

        public override void Close()
        {
            base.Close();
        }

        public void Initialize(IEnumerable<Attribute> attributes, IEnumerable<string> effects)
        {
            Clear();
            InitializeAttributes(attributes);
            InitializeEffects(effects);
            ResizeToContent();

            UI_List_Utils.RefreshHierarchy(LayoutElement, GetComponentsInChildren<UI_Control_Tooltip>().Where(t => t.gameObject.activeSelf).Select(t => t.gameObject).ToList());
        }

        public void Clear()
        {
            foreach(var gameObject in GetComponentsInChildren<UI_Control_Tooltip>().Select(o => o.gameObject))
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        #region private methods
        private void InitializeAttributes(IEnumerable<Attribute> attributes)
        {
            foreach(var attribute in attributes)
            {
                var go = Instantiate(ToolTipGameObject, LayoutElement.transform);
                go.GetComponent<UI_Control_Tooltip>().Initialize(attribute.GetName(), attribute.GetDescription());
            }
        }

        private void InitializeEffects(IEnumerable<string> effects)
        {
            foreach (var effectID in effects)
            {
                var effect = EffectsCollector.TryGetEffect(effectID);
                if (effect.Type == EffectType.Passive)
                {
                    var go = Instantiate(ToolTipGameObject, LayoutElement.transform);
                    go.GetComponent<UI_Control_Tooltip>().Initialize(effect.Name, effect.Label);
                }
            }

        }

        /// <summary>
        /// https://docs.unity3d.com/2022.1/Documentation/Manual/HOWTO-UIFitContentSize.html
        /// </summary>d
        private void ResizeToContent()
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
        #endregion
    }
}

