using RPGTest.Collectors;
using RPGTest.Managers;
using RPGTest.Models.Entity;
using RPGTest.Models.Items;
using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.UI.Battle
{
    public class UI_ActionButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ActionName;
        [SerializeField] private TextMeshProUGUI ActionCost;

        [SerializeField] private Color UnusableColor;
        [SerializeField] private Color UnusableSelectedColor;

        private ActionType m_type;
        private string m_actionId;
        private bool m_interactable;

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
            //GetComponent<Button>().interactable = action.interactable;
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
                                    castCost.AppendLine($"{Math.Abs(cost.Value)} {(cost.Key.ToString().Replace("Current", string.Empty))}");
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
                    m_interactable = action.interactable;

                    if (!m_interactable)
                    {
                        var color = this.GetComponent<Button>().colors;
                        color.normalColor = UnusableColor;
                        color.selectedColor = UnusableSelectedColor;
                        color.pressedColor = UnusableSelectedColor;

                        this.GetComponent<Button>().colors = color;
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
            if (m_interactable)
            {
                PlayerActionSelected(m_type, m_actionId);
            }
        }
    }
}
