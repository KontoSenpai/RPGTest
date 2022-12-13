using RPGTest.Enums;
using System.Linq;

namespace RPGTest.Models.Entity
{

    public abstract partial class Entity
    {
        public virtual float GetHighestBuff(Attribute attribute)
        {
            var buff = Buffs.FindAll(b => b.Attribute == attribute)
                .OrderByDescending( b => b.Value)
                .FirstOrDefault();

            if (buff != null)
            {
                return buff.Value;
            }
            return 1.0f;
        }
        /// <summary>
        /// Apply a buff to the selected Entity.
        /// If a a buff of a same value is re-applied, it's duration will be extended
        /// </summary>
        /// <param name="entity">Entity on which the modification should be applied to</param>
        /// <param name="buff">Buff to assign</param>
        public virtual void AddBuff(Buff buff)
        {
            int existingBuffIndex = Buffs.FindIndex(b => b.Attribute == buff.Attribute && b.Value == buff.Value);

            if (existingBuffIndex != -1)
            {
                Buffs[existingBuffIndex] = buff;
            }
            else
            {
                Buffs.Add(buff);
            }
        }

        /// <summary>
        /// Remove buffs corresponding to the given removal type rule
        /// </summary>
        /// <param name="removalType">RemovalType of the action</param>
        public void RemoveBuffs(RemovalType removalType)
        {
            Buffs.RemoveAll(b => b.RemovalType == removalType);
        }
    }
}
