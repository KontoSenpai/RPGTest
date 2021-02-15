using System;
using System.Collections;
using UnityEngine;

namespace RPGTest.UI
{
    public partial class UI_PauseButtonAnimator : MonoBehaviour
    {
        [SerializeField] private UI_PauseButtonAnimatorController m_ButtonAnimatorController;
        [SerializeField] private Animator animator;
        [SerializeField] private int Index;

        public void Update()
        {
            if(m_ButtonAnimatorController.Index == Index)
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
