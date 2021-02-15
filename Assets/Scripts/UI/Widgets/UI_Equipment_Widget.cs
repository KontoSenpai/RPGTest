using RPGTest.Enums;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_Equipment_Widget : MonoBehaviour
    {
        public Image HeadImage;
        public TextMeshProUGUI HeadValue;

        public Image BodyImage;
        public TextMeshProUGUI BodyValue;

        public Image Accessory1Image;
        public TextMeshProUGUI Accessory1Value;

        public Image Accessory2Image;
        public TextMeshProUGUI Accessory2Value;

        public void DisableWidget()
        {
            foreach(var button in this.gameObject.GetComponents<Button>().Union(this.gameObject.GetComponentsInChildren<Button>()))
            {
                button.interactable = false;
            }
        }

        public void UpdateEquipment(Dictionary<Slot, Equipment> equipment)
        {
            HeadValue.text = equipment[Slot.Head]?.Name ?? "Empty";
            BodyValue.text = equipment[Slot.Body]?.Name ?? "Empty";
            Accessory1Value.text = equipment[Slot.Body]?.Name ?? "Empty";
            Accessory2Value.text = equipment[Slot.Body]?.Name ?? "Empty";
        }
    }
}
