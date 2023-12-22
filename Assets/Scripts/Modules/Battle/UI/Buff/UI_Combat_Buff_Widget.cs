using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Models.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.Modules.Battle.UI
{
    public class UI_Combat_Buff_Widget : MonoBehaviour
    {
        [SerializeField] private Image BuffColor;
        [SerializeField] private TextMeshProUGUI BuffAttribute;
        [SerializeField] private TextMeshProUGUI BuffDuration;

        [SerializeField] private Color BuffColorTemplate;
        [SerializeField] private Color DebuffColorTemplate;

        private Effect m_effect;

        #region public Methods
        public void Initialize(Effect effect)
        {
            m_effect = effect;
            Refresh();
        }

        public void Refresh()
        {
            var effect = EffectsCollector.TryGetEffect(m_effect.Id);

            BuffColor.color = effect.Type == EffectType.Buff ? BuffColorTemplate : DebuffColorTemplate;
            BuffAttribute.text = effect.Icon;
            BuffDuration.text = m_effect.Duration.ToString();
        }

        public string GetId()
        {
            return m_effect.Id;
        }
        #endregion
    }
}
