using RPGTest.Models.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public partial class UI_ItemListUpdator : MonoBehaviour
    {
        /// <summary>
        /// Default Iem instantiation
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private List<GameObject> InstantiateDefault(Item item, int quantity)
        {
            var guiItem = InstantiateItemInternal(item);
            guiItem.GetComponent<UI_InventoryItem>().Initialize(item, quantity);

            return new List<GameObject> { guiItem };
        }

        private List<GameObject> UpdateDefault(List<GameObject> guiItems, Item item, int quantity)
        {
            var guiCDs = new List<GameObject>();
            var guiItem = guiItems.SingleOrDefault((i) => i.GetComponent<UI_InventoryItem>().Item.Id == item.Id);
            if (guiItem != null)
            {
                guiItem.GetComponent<UI_InventoryItem>().UpdateHeldQuantity(quantity);
                if (quantity == 0)
                {
                    guiCDs.Add(guiItem);
                }
            } else if(quantity > 0)
            {
                guiItem = InstantiateItemInternal(item);
                guiItem.GetComponent<UI_InventoryItem>().Initialize(item, quantity);
                guiCDs.Add(guiItem);
            }

            return guiCDs;
        }
    }
}
