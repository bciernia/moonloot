using EasyTalk.Editor.Components;
using EasyTalk.Nodes;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using EasyTalk.Nodes.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETGotoNode : ETNode
    {
        private ETGotoNodeDialogueContent dialogueContent;

        public ETGotoNode() : base("GOTO", "goto-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Goto Node";
            this.connectionLine.AddToClassList("goto-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(168, 96);
        }

        public override float GetMinimumHeight()
        {
            return 105.0f;
        }

        public override void CreateContent()
        {
            dialogueContent = new ETGotoNodeDialogueContent();
            AddContent(dialogueContent);
        }

        public override Node CreateNode()
        {
            GotoNode gotoNode = new GotoNode();
            gotoNode.ID = id;

            CreateInputsForNode(gotoNode);
            CreateOutputsForNode(gotoNode);
            SetSizeForNode(gotoNode);
            SetPositionForNode(gotoNode);

            gotoNode.Dialogue = dialogueContent.Dialogue;
            gotoNode.EntryID = dialogueContent.EntryID;

            return gotoNode;
        }

        public override void InitializeFromNode(Node node)
        {
            GotoNode gotoNode = node as GotoNode;
            this.id = gotoNode.ID;

            dialogueContent.Dialogue = gotoNode.Dialogue;
            dialogueContent.EntryID = gotoNode.EntryID;

            SetPositionFromNode(gotoNode);
            SetSizeFromNode(gotoNode);
            InitializeAllInputsAndOutputsFromNode(gotoNode);
        }

        protected override string GetNodeTooltip()
        {
            return "Loads and enters into another Dialogue asset at the specified entry point.";
        }
    }

    public class ETGotoNodeInputContent : ETNodeContent
    {
        protected override void CreateInputs()
        {
            base.CreateInputs();

            AddInput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETGotoNodeDialogueContent : ETNodeContent
    {
        private Dialogue dialogue;

        private ETDropdownField dropdownField;

        private ETTextField entryIdTextField;

        protected override void CreateInputs()
        {
            base.CreateInputs();
            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexGrow = 0;
            contentContainer.style.justifyContent = Justify.FlexStart;

            dropdownField = new ETDropdownField();
            dropdownField.AddToClassList("goto-dropdown");
            dropdownField.style.paddingLeft = 4;
            dropdownField.style.paddingRight = 4;
            dropdownField.style.alignSelf = Align.Stretch;

            string[] assetGuids = AssetDatabase.FindAssets("t:Dialogue");
            List<string> paths = new List<string>();
            foreach (string assetId in assetGuids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(assetId));
            }
            dropdownField.choices = paths;
            dropdownField.RegisterValueChangedCallback(DialoguePathChanged);
            contentContainer.Add(dropdownField);

            //Add a text field for the entry point ID
            entryIdTextField = new ETTextField("Enter Entry ID...");
            entryIdTextField.AddToClassList("goto-text-input");
            entryIdTextField.style.paddingLeft = 4;
            entryIdTextField.style.paddingRight = 4;
            contentContainer.Add(entryIdTextField);
        }

        private void DialoguePathChanged(ChangeEvent<string> evt)
        {
            this.dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(evt.newValue);
            dropdownField.tooltip = evt.newValue;
        }

        public Dialogue Dialogue
        {
            get { return this.dialogue; }
            set 
            {
                this.dialogue = value;
                dropdownField.value = AssetDatabase.GetAssetPath(this.dialogue);
                dropdownField.tooltip = dropdownField.value;
            }
        }

        public string EntryID
        {
            get { return this.entryIdTextField.value; }
            set { this.entryIdTextField.value = value; }
        }
    }

    public enum DialogueSelectionMode
    {
        SELECT_PATH, ASSET_CHOOSER, FILENAME_LIST 
    }
}
