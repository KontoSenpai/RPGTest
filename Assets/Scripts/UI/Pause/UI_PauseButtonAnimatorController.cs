using RPGTest.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGTest.UI
{
    public partial class UI_PauseButtonAnimatorController : MonoBehaviour
    {
        public int MaxIndex;
        public int Index = 0;

        private Controls m_playerInput;

        public void Awake()
        {
            m_playerInput = new Controls();
            m_playerInput.UI.Navigate.performed += Navigate_performed;
        }

        private void Navigate_performed(InputAction.CallbackContext obj)
        {
            var value = obj.ReadValue<Vector2>();
            int variation = 0;

            if(value.x > 0)
            {
                variation = Mathf.CeilToInt(Index + value.x);
            }
            else
            {
                variation = Mathf.FloorToInt(Index + value.x);
            }

            ChangeIndex(variation);
        }

        public void ChangeIndex(int variation)
        {
            if (variation < 0)
            {
                Index = MaxIndex;
            }
            else if (variation > MaxIndex)
            {
                Index = 0;
            }
            else
            {
                Index = variation;
            }
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();
    }
}
