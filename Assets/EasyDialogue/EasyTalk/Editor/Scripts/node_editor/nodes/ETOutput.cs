using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETOutput : Box
    {
        private int id = NodeUtils.NextID();

        private List<int> connectedInputs = new List<int>();

        private InputOutputType outputType = InputOutputType.ANY;

        public delegate void OnConnectionCreated(int inputId, int outputId);

        public delegate void OnConnectionDeleted(int inputId, int outputId);

        public OnConnectionCreated onConnectionCreated;

        public OnConnectionDeleted onConnectionDeleted;

        private bool allowMultipleConnections = false;

        public List<int> ConnectedInputs
        {
            get { return connectedInputs; }
            set { connectedInputs = value; }
        }

        public ETOutput() : base()
        {
            this.AddToClassList("node-output");

            this.onConnectionCreated += ConnectionCreated;
            this.onConnectionDeleted += ConnectionDeleted;

            this.RegisterCallback<AttachToPanelEvent>(OnAddedToParent);
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public ETOutput(InputOutputType outputType) : this()
        {
            SetOutputType(outputType);
        }

        private void ConnectionCreated(int inputId, int outputId)
        {
            this.RemoveFromClassList(ETUtils.GetClassListForNodeInputOutputType(this.outputType, false));
            this.AddToClassList(ETUtils.GetClassListForNodeInputOutputType(this.outputType, true));
        }

        private void ConnectionDeleted(int inputId, int outputId)
        {
            if (this.connectedInputs.Count == 0)
            {
                this.RemoveFromClassList(ETUtils.GetClassListForNodeInputOutputType(this.outputType, true));
                this.AddToClassList(ETUtils.GetClassListForNodeInputOutputType(this.outputType, false));
            }
        }

        protected void OnAddedToParent(AttachToPanelEvent evt)
        {
            EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public void SetOutputType(InputOutputType outputType)
        {
            RemoveFromClassList(ETUtils.GetClassListForNodeInputOutputType(this.outputType, ConnectedInputs.Count > 0));
            AddToClassList(ETUtils.GetClassListForNodeInputOutputType(outputType, ConnectedInputs.Count > 0));

            this.outputType = outputType;

            if (outputType == InputOutputType.DIALGOUE_FLOW ||
                outputType == InputOutputType.DIALOGUE_TRUE_FLOW ||
                outputType == InputOutputType.DIALOGUE_FALSE_FLOW)
            {
                allowMultipleConnections = false;
            }
            else
            {
                allowMultipleConnections = true;
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

        public InputOutputType OutputType { get { return outputType; } }

        public void Connect(ETInput input)
        {
            AddConnection(input.ID);
            input.AddConnection(this.ID);
        }

        public void Connect(int inputId)
        {
            AddConnection(inputId);

            ETInput input = EasyTalkNodeEditor.Instance.NodeView.GetInput(inputId);
            if (input != null)
            {
                input.Connect(this.ID);
            }
        }

        public void AddConnection(int inputId)
        {
            if (connectedInputs.Count > 0 && !allowMultipleConnections)
            {
                EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(this);
            }

            if (!connectedInputs.Contains(inputId))
            {
                connectedInputs.Add(inputId);

                if (onConnectionCreated != null)
                {
                    EditorApplication.delayCall += delegate { onConnectionCreated(this.ID, inputId); };
                }
            }
        }

        public void Disconnect(ETInput input)
        {
            DeleteConnection(input.ID);
            input.DeleteConnection(this.ID);
        }

        public void Disconnect(int inputId)
        {
            DeleteConnection(inputId);

            ETInput input = EasyTalkNodeEditor.Instance.NodeView.GetInput(inputId);
            if (input != null)
            {
                input.DeleteConnection(this.ID);
            }
        }

        public void DisconnectAll()
        {
            for(int i = 0; i < connectedInputs.Count; i++)
            {
                Disconnect(connectedInputs[i]);
                i--;
            }
        }

        public void DeleteConnection(int inputId)
        {
            if (connectedInputs.Contains(inputId))
            {
                connectedInputs.Remove(inputId);

                if (onConnectionDeleted != null)
                {
                    onConnectionDeleted(this.ID, inputId);
                }
            }
        }
    }
}