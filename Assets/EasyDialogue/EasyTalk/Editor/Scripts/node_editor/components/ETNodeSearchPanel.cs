using EasyTalk.Editor.Nodes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class NodeSearchPanel : Box
    {
        private ETTextField searchField;
        private string currentSearchTerm = null;
        private int currentMatchIdx = 0;
        private List<ETNode> foundNodes = new List<ETNode>();

        public NodeSearchPanel() : base()
        {
            this.style.width = new StyleLength(new Length(302.0f, LengthUnit.Pixel));
            this.style.position = Position.Absolute;
            this.style.bottom = 20.0f;
            this.style.left = 20.0f;
            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems = Align.Center;
            this.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            searchField = new ETTextField();
            searchField.NotifyOfChanges = false;
            searchField.style.width = new StyleLength(new Length(120.0f, LengthUnit.Pixel));
            searchField.style.flexGrow = 1;
            Button findNextButton = new Button(FindNext);
            findNextButton.Add(new Label("Find Next"));
            Button findAllButton = new Button(FindAll);
            findAllButton.Add(new Label("Find All"));

            Button closeButton = new Button(Hide);
            closeButton.Add(new Label("x"));
            //closeButton.style.position = Position.Absolute;
            //closeButton.style.left = new StyleLength(new Length(270.0f, LengthUnit.Pixel));

            this.Add(searchField);
            this.Add(findNextButton);
            this.Add(findAllButton);
            this.Add(closeButton);

            Hide();
        }

        public void Hide()
        {
            this.style.visibility = Visibility.Hidden;
        }

        public void Show()
        {
            this.style.visibility = Visibility.Visible;
        }

        private void FindNext()
        {
            EasyTalkNodeEditor.Instance.NodeView.DeselectAllNodes();

            //If we are on a new search term, or find was called again, start a new search.
            if (searchField.text != currentSearchTerm)
            {
                currentSearchTerm = searchField.text;
                foundNodes = EasyTalkNodeEditor.Instance.NodeView.FindAll(searchField.text);
                currentMatchIdx = 0;
            }
            else
            {
                currentMatchIdx++;
                if (currentMatchIdx >= foundNodes.Count) { currentMatchIdx = 0; }
            }

            if (foundNodes.Count > 0)
            {
                foundNodes[currentMatchIdx].Select();
                EasyTalkNodeEditor.Instance.NodeView.PanAndZoomToSelected();
            }
        }

        private void FindAll()
        {
            EasyTalkNodeEditor.Instance.NodeView.DeselectAllNodes();
            EasyTalkNodeEditor.Instance.NodeView.FindAll(searchField.text, true);
            EasyTalkNodeEditor.Instance.NodeView.PanAndZoomToSelected();
        }

        public ETTextField SearchField { get { return searchField; } }
    }
}