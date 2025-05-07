using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETAIInitNode : ETListNode, ETConversationSpecificNode
    {
        private ETAIInitInputContent inputContent;

        private ETAIInitOutputContent outputContent;

        public ETAIInitNode() : base("AI: INITIALIZE", "ai-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "AI Initilization Node";
            this.connectionLine.AddToClassList("ai-line");
            this.SetDimensions(312, 200);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            inputContent = new ETAIInitInputContent();
            AddContent(inputContent);

            CreateListPanel();
            AddItemToList();

            outputContent = new ETAIInitOutputContent();
            AddContent(outputContent);

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;
            listPanel.style.alignItems = Align.FlexStart;
            listPanel.ContentPanel.style.height = new StyleLength(StyleKeyword.Auto);
        }

        private void OnItemAdded(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() + 52;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent item)
        {
            this.style.height = GetHeight() - 52.0f;

            for (int i = 0; i < Items.Count; i++)
            {
                ETAIMessageItem content = (ETAIMessageItem)Items[i];
                content.MessageLabel.text = "Message " + (i + 1) + " Text";
            }
        }

        public override float GetMinimumHeight()
        {
            return 120.0f + (Items.Count * 52.0f);
        }

        public override Node CreateNode()
        {
            /*AIInitializationNode initNode = new AIInitializationNode();
            initNode.ID = id;
            initNode.ConvoID = inputContent.ConvoID;

            foreach(ETAIMessageItem item in this.Items)
            {
                initNode.AddItem(new AIChatMessage(item.MessageOwner, item.MessageText));
            }

            CreateInputsForNode(initNode);
            CreateOutputsForNode(initNode);
            SetSizeForNode(initNode);
            SetPositionForNode(initNode);

            return initNode;*/
            return null;
        }

        public override void InitializeFromNode(Node node)
        {
            /*AIInitializationNode initNode = (AIInitializationNode)node;
            this.id = initNode.ID;
            
            inputContent.ConvoID = initNode.ConvoID;

            while (Items.Count > initNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < initNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(initNode);

            for (int i = 0; i < Items.Count; i++)
            {
                ETAIMessageItem itemContent = ((ETAIMessageItem)Items[i]);
                AIChatMessage message = ((AIChatMessage)(initNode.Items[i]));
                itemContent.MessageOwner = message.MessageOwner;
                itemContent.MessageText = message.MessageText;
            }

            SetPositionFromNode(initNode);

            InitializeAllInputsAndOutputsFromNode(initNode);*/
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETAIMessageItem item = new ETAIMessageItem();
            item.MessageLabel.text = "Message " + (Items.Count + 1) + " Text";
            item.AddToClassList("list-node-item");
            item.RemoveButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(item);
                }
            };
            return item;
        }

        public void SetConvoIDs(List<string> convoIDs)
        {
            inputContent.SetConvoIDs(convoIDs);
        }

        protected override string GetNodeTooltip()
        {
            return "Initialize an AI.";
        }
    }

    public class ETAIInitInputContent : ETNodeContent
    {
        private ETEditableDropdown convoIdDropdown;

        public ETAIInitInputContent() : base()
        {
            this.style.flexGrow = 0;
            this.style.flexShrink = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();

            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

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
            contentContainer.Add(convoIdDropdown);
        }

        private void OnConvoIDAdded(string convoId)
        {
            EasyTalkNodeEditor.Instance.NodeView.AddConvoID(convoId);
        }

        public string ConvoID
        {
            get { return convoIdDropdown.GetInputValue(); }
            set { convoIdDropdown.SetInputValue(value); }
        }

        public void SetConvoIDs(List<string> convoIds)
        {
            convoIdDropdown.Choices = convoIds;
        }
    }

    public class ETAIInitOutputContent : ETNodeContent
    {
        public ETAIInitOutputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETAIMessageItem : ETNodeContent
    {
        private ETInput textInput;

        private ETNodeButton removeButton;

        private ETEnumField ownerDropdown;

        private ETTextField messageTextField;

        private Label messageLabel;

        public ETAIMessageItem() : base()
        {
            this.style.flexGrow = 1;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            inputPanel.style.marginTop = 4;

            removeButton = new ETNodeButton();
            removeButton.AddToClassList("remove-item-button");
            inputPanel.Add(removeButton);

            textInput = AddInput(InputOutputType.STRING);
            textInput.style.marginTop = 6;
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            /*contentContainer.style.height = new StyleLength(StyleKeyword.Auto);

            ownerDropdown = new ETEnumField(AIMessageOwner.SYSTEM);
            ownerDropdown.AddToClassList("ai-enum-field");
            ownerDropdown.style.marginBottom = 4;
            ownerDropdown.RegisterCallback<ChangeEvent<Enum>>(OnEnumTypeChanged);
            contentContainer.Add(ownerDropdown);

            messageTextField = new ETTextField("Enter message text...");
            messageTextField.AddToClassList("ai-text-input");
            messageTextField.AddToClassList("node-text-area");
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
            contentContainer.Add(messageLabel);*/
        }

        private void OnEnumTypeChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        /*public AIMessageOwner MessageOwner
        {
            get { return (AIMessageOwner)ownerDropdown.value; }
            set { ownerDropdown.value = value; }
        }*/

        public string MessageText
        {
            get { return messageTextField.value; }
            set { messageTextField.value = value; }
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            base.OnConnectionCreated(inputId, outputId);

            if (inputId == textInput.ID)
            {
                contentContainer.style.flexGrow = 0;
                messageTextField.style.display = DisplayStyle.None;
                messageLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            base.OnConnectionDeleted(inputId, outputId);

            if (inputId == textInput.ID)
            {
                contentContainer.style.flexGrow = 1;
                messageTextField.style.display = DisplayStyle.Flex;
                messageLabel.style.display = DisplayStyle.None;
            }
        }

        public ETNodeButton RemoveButton
        {
            get { return removeButton; }
        }

        public Label MessageLabel
        {
            get { return messageLabel; }
        }
    }
}
