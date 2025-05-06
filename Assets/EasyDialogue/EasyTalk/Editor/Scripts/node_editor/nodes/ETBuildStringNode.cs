using EasyTalk.Editor.Components;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETBuildStringNode : ETListNode
    {
        public ETBuildStringNode() : base("BUILD STRING", "string-concat-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "String Concatenation Node";
            this.connectionLine.AddToClassList("string-concat-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(226, 190);
        }

        public override float GetMinimumHeight()
        {
            return 164.0f + (ListPanel.Items.Count * 28.0f);
        }

        public override void CreateContent()
        {
            ETStringConcatenationNodeInputContent inputContent = new ETStringConcatenationNodeInputContent();
            AddContent(inputContent);

            CreateListPanel();
            AddItemToList();
            AddItemToList();

            ETStringConcatenationNodeOutputContent outputContent = new ETStringConcatenationNodeOutputContent();
            AddContent(outputContent);

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETStringConcatenationNodeItemContent item = new ETStringConcatenationNodeItemContent();
            item.StringLabel.text = "String " + (Items.Count + 1);
            item.AddToClassList("string-concat-item");
            item.RemoveItemButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(item);
                }
            };
            return item;
        }

        public void OnItemAdded(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() + 32;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() - 28.0f;

            for(int i = 0; i < Items.Count; i++)
            {
                ETStringConcatenationNodeItemContent content = (ETStringConcatenationNodeItemContent)Items[i];
                content.StringLabel.text = "String " + (i + 1);
            }
        }

        public override Node CreateNode()
        {
            BuildStringNode buildStringNode = new BuildStringNode();
            buildStringNode.ID = id;

            CreateInputsForNode(buildStringNode);
            CreateOutputsForNode(buildStringNode);
            SetSizeForNode(buildStringNode);
            SetPositionForNode(buildStringNode);

            foreach (ETStringConcatenationNodeItemContent item in Items)
            {
                if (item.HasIncomingConnection())
                {
                    buildStringNode.AddItem(new StringItem(""));
                }
                else
                {
                    buildStringNode.AddItem(new StringItem(item.Value));
                }
            }

            return buildStringNode;
        }

        public override void InitializeFromNode(Node node)
        {
            BuildStringNode buildStringNode = node as BuildStringNode;
            this.id = buildStringNode.ID;
            SetPositionFromNode(buildStringNode);

            while (Items.Count > buildStringNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < buildStringNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(buildStringNode);

            for (int i = 0; i < Items.Count; i++)
            {
                ((ETStringConcatenationNodeItemContent)Items[i]).Value = ((StringItem)(buildStringNode.Items[i])).text;
            }

            InitializeAllInputsAndOutputsFromNode(buildStringNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            foreach (ETStringConcatenationNodeItemContent item in Items)
            {
                if (item.Value.ToString().ToLower().Contains(text.ToLower())) { return true; }
            }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            /*TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

            foreach (ETStringConcatenationNodeItemContent item in Items)
            {
                if(item.GetInputs()[0].ConnectedOutputs.Count == 0 && item.Value.ToString().Length > 0)
                {
                    sourceSet.AddOrFindTranslation(item.Value.ToString());
                }
            }*/
        }

        protected override string GetNodeTooltip()
        {
            return "Can be used to dynamically build a string by concatenating all of the input values. The result is sent to the string output.";
        }
    }

    public class ETStringConcatenationNodeInputContent : ETNodeContent
    {
        public ETStringConcatenationNodeInputContent() : base()
        {
            this.style.flexDirection = FlexDirection.Row;
            this.style.flexGrow = 0;
            this.style.flexShrink = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETStringConcatenationNodeOutputContent : ETNodeContent
    {
        public ETStringConcatenationNodeOutputContent() : base()
        {
            this.style.flexDirection = FlexDirection.Row;
            this.style.flexGrow = 1;
            this.style.flexShrink = 0;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.STRING);
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETStringConcatenationNodeItemContent : ETNodeContent
    {
        private ETNodeButton removeItemButton;

        private TextField valueField;

        private ETInput valueInput;

        private Label stringLabel;

        public ETStringConcatenationNodeItemContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER)
        {
            this.style.marginTop = 0.0f;
            this.style.marginBottom = 0.0f;
            this.style.flexGrow = 0;
        }

        public string Value
        {
            get { return valueField.text; }
            set { valueField.value = value; }
        }

        public bool HasIncomingConnection()
        {
            if (valueInput.ConnectedOutputs.Count > 0)
            {
                return true;
            }

            return false;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            valueInput = this.AddInput(InputOutputType.VALUE);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexDirection = FlexDirection.Row;
            this.style.marginBottom = 2.0f;
            this.style.marginTop = 2.0f;

            contentContainer.style.alignItems = Align.Center;
            contentContainer.style.paddingLeft = 0.0f;
            contentContainer.style.paddingRight = 0.0f;
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.justifyContent = Justify.FlexStart;

            removeItemButton = new ETNodeButton();
            removeItemButton.AddToClassList("remove-item-button");
            removeItemButton.style.alignSelf = Align.Center;
            contentContainer.Add(removeItemButton);

            valueField = new ETTextField(ETTextField.ValidationType.STRING, "Enter or Attach value...");
            valueField.AddToClassList("logic-text-input");
            valueField.multiline = false;
            valueField.style.minHeight = 24.0f;
            valueField.AddToClassList("node-text-area");
            valueField.style.alignSelf = Align.Stretch;
            contentContainer.Add(valueField);

            stringLabel = new Label("String Value");
            stringLabel.AddToClassList("logic-label");
            stringLabel.style.display = DisplayStyle.None;
            contentContainer.Add(stringLabel);
        }

        public ETNodeButton RemoveItemButton { get { return removeItemButton; } }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            if (inputId == valueInput.ID)
            {
                valueField.style.display = DisplayStyle.None;
                stringLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            if (inputId == valueInput.ID)
            {
                valueField.style.display = DisplayStyle.Flex;
                stringLabel.style.display = DisplayStyle.None;
            }
        }

        public Label StringLabel
        {
            get { return stringLabel; }
        }
    }
}