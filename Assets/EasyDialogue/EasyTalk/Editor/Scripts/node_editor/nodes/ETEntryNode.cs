using EasyTalk.Editor.Components;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using System.Diagnostics;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETEntryNode : ETNode
    {
        private ETEntryNodeContent entryContent;

        public ETEntryNode() : base("ENTRY", "entry-node") { }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Entry Node";
            this.connectionLine.AddToClassList("entry-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(110, 75);
        }

        public override void CreateContent()
        {
            entryContent = new ETEntryNodeContent();
            AddContent(entryContent);
        }

        public override Node CreateNode()
        {
            EntryNode entryNode = new EntryNode();
            entryNode.ID = id;
            entryNode.EntryPointName = entryContent.EntryPointName;

            CreateOutputsForNode(entryNode);
            SetSizeForNode(entryNode);
            SetPositionForNode(entryNode);

            return entryNode;
        }

        public override void InitializeFromNode(Node node)
        {
            EntryNode entryNode = node as EntryNode;
            this.id = entryNode.ID;
            SetSizeFromNode(entryNode);
            SetPositionFromNode(entryNode);

            entryContent.EntryPointName = entryNode.EntryPointName;

            InitializeOutputFromConnection(this.Outputs[0], entryNode.Outputs[0]);
        }

        public void ShowEntryPointField()
        {
            entryContent.ShowEntryPointField();
        }

        public void HideEntryPointField()
        {
            entryContent.HideEntryPointField();
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (entryContent.EntryPointName.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Provides and entry point for the dialogue. Calling PlayDialogue(Entry ID) on the controller with the Entry ID as a parameter will cause the dialogue playback to start here.";
        }
    }

    public class ETEntryNodeContent : ETNodeContent
    {
        private ETTextField entryPointNameField;

        public ETEntryNodeContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER) { }

        protected override void CreateContent(VisualElement contentContainer)
        {
            entryPointNameField = new ETTextField(ETTextField.ValidationType.STRING, "Entry ID");
            entryPointNameField.AddToClassList("entry-field");
            //HideEntryPointField();
            contentContainer.Add(entryPointNameField);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public string EntryPointName
        {
            get { return entryPointNameField.value; }
            set { this.entryPointNameField.value = value; }
        }

        public void ShowEntryPointField()
        {
            entryPointNameField.style.display = DisplayStyle.Flex;
        }

        public void HideEntryPointField()
        {
            entryPointNameField.style.display = DisplayStyle.None;
        }
    }
}