using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Helpers
{
    public delegate void CancelActionHandler();

    public delegate void ItemsSelectionHandler(List<Item> items);

    public delegate void SlotSelectionHandler(PresetSlot preset, EquipmentSlot slot);

    public delegate void GameObjectSelectionHandler(GameObject guiItem);

    public delegate void GameObjectActionSelectionHandler(GameObject guiItem, UIActionSelection actionSelection);

    public delegate void CharacterEnumActionSelectionHandler(PlayableCharacter character, Enum enumAction);

    public delegate void EnumActionSelectionHandler(Enum enumAction);
}
