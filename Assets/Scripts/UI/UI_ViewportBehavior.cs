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

        private float m_initialYPos = 0.0f;
        // Start is called before the first frame update
        void Start()
        {
            m_initialYPos = this.transform.position.y;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize(int buttonCount, int posCoeff)
        {
            m_currentIndex = 1;
            m_currentIndexOnScreen = 1;
            Scroll.value = 1;
            m_maxButtons = buttonCount;

            var position = this.transform.localPosition;
            position.y = (posCoeff * 40);
            this.transform.localPosition = position;


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