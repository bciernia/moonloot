using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETCompareNumbersNode : ETNode
    {
        private ETCompareNumbersInputContent inputContent;

        private ETCompareNumbersVariableContent variableContent1;

        private ETCompareNumbersVariableContent variableContent2;

        private ETCompareNumbersOutputContent outputContent;

        public ETCompareNumbersNode() : base("COMPARE NUMBERS", "compare-numbers-node") { }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Compare Numbers Node";
            this.connectionLine.AddToClassList("compare-numbers-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(230, 154);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETCompareNumbersInputContent();
            AddContent(inputContent);

            variableContent1 = new ETCompareNumbersVariableContent();
            variableContent1.VariableLabel.text = "Variable 1";
            AddContent(variableContent1);

            variableContent2 = new ETCompareNumbersVariableContent();
            variableContent2.VariableLabel.text = "Variable 2";
            AddContent(variableContent2);

            outputContent = new ETCompareNumbersOutputContent();
            AddContent(outputContent);
        }

        public override Node CreateNode()
        {
            CompareNumbersNode compareNumbersNode = new CompareNumbersNode();
            compareNumbersNode.ID = id;

            CreateInputsForNode(compareNumbersNode);
            CreateOutputsForNode(compareNumbersNode);
            SetSizeForNode(compareNumbersNode);
            SetPositionForNode(compareNumbersNode);

            compareNumbersNode.ComparisonType = inputContent.ComparisonType;

            if (!variableContent1.HasIncomingConnection())
            {
                compareNumbersNode.ValueA = variableContent1.NumberValue;
            }
            else
            {
                compareNumbersNode.ValueA = "";
            }

            if (!variableContent2.HasIncomingConnection())
            {
                compareNumbersNode.ValueB = variableContent2.NumberValue;
            }
            else
            {
                compareNumbersNode.ValueB = "";
            }

            return compareNumbersNode;
        }

        public override void InitializeFromNode(Node node)
        {
            CompareNumbersNode compareNumbersNode = node as CompareNumbersNode;
            this.id = compareNumbersNode.ID;
            SetSizeFromNode(compareNumbersNode);
            SetPositionFromNode(compareNumbersNode);

            inputContent.ComparisonType = compareNumbersNode.ComparisonType;
            variableContent1.NumberValue = compareNumbersNode.ValueA;
            variableContent2.NumberValue = compareNumbersNode.ValueB;

            InitializeAllInputsAndOutputsFromNode(compareNumbersNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (inputContent.ComparisonType.ToString().ToLower().Contains(text.ToLower())) { return true; }
            if (variableContent1.NumberValue.ToLower().Contains(text.ToLower())) { return true; }
            if (variableContent2.NumberValue.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Performs a boolean comparison between two numbers. If encountered via dialogue flow and the operation is true, dialogue will continue down the true (T) path; otherwise, dialogue will continue down the false (F) path. The result is also sent to the boolean output.";
        }
    }

    public class ETCompareNumbersInputContent : ETNodeContent
    {
        private ETEnumField comparisonTypeField;

        public ETCompareNumbersInputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        public NumberComparisonType ComparisonType
        {
            get { return (NumberComparisonType)comparisonTypeField.value; }
            set { comparisonTypeField.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            contentContainer.style.flexDirection = FlexDirection.Column;

            comparisonTypeField = new ETEnumField(NumberComparisonType.GREATER_THAN);
            comparisonTypeField.AddToClassList("number-comparison-type-field");
            comparisonTypeField.AddToClassList("logic-enum-field");
            comparisonTypeField.RegisterCallback<ChangeEvent<Enum>>(OnComparisonTypeChanged);
            contentContainer.Add(comparisonTypeField);
        }

        private void OnComparisonTypeChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }
    }

    public class ETCompareNumbersVariableContent : ETNodeContent
    {
        private ETInput input;

        private ETTextField numberField;

        private Label variableLabel;

        public ETCompareNumbersVariableContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER, false)
        {
            this.style.flexGrow = 0;

            Layout();
        }

        public string NumberValue
        {
            get { return numberField.text; }
            set { numberField.value = value; }
        }

        public bool HasIncomingConnection()
        {
            if (input.ConnectedOutputs.Count > 0)
            {
                return true;
            }

            return false;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();

            input = AddInput(InputOutputType.NUMBER);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexDirection = FlexDirection.Column;
            this.style.paddingRight = 24.0f;

            numberField = new ETTextField(ETTextField.ValidationType.NUMBER, "Enter Number...");
            numberField.AddToClassList("compare-numbers-text-input");
            numberField.AddToClassList("logic-text-input");
            numberField.RegisterCallback<InputEvent>(ValidateNumber);
            contentContainer.Add(numberField);

            variableLabel = new Label("Variable");
            variableLabel.AddToClassList("logic-label");
            variableLabel.style.display = DisplayStyle.None;
            contentContainer.Add(variableLabel);
        }

        private void ValidateNumber(InputEvent evt)
        {
            float value;

            if (!float.TryParse(evt.newData, out value))
            {
                (evt.target as TextField).value = evt.previousData;
            }
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            numberField.style.display = DisplayStyle.None;
            variableLabel.style.display = DisplayStyle.Flex;
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            numberField.style.display = DisplayStyle.Flex;
            variableLabel.style.display = DisplayStyle.None;
        }

        public Label VariableLabel
        {
            get { return variableLabel; }
        }
    }

    public class ETCompareNumbersOutputContent : ETNodeContent
    {
        public ETCompareNumbersOutputContent() : base() { }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            AddOutput(InputOutputType.BOOL);
            AddOutput(InputOutputType.DIALOGUE_TRUE_FLOW);
            AddOutput(InputOutputType.DIALOGUE_FALSE_FLOW);
        }
    }
}