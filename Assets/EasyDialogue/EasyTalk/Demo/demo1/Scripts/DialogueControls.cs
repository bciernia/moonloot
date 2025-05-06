using EasyTalk.Controller;
using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyTalk.Demo
{
    /// <summary>
    /// Handles player controls for going into a dialogue with a character.
    /// </summary>
    public class DialogueControls : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// The Input Action Asset to use for player input controls.
        /// </summary>
        [SerializeField]
        private InputActionAsset inputActions;
#endif


        /// <summary>
        /// Whether the controls are ready to call PlayDialogue() on a controller.
        /// </summary>
        private bool isReady = false;

        /// <summary>
        /// The Dialogue Controller to call PlayDialogue() on when the player presses the right input control button.
        /// </summary>
        private DialogueController controller;

        
        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            InputAction startDialogueAction = inputActions["EnterDialogue"];

            if (startDialogueAction != null)
            {
                startDialogueAction.performed += delegate
                {
                    Play();
                };
            }
#endif
        }

        // Update is called once per frame
        void Update()
        {
#if !ENABLE_INPUT_SYSTEM
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
            {
                Play();
            }
#endif
        }

        /// <summary>
        /// Calls PlayDialogue() on the set Dialogue Controller.
        /// </summary>
        private void Play()
        {
            if (isReady && controller != null)
            {
                isReady = false;
                controller.PlayDialogue();
            }
        }

        /// <summary>
        /// Prepares the controls to call PlayDialogue() on the specified Dialogue Controller.
        /// </summary>
        /// <param name="controller">The Dialogue Controller to get ready to play.</param>
        public void ReadyController(DialogueController controller)
        {
            isReady = true;
            this.controller = controller;
        }

        /// <summary>
        /// Sets the controls not to trigger any controller.
        /// </summary>
        public void UnreadyController()
        {
            isReady = false;
            this.controller = null;
        }
    }
}