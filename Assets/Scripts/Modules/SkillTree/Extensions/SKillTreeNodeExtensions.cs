using System.Collections.Generic;
using System.Linq;
using RPGTest.Modules.SkillTree.Models;
using RPGTest.Modules.UI.Pause.Skills.Models;

namespace RPGTest.Modules.SkillTree.Extensions
{
    public static class SkillTreeNodeExtensions
    {
        /// <summary>
        /// Returns the type of links that should be used for current <see cref="SkillTreeNode"/>
        /// </summary>
        /// <param name="currentNode">Node to get the link type for</param>
        /// <param name="nodes">List of all nodes in a <see cref="SkillTree"/></param>  
        /// <returns>The type of connections from current node (image index)</returns>
        public static UI_Skills_SkillNodeConnections GetUISkillNodeConnections(
            this SkillTreeNode currentNode, IEnumerable<SkillTreeNode> nodes)
        {
            var dependencyLinks = currentNode.GetSkillNodeConnectionsToDependencies(nodes);
            var requirementsLinks = currentNode.GetSkillNodeConnectsionToRequirements(nodes);
            // Retrieve nodes that have the current node as a requirement
            var dependantNodes = nodes.Where(n => n.Requirements != null && n.Requirements.NodeIds.Contains(currentNode.Id)).ToList();
            
            return new UI_Skills_SkillNodeConnections()
            {
                North = dependencyLinks.North || requirementsLinks.North,
                South = dependencyLinks.South || requirementsLinks.South,
                West = dependencyLinks.West || requirementsLinks.West,
                East = dependencyLinks.East || requirementsLinks.East,
            };
        }

        public static UI_Skills_SkillNodeConnections GetSkillNodeConnectionsToDependencies(
            this SkillTreeNode currentNode, IEnumerable<SkillTreeNode> nodes)
        {
            // Retrieve nodes that have the current node as a requirement
            var dependantNodes = nodes.Where(n => n.Requirements != null && n.Requirements.NodeIds.Contains(currentNode.Id)).ToList();

            return new UI_Skills_SkillNodeConnections()
            {
                North = dependantNodes.Any(n => n.XIndex == currentNode.XIndex && n.YIndex < currentNode.YIndex),
                South = dependantNodes.Any(n => n.XIndex == currentNode.XIndex && n.YIndex > currentNode.YIndex),
                West = dependantNodes.Any(n => n.XIndex < currentNode.XIndex && n.YIndex == currentNode.YIndex),
                East = dependantNodes.Any(n => n.XIndex > currentNode.XIndex && n.YIndex == currentNode.YIndex),
            };
        }

        public static UI_Skills_SkillNodeConnections GetSkillNodeConnectsionToRequirements(
            this SkillTreeNode currentNode, IEnumerable<SkillTreeNode> nodes)
        {
            // Retrieve nodes that are requirements for current node
            var requirementNodes = currentNode.Requirements?.NodeIds
                .Select(x => nodes.Single(n => n.Id == x)).ToList()
                ?? new List<SkillTreeNode>();

            return new UI_Skills_SkillNodeConnections()
            {
                North = requirementNodes.Any(n => n.XIndex == currentNode.XIndex && n.YIndex < currentNode.YIndex),
                South = requirementNodes.Any(n => n.XIndex == currentNode.XIndex && n.YIndex > currentNode.YIndex),
                West = requirementNodes.Any(n => n.XIndex < currentNode.XIndex && n.YIndex == currentNode.YIndex),
                East = requirementNodes.Any(n => n.XIndex > currentNode.XIndex && n.YIndex == currentNode.YIndex),
            };
        }
    }
}