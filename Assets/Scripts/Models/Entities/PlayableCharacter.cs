using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Models.Items;
using RPGTest.Models.Abilities;
using RPGTest.Modules.Battle;
using RPGTest.Modules.Battle.Action;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.Models.Entity
{
    [Serializable]
    public class PlayableCharacter : Entity
    {
        #region variables
        private const int m_maxLevel = 99;

        private ActionSequence m_selectedActions;
        private bool m_actionSelected = false;

        private GrowthTable m_GrowthTable = null;
        #endregion

        #region properties
        public int CurrentExperience { get; set; }

        public int NextLevelExperience => GetExperienceToNextLevel(Level);

        public Attributes AttributesGrowth { get; set; }

        public EquipmentSlots EquipmentSlots { get; set; } = new EquipmentSlots();

        public List<string> Abilities { get; set; } = new List<string>();

        public override Range PowerRange { get; set; }

        public bool LimitReady { get; set; }
        #endregion

        public event WaitingForInputHandler PlayerInputRequested;
        public delegate void WaitingForInputHandler(PlayableCharacter character, bool waitStatus);

        public event ATBProgressHandler PlayerATBChanged;
        public delegate void ATBProgressHandler(float atbCharge);

        public event UpdatingBarHandler PlayerWidgetUpdated;
        public delegate void UpdatingBarHandler(Attribute attribute);

        public event ExperienceChangedHandler PlayerExperienceChanged;
        public delegate void ExperienceChangedHandler();

        public event BuffRefreshedHandler PlayerBuffsRefreshed;
        public delegate void BuffRefreshedHandler(List<Buff> buffs);

        #region public methods
        #region overrides
        public override bool FillATB()
        {
            var full = base.FillATB();
            PlayerATBChanged(m_currentATB);
            return full;
        }

        public override void RefillResources()
        {
            //base.ApplyAttributeModification(Attribute.HP, 0);
            ApplyAttributeModification(Attribute.MP, (int)Mathf.Ceil(GetAttribute(Attribute.MaxMP) * .1f));
            ApplyAttributeModification(Attribute.Stamina, (int)Mathf.Ceil(GetAttribute(Attribute.MaxStamina) * .1f));
        }

        public override void ReduceStatusDurations()
        {
            base.ReduceStatusDurations();
            PlayerBuffsRefreshed(Buffs);
        }

        public override void ResetATB(int variation = 0)
        {
            base.ResetATB(variation);
            if (PlayerATBChanged != null)
                PlayerATBChanged(m_currentATB);
        }

        public override IEnumerator SelectAction(BattleManager manager, List<PlayableCharacter> playerParty, List<Enemy> enemyParty, Action<ActionSequence> selectedActions)
        {
            manager.m_waitingForUserInput = true;
            yield return manager.StartCoroutine(ActionChoice());
            manager.m_waitingForUserInput = false;

            selectedActions(m_selectedActions);
        }

        public override float GetAttribute(Attribute attribute)
        {
            GetAttributes().TryGetValue(attribute, out float value);
            return value;
        }

        public override Dictionary<Attribute, float> GetAttributes()
        {
            return GetAttributes(EquipmentSlots.CurrentPreset);
        }

        public override void AddBuff(Buff buff)
        {
            base.AddBuff(buff);
            PlayerBuffsRefreshed(Buffs);
        }

        public override void RemoveBuff(Buff buff)
        {
            base.RemoveBuff(buff);
            PlayerBuffsRefreshed(Buffs);
        }
        #endregion

        public IEnumerable<(Ability ability, bool usable)> GetAbilitiesOfType(AbilityType type)
        {
            List<(Ability ability, bool usable)> validAbilities = new List<(Ability ability, bool usable)>();
            var abilities = Abilities.Select(a => AbilitiesCollector.TryGetAbility(a)).Where(a => type == AbilityType.Default || a.AbilityType == type);

            validAbilities = abilities.Select(a => (a, IsAbilityCastable(a))).ToList();

            if (type == AbilityType.Weapon)
            {
                var preset = EquipmentSlots.GetCurrentWeaponPreset();
                validAbilities = validAbilities.Where(a => a.ability.EquipmentRestrictrion.Any(e => preset.Where(x => x.Value != null).Select(x => x.Value.EquipmentType).Contains(e))).ToList();
            }

            return validAbilities;
        }

        public void CreateNewActionSequence()
        {
            m_selectedActions = new ActionSequence(this);
        }

        public void SetSelectedActions(List<EntityAction> entityActions)
        {
            m_selectedActions.Actions = entityActions;
            m_actionSelected = true;
        }

        public void Initialize(GrowthTable table)
        {
            if (m_GrowthTable == null)
            {
                m_GrowthTable = table;
            }
            CurrentHP = BaseAttributes.MaxHP;
            CurrentMP = BaseAttributes.MaxMP;
        }

        public override void ApplyAttributeModification(Attribute attribute, int value)
        {
            base.ApplyAttributeModification(attribute, value);
            PlayerWidgetUpdated(attribute);
        }

        public override float GetPowerRangeValue()
        {
            var preset = EquipmentSlots.GetCurrentWeaponPreset();

            preset.TryGetValue(Slot.LeftHand, out Equipment leftWeapon);
            preset.TryGetValue(Slot.RightHand, out Equipment rightWeapon);

            if (leftWeapon != null && rightWeapon != null)
            {
                float leftValue = leftWeapon.PowerRange.GetValue();
                float rightValue = rightWeapon.PowerRange.GetValue();
                return (leftValue + rightValue) / 2;
            }
            else if (leftWeapon != null && rightWeapon == null)
            {
                return leftWeapon.PowerRange.GetValue();
            }
            else if (rightWeapon != null && leftWeapon == null)
            {
                return rightWeapon.PowerRange.GetValue();
            }
            else
            {
                return base.GetPowerRangeValue();
            }
        }

        /// <summary>
        /// Add experience point to the playable character
        /// </summary>
        /// <param name="addedValue">Experience points won</param>
        /// <returns bool>Has a level up been triggered</returns>
        public bool AddExperience(int addedExperience)
        {
            if (Level < m_maxLevel)
            {
                CurrentExperience += addedExperience;
                while(CurrentExperience > NextLevelExperience)
                {
                    LevelUp();
                }
            }
            PlayerExperienceChanged();
            return true;
        }

        /// <summary>
        /// Performs a level up operation.
        /// All stats are getting increased
        /// </summary>
        public void LevelUp()
        {
            CurrentExperience = CurrentExperience - NextLevelExperience;
            Level++;

            CurrentHP += AttributesGrowth.MaxHP;
            BaseAttributes.MaxHP += AttributesGrowth.MaxHP;

            CurrentMP += AttributesGrowth.MaxMP;
            BaseAttributes.MaxMP += AttributesGrowth.MaxMP;

            BaseAttributes.MaxStamina += AttributesGrowth.MaxStamina;

            BaseAttributes.Attack += AttributesGrowth.Attack;;
            BaseAttributes.Magic += AttributesGrowth.Magic;

            BaseAttributes.Defense+= AttributesGrowth.Defense;

            BaseAttributes.Resistance += AttributesGrowth.Defense;
        }

        public bool TryEquip(Slot slot, Equipment equipPiece, out List<Item> removedEquipments)
        {
            EquipmentSlots.TryEquip(slot, equipPiece, out removedEquipments);
            removedEquipments.WhereNotNull();
            return EquipmentSlots.Equipment[slot] == equipPiece;
        }
        
        public bool TryUnequip(Slot slot, out List<Item> removedEquipments)
        {
            EquipmentSlots.TryUnequip(slot, out removedEquipments);
            removedEquipments.WhereNotNull();
            return removedEquipments.Any();
        }

        public Dictionary<Attribute, float> GetAttributes(PresetSlot slot)
        {
            var attributes = base.GetAttributes();
            var preset = EquipmentSlots.GetEquipmentPreset(slot);

            attributes[Attribute.TotalAttack] += preset.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Attribute.Attack).Value);
            attributes[Attribute.TotalDefense] += preset.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Attribute.Defense).Value);
            attributes[Attribute.TotalMagic] += preset.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Attribute.Magic).Value);
            attributes[Attribute.TotalResistance] += preset.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Attribute.Resistance).Value);
            attributes[Attribute.TotalSpeed] += preset.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Attribute.Speed).Value);

            return attributes;
        }

        public int GetExperienceToNextLevel(int level)
        {
            return m_GrowthTable.XPToNextLevel[level - 1];
        }
        #endregion

        #region private methodes
        private IEnumerator ActionChoice()
        {
            PlayerInputRequested(this, true);
            m_actionSelected = false;
            while (!m_actionSelected)
            {
                yield return null;
            }
            PlayerInputRequested(this, false);
        }
        #endregion
    }
}
