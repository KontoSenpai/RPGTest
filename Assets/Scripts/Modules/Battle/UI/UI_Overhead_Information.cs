using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RPGTest.Modules.Battle.UI
{
    public class UI_Overhead_Information : MonoBehaviour
    {
        [SerializeField] private GameObject BuffsList;
        [SerializeField] private GameObject DebuffsList;

        [SerializeField] private GameObject BuffInstantiate;

        [SerializeField] private TextMeshProUGUI EntityLabel;

        private List<GameObject> m_buffWidgets = new List<GameObject>();
        private List<GameObject> m_debuffWidgets = new List<GameObject>();

        private const string m_labelFormat = "LVL {0} - {1}";

        public void Initialize(Entity entity)
        {
            EntityLabel.text = string.Format(m_labelFormat, entity.Level.ToString(), entity.Name);
            entity.BuffsRefreshed += OnBuffsRefreshed;
        }

        private void OnBuffsRefreshed(object sender, BuffsRefreshedArgs e)
        {
            RefreshBuffPresentation(e.Buffs, m_buffWidgets, BuffsList);
            RefreshBuffPresentation(e.Debuffs, m_debuffWidgets, DebuffsList);
        }

        private void RefreshBuffPresentation(List<Buff> buffs, List<GameObject> widgets, GameObject parent)
        {
            // Remove widgets that don't have buff anymore
            for (int i = 0; i < widgets.Count; i++)
            {
                var buff = buffs.FirstOrDefault(b => b.Id == widgets[i].GetComponent<UI_Combat_Buff_Widget>().GetId());
                if (buff == null)
                {
                    Destroy(widgets[i]);
                    widgets.RemoveAt(i);
                }
                else
                {
                    widgets[i].GetComponent<UI_Combat_Buff_Widget>().Initialize(buff);
                }
            }

            // Create new widgets
            foreach (var buff in buffs)
            {
                if (!widgets.Any(w => w.GetComponent<UI_Combat_Buff_Widget>().GetId() == buff.Id))
                {
                    InstantiateWidget(buff, parent);
                }
            }
        }

        private GameObject InstantiateWidget(Buff buff, GameObject parent)
        {
            var widget = Instantiate(BuffInstantiate);
            widget.transform.SetParent(parent.transform);
            widget.name = buff.Id;
            widget.transform.localScale = new Vector3(1, 1, 1);
            var camera = FindObjectOfType<Camera>();
            if (camera != null)
            {
                widget.gameObject.transform.LookAt(-1 * camera.gameObject.transform.position);
            }

            var position = widget.transform.localPosition;
            position.z = 0;
            widget.transform.localPosition = position;

            widget.GetComponent<UI_Combat_Buff_Widget>().Initialize(buff);
            m_buffWidgets.Add(widget);
            return widget;
        }

        /// <summary>
        /// Called on battle exit
        /// </summary>
        /// <param name="playableCharacter">Entity to whom the widget was connected to</param>
        public void DisableEvents(Entity playableCharacter)
        {
            playableCharacter.BuffsRefreshed -= OnBuffsRefreshed;
        }
    }
}
