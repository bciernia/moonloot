using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETConditionalValueNode : ETNode
    {
        private ETConditionalValueInputContent inputContent;

        private ETConditionalValueVariableContent trueContent;

        private ETConditionalValueVariableContent falseContent;

        private ETConditionValueOutputContent outputContent;

        private InputOutputType ioType = InputOutputType.STRING;

        private ETTextField.ValidationType validationType = ETTextField.ValidationType.STRING;

        private ConditionalValueNode storedNodeState;

        public ETConditionalValueNode() : base("CONDITIONAL VALUE", "conditional-value-node") { }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Conditional Value Node";
            this.connectionLine.AddToClassList("conditional-value-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(220, 220);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETConditionalValueInputContent();
            inputContent.ValueTypeField.RegisterCallback<ChangeEvent<Enum>>(OnValueTypeChanged);

            AddContent(inputContent);

            trueContent = new ETConditionalValueVariableContent();
            trueContent.ValueField.SetPlaceholderText("Enter True Value...");
            trueContent.VariableLabel.text = "True Value";
            AddContent(trueContent);

            falseContent = new ETConditionalValueVariableContent();
            falseContent.ValueField.SetPlaceholderText("Enter False Value...");
            falseContent.VariableLabel.text = "False Value";
            AddContent(falseContent);

            outputContent = new ETConditionValueOutputContent();
            AddContent(outputContent);

            AddNodeChangeCallbacks();
        }

        private void AddNodeChangeCallbacks()
        {
            inputContent.BoolDropdown.RegisterCallback<ChangeEvent<Enum>>(BoolValueChanged);

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

        protected void BoolValueChanged(ChangeEvent<Enum> evt)
        {
            UpdateStoredNodeState();
        }

        public void UpdateStoredNodeState()
        {
            storedNodeState = CreateNode() as ConditionalValueNode;
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

        private void OnValueTypeChanged(ChangeEvent<Enum> evt)
        {
            if (storedNodeState != null)
            {
                EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(this.ID, storedNodeState, "Conditional Node Changed Value Type");
            }

            ValueOutputType outputType = (ValueOutputType)inputContent.ValueTypeField.value;
            switch (outputType)
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

            trueContent.ValueInput.SetInputType(ioType);
            trueContent.ValueField.SetValidationType(validationType);
            falseContent.ValueInput.SetInputType(ioType);
            falseContent.ValueField.SetValidationType(validationType);

            outputContent.ValueOutput.SetOutputType(ioType);

            if (trueContent.ValueInput.ConnectedOutputs.Count > 0)
            {
                ETOutput output = EasyTalkNodeEditor.Instance.NodeView.GetOutput(trueContent.ValueInput.ConnectedOutputs[0]);
                if (output != null && !ETUtils.IsConnectionAllowed(trueContent.ValueInput.InputType, output.OutputType))
                {
                    trueContent.ValueInput.DisconnectAll();
                }
            }

            if (falseContent.ValueInput.ConnectedOutputs.Count > 0)
            {
                ETOutput output = EasyTalkNodeEditor.Instance.NodeView.GetOutput(falseContent.ValueInput.ConnectedOutputs[0]);
                if (output != null && !ETUtils.IsConnectionAllowed(falseContent.ValueInput.InputType, output.OutputType))
                {
                    falseContent.ValueInput.DisconnectAll();
                }
            }

            if (outputContent.ValueOutput.ConnectedInputs.Count > 0)
            {
                ETInput input = EasyTalkNodeEditor.Instance.NodeView.GetInput(outputContent.ValueOutput.ConnectedInputs[0]);
                if (input != null && !ETUtils.IsConnectionAllowed(input.InputType, outputContent.ValueOutput.OutputType))
                {
                    outputContent.ValueOutput.DisconnectAll();
                }
            }

            EasyTalkNodeEditor.Instance.NodeView.MarkDirtyRepaint();

            EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Conditional Node Changed Value Type");
            UpdateStoredNodeState();
        }

        public override Node CreateNode()
        {
            ConditionalValueNode valueNode = new ConditionalValueNode();
            valueNode.ID = id;
            valueNode.TrueValue = trueContent.Value;
            valueNode.FalseValue = falseContent.Value;
            valueNode.ValueOutputType = inputContent.ValueTypeField.value.ToString();

            BoolOption boolOption = (BoolOption)inputContent.BoolDropdown.value;
            if(boolOption == BoolOption.TRUE)
            {
                valueNode.BoolValue = true;
            }
            else
            {
                valueNode.BoolValue = false;
            }

            CreateInputsForNode(valueNode);
            CreateOutputsForNode(valueNode);
            SetSizeForNode(valueNode);
            SetPositionForNode(valueNode);

            return valueNode;
        }

        public override void InitializeFromNode(Node node)
        {
            ConditionalValueNode valueNode = node as ConditionalValueNode;
            this.id = valueNode.ID;
            trueContent.Value = valueNode.TrueValue;
            falseContent.Value = valueNode.FalseValue;

            if(valueNode.BoolValue)
            {
                inputContent.BoolDropdown.value = BoolOption.TRUE;
            }
            else
            {
                inputContent.BoolDropdown.value = BoolOption.FALSE;
            }

            trueContent.ValueInput.SetInputType(valueNode.Inputs[2].ConnectionType);
            falseContent.ValueInput.SetInputType(valueNode.Inputs[3].ConnectionType);

            SetPositionFromNode(valueNode);
            SetSizeFromNode(valueNode);

            inputContent.ValueTypeField.value = Enum.Parse<ValueOutputType>(valueNode.ValueOutputType);
            InitializeAllInputsAndOutputsFromNode(valueNode);

            UpdateStoredNodeState();
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text)) { return true; }

            if(trueContent.Value.ToLower().Contains(text.ToLower())) { return true; }
            if(falseContent.Value.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            /*if (((ValueOutputType)inputContent.ValueTypeField.value) == ValueOutputType.STRING)
            {
                TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

                if (trueContent.GetInputs()[0].ConnectedOutputs.Count == 0 && trueContent.Value.ToString().Length > 0)
                {
                    sourceSet.AddOrFindTranslation(trueContent.Value.ToString());
                }

                if (falseContent.GetInputs()[0].ConnectedOutputs.Count == 0 && falseContent.Value.ToString().Length > 0)
                {
                    sourceSet.AddOrFindTranslation(falseContent.Value.ToString());
                }
            }*/
        }

        protected override string GetNodeTooltip()
        {
            return "Chooses a value to output based on a true/false condition.";
        }
    }

    public class ETConditionalValueInputContent : ETNodeContent
    {
        private ETEnumField valueTypeField;

        private ETEnumField boolDropdown;

        private ETInput flowInput;

        private ETInput boolInput;

        private Label boolConditionLabel;

        public ETConditionalValueInputContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER)
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            flowInput = AddInput(InputOutputType.DIALGOUE_FLOW);
            boolInput = AddInput(InputOutputType.BOOL);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.justifyContent = Justify.FlexStart;
            contentContainer.style.marginTop = 4.0f;

            valueTypeField = new ETEnumField(ValueOutputType.STRING);
            valueTypeField.PublishUndoableActions = false;
            valueTypeField.AddToClassList("logic-enum-field");
            contentContainer.Add(valueTypeField);

            boolDropdown = new ETEnumField(BoolOption.TRUE);
            boolDropdown.AddToClassList("logic-enum-field");
            contentContainer.Add(boolDropdown);

            boolConditionLabel = new Label("Bool Condition Value");
            boolConditionLabel.AddToClassList("logic-label");
            boolConditionLabel.style.display = DisplayStyle.None;
            boolConditionLabel.style.marginTop = 6.0f;
            contentContainer.Add(boolConditionLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if(inputId == boolInput.ID)
            {
                boolDropdown.style.display = DisplayStyle.None;
                boolConditionLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            base.OnConnectionDeleted(inputId, outputId);

            if (inputId == boolInput.ID)
            {
                boolDropdown.style.display = DisplayStyle.Flex;
                boolConditionLabel.style.display = DisplayStyle.None;
            }
        }

        public ETInput FlowInput
        {
            get { return flowInput; }
        }

        public ETInput BoolInput
        {
            get { return boolInput; }
        }

        public ETEnumField ValueTypeField
        {
            get { return valueTypeField; }
        }

        public ETEnumField BoolDropdown
        {
            get { return boolDropdown; }
        }
    }

    public class ETConditionalValueVariableContent : ETNodeContent
    {
        private ETTextField valueField;

        private ETInput valueInput;

        private Label variableLabel;

        private string placeholderText = "Enter Value...";

        public ETConditionalValueVariableContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) 
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            valueInput = AddInput(InputOutputType.STRING);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            valueField = new ETTextField(ETTextField.ValidationType.STRING, placeholderText);
            valueField.AddToClassList("logic-text-input");
            contentContainer.Add(valueField);

            variableLabel = new Label("Variable");
            variableLabel.AddToClassList("logic-label");
            variableLabel.style.display = DisplayStyle.None;
            contentContainer.Add(variableLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if(inputId == valueInput.ID)
            {
                valueField.style.display = DisplayStyle.None;
                variableLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            base.OnConnectionDeleted(inputId, outputId);

            if (inputId == valueInput.ID)
            {
                valueField.style.display = DisplayStyle.Flex;
                variableLabel.style.display = DisplayStyle.None;
            }
        }

        public ETInput ValueInput
        {
            get { return valueInput; }
        }

        public ETTextField ValueField
        {
            get { return valueField; }
        }

        public string Value
        {
            get { return valueField.text; }
            set { valueField.value = value; }
        }

        public Label VariableLabel
        {
            get { return variableLabel; }
        }
    }

    public class ETConditionValueOutputContent : ETNodeContent
    {
        private ETOutput valueOutput;

        private ETOutput flowOutput;

        public ETConditionValueOutputContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM)
        {
            this.style.flexGrow = 1;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            valueOutput = AddOutput(InputOutputType.STRING);
            flowOutput = AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public ETOutput ValueOutput
        {
            get { return valueOutput; }
        }

        public ETOutput FlowOutput
        {
            get { return flowOutput; }
        }
    }
}
