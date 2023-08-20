using System;
using UnityEngine;

namespace RPGTest.UI.Common
{
    public abstract class UI_Dialog : UI_View
    {
        [SerializeField] protected GameObject ConfirmButton;
        [SerializeField] protected GameObject CancelButton;

        [HideInInspector]
        public EventHandler<EventArgs> DialogActionCancelled;

        [HideInInspector]
        public EventHandler<EventArgs> DialogActionConfirmed;

        public override void Awake()
        {
            base.Awake();
        }
    }
}
