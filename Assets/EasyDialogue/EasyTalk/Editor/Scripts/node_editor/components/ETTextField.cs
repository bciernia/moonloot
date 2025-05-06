using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Editor.Utils;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETTextField : TextField
    {
        private ValidationType validationType;

        private Label placeholderLabel;

        private string oldValue = null;

        public bool PublishUndoableActions { get; set; } = true;

        public bool NotifyOfChanges { get; set; } = true;

        public ETTextField() : this(ValidationType.STRING) { }

        public ETTextField(ValidationType validationType) : this(validationType, null) { }

        public ETTextField(string placeholderText) : this(ValidationType.STRING, placeholderText) { }

        public ETTextField(ValidationType validationType, string placeholderText)
        {
            this.validationType = validationType;
            this.RegisterCallback<InputEvent>(OnInput);
            this.RegisterCallback<ChangeEvent<string>>(OnValueChanged);
            this.RegisterCallback<FocusInEvent>(OnFocusIn);
            this.RegisterCallback<FocusOutEvent>(OnFocusOut);

            if (placeholderText != null && placeholderText.Length > 0)
            {
                placeholderLabel = new Label();
                placeholderLabel.AddToClassList("node-textfield-placeholder-label");
                placeholderLabel.pickingMode = PickingMode.Ignore;
                placeholderLabel.text = placeholderText;
                
                this.Add(placeholderLabel);
            }

            this.style.unityFont = EasyTalkNodeEditor.Instance.EditorSettings.GetCurrentFont();
            this.style.unityFontDefinition = new StyleFontDefinition(EasyTalkNodeEditor.Instance.EditorSettings.GetCurrentFont());
        }

        public void SetPlaceholderText(string placeholderText)
        {
            if(this.placeholderLabel != null)
            {
                this.placeholderLabel.text = placeholderText;
            }
        }

        private void OnInput(InputEvent evt)
        {
            Validate(evt.newData, evt.previousData);
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            AdjustPlaceHolderVisibility(this.value);
            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            oldValue = this.value;
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            if(this.value != oldValue)
            {
                if (PublishUndoableActions)
                {
                    EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeTextFieldChangedAction(ETUtils.FindNodeParent(this), this, oldValue));
                }

                oldValue = this.value;
            }
        }

        private void AdjustPlaceHolderVisibility(string newValue)
        {
            if (placeholderLabel != null)
            {
                if (newValue == null || newValue.Length == 0)
                {
                    placeholderLabel.visible = true;
                }
                else
                {
                    placeholderLabel.visible = false;
                }
            }
        }

        public void HidePlaceholder()
        {
            placeholderLabel.visible = false;
        }

        public void SetValidationType(ValidationType validationType)
        {
            ValidationType oldValidationType = this.validationType;
            this.validationType = validationType;

            if (validationType != oldValidationType)
            {
                switch (validationType)
                {
                    case ValidationType.FLOAT:
                        MakeFloatCompatible();
                        break;
                    case ValidationType.INT:
                        MakeIntCompatible(oldValidationType);
                        break;
                    case ValidationType.NUMBER:
                        MakeNumberCompatible();
                        break;
                    case ValidationType.BOOL:
                        MakeBoolCompatible(oldValidationType);
                        break;
                }
            }
        }

        private void MakeFloatCompatible()
        {
            float floatValue;
            if (float.TryParse(this.value, out floatValue))
            {
                this.value = "" + floatValue;
            }
            else
            {
                this.value = null;// "3.1415";
            }
        }

        private void MakeIntCompatible(ValidationType oldValidationType)
        {
            float floatValue;
            if (oldValidationType == ValidationType.FLOAT && (float.TryParse(value, out floatValue)))
            {
                this.value = "" + (int)floatValue;
            }
            else
            {
                this.value = null;// "777";
            }
        }

        private void MakeNumberCompatible()
        {
            float floatValue;
            if (float.TryParse(this.value, out floatValue))
            {
                this.value = "" + floatValue;
            }
            else
            {
                this.value = null;// "3.1415";
            }
        }

        private void MakeBoolCompatible(ValidationType oldValidationType)
        {
            bool boolValue;
            if (oldValidationType == ValidationType.STRING && bool.TryParse(value.ToLower(), out boolValue))
            {
                this.value = "" + boolValue;
            }
            else
            {
                this.value = null;// "true";
            }
        }

        protected void Validate(string newData, string oldData)
        {
            if (validationType == ValidationType.NUMBER || validationType == ValidationType.FLOAT)
            {
                float value;
                if (newData == null || 
                    (newData.Length > 0 && (!float.TryParse(newData, out value)) &&
                    !(newData.Length == 1 && newData[0] == '-')))
                {
                    this.value = oldData;
                }
            }
            else if (validationType == ValidationType.INT)
            {
                int value;
                if (newData == null || 
                    (newData.Length > 0 && (!int.TryParse(newData, out value)) &&
                    !(newData.Length == 1 && newData[0] == '-')))
                {
                    this.value = oldData;
                }
            }
            else if (validationType == ValidationType.BOOL)
            {
                if (newData == null || (!"true".Contains(newData.ToLower()) && !"false".Contains(newData.ToLower())))
                {
                    this.value = oldData;
                }
                else if (newData.Length > 0 && newData.ToLower()[0] == 't')
                {
                    this.value = "true";
                }
                else if (newData.Length > 0 && newData.ToLower()[0] == 'f')
                {
                    this.value = "false";
                }
                else
                {
                    this.value = "";
                }
            }

            AdjustPlaceHolderVisibility(this.value);

            EasyTalkNodeEditor.Instance.NodesChanged();
        }

        public static ValidationType GetValidationTypeForString(string text)
        {
            //If all cahracters are int, return int
            int intValue;
            if (text.Length > 0 && int.TryParse(text, out intValue))
            {
                return ValidationType.INT;
            }

            //If all characters are int or ., return float
            float floatValue;
            if (text.Length > 0 && float.TryParse(text, out floatValue))
            {
                return ValidationType.FLOAT;
            }

            //If text is 'true' or 'false', return bool
            bool boolValue;
            if (text.Length > 0 && bool.TryParse(text, out boolValue))
            {
                return ValidationType.BOOL;
            }

            //Return string
            return ValidationType.STRING;
        }

        public enum ValidationType { NUMBER, FLOAT, INT, STRING, BOOL }
    }
}