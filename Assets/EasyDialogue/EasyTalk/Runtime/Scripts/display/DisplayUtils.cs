using EasyTalk.Character;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Utility;
using EasyTalk.Settings;
using System.Collections.Generic;

namespace EasyTalk.Display
{
    /// <summary>
    /// This is a utilty class for handling specific display functions, such as show/hide node functionality.
    /// </summary>
    public class DisplayUtils
    {
        /// <summary>
        /// The currently active node.
        /// </summary>
        private static AsyncNode activeNode;

        /// <summary>
        /// A List of panels which are to be shown.
        /// </summary>
        private static List<DialoguePanel> panelsToShow = new List<DialoguePanel>();

        /// <summary>
        /// The number of panels shown.
        /// </summary>
        private static int numPanelsShown = 0;

        /// <summary>
        /// The target number of panels to show.
        /// </summary>
        private static int numPanelsToShow = 0;

        /// <summary>
        /// A List of panels which are to be hidden.
        /// </summary>
        private static List<DialoguePanel> panelsToHide = new List<DialoguePanel>();

        /// <summary>
        /// The number of panels hidden.
        /// </summary>
        private static int numPanelsHidden = 0;

        /// <summary>
        /// The target number of panels to hide.
        /// </summary>
        private static int numPanelsToHide = 0;

        /// <summary>
        /// Processes ths provided Show node, showing each panel/character configured for the node.
        /// </summary>
        /// <param name="node">The Show node to process.</param>
        /// <param name="displayMap">A mapping of Display IDs to Dialogue Panels.</param>
        /// <param name="dialogueSettings">The dialogue settings to use.</param>
        public static void HandleShowNode(AsyncNode node, Dictionary<string, DialoguePanel> displayMap, EasyTalkDialogueSettings dialogueSettings)
        {
            //Find the display for each target in the Show node, set animatable sprites as appropriate, and display the display for each before continuing.
            activeNode = node;
            ShowNode showNode = (ShowNode)node;

            foreach (ShowNodeItem item in showNode.Items)
            {
                if (item.ShowMode == ShowMode.CHARACTER)
                {
                    CharacterDefinition charDef = dialogueSettings.DialogueRegistry.CharacterLibrary.GetCharacterDefinition(item.CharacterName);
                    AnimatableDisplayImage img = charDef.GetPortrayalSprite(item.ImageID);

                    string targetDisplay = img.TargetID;
                    if(item.DisplayID != null && item.DisplayID.Length > 0)
                    {
                        targetDisplay = item.DisplayID;
                    }
                    
                    if (displayMap.ContainsKey(targetDisplay)) //Find the sub-display
                    {
                        DialoguePanel panel = displayMap[targetDisplay];

                        //If the panel isn't active, we need to activate it to make sure it's initial visibility state (isHidden) is set correctly.
                        //This is necessary because if the developer decides to disable the panel or its parent, the panel will cease to operate propertly when told to show itself.
                        if(!panel.isActiveAndEnabled)
                        {
                            panel.Activate();
                        }

                        if (panel.IsHidden && panel is CharacterSpritePanel)
                        {
                            panelsToShow.Add(panel);
                            panel.onShowComplete.AddListener(PanelShown);
                            ((CharacterSpritePanel)panel).SetImageOnPanel(item.CharacterName, item.ImageID);
                            numPanelsToShow++;
                            panel.Show();
                        }
                    }
                }
                else if (item.ShowMode == ShowMode.DISPLAY)
                {
                    //Find the sub-display
                    if (displayMap.ContainsKey(item.DisplayID))
                    {
                        DialoguePanel panel = displayMap[item.DisplayID];

                        if (panel.IsHidden)
                        {
                            panelsToShow.Add(panel);
                            panel.onShowComplete.AddListener(PanelShown);
                            numPanelsToShow++;
                            panel.Show();
                        }
                    }
                }
            }

            if(numPanelsToShow == 0)
            {
                PanelShown();
            }
        }

        /// <summary>
        /// Called when a panel has finished being shown. After the target counter is reached, ExecutionCompleted() is called on the active Show node.
        /// </summary>
        private static void PanelShown()
        {
            numPanelsShown++;

            if (numPanelsToShow <= numPanelsShown)
            {
                //Remove listener from panels.
                foreach (DialoguePanel panel in panelsToShow)
                {
                    panel.onShowComplete.RemoveListener(PanelShown);
                }

                panelsToShow.Clear();
                numPanelsToShow = 0;
                numPanelsShown = 0;

                if (activeNode != null)
                {
                    activeNode.ExecutionCompleted();
                    activeNode = null;
                }
            }
        }

        /// <summary>
        /// Processes the provided Hide node, hiding each panel/character configured for the node.
        /// </summary>
        /// <param name="node">The Hide node to process.</param>
        /// <param name="displayMap">A mapping of Display IDs to Dialogue Panels.</param>
        /// <param name="dialogueSettings">The dialogue settings to use.</param>
        public static void HandleHideNode(AsyncNode node, Dictionary<string, DialoguePanel> displayMap, EasyTalkDialogueSettings dialogueSettings)
        {
            //Find the display for each target in the Hide node, and hide the display for each before continuing.
            activeNode = node;
            HideNode hideNode = (HideNode)node;

            foreach (HideNodeItem item in hideNode.Items)
            {
                if (item.HideMode == HideMode.CHARACTER)
                {
                    //Search through all character sprite displays to see if they are displaying the character targeted by the hide node, and if they are, hide them.
                    foreach(DialoguePanel panel in displayMap.Values)
                    {
                        //If the panel isn't active, we need to activate it to make sure it's initial visibility state (isHidden) is set correctly.
                        //This is necessary because if the developer decides to disable the panel or its parent, the panel will cease to operate propertly when told to hide itself.
                        if (!panel.isActiveAndEnabled)
                        {
                            panel.Activate();
                        }

                        if (!panel.IsHidden && panel is CharacterSpritePanel)
                        {
                            CharacterSpritePanel spritePanel = panel as CharacterSpritePanel;
                            if(spritePanel.CurrentCharacterName != null && spritePanel.CurrentCharacterName.Equals(item.CharacterName))
                            {
                                panelsToHide.Add(spritePanel);
                                panel.onHideComplete.AddListener(PanelHidden);
                                numPanelsToHide++;
                                panel.Hide();
                            }
                        }
                    }
                }
                else if (item.HideMode == HideMode.DISPLAY)
                {
                    //Find the sub-display
                    if (displayMap.ContainsKey(item.DisplayID))
                    {
                        DialoguePanel panel = displayMap[item.DisplayID];

                        if (!panel.IsHidden)
                        {
                            panelsToHide.Add(panel);
                            panel.onHideComplete.AddListener(PanelHidden);
                            numPanelsToHide++;
                            panel.Hide();
                        }
                    }
                }
            }

            if(numPanelsToHide == 0)
            {
                PanelHidden();
            }
        }

        /// <summary>
        /// Called when a panel has finished being hidden. After the target counter is reached, ExecutionCompleted() is called on the active Hide node.
        /// </summary>
        private static void PanelHidden()
        {
            numPanelsHidden++;

            if (numPanelsToHide <= numPanelsHidden)
            {
                //Remove listener from panels.
                foreach (DialoguePanel panel in panelsToShow)
                {
                    panel.onHideComplete.RemoveListener(PanelHidden);
                }

                panelsToHide.Clear();
                numPanelsToHide = 0;
                numPanelsHidden = 0;

                if (activeNode != null)
                {
                    activeNode.ExecutionCompleted();
                    activeNode = null;
                }
            }
        }
    }
}
