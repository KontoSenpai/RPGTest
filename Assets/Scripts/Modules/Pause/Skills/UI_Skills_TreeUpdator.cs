using System;
using System.Collections.Generic;
using System.Linq;
using RPGTest.Collectors;
using RPGTest.Helpers;
using RPGTest.Models.Entity;
using RPGTest.Modules.SkillTree.Extensions;
using RPGTest.Modules.SkillTree.Models;
using RPGTest.Modules.UI.Pause.Skills.Controls;
using RPGTest.Modules.UI.Pause.Skills.Models;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.Modules.UI.Pause.Skills
{
    public class UI_Skills_TreeUpdator : MonoBehaviour
    {
        [SerializeField] private int RowCount = 5;

        [SerializeField] private int YSearchAllowance = 2;
        [SerializeField] private int XSearchAllowance = 1;

        /// <summary>
        /// Initialize the UI SkillTreeNode for all <see cref="SkillTreeNode"/> in a <see cref="PlayableCharacter"/> skill tree
        /// </summary>
        /// <param name="character"></param>
        public void Initialize(PlayableCharacter character)
        {
            SkillTree.Models.SkillTree skillTree = SkillTreesCollector.TryGetSkillTree(character.SkillTreeComponent.SkillTreeId);

            var skillNodes = GetComponentsInChildren<UI_Skills_SkillNode>().ToList();

            for (int i = 0; i < skillNodes.Count; i++)
            {
                var x = Mathf.FloorToInt(i / RowCount);
                var y = Mathf.FloorToInt(i % (RowCount));

                var skillNode = skillTree.SkillNodes?.SingleOrDefault(n => n.XIndex == x && n.YIndex == y);
                var skillNodeConnections = skillNode?.GetUISkillNodeConnections(skillTree.SkillNodes) ?? new UI_Skills_SkillNodeConnections();
                var skillNodeLevel = skillNode != null ? character.SkillTreeComponent.SpentSkillPoints.GetValueOrDefault(skillNode.Id, 0) : 0;

                skillNodes[i].Initialize(skillNode, skillNodeConnections, skillNodeLevel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ExplicitNavigation()
        {
            var skillNodes = GetComponentsInChildren<UI_Skills_SkillNode>().Where(n => n.HasValue()).ToList();
            foreach (var skillNode in skillNodes)
            {
                ExplicitNodeNavigation(skillNode, skillNodes.Where(x => x.gameObject.name != skillNode.gameObject.name));
            }
        }

        #region Private Methods

        private void ExplicitNodeNavigation(UI_Skills_SkillNode node, IEnumerable<UI_Skills_SkillNode> otherNodes)
        {
            var skillNode = node.GetSkillTreeNode();

            var northNode = otherNodes
                .Where(o => o.GetYCoordinate() < node.GetYCoordinate() && MathF.Abs(node.GetXCoordinate() - o.GetXCoordinate()) < YSearchAllowance)
                .OrderByDescending(o => o.GetYCoordinate())
                .ThenBy(o => MathF.Abs(node.GetXCoordinate() - o.GetXCoordinate()))
                .ThenBy(o => o.GetXCoordinate())
                .FirstOrDefault();

            var southNode = otherNodes
                .Where(o => o.GetYCoordinate() > node.GetYCoordinate() && MathF.Abs(node.GetXCoordinate() - o.GetXCoordinate()) < YSearchAllowance)
                .OrderBy(o => o.GetYCoordinate())
                .ThenBy(o => MathF.Abs(node.GetXCoordinate() - o.GetXCoordinate()))
                .ThenByDescending(o => o.GetXCoordinate())
                .FirstOrDefault();

            var westNode = otherNodes
                .Where(o => o.GetXCoordinate() < node.GetXCoordinate() && MathF.Abs(node.GetYCoordinate() - o.GetYCoordinate()) < XSearchAllowance)
                .OrderByDescending(o => o.GetXCoordinate())
                .ThenBy(o => MathF.Abs(node.GetYCoordinate() - o.GetYCoordinate()))
                .ThenBy(o => o.GetYCoordinate())
                .FirstOrDefault();

            var eastNode = otherNodes
                .Where(o => o.GetXCoordinate() > node.GetXCoordinate() && MathF.Abs(node.GetYCoordinate() - o.GetYCoordinate()) < XSearchAllowance)
                .OrderBy(o => o.GetXCoordinate())
                .ThenBy(o => MathF.Abs(node.GetYCoordinate() - o.GetYCoordinate()))
                .ThenByDescending(o => o.GetYCoordinate())
                .FirstOrDefault();

            node.gameObject.GetComponent<Button>().ExplicitNavigation(
                westNode?.gameObject.GetComponent<Button>(), 
                eastNode?.gameObject.GetComponent<Button>(), 
                northNode?.gameObject.GetComponent<Button>(), 
                southNode?.gameObject.GetComponent<Button>());
        }
        #endregion
    }
}