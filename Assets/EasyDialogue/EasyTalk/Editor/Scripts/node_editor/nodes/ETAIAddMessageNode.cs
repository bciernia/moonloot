using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETAIAddMessageNode : ETNode, ETConversationSpecificNode
    {
        private ETAIAddMessageContent messageContent;

        private ETAIAddMessageTextContent messageTextContent;

        public ETAIAddMessageNode() : base("AI: ADD MESSAGE", "ai-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Add Message Node";
            this.connectionLine.AddToClassList("ai-line");
            this.SetDimensions(228, 140);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            messageContent = new ETAIAddMessageContent();
            AddContent(messageContent);

            messageTextContent = new ETAIAddMessageTextContent();
            AddContent(messageTextContent);
        }

        public override Node CreateNode()
        {
            /*AIAddMessageNode addMessageNode = new AIAddMessageNode();
            addMessageNode.ID = id;
            addMessageNode.MessageText = messageTextContent.MessageText;
            addMessageNode.ConvoID = messageContent.ConvoID;
            addMessageNode.MessageOwner = messageContent.MessageOwner;

            CreateInputsForNode(addMessageNode);
            CreateOutputsForNode(addMessageNode);
            SetSizeForNode(addMessageNode);
            SetPositionForNode(addMessageNode);

            return addMessageNode;*/
            return null;
        }

        public override void InitializeFromNode(Node node)
        {
            /*AIAddMessageNode addMessageNode = node as AIAddMessageNode;
            this.id = addMessageNode.ID;
            messageContent.MessageOwner = addMessageNode.MessageOwner;
            messageContent.ConvoID = addMessageNode.ConvoID;
            messageTextContent.MessageText = addMessageNode.MessageText;

            SetSizeFromNode(addMessageNode);
            SetPositionFromNode(addMessageNode);

            InitializeAllInputsAndOutputsFromNode(addMessageNode);*/
        }

        public void SetConvoIDs(List<string> convoIDs)
        {
            messageContent.SetConvoIDs(convoIDs);
        }

        protected override string GetNodeTooltip()
        {
            return "Appends a message to the current prompt.";
        }
    }

    public class ETAIAddMessageContent : ETNodeContent
    {
        private ETEnumField ownerDropdown;

        private ETEditableDropdown convoIdDropdown;

        public ETAIAddMessageContent() : base()
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            /*contentContainer.style.justifyContent = Justify.FlexStart;

            convoIdDropdown = new ETEditableDropdown("Enter Convo ID...");
            List<string> convoIds = EasyTalkNodeEditor.Instance.NodeView.GetConvoIDs();

            if (convoIds.Count > 0)
            {
                EditorApplication.delayCall += delegate
                {
                    convoIdDropdown.Choices = convoIds;
                    convoIdDropdown.SetInputValue(convoIds[0]);
                };
            }
            else
            {
                //EditorApplication.delayCall += delegate { convoIdDropdown.SetInputValue(GUID.Generate().ToString()); };
            }

            convoIdDropdown.AddToClassList("ai-dropdown");
            convoIdDropdown.onChoiceAdded += OnConvoIDAdded;
            convoIdDropdown.style.marginBottom = 4;
            convoIdDropdown.style.flexShrink = 0;
            contentContainer.Add(convoIdDropdown);

            ownerDropdown = new ETEnumField(AIMessageOwner.SYSTEM);
            ownerDropdown.AddToClassList("ai-enum-field");
            ownerDropdown.style.marginBottom = 4;
            ownerDropdown.RegisterCallback<ChangeEvent<Enum>>(OnEnumTypeChanged);
            contentContainer.Add(ownerDropdown);*/
        }

        private void OnEnumTypeChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        private void OnConvoIDAdded(string convoId)
        {
            EasyTalkNodeEditor.Instance.NodeView.AddConvoID(convoId);
        }

        public void SetConvoIDs(List<string> convoIDs)
        {
            convoIdDropdown.Choices = convoIDs;
        }

        /*public AIMessageOwner MessageOwner
        {
            get { return (AIMessageOwner)ownerDropdown.value; }
            set { ownerDropdown.value = value; }
        }*/

        public string ConvoID
        {
            get { return convoIdDropdown.GetInputValue(); }
            set { convoIdDropdown.SetInputValue(value); }
        }
    }

    public class ETAIAddMessageTextContent : ETNodeContent
    {
        private ETInput messageInput;

        private ETTextField messageTextField;

        private Label messageLabel;

        public ETAIAddMessageTextContent() : base()
        {
            this.style.flexShrink = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            messageInput = AddInput(InputOutputType.STRING);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            contentContainer.style.flexGrow = 0;

            messageTextField = new ETTextField(ETTextField.ValidationType.STRING, "Enter message text...");
            messageTextField.AddToClassList("ai-text-input");
            messageTextField.AddToClassList("node-text-area");
            messageTextField.style.marginBottom = 4;
            messageTextField.multiline = true;

#if UNITY_2023_1_OR_NEWER

            messageTextField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            messageTextField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif
            contentContainer.Add(messageTextField);

            messageLabel = new Label("Message Text");
            messageLabel.AddToClassList("ai-label");
            messageLabel.style.display = DisplayStyle.None;
            contentContainer.Add(messageLabel);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if (inputId == messageInput.ID)
            {
                messageTextField.style.display = DisplayStyle.None;
                messageLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            base.OnConnectionDeleted(inputId, outputId);

            if (inputId == messageInput.ID)
            {
                messageTextField.style.display = DisplayStyle.Flex;
                messageLabel.style.display = DisplayStyle.None;
            }
        }

        public string MessageText
        {
            get { return messageTextField.value; }
            set { messageTextField.value = value; }
        }
    }
}
