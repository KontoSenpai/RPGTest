using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_Bar_Widget : MonoBehaviour
    {
        public TextMeshProUGUI TypeOfBar;
        public TextMeshProUGUI CurrentValue;
        public TextMeshProUGUI MaxValue;
        public Image Bar;

        public ColorBarGradient[] ColorGradients;
        public bool Interpolate;

        private float m_currentValue;
        private float m_maxValue;

        public void Initialize(string displayName, int current, int max)
        {
            m_currentValue = current;
            m_maxValue = max;

            TypeOfBar.text = displayName;

            CurrentValue.text = m_currentValue.ToString();
            MaxValue.text = m_maxValue.ToString();

            Bar.fillAmount = m_maxValue > 0 ? m_currentValue / m_maxValue : 1.0f;
        }

        public int GetCurrentValue()
        {
            return (int)Mathf.Ceil(m_currentValue);
        }

        public bool UpdateValues(int current, int max)
        {
            m_currentValue = current;
            m_maxValue = max;

            CurrentValue.text = m_currentValue.ToString();
            MaxValue.text = m_maxValue.ToString();

            float fillAmount = m_maxValue > 0 ? m_currentValue / m_maxValue : 1.0f;
            SetColor(fillAmount);
            Bar.fillAmount = fillAmount;
            return fillAmount >= 1.0f;
        }

        private void SetColor(float fillAmount)
        {
            ColorBarGradient current;
            ColorBarGradient next;
            if(ColorGradients != null && ColorGradients.Length > 0)
            {
                current = ColorGradients.Last(c => fillAmount >= c.Threshold);

                if(Interpolate)
                {
                    next = ColorGradients.FirstOrDefault(c => fillAmount < c.Threshold);

                    if (next == null)
                    {
                        Bar.color = current.Color;
                    }
                    else
                    {
                        Color interpolatedColor = new Color();

                        float a = fillAmount - current.Threshold;
                        float b = next.Threshold - current.Threshold;
                        float c = a / b;

                        interpolatedColor.a = Mathf.Lerp(current.Color.a, next.Color.a, c);
                        interpolatedColor.r = Mathf.Lerp(current.Color.r, next.Color.r, c);
                        interpolatedColor.g = Mathf.Lerp(current.Color.g, next.Color.g, c);
                        interpolatedColor.b = Mathf.Lerp(current.Color.b, next.Color.b, c);

                        Bar.color = interpolatedColor;
                    }
                }
                else
                {
                    Bar.color = current.Color;
                }
            }
        }
    }
    
    [System.Serializable]
    public class ColorBarGradient
    {
        public float Threshold;
        public Color Color;
    }
}