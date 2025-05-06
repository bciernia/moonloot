using EasyTalk.Display;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyTalk.Controller
{
    /// <summary>
    /// The DialogueListener class defines various methods which are called as certain events occur during dialogue playback. It can be extended in order to easily create new functionality
    /// which responds to dialogue events.
    /// </summary>
    public class DialogueListener : MonoBehaviour
    {
        /// <summary>
        /// When set to true, debug logging will be shown for each method called on the listener.
        /// </summary>
        [SerializeField]
        public bool debugEnabled = false;

        /// <summary>
        /// An event which is triggered whenever the dialogue continues.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onContinue = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever options are to be displayed to the player.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onDisplayOptions = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the player chooses an option.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onOptionChosen = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a line of dialogue is to be displayed.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onDisplayLine = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever dialogue playback begins.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onDialogueEntered = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever dialogue playback ends.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onDialogueExited = new UnityEvent();

        /// <summary>
        /// An event which is triggered one frame after dialogue playback ends.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onExitCompleted = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a story node is encountered.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onStory = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a dialogue variable value is updated.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onVariableUpdated = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a character name change is detected.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onCharacterChanged = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever dialogue audio starts playing.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onAudioStarted = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever dialogue audio finishes playing.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onAudioCompleted = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a key is to be processed on a dialogue.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onActivateKey = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a dialogue starts waiting for a certain period of time before continuing.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onWait = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the last line of dialogue in a conversation node is reached.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onConversationEnding = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever a the dialogue flows from one node to another.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onNodeChanged = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the dialogue reaches a pause node, pausing and waiting for Continue() to be called.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onPause = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever an asynchronous node must be processed.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onExecuteAsyncNode = new UnityEvent();

        [SerializeField]
        [HideInInspector]
        public UnityEvent onAppendText = new UnityEvent();

        [SerializeField]
        [HideInInspector]
        public UnityEvent onWaitingForNodeCompletion = new UnityEvent();

        [SerializeField]
        [HideInInspector]
        public UnityEvent onNodeEvaluationCompleted = new UnityEvent();
        
        /*public UnityEvent onAIPromptStarted = new UnityEvent();

        public UnityEvent onAIPromptFinished = new UnityEvent();

        public UnityEvent onConversationClear = new UnityEvent();*/

        /// <summary>
        /// Called whenever the dialogue continues on to the next line.
        /// </summary>
        public virtual void OnContinue() 
        {
            if (debugEnabled) { Debug.Log("OnContinue() called."); }
            if(onContinue != null) { onContinue.Invoke(); }
        }

        /// <summary>
        /// Called whenever dialogue options are to be presented.
        /// </summary>
        /// <param name="options">The list of dialogue options to present.</param>
        public virtual void OnDisplayOptions(List<DialogueOption> options) 
        { 
            if (debugEnabled) { Debug.Log("OnDisplayOptions() called with " + options.Count + " options."); }
            if(onDisplayOptions != null) { onDisplayOptions.Invoke(); }
        }

        /// <summary>
        /// Called whenever an option is chosen from the currently presented list of options.
        /// </summary>
        /// <param name="option">The dialogue option which was chosen.</param>
        public virtual void OnOptionChosen(DialogueOption option)
        {
            if (debugEnabled) { Debug.Log("OnOptionChosen() called: " + option.OptionText); }
            if (onOptionChosen != null) { onOptionChosen.Invoke(); }
        }

        /// <summary>
        /// Called when a line of dialogue is to be presented.
        /// </summary>
        /// <param name="conversationLine">The line of dialogue to be displayed.</param>
        public virtual void OnDisplayLine(ConversationLine conversationLine) 
        {
            if (debugEnabled) { Debug.Log("OnDisplayLine() called: " + conversationLine.Text); }
            if (onDisplayLine != null) { onDisplayLine.Invoke(); }
        }

        /// <summary>
        /// Called whenever a dialogue is entered (when playback begins).
        /// </summary>
        /// <param name="entryPointName">The name of the entry point ID where dialogue playback is starting.</param>
        public virtual void OnDialogueEntered(string entryPointName) 
        {
            if (debugEnabled) { Debug.Log("OnDialogueEntered() called: " + entryPointName); }
            if (onDialogueEntered != null) { onDialogueEntered.Invoke(); }
        }

        /// <summary>
        /// Called whenever a dialogue is exited (when playback ends).
        /// </summary>
        /// <param name="exitPointName">The name of the exit point ID where dialogue playback finished.</param>
        public virtual void OnDialogueExited(string exitPointName)
        {
            if (debugEnabled) { Debug.Log("OnDialogueExited() called: " + exitPointName); }
            if (onDialogueExited != null) { onDialogueExited.Invoke(); }
        }

        /// <summary>
        /// Called at least one frame after a dialogue is exited.
        /// </summary>
        public virtual void OnExitCompleted()
        {
            if(debugEnabled) { Debug.Log("OnExitCompleted() called."); }
            if(onExitCompleted != null) { onExitCompleted.Invoke(); }
        }

        /// <summary>
        /// Called whenever a story node is encountered.
        /// </summary>
        /// <param name="storyText">The text of the story node.</param>
        public virtual void OnStory(string storyText) 
        {
            if (debugEnabled) { Debug.Log("OnStoryEntered() called: " + storyText); }
            if (onStory != null) { onStory.Invoke(); }
        }

        /// <summary>
        /// Called whenever a dialogue variable value is updated.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The new value of the variable.</param>
        public virtual void OnVariableUpdated(string variableName, object value) 
        {
            if (debugEnabled) { Debug.Log("OnVariableUpdated() called: " + variableName + " -> " + value); }
            if (onVariableUpdated != null) { onVariableUpdated.Invoke(); }
        }

        /// <summary>
        /// Called whenever a character change is detected.
        /// </summary>
        /// <param name="oldCharacterName">The old character name.</param>
        /// <param name="newCharacterName">The new character name.</param>
        public virtual void OnCharacterChanged(string oldCharacterName, string newCharacterName) 
        {
            if (debugEnabled) { Debug.Log("OnCharacterChanged() called: " + oldCharacterName + " -> " + newCharacterName); }
            if (onCharacterChanged != null) { onCharacterChanged.Invoke(); }
        }

        /// <summary>
        /// Called whenever audio starts playing for a line of dialogue.
        /// </summary>
        /// <param name="line">The line of dialogue which audio is being played for.</param>
        public virtual void OnAudioStarted(ConversationLine line) 
        {
            if (debugEnabled) { Debug.Log("OnAudioStarted() called: " + line.Text); }
            if (onAudioStarted != null) { onAudioStarted.Invoke(); }
        }

        /// <summary>
        /// Called whenever audio stops playing for a line of dialogue.
        /// </summary>
        /// <param name="line">The line of dialogue which audio was being played for.</param>
        /// <param name="forceStopped">Whether the audio was forced to stop (if false, the audio finished playing).</param>
        public virtual void OnAudioCompleted(ConversationLine line, bool forceStopped) 
        {
            if (debugEnabled) { Debug.Log("OnAudioCompleted() called: " + line.Text); }
            if (onAudioCompleted != null) { onAudioCompleted.Invoke(); }
        }

        /// <summary>
        /// Called whenever a key tag is present in a line of dialogue.
        /// </summary>
        /// <param name="key">The value of the key tag.</param>
        public virtual void OnActivateKey(string key) 
        {
            if (debugEnabled) { Debug.Log("OnActivateKey() called: " + key); }
            if (onActivateKey != null) { onActivateKey.Invoke(); }
        }

        /// <summary>
        /// Called whenever the dialogue encounters a wait node.
        /// </summary>
        /// <param name="timeInSeconds">The amount of time which the dialogue will wait before proceeding (in seconds).</param>
        public virtual void Wait(float timeInSeconds) 
        {
            if (debugEnabled) { Debug.Log("Wait() called: " + timeInSeconds); }
            if (onWait != null) { onWait.Invoke(); }
        }

        /// <summary>
        /// Called whenever the last line of dialogue in a conversation node is reached.
        /// </summary>
        /// <param name="line">The last line of dialogue in the current conversation node.</param>
        /// <param name="nextNode">The next node after the current conversation node.</param>
        public virtual void OnConversationEnding(ConversationLine line, Node nextNode) 
        {
            if (debugEnabled) { Debug.Log("OnConversationEnding() called: " + line.Text + " Next node: " + nextNode.NodeType); }
            if (onConversationEnding != null) { onConversationEnding.Invoke(); }
        }

        /// <summary>
        /// Called whenever dialogue playback moves to the next node.
        /// </summary>
        /// <param name="node">The new node..</param>
        public virtual void OnNodeChanged(Node node) 
        {
            if (debugEnabled) { Debug.Log("Node changed: " + node.NodeType); }
            if (onNodeChanged != null) { onNodeChanged.Invoke(); }
        }

        /// <summary>
        /// Called whenever a pause node is reached during dialogue playback.
        /// </summary>
        /// <param name="signal">The signal string of the pause node.</param>
        public virtual void OnPause(string signal)
        {
            if (debugEnabled) { Debug.Log("Pausing with signal: " + signal); }
            if (onPause != null) { onPause.Invoke(); }
        }

        /// <summary>
        /// Called whenever text is to be appended to the current dialogue's conversation text.
        /// </summary>
        /// <param name="text">The text to append.</param>
        public virtual void OnAppendText(string text)
        {
            if (debugEnabled) { Debug.Log("Append text: '" + text + "'"); }
            if (onAppendText != null) { onAppendText.Invoke(); }
        }

        /// <summary>
        /// Called whenever an async node is encountered and needs some external class to handle its execution.
        /// </summary>
        /// <param name="node">The asynchronous node to process.</param>
        public virtual void OnExecuteAsyncNode(AsyncNode node)
        {
            if (debugEnabled) { Debug.Log("Executing Async Node..."); }
            if (onExecuteAsyncNode != null) { onExecuteAsyncNode.Invoke(); }
        }

        /// <summary>
        /// Called just before an asynchronous node is executed to notify listeners that the dialogue is about to enter a waiting state.
        /// </summary>
        /// <param name="asyncNode">The asynchronous node to be executed.</param>
        public virtual void OnWaitingForNodeEvaluation(Node asyncNode)
        {
            if (debugEnabled) { Debug.Log("Waiting for async node completion: '" + asyncNode.NodeType + "'"); }
            if (onWaitingForNodeCompletion != null) { onWaitingForNodeCompletion.Invoke(); }
        }

        /// <summary>
        /// Called whenever an asynchronous node's evaluation/execution has been commpleted.
        /// </summary>
        /// <param name="asyncNode">The asynchronous node which was executed.</param>
        public virtual void OnNodeEvaluationCompleted(Node asyncNode)
        {
            if (debugEnabled) { Debug.Log("Node is finished being evaluated: '" + asyncNode.NodeType + "'"); }
            if (onNodeEvaluationCompleted != null) { onNodeEvaluationCompleted.Invoke(); }
        }

        /*public virtual void OnAIPromptStarted()
        {
            if (debugEnabled) { Debug.Log("Sending AI prompt..."); }
            if (onAIPromptStarted != null) { onAIPromptStarted.Invoke(); }
        }

        public virtual void OnAIPromptFinished()
        {
            if (debugEnabled) { Debug.Log("Finished AI prompt..."); }
            if (onAIPromptFinished != null) { onAIPromptFinished.Invoke(); }
        }

        public virtual void OnConversationClear()
        {
            if (debugEnabled) { Debug.Log("Clearing conversation text..."); }
            if (onConversationClear != null) { onConversationClear.Invoke(); }
        }*/
    }
}
