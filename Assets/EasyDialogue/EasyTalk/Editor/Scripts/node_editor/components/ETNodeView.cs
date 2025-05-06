using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;
using System.Linq;
using EasyTalk.Editor.Nodes;
using EasyTalk.Editor.Utils;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes;
using EasyTalk.Editor.Ledger.Actions;
using System;
using EasyTalk.Settings;
using EasyTalk.Nodes.Variable;

namespace EasyTalk.Editor.Components
{
    public class ETNodeView : Box
    {
        private List<ETNode> nodes = new List<ETNode>();

        private bool isDraggingNode = false;

        private Vector3 lastMousePos; //in local coordinates

        private Dictionary<int, ETNode> selectedNodes = new Dictionary<int, ETNode>();

        private Dictionary<int, ETOutput> outputIdMap = new Dictionary<int, ETOutput>();

        private Dictionary<int, ETInput> inputIdMap = new Dictionary<int, ETInput>();

        private Dictionary<int, ETNode> outputToNodeMap = new Dictionary<int, ETNode>();

        private Dictionary<int, ETNode> inputToNodeMap = new Dictionary<int, ETNode>();

        private List<string> characters = new List<string>();
        private List<string> convoIds = new List<string>();

        private Rect selectionRect;
        private Vector2 selectionStartPoint;
        private SelectionMode selectionMode;

        private bool creatingConnection = false;
        private ETInput fromInputPort;
        private ETOutput fromOutputPort;

        public delegate void OnVariableNameChanged(string oldVariableName, string newVariableName);
        public OnVariableNameChanged onVariableNameChanged;

        public delegate void OnVariableAdded(string variableName);
        public OnVariableAdded onVariableAdded;

        public delegate void OnVariableRemoved(string variableName);
        public OnVariableRemoved onVariableRemoved;

        private ETQuickCreateWheel quickCreateWheel;

        private float highlightWidth = 4.0f;

        private Thread performanceFilterThread;
        private bool performanceFilterActive = true;
        private int filterCounter = 0;

        private Dictionary<int, Vector2> oldNodePositions = new Dictionary<int, Vector2>();

        public ETNodeView() : base()
        {
            this.name = "Node View";
            this.AddToClassList("node-view");

            this.RegisterCallback<MouseUpEvent>(OnMouseUp);
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.RegisterCallback<AttachToPanelEvent>(OnAddedToParent);

            this.pickingMode = PickingMode.Ignore;
            this.generateVisualContent += DrawCanvas;
            this.usageHints = UsageHints.DynamicTransform;
        }

        public void DeleteAll()
        {
            characters.Clear();
            convoIds.Clear();
            outputIdMap.Clear();
            outputToNodeMap.Clear();
            inputIdMap.Clear();
            inputToNodeMap.Clear();
            selectedNodes.Clear();

            foreach (ETNode node in nodes)
            {
                Remove(node);
            }

            nodes.Clear();
            fromInputPort = null;
            fromOutputPort = null;

            MarkDirtyRepaint();
        }

        public ETNode GetNodeForInput(int inputId)
        {
            if (inputToNodeMap.ContainsKey(inputId))
            {
                return inputToNodeMap[inputId];
            }

            return null;
        }

        public ETNode GetNodeForOutput(int outputId)
        {
            if (outputToNodeMap.ContainsKey(outputId))
            {
                return outputToNodeMap[outputId];
            }

            return null;
        }

        private void OnAddedToParent(AttachToPanelEvent evt)
        {
            if (this.parent != null)
            {
                this.parent.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                this.parent.RegisterCallback<WheelEvent>(OnZoom);
                this.parent.RegisterCallback<KeyDownEvent>(OnKeyDown);
                this.parent.RegisterCallback<KeyUpEvent>(OnKeyUp);
                this.parent.RegisterCallback<MouseUpEvent>(OnMouseUp);
                this.parent.RegisterCallback<MouseDownEvent>(OnMouseDown);

                (this.parent as ETPanZoomPanel).onPan += delegate { filterCounter++; };
                (this.parent as ETPanZoomPanel).onZoomOut += delegate { filterCounter++; };
                (this.parent as ETPanZoomPanel).onZoomIn += delegate { filterCounter++; };

                (this.parent as ETPanZoomPanel).onAutoPan += OnAutoPan;
                (this.parent as ETPanZoomPanel).onAutoPan += OnAutoPanStart;

                if (quickCreateWheel == null)
                {
                    quickCreateWheel = new ETQuickCreateWheel();
                    this.parent.Add(quickCreateWheel);
                    quickCreateWheel.Hide();
                }

                StartPerformanceFilterThread();
            }
        }

        public void OnAutoPanStart(Vector3 mousePos)
        {
            this.lastMousePos = (this.parent as VisualElement).ChangeCoordinatesTo(this, mousePos); ;
        }

        public void OnAutoPan(Vector3 mousePos)
        {
            Vector3 newMousePos = (this.parent as VisualElement).ChangeCoordinatesTo(this, mousePos);
            Vector3 deltaPos = newMousePos - lastMousePos;
            lastMousePos = newMousePos;

            UpdateDraggedNodePositions(deltaPos);

            //Repaint if the user is creating a connection.
            if (creatingConnection)
            {
                MarkDirtyRepaint();
            }

            UpdateSelectionBoxSize();
        }

        private void UpdateDraggedNodePositions(Vector3 positionChange)
        {
            //Move the selected nodes based on how much the mouse was moved if currently dragging nodes.
            if (isDraggingNode)
            {
                bool storeOldPositions = false;
                if (oldNodePositions.Count == 0) { storeOldPositions = true; }

                foreach (ETNode node in selectedNodes.Values)
                {
                    if (storeOldPositions) { oldNodePositions.Add(node.ID, node.transform.position); }
                    node.transform.position = node.transform.position + positionChange;
                }

                MarkDirtyRepaint();
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }

        private void UpdateSelectionBoxSize()
        {
            //Recalculate the rectangle for the current selection box.
            if (selectionMode != SelectionMode.NONE)
            {
                selectionRect = new Rect(
                        Mathf.Min(selectionStartPoint.x, lastMousePos.x),
                        Mathf.Min(selectionStartPoint.y, lastMousePos.y),
                        Mathf.Abs(selectionStartPoint.x - lastMousePos.x),
                        Mathf.Abs(selectionStartPoint.y - lastMousePos.y));
                MarkDirtyRepaint();
            }
        }

        public void OnZoom(WheelEvent evt)
        {
            lastMousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(this, evt.localMousePosition);
            UpdateHighlightWidth();

            EditorApplication.delayCall += MarkDirtyRepaint;
        }

        private void UpdateHighlightWidth()
        {
            if (this.parent is ETPanZoomPanel)
            {
                ETPanZoomPanel pzp = this.parent as ETPanZoomPanel;
                this.highlightWidth = 4.0f / pzp.Zoom;

                foreach(ETNode node in nodes)
                {
                    node.MarkDirtyRepaint();
                }
            }
        }

        public void OnKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.LeftShift && selectionMode != SelectionMode.NONE)
            {
                //If using a selection box, switch to SELECT mode if Shift is released.
                selectionMode = SelectionMode.SELECT;
                MarkDirtyRepaint();
            }

            if (evt.keyCode == KeyCode.LeftAlt)
            {
                quickCreateWheel.Hide();
                //quickCreateGrid.Hide();
            }
        }

        public void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.A)
            {
                if (evt.ctrlKey)
                {
                    //Select all nodes if Ctrl+A is pressed.
                    SelectAllNodes();
                }
                else if (evt.shiftKey)
                {
                    //Deselect all nodes if Ctrl+Shift+A OR Alt+A is pressed.
                    DeselectAllNodes();
                }
            }
            else if (evt.keyCode == KeyCode.I && evt.ctrlKey)
            {
                //Invert the selection if Ctrl+I is pressed.
                InvertSelection();
            }
            else if (evt.keyCode == KeyCode.LeftShift && selectionMode != SelectionMode.NONE)
            {
                //Change the selection mode to DESELECT if currently creating a selection and holding Shift.
                selectionMode = SelectionMode.DESELECT;
                MarkDirtyRepaint();
            }
            else if (evt.keyCode == KeyCode.Delete)
            {
                //Delete the selected nodes if Delete is pressed.
                DeleteNodes(true);
            }
            else if (evt.keyCode == KeyCode.LeftAlt)
            {
                quickCreateWheel.Show();
            }
            else if (evt.keyCode == KeyCode.C && evt.ctrlKey)
            {
                CopySelection();
            }
            else if (evt.keyCode == KeyCode.V && evt.ctrlKey)
            {
                PasteClipboard();
            }
            else if (evt.keyCode == KeyCode.X && evt.ctrlKey && evt.currentTarget != this)
            {
                if (evt.shiftKey)
                {
                    PanAndZoomToAll();
                }
                else
                {
                    PanAndZoomToSelected();
                }
            }
            else if(evt.keyCode == KeyCode.S && evt.ctrlKey && evt.currentTarget != this)
            {
                EasyTalkNodeEditor.Instance.SaveChanges();
            }
            else if(evt.keyCode == KeyCode.O && evt.ctrlKey && evt.currentTarget != this)
            {
                evt.StopImmediatePropagation();
                EasyTalkNodeEditor.Instance.FileManager.OpenFile();
            }
            else if(evt.keyCode == KeyCode.N && evt.ctrlKey && evt.currentTarget != this)
            {
                evt.StopImmediatePropagation();
                EasyTalkNodeEditor.Instance.FileManager.NewFile();
            }
            else if(evt.keyCode == KeyCode.F && evt.ctrlKey && evt.currentTarget != this)
            {
                EasyTalkNodeEditor.Instance.ShowSearchPanel();
            }
            else if(evt.keyCode == KeyCode.Z && evt.ctrlKey && evt.currentTarget != this)
            {
                evt.StopImmediatePropagation();
                EasyTalkNodeEditor.Instance.Ledger.Undo(this);
                this.Focus();
                MarkDirtyRepaint();
            }
            else if(evt.keyCode == KeyCode.Y && evt.ctrlKey && evt.currentTarget != this)
            {
                evt.StopImmediatePropagation();
                EasyTalkNodeEditor.Instance.Ledger.Redo(this);
                this.Focus();
                MarkDirtyRepaint();
            }
        }

        public void CopySelection()
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            foreach (ETNode nmNode in selectedNodes.Values)
            {
                dialogue.Nodes.Add(nmNode.CreateNode());
            }

            string json = JsonUtility.ToJson(dialogue);
            GUIUtility.systemCopyBuffer = json;
        }

        public void PasteClipboard()
        {
            try
            {
                Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
                JsonUtility.FromJsonOverwrite(GUIUtility.systemCopyBuffer, dialogue);

                NodeUtils.GenerateNewIDs(dialogue.Nodes);
                Vector3 centerOffset = (this.parent.ChangeCoordinatesTo(this, this.parent.contentRect.center) - dialogue.GetCenter());

                foreach (Node node in dialogue.Nodes)
                {
                    ETNode nmNode = CreateNode(node.NodeType, node, false);
                    nmNode.transform.position += centerOffset;
                }
            }
            catch
            {
                Debug.Log("Unable to paste clipboard to node view.");
            }

            MarkDirtyRepaint();
        }

        public ETNode CreateNode(NodeType nodeType, Node fromNode = null, bool center = true)
        {
            ETNode node = CreateNodeForType(nodeType);

            if (node == null) { return null; }

            AddNodeToDisplay(node, center);

            if (fromNode != null)
            {
                node.InitializeFromNode(fromNode);
            }

            RegisterNode(node);
            EasyTalkNodeEditor.Instance.NodesChanged();

            EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeCreatedAction(node));

            return node;
        }

        private ETNode CreateNodeForType(NodeType nodeType)
        {
            ETNode node = null;

            switch (nodeType)
            {
                case NodeType.ENTRY: node = new ETEntryNode(); break;
                case NodeType.STORY: node = new ETStoryNode(); break;
                case NodeType.EXIT: node = new ETExitNode(); break;
                case NodeType.JUMPIN: node = new ETJumpInNode(); break;
                case NodeType.JUMPOUT: node = new ETJumpOutNode(); break;
                case NodeType.CONVO: node = new ETConversationNode(); break;
                case NodeType.OPTION: node = new ETOptionNode(); break;
                case NodeType.BOOL_VARIABLE: node = new ETVariableNode(NodeVariableType.BOOL); break;
                case NodeType.INT_VARIABLE: node = new ETVariableNode(NodeVariableType.INT); break;
                case NodeType.FLOAT_VARIABLE: node = new ETVariableNode(NodeVariableType.FLOAT); break;
                case NodeType.STRING_VARIABLE: node = new ETVariableNode(NodeVariableType.STRING); break;
                case NodeType.RANDOM: node = new ETRandomChoiceNode(); break;
                case NodeType.SEQUENCE: node = new ETSequenceNode(); break;
                case NodeType.BOOL_LOGIC: node = new ETBoolLogicNode(); break;
                case NodeType.MATH: node = new ETMathNode(); break;
                case NodeType.NUMBER_COMPARE: node = new ETCompareNumbersNode(); break;
                case NodeType.STRING_COMPARE: node = new ETCompareStringsNode(); break;
                case NodeType.BUILD_STRING: node = new ETBuildStringNode(); break;
                case NodeType.OPTION_MOD: node = new ETOptionModifierNode(); break;
                case NodeType.TRIGGER: node = new ETTriggerScriptNode(); break;
                case NodeType.GET_VARIABLE_VALUE: node = new ETGetVariableNode(); break;
                case NodeType.SET_VARIABLE_VALUE: node = new ETSetVariableNode(); break;
                case NodeType.PATH_SELECT: node = new ETPathSelectorNode(); break;
                case NodeType.VALUE_SELECT: node = new ETValueSelectorNode(); break;
                case NodeType.WAIT: node = new ETWaitNode(); break;
                case NodeType.PAUSE: node = new ETPauseNode(); break;
                case NodeType.CONDITIONAL_VALUE: node = new ETConditionalValueNode(); break;
                case NodeType.PLAYER_INPUT: node = new ETPlayerInputNode(); break;
                case NodeType.SHOW: node = new ETShowNode(); break;
                case NodeType.HIDE: node = new ETHideNode(); break;
                case NodeType.GOTO: node = new ETGotoNode(); break;
                case NodeType.APPEND: node = new ETAppendNode(); break;
                /*case NodeType.AI_INIT: node = new ETAIInitNode(); break;
                case NodeType.AI_ADD_MESSAGE: node = new ETAIAddMessageNode(); break;
                case NodeType.AI_PROMPT: node = new ETAIPromptNode(); break;*/
            }

            return node;
        }

        private void AddNodeToDisplay(ETNode node, bool center = true)
        {
            nodes.Add(node);
            this.Add(node);

            if (center)
            {
                CenterNode(node);
            }
        }

        private void CenterNode(ETNode node)
        {
            Vector2 offset = new Vector2(node.style.width.value.value / 2.0f, node.style.height.value.value / 2.0f);
            node.transform.position = this.parent.ChangeCoordinatesTo(this, this.parent.contentRect.center) - offset;
        }

        public Vector2 GetSelectionCenter()
        {
            Vector2 selectionCenter = Vector2.zero;
            foreach (ETNode node in selectedNodes.Values)
            {
                selectionCenter += node.GetCenter();
            }

            selectionCenter /= selectedNodes.Count;
            return selectionCenter;
        }

        public Rect GetNodeBoundingBox(bool onlySelected = true)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool selectionValid = false;

            foreach (ETNode node in nodes)
            {
                if ((onlySelected && node.IsSelected) || !onlySelected)
                {
                    Vector2 center = node.GetCenter();
                    float left = center.x - (node.GetWidth() / 2.0f);
                    float right = center.x + (node.GetWidth() / 2.0f);
                    float top = center.y - (node.GetHeight() / 2.0f);
                    float bottom = center.y + (node.GetHeight() / 2.0f);
                    minX = Mathf.Min(left, minX);
                    maxX = Mathf.Max(right, maxX);
                    minY = Mathf.Min(top, minY);
                    maxY = Mathf.Max(bottom, maxY);
                    selectionValid = true;
                }
            }

            if (selectionValid)
            {
                return new Rect((maxX + minX) / 2.0f, (maxY + minY) / 2.0f, Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY));
            }
            else
            {
                return this.contentRect;
            }
        }

        public void PanToSelected()
        {
            Vector2 panLocation = this.ChangeCoordinatesTo(parent, GetSelectionCenter());
            (this.parent as ETPanZoomPanel).PanTo(panLocation);
        }

        public void PanAndZoomToSelected()
        {
            PanToSelected();

            Rect bounds = GetNodeBoundingBox();
            (this.parent as ETPanZoomPanel).ZoomToView(bounds);
            UpdateHighlightWidth();
        }

        public void PanToAll()
        {
            Vector3 center = Vector3.zero;
            foreach (ETNode node in nodes)
            {
                center += node.transform.position;
            }
            center /= nodes.Count;
            center = this.ChangeCoordinatesTo(parent, center);
            (this.parent as ETPanZoomPanel).PanTo(center);
        }

        public void PanAndZoomToAll()
        {
            PanToAll();

            Rect bounds = GetNodeBoundingBox(false);
            (this.parent as ETPanZoomPanel).ZoomToView(bounds);
            UpdateHighlightWidth();
        }

        private void RegisterNode(ETNode node)
        {
            RegisterNodeInputs(node);
            RegisterNodeOutputs(node);

            if (node is ETListNode)
            {
                (node as ETListNode).ListPanel.onItemAdded += NodeContentAdded;
                (node as ETListNode).ListPanel.onItemRemoved += NodeContentRemoved;
            }

            if (node is ETVariableNode)
            {
                //Make sure we get notified of variable names changing in variable nodes.
                (node as ETVariableNode).onVariableNameChanged += VariableNameChanged;

                if (onVariableAdded != null)
                {
                    onVariableAdded.Invoke((node as ETVariableNode).VariableName);
                }
            }

            node.onNodeSelected += NodeSelected;
            node.onNodeDeselected += NodeDeselected;
        }

        private void NodeSelected(ETNode node)
        {
            if (!selectedNodes.ContainsKey(node.ID))
            {
                selectedNodes.Add(node.ID, node);
            }

            HandleSettingsForSelection();
        }

        private void NodeDeselected(ETNode node)
        {
            if (selectedNodes.ContainsKey(node.ID))
            {
                selectedNodes.Remove(node.ID);
            }

            HandleSettingsForSelection();
        }

        private void HandleSettingsForSelection()
        {
            if (selectedNodes.Count == 0 || selectedNodes.Count > 1)
            {
                EasyTalkNodeEditor.Instance.SettingsPanel.ClearContentPanel();
                EasyTalkNodeEditor.Instance.SettingsPanel.Hide(true);
            }
            else
            {
                ETNode selectedNode = selectedNodes.First().Value;
                EasyTalkNodeEditor.Instance.SettingsPanel.SetActiveNode(selectedNode);
                EasyTalkNodeEditor.Instance.SettingsPanel.ShowIfActive();
            }
        }

        private void VariableNameChanged(string oldName, string newName)
        {
            //Update all nodes referencing the old variable name to use the new one instead.
            if (onVariableNameChanged != null)
            {
                onVariableNameChanged.Invoke(oldName, newName);
            }
        }

        private void NodeContentAdded(ETNode node, ETNodeContent content)
        {
            foreach (ETInput input in content.GetInputs())
            {
                RegisterNodeInput(input, node);
            }

            foreach (ETOutput output in content.GetOutputs())
            {
                RegisterNodeOutput(output, node);
            }

            MarkDirtyRepaint();
        }

        private void NodeContentRemoved(ETNode node, ETNodeContent content)
        {
            foreach (ETInput input in content.GetInputs())
            {
                UnregisterNodeInput(input);
            }

            foreach (ETOutput output in content.GetOutputs())
            {
                UnregisterNodeOutput(output);
            }

            MarkDirtyRepaint();
        }

        private void RegisterNodeInputs(ETNode node)
        {
            List<ETInput> nodeInputs = node.Inputs;
            foreach (ETInput input in nodeInputs)
            {
                RegisterNodeInput(input, node);
            }
        }

        public void RegisterNodeInput(ETInput input, ETNode node)
        {
            if (!inputIdMap.ContainsKey(input.ID))
            {
                inputIdMap.Add(input.ID, input);
            }

            if (!inputToNodeMap.ContainsKey(input.ID))
            {
                inputToNodeMap.Add(input.ID, node);
            }
        }

        private void RegisterNodeOutputs(ETNode node)
        {
            List<ETOutput> nodeOutputs = node.Outputs;
            foreach (ETOutput output in nodeOutputs)
            {
                RegisterNodeOutput(output, node);
            }
        }

        public void RegisterNodeOutput(ETOutput output, ETNode node)
        {
            if (!outputIdMap.ContainsKey(output.ID))
            {
                outputIdMap.Add(output.ID, output);
            }

            if (!outputToNodeMap.ContainsKey(output.ID))
            {
                outputToNodeMap.Add(output.ID, node);
            }
        }

        public void WindowEnteredOrExited()
        {
            if (selectionMode != SelectionMode.NONE)
            {
                selectionMode = SelectionMode.NONE;
                MarkDirtyRepaint();
            }

            if (creatingConnection)
            {
                creatingConnection = false;

                if (fromInputPort != null)
                {
                    inputToNodeMap[fromInputPort.ID].style.opacity = 1.0f;
                    fromInputPort = null;
                }

                if (fromOutputPort != null)
                {
                    outputToNodeMap[fromOutputPort.ID].style.opacity = 1.0f;
                    fromOutputPort = null;
                }

                MarkDirtyRepaint();
            }
        }

        public void OnMouseMove(MouseMoveEvent evt)
        {
            Vector3 mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(this, evt.localMousePosition);
            Vector3 deltaPos = mousePos - lastMousePos;

            UpdateDraggedNodePositions(deltaPos);

            //Repaint if the user is creating a connection.
            if (creatingConnection)
            {
                MarkDirtyRepaint();
            }

            UpdateSelectionBoxSize();

            lastMousePos = mousePos;
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            EasyTalkNodeEditor.Instance.SetCursor(MouseCursor.Arrow);

            Vector3 mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(this, evt.localMousePosition);

            if (isDraggingNode)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeMovedAction(oldNodePositions));
                oldNodePositions.Clear();
                isDraggingNode = false;

                foreach(ETNode node in selectedNodes.Values)
                {
                    node.NodeMoved();
                }
            }

            EasyTalkNodeEditor.Instance.PanZoomPanel.EdgePanningEnabled = false;

            if (evt.button == 0)
            {
                VisualElement currentElement = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

                if (Vector2.Distance(mousePos, selectionStartPoint) <= 1.0f)
                {
                    //If the user released the mouse very close to the point where they pressed the mouse, deselect all currently selected nodes.
                    if (currentElement is ETPanZoomPanel)
                    {
                        DeselectAllNodes();
                    }

                    selectionMode = SelectionMode.NONE;
                    MarkDirtyRepaint();
                }
                else if (selectionMode != SelectionMode.NONE)
                {
                    //If we're currently selecting nodes within an area, select or deselect all of the nodes in the area after the mouse is released.
                    foreach (ETNode node in nodes)
                    {
                        if (selectionRect.Contains(node.transform.position + new Vector3(node.style.width.value.value / 2.0f, node.style.height.value.value / 2.0f)))
                        {
                            if (evt.shiftKey)
                            {
                                node.Deselect();
                            }
                            else
                            {
                                node.Select();
                            }
                        }
                    }

                    selectionMode = SelectionMode.NONE;
                    MarkDirtyRepaint();
                }
            }
            else if (evt.button == 1 && evt.currentTarget == this)
            {
                //If current element is an input or output, delete the attached connections.
                VisualElement currentElement = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

                if (currentElement is ETInput)
                {
                    ETInput input = currentElement as ETInput;
                    DeleteConnections(input);
                }
                else if (currentElement is ETOutput)
                {
                    ETOutput output = currentElement as ETOutput;
                    DeleteConnections(output);
                }
            }

            if (fromInputPort != null)
            {
                inputToNodeMap[fromInputPort.ID].style.opacity = 1.0f;
            }

            if (fromOutputPort != null)
            {
                outputToNodeMap[fromOutputPort.ID].style.opacity = 1.0f;
            }

            //If we were creating a connection and the mouse was released on an input/output which is connectable, make the connection
            if (creatingConnection)
            {
                VisualElement currentElement = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

                if (fromInputPort != null && currentElement is ETOutput)
                {
                    ETOutput output = currentElement as ETOutput;
                    CreateConnection(fromInputPort, output);
                }
                else if (fromOutputPort != null && currentElement is ETInput)
                {
                    ETInput input = currentElement as ETInput;
                    CreateConnection(input, fromOutputPort);
                }

                MarkDirtyRepaint();
                creatingConnection = false;
                fromInputPort = null;
                fromOutputPort = null;
            }
        }

        public void OnMouseDown(MouseDownEvent evt)
        {
            Vector3 mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(this, evt.localMousePosition);

            if (evt.button == 0)
            {
                VisualElement currentElement = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

                if (currentElement != null)
                {
                    if (currentElement is ETPanZoomPanel)
                    {
                        //Start drawing a selection box to create a selection within an area.
                        selectionMode = (evt.shiftKey) ? SelectionMode.DESELECT : SelectionMode.SELECT;
                        selectionStartPoint = mousePos;
                        selectionRect = new Rect(mousePos.x, mousePos.y, 0f, 0f);
                        EasyTalkNodeEditor.Instance.PanZoomPanel.EdgePanningEnabled = true;
                    }
                    else if (currentElement is ETNode)
                    {
                        if (evt.ctrlKey && currentElement is ETJumpOutNode)
                        {
                            //Jump to the jump-in node associated with the jump-out node, if found.
                            ETJumpOutNode jumpOutNode = (ETJumpOutNode)currentElement;
                            string jumpOutKey = jumpOutNode.Key;

                            if (jumpOutKey != null && jumpOutKey.Length > 0)
                            {
                                foreach (ETNode node in nodes)
                                {
                                    if (node is ETJumpInNode)
                                    {
                                        string jumpInKey = (node as ETJumpInNode).Key;
                                        if (jumpInKey != null && jumpInKey.Length > 0 && jumpInKey.Equals(jumpOutNode.Key))
                                        {
                                            (this.parent as ETPanZoomPanel).PanTo(this.ChangeCoordinatesTo(parent, node.GetCenter()));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if(evt.currentTarget == this)
                        {
                            //Select the node at the mouse position.
                            ETNode node = currentElement as ETNode;

                            //If the user isn't pressing the shift key, or the node is not selected (meaning it is not part of a selection being dragged),
                            //then deselect all of the nodes currently selected.
                            if (!evt.shiftKey && !node.IsSelected)
                            {
                                DeselectAllNodes();
                            }

                            if (evt.shiftKey && node.IsSelected) 
                            {
                                node.Deselect(); 
                            }
                            else
                            {
                                node.Select();
                            }

                            isDraggingNode = true;
                            EasyTalkNodeEditor.Instance.PanZoomPanel.EdgePanningEnabled = true;
                        }
                    }
                    else if (currentElement is ETInput)
                    {
                        //If current element is an output, start creating a connection coming from output.
                        creatingConnection = true;
                        fromInputPort = currentElement as ETInput;
                        inputToNodeMap[fromInputPort.ID].style.opacity = 0.5f;
                        EasyTalkNodeEditor.Instance.PanZoomPanel.EdgePanningEnabled = true;
                        EasyTalkNodeEditor.Instance.SetCursor(MouseCursor.ArrowPlus);
                    }
                    else if (currentElement is ETOutput)
                    {
                        //If current element is an input, start creating a connection coming from input.
                        creatingConnection = true;
                        fromOutputPort = currentElement as ETOutput;
                        outputToNodeMap[fromOutputPort.ID].style.opacity = 0.5f;
                        EasyTalkNodeEditor.Instance.PanZoomPanel.EdgePanningEnabled = true;
                        EasyTalkNodeEditor.Instance.SetCursor(MouseCursor.ArrowPlus);
                    }
                }
            }

            lastMousePos = mousePos;
        }

        public void MoveNode(int nodeId, Vector2 position)
        {
            foreach(ETNode node in nodes)
            {
                if(node.ID == nodeId)
                {
                    node.transform.position = position;
                }
            }
        }

        public void MoveNodes(Dictionary<int, Vector2> positions)
        {
            foreach (ETNode node in nodes)
            {
                if (positions.ContainsKey(node.ID))
                {
                    node.transform.position = positions[node.ID];
                }
            }
        }

        private void CreateConnection(ETInput input, ETOutput output)
        {
            ETNode node = inputToNodeMap[input.ID];
            Node oldNode = node.CreateNode();
            EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeConnectedAction(node, input.ID, output.ID));

            if (input == null || output == null) { return; }

            if (IsConnectionAllowed(input, output))
            {
                input.Connect(output);
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }

        public void CreateConnection(int inputId, int outputId)
        {
            ETInput input = null;
            ETOutput output = null;

            if(inputIdMap.ContainsKey(inputId))
            {
                input = inputIdMap[inputId];
            }

            if(outputIdMap.ContainsKey(outputId))
            {
                output = outputIdMap[outputId];
            }

            CreateConnection(input, output);
        }

        public void DeleteConnection(int inputId, int outputId)
        {
            ETInput input = null;

            if (inputIdMap.ContainsKey(inputId))
            {
                input = inputIdMap[inputId];
                input.Disconnect(outputId);
            }
        }

        public void Reconnect(ETNode node)
        {
            foreach (ETInput input in node.Inputs)
            {
                foreach (int outputId in input.ConnectedOutputs.ToList())
                {
                    CreateConnection(input.ID, outputId);
                }
            }

            foreach (ETOutput output in node.Outputs)
            {
                foreach (int inputId in output.ConnectedInputs.ToList())
                {
                    CreateConnection(output.ID, inputId);
                }
            }
        }

        public void DeleteInputConnections(int inputId)
        {
            if (inputIdMap.ContainsKey(inputId))
            {
                DeleteConnections(inputIdMap[inputId]);
            }
        }

        public void DeleteOutputConnections(int outputId)
        {
            if (outputIdMap.ContainsKey(outputId))
            {
                DeleteConnections(outputIdMap[outputId]);
            }
        }

        public void DeleteConnections(ETInput input)
        {
            if (inputToNodeMap.ContainsKey(input.ID))
            {
                ETNode node = inputToNodeMap[input.ID];
                Node oldNode = node.CreateNode();
                EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(node.ID, oldNode, "Input Node Connections Deleted " + oldNode.ID);

                input.DisconnectAll();

                EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Input Node Connections Deleted " + oldNode.ID);

                MarkDirtyRepaint();
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }

        public void DeleteConnections(ETOutput output)
        {
            if (outputToNodeMap.ContainsKey(output.ID))
            {
                ETNode node = outputToNodeMap[output.ID];
                Node oldNode = node.CreateNode();
                EasyTalkNodeEditor.Instance.Ledger.StartComplexAction(node.ID, oldNode, "Output Node Connections Deleted " + oldNode.ID);

                output.DisconnectAll();

                EasyTalkNodeEditor.Instance.Ledger.EndComplexAction("Output Node Connections Deleted " + oldNode.ID);

                MarkDirtyRepaint();
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }

        public void SelectAllNodes()
        {
            foreach (ETNode node in nodes)
            {
                node.Select();
                node.MarkDirtyRepaint();
            }
        }

        public void DeselectAllNodes()
        {
            foreach (ETNode node in nodes)
            {
                node.Deselect();
                node.MarkDirtyRepaint();
            }
        }

        public void InvertSelection()
        {
            foreach (ETNode node in nodes)
            {
                if (node.IsSelected)
                {
                    node.Deselect();
                }
                else
                {
                    node.Select();
                }

                node.MarkDirtyRepaint();
            }
        }

        public void DeleteNodes(bool onlySelected = false)
        {
            List<Node> deletedNodes = new List<Node>();

            for (int i = 0; i < nodes.Count; i++)
            {
                ETNode node = nodes[i];

                if ((onlySelected && node.IsSelected) || !onlySelected)
                {
                    if(node.IsSelected) { NodeDeselected(node); }

                    deletedNodes.Add(node.CreateNode());
                    UnregisterNode(node);
                    nodes.RemoveAt(i);
                    RemoveNodeFromDisplay(node);
                    i--;
                }
            }

            MarkDirtyRepaint();

            EasyTalkNodeEditor.Instance.NodesChanged();

            EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeDeletedAction(deletedNodes));
        }

        public ETNode FindNode(int nodeId)
        {
            foreach (ETNode node in nodes)
            {
                if (node.ID == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        public void DeleteNode(int nodeId)
        {
            ETNode node = FindNode(nodeId);
            if(node != null)
            {
                DeleteNode(node);
            }
        }

        public void DeleteNode(ETNode node)
        {
            EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeDeletedAction(new List<Node>() { node.CreateNode() }));

            UnregisterNode(node);
            nodes.Remove(node);
            RemoveNodeFromDisplay(node);
        }

        private void RemoveNodeFromDisplay(ETNode node)
        {
            this.Remove(node);
        }

        private void UnregisterNode(ETNode node)
        {
            DeleteConnectionsFromAllInputs(node);
            DeleteConnectionsFromAllOutputs(node);
            UnregisterNodeInputs(node);
            UnregisterNodeOutputs(node);

            if ((node is ETVariableNode) && onVariableRemoved != null)
            {
                onVariableRemoved.Invoke((node as ETVariableNode).VariableName);
            }

            node.onNodeSelected -= NodeSelected;
            node.onNodeDeselected -= NodeDeselected;
        }

        private void DeleteConnectionsFromAllInputs(ETNode node)
        {
            foreach (ETOutput output in node.Outputs)
            {
                DeleteConnectionFromAllInputs(output);
            }
        }

        private void DeleteConnectionsFromAllOutputs(ETNode node)
        {
            foreach (ETInput input in node.Inputs)
            {
                DeleteConnectionFromAllOutputs(input);
            }
        }

        private void UnregisterNodeInputs(ETNode node)
        {
            foreach (ETInput input in node.Inputs)
            {
                UnregisterNodeInput(input);
            }
        }

        private void UnregisterNodeOutputs(ETNode node)
        {
            foreach (ETOutput output in node.Outputs)
            {
                UnregisterNodeOutput(output);
            }
        }

        public void UnregisterNodeOutput(ETOutput output)
        {
            if (outputIdMap.ContainsKey(output.ID))
            {
                outputIdMap.Remove(output.ID);
            }

            if (outputToNodeMap.ContainsKey(output.ID))
            {
                outputToNodeMap.Remove(output.ID);
            }
        }

        public void UnregisterNodeInput(ETInput input)
        {
            if (inputIdMap.ContainsKey(input.ID))
            {
                inputIdMap.Remove(input.ID);
            }

            if (inputToNodeMap.ContainsKey(input.ID))
            {
                inputToNodeMap.Remove(input.ID);
            }
        }

        private void DeleteConnectionFromAllOutputs(ETInput input)
        {
            foreach (int id in input.ConnectedOutputs.ToList())
            {
                if (outputIdMap.ContainsKey(id))
                {
                    outputIdMap[id].Disconnect(input.ID);
                }
            }
        }

        private void DeleteConnectionFromAllInputs(ETOutput output)
        {
            foreach (int id in output.ConnectedInputs.ToList())
            {
                if (inputIdMap.ContainsKey(id))
                {
                    inputIdMap[id].Disconnect(output.ID);
                }
            }
        }

        private bool IsConnectionAllowed(ETInput input, ETOutput output)
        {
            ETNode inputNode = inputToNodeMap[input.ID];
            ETNode outputNode = outputToNodeMap[output.ID];
            if (inputNode == outputNode)
            {
                return false;
            }

            return ETUtils.IsConnectionAllowed(input.InputType, output.OutputType);
        }

        public List<ETVariableNode> GetVariableNodes()
        {
            List<ETVariableNode> variableNodes = new List<ETVariableNode>();
            foreach (ETNode node in nodes)
            {
                if (node is ETVariableNode)
                {
                    variableNodes.Add((ETVariableNode)node);
                }
            }

            return variableNodes;
        }

        public List<string> GetVariableNames()
        {
            List<string> variableNames = new List<string>();

            foreach (ETNode node in nodes)
            {
                if (node is ETVariableNode)
                {
                    string variableName = ((ETVariableNode)node).VariableName;
                    if (!variableNames.Contains(variableName))
                    {
                        variableNames.Add(variableName);
                    }
                }
            }

            if (EasyTalkNodeEditor.Instance.EditorSettings != null)
            {
                DialogueRegistry registry = EasyTalkNodeEditor.Instance.EditorSettings.dialogueRegistry;

                if (registry != null)
                {
                    foreach (GlobalNodeVariable globalVariable in registry.GlobalVariables)
                    {
                        variableNames.Add(globalVariable.VariableName);
                    }
                }
            }

            return variableNames;
        }

        public ETVariableNode GetVariableNode(string variableName)
        {
            foreach (ETNode node in nodes)
            {
                if (node is ETVariableNode)
                {
                    string nodeVariableName = ((ETVariableNode)node).VariableName;
                    if (nodeVariableName.Equals(variableName))
                    {
                        return node as ETVariableNode;
                    }
                }
            }

            return null;
        }

        public List<ETNode> FindAll(string text, bool select = false)
        {
            List<ETNode> matchingNodes = new List<ETNode>();
            foreach (ETNode node in nodes)
            {
                if (node.HasText(text))
                {
                    matchingNodes.Add(node);

                    if (select)
                    {
                        node.Select();
                    }
                }
            }

            if (select)
            {
                MarkDirtyRepaint();
            }

            return matchingNodes;
        }

        public ETInput GetInput(int id)
        {
            if(inputIdMap.ContainsKey(id))
            {
                return inputIdMap[id];
            }

            return null;
        }

        public ETOutput GetOutput(int id)
        {
            if(outputIdMap.ContainsKey(id))
            {
                return outputIdMap[id];
            }

            return null;
        }

        void DrawCanvas(MeshGenerationContext mgc)
        {
            if (creatingConnection)
            {
                DrawLiveConnection(mgc);
            }

            if (selectionMode != SelectionMode.NONE && selectionRect != null)
            {
                DrawSelectionBox(mgc);
            }

            DrawConnections(mgc);
        }

        private void DrawLiveConnection(MeshGenerationContext mgc)
        {
            if (fromInputPort != null)
            {
                Vector3 offset = new Vector2(fromInputPort.contentRect.width / 2.0f, fromInputPort.contentRect.height / 2.0f);
                Vector2 inputCoord = fromInputPort.ChangeCoordinatesTo(this, fromInputPort.transform.position + offset);
                ETNode inputNode = inputToNodeMap[fromInputPort.ID];
                DrawConnection(mgc, inputCoord, inputNode.ConnectionLineColor, lastMousePos, Color.white);
            }
            else if (fromOutputPort != null)
            {
                Vector3 offset = new Vector2(fromOutputPort.contentRect.width / 2.0f, fromOutputPort.contentRect.height / 2.0f);
                Vector2 outputCoord = fromOutputPort.ChangeCoordinatesTo(this, fromOutputPort.transform.position + offset);
                ETNode outputNode = outputToNodeMap[fromOutputPort.ID];
                DrawConnection(mgc, lastMousePos, Color.white, outputCoord, outputNode.ConnectionLineColor);
            }
        }

        private void DrawSelectionBox(MeshGenerationContext mgc)
        {
            if (selectionMode == SelectionMode.SELECT)
            {
                DrawRectangle(mgc, selectionRect, new Color(0.0f, 1.0f, 0.0f, 1.0f));
            }
            else
            {
                DrawRectangle(mgc, selectionRect, new Color(1.0f, 0.0f, 0.0f, 1.0f));
            }
        }

        private void DrawRectangle(MeshGenerationContext mgc, Rect rectangle, Color color)
        {
            Painter2D painter2D = mgc.painter2D;
            painter2D.lineWidth = 4.0f / (this.parent as ETPanZoomPanel).Zoom;
            painter2D.lineJoin = LineJoin.Round;
            painter2D.strokeColor = color;

            painter2D.BeginPath();
            painter2D.MoveTo(new Vector2(rectangle.x, rectangle.y));
            painter2D.LineTo(new Vector2(rectangle.x + rectangle.width, rectangle.y));
            painter2D.LineTo(new Vector2(rectangle.x + rectangle.width, rectangle.y + rectangle.height));
            painter2D.LineTo(new Vector2(rectangle.x, rectangle.y + rectangle.height));
            painter2D.LineTo(new Vector2(rectangle.x, rectangle.y));
            painter2D.Stroke();
            painter2D.ClosePath();
        }

        void DrawConnections(MeshGenerationContext mgc)
        {
            foreach (ETNode node in nodes)
            {
                foreach (ETOutput output in node.Outputs)
                {
                    Vector3 outputOffset = new Vector2(output.contentRect.width / 2.0f, output.contentRect.height / 2.0f);
                    Vector2 outputPoint = output.ChangeCoordinatesTo(this, output.transform.position + outputOffset);

                    //Draw each connection
                    foreach (int inputConnectionId in output.ConnectedInputs)
                    {
                        if (inputIdMap.ContainsKey(inputConnectionId))
                        {
                            ETInput input = inputIdMap[inputConnectionId];
                            Vector3 inputOffset = new Vector2(input.contentRect.width / 2.0f, input.contentRect.height / 2.0f);
                            Vector2 inputPoint = input.ChangeCoordinatesTo(this, input.transform.position + inputOffset);
                            ETNode inputNode = inputToNodeMap[inputConnectionId];
                            try
                            {
                                DrawConnection(mgc, inputPoint, inputNode.ConnectionLineColor, outputPoint, node.ConnectionLineColor);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        void DrawConnection(MeshGenerationContext mgc, Vector2 inputPoint, Color inputColor, Vector2 outputPoint, Color outputColor)
        {
            Color wireColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
            Painter2D painter2D = mgc.painter2D;

            float distance = (inputPoint - outputPoint).magnitude;
            float offset = Mathf.Min(360.0f * (distance / 1500.0f), 1000.0f);
            Vector2 start = outputPoint;
            Vector2 end = inputPoint;
            Vector2 middle = (outputPoint + inputPoint) / 2.0f;
            Vector2 bezierPointA = start + (Vector2.right * offset);
            Vector2 bezierPointB = end - (Vector2.right * offset);
            float distanceOffset = 100.0f;

            if (inputPoint.x < outputPoint.x)
            {
                distanceOffset = Mathf.Lerp(0.0f, 100.0f, 1.0f - Mathf.Clamp(((inputPoint.x - outputPoint.x) / -100.0f), 0.0f, 1.0f));
            }

            bezierPointA = Vector2.Lerp(start, bezierPointA, (distance - distanceOffset) / 50.0f);
            bezierPointB = Vector2.Lerp(end, bezierPointB, (distance - distanceOffset) / 50.0f);

            Color midColor = Color.Lerp(inputColor, outputColor, 0.5f);

            Gradient strokeGradientA = new Gradient();
            strokeGradientA.SetKeys(
                new GradientColorKey[] { new GradientColorKey(outputColor, 0.0f), new GradientColorKey(midColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });

            Gradient strokeGradientB = new Gradient();
            strokeGradientB.SetKeys(
                new GradientColorKey[] { new GradientColorKey(midColor, 0.0f), new GradientColorKey(inputColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });

            painter2D.lineWidth = 5.0f + (HighlightWidth * 0.25f);
            painter2D.lineJoin = LineJoin.Round;

            painter2D.BeginPath();
            painter2D.strokeGradient = strokeGradientA;
            painter2D.MoveTo(start);
            painter2D.BezierCurveTo(start, bezierPointA, middle);
            painter2D.Stroke();
            painter2D.ClosePath();

            painter2D.BeginPath();
            painter2D.strokeGradient = strokeGradientB;
            painter2D.MoveTo(middle);
            painter2D.BezierCurveTo(middle, bezierPointB, end);
            painter2D.Stroke();
            painter2D.ClosePath();
        }

        private void StartPerformanceFilterThread()
        {
            if(performanceFilterThread != null && performanceFilterThread.IsAlive)
            {
                performanceFilterThread.Interrupt();
                performanceFilterThread.Abort();
                performanceFilterThread = null;
            }

            performanceFilterThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(200);
                        if (filterCounter > 0 && performanceFilterActive)
                        {
                            EditorApplication.delayCall += FilterDisplay;
                        }

                    }
                } catch { }
            });

            performanceFilterThread.Name = "Easy Talk Performance Filter Thread";
            performanceFilterThread.Start();
        }

        private void FilterDisplay()
        {
            //If zoomed in enough, unload (remove non-port elements) from all nodes outside the view and reload any that are now 
            //in the view that weren't previously and were unloaded.
            if ((this.parent as ETPanZoomPanel).Zoom > 0.25f)
            {
                foreach (ETNode node in nodes)
                {
                    if (this.parent.worldBound.Overlaps(node.worldBound))
                    {
                        node.Load();
                    }
                    else
                    {
                        //nodesAreUnloaded = true;
                        node.Unload();
                    }
                }
            }
            //Unload all nodes since they can't be seen or modified easily at the current zoom level anyways.
            else
            {
                foreach (ETNode node in nodes)
                {
                    node.Unload();
                }
            }

            filterCounter = 0;
            MarkDirtyRepaint();
        }

        public List<ETNode> GetNodes() { return nodes; }

        public void AddCharacter(string characterName)
        {
            if (!this.characters.Contains(characterName))
            {
                characters.Add(characterName);
            }

            foreach (ETNode node in nodes)
            {
                if (node is ETConversationNode)
                {
                    (node as ETConversationNode).SetCharacterNames(characters);
                }
            }
        }

        public List<string> GetCharacterList()
        {
            return this.characters;
        }

        public void AddConvoID(string convoID)
        {
            if (!this.convoIds.Contains(convoID))
            {
                convoIds.Add(convoID);
            }

            foreach(ETNode node in nodes)
            {
                if(node is ETConversationSpecificNode)
                {
                    (node as ETConversationSpecificNode).SetConvoIDs(convoIds);
                }
            }
        }

        public List<string> GetConvoIDs()
        {
            return this.convoIds;
        }

        private enum SelectionMode { NONE, SELECT, DESELECT }

        public float HighlightWidth
        {
            get { return this.highlightWidth; }
        }

        public bool PerformanceFilterActive
        {
            get { return this.performanceFilterActive; }
            set { this.performanceFilterActive = value; }
        }
    }
}