using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETWaitNode : ETNode
    {
        private ETWaitNodeContent content;

        public ETWaitNode() : base("WAIT", "wait-node") { }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Wait Node";
            this.connectionLine.AddToClassList("wait-line");
            this.resizeType = ResizeType.NONE;
            this.SetDimensions(110, 75);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            content = new ETWaitNodeContent();
            AddContent(content);
        }

        public override Node CreateNode()
        {
            WaitNode waitNode = new WaitNode();
            waitNode.WaitTime = content.WaitTimeValue;

            waitNode.ID = id;

            CreateInputsForNode(waitNode);
            CreateOutputsForNode(waitNode);
            SetSizeForNode(waitNode);
            SetPositionForNode(waitNode);

            return waitNode;
        }

        public override void InitializeFromNode(Node node)
        {
            WaitNode waitNode = node as WaitNode;
            this.id = waitNode.ID;
            content.WaitTimeValue = waitNode.WaitTime;

            SetPositionFromNode(waitNode);
            SetSizeFromNode(waitNode);
            InitializeAllInputsAndOutputsFromNode(waitNode);
        }

        protected override string GetNodeTooltip()
        {
            return "Waits for a specified amount of time (in seconds).";
        }
    }

    public class ETWaitNodeContent : ETNodeContent
    {
        private ETTextField waitTimeField;

        protected override void CreateInputs()
        {
            base.CreateInputs();
            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            waitTimeField = new ETTextField(ETTextField.ValidationType.FLOAT, "1.0");
            waitTimeField.AddToClassList("flow-text-field");
            contentContainer.Add(waitTimeField);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public string WaitTimeValue
        {
            get { return waitTimeField.text; }
            set { waitTimeField.value = value; }
        }
    }
}
