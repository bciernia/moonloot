using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeItemRemovedAction : UndoableNodeAction
    {
        public ETNodeContent itemContent;

        public int index;

        public NodeItemRemovedAction(ETNode node, ETNodeContent itemContent, int index) : base(node)
        {
            this.itemContent = itemContent;
            this.index = index;
        }

        public override void Redo(ETNodeView nodeView)
        {
            ETListNode listNode = ((ETListNode)node);
            listNode.ListPanel.RemoveItem(itemContent, false);
        }

        public override void Undo(ETNodeView nodeView)
        {
            ETListNode listNode = ((ETListNode)node);
            listNode.ListPanel.InsertItem(index, itemContent, false);
        }
    }
}