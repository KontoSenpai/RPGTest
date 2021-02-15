using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RPGTest.Managers;
using RPGTest.Models.Items;
using RPGTest.UI;
using UnityEditor;
using UnityEngine;

namespace RPGTest.UnitTests.Shop
{
    [TestFixture]
    public class ShopUnitTests
    {
        private InventoryManager m_inventoryManager => Object.FindObjectOfType<InventoryManager>();
        private UI_Shop m_shop;
        private UI_ItemInteraction m_shop_purchase;

        public static List<Item> items = new List<Item>()
        {
            new Item()
            {
                Id = "Test01",
                Name = "TestObject1",
                Value = 50
            },
            new Item()
            {
                Id = "Test02",
                Name = "TestObject2",
                Value = 1200
            },
        };


        private void InitializeScene()
        {
            //GameManager
            var gameManager = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/GameManager.prefab", typeof(GameObject));
            Object.Instantiate((GameObject)gameManager);

            GameObject shopObject = InitializeShop();
            m_shop = shopObject.GetComponent<UI_Shop>();
            m_shop_purchase = m_shop.PurchaseWindow.GetComponent<UI_ItemInteraction>();
        }

        private GameObject InitializeShop()
        {
            var shopPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/UI/Shop/UI_Shop_Panel_Menu.prefab", typeof(GameObject));

            var gameObject = Object.Instantiate((GameObject)shopPrefab);

            return gameObject;
        }

        [Test]
        public void AddItemToShopPass()
        {
            InitializeScene();

            m_shop.Initialize(items);

            Assert.AreEqual(items.Select(x => x.Id), m_shop.GetItemsId());
        }

        [Test]
        public void ChangeItemQuantityToPurchasePass()
        {
            InitializeScene();

            //Normal, will bring the purchase quantity to 10
            m_inventoryManager.TryUpdateMoney(9000);
            m_shop_purchase.Initialize(InteractionType.Purchase, items[0]);
            Assert.True(m_shop_purchase.TryChangePurchaseQuantity(10));

            //Anormal, will bring the purchase quantity to -1
            m_shop_purchase.Initialize(InteractionType.Purchase, items[0]);
            Assert.False(m_shop_purchase.TryChangePurchaseQuantity(-1));

            m_shop_purchase.Initialize(InteractionType.Purchase, items[0]);
            //Normal, will bring the purchase quantity to 50
            Assert.True(m_shop_purchase.TryChangePurchaseQuantity(50));
            //Anormal, will bring the purchase quantity to 100
            Assert.False(m_shop_purchase.TryChangePurchaseQuantity(50));
        }

        [Test]
        public void PurchaseItemPass()
        {
            InitializeScene();

            AddItemToShopPass();

            m_inventoryManager.TryUpdateMoney(9900);
            //Normal, will bring money to 9950 and item stack to 1
            Assert.True(m_shop.TryExecuteTransaction(items[0], 1));

            //Anormal, will bring money below 0
            Assert.False(m_shop.TryExecuteTransaction(items[1], 12));

            m_inventoryManager.TryUpdateMoney(10000);
            //Anormal, will bring item stack to 100
            Assert.False(m_shop.TryExecuteTransaction(items[0], 99));
        }
    }
}
