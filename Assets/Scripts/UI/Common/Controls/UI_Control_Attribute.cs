using RPGTest.Helpers;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_Control_Attribute : MonoBehaviour
    {
        public Enums.Attribute Attribute;

        [SerializeField] private TextMeshProUGUI StatName;
        [SerializeField] private TextMeshProUGUI StatValue;

        [SerializeField] private Color PositiveColor;
        [SerializeField] private Color NegativeColor;

        public void Start()
        {
            StatName.text = Attribute.GetDescription();
        }

        public void Refresh(int value, bool displaySymbol = false)
        {
            StatValue.text = value.ToString();

            if (displaySymbol)
            {
                StatValue.color = value > 0 ? PositiveColor : NegativeColor;
            }
        }

        public void Clean()
        {
            StatValue.text = string.Empty;
        }
    }
}
