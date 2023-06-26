using MyBox;
using RPGTest.Models.Items;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_EquipmentCategory : MonoBehaviour
    {
        [Separator("Left Slot")]
        [SerializeField] private UI_InventoryItem LeftEquipmentComponent;
        [Separator("Right Slot")]
        [SerializeField] private UI_InventoryItem RightEquipmentComponent;

        public void InitializeSelection()
        {
            LeftEquipmentComponent.GetComponent<Button>().Select();
        }

        public void Refresh(Equipment LeftEquipment, Equipment RightEquipment)
        {
            Clean();

            LeftEquipmentComponent.Initialize(LeftEquipment, -1);
            RightEquipmentComponent.Initialize(RightEquipment, -1);
        }

        public void Clean()
        {
            LeftEquipmentComponent.Clean();
            RightEquipmentComponent.Clean();
        }

        public void SetInteractable(bool value)
        {
            LeftEquipmentComponent.GetComponent<Button>().interactable = value;
            RightEquipmentComponent.GetComponent<Button>().interactable = value;
        }
    }
}
