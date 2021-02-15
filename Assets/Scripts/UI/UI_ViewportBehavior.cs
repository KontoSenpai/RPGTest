using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace RPGTest.UI
{
    public class UI_ViewportBehavior : MonoBehaviour
    {
        public GameObject ItemTemplate;

        public int MaxDisplayedCount;

        public Scrollbar Scroll;

        private float m_valueJump;
        private int m_maxButtons;
        private int m_currentIndex = 1;
        private int m_currentIndexOnScreen = 1;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize(Vector3 position, int buttonCount, int deltaX)
        {
            m_currentIndex = 1;
            m_currentIndexOnScreen = 1;
            Scroll.value = 1;
            m_maxButtons = buttonCount;

            var viewportTranform = ItemTemplate.GetComponent<RectTransform>();

            var sizeDelta = viewportTranform.sizeDelta;
            sizeDelta.y = sizeDelta.y * (m_maxButtons > MaxDisplayedCount ? MaxDisplayedCount : m_maxButtons);
            sizeDelta.x = deltaX;
            this.GetComponent<RectTransform>().sizeDelta = sizeDelta;

            position.x += (viewportTranform.sizeDelta.x + 20) * 0.835f;
            this.transform.position = position;

            var scrollSize = Scroll.GetComponent<RectTransform>().sizeDelta;
            scrollSize.y = sizeDelta.y;
            Scroll.GetComponent<RectTransform>().sizeDelta = scrollSize;

            if(m_maxButtons > MaxDisplayedCount)
            {
                Scroll.numberOfSteps = m_maxButtons - MaxDisplayedCount;
                m_valueJump = 1.0f / (float)(m_maxButtons - MaxDisplayedCount);
            }
        }

        public void StepChange(int factor)
        {
            if (factor < 0 && m_currentIndexOnScreen == 1)
            {
                if (m_currentIndex > 1)
                {
                    Scroll.value -= m_valueJump * factor;
                }
            }
            else if (factor > 0 && m_currentIndexOnScreen == MaxDisplayedCount)
            {
                if(m_currentIndex < m_maxButtons)
                {
                    Scroll.value -= m_valueJump * factor;
                }
            }
            else if (m_currentIndexOnScreen + factor >= 1 && m_currentIndexOnScreen + factor <= MaxDisplayedCount)
            {
                m_currentIndexOnScreen += factor;
            }

            if (m_currentIndex + factor >= 1 && m_currentIndex + factor <= m_maxButtons)
            {
                m_currentIndex += factor;
            }
        }
    }
}