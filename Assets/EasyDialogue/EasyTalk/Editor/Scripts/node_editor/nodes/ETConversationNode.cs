using EasyTalk.Character;
using EasyTalk.Editor.Components;
using EasyTalk.Localization;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace EasyTalk.Editor.Nodes
{
    public class ETConversationNode : ETListNode
    {
        private ETConversationNodeCharacterContent characterContent;

        private string icon;

        public ETConversationNode() : base("CONVERSATION", "conversation-node") { }

        protected override void Initialize()
        {
            this.supportsSettings = true;
            base.Initialize();

            this.name = "Conversation Node";
            this.connectionLine.AddToClassList("conversation-line");
            this.SetDimensions(268, 140);
        }

        public override float GetMinimumHeight()
        {
            return 112.0f + (ListPanel.Items.Count * 30.0f);
        }

        public override void CreateContent()
        {
            characterContent = new ETConversationNodeCharacterContent();
            AddContent(characterContent);

            CreateListPanel();
            AddItemToList();

            ETConversationNodeOutputContent outputContent = new ETConversationNodeOutputContent();
            AddContent(outputContent);

            listPanel.onItemAdded += OnItemAdded;
            listPanel.onItemRemoved += OnItemRemoved;
        }

        public void SetCharacterNames(List<string> characters)
        {
            characterContent.SetCharacterNames(characters);
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETConversationNodeItem item = new ETConversationNodeItem();
            item.RemoveButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(item);
                }
            };
            return item;
        }

        private void OnItemAdded(ETNode node, ETNodeContent content)
        {
            this.style.height = GetHeight() + 30;
        }

        private void OnItemRemoved(ETNode node, ETNodeContent content)
        {
            this.style.height = GetHeight() - 26;
        }

        public override Node CreateNode()
        {
            ConversationNode convoNode = new ConversationNode();
            convoNode.ID = id;
            convoNode.CharacterName = this.characterContent.GetCharacterName();
            convoNode.Icon = icon;

            CreateInputsForNode(convoNode);
            CreateOutputsForNode(convoNode);
            SetSizeForNode(convoNode);
            SetPositionForNode(convoNode);

            foreach (ETConversationNodeItem item in this.Items)
            {
                convoNode.AddItem(new ConversationItem(item.Text, item.AudioClip, item.AudioClipFile));
            }

            return convoNode;
        }

        public override void InitializeFromNode(Node node)
        {
            ConversationNode conversationNode = node as ConversationNode;
            this.id = conversationNode.ID;
            SetPositionFromNode(conversationNode);

            while (Items.Count > conversationNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < conversationNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(conversationNode);

            characterContent.SetCharacterName(conversationNode.CharacterName);
            icon = conversationNode.Icon;

            for (int i = 0; i < Items.Count; i++)
            {
                ConversationItem conversationItem = (ConversationItem)conversationNode.Items[i];

                if (conversationItem != null)
                {
                    ((ETConversationNodeItem)Items[i]).Text = conversationItem.Text;
                    ((ETConversationNodeItem)Items[i]).AudioClipFile = conversationItem.AudioClipFile;
                }
            }

            InitializeAllInputsAndOutputsFromNode(conversationNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            foreach (ETConversationNodeItem item in Items)
            {
                if (item.Text.ToLower().Contains(text.ToLower())) { return true; }
                if (item.AudioClipFile != null && item.AudioClipFile.ToLower().Contains(text.ToLower())) { return true; }
            }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

            string characterName = characterContent.GetCharacterName();
            if (characterName != null && characterName.Length > 0)
            {
                sourceSet.AddOrFindTranslation(characterName);
            }

            foreach (ETConversationNodeItem item in Items)
            {
                if (item.Text.ToString().Length > 0)
                {
                    sourceSet.AddOrFindTranslation(item.Text.ToString());
                }
            }
        }

        public override void BuildSettingsPanel(ETSettingsPanel settingsPanel)
        {
            base.BuildSettingsPanel(settingsPanel);

            settingsPanel.SetDescription("Conversation Node (" + this.id + ")");

            VisualElement contentPanel = settingsPanel.GetContentPanel();

            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.pickingMode = PickingMode.Ignore;

            bool characterFound = false;
            List<CharacterLibrary> charLibs = EasyTalkNodeEditor.Instance.CharacterLibraries;

            foreach (CharacterLibrary characterLibrary in charLibs) 
            {
                CharacterDefinition charDef = characterLibrary.GetCharacterDefinition(characterContent.GetCharacterName());

                if (charDef != null)
                {
                    if (charDef.IconsSprites.Count > 0)
                    {
                        Foldout iconFoldout = CreateIconFoldout(charDef);
                        scrollView.Add(iconFoldout);

                        characterFound = true;
                    }
                }

                if (characterFound) { break; }
            }

            if(!characterFound)
            {
                Label infoLabel = new Label("No character definition or icons found for '" + characterContent.GetCharacterName() + "' in the character library...");
                infoLabel.style.whiteSpace = WhiteSpace.Normal;
                infoLabel.style.color = new StyleColor(new Color(255.0f, 0.0f, 0.0f));
                scrollView.Add(infoLabel);
            }

            contentPanel.Add(scrollView);

            scrollView.contentViewport.parent.pickingMode = PickingMode.Ignore;
        }

        private Foldout CreateIconFoldout(CharacterDefinition charDef)
        {
            Foldout iconFoldout = new Foldout();
            iconFoldout.AddToClassList("settings-foldout");
            iconFoldout.text = "Icon Settings";

            Box iconSettingsBox = new Box();
            iconSettingsBox.style.flexDirection = FlexDirection.Row;
            iconSettingsBox.style.flexShrink = 0;
            iconSettingsBox.style.flexGrow = 1;

            Label iconIdLabel = new Label("Icon ID:");
            iconIdLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            iconSettingsBox.Add(iconIdLabel);

            List<string> iconIds = GetIconIdList(charDef);
            iconIds.Insert(0, "NONE");

            ETDropdownField iconDropdown = new ETDropdownField();
            iconDropdown.RegisterValueChangedCallback(OnIconChanged);
            iconDropdown.style.flexShrink = 1;
            iconDropdown.style.flexGrow = 1;
            iconDropdown.choices = iconIds;

            if (icon == null)
            {
                iconDropdown.value = "NONE";
            }
            else
            {
                iconDropdown.value = icon;
            }

            iconSettingsBox.Add(iconDropdown);

            iconFoldout.Add(iconSettingsBox);

            return iconFoldout;
        }

        private List<string> GetIconIdList(CharacterDefinition charDef)
        {
            List<string> iconIds = new List<string>();
            foreach(AnimatableDisplayImage icon in charDef.IconsSprites)
            {
                iconIds.Add(icon.ID);
            }

            return iconIds;
        }

        private void OnIconChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue.Equals("NONE"))
            {
                icon = null;
            }
            else
            {
                icon = evt.newValue;
            }
        }

        protected override string GetNodeTooltip()
        {
            return "Used to write lines of dialogue.";
        }
    }

    public class ETConversationNodeCharacterContent : ETNodeContent
    {
        private ETEditableDropdown characterDropdown;

        public ETConversationNodeCharacterContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM)
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            this.style.flexShrink = 0;

            characterDropdown = new ETEditableDropdown("Choose/Enter Character...");
            characterDropdown.Choices = EasyTalkNodeEditor.Instance.NodeView.GetCharacterList();
            characterDropdown.AddToClassList("character-dropdown");
            characterDropdown.onChoiceAdded += OnChoiceAdded;
            contentContainer.Add(characterDropdown);
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
    }

    public class ETConversationNodeOutputContent : ETNodeContent
    {
        public ETConversationNodeOutputContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM)
        {
            this.style.flexShrink = 0;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETConversationNodeItem : ETNodeContent
    {
        private ETNodeButton removeButton;

        private TextField convoField;

        private ETAudioClipZone audioZone;

        public ETConversationNodeItem() : base() { }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            contentContainer.AddToClassList("conversation-line-item");
            //this.style.flexDirection = FlexDirection.Row;

            removeButton = new ETNodeButton();
            removeButton.AddToClassList("remove-item-button");
            contentContainer.Add(removeButton);

            convoField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Conversation Text...");
            convoField.AddToClassList("conversation-text-input");
            convoField.AddToClassList("node-text-area");
            convoField.multiline = true;

#if UNITY_2023_1_OR_NEWER

            convoField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            convoField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif
            contentContainer.Add(convoField);

            audioZone = new ETAudioClipZone();
            contentContainer.Add(audioZone);
        }

        public ETNodeButton RemoveButton
        {
            get { return removeButton; }
        }

        public string Text
        {
            get { return convoField.text; }
            set { convoField.value = value; }
        }

        public AudioClip AudioClip
        {
            get { return audioZone.GetAudioClip(); }
            set { audioZone.SetAudioClip(value); }
        }

        public void SetAudioClip(int assetID)
        {
            audioZone.SetAudioClip(assetID);
        }

        public string AudioClipFile
        {
            get { return audioZone.GetAudioClipFile(); }
            set { audioZone.SetAudioClipFile(value); }
        }
    }
}