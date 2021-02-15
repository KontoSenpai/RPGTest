using System;
using System.Collections;
using UnityEngine;

namespace RPGTest.UI
{
    public partial class UI_ButtonAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int Index;

        private UI_ButtonAnimatorController m_ButtonAnimatorController;

        public void Start()
        {
            m_ButtonAnimatorController = gameObject.GetComponentInParent<UI_ButtonAnimatorController>();
        }

        public void Update()
        {
            if (m_ButtonAnimatorController.Index == Index)
            {
                animator.SetBool("selected", true);
            }
            else
            {
                animator.SetBool("selected", false);
            }
        }

        public void Press()
        {
            animator.SetBool("pressed", true);
        }

        public void PressFinished()
        {
            animator.SetBool("pressed", false);
        }
    }
}
