using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeTextFieldChangedAction : UndoableNodeAction
    {
        public ETTextField textfield;

        public string newValue;

        public string oldValue;

        public NodeTextFieldChangedAction(ETNode node, ETTextField textfield, string oldValue) : base(node)
        {
            this.textfield = textfield;
            this.newValue = textfield.value;
            this.oldValue = oldValue;
        }

        public override void Redo(ETNodeView nodeView)
        {
            textfield.value = newValue;
        }

        public override void Undo(ETNodeView nodeView)
        {
            textfield.value = oldValue;
        }
    }
}
