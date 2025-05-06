using EasyTalk.Editor.Components;
using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Ledger
{
    public class ETLedger
    {
        private int maxUndoSize = 500;

        public Stack<UndoableNodeAction> undoStack;

        public Stack<UndoableNodeAction> redoStack;

        public bool enabled = true;

        private ComplexNodeAction complexAction = null;

        public ETLedger()
        {
            undoStack = new Stack<UndoableNodeAction>(maxUndoSize);
            redoStack = new Stack<UndoableNodeAction>(maxUndoSize);
        }

        public void StartComplexAction(int nodeId, Node oldNodeState, string actionName)
        {
            if (complexAction == null && enabled)
            {
                complexAction = new ComplexNodeAction(nodeId, oldNodeState, actionName);
            }
        }

        public void EndComplexAction(string actionName)
        {
            if (complexAction != null && enabled)
            {
                if (complexAction.actionName.Equals(actionName))
                {
                    undoStack.Push(complexAction);
                    redoStack.Clear();
                    complexAction = null;
                }
            }
        }

        public void AddAction(UndoableNodeAction action)
        {
            if (complexAction != null)
            {
                complexAction.actions.Insert(0, action);
            }
            else
            {
                if (enabled)
                {
                    undoStack.Push(action);
                    redoStack.Clear();
                }
            }
        }

        public void Undo(ETNodeView nodeView)
        {
            if (undoStack.Count > 0)
            {
                enabled = false;
                UndoableNodeAction action = undoStack.Pop();

                if (action != null)
                {
                    action.Undo(nodeView);
                    redoStack.Push(action);
                    EasyTalkNodeEditor.Instance.NodesChanged();
                }

                EditorApplication.delayCall += delegate { enabled = true; };
            }
        }

        public void Redo(ETNodeView nodeView)
        {
            if (redoStack.Count > 0)
            {
                enabled = false;
                UndoableNodeAction action = redoStack.Pop();

                if (action != null)
                {
                    action.Redo(nodeView);
                    undoStack.Push(action);
                    EasyTalkNodeEditor.Instance.NodesChanged();
                }

                EditorApplication.delayCall += delegate { enabled = true; };
            }
        }

        public void Clear()
        {
            complexAction = null;
            undoStack.Clear();
        }

        public void Reset()
        {
            enabled = true;
            Clear();
        }
    }
}