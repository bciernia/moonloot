using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public abstract class UndoableNodeAction
    {
        public ETNode node;

        public UndoableNodeAction(ETNode node)
        {
            this.node = node;
        }

        public abstract void Undo(ETNodeView nodeView);

        public abstract void Redo(ETNodeView nodeView);
    }
}
