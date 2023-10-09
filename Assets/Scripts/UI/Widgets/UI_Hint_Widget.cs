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

        public void Create(string description, List<Texture2D[]> icons)
        {
            Description.text = description;

            for(int i = 0; i < icons.Count; i++)
            {
                GameObject lastIcon = null;
                icons[i].ForEach(i =>
                {
                    var iconGo = Instantiate(IconPrefab);
                    iconGo.transform.SetParent(IconsGo.transform);
                    iconGo.transform.localScale = new Vector3(1, 1, 1);
                    iconGo.GetComponent<RawImage>().texture = i;
                    iconGo.name = i.name;
                    lastIcon = iconGo;
                });
            }
        }
    }
}
