using System.Collections.Generic;
using System.Linq;
using RPGTest.UI.Common;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGTest.Modules.UI.Pause.Skills.Views
{
    public class UI_Skills_SkillTree : UI_View
    {
        #region Public Methods
        public override void Awake()
        {
            base.Awake();

            m_playerInput.UI.Cancel.performed += OnCancel_performed;
            // m_playerInput.UI.SecondaryNavigate.performed += OnSecondaryNavigate_performed;
            //m_playerInput.UI.SecondaryAction.performed += ctx =>
            //{
            //    OnSecondaryAction_performed(ctx);
            //};

            //foreach (var equipment in m_equipmentComponents)
            //{
            //    equipment.ItemSelectionConfirmed += OnSlot_Selected;
            //}
            //PresetSelector.PresetSlotSelected += PresetSelector_PresetSlotSelected;
        }

        public override void Select()
        {
            base.Select();

            var button = GetComponentsInChildren<Button>()
                .FirstOrDefault(b => b.gameObject.activeInHierarchy);
            if (button != null)
            {
                button.Select();
            }
        }

        public override Dictionary<string, string[]> GetInputDisplay(Inputs.Controls playerInput = null)
        {
            var controls = playerInput ?? m_playerInput;
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Slot",
                    new string[]
                    {
                        "UI_" + controls.UI.Navigate.name
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + controls.UI.Submit.name,
                        "UI_" + controls.UI.LeftClick.name,
                    }
                },
                {
                    "Remove Equipment",
                    new string[]
                    {
                        "UI_" + controls.UI.SecondaryAction.name,
                    }
                },
                {
                    "Change Preset",
                    new string[]
                    {
                        "UI_" + controls.UI.SecondaryNavigate.name,
                    }
                }
            };

            return m_inputActions;
        }
        #endregion

        #region  Input Events
        private void OnCancel_performed(InputAction.CallbackContext ctx)
        {
            DisableControls();
        }
        #endregion
    }
}