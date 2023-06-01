using MyBox;
using RPGTest.Helpers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    public enum MenuActionType
    {
        Use,
        Equip,
        Unequip,
        Swap,
        Discard,
        Confirm,
        Cancel
    }

    public class UI_ActionConfirmationDialog : UI_Dialog
    {
        [Separator("Buttons")]
        [SerializeField] private GameObject ButtonGo;
        private List<GameObject> ActionButtons;

        [Separator("Layout")]
        [SerializeField] private GameObject LayoutContent;

        [HideInInspector]
        public ActionSelectedHandler ActionSelected { get; set; }
        [HideInInspector]
        public delegate void ActionSelectedHandler(MenuActionType actiontype);

        #region public Methods
        public override void Awake()
        {
            base.Awake();
            m_playerInput.UI.Cancel.performed += OnCancel_Performed;
            ActionButtons = new List<GameObject>();
        }

        /// <summary>
        /// Open the dialog and enable user inputs
        /// </summary>
        public void Open(List<MenuActionType> actionTypes)
        {
            gameObject.SetActive(true);
            Initialize(actionTypes);

            EnableControls();
            UpdateInputActions();
        }

        /// <summary>
        /// Close the dialog and disable user inputs
        /// </summary>
        public void Close()
        {
            ActionButtons.ForEach(b => Destroy(b));
            ActionButtons.Clear();

            DisableControls();
            gameObject.SetActive(false);
        }
        #endregion

        protected override void UpdateInputActions()
        {
            m_inputActions = new Dictionary<string, string[]>()
            {
                {
                    "Select Action",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Navigate.name  + ".vertical"
                    }
                },
                {
                    "Confirm",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Submit.name,
                        "UI_" + m_playerInput.UI.LeftClick.name
                    }
                },
                {
                    "Cancel",
                    new string[]
                    {
                        "UI_" + m_playerInput.UI.Cancel.name,
                        "UI_" + m_playerInput.UI.RightClick.name,
                    }
                }
            };
            base.UpdateInputActions();
        }

        #region Input Events
        private void OnCancel_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ActionSelectedInternal(MenuActionType.Cancel);
        }
        #endregion

        private void Initialize(List<MenuActionType> actions)
        {
            foreach(var action in actions)
            {
                var buttonGo = Instantiate(ButtonGo);

                buttonGo.transform.name = action.ToString();
                buttonGo.transform.SetParent(LayoutContent.transform);
                buttonGo.transform.localScale = new Vector3(1, 1, 1);

                buttonGo.GetComponentInChildren<TextMeshProUGUI>().text = action.ToString();
                buttonGo.GetComponent<Button>().onClick.AddListener(() => ActionSelectedInternal(action));

                ActionButtons.Add(buttonGo);
            }

            //Expliciting navigation
            foreach (var actionButton in ActionButtons)
            {
                var index = ActionButtons.IndexOf(actionButton);

                actionButton.GetComponent<Button>().ExplicitNavigation(
                    Up: index > 0 ? ActionButtons[index - 1].GetComponent<Button>() : null,
                    Down: index < ActionButtons.Count - 1 ? ActionButtons[index + 1].GetComponent<Button>() : null);
            }

            ActionButtons.First().GetComponent<Button>().Select();
        }

        private void ActionSelectedInternal(MenuActionType action)
        {
            ActionSelected(action);
            Close();
        }
    }
}
