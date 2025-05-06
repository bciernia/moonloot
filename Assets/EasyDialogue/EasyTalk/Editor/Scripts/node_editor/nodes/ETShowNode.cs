using EasyTalk.Character;
using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Utility;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace EasyTalk.Editor.Nodes
{
    public class ETShowNode : ETListNode
    {
        private ETShowNodeInputContent inputContent;
        private ETShowNodeOutputContent outputContent;

        public ETShowNode() : base("SHOW", "display-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Show Node";
            this.connectionLine.AddToClassList("display-line");
            this.SetDimensions(232, 200);
        }

        public override float GetMinimumHeight()
        {
            return 120.0f;
        }

        public override void CreateContent()
        {
            inputContent = new ETShowNodeInputContent();
            AddContent(inputContent);

            CreateListPanel(ETListNodeType.SCROLL);
            AddItemToList();

            outputContent = new ETShowNodeOutputContent();
            AddContent(outputContent);
        }

        public override Node CreateNode()
        {
            ShowNode showNode = new ShowNode();
            showNode.ID = id;

            CreateInputsForNode(showNode);
            CreateOutputsForNode(showNode);
            SetSizeForNode(showNode);
            SetPositionForNode(showNode);

            foreach (ETShowNodeItem item in this.Items)
            {
                ShowNodeItem showNodeItem = new ShowNodeItem(item.ShowMode, item.CharacterName, item.ImageID, item.OverridingDisplayID, item.DisplayID);
                showNodeItem.IsExpanded = item.Expanded;
                showNode.AddItem(showNodeItem);
            }

            return showNode;
        }

        public override void InitializeFromNode(Node node)
        {
            ShowNode showNode = node as ShowNode;
            this.id = showNode.ID;
            SetPositionFromNode(showNode);

            while (Items.Count > showNode.Items.Count)
            {
                RemoveItemFromList(Items[Items.Count - 1]);
            }

            while (Items.Count < showNode.Items.Count)
            {
                AddItemToList();
            }

            SetSizeFromNode(showNode);

            for (int i = 0; i < Items.Count; i++)
            {
                ShowNodeItem showItem = (ShowNodeItem)showNode.Items[i];
                ETShowNodeItem nodeItem = ((ETShowNodeItem)Items[i]);

                if (showItem != null)
                {
                    nodeItem.ShowMode = showItem.ShowMode;
                    nodeItem.CharacterName = showItem.CharacterName;
                    nodeItem.ImageID = showItem.ImageID;
                    nodeItem.OverridingDisplayID = showItem.OverrideDisplayID;
                    nodeItem.DisplayID = showItem.DisplayID;
                    nodeItem.Expanded = showItem.IsExpanded;
                }
            }

            InitializeAllInputsAndOutputsFromNode(showNode);
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETShowNodeItem item = new ETShowNodeItem();
            item.RemoveButton.onButtonClicked += delegate
            {
                if (Items.Count > 1)
                {
                    RemoveItemFromList(item);
                }
            };
            return item;
        }

        protected override string GetNodeTooltip()
        {
            return "Shows one or more displays or character images before continuing.";
        }
    }

    public class ETShowNodeInputContent : ETNodeContent
    {
        public ETShowNodeInputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETShowNodeOutputContent : ETNodeContent
    {
        public ETShowNodeOutputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETShowNodeItem : ETNodeContent
    {
        private Foldout foldout;
        private ShowMode showMode;
        private ETNodeButton removeButton;

        private ETEnumField modeDropdown;
        private ETDropdownField characterDropdown;
        private ETToggle overrideDisplayIDToggle;
        private ETTextField displayIdField;
        private ETDropdownField imageDropdown;

        protected override void CreateContent(VisualElement contentContainer)
        {
            contentContainer.style.flexDirection = FlexDirection.Row;

            removeButton = new ETNodeButton();
            removeButton.AddToClassList("remove-item-button");
            removeButton.style.alignSelf = Align.FlexStart;
            removeButton.style.marginTop = 2;
            contentContainer.Add(removeButton);

            //Create a foldout
            foldout = new Foldout();
            foldout.AddToClassList("display-foldout");
            foldout.style.flexGrow = 1;
            foldout.text = "";

            //Show dropdown for mode
            modeDropdown = new ETEnumField(showMode);
            modeDropdown.AddToClassList("display-dropdown");
            modeDropdown.RegisterCallback<ChangeEvent<System.Enum>>(ModeChanged);
            foldout.Add(modeDropdown);

            characterDropdown = CreateCharacterDropdown();
            characterDropdown.tooltip = "The character to display.";
            characterDropdown.AddToClassList("display-dropdown");
            foldout.Add(characterDropdown);

            imageDropdown = CreateSpriteDropdown();
            imageDropdown.tooltip = "The ID of the portrayal to display (defined in the Character Library).";
            imageDropdown.AddToClassList("display-dropdown");
            foldout.Add(imageDropdown);

            overrideDisplayIDToggle = new ETToggle("Override Target ID?");
            overrideDisplayIDToggle.AddToClassList("show-node-toggle");
            overrideDisplayIDToggle.tooltip = "Whether the character portrayal should be displayed on a different target display ID than the default (the default is configured in the Character Library).";
            overrideDisplayIDToggle.RegisterValueChangedCallback(OverrideDisplayIdChanged);
            foldout.Add(overrideDisplayIDToggle);

            displayIdField = CreateDisplayIdTextfield();
            displayIdField.AddToClassList("display-text-input");
            foldout.Add(displayIdField);

            SetupView();
            contentContainer.Add(foldout);
        }

        private void OverrideDisplayIdChanged(ChangeEvent<bool> evt)
        {
            SetupView();
        }

        private void ModeChanged(ChangeEvent<System.Enum> evt)
        {
            SetupView();

            if (((ShowMode)evt.newValue) == ShowMode.CHARACTER)
            {
                displayIdField.tooltip = "The Display ID to display the character's portrayal image on. If this value is unset, the default target (defined in the Character Library) will be used.";
            }
            else
            {
                displayIdField.tooltip = "The Display ID for the display which is to be shown.";
            }
        }

        private void SetupView()
        {
            showMode = (ShowMode)modeDropdown.value;
            characterDropdown.style.display = DisplayStyle.None;
            displayIdField.style.display = DisplayStyle.None;
            imageDropdown.style.display = DisplayStyle.None;
            overrideDisplayIDToggle.style.display = DisplayStyle.None;

            //Create a Dropdown for choosing the type (character, display, icon, sprite)
            switch (showMode)
            {
                case ShowMode.DISPLAY:
                    //Show display ID field
                    displayIdField.style.display = DisplayStyle.Flex;
                    break;
                case ShowMode.CHARACTER:
                    //Show character dropdown
                    characterDropdown.style.display = DisplayStyle.Flex;
                    //Show sprite dropdown
                    imageDropdown.style.display = DisplayStyle.Flex;
                    //Show the override display ID toggle
                    overrideDisplayIDToggle.style.display = DisplayStyle.Flex;

                    if (overrideDisplayIDToggle.value)
                    {
                        //Show display ID field
                        displayIdField.style.display = DisplayStyle.Flex;
                    }
                    break;
            }

            UpdateFoldoutLabel();
        }

        public bool OverridingDisplayID
        {
            get { return overrideDisplayIDToggle.value; }
            set { this.overrideDisplayIDToggle.value = value; }
        }

        private void UpdateFoldoutLabel()
        {
            if(this.ShowMode == ShowMode.DISPLAY)
            {
                if (this.DisplayID != null && this.DisplayID.Length > 0)
                {
                    foldout.text = "Display (" + this.DisplayID + ")";
                }
                else
                {
                    foldout.text = "Display (Unset)";
                }
            }
            else
            {
                if(this.CharacterName != null && this.CharacterName.Length > 0)
                {
                    if(this.ImageID != null && this.ImageID.Length > 0)
                    {
                        foldout.text = "" + this.CharacterName + " (" + this.ImageID +")";
                    }
                    else
                    {
                        foldout.text = "" + this.CharacterName + " (Unset)";
                    }
                }
                else
                {
                    foldout.text = "Character (Unset)";
                }
            }
        }

        private ETDropdownField CreateCharacterDropdown()
        {
            ETDropdownField newCharacterDropdown = new ETDropdownField();
            newCharacterDropdown.RegisterValueChangedCallback(OnCharacterChanged);

            List<string> characterNames = EasyTalkNodeEditor.Instance.GetUniqueCharacterNameList();
            newCharacterDropdown.choices = characterNames;
            return newCharacterDropdown;
        }

        private void OnCharacterChanged(ChangeEvent<string> evt)
        {
            //Update the icon/sprites dropdowns for the chosen character
            UpdateSpriteDropdownChoices();
            UpdateFoldoutLabel();
        }

        private ETTextField CreateDisplayIdTextfield()
        {
            ETTextField displayIdTextField = new ETTextField("Enter Display ID...");
            displayIdTextField.style.flexBasis = new StyleLength(StyleKeyword.Auto);
            displayIdTextField.RegisterValueChangedCallback(OnDisplayIDChanged);
            return displayIdTextField;
        }

        private void OnDisplayIDChanged(ChangeEvent<string> evt)
        {
            UpdateFoldoutLabel();
        }

        private ETDropdownField CreateSpriteDropdown()
        {
            ETDropdownField newImageDropdown = new ETDropdownField();
            newImageDropdown.RegisterValueChangedCallback(OnSpriteChanged);
            return newImageDropdown;
        }

        private void OnSpriteChanged(ChangeEvent<string> evt)
        {
            UpdateFoldoutLabel();
        }

        private void UpdateSpriteDropdownChoices()
        {
            List<CharacterLibrary> charLibs = EasyTalkNodeEditor.Instance.CharacterLibraries;

            foreach (CharacterLibrary charLib in charLibs)
            {
                CharacterDefinition character = charLib.GetCharacterDefinition(CharacterName);

                if (character != null)
                {
                    List<string> spriteIds = new List<string>();
                    foreach (AnimatableDisplayImage sprite in character.PortrayalSprites)
                    {
                        spriteIds.Add(sprite.ID);
                    }

                    imageDropdown.choices = spriteIds;
                    break;
                }
            }
        }

        public bool Expanded
        {
            get { return foldout.value; }
            set {  foldout.value = value; }
        }

        public ETNodeButton RemoveButton
        {
            get { return removeButton; }
        }

        public ShowMode ShowMode 
        { 
            get { return showMode; }
            set 
            {  
                showMode = value;
                modeDropdown.value = showMode;
            }
        }

        public string CharacterName
        {
            get
            {
                if(this.showMode == ShowMode.DISPLAY)
                {
                    return null;
                }

                return characterDropdown.value;
            }
            set
            {
                characterDropdown.value = value;
            }
        }

        public string ImageID
        {
            get
            {
                if(this.showMode == ShowMode.DISPLAY)
                {
                    return null;
                }
                else
                {
                    return imageDropdown.value;
                }
            }
            set
            {
                if(showMode == ShowMode.CHARACTER)
                {
                    imageDropdown.value = value;
                }
            }
        }

        public string DisplayID
        {
            get
            {
                if(showMode == ShowMode.DISPLAY)
                {
                    return displayIdField.value;
                }
                else if(overrideDisplayIDToggle.value)
                {
                    return displayIdField.value;
                }

                return null;
            }
            set
            {
                displayIdField.value = value;
            }
        }
    }
}
