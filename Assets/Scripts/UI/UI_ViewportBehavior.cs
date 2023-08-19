using System;
using UnityEngine;
using UnityEngine.UI;


namespace RPGTest.UI
{
    //*** ENUMS ***//
    public enum ScrollDirection
    {
        VERTICAL,
        HORIZONTAL,
        BOTH
    }

    public enum ScrollType
    {
        ITEM, // If item is out of scope, scroll enough to see it
        FLOAT, // If item is last in scope, scroll enough to see the next one
        PAGE, // If item is out of scope, scroll enough to display a full new page
    }

    // https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/src/88c43891c0bf44f136e3021ad6c89d704dfebc83/Scripts/Utilities/UIScrollToSelection.cs?at=master&fileviewer=file-view-default
    public class UI_ViewportBehavior : MonoBehaviour
    {
        [Header("[ Settings ]")]
        [SerializeField]
        private ScrollDirection ScrollDirection;
        [SerializeField]
        private ScrollType ScrollType;

        [SerializeField]
        private float ScrollSpeed = 10f;

        [SerializeField] bool debug;

        private RectTransform m_scrollWindow { get; set; }
        private ScrollRect m_scrollRect { get; set; }

        protected RectTransform LayoutListGroup
        {
            get { return m_scrollRect != null ? m_scrollRect.content : null; }
        }

        protected RectTransform CurrentTargetRectTransform { get; set; }

        //*** METHODS - PROTECTED ***//
        protected virtual void Awake()
        {
            m_scrollRect = GetComponent<ScrollRect>();
            m_scrollWindow = m_scrollRect.GetComponent<RectTransform>();
        }

        public void InitializeStepAmount(GameObject element, int count)
        {
            switch (ScrollDirection)
            {
                case ScrollDirection.VERTICAL:
                    if (count == 0)
                    {
                        m_scrollRect.verticalScrollbar.numberOfSteps = 0;
                    } else
                    {
                        var rectTransform = element.GetComponent<RectTransform>();
                        var fitInViewport = Mathf.FloorToInt(m_scrollWindow.rect.height / rectTransform.rect.height);
                        m_scrollRect.verticalScrollbar.numberOfSteps = (count - fitInViewport) + 1;
                    }

                    m_scrollRect.verticalNormalizedPosition = 1;

                    break;
                case ScrollDirection.HORIZONTAL:
                    //UpdateHorizontalScrollPosition(selection);
                    throw new NotImplementedException();
                case ScrollDirection.BOTH:
                    //UpdateVerticalScrollPosition(selection);
                    //UpdateHorizontalScrollPosition(selection);
                    throw new NotImplementedException();
            }
        }

        public void ScrollToSelection(GameObject currentSelection, GameObject previousSelection)
        {
            if (currentSelection == previousSelection)
            {
                return;
            }

            CurrentTargetRectTransform = (currentSelection != null) ?
                currentSelection.GetComponent<RectTransform>() :
                null;

            // check main references
            if (m_scrollRect == null || LayoutListGroup == null || m_scrollWindow == null)
            {
                return;
            }

            // depending on selected scroll direction move the scroll rect to selection
            switch (ScrollDirection)
            {
                case ScrollDirection.VERTICAL:
                    UpdateVerticalScrollPosition(CurrentTargetRectTransform);
                    break;
                case ScrollDirection.HORIZONTAL:
                    //UpdateHorizontalScrollPosition(selection);
                    throw new NotImplementedException();
                case ScrollDirection.BOTH:
                    //UpdateVerticalScrollPosition(selection);
                    //UpdateHorizontalScrollPosition(selection);
                    throw new NotImplementedException();
            }
        }

        private void UpdateVerticalScrollPosition(RectTransform selection)
        {
            // move the current scroll rect to correct position
            float selectionPosition = -selection.anchoredPosition.y - (selection.rect.height * (1 - selection.pivot.y));

            float elementHeight = selection.rect.height;
            float maskHeight = m_scrollWindow.rect.height;
            float listAnchorPosition = LayoutListGroup.anchoredPosition.y;

            // get the element offset value depending on the cursor move direction
            float newScrollOffset = GetScrollOffset(selectionPosition, elementHeight, listAnchorPosition, maskHeight);

            // move the target scroll rect
            m_scrollRect.verticalNormalizedPosition += (newScrollOffset / LayoutListGroup.rect.height) * 100;
        }

        /// <summary>
        /// Calculate the scrolling viewport position
        /// </summary>
        /// <param name="elementPosition">Position of the element in the viewport</param>
        /// <param name="elementLength">Length of the element for desired direction</param>
        /// <param name="listAnchorPosition">Position of the anchor of the viewport holding the lements</param>
        /// <param name="maskLength">Size of the visible portion of the viewport</param>
        /// <returns></returns>
        private float GetScrollOffset(float elementPosition, float elementLength, float listAnchorPosition, float maskLength)
        {
            switch (ScrollType)
            {
                case ScrollType.FLOAT:
                    if (elementPosition == 0.0f && listAnchorPosition == 0.0f)
                    {
                        return 0.0f;
                    }
                    else if (elementPosition <= listAnchorPosition)
                    {
                        return m_scrollRect.verticalScrollbar.numberOfSteps;
                        //return listAnchorPosition - elementLength;
                    }
                    else if (elementPosition >= (listAnchorPosition + maskLength - elementLength))
                    {
                        return -m_scrollRect.verticalScrollbar.numberOfSteps;
                        //return listAnchorPosition + elementLength;
                    }
                    return 0.0f;
            }
            return 0;
        }
    }
}