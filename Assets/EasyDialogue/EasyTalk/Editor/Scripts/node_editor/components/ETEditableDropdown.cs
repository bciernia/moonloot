using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Editor.Nodes;
using EasyTalk.Editor.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETEditableDropdown : Box
    {
        private ETTextField inputField;

        public delegate void OnChoiceAdded(string choice);

        public OnChoiceAdded onChoiceAdded;

        private List<string> choices = new List<string>();

        public ETEditableDropdown(string placeholderText = "") : base()
        {
            this.style.alignItems = Align.Auto;

            this.pickingMode = PickingMode.Ignore;
            inputField = new ETTextField(placeholderText);
            inputField.AddToClassList("editable-dropdown-field");
            inputField.RegisterCallback<FocusOutEvent>(OnInputFocusLost, TrickleDown.TrickleDown);
            this.Add(inputField);

            ETNodeButton dropDownButton = new ETNodeButton();
            dropDownButton.AddToClassList("editable-dropdown-button");
            dropDownButton.onButtonClicked += ShowDropdown;
            this.Add(dropDownButton);
        }

        private void ShowDropdown()
        {
            GenericDropdownMenu menu = new GenericDropdownMenu();

            choices.Sort();

            foreach (string choice in choices)
            {
                menu.AddItem(choice, false, delegate 
                {
                    EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeTextFieldChangedAction(ETUtils.FindNodeParent(this), inputField, inputField.value));
                    inputField.value = choice;
                    EasyTalkNodeEditor.Instance.NodesChanged();
                });
            }

            Vector2 worldPos = inputField.ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.PanZoomPanel, new Vector2(0.0f, 0.0f));
            menu.DropDown(new Rect(worldPos, new Vector2(this.resolvedStyle.width, 40)), EasyTalkNodeEditor.Instance.PanZoomPanel, true);
        }

        public string GetInputValue() { return inputField.value; }

        public void SetInputValue(string value)
        {
            inputField.value = value;

            if (!choices.Contains(value))
            {
                choices.Add(value);

                if (onChoiceAdded != null)
                {
                    onChoiceAdded(inputField.value);
                }
            }
        }

        private void OnInputFocusLost(FocusOutEvent evt)
        {
            if (!this.choices.Contains(inputField.value))
            {
                this.choices.Add(inputField.value);

                if (onChoiceAdded != null)
                {
                    onChoiceAdded(inputField.value);
                }
            }
        }

        public List<string> Choices
        {
            get { return this.choices; }
            set { this.choices = value; }
        }
    }
}