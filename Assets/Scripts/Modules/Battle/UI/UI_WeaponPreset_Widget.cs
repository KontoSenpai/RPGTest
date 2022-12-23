using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGTest.Modules.Battle.UI
{
    public class UI_WeaponPreset_Widget : MonoBehaviour
    {
        public GameObject FirstPreset;

        public GameObject SecondPreset;

        public void TogglePreset()
        {
            FirstPreset.SetActive(!FirstPreset.activeSelf);
            SecondPreset.SetActive(!SecondPreset.activeSelf);
        }

        public void Initialize(bool isFirstPreset)
        {
            FirstPreset.SetActive(isFirstPreset);
            SecondPreset.SetActive(!isFirstPreset);
        }
    }
}
