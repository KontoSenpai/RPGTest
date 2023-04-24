using RPGTest.Managers;
using System;
using System.Collections;
using UnityEngine;

namespace RPGTest.UI
{
    public class UI_Base : MonoBehaviour
    {
        public EventHandler UIOpened { get; set; }
        public EventHandler DevicedChanged { get; set; }
        public EventHandler UIClosed { get; set; }

        protected InputDisplayManager InputManager => FindObjectOfType<InputDisplayManager>();

        public IEnumerator Fade(bool fadeIn, float step = 0.2f)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (fadeIn)
            {
                do
                {
                    canvasGroup.alpha += step;
                    yield return new WaitForSeconds(0.05f);
                } while (canvasGroup.alpha < 1.0f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                do
                {
                    canvasGroup.alpha -= step;
                    yield return new WaitForSeconds(0.05f);
                } while (canvasGroup.alpha > 0);
            }
        }

        public GameObject InstantiateItemInViewport(GameObject itemToInstantiate, string id, GameObject uiList)
        {
            var listSize = uiList.GetComponentInParent<RectTransform>();

            GameObject uiItem = Instantiate(itemToInstantiate);
            uiItem.transform.SetParent(uiList.transform);
            uiItem.name = id;

            uiItem.transform.localScale = new Vector3(1, 1, 1);

            return uiItem;
        }

        public virtual void UpdateHints()
        {

        }
    }
}
