using RPGTest.Collectors;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_Control_Tooltip : MonoBehaviour {

        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI Description;

        public void Initialize(string name, string description)
        {
            Name.text = LocalizationCollectors.TryGetLocalizedLine(name, out string localizedName) ? localizedName : name;
            Description.text = LocalizationCollectors.TryGetLocalizedLine(description, out string localizedDescription) ? localizedDescription : description;
        }
    }
}