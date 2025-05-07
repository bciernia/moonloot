using System.Collections;
using System.Collections.Generic;
using System.Transactions;


#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// The ScrollableOptionDisplay is an implementation of an OptionListDisplay which is scrollable, so that it can display a virtually unlimited number of options.
    /// </summary>
    public class ScrollableOptionDisplay : OptionListDisplay
    {
        /// <summary>
        /// The scrollable content container which contains the dialogue option buttons.
        /// </summary>
        [Tooltip("This should be set to the scrollable content container containing the option buttons.")]
        [SerializeField]
        private GameObject scrollableContentContainer;

        /// <summary>
        /// The number of options currently being displayed.
        /// </summary>
        private int numOptionsAvailable = 1;

        /// <inheritdoc/>
        public override void SetOptions(List<DialogueOption> options)
        {
            OnOptionsSet(options);

            numOptionsAvailable = 0;
            int firstOption = -1;

            //Setup all of the option buttons to display to the player.
            for (int i = 0; i < options.Count; i++)
            {
                DialogueOption currentOption = options[i];

                if (i >= optionButtons.Count)
                {
                    if (isDynamic)
                    {
                        CreateNewOptionButton();
                    }
                    else { break; }
                }

                DialogueButton optionButton = optionButtons[i];
                optionButton.gameObject.SetActive(true);
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

                //Add the size of the button to the total scrollable height.
                RectTransform buttonTransform = optionButton.GetComponent<RectTransform>();
                numOptionsAvailable++;
            }

            //Deactivate unused option buttons.
            for (int i = options.Count; i < optionButtons.Count; i++)
            {
                DialogueButton button = optionButtons[i];
                button.gameObject.SetActive(false);
            }

            //Select the first option.
            selectedButtonIdx = firstOption;
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

            StartCoroutine(ScrollToSelectedOptionAfterDelay());
        }

        /// <summary>
        /// Asynchronously scrolls the scrollview to view the currently selected option after one frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ScrollToSelectedOptionAfterDelay()
        {
            yield return new WaitForEndOfFrame();
            optionButtons[selectedButtonIdx].DisplayHighlighted();
            ScrollToSelectedOption();
        }

        /// <inheritdoc/>
        public override bool SelectNextOption()
        {
            bool optionWasSelected = base.SelectNextOption();
            ScrollToSelectedOption();

            return optionWasSelected;
        }

        /// <inheritdoc/>
        public override bool SelectPreviousOption()
        {
            bool optionWasSelected = base.SelectPreviousOption();
            ScrollToSelectedOption();

            return optionWasSelected;
        }

        /// <summary>
        /// Scroll to the currently selected option.
        /// </summary>
        protected void ScrollToSelectedOption()
        {
            DialogueButton currentButton = optionButtons[selectedButtonIdx];
            RectTransform buttonRect = currentButton.GetComponent<RectTransform>();
            RectTransform scrollableRect = scrollableContentContainer.GetComponent<RectTransform>();

            //Get the world position corners for the selected option and the scrollview.
            Vector3[] buttonCorners = new Vector3[4];
            buttonRect.GetWorldCorners(buttonCorners);
            Vector3[] scrollableCorners = new Vector3[4];
            scrollableRect.GetWorldCorners(scrollableCorners);

            //Calculate the button position (between the top of the button and the bottom of the button) based on where it is at in the list/scrollview.
            float buttonPosition = Mathf.Lerp(buttonCorners[1].y, buttonCorners[0].y, ((float)selectedButtonIdx) / (numOptionsAvailable - 1.0f));

            //Calculate the normalized position of the button based on where it is relative to the entire size of the scrollview.
            float normalizedPositionOfOption = (buttonPosition - scrollableCorners[0].y) / (scrollableCorners[1].y - scrollableCorners[0].y);

            //Adjust the scrollview position to view the selected option.
            this.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = normalizedPositionOfOption;
        }
    }
}