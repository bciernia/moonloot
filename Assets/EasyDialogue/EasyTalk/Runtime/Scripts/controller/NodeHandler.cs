using EasyTalk.Display;
using EasyTalk.Localization;
using EasyTalk.Nodes;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using EasyTalk.Nodes.Tags;
using EasyTalk.Nodes.Variable;
using EasyTalk.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace EasyTalk.Controller
{
    /// <summary>
    /// This class is used to process a collection of linked nodes, such as a dialogue and handles all of the logic for moving from one node to the next, evaluating 
    /// variable values, and sending messages (via callbacks) about what is happening in a dialogue as it is processed.
    /// </summary>
    public class NodeHandler
    {
        /// <summary>
        /// The dialogue to process.
        /// </summary>
        private Dialogue dialogue;

        /// <summary>
        /// The Dialogue Listener to use for callbacks as events happen during dialogue processing.
        /// </summary>
        private DialogueListener listener;

        /// <summary>
        /// A mapping of node input IDs to the nodes which those inputs belong to.
        /// </summary>
        protected Dictionary<int, Node> inputIdToNodeMap = new Dictionary<int, Node>();

        /// <summary>
        /// A mapping of node output IDs to the nodes which those inputs belong to.
        /// </summary>
        protected Dictionary<int, Node> outputIdToNodeMap = new Dictionary<int, Node>();

        /// <summary>
        /// A mapping of jump keys/IDs to the 'jump in' nodes they belong to.
        /// </summary>
        protected Dictionary<string, Node> jumpMap = new Dictionary<string, Node>();

        /// <summary>
        /// A mapping of variable names to their definitions and values.
        /// </summary>
        protected Dictionary<string, NodeVariable> variables = new Dictionary<string, NodeVariable>();

        /// <summary>
        /// A mapping of global variable names to their definitions and values.
        /// </summary>
        protected static Dictionary<string, NodeVariable> globalVariables = new Dictionary<string, NodeVariable>();

        /// <summary>
        /// A mapping of entry IDs to the 'entry' nodes they belong to.
        /// </summary>
        protected Dictionary<string, Node> entryMap = new Dictionary<string, Node>();

        /// <summary>
        /// The default 'entry' node which is used if an entry ID is not provided, or the 'entry' node with the ID specified can't be found.
        /// </summary>
        protected Node entryNode;

        /// <summary>
        /// The current node being processed/displayed.
        /// </summary>
        protected Node currentNode;

        /// <summary>
        /// The line index of the conversation when a 'conversation' node is being processed.
        /// </summary>
        protected int convoIdx = 0;

        /// <summary>
        /// The GameObject this NodeHandler is being used by.
        /// </summary>
        protected GameObject owner = null;

        /// <summary>
        /// Keeps track of whether the node handler is currently processing and waiting for an asynchronous node to finish.
        /// </summary>
        private bool isProcessingAsyncNode = false;

        /// <summary>
        /// Keeps track of whether the node handler waiting for an output value to be determined for the current node.
        /// </summary>
        private bool isWaitingForNodeValueDetermination = false;

        /// <summary>
        /// A Dictionary which maps node and output IDs to the values output by the respective components.
        /// </summary>
        private Dictionary<int, object> nodeValues = new Dictionary<int, object>();

        /// <summary>
        /// The Dialogue Settings currently in use.
        /// </summary>
        private EasyTalkDialogueSettings dialogueSettings = null;

        /// <summary>
        /// A List of Tasks 
        /// </summary>
        private List<Task> tasks = new List<Task>();

        /// <summary>
        /// Creates a new NodeHandler.
        /// </summary>
        /// <param name="owner">The GameObject which the NodeHandler is associated with.</param>
        public NodeHandler(GameObject owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Called by Dialogue Controllers to run queued up Tasks.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                Task task = tasks[i];
                tasks.RemoveAt(0);
                task.RunSynchronously();
                i--;
            }
        }

        /// <summary>
        /// Adds a task to the current queue.
        /// </summary>
        /// <param name="task">The Task to add to the queue.</param>
        public void AddTask(Task task)
        {
            tasks.Add(task);
        }

        /*public void PromptFinished()
        {
            Task promptFinishedTask = new Task(() =>
            {
                listener.OnAIPromptFinished();
            });

            tasks.Add(promptFinishedTask);
        }*/

        /// <summary>
        /// Creates a Task to call AsyncExecutionCompleted on the main thread.
        /// </summary>
        /// <param name="asyncNode">The AsyncNode which has completed execution.</param>
        public void ExecutionCompleted(AsyncNode asyncNode)
        {
            Task executionCompletedTask = new Task(() =>
            {
                AsyncExecutionCompleted(asyncNode);
            });

            tasks.Add(executionCompletedTask);
        }

        /// <summary>
        /// Queues up a Task to call OnDisplayLine on the node handler's Dialogue Listener. Normally, this will cause a line of dialogue to be displayed.
        /// </summary>
        /// <param name="line">The line of dialogue to queue up.</param>
        public void DisplayLine(ConversationLine line)
        {
            Task displayLineTask = new Task(() =>
            {
                listener.OnDisplayLine(line);
            });

            tasks.Add(displayLineTask);
        }

        public void HandleAppendNode()
        {
            ConversationLine line = GetConversationLine();
            
            if (listener != null)
            {
                listener.OnAppendText(line.Text);
            }

            DisplayLine(line);
        }
        
        /// <summary>
        /// Initializes the NodeHandler by setting the dialogue, clearing various mappings, determining the default 'entry' node, and creating variables.
        /// </summary>
        /// <param name="dialogue">The Dialogue to use.</param>
        public void Initialize(Dialogue dialogue)
        {
            this.dialogue = dialogue;
            inputIdToNodeMap.Clear();
            outputIdToNodeMap.Clear();
            jumpMap.Clear();
            variables.Clear();

            entryNode = FindEntryNode();

            foreach (Node node in dialogue.Nodes)
            {
                //Add each node to a map for each of its inputs so that when jumping from an output to a node, it can quickly be found via an input id.
                foreach (NodeConnection conn in node.Inputs)
                {
                    inputIdToNodeMap.Add(conn.ID, node);
                }

                //Add each node to a map for each of its outputs so that when jumping from an input to a node, it can quickly be found via an output id.
                foreach (NodeConnection conn in node.Outputs)
                {
                    outputIdToNodeMap.Add(conn.ID, node);
                }

                if (node.NodeType == NodeType.JUMPIN)
                {
                    //Add jump nodes to jump map so whenever a jumpout is moved to, the jumped to node can quickly be found.
                    jumpMap.Add(((JumpInNode)node).Key, node);
                }
                else if (node.NodeType == NodeType.BOOL_VARIABLE)
                {
                    CreateBoolVariable(node);
                }
                else if (node.NodeType == NodeType.INT_VARIABLE)
                {
                    CreateIntVariable(node);
                }
                else if (node.NodeType == NodeType.FLOAT_VARIABLE)
                {
                    CreateFloatVariable(node);
                }
                else if (node.NodeType == NodeType.STRING_VARIABLE)
                {
                    CreateStringVariable(node);
                }
                else if (node.NodeType == NodeType.ENTRY)
                {
                    EntryNode entryNode = node as EntryNode;
                    if (entryNode.EntryPointName != null && entryNode.EntryPointName.Length > 0)
                    {
                        entryMap.TryAdd(entryNode.EntryPointName, entryNode);
                    }
                }
            }
        }

        /// <summary>
        /// Forces an exit to be triggered immediately on the Dialogue.
        /// </summary>
        public void ForceExit()
        {
            listener.OnDialogueExited(null);
            currentNode = null;
        }

        /// <summary>
        /// Jumps to the specified node after a certain amount of time.
        /// </summary>
        /// <param name="delay">The delay, in seconds.</param>
        /// <param name="node">The node to jump to.</param>
        /// <returns></returns>
        public IEnumerator JumpToNodeAfterDelay(float delay, Node node)
        {
            yield return new WaitForSeconds(delay);

            currentNode = node;
            ProcessNode(currentNode);
        }

        /// <summary>
        /// Chooses the option, and thus continues down the corresponding path, of the option specified.
        /// </summary>
        /// <param name="optionIdx">The option index to choose. This should be the index of the option as it was originally in the 'option' node, which may differ from
        /// the index of the option as it occurs in the List of options displayed to the player. It is recommended to use DialogueOption.OptionIndex for reliability.</param>
        public void ChooseOption(int optionIdx)
        {
            if (!(currentNode is OptionNode)) { return; }

            NodeConnection outputConnection = currentNode.Outputs[optionIdx];

            if (outputConnection.AttachedIDs.Count > 0)
            {
                int attachedId = outputConnection.AttachedIDs[0];
                currentNode = inputIdToNodeMap[attachedId];
                ProcessNode(currentNode);
            }
            else
            {
                listener.OnDialogueExited(null);
            }
        }

        /// <summary>
        /// Processes the node specified according to its type.
        /// </summary>
        /// <param name="node">The node to process.</param>
        public void ProcessNode(Node node)
        {
            if (node == null)
            {
                listener.OnDialogueExited(null);
                return;
            }

            listener.OnNodeChanged(node);

            switch (node.NodeType)
            {
                case NodeType.CONVO: HandleConversation(); break;
                case NodeType.OPTION: HandleOptionNode(); break;
                case NodeType.JUMPOUT: HandleJump(); break;
                case NodeType.EXIT: HandleExit(); break;
                case NodeType.STORY: HandleStoryNode(); break;
                case NodeType.RANDOM: ProcessCurrentNode(); break;
                case NodeType.SEQUENCE: ProcessCurrentNode(); break;
                case NodeType.GET_VARIABLE_VALUE: ProcessCurrentNode(); break;
                case NodeType.SET_VARIABLE_VALUE: ProcessCurrentNode(); break;
                case NodeType.BUILD_STRING: ProcessCurrentNode(); break;
                case NodeType.MATH: ProcessCurrentNode(); break;
                case NodeType.TRIGGER: ProcessCurrentNode(); break;
                case NodeType.BOOL_LOGIC: ProcessCurrentNode(); break;
                case NodeType.NUMBER_COMPARE: ProcessCurrentNode(); break;
                case NodeType.STRING_COMPARE: ProcessCurrentNode(); break;
                case NodeType.PATH_SELECT: HandlePathSelectorNode(); break;
                case NodeType.VALUE_SELECT: ProcessCurrentNode(); break;
                case NodeType.WAIT: HandleWaitNode(); break;
                case NodeType.PAUSE: HandlePauseNode(); break;
                case NodeType.CONDITIONAL_VALUE: ProcessCurrentNode(); break;
                case NodeType.PLAYER_INPUT: ProcessCurrentNode(); break;
                case NodeType.AI_INIT: ProcessCurrentNode(); break;
                case NodeType.AI_ADD_MESSAGE: ProcessCurrentNode(); break;
                case NodeType.AI_PROMPT: ProcessCurrentNode(); break;
                case NodeType.AI_CLEAR: ProcessCurrentNode(); break;
                case NodeType.AI_READ: ProcessCurrentNode(); break;
                case NodeType.SHOW: ProcessCurrentNode(); break;
                case NodeType.HIDE: ProcessCurrentNode(); break;
                case NodeType.GOTO: HandleGotoNode(); break;
                case NodeType.APPEND: HandleAppendNode(); break;
            }
        }

        /// <summary>
        /// Loads the Dialogue asset specified in the GOTO node and enters the dialogue.
        /// </summary>
        private void HandleGotoNode()
        {
            GotoNode node = currentNode as GotoNode;
            this.Initialize(node.Dialogue);
            this.EnterDialogue(node.EntryID);
        }

        /// <summary>
        /// Called whenever asynchronous execution of an AsyncNode has been completed.
        /// </summary>
        /// <param name="asyncNode">The AsyncNode which execution completed on.</param>
        private void AsyncExecutionCompleted(AsyncNode asyncNode)
        {
            isProcessingAsyncNode = false;
            listener.OnNodeEvaluationCompleted(asyncNode as Node);

            if (asyncNode.AsyncCompletionMode == AsyncCompletionMode.REPROCESS_CURRENT)
            {
                ProcessNode(currentNode);
                asyncNode.Reset();
            }
            else if(asyncNode.AsyncCompletionMode == AsyncCompletionMode.PROCEED_TO_NEXT)
            {
                asyncNode.Reset();
                currentNode = GetNextNode();
                ProcessNode(currentNode);
            }
            else if(asyncNode.AsyncCompletionMode == AsyncCompletionMode.PROPAGATE_ONLY)
            {
                Propagate(nodeValues, asyncNode as Node);
            }
        }

        /// <summary>
        /// Waits for the configured duration of the wait node currently being processed before continuing.
        /// </summary>
        public void HandleWaitNode()
        {
            float timeout = ((WaitNode)currentNode).GetWaitTime();
            listener.Wait(timeout);
        }

        /// <summary>
        /// Triggers the OnPause() callback on the Dialogue Listener and waits for a continue.
        /// </summary>
        public void HandlePauseNode()
        {
            PauseNode pauseNode = currentNode as PauseNode;
            listener.OnPause(pauseNode.Signal);
        }

        /// <summary>
        /// Triggers the OnStory() callback on the Dialogue Listener and waits for a continue.
        /// </summary>
        public void HandleStoryNode()
        {
            StoryNode storyNode = currentNode as StoryNode;
            listener.OnStory(storyNode.Summary);
        }        

        /// <summary>
        /// Processes the current node, determining any dependent values used by the node and setting values as necessary on outputs. Once the node
        /// is processed, the next node will be determined and processed.
        /// </summary>
        public void ProcessCurrentNode()
        {
            //Check value output for connection. If connected to something, propogate forward.
            if(!isWaitingForNodeValueDetermination)
            {
                nodeValues.Clear();
            }

            //Attempt tp propagate the values of the currently evaluated node to any downstream connected nodes.
            Node undeterminedNode = Propagate(nodeValues, currentNode);

            if (undeterminedNode == null) //Move to the next node since the values of all nodes used by the current node could be determined.
            {
                if (currentNode is ConditionalNode)
                {
                    ConditionalNode condNode = currentNode as ConditionalNode;
                    bool conditionState = (bool)nodeValues[currentNode.ID];
                    NodeConnection outputConnection = conditionState ? condNode.GetTrueOutput() : condNode.GetFalseOutput();

                    if (outputConnection.AttachedIDs.Count > 0)
                    {
                        int attachedId = outputConnection.AttachedIDs[0];
                        currentNode = inputIdToNodeMap[attachedId];
                        ProcessNode(currentNode);
                    }
                    else
                    {
                        listener.OnDialogueExited(null);
                    }
                }
                else if(currentNode is AsyncNode && !((AsyncNode)currentNode).IsExecutionComplete())
                {
                    listener.OnWaitingForNodeEvaluation(currentNode);
                    ((AsyncNode)currentNode).IsExecutingFromDialogueFlow = true;
                    ((AsyncNode)currentNode).Reset();
                    ((AsyncNode)currentNode).Execute(this, nodeValues, owner);
                    isProcessingAsyncNode = true;
                }
                else
                {
                    currentNode = GetNextNode();
                    ProcessNode(currentNode);
                }

                isWaitingForNodeValueDetermination = false;
            }
            else //Since we couldn't fully process the node, do whatever needs to be done to process it.
            {
                isWaitingForNodeValueDetermination = true;

                //Process the node if it is an asyncronous node.
                if (undeterminedNode is AsyncNode)
                {
                    if (!((AsyncNode)undeterminedNode).IsExecutionComplete())
                    {
                        //Wait for async node.
                        listener.OnWaitingForNodeEvaluation(undeterminedNode);
                        ((AsyncNode)undeterminedNode).IsExecutingFromDialogueFlow = (currentNode.ID == undeterminedNode.ID);
                        ((AsyncNode)undeterminedNode).Reset();
                        ((AsyncNode)undeterminedNode).Execute(this, nodeValues, owner);
                        isProcessingAsyncNode = true;
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the specified node to determine its output value based on its configuration and any incoming values that it depends upon, then pushes the
        /// evaluated final value to any nodes connected along the output value path.
        /// </summary>
        /// <param name="nodeOutputValues">A map of node IDs and connection IDs to the output values that those IDs correspond to.</param>
        /// <param name="node">The node to evaluate.</param>
        /// <returns></returns>
        private Node Propagate(Dictionary<int, object> nodeOutputValues, Node node)
        {
            //Determine the value for the current node and store it.
            Node undeterminedNode = DetermineNodeValue(nodeOutputValues, node);
            if (undeterminedNode != null)
            {
                isWaitingForNodeValueDetermination = true;
                return undeterminedNode;
            }

            //If the node has connected outputs, check any value outputs and propagate forward to those
            if (node.HasConnectedOutputs())
            {
                foreach (NodeConnection connection in node.Outputs)
                {
                    foreach (int id in connection.AttachedIDs)
                    {
                        Node nextNode = inputIdToNodeMap[id];
                        if (!connection.IsDialogueFlowConnection())
                        {
                            undeterminedNode = Propagate(nodeOutputValues, nextNode);
                            if (undeterminedNode != null)
                            {
                                isWaitingForNodeValueDetermination = true;
                                return undeterminedNode;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively determines the output value of the specified node.
        /// </summary>
        /// <param name="nodeOutputValues">A map of node IDs and connection IDs to the output values that those IDs correspond to.</param>
        /// <param name="node">The node to evaluate.</param>
        /// <returns>If the output values of the node could be determined successfully, this method returns null; otherwise this method returns the node whose value
        /// could not be determined (this may differ from the node originally passed into this method, since it could be a dependency node).</returns>
        private Node DetermineNodeValue(Dictionary<int, object> nodeOutputValues, Node node)
        {
            if (node is FunctionalNode)
            {
                FunctionalNode vrNode = (FunctionalNode)node;

                //Evaluate all of the node dependencies coming in through inputs.
                if (vrNode.HasDependencies())
                {
                    List<int> outputIds = vrNode.GetDependencyOutputIDs();
                    foreach (int id in outputIds)
                    {
                        if (!nodeOutputValues.ContainsKey(id))
                        {
                            Node dependencyNode = outputIdToNodeMap[id];

                            Node undeterminedNode = DetermineNodeValue(nodeOutputValues, dependencyNode);
                            if (undeterminedNode != null)
                            {
                                isWaitingForNodeValueDetermination = true;
                                return undeterminedNode;
                            }
                        }
                    }
                }

                //After all dependencies have been evaluated, determine the final value of the specified node.
                if (!vrNode.DetermineAndStoreValue(this, nodeOutputValues, owner))
                {
                    isWaitingForNodeValueDetermination = true;
                    return vrNode as Node;
                }
            }

            isWaitingForNodeValueDetermination = false;
            return null;
        }

        /// <summary>
        /// Handles the 'jump out' node by finding the associated 'jump in' node and moving to the next node after the 'jump in'.
        /// </summary>
        public void HandleJump()
        {
            currentNode = jumpMap[((JumpOutNode)currentNode).Key];
            currentNode = GetNextNode();
            ProcessNode(currentNode);
        }

        /// <summary>
        /// Handles the current 'conversation' node, sending a signal via the onDisplayConversationLine callback to display the first line in the 'conversation' node. If
        /// the 'conversation' node only has 1 line, the atConversationNodeEnding callback will also be triggered.
        /// </summary>
        public void HandleConversation()
        {
            if (currentNode.NodeType == NodeType.CONVO)
            {
                convoIdx = 0;
                ConversationNode convoNode = currentNode as ConversationNode;
                ConversationLine convoLine = GetConversationLine();

                listener.OnDisplayLine(convoLine);

                //Display options for the conversation if there is only one line in the convo.
                if (convoIdx >= ((ConversationNode)currentNode).Items.Count - 1)
                {
                    listener.OnConversationEnding(convoLine, GetNextNode());
                }
            }
        }

        /// <summary>
        /// Builds a ConversationLine object containing information about the current line of dialogue to be displayed. This method handles variable injection, tag extraction,
        /// and translation on the text in the 'conversation' node's current line of dialogue.
        /// </summary>
        /// <returns>A ConversationLine containing information about the current line of dialogue to be displayed.</returns>
        private ConversationLine GetConversationLine()
        {
            ConversationLine line = null;
            string text = "";

            if (currentNode is ConversationNode)
            {
                ConversationNode convoNode = ((ConversationNode)currentNode);

                if (convoIdx < convoNode.Items.Count)
                {
                    ConversationItem convoItem = convoNode.Items[convoIdx] as ConversationItem;
                    line = new ConversationLine();
                    text = convoItem.Text;

                    line.AudioClip = convoItem.AudioClip;
                    line.OriginalCharacterName = convoNode.CharacterName;

                    Node nextNode = GetNextNode();
                    if (nextNode != null && (nextNode is OptionNode) && convoIdx == convoNode.Items.Count - 1)
                    {
                        line.PrecedesOption = true;
                    }
                }
            }
            else if(currentNode is AppendNode)
            {
                AppendNode appendNode = currentNode as AppendNode;
                line = new ConversationLine();
                text = appendNode.Text;

                Node nextNode = GetNextNode();
                if (nextNode != null && (nextNode is OptionNode))
                {
                    line.PrecedesOption = true;
                }

                line.AudioClip = appendNode.AudioClip;
                line.TextDisplayMode = TextDisplayMode.APPEND;
            }

            if (line != null)
            {
                Dictionary<string, NodeTag> tags = new Dictionary<string, NodeTag>();
                text = NodeTag.ExtractTags(text, tags);

                string untranslatedText;
                text = Translate(text, out untranslatedText);

                line.Text = text;
                line.PreTranslationText = untranslatedText;

                if (tags.ContainsKey("append")) { line.TextDisplayMode = TextDisplayMode.APPEND; }
                if (tags.ContainsKey("key")) { line.Key = (tags["key"] as KeyTag).keyValue; }
                if (tags.ContainsKey("target")) { line.Target = (tags["target"] as TargetTag).target; }
                if (tags.ContainsKey("id")) { line.ID = (tags["id"] as IDTag).id; }
                if (tags.ContainsKey("name"))
                {
                    NameTag nameTag = (tags["name"] as NameTag);
                    line.OriginalCharacterName = nameTag.name;

                    //If a particular icon was specified, set the icon for the line.
                    line.IconID = nameTag.iconId;
                }

                if (tags.ContainsKey("autoplay")) 
                { 
                    line.AutoPlay = true;
                    AutoplayTag autoplayTag = (tags["autoplay"] as AutoplayTag);

                    if (autoplayTag.overrideDelay) 
                    {
                        line.OverrideAutoplayDelay = true;
                        line.AutoPlayDelay = autoplayTag.delay;
                    }
                }

                //Remove TextMeshPro tags from the character name before performing a translation, since the source/original name in the translation library is TMP tag free.
                string tagFreeCharacterName = TMPTag.RemoveTags(line.OriginalCharacterName);

                //Translate the character name. If the returned result is the same as the original, then we can just use the original character name (tags included).
                string translatedCharacterName = Translate(tagFreeCharacterName);

                if(translatedCharacterName.Equals(tagFreeCharacterName))
                {
                    line.TranslatedCharacterName = line.OriginalCharacterName;
                }
                else
                {
                    line.TranslatedCharacterName = translatedCharacterName;
                }
            }

            return line;
        }

        /// <summary>
        /// Returns the next node along the Dialogue flow path.
        /// </summary>
        /// <returns>The next node to be moved to after the current node.</returns>
        public Node GetNextNode()
        {
            NodeConnection outputConnection = (currentNode as DialogueFlowNode).GetFlowOutput();
            if (outputConnection.AttachedIDs.Count > 0)
            {
                int attachedId = outputConnection.AttachedIDs[0];
                return inputIdToNodeMap[attachedId];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Evaluates the current 'path select' node to determine which path to continue down and the moves along that path.
        /// </summary>
        public void HandlePathSelectorNode()
        {
            Dictionary<int, object> nodeValues = new Dictionary<int, object>();
            PathSelectorNode selectorNode = currentNode as PathSelectorNode;
            
            //Find the value of the input index node if there is one.
            List<int> inputNodeOutputIds = currentNode.FindDependencyOutputIDs();
            if(inputNodeOutputIds.Count > 0)
            {
                Node inputNode = outputIdToNodeMap[inputNodeOutputIds[0]];
                if(inputNode is FunctionalNode)
                {
                    DetermineNodeValue(nodeValues, inputNode);
                }
            }

            //Store the index value in the selector node.
            selectorNode.FindIndex(nodeValues);

            //Get the dialogue flow output of the node (which should be based on the index value).
            NodeConnection selectedOutputConnection = selectorNode.GetFlowOutput();
            int attachedId = selectedOutputConnection.AttachedIDs[0];

            //Handle the new node.
            currentNode = inputIdToNodeMap[attachedId];
            ProcessNode(currentNode);
        }

        /// <summary>
        /// Builds a List of DialogueOptions to present to the player based on the current 'option' node. This method handles 'option modifier' nodes and also
        /// deals with variable injection, translation, and tag extraction. Once the List of DialogueOptions is created, this method calls the onDisplayOptions callback
        /// so that the options can be used by anything registered with the delegate.
        /// </summary>
        public void HandleOptionNode()
        {
            if(!isWaitingForNodeValueDetermination)
            {
                nodeValues.Clear();
            }

            OptionNode optionNode = currentNode as OptionNode;
            List<int> optionModifierOutputIds = currentNode.FindDependencyOutputIDs();

            foreach (int outputId in optionModifierOutputIds)
            {
                Node inputNode = outputIdToNodeMap[outputId];
                if (inputNode is OptionModifierNode)
                {
                    Node undeterminedNode = DetermineNodeValue(nodeValues, inputNode);
                    if(undeterminedNode != null)
                    {
                        isWaitingForNodeValueDetermination = true;

                        //Process the node if it is an asyncronous node.
                        if (undeterminedNode is AsyncNode)
                        {
                            if (!((AsyncNode)undeterminedNode).IsExecutionComplete())
                            {
                                //Wait for async node.
                                listener.OnWaitingForNodeEvaluation(undeterminedNode);
                                ((AsyncNode)undeterminedNode).IsExecutingFromDialogueFlow = false;
                                ((AsyncNode)undeterminedNode).Reset();
                                ((AsyncNode)undeterminedNode).Execute(this, nodeValues, owner);
                                isProcessingAsyncNode = true;
                            }
                        }

                        return;
                    }
                }
            }

            List<DialogueOption> options = new List<DialogueOption>();

            for (int i = 0; i < optionNode.Items.Count; i++)
            {
                DialogueOption option = new DialogueOption();
                option.OptionIndex = i;

                NodeConnection modifierConnection = optionNode.Inputs[i + 1];

                if (modifierConnection.AttachedIDs.Count > 0)
                {
                    OptionModifier modifier = nodeValues[modifierConnection.AttachedIDs[0]] as OptionModifier;

                    if (modifier.text != null)
                    {
                        option.OptionText = modifier.text;
                    }
                    else if (optionNode.Items[i] != null && (optionNode.Items[i] as OptionItem).text != null)
                    {
                        option.OptionText = (optionNode.Items[i] as OptionItem).text;
                    }

                    option.IsDisplayed = modifier.displayed;
                    option.IsSelectable = modifier.selectable;
                }
                else
                {
                    if (optionNode.Items[i] != null && (optionNode.Items[i] as OptionItem).text != null)
                    {
                        option.OptionText = (optionNode.Items[i] as OptionItem).text;
                    }
                    else
                    {
                        option.OptionText = "...";
                    }

                    option.IsDisplayed = true;
                    option.IsSelectable = true;
                }

                string optionText = option.OptionText;

                Dictionary<string, NodeTag> tags = new Dictionary<string, NodeTag>();
                optionText = NodeTag.ExtractTags(optionText, tags);

                if (tags.ContainsKey("display")) { option.IsDisplayed = (tags["display"] as DisplayTag).display; }
                if (tags.ContainsKey("selectable")) { option.IsSelectable = (tags["selectable"] as SelectableTag).selectable; }
                if (tags.ContainsKey("id")) { option.ID = (tags["id"] as IDTag).id; }

                string untranslatedText;
                optionText = Translate(optionText, out untranslatedText);

                option.OptionText = optionText;
                option.PreTranslationText = untranslatedText;
                options.Add(option);
            }

            listener.OnDisplayOptions(options);
        }

        /// <summary>
        /// Performs a lookup in the active Translation Library for the text provided and returns a translation, if there is one. The text is attempted to be translated into
        /// whatever langauge is set on EasyTalkGameState.Instance.Language.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <returns>A translation of the provided text, if there is a match in the current Translation Library.</returns>
        public string Translate(string text)
        {
            string preTranslationText;
            return Translate(text, out preTranslationText);
        }

        /// <summary>
        /// Performs a lookup in the active Translation Library for the text provided and returns a translation, if there is one. The text is attempted to be translated into
        /// whatever langauge is set on EasyTalkGameState.Instance.Language. This method also sets the provided preTranslationText string to the string being translated, immediately prior to translation. The 
        /// preTranslationText may differ from the originally provided text if the EasyTalkDialogueSettings.Instance.TranslationEvaluationMode is set to TRANSLATE_AFTER_VARIABLE_EVALUATION, since in that mode, all
        /// variable tags are replaced with their values prior to translation.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <param name="preTranslationText">A string to store the text value which was translated in. This value is the text which is ultimately translated and may differ from the provided text string.</param>
        /// <returns>A translation of the provided text, if there is a match in the current Translation Library.</returns>
        public string Translate(string text, out string preTranslationText)
        {
            string finalText = text;
            preTranslationText = finalText;

            if (dialogueSettings != null)
            {
                if (dialogueSettings.TranslationEvaluationMode == TranslationEvaluationMode.TRANSLATE_BEFORE_VARIABLE_EVALUATION) { finalText = TranslateText(finalText); }

                finalText = ReplaceVariablesInString(finalText);

                if (dialogueSettings.TranslationEvaluationMode == TranslationEvaluationMode.TRANSLATE_AFTER_VARIABLE_EVALUATION)
                {
                    preTranslationText = finalText;
                    finalText = TranslateText(finalText);
                }
            }

            return finalText;
        }

        /// <summary>
        /// Handles the current 'exit' node by exiting the dialogue and triggering the onDialogueExited callback.
        /// </summary>
        public void HandleExit()
        {
            if (currentNode != null)
            {
                ExitNode exitNode = currentNode as ExitNode;
                listener.OnDialogueExited(exitNode.ExitPointName);
            }
            else
            {
                listener.OnDialogueExited(null);
            }
        }

        /// <summary>
        /// This method does variable value injection, replacing all variable references in the specified string with their associated values.
        /// </summary>
        /// <param name="text">The string to inject variable values into.</param>
        /// <returns>The modified string with variable references replaced by their respective values.</returns>
        public string ReplaceVariablesInString(string text)
        {
            try
            {
                string newText = text;
                int variableStartIdx = -1;
                int variableEndIdx = -1;
                while ((variableStartIdx = newText.IndexOf("(@")) > -1 && (variableEndIdx = newText.IndexOf(")", variableStartIdx + 2)) > -1)
                {
                    string variableName = newText.Substring(variableStartIdx + 2, variableEndIdx - variableStartIdx - 2);
                    NodeVariable variable = GetVariable(variableName);

                    if (variable != null)
                    {
                        newText = newText.Substring(0, variableStartIdx) + variable.currentValue + newText.Substring(variableEndIdx + 1);
                    }
                    else { break; }
                }

                return newText;
            }
            catch
            {
                return text;
            }
        }

        /// <summary>
        /// Returns the value of the variable specified, if available; otherwise returns null.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve the value of.</param>
        /// <returns>The value of the specified variable.</returns>
        public object GetVariableValue(string variableName)
        {
            if(globalVariables.ContainsKey(variableName))
            {
                return globalVariables[variableName];
            }
            else if (variables.ContainsKey(variableName))
            {
                return variables[variableName];
            }

            return null;
        }

        /// <summary>
        /// Sets the value of the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="variableValue">The value to set on the variable.</param>
        public void SetVariableValue(string variableName, object variableValue)
        {
            if(globalVariables.ContainsKey(variableName))
            {
                globalVariables[variableName].currentValue = variableValue;
                listener.OnVariableUpdated(variableName, variableValue);
            }
            else if (variables.ContainsKey(variableName))
            {
                variables[variableName].currentValue = variableValue;
                listener.OnVariableUpdated(variableName, variableValue);
            }
        }

        /// <summary>
        /// Returns the NodeVariable associated with the specified variable name.
        /// </summary>
        /// <param name="variableName">The name of the variable to retrieve.</param>
        /// <returns>The NodeVariable for the specified variable name.</returns>
        public NodeVariable GetVariable(string variableName)
        {
            if(globalVariables.ContainsKey(variableName))
            {
                return globalVariables[variableName];
            }
            else if (variables.ContainsKey(variableName))
            {
                return variables[variableName];
            }

            return null;
        }

        /// <summary>
        /// Finds and returns the first 'entry' node in the Dialogue.
        /// </summary>
        /// <returns>The first 'entry' node found int the Dialogue.</returns>
        public EntryNode FindEntryNode()
        {
            foreach (Node node in dialogue.Nodes)
            {
                if (node.NodeType == NodeType.ENTRY)
                {
                    return (EntryNode)node;
                }
            }

            return null;
        }

        /// <summary>
        /// Enters and begins processing the Dialogue. If no entry point is specified, the Dialogue will enter at the first 'entry' node found in the Dialogue.
        /// </summary>
        /// <param name="entryPointName">The optional name of the entry point where the Dialogue should start being processed.</param>
        public void EnterDialogue(string entryPointName = null)
        {
            if (entryPointName == null)
            {
                currentNode = entryNode;
            }
            else
            {
                if (entryMap.ContainsKey(entryPointName))
                {
                    currentNode = entryMap[entryPointName];
                }
                else { currentNode = entryNode; }
            }

            if (currentNode == null) { return; }

            listener.OnDialogueEntered(entryPointName);

            currentNode = GetNextNode();

            //Reset flags
            ResetVariablesOnEntry();

            ProcessNode(currentNode);
        }

        /// <summary>
        /// Continues along to the next line of dialogue, or the next node if the last line is currently being displayed.
        /// </summary>
        public void Continue()
        {
            if(currentNode == null)
            {
                listener.OnDialogueExited(null);
                return;
            }

            if (currentNode.NodeType == NodeType.CONVO)
            {
                ConversationNode convoNode = ((ConversationNode)currentNode);
                convoIdx++;

                ConversationLine currentLine = GetConversationLine();

                if (currentLine != null && convoIdx < convoNode.Items.Count)
                {
                    listener.OnDisplayLine(currentLine);
                }
                
                if(currentLine != null && convoIdx == convoNode.Items.Count - 1)
                {
                    Node nextNode = GetNextNode();
                    listener.OnConversationEnding(currentLine, nextNode);
                }
                else if (convoIdx > convoNode.Items.Count - 1)
                {
                    Node nextNode = GetNextNode();

                    //If we are past the last conversation item, go to the next node.
                    convoIdx = 0;
                    currentNode = nextNode;
                    ProcessNode(currentNode);
                }
            }else if(isProcessingAsyncNode)
            {
                //Interrupt the async operation, if permitted, and move on to the next node.
                if (currentNode is AsyncNode)
                {
                    AsyncNode asyncNode = (AsyncNode)currentNode;
                    if(asyncNode.IsSkippable())
                    {
                        asyncNode.Interrupt();
                        isProcessingAsyncNode = false;

                        currentNode = GetNextNode();
                        ProcessNode(currentNode);
                    }
                }
            }
            else if (currentNode.NodeType == NodeType.STORY || 
                currentNode.NodeType == NodeType.PAUSE || 
                currentNode.NodeType == NodeType.WAIT ||
                currentNode.NodeType == NodeType.AI_PROMPT ||
                currentNode.NodeType == NodeType.APPEND)
            {
                currentNode = GetNextNode();
                ProcessNode(currentNode);
            }
        }

        /// <summary>
        /// Resets the values of each variable which is set to "reset on entry".
        /// </summary>
        public void ResetVariablesOnEntry()
        {
            foreach (NodeVariable variable in variables.Values)
            {
                if (variable.resetOnEntry)
                {
                    variable.currentValue = variable.initialValue;
                }
            }
        }

        /// <summary>
        /// Creates a new bool type NodeVariable from the provided bool variable node.
        /// </summary>
        /// <param name="node">The node to use.</param>
        private void CreateBoolVariable(Node node)
        {
            VariableNode varNode = (VariableNode)node;

            bool boolValue = true;
            if (varNode.VariableValue != null)
            {
                bool.TryParse(varNode.VariableValue, out boolValue);
            }

            CreateVariable(typeof(bool), varNode.VariableName, boolValue, boolValue, varNode.ResetOnEntry, varNode.IsGlobal);
        }

        /// <summary>
        /// Creates a new int type NodeVariable from the provided int variable node.
        /// </summary>
        /// <param name="node">The node to use.</param>
        private void CreateIntVariable(Node node)
        {
            VariableNode varNode = (VariableNode)node;

            int intValue = 0;
            if (varNode.VariableValue != null)
            {
                int.TryParse(varNode.VariableValue, out intValue);
            }

            CreateVariable(typeof(int), varNode.VariableName, intValue, intValue, varNode.ResetOnEntry, varNode.IsGlobal);
        }

        /// <summary>
        /// Creates a new float type NodeVariable from the provided float variable node.
        /// </summary>
        /// <param name="node">The node to use.</param>
        private void CreateFloatVariable(Node node)
        {
            VariableNode varNode = (VariableNode)node;

            float floatValue = 0;
            if (varNode.VariableValue != null)
            {
                float.TryParse(varNode.VariableValue, out floatValue);
            }

            CreateVariable(typeof(float), varNode.VariableName, floatValue, floatValue, varNode.ResetOnEntry, varNode.IsGlobal);
        }

        /// <summary>
        /// Creates a new string type NodeVariable from the provided string variable node.
        /// </summary>
        /// <param name="node">The node to use.</param>
        private void CreateStringVariable(Node node)
        {
            VariableNode varNode = (VariableNode)node;

            string stringValue = "";
            if (varNode.VariableValue != null)
            {
                stringValue = varNode.VariableValue;
            }

            CreateVariable(typeof(string), varNode.VariableName, stringValue, stringValue, varNode.ResetOnEntry, varNode.IsGlobal);
        }

        /// <summary>
        /// Creates a variable for use by the node handler. Global variables persist for use by all node handlers.
        /// </summary>
        /// <param name="type">The type of the variable (int, float, string, or bool).</param>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="initialValue">The initial/default value of the variable.</param>
        /// <param name="currentValue">The current value of the variable.</param>
        /// <param name="resetOnEntry">Whether the variable should be reset whenever the dialogue is entered (only applicable for local variables, not global).</param>
        /// <param name="isGlobal">Whether the variable is global (persists to all dialogues).</param>
        private void CreateVariable(Type type, string variableName, object initialValue, object currentValue, bool resetOnEntry, bool isGlobal)
        {
            NodeVariable variable = new NodeVariable();
            variable.variableType = type;
            variable.variableName = variableName;
            variable.initialValue = initialValue;
            variable.currentValue = currentValue;
            variable.resetOnEntry = resetOnEntry;
            variable.isGlobal = isGlobal;

            if (!isGlobal)
            {
                variables.Add(variableName, variable);
            }
            else
            {
                globalVariables.Add(variableName, variable);
            }
        }

        /// <summary>
        /// Saves the values of all local variables in the Dialogue to either a file, or to the PlayerPrefs.
        /// </summary>
        /// <param name="prefix">The prefix to use when saving. This prefix is appended to the beginning of the dialogue name.</param>
        /// <param name="saveToPlayerPrefs">Whether the variable states should be saved to PlayerPrefs. If set to false, the variable states will be saved to
        /// a JSON file instead.</param>
        public void SaveVariableValues(string prefix = "", bool saveToPlayerPrefs = false)
        {
            NodeVariableValueCollection valueCollection = GetVariableValues();
            SaveVariableValues(valueCollection, dialogue.name, prefix, saveToPlayerPrefs);
        }

        /// <summary>
        /// Saves global variable values to PlayerPrefs or a JSON file.
        /// </summary>
        /// <param name="prefix">The prefix to use when saving. The prefix is appended before '_global' or '_global.json'.</param>
        /// <param name="saveToPlayerPrefs">Whether variable values should be saved to PlayerPrefs rather than a JSON file.</param>
        public void SaveGlobalVariableValues(string prefix = "", bool saveToPlayerPrefs = false)
        {
            NodeVariableValueCollection valueCollection = GetGlobalVariableValues();
            SaveVariableValues(valueCollection, "global", prefix, saveToPlayerPrefs);
        }

        /// <summary>
        /// Saves variable values from the provided NodeVariableValueCollection to PlayerPrefs or a JSON file.
        /// </summary>
        /// <param name="variableValues">The collection of variable values to save.</param>
        /// <param name="suffix">The suffix to use when saving the values.</param>
        /// <param name="prefix">The prefix to use when saving the values.</param>
        /// <param name="saveToPlayerPrefs">Whether the values should be saved to PlayerPrefs rather than a JSON file.</param>
        private void SaveVariableValues(NodeVariableValueCollection variableValues, string suffix, string prefix = "", bool saveToPlayerPrefs = false)
        {
            string json = JsonUtility.ToJson(variableValues);

            if (saveToPlayerPrefs)
            {
                PlayerPrefs.SetString(prefix + "_" + suffix, json);
            }
            else
            {
                string savePath = Application.persistentDataPath + "/" + prefix + "_" + suffix + ".json";

                try
                {
                    if (File.Exists(savePath))
                    {
                        if (File.Exists(savePath + ".tmp"))
                        {
                            File.Delete(savePath + ".tmp");
                        }

                        File.Move(savePath, savePath + ".tmp");
                    }

                    StreamWriter streamWriter = new StreamWriter(savePath);
                    streamWriter.Write(json);
                    streamWriter.Close();
                }
                catch (Exception e)
                {
                    Debug.Log("Unable to save dialogue variable values to " + savePath + " " + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Loads variable values for the dialogue from a save if available.
        /// </summary>
        /// <param name="prefix">The prefix to use when loading. This prefix is appended to ('_' + the dialogue name) or )'_' + the dialogue name + '.json'), depending on whether 
        /// values are to be loaded from PlayerPrefs or a JSON file.</param>
        /// <param name="loadFromPlayerPrefs"></param>
        public void LoadVariableValues(string prefix = "", bool loadFromPlayerPrefs = false)
        {
            NodeVariableValueCollection localVariableValues = LoadVariableValueCollection(dialogue.name, prefix, loadFromPlayerPrefs);

            if (localVariableValues != null)
            {
                LoadVariableValues(localVariableValues);
            }
        }

        /// <summary>
        /// Loads global variable values from a save if available.
        /// </summary>
        /// <param name="prefix">The prefix to use when loading. This prefix is appended to '_global' or '_global.json', depending on whether 
        /// values are to be loaded from player prefs or a JSON file.</param>
        /// <param name="loadFromPlayerPrefs">If true, variable states will be loaded from PlayerPrefs rather than a JSON file.</param>
        public void LoadGlobalVariableValues(string prefix = "", bool loadFromPlayerPrefs = false)
        {
            NodeVariableValueCollection globalVariableValues = LoadVariableValueCollection("global", prefix, loadFromPlayerPrefs);

            if (globalVariableValues != null)
            {
                LoadGlobalVariableValues(globalVariableValues);
            }
        }

        /// <summary>
        /// Loads the states of variables from a save if available.
        /// </summary>
        /// <param name="suffix">The suffix to use when loading the values.</param>
        /// <param name="prefix">The prefix to use when loading the values.</param>
        /// <param name="loadFromPlayerPrefs">If true, variable states will be loaded from PlayerPrefs rather than a JSON file.</param>
        private NodeVariableValueCollection LoadVariableValueCollection(string suffix, string prefix = "", bool loadFromPlayerPrefs = false)
        {
            string json = null;

            if (loadFromPlayerPrefs)
            {
                json = PlayerPrefs.GetString(prefix + "_" + suffix);
            }
            else
            {
                string loadPath = Application.persistentDataPath + "/" + prefix + "_" + suffix + ".json";

                if (File.Exists(loadPath))
                {
                    try
                    {
                        StreamReader streamReader = new StreamReader(loadPath);
                        json = streamReader.ReadToEnd();
                        streamReader.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Cannot read variable values fro dialogue from " + loadPath + " " + e.StackTrace);
                    }
                }
            }

            if (json != null)
            {
                try
                {
                    NodeVariableValueCollection valueCollection = JsonUtility.FromJson<NodeVariableValueCollection>(json);
                    return valueCollection;
                }
                catch (Exception e)
                {
                    Debug.LogError("Cannot load variable values from json: '" + json + "' " + e.StackTrace);
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the variable values from the specified NodeVariableValueCollection into the Dialogue.
        /// </summary>
        /// <param name="valueCollection">The collection of node variable values to load.</param>
        public void LoadVariableValues(NodeVariableValueCollection valueCollection)
        {
            if (valueCollection != null)
            {
                LoadVariableValuesFromCollection(valueCollection, variables);
            }
        }

        /// <summary>
        /// Loads the values from the specified NodeVariableValueCollection into the collection of global variables.
        /// </summary>
        /// <param name="valueCollection">The collection of global variable values to load.</param>
        public void LoadGlobalVariableValues(NodeVariableValueCollection valueCollection)
        {
            if (valueCollection != null)
            {
                LoadVariableValuesFromCollection(valueCollection, globalVariables);
            }
        }

        /// <summary>
        /// Loads the values from the specified NodeVariableValueCollection into the provided Dictionary of variable names mapped to NodeVariable values.
        /// </summary>
        /// <param name="valueCollection">The collection of variable values to load.</param>
        /// <param name="dialogueVariables">The Dictionary to load variables into.</param>
        private void LoadVariableValuesFromCollection(NodeVariableValueCollection valueCollection, Dictionary<string, NodeVariable> dialogueVariables)
        {
            foreach (NodeVariableValue nodeValue in valueCollection.values)
            {
                NodeVariable variable = null;

                if (dialogueVariables.ContainsKey(nodeValue.variableName))
                {
                    variable = dialogueVariables[nodeValue.variableName];

                    object value = null;

                    if (variable.variableType == typeof(float))
                    {
                        value = Convert.ToSingle(nodeValue.value);
                    }
                    else if (variable.variableType == typeof(int))
                    {
                        value = Convert.ToInt32(nodeValue.value);
                    }
                    else if (variable.variableType == typeof(bool))
                    {
                        value = Convert.ToBoolean(nodeValue.value);
                    }
                    else if (variable.variableType == typeof(string))
                    {
                        value = Convert.ToString(nodeValue.value);
                    }

                    variable.currentValue = value;
                }
            }
        }

        /// <summary>
        /// Returns a new NodeVariableValueCollection containing the current values of each variable in the Dialogue.
        /// </summary>
        /// <returns>A collection of current variable values for the Dialogue.</returns>
        public NodeVariableValueCollection GetVariableValues()
        {
            return CreateNodeVariableValueCollection(variables);
        }

        /// <summary>
        /// Returns a new NodeVariableValueCollection containing the current values of all global dialogue variables.
        /// </summary>
        /// <returns>A collection of the current global dialogue variable values.</returns>
        public NodeVariableValueCollection GetGlobalVariableValues()
        {
            return CreateNodeVariableValueCollection(globalVariables);
        }

        /// <summary>
        /// Creates and returns a new NodeVariableValueCollection containing the current values of all of the variables in the provided Dictionary.
        /// </summary>
        /// <param name="dialogueVariables">The variable dictionary to populate from.</param>
        /// <returns>A collection of variable values from the Dictionary provided.</returns>
        private NodeVariableValueCollection CreateNodeVariableValueCollection(Dictionary<string, NodeVariable> dialogueVariables)
        {
            NodeVariableValueCollection valueCollection = new NodeVariableValueCollection();

            foreach (string varName in dialogueVariables.Keys)
            {
                NodeVariableValue varState = new NodeVariableValue();
                varState.variableName = varName;
                varState.value = dialogueVariables[varName].currentValue.ToString();
                valueCollection.values.Add(varState);
            }

            return valueCollection;
        }

        /// <summary>
        /// Translates the specified string to a localized string using the TranslationLibrary of the Dialogue.
        /// </summary>
        /// <param name="text">The line of text to translate.</param>
        /// <returns>The translated text, if a translation is found that matches the text provided.</returns>
        public string TranslateText(string text)
        {
            if(dialogue.TranslationLibrary == null)
            {
                return text;
            }

            string languageCode = EasyTalkGameState.Instance.Language;

            if(languageCode == null || languageCode.Equals(dialogue.TranslationLibrary.originalLanguage))
            {
                return text;
            }

            string newText = text;
            NodeTag translateTag;
            newText = NodeTag.ExtractTag(newText, "translate", out translateTag);

            if(translateTag != null)
            {
                if(!(translateTag as TranslateTag).translate)
                {
                    return newText;
                }
            }

            if (languageCode != null)
            {
                if(languageCode.Equals(dialogue.TranslationLibrary.originalLanguage))
                {
                    return newText;
                }
                else if(newText != null)
                {
                    Translation translation = dialogue.TranslationLibrary.GetTranslation(newText, languageCode);
                    if (translation != null && translation.text != null && translation.text.Length > 0)
                    {
                        return translation.text;
                    }
                }
            }

            return newText;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveVariables()
        {
            SaveVariableValues(EasyTalkGameState.Instance.SessionID);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveGlobalVariables()
        {
            SaveGlobalVariableValues(EasyTalkGameState.Instance.SessionID);
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadVariables()
        {
            LoadVariableValues(EasyTalkGameState.Instance.SessionID);
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadGlobalVariables()
        {
            LoadGlobalVariableValues(EasyTalkGameState.Instance.SessionID);
        }

        /// <summary>
        /// Sets the Dialogue Listener which the node handler should send events to during Dialogue processing.
        /// </summary>
        /// <param name="listener">The Dialogue Listener to use.</param>
        public void SetListener(DialogueListener listener)
        {
            this.listener = listener;
        }

        /// <summary>
        /// Gets the Dialogue Listener of the node handler.
        /// </summary>
        public DialogueListener Listener
        {
            get { return this.listener; }
        }

        /// <summary>
        /// Gets or sets the Dialogue Settings used by the node handler.
        /// </summary>
        public EasyTalkDialogueSettings DialogueSettings
        {
            get { return dialogueSettings; }
            set { dialogueSettings = value; }
        }

        /// <summary>
        /// Attempts to initialize all global variables, if they can be found in either the DialogueRegistry provided, or in a DialogueRegistry in the current 
        /// Dialogue Settings, or in the Dialogue Settings of a Dialogue Display in the Hierarchy.
        /// </summary>
        /// <param name="registry">A Dialogue Registry to initialize global variables from.</param>
        public void InitializeGlobalVariables(DialogueRegistry registry = null)
        {
            DialogueRegistry dialogueRegistry = registry;

            if (registry == null)
            {
                if (dialogueSettings != null && dialogueSettings.DialogueRegistry != null)
                {
                    dialogueRegistry = dialogueSettings.DialogueRegistry;
                }
                else if(dialogueSettings == null)
                {
#if UNITY_6000_0_OR_NEWER
                    DialogueDisplay display = GameObject.FindFirstObjectByType<DialogueDisplay>(FindObjectsInactive.Include);
#else
                    DialogueDisplay display = GameObject.FindObjectOfType<DialogueDisplay>(true);
#endif
					if (display != null && display.DialogueSettings != null)
					{
						dialogueSettings = display.DialogueSettings;

						if (display.DialogueSettings.DialogueRegistry != null)
						{
							dialogueRegistry = display.DialogueSettings.DialogueRegistry;
						}
					}
                }
            }
            

            if(dialogueRegistry != null)
            {
                foreach (GlobalNodeVariable globalVar in dialogueRegistry.GlobalVariables)
                {
                    if (!globalVariables.ContainsKey(globalVar.VariableName))
                    {
                        switch (globalVar.VariableType)
                        {
                            case GlobalVariableType.STRING:
                                CreateVariable(globalVar.GetTrueType(), globalVar.VariableName, globalVar.InitialValue, globalVar.InitialValue, false, true); break;
                            case GlobalVariableType.INT:
                                int intValue = 0;
                                int.TryParse(globalVar.InitialValue, out intValue);
                                CreateVariable(globalVar.GetTrueType(), globalVar.VariableName, intValue, intValue, false, true); break;
                            case GlobalVariableType.FLOAT:
                                float floatValue = 0.0f;
                                float.TryParse(globalVar.InitialValue, out floatValue);
                                CreateVariable(globalVar.GetTrueType(), globalVar.VariableName, floatValue, floatValue, false, true); break;
                            case GlobalVariableType.BOOL:
                                bool boolValue = true;
                                bool.TryParse(globalVar.InitialValue, out boolValue);
                                CreateVariable(globalVar.GetTrueType(), globalVar.VariableName, boolValue, boolValue, false, true); break;
                        }

                    }
                }
            }
        }
    }

    /// <summary>
    /// This class is used to store a collection of node variable values.
    /// </summary>
    [Serializable]
    public class NodeVariableValueCollection
    {
        /// <summary>
        /// The collection of node variable values.
        /// </summary>
        [SerializeField]
        public List<NodeVariableValue> values = new List<NodeVariableValue>();
    }

    /// <summary>
    /// This class stores a node variable's name and value.
    /// </summary>
    [Serializable]
    public class NodeVariableValue
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        [SerializeField]
        public string variableName;

        /// <summary>
        /// The value of the variable.
        /// </summary>
        [SerializeField]
        public string value;
    }
}