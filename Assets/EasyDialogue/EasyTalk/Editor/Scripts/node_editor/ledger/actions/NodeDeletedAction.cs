using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeDeletedAction : UndoableNodeAction
    {
        public List<Node> oldNodes = new List<Node>();

        public NodeDeletedAction(List<Node> nodes) : base(null)
        {
            this.oldNodes = nodes;
        }

        public override void Redo(ETNodeView nodeView)
        {
            foreach (Node node in oldNodes)
            {
                nodeView.DeleteNode(node.ID);
            }

            nodeView.MarkDirtyRepaint();
        }

        public override void Undo(ETNodeView nodeView)
        {
            List<ETNode> newNodes = new List<ETNode>();

            foreach (Node node in oldNodes)
            {
                ETNode newNode = nodeView.CreateNode(node.NodeType, node, false);
                newNodes.Add(newNode);
            }

            nodeView.MarkDirtyRepaint();
        }
    }
}
