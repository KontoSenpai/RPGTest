using UnityEngine;
using UnityEngine.UI;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using TMPro;

namespace RPGTest.UI.Battle
{
    public class UI_PartyMember_Widget : MonoBehaviour
    {
        #region Properties
        public GameObject ActionWidget;
 
        public TextMeshProUGUI Name;
        public TextMeshProUGUI LevelValue;

        public UI_Bar_Widget HPBarWidget;
        public UI_Bar_Widget ManaBarWidget;
        public UI_Bar_Widget StaminaBarWidget;
        public UI_Bar_Widget ATBBarWidget;
        public AudioSource AudioSource;
        public AudioClip ATBFullSoundClip;

        public UI_Stances_Widget StanceWidget;
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

            HPBarWidget.Initialize("HP", (int)m_playableCharacter.GetAttribute("CurrentHP"), m_playableCharacter.BaseAttributes.MaxHP);
            ManaBarWidget.Initialize("Mana", (int)m_playableCharacter.GetAttribute("CurrentMP"), m_playableCharacter.BaseAttributes.MaxMP);
            StaminaBarWidget.Initialize("Stamina", (int)m_playableCharacter.GetAttribute("CurrentStamina"), m_playableCharacter.BaseAttributes.MaxStamina);
            ATBBarWidget.Initialize("ATB", (int)0, 1000);

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
        private void Player_OnPlayerWidgetUpdated(Enums.Attribute attribute)
        {
            switch(attribute)
            {
                case Enums.Attribute.HP:
                    HPBarWidget.UpdateValues((int)m_playableCharacter.GetAttribute("CurrentHP"), m_playableCharacter.BaseAttributes.MaxHP);
                    break;
                case Enums.Attribute.MP:
                    ManaBarWidget.UpdateValues((int)m_playableCharacter.GetAttribute("CurrentMP"), m_playableCharacter.BaseAttributes.MaxMP);
                    break;
                case Enums.Attribute.Stamina:
                    StaminaBarWidget.UpdateValues((int)m_playableCharacter.GetAttribute("CurrentStamina"), m_playableCharacter.BaseAttributes.MaxStamina);
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
                AudioSource.PlayOneShot(ATBFullSoundClip);
                ActionWidget.GetComponent<UI_ActionSelection_Widget>().Initialize(m_playableCharacter);
            }
        }
        #endregion
    }
}
