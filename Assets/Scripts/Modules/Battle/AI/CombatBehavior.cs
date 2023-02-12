using RPGTest.Collectors;
using RPGTest.Enums;
using RPGTest.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using Attribute = RPGTest.Enums.Attribute;

namespace RPGTest.Modules.Battle.AI
{
    public class CombatBehavior
    {
        public List<CombatThreshold> CombatThresholds { get; private set; }

    }

    public class CombatThreshold
    {
        public float Threshold { get; private set; }

        public List<AbilityBehavior> Abilities { get; private set; }

        public int MaxActionQueue { get; private set; } = 1;
    }

    public class AbilityBehavior
    {
        public string Id { get; set; }

        private float m_probability;

        public float Probability
        {
            get
            {
                if(Applicable)
                {
                    return m_probability + ProbabilityVariation;
                }
                else
                {
                    return m_probability;
                }
            }
            set { m_probability = value; }
        }

        public AbilityDecision Decision { get; set; }

        public bool Applicable { get; set; } = false;

        public float ProbabilityVariation { get; private set; } = 0.0f;

         

        public List<Entity> EvaluateAbility(Entity CasterEntity, List<Entity> casterAllies, List<Entity> casterEnemies)
        {
            Applicable = false;
            List<Entity> potentialTargets = new List<Entity>();
            List<TargetType> abilityTargetTypes = AbilitiesCollector.TryGetAbility(Id).TargetTypes;
            var abilityTargetType = abilityTargetTypes.FirstOrDefault();
            if (Decision != null)
            {
                switch (abilityTargetType)
                {
                    case TargetType.Self:
                        return EvaluateTargets(new List<Entity>() { CasterEntity });
                    case TargetType.SingleAlly:
                        potentialTargets = EvaluateTargets(casterAllies);
                        return new List<Entity>() { potentialTargets[new System.Random().Next(potentialTargets.Count - 1)] };
                    case TargetType.Allies:
                        return EvaluateTargets(casterAllies);
                    case TargetType.SingleEnemy:
                        potentialTargets = EvaluateTargets(casterEnemies);
                        return new List<Entity>() { potentialTargets[new System.Random().Next(potentialTargets.Count - 1)] };
                    case TargetType.Enemies:
                        return EvaluateTargets(casterEnemies);
                    case TargetType.All:
                        var targets = new List<Entity>();
                        targets.AddRange(casterAllies);
                        targets.AddRange(casterEnemies);
                        return EvaluateTargets(targets);
                }
            }
            else
            {
                switch (abilityTargetType)
                {
                    case TargetType.Self:
                        return new List<Entity>() { CasterEntity };
                    case TargetType.SingleAlly:
                        return new List<Entity>() { casterAllies[new System.Random().Next(casterAllies.Count - 1)] };
                    case TargetType.Allies:
                        return casterAllies;
                    case TargetType.SingleEnemy:
                        return new List<Entity>() { casterEnemies[new System.Random().Next(casterEnemies.Count - 1)] };
                    case TargetType.Enemies:
                        return casterEnemies;
                    case TargetType.All:
                        var targets = new List<Entity>();
                        targets.AddRange(casterAllies);
                        targets.AddRange(casterEnemies);
                        return targets;
                }
            }
            return null;
        }

        private List<Entity> EvaluateTargets(List<Entity> potentialTargets)
        {
            List<Entity> targets = new List<Entity>();

            foreach (var potentialTarget in potentialTargets)
            {
                if (Decision.TargetAttribute != Attribute.None)
                {
                    float attributeValue = potentialTarget.GetAttribute(Decision.TargetAttribute);
                    Applicable = true;
                    targets.Add(potentialTarget);
                }

                if (Decision.TargetElements != null)
                {
                    //TODO logic about element resistance
                }

                if (Decision.TargetStatusEffects != null)
                {
                    //TODO logic about status ailment
                }
            }
            return targets;
        }

    }

    public class AbilityDecision
    {
        public Attribute TargetAttribute { get; private set; }

        public float AttributeValue { get; private set; } = -1.0f;

        public List<Element> TargetElements { get; private set; }

        public float ElementValue { get; private set; } = 999f;

        public List<StatusEffect> TargetStatusEffects {get; private set;}
    }
}
