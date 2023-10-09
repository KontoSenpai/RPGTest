using RPGTest.Models.Entity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{

    public class CharacterFilterSelectedEventArgs : EventArgs
    {
        public CharacterFilterSelectedEventArgs(PlayableCharacter character)
        {
            Character = character;
        }

        public PlayableCharacter Character { get; }
    }

    public class UI_CharacterFilter : MonoBehaviour
    {
        [SerializeField] private Image Portrait;

        private PlayableCharacter m_character;

        public void Initialize(PlayableCharacter character)
        {
            m_character = character;

            if (Portrait != null)
            {
                var portrait = (Texture2D)Resources.Load($"Portraits/{character.Id}");
                if (portrait != null)
                {
                    Portrait.sprite = Sprite.Create(portrait, new Rect(0.0f, 0.0f, portrait.width, portrait.height), new Vector2(0.5f, 0.5f), 100.0f);
                }
                else
                {
                    Portrait.sprite = null;
                }
            }
        }

        public PlayableCharacter GetCharacter()
        {
            return m_character;
        }

        public void OnCharacter_Selected()
        {
            CharacterFilterSelected.Invoke(this, new CharacterFilterSelectedEventArgs(m_character));
        }

        [HideInInspector]
        public event EventHandler<CharacterFilterSelectedEventArgs> CharacterFilterSelected;
    }
}
