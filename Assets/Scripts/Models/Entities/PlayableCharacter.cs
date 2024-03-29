﻿using RPGTest.Collectors;
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
using RPGTest.Models.Effects;

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

        public void ChangeEquipmentPreset()
        {
            this.EquipmentSlots.ChangePreset();
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

        public int GetExperienceToNextLevel(int level)
        {
            return m_GrowthTable.XPToNextLevel[level - 1];
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

        public bool TryEquip(PresetSlot preset, Slot slot, Equipment equipPiece, out List<Item> removedEquipments)
        {
            return EquipmentSlots.TryEquip(preset, slot, equipPiece, out removedEquipments);
        }
        
        public bool TryUnequip(PresetSlot preset, Slot slot, out List<Item> removedEquipments)
        {
            EquipmentSlots.TryUnequip(preset, slot, out removedEquipments);
            removedEquipments.WhereNotNull();
            return removedEquipments.Any();
        }

        #region Attributes
        /// <summary>
        /// Returns attribute values for current preset slot
        /// </summary>
        /// <returns>Values for each attributes</returns>
        public override Dictionary<Attribute, float> GetAttributes()
        {
            return GetAttributes(EquipmentSlots.CurrentPreset);
        }

        /// <summary>
        /// Returns attribute values for given preset
        /// </summary>
        /// <param name="preset">Preset Slot to use for equipment values</param>
        /// <returns>Values for each attributes</returns>
        public Dictionary<Attribute, float> GetAttributes(PresetSlot preset)
        {
            var equipmentPreset = EquipmentSlots.GetEquipmentPreset(preset);

            return GetAttributes(equipmentPreset);
        }

        /// <summary>
        /// Returns attribute values for given equipments presets
        /// </summary>
        /// <param name="equipmentPreset">Map of equipment</param>
        /// <param name="attribute">Attribute to retrieve the value of</param>
        /// <returns>Value of desired attribute</returns>
        public Dictionary<Attribute, float> GetAttributes(Dictionary<Slot, Equipment> equipmentPreset)
        {
            return GetAttributesInternal(equipmentPreset);
        }

        /// <summary>
        /// Returns the value for all attributes of character with given equipment slots
        /// </summary>
        /// <param name="equipmentPreset">Map of equipment</param>
        /// <returns>Dicionary of attributes and values</returns>
        private Dictionary<Attribute, float> GetAttributesInternal(Dictionary<Slot, Equipment> equipmentSlots)
        {
            var attributes = base.GetAttributes();

            // Retrieve equipment passive effects
            var passiveEffects = new List<Effect>();
            foreach (var equipment in equipmentSlots.Where(e => e.Value != null))
            {
                foreach (var effectID in equipment.Value.Effects)
                {
                    var effect = EffectsCollector.TryGetEffect(effectID);
                    if (effect.Type == EffectType.Passive && effect.AreConditionsRespected(this) && effect.Potency.Attribute != Attribute.None)
                    {
                        passiveEffects.Add(EffectsCollector.TryGetEffect(effectID));
                    }
                }
            }

            // Retrieve weapon attributes
            var weaponAttributes = new Dictionary<Attribute, float>();
            foreach(var equipment in equipmentSlots.Where(e => e.Value != null && e.Value.IsWeapon))
            {
                foreach (var attribute in equipment.Value.Attributes)
                {
                    // adjust value if equipmentBuff
                    var finalValue = GetEquipmentAttributeValue(attribute.Key, attribute.Value, passiveEffects);

                    if (!weaponAttributes.ContainsKey(attribute.Key))
                    {
                        weaponAttributes.Add(attribute.Key, finalValue);
                    } 
                    else
                    {
                        weaponAttributes[attribute.Key] += finalValue;
                    }
                }
            }

            // retrieve remaining attributes
            var equipmentAttributes = new Dictionary<Attribute, float>();
            foreach (var equipment in equipmentSlots.Where(e => e.Value != null && !e.Value.IsWeapon))
            {
                foreach (var attribute in equipment.Value.Attributes)
                {
                    if (!equipmentAttributes.ContainsKey(attribute.Key))
                    {
                        equipmentAttributes.Add(attribute.Key, attribute.Value);
                    }
                    else
                    {
                        equipmentAttributes[attribute.Key] += attribute.Value;
                    }
                }
            }

            foreach (var attribute in attributes.Keys.ToList())
            {
                attributes[attribute] += weaponAttributes.SingleOrDefault(e => e.Key == attribute).Value + equipmentAttributes.SingleOrDefault(e => e.Key == attribute).Value;
            }

            return attributes;
        }

        private float GetEquipmentAttributeValue(Attribute attribute, float value, List<Effect> passives)
        {
            switch (attribute)
            {
                case Attribute.Attack:
                    return Mathf.Ceil(value * 1.0f + passives.Where(p => p.Potency.Attribute == Attribute.EquipmentAttack).Sum(p => p.Potency.Potency / 100));
            }

            return value;
        }
        #endregion

        #region Attribute
        /// <summary>
        /// Retrieve the value of a specific attribute using current preset slot
        /// </summary>
        /// <param name="attribute">Attribute to retrieve the value of</param>
        /// <returns>Value of desired attribute</returns>
        public override float GetAttribute(Attribute attribute)
        {
            return GetAttribute(EquipmentSlots.CurrentPreset, attribute);
        }

        /// <summary>
        /// Retrieve the value of a specific attribute for given preset
        /// </summary>
        /// <param name="preset">Preset Slot to use for equipment value</param>
        /// <param name="attribute">Attribute to retrieve the value of</param>
        /// <returns>Value of desired attribute</returns>
        public float GetAttribute(PresetSlot preset, Attribute attribute)
        {
            var equipmentPreset = EquipmentSlots.GetEquipmentPreset(preset);

            return GetAttribute(equipmentPreset, attribute);
        }

        /// <summary>
        /// Retrieve the value of a specific attribute for given equipments presets
        /// </summary>
        /// <param name="equipmentPreset">Map of equipment</param>
        /// <param name="attribute">Attribute to retrieve the value of</param>
        /// <returns>Value of desired attribute</returns>
        public float GetAttribute(Dictionary<Slot, Equipment> equipmentPreset, Attribute attribute)
        {
            return GetAttributeInternal(equipmentPreset, attribute);
        }

        /// <summary>
        /// Returns the value for given attribute of character with given equipment slots
        /// </summary>
        /// <param name="equipmentPreset">Map of equipment</param>
        /// <param name="attribute">Attribute to retrieve the value of</param>
        /// <returns>The value of the attribute, -1 if it doesn't exists</returns>
        private float GetAttributeInternal(Dictionary<Slot, Equipment> equipmentSlots, Attribute attribute)
        {
            if (GetAttributesInternal(equipmentSlots).TryGetValue(attribute, out float value))
            {
                return value;
            }

            return -1;
        }
        #endregion

        #region Elemental Resistances
        /// <summary>
        /// Retrieve the elemental resistance values for current preset slot
        /// </summary>
        /// <returns></returns>
        public override Dictionary<Element, float> GetElementalResistances()
        {
            return GetElementalResistances(EquipmentSlots.CurrentPreset);
        }

        /// <summary>
        /// Retrieve Elemental Resistances for given preset
        /// </summary>
        /// <param name="preset">Preset Slot to use for equipment value</param>
        /// <returns></returns>
        public Dictionary<Element, float> GetElementalResistances(PresetSlot preset)
        {
            var equipmentPreset = EquipmentSlots.GetEquipmentPreset(preset);

            return GetElementalResistancesInternal(equipmentPreset);
        }

        /// <summary>
        /// Retrieve Elemental Resistances of character for equipment preset
        /// </summary>
        /// <param name="equipmentPreset">Map of equipment</param>
        /// <returns></returns>
        public Dictionary<Element, float> GetElementalResistances(Dictionary<Slot, Equipment> equipmentPreset)
        {
            return GetElementalResistancesInternal(equipmentPreset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentPreset">Map of equipment</param>
        /// <returns></returns>
        private Dictionary<Element, float> GetElementalResistancesInternal(Dictionary<Slot, Equipment> equipmentSlots)
        {
            var elementalResitances = base.GetElementalResistances();

            foreach (var elementalResitance in elementalResitances.Keys.ToList())
            {
                elementalResitances[elementalResitance] += equipmentSlots.Where(e => e.Value != null).Sum(x => x.Value.ElementalResistances.SingleOrDefault(a => a.Key == elementalResitance).Value);
            }

            return elementalResitances;
        }
        #endregion

        #region Status Effect Resistances
        public override Dictionary<StatusEffect, float> GetStatusEffectResistances()
        {
            return GetStatusEffectResistances(EquipmentSlots.CurrentPreset);
        }

        /// <summary>
        /// Retrieve Status Resistances for given preset
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        public Dictionary<StatusEffect, float> GetStatusEffectResistances(PresetSlot preset)
        {
            var equipmentPreset = EquipmentSlots.GetEquipmentPreset(preset);

            return GetStatusEffectResistances(equipmentPreset);
        }

        /// <summary>
        /// Retrieve Elemental Resistances of character for equipment preset
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        public Dictionary<StatusEffect, float> GetStatusEffectResistances(Dictionary<Slot, Equipment> equipmentPreset)
        {
            return GetStatusEffectResistancesInternal(equipmentPreset);
        }

        private Dictionary<StatusEffect, float> GetStatusEffectResistancesInternal(Dictionary<Slot, Equipment> equipmentSlots)
        {
            var statusEffectResistances = base.GetStatusEffectResistances();

            foreach (var statusEffectResistance in statusEffectResistances.Keys.ToList())
            {
                statusEffectResistances[statusEffectResistance] += equipmentSlots.Where(e => e.Value != null).Sum(x => x.Value.StatusEffectResistances.SingleOrDefault(a => a.Key == statusEffectResistance).Value);
            }

            return statusEffectResistances;
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
