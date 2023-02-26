using System;
using TMPro;
using UnityEngine;

namespace RPGTest.Modules.Battle
{
    public class BattleSprite : MonoBehaviour
    {
        [SerializeField] private float YOffset = 200;
        [SerializeField] private float XOffset = 100;

        public void Initialize(Vector3 position, int value)
        {
            position = Camera.current.WorldToScreenPoint(position);
            position.y += YOffset;
            position.x += XOffset;

            GetComponentInChildren<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            GetComponentInChildren<TextMeshProUGUI>().transform.position = position;
            GetComponentInChildren<TextMeshProUGUI>().text = Math.Abs(value).ToString();
            if (value < 0)
            {
                GetComponent<Animator>().SetBool("Damage", true);
            }
            else
            {
                GetComponent<Animator>().SetBool("Heal", true);
            }
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}
