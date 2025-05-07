using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Variable;
using EasyTalk.Settings;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETSetVariableNode : ETNode
    {
        private ETSetVariableContent content;

        public SetVariableNode storedNodeState;

        public ETSetVariableNode() : base("SET VARIABLE", "set-variable-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Set Variable Node";
            this.connectionLine.AddToClassList("set-variable-line");
            this.resizeType = ResizeType.HORIZONTAL;
            this.SetDimensions(200, 100);
        }

        public override void CreateContent()
        {
            content = new ETSetVariableContent();
            AddContent(content);

            content.EnableValueChangeCallback();
            AddNodeChangeCallbacks();
        }

        public override Node CreateNode()
        {
            SetVariableNode setVariableNode = new SetVariableNode();
            setVariableNode.ID = id;

            CreateInputsForNode(setVariableNode);
            CreateOutputsForNode(setVariableNode);
            SetSizeForNode(setVariableNode);
            SetPositionForNode(setVariableNode);

            setVariableNode.VariableName = content.VariableName;

            if (content.HasIncomingConnection())
            {
                setVariableNode.VariableValue = "";
            }
            else
            {
                setVariableNode.VariableValue = content.VariableValue;
            }

            return setVariableNode;
        }

        public override void InitializeFromNode(Node node)
        {
            SetVariableNode setVariableNode = node as SetVariableNode;
            this.id = setVariableNode.ID;
            SetSizeFromNode(setVariableNode);
            SetPositionFromNode(setVariableNode);

            content.DisableValueChangeCallback();
            content.VariableName = setVariableNode.VariableName;
            EditorApplication.delayCall += delegate { content.VariableValue = setVariableNode.VariableValue; };
            content.EnableValueChangeCallback();

            InitializeAllInputsAndOutputsFromNode(setVariableNode);

            UpdateStoredNodeState();
        }

        private void AddNodeChangeCallbacks()
        {
            foreach (ETInput input in Inputs)
            {
                input.onConnectionCreated += ConnectionCreatedOrDeleted;
                input.onConnectionDeleted += ConnectionCreatedOrDeleted;
            }

            foreach (ETOutput output in Outputs)
            {
                output.onConnectionCreated += ConnectionCreatedOrDeleted;
                output.onConnectionDeleted += ConnectionCreatedOrDeleted;
            }
        }

        protected void VariableSelectionChanged(ChangeEvent<string> evt)
        {
            UpdateStoredNodeState();
        }

        public void UpdateStoredNodeState()
        {
            storedNodeState = CreateNode() as SetVariableNode;
        }

        public override void NodeMoved()
        {
            UpdateStoredNodeState();
        }

        public override void NodeResized()
        {
            UpdateStoredNodeState();
        }

        private void ConnectionCreatedOrDeleted(int inputId, int outputId)
        {
            UpdateStoredNodeState();
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (content.VariableName.ToLower().Contains(text.ToLower())) { return true; }
            if (content.VariableValue.ToString().ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        public override void CreateLocalizations(TranslationLibrary library)
        {
            /*TranslationSet sourceSet = library.GetOrCreateOriginalTranslationSet();

            if (content.GetInputs()[1].ConnectedOutputs.Count == 0 && content.VariableValue.ToString().Length > 0)
            {
                sourceSet.AddOrFindTranslation(content.VariableValue.ToString());
            }*/
        }

        protected override string GetNodeTooltip()
        {
            return "Used to set the value of a dialogue variable.";
        }
    }

    public class ETSetVariableContent : ETNodeContent
    {
        private ETDropdownField variableNameDropdown;

        private ETInput valueInput;

        private ETTextField valueTextField;

        private Label valueLabel;

        private bool hasVariables = true;

        public ETSetVariableContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM)
        {
            EasyTalkNodeEditor.Instance.NodeView.onVariableNameChanged += VariableNameChanged;
            EasyTalkNodeEditor.Instance.NodeView.onVariableAdded += VariableAdded;
            EasyTalkNodeEditor.Instance.NodeView.onVariableRemoved += VariableRemoved;
        }

        public string VariableValue
        {
            get { return valueTextField.text; }
            set { valueTextField.value = value; }
        }

        public bool HasIncomingConnection()
        {
            if (valueInput.ConnectedOutputs.Count > 0)
            {
                return true;
            }

            return false;
        }

        public string VariableName
        {
            get { return this.variableNameDropdown.value; }
            set { variableNameDropdown.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
            valueInput = this.AddInput(InputOutputType.VALUE);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexDirection = FlexDirection.Column;
            contentContainer.style.height = new StyleLength(StyleKeyword.Auto);
            contentContainer.style.alignSelf = Align.FlexStart;

            variableNameDropdown = new ETDropdownField();
            variableNameDropdown.PublishUndoableActions = false;
            variableNameDropdown.AddToClassList("set-variable-dropdown");
            List<string> variableNames = EasyTalkNodeEditor.Instance.NodeView.GetVariableNames();
            variableNameDropdown.choices = variableNames;

            if (variableNameDropdown.choices.Count == 0)
            {
                variableNameDropdown.choices = new List<string>() { "No Variables" };
                hasVariables = false;
            }

            contentContainer.Add(variableNameDropdown);

            valueTextField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Value...");
            valueTextField.AddToClassList("set-variable-input");
            valueTextField.style.marginTop = 4.0f;
            contentContainer.Add(valueTextField);

            valueLabel = new Label("Value");
            valueLabel.AddToClassList("set-variable-label");
            valueLabel.style.display = DisplayStyle.None;
            valueLabel.style.marginTop = 8.0f;
            contentContainer.Add(valueLabel);
        }

        public void EnableValueChangeCallback()
        {
            variableNameDropdown.RegisterCallback<ChangeEvent<string>>(OnChoiceChanged);
        }

        public void DisableValueChangeCallback()
        {
            variableNameDropdown.UnregisterCallback<ChangeEvent<string>>(OnChoiceChanged);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public override void Layout()
        {
            base.Layout();
            variableNameDropdown.value = variableNameDropdown.choices[0];
            UpdateInputForChoice(variableNameDropdown.value);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            if (inputId == valueInput.ID)
            {
                valueTextField.style.display = DisplayStyle.None;
                valueLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            if (inputId == valueInput.ID)
            {
                valueTextField.style.display = DisplayStyle.Flex;
                valueLabel.style.display = DisplayStyle.None;
            }
        }

        private void OnChoiceChanged(ChangeEvent<string> evt)
        {
            ETSetVariableNode node = (ETUtils.FindNodeParent(this) as ETSetVariableNode);
            SetVariableNode storedNodeState = node.storedNodeState;

            EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(node.ID, storedNodeState, "Changed SetVariable Node Variable Selection");
            UpdateInputForChoice(evt.newValue);
            EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Changed SetVariable Node Variable Selection");
        }

        private void UpdateInputForChoice(string choice)
        {
            bool usingRegistryVariable = false;

            //Determine the value type of the variable
            DialogueRegistry registry = EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry;
            if (registry != null)
            {
                int idx = registry.FindVariable(choice);
                if (idx >= 0)
                {
                    usingRegistryVariable = true;
                    InputOutputType inputType = valueInput.InputType;
                    GlobalNodeVariable globalVar = registry.GlobalVariables[idx];
                    InputOutputType currentInputType = valueInput.InputType;

                    switch (globalVar.VariableType)
                    {
                        case GlobalVariableType.INT:
                            valueInput.SetInputType(InputOutputType.NUMBER);
                            valueTextField.SetValidationType(ETTextField.ValidationType.INT);
                            break;
                        case GlobalVariableType.STRING:
                            valueInput.SetInputType(InputOutputType.STRING);
                            valueTextField.SetValidationType(ETTextField.ValidationType.STRING);
                            break;
                        case GlobalVariableType.FLOAT:
                            valueInput.SetInputType(InputOutputType.NUMBER);
                            valueTextField.SetValidationType(ETTextField.ValidationType.FLOAT);
                            break;
                        case GlobalVariableType.BOOL:
                            valueInput.SetInputType(InputOutputType.BOOL);
                            valueTextField.SetValidationType(ETTextField.ValidationType.BOOL);
                            break;
                    }

                    if (valueInput.InputType != currentInputType)
                    {
                        EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(valueInput);
                        valueInput.MarkDirtyRepaint();
                    }
                }
            }

            if (!usingRegistryVariable)
            {
                //Determine the value type of the variable
                ETVariableNode node = EasyTalkNodeEditor.Instance.NodeView.GetVariableNode(choice);
                InputOutputType currentInputType = valueInput.InputType;

                if (node != null)
                {
                    switch (node.VariableType)
                    {
                        case NodeVariableType.INT:
                            valueInput.SetInputType(InputOutputType.NUMBER);
                            valueTextField.SetValidationType(ETTextField.ValidationType.INT);
                            break;
                        case NodeVariableType.STRING:
                            valueInput.SetInputType(InputOutputType.STRING);
                            valueTextField.SetValidationType(ETTextField.ValidationType.STRING);
                            break;
                        case NodeVariableType.FLOAT:
                            valueInput.SetInputType(InputOutputType.NUMBER);
                            valueTextField.SetValidationType(ETTextField.ValidationType.FLOAT);
                            break;
                        case NodeVariableType.BOOL:
                            valueInput.SetInputType(InputOutputType.BOOL);
                            valueTextField.SetValidationType(ETTextField.ValidationType.BOOL);
                            break;
                    }

                    if (valueInput.InputType != currentInputType)
                    {
                        EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(valueInput);
                        valueInput.MarkDirtyRepaint();
                    }
                }
            }

            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        public void VariableAdded(string variableName)
        {
            if (!hasVariables)
            {
                variableNameDropdown.choices.Clear();
            }

            variableNameDropdown.choices.Add(variableName);
            variableNameDropdown.choices.Sort();

            if (!hasVariables)
            {
                variableNameDropdown.value = variableNameDropdown.choices[0];
                hasVariables = true;
            }
        }

        public void VariableRemoved(string variableName)
        {
            if (variableNameDropdown.choices.Contains(variableName))
            {
                variableNameDropdown.choices.Remove(variableName);
            }

            if (variableNameDropdown.value.Equals(variableName))
            {
                if (variableNameDropdown.choices.Count > 0)
                {
                    variableNameDropdown.value = variableNameDropdown.choices[0];
                }
            }

            if (variableNameDropdown.choices.Count == 0)
            {
                variableNameDropdown.choices = new List<string>() { "No Variables" };
                variableNameDropdown.value = variableNameDropdown.choices[0];
                valueInput.SetInputType(InputOutputType.VALUE);
                hasVariables = false;
            }

            variableNameDropdown.choices.Sort();
        }

        public void VariableNameChanged(string oldVariableName, string newVariableName)
        {
            string oldChosenVariable = variableNameDropdown.value;

            int oldIdx = variableNameDropdown.choices.IndexOf(oldVariableName);
            if (oldIdx > -1)
            {
                variableNameDropdown.choices[oldIdx] = newVariableName;
            }

            variableNameDropdown.choices.Sort();

            //variableNameDropdown.choices = NMGUI.Instance.GetEditor().NodeView.GetVariableNames();

            if (oldChosenVariable.Equals(oldVariableName))
            {
                //If the old chosen name matches the changed variable's old name, set the value to the new name
                variableNameDropdown.value = newVariableName;
            }
            else
            {
                //If the old chosen variable name does not match the changed variable's old name, reset to the previously chosen name
                variableNameDropdown.value = oldChosenVariable;
            }
        }
    }
}