using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETAIPromptNode : ETNode, ETConversationSpecificNode
    {
        private AIPromptContent content;

        public ETAIPromptNode() : base("AI: PROMPT", "ai-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "AI Initilization Node";
            this.connectionLine.AddToClassList("ai-line");
            this.SetDimensions(270, 160);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            content = new AIPromptContent();
            AddContent(content);
        }

        public override Node CreateNode()
        {
            /*AIPromptNode promptNode = new AIPromptNode();
            promptNode.ID = id;
            promptNode.CharacterName = content.GetCharacterName();
            promptNode.ConvoID = content.ConvoID;

            CreateInputsForNode(promptNode);
            CreateOutputsForNode(promptNode);
            SetSizeForNode(promptNode);
            SetPositionForNode(promptNode);

            return promptNode;*/
            return null;
        }

        public override void InitializeFromNode(Node node)
        {
            /*AIPromptNode promptNode = node as AIPromptNode;
            this.id = promptNode.ID;
            content.SetCharacterName(promptNode.CharacterName);
            content.ConvoID = promptNode.ConvoID;

            SetSizeFromNode(promptNode);
            SetPositionFromNode(promptNode);

            InitializeAllInputsAndOutputsFromNode(promptNode);*/
        }

        public void SetConvoIDs(List<string> convoIDs)
        {
            content.SetConvoIDs(convoIDs);
        }

        protected override string GetNodeTooltip()
        {
            return "Send a prompt to an AI.";
        }
    }

    public class AIPromptContent : ETNodeContent
    {
        private ETEditableDropdown characterDropdown;

        private ETEnumField promptModeDropdown;

        private ETEnumField outputModeDropdown;

        private ETEditableDropdown convoIdDropdown;

        private ETOutput stringOutput;

        protected override void CreateInputs()
        {
            base.CreateInputs();

            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            /*contentContainer.style.justifyContent = Justify.FlexStart;

            characterDropdown = new ETEditableDropdown("Choose/Enter Character...");
            characterDropdown.Choices = EasyTalkNodeEditor.Instance.NodeView.GetCharacterList();
            characterDropdown.AddToClassList("ai-dropdown");
            characterDropdown.onChoiceAdded += OnChoiceAdded;
            characterDropdown.style.marginBottom = 4;
            contentContainer.Add(characterDropdown);

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
            contentContainer.Add(convoIdDropdown);

            promptModeDropdown = new ETEnumField(AIPromptMode.FULL);
            promptModeDropdown.AddToClassList("ai-enum-field");
            promptModeDropdown.RegisterCallback<ChangeEvent<Enum>>(OnEnumTypeChanged);
            promptModeDropdown.RegisterCallback<ChangeEvent<Enum>>(PromptModeChanged);
            promptModeDropdown.style.marginBottom = 4;
            contentContainer.Add(promptModeDropdown);

            outputModeDropdown = new ETEnumField(AIOutputMode.DISPLAY);
            outputModeDropdown.AddToClassList("ai-enum-field");
            outputModeDropdown.RegisterCallback<ChangeEvent<Enum>>(OnEnumTypeChanged);
            outputModeDropdown.style.marginBottom= 4;
            contentContainer.Add(outputModeDropdown);*/
        }

        private void OnEnumTypeChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        protected void PromptModeChanged(ChangeEvent<Enum> evt)
        {
            //If the prompt mode is async, don't allow a string output
            /*AIPromptMode promptMode = (AIPromptMode)promptModeDropdown.value;
            if (promptMode == AIPromptMode.FULL)
            {
                stringOutput.style.display = DisplayStyle.Flex;
                outputModeDropdown.style.display = DisplayStyle.Flex;
            }
            else
            {
                stringOutput.DisconnectAll();
                stringOutput.style.display = DisplayStyle.None;
                outputModeDropdown.value = AIOutputMode.DISPLAY;
                outputModeDropdown.style.display = DisplayStyle.None;
            }*/
        }

        private void OnChoiceAdded(string character)
        {
            EasyTalkNodeEditor.Instance.NodeView.AddCharacter(character);
        }

        public string GetCharacterName()
        {
            return this.characterDropdown.GetInputValue();
        }

        public void SetCharacterName(string characterName)
        {
            this.characterDropdown.SetInputValue(characterName);
        }

        public void SetCharacterNames(List<string> characters)
        {
            characterDropdown.Choices = characters;
        }

        private void OnConvoIDAdded(string convoId)
        {
            EasyTalkNodeEditor.Instance.NodeView.AddConvoID(convoId);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            stringOutput = AddOutput(InputOutputType.STRING);
            AddOutput(InputOutputType.DIALGOUE_FLOW);
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
}
