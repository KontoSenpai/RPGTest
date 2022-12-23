using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace RPGTest.Modules.Battle.UI
{
    public class UI_PanelMulticast : MonoBehaviour
    {
        public TextMeshProUGUI ActionName;
        private Animator m_animator;

        public void Start()
        {
            m_animator = GetComponent<Animator>();
        }

        public void SetEnabled(bool enabled)
        {
            m_animator.SetBool("Enabled", enabled);
        }

        public void SetSelected(bool selected)
        {
            m_animator.SetBool("Selected", selected);
        }
        
        public void SetActionName(string actionName)
        {
            ActionName.text = actionName;
        }
    }
}
