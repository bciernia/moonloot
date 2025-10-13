using EasyTalk.Editor.Components;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Logic;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETTriggerScriptNode : ETNode
    {
        private ETTriggerScriptNodeInputContent inputContent;

        private ETTriggerScriptNodeOutputContent outputContent;

        private ETInputParameterContent[] methodParameters = new ETInputParameterContent[4];

        private ETTriggerFilterContent filterContent;

        public TriggerScriptNode storedNodeState = null;

        public ETTriggerScriptNode() : base("TRIGGER SCRIPT", "trigger-script-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "Trigger Script Node";
            this.connectionLine.AddToClassList("trigger-script-line");
            this.resizeType = ResizeType.BOTH;
            this.SetDimensions(220, 320);

            this.style.alignItems = Align.FlexStart;
        }

        public override void CreateContent()
        {
            inputContent = new ETTriggerScriptNodeInputContent();
            AddContent(inputContent);

            for (int i = 0; i < 4; i++)
            {
                ETInputParameterContent parameterContent = new ETInputParameterContent();
                parameterContent.VariableLabel.text = "Parameter " + (i + 1);
                AddContent(parameterContent);
                methodParameters[i] = parameterContent;
            }

            filterContent = new ETTriggerFilterContent();
            AddContent(filterContent);

            outputContent = new ETTriggerScriptNodeOutputContent();
            AddContent(outputContent);

            inputContent.onClassChanged += OnClassChanged;

            AddNodeChangeCallbacks();
            EnableMethodChangeEvents();
        }

        private void EnableMethodChangeEvents()
        {
            inputContent.MethodDropdown.RegisterCallback<ChangeEvent<string>>(OnMethodChanged);
            inputContent.MethodField.RegisterCallback<ChangeEvent<string>>(OnMethodChanged);
        }

        private void DisableMethodChangeEvents() 
        {
            inputContent.MethodDropdown.UnregisterCallback<ChangeEvent<string>>(OnMethodChanged);
            inputContent.MethodField.UnregisterCallback<ChangeEvent<string>>(OnMethodChanged);
        }

        private void AddNodeChangeCallbacks()
        {
            foreach(ETInputParameterContent paramContent in methodParameters)
            {
                paramContent.VariableField.RegisterCallback<ChangeEvent<string>>(NodeChanged);
            }

            foreach(ETInput input in Inputs) 
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

        private void NodeChanged(ChangeEvent<string> evt)
        {
            UpdateStoredNodeState();
        }

        public void UpdateStoredNodeState()
        {
            storedNodeState = CreateNode() as TriggerScriptNode;
        }

        private void OnClassChanged()
        {
            foreach (ETInputParameterContent paramContent in methodParameters)
            {
                EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(paramContent.Input);
            }
        }

        private void OnMethodChanged(ChangeEvent<string> evt)
        {
            if (storedNodeState != null)
            {
                EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(this.ID, storedNodeState, "Trigger Script Method Changed");
            }

            //Determine how many variables to show, and set their types based on the chosen method.
            UpdateParameterDisplay();

            EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Trigger Script Method Changed");
            UpdateStoredNodeState();
        }

        private void UpdateParameterDisplay()
        {
            HideAllParameters();
            string chosenMethod = inputContent.MethodSignature;

            if (chosenMethod == null || chosenMethod.Length == 0) { return; }

            string[] parameterTypes = GetParameterTypeNamesFromSignature(chosenMethod);

            for (int i = 0; i < methodParameters.Length; i++)
            {
                if (i >= parameterTypes.Length || parameterTypes[i] == null || parameterTypes[i].Length == 0)
                {
                    //Delete connections for any parameters which are no longer visible.
                    EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(methodParameters[i].GetInputs()[0]);
                    continue;
                }
                else if (i < parameterTypes.Length)
                {
                    //Change the input type for the parameter to match the method input type.
                    methodParameters[i].style.display = DisplayStyle.Flex;
                    ETInput currentInput = methodParameters[i].Input;
                    InputOutputType currentInputType = currentInput.InputType;
                    InputOutputType newInputType = ETUtils.GetInputOutputTypeForString(parameterTypes[i]);
                    currentInput.SetInputType(newInputType);
                    UpdateFieldType(newInputType, methodParameters[i].VariableField);

                    //If the parameter type changed, delete any connections coming into it.
                    if (!ETUtils.IsConnectionAllowed(currentInputType, newInputType))
                    {
                        EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(currentInput);
                    }
                }
            }

            int returnTypeIdx = chosenMethod.IndexOf(":") + 1;
            if (returnTypeIdx > 0)
            {
                string returnType = chosenMethod.Substring(returnTypeIdx);
                InputOutputType currentOutputType = outputContent.ValueOutput.OutputType;
                InputOutputType newOutputType = ETUtils.GetInputOutputTypeForString(returnType);
                outputContent.ValueOutput.SetOutputType(newOutputType);
                outputContent.ValueOutput.style.display = DisplayStyle.Flex;

                if (!ETUtils.IsConnectionAllowed(currentOutputType, newOutputType))
                {
                    EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(outputContent.ValueOutput);
                }
            }
            else
            {
                outputContent.ValueOutput.SetOutputType(InputOutputType.VALUE);
                outputContent.ValueOutput.style.display = DisplayStyle.None;
                EasyTalkNodeEditor.Instance.NodeView.DeleteConnections(outputContent.ValueOutput);
            }
        }

        private string[] GetParameterTypeNamesFromSignature(string methodSignature)
        {
            int startIdx = methodSignature.IndexOf("(");
            int endIdx = methodSignature.IndexOf(")");

            string[] parameterTypeNames;

            if (startIdx > 0 && endIdx > startIdx + 1)
            {
                string parameterString = methodSignature.Substring(startIdx + 1, endIdx - (startIdx + 1));
                parameterTypeNames = parameterString.Split(',');
            }
            else
            {
                parameterTypeNames = new string[0];
            }

            return parameterTypeNames;
        }

        private void UpdateFieldType(InputOutputType newInputType, ETTextField variableField)
        {
            switch (newInputType)
            {
                case InputOutputType.VALUE: variableField.SetValidationType(ETTextField.ValidationType.STRING); break;
                case InputOutputType.NUMBER: variableField.SetValidationType(ETTextField.ValidationType.NUMBER); break;
                case InputOutputType.INT: variableField.SetValidationType(ETTextField.ValidationType.INT); break;
                case InputOutputType.FLOAT: variableField.SetValidationType(ETTextField.ValidationType.FLOAT); break;
                case InputOutputType.BOOL: variableField.SetValidationType(ETTextField.ValidationType.BOOL); break;
                case InputOutputType.STRING: variableField.SetValidationType(ETTextField.ValidationType.STRING); break;
            }
        }

        private void HideAllParameters()
        {
            for (int i = 0; i < methodParameters.Length; i++)
            {
                methodParameters[i].style.display = DisplayStyle.None;
            }
        }

        public override Node CreateNode()
        {
            TriggerScriptNode triggerScriptNode = new TriggerScriptNode();
            triggerScriptNode.ID = id;

            CreateInputsForNode(triggerScriptNode);
            CreateOutputsForNode(triggerScriptNode);
            SetSizeForNode(triggerScriptNode);
            SetPositionForNode(triggerScriptNode);

            triggerScriptNode.TriggeredClassName = inputContent.ClassName;
            triggerScriptNode.TriggeredMethodName = inputContent.MethodName;
            triggerScriptNode.MethodSignature = inputContent.MethodSignature;
            triggerScriptNode.TriggerType = filterContent.TriggerType;
            triggerScriptNode.ObjectTagOrName = filterContent.ObjectTagOrName;

            string[] paramTypes = GetParameterTypeNamesFromSignature(inputContent.MethodSignature);
            string[] paramValues = null;

            if (paramTypes.Length > 0)
            {
                paramValues = new string[paramTypes.Length];
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (methodParameters[i].Input.ConnectedOutputs.Count == 0)
                    {
                        paramValues[i] = methodParameters[i].VariableValue;
                    }
                    else
                    {
                        paramValues[i] = "";
                    }
                }
            }

            triggerScriptNode.ParameterTypes = paramTypes;
            triggerScriptNode.ParameterValues = paramValues;

            return triggerScriptNode;
        }

        public override void InitializeFromNode(Node node)
        {
            TriggerScriptNode triggerScriptNode = node as TriggerScriptNode;
            this.id = triggerScriptNode.ID;
            SetSizeFromNode(triggerScriptNode);
            SetPositionFromNode(triggerScriptNode);

            //Prevent the change of the class name from triggering events.
            DisableMethodChangeEvents();

            //Update the class name used.
            inputContent.ClassName = triggerScriptNode.TriggeredClassName;

            //Set the class name and initialize the method dropdown with methods of that class.
            inputContent.UpdateClassType();
            inputContent.SetupMethodDropdown();

            //Update the method signature used.
            inputContent.MethodSignature = triggerScriptNode.MethodSignature;

            //Update to show parameters based on the method.
            UpdateParameterDisplay();

            //Re-enable events for when the class name is changed.
            EnableMethodChangeEvents();

            //Update the trigger type and object or tag name.
            filterContent.TriggerType = triggerScriptNode.TriggerType;
            filterContent.ObjectTagOrName = triggerScriptNode.ObjectTagOrName;

            InitializeAllInputsAndOutputsFromNode(triggerScriptNode);

            if (triggerScriptNode.ParameterValues != null)
            {
                for (int i = 0; i < triggerScriptNode.ParameterValues.Length; i++)
                {
                    methodParameters[i].VariableValue = triggerScriptNode.ParameterValues[i];
                }
            }

            UpdateStoredNodeState();
        }

        public override bool HasText(string text)
        {
            if (base.HasText(text))
            {
                return true;
            }

            if (inputContent.ClassName.ToLower().Contains(text.ToLower())) { return true; }
            if (inputContent.MethodSignature.ToLower().Contains(text.ToLower())) { return true; }

            foreach (ETInputParameterContent parameter in methodParameters)
            {
                if (parameter.VariableValue.ToLower().Contains(text.ToLower())) { return true; }
            }

            return false;
        }

        protected override string GetNodeTooltip()
        {
            return "Used to call a static class method or a Monobehavior method.";
        }
    }

    public class ETTriggerScriptNodeOutputContent : ETNodeContent
    {
        private ETOutput valueOutput;

        public ETTriggerScriptNodeOutputContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM) { }

        public ETOutput ValueOutput { get { return this.valueOutput; } }

        protected override void CreateOutputs()
        {
            base.CreateOutputs();
            valueOutput = this.AddOutput(InputOutputType.VALUE);
            this.AddOutput(InputOutputType.DIALGOUE_FLOW);
        }
    }

    public class ETTriggerScriptNodeInputContent : ETNodeContent
    {
        private ETTextField scriptClassNameField;

        private DropdownField methodDropdown;

        private ETTextField methodField;

        public delegate void OnClassChanged();

        public OnClassChanged onClassChanged;

        private Type classTypeSelected;

        public ETTriggerScriptNodeInputContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM) { }

        public override void Layout()
        {
            base.Layout();
            this.style.flexGrow = 0;
            this.style.flexShrink = 0;
            this.style.flexBasis = 60;
            this.style.paddingBottom = 0;
            this.style.flexDirection = FlexDirection.Row;
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            this.AddInput(InputOutputType.DIALGOUE_FLOW);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems = Align.FlexStart;

            scriptClassNameField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Class Name/Drop Script or Game Object...");
            scriptClassNameField.PublishUndoableActions = false;
            scriptClassNameField.AddToClassList("trigger-script-field");
            scriptClassNameField.AddToClassList("node-text-area");
            scriptClassNameField.style.maxHeight = 32.0f;
            scriptClassNameField.RegisterCallback<DragExitedEvent>(OnDragExited);
            scriptClassNameField.RegisterCallback<InputEvent>(OnClassSelectionChanged);
            scriptClassNameField.RegisterCallback<DragEnterEvent>(OnDragEnter);
            scriptClassNameField.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            contentContainer.Add(scriptClassNameField);

            methodDropdown = new DropdownField();
            methodDropdown.AddToClassList("trigger-script-enum-field");
            methodDropdown.style.display = DisplayStyle.None;
            methodDropdown.RegisterCallback<ChangeEvent<string>>(OnMethodChanged);
            contentContainer.Add(methodDropdown);

            methodField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Method Signature...");
            methodField.AddToClassList("trigger-script-field");
            methodField.AddToClassList("node-text-area");
            contentContainer.Add(methodField);

            contentContainer.style.flexGrow = 0;
        }

        public void OnMethodChanged(ChangeEvent<string> evt)
        {
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        public DropdownField MethodDropdown { get { return methodDropdown; } }

        public TextField MethodField { get { return methodField; } }

        public string ClassName
        {
            get { return scriptClassNameField.text; }
            set { scriptClassNameField.value = value; }
        }

        public string MethodName
        {
            get
            {
                string methodSignature = MethodSignature;
                int parenthesesIdx = methodSignature.IndexOf("(");

                if (methodSignature != null && methodSignature.Length > 0 && parenthesesIdx > 0)
                {
                    return methodSignature.Substring(0, parenthesesIdx);
                }
                else
                {
                    return methodSignature;
                }
            }
        }


        public string MethodSignature
        {
            get
            {
                if (methodDropdown.style.display == DisplayStyle.None)
                {
                    return methodField.text;
                }

                return methodDropdown.value;
            }
            set
            {
                if (methodDropdown.style.display == DisplayStyle.None)
                {
                    methodField.value = value;
                }
                else
                {
                    methodDropdown.value = value;
                }
            }
        }

        private void OnClassSelectionChanged(InputEvent evt)
        {
            ClassChanged();
        }

        private void ClassChanged()
        {
            bool classChangeCompleted = false;

            try
            {
                //Determine the class type given the current value in the class name field.
                UpdateClassType();

                if (classTypeSelected != null)
                {
                    ETTriggerScriptNode node = (ETUtils.FindNodeParent(this) as ETTriggerScriptNode);
                    TriggerScriptNode storedNodeState = node.storedNodeState;
                    if (storedNodeState != null)
                    {
                        EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(node.ID, storedNodeState, "Trigger Script Node Class Changed Action");
                    }

                    classChangeCompleted = true;

                    if (onClassChanged != null)
                    {
                        onClassChanged();
                    }

                    //Set up the method dropdown.
                    SetupMethodDropdown();
                }
                else
                {
                    methodDropdown.style.display = DisplayStyle.None;
                    methodField.style.display = DisplayStyle.Flex;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            //Delay the completion of the undo action since it takes time for events to propagate from changes to fields like the method dropdown...
            EditorApplication.delayCall += delegate
            {
                EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Trigger Script Node Class Changed Action");
                if (classChangeCompleted)
                {
                    ETTriggerScriptNode triggerScriptNode = ETUtils.FindNodeParent(this) as ETTriggerScriptNode;
                    triggerScriptNode.UpdateStoredNodeState();
                }
            };
        }

        public void UpdateClassType()
        {
            string className = scriptClassNameField.value;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (className != null && className.Length > 0)
                {
                    classTypeSelected = assembly.GetType(className);

                    if(classTypeSelected != null)
                    {
                        break;
                    }
                }
            }
        }

        public void SetupMethodDropdown()
        {
            if (classTypeSelected == null) { return; }

            methodDropdown.choices.Clear();
            methodDropdown.style.display = DisplayStyle.Flex;
            methodField.style.display = DisplayStyle.None;

            List<string> methodNames = GetMethodSignatures(classTypeSelected);

            methodDropdown.choices = methodNames;

            if (methodDropdown.choices.Count > 0)
            {
                methodDropdown.value = methodDropdown.choices[0];
            }
            else
            {
                methodDropdown.value = null;
            }
        }

        private List<string> GetMethodSignatures(Type classType)
        {
            MethodInfo[] methods = classType.GetMethods();

            List<string> methodSignatures = new List<string>();
            List<string> declaredSignatures = new List<string>();

            foreach (MethodInfo method in methods)
            {
                if (method.IsAbstract || method.IsPrivate) { return methodSignatures; }

                //If the type isn't a MonoBehaviour, only allow static methods to be used since we have no way
                //of looking up non-MonoBehaviour instances of objects.
                if (!(typeof(MonoBehaviour)).IsAssignableFrom(classTypeSelected) && !method.IsStatic)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 4) { continue; }

                string paramString = "(";

                bool valid = true;
                for (int p = 0; p < parameters.Length; p++)
                {
                    ParameterInfo param = parameters[p];

                    if (!IsTypeSupported(param.ParameterType))
                    {
                        valid = false;
                        break;
                    }

                    paramString += ETUtils.GetSimplifiedStringForPrimitiveType(param.ParameterType);
                    if (p < parameters.Length - 1)
                    {
                        paramString += ",";
                    }
                }

                paramString += ")";
                string methodSig = method.Name + paramString;

                if (method.ReturnType != null && IsTypeSupported(method.ReturnType))
                {
                    methodSig += ":" + ETUtils.GetSimplifiedStringForPrimitiveType(method.ReturnType);
                }

                if (valid) 
                {
                    if (method.DeclaringType == classType)
                    {
                        declaredSignatures.Add(methodSig);
                    }
                    else
                    {
                        methodSignatures.Add(methodSig);
                    }
                }
            }

            declaredSignatures.Sort();
            methodSignatures.Sort();

            for(int i = declaredSignatures.Count-1; i >= 0; i--)
            {
                methodSignatures.Insert(0, declaredSignatures[i]);
            }

            return methodSignatures;
        }

        private bool IsTypeSupported(Type type)
        {
            if (!(type == typeof(byte) ||
                  type == typeof(short) ||
                  type == typeof(int) ||
                  type == typeof(long) ||
                  type == typeof(float) ||
                  type == typeof(double) ||
                  type == typeof(decimal) ||
                  type == typeof(bool) ||
                  type == typeof(string) ||
                  type == typeof(object)))
            {
                return false;
            }

            return true;
        }

        private void OnDragEnter(DragEnterEvent evt)
        {
            EasyTalkNodeEditor.Instance.SetDragAndDropMode(DragAndDropVisualMode.Generic);
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            EasyTalkNodeEditor.Instance.SetDragAndDropMode(DragAndDropVisualMode.None);
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            if (DragAndDrop.paths.Length == 0 && DragAndDrop.objectReferences.Length == 0) { return; }

            List<string> classNames = new List<string>();

            if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && DragAndDrop.paths[0].EndsWith(".cs"))
            {
                classNames = ETClassReader.ReadClasses(DragAndDrop.paths[0]);
            }
            else if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
            {
                UnityEngine.Object obj = DragAndDrop.objectReferences[0];

                if (obj is GameObject)
                {
                    GameObject gameObj = (GameObject)obj;
                    foreach (Component component in gameObj.GetComponents<Component>())
                    {
                        classNames.Add(component.GetType().FullName.ToString());
                    }
                }
            }

            if (classNames.Count > 1)
            {
                GenericDropdownMenu menu = new GenericDropdownMenu();

                foreach (string className in classNames)
                {
                    //Debug.Log("Class name: " + className);
                    menu.AddItem(className, false, 
                        delegate 
                        { 
                            scriptClassNameField.value = className;
                            ClassChanged();
                        });
                }

                //Popup menu to choose class.
                Vector2 worldPos = scriptClassNameField.ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.PanZoomPanel, new Vector2(0.0f, 0.0f));
                menu.DropDown(new Rect(worldPos, new Vector2(0, 40)), EasyTalkNodeEditor.Instance.PanZoomPanel, false);
            }
            else if (classNames.Count == 1)
            {
                scriptClassNameField.value = classNames[0];
                ClassChanged();
            }

            EasyTalkNodeEditor.Instance.SetDragAndDropMode(DragAndDropVisualMode.None);
        }
    }

    public class ETInputParameterContent : ETNodeContent
    {
        private ETInput input;

        private ETTextField variableField;

        private Label variableLabel;

        public ETInputParameterContent() : base(NodeAlignment.CENTER, NodeAlignment.CENTER)
        {
            this.style.flexGrow = 0;
            this.style.paddingBottom = 0;
        }

        public ETInput Input { get { return input; } }

        public ETTextField VariableField
        {
            get { return variableField; }
        }

        public string VariableValue
        {
            get { return VariableField.text; }
            set { VariableField.value = value; }
        }

        protected override void CreateInputs()
        {
            base.CreateInputs();
            input = AddInput(InputOutputType.VALUE);
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.flexGrow = 0;

            variableField = new ETTextField(ETTextField.ValidationType.STRING, "Enter Variable Value...");
            variableField.AddToClassList("trigger-script-field");
            variableField.AddToClassList("node-text-area");
            contentContainer.Add(variableField);

            variableLabel = new Label("Parameter");
            variableLabel.AddToClassList("trigger-script-label");
            variableLabel.style.display = DisplayStyle.None;
            contentContainer.Add(variableLabel);
        }

        protected override void OnConnectionCreated(int inputId, int outputId)
        {
            if (inputId == input.ID)
            {
                variableField.style.display = DisplayStyle.None;
                variableLabel.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnConnectionDeleted(int inputId, int outputId)
        {
            if (inputId == input.ID)
            {
                variableField.style.display = DisplayStyle.Flex;
                variableLabel.style.display = DisplayStyle.None;
            }
        }

        public Label VariableLabel { get { return variableLabel; } }
    }

    public class ETTriggerFilterContent : ETNodeContent
    {
        private TextField tagOrNameField;

        private ETEnumField filterTypeDropdown;

        public ETTriggerFilterContent() : base(NodeAlignment.TOP, NodeAlignment.BOTTOM) { }

        public TriggerFilterType TriggerType
        {
            get { return (TriggerFilterType)filterTypeDropdown.value; }
            set { filterTypeDropdown.value = value; }
        }

        public string ObjectTagOrName
        {
            get
            {
                if (TriggerType == TriggerFilterType.TAG || TriggerType == TriggerFilterType.NAME)
                {
                    return tagOrNameField.text;
                }

                return null;
            }
            set
            {
                if (TriggerType == TriggerFilterType.TAG || TriggerType == TriggerFilterType.NAME)
                {
                    tagOrNameField.value = value;
                }
            }
        }

        public override void Layout()
        {
            base.Layout();
            this.style.flexGrow = 0;
			this.style.paddingRight = 32.0f;
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);

            contentContainer.style.marginTop = 10;
            contentContainer.style.justifyContent = Justify.FlexStart;

            Box filterTypeBox = new Box();
            filterTypeBox.style.flexShrink = 0;
            filterTypeBox.style.paddingLeft = 32.0f;
            filterTypeBox.style.flexDirection = FlexDirection.Row;
            filterTypeBox.style.alignItems = Align.Center;

            Label filterTypeLabel = new Label("Apply To:");
            filterTypeLabel.AddToClassList("trigger-script-label");
            filterTypeLabel.style.justifyContent = Justify.Center;

            filterTypeDropdown = new ETEnumField(TriggerFilterType.SELF);
            filterTypeDropdown.AddToClassList("trigger-script-enum-field");
            filterTypeDropdown.RegisterCallback<ChangeEvent<Enum>>(OnTriggerFilterChanged);

            filterTypeBox.Add(filterTypeLabel);
            filterTypeBox.Add(filterTypeDropdown);
            contentContainer.Add(filterTypeBox);

            tagOrNameField = new ETTextField("Enter Tag or Name...");
            tagOrNameField.AddToClassList("trigger-script-tag-field");
            tagOrNameField.style.display = DisplayStyle.None;
			tagOrNameField.style.alignSelf = Align.Stretch;
            tagOrNameField.style.flexGrow = 1;
            tagOrNameField.style.paddingLeft = 32.0f;
            contentContainer.Add(tagOrNameField);
        }

        private void OnTriggerFilterChanged(ChangeEvent<Enum> evt)
        {
            TriggerFilterType filterType = Enum.Parse<TriggerFilterType>(evt.newValue.ToString());

            if (filterType == TriggerFilterType.TAG)
            {
                tagOrNameField.style.display = DisplayStyle.Flex;
            }
            else if (filterType == TriggerFilterType.NAME)
            {
                tagOrNameField.style.display = DisplayStyle.Flex;
            }
            else
            {
                tagOrNameField.style.display = DisplayStyle.None;
            }
        }
    }
}