using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETExitNode : ETNode
    {
        private ETExitNodeContent content;

        public ETExitNode() : base("EXIT", "exit-node") { }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Exit Node";
            this.connectionLine.AddToClassList("exit-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(110, 75);
        }

        public override void CreateContent()
        {
            content = new ETExitNodeContent();
            AddContent(content);
        }

        public override Node CreateNode()
        {
            ExitNode exitNode = new ExitNode();
            exitNode.ID = id;
            exitNode.ExitPointName = content.ExitPointName;

            CreateInputsForNode(exitNode);
            SetSizeForNode(exitNode);
            SetPositionForNode(exitNode);

            return exitNode;
        }

        public override void InitializeFromNode(Node node)
        {
            ExitNode exitNode = node as ExitNode;
            this.id = exitNode.ID;
            SetSizeFromNode(exitNode);
            SetPositionFromNode(exitNode);

            content.ExitPointName = exitNode.ExitPointName;

            InitializeInputFromConnection(this.Inputs[0], exitNode.Inputs[0]);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (content.ExitPointName.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Exits the dialogue.";
        }
    }

    public class ETExitNodeContent : ETNodeContent
    {
        private ETTextField exitPointNameField;

        public ETExitNodeContent() : base()
        {
            this.inputAlignment = NodeAlignment.CENTER;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            contentContainer.style.justifyContent = Justify.FlexStart;
            exitPointNameField = new ETTextField(ETTextField.ValidationType.STRING, "Exit ID");
            exitPointNameField.AddToClassList("exit-field");
            contentContainer.Add(exitPointNameField);
        }

        public string ExitPointName
        {
            get { return exitPointNameField.value; }
            set { exitPointNameField.value = value; }
        }
    }
}