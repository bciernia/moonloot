using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETCompareStringsNode : ETNode
    {
        private ETCompareStringsInputContent inputContent;

        private ETCompareStringsVariableContent stringVariable1;

        private ETCompareStringsVariableContent stringVariable2;

        private ETCompareStringsOutputContent outputContent;

        public ETCompareStringsNode() : base("COMPARE STRINGS", "compare-strings-node")
        {
            //this.isResizable = false;
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Compare Strings Node";
            this.connectionLine.AddToClassList("compare-strings-node-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(230, 156);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETCompareStringsInputContent();
            AddContent(inputContent);

            stringVariable1 = new ETCompareStringsVariableContent();
            stringVariable1.StringLabel.text = "String 1";
            AddContent(stringVariable1);

            stringVariable2 = new ETCompareStringsVariableContent();
            stringVariable2.StringLabel.text = "String 2";
            AddContent(stringVariable2);

            outputContent = new ETCompareStringsOutputContent();
            AddContent(outputContent);
        }

        public override Node CreateNode()
        {
            CompareStringsNode compareStringsNode = new CompareStringsNode();
            compareStringsNode.ID = id;

            CreateInputsForNode(compareStringsNode);
            CreateOutputsForNode(compareStringsNode);
            SetSizeForNode(compareStringsNode);
            SetPositionForNode(compareStringsNode);

            compareStringsNode.ComparisonType = inputContent.ComparisonType;

            if (!stringVariable1.HasIncomingConnection())
            {
                compareStringsNode.ValueA = stringVariable1.StringValue;
            }
            else
            {
                compareStringsNode.ValueA = "";
            }

            if (!stringVariable2.HasIncomingConnection())
            {
                compareStringsNode.ValueB = stringVariable2.StringValue;
            }
            else
            {
                compareStringsNode.ValueB = "";
            }

            return compareStringsNode;
        }

        public override void InitializeFromNode(Node node)
        {
            CompareStringsNode compareStringsNode = node as CompareStringsNode;
            this.id = compareStringsNode.ID;
            SetSizeFromNode(compareStringsNode);
            SetPositionFromNode(compareStringsNode);

            inputContent.ComparisonType = compareStringsNode.ComparisonType;
            stringVariable1.StringValue = compareStringsNode.ValueA;
            stringVariable2.StringValue = compareStringsNode.ValueB;

            InitializeAllInputsAndOutputsFromNode(compareStringsNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (inputContent.ComparisonType.ToString().ToLower().Contains(text.ToLower())) { return true; }
            if (stringVariable1.StringValue.ToLower().Contains(text.ToLower())) { return true; }
            if (stringVariable2.StringValue.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            /*TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

            if (stringVariable1.GetInputs()[0].ConnectedOutputs.Count == 0 && stringVariable1.StringValue.ToString().Length > 0)
            {
                sourceSet.AddOrFindTranslation(stringVariable1.StringValue.ToString());
            }

            if (stringVariable2.GetInputs()[0].ConnectedOutputs.Count == 0 && stringVariable2.StringValue.ToString().Length > 0)
            {
                sourceSet.AddOrFindTranslation(stringVariable2.StringValue.ToString());
            }*/
        }

        protected override string GetNodeTooltip()
        {
            return "Performs a boolean comparison between two strings. If encountered via dialogue flow and the operation is true, dialogue will continue down the true (T) path; otherwise, dialogue will continue down the false (F) path. The result is also sent to the boolean output.";
        }
    }

    public class ETCompareStringsInputContent : ETNodeContent
    {
        private ETEnumField comparisonTypeField;

        public ETCompareStringsInputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        public StringComparisonType ComparisonType
        {
            get { return (StringComparisonType)comparisonTypeField.value; }
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

            comparisonTypeField = new ETEnumField(StringComparisonType.EQUAL);
            comparisonTypeField.AddToClassList("string-comparison-type-field");
            comparisonTypeField.AddToClassList("logic-enum-field");
            comparisonTypeField.RegisterCallback<ChangeEvent<Enum>>(OnComparisonTypeChanged);
            contentContainer.Add(comparisonTypeField);
        }

        private void OnComparisonTypeChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }
    }

    public class ETCompareStringsVariableContent : ETNodeContent
    {
        private ETInput input;

        private ETTextField stringField;

        private Label stringLabel;

        public ETCompareStringsVariableContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER, false)
        {
            this.style.flexGrow = 0;

            Layout();
        }

        public string StringValue
        {
            get { return stringField.text; }
            set { stringField.value = value; }
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
            input = AddInput(InputOutputType.STRING);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexDirection = FlexDirection.Column;
            this.style.paddingRight = 24.0f;

            stringField = new ETTextField(ETTextField.ValidationType.STRING, "Enter String...");
            stringField.AddToClassList("compare-strings-text-input");
            stringField.AddToClassList("logic-text-input");
            contentContainer.Add(stringField);

            stringLabel = new Label("String");
            stringLabel.AddToClassList("logic-label");
            stringLabel.style.display = DisplayStyle.None;
            contentContainer.Add(stringLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            stringField.style.display = DisplayStyle.None;
            stringLabel.style.display = DisplayStyle.Flex;
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            stringField.style.display = DisplayStyle.Flex;
            stringLabel.style.display = DisplayStyle.None;
        }

        public Label StringLabel
        {
            get { return stringLabel; }
        }
    }

    public class ETCompareStringsOutputContent : ETNodeContent
    {
        public ETCompareStringsOutputContent() : base() { }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            AddOutput(InputOutputType.BOOL);
            AddOutput(InputOutputType.DIALOGUE_TRUE_FLOW);
            AddOutput(InputOutputType.DIALOGUE_FALSE_FLOW);
        }
    }
}