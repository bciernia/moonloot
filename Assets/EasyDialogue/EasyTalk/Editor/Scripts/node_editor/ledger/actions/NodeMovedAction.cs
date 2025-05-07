using EasyTalk.Editor.Components;
using EasyTalk.Editor.Nodes;
using EasyTalk.Editor.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Editor.Ledger.Actions
{
    public class NodeMovedAction : UndoableNodeAction
    {
        public Dictionary<int, Vector2> newPositions = new Dictionary<int, Vector2>();

        public Dictionary<int, Vector2> oldPositions = new Dictionary<int, Vector2>();

        public NodeMovedAction(Dictionary<int, Vector2> oldPositions) : base(null)
        {
            foreach (int id in oldPositions.Keys)
            {
                this.oldPositions.Add(id, oldPositions[id]);

                ETNode node = EasyTalkNodeEditor.Instance.NodeView.FindNode(id);
                if (node != null)
                {
                    this.newPositions.Add(id, node.transform.position);
                }
            }
        }

        public override void Redo(ETNodeView nodeView)
        {
            nodeView.MoveNodes(newPositions);
            nodeView.MarkDirtyRepaint();
        }

        public override void Undo(ETNodeView nodeView)
        {
            nodeView.MoveNodes(oldPositions);
            nodeView.MarkDirtyRepaint();
        }
    }
}