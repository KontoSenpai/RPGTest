using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using UnityEngine;

namespace RPGTest.UI.Common
{
    // Base class to be used by every "widgets" that need to display informations about entities
    public abstract class UI_View_BaseEntityComponent : MonoBehaviour
    {
        [SerializeField] private GameObject Header;
        [SerializeField] private bool ShowHeader;

        protected Entity m_entity;

        protected PlayableCharacter m_character;
        protected PresetSlot m_preset;

        public virtual void Initialize(Entity entity)
        {
            m_entity = entity;
            if (Header != null)
            {
                Header.SetActive(ShowHeader);
            }
        }

        public virtual void Initialize(PlayableCharacter character, PresetSlot preset)
        {
            m_character = character;
            m_preset = preset;

            if (Header != null)
            {
                Header.SetActive(ShowHeader);
            }
        }

        public abstract void Refresh();

        public abstract void Refresh(PresetSlot preset);

        public abstract void Preview(PresetSlot preset, Slot slot, Equipment equipment);

        public abstract void Unpreview();
    }
}
