using System.Collections;
using System.Collections.Generic;
using System.Data;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;

namespace EasyTalk.Display
{
    /// <summary>
    /// The OptionListDisplay is an implementation of an Option Display which displays an aligned collection of dialogue option buttons to the player.
    /// </summary>
    public class OptionListDisplay : OptionDisplay
    {
        /// <summary>
        /// The List of option buttons used to display dialogue options.
        /// </summary>
        [Tooltip("The list of Option Buttons available in the Option Display.")]
        [SerializeField]
        protected List<DialogueButton> optionButtons = new List<DialogueButton>();

        /// <summary>
        /// Whether the next/previous option selection controls should be reversed when selecting options.
        /// </summary>
        [Tooltip("If set to true, player input controls will be reversed for option selection, e.g. " +
            "down will select the previous choice rather than the next (as is normal), etc..")]
        [SerializeField]
        protected bool reverseControls = false;

        /// <summary>
        /// Whether new option buttons should be dynamically created if the number of options set on the display exceeds the number of option buttons currently available.
        /// </summary>
        [Tooltip("If set to true, new option buttons will be automatically created as needed by cloning the first button.")]
        [SerializeField]
        protected bool isDynamic = true;

        /// <summary>
        /// The index of the currently selected option button.
        /// </summary>
        protected int selectedButtonIdx = 0;

        /// <summary>
        /// 
        /// </summary>
        protected DialogueOption selectedOption;

        /// <summary>
        /// Initializes the display and sets up the buttons' click actions.
        /// </summary>
        public override void Init()
        {
            base.Init();

            foreach (DialogueButton optionButton in optionButtons)
            {
                optionButton.onClick.AddListener(
                    delegate
                    {
                        selectedOption = ((DialogueOption)optionButton.Value);
                        selectedButtonIdx = optionButtons.IndexOf(optionButton);
                        OptionChosen();
                    }
                );

                optionButton.onEnter.AddListener(
                    delegate
                    {
                        if (optionButton.IsClickable)
                        {
                            selectedOption = ((DialogueOption)optionButton.Value);
                            DialogueOption oldOption = ((DialogueOption)optionButtons[selectedButtonIdx].Value);

                            int newSelectedButtonIdx = optionButtons.IndexOf(optionButton);

                            if (newSelectedButtonIdx != selectedButtonIdx)
                            {
                                optionButtons[selectedButtonIdx].DisplayNormal();
                            }

                            selectedButtonIdx = newSelectedButtonIdx;

                            foreach (OptionDisplayListener listener in optionDisplayListeners)
                            {
                                listener.OnOptionChanged(oldOption, selectedOption);
                            }

                            OptionSelected(selectedOption);
                        }
                    }
                );
            }
        }

        /// <inheritdoc/>
        protected override void ReadyOptions()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                if (optionButtons[i].gameObject.activeSelf)
                {
                    optionButtons[i].Interactable = true;
                    optionButtons[i].DisplayNormal();
                }
            }

            if (selectedOption != null)
            {
                OptionSelected(selectedOption);
            }

            StartCoroutine(HighlightOptionAfterDelay());
        }

        /// <inheritdoc/>
        public override int GetSelectedOption()
        {
            return selectedOption.OptionIndex;
        }

        /// <inheritdoc/>
        public override void SetOptions(List<DialogueOption> options)
        {
            OnOptionsSet(options);

            int firstOption = -1;

            for (int i = 0; i < options.Count; i++)
            {
                if (i > optionButtons.Count - 1)
                {
                    if (isDynamic)
                    {
                        CreateNewOptionButton();
                    }
                    else { break; }
                }

                DialogueOption currentOption = options[i];

                DialogueButton optionButton = optionButtons[i];
                optionButton.Value = currentOption;
                optionButton.Interactable = false;
                optionButton.IsClickable = currentOption.IsSelectable;
                optionButton.SetText(currentOption.OptionText);

                if (currentOption.IsSelectable)
                {
                    //optionButton.DisplayNormal();
                    if (firstOption < 0) 
                    { 
                        firstOption = i;
                        selectedOption = currentOption;
                    }
                }

                optionButton.gameObject.SetActive(true);
            }

            for (int i = options.Count; i < optionButtons.Count; i++)
            {
                optionButtons[i].gameObject.SetActive(false);
            }

            selectedButtonIdx = firstOption;
        }

        /// <summary>
        /// Creates a new dialogue option button (cloned from the first button) and adds it to the display.
        /// </summary>
        /// <returns>The new DialogueButton.</returns>
        protected DialogueButton CreateNewOptionButton()
        {
            GameObject newOptionButtonObject = GameObject.Instantiate(optionButtons[0].gameObject, optionButtons[0].transform.parent);
            DialogueButton optionButton = newOptionButtonObject.GetComponent<DialogueButton>();

            optionButtons.Add(optionButton);

            optionButton.onClick.AddListener(
                delegate
                {
                    selectedOption = ((DialogueOption)optionButton.Value);
                    selectedButtonIdx = optionButtons.IndexOf(optionButton);
                    OptionChosen();
                }
            );

            return optionButton;
        }

        /// <summary>
        /// Asynchronously highlights the selected option after a single frame delay.
        /// </summary>
        /// <returns></returns>
        private IEnumerator HighlightOptionAfterDelay()
        {
            yield return new WaitForEndOfFrame();
            optionButtons[selectedButtonIdx].DisplayHighlighted();
        }

        /// <inheritdoc/>
        public override bool SelectNextOption()
        {
            int originalIdx = selectedButtonIdx;
            optionButtons[selectedButtonIdx].DisplayNormal();

            DialogueOption oldOption = selectedOption;

            do
            {
                if (reverseControls)
                {
                    selectedButtonIdx--;
                    if (selectedButtonIdx < 0)
                    {
                        selectedButtonIdx = optionButtons.Count - 1;
                    }
                }
                else
                {
                    selectedButtonIdx++;
                    if (selectedButtonIdx >= optionButtons.Count)
                    {
                        selectedButtonIdx = 0;
                    }
                }

                if (selectedButtonIdx == originalIdx) { break; }
            }
            while (!optionButtons[selectedButtonIdx].isActiveAndEnabled || !optionButtons[selectedButtonIdx].IsClickable);

            selectedOption = ((DialogueOption)optionButtons[selectedButtonIdx].Value);
            OptionSelected(selectedOption);

            if(oldOption != selectedOption)
            {
                OptionChanged(oldOption, selectedOption);
            }

            optionButtons[selectedButtonIdx].DisplayHighlighted();
            optionButtons[selectedButtonIdx].PlayHoverSound();

            return true;
        }

        /// <inheritdoc/>
        public override bool SelectPreviousOption()
        {
            int originalIdx = selectedButtonIdx;
            optionButtons[selectedButtonIdx].DisplayNormal();

            DialogueOption oldOption = selectedOption;

            do
            {
                if (reverseControls)
                {
                    selectedButtonIdx++;
                    if (selectedButtonIdx >= optionButtons.Count)
                    {
                        selectedButtonIdx = 0;
                    }
                }
                else
                {
                    selectedButtonIdx--;
                    if (selectedButtonIdx < 0)
                    {
                        selectedButtonIdx = optionButtons.Count - 1;
                    }
                }

                if (selectedButtonIdx == originalIdx) { break; }
            }
            while (!optionButtons[selectedButtonIdx].isActiveAndEnabled || !optionButtons[selectedButtonIdx].IsClickable);

            selectedOption = ((DialogueOption)optionButtons[selectedButtonIdx].Value);
            OptionSelected(selectedOption);

            if (oldOption != selectedOption)
            {
                OptionChanged(oldOption, selectedOption);
            }

            optionButtons[selectedButtonIdx].DisplayHighlighted();
            optionButtons[selectedButtonIdx].PlayHoverSound();

            return true;
        }

        /// <inheritdoc/>
        public override List<DialogueButton> GetOptionButtons()
        {
            return optionButtons;
        }

        /// <inheritdoc/>
        public override void ActivateButtons()
        {
            foreach(DialogueButton button in optionButtons)
            {
                button.Interactable = true;
            }
        }

        /// <inheritdoc/>
        public override void DeactivateButtons()
        {
            foreach (DialogueButton button in optionButtons)
            {
                button.Interactable = false;
            }
        }
    }
}