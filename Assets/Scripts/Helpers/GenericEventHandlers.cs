using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Items;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Helpers
{
    public delegate void CancelActionHandler();

    public delegate void ItemsSelectionHandler(List<Item> items);

    public delegate void SlotSelectionHandler(PresetSlot preset, Slot slot);

    public delegate void ItemSelectionHandler(GameObject guiItem);
}
