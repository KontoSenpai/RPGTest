using MyBox;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_EquipmentCategory : MonoBehaviour
    {
        [SerializeField] GameObject InstantiableStatPanel;
        private List<GameObject> InstantiatedStatPanels = new List<GameObject>();
        [Separator("Left Slot")]
        [SerializeField] private Image LeftImage;
        [SerializeField] private TextMeshProUGUI LeftItemName;
        [SerializeField] private GameObject LeftStatsPanel;
        [Separator("Right Slot")]
        [SerializeField] private Image RightImage;
        [SerializeField] private TextMeshProUGUI RightItemName;
        [SerializeField] private GameObject RightStatsPanel;

        public void Clean()
        {
            InstantiatedStatPanels.ForEach(p => Destroy(p));
            InstantiatedStatPanels.Clear();
        }

        public void Refresh(Equipment LeftEquipment, Equipment RightEquipment)
        {
            Clean();
            if(LeftEquipment != null)
            {
                LeftItemName.text = LeftEquipment.Name;
                foreach (KeyValuePair<Attribute, int> attribute in LeftEquipment.Attributes)
                {
                    GameObject instantiatedObject = Instantiate(InstantiableStatPanel);
                    instantiatedObject.GetComponent<UI_StatWidget>().SetStatText(attribute.Key.GetDescription(), attribute.Value);
                    instantiatedObject.transform.parent = LeftStatsPanel.transform;
                    InstantiatedStatPanels.Add(instantiatedObject);
                }
            }
            else
            {
                LeftItemName.text = "Empty";
            }

            if (RightEquipment != null)
            {
                RightItemName.text = RightEquipment.Name;
                foreach (KeyValuePair<Attribute, int> attribute in RightEquipment.Attributes)
                {
                    GameObject instantiatedObject = Instantiate(InstantiableStatPanel);
                    instantiatedObject.GetComponent<UI_StatWidget>().SetStatText(attribute.Key.GetDescription(), attribute.Value);
                    instantiatedObject.transform.parent = RightStatsPanel.transform;
                    InstantiatedStatPanels.Add(instantiatedObject);
                }
            }
            else
            {
                RightItemName.text = "Empty";
            }
        }
    }
}
