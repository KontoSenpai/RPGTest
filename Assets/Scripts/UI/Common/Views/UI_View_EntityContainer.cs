using RPGTest.Models;
using RPGTest.Models.Entity;
using UnityEngine;

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

        public void Refresh(Entity entity, PresetSlot slot)
        {
            var character = (PlayableCharacter)entity;
            var components = GetComponentsInChildren<UI_View_BaseEntityComponent>();
            foreach (var component in components)
            {
                component.Refresh(slot);
            }
        }
    }
}
