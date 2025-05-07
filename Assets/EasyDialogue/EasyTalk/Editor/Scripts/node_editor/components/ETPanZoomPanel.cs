using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETPanZoomPanel : Box
    {
        private VisualElement content;

        private Texture2D bgGrid;

        private bool isPanning = false;

        private Vector3 panAmount = Vector3.zero;

        private float zoomLevel = 1.0f;

        private Vector3 lastMousePos;

        private Image gridImage;

        private float minZoom = 0.05f;
        private float maxZoom = 50.0f;

        private bool isEdgePanningEnabled = false;
        private bool isEdgePanning = false;
        private Box topPanBox;
        private Box bottomPanBox;
        private Box leftPanBox;
        private Box rightPanBox;

        public delegate void OnZoomIn();
        public OnZoomIn onZoomIn;
        public delegate void OnZoomOut();
        public OnZoomOut onZoomOut;

        public delegate void OnPan();
        public OnPan onPan;
        public delegate void OnAutoPanStart(Vector3 mousePos);
        public OnAutoPanStart onAutoPanStart;
        public delegate void OnAutoPan(Vector3 mousePos);
        public OnAutoPan onAutoPan;

        private Vector3 autoPanPos;

        public ETPanZoomPanel(VisualElement content)
        {
            this.content = content;

            this.pickingMode = PickingMode.Position;
            this.style.width = new StyleLength(new Length(100.0f, LengthUnit.Percent));
            this.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
            this.focusable = true;

            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            this.RegisterCallback<MouseUpEvent>(OnMouseUp);
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.RegisterCallback<WheelEvent>(OnWheel);
            this.RegisterCallback<KeyDownEvent>(OnKeyDown);

            AddGrid();

            this.Add(content);

            CreateEdgePanningBoxes();
        }

        private void AddGrid()
        {
            bgGrid = Resources.Load<Texture2D>("images/grids/grid6");
            gridImage = new Image();
            gridImage.AddToClassList("background-grid");
            gridImage.scaleMode = ScaleMode.StretchToFill;
            gridImage.pickingMode = PickingMode.Ignore;
            gridImage.image = bgGrid;
            float widthScale = this.contentRect.width / this.contentRect.height;
            float heightScale = this.contentRect.height / this.contentRect.width;
            gridImage.uv = new Rect(0.0f, 0.0f, widthScale, heightScale);
            this.Add(gridImage);
        }

        private void CreateEdgePanningBoxes()
        {
            topPanBox = new Box();
            topPanBox.name = "top-edge-panning-box";
            topPanBox.RegisterCallback<MouseEnterEvent>(OnPanBoxMouseEnter);
            topPanBox.RegisterCallback<MouseLeaveEvent>(OnPanBoxMouseExit);
            topPanBox.RegisterCallback<MouseMoveEvent>(AutoPanPositionChanged);

            bottomPanBox = new Box();
            bottomPanBox.name = "bottom-edge-panning-box";
            bottomPanBox.RegisterCallback<MouseEnterEvent>(OnPanBoxMouseEnter);
            bottomPanBox.RegisterCallback<MouseLeaveEvent>(OnPanBoxMouseExit);
            bottomPanBox.RegisterCallback<MouseMoveEvent>(AutoPanPositionChanged);

            leftPanBox = new Box();
            leftPanBox.name = "left-edge-panning-box";
            leftPanBox.RegisterCallback<MouseEnterEvent>(OnPanBoxMouseEnter);
            leftPanBox.RegisterCallback<MouseLeaveEvent>(OnPanBoxMouseExit);
            leftPanBox.RegisterCallback<MouseMoveEvent>(AutoPanPositionChanged);

            rightPanBox = new Box();
            rightPanBox.name = "right-edge-panning-box";
            rightPanBox.RegisterCallback<MouseEnterEvent>(OnPanBoxMouseEnter);
            rightPanBox.RegisterCallback<MouseLeaveEvent>(OnPanBoxMouseExit);
            rightPanBox.RegisterCallback<MouseMoveEvent>(AutoPanPositionChanged);

            this.Add(topPanBox);
            this.Add(bottomPanBox);
            this.Add(leftPanBox);
            this.Add(rightPanBox);
        }

        public void OnPanBoxMouseEnter(MouseEnterEvent evt)
        {
            autoPanPos = (evt.target as VisualElement).ChangeCoordinatesTo(this, evt.localMousePosition);
            isEdgePanning = true;
            if (evt.target == topPanBox)
            {
                panDirection = new Vector3(0.0f, 1.0f * zoomLevel, 0.0f);
                //panDirection = new Vector3(0.0f, 1.0f, 0.0f);
            }
            else if (evt.target == bottomPanBox)
            {
                panDirection = new Vector3(0.0f, -1.0f * zoomLevel, 0.0f);
                //panDirection = new Vector3(0.0f, -1.0f, 0.0f);
            }
            else if (evt.target == leftPanBox)
            {
                panDirection = new Vector3(1.0f * zoomLevel, 0.0f, 0.0f);
                //panDirection = new Vector3(1.0f, 0.0f, 0.0f);
            }
            else if (evt.target == rightPanBox)
            {
                panDirection = new Vector3(-1.0f * zoomLevel, 0.0f, 0.0f);
                //panDirection = new Vector3(-1.0f, 0.0f, 0.0f);
            }

            if(onAutoPanStart != null) { onAutoPanStart(autoPanPos); }
        }

        private void AutoPanPositionChanged(MouseMoveEvent evt)
        {
            autoPanPos = (evt.target as VisualElement).ChangeCoordinatesTo(this, evt.localMousePosition);
        }

        private void OnPanBoxMouseExit(MouseLeaveEvent evt)
        {
            isEdgePanning = false;
        }

        private Vector3 panDirection = Vector3.zero;

        public void Update()
        {
            if (isEdgePanningEnabled && isEdgePanning)
            {
                if (onAutoPan != null) { onAutoPan(autoPanPos); }
                Pan(panDirection);
            }
        }

        public Vector2 PanAmount
        {
            get { return panAmount; }
            set { panAmount = value; }
        }

        public bool EdgePanningEnabled
        {
            set
            {
                this.isEdgePanningEnabled = value;

                if (this.isEdgePanningEnabled)
                {
                    topPanBox.style.display = DisplayStyle.Flex;
                    bottomPanBox.style.display = DisplayStyle.Flex;
                    leftPanBox.style.display = DisplayStyle.Flex;
                    rightPanBox.style.display = DisplayStyle.Flex;
                }
                else if (!this.isEdgePanningEnabled)
                {
                    topPanBox.style.display = DisplayStyle.None;
                    bottomPanBox.style.display = DisplayStyle.None;
                    leftPanBox.style.display = DisplayStyle.None;
                    rightPanBox.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //MarkDirtyRepaint();
        }

        public void RepaintBackgroundGrid()
        {
            float widthScale = this.contentRect.width / 256.0f;
            float heightScale = this.contentRect.height / 256.0f;
            float xOffset = -(panAmount.x / 256.0f);
            float yOffset = (panAmount.y / 256.0f);

            gridImage.uv = new Rect(
                0.0f + xOffset,
                0.0f + yOffset,
                widthScale * (1.0f / zoomLevel),
                heightScale * (1.0f / zoomLevel));

            gridImage.MarkDirtyRepaint();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey)
            {
                if (evt.keyCode == KeyCode.Plus || evt.keyCode == KeyCode.KeypadPlus || evt.keyCode == KeyCode.Equals)
                {
                    ZoomIn();
                }
                else if (evt.keyCode == KeyCode.Minus || evt.keyCode == KeyCode.KeypadMinus || evt.keyCode == KeyCode.Underscore)
                {
                    ZoomOut();
                }
            }
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            Vector3 mousePos = evt.localMousePosition;

            if (isPanning)
            {
                Vector3 deltaPos = mousePos - lastMousePos;
                Pan(deltaPos);

                lastMousePos = mousePos;
            }
        }

        private void Pan(Vector3 amount)
        {
            content.transform.position = content.transform.position + amount;
            panAmount += amount * (1.0f / zoomLevel);
            RepaintBackgroundGrid();

            if(onPan != null)
            {
                onPan();
            }
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            isPanning = false;
            EasyTalkNodeEditor.Instance.SetCursor(MouseCursor.Arrow);
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            Vector3 mousePos = evt.localMousePosition;

            if (evt.button == 2 || (evt.button == 0 && evt.ctrlKey))
            {
                EasyTalkNodeEditor.Instance.SetCursor(MouseCursor.Pan);
                isPanning = true;
                lastMousePos = mousePos;
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            Vector2 mousePos = evt.localMousePosition;

            //Calculate the relative position (as a percentage) of the mouse within the content.
            Vector2 contentMousePos = this.ChangeCoordinatesTo(content, mousePos);
            float mouseXPercentOffset = (contentMousePos.x / content.contentRect.width);
            float mouseYPercentOffset = (contentMousePos.y / content.contentRect.height);

            //Zoom in or out
            if (evt.delta.y < 0.0f)
            {
                //Zooming in
                ZoomIn();
            }
            else if (evt.delta.y > 0.0f)
            {
                //Zooming out
                ZoomOut();
            }

            //Find offset to zero after scale (size difference / 2)
            float xOffset = (content.contentRect.width - content.worldBound.width) / 2.0f;
            float yOffset = (content.contentRect.height - content.worldBound.height) / 2.0f;
            Vector2 offset = new Vector2(xOffset, yOffset);

            //Calculate the new mouse offset after scaling (by position)
            Vector2 mouseOffset = new Vector2(mouseXPercentOffset * content.worldBound.width, mouseYPercentOffset * content.worldBound.height);

            //Offset the content to the current mouse position, then offset by the mouse offset percentage, then offset depending on the current scale.
            content.transform.position = mousePos - mouseOffset - offset;
        }

        public void ZoomIn()
        {
            zoomLevel *= 1.25f;
            zoomLevel = Mathf.Min(zoomLevel, maxZoom);
            //content.transform.scale = new Vector3(zoomLevel, zoomLevel, zoomLevel);

            Vector2 center = GetCenter();
            Vector2 contentCenterPos = this.ChangeCoordinatesTo(content, center);
            float xPercentOffset = (contentCenterPos.x / content.contentRect.width);
            float yPercentOffset = (contentCenterPos.y / content.contentRect.height);

            content.transform.scale = new Vector3(zoomLevel, zoomLevel, zoomLevel);

            Vector2 contentPositionOffset = new Vector2(xPercentOffset * content.worldBound.width, yPercentOffset * content.worldBound.height);
            float xOffset = (content.contentRect.width - content.worldBound.width) / 2.0f;
            float yOffset = (content.contentRect.height - content.worldBound.height) / 2.0f;
            Vector2 offset = new Vector2(xOffset, yOffset);
            content.transform.position = center - contentPositionOffset - offset;

            if(onZoomIn != null) { onZoomIn.Invoke(); }

            RepaintBackgroundGrid();
        }

        public void ZoomOut()
        {
            zoomLevel *= 0.8f;
            zoomLevel = Mathf.Max(zoomLevel, minZoom);
            //content.transform.scale = new Vector3(zoomLevel, zoomLevel, zoomLevel);

            Vector2 center = GetCenter();
            Vector2 contentCenterPos = this.ChangeCoordinatesTo(content, center);
            float xPercentOffset = (contentCenterPos.x / content.contentRect.width);
            float yPercentOffset = (contentCenterPos.y / content.contentRect.height);

            content.transform.scale = new Vector3(zoomLevel, zoomLevel, zoomLevel);

            Vector2 contentPositionOffset = new Vector2(xPercentOffset * content.worldBound.width, yPercentOffset * content.worldBound.height);
            float xOffset = (content.contentRect.width - content.worldBound.width) / 2.0f;
            float yOffset = (content.contentRect.height - content.worldBound.height) / 2.0f;
            Vector2 offset = new Vector2(xOffset, yOffset);
            content.transform.position = center - contentPositionOffset - offset;

            if(onZoomOut != null) { onZoomOut.Invoke(); }

            RepaintBackgroundGrid();
        }

        public Vector2 GetCenter()
        {
            Vector2 center = this.parent.contentRect.center;
            return center;
        }

        public Vector2 GetTopLeft()
        {
            Vector2 topLeft = new Vector2(this.parent.contentRect.xMin, this.parent.contentRect.yMin);
            return topLeft;
        }

        public Vector2 GetBottomRight()
        {
            Vector2 bottomRight = new Vector2(this.parent.contentRect.xMax, this.parent.contentRect.yMax);
            return bottomRight;
        }

        public void PanTo(Vector2 point)
        {
            if (float.IsNaN(point.x) || float.IsNaN(point.y)) { return; }

            this.Pan(GetCenter() - point);
        }

        public void ZoomToView(Rect rectangle)
        {
            Vector2 contentTopLeft = this.ChangeCoordinatesTo(content, GetTopLeft());
            Vector2 contentBottomRight = this.ChangeCoordinatesTo(content, GetBottomRight());
            bool zoomedOut = false;
            int totalZooms = 0;

            if (float.IsNaN(contentTopLeft.x) || float.IsNaN(contentBottomRight.x) ||
                float.IsNaN(contentTopLeft.y) || float.IsNaN(contentBottomRight.y))
            {
                return;
            }

            while (totalZooms < 20 &&
                (rectangle.yMin < contentTopLeft.y ||
                rectangle.yMax > contentBottomRight.y ||
                rectangle.xMin < contentTopLeft.x ||
                rectangle.xMax > contentBottomRight.x))
            {
                ZoomOut();
                zoomedOut = true;
                totalZooms++;
                contentTopLeft = this.ChangeCoordinatesTo(content, GetTopLeft());
                contentBottomRight = this.ChangeCoordinatesTo(content, GetBottomRight());

                if (float.IsNaN(contentTopLeft.x) || float.IsNaN(contentBottomRight.x) ||
                float.IsNaN(contentTopLeft.y) || float.IsNaN(contentBottomRight.y))
                {
                    break; ;
                }
            }

            //bool zoomedIn = false;
            totalZooms = 0;
            if (!zoomedOut)
            {
                while ((rectangle.yMin > contentTopLeft.y &&
                    rectangle.yMax < contentBottomRight.y &&
                    rectangle.xMin > contentTopLeft.x &&
                    rectangle.xMax < contentBottomRight.x))
                {
                    ZoomIn();
                    //zoomedIn = true;
                    totalZooms++;
                    contentTopLeft = this.ChangeCoordinatesTo(content, GetTopLeft());
                    contentBottomRight = this.ChangeCoordinatesTo(content, GetBottomRight());

                    if (float.IsNaN(contentTopLeft.x) || float.IsNaN(contentBottomRight.x) ||
                        float.IsNaN(contentTopLeft.y) || float.IsNaN(contentBottomRight.y))
                    {
                        break;
                    }
                }
            }
        }

        public float Zoom
        {
            get { return this.zoomLevel; }
            set { this.zoomLevel = value; }
        }
    }
}