using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETValueSelectorNode : ETListNode
    {
        private ETValueSelectInputContent inputContent;

        private ETValueSelectOutputContent outputContent;

        private InputOutputType ioType = InputOutputType.STRING;

        private ETTextField.ValidationType validationType = ETTextField.ValidationType.STRING;

        public ValueSelectorNode storedNodeState;

        public ETValueSelectorNode() : base("VALUE SELECT", "value-select-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Path Selector Node";
            this.connectionLine.AddToClassList("value-select-line");
            this.resizeType = ResizeType.BOTH;
            this.SetDimensions(196, 224);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETValueSelectInputContent();
            inputContent.OutputTypeField.RegisterCallback<ChangeEvent<Enum>>(OnOutputTypeChanged);
            this.AddContent(inputContent);

            CreateListPanel();
            listPanel.style.flexGrow = 0;
            listPanel.style.paddingLeft = 0;
            listPanel.style.paddingRight = 0;
            AddItemToList();
            AddItemToList();

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;

            outputContent = new ETValueSelectOutputContent();
            this.AddContent(outputContent);

            AddNodeChangeCallbacks();
        }

        private void AddNodeChangeCallbacks()
        {
            foreach (ETInput input in Inputs)
            {
                input.onConnectionCreated += ConnectionCreatedOrDeleted;
                input.onConnectionDeleted += ConnectionCreatedOrDeleted;
            }

            foreach (ETOutput output in Outputs)
            {
                output.onConnectionCreated += ConnectionCreatedOrDeleted;
                output.onConnectionDeleted += ConnectionCreatedOrDeleted;
            }
        }

        public void UpdateStoredNodeState()
        {
            storedNodeState = CreateNode() as ValueSelectorNode;
        }

        public override void NodeMoved()
        {
            UpdateStoredNodeState();
        }

        public override void NodeResized()
        {
            UpdateStoredNodeState();
        }

        private void ConnectionCreatedOrDeleted(int inputId, int outputId)
        {
            UpdateStoredNodeState();
        }

        private void OnOutputTypeChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(this.ID, storedNodeState, "Changed Value Selector Node Output Type");

            ValueOutputType outputType = (ValueOutputType)inputContent.OutputTypeField.value;
            switch(outputType)
            {
                case ValueOutputType.INT: 
                    ioType = InputOutputType.INT;
                    validationType = ETTextField.ValidationType.INT;
                    break;
                case ValueOutputType.FLOAT: 
                    ioType = InputOutputType.FLOAT;
                    validationType = ETTextField.ValidationType.FLOAT;
                    break;
                case ValueOutputType.BOOL: 
                    ioType = InputOutputType.BOOL; 
                    validationType = ETTextField.ValidationType.BOOL;
                    break;
                case ValueOutputType.STRING: 
                    ioType = InputOutputType.STRING; 
                    validationType = ETTextField.ValidationType.STRING;
                    break;
            }

            foreach(ETValueContent valueContent in Items)
            {
                ETInput valueInput = valueContent.GetInputs()[0];
                valueInput.SetInputType(ioType);
                valueContent.ValueField.SetValidationType(validationType);

                if (valueInput.ConnectedOutputs.Count > 0)
                {
                    ETOutput output = EasyTalkNodeEditor.Instance.NodeView.GetOutput(valueInput.ConnectedOutputs[0]);
                    if (output != null && !ETUtils.IsConnectionAllowed(valueInput.InputType, output.OutputType))
                    {
                        valueInput.DisconnectAll();
                    }
                }
            }

            outputContent.ValueOutput.SetOutputType(ioType);

            EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(outputContent.ValueOutput);

            EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Changed Value Selector Node Output Type");
            UpdateStoredNodeState();
        }

        private void OnItemAdded(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() + 32;
            UpdateStoredNodeState();
        }

        private void OnItemRemoved(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() - 25.0f;

            for (int i = 0; i < Items.Count; i++)
            {
                ETValueContent content = (ETValueContent)Items[i];
                content.VariableLabel.text = "Value " + (i + 1);
            }

            UpdateStoredNodeState();
        }

        public override Node CreateNode()
        {
            ValueSelectorNode selectorNode = new ValueSelectorNode();
            selectorNode.ID = id;
            selectorNode.Index = inputContent.Index;
            selectorNode.ValueOutputType = inputContent.OutputTypeField.value.ToString();

            CreateInputsForNode(selectorNode);
            CreateOutputsForNode(selectorNode);
            SetSizeForNode(selectorNode);
            SetPositionForNode(selectorNode);

            for (int i = 0; i < Items.Count; i++)
            {
                ETValueContent valueContent = (ETValueContent)Items[i];
                if (valueContent.ValueField.text != null && valueContent.ValueField.text.Length > 0)
                {
                    selectorNode.AddItem(new ValueSelectorListItem(valueContent.ValueField.text));
                }
                else
                {
                    selectorNode.AddItem(new ValueSelectorListItem());
                }
            }

            return selectorNode;
        }

        public override void InitializeFromNode(Node node)
        {
            ValueSelectorNode selectorNode = node as ValueSelectorNode;
            this.id = selectorNode.ID;

            SetPositionFromNode(selectorNode);

            while (Items.Count > selectorNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < selectorNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(selectorNode);

            inputContent.OutputTypeField.value = Enum.Parse<ValueOutputType>(selectorNode.ValueOutputType);
            inputContent.Index = selectorNode.Index;

            for (int i = 0; i < Items.Count; i++)
            {
                ETValueContent valueContent = Items[i] as ETValueContent;
                valueContent.ValueField.value = (selectorNode.Items[i] as ValueSelectorListItem).Value;
                valueContent.GetInputs()[0].SetInputType(selectorNode.Inputs[i + 2].ConnectionType);
            }

            InitializeAllInputsAndOutputsFromNode(selectorNode);

            UpdateStoredNodeState();
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETValueContent content = new ETValueContent();
            content.GetInputs()[0].SetInputType(ioType);
            content.ValueField.SetValidationType(validationType);
            content.VariableLabel.text = "Value " + (Items.Count + 1);

            content.RemoveButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(content);
                }
            };
            return content;
        }

        public override float GetMinimumWidth()
        {
            return 196.0f;
        }

        public override float GetMinimumHeight()
        {
            return 164.0f + (ListPanel.Items.Count * 29.0f);
        }

        public override bool HasText(string text)
        {
            if(base.HasText(text)) { return true; }

            foreach(ETValueContent valueContent in Items)
            {
                if (valueContent.ValueField.text.ToLower().Contains(text.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            /*if (((ValueOutputType)inputContent.OutputTypeField.value) == ValueOutputType.STRING)
            {
                TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

                foreach(ETValueContent valueContent in Items)
                {
                    if (valueContent.GetInputs()[0].ConnectedOutputs.Count == 0 && valueContent.ValueField.value.ToString().Length > 0)
                    {
                        sourceSet.AddOrFindTranslation(valueContent.ValueField.value.ToString());
                    }
                }
            }*/
        }

        protected override string GetNodeTooltip()
        {
            return "Allows a value to be selected (via an integer index) and sent to another node's input.";
        }
    }

    public class ETValueSelectInputContent : ETNodeContent
    {
        private ETEnumField outputTypeField;

        private ETTextField indexField;

        private Label indexLabel;

        private ETInput indexInput;

        public ETValueSelectInputContent() : base()
        {
            this.style.flexGrow = 0;
            this.style.flexShrink = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
            indexInput = this.AddInput(InputOutputType.INT);
            indexInput.onConnectionCreated += HideIndexField;
            indexInput.onConnectionDeleted += ShowIndexField;
        }

        private void HideIndexField(int inputId, int outputId)
        {
            indexField.style.visibility = Visibility.Hidden;
        }

        private void ShowIndexField(int inputId, int outputId)
        {
            indexField.style.visibility = Visibility.Visible;
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.justifyContent = Justify.FlexStart;

            outputTypeField = new ETEnumField(ValueOutputType.STRING);
            outputTypeField.PublishUndoableActions = false;
            outputTypeField.AddToClassList("logic-enum-field");
            contentContainer.Add(outputTypeField);

            indexField = new ETTextField(ETTextField.ValidationType.INT, "0");
            indexField.AddToClassList("logic-text-input");
            contentContainer.Add(indexField);

            indexLabel = new Label("Choice Index");
            indexLabel.AddToClassList("logic-label");
            indexLabel.style.display = DisplayStyle.None;
            indexLabel.style.marginTop = 8.0f;
            contentContainer.Add(indexLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if (inputId == indexInput.ID)
            {
                indexField.style.display = DisplayStyle.None;
                indexLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            base.OnConnectionDeleted(inputId, outputId);

            if (inputId == indexInput.ID)
            {
                indexField.style.display = DisplayStyle.Flex;
                indexLabel.style.display = DisplayStyle.None;
            }
        }

        public ETEnumField OutputTypeField { get { return outputTypeField; } }

        public string Index
        {
            get { return indexField.text; }
            set { indexField.value = value; }
        }
    }

    public class ETValueContent : ETNodeContent
    {
        private ETInput input;

        private ETTextField valueField;

        private ETNodeButton removeItemButton;

        private Label variableLabel;

        public ETValueContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM) 
        {
            this.style.flexGrow = 0;
            this.style.flexShrink = 0;
            this.style.paddingRight = 0;
            this.style.marginLeft = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            input = this.AddInput(InputOutputType.STRING);

            inputPanel.style.justifyContent = Justify.Center;
            inputPanel.style.alignSelf = Align.Center;
            inputPanel.style.paddingLeft = 0;
            inputPanel.style.marginLeft = 2;
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexGrow = 1;
            this.style.marginLeft = 8.0f;
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.paddingLeft = 0;
            contentContainer.style.marginLeft = 0;
            contentContainer.style.marginRight = 8;
            contentContainer.style.justifyContent = Justify.FlexStart;

            removeItemButton = new ETNodeButton();
            removeItemButton.name = "random-node-remove-item-button";
            removeItemButton.AddToClassList("remove-item-button");
            removeItemButton.style.alignSelf = Align.Center;
            removeItemButton.style.marginLeft = 0;
            removeItemButton.style.marginRight = 0;
            removeItemButton.style.left = 0;
            contentContainer.Add(removeItemButton);

            valueField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Value...");
            valueField.AddToClassList("logic-text-input");
            valueField.style.flexGrow = 1;
            valueField.style.alignSelf = Align.Center;
            valueField.style.marginRight = 15;
            contentContainer.Add(valueField);

            variableLabel = new Label("Value");
            variableLabel.style.alignSelf = Align.Center;
            variableLabel.AddToClassList("logic-label");
            variableLabel.style.display = DisplayStyle.None;
            contentContainer.Add(variableLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if(inputId == input.ID)
            {
                valueField.style.display = DisplayStyle.None;
                variableLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            base.OnConnectionDeleted(inputId, outputId);

            if (inputId == input.ID)
            {
                valueField.style.display = DisplayStyle.Flex;
                variableLabel.style.display = DisplayStyle.None;
            }
        }

        public ETNodeButton RemoveButton { get { return removeItemButton; } }

        public ETTextField ValueField { get { return valueField; } }

        public Label VariableLabel
        {
            get { return variableLabel; }
        }
    }

    public class ETValueSelectOutputContent : ETNodeContent
    {
        private ETOutput valueOutput;

        private ETOutput flowOutput;

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            valueOutput = this.AddOutput(InputOutputType.STRING);
            flowOutput = this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public ETOutput ValueOutput { get { return valueOutput; } }

        public ETOutput FlowOutput { get { return flowOutput; } }
    }
}
