using RPGTest.Helpers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_Hint_Widget: MonoBehaviour
    {
        [SerializeField] private GameObject IconPrefab;
        [SerializeField] private GameObject IconsGo;
        [SerializeField] private TextMeshProUGUI Description;

        private List<GameObject> m_icons;

        public void Create(Texture2D[] icons, string description)
        {
            Description.text = description;

            icons.ForEach(icon =>
            {
                var iconGo = Instantiate(IconPrefab);
                iconGo.transform.SetParent(IconsGo.transform);
                iconGo.transform.localScale = new Vector3(1, 1, 1);
                iconGo.GetComponent<RawImage>().texture = icon;
            });
        }
    }
}
