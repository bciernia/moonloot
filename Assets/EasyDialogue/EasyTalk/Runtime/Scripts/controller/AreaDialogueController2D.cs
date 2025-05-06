using UnityEngine;
using UnityEngine.Events;

namespace EasyTalk.Controller
{
    /// <summary>
    /// Extends the standard DialogueController by adding functionality to either play dialogue automatically when a collider (such as a player) enters the trigger collider
    /// on the AreaDialogueController's GameObject, or sends a notification via the onPrompt UnityEvent to allow something else to determine whether the dialogue should play.
    /// </summary>
    public class AreaDialogueController2D : DialogueController
    {
        /// <summary>
        /// The collider which activates the controller when colliding with this GameObject.
        /// </summary>
        [Tooltip("This is the collider of the object which, when it collides with this GameObject, will trigger the Dialogue to start, or trigger the prompt, depending" +
            "on the activation mode.")]
        [SerializeField]
        private Collider2D activator;

        /// <summary>
        /// The ID of the entry point where the Dialogue is entered when the controller is activated.
        /// </summary>
        [Tooltip("The ID of the Entry node where the Dialogue should enter. Can leave blank if you only have one entry node in the Dialogue.")]
        [SerializeField]
        private string entryPoint;

        /// <summary>
        /// The activation mode of the controller.
        /// </summary>
        [Tooltip("If set to PLAY_ON_ENTER, the dialogue will automatically start playing whenever the activator collider enters the trigger " +
            "collider of this GameObject. If set to PROMPT_ON_ENTER, the Prompt() method will be called, which by default invokes all registered" +
            " callbacks with the onPrompt Unity Event.")]
        [SerializeField]
        private DialogueActivationMode activationMode = DialogueActivationMode.PLAY_ON_ENTER;

        /// <summary>
        /// The deactivation mode of the controller.
        /// </summary>
        [Tooltip("If set to FINISH_PLAYING, the Dialogue will continue playing even after the activator collider leaves the trigger area of this GameObject. If set" +
            "to EXIT_ON_LEAVE_AREA, the Dialogue will exit immediately when the activator leaves the trigger area.")]
        [SerializeField]
        private DialogueDeactivationMode deactivationMode = DialogueDeactivationMode.FINISH_PLAYING;

        /// <summary>
        /// An event which is triggered when the activator enters the trigger area of this GameObject and the activation mode is set to PROMPT_ON_ENTER.
        /// </summary>
        [Tooltip("This event is triggered whenever the activator enters the trigger collider of this GameObject and the activation mode is set to PROMPT_ON_ENTER.")]
        [SerializeField]
        public UnityEvent onPrompt = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the activator collider enters the trigger of this GameObject.
        /// </summary>
        [Tooltip("This event is triggered whenever the activator enters the trigger collider of this GameObject.")]
        [SerializeField]
        public UnityEvent onAreaEntered = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the activator collider leaves the trigger of this GameObject.
        /// </summary>
        [Tooltip("This event is triggered whenever the activator exits the trigger collider of this GameObject.")]
        [SerializeField]
        public UnityEvent onAreaExited = new UnityEvent();

        /// <summary>
        /// Triggers the onPrompt event.
        /// </summary>
        public virtual void Prompt()
        {
            if (onPrompt != null) { onPrompt.Invoke(); }
        }

        /// <summary>
        /// Plays the Dialogue and triggers the onPlay event.
        /// </summary>
        public virtual void Play()
        {
            if (entryPoint == null || entryPoint.Length == 0)
            {
                PlayDialogue();
            }
            else
            {
                PlayDialogue(entryPoint);
            }
        }

        /// <summary>
        /// Exits the Dialogue.
        /// </summary>
        public virtual void Exit()
        {
            ExitDialogue();
        }

        /// <summary>
        /// If the 'other' collider is the activator then the Dialogue will be played or the onPrompt event will be triggered, depending on the activation mode. This method will
        /// also trigger the onAreaEntered event.
        /// </summary>
        /// <param name="other">The collider which entered this trigger.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == activator)
            {
                if (onAreaEntered != null) { onAreaEntered.Invoke(); }

                if (activationMode == DialogueActivationMode.PLAY_ON_ENTER)
                {
                    Play();
                }
                else
                {
                    Prompt();
                }
            }
        }

        /// <summary>
        /// If the 'other' collider is the activator then the Dialogue will be exited if the deactivation mode is EXIT_ON_LEAVE_AREA. This method also triggers the 
        /// onAreaExited event.
        /// </summary>
        /// <param name="other">The collider which entered this trigger.</param>
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == activator)
            {
                if (onAreaExited != null) { onAreaExited.Invoke(); }

                if (deactivationMode == DialogueDeactivationMode.EXIT_ON_LEAVE_AREA && isRunning)
                {
                    Exit();
                }
            }
        }

        /// <summary>
        /// The dialogue activation mode for an area controller.
        /// </summary>
        public enum DialogueActivationMode
        {
            PLAY_ON_ENTER,
            PROMPT_ON_ENTER
        }

        /// <summary>
        /// The dialogue deactivation mode for an area controller.
        /// </summary>
        public enum DialogueDeactivationMode
        {
            EXIT_ON_LEAVE_AREA,
            FINISH_PLAYING
        }
    }
}