using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.UI.Widgets;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.PartyMenu
{
    public class UI_PresetStats_Widget : MonoBehaviour
    {
        public string BackendName;
        public TextMeshProUGUI EnduranceValue;

        public TextMeshProUGUI AttackValue;
        public TextMeshProUGUI MagicValue;
        public TextMeshProUGUI DefenseValue;
        public TextMeshProUGUI ResistanceValue;

        public PresetSlot PresetSlot;
    }
}
