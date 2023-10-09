using RPGTest.Enums;
using RPGTest.Helpers;

namespace RPGTest.UI.Common
{
    public class UI_Control_StatPillAttribute : UI_Control_StatPill
    {
        public Attribute Attribute;

        public void Start()
        {
            if (Attribute != Attribute.None)
            {
                Name.text = Attribute.GetShortName();
            }
            
        }

        public void Initialize(Attribute attribute, int value, bool displaySymbol = false)
        {
            Name.text = attribute.GetShortName();
            m_value = value;

            SetValue(m_value);

            if (displaySymbol)
            {
                Value.color = value > 0 ? PositiveColor : NegativeColor;
            }
        }
    }
}
