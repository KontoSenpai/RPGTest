using RPGTest.AI;
using RPGTest.Assets.Scripts.Models.Items;
using RPGTest.Enums;
using RPGTest.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGTest.Models.Entity
{
    [Serializable]
    public class Enemy : Entity
    {
        public CombatBehavior CombatBehavior { get; set; }

        public List<CombatThreshold> CombatThresholds { get; private set; }

        public int ExperienceBounty { get; set; }

        public int GoldBounty { get; set; }

        public LootTable LootTable { get; set; }

        /// <summary>
        /// Required for serialization
        /// </summary>
        public Enemy()
        {

        }

        public Enemy(Enemy model, string suffix)
        {
            CombatThresholds = model.CombatThresholds;
            Id = model.Id;

            Level = model.Level;
            CurrentHP = model.BaseAttributes.MaxHP;
            CurrentMP = model.BaseAttributes.MaxMP;
            CurrentStamina = model.BaseAttributes.MaxStamina;

            Name = $"{model.Name}{(string.IsNullOrEmpty(suffix) ? "" : $" {suffix}")}";

            BaseAttributes = model.BaseAttributes;

            StatusEffectResistance = model.StatusEffectResistance;

            StatusEffects = model.StatusEffects;

            ElementResistance = model.ElementResistance;

            GoldBounty = model.GoldBounty;

            ExperienceBounty = model.ExperienceBounty;

            LootTable = model.LootTable;
        }

        public override IEnumerator SelectAction(BattleManager manager, List<PlayableCharacter> playerParty, List<Enemy> enemyParty, System.Action<ActionSequence> selectedActions)
        {
            var hpPercentage = CurrentHP / BaseAttributes.MaxHP;
            List<TempAbility> tempAbilities = new List<TempAbility>();
            var currentThresold = CombatThresholds.Where(c => hpPercentage <= c.Threshold).OrderByDescending(c => c.Threshold).FirstOrDefault();
            foreach (var ability in currentThresold.Abilities)
            {
                tempAbilities.Add(new TempAbility(ability.Id,
                                                    ability.Probability,
                                                    ability.EvaluateAbility(this, enemyParty.ToList<Entity>(), playerParty.ToList<Entity>())));
            }

            var selectedAbility = GetChosenAbility(tempAbilities);
            ActionSequence actionSequence = new ActionSequence(this);
            actionSequence.AddAction( new EntityAction(this, ActionType.Ability, selectedAbility.AbilityId, selectedAbility.Targets));
            selectedActions(actionSequence);

            yield return null;
        }

        private TempAbility GetChosenAbility(List<TempAbility> abilities)
        {
            abilities.OrderBy(x => x.Probability);
            var index = UnityEngine.Random.Range(0, abilities.Select(x => x.Probability).Sum());
            List<float> previousProbalities = new List<float>();
            foreach(var ability in abilities)
            {
                if (index <= previousProbalities.Sum() + ability.Probability)
                {
                    return ability;
                }
                else
                {
                    previousProbalities.Add(ability.Probability);
                }
            }
            return null;
        }
    }

    public class TempAbility
    {
        public string AbilityId { get; private set; }
        public List<Entity> Targets { get; private set; }

        public float Probability { get; private set; }

        public TempAbility(string id, float probability, List<Entity> targets)
        {
            AbilityId = id;
            Probability = probability;
            Targets = targets;
        }
    }

    public class EnemyAbility
    {
        public string Id { get; private set; }
        public Condition Trigger { get; private set; }
    }

    public class Condition
    {
        public TargetType target { get; private set; }

        public Dictionary<Enums.Attribute, int> AttributeThresold { get; private set; }
    }
}
