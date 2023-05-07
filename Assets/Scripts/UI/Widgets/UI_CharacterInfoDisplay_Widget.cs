using RPGTest.Models.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Widgets
{
    public class UI_CharacterInfoDisplay_Widget: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI Level;

        [SerializeField] private Image Portrait;

        public void Refresh(PlayableCharacter character)
        {
            if (character == null) return;

            if (Name != null)
            {
                Name.text = character.Name;
            }
            if (Level != null)
            {
                Level.text = character.Level.ToString();
            }
            if (Portrait != null)
            {
                var portrait = ((Texture2D)Resources.Load($"Portraits/{character.Id}"));
                if (portrait != null)
                {
                    Portrait.sprite = Sprite.Create(portrait, new Rect(0.0f, 0.0f, portrait.width, portrait.height), new Vector2(0.5f, 0.5f), 100.0f);
                } else
                {
                    Portrait.sprite = null;
                }
            }
        }
    }
}
