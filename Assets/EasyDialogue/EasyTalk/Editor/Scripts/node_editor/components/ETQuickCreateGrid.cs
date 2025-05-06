using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETQuickCreateGrid : Box
    {
        private NodeType chosenType = NodeType.UNKNOWN;

        public ETQuickCreateGrid() : base()
        {
            this.AddToClassList("quick-create-grid");
            this.style.alignSelf = Align.Center;

            //Add dialogue buttons
            ETQuickCreateGridButton convoButton = new ETQuickCreateGridButton("CONVO", "quick-create-grid-button-convo", NodeType.CONVO);
            convoButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton optionButton = new ETQuickCreateGridButton("OPTION", "quick-create-grid-button-option", NodeType.OPTION);
            optionButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton entryButton = new ETQuickCreateGridButton("ENTRY", "quick-create-grid-button-entry", NodeType.ENTRY);
            entryButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton exitButton = new ETQuickCreateGridButton("EXIT", "quick-create-grid-button-exit", NodeType.EXIT);
            exitButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton optionModifierButton = new ETQuickCreateGridButton("OPTION MOD", "quick-create-grid-button-option-mod", NodeType.OPTION_MOD);
            optionModifierButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton storyButton = new ETQuickCreateGridButton("STORY", "quick-create-grid-button-story", NodeType.STORY);
            storyButton.onChoiceSelected += CreateNode;

            Box convoBox = new Box();
            convoBox.AddToClassList("quick-create-grid-button-box");
            convoBox.AddToClassList("quick-create-grid-button-convo-box");
            convoBox.Add(convoButton);
            convoBox.Add(optionButton);
            convoBox.Add(entryButton);
            convoBox.Add(exitButton);
            convoBox.Add(optionModifierButton);
            convoBox.Add(storyButton);

            //Add logic buttons
            ETQuickCreateGridButton boolLogicButton = new ETQuickCreateGridButton("BOOL LOGIC", "quick-create-grid-button-bool-logic", NodeType.BOOL_LOGIC);
            boolLogicButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton mathOperationButton = new ETQuickCreateGridButton("MATH OPERATION", "quick-create-grid-button-math", NodeType.MATH);
            mathOperationButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton stringCompareButton = new ETQuickCreateGridButton("STRING COMPARE", "quick-create-grid-button-string-compare", NodeType.STRING_COMPARE);
            stringCompareButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton numberCompareButton = new ETQuickCreateGridButton("NUMBER COMPARE", "quick-create-grid-button-number-compare", NodeType.NUMBER_COMPARE);
            numberCompareButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton buildStringButton = new ETQuickCreateGridButton("BUILD STRING", "quick-create-grid-button-build-string", NodeType.BUILD_STRING);
            buildStringButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton triggerScriptButton = new ETQuickCreateGridButton("TRIGGER SCRIPT", "quick-create-grid-button-trigger", NodeType.TRIGGER);
            triggerScriptButton.onChoiceSelected += CreateNode;

            Box logicBox = new Box();
            logicBox.AddToClassList("quick-create-grid-button-box");
            logicBox.AddToClassList("quick-create-grid-button-logic-box");
            logicBox.Add(boolLogicButton);
            logicBox.Add(mathOperationButton);
            logicBox.Add(stringCompareButton);
            logicBox.Add(numberCompareButton);
            logicBox.Add(buildStringButton);
            logicBox.Add(triggerScriptButton);

            //Add variable buttons
            ETQuickCreateGridButton getVariableButton = new ETQuickCreateGridButton("GET VARIABLE", "quick-create-grid-button-get-variable", NodeType.GET_VARIABLE_VALUE);
            getVariableButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton setVariableButton = new ETQuickCreateGridButton("SET VARIABLE", "quick-create-grid-button-set-variable", NodeType.SET_VARIABLE_VALUE);
            setVariableButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton intButton = new ETQuickCreateGridButton("INT", "quick-create-grid-button-int", NodeType.INT_VARIABLE);
            intButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton floatButton = new ETQuickCreateGridButton("FLOAT", "quick-create-grid-button-float", NodeType.FLOAT_VARIABLE);
            floatButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton boolButton = new ETQuickCreateGridButton("BOOL", "quick-create-grid-button-bool", NodeType.BOOL_VARIABLE);
            boolButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton stringButton = new ETQuickCreateGridButton("STRING", "quick-create-grid-button-string", NodeType.STRING_VARIABLE);
            stringButton.onChoiceSelected += CreateNode;

            Box variableBox = new Box();
            variableBox.AddToClassList("quick-create-grid-button-box");
            variableBox.AddToClassList("quick-create-grid-button-variable-box");
            variableBox.Add(getVariableButton);
            variableBox.Add(setVariableButton);
            variableBox.Add(intButton);
            variableBox.Add(floatButton);
            variableBox.Add(boolButton);
            variableBox.Add(stringButton);

            //Add flow buttons
            ETQuickCreateGridButton jumpInButton = new ETQuickCreateGridButton("JUMP IN", "quick-create-grid-button-jump-in", NodeType.JUMPIN);
            jumpInButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton jumpOutButton = new ETQuickCreateGridButton("JUMP OUT", "quick-create-grid-button-jump-out", NodeType.JUMPOUT);
            jumpOutButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton randomButton = new ETQuickCreateGridButton("RANDOM", "quick-create-grid-button-random", NodeType.RANDOM);
            randomButton.onChoiceSelected += CreateNode;
            ETQuickCreateGridButton sequenceButton = new ETQuickCreateGridButton("SEQUENCE", "quick-create-grid-button-sequence", NodeType.SEQUENCE);
            sequenceButton.onChoiceSelected += CreateNode;

            Box flowBox = new Box();
            flowBox.AddToClassList("quick-create-grid-button-box");
            flowBox.AddToClassList("quick-create-grid-button-flow-box");
            flowBox.Add(jumpInButton);
            flowBox.Add(jumpOutButton);
            flowBox.Add(randomButton);
            flowBox.Add(sequenceButton);

            Add(convoBox);
            Add(logicBox);
            Add(variableBox);
            Add(flowBox);
        }

        public void Hide()
        {
            this.visible = false;

            if (chosenType != NodeType.UNKNOWN)
            {
                EasyTalkNodeEditor.Instance.NodeView.CreateNode(chosenType);
                chosenType = NodeType.UNKNOWN;
            }
        }

        public void Show()
        {
            this.visible = true;
            this.style.width = this.resolvedStyle.height;
        }

        public void CreateNode(NodeType nodeType)
        {
            this.chosenType = nodeType;
        }
    }

    public class ETQuickCreateGridButton : Box
    {
        public delegate void OnChoiceSelected(NodeType nodeType);

        public OnChoiceSelected onChoiceSelected;

        private NodeType nodeTypeToCreate;

        public ETQuickCreateGridButton(string text, string ussClass, NodeType nodeTypeToCreate) : base()
        {
            this.AddToClassList("quick-create-grid-button");
            this.AddToClassList(ussClass);

            this.nodeTypeToCreate = nodeTypeToCreate;
            this.RegisterCallback<MouseEnterEvent>(MouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(MouseLeave);

            Label label = new Label(text);
            label.AddToClassList("quick-create-grid-label");
            Add(label);
        }

        public void MouseEnter(MouseEnterEvent evt)
        {
            if (onChoiceSelected != null)
            {
                onChoiceSelected(nodeTypeToCreate);
            }

            this.style.opacity = 1.0f;
        }

        public void MouseLeave(MouseLeaveEvent evt)
        {
            if (onChoiceSelected != null)
            {
                onChoiceSelected(NodeType.UNKNOWN);
            }

            this.style.opacity = 0.7f;
        }
    }
}