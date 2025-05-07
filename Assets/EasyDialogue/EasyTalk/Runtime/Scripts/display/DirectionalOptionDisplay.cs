using System;
using System.Collections.Generic;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// An implementation of a dialogue option display which allows each option to be attributes to a specific Vector2 direction.
    /// </summary>
    public class DirectionalOptionDisplay : OptionDisplay
    {
        /// <summary>
        /// A List of the directional option elements used by the option display. Each element contains a button and a linked image which is manipulated whenever 
        /// an interaction with a option button occurs.
        /// </summary>
        [Tooltip("The Directional Option Elements used by the Option Display. Each option element ties an Option Button to an image which will" +
            "be highlighted as their associated buttons are highlighted, etc..")]
        [SerializeField]
        private List<DirectionalOptionElement> optionElements;

        /// <summary>
        /// The central location of the option display. This location is used to calculate the relative direction to each option unless an option's direction has been overriden.
        /// </summary>
        [Tooltip("The location of the center of the display. It is from this point that the direction to each Directional Option Element is determined." +
            "When input comes from the player in a certain direction, that direction is compared to the direction of each Option Element to determine if that" +
            "is the option which should be selected.")]
        [SerializeField]
        private Transform centerTransform;

        /// <summary>
        /// The main/central image used in the directional display.
        /// </summary>
        [Tooltip("The main image to use for the directional display.")]
        [SerializeField]
        private Image mainImage;

        /// <summary>
        /// If true, the option display's linked images will use the same colors as their corresponding buttons when in normal, pressed, highlighted, and disabled modes.
        /// </summary>
        [Tooltip("If set to true, the images linked to the Option Buttons (via Directional Option Elements) will use the same colors as the Option Buttons when" +
            "options are hovered over, left, pressed, etc..")]
        [SerializeField]
        private bool useOptionButtonColors = true;

        /// <summary>
        /// The color to use on a linked image when in normal mode (only used if useOptionButtonColors is set to false).
        /// </summary>
        [Tooltip("The color to use for a an image linked to an Option Button when the option isn't selected, disabled, or pressed.")]
        [SerializeField]
        [ColorUsage(true, true)]
        private Color linkNormalColor = new Color(0.8f, 0.8f, 0.8f);

        /// <summary>
        /// The color to use on a linked image when in highlighted mode (only used if useOptionButtonColors is set to false).
        /// </summary>
        [Tooltip("The color to use for a an image linked to an Option Button when the option is selected or hovered.")]
        [SerializeField]
        [ColorUsage(true, true)]
        private Color linkHighlightColor = new Color(1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The color to use on a linked image when in pressed mode (only used if useOptionButtonColors is set to false).
        /// </summary>
        [Tooltip("The color to use for a an image linked to an Option Button when the option is pressed.")]
        [SerializeField]
        [ColorUsage(true, true)]
        private Color linkPressedColor = new Color(0.6f, 0.6f, 0.6f);

        /// <summary>
        /// The color to use on a linked image when in disabled mode (only used if useOptionButtonColors is set to false).
        /// </summary>
        [Tooltip("The color to use for a an image linked to an Option Button when the option is disabled")]
        [SerializeField]
        [ColorUsage(true, true)]
        private Color linkDisabledColor = new Color(0.35f, 0.35f, 0.35f);

        /// <summary>
        /// The currently selected option element.
        /// </summary>
        private DirectionalOptionElement currentlySelectedOptionElement;

        /// <summary>
        /// Initializes the option display. This method sets up the option element links to update when the mouse interacts with their corresponding buttons and sets up the 
        /// actions of the buttons when they are clicked.
        /// </summary>
        public override void Init()
        {
            base.Init();

            foreach (DirectionalOptionElement optionElement in optionElements)
            {
                if (optionElement.button != null)
                {
                    SetUpButtonAction(optionElement);

                    if (optionElement.linkedImage != null)
                    {
                        SetUpLinkHighlight(optionElement);
                        SetUpLinkExit(optionElement);
                        SetUpLinkPress(optionElement);
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the dialogue option button associated with the provided DirectionalOptionElement to choose the currently selected option whenever it is clicked.
        /// </summary>
        /// <param name="optionElement">The directional option element to set up.</param>
        private void SetUpButtonAction(DirectionalOptionElement optionElement)
        {
            DialogueButton optionButton = optionElement.button;
            optionButton.onClick.AddListener(
                delegate
                {
                    currentlySelectedOptionElement = optionElement;
                    OptionChosen();
                }
            );
        }

        /// <summary>
        /// Sets up the link image for the provided DirectionalOptionElement to update its color when the associated option button is hovered over by the mouse.
        /// </summary>
        /// <param name="optionElement">The directional option element to set up.</param>
        private void SetUpLinkHighlight(DirectionalOptionElement optionElement)
        {
            optionElement.button.onEnter.AddListener(delegate
            {
                if (optionElement.button.IsClickable)
                {
                    if (currentlySelectedOptionElement != null && currentlySelectedOptionElement.button != null)
                    {
                        currentlySelectedOptionElement.button.DisplayNormal();

                        if (useOptionButtonColors)
                        {
                            currentlySelectedOptionElement.linkedImage.color = optionElement.button.normalButtonColor;
                        }
                        else
                        {
                            currentlySelectedOptionElement.linkedImage.color = LinkNormalColor;
                        }
                    }

                    if (useOptionButtonColors)
                    {
                        optionElement.linkedImage.color = optionElement.button.highlightedButtonColor;
                    }
                    else
                    {
                        optionElement.linkedImage.color = LinkHighlightColor;
                    }

                    optionElement.button.DisplayHighlighted();

                    DialogueOption oldOption = (DialogueOption)currentlySelectedOptionElement.button.Value;
                    DialogueOption newOption = (DialogueOption)optionElement.button.Value;

                    currentlySelectedOptionElement = optionElement;

                    if(oldOption != newOption)
                    {
                        OptionChanged(oldOption, newOption);
                    }

                    OptionSelected(newOption);
                }
            });
        }

        /// <summary>
        /// Sets up the link image for the provided DirectionalOptionElement to update its color when the associated option button is left by the mouse.
        /// </summary>
        /// <param name="optionElement">The directional option element to set up.</param>
        private void SetUpLinkExit(DirectionalOptionElement optionElement)
        {
            optionElement.button.onLeave.AddListener(delegate
            {
                if (optionElement.button.IsClickable)
                {
                    if (useOptionButtonColors)
                    {
                        optionElement.linkedImage.color = optionElement.button.normalButtonColor;
                    }
                    else
                    {
                        optionElement.linkedImage.color = linkNormalColor;
                    }
                }
            });
        }

        /// <summary>
        /// Sets up the link image for the provided DirectionalOptionElement to update its color when the associated option button is pressed.
        /// </summary>
        /// <param name="optionElement">The directional option element to set up.</param>
        private void SetUpLinkPress(DirectionalOptionElement optionElement)
        {
            optionElement.button.onPress.AddListener(delegate
            {
                if (optionElement.button.IsClickable)
                {
                    if (useOptionButtonColors)
                    {
                        optionElement.linkedImage.color = optionElement.button.pressedButtonColor;
                    }
                    else
                    {
                        optionElement.linkedImage.color = linkPressedColor;
                    }
                }
            });
        }

        /// <summary>
        /// Selects the dialogue option with the specified index from the displayed List of options.
        /// </summary>
        /// <param name="index">The index of the option to select.</param>
        /// <param name="playSound">Whether the button's hover sound should be played.</param>
        public void SelectOption(int index, bool playSound = true)
        {
            DialogueOption oldOption = null;
            if (currentlySelectedOptionElement != null)
            {
                oldOption = (DialogueOption)currentlySelectedOptionElement.button.Value;
            }

            currentlySelectedOptionElement = optionElements[index];

            HighlightSelectedOption(playSound);

            DialogueOption newOption = (DialogueOption)currentlySelectedOptionElement.button.Value;

            if (oldOption != newOption)
            {
                OptionChanged(oldOption, newOption);
            }

            OptionSelected(newOption);
        }

        /// <summary>
        /// Highlight the currently selected option.
        /// </summary>
        /// <param name="playSound">Whether the selection sound for the option should be played (default true).</param>
        protected void HighlightSelectedOption(bool playSound = true)
        {
            DialogueButton button = currentlySelectedOptionElement.button;

            if (playSound && button.isActiveAndEnabled)
            {
                button.PlayHoverSound();
            }

            button.DisplayHighlighted();

            if (useOptionButtonColors)
            {
                currentlySelectedOptionElement.linkedImage.color = button.highlightedButtonColor;
            }
            else
            {
                currentlySelectedOptionElement.linkedImage.color = linkHighlightColor;
            }
        }

        /// <summary>
        /// Deselects the option at the specified index (from the List of directional option elements).
        /// </summary>
        /// <param name="index">The index of the option to deselect.</param>
        public void DeselectOption(int index)
        {
            DirectionalOptionElement element = optionElements[index];
            DialogueButton button = optionElements[index].button;

            if (button.IsClickable)
            {
                button.DisplayNormal();
            }
            else
            {
                button.DisplayDisabled();
            }

            if (useOptionButtonColors)
            {
                if (button.IsClickable)
                {
                    element.linkedImage.color = button.normalButtonColor;
                }
                else
                {
                    element.linkedImage.color = button.disabledButtonColor;
                }
            }
            else
            {
                if (button.IsClickable)
                {
                    element.linkedImage.color = linkNormalColor;
                }
                else
                {
                    element.linkedImage.color = linkDisabledColor;
                }
            }
        }

        /// <summary>
        /// Returns the directional option element which is currently selected.
        /// </summary>
        /// <returns>The currently selected directional option element.</returns>
        public DirectionalOptionElement GetSelectedOptionElement()
        {
            return currentlySelectedOptionElement;
        }

        /// <summary>
        /// Returns the link image at the specified index.
        /// </summary>
        /// <param name="index">The index of the link image to retrieve.</param>
        /// <returns>The link image configured for the directional option element at the specified index.</returns>
        public Image GetLinkImage(int index)
        {
            return optionElements[index].linkedImage;
        }

        /// <summary>
        /// Returns the dialogue button at the specified index.
        /// </summary>
        /// <param name="index">The index of the button to retrieve.</param>
        /// <returns>The dialogue button configured for the directional option element at the specified index.</returns>
        public DialogueButton GetButton(int index)
        {
            return optionElements[index].button;
        }

        /// <summary>
        /// Gets the List of DirectionalOptionElements containing the dialogue option buttons and linked images.
        /// </summary>
        public List<DirectionalOptionElement> OptionElements { get { return optionElements; } }

        /// <inheritdoc/>
        public override bool SelectOptionInDirection(Vector2 direction)
        {
            float currentAngle = float.MaxValue;
            DirectionalOptionElement chosenButtonLinkElement = null;

            //Find the angle between the center image and each option, choose whichever one has the smallest angle.
            for (int i = 0; i < OptionElements.Count; i++)
            {
                DirectionalOptionElement element = OptionElements[i];
                DialogueButton button = element.button;

                if (button.isActiveAndEnabled && button.IsClickable)
                {
                    Vector2 buttonDirection = Vector2.zero;

                    if (element.useCustomDirectionVector)
                    {
                        buttonDirection = element.directionVector;
                    }
                    else
                    {
                        buttonDirection = button.transform.position - centerTransform.transform.position;
                    }

                    float buttonAngle = Vector2.Angle(direction, buttonDirection);
                    if (buttonAngle < currentAngle)
                    {
                        currentAngle = buttonAngle;
                        chosenButtonLinkElement = element;
                    }
                }
            }

            if (chosenButtonLinkElement != currentlySelectedOptionElement)
            {
                if (currentlySelectedOptionElement != null)
                {
                    int idx = OptionElements.IndexOf(currentlySelectedOptionElement);
                    DeselectOption(idx);
                }

                if (chosenButtonLinkElement != null)
                {
                    int idx = OptionElements.IndexOf(chosenButtonLinkElement);
                    SelectOption(idx);
                }
            }

            if (currentlySelectedOptionElement != null)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override void SetOptions(List<DialogueOption> options)
        {
            OnOptionsSet(options);

            int buttonIdx = 0;
            bool isOptionSelected = false;

            for (int i = 0; i < OptionElements.Count; i++)
            {
                DirectionalOptionElement currentElement = OptionElements[i];

                bool isButtonActive = false;
                
                if(currentElement.activationMask.Count >= options.Count)
                {
                    isButtonActive = currentElement.activationMask[options.Count - 1];
                }

                DialogueButton optionButton = currentElement.button;
                optionButton.gameObject.SetActive(isButtonActive);

                Image linkedImage = currentElement.linkedImage;
                linkedImage.gameObject.SetActive(isButtonActive);

                if (isButtonActive)
                {
                    DialogueOption currentOption = options[buttonIdx];
                    optionButton.Value = currentOption;
                    optionButton.Interactable = false;
                    optionButton.IsClickable = currentOption.IsSelectable;
                    optionButton.SetText(currentOption.OptionText);

                    buttonIdx++;

                    if (!isOptionSelected && currentOption.IsSelectable)
                    {
                        isOptionSelected = true;
                        currentlySelectedOptionElement = optionElements[i];
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void ReadyOptions()
        {
            for (int i = 0; i < optionElements.Count; i++)
            {
                if (optionElements[i].button.gameObject.activeSelf)
                {
                    optionElements[i].button.Interactable = true;
                    optionElements[i].button.DisplayNormal();
                }
            }

            if (currentlySelectedOptionElement != null)
            {
                OptionSelected(currentlySelectedOptionElement.button.Value as DialogueOption);
            }

            HighlightSelectedOption(true);
        }

        /// <inheritdoc/>
        public override int GetSelectedOption()
        {
            return ((DialogueOption)currentlySelectedOptionElement.button.Value).OptionIndex;
        }

        /// <inheritdoc/>
        public override List<DialogueButton> GetOptionButtons()
        {
            List<DialogueButton> buttons = new List<DialogueButton>();
            foreach (DirectionalOptionElement element in OptionElements)
            {
                buttons.Add(element.button);
            }

            return buttons;
        }

        /// <inheritdoc/>
        public override void ActivateButtons()
        {
            foreach (DirectionalOptionElement element in optionElements)
            {
                element.button.Interactable = true;
            }
        }

        /// <inheritdoc/>
        public override void DeactivateButtons()
        {
            foreach (DirectionalOptionElement element in optionElements)
            {
                element.button.Interactable = false;
            }
        }

        /// <summary>
        /// Gets or sets the center transform used by the directional option display.
        /// </summary>
        public Transform CenterTransform
        {
            get { return this.centerTransform; }
            set { this.centerTransform = value; }
        }

        /// <summary>
        /// Gets or sets the link images used for each option element.
        /// </summary>
        public List<Image> LinkedImages
        {
            get
            {
                List<Image> images = new List<Image>();
                foreach(DirectionalOptionElement element in optionElements)
                {
                    images.Add(element.linkedImage);
                }
                return images;
            }

            set
            {
                for(int i = 0; i < value.Count; i++)
                {
                    Image currentImage = value[i];
                    if(optionElements.Count > i)
                    {
                        optionElements[i].linkedImage = currentImage;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the directional option display's option element link images should inherit their colors from the buttons or not.
        /// </summary>
        public bool UseOptionButtonColors
        {
            get { return this.useOptionButtonColors; }
            set { this.useOptionButtonColors = value; }
        }

        /// <summary>
        /// Gets or sets the color to use for option element link images when their corresponding button is in 'normal' mode (only applicable when useOptionButtonColors is false).
        /// </summary>
        public Color LinkNormalColor
        {
            get { return this.linkNormalColor; }
            set { this.linkNormalColor = value; }
        }

        /// <summary>
        /// Gets or sets the color to use for option element link images when their corresponding button is in 'highlighted' mode (only applicable when useOptionButtonColors is false).
        /// </summary>
        public Color LinkHighlightColor
        {
            get { return this.linkHighlightColor; }
            set { this.linkHighlightColor = value; }
        }

        /// <summary>
        /// Gets or sets the color to use for option element link images when their corresponding button is in 'disabled' mode (only applicable when useOptionButtonColors is false).
        /// </summary>
        public Color LinkDisabledColor
        {
            get { return this.linkDisabledColor; }
            set { this.linkDisabledColor = value; }
        }

        /// <summary>
        /// Gets or sets the color to use for option element link images when their corresponding button is in 'pressed' mode (only applicable when useOptionButtonColors is false).
        /// </summary>
        public Color LinkPressedColor
        {
            get { return this.linkPressedColor; }
            set { this.linkPressedColor = value; }
        }

        /// <summary>
        /// Gets or sets the main/central image used by the directional option display.
        /// </summary>
        public Image MainImage
        {
            get { return this.mainImage; }
            set { this.mainImage = value; }
        }
    }

    /// <summary>
    /// A Directional Option Element establishes a link between a button and another image. When the button is highlighted, the corresponding image will be highlighted, etc.. In 
    /// addition, the element allows for a custom direction to be defined for the element to be selected.
    /// </summary>
    [Serializable]
    public class DirectionalOptionElement
    {
        /// <summary>
        /// The button to use.
        /// </summary>
        [Tooltip("The Option Button used for this element.")]
        [SerializeField]
        public DialogueButton button;

        /// <summary>
        /// The linked image.
        /// </summary>
        [Tooltip("The image linked to the Option Button. It will be highlighted when the Option Button is hovered over, etc..")]
        [SerializeField]
        public Image linkedImage;

        /// <summary>
        /// Whether the option should be attributed to a custom direction vector rather than being based on the relative location of the option.
        /// </summary>
        [Tooltip("If true, allows a custom direction vector to be used for activating/choosing the option associated with this element.")]
        [SerializeField]
        public bool useCustomDirectionVector = false;

        /// <summary>
        /// The custom direction vector to use when useCustomDirectionVector is set to true.
        /// </summary>
        [Tooltip("The direction vector to compare to player input to see if this is the chosen option element.")]
        [SerializeField]
        public Vector3 directionVector = Vector3.zero;

        /// <summary>
        /// The mask which determines when the option element is shown/used based on the number of options being presented.
        /// </summary>
        [Tooltip("This mask determines when the button/link is activated based on the number of options being presented. For example, if 2 options are presented and the second" +
            " toggle is checked, this element will be activated whenever 2 options are presented, but if the second toggle isn't checked, this option element will not be used.")]
        [SerializeField]
        public List<bool> activationMask = new List<bool>();
    }
}