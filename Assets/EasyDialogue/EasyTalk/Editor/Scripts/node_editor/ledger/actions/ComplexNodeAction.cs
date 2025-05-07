using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class ComplexNodeAction : UndoableNodeAction
    {
        public List<UndoableNodeAction> actions = new List<UndoableNodeAction>();

        public string actionName;

        public Node newNode;

        public Node oldNode;

        public int nodeId;

        public ComplexNodeAction(int nodeId, Node oldNode, string actionName) : base(null)
        {
            this.actionName = actionName;
            this.oldNode = oldNode;
            this.nodeId = nodeId;
        }

        public override void Redo(ETNodeView nodeView)
        {
            nodeView.DeleteNode(newNode.ID);
            nodeView.CreateNode(newNode.NodeType, newNode, false);

            nodeView.MarkDirtyRepaint();
        }

        public override void Undo(ETNodeView nodeView)
        {
            newNode = EasyTalkNodeEditor.Instance.NodeView.FindNode(nodeId).CreateNode();

            nodeView.DeleteNode(oldNode.ID);
            nodeView.CreateNode(oldNode.NodeType, oldNode, false);

            nodeView.MarkDirtyRepaint();
        }
    }
}
