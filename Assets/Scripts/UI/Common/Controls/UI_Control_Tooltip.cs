using TMPro;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public class UI_Control_Tooltip : MonoBehaviour {

        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI Description;

        public void InitializeWithLocalization(string name, string lineID)
        {
            
        }

        public void Initialize(string name, string description)
        {
            Name.text = name;
            Description.text = description;
        }
    }
}