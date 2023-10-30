using RPGTest.Enums;
using RPGTest.Inputs;
using RPGTest.Interactibles;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using RPGTest.UI.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// Display informations about an Entity
    /// </summary>
    public class UI_View_EntityDetails : UI_View_BaseEntityComponent
    {
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private TextMeshProUGUI LevelValue;
        [SerializeField] private Image Portrait;

        [SerializeField] private UI_Stances_Widget StanceWidget;

        #region Public Methods
        public override void Initialize(PlayableCharacter character, PresetSlot preset)
        {
            base.Initialize(character, preset);

            RefreshInternal(character, preset);
        }

        public void Refresh(EquipmentSlots equipmentSlots, PresetSlot slot)
        {
            var attributes = m_character.GetAttributes(m_preset);
            RefreshInternal(m_character, attributes);
        }

        public override void Clear()
        {
            Name.text = string.Empty;
            LevelValue.text = string.Empty;
            Portrait.sprite = null;

            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>().Where(c => c.gameObject != this.gameObject);
            foreach (var component in components)
            {
                component.Clear();
            }
        }

        public override void Refresh()
        {
            throw new System.NotImplementedException();
        }

        public override void Refresh(PresetSlot preset)
        {
            throw new NotImplementedException();
        }

        public override void Preview(PresetSlot preset, Slot slot, Equipment equipment)
        {
            throw new NotImplementedException();
        }

        public override void Unpreview()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Internal Refresh for characters
        /// </summary>
        /// <param name="character"></param>
        /// <param name="slot"></param>
        private void RefreshInternal(PlayableCharacter character, PresetSlot slot)
        {
            LevelValue.text = character.Level.ToString();
            RefreshInternal(character, character.GetAttributes(slot));

            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>().Where(c => c.gameObject != this.gameObject);
            foreach (var component in components)
            {
                component.Initialize(character, m_preset);
            }
        }

        /// <summary>
        /// Generic Internal Refresh for entities
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
        private void RefreshInternal(Entity entity, Dictionary<Enums.Attribute, float> attributes)
        {
            Name.text = entity.Name;

            if (Portrait != null)
            {
                var portrait = ((Texture2D)Resources.Load($"Portraits/{entity.Id}"));
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
        #endregion
    }
}
