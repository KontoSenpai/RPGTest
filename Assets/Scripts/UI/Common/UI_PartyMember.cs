using MyBox;
using RPGTest.Enums;
using RPGTest.Inputs;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private Image Portrait;

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

        #region Public Events
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
            if (m_character == null)
            {
                Debug.LogWarning("null character");
                Debug.Break();
            }
            RefreshInternal(m_character, m_character.EquipmentSlots.CurrentPreset);
        }

        public void Refresh(PlayableCharacter character, PresetSlot slot)
        {
            RefreshInternal(character, slot);
        }

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
        #endregion

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

        #region Private Methods
        private void RefreshInternal(PlayableCharacter character, PresetSlot slot)
        {
            if (ContentPanel != null)
                ContentPanel.SetActive(true);
            
            Name.text = character.Name;
            LevelValue.text = character.Level.ToString();

            if (Portrait != null)
            {
                var portrait = ((Texture2D)Resources.Load($"Portraits/{character.Id}"));
                if (portrait != null)
                {
                    Portrait.sprite = Sprite.Create(portrait, new Rect(0.0f, 0.0f, portrait.width, portrait.height), new Vector2(0.5f, 0.5f), 100.0f);
                }
                else
                {
                    Portrait.sprite = null;
                }
            }

            if (HPBarWidget != null)
                HPBarWidget.UpdateValues(character.CurrentHP, character.GetAttribute(slot, Enums.Attribute.MaxHP));
            
            if (ManaBarWidget != null)
                ManaBarWidget.UpdateValues(character.CurrentMP, character.GetAttribute(slot, Enums.Attribute.MaxMP));

            if (ExperienceBarWidget != null)
                ExperienceBarWidget.UpdateValues(character.CurrentExperience, character.NextLevelExperience);

            if (StanceWidget != null)
                StanceWidget.ChangeStanceImage((int)character.Stance.GetCurrentStance());
        }
        #endregion
    }
}
