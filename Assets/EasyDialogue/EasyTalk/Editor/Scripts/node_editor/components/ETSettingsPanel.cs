using EasyTalk.Editor.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETSettingsPanel : Box
    {
        private ETNode activeNode;

        private Label descriptionLabel;

        private Box contentPanel;

        private bool isActive = false;

        public ETSettingsPanel() : base()
        {
            //this.pickingMode = PickingMode.Ignore;
            this.AddToClassList("settings-panel");

            //Add a settings label
            Label settingsLabel = new Label("Settings");
            settingsLabel.style.fontSize = 16;
            settingsLabel.style.paddingLeft = 8;
            settingsLabel.style.paddingTop = 8;
            this.Add(settingsLabel);

            //Add an identifier
            descriptionLabel = new Label("Node");
            descriptionLabel.style.paddingLeft = 8;
            descriptionLabel.style.paddingTop = 8;
            this.Add(descriptionLabel);

            Button closeButton = new Button();
            closeButton.text = "X";
            closeButton.style.position = Position.Absolute;
            closeButton.style.right = 4;
            closeButton.style.top = 4;
            closeButton.clicked += Close;
            this.Add(closeButton);

            contentPanel = new Box();
            contentPanel.pickingMode = PickingMode.Ignore;
            contentPanel.AddToClassList("settings-content-panel");
            this.Add(contentPanel);
        }

        public void SetActiveNode(ETNode node)
        {
            this.activeNode = node;
            node.BuildSettingsPanel(this);
        }

        public void SetColor(Color color)
        {
            this.style.backgroundColor = color;
        }

        public Box GetContentPanel()
        {
            return contentPanel;
        }

        public void ClearContentPanel()
        {
            this.contentPanel.Clear();
        }

        private void Close()
        {
            Hide(false);
        }

        public void Show()
        {
            this.style.display = DisplayStyle.Flex;
            isActive = true;
        }

        public void ShowIfActive()
        {
            if(isActive && activeNode.SupportsSettings)
            {
                Show();
            }
        }

        public void Hide(bool keepActive = true)
        {
            this.style.display = DisplayStyle.None;
            if(!keepActive)
            {
                isActive = false;
            }
        }

        public void SetDescription(string desc)
        {
            this.descriptionLabel.text = desc;
        }
    }
}
