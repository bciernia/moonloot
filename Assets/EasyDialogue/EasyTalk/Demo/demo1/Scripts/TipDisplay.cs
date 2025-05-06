using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyTalk.Display;
using UnityEngine.UI;
using EasyTalk.Nodes.Common;
using EasyTalk.Controller;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyTalk.Demo
{
    /// <summary>
    /// Displays tips in the demo.
    /// </summary>
    public class TipDisplay : DialoguePanel
    {
        [SerializeField]
        private DialogueController tipController;

        /// <summary>
        /// The text component to display tips on.
        /// </summary>
        [SerializeField]
        private Text text;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// The input actions asset to use.
        /// </summary>
        [SerializeField]
        private InputActionAsset playerInputActions;
#endif

        /// <summary>
        /// A flag used to track whether tips should be displayed to the player or not.
        /// </summary>
        private bool showTips = true;

        private void Awake()
        {
            Init();
            HideImmediately();

#if ENABLE_INPUT_SYSTEM
            InputAction nextTipAction = playerInputActions.FindAction("NextTip");

            if(nextTipAction != null )
            {
                nextTipAction.performed += delegate
                {
                    tipController.Continue();
                };
            }

            InputAction hideTipsAction = playerInputActions.FindAction("HideTips");

            if (hideTipsAction != null)
            {
                hideTipsAction.performed += delegate
                {
                    showTips = !showTips;
                    this.gameObject.SetActive(false);
                };
            }
#endif
        }

        private void Update()
        {
#if !ENABLE_INPUT_SYSTEM
            if(Input.GetKeyDown(KeyCode.H)) 
            {
                this.gameObject.SetActive(false);
                showTips = !showTips;
                return;
            }

            if(Input.GetKeyDown(KeyCode.N))
            {
                tipController.Continue();
            }
#endif
        }

        public override void OnDialogueExited(string exitPointName)
        {
            base.OnDialogueExited(exitPointName);

            if(this.gameObject.activeSelf)
            {
                Hide();
            }
        }

        public override void OnDisplayLine(ConversationLine conversationLine)
        {
            base.OnDisplayLine(conversationLine);

            text.text = conversationLine.Text;

            if (showTips)
            {
                this.gameObject.SetActive(true);
                Show();
            }
        }
    }
}
