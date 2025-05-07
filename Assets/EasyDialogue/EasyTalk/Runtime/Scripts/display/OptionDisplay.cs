using EasyTalk.Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// Extends upon the DialoguePanel class to create an abstract base implementation of a display for displaying dialogue options to a player.
    /// </summary>
    public abstract class OptionDisplay : DialoguePanel
    {
        /// <summary>
        /// A list of images used to create the option display panel.
        /// </summary>
        [Tooltip("The images used for the option display panel.")]
        [NonReorderable]
        [SerializeField]
        protected List<Image> images;

        /// <summary>
        /// Listeners for option display events which are triggered whenever the options are displayed or the selected option changes.
        /// </summary>
        [Tooltip("A set of Option Display Listeners which listen for the list of options to be set or the selected option to change on an option display.")]
        [NonReorderable]
        [SerializeField]
        protected List<OptionDisplayListener> optionDisplayListeners;

        /// <summary>
        /// An event which is triggered when the presented options are updated.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onOptionsSet = new UnityEvent();

        /// <summary>
        /// An event which is triggered when the player selects (highlights) an option.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onOptionSelected = new UnityEvent();

        /// <summary>
        /// An event which is triggered when the player changes the selection from one option to another.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onOptionChanged = new UnityEvent();

        /// <summary>
        /// Initializes the display and hides it immediately.
        /// </summary>
        private void Awake()
        {
            Init();
            HideImmediately();
        }

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();
            onShowComplete.AddListener(ReadyOptions);
        }

        /// <summary>
        /// Make options available and ready for player interactions, reset the display for each option, and highlight the default/selected option.
        /// </summary>
        protected virtual void ReadyOptions() { }

        /// <summary>
        /// Setup the display to show the options specified.
        /// </summary>
        /// <param name="options">The dialogue options the display needs to present.</param>
        public abstract void SetOptions(List<DialogueOption> options);

        /// <summary>
        /// The list of images used to create the option display panel.
        /// </summary>
        public List<Image> Images
        {
            get { return images; }
        }

        /// <summary>
        /// Returns a List containing all of the options buttons of the option display.
        /// </summary>
        /// <returns></returns>
        public abstract List<DialogueButton> GetOptionButtons();

        /// <summary>
        /// Selects the next option (relative to the currently selected one).
        /// </summary>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        public virtual bool SelectNextOption() { return false; }

        /// <summary>
        /// Selects the previous option (relative to the currently selected one).
        /// </summary>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        public virtual bool SelectPreviousOption() { return false; }

        /// <summary>
        /// Selected the option corresponding to the specified direction and returns the option index of it. 
        /// This is typically used for selecting options using a joystick, d-pad, or arrow keys.
        /// </summary>
        /// <param name="direction">The direction to get an option for.</param>
        /// <returns>Returns true if an option was selected; false otherwise.</returns>
        public virtual bool SelectOptionInDirection(Vector2 direction) { return false; }

        /// <summary>
        /// Returns the index of the option currently selected.
        /// </summary>
        /// <returns>The index of the option currently selected.</returns>
        public abstract int GetSelectedOption();

        /// <summary>
        /// This method should be called whenever an option is finalized as the choice the player wants to make. It will call the callback methods
        /// assigned to the onOptionChosen delegate.
        /// </summary>
        protected void OptionChosen()
        {
            if (onOptionChosen != null)
            {
                onOptionChosen.Invoke();
            }
        }

        /// <summary>
        /// Activates the buttons used for selecting options.
        /// </summary>
        public abstract void ActivateButtons();

        /// <summary>
        /// Deactivates the buttons used for selecting options.
        /// </summary>
        public abstract void DeactivateButtons();

        /// <summary>
        /// Calls the OnOptionsSet() method on all Option Display Listeners registered with the Option Display.
        /// </summary>
        /// <param name="options">The List of Dialogue Options set on the Option Display.</param>
        protected virtual void OnOptionsSet(List<DialogueOption> options)
        {
            if(onOptionsSet != null) { onOptionsSet.Invoke(); }

            foreach(OptionDisplayListener listener in optionDisplayListeners)
            {
                listener.OnOptionsSet(options);
            }
        }

        /// <summary>
        /// Calls the OnOptionSelected() method on all Option Display Listeners registered with the Option Display.
        /// </summary>
        /// <param name="option">The Dialogue Option to pass to the OnOptionSelected() method of each listener.</param>
        protected virtual void OptionSelected(DialogueOption option)
        {
            if(onOptionSelected != null) { onOptionSelected.Invoke(); }

            foreach (OptionDisplayListener listener in optionDisplayListeners)
            {
                listener.OnOptionSelected(option);
            }
        }

        /// <summary>
        /// Calls the OnOptionChanged() method on all Option Display Listeners registered with the Option Display.
        /// </summary>
        /// <param name="oldOption">The previously selected Dialogue Option.</param>
        /// <param name="newOption">The newly selected Dialogue Option.</param>
        protected virtual void OptionChanged(DialogueOption oldOption, DialogueOption newOption)
        {
            if(onOptionChanged != null) { onOptionChanged.Invoke(); }

            foreach (OptionDisplayListener listener in optionDisplayListeners)
            {
                listener.OnOptionChanged(oldOption, newOption);
            }
        }

        /// <summary>
        /// Translates the text of each Dialogue Button in the option display based on the current language set on EasyTalkGameState.Instance.Language.
        /// </summary>
        /// <param name="controller">The Dialogue Controller currently being used.</param>
        public virtual void TranslateOptions(DialogueController controller)
        {
            if (controller != null)
            {
                List<DialogueButton> optionButtons = GetOptionButtons();

                foreach (DialogueButton button in optionButtons)
                {
                    DialogueOption option = ((DialogueOption)button.Value);

                    if (option != null)
                    {
                        string text = option.PreTranslationText;
                        text = controller.GetNodeHandler().Translate(text);

                        if(button.StandardText != null)
                        {
                            button.StandardText.text = text;
                        }

#if TEXTMESHPRO_INSTALLED
                        if (button.TMPText != null)
                        {
                            button.TMPText.text = text;
                        }
#endif
                    }
                }
            }
        }
    }
}