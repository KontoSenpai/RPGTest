using RPGTest.Managers;
using RPGTest.UI.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.UI
{
    public class UI_Controls_Display: MonoBehaviour
    {
        [SerializeField]
        private GameObject HintWidgetObject;

        private List<GameObject> HintWidgets { get; set; }

        public void Awake()
        {
            HintWidgets = new List<GameObject>();
        }

        public void Refresh(List<InputDisplay> inputs)
        {
            HintWidgets.ForEach(go => Destroy(go));
            HintWidgets.Clear();
            inputs.ForEach(i =>
            {
                var widget = Instantiate(HintWidgetObject);
                widget.transform.SetParent(this.transform);
                widget.transform.localScale = new Vector3(1, 1, 1);
                widget.name = i.Description;
                widget.GetComponent<UI_Hint_Widget>().Create(i.Description, i.Icons);
                HintWidgets.Add(widget);
            });
        }
    }
}
