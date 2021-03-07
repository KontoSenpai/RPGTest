using RPGTest.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTest.Models
{
    public class Stance
    {
        public StancesTypes BaseStance { get; set; } = StancesTypes.Balance;

        private StancesTypes m_currentStance;


        private List<StancesTypes> m_stancesCyclingArray = new List<StancesTypes>()
        {
            StancesTypes.Offense,
            StancesTypes.Balance,
            StancesTypes.Defense
        };


        public void InitializeCurrentStance()
        {
            m_currentStance = BaseStance;
        }

        public StanceVariations GetCurrentStanceVariations()
        {
            return m_stancesVariations[m_currentStance];
        }

        public StancesTypes GetCurrentStance()
        {
            return m_currentStance;
        }

        public void ChangeCurrentStance(bool cycleRight)
        {
            int currentIndex = m_stancesCyclingArray.IndexOf(m_currentStance);
            if (cycleRight)
            {
                if (currentIndex < m_stancesCyclingArray.Count -1)
                {
                    m_currentStance = m_stancesCyclingArray[currentIndex + 1];
                }
                else
                {
                    m_currentStance = m_stancesCyclingArray[0];
                }
            }
            else
            {
                if (currentIndex > 0)
                {

                    m_currentStance = m_stancesCyclingArray[currentIndex - 1];
                }
                else
                {
                    m_currentStance = m_stancesCyclingArray[m_stancesCyclingArray.Count - 1];
                }
            }
        }

        private static Dictionary<StancesTypes, StanceVariations> m_stancesVariations = new Dictionary<StancesTypes, StanceVariations>()
        {
            {
                StancesTypes.Offense, new StanceVariations()
                {
                    Attributes = new Dictionary<Attribute, float>()
                    {
                        { Attribute.Speed , 1.1f },
                        { Attribute.Block, 0.75f }
                    }
                }
            },
            {
                StancesTypes.Balance, new StanceVariations()
                {
                    Attributes = new Dictionary<Attribute, float>()
                }
            },
            {
                StancesTypes.Defense, new StanceVariations()
                {
                    Attributes = new Dictionary<Attribute, float>()
                    {
                        { Attribute.Attack, 1.2f },
                        { Attribute.Speed , 0.7f },
                        { Attribute.Block , 1.25f }
                    }
                }
            }
        };
    }

    public class StanceVariations
    {
        public Dictionary<Attribute, float> Attributes { get; set; }
    }
}

