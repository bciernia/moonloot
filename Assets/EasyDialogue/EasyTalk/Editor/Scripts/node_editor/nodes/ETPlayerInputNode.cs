using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Utility;
using UnityEngine.UIElements;


namespace EasyTalk.Editor.Nodes
{
    public class ETPlayerInputNode : ETNode
    {
        private PlayerInputContent inputContent;

        public ETPlayerInputNode() : base("PLAYER INPUT", "input-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Player Input Node";
            this.connectionLine.AddToClassList("input-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(200, 95);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new PlayerInputContent();
            AddContent(inputContent);
        }

        public override Node CreateNode()
        {
            PlayerInputNode playerInputNode = new PlayerInputNode();
            playerInputNode.ID = id;
            playerInputNode.HintText = inputContent.HintText;

            CreateInputsForNode(playerInputNode);
            CreateOutputsForNode(playerInputNode);
            SetSizeForNode(playerInputNode);
            SetPositionForNode(playerInputNode);

            return playerInputNode;
        }

        public override void InitializeFromNode(Node node)
        {
            PlayerInputNode playerInputNode = node as PlayerInputNode;
            this.id = playerInputNode.ID;
            this.inputContent.HintText = playerInputNode.HintText;

            SetSizeFromNode(playerInputNode);
            SetPositionFromNode(playerInputNode);

            InitializeAllInputsAndOutputsFromNode(playerInputNode);
        }

        protected override string GetNodeTooltip()
        {
            return "Pull up an interface for the player to enter text. The text can then be sent to another node via the string output.";
        }
    }

    public class PlayerInputContent : ETNodeContent
    {
        private ETTextField hintTextField;

        protected override void CreateInputs()
        {
            base.CreateInputs();

            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            hintTextField = new ETTextField("Enter Hint Text...");
            hintTextField.AddToClassList("input-text-input");
            hintTextField.AddToClassList("node-text-area");
            hintTextField.multiline = true;

#if UNITY_2023_1_OR_NEWER

            hintTextField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            hintTextField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif

            contentContainer.Add(hintTextField);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            this.AddOutput(InputOutputType.STRING);
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public string HintText
        {
            get { return hintTextField.value; }
            set { this.hintTextField.value = value; }
        }
    }
}
