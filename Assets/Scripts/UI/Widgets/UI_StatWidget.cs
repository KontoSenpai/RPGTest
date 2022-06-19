using TMPro;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_StatWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI StatName;
        [SerializeField] private TextMeshProUGUI StatValue;

        public void Refresh(string attribute, int value, bool displaySymbol = false)
        {

            if (displaySymbol)
            {
                StatName.text = $"{attribute} :";
                StatValue.text = value > 0 ? $"+{value}" : $"{value}";
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
