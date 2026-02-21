using System;

namespace EasyTalk.Controller
{
    /// <summary>
    /// The EasyTalkGameState is used to keep track of global settings used by the EasyTalk system during runtime.
    /// </summary>
    public class EasyTalkGameState
    {
        /// <summary>
        /// The active EasyTalkGameState instance.
        /// </summary>
        private static EasyTalkGameState instance;

        /// <summary>
        /// The session ID used for the current game/playthrough.
        /// </summary>
        private string sessionId = Guid.NewGuid().ToString();

        /// <summary>
        /// The language currently being used by EasyTalk.
        /// </summary>
        private string language = "en";

        private bool isLanguageOverridden = false;

        /// <summary>
        /// Defines a delegate method to be called when the language is changed.
        /// </summary>
        /// <param name="oldLanguage">The old language being used.</param>
        /// <param name="newLanguage">The new language to use.</param>
        public delegate void OnLanguageChanged(string oldLanguage, string newLanguage);

        /// <summary>
        /// The callback which is triggered when the language changes.
        /// </summary>
        public event OnLanguageChanged onLanguageChanged;

        /// <summary>
        /// Gets or sets the language used by EasyTalk. If set, the onLanguageChanged callback will also be triggered.
        /// </summary>
        public string Language
        {
            get { return language; }
            set
            {
                string oldLanguage = language;
                this.language = value;

                if (!oldLanguage.Equals(value))
                {
                    isLanguageOverridden = true;

                    if (onLanguageChanged != null)
                    {
                        onLanguageChanged(oldLanguage, language);
                    }
                }
            }
        }

        public void SetLanguageWithoutOverride(string languageCode)
        {
            string oldLanguage = language;
            this.language = languageCode;

            if (!oldLanguage.Equals(languageCode))
            {
                if (onLanguageChanged != null)
                {
                    onLanguageChanged(oldLanguage, language);
                }
            }
        }

        public bool IsLanguageOverridden {  get { return isLanguageOverridden; } }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        public string SessionID
        {
            get { return sessionId; }
            set { this.sessionId = value; }
        }

        /// <summary>
        /// Gets the instance of the EasyTalk game state.
        /// </summary>
        public static EasyTalkGameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EasyTalkGameState();
                }

                return instance;
            }
        }

        public void  SetOriginalLanguage(string originalLanguageCode)
        {
            if(!isLanguageOverridden)
            {
                language = originalLanguageCode;
            }
        }
    }
}
