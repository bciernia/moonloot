using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETRandomChoiceNode : ETListNode
    {
        public ETRandomChoiceNode() : base("RANDOM", "random-choice-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Random Choice Node";
            this.connectionLine.AddToClassList("random-choice-line");
            this.SetDimensions(160, 130);
        }

        public override float GetMinimumHeight()
        {
            return 78.0f + (ListPanel.Items.Count * 25.0f);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            ETRandomChoiceInputContent content = new ETRandomChoiceInputContent();
            this.AddContent(content);

            CreateListPanel();
            listPanel.style.flexGrow = 0;
            AddItemToList();
            AddItemToList();

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETRandomChoiceItemContent item = new ETRandomChoiceItemContent();
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
            this.style.height = GetHeight() + 32;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() - 25.0f;
        }

        public override Node CreateNode()
        {
            RandomNode randomNode = new RandomNode();
            randomNode.ID = id;

            CreateInputsForNode(randomNode);
            CreateOutputsForNode(randomNode);
            SetSizeForNode(randomNode);
            SetPositionForNode(randomNode);

            for (int i = 0; i < Items.Count; i++)
            {
                randomNode.AddItem(new RandomItem());
            }

            return randomNode;
        }

        public override void InitializeFromNode(Node node)
        {
            RandomNode randomNode = node as RandomNode;
            this.id = randomNode.ID;
            SetPositionFromNode(randomNode);

            while (Items.Count > randomNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < randomNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(randomNode);

            InitializeAllInputsAndOutputsFromNode(randomNode);
        }

        protected override string GetNodeTooltip()
        {
            return "This node will choose a random path each time it is encountered.";
        }
    }

    public class ETRandomChoiceInputContent : ETNodeContent
    {
        public ETRandomChoiceInputContent() : base()
        {
            this.style.flexGrow = 1;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETRandomChoiceItemContent : ETNodeContent
    {
        private ETNodeButton removeItemButton;

        public ETRandomChoiceItemContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) { }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexGrow = 0;

            removeItemButton = new ETNodeButton();
            removeItemButton.name = "random-node-remove-item-button";
            removeItemButton.AddToClassList("remove-item-button");
            contentContainer.Add(removeItemButton);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public ETNodeButton RemoveButton { get { return removeItemButton; } }
    }
}