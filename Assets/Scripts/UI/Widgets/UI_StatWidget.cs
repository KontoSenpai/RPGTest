using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace RPGTest.UI.Widgets
{
    public class UI_StatWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI Stat;

        public void SetStatText(string attribute, int value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(attribute);
            builder.Append(value > 0 ? $"+{value}" : $"{value}");
            Stat.text = builder.ToString();
        }
    }
}
