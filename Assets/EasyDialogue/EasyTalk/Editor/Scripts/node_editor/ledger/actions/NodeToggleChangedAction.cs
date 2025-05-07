using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeToggleChangedAction : UndoableNodeAction
    {
        public ETToggle toggle;

        public bool newValue;

        public bool oldValue;

        public NodeToggleChangedAction(ETNode node, ETToggle toggle, bool oldValue) : base(node)
        {
            this.toggle = toggle;
            this.newValue = toggle.value;
            this.oldValue = oldValue;
        }

        public override void Redo(ETNodeView nodeView)
        {
            toggle.value = newValue;
        }

        public override void Undo(ETNodeView nodeView)
        {
            toggle.value = oldValue;
        }
    }
}