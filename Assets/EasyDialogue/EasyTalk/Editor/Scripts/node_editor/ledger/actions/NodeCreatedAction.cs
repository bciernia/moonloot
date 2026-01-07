using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;
using EasyTalk.Nodes.Core;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeCreatedAction : UndoableNodeAction
    {
        public NodeCreatedAction(ETNode node) : base(node) { }

        public override void Redo(ETNodeView nodeView)
        {
            int currentID = NodeUtils.CurrentID();
            Node newNode = node.CreateNode();
            NodeUtils.SetCurrentID(currentID);

            nodeView.CreateNode(newNode.NodeType, newNode, false);
        }

        public override void Undo(ETNodeView nodeView)
        {
            nodeView.DeleteNode(node.ID);
        }
    }
}