using EasyTalk.Nodes.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyTalk.Controller;
using EasyTalk.Nodes.Core;
using static EasyTalk.Controller.DialogueController;
using EasyTalk.Nodes.Utility;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// This is an implementation of a Dialogue Display which supports a conversation display (for displaying dialogue and character information), an option display (for 
    /// displaying options to the player), and a continue display (for showing the player when they continue or skip ahead in the conversation).
    /// </summary>
    public class DialogueDisplay : AbstractDialogueDisplay
    {
        /// <summary>
        /// The conversation display to use when displaying dialogue.
        /// </summary>
        [Tooltip("The Conversation Display used to show text from conversation lines and character names.")]
        [SerializeField]
        protected ConversationDisplay convoDisplay;

        /// <summary>
        /// The option display to use when displaying options.
        /// </summary>
        [Tooltip("The Option Display used to present options to the player.")]
        [SerializeField]
        protected OptionDisplay optionDisplay;

        /// <summary>
        /// The continue display to use when indicating to the player that they can continue on in a conversation.
        /// </summary>
        [Tooltip("The Continue Display which can be shown to the player to let them know that they can continue.")]
        [SerializeField]
        protected ContinueDisplay continueDisplay;

        /// <summary>
        /// The text input display used to retrieve text input from the player.
        /// </summary>
        [SerializeField]
        protected TextInputDisplay textInputDisplay;

        /// <summary>
        /// The character display used to show character portrayals during dialogue playback.
        /// </summary>
        [SerializeField]
        protected CharacterDisplay characterDisplay;

        /// <summary>
        /// The icon display used to show character icons during dialogue playback.
        /// </summary>
        [SerializeField]
        protected IconDisplay iconDisplay;

        /// <summary>
        /// The default Conversation Display ID to use when a Dialogue Controller doesn't specify a Conversation Display or ID.
        /// </summary>
        [Tooltip("This value specifies the Display ID of the Conversation Display which the Dialogue Display should fallback to whenever the actively running Dialogue Controller doesn't " +
            "specifiy a specific Conversation Display or Display ID to use.")]
        [SerializeField]
        protected string defaultConvoDisplayID = "main";

        /// <summary>
        /// Whether the dialogue display should be hidden when a dialogue is exited.
        /// </summary>
        [Tooltip("If set to true, whenever a dialogue is exited, the dialogue display will be hidden.")]
        [SerializeField]
        protected bool hideDisplayOnExit = false;

        /// <summary>
        /// Whether the conversation display should be hidden when a pause node is reached.
        /// </summary>
        [Tooltip("If true, the conversation display will be hidden whenever a pause node is encountered.")]
        [SerializeField]
        protected bool hideOnPause = false;

        /// <summary>
        /// Whether the conversation display should be hidden when options are being presented to the player.
        /// </summary>
        [Tooltip("When set to 'true', the conversation display will be hidden whenever options are presented to the player.")]
        [SerializeField]
        protected bool hideConvoWhenShowingOptions = false;

        /// <summary>
        /// Whether the conversation display should be reset when options are being presented.
        /// </summary>
        [Tooltip("When set to true, the conversation display text and character name will be cleared whenever options are being presented to the player.")]
        [SerializeField]
        protected bool clearConvoWhenShowingOptions = false;

        /// <summary>
        /// Whether the conversation dispaly should be hidden and updated offscreen whenever the a character change is detected.
        /// </summary>
        [Tooltip("When set to 'true', if the character name changes during a conversation, the conversation display will be hidden, updated, and re-animated before " +
            "being shown. NOTE: The conversation panel will NOT be refreshed if the change is from/to a blank character name.")]
        [SerializeField]
        protected bool refreshConvoOnCharacterChange = true;

        /// <summary>
        /// Whether the conversation display should be hidden and updated offscreen whenever the dialogue text changes.
        /// </summary>
        [Tooltip("When set to 'true', the conversation display will be hidden, updated, and re-animated for each line of dialogue.")]
        [SerializeField]
        protected bool refreshConvoOnTextChange = false;

        /// <summary>
        /// Whether lines of dialogue can be skipped before their audio or display time is completed.
        /// </summary>
        [SerializeField]
        [Tooltip("When in AUTOPLAY mode, setting this to true allows the player to skip through dialogue using input controls.")]
        protected bool areLinesSkippable = false;

        /// <summary>
        /// The amount of time each word should count toward the total display time of a line of dialogue if there is no audio file.
        /// </summary>
        [SerializeField]
        [Tooltip("When in autoplay mode, if no audio clip is included, each line of dialogue will be analyzed to determine the word count. " +
            "That word count is multiplied by this value to determine how long the line should be displayed.")]
        protected float timePerWord = 0.2f;

        /// <summary>
        /// The minimum amount of time a line of dialogue will be shown when in AUTOPLAY playback mode.
        /// </summary>
        [SerializeField]
        [Tooltip("The minimum amount of time a line of dialogue will be shown when in AUTOPLAY mode.")]
        protected float minConvoTime = 1.0f;

        /// <summary>
        /// A delay which is added on top of the normal display time for a line of dialogue when in AUTOPLAY playback mode.
        /// </summary>
        [SerializeField]
        [Tooltip("How much additional time should be taken for each line of dialogue before continuing to the next line/node. (NOTE: This is " +
            "only applicable when the controller is set to AUTOPLAY mode.)")]
        protected float convoLineDelay = 1.0f;

        /// <summary>
        /// Whether the continue display should be used to let the player know when they can continue or skip ahead.
        /// </summary>
        [Tooltip("If set to 'true', the Continue Display will be shown to the player each time they are allowed to continue along to the next line in the conversation.")]
        [SerializeField]
        private bool useContinueDisplay = true;

        /// <summary>
        /// The continuation mode of the dialogue display. This determiens when the player is allowed to continue or skip ahead.
        /// </summary>
        [Tooltip("Indicates when the player is allowed to continue to the next line of dialogue." +
            "\nIMMEDIATE: As soon as a line is displayed the player can continue." +
            "\nAFTER_AUDIO: The player will be allowed to continue after the audio file assigned to the dialogue finishes playing." +
            "\nAFTER_DELAY: The player will be allowed to continue after the continuation delay has elapsed from the time the line of dialogue is shown." +
            "\nAFTER_AUDIO_AND_DELAY: The player will be allowed to continue after both audio and the continuation delay have completed." +
            "\nAFTER_AUDIO_OR_DELAY: The player will be allowed to continue after either the audio finishes, or the continuation delay, whichever occurs first.")]
        [SerializeField]
        private DialogueDelayMode continuationMode = DialogueDelayMode.IMMEDIATE;
        
        /// <summary>
        /// The minimum amount of time a line of dialogue will be shown before permitting the player to continue or skip ahead.
        /// </summary>
        [Tooltip("The amount of time (in seconds) to wait after showing a line of dialogue before allowing the player to continue.")]
        [SerializeField]
        private float continuationDelay = 1.0f;

        /// <summary>
        /// When set to true, the display will attempt to find a conversation display with a display ID matching the new (untranslated) character name and set the 
        /// dialogue display to use that, if found.
        /// </summary>
        [SerializeField]
        [Tooltip("When set to true, whenever the character changes while showing dialogue lines, the display will attempt to find a " +
            "Conversation Display with the same Display ID as the character name and tell the Dialogue Display to use it.")]
        protected bool switchConvoDisplayOnCharacterChange = false;

        /// <summary>
        /// Whether options should be presented automatically after the prior line of dialogue. If this is false, then options won't be shown until Continue() is called. 
        /// </summary>
        [SerializeField]
        [Tooltip("If set to true, options will be automatically presented after either the prior conversation line's audio clip has played, " +
            "or if there is no audio clip, after the configured option delay time. When set to false, options will not be shown until the player continues via input.")]
        private bool presentOptionsAutomatically = false;

        /// <summary>
        /// Determines how/when options are displayed when an option node is processed, either immediately, after a delay, or after a line of dialogue's audio is completed.
        /// </summary>
        [SerializeField]
        [Tooltip("When presenting options automatically, this will determine how quickly the options are displayed to the player when an option " +
            "node is encountered. Options can be displayed immediately, or they can be delayed after waiting on convo audio to finish, waiting on a delay, or both.")]
        private DialogueDelayMode optionDelayMode = DialogueDelayMode.IMMEDIATE;

        /// <summary>
        /// The delay to use when presenting options.
        /// </summary>
        [SerializeField]
        [Tooltip("How long the system should wait before showing options when options are to be presented. This is in addition to whatever time " +
            "is needed for transition animations.")]
        protected float optionDelay = 2.0f;

        /// <summary>
        /// Whether the dialogue display should automatically call Continue() on the Dialogue Controller when a story node is encountered.
        /// </summary>
        [Tooltip("When true, the Dialogue Display will automatically call Continue() on the Dialogue Controller whenever a story node is reached rather than waiting.")]
        [SerializeField]
        protected bool continueOnStory = true;

        /*
        /// <summary>
        /// Whether the display should allow dialogue to be skipped when it is generated by an AI server.
        /// </summary>
        [Tooltip("When true, the dialogue display will allow the player to skip dialogue which is being generated by AI.")]
        [SerializeField]
        protected bool allowAISkip = false;*/

        [SerializeField]
        protected Image loadingIcon;

        /// <summary>
        /// The coroutine for delaying options after a certain amount of time.
        /// </summary>
        private Coroutine displayOptionsAfterDelayRoutine;

        /// <summary>
        /// The coroutine for automaticallly playing lines of dialogue.
        /// </summary>
        private Coroutine autoplayRoutine;

        /// <summary>
        /// The display ID of the conversation display currently being targeted.
        /// </summary>
        private string currentTarget = null;

        /// <summary>
        /// The line of dialogue currently being displayed.
        /// </summary>
        private ConversationLine currentLine = null;

        /// <summary>
        /// The List of dialogue options currently being presented.
        /// </summary>
        private List<DialogueOption> displayedOptions;

        /// <summary>
        /// A flag used to control whether automatic continuation is allowed when the controller is in Autoplay mode.
        /// (Prevents Continue() from being called when options are about to be shown).
        /// </summary>
        private bool allowAutoContinue = true;

        /// <summary>
        /// A mapping of Display IDs to Dialogue Panels for all subdisplays of the main Dialogue Display.
        /// </summary>
        protected Dictionary<string, DialoguePanel> subDisplayMap = new Dictionary<string, DialoguePanel>();

        /// <inheritdoc/>
        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// Initializes the dialogue display.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            InitializeOptionDisplay();
            InitializeContinueDisplay();
            InitializeCharacterDisplay();
            InitializeIconDisplay();
            IndexSubDisplays();
            Reset();
        }

        /// <summary>
        /// Called whenever a dialogue has begun playback. This will reset the dialogue display components to ready them for display.
        /// </summary>
        /// <param name="entryPointName">The name of the entry point where the playback began.</param>
        public override void OnDialogueEntered(string entryPointName)
        {
            base.OnDialogueEntered(entryPointName);

            if (dialogueController.GetConversationDisplay() != null)
            {
                SetConversationDisplay(dialogueController.GetConversationDisplay());
            }
            else if(dialogueController.GetConversationDisplayID() != null && dialogueController.GetConversationDisplayID().Length > 0)
            {
                UseConversationDisplayForTarget(dialogueController.GetConversationDisplayID());
            }
            else if(defaultConvoDisplayID != null && defaultConvoDisplayID.Length > 0)
            {
                UseConversationDisplayForTarget(defaultConvoDisplayID);
            }

            Reset();
        }

        /// <summary>
        /// Called whenever a dialogue exits. This will hide the display if hideDisplayOnExit is set to true; otherwise it just resets the display.
        /// </summary>
        /// <param name="exitPointName">The name of the exit point where the dialogue exited.</param>
        public override void OnDialogueExited(string exitPointName)
        {
            base.OnDialogueExited(exitPointName);

            StopAllCoroutines();

            if (hideDisplayOnExit)
            {
                Hide();
            }

            Reset();
        }

        /// <summary>
        /// Resets the display by disabling continuation and option selection, deactivating option buttons, and resetting the dialogue text.
        /// </summary>
        private void Reset()
        {
            this.DisableContinue();
            this.DisableOptionSelection();

            if (convoDisplay != null) 
            {
                convoDisplay.Reset();
            }

            if (optionDisplay != null) { optionDisplay.DeactivateButtons(); }
        }

        /// <summary>
        /// Hides the display components.
        /// </summary>
        public void Hide()
        {
            if (convoDisplay != null) { convoDisplay.Hide(); }
            if (optionDisplay != null) { optionDisplay.Hide(); }
            if (continueDisplay != null) { continueDisplay.Hide(); }
        }

        /// <summary>
        /// Displays the line of dialogue provided.
        /// </summary>
        /// <param name="currentLine">The line of dialogue to display.</param>
        public override void OnDisplayLine(ConversationLine currentLine)
        {
            base.OnDisplayLine(currentLine);

            if (autoplayRoutine != null)
            {
                StopAllCoroutines();
                convoDisplay.ForceFinish();
                autoplayRoutine = null;
            }

            ConversationLine oldLine = currentLine;
            this.currentLine = currentLine;

            if (currentLine.Target != null && currentTarget != currentLine.Target)
            {
                UseConversationDisplayForTarget(currentLine.Target);
            }

            if (dialogueController.GetPlaybackType() == PlaybackType.AUTOPLAY || currentLine.AutoPlay)
            {
                //Reset the allowAutoContinue flag.
                allowAutoContinue = true;

                //Display each line, and wait for a timer to finish or audio to complete before moving on to the next.
                autoplayRoutine = StartCoroutine(PlayAutoConversation(currentLine, oldLine));
            }
            else
            {
                DisplayLine(currentLine);
                dialogueController.SwitchLineAudio(currentLine, oldLine);
            }
        }

        /*public override void OnAIPromptStarted()
        {
            base.OnAIPromptStarted();

            loadingIcon.gameObject.SetActive(true);

            //If skipping AI conversations is allowed, allow continue.
            if(allowAISkip)
            {
                this.AllowContinue();
            }
        }

        public override void OnAIPromptFinished()
        {
            base.OnAIPromptFinished();

            loadingIcon.gameObject.SetActive(false);
        }

        public override void OnConversationClear()
        {
            base.OnConversationClear();

            convoDisplay.Reset();
        }*/

        /// <inheritdoc/>
        public override void OnAppendText(string text)
        {
            base.OnAppendText(text);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnAppendText(text);
            }
        }


        /// <summary>
        /// Handles the logic to display the specified line of Dialogue. This method also calls OnDisplayLine() on all Dialogue Listeners.
        /// </summary>
        /// <param name="currentLine">The line of dialogue to display.</param>
        private void DisplayLine(ConversationLine currentLine)
        {
            //Call and callbacks that are registered to let them know that a line of dialogue is being handled/displayed.
            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnDisplayLine(currentLine);
            }

            //If the convo display is hidden, we should wait until it's available for display to change the displayed text/character name
            //in case the display is in the middle of a transition.
            if (convoDisplay.IsHidden)
            {
                StartCoroutine(DisplayConversationLineAsync(currentLine));
            }
            else
            {
                if (refreshConvoOnTextChange)
                {
                    StartCoroutine(DisplayConversationLineAsync(currentLine, true));
                }
                else
                {
                    convoDisplay.Activate();

                    if (currentLine.TextDisplayMode == TextDisplayMode.REPLACE)
                    {
                        //Set the character name and text of the conversation display.
                        convoDisplay.SetCharacterName(currentLine.TranslatedCharacterName, currentLine.OriginalCharacterName);
                        convoDisplay.SetConversationText(currentLine);
                    }
                    else
                    {
                        convoDisplay.AppendText(currentLine);
                    }

                    if ((!currentLine.PrecedesOption || !PresentOptionsAutomatically) &&
                        ((dialogueController.GetPlaybackType() == PlaybackType.AUTOPLAY && areLinesSkippable) || (dialogueController.GetPlaybackType() == PlaybackType.WAIT_FOR_COMMAND)))
                    {
                        if (continuationMode == DialogueDelayMode.IMMEDIATE) { this.AllowContinue(); }
                        else { StartCoroutine(WaitForContinue()); }
                    }
                }
            }
        }

        /// <summary>
        /// Calls OnDisplayLine on all registered Dialogue Listeners to display the specified line of dialogue and continues automatically after a certain period of time,
        /// depending on any audio file attributed to the line of dialogue, timePerWord, convoLineDelay, and minConvoTime settings. After the necessary amount of time has elapsed, this method
        /// will trigger the OnContinue method of all registered Dialogue Listeners.
        /// </summary>
        /// <param name="lineToPlay">The line of dialogue to start from.</param>
        /// <param name="oldLine">The previous line of dialogue.</param>
        /// <returns></returns>
        protected virtual IEnumerator PlayAutoConversation(ConversationLine lineToPlay, ConversationLine oldLine = null)
        {
            //Determine how long the line of dialogue should be shown for, then display it.
            lineToPlay.PlayTime = GetTextPlayTime(lineToPlay.Text, lineToPlay.AudioClip);
            lineToPlay.AutoPlay = true;
            DisplayLine(lineToPlay);

            float startTime = Time.time;
            yield return WaitForDialogueAudioOrReading(lineToPlay, oldLine);

            //Add an extra delay for how long the line of dialogue should be displayed before continuing.
            if (lineToPlay.OverrideAutoplayDelay)
            {
                //Use the setting from the "autoplay" tag.
                yield return new WaitForSeconds(lineToPlay.AutoPlayDelay);
            }
            else
            {
                //Use the setting from the display.
                yield return new WaitForSeconds(convoLineDelay);
            }

            //If the line doesn't override the delay time, we should also wait for the line delay and the minimum conversation time before continuing.
            if (!lineToPlay.OverrideAutoplayDelay)
            {
                //Wait for the minimum amount of time that a line of dialogue is meant to be displayed.
                if (Time.time - startTime < minConvoTime)
                {
                    yield return new WaitForSeconds(minConvoTime - (Time.time - startTime));
                }
            }

            if (allowAutoContinue)
            {
                dialogueController.Continue();
            }
            else
            {
                allowAutoContinue = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineToPlay"></param>
        /// <param name="oldLine"></param>
        /// <returns></returns>
        private IEnumerator WaitForDialogueAudioOrReading(ConversationLine lineToPlay, ConversationLine oldLine)
        {
            //Stop any dialogue audio clip that is already playing.
            AudioSource source = dialogueController.GetAudioSource();
            if (source != null)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                    OnAudioCompleted(oldLine, true);
                }
            }

            //If there is an audio clip, play the audio clip and wait until it's over before continuing to the next line of dialogue.
            if (lineToPlay.AudioClip != null)
            {
                if (source != null)
                {
                    source.clip = lineToPlay.AudioClip;
                    source.Play();
                    OnAudioStarted(lineToPlay);
                }

                yield return new WaitForSeconds(lineToPlay.AudioClip.length);

                OnAudioCompleted(lineToPlay, false);
            }
            else
            {
                //Display the line of dialogue for a duration based on how many words are in the line.
                yield return new WaitForSeconds(lineToPlay.Text.Split(' ').Length * timePerWord);
            }
        }

        /// <summary>
        /// Asynchronously displays the line of dialogue provided, transitioning the conversation display to be shown if it is currently hidden.
        /// </summary>
        /// <param name="currentLine">The line of dialogue to display.</param>
        /// <param name="hideWhileUpdating">Whether the conversation display needs to be hidden prior to updating the dialogue text.</param>
        /// <returns></returns>
        private IEnumerator DisplayConversationLineAsync(ConversationLine currentLine, bool hideWhileUpdating = false)
        {
            if (hideWhileUpdating && !convoDisplay.IsHidden)
            {
                convoDisplay.Hide();
            }

            yield return convoDisplay.WaitForAnimation();

            convoDisplay.Activate();

            if (currentLine.TextDisplayMode == TextDisplayMode.REPLACE)
            {
                convoDisplay.SetCharacterName(currentLine.TranslatedCharacterName, currentLine.OriginalCharacterName);
                convoDisplay.SetConversationText(currentLine);
            }
            else
            {
                convoDisplay.AppendText(currentLine);
            }

            convoDisplay.Show();

            yield return convoDisplay.WaitForAnimation();

            if ((!currentLine.PrecedesOption || !PresentOptionsAutomatically) && 
                ((dialogueController.GetPlaybackType() == PlaybackType.AUTOPLAY && areLinesSkippable) || (dialogueController.GetPlaybackType() == PlaybackType.WAIT_FOR_COMMAND)))
            {
                if (continuationMode == DialogueDelayMode.IMMEDIATE) { this.AllowContinue(); }
                else { StartCoroutine(WaitForContinue()); }
            }
        }

        /// <summary>
        /// Searches for a conversation display with a Display ID the same as the target (if not null or empty) and sets the dialogue display to use that conversation
        /// display if found. This method will also tell the dialogue display to hide the current conversation display being displayed, if there is one.
        /// </summary>
        /// <param name="target">The display ID of the target conversation display.</param>
        private void UseConversationDisplayForTarget(string target)
        {
            if (target == null || target.Length == 0) { return; }

            if(currentTarget == target) { return; }

            if(convoDisplay != null && target.Equals(convoDisplay.DisplayID)) 
            {
                SetConversationDisplay(convoDisplay);
                return;
            }

            ConversationDisplay[] foundConvoDisplays = null;

#if UNITY_2022_3_OR_NEWER
            foundConvoDisplays = GameObject.FindObjectsByType<ConversationDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            foundConvoDisplays = GameObject.FindObjectsOfType<ConversationDisplay>(true);
#endif

            foreach (ConversationDisplay foundConvoDisplay in foundConvoDisplays)
            {
                if (foundConvoDisplay.DisplayID.ToLower().Equals(target.ToLower()))
                {
                    SetConversationDisplay(foundConvoDisplay);
                    break;
                }
            }
        }

        /// <summary>
        /// Determines the minimum recommended amount of time to display the specified dialogue text, based on the associated audio file, or if there is no associated audio
        /// file, based on the number of words in the line.
        /// </summary>
        /// <param name="text">The dialogue text to determine display time for.</param>
        /// <param name="clip">The audio clip associated with the text, if there is one.</param>
        private float GetTextPlayTime(string text, AudioClip clip)
        {
            float displayTime = convoLineDelay;

            if (clip != null)
            {
                displayTime = clip.length;
            }
            else
            {
                displayTime = text.Split(' ').Length * timePerWord;
            }

            return displayTime;
        }

        /// <inheritdoc/>
        public override void OnConversationEnding(ConversationLine line, Node nextNode)
        {
            base.OnConversationEnding(line, nextNode);

            //If the next node is an option node display the options.
            if (nextNode != null && nextNode.NodeType == NodeType.OPTION && presentOptionsAutomatically)
            {
                allowAutoContinue = false;

                if (displayOptionsAfterDelayRoutine == null)
                {
                    float delay = (currentLine.AudioClip != null) ? currentLine.AudioClip.length : optionDelay;

                    switch (optionDelayMode)
                    {
                        case DialogueDelayMode.IMMEDIATE: delay = 0.0f; break;
                        case DialogueDelayMode.AFTER_AUDIO: break;
                        case DialogueDelayMode.AFTER_DELAY: delay = optionDelay; break;
                        case DialogueDelayMode.AFTER_AUDIO_AND_DELAY: delay = Mathf.Max(delay, optionDelay); break;
                        case DialogueDelayMode.AFTER_AUDIO_OR_DELAY: delay = Mathf.Min(delay, optionDelay); break;
                    }

                    displayOptionsAfterDelayRoutine = StartCoroutine(dialogueController.GoToOptionsAfterDelay(delay, (OptionNode)nextNode));
                }
                else
                {
                    StopDelayedOptionDisplay();
                }
            }
        }

        /// <inheritdoc/>
        public override void OnNodeChanged(Node node)
        {
            base.OnNodeChanged(node);

            StopDelayedOptionDisplay();
        }

        /// <summary>
        /// Waits to allow the player to continue based on the dialogue delay mode set on this display.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForContinue()
        {
            float startTime = Time.time;

            yield return new WaitForEndOfFrame();
            
            if (continuationMode == DialogueDelayMode.AFTER_AUDIO || continuationMode == DialogueDelayMode.AFTER_AUDIO_AND_DELAY)
            {
                //Wait for audio completion.
                while (dialogueController.IsAudioPlaying())
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            
            if (continuationMode == DialogueDelayMode.AFTER_DELAY || continuationMode == DialogueDelayMode.AFTER_AUDIO_AND_DELAY)
            {
                //Wait for timer completion.
                while (Time.time - startTime < continuationDelay)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            
            if (continuationMode == DialogueDelayMode.AFTER_AUDIO_OR_DELAY)
            {
                //Wait for audio or timer completion
                while (((currentLine != null && currentLine.AudioClip == null) || 
                    dialogueController.IsAudioPlaying()) && Time.time - startTime < continuationDelay)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            AllowContinue();
        }

        /// <summary>
        /// Displays the List of dialogue options to the player.
        /// </summary>
        /// <param name="options">The List of dialogue options to display.</param>
        public override void OnDisplayOptions(List<DialogueOption> options)
        {
            base.OnDisplayOptions(options);

            displayedOptions = new List<DialogueOption>();
            foreach (DialogueOption option in options)
            {
                if (option.IsDisplayed)
                {
                    displayedOptions.Add(option);
                }
            }

            if (displayedOptions.Count > 0)
            {
                foreach (DialogueListener listener in dialogueListeners)
                {
                    listener.OnDisplayOptions(displayedOptions);
                }

                if (optionDisplay.IsHidden)
                {
                    StartCoroutine(DisplayOptionsAsync(displayedOptions));
                }

                if(clearConvoWhenShowingOptions)
                {
                    convoDisplay.Reset();
                }

                if (hideConvoWhenShowingOptions)
                {
                    convoDisplay.Hide();
                }
            }
            else
            {
                dialogueController.ExitDialogue();
            }
        }

        /// <summary>
        /// Asynchronously displays the provided list of dialogue options to the player and enables option selection once the options are displayed.
        /// </summary>
        /// <param name="options">The list of dialogue options to display.</param>
        /// <returns></returns>
        private IEnumerator DisplayOptionsAsync(List<DialogueOption> options)
        {
            yield return optionDisplay.WaitForAnimation();
            optionDisplay.Activate();
            this.DisableContinue();
            optionDisplay.SetOptions(options);
            optionDisplay.Show();

            yield return optionDisplay.WaitForAnimation();
            optionDisplay.ActivateButtons();
            this.AllowOptionSelection();
        }

        /// <summary>
        /// Stops an in-progress automatic option presentation from being triggered.
        /// </summary>
        private void StopDelayedOptionDisplay()
        {
            if (displayOptionsAfterDelayRoutine != null)
            {
                StopCoroutine(displayOptionsAfterDelayRoutine);
                displayOptionsAfterDelayRoutine = null;
            }
        }

        /// <inheritdoc/>
        public override bool SelectOptionInDirection(Vector2 direction)
        {
            return optionDisplay.SelectOptionInDirection(direction);
        }

        /// <inheritdoc/>
        public override void ChooseSelectedOption()
        {
            optionDisplay.Hide();

            int selectedOptionIdx = optionDisplay.GetSelectedOption();

            DialogueOption option = GetOptionWithIndex(selectedOptionIdx);
            dialogueController.ChooseOption(option);
            displayedOptions = null;

            optionDisplay.DeactivateButtons();
            this.DisableOptionSelection();
        }

        /// <summary>
        /// Returns the DialogueOption with the specified option index. NOTE: This is NOT the same as the order of options displayed, since some options may be hidden. 
        /// Instead, it is the original option index assigned to the option prior to filtering.
        /// </summary>
        /// <param name="optionIdx">The option index to retrieve the DialogueOption of.</param>
        /// <returns>The DialogueOption with the specified index.</returns>
        private DialogueOption GetOptionWithIndex(int optionIdx)
        {
            foreach (DialogueOption option in displayedOptions)
            {
                if (option.OptionIndex == optionIdx)
                {
                    return option;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool SelectNextOption()
        {
            return optionDisplay.SelectNextOption();
        }

        /// <inheritdoc/>
        public override bool SelectPreviousOption()
        {
            return optionDisplay.SelectPreviousOption();
        }

        /// <summary>
        /// This method is called whenever a character change is detected during dialogue playback.
        /// </summary>
        /// <param name="oldCharacterName">The name of the previous character.</param>
        /// <param name="newCharacterName">The name of the new character.</param>
        public override void OnCharacterChanged(string oldCharacterName, string newCharacterName)
        {
            base.OnCharacterChanged(oldCharacterName, newCharacterName);

            if (switchConvoDisplayOnCharacterChange)
            {
                UseConversationDisplayForTarget(newCharacterName);
            }

            if (refreshConvoOnCharacterChange && 
                oldCharacterName != null && newCharacterName != null && 
                oldCharacterName.Length > 0 && newCharacterName.Length > 0)
            {
                this.DisableContinue();
                convoDisplay.Hide();
            }
        }

        /// <inheritdoc/>
        public override void OnContinue()
        {
            base.OnContinue();

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnContinue();
            }

            if (!areLinesSkippable && dialogueController.GetPlaybackType() == PlaybackType.AUTOPLAY) { return; }

            //If we are auto-playing a conversation and allowing lines to be skipped, stop the auto-play.
            if (autoplayRoutine != null && areLinesSkippable)
            {
                StopAllCoroutines();
                autoplayRoutine = null;
            }

            convoDisplay.ForceFinish();
        }

        /// <inheritdoc/>
        public override void OnStory(string storyText)
        {
            base.OnStory(storyText);
            
            if(continueOnStory && dialogueController != null)
            {
                dialogueController.Continue();
            }
        }

        /// <inheritdoc/>
        public override ConversationDisplay GetConversationDisplay() { return convoDisplay; }

        /// <inheritdoc/>
        public override void SetConversationDisplay(ConversationDisplay convoDisplay)
        {
            if (this.convoDisplay != null && this.convoDisplay != convoDisplay)
            {
                this.convoDisplay.Hide();
            }

            this.convoDisplay = convoDisplay;

            if (this.convoDisplay != null)
            {
                currentTarget = this.convoDisplay.DisplayID;
                UpdateFontsForLanguage(this.convoDisplay);
            }
        }

        /// <inheritdoc/>
        public override OptionDisplay GetOptionDisplay() { return optionDisplay; }

        /// <inheritdoc/>
        public override void SetOptionDisplay(OptionDisplay optionDisplay)
        {
            if (this.optionDisplay != null && this.optionDisplay != optionDisplay)
            {
                this.optionDisplay.Hide();
                DeinitializeOptionDisplay();
            }

            this.optionDisplay = optionDisplay;
            
            if (this.optionDisplay != null)
            {
                InitializeOptionDisplay();
                UpdateFontsForLanguage(this.optionDisplay);
            }
        }

        /// <inheritdoc/>
        public override ContinueDisplay GetContinueDisplay() { return continueDisplay; }

        /// <inheritdoc/>
        public override void SetContinueDisplay(ContinueDisplay continueDisplay)
        {
            if (this.continueDisplay != null && this.continueDisplay != continueDisplay)
            {
                this.continueDisplay.Hide();
                DeinitializeContinueDisplay();
            }

            this.continueDisplay = continueDisplay;

            if (this.continueDisplay != null)
            {
                InitializeContinueDisplay();
                UpdateFontsForLanguage(this.continueDisplay);
            }
        }

        /// <inheritdoc/>
        public override TextInputDisplay GetTextInputDisplay()
        {
            return this.textInputDisplay;
        }

        /// <inheritdoc/>
        public override void SetTextInputDisplay(TextInputDisplay textInputDisplay)
        {
            this.textInputDisplay = textInputDisplay;

            if (this.textInputDisplay != null)
            {
                UpdateFontsForLanguage(this.textInputDisplay);
            }
        }

        /// <inheritdoc/>
        public override void OnExecuteAsyncNode(AsyncNode node)
        {
            base.OnExecuteAsyncNode(node);

            if (node is PlayerInputNode)
            {
                //Show the text input display and wait for the player to input text before continuing.
                textInputDisplay.SetInputNode(node as PlayerInputNode);
                textInputDisplay.Reset();
                textInputDisplay.HideOnAwake = false;
                textInputDisplay.Show();
            }
            else if(node is ShowNode)
            {
                //Find the display for each target and show them all before continuing.
                DisplayUtils.HandleShowNode(node, subDisplayMap, DialogueSettings);
            }
            else if(node is HideNode)
            {
                //Find the display for each target and hide them all before continuing.
                DisplayUtils.HandleHideNode(node, subDisplayMap, DialogueSettings);
            }
        }

        /// <summary>
        /// Sets up the option display to trigger the ChooseSelectedOption method whenever an option is chosen.
        /// </summary>
        private void InitializeOptionDisplay()
        {
            if (optionDisplay != null)
            {
                optionDisplay.onOptionChosen.AddListener(ChooseSelectedOption);
            }
        }

        /// <summary>
        /// Removes the ChooseSelectedOption method from being triggered by the current option display.
        /// </summary>
        private void DeinitializeOptionDisplay()
        {
            if (optionDisplay != null)
            {
                optionDisplay.onOptionChosen.RemoveListener(ChooseSelectedOption);
            }
        }


        /// <summary>
        /// Sets up the continue display to tell the dialogue display that it is ready for dialogue continuation when the continue display is shown.
        /// </summary>
        private void InitializeContinueDisplay()
        {
            if (continueDisplay != null)
            {
                continueDisplay.onShowComplete.AddListener(base.AllowContinue);
            }
        }

        /// <summary>
        /// Removes the listener which allows continuation after the continue display is shown.
        /// </summary>
        private void DeinitializeContinueDisplay()
        {
            if (continueDisplay != null)
            {
                continueDisplay.onShowComplete.RemoveListener(base.AllowContinue);
            }
        }

        /// <summary>
        /// Adds the character display to the dialogue display's list of dialogue listeners.
        /// </summary>
        private void InitializeCharacterDisplay()
        {
            if (characterDisplay != null)
            {
                dialogueListeners.Add(characterDisplay);
            }
        }

        /// <summary>
        /// Adds the icon display to the dialogue display's list of dialogue listeners, and adds it as a conversation display listener to the conversation display.
        /// </summary>
        private void InitializeIconDisplay()
        {
            if (iconDisplay != null)
            {
                dialogueListeners.Add(iconDisplay);

                if(convoDisplay != null)
                {
                    IconConversationDisplayListener iconConvoListener = iconDisplay.GetComponent<IconConversationDisplayListener>();

                    if(iconConvoListener != null)
                    {
                        convoDisplay.ConversationDisplayListeners.Add(iconConvoListener);
                    }
                }
            }
        }

        /// <summary>
        /// Finds all Dialogue Panels which are children of the display and adds them to a Map for quick searching of sub displays.
        /// </summary>
        private void IndexSubDisplays()
        {
            DialoguePanel[] spritePanels = GetComponentsInChildren<DialoguePanel>(true);
            foreach (DialoguePanel panel in spritePanels)
            {
                if (panel.DisplayID != null && panel.DisplayID.Length > 0 && !subDisplayMap.ContainsKey(panel.DisplayID))
                {
                    subDisplayMap.Add(panel.DisplayID, panel);
                }
            }
        }

        /// <summary>
        /// Finds and returns the Dialogue Display parent of the specified GameObject, if there is one.
        /// </summary>
        /// <param name="obj">The GameObject to find the parent Dialogue Display of.</param>
        /// <returns>The parent Dialogue Display of the specified GameObject.</returns>
        public static DialogueDisplay GetParentDialogueDisplay(GameObject obj)
        {
            return obj.GetComponentInParent<DialogueDisplay>();
        }

        /// <inheritdoc/>
        public override void OnPause(string signal)
        {
            base.OnPause(signal);

            if(hideOnPause)
            {
                convoDisplay.Hide();
            }
        }

        /// <inheritdoc/>
        public override void AllowContinue()
        {
            if (continueDisplay != null && useContinueDisplay)
            {
                continueDisplay.Show();
            }
            else
            {
                base.AllowContinue();
            }
        }

        /// <inheritdoc/>
        public override void DisableContinue()
        {
            base.DisableContinue();

            if(continueDisplay != null)
            {
                continueDisplay.Hide(false);
            }
        }

        /// <summary>
        /// Gets or sets the dialogue continuation mode to use.
        /// </summary>
        public DialogueDelayMode ContinuationMode
        {
            get { return continuationMode; }
            set { this.continuationMode = value; }
        }

        /// <summary>
        /// Gets or sets the dialogue continuation delay.
        /// </summary>
        public float ContinuationDelay
        {
            get { return this.continuationDelay; }
            set { this.continuationDelay = value; }
        }

        /// <summary>
        /// Gets or sets whether the dialogue display should use a continue display during dialogue playback.
        /// </summary>
        public bool UseContinueDisplay
        {
            get { return this.useContinueDisplay; }
            set { this.useContinueDisplay = value; }
        }

        /// <summary>
        /// Gets or sets whether the conversation display should be switched when a character change is detected.
        /// </summary>
        public bool SwitchConvoDisplayOnCharacterChange
        {
            get { return this.switchConvoDisplayOnCharacterChange; }
            set { this.switchConvoDisplayOnCharacterChange = value; }
        }

        /// <summary>
        /// Gets or sets whether options should be presented automatically as a conversation node is ending.
        /// </summary>
        public bool PresentOptionsAutomatically
        {
            get { return this.presentOptionsAutomatically; }
            set { this.presentOptionsAutomatically = value; }
        }

        /// <summary>
        /// Gets or sets the delay mode to use when presenting options.
        /// </summary>
        public DialogueDelayMode OptionDelayMode
        {
            get { return this.optionDelayMode; }
            set { this.optionDelayMode = value; }
        }
    }

    /// <summary>
    /// An enum defining various action delay modes for a Dialogue Display.
    /// </summary>
    public enum DialogueDelayMode { IMMEDIATE, AFTER_AUDIO, AFTER_DELAY, AFTER_AUDIO_AND_DELAY, AFTER_AUDIO_OR_DELAY }
}