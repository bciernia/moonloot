using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETMathNode : ETNode
    {
        private ETMathInputContent inputContent;

        private ETMathVariableContent variableContent1;

        private ETMathVariableContent variableContent2;

        private ETMathOutputContent outputContent;

        public ETMathNode() : base("MATH", "math-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Math Node";
            this.connectionLine.AddToClassList("math-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(166, 140);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETMathInputContent();
            AddContent(inputContent);

            variableContent1 = new ETMathVariableContent();
            variableContent1.VariableLabel.text = "Variable 1";
            AddContent(variableContent1);

            variableContent2 = new ETMathVariableContent();
            variableContent2.VariableLabel.text = "Variable 2";
            AddContent(variableContent2);

            outputContent = new ETMathOutputContent();
            AddContent(outputContent);
        }

        public override Node CreateNode()
        {
            MathNode mathNode = new MathNode();
            mathNode.ID = id;

            CreateInputsForNode(mathNode);
            CreateOutputsForNode(mathNode);
            SetSizeForNode(mathNode);
            SetPositionForNode(mathNode);

            mathNode.MathOperation = inputContent.Operation;

            if (!variableContent1.HasIncomingConnection())
            {
                mathNode.ValueA = variableContent1.VariableValue;
            }
            else
            {
                mathNode.ValueA = "";
            }

            if (!variableContent2.HasIncomingConnection())
            {
                mathNode.ValueB = variableContent2.VariableValue;
            }
            else
            {
                mathNode.ValueB = "";
            }

            return mathNode;
        }

        public override void InitializeFromNode(Node node)
        {
            MathNode mathNode = node as MathNode;
            this.id = mathNode.ID;
            SetSizeFromNode(mathNode);
            SetPositionFromNode(mathNode);

            inputContent.Operation = mathNode.MathOperation;
            variableContent1.VariableValue = mathNode.ValueA;
            variableContent2.VariableValue = mathNode.ValueB;

            InitializeAllInputsAndOutputsFromNode(mathNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (inputContent.Operation.ToString().ToLower().Contains(text.ToLower())) { return true; }
            if (variableContent1.VariableValue.ToLower().Contains(text.ToLower())) { return true; }
            if (variableContent2.VariableValue.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Perform basic math functions and send the result to another node via the value output.";
        }
    }

    public class ETMathInputContent : ETNodeContent
    {
        private ETEnumField operationField;

        public ETMathInputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        public MathOperation Operation
        {
            get { return (MathOperation)operationField.value; }
            set { operationField.value = value; }
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
            contentContainer.style.flexGrow = 0;

            operationField = new ETEnumField(MathOperation.ADD);
            operationField.name = "math-operation-type-field";
            operationField.AddToClassList("logic-enum-field");
            operationField.RegisterCallback<ChangeEvent<Enum>>(OnOperationChanged);
            contentContainer.Add(operationField);
        }

        private void OnOperationChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }
    }

    public class ETMathVariableContent : ETNodeContent
    {
        private ETTextField numberField;

        private ETInput variableInput;

        private Label variableLabel;

        public ETMathVariableContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER, false)
        {
            Layout();
        }

        public string VariableValue
        {
            get { return numberField.text; }
            set { numberField.value = value; }
        }

        public bool HasIncomingConnection()
        {
            if (variableInput.ConnectedOutputs.Count > 0)
            {
                return true;
            }

            return false;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            variableInput = AddInput(InputOutputType.NUMBER);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            this.style.flexGrow = 0;

            contentContainer.style.flexDirection = FlexDirection.Column;
            contentContainer.style.paddingRight = 24.0f;
            contentPanel.style.flexGrow = 0;

            numberField = new ETTextField(ETTextField.ValidationType.NUMBER, "Enter Number...");
            numberField.AddToClassList("logic-text-input");
            contentContainer.Add(numberField);

            variableLabel = new Label("Variable");
            variableLabel.AddToClassList("logic-label");
            variableLabel.style.display = DisplayStyle.None;
            contentContainer.Add(variableLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            if (inputId == variableInput.ID)
            {
                numberField.style.display = DisplayStyle.None;
                variableLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            if (inputId == variableInput.ID)
            {
                numberField.style.display = DisplayStyle.Flex;
                variableLabel.style.display = DisplayStyle.None;
            }
        }

        public Label VariableLabel
        {
            get { return variableLabel; }
        }
    }

    public class ETMathOutputContent : ETNodeContent
    {
        public ETMathOutputContent() : base() { }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            AddOutput(InputOutputType.NUMBER);
            AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }
}