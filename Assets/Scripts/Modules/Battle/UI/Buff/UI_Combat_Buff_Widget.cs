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
            BuffColor.color = m_buff.Value > 0 ? BuffColorTemplate : DebuffColorTemplate;
            BuffAttribute.text = m_buff.Attribute.ToString()[0].ToString();
            BuffDuration.text = m_buff.Duration.ToString();
        }

        public Enums.Attribute GetAttribute()
        {
            return m_buff.Attribute;
        }
        #endregion
    }
}
