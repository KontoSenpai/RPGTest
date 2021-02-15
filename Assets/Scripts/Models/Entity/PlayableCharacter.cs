using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Helpers;
using RPGTest.Managers;
using RPGTest.Models.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public AttributesGrowth AttributesGrowth { get; set; }

        public EquipmentSlots EquipmentSlots { get; set; } = new EquipmentSlots();

        public List<string> Abilities { get; set; } = new List<string>();

        public override Range PowerRange { get; set; }

        public bool LimitReady { get; set; }
        #endregion

        public event WaitingForInputHandler PlayerInputRequested;
        public delegate void WaitingForInputHandler(bool waitStatus);

        public event ATBProgressHandler PlayerATBChanged;
        public delegate void ATBProgressHandler(float atbCharge);

        public event UpdatingBarHandler PlayerWidgetUpdated;
        public delegate void UpdatingBarHandler(Enums.Attribute attribute);

        public event ExperienceChangedHandler PlayerExperienceChanged;
        public delegate void ExperienceChangedHandler();

        #region public methods
        public IEnumerable<(Ability ability, bool usable)> GetAbilitiesOfType(AbilityType type)
        {
            List<(Ability ability, bool usable)> validAbilities = new List<(Ability ability, bool usable)>();
            var abilities = Abilities.Select(a => AbilitiesCollector.TryGetAbility(a)).Where(a => a.AbilityType == type);

            validAbilities = abilities.Select(a => (a, IsAbilityCastable(a))).ToList();

            if (type == AbilityType.Weapon)
            {
                var preset = EquipmentSlots.GetCurrentWeaponPreset();
                validAbilities = validAbilities.Where(a => a.ability.EquipmentRestrictrion.Any(e => preset.Where(x => x.Value != null).Select(x => x.Value.EquipmentType).Contains(e))).ToList();
            }

            return validAbilities;
        }


        public override bool FillATB()
        {
            var full = base.FillATB();
            PlayerATBChanged(m_currentATB);
            return full;
        }

        public override void ResetATB(int variation = 0)
        {
            base.ResetATB(variation);
            if(PlayerATBChanged != null)
                PlayerATBChanged(m_currentATB);
        }

        public void CreateNewActionSequence()
        {
            m_selectedActions = new ActionSequence(this);
        }

        public override IEnumerator SelectAction(BattleManager manager, List<PlayableCharacter> playerParty, List<Enemy> enemyParty, Action<ActionSequence> selectedActions)
        {
            manager.m_waitingForUserInput = true;
            yield return manager.StartCoroutine(ActionChoice());
            manager.m_waitingForUserInput = false;

            selectedActions(m_selectedActions);
        }

        private IEnumerator ActionChoice()
        {
            PlayerInputRequested(true);
            m_actionSelected = false;
            while (!m_actionSelected)
            {
                yield return null;
            }
            PlayerInputRequested(false);
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

        public override void ApplyDamage(Enums.Attribute attribute, int value)
        {
            base.ApplyDamage(attribute, value);
            PlayerWidgetUpdated(attribute);
        }

        public bool IsAbilityCastable(Ability ability)
        {
            var enoughResources = true;

            foreach (var castCost in ability.CastCost)
            {
                var cost = Math.Abs(castCost.Value);
                switch (castCost.Key)
                {
                    case Enums.Attribute.HP:
                        if (GetAttribute("CurrentHP") < cost)
                        {
                            return false;
                        }
                        break;
                    case Enums.Attribute.MP:
                        if (GetAttribute("CurrentMP") < cost)
                        {
                            return false;
                        }
                        break;
                    case Enums.Attribute.Stamina:
                        if (GetAttribute("CurrentStamina") < cost)
                        {
                            return false;
                        }
                        break;
                }
            }
            return enoughResources;
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

            Debug.Log($"{Name } Levelled up to level {Level}");

            CurrentHP += AttributesGrowth.HP;
            BaseAttributes.MaxHP += AttributesGrowth.HP;

            CurrentMP += AttributesGrowth.MP;
            BaseAttributes.MaxMP += AttributesGrowth.MP;

            BaseAttributes.MaxStamina += AttributesGrowth.Stamina;

            BaseAttributes.Strength += AttributesGrowth.Strength;
            BaseAttributes.Agility += AttributesGrowth.Agility;

            BaseAttributes.Constitution += AttributesGrowth.Constitution;

            BaseAttributes.Mental += AttributesGrowth.Mental;
            BaseAttributes.Resilience += AttributesGrowth.Resilience;
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

        public override float GetAttribute(string attributeName)
        {
            GetAttributes().TryGetValue(attributeName, out float value);
            return value;
        }
        #endregion

        public override Dictionary<string, float> GetAttributes()
        {
            var attributes = base.GetAttributes();

            var preset1 = EquipmentSlots.GetEquipmentPreset(PresetSlot.First);
            attributes.Add("TotalStrengthPreset1", BaseAttributes.Strength + preset1.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Strength).Value));
            attributes.Add("TotalConstitutionPreset1", BaseAttributes.Constitution + preset1.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Constitution).Value));
            attributes.Add("TotalMentalPreset1", BaseAttributes.Mental + preset1.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Mental).Value));
            attributes.Add("TotalResiliencePreset1", BaseAttributes.Resilience + preset1.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Resilience).Value));

            var preset2 = EquipmentSlots.GetEquipmentPreset(PresetSlot.Second);
            attributes.Add("TotalStrengthPreset2", BaseAttributes.Strength + preset2.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Strength).Value));
            attributes.Add("TotalConstitutionPreset2", BaseAttributes.Constitution + preset2.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Constitution).Value));
            attributes.Add("TotalMentalPreset2", BaseAttributes.Mental + preset2.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Mental).Value));
            attributes.Add("TotalResiliencePreset2", BaseAttributes.Resilience + preset2.Where(e => e.Value != null).Sum(x => x.Value.Attributes.SingleOrDefault(a => a.Key == Enums.Attribute.Resilience).Value));

            return attributes;
        }

        public int GetExperienceToNextLevel(int level)
        {
            return m_GrowthTable.XPToNextLevel[level - 1];
        }
    }
}
