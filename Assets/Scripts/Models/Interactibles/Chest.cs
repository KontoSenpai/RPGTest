using System.Collections.Generic;

namespace RPGTest.Models.Interactibles
{
    public class Chest : IdObject
    {
        public Dictionary<string, int> Inventory { get; set; }
    }
}
