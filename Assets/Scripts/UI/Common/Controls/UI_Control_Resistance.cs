using RPGTest.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_Control_Resistance : MonoBehaviour
    {
        public Element Element = Element.None;
        public StatusEffect Status = StatusEffect.None;

        [SerializeField] private Image Image;
        [SerializeField] private TextMeshProUGUI Value;

        [SerializeField] private TextMeshProUGUI PreviewText;

        private float m_value = 0;

        public void Initialize(float value)
        {
            m_value = value;
            SetValue(m_value);
        }

        public void Initialize(Element element, float value)
        {
            Element = element;
            Initialize(value);
        }

        public void Initialize(StatusEffect status, float value)
        {
            Status = status;
            Initialize(value);
        }

        public void Preview(float value)
        {
            SetValue(value);

            var diff = value - m_value;

            PreviewText.gameObject.SetActive(diff != 0);
            if (diff == 0)
            {
                return;
            }

            PreviewText.text = $"{(diff > 0 ? '↑' : '↓' )} {diff}";
            PreviewText.color = diff > 0 ? Color.green : Color.red;
        }

        public void Unpreview()
        {
            PreviewText.gameObject.SetActive(false);

            SetValue(m_value);
        }

        private void SetValue(float value)
        {
            if (value > 1.0f)
            {
                Value.text = "100 %";
            }
            else if (value < -1.0f)
            {
                Value.text = "-100 %";
            }
            else
            {
                Value.text = value * 100 + "%";
            }
        }
    }
}
