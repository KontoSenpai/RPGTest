using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Models;
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


        private Buff m_buff;

        #region public Methods
        public void Initialize(Buff buff)
        {
            m_buff = buff;
            Refresh();
        }

        public void Refresh()
        {
            var effect = EffectsCollector.TryGetEffect(m_buff.Id);

            BuffColor.color = effect.Type == EffectType.Buff ? BuffColorTemplate : DebuffColorTemplate;
            BuffAttribute.text = effect.Icon;
            BuffDuration.text = m_buff.Duration.ToString();
        }

        public string GetId()
        {
            return m_buff.Id;
        }
        #endregion
    }
}
