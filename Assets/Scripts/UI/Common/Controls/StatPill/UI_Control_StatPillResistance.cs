using RPGTest.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_Control_StatPillResistance : UI_Control_StatPill
    {
        public Element Element = Element.None;
        public StatusEffect Status = StatusEffect.None;

        [SerializeField] private Image Image; // TODO : load image of appropriate element.

        public void Initialize(Element element, int value)
        {
            Element = element;
            Initialize(value, false);
        }

        public void Initialize(StatusEffect status, int value)
        {
            Status = status;
            Initialize(value, false);
        }
    }
}
