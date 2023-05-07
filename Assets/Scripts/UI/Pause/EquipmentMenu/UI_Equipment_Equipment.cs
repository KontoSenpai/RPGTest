using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using TMPro;
using UnityEngine;

namespace RPGTest.UI
{
    public class UI_Equipment_Equipment : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI LeftHand;
        [SerializeField] private TextMeshProUGUI RightHand;

        [SerializeField] private TextMeshProUGUI Head;
        [SerializeField] private TextMeshProUGUI Body;

        [SerializeField] private TextMeshProUGUI Acessory1;
        [SerializeField] private TextMeshProUGUI Acessory2;

        private const string NO_EQUIPMENT_TEXT = "None";

        public void Refresh(PlayableCharacter character, PresetSlot slot)
        {
            if (character == null) return;

            var preset = character.EquipmentSlots.GetEquipmentPreset(slot);

            LeftHand.text = preset[Slot.LeftHand]?.Name ?? NO_EQUIPMENT_TEXT;
            RightHand.text = preset[Slot.RightHand]?.Name ?? NO_EQUIPMENT_TEXT;

            Head.text = preset[Slot.Head]?.Name ?? NO_EQUIPMENT_TEXT;
            Body.text = preset[Slot.Body]?.Name ?? NO_EQUIPMENT_TEXT;

            Acessory1.text = preset[Slot.Accessory1]?.Name ?? NO_EQUIPMENT_TEXT;
            Acessory2.text = preset[Slot.Accessory2]?.Name ?? NO_EQUIPMENT_TEXT;
        }
    }
}
