using EasyTalk.Display;
using EasyTalk.Utils;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyTalk.Display
{
    /// <summary>
    /// This class implements the logic which handles player input controls for Dialogue Displays.
    /// </summary>
    public class DialogueInputHandler : MonoBehaviour
    {
        /// <summary>
        /// The Dialogue Display to control.
        /// </summary>
        [Tooltip("The Dialogue Display to control. If left unset, the input handler will attempt to retrieve a display from the GameObject on Awake.")]
        [SerializeField] private AbstractDialogueDisplay dialogueDisplay;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// The input action mapping used to process player input and control the dialogue display.
        /// </summary>
        [Tooltip("The InputAction asset to use to detect player input events and control the dialogue system.")]
        [SerializeField] InputActionAsset inputActions;

        /// <summary>
        /// The name of the action which when triggered will cause the dialogue to continue forward in the currently active conversation (only applicable when displaying dialogue).
        /// </summary>
        [Tooltip("The name of the input action which triggers the dialogue to continue playing to the next line/options.")]
        [SerializeField] private string continueActionName = "Continue";

        /// <summary>
        /// The name of the action which when triggered will select the next option (only applicable when options are being displayed).
        /// </summary>
        [Tooltip("The name of the input action which triggers the dialogue display to select the next option whenever options are presented to the player.")]
        [SerializeField] private string nextOptionActionName = "NextOption";

        /// <summary>
        /// The name of the action which when triggered will select the previous option (only applicable when options are being displayed).
        /// </summary>
        [Tooltip("The name of the input action which triggers the dialogue display to select the previous option whenever options are presented to the player.")]
        [SerializeField] private string previousOptionActionName = "PreviousOption";

        /// <summary>
        /// The name of the action which chooses the currently select option and signals the dialogue to continue down the path associated with 
        /// that option (only applicable when options are being displayed).
        /// </summary>
        [Tooltip("The name of the input action which triggers the dialogue system to choose the currently selected option and continue playing.")]
        [SerializeField] private string chooseOptionActionName = "ChooseCurrentOption";

        /// <summary>
        /// The name of the action which triggers am exit of the currently running dialogue (only applicable if allowQuickExit is true).
        /// </summary>
        [Tooltip("The name of the input action which exits the conversation immediately (only applicable if 'allowQuickExit' is set to true).")]
        [SerializeField] private string exitConversationActionName = "ExitConversation";

        /// <summary>
        /// The name of the action which selects an option in a certain direction along the X-axis (only applicable to directional displays when options are being presented).
        /// </summary>
        [Tooltip("The name of the input action which triggers the selection of an option in a direction along a horizontal axis. (This is typically only used for directional option displays)")]
        [SerializeField] private string selectOptionInXDirectionActionName = "SelectOptionInXDirection";

        /// <summary>
        /// The name of the action which selects an option in a certain direction along the Y-axis (only applicable to directional displays when options are being presented).
        /// </summary>
        [Tooltip("The name of the input action which triggers the selection of an option in a direction along a vertical axis. (This is typically only used for directional option displays)")]
        [SerializeField] private string selectOptionInYDirectionActionName = "SelectOptionInYDirection";

        /// <summary>
        /// The name of the action which will submit a player-entered string via a text input display and move on in the dialogue.
        /// </summary>
        [Tooltip("The name of the action which will submit a player-entered string via a text input display and move on in the dialogue.")]
        [SerializeField] private string playerStringSubmissionActionName = "SubmitText";

        /// <summary>
        /// The name of the action which will move the text caret (cursor) to the right by one position when editing text in a text input display.
        /// </summary>
        [Tooltip("The name of the action which will move the text caret (cursor) to the right by one position when editing text in a text input display.")]
        [SerializeField] private string moveTextCursorRightActionName = "MoveTextCursorRight";

        /// <summary>
        /// The name of the action which will move the text caret (cursor) to the left by one position when editing text in a text input display.
        /// </summary>
        [Tooltip("The name of the action which will move the text caret (cursor) to the left by one position when editing text in a text input display.")]
        [SerializeField] private string moveTextCursorLeftActionName = "MoveTextCursorLeft";

        /// <summary>
        /// The name of the action which will change the current character to the next character in the input character list when editing text in a text input display.
        /// </summary>
        [Tooltip("The name of the action which will change the current character to the next character in the input character list when editing text in a text input display.")]
        [SerializeField] private string gotoNextTextCharacterActionName = "GotoNextTextCharacter";

        /// <summary>
        /// The name of the action which will change the current character to the last character in the input character list when editing text in a text input display.
        /// </summary>
        [Tooltip("The name of the action which will change the current character to the last character in the input character list when editing text in a text input display.")]
        [SerializeField] private string gotoLastTextCharacterActionName = "GotoLastTextCharacter";
#else
        /// <summary>
        /// The name of the input which triggers the dialogue to continue (only applicable when dialogue is being displayed).
        /// </summary>
        [SerializeField] private string continueButtonName = "Submit";

        /// <summary>
        /// The name of the input which triggers the dialogue to submit a player's input string (from a text input display), when applicable.
        /// </summary>
        [Tooltip("The name of the input which triggers the dialogue to submit a player's input string (from a text input display), when applicable.")]
        [SerializeField] private string playerStringSubmissionButtonName = "Submit";

        /// <summary>
        /// The name of the input which chooses the currently selected option (only applicable when options are being presented).
        /// </summary>
        [SerializeField] private string chooseSelectedOptionButtonName = "Submit";

        /// <summary>
        /// The name of the input which causes the dialogue to exit (only applicable when allowQuickExit is set to true).
        /// </summary>
        [SerializeField] private string quickExitButtonName = "Cancel";

        /// <summary>
        /// The name of the horizontal input axis. This is used to select options when they are being shown.
        /// </summary>
        [SerializeField] private string horizontalAxisName = "Horizontal";

        /// <summary>
        /// The name of the horizontal input axis for the DPAD. This is used to select options when they are being shown.
        /// </summary>
        [SerializeField] private string dpadHorizontalAxisName = "DPAD_Horizontal";

        /// <summary>
        /// The name of the vertical input axis. This is used to select options when they are being shown.
        /// </summary>
        [SerializeField] private string verticalAxisName = "Vertical";

        /// <summary>
        /// The name of the vertical input axis for the DPAD. This is used to select options when they are being shown.
        /// </summary>
        [SerializeField] private string dpadVerticalAxisName = "DPAD_Vertical";

        /// <summary>
        /// The amount of time which must pass in order for the input axis controls to be usable again after a prior action is triggered (such as selecting an option).
        /// </summary>
        private float axisResetTime = 0.3f;

        /// <summary>
        /// A flag used to keep track of whether warning messages have been made when reading input so that messages aren't resent repeatedly during gameplay.
        /// </summary>
        private bool inputWarningSent = false;

        /// <summary>
        /// A flag indicating whether the axis controls are ready to be triggered to choose an option based on user input.
        /// </summary>
        private bool allowAxisTrigger = false;

        /// <summary>
        /// Keeps track of the last time an input axis was triggered.
        /// </summary>
        private float lastAxisTriggerTime;
#endif

        private void Awake()
        {
            SetupUtils.SetUpEventSystem();

            if (dialogueDisplay == null)
            {
                dialogueDisplay = GetComponent<AbstractDialogueDisplay>();
            }

#if ENABLE_INPUT_SYSTEM
            SetupInputs();
#endif
        }

        /// <summary>
        /// The update loop handles user input if using the old input system.
        /// </summary>
        private void Update()
        {
#if !ENABLE_INPUT_SYSTEM //Handle inputs using the old input manager system.
            HandleUserInput();
#endif
        }

        /// <summary>
        /// Disables the player input controls on the dialogue display when using the new input system.
        /// </summary>
        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM //Disable the input actions (if using the new input system).
            if (inputActions != null) { inputActions.Disable(); }
#endif
        }

        /// <summary>
        /// Enables the player input controls on the dialogue display when using the new input system.
        /// </summary>
        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM //Enable the input actions (if using the new input system).
            if (inputActions != null) { inputActions.Enable(); }
#endif
        }


#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Sets up each input action to call the approrate dialogue control methods when those inputs are actvated by the player.
        /// </summary>
        protected void SetupInputs()
        {
            //Add actions to handle user input using the new input system.
            if (inputActions != null)
            {
                SetUpContinueAction();
                SetUpNextOptionAction();
                SetUpPreviousOptionAction();
                SetUpChooseOptionAction();
                SetUpExitConversationAction();
                SetUpSelectOptionInXDirectionAction();
                SetUpSelectOptionInYDirectionAction();
                SetUpPlayerStringAction();
                SetUpMoveTextCursorRightAction();
                SetUpMoveTextCursorLeftAction();
                SetUpGotoNextCharacterAction();
                SetUpGotoLastCharacterAction();
            }
        }

        /// <summary>
        /// Sets up the input action for selecting an option along the vertical axis direction.
        /// </summary>
        private void SetUpSelectOptionInYDirectionAction()
        {
            InputAction selectOptionInYDirectionAction = inputActions.FindAction(selectOptionInYDirectionActionName);
            if (selectOptionInYDirectionAction != null)
            {
                selectOptionInYDirectionAction.performed += delegate
                {
                    if (dialogueDisplay.IsOptionSelectionAllowed)
                    {
                        SelectOptionFromInputDirection();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for selecting an option along the horizontal axis direction.
        /// </summary>
        private void SetUpSelectOptionInXDirectionAction()
        {
            InputAction selectOptionInXDirectionAction = inputActions.FindAction(selectOptionInXDirectionActionName);
            if (selectOptionInXDirectionAction != null)
            {
                selectOptionInXDirectionAction.performed += delegate
                {
                    if (dialogueDisplay.IsOptionSelectionAllowed)
                    {
                        SelectOptionFromInputDirection();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for exiting the dialogue conversation.
        /// </summary>
        private void SetUpExitConversationAction()
        {
            InputAction exitConversationAction = inputActions.FindAction(exitConversationActionName);
            if (exitConversationAction != null)
            {
                exitConversationAction.performed += delegate
                {
                    if (dialogueDisplay.IsQuickExitAllowed)
                    {
                        dialogueDisplay.DisableOptionSelection();
                        dialogueDisplay.DisableContinue();
                        dialogueDisplay.ExitDialogue();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for finalizing and choosing the currently selected option.
        /// </summary>
        private void SetUpChooseOptionAction()
        {
            InputAction chooseOptionAction = inputActions.FindAction(chooseOptionActionName);
            if (chooseOptionAction != null)
            {
                chooseOptionAction.performed += delegate
                {
                    if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && dialogueDisplay.IsOptionSelectionAllowed)
                    {
                        dialogueDisplay.ChooseSelectedOption();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for selecting the previous option.
        /// </summary>
        private void SetUpPreviousOptionAction()
        {
            InputAction previousOptionAction = inputActions.FindAction(previousOptionActionName);
            if (previousOptionAction != null)
            {
                previousOptionAction.performed += delegate
                {
                    if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && dialogueDisplay.IsOptionSelectionAllowed)
                    {
                        dialogueDisplay.SelectPreviousOption();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for selecting the next option.
        /// </summary>
        private void SetUpNextOptionAction()
        {
            InputAction nextOptionAction = inputActions.FindAction(nextOptionActionName);
            if (nextOptionAction != null)
            {
                nextOptionAction.performed += delegate
                {
                    if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && dialogueDisplay.IsOptionSelectionAllowed)
                    {
                        dialogueDisplay.SelectNextOption();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for continuing to the next part of the conversation when the player hits the continue button.
        /// </summary>
        private void SetUpContinueAction()
        {
            InputAction continueAction = inputActions.FindAction(continueActionName);
            if (continueAction != null)
            {
                continueAction.performed += delegate
                {
                    if (dialogueDisplay.IsCurrentlyInConversation && dialogueDisplay.IsContinueAllowed)
                    {
                        dialogueDisplay.Continue();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for submitting a player's string into a text input display.
        /// </summary>
        private void SetUpPlayerStringAction()
        {
            InputAction playerStringAction = inputActions.FindAction(playerStringSubmissionActionName);
            if (playerStringAction != null)
            {
                playerStringAction.performed += delegate
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        inputDisplay.TextInputEntered();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for moving the text input caret to the right by one character.
        /// </summary>
        private void SetUpMoveTextCursorRightAction()
        {
            InputAction moveCursorRightAction = inputActions.FindAction(moveTextCursorRightActionName);
            if (moveCursorRightAction != null)
            {
                moveCursorRightAction.performed += delegate
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        inputDisplay.MoveCursorRight();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action for moving the text input caret to the left by one character.
        /// </summary>
        private void SetUpMoveTextCursorLeftAction()
        {
            InputAction moveCursorLeftAction = inputActions.FindAction(moveTextCursorLeftActionName);
            if (moveCursorLeftAction != null)
            {
                moveCursorLeftAction.performed += delegate
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        inputDisplay.MoveCursorLeft();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action to change the character at the current caret/cursor location in a text display to whatever the next character is.
        /// </summary>
        private void SetUpGotoNextCharacterAction()
        {
            InputAction gotoNextCharacterAction = inputActions.FindAction(gotoNextTextCharacterActionName);
            if (gotoNextCharacterAction != null)
            {
                gotoNextCharacterAction.performed += context =>
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction) 
                        {
                            inputDisplay.CycleNextCharacters();
                        }
                        else
                        {
                            inputDisplay.NextCharacter();
                        }
                    }
                };

                gotoNextCharacterAction.canceled += delegate
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        inputDisplay.StopCharacterCycling();
                    }
                };
            }
        }

        /// <summary>
        /// Sets up the input action to change the character at the current caret/cursor location in a text display to whatever the previous character is.
        /// </summary>
        private void SetUpGotoLastCharacterAction()
        {
            InputAction gotoLastCharacterAction = inputActions.FindAction(gotoLastTextCharacterActionName);
            if (gotoLastCharacterAction != null)
            {
                gotoLastCharacterAction.performed += context =>
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
                        {
                            inputDisplay.CycleLastCharacters();
                        }
                        else
                        {
                            inputDisplay.LastCharacter();
                        }
                    }
                };

                gotoLastCharacterAction.canceled += delegate
                {
                    TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                    if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                    {
                        inputDisplay.StopCharacterCycling();
                    }
                };
            }
        }
#endif

        /// <summary>
        /// Checks the value of the x and y axis inputs and attempts to select the option in the corresponding direction.
        /// </summary>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        private bool SelectOptionFromInputDirection()
        {
#if ENABLE_INPUT_SYSTEM
            float xValue = inputActions[selectOptionInXDirectionActionName].ReadValue<float>();
            float yValue = inputActions[selectOptionInYDirectionActionName].ReadValue<float>();
#else
        float xValue = Input.GetAxis(horizontalAxisName);
        float yValue = Input.GetAxis(verticalAxisName);
#endif

            Vector2 direction = new Vector2(xValue, yValue);

            if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && direction.magnitude > 0.5f)
            {
                return dialogueDisplay.SelectOptionInDirection(direction.normalized);
            }

            return false;
        }

#if !ENABLE_INPUT_SYSTEM //Handle user input controls (uses the old input manager).
        /// <summary>
        /// Handles user input controls when using the old input system.
        /// </summary>
        private void HandleUserInput()
        {
            if (dialogueDisplay.CurrentController != null)
            {
                HandleContinueButton();
                HandleChooseOption();
                HandleExitConversation();

                float horizontal = GetHorizontalAxisValue();
                float vertical = GetVerticalAxisValue();
                float dpadHorizontal = GetDPadHorizontalValue();
                float dpadVertical = GetDPadVerticalValue();

                float axisActivationThreshold = 0.2f;

                if (Mathf.Abs(dpadHorizontal) > Mathf.Abs(horizontal)) { horizontal = dpadHorizontal; }
                if (Mathf.Abs(dpadVertical) > Mathf.Abs(vertical)) { vertical = dpadVertical; }

                CheckToAllowAxisTriggering(horizontal, vertical, axisActivationThreshold);
                HandleSelectNextOption(horizontal, vertical, axisActivationThreshold);
                HandleSelectPreviousOption(horizontal, vertical, axisActivationThreshold);
                HandleSelectOptionInDirection(horizontal, vertical, axisActivationThreshold);
                HandleMoveTextCursor(dpadHorizontal);
                HandleChangeTextCharacter(dpadVertical);
                HandlePlayerTextSubmission();
            }
        }

        /// <summary>
        /// Sets the allowAxisTrigger flag to true if enough time has passed or if the horizontal and vertical axis inputs have moved back down below the activation threshold.
        /// </summary>
        /// <param name="horizontal">The horizontal axis input value.</param>
        /// <param name="vertical">The vertical axis input value.</param>
        /// <param name="axisActivationThreshold">The threshold value which the horizontal or vertical axis values must be below in order for allowAxisTrigger to be set to true (unless enough time has elapsed).</param>
        private void CheckToAllowAxisTriggering(float horizontal, float vertical, float axisActivationThreshold)
        {
            if ((horizontal < axisActivationThreshold && horizontal > -axisActivationThreshold) &&
                                (vertical < axisActivationThreshold && vertical > -axisActivationThreshold))
            {
                allowAxisTrigger = true;
            }
            else if ((Time.time - lastAxisTriggerTime > axisResetTime))
            {
                allowAxisTrigger = true;
            }
        }

        /// <summary>
        /// When options are being presented to the player, this method attempts to select the option which corresponds to the current input axis direction (horizontal + vertical)
        /// if the axis control is allowed to be triggered.
        /// </summary>
        /// <param name="horizontal">The horizontal axis input value.</param>
        /// <param name="vertical">The vertical axis input value.</param>
        /// <param name="axisActivationThreshold">The threshold value which the horizontal or vertical axis values must exceed in order for the action to be triggered.</param>
        private void HandleSelectOptionInDirection(float horizontal, float vertical, float axisActivationThreshold)
        {
            if ((Mathf.Abs(horizontal) > axisActivationThreshold || Mathf.Abs(vertical) > axisActivationThreshold) && allowAxisTrigger)
            {
                if (dialogueDisplay.IsOptionSelectionAllowed)
                {
                    lastAxisTriggerTime = Time.time;
                    allowAxisTrigger = !SelectOptionFromInputDirection();
                }
            }
        }

        /// <summary>
        /// If 'allowQuickExit' is set to true, this method attempts to exit the dialogue conversation immediately whenever the input button mapped to 'quickExitButtonName' is pressed.
        /// </summary>
        private void HandleExitConversation()
        {
            try
            {
                if (Input.GetButtonDown(quickExitButtonName))
                {
                    if (dialogueDisplay.IsQuickExitAllowed)
                    {
                        dialogueDisplay.DisableOptionSelection();
                        dialogueDisplay.DisableContinue();
                        dialogueDisplay.ExitDialogue();
                    }
                }
            }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for the 'quick exit' button. Make sure there is a button configured in the Input Manager named '" + quickExitButtonName + "'");
                }
                inputWarningSent = true;

            }
        }

        /// <summary>
        /// When presenting options, this method attempts to finalize and choose the currently selected option whenever the input button mapped to 'chooseSelectedOptionButtonName' is pressed.
        /// </summary>
        private void HandleChooseOption()
        {
            try
            {
                if (Input.GetButtonDown(chooseSelectedOptionButtonName))
                {
                    if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && dialogueDisplay.IsOptionSelectionAllowed)
                    {
                        dialogueDisplay.ChooseSelectedOption();
                    }
                }
            }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for the 'choose option' button. Make sure there is a button configured in the Input Manager named '" + chooseSelectedOptionButtonName + "'");
                }
                inputWarningSent = true;
            }
        }

        /// <summary>
        /// When options are being presented to the player, this method attempts to select the previous option if the player is pressing the axis controls 
        /// in the configured direction and the control is allowed to be triggered.
        /// </summary>
        /// <param name="horizontal">The horizontal axis input value.</param>
        /// <param name="vertical">The vertical axis input value.</param>
        /// <param name="axisActivationThreshold">The threshold value which the horizontal or vertical axis values must exceed in order for the action to be triggered.</param>
        private void HandleSelectPreviousOption(float horizontal, float vertical, float axisActivationThreshold)
        {
            if ((horizontal < -axisActivationThreshold || vertical > axisActivationThreshold) && allowAxisTrigger)
            {
                if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && dialogueDisplay.IsOptionSelectionAllowed)
                {
                    lastAxisTriggerTime = Time.time;
                    allowAxisTrigger = !dialogueDisplay.SelectPreviousOption();
                }
            }
        }

        /// <summary>
        /// When options are being presented to the player, this method attempts to select the next option if the player is pressing the axis controls 
        /// in the configured direction and the control is allowed to be triggered.
        /// </summary>
        /// <param name="horizontal">The horizontal axis input value.</param>
        /// <param name="vertical">The vertical axis input value.</param>
        /// <param name="axisActivationThreshold">The threshold value which the horizontal or vertical axis values must exceed in order for the action to be triggered.</param>
        private void HandleSelectNextOption(float horizontal, float vertical, float axisActivationThreshold)
        {
            if ((horizontal > axisActivationThreshold || vertical < -axisActivationThreshold) && allowAxisTrigger)
            {
                if (!dialogueDisplay.IsCurrentlyInConversation && !dialogueDisplay.GetOptionDisplay().IsHidden && dialogueDisplay.IsOptionSelectionAllowed)
                {
                    lastAxisTriggerTime = Time.time;
                    allowAxisTrigger = !dialogueDisplay.SelectNextOption();
                }
            }
        }

        /// <summary>
        /// Retrieves the vertical axis value from the player's input controls (uses the axis mapped to 'dpadVerticalAxisName'). This is intended for DPAD input, but can be used for any alternative axis.
        /// </summary>
        /// <returns>The vertical axis value. If there is no axis configured with the proper name, this method returns 0.</returns>
        private float GetDPadVerticalValue()
        {
            try
            {
                float dpadVertical = Input.GetAxis(dpadVerticalAxisName);
                return dpadVertical;
                }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for D-Pad vertical axis. Make sure there is an axis configured in the Input Manager named '" + dpadVerticalAxisName + "'");
                }
                inputWarningSent = true;
            }

            return 0.0f;
        }

        /// <summary>
        /// Retrieves the horizontal axis value from the player's input controls (uses the axis mapped to 'dpadHorizontalAxisName'). This is intended for DPAD input, but can be used for any alternative axis.
        /// </summary>
        /// <returns>The horizontal axis value. If there is no axis configured with the proper name, this method returns 0.</returns>
        private float GetDPadHorizontalValue()
        {
            try
            {
                float dpadHorizontal = Input.GetAxis(dpadHorizontalAxisName);
                return dpadHorizontal;
            }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for D-Pad horizontal axis. Make sure there is an axis configured in the Input Manager named '" + dpadHorizontalAxisName + "'");
                }
                inputWarningSent = true;
            }

            return 0.0f;
        }

        /// <summary>
        /// Retrieves the vertical axis value from the player's input controls (uses the axis mapped to 'verticalAxisName').
        /// </summary>
        /// <returns>The vertical axis value. If there is no axis configured with the proper name, this method returns 0.</returns>
        private float GetVerticalAxisValue()
        {
            try
            {
                float vertical = Input.GetAxis(verticalAxisName);
                return vertical;
            }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for the vertical axis. Make sure there is an axis configured in the Input Manager named '" + verticalAxisName + "'");
                }
                inputWarningSent = true;
            }

            return 0.0f;
        }

        /// <summary>
        /// Retrieves the horizontal axis value from the player's input controls (uses the axis mapped to 'horizontalAxisName').
        /// </summary>
        /// <returns>The horizontal axis value. If there is no axis configured with the proper name, this method returns 0.</returns>
        private float GetHorizontalAxisValue()
        {
            try
            {
                float horizontal = Input.GetAxis(horizontalAxisName);
                return horizontal;
            }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for the horizontal axis. Make sure there is an axis configured in the Input Manager named '" + horizontalAxisName + "'");
                }
                inputWarningSent = true;
            }

            return 0.0f;
        }

        /// <summary>
        /// Checks to see if the player pressed the continue button. If they did and continuation is allowed, continues to the next part of the conversation.
        /// </summary>

        private void HandleContinueButton()
        {
            try
            {
                if (Input.GetButtonDown(continueButtonName))
                {
                    if (dialogueDisplay.IsCurrentlyInConversation && dialogueDisplay.IsContinueAllowed)
                    {
                        dialogueDisplay.Continue();
                    }
                }
            }
            catch
            {
                if (!inputWarningSent)
                {
                    Debug.LogWarning("Encountered an error when attempting to read input for continue button. Make sure there is a button configured in the Input Manager named '" + continueButtonName + "'");
                }
                inputWarningSent = true;
            }
        }

        /// <summary>
        /// Checks to see if the player has pressed the button to submit text, and submits the player generated text, if applicable.
        /// </summary>
        private void HandlePlayerTextSubmission()
        {
            if (Input.GetButtonDown(playerStringSubmissionButtonName))
            {
                TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                {
                    inputDisplay.TextInputEntered();
                }
            }
        }

        /// <summary>
        /// Checks to see if the player pressed a button to change the caret (cursor) position to edit the text on a text input display.
        /// </summary>
        private void HandleMoveTextCursor(float dpadHorizontal) 
        {
            if(Mathf.Abs(dpadHorizontal) > 0.0f) 
            {
                TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                {
                    if(dpadHorizontal > 0.0f) 
                    {
                        inputDisplay.MoveCursorRight();
                    }
                    else
                    {
                        inputDisplay.MoveCursorLeft();
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the player pressed a button to change the text input character being input into a text display.
        /// </summary>

        private void HandleChangeTextCharacter(float dpadVertical) 
        {
            if(Mathf.Abs(dpadVertical) > 0.0f) 
            {
                TextInputDisplay inputDisplay = dialogueDisplay.GetTextInputDisplay();
                if (inputDisplay != null && inputDisplay.isActiveAndEnabled)
                {
                    if(dpadVertical > 0.0f) 
                    {
                        inputDisplay.NextCharacter();
                    }
                    else
                    {
                        inputDisplay.LastCharacter();
                    }
                }
            }
        }
#endif
    }
}