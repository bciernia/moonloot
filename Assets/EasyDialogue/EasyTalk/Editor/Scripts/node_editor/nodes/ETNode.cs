using EasyTalk.Editor.Components;
using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Localization;
using EasyTalk.Nodes.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public abstract class ETNode : Box
    {
        protected int id = NodeUtils.NextID();

        protected List<ETNodeContent> contents = new List<ETNodeContent>();

        protected float startWidth = 400.0f;

        protected float startHeight = 200.0f;

        private bool selected = false;

        protected Label titleLabel;

        protected VisualElement leftResizeHandle;
        protected VisualElement rightResizeHandle;
        protected VisualElement topResizeHandle;
        protected VisualElement bottomResizeHandle;

        private Vector2 lastMousePos;
        private ResizeMode resizeMode = ResizeMode.NONE;

        protected Box connectionLine;

        protected ResizeType resizeType = ResizeType.BOTH;

        protected string title;

        private Dictionary<VisualElement, List<VEInfo>> veInfo = new Dictionary<VisualElement, List<VEInfo>>();

        private Vector2 oldSize = Vector2.zero;
        private Vector2 oldPosition = Vector2.zero;

        public delegate void OnNodeSelected(ETNode node);
        public OnNodeSelected onNodeSelected;

        public delegate void OnNodeDeselected(ETNode node);
        public OnNodeDeselected onNodeDeselected;

        protected bool supportsSettings = false;

        public ETNode(string title) : base()
        {
            this.title = title;
            this.usageHints = UsageHints.DynamicTransform;

            Initialize();
            SetupNode();
            CreateContent();
        }

        public ETNode(string title, string nodeClass) : base()
        {
            this.title = title;
            this.AddToClassList(nodeClass);

            Initialize();
            SetupNode();
            CreateContent();
        }

        public ETNode(string title, string nodeClass, bool createContent = true) : base()
        {
            this.title = title;
            this.AddToClassList(nodeClass);

            Initialize();
            SetupNode();

            if (createContent)
            {
                CreateContent();
            }
        }
        
        protected virtual void Initialize()
        {
            this.AddToClassList("node");
            
            connectionLine = new Box();
            connectionLine.AddToClassList("node-line");
            connectionLine.pickingMode = PickingMode.Ignore;

            this.style.width = startWidth;
            this.style.height = startHeight;
            this.pickingMode = PickingMode.Position;

            this.generateVisualContent += DrawHighlight;

            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.RegisterCallback<AttachToPanelEvent>(OnAddedToParent);
            this.RegisterCallback<MouseUpEvent>(OnMouseUp);

            this.Add(connectionLine);

            if (supportsSettings)
            {
                CreateSettingsButton();
            }
        }

        private void CreateSettingsButton()
        {
            ETNodeButton settingsButton = new ETNodeButton();
            settingsButton.ClearClassList();
            settingsButton.AddToClassList("settings-button");
            settingsButton.onButtonClicked += delegate
            {
                EasyTalkNodeEditor.Instance.SettingsPanel.SetActiveNode(this);
                EasyTalkNodeEditor.Instance.SettingsPanel.Show();
            };

            this.Add(settingsButton);
        }

        private void SetupNode()
        {
            this.tooltip = GetNodeTooltip();
            CreateTitleLabel();
            CreateResizeHandles();

            if (resizeType == ResizeType.BOTH || resizeType == ResizeType.VERTICAL)
            {
                topResizeHandle.BringToFront();
                bottomResizeHandle.BringToFront();
            }

            if (resizeType == ResizeType.BOTH || resizeType == ResizeType.HORIZONTAL)
            {
                leftResizeHandle.BringToFront();
                rightResizeHandle.BringToFront();
            }
        }

        public int ID { get { return id; } }

        protected virtual void AddContent(ETNodeContent content)
        {
            contents.Add(content);
            this.Add(content);
        }

        protected virtual void AddContent(ETNodeContent content, int idx)
        {
            contents.Insert(idx, content);
            this.Insert(idx, content);
        }

        protected virtual void RemoveContent(ETNodeContent content)
        {
            contents.Remove(content);
            this.Remove(content);
        }

        protected virtual void RemoveContent(int idx)
        {
            contents.RemoveAt(idx);
            this.RemoveAt(idx);
        }

        public virtual void CreateContent() { }

        public void SetDimensions(float width, float height)
        {
            this.style.width = width;
            this.style.height = height;
            this.startWidth = width;
            this.startHeight = height;
        }

        private void OnAddedToParent(AttachToPanelEvent evt)
        {
            if (this.parent != null)
            {
                EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }
        }

        public Color ConnectionLineColor
        {
            get { return connectionLine.resolvedStyle.color; }
        }

        public void SetTitle(string title)
        {
            titleLabel.text = title;
        }

        private void CreateTitleLabel()
        {
            titleLabel = new Label(title);
            titleLabel.ClearClassList();
            titleLabel.AddToClassList("node-label");
            titleLabel.pickingMode = PickingMode.Ignore;
            this.Add(titleLabel);
        }

        protected abstract string GetNodeTooltip();

        private void CreateResizeHandles()
        {
            if (resizeType == ResizeType.BOTH || resizeType == ResizeType.VERTICAL)
            {
                topResizeHandle = new Box();
                topResizeHandle.name = "Top Resize Handle";
                topResizeHandle.pickingMode = PickingMode.Position;
                topResizeHandle.AddToClassList("vertical-resize-handle");
                topResizeHandle.AddToClassList("top-resize-handle");
                this.Add(topResizeHandle);

                bottomResizeHandle = new Box();
                bottomResizeHandle.name = "Bottom Resize Handle";
                bottomResizeHandle.pickingMode = PickingMode.Position;
                bottomResizeHandle.AddToClassList("vertical-resize-handle");
                bottomResizeHandle.AddToClassList("bottom-resize-handle");
                this.Add(bottomResizeHandle);
            }

            if (resizeType == ResizeType.BOTH || resizeType == ResizeType.HORIZONTAL)
            {
                leftResizeHandle = new Box();
                leftResizeHandle.name = "Left Resize Handle";
                leftResizeHandle.pickingMode = PickingMode.Position;
                leftResizeHandle.AddToClassList("horizontal-resize-handle");
                leftResizeHandle.AddToClassList("left-resize-handle");
                this.Add(leftResizeHandle);

                rightResizeHandle = new Box();
                rightResizeHandle.name = "Right Resize Handle";
                rightResizeHandle.pickingMode = PickingMode.Position;
                rightResizeHandle.AddToClassList("horizontal-resize-handle");
                rightResizeHandle.AddToClassList("right-resize-handle");
                this.Add(rightResizeHandle);
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            Vector3 mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.NodeView, evt.localMousePosition);

            if (resizeMode != ResizeMode.NONE)
            {
                Vector2 change = new Vector2(mousePos.x - lastMousePos.x, mousePos.y - lastMousePos.y);

                if (resizeMode == ResizeMode.TOP)
                {
                    float currentHeight = this.style.height.value.value;
                    float newHeight = currentHeight - change.y;
                    if (newHeight >= GetMinimumHeight() || (currentHeight < GetMinimumHeight() && newHeight > currentHeight))
                    {
                        this.style.height = new StyleLength(newHeight);
                        this.transform.position = this.transform.position + (Vector3.up * change.y);
                        EasyTalkNodeEditor.Instance.NodesChanged();
                    }
                }
                else if (resizeMode == ResizeMode.BOTTOM)
                {
                    float currentHeight = this.style.height.value.value;
                    float newHeight = currentHeight + change.y;
                    if (newHeight >= GetMinimumHeight() || (currentHeight < GetMinimumHeight() && newHeight > currentHeight))
                    {
                        this.style.height = new StyleLength(newHeight);
                        EasyTalkNodeEditor.Instance.NodesChanged();
                    }
                }
                else if (resizeMode == ResizeMode.LEFT)
                {
                    float currentWidth = this.style.width.value.value;
                    float newWidth = currentWidth - change.x;
                    if (newWidth >= GetMinimumWidth() || (currentWidth < GetMinimumWidth() && newWidth > currentWidth))
                    {
                        this.style.width = new StyleLength(newWidth);
                        this.transform.position = this.transform.position + (Vector3.right * change.x);
                        EasyTalkNodeEditor.Instance.NodesChanged();
                    }
                }
                else if (resizeMode == ResizeMode.RIGHT)
                {
                    float currentWidth = this.style.width.value.value;
                    float newWidth = currentWidth + change.x;
                    if (newWidth >= GetMinimumWidth() || (currentWidth < GetMinimumWidth() && newWidth > currentWidth))
                    {
                        this.style.width = new StyleLength(newWidth);
                        EasyTalkNodeEditor.Instance.NodesChanged();
                    }
                }

                EasyTalkNodeEditor.Instance.NodeView.MarkDirtyRepaint();
            }

            lastMousePos = mousePos;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            Vector3 mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.NodeView, evt.localMousePosition);

            if (evt.button == 0)
            {
                VisualElement currentElement = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

                if (currentElement == topResizeHandle)
                {
                    resizeMode = ResizeMode.TOP;
                }
                else if (currentElement == bottomResizeHandle)
                {
                    resizeMode = ResizeMode.BOTTOM;
                }
                else if (currentElement == leftResizeHandle)
                {
                    resizeMode = ResizeMode.LEFT;
                }
                else if (currentElement == rightResizeHandle)
                {
                    resizeMode = ResizeMode.RIGHT;
                }

                oldPosition = this.transform.position;
                oldSize = new Vector2(this.style.width.value.value, this.style.height.value.value);
            }

            lastMousePos = mousePos;
        }

        public void OnMouseUp(MouseUpEvent evt)
        {
            if (resizeMode != ResizeMode.NONE) 
            { 
                resizeMode = ResizeMode.NONE;

                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeResizedAction(this, oldSize, oldPosition));
                oldPosition = this.transform.position;
                oldSize = new Vector2(this.style.width.value.value, this.style.height.value.value);
            }
        }

        public bool IsSelected
        {
            get { return this.selected; }
        }

        public void Select()
        {
            if (!selected)
            {
                selected = true;
                BringToFront();
                MarkDirtyRepaint();

                if(onNodeSelected != null)
                {
                    onNodeSelected(this);
                }
            }
        }

        public void Deselect()
        {
            if (selected)
            {
                selected = false;
                MarkDirtyRepaint();

                if(onNodeDeselected != null)
                {
                    onNodeDeselected(this);
                }
            }
        }

        public virtual List<ETInput> Inputs
        {
            get
            {
                List<ETInput> nodeInputs = new List<ETInput>();
                foreach (ETNodeContent content in contents)
                {
                    nodeInputs.AddRange(content.GetInputs());
                }

                return nodeInputs;
            }
        }

        public virtual List<ETOutput> Outputs
        {
            get
            {
                List<ETOutput> nodeOutputs = new List<ETOutput>();
                foreach (ETNodeContent content in contents)
                {
                    nodeOutputs.AddRange(content.GetOutputs());
                }

                return nodeOutputs;
            }
        }

        public List<ETNodeContent> Contents
        {
            get { return contents; }
            set { contents = value; }
        }

        public Vector2 GetTopLeft()
        {
            return new Vector2(0.0f, 0.0f);
        }

        public Vector2 GetTopRight()
        {
            return new Vector2(this.contentRect.width, 0.0f);
        }

        public Vector2 GetBottomLeft()
        {
            return new Vector2(0.0f, this.contentRect.height + 6.0f);
        }

        public Vector2 GetBottomRight()
        {
            return new Vector2(this.contentRect.width, this.contentRect.height + 6.0f);
        }

        public virtual float GetMinimumWidth()
        {
            return this.startWidth;
        }

        public virtual float GetMinimumHeight()
        {
            return this.startHeight;
        }

        public Vector2 GetCenter()
        {
            return this.transform.position + new Vector3(this.style.width.value.value / 2.0f, this.style.height.value.value / 2.0f, 0.0f);
        }

        public float GetWidth()
        {
            return this.style.width.value.value;
        }

        public float GetHeight()
        {
            return this.style.height.value.value;
        }

        public abstract Node CreateNode();

        public abstract void InitializeFromNode(Node node);

        public void SetSizeFromNode(Node node)
        {
            this.style.width = node.Width;
            this.style.height = node.Height;
        }

        public void SetPositionFromNode(Node node)
        {
            this.transform.position = new Vector3(node.XPosition, node.YPosition);
        }

        public void InitializeAllInputsAndOutputsFromNode(Node node)
        {
            InitializeAllInputsFromNode(node);
            InitializeAllOutputsFromNode(node);
        }

        public void InitializeAllInputsFromNode(Node node)
        {
            for (int i = 0; i < node.Inputs.Count; i++)
            {
                ETInput thisInput = this.Inputs[i];
                NodeConnection conn = node.Inputs[i];
                thisInput.ID = conn.ID;

                foreach (int outputId in conn.AttachedIDs)
                {
                    thisInput.Connect(outputId);
                }
            }
        }

        public void InitializeAllOutputsFromNode(Node node)
        {
            for (int i = 0; i < node.Outputs.Count; i++)
            {
                ETOutput thisOutput = this.Outputs[i];
                NodeConnection conn = node.Outputs[i];
                thisOutput.ID = conn.ID;

                foreach (int inputId in conn.AttachedIDs)
                {
                    thisOutput.Connect(inputId);
                }
            }
        }

        public void InitializeInputFromConnection(ETInput input, NodeConnection connection)
        {
            input.ID = connection.ID;

            if (connection.AttachedIDs.Count > 0)
            {
                foreach (int connId in connection.AttachedIDs)
                {
                    input.Connect(connId);
                }
            }
        }

        public void InitializeOutputFromConnection(ETOutput output, NodeConnection connection)
        {
            output.ID = connection.ID;

            if (connection.AttachedIDs.Count > 0)
            {
                foreach (int connId in connection.AttachedIDs)
                {
                    output.Connect(connId);
                }
            }
        }

        public void CreateInputsForNode(Node node)
        {
            foreach (ETInput input in this.Inputs)
            {
                NodeConnection inputConnection = new NodeConnection(node.ID, input.InputType);
                inputConnection.ID = input.ID;

                inputConnection.AttachedIDs = input.ConnectedOutputs.ToList();
                node.AddInput(inputConnection);
            }
        }

        public void CreateOutputsForNode(Node node)
        {
            foreach (ETOutput output in this.Outputs)
            {
                NodeConnection outputConnection = new NodeConnection(node.ID, output.OutputType);
                outputConnection.ID = output.ID;
                outputConnection.AttachedIDs = output.ConnectedInputs.ToList();
                node.AddOutput(outputConnection);
            }
        }

        public void SetSizeForNode(Node node)
        {
            node.Width = this.GetWidth();
            node.Height = this.GetHeight();
        }

        public void SetPositionForNode(Node node)
        {
            //Vector2 center = this.GetCenter();
            node.XPosition = this.transform.position.x;
            node.YPosition = this.transform.position.y;
        }

        void DrawHighlight(MeshGenerationContext mgc)
        {
            if (selected)
            {
                Painter2D painter2D = mgc.painter2D;

                Vector2 pointA = GetTopLeft();
                Vector2 pointB = GetTopRight();
                Vector2 pointC = GetBottomRight();
                Vector2 pointD = GetBottomLeft();
                Vector2 pointE = GetTopLeft();

                Gradient strokeGradientA = new Gradient();
                strokeGradientA.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(connectionLine.resolvedStyle.color, 0.0f), new GradientColorKey(connectionLine.resolvedStyle.color, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });

                painter2D.lineWidth = Mathf.Max(EasyTalkNodeEditor.Instance.NodeView.HighlightWidth, 4.0f);
                painter2D.lineJoin = LineJoin.Round;

                painter2D.BeginPath();
                painter2D.strokeGradient = strokeGradientA;
                painter2D.MoveTo(pointA);
                painter2D.LineTo(pointB);
                painter2D.LineTo(pointC);
                painter2D.LineTo(pointD);
                painter2D.LineTo(pointE);
                painter2D.Stroke();
                painter2D.ClosePath();
            }
        }

        public enum ResizeType { NONE, BOTH, HORIZONTAL, VERTICAL }

        private enum ResizeMode { NONE, TOP, BOTTOM, LEFT, RIGHT };

        public virtual void NodeResized() { }

        public virtual void NodeMoved() { }

        public virtual bool HasText(string text)
        {
            try { return this.title.ToLower().Contains(text.ToLower()); } catch { }

            return false;
        }

        public void HideContent()
        {
            foreach(ETNodeContent content in contents)
            {
                content.visible = false;
            }
        }

        public void ShowContent()
        {
            foreach (ETNodeContent content in contents)
            {
                content.visible = true;
            }
        }

        public bool IsUnloaded()
        {
            return veInfo.Count > 0;
        }

        public void Unload()
        {
            //Don't unload if already unloaded.
            if(IsUnloaded()) { return; }

            Unload(this);

            foreach(List<VEInfo> infos in veInfo.Values)
            {
                foreach(VEInfo info in infos)
                {
                    VisualElement parent = info.element.parent;
                    parent.Remove(info.element);
                }
            }
        }

        private void Unload(VisualElement parent)
        {
            foreach (VisualElement child in parent.Children())
            {
                if ((child is ETTextField) || (child is Label) || (child is EnumField) || (child is TextField) ||
                    (child is ETNodeButton) || (child is ETAudioClipZone) || (child is ETEditableDropdown) || 
                    (child is DropdownField) || (child is Toggle) || (child is Foldout))
                {
                    List<VEInfo> info;
                    if (veInfo.ContainsKey(parent))
                    {
                        info = veInfo[parent];
                    }
                    else
                    {
                        info = new List<VEInfo>();
                        veInfo.Add(parent, info);
                    }

                    VEInfo childInfo = new VEInfo();
                    childInfo.element = child;
                    childInfo.index = parent.IndexOf(child);

                    info.Add(childInfo);
                }
                else
                {
                    Unload(child);
                }
            }
        }

        public void Load()
        {
            //No need to do anything if the node is loaded.
            if(!IsUnloaded()) { return; }

            foreach(VisualElement parent in veInfo.Keys)
            {
                foreach(VEInfo childInfo in veInfo[parent])
                {
                    try
                    {
                        if (childInfo.index > parent.childCount)
                        {
                            parent.Add(childInfo.element);
                        }
                        else
                        {
                            parent.Insert(childInfo.index, childInfo.element);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            veInfo.Clear();
        }

        public virtual void CreateLocalizations(TranslationLibrary library) { }

        public class VEInfo
        {
            public VisualElement element;
            public int index;
        }

        public virtual void BuildSettingsPanel(ETSettingsPanel settingsPanel) 
        {
            settingsPanel.ClearContentPanel();
        }

        public bool SupportsSettings { get { return supportsSettings; } }
    }
}