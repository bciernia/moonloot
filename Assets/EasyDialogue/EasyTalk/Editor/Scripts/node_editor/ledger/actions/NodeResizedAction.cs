using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;
using UnityEngine;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeResizedAction : UndoableNodeAction
    {
        public Vector2 newSize;

        public Vector2 newPosition;

        public Vector2 oldSize;

        public Vector2 oldPosition;

        public NodeResizedAction(ETNode node, Vector2 oldSize, Vector2 oldPosition) : base(node)
        {
            this.newSize = new Vector2(node.style.width.value.value, node.style.height.value.value);
            this.newPosition = node.transform.position;
            this.oldSize = oldSize;
            this.oldPosition = oldPosition;
        }

        public override void Redo(ETNodeView nodeView)
        {
            node.style.width = newSize.x;
            node.style.height = newSize.y;

            node.transform.position = newPosition;
        }

        public override void Undo(ETNodeView nodeView)
        {
            node.style.width = oldSize.x;
            node.style.height = oldSize.y;

            node.transform.position = oldPosition;
        }
    }
}