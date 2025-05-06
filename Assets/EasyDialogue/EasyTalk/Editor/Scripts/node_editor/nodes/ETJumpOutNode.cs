using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETJumpOutNode : ETNode
    {
        private ETJumpOutNodeContent content;

        public ETJumpOutNode() : base("JUMP", "jump-out-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Jump Out Node";
            this.connectionLine.AddToClassList("jump-out-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(148, 75);
        }

        public override void CreateContent()
        {
            content = new ETJumpOutNodeContent();
            AddContent(content);
        }

        public string Key { get { return content.Key; } }

        public override Node CreateNode()
        {
            JumpOutNode jumpOutNode = new JumpOutNode();
            jumpOutNode.ID = id;

            CreateInputsForNode(jumpOutNode);
            SetSizeForNode(jumpOutNode);
            SetPositionForNode(jumpOutNode);

            jumpOutNode.Key = content.Key;

            return jumpOutNode;
        }

        public override void InitializeFromNode(Node node)
        {
            JumpOutNode jumpOutNode = node as JumpOutNode;
            this.id = jumpOutNode.ID;
            SetSizeFromNode(jumpOutNode);
            SetPositionFromNode(jumpOutNode);

            content.Key = jumpOutNode.Key;

            InitializeAllInputsAndOutputsFromNode(jumpOutNode);
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
            return "Jump to a Jump-In node in the dialogue.";
        }
    }

    public class ETJumpOutNodeContent : ETNodeContent
    {
        private TextField keyField;

        public ETJumpOutNodeContent() : base()
        {
            this.inputAlignment = NodeAlignment.CENTER;
        }

        public string Key
        {
            get { return keyField.text; }
            set { keyField.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
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