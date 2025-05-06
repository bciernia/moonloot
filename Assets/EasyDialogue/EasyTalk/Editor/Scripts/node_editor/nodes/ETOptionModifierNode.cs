using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETOptionModifierNode : ETNode
    {
        private ETOptionTextModifierNodeContent textContent;

        private ETOptionDisplayedModifierNodeContent displayContent;

        private ETOptionSelectableModifierNodeContent selectableContent;

        private ETOptionModifierOutputNodeContent outputContent;

        public ETOptionModifierNode() : base("OPTION MOD", "option-modifier-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Option Modifier Node";
            this.connectionLine.AddToClassList("option-modifier-line");
            this.resizeType = ResizeType.NONE;
            this.SetDimensions(150, 145);
        }

        public override void CreateContent()
        {
            textContent = new ETOptionTextModifierNodeContent();
            AddContent(textContent);

            displayContent = new ETOptionDisplayedModifierNodeContent();
            AddContent(displayContent);

            selectableContent = new ETOptionSelectableModifierNodeContent();
            AddContent(selectableContent);

            outputContent = new ETOptionModifierOutputNodeContent();
            AddContent(outputContent);

            textContent.TextInput.onConnectionCreated += OnTextConnectionCreated;
            textContent.TextInput.onConnectionDeleted += OnTextConnectionDeleted;
        }

        public bool HasTextOverride()
        {
            return textContent.HasTextInput();
        }

        private void OnTextConnectionCreated(int inputId, int outputId)
        {
            List<int> connectedOptionInputs = outputContent.ModifierOutput.ConnectedInputs;
            for (int i = 0; i < connectedOptionInputs.Count; i++)
            {
                ETOptionNode optionNode = EasyTalkNodeEditor.Instance.NodeView.GetNodeForInput(connectedOptionInputs[i]) as ETOptionNode;
                optionNode.HideOptionText(connectedOptionInputs[i]);
            }
        }

        private void OnTextConnectionDeleted(int inputId, int outputId)
        {
            List<int> connectedOptionInputs = outputContent.ModifierOutput.ConnectedInputs;
            for (int i = 0; i < connectedOptionInputs.Count; i++)
            {
                ETOptionNode optionNode = EasyTalkNodeEditor.Instance.NodeView.GetNodeForInput(connectedOptionInputs[i]) as ETOptionNode;
                optionNode.ShowOptionText(connectedOptionInputs[i]);
            }
        }

        public override Node CreateNode()
        {
            OptionModifierNode optionFilterNode = new OptionModifierNode();
            optionFilterNode.ID = id;

            CreateInputsForNode(optionFilterNode);
            CreateOutputsForNode(optionFilterNode);
            SetSizeForNode(optionFilterNode);
            SetPositionForNode(optionFilterNode);

            optionFilterNode.IsDisplayed = displayContent.IsDisplayedChecked;
            optionFilterNode.IsSelectable = selectableContent.IsSelectableChecked;

            return optionFilterNode;
        }

        public override void InitializeFromNode(Node node)
        {
            OptionModifierNode optionModifierNode = node as OptionModifierNode;
            this.id = optionModifierNode.ID;
            SetSizeFromNode(optionModifierNode);
            SetPositionFromNode(optionModifierNode);

            displayContent.IsDisplayedChecked = optionModifierNode.IsDisplayed;
            selectableContent.IsSelectableChecked = optionModifierNode.IsSelectable;

            InitializeAllInputsAndOutputsFromNode(optionModifierNode);
        }

        protected override string GetNodeTooltip()
        {
            return "Modifies dialogue options (on Option nodes) to change whether they are hidden/visible, enabled/disabled, or to change the text.";
        }
    }

    public class ETOptionModifierOutputNodeContent : ETNodeContent
    {
        private ETOutput modifierOutput;

        public ETOptionModifierOutputNodeContent() : base(NodeAlignment.CENTER, NodeAlignment.BOTTOM) { }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            modifierOutput = this.AddOutput(InputOutputType.OPTION_FILTER);
        }

        public ETOutput ModifierOutput { get { return modifierOutput; } }
    }

    public class ETOptionSelectableModifierNodeContent : ETNodeContent
    {
        private ETToggle selectableToggle;

        public ETOptionSelectableModifierNodeContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) { }

        public bool IsSelectableChecked
        {
            get { return selectableToggle.value; }
            set { selectableToggle.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.BOOL);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.contentPanel.style.flexDirection = FlexDirection.Row;
            this.contentPanel.style.alignSelf = Align.Center;
            this.style.flexGrow = 0;

            selectableToggle = new ETToggle();
            selectableToggle.AddToClassList("option-modifier-selectable-toggle");
            selectableToggle.value = true;
            selectableToggle.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
            contentContainer.Add(selectableToggle);

            Label selectableLabel = new Label("Selectable");
            selectableLabel.AddToClassList("option-modifier-selectable-label");
            contentContainer.Add(selectableLabel);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            selectableToggle.style.display = DisplayStyle.None;
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            selectableToggle.style.display = DisplayStyle.Flex;
        }
    }

    public class ETOptionTextModifierNodeContent : ETNodeContent
    {
        private ETInput textInput;

        public ETOptionTextModifierNodeContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) { }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            textInput = this.AddInput(InputOutputType.STRING);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexGrow = 0;

            Label textLabel = new Label("Text");
            textLabel.AddToClassList("option-modifier-text-label");
            contentContainer.Add(textLabel);
        }

        public bool HasTextInput()
        {
            return textInput.ConnectedOutputs.Count > 0;
        }

        public ETInput TextInput { get { return textInput; } }
    }

    public class ETOptionDisplayedModifierNodeContent : ETNodeContent
    {
        private ETToggle displayedToggle;

        public ETOptionDisplayedModifierNodeContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) { }

        public bool IsDisplayedChecked
        {
            get { return displayedToggle.value; }
            set { displayedToggle.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.BOOL);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.contentPanel.style.flexDirection = FlexDirection.Row;
            this.contentPanel.style.alignSelf = Align.Center;
            this.style.flexGrow = 0;

            displayedToggle = new ETToggle();
            displayedToggle.AddToClassList("option-modifier-displayed-toggle");
            displayedToggle.value = true;
            displayedToggle.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
            contentContainer.Add(displayedToggle);

            Label displayedLabel = new Label("Displayed");
            displayedLabel.AddToClassList("option-modifier-displayed-label");
            contentContainer.Add(displayedLabel);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            displayedToggle.style.display = DisplayStyle.None;
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            displayedToggle.style.display = DisplayStyle.Flex;
        }
    }
}