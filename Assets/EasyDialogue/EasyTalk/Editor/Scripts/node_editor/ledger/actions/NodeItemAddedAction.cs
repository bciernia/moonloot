using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeItemAddedAction : UndoableNodeAction
    {
        public ETNodeContent itemContent;

        public NodeItemAddedAction(ETNode node, ETNodeContent itemContent) : base(node)
        {
            this.itemContent = itemContent;
        }

        public override void Redo(ETNodeView nodeView)
        {
            ETListNode listNode = ((ETListNode)node);
            listNode.ListPanel.AddItem(itemContent, false);
        }

        public override void Undo(ETNodeView nodeView)
        {
            ETListNode listNode = ((ETListNode)node);
            listNode.ListPanel.RemoveItem(itemContent, false);
        }
    }
}
