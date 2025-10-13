using EasyTalk.Nodes.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EasyTalk.Controller;
using EasyTalk.Localization;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

namespace EasyTalk.Display
{
    /// <summary>
    /// A display for obtaining text input from the plpayer during dialogue playback.
    /// </summary>
    public class TextInputDisplay : DialoguePanel
    {
        /// <summary>
        /// The text input field.
        /// </summary>
        [SerializeField]
        private InputField inputField;

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text input field (TextMesh Pro version).
        /// </summary>
        [SerializeField]
        private TMP_InputField textMeshProInputField;
#endif

        /// <summary>
        /// The PlayerInput node being processed.
        /// </summary>
        private PlayerInputNode inputNode;

        /// <summary>
        /// Whether the input display should be hidden immediately on awake.
        /// </summary>
        private bool hideOnAwake = true;

        /// <summary>
        /// The default set of characters which the player can cycle through if using a gamepad.
        /// </summary>
        private char[] defaultCharacters = new char[] {
            ' ',
            'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'I', 'J', 'K', 'L',
            'M', 'N', 'O', 'P', 'Q', 'R',
            'S', 'T', 'U', 'V', 'W', 'X',
            'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z'};
        
        /// <summary>
        /// The set of input characters in use (only applicable when cycling via a gamepad).
        /// </summary>
        private char[] inputCharacters = null;

        /// <summary>
        /// The index of the character currently chosen as the gamepad input character.
        /// </summary>
        private int currentCharacter = 0;

        /// <summary>
        /// Whether the display should quickly cycle forward through input characters (if a gamepad button is held down).
        /// </summary>
        private bool cyclingCharactersForward = false;

        /// <summary>
        /// Whether the display should quickly cycle backwards through input characters (if a gamepad button is held down).
        /// </summary>
        private bool cyclingCharactersBackward = false;

        /// <summary>
        /// Keeps track of the last time at which the input character was cycled (only relevant when a gamepad button is held down).
        /// </summary>
        private float lastCycleTime = 0.0f;

        /// <summary>
        /// /The delay between switching input characters when they are being cycled (only relevant when a gamepad button is held down).
        /// </summary>
        private float cycleDelay = 0.1f;

        /// <inheritdoc>
        private void Awake()
        {
            Init();

            if (hideOnAwake)
            {
                HideImmediately();
            }

            onShowComplete.AddListener(FocusTextInput);

            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.onSubmit.AddListener(TextInputEntered);
                }
            }
            else
            {

#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.onSubmit.AddListener(TextInputEntered);
                }
#else
                if (inputField != null)
                {
                    inputField.onSubmit.AddListener(TextInputEntered);
                }
#endif
            }

            //Update the character set being used based on the currently set language
            UpdateInputCharacterSet();

            //Register the display with the game state so that we can detect when the language changes and update the character set
            EasyTalkGameState.Instance.onLanguageChanged += LanguageChanged;
        }

        /// <inheritdoc>
        private void OnDestroy()
        {
            //Unregister from the game state
            EasyTalkGameState.Instance.onLanguageChanged -= LanguageChanged;
        }

        /// <summary>
        /// Called whenever the language is changed on the EasyTalkGameState. This will attempt to update the character set used by the input display.
        /// </summary>
        /// <param name="oldLanguage">The prior language being used.</param>
        /// <param name="newLanguage">The new language to use.</param>
        protected void LanguageChanged(string oldLanguage, string newLanguage)
        {
            UpdateInputCharacterSet();
        }

        /// <summary>
        /// Update the characters being used.
        /// </summary>
        private void UpdateInputCharacterSet()
        {
            bool foundCharacterSet = false;
            DialogueDisplay parentDisplay = DialogueDisplay.GetParentDialogueDisplay(this.gameObject);

            if (parentDisplay != null && parentDisplay.DialogueSettings != null && parentDisplay.DialogueSettings.LanguageInputs != null)
            {

                foreach (LanguageInputCharacterSet characterSet in parentDisplay.DialogueSettings.LanguageInputs.CharacterSet)
                {
                    if (characterSet.LanguageCode.Equals(EasyTalkGameState.Instance.Language))
                    {
                        inputCharacters = characterSet.Characters;
                        foundCharacterSet = true;
                        break;
                    }
                }
            }

            if (!foundCharacterSet) { inputCharacters = defaultCharacters; }
        }

        /// <inheritdoc>
        private void Update()
        {
            if (Time.time - lastCycleTime > cycleDelay)
            {
                if (cyclingCharactersForward)
                {
                    NextCharacter();
                }
                else if (cyclingCharactersBackward)
                {
                    LastCharacter();
                }

                lastCycleTime = Time.time;
            }
        }

        /// <summary>
        /// Switches the currently selected input character to the last character in the character list.
        /// </summary>
        public void LastCharacter()
        {
            //Change the character at the cursor location to the last character.
            currentCharacter--;
            if(currentCharacter < 0) { currentCharacter = inputCharacters.Length - 1; }
            UpdateCharacterAtCursor();

            int caretPos = GetCaretPosition();
            if (caretPos < 1) { SetCaretPosition(1); }
        }

        /// <summary>
        /// Switches the currently selected input character to the next character in the character list.
        /// </summary>
        public void NextCharacter()
        {
            //Change the character at the cursor location to the next character
            currentCharacter++;
            if(currentCharacter >= inputCharacters.Length) { currentCharacter = 0; }
            UpdateCharacterAtCursor();

            int caretPos = GetCaretPosition();
            if (caretPos < 1) { SetCaretPosition(1); }
        }

        /// <summary>
        /// Turns on input character cycling, meaning the input display will cycle through to the next input character
        /// automatically while a gamepad input is held down.
        /// </summary>
        public void CycleNextCharacters()
        {
            cyclingCharactersForward = true;
        }

        /// <summary>
        /// Turns on input character cycling, meaning the input display will cycle through to the previous input character
        /// automatically while a gamepad input is held down.
        /// </summary>
        public void CycleLastCharacters()
        {
            cyclingCharactersBackward = true;
        }

        /// <summary>
        /// Turns automatic input character cycling off.
        /// </summary>
        public void StopCharacterCycling()
        {
            cyclingCharactersForward = false;
            cyclingCharactersBackward = false;
        }

        /// <summary>
        /// Changes the text input character at the current caret position to the currently selected character.
        /// </summary>
        private void UpdateCharacterAtCursor()
        {
            string currentText = GetText();

            int caretPosition = GetCaretPosition();
            string preText = "";
            if (caretPosition > 1)
            {
                preText = currentText.Substring(0, caretPosition - 1);
            }

            string postText = "";
            if ((caretPosition) < currentText.Length)
            {
                postText = currentText.Substring(caretPosition, currentText.Length - caretPosition);
            }

            string newText = preText + inputCharacters[currentCharacter] + postText;
            SetText(newText);
        }

        /// <summary>
        /// Moves the cursor (caret position) left by one character.
        /// </summary>
        public void MoveCursorLeft()
        {
            int caretPosition = GetCaretPosition();
            caretPosition--;
            if(caretPosition < 1) { caretPosition = 1; }

            SetCurrentCharacter(GetText()[caretPosition-1]);
            SetCaretPosition(caretPosition );
        }

        /// <summary>
        /// Moves the cursor (caret position) right by one character.
        /// </summary>
        public void MoveCursorRight()
        {
            int caretPosition = GetCaretPosition();
            caretPosition++;

            //Extend the text by adding spaces to move the cursor over to the right.
            string currentText = GetText();
            while(currentText.Length < caretPosition)
            {
                currentText += ' ';
            }

            SetText(currentText);
            SetCurrentCharacter(currentText[caretPosition - 1]);
            SetCaretPosition(caretPosition);
        }

        /// <summary>
        /// Searches through the input character list to find the specified character and sets the currently selected character index if found.
        /// </summary>
        /// <param name="character"></param>
        private void SetCurrentCharacter(char character)
        {
            for(int i = 0; i < inputCharacters.Length; i++)
            {
                if(inputCharacters[i] == character)
                {
                    currentCharacter = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the current text value of the input display.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    return inputField.text;
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    return textMeshProInputField.text;
                }
#else
                if (inputField != null)
                {
                    return inputField.text;
                }
#endif
            }

            return "";
        }

        /// <summary>
        /// Sets the text value of the input display.
        /// </summary>
        /// <param name="text">The text to set.</param>
        public void SetText(string text)
        {
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.text = text;
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.text = text;
                }
#else
                if (inputField != null)
                {
                    inputField.text = text;
                }
#endif
            }
        }

        /// <summary>
        /// Returns the current caret position of the input display.
        /// </summary>
        /// <returns></returns>
        private int GetCaretPosition()
        {
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    return inputField.caretPosition;
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    return textMeshProInputField.caretPosition;
                }
#else
                if (inputField != null)
                {
                    return inputField.caretPosition;
                }
#endif
            }

            return 1;
        }

        /// <summary>
        /// Sets the current caret position of the input display.
        /// </summary>
        /// <param name="pos">The position to place the caret (cursor) at.</param>
        private void SetCaretPosition(int pos)
        {
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.caretPosition = pos;
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.caretPosition = pos;
                }
#else
                if (inputField != null)
                {
                    inputField.caretPosition = pos;
                }
#endif
            }
        }

        /// <summary>
        /// Gets or sets wether or not the display will b e hidden immediately on Awake().
        /// </summary>
        public bool HideOnAwake
        {
            get { return hideOnAwake; }
            set { this.hideOnAwake = value; }
        }

        /// <summary>
        /// Resets the text input.
        /// </summary>
        public void Reset()
        {
            currentCharacter = 0;
            SetText("");
        }

        /// <summary>
        /// Sets the active PlayerInput node. This method also sets the hint/placeholder text configured in the node.
        /// </summary>
        /// <param name="node">The PlayerInput node to use.</param>
        public void SetInputNode(PlayerInputNode node)
        {
            this.inputNode = node;

            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.placeholder.GetComponent<Text>().text = inputNode.HintText;
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.placeholder.GetComponent<TMP_Text>().text = inputNode.HintText;
                }
#else
                if (inputField != null)
                {
                    inputField.placeholder.GetComponent<Text>().text = inputNode.HintText;
                }
#endif
            }
        }

        /// <summary>
        /// Gives the text input focus.
        /// </summary>
        private void FocusTextInput()
        {
            EventSystem.current.SetSelectedGameObject(null);
            
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    EventSystem.current.SetSelectedGameObject(textMeshProInputField.gameObject);
                }
#else
                if (inputField != null)
                {
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                }
#endif
            }
        }

        /// <summary>
        /// When called, this will call TextInputEntered() with the current text of the input field.
        /// </summary>
        public void TextInputEntered()
        {
            string text = GetText();
            if (text == null) { text = ""; }

            TextInputEntered(text.TrimEnd());
        }

        /// <summary>
        /// Sets the text entered by the player on the active PlayerInput node and calls ExecutionCompleted();
        /// </summary>
        /// <param name="text"></param>
        private void TextInputEntered(string text)
        {
            if (this.inputNode != null)
            {
                this.inputNode.InputText = text;
                this.inputNode.ExecutionCompleted();
                this.inputNode = null;
            }

            Hide();
        }

        private void OnValidate()
        {
#if TEXTMESHPRO_INSTALLED
            if (this.textMeshProInputField == null)
            {
                TMP_InputField[] tmpTextComponents = GetComponentsInChildren<TMP_InputField>(true);
                foreach (TMP_InputField txt in tmpTextComponents)
                {
                    if (txt.gameObject.name.Contains("InputField") && txt.gameObject.name.Contains("TMP"))
                    {
                        this.textMeshProInputField = txt;
                        break;
                    }
                }
            }
#endif
        }
    }
}
