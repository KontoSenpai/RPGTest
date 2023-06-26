using TMPro;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_StatWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI StatName;
        [SerializeField] private TextMeshProUGUI StatValue;

        [SerializeField] private Color PositiveColor;
        [SerializeField] private Color NegativeColor;

        public void Refresh(string attribute, int value, bool displaySymbol = false)
        {
            if (displaySymbol)
            {
                StatName.text = $"{attribute}";
                StatValue.text = value.ToString();
                StatValue.color = value > 0 ? PositiveColor : NegativeColor;
            }
            else
            {
                StatName.text = attribute;
                StatValue.text = value.ToString();
            }
        }

        public void Clean()
        {
            StatValue.text = string.Empty;
        }
    }
}
