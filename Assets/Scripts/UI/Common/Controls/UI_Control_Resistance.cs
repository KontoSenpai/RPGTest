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

        public void Initialize(float value)
        {
            Value.text = value * 100 + "%";
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
    }
}
