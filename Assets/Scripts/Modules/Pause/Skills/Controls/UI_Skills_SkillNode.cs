using System;
using RPGTest.Modules.SkillTree.Models;
using RPGTest.Modules.UI.Pause.Skills.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.Modules.UI.Pause.Skills.Controls
{
    public class SkillNodeSelectedEventArgs : EventArgs
    {
        public SkillNodeSelectedEventArgs(SkillTreeNode node)
        {
            SkillNode = node;
        }

        public SkillTreeNode SkillNode { get; }
    }

    public class UI_Skills_SkillNode : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI SkillNodeLevel;

        [SerializeField] private Image NodeBackground;
        [SerializeField] private Image NodeOutline;

        [SerializeField] private Image SkillImage;

        [SerializeField] private Color NodeOutlineUnlearntColor;
        [SerializeField] private Color NodeOutlineLearntColor;
        [SerializeField] private Color NodeOutlineMasteredColor;

        [SerializeField] private Image NodeLinkN;
        [SerializeField] private Image NodeLinkS;
        [SerializeField] private Image NodeLinkW;
        [SerializeField] private Image NodeLinkE;

        private SkillTreeNode m_skillNode;

        public void Initialize(SkillTreeNode node, UI_Skills_SkillNodeConnections connections, int level)
        {
            m_skillNode = node;

            GetComponent<Button>().enabled = node != null;

            NodeBackground.enabled = node != null;
            NodeOutline.enabled = node != null;

            NodeLinkN.enabled = connections.North;
            NodeLinkS.enabled = connections.South;
            NodeLinkW.enabled = connections.West;
            NodeLinkE.enabled = connections.East;

            if (node == null)
            {
                SkillNodeLevel.text = "";
                return;
            }

            SkillNodeLevel.text = $"{level}/{node.MaxRank}";
            if (level == node.MaxRank)
            {
                NodeOutline.color = NodeOutlineMasteredColor;
            }
            else
            {
                NodeOutline.color = level > 0 ? NodeOutlineLearntColor : NodeOutlineUnlearntColor;
            }
        }

        public void OnSkillNode_Selected()
        {
            SkillNodeSelected?.Invoke(this, new SkillNodeSelectedEventArgs(m_skillNode));
        }

        public int GetXCoordinate()
        {
            return m_skillNode.XIndex;
        }

        public int GetYCoordinate()
        {
            return m_skillNode.YIndex;
        }

        public SkillTreeNode GetSkillTreeNode()
        {
            return m_skillNode;
        }

        public bool HasValue()
        {
            return m_skillNode != null;
        }

        [HideInInspector]
        public event EventHandler<SkillNodeSelectedEventArgs> SkillNodeSelected;
    }
}