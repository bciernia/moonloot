using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETPauseNode : ETNode
    {
        private ETPauseNodeContent content;

        public ETPauseNode() : base("PAUSE", "pause-node")
        {
            base.Initialize();
            this.name = "Pause Node";
            this.connectionLine.AddToClassList("pause-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(180, 75);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            content = new ETPauseNodeContent();
            AddContent(content);
        }

        public override Node CreateNode()
        {
            PauseNode pauseNode = new PauseNode();
            pauseNode.Signal = content.SignalFieldValue;

            pauseNode.ID = id;

            CreateInputsForNode(pauseNode);
            CreateOutputsForNode(pauseNode);
            SetSizeForNode(pauseNode);
            SetPositionForNode(pauseNode);

            return pauseNode;
        }

        public override void InitializeFromNode(Node node)
        {
            PauseNode pauseNode = node as PauseNode;
            this.id = pauseNode.ID;
            content.SignalFieldValue = pauseNode.Signal;

            SetPositionFromNode(pauseNode);
            SetSizeFromNode(pauseNode);
            InitializeAllInputsAndOutputsFromNode(pauseNode);
        }

        public override bool HasText(string text)
        {
            if(base.HasText(text))
            {
                return true;
            }

            if(content.SignalFieldValue.ToLower().Contains(text.ToLower()))
            {
                return true;
            }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Used to pause the dialogue until Continue() is called on the controller. The value entered in the signal field is sent to Dialogue Listeners which override the OnPause() method.";
        }
    }

    public class ETPauseNodeContent : ETNodeContent
    {
        private ETTextField signalField;

        protected override void CreateInputs()
        {
            base.CreateInputs();
            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            signalField = new ETTextField(ETTextField.ValidationType.STRING, "Enter signal...");
            signalField.AddToClassList("flow-text-field");
            contentContainer.Add(signalField);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public string SignalFieldValue
        {
            get { return signalField.text; }
            set { signalField.value = value; }
        }
    }
}
