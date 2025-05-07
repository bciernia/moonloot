using EasyTalk.Animation;
using EasyTalk.Controller;
using EasyTalk.Localization;
using EasyTalk.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EasyTalk.Display
{
    /// <summary>
    /// The DialoguePanel is an abstract component which provides a set of core features for components/panels which make up a dialogue display. Specifically, this class 
    /// provides functionality for showing and hiding a dialogue panel and handling the animation associated with those actions.
    /// </summary>
    public abstract class DialoguePanel : DialogueListener, LocalizableComponent
    {
        /// <summary>
        /// The display ID of the panel.
        /// </summary>
        [Tooltip("An identifier for the panel.")]
        [SerializeField]
        private string displayID = null;

        /// <summary>
        /// Whether the Display uses standard Text components rather than TextMeshPro components, even with TextMeshPro installed.
        /// </summary>
        [Tooltip("Forces the display to use standard Unity text components rather than TextMeshPro, even when TextMeshPro is installed.")]
        [SerializeField]
        protected bool forceStandardText = false;

        /// <summary>
        /// The animation type to use when hiding/showing the panel.
        /// </summary>
        [Tooltip("The type of animation to use when hiding and showing the panel." +
            "\nNONE: No animation, immediately shows/hides the panel." +
            "\nFADE: Fades all image and text components alpha channels to 0 when hiding, and back when showing." +
            "\nSLIDE_LEFT: Slides the panel to the left out of the canvas/screen when hiding, and slides in from the left when showing." +
            "\nSLIDE_RIGHT: Slides the panel to the right out of the canvas/screen when hiding, and slides in from the right when showing." +
            "\nSLIDE_TOP: Slides the panel to the top out of the canvas/screen when hiding, and slides in from the top when showing." +
            "\nSLIDE_BOTTOM: Slides the panel to the bottom out of the canvas/screen when hiding, and slides in from the bottom when showing.")]
        [SerializeField]
        private UIAnimator.Animation animationType = UIAnimator.Animation.NONE;

        /// <summary>
        /// The animation curve to use for controling the timing of the show/hide animations for the panel.
        /// </summary>
        [Tooltip("The animation curve to use when hiding and showing the panel.")]
        [SerializeField]
        private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        /// <summary>
        /// The amount of time that show/hide animations should take to complete.
        /// </summary>
        [Tooltip("The amount of time to the hide/show transition animation should take to complete.")]
        [SerializeField]
        private float animationTime = 0.4f;

        /// <summary>
        /// Whether the panel should return to its original starting position when shown (only applicable when using sliding animation modes). If false, 
        /// the panel will just move into the canvas until it is fully visible when shown.
        /// </summary>
        [Tooltip("If set to true, the panel will return to its starting position when being shown; otherwise, the panel will attempt to " +
            "move into the canvas/screen and stop once fully visible")]
        [SerializeField]
        private bool returnToOriginalPosition = true;

        /// <summary>
        /// Whether the panel should override the base dialogue font size settings for its child text components. 
        /// When this is true, the font auto-sizing applied to each child text component will come from the min and max 
        /// font size settings set on this component.
        /// </summary>
        [Tooltip("If set to true, this component will apply the configured minimum and maximum font sizes to all child text components whenever they are in " +
            "auto-size mode.")]
        [SerializeField]
        private bool overrideFontSizeSettings = false;

        /// <summary>
        /// Whenever this is set to a LanguageFontOverrides asset, then whenever the language is switched, the fonts and font size
        /// settings used on child components of this component will be based on the settings in the LanguageFontOverrides. 
        /// </summary>
        [Tooltip("Optional. When this value is set to a LanguageFontOverrides asset, all child text components will have their fonts and font sizes set " +
            "based on the language being used and the settings in the asset. If the font settings are also being overridden in this component, then the overridden " +
            "minimum and maximum font sizes take priority when a text component is in auto-size mode.")]
        [SerializeField]
        private LanguageFontOverrides languageFontOverrides;

        /// <summary>
        /// The minimum font size to use when auto-sizing child text components.
        /// </summary>
        [Tooltip("The minimum font size to use on all child text components when in font auto-sizing mode.")]
        [SerializeField]
        private int minFontSize = 0;

        /// <summary>
        /// The maximum font size to use when auto-sizing child text components.
        /// </summary>
        [Tooltip("The maximum font size to use on all child text components when in font auto-sizing mode.")]
        [SerializeField]
        private int maxFontSize = 64;

        /// <summary>
        /// An event which is triggered whenever the panel starts being hidden.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onHideStart = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the panel has finished transitioning to a hidden state.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onHideComplete = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the panel starts being shown.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onShowStart = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the panel has finished transitioning to a shown state.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public UnityEvent onShowComplete = new UnityEvent();

        /// <summary>
        /// The original position of the panel.
        /// </summary>
        protected Vector3 originalPosition = Vector3.zero;

        /// <summary>
        /// Whether the panel is currently hidden.
        /// </summary>
        protected bool isHidden = false;

        /// <summary>
        /// A coroutine which runs when transitioning the panel to be shown.
        /// </summary>
        private Coroutine showCoroutine;

        /// <summary>
        /// A cortouine which runs when transitioning the panel to a be hidden.
        /// </summary>
        private Coroutine hideCoroutine;

        /// <summary>
        /// Initializes the panel by storing the original position.
        /// </summary>
        public virtual void Init()
        {
            this.originalPosition = this.transform.localPosition;
            this.onShowStart.AddListener(OnShowStart);
            this.onShowComplete.AddListener(OnShowComplete);
            this.onHideStart.AddListener(OnHideStart);
            this.onHideComplete.AddListener(OnHideComplete);
        }

        /// <summary>
        /// Enables and disables TextMeshPro components and Unity standard Text components based on whether TextMeshPro is installed.
        /// </summary>
        private void OnValidate()
        {
            SetupUtils.SetUpTextComponents(this.gameObject, forceStandardText);
        }

        /// <summary>
        /// Gets or sets the Display ID of the panel.
        /// </summary>
        public string DisplayID
        {
            get { return this.displayID; }
            set { this.displayID = value; }
        }

        /// <summary>
        /// Gets whether the panel is currently hidden.
        /// </summary>
        public bool IsHidden
        {
            get { return isHidden; }
        }

        /// <summary>
        /// Sets the panel to be active, and recursively sets all parent game objects to be active.
        /// </summary>
        public void Activate()
        {
            this.gameObject.SetActive(true);

            Transform parent = this.transform;
            while ((parent = parent.transform.parent) != null)
            {
                if (!parent.gameObject.activeSelf) { parent.gameObject.SetActive(true); }
            }
        }

        /// <summary>
        /// Makes the panel inactive.
        /// </summary>
        public void Deactivate()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Gets the original position of the panel.
        /// </summary>
        public Vector3 OriginalPosition { get { return originalPosition; } }

        /// <summary>
        /// Returns whether or not the panel should force standard text component usage, even when TextMesh Pro is available.
        /// </summary>
        /// <returns></returns>
        public bool ForceStandardText()
        {
            return forceStandardText;
        }

        /// <summary>
        /// Asynchronously slides the display out of its Canvas' bounds.
        /// </summary>
        /// <param name="deactivateAfterSlide">Whether the panel should be set to inactive after the slide out animation has completed.</param>
        /// <returns></returns>
        protected IEnumerator SlideDisplayOut(bool deactivateAfterSlide = true)
        {
            WaitForAnimation();

            this.isHidden = true;
            if (onHideStart != null) { onHideStart.Invoke(); }

            yield return UIAnimator.SlideOutComponent(
                this.GetComponentInParent<Canvas>(), 
                this.GetComponent<RectTransform>(),
                UIAnimator.GetSlideDirection(animationType), 
                animationTime, 
                animationCurve);

            if (deactivateAfterSlide) { this.Deactivate(); }

            if (onHideComplete != null) { onHideComplete.Invoke(); }
        }

        /// <summary>
        /// Asynchronously slides the display into its Canvas' bounds. If returnToOriginalPosition is set to true, then the panel will slide to its original position.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator SlideDisplayIn()
        {
            WaitForAnimation();

            this.Activate();
            this.isHidden = false;
            if (onShowStart != null) { onShowStart.Invoke(); }

            if (returnToOriginalPosition)
            {
                yield return UIAnimator.SlideComponentToPosition(
                    this.GetComponent<RectTransform>(), 
                    this.OriginalPosition, 
                    animationTime, 
                    animationCurve);
            }
            else
            {
                yield return UIAnimator.SlideInComponent(
                    this.GetComponentInParent<Canvas>(), 
                    this.GetComponent<RectTransform>(), 
                    animationTime, 
                    animationCurve);
            }
            
            if (onShowComplete != null) { onShowComplete.Invoke(); }
        }

        /// <summary>
        /// Asynchronously fades the display and all of its child text and image components in to their original transparency values.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator FadeDisplayIn()
        {
            WaitForAnimation();

            this.Activate();
            this.isHidden = false;
            if (onShowStart != null) { onShowStart.Invoke(); }

            yield return UIAnimator.FadeIn(this.gameObject, animationTime, animationCurve);
            
            if (onShowComplete != null) { onShowComplete.Invoke(); }
        }

        /// <summary>
        /// Asynchronously fades the display and all of its child text and image components out to be completely transparent.
        /// </summary>
        /// <param name="deactivateAfterFade">Whether the panel should be set to inactive after the fade out animation has completed.</param>
        /// <returns></returns>
        protected IEnumerator FadeDisplayOut(bool deactivateAfterFade = true)
        {
            WaitForAnimation();

            if (onHideStart != null) { onHideStart.Invoke(); }
            this.isHidden = true;

            yield return UIAnimator.FadeOut(this.gameObject, animationTime, animationCurve);

            if (onHideComplete != null) { onHideComplete.Invoke(); }
            if (deactivateAfterFade) { this.Deactivate(); }
        }

        /// <summary>
        /// Asynchronously waits for the currently running animation on this panel to complete.
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForAnimation()
        {
            while (UIAnimator.IsItemAnimating(this.gameObject.GetInstanceID())) { yield return new WaitForEndOfFrame(); }
        }

        /// <summary>
        /// Hides the panel.
        /// </summary>
        /// <param name="deactivateAfterHide">Whether the panel should be set to inactive after it is hidden.</param>
        public virtual void Hide(bool deactivateAfterHide = true)
        {
            if (!this.isHidden)
            {
                Activate();

                if (gameObject.activeSelf)
                {
                    CancelHideRoutine();

                    if (animationType != UIAnimator.Animation.NONE)
                    {
                        this.isHidden = true;

                        if (animationType == UIAnimator.Animation.FADE)
                        {
                            StartCoroutine(FadeDisplayOut(deactivateAfterHide));
                        }
                        else
                        {
                            StartCoroutine(SlideDisplayOut(deactivateAfterHide));
                        }
                    }
                    else
                    {
                        HideImmediately();

                        if (deactivateAfterHide) { this.Deactivate(); }
                    }
                }
            }
        }

        /// <summary>
        /// Hides the panel immediately (doesn't use a transition animation).
        /// </summary>
        /// <param name="deactivateAfterHide">Whether the panel should be set to inactive after it is hidden.</param>
        public void HideImmediately(bool deactivateAfterHide = true)
        {
            CancelHideRoutine();

            this.isHidden = true;
            if (onHideStart != null) { onHideStart.Invoke(); }

            if (animationType != UIAnimator.Animation.NONE)
            {
                if (animationType == UIAnimator.Animation.FADE)
                {
                    UIAnimator.FadeOutImmediately(this.gameObject);
                }
                else
                {
                    UIAnimator.SlideOutComponentImmediately(
                        this.GetComponentInParent<Canvas>(),
                        this.GetComponent<RectTransform>(),
                        UIAnimator.GetSlideDirection(animationType));
                }
            }

            if (onHideComplete != null) { onHideComplete.Invoke(); }
            if (deactivateAfterHide) { this.Deactivate(); }
        }

        /// <summary>
        /// Shows the panel immediately (doesn't use a transition animation).
        /// </summary>
        public void ShowImmediately()
        {
            CancelShowRoutine();

            if (onShowStart != null) { onShowStart.Invoke(); }
            this.Activate();
            
            this.isHidden = false;
            if (onShowComplete != null) { onShowComplete.Invoke(); }
        }

        /// <summary>
        /// Shows the panel.
        /// </summary>
        public virtual void Show()
        {
            if (!this.isActiveAndEnabled) { Activate(); }

            if (gameObject.activeSelf)
            {
                CancelShowRoutine();

                if (animationType == UIAnimator.Animation.NONE)
                {
                    this.ShowImmediately();
                }
                else if (animationType == UIAnimator.Animation.FADE)
                {
                    StartCoroutine(this.FadeDisplayIn());
                }
                else
                {
                    StartCoroutine(this.SlideDisplayIn());
                }
            }
        }

        /// <summary>
        /// Stops the coroutine for showing the panel, if it's running.
        /// </summary>
        private void CancelShowRoutine()
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        /// <summary>
        /// Stops the coroutine for hiding the panel, if it's running.
        /// </summary>
        private void CancelHideRoutine()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
        }

        /// <summary>
        /// Callback for when the panel begines to be shown.
        /// </summary>
        protected virtual void OnShowStart() { }

        /// <summary>
        /// Callback for when the panel is finished being shown (when all animations are complete).
        /// </summary>
        protected virtual void OnShowComplete() { }

        /// <summary>
        /// Callback for when the panel begins to be hidden.
        /// </summary>
        protected virtual void OnHideStart() { }

        /// <summary>
        /// Callback for when the panel has finished being hidden (when all animations are complete).
        /// </summary>
        protected virtual void OnHideComplete() { }


        /// <summary>
        /// Gets or sets the animation type used for showing and hiding the panel.
        /// </summary>
        public UIAnimator.Animation AnimationType { get { return animationType; } set { this.animationType = value; } }

        /// <summary>
        /// Gets or sets whether the panel should override the default language-specific minimum and maximum font sizes when the language is updated.
        /// </summary>
        public bool OverrideFontSizes 
        {
            get { return overrideFontSizeSettings; }
            set { overrideFontSizeSettings = value; } 
        }

        /// <summary>
        /// Gets or sets the LanguageFontOverrides which are used to change fonts when the language is changed.
        /// </summary>
        public LanguageFontOverrides LanguageFontOverrides
        {
            get { return languageFontOverrides; }
            set { languageFontOverrides = value; }
        }

        /// <summary>
        /// Gets or sets the minimum font size to use.
        /// </summary>
        public int MinFontSize
        {
            get { return minFontSize; }
            set { minFontSize = value; }
        }

        /// <summary>
        /// Gets or sets the maximum font size to use.
        /// </summary>
        public int MaxFontSize
        {
            get { return maxFontSize; }
            set { maxFontSize = value; }
        }
    }
}