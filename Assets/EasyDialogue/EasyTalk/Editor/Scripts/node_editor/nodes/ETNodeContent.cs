using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public abstract class ETNodeContent : Box
    {
        protected Box inputPanel;

        protected Box outputPanel;

        protected Box contentPanel;

        protected List<ETInput> inputs = new List<ETInput>();

        protected List<ETOutput> outputs = new List<ETOutput>();

        protected NodeAlignment inputAlignment = NodeAlignment.TOP;

        protected NodeAlignment outputAlignment = NodeAlignment.BOTTOM;

        private bool isLayoutComplete = false;

        public ETNodeContent() : this(NodeAlignment.TOP, NodeAlignment.BOTTOM) { }

        public ETNodeContent(NodeAlignment inputAlignment, NodeAlignment outputAlignment) : this(inputAlignment, outputAlignment, true) { }

        public ETNodeContent(NodeAlignment inputAlignment, NodeAlignment outputAlignment, bool layout = true)
        {
            this.inputAlignment = inputAlignment;
            this.outputAlignment = outputAlignment;

            InitializePanels();

            if (layout)
            {
                Layout();
            }
        }

        public bool HasInputs()
        {
            return this.GetInputs().Count > 0;
        }

        public bool HasOutputs()
        {
            return this.GetOutputs().Count > 0;
        }

        public virtual List<ETInput> GetInputs()
        {
            return inputs;
        }

        public virtual List<ETOutput> GetOutputs()
        {
            return outputs;
        }

        protected virtual void CreateInputs() { }

        protected virtual void CreateOutputs() { }

        protected virtual void CreateContent(VisualElement contentContainer) { }

        protected ETInput AddInput(InputOutputType inputType)
        {
            ETInput input = new ETInput(inputType);
            inputs.Add(input);
            inputPanel.Add(input);

            input.onConnectionCreated += OnConnectionCreated;
            input.onConnectionDeleted += OnConnectionDeleted;
            return input;
        }

        protected virtual void OnConnectionCreated(int inputId, int outputId) { }

        protected ETOutput AddOutput(InputOutputType outputType)
        {
            ETOutput output = new ETOutput(outputType);
            outputs.Add(output);
            outputPanel.Add(output);

            output.onConnectionCreated += OnConnectionCreated;
            output.onConnectionDeleted += OnConnectionDeleted;
            return output;
        }

        protected virtual void OnConnectionDeleted(int inputId, int outputId) { }

        private void InitializePanels()
        {
            this.AddToClassList("node-content");
            this.pickingMode = PickingMode.Ignore;
            this.style.backgroundColor = new Color(0, 0, 0, 0);

            inputPanel = new Box();
            inputPanel.pickingMode = PickingMode.Ignore;
            inputPanel.AddToClassList("node-input-panel");

            switch (inputAlignment)
            {
                case NodeAlignment.TOP: inputPanel.style.alignSelf = Align.FlexStart; break;
                case NodeAlignment.CENTER: inputPanel.style.alignSelf = Align.Center; break;
                case NodeAlignment.BOTTOM: inputPanel.style.alignSelf = Align.FlexEnd; break;
            }

            outputPanel = new Box();
            outputPanel.pickingMode = PickingMode.Ignore;
            outputPanel.AddToClassList("node-output-panel");

            switch (outputAlignment)
            {
                case NodeAlignment.TOP: outputPanel.style.alignSelf = Align.FlexStart; break;
                case NodeAlignment.CENTER: outputPanel.style.alignSelf = Align.Center; break;
                case NodeAlignment.BOTTOM: outputPanel.style.alignSelf = Align.FlexEnd; break;
            }

            contentPanel = new Box();
            contentPanel.pickingMode = PickingMode.Ignore;
            contentPanel.AddToClassList("node-content-panel");
        }

        protected void RemoveComponents()
        {
            for (int i = 0; i < this.childCount; i++)
            {
                this.RemoveAt(0);
            }

            inputs.Clear();
            outputs.Clear();
        }

        public virtual void Layout()
        {
            if (isLayoutComplete) { return; }

            this.AddToClassList("node-content");
            //RemoveComponents();

            CreateInputs();
            CreateContent(contentPanel);
            CreateOutputs();

            if (HasInputs() && HasOutputs())
            {
                //Center and fill width
                this.AddToClassList("node-content-panel-with-inputs-and-outputs");
                this.Add(inputPanel);
                this.Add(contentPanel);
                this.Add(outputPanel);
            }
            else if (HasInputs())
            {
                //Align left, fill to output edge
                this.AddToClassList("node-content-panel-with-inputs");
                this.Add(inputPanel);
                this.Add(contentPanel);
            }
            else if (HasOutputs())
            {
                //Align right, fill to input edge
                this.AddToClassList("node-content-panel-with-outputs");
                this.Add(contentPanel);
                this.Add(outputPanel);
            }
            else
            {
                //Center, fill between input and output edges
                this.AddToClassList("node-content-panel-without-inputs-or-outputs");
                this.Add(contentPanel);
            }

            isLayoutComplete = true;
        }

        public ETNode GetParentNode(VisualElement parent)
        {
            while (parent != null && !(parent is ETNode))
            {
                return GetParentNode(parent.parent);
            }

            return parent as ETNode;
        }

        public VisualElement ContentPanel { get { return contentPanel; } }

        public enum NodeAlignment { TOP, CENTER, BOTTOM }
    }
}