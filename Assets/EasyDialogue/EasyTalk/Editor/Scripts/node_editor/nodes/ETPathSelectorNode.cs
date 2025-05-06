using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETPathSelectorNode : ETListNode
    {
        private ETPathSelectInputContent inputContent;

        public ETPathSelectorNode() : base("PATH SELECT", "path-select-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Path Selector Node";
            this.connectionLine.AddToClassList("path-select-line");
            this.resizeType = ResizeType.BOTH;
            this.SetDimensions(186, 146);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETPathSelectInputContent();
            this.AddContent(inputContent);

            CreateListPanel();
            listPanel.style.flexGrow = 1;
            listPanel.ContentPanel.style.justifyContent = Justify.FlexEnd;
            AddItemToList();
            AddItemToList();

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;
        }

        private void OnItemAdded(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() + 32;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() - 25.0f;
        }

        public override Node CreateNode()
        {
            PathSelectorNode selectorNode = new PathSelectorNode();
            selectorNode.ID = id;
            selectorNode.Index = inputContent.Index;

            CreateInputsForNode(selectorNode);
            CreateOutputsForNode(selectorNode);
            SetSizeForNode(selectorNode);
            SetPositionForNode(selectorNode);

            for (int i = 0; i < Items.Count; i++)
            {
                selectorNode.AddItem(new PathSelectorListItem());
            }

            return selectorNode;
        }

        public override void InitializeFromNode(Node node)
        {
            PathSelectorNode selectorNode = node as PathSelectorNode;
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

            InitializeAllInputsAndOutputsFromNode(selectorNode);

            inputContent.Index = selectorNode.Index;
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETPathContent content = new ETPathContent();
            content.RemoveButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(content);

                    for (int i = 0; i < this.ListPanel.Items.Count; i++) 
                    {
                        ETPathContent pathContent = (ETPathContent)this.ListPanel.Items[i];
                        pathContent.SetIndex(i);
                    }
                }
            };

            content.SetIndex(this.ListPanel.Items.Count);
            return content;
        }

        public override float GetMinimumWidth()
        {
            return 186.0f;
        }

        public override float GetMinimumHeight()
        {
            return 96.0f + (ListPanel.Items.Count * 25.0f);
        }

        protected override string GetNodeTooltip()
        {
            return "Proceeds to the selected path (based on the index value). The first path has an index of 0.";
        }
    }

    public class ETPathSelectInputContent : ETNodeContent
    {
        private ETInput indexInput;

        private ETTextField indexField;

        private Label indexLabel;

        public ETPathSelectInputContent() : base()
        {
            this.style.flexGrow = 0;
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

            contentContainer.style.justifyContent = Justify.FlexEnd;
            indexField = new ETTextField(ETTextField.ValidationType.INT, "0");
            indexField.AddToClassList("flow-text-field");
            contentContainer.Add(indexField);

            indexLabel = new Label("Path Index");
            indexLabel.AddToClassList("flow-label");
            indexLabel.style.marginBottom = 6.0f;
            indexLabel.style.display = DisplayStyle.None;
            contentContainer.Add(indexLabel);
        }

        public string Index
        {
            get { return indexField.text; }
            set { indexField.value = value; }
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if(inputId == indexInput.ID)
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
    }

    public class ETPathContent : ETNodeContent
    {
        private ETNodeButton removeItemButton;

        private Label indexLabel;

        private int index = 0;

        public ETPathContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM) { }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexGrow = 0;

            removeItemButton = new ETNodeButton();
            removeItemButton.name = "random-node-remove-item-button";
            removeItemButton.AddToClassList("remove-item-button");
            removeItemButton.style.alignSelf = Align.Center;
            contentContainer.Add(removeItemButton);

            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.justifyContent = Justify.FlexEnd;
            indexLabel = new Label("" + index);
            indexLabel.style.alignSelf = Align.Center;
            indexLabel.AddToClassList("path-select-label");
            contentContainer.Add(indexLabel);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);

            outputPanel.style.justifyContent = Justify.Center;
            outputPanel.style.alignSelf = Align.Center;
        }

        public ETNodeButton RemoveButton { get { return removeItemButton; } }

        public void SetIndex(int index)
        {
            this.index = index;
            indexLabel.text = "" + index;
        }
    }
}
