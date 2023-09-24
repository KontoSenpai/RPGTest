using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Items;
using System.Collections.Generic;

namespace RPGTest.Helpers
{
    public delegate void CancelActionHandler();

    public delegate void ItemsSelectionHandler(List<Item> items);

    public delegate void SlotSelectionHandler(PresetSlot preset, Slot slot);
}
