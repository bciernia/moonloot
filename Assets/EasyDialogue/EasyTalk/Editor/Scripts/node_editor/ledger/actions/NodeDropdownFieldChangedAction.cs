using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeDropdownFieldChangedAction : UndoableNodeAction
    {
        public ETDropdownField dropdownField;

        public string newValue;

        public string oldValue;

        public NodeDropdownFieldChangedAction(ETNode node, ETDropdownField dropdownField, string oldValue) : base(node)
        {
            this.dropdownField = dropdownField;
            this.newValue = dropdownField.value;
            this.oldValue = oldValue;
        }

        public override void Redo(ETNodeView nodeView)
        {
            dropdownField.value = newValue;
        }

        public override void Undo(ETNodeView nodeView)
        {
            dropdownField.value = oldValue;
        }
    }
}