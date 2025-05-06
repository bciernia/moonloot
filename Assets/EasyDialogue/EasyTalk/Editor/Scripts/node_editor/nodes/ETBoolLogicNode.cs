using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETBoolLogicNode : ETNode
    {
        private ETBoolLogicContent content;

        public LogicNode storedNodeState;

        public ETBoolLogicNode() : base("BOOL LOGIC", "bool-logic-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Bool Logic Node";
            this.connectionLine.AddToClassList("bool-logic-line");
            this.resizeType = ResizeType.NONE;
            this.SetDimensions(160, 120);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            content = new ETBoolLogicContent();
            AddContent(content);

            AddNodeChangeCallbacks();
        }

        public override Node CreateNode()
        {
            LogicNode logicNode = new LogicNode();
            logicNode.ID = id;

            CreateInputsForNode(logicNode);
            CreateOutputsForNode(logicNode);
            SetSizeForNode(logicNode);
            SetPositionForNode(logicNode);

            logicNode.LogicMode = content.LogicMode;

            return logicNode;
        }

        public override void InitializeFromNode(Node node)
        {
            LogicNode logicNode = node as LogicNode;
            this.id = logicNode.ID;
            SetSizeFromNode(logicNode);
            SetPositionFromNode(logicNode);

            content.LogicMode = logicNode.LogicMode;

            InitializeAllInputsAndOutputsFromNode(logicNode);

            UpdateStoredNodeState();
        }

        private void AddNodeChangeCallbacks()
        {
            foreach (ETInput input in Inputs)
            {
                input.onConnectionCreated += ConnectionCreatedOrDeleted;
                input.onConnectionDeleted += ConnectionCreatedOrDeleted;
            }

            foreach (ETOutput output in Outputs)
            {
                output.onConnectionCreated += ConnectionCreatedOrDeleted;
                output.onConnectionDeleted += ConnectionCreatedOrDeleted;
            }
        }

        public void UpdateStoredNodeState()
        {
            storedNodeState = CreateNode() as LogicNode;
        }

        public override void NodeMoved()
        {
            UpdateStoredNodeState();
        }

        public override void NodeResized()
        {
            UpdateStoredNodeState();
        }

        private void ConnectionCreatedOrDeleted(int inputId, int outputId)
        {
            UpdateStoredNodeState();
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            return content.LogicMode.ToString().ToLower().Contains(text.ToLower());
        }

        protected override string GetNodeTooltip()
        {
            return "Performs a boolean operation and sends the result to the boolean output. If encountered in dialogue flow and the result of the operation is true, the dialogue will continue down the true (T) path; otherwise, the dialogue will continue down the false (F) path.";
        }
    }

    public class ETBoolLogicContent : ETNodeContent
    {
        private ETEnumField operationField;

        private ETInput dialogueInput;

        private ETInput valueAInput;

        private ETInput valueBInput;

        public ETBoolLogicContent() : base(NodeAlignment.TOP, NodeAlignment.TOP) { }

        public LogicNode.LogicOperation LogicMode
        {
            get { return (LogicNode.LogicOperation)operationField.value; }
            set { operationField.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();

            dialogueInput = AddInput(InputOutputType.DIALGOUE_FLOW);
            valueAInput = AddInput(InputOutputType.BOOL);
            valueBInput = AddInput(InputOutputType.BOOL);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            AddOutput(InputOutputType.BOOL);
            AddOutput(InputOutputType.DIALOGUE_TRUE_FLOW);
            AddOutput(InputOutputType.DIALOGUE_FALSE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexDirection = FlexDirection.Row;

            operationField = new ETEnumField(LogicNode.LogicOperation.AND);
            operationField.PublishUndoableActions = false;
            operationField.AddToClassList("bool-logic-type-field");
            operationField.AddToClassList("logic-enum-field");
            operationField.RegisterCallback<ChangeEvent<Enum>>(LogicOperationChanged);
            contentContainer.Add(operationField);
        }

        private void LogicOperationChanged(ChangeEvent<Enum> evt)
        {
            ETBoolLogicNode node = (ETUtils.FindNodeParent(this) as ETBoolLogicNode);
            LogicNode storedNodeState = node.storedNodeState;
            EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(node.ID, storedNodeState, "Changed Bool Logic Mode");

            LogicNode.LogicOperation logicMode = Enum.Parse<LogicNode.LogicOperation>(evt.newValue.ToString());
            if (logicMode == LogicNode.LogicOperation.NOT || logicMode == LogicNode.LogicOperation.IS_TRUE || logicMode == LogicNode.LogicOperation.IS_FALSE)
            {
                EasyTalkNodeEditor.Instance.NodeView.UnregisterNodeInput(valueBInput);
                EasyTalkNodeEditor.Instance.NodeView.MarkDirtyRepaint();
                EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(valueBInput);

                if (inputPanel.Contains(valueBInput))
                {
                    inputPanel.Remove(valueBInput);
                }
            }
            else
            {
                if (!inputPanel.Contains(valueBInput))
                {
                    inputPanel.Add(valueBInput);
                    EasyTalkNodeEditor.Instance.NodeView.RegisterNodeInput(valueBInput, GetParentNode(this.parent));
                }
            }

            EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Changed Bool Logic Mode");
            ETBoolLogicNode boolLogicNode = ETUtils.FindNodeParent(this) as ETBoolLogicNode;
            boolLogicNode.UpdateStoredNodeState();

            EasyTalkNodeEditor.Instance.NodesChanged();
        }
    }
}