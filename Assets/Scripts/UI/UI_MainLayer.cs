using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI
{
    public class UI_MainLayer : UI_Base
    {
        public Image LoadingImage;
        public GameObject InteractionPanel;
        public TextMeshProUGUI InteractionText;

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

        public void DisplayInteractionPanel(string textToDisplay)
        {
            InteractionPanel.SetActive(true);
            InteractionText.text = textToDisplay;
        }

        public void HideInteractionPanel()
        {
            InteractionPanel.SetActive(false);
        }
    }
}
