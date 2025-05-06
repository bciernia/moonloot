using EasyTalk.Controller;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyTalk.Demo
{
    /// <summary>
    /// Provides controls and functionality to allow the player to easily change languages in the EasyTalk Demo.
    /// </summary>
    public class LanguageControl : MonoBehaviour
    {
        /// <summary>
        /// The PlayerController used to control the player's position and camera.
        /// </summary>
        [SerializeField]
        private PlayerController playerController;

        /// <summary>
        /// The component which contains the buttons for changing languages.
        /// </summary>
        [SerializeField]
        private GameObject languageButtonContainer;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// The input action asset in which the player controls are configured.
        /// </summary>
        [SerializeField]
        private InputActionAsset playerControlInputActions;
#endif

        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            InputAction changeLanguageAction = playerControlInputActions.FindAction("ChangeLanguage");
            if (changeLanguageAction != null)
            {
                changeLanguageAction.performed += delegate
                {
                    playerController.DisableCameraRotation();
                    languageButtonContainer.SetActive(true);
                };
            }
#endif
        }

        private void Update()
        {
#if !ENABLE_INPUT_SYSTEM
            if (Input.GetKeyDown(KeyCode.L))
            {
                playerController.DisableCameraRotation();
                languageButtonContainer.SetActive(true);
            }
#endif
        }

        /// <summary>
        /// Changes the language being used by the dialogue system.
        /// </summary>
        /// <param name="languageCode">The ISO-639 language code to switch to.</param>
        public void SetLanguage(string languageCode)
        {
            EasyTalkGameState.Instance.Language = languageCode;
        }
    }
}
