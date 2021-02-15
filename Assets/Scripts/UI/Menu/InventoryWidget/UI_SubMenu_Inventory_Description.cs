using System;
using TMPro;
using UnityEngine;
using RPGTest.Models.Items;

namespace RPGTest.UI.InventoryMenu
{
    public class UI_SubMenu_Inventory_Description : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI TXTItemName;
        [SerializeField] private TextMeshProUGUI TXTItemValue;
        [SerializeField] private TextMeshProUGUI TXTItemDescription;

        public void SetVisible(bool visible)
        {
            TXTItemName.gameObject.SetActive(visible);
            TXTItemValue.gameObject.SetActive(visible);
            TXTItemDescription.gameObject.SetActive(visible);
        }

        public void Refresh(Item item)
        {
            TXTItemName.text = item.Name;
            TXTItemValue.text = (item.Value / 4).ToString();
            TXTItemDescription.text = item.Description;
        }
    }
}
