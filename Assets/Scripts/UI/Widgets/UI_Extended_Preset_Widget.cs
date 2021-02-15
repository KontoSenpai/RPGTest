using RPGTest.Enums;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_Extended_Preset_Widget : MonoBehaviour
    {
        public Button Item1;
        public Image LeftHandImage;
        public TextMeshProUGUI LeftHandValue;

        public Button Item2;
        public Image RightHandImage;
        public TextMeshProUGUI RightHandValue;

        public void DisablePreset()
        {
            this.gameObject.GetComponent<Button>().interactable = false;
        }

        public void ClearListeners()
        {
            Item1.onClick.RemoveAllListeners();
            Item2.onClick.RemoveAllListeners();
        }

        public void UpdatePreset(Dictionary<Slot, Equipment> preset)
        {
            LeftHandValue.text = preset[Slot.LeftHand]?.Name ?? "Empty";
            RightHandValue.text = preset[Slot.RightHand]?.Name ?? "Empty";
        }
    }
}
