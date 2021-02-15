using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using System;
using TMPro;
using UnityEngine;

namespace RPGTest.UI
{
    public class UI_Member_Widget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI LevelValue;

        [SerializeField] private UI_Bar_Widget HPBarWidget;
        [SerializeField] private UI_Bar_Widget ManaBarWidget;
        [SerializeField] private UI_Bar_Widget ExperienceBarWidget;

        [SerializeField] private UI_Stances_Widget StanceWidget;

        [SerializeField] private UI_Extended_Preset_Widget PresetWidget1;
        [SerializeField] private UI_Extended_Preset_Widget PresetWidget2;
        [SerializeField] private UI_Equipment_Widget EquipmentWidget;

        [SerializeField] private GameObject Cover;

        [HideInInspector]
        public MemberSelectedHandler MemberSelected { get; set; }
        [HideInInspector]
        public delegate void MemberSelectedHandler(GameObject item);

        [HideInInspector]
        public EquipmentSlotSelectedHandler EquipmentSlotSelected { get; set; }
        [HideInInspector]
        public delegate void EquipmentSlotSelectedHandler(GameObject item, Slot slot);

        public bool TwoStepSelect = false;

        private PlayableCharacter m_character;
        private Action m_secondaryAction;

        public void Initialize(PlayableCharacter character)
        {
            m_character = character;
            Refresh();
        }

        public PlayableCharacter GetCharacter()
        {
            return m_character;
        }

        public void SetPresetsNavigation()
        {
            PresetWidget1.Item1.ExplicitNavigation(Right: PresetWidget2.Item1, Down: PresetWidget1.Item2);
            PresetWidget1.Item2.ExplicitNavigation(Right: PresetWidget2.Item2, Up: PresetWidget1.Item1);
            PresetWidget2.Item1.ExplicitNavigation(Left: PresetWidget1.Item1, Down: PresetWidget2.Item2);
            PresetWidget2.Item2.ExplicitNavigation(Left: PresetWidget1.Item2, Up: PresetWidget2.Item1);
        }


        public void Refresh()
        {
            Name.text = m_character.Name;
            LevelValue.text = m_character.Level.ToString();

            HPBarWidget.UpdateValues(m_character.CurrentHP, m_character.BaseAttributes.MaxHP);
            ManaBarWidget.UpdateValues(m_character.CurrentMP, m_character.BaseAttributes.MaxMP);
            if (ExperienceBarWidget != null)
            {
                ExperienceBarWidget.UpdateValues(m_character.CurrentExperience, m_character.NextLevelExperience);
            }

            if (StanceWidget != null)
            {
                StanceWidget.ChangeStanceImage((int)m_character.Stance.GetCurrentStance());
            }

            if (PresetWidget1 != null)
            {
                PresetWidget1.UpdatePreset(m_character.EquipmentSlots.GetWeaponPreset(PresetSlot.First));
            }

            if (PresetWidget2 != null)
            {
                PresetWidget2.UpdatePreset(m_character.EquipmentSlots.GetWeaponPreset(PresetSlot.Second));
            }

            if(EquipmentWidget != null)
            {
                EquipmentWidget.UpdateEquipment(m_character.EquipmentSlots.GetCurrentEquipmentPreset());
            }
        }

        public void Select()
        {
            if(TwoStepSelect)
            {
                m_secondaryAction.Invoke();
            }
            MemberSelected(this.gameObject);
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
            EquipmentSlotSelected(this.gameObject, selectedSlot);
        }

        public void SelectWeapon()
        {
            EquipmentWidget.DisableWidget();
            PresetWidget1.ClearListeners();
            PresetWidget2.ClearListeners();
            PresetWidget1.Item1.onClick.AddListener(() => SelectSlot(Slot.LeftHand1));
            PresetWidget1.Item2.onClick.AddListener(() => SelectSlot(Slot.RightHand1));

            PresetWidget2.Item1.onClick.AddListener(() => SelectSlot(Slot.LeftHand2));
            PresetWidget2.Item2.onClick.AddListener(() => SelectSlot(Slot.RightHand2));

            PresetWidget1.Item1.Select();
        }

        public void RefreshExperience()
        {
            ExperienceBarWidget.UpdateValues(m_character.CurrentExperience, m_character.NextLevelExperience);
            LevelValue.text = m_character.Level.ToString();
        }
    }
}
