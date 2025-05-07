using EasyTalk.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public class ETNodeButton : Box
    {
        private bool isButtonPressed = false;

        public delegate void OnButtonClicked();

        public OnButtonClicked onButtonClicked;

        public ETNodeButton() : base()
        {
            this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.RegisterCallback<MouseUpEvent>(OnMouseUp);
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);

            this.RegisterCallback<AttachToPanelEvent>(OnAddedToParent);
        }

        protected void OnAddedToParent(AttachToPanelEvent evt)
        {
            EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseUpEvent>(OnMouseUp);
            EasyTalkNodeEditor.Instance.PanZoomPanel.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public void OnMouseUp(MouseUpEvent evt)
        {
            Vector2 mousePos = (evt.target as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.PanZoomPanel, evt.localMousePosition);

            VisualElement elementAtMouse = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

            if (elementAtMouse == this || evt.target == this)
            {
                this.style.opacity = 1.0f;

                if (isButtonPressed && onButtonClicked != null)
                {
                    onButtonClicked.Invoke();
                }
            }

            isButtonPressed = false;
        }

        public void OnMouseDown(MouseDownEvent evt)
        {
            Vector2 mousePos = (evt.target as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.NodeView, evt.localMousePosition);
            VisualElement elementAtMouse = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

            if (evt.button == 0 && (elementAtMouse == this || evt.target == this))
            {
                isButtonPressed = true;
                this.style.opacity = 0.5f;
            }
        }

        public void OnMouseMove(MouseMoveEvent evt)
        {
            Vector2 mousePos = (evt.target as VisualElement).ChangeCoordinatesTo(EasyTalkNodeEditor.Instance.NodeView, evt.localMousePosition);
            VisualElement elementAtMouse = EasyTalkNodeEditor.Instance.GetElementAtMouse(mousePos);

            if (elementAtMouse == this || evt.target == this)
            {
                this.style.opacity = 1.0f;
            }
            else
            {
                this.style.opacity = 0.7f;
            }
        }
    }
}