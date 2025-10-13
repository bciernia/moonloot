using EasyTalk.Display;
using EasyTalk.Nodes;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Variable;
using EasyTalk.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyTalk.Controller
{
    /// <summary>
    /// Controller for managing dialogue flow between a dialogue display and a dialogue node handler.
    /// </summary>
    public class DialogueController : DialogueListener
    {
        /// <summary>
        /// The node handler used to process the Dialogue.
        /// </summary>
        private NodeHandler nodeHandler;

        /// <summary>
        /// The Dialogue to use.
        /// </summary>
        [SerializeField]
        [Tooltip("The dialogue to run.")]
        protected Dialogue dialogue;

        /// <summary>
        /// The Dialogue Registry to use when initializing global variables.
        /// </summary>
        [SerializeField]
        [Tooltip("(OPTIONAL) The Dialogue Registry to initialze global variables from. If this is not set, global variables can still be loaded from the registry set on " +
            "Dialogue Settings of the Dialogue Display.")]
        protected DialogueRegistry dialogueRegistry;

        /// <summary>
        /// The playback type of the controller.
        /// </summary>
        [SerializeField]
        [Tooltip("AUTOPLAY mode will automatically cycle through lines of dialog and only stop when presenting options to the user." +
            "\nWAIT_FOR_COMMAND mode waits for user input to move to the next line of dialogue.")]
        protected PlaybackType playbackType;

        /// <summary>
        /// Determines whether the controller will attempt to register and use a Dialogue Display automatically.
        /// </summary>
        [SerializeField]
        [Tooltip("If unchecked (set to false), the controller will process Dialogue and send events to Dialogue Listeners, but will not automatically register a Dialogue Display. Continue() must be called on the" +
            "controller to progress it after lines of dialogue are displayed, and options must be handled when necessary.")]
        private bool useDialogueDisplay = true;

        /// <summary>
        /// The dialogue display currently being used by this controller.
        /// </summary>
        [SerializeField]
        [Tooltip("(OPTIONAL) The dialogue display UI to use when presenting dialogue and options to the player. If left empty, the controller " +
            "will attempt to find a Dialogue Display when PlayDialogue() is called.")]
        protected AbstractDialogueDisplay dialogueDisplay;

        /// <summary>
        /// The converstion display to use when playing dialogue.
        /// </summary>
        [SerializeField]
        [Tooltip("(OPTIONAL) The Conversation Display to use when PlayDialogue() is called on this controller.")]
        protected ConversationDisplay conversationDisplay = null;

        /// <summary>
        /// The ID of the conversation display to use when initializing a conversation on a dialogue display and no conversation display is set.
        /// </summary>
        [SerializeField]
        [Tooltip("(OPTIONAL) The Display ID of the conversation display to set when first initializing a conversation on the dialogue display.")]
        protected string conversationDisplayID = null;

        /// <summary>
        /// The audio source to use for playing lines of dialogue.
        /// </summary>
        [SerializeField]
        [Tooltip("(OPTIONAL) The audio source to use when playing audio clips attached to dialogue.")]
        protected AudioSource audioSource;

        /// <summary>
        /// A collection of DialogueListeners to call as dialogue playback occurs.
        /// </summary>
        [Tooltip("The Dialogue Controller will call the relevant methods on all Dialogue Listeners in this list as events occur during dialogue playback.")]
        [NonReorderable]
        [SerializeField]
        protected List<DialogueListener> dialogueListeners = new List<DialogueListener>();

        /// <summary>
        /// An event which is triggered when dialogue playback starts.
        /// </summary>
        [Tooltip("This event is triggered when dialogue playback starts.")]
        [SerializeField]
        [HideInInspector]
        public UnityEvent onPlay = new UnityEvent();

        /// <summary>
        /// An event which is triggered when dialogue playback stops.
        /// </summary>
        [Tooltip("This event is triggered when the dialogue playback stops.")]
        [SerializeField]
        [HideInInspector]
        public UnityEvent onStop = new UnityEvent();

        /// <summary>
        /// Whether the dialogue is currently being played.
        /// </summary>
        protected bool isRunning = false;

        /// <summary>
        /// A coroutine for playing audio for a line of dialogue.
        /// </summary>
        private Coroutine audioRoutine = null;

        /// <summary>
        /// Whetehr null keys should be sent when no key exists on a line of dialogue.
        /// </summary>
        private bool sendNullKeys = true;

        /// <summary>
        /// A flag to keep track of whether audio is currently playing.
        /// </summary>
        private bool isAudioPlaying = false;

        /// <summary>
        /// The name of the character which dialogue is currently being shown for.
        /// </summary>
        private string currentCharacterName = null;

        /// <inheritdoc/>
        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// Calls Update() on the node handler.
        /// </summary>
        private void Update()
        {
            if (nodeHandler != null)
            {
                nodeHandler.Update();
            }
        }

        /// <summary>
        /// Initializes the controller to prepare it for playing dialogue.
        /// </summary>
        protected virtual void Init()
        {
            nodeHandler = new NodeHandler(this.gameObject);
            nodeHandler.SetListener(this);

            if (dialogue != null)
            {
                nodeHandler.Initialize(dialogue);
            }

            if (dialogueDisplay == null && useDialogueDisplay)
            {
                dialogueDisplay = FindDialogueDisplay();
            }
        }

        /// <summary>
        /// Starts playback of the dialogue at the specified entry node.
        /// </summary>
        /// <param name="entryPointName">The name of the entry point ID where the dialogue should start from.</param>
        public void PlayDialogue(string entryPointName = null)
        {
            StopAllCoroutines();

            if(dialogueDisplay == null && useDialogueDisplay)
            {
                dialogueDisplay = FindDialogueDisplay();
            }

            if (dialogueDisplay != null && useDialogueDisplay)
            {
                if (dialogueDisplay.IsPlaying)
                {
                    dialogueDisplay.ExitDialogue();
                }

                dialogueDisplay.SetActiveDialogueController(this);
                nodeHandler.DialogueSettings = dialogueDisplay.DialogueSettings;

                //If no Dialogue Registry was set on the controller, use the one from the dialogue display
                if (dialogueRegistry == null && dialogueDisplay.DialogueSettings != null) { dialogueRegistry = dialogueDisplay.DialogueSettings.DialogueRegistry; }
            }

            //Initialize global variables on the node handler using the current dialogue registry, if there is one.
            nodeHandler.InitializeGlobalVariables(dialogueRegistry);

            isRunning = true;

            nodeHandler.EnterDialogue(entryPointName);
        }

        /// <summary>
        /// Changes the Dialogue used by the controller.
        /// </summary>
        /// <param name="dialogue">The Dialogue to use.</param>
        public void ChangeDialogue(Dialogue dialogue)
        {
            this.dialogue = dialogue;
            nodeHandler.Initialize(dialogue);
        }

        /// <summary>
        /// Finds a dialogue display to use.
        /// </summary>
        private AbstractDialogueDisplay FindDialogueDisplay()
        {
            if (dialogueDisplay == null)
            {
                if(AbstractDialogueDisplay.Instance != null)
                {
                    return AbstractDialogueDisplay.Instance;
                }
                else
                {
#if UNITY_2022_3_OR_NEWER
                    return GameObject.FindFirstObjectByType<DialogueDisplay>(FindObjectsInactive.Include);
#else
                    return GameObject.FindObjectOfType<DialogueDisplay>(true);
#endif
                }
            }
            
            return dialogueDisplay;
        }

        /// <summary>
        /// Exits the dialogue playback immediately.
        /// </summary>
        public void ExitDialogue()
        {
            bool running = isRunning;
            isRunning = false;

            if (running != isRunning && nodeHandler != null)
            {
                nodeHandler.ForceExit();
            }
        }

        /// <summary>
        /// Called when dialogue playback begins. This will switch conversation displays used if set to do so and call OnDialogueEntered on all Dialogue Listeners registered with the controller.
        /// </summary>
        /// <param name="entryPointName">The name of the entry point where the Dialogue playback began.</param>
        public override void OnDialogueEntered(string entryPointName)
        {
            base.OnDialogueEntered(entryPointName);

            if (onPlay != null)
            {
                onPlay.Invoke();
            }

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnDialogueEntered(entryPointName);
            }
        }

        /// <summary>
        /// Called when dialogue playback exits. This will stop the autoplay routine and call OnDialogueExited on all registered Dialogue Listeners.
        /// </summary>
        /// <param name="exitPointName">The name of the exit point where the dialogue exited.</param>
        public override void OnDialogueExited(string exitPointName)
        {
            base.OnDialogueExited(exitPointName);

            isRunning = false;

            if (onStop != null)
            {
                onStop.Invoke();
            }

            foreach (DialogueListener listener in dialogueListeners.ToList())
            {
                listener.OnDialogueExited(exitPointName);
            }

            if (this.gameObject.activeSelf)
            {
                StartCoroutine(ExitCompleted());
            }
        }

        /// <summary>
        /// Called at the end of OnDialogueExited(). Waits for one frame and then calls OnExitCompleted on this controller and all registered dialogue listeners.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExitCompleted()
        {
            yield return new WaitForEndOfFrame();

            base.OnExitCompleted();

            foreach (DialogueListener listener in dialogueListeners.ToList())
            {
                listener.OnExitCompleted();
            }
        }

        /// <summary>
        /// Called whenever options are to be presented. This method filters out options which are set to not be displayed before sending the filtered list on to
        /// the OnDisplayOptions method on all registered Dialogue Listeners.
        /// </summary>
        /// <param name="options">The list of dialogue options to present.</param>
        public override void OnDisplayOptions(List<DialogueOption> options)
        {
            base.OnDisplayOptions(options);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnDisplayOptions(options);
            }
        }

        /// <summary>
        /// Called whenever a variable value is updated in the dialogue. This method will trigger the OnVariableUpdated method on all registered Dialogue Listeners.
        /// </summary>
        /// <param name="variableName">The name of the variable which was updated.</param>
        /// <param name="variableValue">The new value of the variable.</param>
        public override void OnVariableUpdated(string variableName, object variableValue)
        {
            base.OnVariableUpdated(variableName, variableValue);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnVariableUpdated(variableName, variableValue);
            }
        }

        /// <summary>
        /// Called whenever a line of dialogue is to be displayed. This method does the following:
        /// <list type="bullet">
        /// <item>Stops the current autoplay routine if it is running.</item>
        /// <item>Switches conversation displays if the controller is set to do so, such as when the character changes or a target tag is specified on the l ine of dialogue.</item>
        /// <item>Send a key tag value to any registered Dialogue Listeners via the OnActivateKey method.</item>
        /// <item>Starts the autoplay routine or calls the OnDisplayLine method on all registered Dialogue Listeners.</item>
        /// <item>Switches the audio to play audio for the new line of dialogue.</item>
        /// </list>
        /// </summary>
        /// <param name="line">The line of dialogue to display.</param>
        public override void OnDisplayLine(ConversationLine line)
        {
            base.OnDisplayLine(line);

            if ((currentCharacterName == null && line.OriginalCharacterName != null) ||
                    (currentCharacterName != null && line.OriginalCharacterName == null) ||
                    !currentCharacterName.Equals(line.OriginalCharacterName))
            {
                    CharacterChanged(line.OriginalCharacterName);
            }

            ActivateKey(line);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnDisplayLine(line);
            }
        }

        /// <summary>
        /// Activates the key of the specified line of dialogue by sending its key value to all registered Dialogue Listeners by calling their OnActivateKey methods.
        /// </summary>
        /// <param name="line">The line of dialogue to activate the key of.</param>
        protected virtual void ActivateKey(ConversationLine line)
        {
            if (line.Key != null || sendNullKeys)
            {
                base.OnActivateKey(line.Key);

                foreach(DialogueListener listener in dialogueListeners)
                {
                    listener.OnActivateKey(line.Key);
                }
            }
        }

        /// <summary>
        /// Stops any audio currently playing and starts playing audio for the provided line of dialogue.
        /// </summary>
        /// <param name="line">The line of dialogue to play audio for.</param>
        /// <param name="previousLine">The line of dialogue which was last displayed.</param>
        public void SwitchLineAudio(ConversationLine line, ConversationLine previousLine)
        {
            AudioSource source = GetAudioSource();

            if (source != null)
            {
                //Stop any currently playing audio clip.
                if (source.isPlaying)
                {
                    source.Stop();
                    OnAudioCompleted(previousLine, true);
                }

                //Cancel any pending coroutine that might be waiting to stop audio from playing.
                if (audioRoutine != null)
                {
                    StopCoroutine(audioRoutine);
                    audioRoutine = null;
                }

                //Start a new coroutine to play audio.
                audioRoutine = StartCoroutine(PlayAudioClip(line));
            }
        }

        /// <summary>
        /// Plays the audio clip for the specified line of dialogue.
        /// </summary>
        /// <param name="line">The line of dialogue to play audio for.</param>
        /// <returns></returns>
        protected IEnumerator PlayAudioClip(ConversationLine line)
        {
            AudioSource source = GetAudioSource();

            if (line.AudioClip != null)
            {
                source.clip = line.AudioClip;
                source.Play();
                OnAudioStarted(line);

                yield return new WaitForSeconds(line.AudioClip.length);

                OnAudioCompleted(line, false);
            }
        }

        /// <summary>
        /// Called whenever a character change is detected. If this controller is set to switch conversation displays on character change, it will attempt to do so. Additionally, 
        /// any Dialogue Listeners registered will have their OnCharacterChanged method triggered.
        /// </summary>
        /// <param name="newCharacterName">The name of the character being switched to.</param>
        public void CharacterChanged(string newCharacterName)
        {
            base.OnCharacterChanged(this.currentCharacterName, newCharacterName);

            string oldCharacterName = this.currentCharacterName;
            this.currentCharacterName = newCharacterName;

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnCharacterChanged(oldCharacterName, newCharacterName);
            }
        }

        /// <summary>
        /// Finds and returns an audio source on this component or its child components. If the audio source is set on the controller, it will use that audio source instead of 
        /// searching components for an audio source.
        /// </summary>
        /// <returns>An AudioSource on this GameObject or from its children if none exists on this GameObject.</returns>
        public AudioSource GetAudioSource()
        {
            AudioSource source = audioSource;

            if (source == null)
            {
                source = GetComponentInChildren<AudioSource>();
            }

            if(source == null && dialogueDisplay != null)
            {
                source = dialogueDisplay.GetComponent<AudioSource>();
            }

            if (source == null)
            {
#if UNITY_2022_3_OR_NEWER
                source = GameObject.FindFirstObjectByType<AudioSource>(FindObjectsInactive.Exclude);
#else
                source = GameObject.FindObjectOfType<AudioSource>();
#endif
            }

            return source;
        }

        /// <summary>
        /// Returns whether audio is currently being played by the controller.
        /// </summary>
        /// <returns>Whether audio is currently being played by the controller.</returns>
        public bool IsAudioPlaying()
        {
            return this.isAudioPlaying;
        }

        /// <summary>
        /// Called whenever the last line of dialogue in a conversation node is reached. If the next node is an option node, and options are set to be presented 
        /// automatically, then the DisplayOptionsAfterDelay coroutine will be started to display options based on the delay settings. THis method also calls OnConversationEnding on all
        /// registered Dialogue Listeners.
        /// </summary>
        /// <param name="currentLine">The current line of dialogue being displayed.</param>
        /// <param name="nextNode">The next node to move to after the current conversation node.</param>
        public override void OnConversationEnding(ConversationLine currentLine, Node nextNode)
        {
            base.OnConversationEnding(currentLine, nextNode);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnConversationEnding(currentLine, nextNode);
            }
        }

        /// <summary>
        /// Tells the node handler to jump to the specified option node after a certain period of time.
        /// </summary>
        /// <param name="delay">The delay, in seconds.</param>
        /// <param name="optionNode">The option node to go to.</param>
        /// <returns></returns>
        public virtual IEnumerator GoToOptionsAfterDelay(float delay, OptionNode optionNode)
        {
            yield return nodeHandler.JumpToNodeAfterDelay(delay, optionNode);
        }

        /// <summary>
        /// Makes the dialogue continue forward from the current point. This method will also 
        /// trigger the OnContinue method on all registered Dialogue Listeners.
        /// 
        /// Note that a continuation is only allowed when the current node is a converation, story, or wait node.
        /// </summary>
        public void Continue()
        {
            if (!isRunning) { return; }

            base.OnContinue();

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnContinue();
            }

            nodeHandler.Continue();
        }

        /// <summary>
        /// Called whenever audio playback starts for a line of dialogue. This will call the OnAudioStarted method of all registered Dialogue Listeners.
        /// </summary>
        /// <param name="line">The line of dialogue audio started for.</param>
        public override void OnAudioStarted(ConversationLine line)
        {
            base.OnAudioStarted(line);

            isAudioPlaying = true;

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnAudioStarted(line);
            }
        }

        /// <summary>
        /// Called whenever audio playback finishes or is interrupted for a line of dialogue. This will call the OnAudioCompleted method of all registered Dialogue Listeners.
        /// </summary>
        /// <param name="line">The line of dialogue which audio ended for.</param>
        /// <param name="forced">Whether the audio was forcibly interrupted.</param>
        public override void OnAudioCompleted(ConversationLine line, bool forced)
        {
            base.OnAudioCompleted(line, forced);

            isAudioPlaying = false;
            
            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnAudioCompleted(line, forced);
            }
        }

        /// <summary>
        /// Called whenever the current node being processed changes.
        /// </summary>
        /// <param name="newNode">The new node.</param>
        public override void OnNodeChanged(Node newNode)
        {
            base.OnNodeChanged(newNode);
            
            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnNodeChanged(newNode);
            }
        }

        /// <summary>
        /// Chooses the specified option and calls the OnOptionChosen method of all registered Dialogue Listeners.
        /// </summary>
        /// <param name="option">The option to choose.</param>
        public void ChooseOption(DialogueOption option)
        {
            if (!isRunning) { return; }

            base.OnOptionChosen(option);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnOptionChosen(option);
            }

            nodeHandler.ChooseOption(option.OptionIndex);
        }

        /// <summary>
        /// Creates a coroutine to wait for the specified duration before continuing. This method also calls the Wait method on all registered Dialogue Listeners.
        /// </summary>
        /// <param name="timeInSeconds">The amount of time to wait, in seconds.</param>
        public override void Wait(float timeInSeconds)
        {
            base.Wait(timeInSeconds);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.Wait(timeInSeconds);
            }

            StartCoroutine(WaitAsync(timeInSeconds));
        }

        /// <summary>
        /// Waits for the specified duration before continuing.
        /// </summary>
        /// <param name="timeInSeconds">The amount of time to wait, in seconds.</param>
        /// <returns></returns>
        private IEnumerator WaitAsync(float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);

            nodeHandler.Continue();
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
        public override void OnWaitingForNodeEvaluation(Node asyncNode)
        {
            base.OnWaitingForNodeEvaluation(asyncNode);

            foreach(DialogueListener listener in dialogueListeners)
            {
                listener.OnWaitingForNodeEvaluation(asyncNode);
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

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnAppendText(text);
            }
        }

        /// <inheritdoc/>
        public override void OnNodeEvaluationCompleted(Node asyncNode)
        {
            base.OnNodeEvaluationCompleted(asyncNode);

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnNodeEvaluationCompleted(asyncNode);
            }
        }

        /*
        public override void OnAIPromptStarted()
        {
            base.OnAIPromptStarted();

            foreach (DialogueListener listener in dialogueListeners)
            {
                listener.OnAIPromptStarted();
            }
        }

        public override void OnAIPromptFinished()
        {
            base.OnAIPromptFinished();

            foreach (DialogueListener listener in dialogueListeners)
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
        /// Returns the node handler used by this controller.
        /// </summary>
        /// <returns>The node handler being used.</returns>
        public NodeHandler GetNodeHandler()
        {
            return nodeHandler;
        }

        /// <summary>
        /// Returns the conversation display to use when entering dialogue playback via this controller.
        /// </summary>
        /// <returns>The conversation display to use.</returns>
        public ConversationDisplay GetConversationDisplay()
        {
            return conversationDisplay;
        }

        /// <summary>
        /// Returns the Display ID of the conversation display to use when entering dialogue playback via this controller.
        /// </summary>
        /// <returns></returns>
        public string GetConversationDisplayID()
        {
            if(conversationDisplay != null)
            {
                return conversationDisplay.DisplayID;
            }

            return conversationDisplayID;
        }

        /// <summary>
        /// Sets the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void SetStringVariable(string variableName, string value)
        {
            SetVariableValue(variableName, value, typeof(string));
        }

        /// <summary>
        /// Returns the string value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve the value of.</param>
        /// <returns>The string value of the specified variable.</returns>
        public string GetStringVariable(string variableName)
        {
            return (string)GetVariableValue(variableName, typeof(string));
        }

        /// <summary>
        /// Sets the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void SetBoolVariable(string variableName, bool value)
        {
            SetVariableValue(variableName, value, typeof(bool));
        }

        /// <summary>
        /// Returns the bool value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve the value of.</param>
        /// <returns>The bool value of the specified variable.</returns>
        public bool GetBoolVariable(string variableName)
        {
            return (bool)GetVariableValue(variableName, typeof(bool));
        }

        /// <summary>
        /// Sets the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void SetIntVariable(string variableName, int value)
        {
            SetVariableValue(variableName, value, typeof(int));
        }

        /// <summary>
        /// Returns the int value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve the value of.</param>
        /// <returns>The int value of the specified variable.</returns>
        public int GetIntVariable(string variableName)
        {
            return (int)GetVariableValue(variableName, typeof(int));
        }

        /// <summary>
        /// Sets the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        public void SetFloatVariable(string variableName, float value)
        {
            SetVariableValue(variableName, value, typeof(float));
        }

        /// <summary>
        /// Returns the float value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve the value of.</param>
        /// <returns>The float value of the specified variable.</returns>
        public float GetFloatVariable(string variableName)
        {
            return (float)GetVariableValue(variableName, typeof(float));
        }

        /// <summary>
        /// Sets the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="valueType">The type of the variable.</param>
        /// <exception cref="Exception"></exception>
        private void SetVariableValue(string variableName, object value, Type valueType)
        {
            NodeVariable variable = nodeHandler.GetVariable(variableName);

            if (variable != null)
            {
                if (variable.variableType == valueType)
                {
                    nodeHandler.SetVariableValue(variableName, value);
                }
                else
                {
                    throw new Exception("You are attempting to set a " + valueType + " value on a " + variable.variableType + " variable. Value will not be set on '" + variableName + "'.");
                }
            }
            else
            {
                throw new Exception("Variable '" + variableName + "' does not exist.");
            }
        }

        /// <summary>
        /// Returns the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="variableType">The variable type.</param>
        /// <returns>The value of the specified variable.</returns>
        /// <exception cref="Exception"></exception>
        private object GetVariableValue(string variableName, Type variableType)
        {
            NodeVariable variable = nodeHandler.GetVariable(variableName);

            if (variable != null)
            {
                if (variable.variableType == variableType)
                {
                    return variable.currentValue;
                }

                throw new Exception("You are requesting a " + variableType + " value from a " + variable.variableType + " variable. Cannot retrieve value from '" + variableName + "'.");
            }
            else
            {
                throw new Exception("Variable '" + variableName + "' does not exist.");
            }
        }

        /// <summary>
        /// Adds the specified Dialogue Listener to the controller's list of listeners.
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
        /// Removes the specified Dialogue Listener from the controller's list of listeners.
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
        /// Removes all Dialogue Listeners from the controller.
        /// </summary>
        public void RemoveDialogueListeners()
        {
            dialogueListeners.Clear();
        }

        /// <summary>
        /// Gets the playback type of this controller.
        /// </summary>
        /// <returns>The playback type.</returns>
        public PlaybackType GetPlaybackType() { return this.playbackType; }

        /// <summary>
        /// Sets the playback type of this controller.
        /// </summary>
        /// <param name="playbackType">The playback type to use.</param>
        public void SetPlaybackType(PlaybackType playbackType) { this.playbackType = playbackType; }

        /// <summary>
        /// Gets or sets the Dialogue Display currently used by this controller.
        /// </summary>
        public AbstractDialogueDisplay DialogueDisplay
        {
            get { return this.dialogueDisplay; }
            set { this.dialogueDisplay = value; }
        }

        /// <summary>
        /// Saves the values of all variables in the Dialogue to either a file, or to the PlayerPrefs.
        /// </summary>
        /// <param name="prefix">The prefix to use when saving. This prefix is appended to the beginning of the dialogue name.</param>
        /// <param name="saveToPlayerPrefs">Whether the variable states should be saved to PlayerPrefs. If set to false, the variable states will be saved to
        /// a JSON file instead.</param>
        public void SaveVariableValues(string prefix = "", bool saveToPlayerPrefs = false)
        {
            nodeHandler.SaveVariableValues(prefix, saveToPlayerPrefs);
        }

        /// <summary>
        /// Saves all currently loaded global variable values to PlayerPrefs or a JSON file.
        /// </summary>
        /// <param name="prefix">The prefix to append to the beginning of the PlayerPrefs entry of JSON file name.</param>
        /// <param name="saveToPlayerPrefs">Whether the values should be saved to PlayerPrefs rather than a JSON file.</param>
        public void SaveGlobalVariableValues(string prefix = "", bool saveToPlayerPrefs = false)
        {
            nodeHandler.SaveGlobalVariableValues(prefix, saveToPlayerPrefs);
        }

        /// <summary>
        /// Loads the states of the Dialogue's variables from a save if available.
        /// </summary>
        /// <param name="prefix">The prefix to use when loading. This prefix is appended to the beginning of the dialogue name.</param>
        /// <param name="loadFromPlayerPrefs">If true, variable states will be loaded from PlayerPrefs rather than a JSON file.</param>
        public void LoadVariableValues(string prefix = "", bool loadFromPlayerPrefs = false)
        {
            nodeHandler.LoadVariableValues(prefix, loadFromPlayerPrefs);
        }

        /// <summary>
        /// Loads global variable values from PlayerPrefs or a JSON file.
        /// </summary>
        /// <param name="prefix">The prefix to append to the beginning of the PlayerPrefs entry or JSON file name.</param>
        /// <param name="loadFromPlayerPrefs">Whether the variable values should be loaded from PlayerPrefs rather than a JSON file.</param>
        public void LoadGlobalVariableValues(string prefix = "", bool loadFromPlayerPrefs = false)
        {
            //Attempt to initialize the global variables from a dialogue registry prior to loading values
            InitializeGlobalVariables(dialogueRegistry);

            //Load the global variable values, if possible. Note: this will fail to load anything if no initialization 
            //occured (for example, if the registry was null and one couldn't be found).
            nodeHandler.LoadGlobalVariableValues(prefix, loadFromPlayerPrefs);
        }

        /// <summary>
        /// Initializes global variable values from the Dialogue Registry provided. If the registry is null, an attempt will be made to load a registry from the
        /// active Dialogue Settings set on the node handler, and then will attempt to locate a registry on a Dialogue Display's Dialogue Settings from the scene's Hierarchy.
        /// </summary>The Dialogue Registry to initialize global variables from.
        /// <param name="registry">The d</param>
        public void InitializeGlobalVariables(DialogueRegistry registry = null)
        {
            if (registry == null)
            {
                registry = dialogueRegistry;
            }

            if (nodeHandler != null)
            {
                nodeHandler.InitializeGlobalVariables(registry);
            }
        }

        /// <summary>
        /// Defines dialogue playback types used by dialogue controllers.
        /// </summary>
        public enum PlaybackType
        {
            AUTOPLAY, WAIT_FOR_COMMAND
        }

        /// <summary>
        /// Sets the playback mode of the controller. If the passed in string is "auto" or "autoplay", the mode will be set to AUTOPLAY; otherwise it will be set to WAIT_FOR_COMMAND mode.
        /// </summary>
        /// <param name="playbackType">A string to the playback type to use.</param>
        public void SetPlaybackType(string playbackType)
        {
            if (playbackType.ToLower().Equals("auto") || playbackType.ToLower().Equals("autoplay"))
            {
                this.playbackType = PlaybackType.AUTOPLAY;
            }
            else
            {
                this.playbackType = PlaybackType.WAIT_FOR_COMMAND;
            }
        }

        /// <summary>
        /// Changes the playback mode of the controller to whichever mode it isn't currently in. If the controller is in WAIT_FOR_COMMAND mode, calling this method will
        /// switch it to AUTOPLAY mode, and vice versa.
        /// </summary>
        public void ChangePlaybackType()
        {
            if (this.playbackType == PlaybackType.WAIT_FOR_COMMAND)
            {
                this.playbackType = PlaybackType.AUTOPLAY;
            }
            else
            {
                this.playbackType = PlaybackType.WAIT_FOR_COMMAND;
            }
        }

        /// <summary>
        /// Gets the Dialogue currently set on the dialogue controller.
        /// </summary>
        public Dialogue CurrentDialogue { get { return this.dialogue; } }
    }
}