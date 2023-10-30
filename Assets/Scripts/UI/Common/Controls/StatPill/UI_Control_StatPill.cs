using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_Control_StatPill : MonoBehaviour
    {
        [SerializeField] protected bool PercentageValue;

        [SerializeField] protected TextMeshProUGUI Name;
        [SerializeField] protected TextMeshProUGUI Value;
        [SerializeField] protected TextMeshProUGUI PreviewText;

        [SerializeField] protected Color PositiveColor;
        [SerializeField] protected Color NegativeColor;

        protected int m_value = 0;

        public void Initialize(int value, bool displaySymbol = false)
        {
            m_value = value;

            SetValue(m_value);

            if (displaySymbol)
            {
                Value.color = value > 0 ? PositiveColor : NegativeColor;
            }
        }

        public virtual void Preview(int value)
        {
            SetValue(value);

            var diff = value - m_value;

            PreviewText.gameObject.SetActive(diff != 0);
            if (diff == 0)
            {
                return;
            }

            PreviewText.text = $"{(diff > 0 ? '↑' : '↓')} {diff} {(PercentageValue ? "%": string.Empty)}";
            PreviewText.color = diff > 0 ? Color.green : Color.red;
        }

        public void Unpreview()
        {
            PreviewText.gameObject.SetActive(false);

            SetValue(m_value);
        }

        public void Clean()
        {
            Value.text = string.Empty;
        }

        protected void SetValue(int value)
        {
            Value.text = $"{value.ToString()} {(PercentageValue ? "%" : string.Empty)}";
        }
    }
}
