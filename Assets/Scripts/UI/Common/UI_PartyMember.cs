using MyBox;
using RPGTest.Enums;
using RPGTest.Inputs;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// Display of a party member.
    /// </summary>
    public class UI_PartyMember : AdvancedButton
    {
        [SerializeField] private GameObject ContentPanel;
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI LevelValue;
        [SerializeField] private GameObject Cover;
        [SerializeField] private UI_Stances_Widget StanceWidget;

        [Separator("Bars")]
        [SerializeField] private UI_Bar_Widget HPBarWidget;
        [SerializeField] private UI_Bar_Widget ManaBarWidget;
        [SerializeField] private UI_Bar_Widget ExperienceBarWidget;

        [HideInInspector]
        public MemberSelectedHandler MemberSelected { get; set; }
        [HideInInspector]
        public delegate void MemberSelectedHandler(UIActionSelection selection, GameObject item);

        [HideInInspector]
        public EquipmentSlotSelectedHandler EquipmentSlotSelected { get; set; }
        [HideInInspector]
        public delegate void EquipmentSlotSelectedHandler(PresetSlot preset, Slot slot);

        public bool TwoStepSelect = false;

        private PlayableCharacter m_character;
        private Action m_secondaryAction;

        public void Initialize(PlayableCharacter character)
        {
            m_character = character;
            
            if (Cover.activeSelf)
            {
                ToggleCover();
            }

            if (m_character == null)
            {
                ContentPanel.SetActive(false);
                return;
            }

            Refresh();
        }

        public PlayableCharacter GetCharacter()
        {
            return m_character;
        }

        public void Refresh()
        {
            ContentPanel.SetActive(true);
            Name.text = m_character.Name;
            LevelValue.text = m_character.Level.ToString();

            HPBarWidget.UpdateValues(m_character.CurrentHP, m_character.BaseAttributes.MaxHP);
            ManaBarWidget.UpdateValues(m_character.CurrentMP, m_character.BaseAttributes.MaxMP);
            ExperienceBarWidget.UpdateValues(m_character.CurrentExperience, m_character.NextLevelExperience);

            if (StanceWidget != null)
            {
                StanceWidget.ChangeStanceImage((int)m_character.Stance.GetCurrentStance());
            }
        }


        #region Input Events
        // Select for swap
        public void PrimaryAction_Selected()
        {
            if (TwoStepSelect)
            {
                m_secondaryAction.Invoke();
            }
            MemberSelected(UIActionSelection.Primary, gameObject);
        }

        // Select for SubMenu
        public void SecondaryAction_Selected()
        {
            if (TwoStepSelect)
            {
                m_secondaryAction.Invoke();
            }
            MemberSelected(UIActionSelection.Secondary, gameObject);
        }
        #endregion



        public void SetSecondarySelect(Action action)
        {
            m_secondaryAction = action;
        }

        public void ToggleCover()
        {
            Cover.SetActive(!Cover.activeSelf);
        }

        public void SelectSlot(Slot selectedSlot)
        {
            //EquipmentSlotSelected(this.gameObject, selectedSlot);
        }

        public void RefreshExperience()
        {
            ExperienceBarWidget.UpdateValues(m_character.CurrentExperience, m_character.NextLevelExperience);
            LevelValue.text = m_character.Level.ToString();
        }
    }
}
