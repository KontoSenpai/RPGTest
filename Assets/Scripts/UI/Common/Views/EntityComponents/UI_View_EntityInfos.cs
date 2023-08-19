using RPGTest.Enums;
using RPGTest.Inputs;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// Display informations about an Entity
    /// </summary>
    public class UI_View_EntityInfos : AdvancedButton
    {
        [SerializeField] private GameObject ContentPanel;

        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI LevelValue;

        [SerializeField] private Image Portrait;

        [SerializeField] private GameObject Cover;
        [SerializeField] private UI_Stances_Widget StanceWidget;

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

        private Entity m_entity;
        private PresetSlot m_preset;

        private Action m_secondaryAction;

        #region Public Events
        public void Initialize(Entity entity, PresetSlot preset)
        {
            m_entity = entity;
            m_preset = preset;
            
            if (Cover != null && Cover.activeSelf)
            {
                ToggleCover();
            }

            if (m_entity == null)
            {
                ContentPanel.SetActive(false);
                return;
            }

            Refresh();
        }

        public Entity GetEntity()
        {
            return m_entity;
        }

        public PlayableCharacter GetPlayableCharacter()
        {
            return (PlayableCharacter)m_entity;
        }

        public PresetSlot GetPresetSlot()
        {
            return m_preset;
        }    

        public void Refresh()
        {
            if (m_entity == null)
            {
                Debug.LogWarning("null character");
                Debug.Break();
            }

            var attributes = m_entity.GetAttributes();
            if (m_entity.GetType() == typeof(PlayableCharacter))
            {
                var character = (PlayableCharacter)m_entity;
                attributes = character.GetAttributes(m_preset);
            }
            RefreshInternal(m_entity, attributes);
        }

        public void Refresh(Entity character, PresetSlot slot)
        {
            //RefreshInternal(character, slot);
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
            RefreshExperienceInternal((PlayableCharacter)m_entity);
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
        private void RefreshExperienceInternal(PlayableCharacter character)
        {
            ExperienceBarWidget.UpdateValues(character.CurrentExperience, character.NextLevelExperience);
            LevelValue.text = m_entity.Level.ToString();
        }

        private void RefreshInternal(Entity entity, Dictionary<Enums.Attribute, float> attributes)
        {
            if (ContentPanel != null)
                ContentPanel.SetActive(true);

            Name.text = entity.Name;

            if (Portrait != null)
            {
                var portrait = ((Texture2D)Resources.Load($"Portraits/{entity.Id}"));
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
                HPBarWidget.UpdateValues(entity.CurrentHP, attributes[Enums.Attribute.MaxHP]);

            if (ManaBarWidget != null)
                ManaBarWidget.UpdateValues(entity.CurrentMP, attributes[Enums.Attribute.MaxMP]);

            if (StanceWidget != null)
                StanceWidget.ChangeStanceImage((int)entity.Stance.GetCurrentStance());
        }

        private void RefreshInternal(PlayableCharacter character, PresetSlot slot)
        {
            RefreshInternal(character, character.GetAttributes(slot));
            
            LevelValue.text = character.Level.ToString();
            character.GetAttributes(slot);

            if (ExperienceBarWidget != null)
            {
                RefreshExperienceInternal(character);
            }
        }
        #endregion
    }
}
