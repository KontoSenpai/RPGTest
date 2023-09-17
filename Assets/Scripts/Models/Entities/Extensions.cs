using RPGTest.Models.Effects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Models.Entity
{
    public static class Extensions
    { 
        public static int CalculateDamage(this Entity entity, Entity caster, Effect effect)
        {
            List<float> attackValues = new List<float>();

            foreach (var scaling in effect.Scalings)
            {
                // Get entity defensive response to scaling
                var attackPower = caster.GetOffensiveAttribute(scaling.Key) * scaling.Value;

                var defensePower = entity.GetDefensiveAttribute(scaling.Key) * (1 - effect.Potency.IgnoreDefense) * 0.6f;

                attackValues.Add(attackPower / (1 + defensePower));
            }

            var attackValue = (int)Mathf.Ceil((attackValues.Sum() * -1) * effect.PowerRange.GetValue());

            return attackValue;
        }

        public static int CalculateHealing(this Entity entity, Entity caster, Effect effect)
        {
            List<float> healValues = new List<float>();

            foreach (var scaling in effect.Scalings)
            {
                // Get entity defensive response to scaling
                var healPower = caster.GetOffensiveAttribute(scaling.Key) * scaling.Value;

                healValues.Add(healPower);
            }

            var healValue = (int)Mathf.Ceil((healValues.Sum()) * effect.PowerRange.GetValue());

            return healValue;
        }

        public static float GetDefensiveAttribute(this Entity entity, Enums.Attribute attribute)
        {
            switch (attribute) {
                case Enums.Attribute.Attack:
                case Enums.Attribute.Defense:
                    return entity.GetAttribute(Enums.Attribute.Defense);
                case Enums.Attribute.Magic:
                case Enums.Attribute.Resistance:
                    return entity.GetAttribute(Enums.Attribute.Resistance);
            }
            return 0.0f;
        }

        public static float GetOffensiveAttribute(this Entity entity, Enums.Attribute attribute)
        {
            switch (attribute)
            {
                case Enums.Attribute.Attack:
                    return entity.GetAttribute(Enums.Attribute.Attack);
                case Enums.Attribute.Defense:
                    return entity.GetAttribute(Enums.Attribute.Defense);
                case Enums.Attribute.Magic:
                    return entity.GetAttribute(Enums.Attribute.Magic);
                case Enums.Attribute.Resistance:
                    return entity.GetAttribute(Enums.Attribute.Resistance);
                default:
                    Debug.LogError("Unsupported attribute");
                    return 0.0f;
            }
        }

        public static int GetFirstAliveIndex(this List<Enemy> entities)
        {
            for(int i = 0; i < entities.Count; i++)
            {
                if(entities[i].IsAlive)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetLastAliveIndex(this List<Enemy> entities)
        {
            for (int i = entities.Count - 1; i > 0 ; i--)
            {
                if (entities[i].IsAlive)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetIndexOfAlly(this List<PlayableCharacter> entities, PlayableCharacter ally)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].Id == ally.Id)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
