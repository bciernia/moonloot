using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETQuickCreateWheel : Box
    {
        private NodeType chosenType = NodeType.UNKNOWN;

        private List<VisualElement> buttons = new List<VisualElement>();

        public ETQuickCreateWheel() : base()
        {
            this.AddToClassList("quick-create-wheel");

            //Add dialogue buttons
            ETQuickCreateButton convoButton = new ETQuickCreateButton("CONVO", "quick-create-wheel-button-convo", NodeType.CONVO);
            convoButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton appendButton = new ETQuickCreateButton("APPEND", "quick-create-wheel-button-append", NodeType.APPEND);
            appendButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton optionButton = new ETQuickCreateButton("OPTION", "quick-create-wheel-button-option", NodeType.OPTION);
            optionButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton entryButton = new ETQuickCreateButton("ENTRY", "quick-create-wheel-button-entry", NodeType.ENTRY);
            entryButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton exitButton = new ETQuickCreateButton("EXIT", "quick-create-wheel-button-exit", NodeType.EXIT);
            exitButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton optionModifierButton = new ETQuickCreateButton("OPTION MOD", "quick-create-wheel-button-option-mod", NodeType.OPTION_MOD);
            optionModifierButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton storyButton = new ETQuickCreateButton("STORY", "quick-create-wheel-button-story", NodeType.STORY);
            storyButton.onChoiceSelected += CreateNode;

            //Add logic buttons
            ETQuickCreateButton valueSelectButton = new ETQuickCreateButton("VALUE SELECT", "quick-create-wheel-button-bool-logic", NodeType.VALUE_SELECT);
            valueSelectButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton conditionalValueButton = new ETQuickCreateButton("CONDITIONAL VALUE", "quick-create-wheel-button-bool-logic", NodeType.CONDITIONAL_VALUE);
            conditionalValueButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton boolLogicButton = new ETQuickCreateButton("BOOL LOGIC", "quick-create-wheel-button-bool-logic", NodeType.BOOL_LOGIC);
            boolLogicButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton mathOperationButton = new ETQuickCreateButton("MATH OPERATION", "quick-create-wheel-button-math", NodeType.MATH);
            mathOperationButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton stringCompareButton = new ETQuickCreateButton("STRING COMPARE", "quick-create-wheel-button-string-compare", NodeType.STRING_COMPARE);
            stringCompareButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton numberCompareButton = new ETQuickCreateButton("NUMBER COMPARE", "quick-create-wheel-button-number-compare", NodeType.NUMBER_COMPARE);
            numberCompareButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton buildStringButton = new ETQuickCreateButton("BUILD STRING", "quick-create-wheel-button-build-string", NodeType.BUILD_STRING);
            buildStringButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton triggerScriptButton = new ETQuickCreateButton("TRIGGER SCRIPT", "quick-create-wheel-button-trigger", NodeType.TRIGGER);
            triggerScriptButton.onChoiceSelected += CreateNode;

            //Add variable buttons
            ETQuickCreateButton getVariableButton = new ETQuickCreateButton("GET VARIABLE", "quick-create-wheel-button-get-variable", NodeType.GET_VARIABLE_VALUE);
            getVariableButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton setVariableButton = new ETQuickCreateButton("SET VARIABLE", "quick-create-wheel-button-set-variable", NodeType.SET_VARIABLE_VALUE);
            setVariableButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton intButton = new ETQuickCreateButton("INT VARIABLE", "quick-create-wheel-button-int", NodeType.INT_VARIABLE);
            intButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton floatButton = new ETQuickCreateButton("FLOAT VARIABLE", "quick-create-wheel-button-float", NodeType.FLOAT_VARIABLE);
            floatButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton boolButton = new ETQuickCreateButton("BOOL VARIABLE", "quick-create-wheel-button-bool", NodeType.BOOL_VARIABLE);
            boolButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton stringButton = new ETQuickCreateButton("STRING VARIABLE", "quick-create-wheel-button-string", NodeType.STRING_VARIABLE);
            stringButton.onChoiceSelected += CreateNode;

            //Add flow buttons
            ETQuickCreateButton jumpInButton = new ETQuickCreateButton("JUMP IN", "quick-create-wheel-button-jump-in", NodeType.JUMPIN);
            jumpInButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton jumpOutButton = new ETQuickCreateButton("JUMP OUT", "quick-create-wheel-button-jump-out", NodeType.JUMPOUT);
            jumpOutButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton waitButton = new ETQuickCreateButton("WAIT", "quick-create-wheel-button-sequence", NodeType.WAIT);
            waitButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton randomButton = new ETQuickCreateButton("RANDOM", "quick-create-wheel-button-random", NodeType.RANDOM);
            randomButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton sequenceButton = new ETQuickCreateButton("SEQUENCE", "quick-create-wheel-button-sequence", NodeType.SEQUENCE);
            sequenceButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton pathSelectButton = new ETQuickCreateButton("PATH SELECT", "quick-create-wheel-button-sequence", NodeType.PATH_SELECT);
            pathSelectButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton pauseButton = new ETQuickCreateButton("PAUSE", "quick-create-wheel-button-sequence", NodeType.PAUSE);
            pauseButton.onChoiceSelected += CreateNode;

            //Add utility buttons
            ETQuickCreateButton playerInputButton = new ETQuickCreateButton("PLAYER INPUT", "quick-create-wheel-button-player-input", NodeType.PLAYER_INPUT);
            playerInputButton.onChoiceSelected += CreateNode;

            ETQuickCreateButton showButton = new ETQuickCreateButton("SHOW", "quick-create-wheel-button-show", NodeType.SHOW);
            showButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton hideButton = new ETQuickCreateButton("HIDE", "quick-create-wheel-button-hide", NodeType.HIDE);
            hideButton.onChoiceSelected += CreateNode;

            ETQuickCreateButton gotoButton = new ETQuickCreateButton("GOTO", "quick-create-wheel-button-goto", NodeType.GOTO);
            gotoButton.onChoiceSelected += CreateNode;

            //Add AI buttons
            /*ETQuickCreateButton aiInitButton = new ETQuickCreateButton("AI: INIT", "quick-create-wheel-button-jump-in", NodeType.AI_INIT);
            aiInitButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton aiAddMessageButton = new ETQuickCreateButton("AI: ADD MESSAGE", "quick-create-wheel-button-jump-in", NodeType.AI_ADD_MESSAGE);
            aiAddMessageButton.onChoiceSelected += CreateNode;
            ETQuickCreateButton aiPromptButton = new ETQuickCreateButton("AI: PROMPT", "quick-create-wheel-button-jump-in", NodeType.AI_PROMPT);
            aiPromptButton.onChoiceSelected += CreateNode;*/

            buttons.Add(convoButton);
            buttons.Add(appendButton);
            buttons.Add(optionButton);
            buttons.Add(entryButton);
            buttons.Add(exitButton);
            buttons.Add(optionModifierButton);
            buttons.Add(storyButton);
            buttons.Add(valueSelectButton);
            buttons.Add(conditionalValueButton);
            buttons.Add(boolLogicButton);
            buttons.Add(mathOperationButton);
            buttons.Add(stringCompareButton);
            buttons.Add(numberCompareButton);
            buttons.Add(buildStringButton);
            buttons.Add(triggerScriptButton);
            buttons.Add(showButton);
            buttons.Add(hideButton);
            
            buttons.Add(playerInputButton);
            buttons.Add(getVariableButton);
            buttons.Add(setVariableButton);
            buttons.Add(intButton);
            buttons.Add(floatButton);
            buttons.Add(boolButton);
            buttons.Add(stringButton);
            buttons.Add(jumpInButton);
            buttons.Add(jumpOutButton);
            buttons.Add(gotoButton);
            buttons.Add(waitButton);
            buttons.Add(pauseButton);
            buttons.Add(randomButton);
            buttons.Add(sequenceButton);
            buttons.Add(pathSelectButton);
            /*buttons.Add(aiInitButton);
            buttons.Add(aiAddMessageButton);
            buttons.Add(aiPromptButton);*/

            AddButtons();

            this.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
        }

        public void GeometryChanged(GeometryChangedEvent evt)
        {
            this.style.width = this.resolvedStyle.height;
            PositionButtons();
        }

        public void AddButtons()
        {
            foreach(VisualElement button in buttons)
            {
                Add(button);
            }
        }

        public void PositionButtons()
        {
            float radius = Mathf.Min(this.resolvedStyle.width / 2.0f, this.resolvedStyle.height / 2.0f) * 0.9f;
            float fontSize = radius / 20.0f;

            //With a 30 degree clear zone on the top and bottom, calculate the total y-space available
            float posY = Mathf.Sin(Mathf.Deg2Rad * 45.0f);
            float negY = Mathf.Sin(Mathf.Deg2Rad * -45.0f);

            float yRange = posY - negY;
            float spacePerButton = yRange / (buttons.Count / 2);
            float currY = negY;

            float xMult = -1.0f;
            float yMult = 1.0f;
            int midIdx = buttons.Count / 2;
            
            for (int i = 0; i < buttons.Count; i++)
            {
                VisualElement button = buttons[i];
                button.style.position = Position.Absolute;

                float angle = Mathf.Asin(currY);
                float x = Mathf.Cos(angle) * radius * xMult;
                float y = currY * radius * yMult;

                Vector3 buttonOffset = Vector3.zero;

                if (i > midIdx)
                {
                    buttonOffset = new Vector3(button.resolvedStyle.width, 0, 0);
                }

                buttonOffset += new Vector3(0.0f, button.resolvedStyle.height/2, 0.0f);

                button.transform.position =
                    this.transform.position + new Vector3(this.resolvedStyle.width / 2.0f, this.resolvedStyle.height / 2.0f)
                    + new Vector3(x, y) - buttonOffset;

                currY += spacePerButton;

                if (i == midIdx)
                {
                    currY = negY;
                    xMult *= -1.0f;
                    yMult *= -1.0f;
                }

                foreach (Label label in button.Children())
                {
                    label.style.fontSize = fontSize;
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
            }
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
            this.style.width = this.resolvedStyle.height;
            this.visible = true;

            PositionButtons();
        }

        public void CreateNode(NodeType nodeType)
        {
            this.chosenType = nodeType;
        }
    }

    public class ETQuickCreateButton : Box
    {
        public delegate void OnChoiceSelected(NodeType nodeType);

        public OnChoiceSelected onChoiceSelected;

        public NodeType nodeTypeToCreate;

        public ETQuickCreateButton(string text, string ussClass, NodeType nodeTypeToCreate) : base()
        {
            this.AddToClassList("quick-create-wheel-button");
            this.AddToClassList(ussClass);

            this.nodeTypeToCreate = nodeTypeToCreate;
            this.RegisterCallback<MouseEnterEvent>(MouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(MouseLeave);
            this.RegisterCallback<MouseDownEvent>(MouseDown);
            this.RegisterCallback<MouseUpEvent>(MouseUp);

            Label label = new Label(text);
            //label.AddToClassList("quick-create-label");
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

        public void MouseDown(MouseDownEvent evt)
        {
            EasyTalkNodeEditor.Instance.NodeView.CreateNode(nodeTypeToCreate);
            this.style.opacity = 0.7f;
        }

        public void MouseUp(MouseUpEvent evt)
        {
            this.style.opacity = 1.0f;
        }
    }
}