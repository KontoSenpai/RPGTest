using UnityEngine;
using UnityEngine.UI;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using TMPro;
using RPGTest.Enums;

namespace RPGTest.UI.Battle
{
    public class UI_PartyMember_Widget : MonoBehaviour
    {
        #region Properties
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI LevelValue;

        [SerializeField] private UI_Bar_Widget HPBarWidget;
        [SerializeField] private UI_Bar_Widget ManaBarWidget;
        [SerializeField] private UI_Bar_Widget StaminaBarWidget;
        [SerializeField] private UI_Bar_Widget ATBBarWidget;

        public UI_Stances_Widget StanceWidget;

        [HideInInspector]
        public ActionWindowRequestedHandler ActionWindowRequested { get; set; }
        [HideInInspector]
        public delegate void ActionWindowRequestedHandler(PlayableCharacter character);
        #endregion

        #region variables
        private PlayableCharacter m_playableCharacter;
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }

        public void Initialize(PlayableCharacter playableCharacter)
        {
            m_playableCharacter = playableCharacter;
            Name.text = m_playableCharacter.Name;
            LevelValue.text = m_playableCharacter.Level.ToString();

            // TODO : Values for current preset
            HPBarWidget.Initialize("HP", (int)m_playableCharacter.GetAttribute(Attribute.HP), (int)m_playableCharacter.GetAttribute(Attribute.MaxHP));
            ManaBarWidget.Initialize("Mana", (int)m_playableCharacter.GetAttribute(Attribute.MP), (int)m_playableCharacter.GetAttribute(Attribute.MaxMP));
            StaminaBarWidget.Initialize("Stamina", (int)m_playableCharacter.GetAttribute(Attribute.Stamina), (int)m_playableCharacter.GetAttribute(Attribute.MaxStamina));
            ATBBarWidget.Initialize("ATB", 0, 1000);

            m_playableCharacter.PlayerInputRequested += Player_OnPlayerInputRequested;
            m_playableCharacter.PlayerWidgetUpdated += Player_OnPlayerWidgetUpdated;
            m_playableCharacter.PlayerATBChanged += Player_OnPlayerATBChanged;

            m_playableCharacter.Stance.InitializeCurrentStance();
            if(StanceWidget != null)
            {
                StanceWidget.ChangeStanceImage((int)m_playableCharacter.Stance.GetCurrentStance());
            }
        }

        public PlayableCharacter GetPlayableCharacter()
        {
            return m_playableCharacter;
        }

        public void DisableEvents()
        {
            m_playableCharacter.PlayerInputRequested -= Player_OnPlayerInputRequested;
            m_playableCharacter.PlayerWidgetUpdated -= Player_OnPlayerWidgetUpdated;
            m_playableCharacter.PlayerATBChanged -= Player_OnPlayerATBChanged;
        }

        #region events
        private void Player_OnPlayerWidgetUpdated(Attribute attribute)
        {
            switch(attribute)
            {
                case Attribute.MaxHP:
                    HPBarWidget.UpdateValues((int)m_playableCharacter.GetAttribute(Attribute.HP), (int)m_playableCharacter.GetAttribute(Attribute.MaxHP));
                    break;
                case Attribute.MaxMP:
                    ManaBarWidget.UpdateValues((int)m_playableCharacter.GetAttribute(Attribute.MP), (int)m_playableCharacter.GetAttribute(Attribute.MaxMP));
                    break;
                case Attribute.MaxStamina:
                    StaminaBarWidget.UpdateValues((int)m_playableCharacter.GetAttribute(Attribute.Stamina), (int)m_playableCharacter.GetAttribute(Attribute.MaxStamina));
                    break;
            }
        }

        private void Player_OnPlayerATBChanged(float atb)
        {
            ATBBarWidget.UpdateValues((int)atb, 1000);
        }

        private void Player_OnPlayerInputRequested(bool waitStatus)
        {
            if (waitStatus)
            {
                ActionWindowRequested(m_playableCharacter);
                //AudioSource.PlayOneShot(ATBFullSoundClip);
                //ActionWidget.GetComponent<UI_ActionSelection_Widget>().Initialize(m_playableCharacter);
            }
        }
        #endregion
    }
}
