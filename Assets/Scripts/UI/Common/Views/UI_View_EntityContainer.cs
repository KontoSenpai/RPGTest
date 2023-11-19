using RPGTest.Enums;
using RPGTest.Models;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace RPGTest.UI.Common
{
    /// <summary>
    /// A container to wrap all individual entity views
    /// </summary>
    public class UI_View_EntityContainer : MonoBehaviour
    {
        public void Initialize(Entity entity)
        {
            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach (var component in components)
            {
                component.Initialize(entity);
            }
        }

        public void Initialize(Entity entity, PresetSlot preset)
        {
            var character = (PlayableCharacter)entity;

            var infos = GetComponentInChildren<UI_View_EntityInfos>();
            
            if (infos != null)
            {
                infos.Initialize(entity, preset);
            }


            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach(var component in components)
            {
                component.Initialize(character, preset);
            }
        }

        public void Refresh(PresetSlot preset)
        {
            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach (var component in components)
            {
                component.Refresh(preset);
            }
        }

        public void Preview(PresetSlot preset, Slot slot, Equipment equipment)
        {
            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach (var component in components)
            {
                component.Preview(preset, slot, equipment);
            }
        }

        public void Unpreview()
        {
            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach (var component in components)
            {
                component.Unpreview();
            }
        }

        public void Clear()
        {
            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach (var component in components)
            {
                component.Clear();
            }
        }
    }
}
