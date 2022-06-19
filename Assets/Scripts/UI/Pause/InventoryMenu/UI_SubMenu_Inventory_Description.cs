using System;
using TMPro;
using UnityEngine;
using RPGTest.Models.Items;
using UnityEngine.UI;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_Description : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI TXTItemValue;
        [SerializeField] private Image ImgCurrency;
        [SerializeField] private TextMeshProUGUI TXTItemDescription;

        public void SetVisible(bool visible)
        {
            TXTItemValue.gameObject.SetActive(visible);
            TXTItemDescription.gameObject.SetActive(visible);
        }

        public void Refresh(Item item)
        {
            if (item.Value > 0)
                TXTItemValue.text = (item.Value / 4).ToString();
            else
            {
                TXTItemValue.text = "Untradable";
                ImgCurrency.enabled = false;
            }
                

            TXTItemDescription.text = item.Description;
        }
    }
}
