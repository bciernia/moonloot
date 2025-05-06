using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETSequenceNode : ETListNode
    {
        public ETSequenceNode() : base("SEQUENCE", "sequence-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Sequence Node";
            this.connectionLine.AddToClassList("sequence-line");
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
            ListPanel.style.flexGrow = 0;
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

        private void OnItemAdded(ETNode node, ETNodeContent content)
        {
            this.style.height = GetHeight() + 32;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent content)
        {
            this.style.height = GetHeight() - 25.0f;
        }

        public override Node CreateNode()
        {
            SequenceNode sequenceNode = new SequenceNode();
            sequenceNode.ID = id;

            CreateInputsForNode(sequenceNode);
            CreateOutputsForNode(sequenceNode);
            SetSizeForNode(sequenceNode);
            SetPositionForNode(sequenceNode);

            for (int i = 0; i < Items.Count; i++)
            {
                sequenceNode.AddItem(new SequenceItem());
            }

            return sequenceNode;
        }

        public override void InitializeFromNode(Node node)
        {
            SequenceNode sequenceNode = node as SequenceNode;
            this.id = sequenceNode.ID;
            SetPositionFromNode(sequenceNode);

            while (Items.Count > sequenceNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < sequenceNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(sequenceNode);

            InitializeAllInputsAndOutputsFromNode(sequenceNode);
        }

        protected override string GetNodeTooltip()
        {
            return "Each time this node is encountered in the dialogue, it will choose the next path, in sequence, to proceed down.";
        }
    }

    public class ETSequenceInputContent : ETNodeContent
    {
        public ETSequenceInputContent() : base()
        {
            this.style.flexGrow = 1;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETSequenceItemContent : ETNodeContent
    {
        private ETNodeButton removeItemButton;

        public ETSequenceItemContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) { }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexGrow = 0;

            removeItemButton = new ETNodeButton();
            removeItemButton.name = "sequence-node-remove-item-button";
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