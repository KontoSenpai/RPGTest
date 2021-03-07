using RPGTest.Collectors;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Battle
{
    public class UI_ActionButton : MonoBehaviour
    {
        public TextMeshProUGUI ActionName;
        public TextMeshProUGUI ActionCost;

        private ActionType m_type;
        private string m_actionId;

        public event ActionSelectedHandler PlayerActionSelected;
        public delegate void ActionSelectedHandler(ActionType type, string actionId);

        void Start()
        {

        }

        void Update()
        {

        }

        public void InitializeAction(ActionType type, (string id, bool interactable) action)
        {
            m_type = type;
            m_actionId = action.id;
            GetComponent<Button>().interactable = action.interactable;
            switch (m_type)
            {
                case ActionType.Ability:
                    var ability = AbilitiesCollector.TryGetAbility(m_actionId);
                    ActionName.text = ability.Name;
                    StringBuilder castCost = new StringBuilder();
                    if(ability.CastCost != null && ability.CastCost.Any())
                    {
                        foreach(var cost in ability.CastCost)
                        {
                            switch(cost.Key)
                            {
                                case Enums.Attribute.HP:
                                case Enums.Attribute.MP:
                                case Enums.Attribute.Stamina:
                                    castCost.AppendLine($"{Math.Abs(cost.Value)}% {(cost.Key.ToString().Replace("Current", string.Empty))}");
                                    break;
                                case Enums.Attribute.MaxHP:
                                case Enums.Attribute.MaxMP:
                                case Enums.Attribute.MaxStamina:
                                    castCost.AppendLine($"{cost.Value} {cost.Key.ToString()}");
                                    break;
                            }
                        }
                        ActionCost.text = castCost.ToString().Trim();
                    }

                    ActionCost.enabled = !string.IsNullOrEmpty(ActionCost.text);
                    break;
                case ActionType.Item:
                    var inventoryManager = FindObjectOfType<GameManager>().InventoryManager;
                    if (inventoryManager.TryGetItem(m_actionId, out Item item))
                    {
                        ActionName.text = item.Name;
                    }
                    ActionCost.text = inventoryManager.GetHeldItemQuantity(m_actionId).ToString();
                    break;
            }
            var size = this.GetComponent<RectTransform>();
            var delta = size.sizeDelta;
            delta.x = 260;
            size.sizeDelta = delta;
        }

        public void Select()
        {
            PlayerActionSelected(m_type, m_actionId);
        }
    }
}
