using RPGTest.Collectors;
using RPGTest.Models.Effects;

namespace RPGTest.UI.Common
{
    public class UI_Control_StatPillEffect : UI_Control_StatPill
    {
        public void Initialize(Effect effect)
        {
            Name.text = LocalizationCollectors.TryGetLocalizedLine(effect.Name, out string localizedText) ? localizedText : effect.Name;
            Value.text = string.Empty;
        }

        public override void Preview(int value)
        {

        }
    }
}
