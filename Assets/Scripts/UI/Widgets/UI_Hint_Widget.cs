using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_Hint_Widget: MonoBehaviour
    {
        [SerializeField] private GameObject IconFolder;
        [SerializeField] private TextMeshProUGUI Description;

        private List<GameObject> m_icons;

        public void Create(string[] icons, string description)
        {
            // TODO : for next time, instanciate icons in the horizontal layout

            Description.text = description;
        }
    }
}
