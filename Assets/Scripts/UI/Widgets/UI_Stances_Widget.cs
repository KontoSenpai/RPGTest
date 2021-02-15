using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_Stances_Widget : MonoBehaviour
    {
        public List<Sprite> StancesImages;
        public Image CurrentStance;


        public void ChangeStanceImage(int index)
        {
            if (index < 0 || index >= StancesImages.Count)
                return;

            CurrentStance.sprite = StancesImages[index];
        }
    }
}
