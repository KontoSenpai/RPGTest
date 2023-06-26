using MyBox;
using RPGTest.Enums;
using RPGTest.Models.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public class UI_Item_Informations : MonoBehaviour
    {
        [Separator("Base Information")]
        [SerializeField] private Image Image;
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI SubType;

        [Separator("Detailled Information")]
        [SerializeField] private TextMeshProUGUI CurrencyValue;
        [SerializeField] private Image ImgCurrency;
        [SerializeField] private TextMeshProUGUI Description;

        public void Awake(){}

        public virtual void OnEnable(){}

        public virtual void OnDisable() {}

        public void SetVisible(bool visible)
        {
        }

        public void Refresh(Item item)
        {
            Name.text = item.Name;
            var subType = "";
            switch (item.Type)
            {
                case ItemType.Consumable:
                    subType = ((Consumable)item).ConsumableType.ToString();
                    break;
                case ItemType.Equipment:
                    subType = ((Equipment)item).EquipmentType.ToString();
                    break;
                default:
                    subType = item.Type.ToString();
                    break;
            }
            SubType.text = subType.ToString();

            if (item.Value > 0)
                CurrencyValue.text = (item.Value / 4).ToString();
            else
            {
                CurrencyValue.text = "Untradable";
                ImgCurrency.enabled = false;
            }

            Description.text = item.Description;
        }
    }
}
