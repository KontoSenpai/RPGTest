using RPGTest.Inputs;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_MainLayer : UI_Base
    {
        [SerializeField] private Image LoadingImage;
        [SerializeField] private GameObject InteractionPanel;
        [SerializeField] private TextMeshProUGUI InteractionText;
        
        [SerializeField] private GameObject InputWidget;

        public IEnumerator FadeLoading(bool fade, float step = 0.2f)
        {
            var color = LoadingImage.color;
            if(fade)
            {
                do
                {
                    color.a += step;
                    LoadingImage.color = color;
                    yield return new WaitForSeconds(0.05f);
                } while (color.a < 1.0f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                do
                {
                    color.a -= step;
                    LoadingImage.color = color;
                    yield return new WaitForSeconds(0.05f);
                } while (color.a > 0);
            }
        }

        public override void Awake()
        {
            m_playerInput = new Controls();
        }

        public virtual void OnEnable()
        {
            InputManager.SchemeChanged += onScheme_Changed;
            UpdateControlsDisplay(GetControlDescriptions());
        }

        public virtual void OnDisable()
        {
            if(InputManager)
            {
                InputManager.SchemeChanged -= onScheme_Changed;
            }
        }

        #region events
        protected void onScheme_Changed(object sender, EventArgs e)
        {
            UpdateControlsDisplay(GetControlDescriptions());
        }
        #endregion

        private Dictionary<string, string[]> GetControlDescriptions()
        {
            return new Dictionary<string, string[]>()
            {
                {
                    "Move",
                    new string[]
                    {
                        m_playerInput.Player.Move.name
                    }
                },
                {
                    "Jump",
                    new string[]
                    {
                        m_playerInput.Player.Jump.name
                    }
                }
            };
        }

        public void DisplayInteractionPanel(string textToDisplay)
        {
            InteractionPanel.SetActive(true);
            InteractionText.text = textToDisplay;
        }

        // Update the input Hints on current page
        protected void UpdateControlsDisplay(Dictionary<string, string[]> actions)
        {
            var inputDisplays = InputManager.GetInputDisplays(actions);
            InputWidget.GetComponent<UI_Controls_Display>().Refresh(inputDisplays);
        }

        public void HideInteractionPanel()
        {
            InteractionPanel.SetActive(false);
        }
    }
}
