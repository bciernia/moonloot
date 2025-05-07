using EasyTalk.Display;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

using UnityEngine.UI;

namespace EasyTalk.Utils
{
    /// <summary>
    /// Provides utility functions for correctly setting up prefabs and GameObjects in the scene.
    /// </summary>
    public class SetupUtils
    {
        public static void SetUpEventSystem()
        {
            EventSystem eventSystem = null;

#if UNITY_2022_3_OR_NEWER
            eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
#else
            eventSystem = GameObject.FindObjectOfType<EventSystem>();
#endif

            if (eventSystem == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();

                #if ENABLE_INPUT_SYSTEM
                    eventSystemObj.AddComponent<InputSystemUIInputModule>();
                #else
                    eventSystemObj.AddComponent<StandaloneInputModule>();
                #endif
            }
        }

        /// <summary>
        /// If TextMeshPro is enabled/installed and forceStandardText is false, this method enables all TextMeshPro components and disables all Unity
        /// standard Text components on the GameObject provided; otherwise, all TextMeshPro components are disabled and the standard text components 
        /// will be enabled.
        /// <param name="gameObject">The GameObject to enable/disable text components on.</param>
        /// <param name="forceStandardText">Whether the GameObject should be forced to use standard text components, even if TextMeshPro is installed/enabled.</param>
        /// </summary>
        public static void SetUpTextComponents(GameObject gameObject, bool forceStandardText = false)
        {
            bool isStandardTextForced = forceStandardText;

            Component[] components = gameObject.GetComponents<MonoBehaviour>();
            foreach(Component comp in components)
            {
                if(comp is DialoguePanel)
                {
                    isStandardTextForced = (comp as DialoguePanel).ForceStandardText();
                    break;
                }
            }

            if (isStandardTextForced)
            {
                DisableTextMeshProObjects(gameObject);
                EnableStandardTextObjects(gameObject);
            }
            else
            {
                #if TEXTMESHPRO_INSTALLED
                    EnableTextMeshProObjects(gameObject);
                    DisableStandardTextObjects(gameObject);
                #else
                    //Disable TextMeshPro components and enable normal text components.
                    DisableTextMeshProObjectsByName(gameObject);
                    EnableStandardTextObjects(gameObject);
                #endif
            }
        }

        /// <summary>
        /// Finds all TextMeshPro components in the children of the provided GameObject (including itself) and sets them to be inactive.
        /// </summary>
        /// <param name="gameObject">The GameObject to search and disable GameObjects on if they contain TextMeshPro components.</param>
        private static void DisableTextMeshProObjects(GameObject gameObject)
        {
            #if TEXTMESHPRO_INSTALLED
                foreach (TMP_Text tmpText in gameObject.GetComponentsInChildren<TMP_Text>(false))
                {
                    tmpText.gameObject.SetActive(false);
                }

                foreach (TMP_InputField inputField in gameObject.GetComponentsInChildren<TMP_InputField>(false))
                {
                    inputField.gameObject.SetActive(false);
                }
            #endif
        }

        /// <summary>
        /// Finds all TextMeshPro components in the children of the provided GameObject (including itself) and sets them to be active.
        /// </summary>
        /// <param name="gameObject">The GameObject to search and enable GameObjects on if they contain TextMeshPro components.</param>
        private static void EnableTextMeshProObjects(GameObject gameObject)
        {
            #if TEXTMESHPRO_INSTALLED
                foreach (TMP_Text tmpText in gameObject.GetComponentsInChildren<TMP_Text>(true))
                {
                    tmpText.gameObject.SetActive(true);
                }

                foreach (TMP_InputField inputField in gameObject.GetComponentsInChildren<TMP_InputField>(true))
                {
                    inputField.gameObject.SetActive(true);
                }
            #endif
        }

        /// <summary>
        /// Finds all Unity standard Text components in the children of the provided GameObject (including itself) and sets them to be inactive.
        /// </summary>
        /// <param name="gameObject">The GameObject to search and disable GameObjects on if they contain Text components.</param>
        private static void DisableStandardTextObjects(GameObject gameObject)
        {
            foreach (Text text in gameObject.GetComponentsInChildren<Text>(false))
            {
                text.gameObject.SetActive(false);
            }

            foreach (InputField inputField in gameObject.GetComponentsInChildren<InputField>(false))
            {
                inputField.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Finds all Unity standard Text components in the children of the provided GameObject (including itself) and sets them to be active.
        /// </summary>
        /// <param name="gameObject">The GameObject to search and enable GameObjects on if they contain Text components.</param>
        private static void EnableStandardTextObjects(GameObject gameObject)
        {
            foreach (Text text in gameObject.GetComponentsInChildren<Text>(true))
            {
                text.gameObject.SetActive(true);
            }

            foreach (InputField inputField in gameObject.GetComponentsInChildren<InputField>(true))
            {
                inputField.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Finds all components in the children of the provided GameObject (including itself) with names which include the text '(TMP)' 
        /// and sets them to be inactive.
        /// </summary>
        /// <param name="gameObject">The GameObject to search and disable GameObjects on if they contain TextMeshPro components.</param>
        private static void DisableTextMeshProObjectsByName(GameObject gameObject)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;

                if(child.name.Contains("(TMP)")) 
                {
                    child.SetActive(false);
                }

                DisableTextMeshProObjectsByName(child);
            }

            if(gameObject.name.Contains("(TMP)"))
            {
                gameObject.SetActive(false);
            }
        }
    }
}