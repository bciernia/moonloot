using System.Collections;
using System.Collections.Generic;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// This is an implementation of a button component used by the dialogue display system.
    /// </summary>
    public class DialogueButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {

    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text element of the button.
        /// </summary>
        [Tooltip("The text element used to display option text on the button.")]
        [SerializeField]
        private TMP_Text textMeshProText;
    #endif

        /// <summary>
        /// The text element of the button.
        /// </summary>
        [Tooltip("The text element used to display option text on the button.")]
        [SerializeField]
        private Text text;

        /// <summary>
        /// The color of the text when the button is in 'normal' mode.
        /// </summary>
        [Tooltip("The color to use for the button's text when the button isn't selected/highlighted, disabled, or pressed.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color normalTextColor = new Color(0.85f, 0.85f, 0.85f);

        /// <summary>
        /// The color of the text when the button is in 'disabled' mode.
        /// </summary>
        [Tooltip("The color to use for the button's text when the button is disabled.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color disabledTextColor = new Color(0.4f, 0.4f, 0.4f);

        /// <summary>
        /// The color of the text when the button is in 'highlighted' mode.
        /// </summary>
        [Tooltip("The color to use for the button's text when the button is selected/highlighted.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color highlightedTextColor = new Color(1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The color of the text when the button is in 'pressed' mode.
        /// </summary>
        [Tooltip("The color to use for the button's text when the button is pressed.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color pressedTextColor = new Color(0.7f, 0.7f, 0.7f);

        /// <summary>
        /// The image used for the button.
        /// </summary>
        [Tooltip("The background image of button.")]
        [SerializeField]
        public Image backgroundImage;

        /// <summary>
        /// The color of the image when the button is in 'normal' mode.
        /// </summary>
        [Tooltip("The color to use for the button's image when the button isn't selected/highlighted, disabled, or pressed.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color normalButtonColor = new Color(0.3f, 0.3f, 0.3f);

        /// <summary>
        /// The color of the image when the button is in 'disabled' mode.
        /// </summary>
        [Tooltip("The color to use for the button's image when the button is disabled.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color disabledButtonColor = new Color(0.15f, 0.15f, 0.15f);

        /// <summary>
        /// The color of the image when the button is in 'highlighted' mode.
        /// </summary>
        [Tooltip("The color to use for the button's image when the button is selected/highlighted.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color highlightedButtonColor = new Color(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// The color of the image when the button is in 'pressed' mode.
        /// </summary>
        [Tooltip("The color to use for the button's image when the button is pressed.")]
        [SerializeField]
        [ColorUsage(true)]
        public Color pressedButtonColor = new Color(0.25f, 0.25f, 0.25f);

        /// <summary>
        /// The sound to play when the player hovers over the button.
        /// </summary>
        [Tooltip("The sound to play whenever the player hovers over the button.")]
        [SerializeField]
        public AudioClip hoverSound;

        /// <summary>
        /// The audio source used by the button.
        /// </summary>
        [Tooltip("The Audio Source used by the button. If unset, the button will attempt to find an Audio Source on its parent.")]
        [SerializeField]
        private AudioSource audioSource;

        /// <summary>
        /// A Unity Event that gets triggered when the button is clicked.
        /// </summary>
        [Tooltip("Callback triggered whenever the button is clicked.")]
        [SerializeField]
        public UnityEvent onClick = new UnityEvent();

        /// <summary>
        /// A Unity Event that gets triggered when the button is entered by the mouse.
        /// </summary>
        [Tooltip("Callback triggered whenever the button is entered by the mouse.")]
        [SerializeField]
        public UnityEvent onEnter = new UnityEvent();

        /// <summary>
        /// A Unity Event that gets triggered when the button is left by the mouse.
        /// </summary>
        [Tooltip("Callback triggered whenever the button is left by the mouse.")]
        [SerializeField]
        public UnityEvent onLeave = new UnityEvent();

        /// <summary>
        /// A Unity Event that gets triggered when the button is pressed down.
        /// </summary>
        [Tooltip("Callback triggered whenever the button is pressed.")]
        [SerializeField]
        public UnityEvent onPress = new UnityEvent();

        /// <summary>
        /// A Unity Event that gets triggered when the button goes into 'highlighted' mode.
        /// </summary>
        [Tooltip("Callback triggered whenever the button goes into 'highlighted' mode.")]
        [SerializeField]
        public UnityEvent onHighlighted = new UnityEvent();

        /// <summary>
        /// A Unity Event that gets triggered when the button goes into 'normal' mode.
        /// </summary>
        [Tooltip("Callback triggered whenever the button goes back into 'normal' mode.")]
        [SerializeField]
        public UnityEvent onNormal = new UnityEvent();

        /// <summary>
        /// Whether the button is clickable.
        /// </summary>
        private bool isClickable = true;

        /// <summary>
        /// A flag indicating whether the button is ready for a click to be completed (set to true when the button is pressed and the mouse is in the button's area).
        /// </summary>
        private bool isClickReady = false;

        /// <summary>
        /// A value attributed to the button.
        /// </summary>
        private object value;

        /// <summary>
        /// A collection of temporary audio sources used to queue up sounds to prevent hard cutoffs.
        /// </summary>
        private List<AudioSource> onDemandAudioSources = new List<AudioSource>();

        /// <summary>
        /// The maximum number of audio sources the button is allowed to create at once.
        /// </summary>
        private int maxAudioSources = 4;

        /// <summary>
        /// The index of the next available audio source.
        /// </summary>
        private int currentSourceIdx = 0;

        /// <summary>
        /// A flag which controls whether the button is interactable.
        /// </summary>
        private bool isInteractable = true;

        /// <summary>
        /// A flag used to prevent butons from being re-highlighted after a button press when the mouse is still on the button.
        /// </summary>
        private bool allowHighlight = true;

        /// <summary>
        /// Sets up various callbacks for mouse interactions.
        /// </summary>
        private void Awake()
        {
            onEnter.AddListener(DisplayHighlighted);
            onEnter.AddListener(PlayHoverSound);
            onLeave.AddListener(DisplayNormal);
            onPress.AddListener(DisplayPressed);
            onClick.AddListener(DisplayHighlighted);
        }

        /// <summary>
        /// A flag which controls whether the button is interactable.
        /// </summary>
        public bool Interactable
        {
            get { return this.isInteractable; }
            set 
            { 
                this.isInteractable = value; 

                if(StandardText != null)
                {
                    StandardText.raycastTarget = value;
                }

#if TEXTMESHPRO_INSTALLED
                if (TMPText != null)
                {
                    TMPText.raycastTarget = value;
                }
#endif

                if (backgroundImage != null)
                {
                    backgroundImage.raycastTarget = value;
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer is released over the button.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isClickReady || !isInteractable) { return; }

            if (isClickable && onClick != null)
            {
                onClick.Invoke();
            }

            isClickReady = false;
        }

        /// <summary>
        /// Called when the mouse is pressed over the button.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if(!isInteractable) { return; }

            isClickReady = true;
            allowHighlight = false;

            if (isClickable && onPress != null)
            {
                onPress.Invoke();
            }
        }

        /// <summary>
        /// Called when the mouse moves over the button.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!isInteractable) { return; }

            allowHighlight = true;

            if (isClickable && onEnter != null)
            {
                onEnter.Invoke();
            }
        }

        /// <summary>
        /// Called when the mouse leaves the button.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if(!isInteractable) { return; }

            isClickReady = false;
            allowHighlight = true;

            if (isClickable && onLeave != null)
            {
                onLeave.Invoke();
            }
        }

        /// <summary>
        /// Plays the hover sound set on the button.
        /// </summary>
        public void PlayHoverSound()
        {
            if (hoverSound != null && isClickable)
            {
                if (audioSource == null)
                {
                    audioSource = this.gameObject.AddComponent<AudioSource>();
                }

                if(audioSource == null) { return; }

                if (audioSource.isPlaying)
                {
                    AudioSource tempAudioSource;

                    if (onDemandAudioSources.Count < maxAudioSources)
                    {
                        tempAudioSource = this.gameObject.AddComponent<AudioSource>();
                        tempAudioSource.volume = audioSource.volume;
                        tempAudioSource.pitch = audioSource.pitch;
                        tempAudioSource.panStereo = audioSource.panStereo;
                        tempAudioSource.mute = audioSource.mute;
                        tempAudioSource.velocityUpdateMode = audioSource.velocityUpdateMode;
                        tempAudioSource.dopplerLevel = audioSource.dopplerLevel;
                        tempAudioSource.loop = audioSource.loop;
                        onDemandAudioSources.Add(tempAudioSource);
                    }
                    else
                    {
                        tempAudioSource = onDemandAudioSources[currentSourceIdx];

                        currentSourceIdx++;
                        if (currentSourceIdx >= maxAudioSources)
                        {
                            currentSourceIdx = 0;
                        }
                    }

                    StartCoroutine(PlaySoundAsync(tempAudioSource, hoverSound));
                }
                else
                {
                    audioSource.clip = hoverSound;
                    audioSource.Play();
                }
            }
        }

        /// <summary>
        /// Plays the provided sound asynchronously and removes the audio source from the on demand queue once the sound is finished playing.
        /// </summary>
        /// <param name="source">The audio source to use.</param>
        /// <param name="clip">The audio clip to play.</param>
        /// <returns></returns>
        private IEnumerator PlaySoundAsync(AudioSource source, AudioClip clip)
        {
            source.clip = hoverSound;
            source.Play();

            yield return new WaitForSeconds(clip.length);
            onDemandAudioSources.Remove(source);
            Destroy(source);
        }

        /// <summary>
        /// Sets the button to display itself in highlighted mode.
        /// </summary>
        public void DisplayHighlighted()
        {
            if(!allowHighlight) 
            { 
                return;
            }

            EventSystem eventSystem = null;

#if UNITY_2022_3_OR_NEWER
            eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
#else
            eventSystem = GameObject.FindObjectOfType<EventSystem>();
#endif

            if(eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(this.gameObject);
            }

            if (StandardText != null)
            {
                StandardText.color = new Color(highlightedTextColor.r, highlightedTextColor.g, highlightedTextColor.b, highlightedTextColor.a);
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPText != null)
            {
                TMPText.color = new Color(highlightedTextColor.r, highlightedTextColor.g, highlightedTextColor.b, highlightedTextColor.a);
            }
#endif

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(highlightedButtonColor.r, highlightedButtonColor.g, highlightedButtonColor.b, highlightedButtonColor.a);
            }

            if(onHighlighted != null)
            {
                onHighlighted.Invoke();
            }
        }

        /// <summary>
        /// Sets the button to display itself in normal mode.
        /// </summary>
        public void DisplayNormal()
        {
            if (!isClickable)
            {
                DisplayDisabled();
                return;
            }

            if (StandardText != null)
            {
                StandardText.color = new Color(normalTextColor.r, normalTextColor.g, normalTextColor.b, normalTextColor.a);
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPText != null)
            {
                TMPText.color = new Color(normalTextColor.r, normalTextColor.g, normalTextColor.b, normalTextColor.a);
            }
#endif

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(normalButtonColor.r, normalButtonColor.g, normalButtonColor.b, normalButtonColor.a);
            }

            if(onNormal != null)
            {
                onNormal.Invoke();
            }
        }

        /// <summary>
        /// Sets the button to display itself in pressed mode.
        /// </summary>
        public void DisplayPressed()
        {
            if (StandardText != null)
            {
                StandardText.color = new Color(pressedTextColor.r, pressedTextColor.g, pressedTextColor.b, pressedTextColor.a);
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPText != null)
            {
                TMPText.color = new Color(pressedTextColor.r, pressedTextColor.g, pressedTextColor.b, pressedTextColor.a);
            }
#endif

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(pressedButtonColor.r, pressedButtonColor.g, pressedButtonColor.b, pressedButtonColor.a);
            }
        }

        /// <summary>
        /// Sets the button to display itself in disabled mode.
        /// </summary>
        public void DisplayDisabled()
        {
            if (StandardText != null)
            {
                StandardText.color = new Color(disabledTextColor.r, disabledTextColor.g, disabledTextColor.b, disabledTextColor.a);
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPText != null)
            {
                TMPText.color = new Color(disabledTextColor.r, disabledTextColor.g, disabledTextColor.b, disabledTextColor.a);
            }
#endif

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(disabledButtonColor.r, disabledButtonColor.g, disabledButtonColor.b, disabledButtonColor.a);
            }
        }

        /// <summary>
        /// Whether the button is clickable. Returns false if the button is disabled. Setting this value to true will change the button to display in normal mode, and setting it to
        /// false will set it to display in disabled mode.
        /// </summary>
        public bool IsClickable
        {
            get { return this.isClickable; }
            set
            {
                this.isClickable = value;
                if (this.isClickable)
                {
                    DisplayNormal();
                }
                else
                {
                    DisplayDisabled();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value attributed to the button.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Removes all leftover on-demand audio sources from the button.
        /// </summary>
        private void OnDisable()
        {
            for (int i = 0; i < onDemandAudioSources.Count; i++)
            {
                AudioSource source = onDemandAudioSources[i];
                Destroy(source);
                onDemandAudioSources.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// Sets the text of the button.
        /// </summary>
        /// <param name="text">The text to use.</param>
        public void SetText(string text)
        {
            if(StandardText != null)
            {
                StandardText.text = text;
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPText != null)
            {
                TMPText.text = text;
            }
#endif
        }

        /// <summary>
        /// Gets or sets the standard Unity Text component of the button.
        /// </summary>
        public Text StandardText
        {
            get { return this.text; }
            set { this.text = value; }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Gets or sets the TextMeshPro Text component of the button.
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
                    if (txt.gameObject.name.Contains("Text") && txt.gameObject.name.Contains("TMP"))
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