#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// This class is an implementation of a continue display, used to indicate to the player when they can continue during playback of dialogue.
    /// </summary>
    public class ContinueDisplay : DialoguePanel
    {
        /// <summary>
        /// The background image of the continue display.
        /// </summary>
        [Tooltip("The background image of the Continue Display.")]
        [SerializeField]
        private Image backgroundImage;


    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text of the continue display.
        /// </summary>
        [Tooltip("The text element of the Continue Display.")]
        [SerializeField]
        private TMP_Text textMeshProText;
    #endif

        /// <summary>
        /// The text of the continue display.
        /// </summary>
        [Tooltip("The text element of the Continue Display.")]
        [SerializeField]
        private Text text;

        /// <summary>
        /// Initializes and hides the display on awake.
        /// </summary>
        private void Awake()
        {
            Init();
            HideImmediately();            
        }

        /// <summary>
        /// Resets the button to normal display mode.
        /// </summary>
        private void ResetButton()
        {
            DialogueButton continueButton = GetComponent<DialogueButton>();
            if (continueButton != null) { continueButton.DisplayNormal(); }
        }

        /// <summary>
        /// If the continue display has a DialogueButton component, calling this method will make it interactable so that the player can interact with it with the mouse.
        /// </summary>
        private void EnableButton()
        {
            DialogueButton continueButton = GetComponent<DialogueButton>();
            if (continueButton != null) { continueButton.Interactable = true; }
        }

        /// <summary>
        /// If the continue display has a DialogueButton component, calling this method will make it non-interactable so that it can not be interacted with by the player/mouse.
        /// </summary>
        private void DisableButton()
        {
            DialogueButton continueButton = GetComponent<DialogueButton>();
            if (continueButton != null) { continueButton.Interactable = false; }
        }

        /// <summary>
        /// Called whenever the display finishes being shown. IF the continue display has a DialogueButton component, it is set to be interactable, meaning the player can click on
        /// or interact with the button.
        /// </summary>
        protected override void OnShowComplete()
        {
            base.OnShowComplete();
            EnableButton();
        }

        /// <summary>
        /// Called whenever the display is being hidden. If the continue display has a DialogueButton component, it is set to be non-interactable, meaning the player cannot click on
        /// or interact with the button.
        /// </summary>
        protected override void OnHideStart()
        {
            base.OnHideStart();
            DisableButton();
        }

        /// <summary>
        /// Gets or sets the background image of the display.
        /// </summary>
        public Image BackgroundImage
        {
            get { return backgroundImage; }
            set { backgroundImage = value; }
        }

        /// <summary>
        /// Gets or sets the standard Unity Text component of the display.
        /// </summary>
        public Text StandardText 
        { 
            get { return this.text; } 
            set { this.text = value; }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Gets or sets the TextMeshPro Text component of the display.
        /// </summary>
        public TMP_Text TMPText
        {
            get { return textMeshProText; }
            set { textMeshProText = value; }
        }
#endif

        private void OnValidate()
        {
#if TEXTMESHPRO_INSTALLED
            if (this.TMPText == null)
            {
                TMP_Text[] tmpTextComponents = GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text txt in tmpTextComponents)
                {
                    if (txt.gameObject.name.Contains("ContinueText") && txt.gameObject.name.Contains("TMP"))
                    {
                        this.TMPText = txt;
                        break;
                    }
                }
            }
#endif
        }
    }
}
