using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Utility;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETHideNode : ETListNode
    {
        private ETHideNodeInputContent inputContent;
        private ETHideNodeOutputContent outputContent;

        public ETHideNode() : base("HIDE", "display-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Hide Node";
            this.connectionLine.AddToClassList("display-line");
            this.SetDimensions(188, 170);
        }

        public override float GetMinimumHeight()
        {
            return 120.0f;
        }

        public override void CreateContent()
        {
            inputContent = new ETHideNodeInputContent();
            AddContent(inputContent);

            CreateListPanel(ETListNodeType.SCROLL);
            AddItemToList();

            outputContent = new ETHideNodeOutputContent();
            AddContent(outputContent);
        }

        public override Node CreateNode()
        {
            HideNode showNode = new HideNode();
            showNode.ID = id;

            CreateInputsForNode(showNode);
            CreateOutputsForNode(showNode);
            SetSizeForNode(showNode);
            SetPositionForNode(showNode);

            foreach (ETHideNodeItem item in this.Items)
            {
                HideNodeItem showNodeItem = new HideNodeItem(item.HideMode, item.CharacterName, item.DisplayID);
                showNodeItem.IsExpanded = item.Expanded;
                showNode.AddItem(showNodeItem);
            }

            return showNode;
        }

        public override void InitializeFromNode(Node node)
        {
            HideNode showNode = node as HideNode;
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
                HideNodeItem hideItem = (HideNodeItem)showNode.Items[i];
                ETHideNodeItem nodeItem = ((ETHideNodeItem)Items[i]);

                if (hideItem != null)
                {
                    nodeItem.HideMode = hideItem.HideMode;
                    nodeItem.CharacterName = hideItem.CharacterName;
                    nodeItem.DisplayID = hideItem.DisplayID;
                    nodeItem.Expanded = hideItem.IsExpanded;
                }
            }

            InitializeAllInputsAndOutputsFromNode(showNode);
        }

        protected override ETNodeContent CreateNewItem()
        {
            ETHideNodeItem item = new ETHideNodeItem();
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
            return "Hides one or more displays or character images before continuing.";
        }
    }

    public class ETHideNodeInputContent : ETNodeContent
    {
        public ETHideNodeInputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETHideNodeOutputContent : ETNodeContent
    {
        public ETHideNodeOutputContent() : base()
        {
            this.style.flexGrow = 0;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETHideNodeItem : ETNodeContent
    {
        private Foldout foldout;
        private HideMode hideMode;
        private ETNodeButton removeButton;

        private ETEnumField modeDropdown;
        private ETDropdownField characterDropdown;
        private ETTextField displayIdField;

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
            modeDropdown = new ETEnumField(hideMode);
            modeDropdown.AddToClassList("display-dropdown");
            modeDropdown.RegisterCallback<ChangeEvent<System.Enum>>(ModeChanged);
            foldout.Add(modeDropdown);

            characterDropdown = CreateCharacterDropdown();
            characterDropdown.AddToClassList("display-dropdown");
            foldout.Add(characterDropdown);

            displayIdField = CreateDisplayIdDropdown();
            displayIdField.AddToClassList("display-text-input");
            foldout.Add(displayIdField);

            SetupView();
            contentContainer.Add(foldout);
        }

        private void ModeChanged(ChangeEvent<System.Enum> evt)
        {
            SetupView();
        }

        private void SetupView()
        {
            hideMode = (HideMode)modeDropdown.value;
            characterDropdown.style.display = DisplayStyle.None;
            displayIdField.style.display = DisplayStyle.None;

            //Create a Dropdown for choosing the type (character, display, icon, sprite)
            switch (hideMode)
            {
                case HideMode.DISPLAY:
                    //Show display ID field
                    displayIdField.style.display = DisplayStyle.Flex;
                    break;
                case HideMode.CHARACTER:
                    //Show character dropdown
                    characterDropdown.style.display = DisplayStyle.Flex;
                    break;
            }

            UpdateFoldoutLabel();
        }

        private void UpdateFoldoutLabel()
        {
            if (this.HideMode == HideMode.DISPLAY)
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
                if (this.CharacterName != null && this.CharacterName.Length > 0)
                {
                    foldout.text = "" + this.CharacterName;
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
            UpdateFoldoutLabel();
        }

        private ETTextField CreateDisplayIdDropdown()
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

        public bool Expanded
        {
            get { return foldout.value; }
            set { foldout.value = value; }
        }

        public ETNodeButton RemoveButton
        {
            get { return removeButton; }
        }

        public HideMode HideMode
        {
            get { return hideMode; }
            set
            {
                hideMode = value;
                modeDropdown.value = hideMode;
            }
        }

        public string CharacterName
        {
            get
            {
                if (this.hideMode == HideMode.DISPLAY)
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

        public string DisplayID
        {
            get
            {
                if (hideMode == HideMode.DISPLAY)
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
