using RPGTest.Models.Entity;
using System.Linq;

namespace RPGTest.Models.Effects
{
    public partial class Effect : IdObject
    {
        public bool AreConditionsRespected(Entity.Entity caster)
        {
            if (caster.GetType() == typeof(PlayableCharacter) && Conditions.TwoHanded != null)
            {
                var character = (PlayableCharacter)caster;

                return character.EquipmentComponent.GetWeaponSlots().Values.Any(w => w.TwoHanded);
            }

            return true;
        }
    }
}