using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Display
{
    /// <summary>
    /// Defines methods for option display listeners, which listen for changes to option displays.
    /// </summary>
    public class OptionDisplayListener : MonoBehaviour
    {
        /// <summary>
        ///  When set to true, debug logging will be shown for each method called on the listener.
        /// </summary>
        [SerializeField]
        protected bool debugEnabled = false;

        /// <summary>
        /// Called whenever dialogue options are set on the option display.
        /// </summary>
        /// <param name="options">A List of Dialogue Options which was set on the Option Display.</param>
        public virtual void OnOptionsSet(List<DialogueOption> options)
        {
            if (debugEnabled) { Debug.Log("Option Display -> " + options.Count + " Options were set to be presented."); }
        }

        /// <summary>
        /// Called whenever an option is selected in the option display.
        /// </summary>
        /// <param name="selectedOption">The option which was selected.</param>
        public virtual void OnOptionSelected(DialogueOption selectedOption)
        {
            if (debugEnabled) { Debug.Log("Option Display -> Option '" + selectedOption.OptionText + "' was selected."); }
        }

        /// <summary>
        /// Called whenever the option selection is changed in the option display.
        /// </summary>
        /// <param name="oldOption">The previously selected option.</param>
        /// <param name="newOption">The newly selected option.</param>
        public virtual void OnOptionChanged(DialogueOption oldOption, DialogueOption newOption)
        {
            if (debugEnabled) { Debug.Log("Option Display -> Selected option was changed from '" + oldOption.OptionText + "' to '" + newOption.OptionText + "'"); }
        }
    }
}
