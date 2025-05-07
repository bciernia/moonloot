using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Variable;
using EasyTalk.Settings;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETGetVariableNode : ETNode
    {
        private ETGetVariableContent content;

        public ETGetVariableNode() : base("GET VARIABLE", "get-variable-node") { }

        protected override void Initialize()
        {
            base.Initialize();
            this.name = "Get Variable Node";
            this.connectionLine.AddToClassList("get-variable-line");
            this.resizeType = ResizeType.NONE;
            this.SetDimensions(200, 100);
        }

        public override void CreateContent()
        {
            content = new ETGetVariableContent();
            AddContent(content);
        }

        public override Node CreateNode()
        {
            GetVariableNode getVariableNode = new GetVariableNode();
            getVariableNode.ID = id;

            CreateInputsForNode(getVariableNode);
            CreateOutputsForNode(getVariableNode);
            SetSizeForNode(getVariableNode);
            SetPositionForNode(getVariableNode);

            getVariableNode.VariableName = content.VariableName;

            return getVariableNode;
        }

        public override void InitializeFromNode(Node node)
        {
            GetVariableNode getVariableNode = node as GetVariableNode;
            this.id = getVariableNode.ID;
            SetSizeFromNode(getVariableNode);
            SetPositionFromNode(getVariableNode);

            content.VariableName = getVariableNode.VariableName;

            InitializeAllInputsAndOutputsFromNode(getVariableNode);
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (content.VariableName.ToLower().Contains(text.ToLower())) { return true; }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Retrieves the value of a dialogue variable and sends it to the output.";
        }
    }

    public class ETGetVariableContent : ETNodeContent
    {
        private ETDropdownField variableNameDropdown;

        private ETOutput valueOutput;

        private bool hasVariables = true;

        public ETGetVariableContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM)
        {
            EasyTalkNodeEditor.Instance.NodeView.onVariableNameChanged += VariableNameChanged;
            EasyTalkNodeEditor.Instance.NodeView.onVariableAdded += VariableAdded;
            EasyTalkNodeEditor.Instance.NodeView.onVariableRemoved += VariableRemoved;
        }

        public string VariableName
        {
            get { return this.variableNameDropdown.value; }
            set { this.variableNameDropdown.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.flexGrow = 0;
            contentContainer.style.alignSelf = Align.FlexStart;

            variableNameDropdown = new ETDropdownField();
            variableNameDropdown.AddToClassList("get-variable-dropdown");

            List<string> variableNames = EasyTalkNodeEditor.Instance.NodeView.GetVariableNames();
            variableNameDropdown.choices = variableNames;
            variableNameDropdown.RegisterCallback<ChangeEvent<string>>(OnChoiceChanged);

            if (variableNameDropdown.choices.Count == 0)
            {
                variableNameDropdown.choices = new List<string>() { "No Variables" };
                hasVariables = false;
            }

            contentContainer.Add(variableNameDropdown);
        }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            valueOutput = this.AddOutput(InputOutputType.VALUE);
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }

        public override void Layout()
        {
            base.Layout();
            variableNameDropdown.value = variableNameDropdown.choices[0];
            UpdateOutputForChoice(variableNameDropdown.value);
        }

        private void OnChoiceChanged(ChangeEvent<string> evt)
        {
            UpdateOutputForChoice(evt.newValue);
        }

        private void UpdateOutputForChoice(string choice)
        {
            bool usingRegistryVariable = false;

            //Determine the value type of the variable
            DialogueRegistry registry = EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry;
            if(registry != null)
            {
                int idx = registry.FindVariable(choice);
                if(idx >= 0)
                {
                    usingRegistryVariable = true;
                    InputOutputType currentOutputType = valueOutput.OutputType;
                    GlobalNodeVariable globalVar = registry.GlobalVariables[idx];

                    switch (globalVar.VariableType)
                    {
                        case GlobalVariableType.INT: valueOutput.SetOutputType(InputOutputType.INT); break;
                        case GlobalVariableType.STRING: valueOutput.SetOutputType(InputOutputType.STRING); break;
                        case GlobalVariableType.FLOAT: valueOutput.SetOutputType(InputOutputType.FLOAT); break;
                        case GlobalVariableType.BOOL: valueOutput.SetOutputType(InputOutputType.BOOL); break;
                    }

                    if (valueOutput.OutputType != currentOutputType)
                    {
                        valueOutput.DisconnectAll();
                        valueOutput.MarkDirtyRepaint();
                    }
                }
            }

            if (!usingRegistryVariable)
            {
                ETVariableNode node = EasyTalkNodeEditor.Instance.NodeView.GetVariableNode(choice);
                InputOutputType currentOutputType = valueOutput.OutputType;

                if (node != null)
                {
                    switch (node.VariableType)
                    {
                        case NodeVariableType.INT: valueOutput.SetOutputType(InputOutputType.INT); break;
                        case NodeVariableType.STRING: valueOutput.SetOutputType(InputOutputType.STRING); break;
                        case NodeVariableType.FLOAT: valueOutput.SetOutputType(InputOutputType.FLOAT); break;
                        case NodeVariableType.BOOL: valueOutput.SetOutputType(InputOutputType.BOOL); break;
                    }

                    if (valueOutput.OutputType != currentOutputType)
                    {
                        valueOutput.DisconnectAll();
                        valueOutput.MarkDirtyRepaint();
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
                valueOutput.SetOutputType(InputOutputType.VALUE);
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