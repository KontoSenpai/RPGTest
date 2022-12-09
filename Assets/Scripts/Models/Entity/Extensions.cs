using System;
using System.Collections.Generic;
using System.Linq;
using RPGTest.Enums;

namespace RPGTest.Models.Entity
{
    public static class Extensions
    {
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

        /// <summary>
        /// Apply a buff to the Entity.
        /// If a a buff of a same value is re-applied, it's duration will be extended
        /// </summary>
        /// <param name="entity">Entity on which the modification should be applied to</param>
        /// <param name="buff">Buff to add</param>
        public static void AddBuff(this Entity entity, Buff buff)
        {
            int existingBuffIndex = entity.Buffs.FindIndex(b => b.Attribute == buff.Attribute && b.Value == buff.Value);

            if (existingBuffIndex != -1)
            {
                entity.Buffs[existingBuffIndex] = buff;
            }
            else
            {
                entity.Buffs.Add(buff);
            }
        }

        public static Buff GetBuff(this Entity entity, Predicate<Buff> predicate)
        {
            var buff = entity.Buffs.FirstOrDefault(b => predicate(b));

            return buff;
        }

        public static void RemoveBuffs(this Entity entity, Predicate<Buff> predicate)
        {
            entity.Buffs.RemoveAll(b => predicate(b));
        }
    }
}
