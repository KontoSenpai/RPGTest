using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Managers;
using RPGTest.Models.Items;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class ItemUpdate
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }

    public partial class UI_ItemListUpdator : MonoBehaviour
    {
        [SerializeField] protected GameObject ItemGo;
        [SerializeField] protected bool StackOwnersOnSingleGuiItem;
        [SerializeField] protected bool DisplayItemValue;

        private PartyManager m_partyManager => FindObjectOfType<GameManager>().PartyManager;

        public List<GameObject> InstantiateItems(Dictionary<string, int> items)
        {
            var guiItems = new List<GameObject>();
            foreach (var item in items)
            {
                var itemModel = ItemCollector.TryGetItem(item.Key);
                guiItems.AddRange(InstantiateItem(itemModel, item.Value));
            };

            return guiItems;
        }

        public List<GameObject> UpdateItems(List<GameObject> guiItems, List<ItemUpdate> itemUpdates)
        {
            List<GameObject> guiCDs = new List<GameObject>();
            foreach (var itemUpdate in itemUpdates)
            {
                guiCDs.AddRange(UpdateItem(guiItems, itemUpdate.Item, itemUpdate.Quantity));
            };

            return guiCDs;
        }

        /// <summary>
        /// Will instantiate the required amount of gui items for given item
        /// </summary>
        /// <param name="itemDisplay"></param>
        /// <returns></returns>
        public List<GameObject> InstantiateItem(Item item, int quantity)
        {
            switch (item.Type)
            {
                case ItemType.Equipment:
                    return InstantiateEquipment(item, quantity);
                default:
                    return InstantiateDefault(item, quantity);
            }
        }

        public List<GameObject> UpdateItem(List<GameObject> guiItems, Item item, int quantity)
        {
            switch (item.Type)
            {
                case ItemType.Equipment:
                    return UpdateEquipment(guiItems, item, quantity);
                default:
                    return UpdateDefault(guiItems, item, quantity);
            }
        }

        /// <summary>
        /// Instantiate a guiItem for given item
        /// </summary>
        /// <param name="item">Item for which to create a GuiItem</param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private GameObject InstantiateItemInternal(Item item)
        {
            var guiItem = Instantiate(ItemGo);
            guiItem.name = item.Id;
            guiItem.transform.localScale = new Vector3(1, 1, 1);

            return guiItem;
        }
    }
}
