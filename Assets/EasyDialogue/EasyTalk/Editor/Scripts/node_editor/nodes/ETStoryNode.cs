using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETStoryNode : ETNode
    {
        private ETStoryNodeContent storyContent;

        public ETStoryNode() : base("STORY", "story-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Story Node";
            this.connectionLine.AddToClassList("story-line");
            this.resizeType = ResizeType.BOTH;
            this.SetDimensions(240, 120);
        }

        public override void CreateContent()
        {
            storyContent = new ETStoryNodeContent();
            AddContent(storyContent);
        }

        public override Node CreateNode()
        {
            StoryNode storyNode = new StoryNode();
            storyNode.ID = id;

            CreateInputsForNode(storyNode);
            CreateOutputsForNode(storyNode);
            SetSizeForNode(storyNode);
            SetPositionForNode(storyNode);

            storyNode.Summary = storyContent.Summary;

            return storyNode;
        }

        public override void InitializeFromNode(Node node)
        {
            StoryNode storyNode = node as StoryNode;
            this.id = storyNode.ID;
            SetSizeFromNode(storyNode);
            SetPositionFromNode(storyNode);

            storyContent.Summary = storyNode.Summary;

            InitializeAllInputsAndOutputsFromNode(storyNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (storyContent.Summary.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Useful for writing out branching storylines or implementing custom logic with Dialogue Listeners.";
        }
    }

    public class ETStoryNodeContent : ETNodeContent
    {
        private ETTextField storyField;

        public ETStoryNodeContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM) { }

        public string Summary
        {
            get { return storyField.text; }
            set { storyField.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            storyField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Story Text...");
            storyField.AddToClassList("story-text-area");
            storyField.AddToClassList("node-text-area");
            storyField.multiline = true;

#if UNITY_2023_1_OR_NEWER

            storyField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            storyField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif

            contentContainer.Add(storyField);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }
}