using EasyTalk.Controller;
using EasyTalk.Localization;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using EasyTalk.Settings;
using EasyTalk.Utils;
using System.Collections.Generic;


#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.Events;

namespace EasyTalk.Display
{
    /// <summary>
    /// This class is an abstract framework laying the foundation for a Dialogue Display.
    /// </summary>
    public abstract class AbstractDialogueDisplay : DialogueListener
    {
        /// <summary>
        /// The EasyTalk dialogue settings to use.
        /// </summary>
        [Tooltip("The EasyTalk dialogue settings to use.")]
        [SerializeField]
        protected EasyTalkDialogueSettings dialogueSettings;

        /// <summary>
        /// The currently active dialogue controller.
        /// </summary>
        protected DialogueController dialogueController;

        /// <summary>
        /// An instance of a Dialogue Display.
        /// </summary>
        protected static AbstractDialogueDisplay instance;

        /// <summary>
        /// When true, the dialogue display will be destroyed when a new scene is loaded.
        /// </summary>
        [Tooltip("If set to true, the dialogue display will be destroyed when a new scene is loaded.")]
        [SerializeField]
        protected bool destroyOnLoad = false;

        /// <summary>
        /// When set to true, the dialogue display allows an immediate exit from the currently running dialogue.
        /// </summary>
        [Tooltip("When set to true, the player can exit a playing dialogue by pressing a button corresponding to the input for the Exit Conversation Action.")]
        [SerializeField]
        private bool allowQuickExit;

        /// <summary>
        /// A collection of Dialogue Listeners to call as dialogue playback occurs.
        /// </summary>
        [Tooltip("The Dialogue Display will call the relevant methods on all Dialogue Listeners in this list as events occur during dialogue playback.")]
        [NonReorderable]
        [SerializeField]
        protected List<DialogueListener> dialogueListeners = new List<DialogueListener>();

        /// <summary>
        /// A Unity Event which is triggered whenever continuation of the current dialogue is allowed.
        /// </summary>
        [Tooltip("This event is riggered whenever continuation to the next line of dialogue is allowed.")]
        [SerializeField]
        [HideInInspector]
        public UnityEvent onContinueEnabled;

        /// <summary>
        /// A Unity Event which is triggered whenever continuation of the current dialogue is disabled.
        /// </summary>
        [Tooltip("This event is triggered whenever continuation to the next line of dialogue is disabled.")]
        [SerializeField]
        [HideInInspector]
        public UnityEvent onContinueDisabled;

        /// <summary>
        /// A Unity Event which is triggered whenever option selection is enabled (after options are presented).
        /// </summary>
        [Tooltip("This event is triggered whenever the player is allowed to select options (this normally occurs after options are displayed to the player).")]
        [SerializeField]
        [HideInInspector]
        public UnityEvent onOptionSelectionEnabled;

        /// <summary>
        ///  A Unity Event which is triggered whenever option selection is disabled (after an option is chosen).
        /// </summary>
        [Tooltip("This event is triggered whenever option selection is disallowed (this normally occurs after an option is chosen).")]
        [SerializeField]
        [HideInInspector]
        public UnityEvent onOptionSelectionDisabled;

        /// <summary>
        /// A flag indicating whether the display is currently playing a dialogue.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// A flag indicating whether the dialogue is currently displaying a conversation (rather than options).
        /// </summary>
        public bool IsCurrentlyInConversation { get; protected set; } = false;

        /// <summary>
        /// A flag indicating whether continuation is currently permitted in the conversation.
        /// </summary>
        private bool isContinueAllowed = false;

        /// <summary>
        /// A flag indicating whether option selection is currently permitted.
        /// </summary>
        private bool isOptionSelectionAllowed = false;

        /// <inheritdoc/>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            if(!destroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            SetupUtils.SetUpTextComponents(this.gameObject, false);
            UpdateFontsForLanguage(this);
        }

        /// <summary>
        /// Enables and disables Unity standard Text components and TextMeshPro components based on whether TextMeshPro is enabled/installed.
        /// </summary>
        private void OnValidate()
        {
            SetupUtils.SetUpTextComponents(this.gameObject, false);
        }

        /// <summary>
        /// If using the new input system, Init() sets up the input actions. This method also registers a method with the onLanguageChanged delegate so that the dialogue
        /// display can update fonts of all child components whenever the language is changed.
        /// </summary>
        protected virtual void Init()
        {
            EasyTalkGameState.Instance.onLanguageChanged += LanguageChanged;
        }

        /// <summary>
        /// Unregister the LanguageChanged method frm the onLanguageChanged delegate so that the EasyTalkGameState instance will no longer try to call this object.
        /// </summary>
        private void OnDestroy()
        {
            EasyTalkGameState.Instance.onLanguageChanged -= LanguageChanged;
        }

        /// <summary>
        /// If continuation is currently permitted, this will call the dialogue controller's Continue() method to continue to the next part of the current converstaion 
        /// and disable continuation.
        /// </summary>
        public virtual void Continue()
        {
            if (isContinueAllowed && dialogueController != null)
            {
                DisableContinue();
                dialogueController.Continue();
            }
        }

        /// <summary>
        /// Selects the next option in the option display (only applicable when options are being presented).
        /// </summary>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        public abstract bool SelectNextOption();

        /// <summary>
        /// Selects the previous option in the option display (only applicable when options are being presented).
        /// </summary>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        public abstract bool SelectPreviousOption();

        /// <summary>
        /// Chooses the selected option. This method should notify the Dialogue Controller of the option that was chosen using DialogueOption.OptionIndex.
        /// </summary>
        public abstract void ChooseSelectedOption();

        /// <summary>
        /// Selects the option which most closely corresponds to the specified direction.
        /// </summary>
        /// <param name="direction">The direction to select an option in.</param>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        public abstract bool SelectOptionInDirection(Vector2 direction);

        /// <summary>
        /// This method tells the active dialogue controller to exit the dialogue immediately.
        /// </summary>
        public virtual void ExitDialogue()
        {
            if (dialogueController != null)
            {
                dialogueController.ExitDialogue();
            }
        }

        /// <summary>
        /// Set the active dialogue controller for the dialogue display to communicate with.
        /// </summary>
        /// <param name="dialogueController">The dialogue controller to use.</param>
        public void SetActiveDialogueController(DialogueController dialogueController)
        {
            dialogueController.AddDialogueListener(this);
            this.dialogueController = dialogueController;
        }

        /// <summary>
        /// This method is called when options are displayed and sets the IsCurrentlyInConversation flag to false.
        /// </summary>
        /// <param name="options">The List of dialogue options being displayed.</param>
        public override void OnDisplayOptions(List<DialogueOption> options)
        {
            base.OnDisplayOptions(options);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnDisplayOptions(options);
            }

            IsCurrentlyInConversation = false;
        }

        /// <summary>
        /// This method is called when a line of dialogue is being displayed and sets the IsCurrentlyInConversation flag to true.
        /// </summary>
        /// <param name="line">The line of dialogue being displayed.</param>
        public override void OnDisplayLine(ConversationLine line)
        {
            base.OnDisplayLine(line);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnDisplayLine(line);
            }

            IsCurrentlyInConversation = true;
        }

        /// <summary>
        /// This method is called when the dialogue exits playback and sets the IsCurrentlyInConversation flag to false;
        /// </summary>
        /// <param name="exitName">The name of the exit point where the dialogue exited.</param>
        public override void OnDialogueExited(string exitName)
        {
            base.OnDialogueExited(exitName);

            IsPlaying = false;

            dialogueController.RemoveDialogueListener(this);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnDialogueExited(exitName);
            }

            IsCurrentlyInConversation = false;
        }

        /// <inheritdoc/>
        public override void OnExitCompleted()
        {
            base.OnExitCompleted();

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnExitCompleted();
            }
        }

        /// <summary>
        /// Returns the conversation display being used by this dialogue display.
        /// </summary>
        /// <returns>The conversation display being used.</returns>
        public abstract ConversationDisplay GetConversationDisplay();

        /// <summary>
        /// Sets the conversation display used by the dialogue display.
        /// </summary>
        /// <param name="convoDisplay">The conversation display to use.</param>
        public abstract void SetConversationDisplay(ConversationDisplay convoDisplay);

        /// <summary>
        /// Returns the option display being used by this dialogue display.
        /// </summary>
        /// <returns>The option display being used.</returns>
        public abstract OptionDisplay GetOptionDisplay();

        /// <summary>
        /// Sets the option display being used by this dialogue display.
        /// </summary>
        /// <param name="optionDisplay">The option display to use.</param>
        public abstract void SetOptionDisplay(OptionDisplay optionDisplay);

        /// <summary>
        /// Returns the continue display being used by this dialogue display.
        /// </summary>
        /// <returns>The continue display being used.</returns>
        public abstract ContinueDisplay GetContinueDisplay();

        /// <summary>
        /// Sets the continue display used by this dialogue display.
        /// </summary>
        /// <param name="continueDisplaY">The continue display to use.</param>
        public abstract void SetContinueDisplay(ContinueDisplay continueDisplaY);

        /// <summary>
        /// Gets the TextInputDisplay used to retrieve text input from the player.
        /// </summary>
        /// <returns></returns>
        public abstract TextInputDisplay GetTextInputDisplay();

        /// <summary>
        /// Sets the TextInputDisplay used to retrieve text input from the player.
        /// </summary>
        /// <param name="textInputDisplay">The TextInputDisplay to use.</param>
        public abstract void SetTextInputDisplay(TextInputDisplay textInputDisplay);

        /// <summary>
        /// Gets the dialogue display instance.
        /// </summary>
        public static AbstractDialogueDisplay Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Returns the current DialogueController being used by this dialogue display, if there is one.
        /// </summary>
        public DialogueController CurrentController
        {
            get { return dialogueController; }
        }

        /// <summary>
        /// Gets or sets whether the dialogue is currently allowing options to be selected by the player.
        /// </summary>
        public bool IsOptionSelectionAllowed
        {
            get { return isOptionSelectionAllowed; }
            set { isOptionSelectionAllowed = value; }
        }

        /// <summary>
        /// Gets or sets whether the player is allowed to immediately exit the dialogue by pressing a button.
        /// </summary>
        public bool IsQuickExitAllowed
        {
            get { return this.allowQuickExit; }
            set { this.allowQuickExit = value; }
        }

        /// <summary>
        /// Gets or sets whether continuation is allowed by the player by pressing a button.
        /// </summary>
        public bool IsContinueAllowed
        {
            get { return this.isContinueAllowed; }
            set { this.isContinueAllowed = value; }
        }

        /// <summary>
        /// Allows the current conversation to be continued.
        /// </summary>
        public virtual void AllowContinue()
        {
            this.isContinueAllowed = true;

            if (onContinueEnabled != null)
            {
                onContinueEnabled.Invoke();
            }
        }

        /// <summary>
        /// Disallows continuation of the current conversation.
        /// </summary>
        public virtual void DisableContinue()
        {
            this.isContinueAllowed = false;

            if (onContinueDisabled != null)
            {
                onContinueDisabled.Invoke();
            }
        }

        /// <summary>
        /// Allows option selection.
        /// </summary>
        public void AllowOptionSelection()
        {
            this.isOptionSelectionAllowed = true;

            if (onOptionSelectionEnabled != null)
            {
                onOptionSelectionEnabled.Invoke();
            }
        }

        /// <summary>
        /// Disallows option selection.
        /// </summary>
        public void DisableOptionSelection()
        {
            this.isOptionSelectionAllowed = false;

            if (onOptionSelectionDisabled != null)
            {
                onOptionSelectionDisabled.Invoke();
            }
        }

        /// <summary>
        /// This method is called when a continue occurs.
        /// </summary>
        public override void OnContinue()
        {
            base.OnContinue();

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnContinue();
            }

            DisableContinue();
        }

        /// <summary>
        /// Sets the language used by the dialogue display.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to use.</param>
        public void SetLanguage(string languageCode)
        {
            EasyTalkGameState.Instance.Language = languageCode;
        }

        /// <summary>
        /// This method is called whenever the language is changed by setting the language on the EasyTalkGameState.
        /// </summary>
        /// <param name="oldLanguage">The previous ISO-639 language code being used.</param>
        /// <param name="newLanguage">The new ISO-639 language code to use.</param>
        protected void LanguageChanged(string oldLanguage, string newLanguage)
        {
            UpdateFontsForLanguage(this);
            TranslateComponents();
        }

        /// <summary>
        /// Translates the text set on the conversation display and option display based on the currently set language of EasyTalkGameState.Instance.
        /// </summary>
        protected void TranslateComponents()
        {
            ConversationDisplay convoDisplay = GetConversationDisplay();
            convoDisplay.TranslateText(this.dialogueController);

            OptionDisplay optionDisplay = GetOptionDisplay();
            optionDisplay.TranslateOptions(this.dialogueController);
        }

        /// <summary>
        /// Updates all of the fonts on child text components based on the language currently being used.
        /// </summary>
        /// <param name="component">The component to update.</param>
        protected void UpdateFontsForLanguage(Component component)
        {
            LanguageFontOverride fontOverride = dialogueSettings.LanguageFontOverrides.GetOverrideForLanguage(EasyTalkGameState.Instance.Language);

            //Update fonts for normal standard Unity text components.
            ComponentFontManager.UpdateFonts(component, fontOverride);

#if TEXTMESHPRO_INSTALLED
            //Update fonts for TextMeshPro text components
            ComponentFontManager.UpdateTextMeshProFonts(component, fontOverride);
#endif
        }

        /// <summary>
        /// Adds the specified Dialogue Listener to the list of listeners.
        /// </summary>
        /// <param name="dialogueListener">The Dialogue Listener to add.</param>
        public void AddDialogueListener(DialogueListener dialogueListener)
        {
            if (!dialogueListeners.Contains(dialogueListener))
            {
                dialogueListeners.Add(dialogueListener);
            }
        }

        /// <summary>
        /// Removes the specified Dialogue Listener from the list of listeners.
        /// </summary>
        /// <param name="dialogueListener">The Dialogue Listener to remove.</param>
        public void RemoveDialogueListener(DialogueListener dialogueListener)
        {
            if (dialogueListeners.Contains(dialogueListener))
            {
                dialogueListeners.Remove(dialogueListener);
            }
        }

        /// <summary>
        /// Removes all Dialogue Listeners of the display.
        /// </summary>
        public void RemoveDialogueListeners()
        {
            dialogueListeners.Clear();
        }

        /// <inheritdoc/>
        public override void OnOptionChosen(DialogueOption option) 
        { 
            base.OnOptionChosen(option);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnOptionChosen(option);
            }
        }

        /// <inheritdoc/>
        public override void OnDialogueEntered(string entryPointName) 
        {
            base.OnDialogueEntered(entryPointName);

            IsPlaying = true;

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnDialogueEntered(entryPointName);
            }
        }

        /// <inheritdoc/>
        public override void OnStory(string storyText) 
        {
            base.OnStory(storyText);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnStory(storyText);
            }
        }

        /// <inheritdoc/>
        public override void OnVariableUpdated(string variableName, object value) 
        {
            base.OnVariableUpdated(variableName, value);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnVariableUpdated(variableName, value);
            }
        }

        /// <inheritdoc/>
        public override void OnCharacterChanged(string oldCharacterName, string newCharacterName) 
        {
            base.OnCharacterChanged(oldCharacterName, newCharacterName);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnCharacterChanged(oldCharacterName, newCharacterName);
            }
        }

        /// <inheritdoc/>
        public override void OnAudioStarted(ConversationLine line) 
        {
            base.OnAudioStarted(line);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnAudioStarted(line);
            }
        }

        /// <inheritdoc/>
        public override void OnAudioCompleted(ConversationLine line, bool forceStopped) 
        {
            base.OnAudioCompleted(line, forceStopped);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnAudioCompleted(line, forceStopped);
            }
        }

        /// <inheritdoc/>
        public override void OnActivateKey(string key) 
        {
            base.OnActivateKey(key);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnActivateKey(key);
            }
        }

        /// <inheritdoc/>
        public override void Wait(float timeInSeconds) 
        {
            base.Wait(timeInSeconds);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.Wait(timeInSeconds);
            }
        }

        /// <inheritdoc/>
        public override void OnConversationEnding(ConversationLine line, Node nextNode) 
        {
            base.OnConversationEnding(line, nextNode);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnConversationEnding(line, nextNode);
            }
        }

        /// <inheritdoc/>
        public override void OnNodeChanged(Node node) 
        {
            base.OnNodeChanged(node);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnNodeChanged(node);
            }
        }

        /// <inheritdoc/>
        public override void OnPause(string signal)
        {
            base.OnPause(signal);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnPause(signal);
            }
        }

        /// <inheritdoc/>
        public override void OnExecuteAsyncNode(AsyncNode node)
        {
            base.OnExecuteAsyncNode(node);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnExecuteAsyncNode(node);
            }
        }

        /// <inheritdoc/>
        public override void OnAppendText(string text)
        {
            base.OnAppendText(text);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnAppendText(text);
            }
        }

        /// <inheritdoc/>
        public override void OnWaitingForNodeEvaluation(Node asyncNode)
        {
            base.OnWaitingForNodeEvaluation(asyncNode);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnWaitingForNodeEvaluation(asyncNode);
            }
        }

        /// <inheritdoc/>
        public override void OnNodeEvaluationCompleted(Node asyncNode)
        {
            base.OnNodeEvaluationCompleted(asyncNode);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnNodeEvaluationCompleted(asyncNode);
            }
        }

        /*public override void OnAIPromptStarted()
        {
            base.OnAIPromptStarted();

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnAIPromptStarted();
            }
        }

        public override void OnAIPromptFinished()
        {
            base.OnAIPromptFinished();

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnAIPromptFinished();
            }
        }

        public override void OnConversationClear()
        {
            base.OnConversationClear();

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnConversationClear();
            }
        }*/

        /// <summary>
        /// Gets or sets the EasyTalk Dialogue Settings used by the display.
        /// </summary>
        public EasyTalkDialogueSettings DialogueSettings
        {
            get { return this.dialogueSettings; }
            set { this.dialogueSettings = value; }
        }

        /// <summary>
        /// Internal class used to keep track of font and font size settings for standard text components used by a dialogue display.
        /// </summary>
        public class TextFontSettings
        {
            public Font font;
            public int minFontSize;
            public int maxFontSize;

            public TextFontSettings(Font font, int minFontSize, int maxFontSize)
            {
                this.font = font;
                this.minFontSize = minFontSize;
                this.maxFontSize = maxFontSize;
            }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Internal class used to keep track of font and font size settings for TextMeshPro text components used by a dialogue display.
        /// </summary>
        public class TMPTextFontSettings
        {
            public TMP_FontAsset font;
            public float minFontSize;
            public float maxFontSize;

            public TMPTextFontSettings(TMP_FontAsset font, float minFontSize, float maxFontSize)
            {
                this.font = font;
                this.minFontSize = minFontSize;
                this.maxFontSize = maxFontSize;
            }
        }
#endif
    }
}