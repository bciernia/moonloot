using EasyTalk.Editor.Components;
using EasyTalk.Localization;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETAppendNode : ETNode
    {
        private ETAppendContent appendContent;

        public ETAppendNode() : base("APPEND", "append-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Append Node";
            this.connectionLine.AddToClassList("append-line");
            this.SetDimensions(248, 110);
        }

        public override void CreateContent()
        {
            base.CreateContent();

            appendContent = new ETAppendContent();
            AddContent(appendContent);
        }

        public override Node CreateNode()
        {
            AppendNode appendNode = new AppendNode();
            appendNode.ID = id;
            appendNode.Text = appendContent.Text;
            appendNode.AudioClip = appendContent.AudioClip;
            appendNode.AudioClipFile = appendContent.AudioClipFile;

            CreateInputsForNode(appendNode);
            CreateOutputsForNode(appendNode);
            SetSizeForNode(appendNode);
            SetPositionForNode(appendNode);

            return appendNode;
        }

        public override void InitializeFromNode(Node node)
        {
            AppendNode appendNode = node as AppendNode;
            this.id = appendNode.ID;
            appendContent.Text = appendNode.Text;
            appendContent.AudioClip = appendNode.AudioClip;
            appendContent.AudioClipFile = appendNode.AudioClipFile;

            SetSizeFromNode(appendNode);
            SetPositionFromNode(appendNode);

            InitializeAllInputsAndOutputsFromNode(appendNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (appendContent.Text.ToLower().Contains(text.ToLower())) { return true; }
            if (appendContent.AudioClipFile != null && appendContent.AudioClipFile.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

            if (appendContent.Text.ToString().Length > 0)
            {
                sourceSet.AddOrFindTranslation(appendContent.Text.ToString());
            }
        }

        protected override string GetNodeTooltip()
        {
            return "Append text to the currently displayed conversation.";
        }
    }

    public class ETAppendContent : ETNodeContent
    {
        private ETTextField textField;

        private ETAudioClipZone audioZone;

        protected override void CreateInputs()
        {
            base.CreateInputs();

            AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexDirection = FlexDirection.Row;

            textField = new ETTextField(ETTextField.ValidationType.STRING, "Enter text to append...");
            textField.AddToClassList("conversation-text-input");
            textField.AddToClassList("node-text-area");
            textField.multiline = true;

#if UNITY_2023_1_OR_NEWER

            textField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            textField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif

            contentContainer.Add(textField);

            audioZone = new ETAudioClipZone();
            contentContainer.Add(audioZone);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public string Text 
        { 
            get { return textField.value; } 
            set { textField.value = value; }
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
