using RPGTest.Enums;
using RPGTest.Helpers;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_Control_Attribute : MonoBehaviour
    {
        public Attribute Attribute;

        [SerializeField] private TextMeshProUGUI StatName;
        [SerializeField] private TextMeshProUGUI StatValue;
        [SerializeField] private TextMeshProUGUI PreviewText;

        [SerializeField] private Color PositiveColor;
        [SerializeField] private Color NegativeColor;

        private int m_value = 0;

        public void Start()
        {
            if (Attribute != Attribute.None)
            {
                StatName.text = Attribute.GetShortName();
            }
            
        }

        public void Initialize(Attribute attribute, int value, bool displaySymbol = false)
        {
            StatName.text = attribute.GetShortName();
            m_value = value;

            SetValue(m_value);

            if (displaySymbol)
            {
                StatValue.color = value > 0 ? PositiveColor : NegativeColor;
            }
        }

        public void Initialize(int value, bool displaySymbol = false)
        {
            m_value = value;

            SetValue(m_value);

            if (displaySymbol)
            {
                StatValue.color = value > 0 ? PositiveColor : NegativeColor;
            }
        }

        public void Preview(int value)
        {
            SetValue(value);

            var diff = value - m_value;

            PreviewText.gameObject.SetActive(diff != 0);
            if (diff == 0)
            {
                return;
            }

            PreviewText.text = $"{(diff > 0 ? '↑' : '↓')} {diff}";
            PreviewText.color = diff > 0 ? Color.green : Color.red;
        }

        public void Unpreview()
        {
            PreviewText.gameObject.SetActive(false);

            SetValue(m_value);
        }

        public void Clean()
        {
            StatValue.text = string.Empty;
        }

        private void SetValue(int value)
        {
            StatValue.text = value.ToString();
        }
    }
}
