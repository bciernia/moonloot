using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETInput : Box
    {
        private int id = NodeUtils.NextID();

        private List<int> connectedOutputs = new List<int>();

        private InputOutputType inputType = InputOutputType.ANY;

        public delegate void OnConnectionCreated(int inputId, int outputId);

        public delegate void OnConnectionDeleted(int inputId, int outputId);

        public OnConnectionCreated onConnectionCreated;

        public OnConnectionDeleted onConnectionDeleted;

        private bool allowMultipleConnections = false;

        public List<int> ConnectedOutputs
        {
            get { return connectedOutputs; }
            set { connectedOutputs = value; }
        }

        public ETInput() : base()
        {
            this.AddToClassList("node-input");

            this.onConnectionCreated += ConnectionCreated;
            this.onConnectionDeleted += ConnectionDeleted;

            this.RegisterCallback<AttachToPanelEvent>(OnAddedToParent);
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public ETInput(InputOutputType inputType) : this()
        {
            SetInputType(inputType);
        }

        private void ConnectionCreated(int inputId, int outputId)
        {
            this.RemoveFromClassList(ETUtils.GetClassListForNodeInputOutputType(this.inputType, false));
            this.AddToClassList(ETUtils.GetClassListForNodeInputOutputType(this.inputType, true));
        }

        private void ConnectionDeleted(int inputId, int outputId)
        {
            if (this.connectedOutputs.Count == 0)
            {
                this.RemoveFromClassList(ETUtils.GetClassListForNodeInputOutputType(this.inputType, true));
                this.AddToClassList(ETUtils.GetClassListForNodeInputOutputType(this.inputType, false));
            }
        }

        protected void OnAddedToParent(AttachToPanelEvent evt)
        {
            EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public void SetInputType(InputOutputType inputType)
        {
            RemoveFromClassList(ETUtils.GetClassListForNodeInputOutputType(this.inputType, this.ConnectedOutputs.Count > 0));
            AddToClassList(ETUtils.GetClassListForNodeInputOutputType(inputType, this.connectedOutputs.Count > 0));

            this.inputType = inputType;

            if (inputType == InputOutputType.DIALGOUE_FLOW)
            {
                allowMultipleConnections = true;
            }
            else
            {
                allowMultipleConnections = false;
            }
        }

        public void OnMouseMove(MouseMoveEvent evt)
        {
            Vector2 mousePos = (evt.target as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.NodeView, evt.localMousePosition);

            VisualElement elementAtMouse = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);
            if (elementAtMouse == this || evt.target == this)
            {
                this.style.opacity = 1.0f;
            }
            else
            {
                this.style.opacity = 0.7f;
            }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public InputOutputType InputType { get { return inputType; } }

        public void Connect(ETOutput output)
        {
            AddConnection(output.ID);
            output.AddConnection(this.ID);
        }

        public void Connect(int outputId)
        {
            AddConnection(outputId);

            ETOutput output = EasyTalkNodeEditor.Instance.NodeView.GetOutput(outputId);
            if(output != null)
            {
                output.AddConnection(this.ID);
            }
        }

        public void AddConnection(int outputId)
        {
            if (connectedOutputs.Count > 0 && !allowMultipleConnections)
            {
                EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(this);
            }

            if (!connectedOutputs.Contains(outputId))
            {
                connectedOutputs.Add(outputId);

                if (onConnectionCreated != null)
                {
                    EditorApplication.delayCall += delegate { onConnectionCreated(this.ID, outputId); };
                }
            }
        }

        public void Disconnect(ETOutput output)
        {
            DeleteConnection(output.ID);
            output.DeleteConnection(this.id);
        }

        public void Disconnect(int outputId)
        {
            DeleteConnection(outputId);

            ETOutput output = EasyTalkNodeEditor.Instance.NodeView.GetOutput(outputId);
            if (output != null)
            {
                output.DeleteConnection(this.ID);
            }
        }

        public void DisconnectAll()
        {
            for (int i = 0; i < connectedOutputs.Count; i++)
            {
                Disconnect(connectedOutputs[i]);
                i--;
            }
        }

        public void DeleteConnection(int outputId)
        {
            if (connectedOutputs.Contains(outputId))
            {
                connectedOutputs.Remove(outputId);

                if (onConnectionDeleted != null)
                {
                    onConnectionDeleted(this.ID, outputId);
                }
            }
        }
    }
}