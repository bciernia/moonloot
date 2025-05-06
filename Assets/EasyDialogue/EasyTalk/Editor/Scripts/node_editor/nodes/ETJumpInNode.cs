using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETJumpInNode : ETNode
    {
        private ETJumpInNodeContent content;

        public ETJumpInNode() : base("JUMP", "jump-in-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Jump In Node";
            this.connectionLine.AddToClassList("jump-in-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(148, 75);
        }

        public override void CreateContent()
        {
            content = new ETJumpInNodeContent();
            AddContent(content);
        }

        public string Key { get { return content.Key; } }

        public override Node CreateNode()
        {
            JumpInNode jumpInNode = new JumpInNode();
            jumpInNode.ID = id;

            CreateOutputsForNode(jumpInNode);
            SetSizeForNode(jumpInNode);
            SetPositionForNode(jumpInNode);

            jumpInNode.Key = content.Key;

            return jumpInNode;
        }

        public override void InitializeFromNode(Node node)
        {
            JumpInNode jumpInNode = node as JumpInNode;
            this.id = jumpInNode.ID;
            SetSizeFromNode(jumpInNode);
            SetPositionFromNode(jumpInNode);

            content.Key = jumpInNode.Key;

            InitializeAllInputsAndOutputsFromNode(jumpInNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (content.Key.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Provides a \"jump\" point to jump to from a Jump-Out node.";
        }
    }

    public class ETJumpInNodeContent : ETNodeContent
    {
        private ETTextField keyField;

        public ETJumpInNodeContent() : base()
        {
            this.outputAlignment = NodeAlignment.CENTER;
        }

        public string Key
        {
            get { return this.keyField.text; }
            set { keyField.value = value; }
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            keyField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Jump Key");
            keyField.AddToClassList("jump-text-field");
            keyField.multiline = false;
            contentContainer.Add(keyField);
        }
    }
}