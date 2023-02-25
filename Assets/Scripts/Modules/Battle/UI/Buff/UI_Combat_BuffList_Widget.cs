using RPGTest.Models;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Modules.Battle.UI
{
    public class UI_Combat_BuffList_Widget : MonoBehaviour
    {
        [SerializeField] private GameObject BuffsList;
        [SerializeField] private GameObject DebuffsList;

        [SerializeField] private GameObject BuffInstantiate;

        private List<GameObject> m_allWidgets = new List<GameObject>();

        public void Initialize(PlayableCharacter playableCharacter)
        {
            playableCharacter.BuffsRefreshed += OnBuffsRefreshed;
        }

        private void OnBuffsRefreshed(object sender, BuffsRefreshedArgs e)
        {
            // Remove widgets that don't have buff anymore
            for(int i = 0; i < m_allWidgets.Count; i++)
            {
                var buff = e.Buffs.FirstOrDefault(b => b.Id == m_allWidgets[i].GetComponent<UI_Combat_Buff_Widget>().GetId());
                if (buff == null)
                {
                    Destroy(m_allWidgets[i]);
                    m_allWidgets.RemoveAt(i);
                }
                else
                {
                    m_allWidgets[i].GetComponent<UI_Combat_Buff_Widget>().Initialize(buff);
                }
            }

            // Create new widgets
            foreach(var buff in e.Buffs)
            {
               if (!m_allWidgets.Any(w => w.GetComponent<UI_Combat_Buff_Widget>().GetId() == buff.Id))
                {
                    InstantiateBuff(buff);
                }
            }
        }

        private GameObject InstantiateBuff(Buff buff)
        {
            var widget = Instantiate(BuffInstantiate);
            widget.transform.SetParent(BuffsList.transform);
            widget.name = buff.Id;
            widget.transform.localScale = new Vector3(1, 1, 1);

            widget.GetComponent<UI_Combat_Buff_Widget>().Initialize(buff);
            m_allWidgets.Add(widget);
            return widget;
        }

        /// <summary>
        /// Called on battle exit
        /// </summary>
        /// <param name="playableCharacter">Entity to whom the widget was connected to</param>
        public void DisableEvents(PlayableCharacter playableCharacter)
        {
            playableCharacter.BuffsRefreshed -= OnBuffsRefreshed;
        }
    }
}
