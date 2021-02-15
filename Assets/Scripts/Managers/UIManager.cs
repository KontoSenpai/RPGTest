using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using RPGTest.UI;

namespace RPGTest.Managers
{

    public class UIManager : MonoBehaviour
    {
        public List<UI_Base> UIScripts;

        [HideInInspector]
        public MenuChangedHandler MenuChanged { get; set; }
        [HideInInspector]
        public delegate void MenuChangedHandler(bool inMenu);

        public void Start()
        {            
            foreach(var script in UIScripts)
            {
                script.UIOpened += OnUIOpened;
                script.UIClosed += OnUIClosed;
            }
        }

        public T GetUIComponent<T>()
        {
            var uiScript = UIScripts.SingleOrDefault(x => x.GetType() == typeof(T));
            return (T) Convert.ChangeType(uiScript, typeof(T));
        }

        public void OnUIOpened(object sender, EventArgs e)
        {
            UIScripts.ForEach(x => x.gameObject.SetActive((UI_Base)sender == x));
            MenuChanged(true);
        }

        public void OnUIClosed(object sender, EventArgs e)
        {
            UIScripts.SingleOrDefault(x => (UI_Base)sender == x).gameObject.SetActive(false);
            UIScripts[0].gameObject.SetActive(true);
            MenuChanged(false);
        }
    }
}
