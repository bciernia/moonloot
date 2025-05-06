using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Localization;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETOptionNode : ETListNode
    {
        public ETOptionNode() : base("OPTION", "option-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Option Node";
            this.connectionLine.AddToClassList("option-line");
            this.SetDimensions(228, 136);
        }

        public override float GetMinimumHeight()
        {
            return 78.0f + (ListPanel.Items.Count * 30.0f);
        }

        public override void CreateContent()
        {
            ETOptionNodeContent nodeInput = new ETOptionNodeContent();
            AddContent(nodeInput);

            CreateListPanel();
            AddItemToList();
            AddItemToList();

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETOptionNodeItemContent item = new ETOptionNodeItemContent();
            item.OptionLabel.text = "Option " + (Items.Count + 1);
            item.AddToClassList("list-node-item");
            item.RemoveButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(item);
                }
            };
            return item;
        }

        private void OnItemAdded(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() + 38;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() - 22.0f;

            for (int i = 0; i < Items.Count; i++)
            {
                ETOptionNodeItemContent content = (ETOptionNodeItemContent)Items[i];
                content.OptionLabel.text = "Option " + (i + 1);
            }
        }

        public void HideOptionText(int optionInputId)
        {
            bool found = false;
            foreach (ETOptionNodeItemContent item in listPanel.Items)
            {
                foreach (ETInput input in item.GetInputs())
                {
                    if (input.ID == optionInputId)
                    {
                        item.HideTextInput();
                        found = true;
                        break;
                    }
                }

                if (found) { break; }
            }
        }

        public void ShowOptionText(int optionInputId)
        {
            bool found = false;
            foreach (ETOptionNodeItemContent item in listPanel.Items)
            {
                foreach (ETInput input in item.GetInputs())
                {
                    if (input.ID == optionInputId)
                    {
                        item.ShowTextInput();
                        found = true;
                        break;
                    }
                }

                if (found) { break; }
            }
        }

        public override Node CreateNode()
        {
            OptionNode optionNode = new OptionNode();
            optionNode.ID = id;

            CreateInputsForNode(optionNode);
            CreateOutputsForNode(optionNode);
            SetSizeForNode(optionNode);
            SetPositionForNode(optionNode);

            foreach (ETOptionNodeItemContent item in this.Items)
            {
                optionNode.AddItem(new OptionItem(item.OptionText));
            }

            return optionNode;
        }

        public override void InitializeFromNode(Node node)
        {
            OptionNode optionNode = node as OptionNode;
            this.id = optionNode.ID;
            SetPositionFromNode(optionNode);

            while (Items.Count > optionNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < optionNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(optionNode);

            for (int i = 0; i < Items.Count; i++)
            {
                ((ETOptionNodeItemContent)Items[i]).OptionText = ((OptionItem)(optionNode.Items[i])).text;
            }

            InitializeAllInputsAndOutputsFromNode(optionNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            foreach (ETOptionNodeItemContent item in Items)
            {
                if (item.OptionText.ToLower().Contains(text.ToLower())) { return true; }
            }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

            foreach (ETOptionNodeItemContent item in Items)
            {
                if (item.OptionText.ToString().Length > 0)
                {
                    sourceSet.AddOrFindTranslation(item.OptionText.ToString());
                }
            }
        }

        protected override string GetNodeTooltip()
        {
            return "Creates options to be presented to the user. Options can be hidden, disabled, or have their text changed dynamically by using Option Modifier nodes.";
        }
    }

    public class ETOptionNodeContent : ETNodeContent
    {
        public ETOptionNodeContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM)
        {
            this.style.flexShrink = 0;
            this.style.flexGrow = 0;
            this.style.paddingBottom = 0.0f;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();

            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETOptionNodeItemContent : ETNodeContent
    {
        private ETNodeButton removeButton;

        private ETTextField optionField;

        private ETInput modifierInput;

        private Label optionLabel;

        public ETOptionNodeItemContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER)
        {
            this.style.flexShrink = 0;
        }

        public ETNodeButton RemoveButton
        {
            get { return removeButton; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            modifierInput = this.AddInput(InputOutputType.OPTION_FILTER);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.alignItems = Align.Center;
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.justifyContent = Justify.FlexStart;

            removeButton = new ETNodeButton();
            removeButton.AddToClassList("remove-item-button");
            removeButton.style.alignSelf = Align.Center;
            contentContainer.Add(removeButton);

            optionField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Option Text...");
            optionField.AddToClassList("option-field");
            optionField.AddToClassList("node-text-area");
            optionField.multiline = true;

#if UNITY_2023_1_OR_NEWER

            optionField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            optionField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif

            optionField.style.alignSelf = Align.Stretch;
            contentContainer.Add(optionField);

            optionLabel = new Label("Option");
            optionLabel.AddToClassList("option-label");
            optionLabel.style.display = DisplayStyle.None;
            contentContainer.Add(optionLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            //Check option modifer to see if it has a value connected to the text input.
            //If it does, disable the text field.
            if (inputId == modifierInput.ID)
            {
                ETOptionModifierNode modifierNode = EasyTalkNodeEditor.Instance.NodeView.GetNodeForOutput(outputId) as ETOptionModifierNode;
                if (modifierNode != null && modifierNode.HasTextOverride())
                {
                    HideTextInput();
                }
            }
        }

        public void HideTextInput()
        {
            optionLabel.style.display = DisplayStyle.Flex;
            optionField.style.display = DisplayStyle.None;
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            if (inputId == modifierInput.ID)
            {
                ShowTextInput();
            }
        }

        public void ShowTextInput()
        {
            optionLabel.style.display = DisplayStyle.None;
            optionField.style.display = DisplayStyle.Flex;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public string OptionText
        {
            get { return optionField.text; }
            set { optionField.value = value; }
        }

        public Label OptionLabel
        {
            get { return optionLabel; }
        }
    }
}

