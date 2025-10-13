using EasyTalk.Editor.Components;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Variable;
using EasyTalk.Settings;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETVariableNode : ETNode
    {
        private NodeVariableType variableType;

        private ETVariableNodeContent content;

        public delegate void OnVariableNameChanged(string oldVariableName, string newVariableName);

        public OnVariableNameChanged onVariableNameChanged;

        public ETVariableNode(NodeVariableType variableType) : base(variableType.ToString().ToUpper(), variableType.ToString().ToLower() + "-variable-node", false)
        {
            this.variableType = variableType;
            CreateContent();
        }

        public NodeVariableType VariableType
        {
            get { return variableType; }
        }

        public string VariableName
        {
            get { return content.VariableName; }
        }

        public string Value
        {
            get { return content.Value; }
        }

        public bool ResetOnEntry
        {
            get { return content.ResetOnEntry; }
        }

        public bool IsGlobal
        {
            get { return content.IsGlobal; }
        }

        protected override void Initialize()
        {
            base.Initialize();

            string prefix = this.variableType.ToString().ToLower();
            this.name = prefix + " Variable Node";
            this.connectionLine.AddToClassList(prefix + "-variable-line");
            this.resizeType = ResizeType.NONE;
            this.SetDimensions(170, 150);
        }

        public override void CreateContent()
        {
            content = new ETVariableNodeContent(this.variableType);
            content.style.paddingBottom = 4.0f;
            content.style.paddingLeft = 8.0f;
            AddContent(content);

            content.VariableNameField.RegisterCallback<InputEvent>(VariableNameChanged);
            content.VariableNameField.RegisterCallback<ChangeEvent<string>>(VariableNameChanged);
        }

        public void VariableNameChanged(InputEvent evt)
        {
            if (IsGlobal)
            {
                UpdateRegistry(evt.previousData, evt.newData);
            }

            if (onVariableNameChanged != null)
            {
                onVariableNameChanged(evt.previousData, evt.newData);
            }
        }

        public void VariableNameChanged(ChangeEvent<string> evt)
        {
            if (IsGlobal)
            {
                UpdateRegistry(evt.previousValue, evt.newValue);
            }

            if (onVariableNameChanged != null)
            {
                onVariableNameChanged(evt.previousValue, evt.newValue);
            }
        }

        private void UpdateRegistry(string oldName, string newName)
        {
            if (EasyTalkNodeEditor.Instance.EditorSettings != null)
            {
                DialogueRegistry registry = EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry;

                if (registry != null)
                {
                    foreach (GlobalNodeVariable globalVariable in registry.GlobalVariables)
                    {
                        if (globalVariable.VariableName.Equals(oldName))
                        {
                            globalVariable.VariableName = newName;
                            EditorUtility.SetDirty(EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry);
                            break;

                        }
                    }
                }
            }
        }

        public override Node CreateNode()
        {
            VariableNode variableNode = new VariableNode();
            variableNode.ID = id;

            switch (variableType)
            {
                case NodeVariableType.FLOAT: variableNode.NodeType = NodeType.FLOAT_VARIABLE; break;
                case NodeVariableType.INT: variableNode.NodeType = NodeType.INT_VARIABLE; break;
                case NodeVariableType.STRING: variableNode.NodeType = NodeType.STRING_VARIABLE; break;
                case NodeVariableType.BOOL: variableNode.NodeType = NodeType.BOOL_VARIABLE; break;
            }

            CreateOutputsForNode(variableNode);
            SetSizeForNode(variableNode);
            SetPositionForNode(variableNode);

            variableNode.VariableName = content.VariableName;
            variableNode.ResetOnEntry = content.ResetOnEntry;
            variableNode.IsGlobal = content.IsGlobal;

            if (variableType == NodeVariableType.BOOL)
            {
                variableNode.VariableValue = content.BoolValue.ToString();
            }
            else
            {
                variableNode.VariableValue = content.Value;
            }

            return variableNode;
        }

        public override void InitializeFromNode(Node node)
        {
            VariableNode variableNode = node as VariableNode;
            this.id = variableNode.ID;
            SetSizeFromNode(variableNode);
            SetPositionFromNode(variableNode);

            content.VariableName = variableNode.VariableName;
            content.ResetOnEntry = variableNode.ResetOnEntry;
            content.IsGlobal = variableNode.IsGlobal;

            if (variableNode.NodeType == NodeType.BOOL_VARIABLE)
            {
                bool boolValue = true;
                bool.TryParse(variableNode.VariableValue, out boolValue);
                content.BoolValue = boolValue;
            }
            else
            {
                content.Value = variableNode.VariableValue;
            }

            InitializeAllInputsAndOutputsFromNode(variableNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            try { if (VariableType.ToString().ToLower().Contains(text.ToLower())) { return true; } } catch { }
            try { if (VariableName.ToString().ToLower().Contains(text.ToLower())) { return true; } } catch { }
            try { if (Value.ToString().ToLower().Contains(text.ToLower())) { return true; } } catch { }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            /*if (content.VariableType == NodeVariableType.STRING)
            {
                TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

                if (content.Value.ToString().Length > 0)
                {
                    sourceSet.AddOrFindTranslation(content.Value.ToString());
                }
            }*/
        }

        protected override string GetNodeTooltip()
        {
            return "Declares a variable for use in the Dialogue.";
        }
    }

    public class ETVariableNodeContent : ETNodeContent
    {
        private NodeVariableType variableType;

        private static int variableIdx = 1;

        private ETTextField variableNameField;

        private ETTextField valueField;

        private ETToggle resetOnEntryToggle;

        private ETToggle isGlobalToggle;

        private ETEnumField boolDropdown;

        private Box resetBox;

        private Box globalBox;

        public ETVariableNodeContent(NodeVariableType variableType) : base(NodeAlignment.TOP, NodeAlignment.BOTTOM, false)
        {
            this.variableType = variableType;
            this.Layout();
        }

        public NodeVariableType VariableType
        {
            get { return variableType; }
        }

        public string VariableName
        {
            get { return variableNameField.text; }
            set { variableNameField.value = value; }
        }

        public TextField VariableNameField
        {
            get { return variableNameField; }
        }

        public string Value
        {
            get { return valueField.text; }
            set { valueField.value = value; }
        }

        public bool BoolValue
        {
            get
            {
                switch ((BoolOption)boolDropdown.value)
                {
                    case BoolOption.TRUE: return true;
                    case BoolOption.FALSE: return false;
                }

                return false;
            }
            set
            {
                if (value)
                {
                    boolDropdown.value = BoolOption.TRUE;
                }
                else
                {
                    boolDropdown.value = BoolOption.FALSE;
                }
            }
        }

        public bool ResetOnEntry
        {
            get { return resetOnEntryToggle.value; }
            set { resetOnEntryToggle.value = value; }
        }

        public bool IsGlobal
        {
            get { return isGlobalToggle.value; }
            set { isGlobalToggle.value = value; }
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            contentContainer.style.justifyContent = Justify.FlexStart;

            variableNameField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Variable Name...");
            variableNameField.AddToClassList("variable-text-input");
            variableNameField.value = "myVariable" + variableIdx;
            variableNameField.HidePlaceholder();
            contentContainer.Add(variableNameField);
            variableIdx++;

            if (variableType == NodeVariableType.BOOL)
            {
                boolDropdown = new ETEnumField(BoolOption.TRUE);
                boolDropdown.RegisterCallback<ChangeEvent<Enum>>(OnBoolChanged);
                boolDropdown.AddToClassList("bool-enum-field");
                contentContainer.Add(boolDropdown);
            }
            else
            {
                valueField = new ETTextField("Enter Value...");
                valueField.AddToClassList("variable-text-input");

                switch (variableType)
                {
                    case NodeVariableType.STRING:
                        //valueField.value = "myString";
                        valueField.SetValidationType(ETTextField.ValidationType.STRING);
                        break;
                    case NodeVariableType.INT:
                        //valueField.value = "" + 0;
                        valueField.SetValidationType(ETTextField.ValidationType.INT);
                        break;
                    case NodeVariableType.FLOAT:
                        //valueField.value = "" + 3.1415f;
                        valueField.SetValidationType(ETTextField.ValidationType.FLOAT);
                        break;
                }

                valueField.RegisterCallback<ChangeEvent<string>>(OnValueFieldChanged);
                contentContainer.Add(valueField);
            }

            contentContainer.Add(CreateResetOnEntryToggle());
            contentContainer.Add(CreateGlobalVariableToggle());
        }

        private void OnValueFieldChanged(ChangeEvent<string> evt)
        {
            UpdateGlobalVariableInitialValue();
        }

        private Box CreateResetOnEntryToggle()
        {
            resetBox = new Box();
            resetBox.style.flexDirection = FlexDirection.RowReverse;
            resetBox.style.alignSelf = Align.FlexStart;

            resetOnEntryToggle = new ETToggle();
            resetOnEntryToggle.value = true;
            resetOnEntryToggle.RegisterCallback<ChangeEvent<bool>>(OnResetToggleChanged);

            Label resetLabel = new Label("Reset on Entry?");
            resetLabel.AddToClassList("reset-variable-label");
            resetBox.Add(resetLabel);
            resetBox.Add(resetOnEntryToggle);
            return resetBox;
        }

        private Box CreateGlobalVariableToggle()
        {
            globalBox = new Box();
            globalBox.style.flexDirection = FlexDirection.RowReverse;
            globalBox.style.alignSelf = Align.FlexStart;

            isGlobalToggle = new ETToggle();
            isGlobalToggle.value = false;
            isGlobalToggle.RegisterCallback<ChangeEvent<bool>>(OnGlobalToggleChanged);

            Label resetLabel = new Label("Is Global?");
            resetLabel.AddToClassList("reset-variable-label");
            globalBox.Add(resetLabel);
            globalBox.Add(isGlobalToggle);
            return globalBox;
        }

        private void OnBoolChanged(ChangeEvent<Enum> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
            UpdateGlobalVariableInitialValue();
        }

        private void UpdateGlobalVariableInitialValue()
        {
            if (IsGlobal)
            {
                if (EasyTalkNodeEditor.Instance.EditorSettings != null)
                {
                    DialogueRegistry registry = EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry;

                    if (registry != null)
                    {
                        int idx = registry.FindVariable(VariableName);
                        if (idx > -1)
                        {
                            registry.GlobalVariables[idx].InitialValue = GetInitialValue().ToString();
                        }
                    }
                }
            }
        }

        private void OnResetToggleChanged(ChangeEvent<bool> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        private void OnGlobalToggleChanged(ChangeEvent<bool> evt)
        {
            if (EasyTalkNodeEditor.Instance.EditorSettings != null)
            {
                DialogueRegistry registry = EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry;

                if (registry != null)
                {
                    int foundIdx = registry.FindVariable(VariableName);

                    if (IsGlobal)
                    {
                        GlobalNodeVariable globalVar = new GlobalNodeVariable();
                        globalVar.VariableName = VariableName;
                        globalVar.VariableType = GetGlobalVariableType();
                        globalVar.InitialValue = GetInitialValue().ToString();

                        if (foundIdx < 0)
                        {
                            registry.GlobalVariables.Add(globalVar);
                        }
                        else
                        {
                            registry.GlobalVariables[foundIdx] = globalVar;
                        }

                        EditorUtility.SetDirty(EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry);
                    }
                    else //If the global toggle was unchecked, remove the variable from the registry
                    {
                        if (foundIdx >= 0)
                        {
                            registry.GlobalVariables.RemoveAt(foundIdx);
                            EditorUtility.SetDirty(EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry);
                        }
                    }
                }
            }

            if (isGlobalToggle.value)
            {
                resetOnEntryToggle.value = false;
                resetBox.style.display = DisplayStyle.None;
            }
            else
            {
                resetBox.style.display = DisplayStyle.Flex;
            }

            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        private object GetInitialValue()
        {
            switch (variableType)
            {
                case NodeVariableType.BOOL: return BoolValue;
                case NodeVariableType.FLOAT: 
                    float floatValue = 0.0f; 
                    float.TryParse(Value, out floatValue);
                    return floatValue;
                case NodeVariableType.INT: 
                    int intValue = 0; 
                    int.TryParse(Value, out intValue);
                    return intValue;
                case NodeVariableType.STRING: return Value;
            }

            return null;
        }

        private GlobalVariableType GetGlobalVariableType()
        {
            switch (variableType)
            {
                case NodeVariableType.BOOL: return GlobalVariableType.BOOL;
                case NodeVariableType.FLOAT: return GlobalVariableType.FLOAT;
                case NodeVariableType.INT: return GlobalVariableType.INT;
                case NodeVariableType.STRING: return GlobalVariableType.STRING;
            }

            return GlobalVariableType.STRING;
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();

            InputOutputType inputOutputType = InputOutputType.BOOL;

            switch (variableType)
            {
                case NodeVariableType.BOOL: inputOutputType = InputOutputType.BOOL; break;
                case NodeVariableType.FLOAT: inputOutputType = InputOutputType.FLOAT; break;
                case NodeVariableType.INT: inputOutputType = InputOutputType.INT; break;
                case NodeVariableType.STRING: inputOutputType = InputOutputType.STRING; break;
            }

            this.AddOutput(inputOutputType);
        }
    }

    public enum BoolOption
    {
        TRUE, FALSE
    }

    public enum NodeVariableType
    {
        NONE, BOOL, FLOAT, INT, STRING
    }
}