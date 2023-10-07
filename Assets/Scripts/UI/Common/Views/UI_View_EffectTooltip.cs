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
        private Canvas ReferenceCanvas;

        [SerializeField]
        private UI_Control_Tooltip ToolTipGameObject;

        [SerializeField]
        private GridLayoutGroup LayoutGroup => GetComponent<GridLayoutGroup>();

        [SerializeField]
        private GameObject LayoutElement;

        [SerializeField]
        private float OffsetX = 0;
        [SerializeField]
        private float OffsetY = 0;

        [SerializeField]
        private Vector2 UpperAnchorMax = new Vector2(.0f, 1.0f);
        [SerializeField]
        private Vector2 UpperAnchorMin = new Vector2(.0f, 1.0f);
        [SerializeField] 
        private Vector2 UpperPivot = new Vector2(.0f, 1.0f);

        [SerializeField]
        private Vector2 LowerAnchorMax = new Vector2();
        [SerializeField]
        private Vector2 LowerAnchorMin = new Vector2();
        [SerializeField]
        private Vector2 LowerPivot = new Vector2(.0f, 0.0f);

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

        public void MoveToGameObject(GameObject root, GameObject gameObject)
        {
            var itemTransform = gameObject.GetComponent<RectTransform>();
            var tooltipTransform = GetComponent<RectTransform>();

            var canvasPosition = ReferenceCanvas.GetComponent<RectTransform>().InverseTransformPoint(itemTransform.position);

            if (canvasPosition.y < 0)
            {
                tooltipTransform.anchorMax = LowerAnchorMax;
                tooltipTransform.anchorMin = LowerAnchorMin;
                tooltipTransform.pivot = LowerPivot;
            } else
            {
                tooltipTransform.anchorMax = UpperAnchorMax;
                tooltipTransform.anchorMin = UpperAnchorMin;
                tooltipTransform.pivot = UpperPivot;
            }

            canvasPosition.x += OffsetX;
            canvasPosition.y += OffsetY;

            tooltipTransform.anchoredPosition = canvasPosition;
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

