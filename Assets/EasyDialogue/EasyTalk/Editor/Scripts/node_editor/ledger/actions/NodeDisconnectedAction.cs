using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeDisconnectedAction : UndoableNodeAction
    {
        public int inputId;

        public int outputId;

        public NodeDisconnectedAction(ETNode node, int inputId, int outputId) : base(node)
        {
            this.inputId = inputId;
            this.outputId = outputId;
        }

        public override void Redo(ETNodeView nodeView)
        {
            nodeView.DeleteConnection(inputId, outputId);
            nodeView.MarkDirtyRepaint();
        }

        public override void Undo(ETNodeView nodeView)
        {
            nodeView.CreateConnection(inputId, outputId);
            nodeView.MarkDirtyRepaint();
        }
    }
}
