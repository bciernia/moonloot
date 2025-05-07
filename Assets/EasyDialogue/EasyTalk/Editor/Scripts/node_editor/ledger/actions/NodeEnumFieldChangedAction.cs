using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;
using System;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeEnumFieldChangedAction : UndoableNodeAction
    {
        public ETEnumField enumField;

        public Enum newValue;

        public Enum oldValue;

        public NodeEnumFieldChangedAction(ETNode node, ETEnumField enumField, Enum oldValue) : base(node)
        {
            this.enumField = enumField;
            this.newValue = enumField.value;
            this.oldValue = oldValue;
        }

        public override void Redo(ETNodeView nodeView)
        {
            enumField.value = newValue;
        }

        public override void Undo(ETNodeView nodeView)
        {
            enumField.value = oldValue;
        }
    }
}