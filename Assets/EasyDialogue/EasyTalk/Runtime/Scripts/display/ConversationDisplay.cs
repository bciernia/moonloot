using EasyTalk.Character;
using EasyTalk.Controller;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Tags;
using EasyTalk.Settings;
using System.Collections;
using System.Collections.Generic;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// This is an implementation of a conversation display which is used to display lines of dialogue to a player in addition to the name of the character who is speaking if desired.
    /// </summary>
    public class ConversationDisplay : DialoguePanel
    {
    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text element used to display the character name.
        /// </summary>
        [Tooltip("The text element to use for displaying character names.")]
        [SerializeField]
        private TMP_Text textMeshProCharacterNameText;
    #endif

        /// <summary>
        /// The text element used to display the character name.
        /// </summary>
        [Tooltip("The text element to use for displaying character names.")]
        [SerializeField]
        private Text characterNameText;

        /// <summary>
        /// The background image of the character name display.
        /// </summary>
        [Tooltip("The background image used for the character name.")]
        [SerializeField]
        private Image characterNameBackgroundImage;


    #if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text element used to display lines of dialogue.
        /// </summary>
        [Tooltip("The text element to use for displaying lines of dialogue.")]
        [SerializeField]
        private TMP_Text textMeshProConvoText;
    #endif

        /// <summary>
        /// The text element used to display lines of dialogue.
        /// </summary>
        [Tooltip("The text element to use for displaying lines of dialogue.")]
        [SerializeField]
        private Text convoText;

        /// <summary>
        /// The display mode of the dialogue text.
        /// </summary>
        [Tooltip("The way text should be displayed on the Conversation Display." +
            "\nFULL: Lines of dialogue will be shown in their entirety." +
            "\nBY_WORD: Lines of dialogue will be shown one word at a time." +
            "\nBY_CHARACTER: Lines of dialogue will be shown one character at a time.")]
        [SerializeField]
        private TextDisplayMode textDisplayMode = TextDisplayMode.FULL;

        /// <summary>
        /// When the text display mode is set to BY_WORD, this value controls how many words are displayed per second.
        /// </summary>
        [Tooltip("How many words from a line of dialogue should be displayed per second.")]
        [SerializeField]
        private float wordsPerSecond = 5.0f;

        /// <summary>
        /// When the text display mode is set to BY_CHARACTER, this value controls how many characters are displayed per second.
        /// </summary>
        [Tooltip("How many characters from a line of dialogue should be displayed per second.")]
        [SerializeField]
        private float charactersPerSecond = 20.0f;

        /// <summary>
        /// The images used to create the conversation display panel.
        /// </summary>
        [Tooltip("A list of images used in the Conversation Display.")]
        [NonReorderable]
        [SerializeField]
        private List<Image> conversationPanelImages = new List<Image>();

        /// <summary>
        /// Whether the conversation display should be hidden on awake.
        /// </summary>
        [Tooltip("If set to true, the conversation display will be hidden when Awake() is called on the GameObject.")]
        [SerializeField]
        private bool hideOnAwake = true;

        /// <summary>
        /// The delimiter text to prepend whenever text is appended to the display.
        /// </summary>
        [SerializeField]
        private string appendDelimiter = " ";

        /// <summary>
        /// Listeners for conversation display events which are triggered whenever the character name is updated or the conversation text is updated.
        /// </summary>
        [Tooltip("A set of Conversation Display Listeners which listen for the character name or conversation text being set a conversation display.")]
        [NonReorderable]
        [SerializeField]
        private List<ConversationDisplayListener> conversationDisplayListeners = new List<ConversationDisplayListener>();

        /// <summary>
        /// An event which is triggered when the character name is updated.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected UnityEvent onCharacterNameUpdated = new UnityEvent();

        /// <summary>
        /// An event which is triggered when the covnersation text is updated.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected UnityEvent onConversationTextUpdated = new UnityEvent();

        /// <summary>
        /// An event which is triggered whenever the display is cleared out, meaning the conversation text and character name are set to empty strings.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected UnityEvent onReset = new UnityEvent();

        /// <summary>
        /// An event which is triggered when the conversation text starts being displayed.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected UnityEvent onTextDisplayStarted = new UnityEvent();

        /// <summary>
        /// An event which is triggered when the conversation text has finished being displayed.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected UnityEvent onTextDisplayCompleted = new UnityEvent();

        /// <summary>
        /// The current line of dialogue being displayed by the conversation display.
        /// </summary>
        protected ConversationLine currentLine;

        /// <summary>
        /// The final target text to display on the conversation display.
        /// </summary>
        private string targetText = "";

        /// <summary>
        /// The coroutine for displaying text over time.
        /// </summary>
        private Coroutine displayTextCoroutine;

        /// <summary>
        /// This setting determines how gibberish sounds are played.
        /// </summary>
        [Tooltip("Determines how gibberish sounds are played as dialogue text is being displayed." +
            "\nNONE: No gibberish sounsd will be played." +
            "\nCYCLE_RANDOM: Gibberish audio clips will be randomly selected and played consecutively until the text has finished displaying." +
            "\nLOOP_SINGLE: A random gibberish audio clip will be selected from the list, and looped until the text has finished displaying." +
            "\nPLAY_RANDOM_PER_ITEM: Each time a new character or word is displayed, a random gibberish sound will be selected from the list and played.")]
        [SerializeField]
        private GibberishMode gibberishMode = GibberishMode.NONE;

        /// <summary>
        /// The audio source to use when playing gibberish sounds.
        /// </summary>
        [SerializeField]
        private AudioSource gibberishAudioSource;

        /// <summary>
        /// A collection of audio clips to use for playing gibberish audio.
        /// </summary>
        [NonReorderable]
        [SerializeField]
        private List<AudioClip> gibberishAudioClips = new List<AudioClip>();

        /// <summary>
        /// A temporary list of audio clips to use for character gibberish when a character doesn't override the defaults.
        /// </summary>
        private List<AudioClip> defaultGibberishAudioClips = new List<AudioClip>();

        /// <summary>
        /// A flag which indicates whether the conversation display is currently processing one text item at a time, such as a word or character.
        /// </summary>
        private bool isPerItemDisplayOngoing = false;

        /// <summary>
        /// A coroutine for playing random gibberish sounds.
        /// </summary>
        private Coroutine gibberishCoroutine;

        /// <summary>
        /// The character library.
        /// </summary>
        private CharacterLibrary characterLibrary;

        /// <summary>
        /// Initializes the display and hides it on awake.
        /// </summary>
        private void Awake()
        {
            Init();
            InitializeCharacterLibrary();

            BackupDefaultGibberish();

            if (hideOnAwake)
            {
                HideImmediately();
            }
        }

        /// <summary>
        /// Activates the conversation text.
        /// </summary>
        public void ShowConversationText()
        {
            if (forceStandardText)
            {
                if (StandardConvoText != null)
                {
                    StandardConvoText.gameObject.SetActive(true);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (TMPConvoText != null)
                {
                    TMPConvoText.gameObject.SetActive(true);
                }
#else
                if (StandardConvoText != null)
                {
                    StandardConvoText.gameObject.SetActive(true);
                }
#endif
            }
        }

        /// <summary>
        /// Deactivates the conversation text.
        /// </summary>
        public virtual void HideConversationText()
        {
            if (forceStandardText)
            {
                if (StandardConvoText != null)
                {
                    StandardConvoText.gameObject.SetActive(false);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (TMPConvoText != null)
                {
                    TMPConvoText.gameObject.SetActive(false);
                }
#else
                if (StandardConvoText != null)
                {
                    StandardConvoText.gameObject.SetActive(false);
                }
#endif
            }
        }

        /// <summary>
        /// Activates the character name text.
        /// </summary>
        public void ShowCharacterName()
        {
            if(forceStandardText)
            {
                if (StandardCharacterNameText != null)
                {
                    StandardCharacterNameText.gameObject.SetActive(true);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (TMPCharacterNameText != null)
                {
                    TMPCharacterNameText.gameObject.SetActive(true);
                }
#else
                if (StandardCharacterNameText != null)
                {
                    StandardCharacterNameText.gameObject.SetActive(true);
                }
#endif
            }
        }

        /// <summary>
        /// Deactivates the character name text.
        /// </summary>
        public virtual void HideCharacterName()
        {
            if (forceStandardText)
            {
                if (StandardCharacterNameText != null)
                {
                    StandardCharacterNameText.gameObject.SetActive(false);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (TMPCharacterNameText != null)
                {
                    TMPCharacterNameText.gameObject.SetActive(false);
                }
#else
                if (StandardCharacterNameText != null)
                {
                    StandardCharacterNameText.gameObject.SetActive(false);
                }
#endif
            }
        }

        /// <summary>
        /// Sets the value of the conversation display dialogue text element.
        /// </summary>
        /// <param name="text">The text to use.</param>
        public void SetConversationText(string text)
        {
            if (StandardConvoText != null)
            {
                StandardConvoText.text = text;
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPConvoText != null)
            {
                TMPConvoText.text = text;
            }
#endif

            if (onConversationTextUpdated != null) { onConversationTextUpdated.Invoke(); }

            foreach(ConversationDisplayListener listener in conversationDisplayListeners)
            {
                listener.OnConversationTextUpdated(text);
            }
        }

        /// <summary>
        /// Forces the display to finish displaying its text.
        /// </summary>
        public void ForceFinish()
        {
            //Stop the coroutines and set the text to whatever it's supposed to be.
            if (displayTextCoroutine != null)
            {
                StopCoroutine(displayTextCoroutine);
                displayTextCoroutine = null;
            }

            isPerItemDisplayOngoing = false;
            StopGibberish();

            SetConversationText(targetText);

            if (onTextDisplayCompleted != null) { onTextDisplayCompleted.Invoke(); }
        }

        /// <summary>
        /// Appends the line of dialogue provided to the current text of the conversation display.
        /// </summary>
        /// <param name="appendedLine">The line of dialogue to append to the display.</param>
        public void AppendText(ConversationLine appendedLine)
        {
            if(appendedLine.AutoPlay)
            {
                float delay = CalculateAutoDelayForText(appendedLine.Text, appendedLine.PlayTime);
                displayTextCoroutine = StartCoroutine(AppendTextAsync(appendedLine.Text, delay));
            }
            else
            {
                float delay = CalculateDelay();
                displayTextCoroutine = StartCoroutine(AppendTextAsync(appendedLine.Text, delay));
            }
        }

        /// <summary>
        /// Asynchronously appends the text specified to the currently displayed text.
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <returns></returns>
        private IEnumerator AppendTextAsync(string text, float delay = 0.0f)
        {
            string appendText = appendDelimiter + text;
            targetText += appendText;

            if (onTextDisplayStarted != null) { onTextDisplayStarted.Invoke(); }

            //Display the text in full immediately.
            if (TextPresentationMode == TextDisplayMode.FULL)
            {
                SetConversationText(appendText);
                if (onTextDisplayCompleted != null) { onTextDisplayCompleted.Invoke(); }
                yield break;
            }

            //Display the text, one word at a time.
            if (TextPresentationMode == TextDisplayMode.BY_WORD)
            {
                yield return SetConversationTextByWord(appendText, delay, true);
            }
            //Display the text, one character at time.
            else if (TextPresentationMode == TextDisplayMode.BY_CHARACTER)
            {
                yield return SetConversationTextByCharacter(appendText, delay, true);
            }
        }

        /// <summary>
        /// Returns the current text shown on the conversation display.
        /// </summary>
        /// <returns>The text currently shown on the display.</returns>
        private string GetConversationText()
        {
            if (StandardConvoText != null)
            {
                return StandardConvoText.text;
            }

#if TEXTMESHPRO_INSTALLED
            if (TMPConvoText != null)
            {
                return TMPConvoText.text;
            }
#endif
            return null;
        }

        /// <summary>
        /// Displays the dialogue text specified on the conversation display.
        /// </summary>
        /// <param name="line">The line of dialogue to display.</param>
        public void SetConversationText(ConversationLine line)
        {
            if (displayTextCoroutine != null)
            {
                StopCoroutine(displayTextCoroutine);
                displayTextCoroutine = null;
            }

            displayTextCoroutine = StartCoroutine(SetConversationTextAsync(line));
        }

        /// <summary>
        /// Asynchronously sets the conversation display's dialogue text based on the text display mode to the text of the line of dialogue provided.
        /// </summary>
        /// <param name="line">The line of dialogue to use when setting the displayed dialogue text.</param>
        /// <returns></returns>
        public IEnumerator SetConversationTextAsync(ConversationLine line)
        {
            this.currentLine = line;

            if (line.AutoPlay)
            {
                float delay = CalculateAutoDelayForText(line.Text, line.PlayTime);
                yield return SetConversationTextAsync(line.Text, delay);
            }
            else
            {
                float delay = CalculateDelay();
                yield return SetConversationTextAsync(line.Text, delay);
            }
        }

        private float CalculateDelay()
        {
            float delay = 0.0f;
            if (TextPresentationMode == TextDisplayMode.BY_WORD)
            {
                delay = 1.0f / WordsPerSecond;
            }
            else if (TextPresentationMode == TextDisplayMode.BY_CHARACTER)
            {
                delay = 1.0f / CharactersPerSecond;
            }

            return delay;
        }

        private float CalculateAutoDelayForText(string text, float totalPlayTime)
        {
            float delay = 0.0f;
            if (TextPresentationMode == TextDisplayMode.BY_WORD)
            {
                string[] words = text.Split(' ');
                float desiredCompletionTime = WordsPerSecond * words.Length;

                if (desiredCompletionTime < totalPlayTime)
                {
                    delay = 1.0f / WordsPerSecond;
                }
                else
                {
                    float timingModifier = totalPlayTime / desiredCompletionTime;
                    delay = WordsPerSecond * timingModifier;
                }
            }
            else if (TextPresentationMode == TextDisplayMode.BY_CHARACTER)
            {
                float desiredCompletionTime = CharactersPerSecond * text.Length;

                if (desiredCompletionTime < totalPlayTime)
                {
                    delay = 1.0f / CharactersPerSecond;
                }
                else
                {
                    float timingModifier = totalPlayTime / desiredCompletionTime;
                    delay = CharactersPerSecond * timingModifier;
                }
            }

            return delay;
        }

        /// <summary>
        /// Asynchronously sets the displayed dialogue text based on the text display mode set on this conversation display. The delay is used to sync the timing of
        /// the text with the total time alloted to the line of dialogue (applies when the dialogue controller is in AUTOPLAY mode).
        /// </summary>
        /// <param name="text">The text to set.</param>
        /// <param name="delay">A delay added each time the text is updated when in BY_WORD or BY_CHARACTER mode.</param>
        /// <returns></returns>
        public IEnumerator SetConversationTextAsync(string text, float delay = 0.0f)
        {
            targetText = text;
            if (onTextDisplayStarted != null) { onTextDisplayStarted.Invoke(); }

            //If the text is blank, set it to an empty string on the convo display.
            if (text == null || text.Length == 0)
            {
                SetConversationText("");
                if (onTextDisplayCompleted != null) { onTextDisplayCompleted.Invoke(); }
                yield break;
            }

            //Display the text in full immediately.
            if (TextPresentationMode == TextDisplayMode.FULL)
            {
                SetConversationText(text);
                if (onTextDisplayCompleted != null) { onTextDisplayCompleted.Invoke(); }
                yield break;
            }

            //Display the text, one word at a time.
            if (TextPresentationMode == TextDisplayMode.BY_WORD)
            {
                yield return SetConversationTextByWord(text, delay);
            }
            //Display the text, one character at time.
            else if (TextPresentationMode == TextDisplayMode.BY_CHARACTER)
            {
                yield return SetConversationTextByCharacter(text, delay);
            }
        }

        /// <summary>
        /// Updates the displayed dialogue text by displaying one word at a time, using a specific delay per word.
        /// </summary>
        /// <param name="text">The full text to display.</param>
        /// <param name="delay">The delay to use between words.</param>
        /// <param name="append">Whether the specified text should be appended to the currently displayed text (default false).</param>
        /// <returns></returns>
        private IEnumerator SetConversationTextByWord(string text, float delay, bool append = false)
        {
            //Initialize variables, split text into individual words, and reset the convo display.
            isPerItemDisplayOngoing = true;
            int index = 0;
            string oldString = GetConversationText();
            string currentString = "";

            if(append)
            {
                currentString = oldString;
            }
            else
            {
                SetConversationText(currentString);
            }

            //Find any tags in the text so they can be applied as necessary.
            List<TMPTag> tags = TMPTag.FindTags(text);
            List<TMPTag> tagStack = new List<TMPTag>();
            int tagIdx = 0;
            int addedCharCount = 0;

            if(gibberishMode == GibberishMode.LOOP_SINGLE)
            {
                PlayRandomGibberish(true);
            }
            else if(gibberishMode == GibberishMode.CYCLE_RANDOM)
            {
                CycleRandomGibberish();
            }

            while (index < text.Length)
            {
                if (gibberishMode == GibberishMode.PLAY_RANDOM_PER_ITEM)
                {
                    PlayRandomGibberish(false);
                }

                yield return new WaitForSeconds(delay);

                //Remove any characters which were appended for tags.
                if (addedCharCount > 0)
                {
                    currentString = currentString.Substring(0, currentString.Length - addedCharCount);
                }

                //Add any tags preceeding the next word to the current string.
                while (tagIdx < tags.Count && index < text.Length && index == tags[tagIdx].startIdx)
                {
                    //Increase the index by the tag length and append it to the current string.
                    index += tags[tagIdx].endIdx - tags[tagIdx].startIdx + 1;
                    currentString += tags[tagIdx].fullTagText;

                    //Add the tag to the stack if it's an opening tag.
                    if (!tags[tagIdx].isCloseTag) { tagStack.Add(tags[tagIdx]); }
                    else
                    {
                        //Remove tags from the stack if we're no longer looking for closing tags.
                        for (int i = 0; i < tagStack.Count; i++)
                        {
                            TMPTag tag = tagStack[i];
                            if (tag.name.Equals(tags[tagIdx].name))
                            {
                                tagStack.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    tagIdx++;
                }

                //Append the next word to the string.
                while (index < text.Length && text[index] != '<' && text[index] != ' ')
                {
                    currentString += text[index];
                    index++;
                }

                if (index < text.Length && text[index] == ' ')
                {
                    currentString += " ";
                    index++;
                }

                //Append closing tags to the string.
                addedCharCount = 0;
                foreach (TMPTag tag in tagStack)
                {
                    string closeTag = "</" + tag.name + ">";
                    currentString += closeTag;
                    addedCharCount += closeTag.Length;
                }

                //Display the current string.
                SetConversationText(currentString);
            }

            //Display the final text.
            if (append)
            {
                SetConversationText(oldString + text);
            }
            else
            {
                SetConversationText(text);
            }

            isPerItemDisplayOngoing = false;
            StopGibberish();

            if (onTextDisplayCompleted != null) { onTextDisplayCompleted.Invoke(); }
        }

        /// <summary>
        /// Updates the displayed dialogue text by displaying one character at a time, using a specific delay per character.
        /// </summary>
        /// <param name="text">The full text to display.</param>
        /// <param name="delay">The delay to use between characters.</param>
        /// <param name="append">Whether the specified text should be appended to the currently displayed text (default false).</param>
        /// <returns></returns>
        private IEnumerator SetConversationTextByCharacter(string text, float delay, bool append = false)
        {
            isPerItemDisplayOngoing = true;
            int index = 0;
            string currentString = "";
            string oldString = GetConversationText();

            if (append)
            {
                currentString = oldString + text[0];
            }
            else
            {
                SetConversationText(currentString);
            }            

            //Find any tags in the text so they can be applied as necessary.
            List<TMPTag> tags = TMPTag.FindTags(text);
            List<TMPTag> tagStack = new List<TMPTag>();
            int tagIdx = 0;
            int addedCharCount = 0;

            if (gibberishMode == GibberishMode.LOOP_SINGLE)
            {
                PlayRandomGibberish(true);
            }
            else if (gibberishMode == GibberishMode.CYCLE_RANDOM)
            {
                CycleRandomGibberish();
            }

            while (index < text.Length)
            {
                if(gibberishMode == GibberishMode.PLAY_RANDOM_PER_ITEM)
                {
                    PlayRandomGibberish(false);
                }

                yield return new WaitForSeconds(delay);

                if (addedCharCount > 0)
                {
                    currentString = currentString.Substring(0, currentString.Length - addedCharCount);
                }

                //Add any tags preceeding the next word to the current string.
                while (tagIdx < tags.Count && index < text.Length && index == tags[tagIdx].startIdx)
                {
                    //Increase the index by the tag length and append it to the current string.
                    index += tags[tagIdx].endIdx - tags[tagIdx].startIdx + 1;
                    currentString += tags[tagIdx].fullTagText;

                    //Add the tag to the stack if it's an opening tag.
                    if (!tags[tagIdx].isCloseTag) { tagStack.Add(tags[tagIdx]); }
                    else
                    {
                        //Remove tags from thh stack if we're no longer looking for closing tags.
                        for (int i = 0; i < tagStack.Count; i++)
                        {
                            TMPTag tag = tagStack[i];
                            if (tag.name.Equals(tags[tagIdx].name))
                            {
                                tagStack.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    tagIdx++;
                }

                if (index < text.Length)
                {
                    currentString += "" + text[index];
                }

                string tagStackString = "";
                addedCharCount = 0;
                foreach (TMPTag tag in tagStack)
                {
                    string closeTag = "</" + tag.name + ">";
                    currentString += closeTag;
                    addedCharCount += closeTag.Length;
                    tagStackString += tag.fullTagText + ", ";
                }

                SetConversationText(currentString);
                index++;
            }

            if (append)
            {
                SetConversationText(oldString + text);
            }
            else
            {
                SetConversationText(text);
            }

            isPerItemDisplayOngoing = false;
            StopGibberish();

            if (onTextDisplayCompleted != null) { onTextDisplayCompleted.Invoke(); }
        }

        /// <summary>
        /// Stops any currently playing gibberish audio.
        /// </summary>
        private void StopGibberish()
        {
            if(gibberishCoroutine != null)
            {
                StopCoroutine(gibberishCoroutine);
                gibberishCoroutine = null;
            }

            if (gibberishAudioSource != null)
            {
                gibberishAudioSource.Stop();
            }
        }

        /// <summary>
        /// Plays a random gibberish audio clip.
        /// </summary>
        /// <param name="loop">Whether the audio should be looped.</param>
        /// <returns>The AudioClip which is played.</returns>
        private AudioClip PlayRandomGibberish(bool loop)
        {
            if (gibberishAudioSource != null && gibberishAudioClips.Count > 0)
            {
                AudioClip clip = gibberishAudioClips[Random.Range(0, gibberishAudioClips.Count)];
                gibberishAudioSource.clip = clip;
                gibberishAudioSource.loop = loop;
                gibberishAudioSource.Play();
                return clip;
            }

            return null;
        }

        /// <summary>
        /// Starts a coroutine to cycle through random gibberish audio clips. After one audio clip finishes, another random one is selected and played.
        /// </summary>
        private void CycleRandomGibberish()
        {
            gibberishCoroutine = StartCoroutine(CycleGibberishAsync());
        }

        /// <summary>
        /// Cycles through gibberish audio files randomly, playing each one until completion before switching to another.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CycleGibberishAsync()
        {
            while (isPerItemDisplayOngoing)
            {
                AudioClip clipToPlay = PlayRandomGibberish(false);
                if (clipToPlay != null)
                {
                    yield return new WaitForSeconds(clipToPlay.length);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the character name text to display the provided character name.
        /// </summary>
        /// <param name="characterName">The character name to use on the display.</param>
        /// <param name="sourceName">The original character name (pre-translation).</param>
        public void SetCharacterName(string characterName, string sourceName)
        {
            StandardCharacterNameText.text = characterName;

#if TEXTMESHPRO_INSTALLED
            TMPCharacterNameText.text = characterName;
#endif

            if (onCharacterNameUpdated != null) { onCharacterNameUpdated.Invoke(); }

            //Sets up the gibberish audio based on the character who is speaking.
            SetUpGibberishAudio(sourceName);

            foreach (ConversationDisplayListener listener in conversationDisplayListeners)
            {
                listener.OnCharacterNameUpdated(characterName, sourceName);
            }
        }

        /// <summary>
        /// Replaces the default gibberish audio files of the conversation display with those of the character specified, if the character is set to
        /// override the default gibberish audio with their own (in the character library).
        /// </summary>
        /// <param name="characterName">The name of the character to retrieve gibberish audio clips for.</param>
        private void SetUpGibberishAudio(string characterName)
        {
            if (CharacterLibrary != null)
            {
                CharacterDefinition characterDefinition = CharacterLibrary.GetCharacterDefinition(characterName);
                if (characterDefinition != null && characterDefinition.OverrideDefaultGibberishAudio)
                {
                    BackupDefaultGibberish();
                    gibberishAudioClips = characterDefinition.GibberishAudioClips;
                }
                else
                {
                    RestoreDefaultGibberish();
                }
            }
            else
            {
                RestoreDefaultGibberish();
            }
        }

        /// <summary>
        /// Creates a backuup of the default gibberish audio files so that they can be restored when switching to other characters who
        /// may not support overriding gibberish audio.
        /// </summary>
        private void BackupDefaultGibberish()
        {
            defaultGibberishAudioClips = gibberishAudioClips;
        }

        /// <summary>
        /// Restores the default gibberish audio files.
        /// </summary>
        private void RestoreDefaultGibberish()
        {
            gibberishAudioClips = defaultGibberishAudioClips;
        }

        /// <summary>
        /// Resets the conversation display by setting the character name and conversation text to blank values.
        /// </summary>
        public void Reset()
        {
            targetText = "";
            SetConversationText("");
            SetCharacterName("", "");

            if(onReset != null) { onReset.Invoke(); }

            foreach(ConversationDisplayListener listener in conversationDisplayListeners)
            {
                listener.OnReset();
            }
        }

        /// <summary>
        /// Sets the character library from the Dialogue Registry.
        /// </summary>
        private void InitializeCharacterLibrary()
        {
            if (characterLibrary == null)
            {
                DialogueDisplay parentDisplay = DialogueDisplay.GetParentDialogueDisplay(this.gameObject);

                if (parentDisplay != null)
                {
                    EasyTalkDialogueSettings settings = parentDisplay.DialogueSettings;

                    if (settings != null && settings.DialogueRegistry != null)
                    {
                        characterLibrary = settings.DialogueRegistry.CharacterLibrary;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Character Library.
        /// </summary>
        private CharacterLibrary CharacterLibrary
        {
            get
            {
                InitializeCharacterLibrary();
                return this.characterLibrary;
            }
        }

        /// <summary>
        /// Gets or sets the standard Unity Text component of the display for the character name.
        /// </summary>
        public Text StandardCharacterNameText
        {
            get { return this.characterNameText; }
            set { this.characterNameText = value; }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Gets or sets the TextMeshPro Text component of the display for the character name.
        /// </summary>
        public TMP_Text TMPCharacterNameText
        {
            get { return textMeshProCharacterNameText; }
            set { textMeshProCharacterNameText = value; }
        }
#endif

        /// <summary>
        /// Gets the image used for the character name panel.
        /// </summary>
        public Image CharacterNameBackgroundImage
        {
            get { return characterNameBackgroundImage; }
        }

        /// <summary>
        /// Gets or sets the standard Unity Text component of the display.
        /// </summary>
        public Text StandardConvoText
        {
            get { return this.convoText; }
            set { this.convoText = value; }
        }

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// Gets or sets the TextMeshPro Text component of the display.
        /// </summary>
        public TMP_Text TMPConvoText
        {
            get { return textMeshProConvoText; }
            set { textMeshProConvoText = value; }
        }
#endif

        /// <summary>
        /// Gets the List of images used for the conversation display panel.
        /// </summary>
        public List<Image> ConversationPanelImages
        {
            get { return conversationPanelImages; }
        }

        /// <summary>
        /// Gets or sets the display mode used for dialogue text.
        /// </summary>
        public TextDisplayMode TextPresentationMode
        {
            get { return textDisplayMode; }
            set { textDisplayMode = value; }
        }

        /// <summary>
        /// Gets or sets the number of words per second to display when displaying dialogue in BY_WORD mode.
        /// </summary>
        public float WordsPerSecond
        {
            get { return wordsPerSecond; }
            set { wordsPerSecond = value; }
        }

        /// <summary>
        /// Gets or sets the number of characters per second to display when displaying dialogue in BY_CHARACTER mode.
        /// </summary>
        public float CharactersPerSecond
        {
            get { return charactersPerSecond; }
            set { charactersPerSecond = value; }
        }

        /// <summary>
        /// Gets or sets the List of ConversationDisplayListeners to be called by the Conversation Display.
        /// </summary>
        public List<ConversationDisplayListener> ConversationDisplayListeners
        {
            get { return this.conversationDisplayListeners; }
            set { this.conversationDisplayListeners = value; }
        }

        /// <summary>
        /// Defines possible modes for displaying dialogue text.
        /// </summary>
        public enum TextDisplayMode
        {
            FULL, BY_WORD, BY_CHARACTER
        }

        /// <summary>
        /// Translates the conversation text and character name based on the current language set on EasyTalkGameState.Instance.Language.
        /// </summary>
        /// <param name="controller">The Dialogue Controller being used.</param>
        public virtual void TranslateText(DialogueController controller)
        {
            if (controller != null)
            {
                string convoText = currentLine.PreTranslationText;
                string translatedText = controller.GetNodeHandler().Translate(convoText);

                if (this.StandardConvoText != null)
                {
                    this.StandardConvoText.text = translatedText;
                }

#if TEXTMESHPRO_INSTALLED
                if (this.TMPConvoText != null)
                {
                    this.TMPConvoText.text = translatedText;
                }
#endif

                string characterNameText = currentLine.OriginalCharacterName;
                string translatedCharacterName = controller.GetNodeHandler().Translate(characterNameText);

                if (this.StandardCharacterNameText != null)
                {
                    this.StandardCharacterNameText.text = translatedCharacterName;
                }

#if TEXTMESHPRO_INSTALLED
                if (this.TMPCharacterNameText != null)
                {
                    this.TMPCharacterNameText.text = translatedCharacterName;
                }
#endif
            }
        }

        private void OnValidate()
        {
#if TEXTMESHPRO_INSTALLED
            if(this.TMPCharacterNameText == null)
            {
                TMP_Text[] tmpTextComponents = GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text txt in tmpTextComponents)
                {
                    if (txt.gameObject.name.Contains("CharacterName") && txt.gameObject.name.Contains("TMP"))
                    {
                        this.TMPCharacterNameText = txt;
                        break;
                    }
                }
            }

            if(this.TMPConvoText == null)
            {
                TMP_Text[] tmpTextComponents = GetComponentsInChildren<TMP_Text>(true);
                foreach(TMP_Text txt in tmpTextComponents)
                {
                    if(txt.gameObject.name.Contains("Convo") && txt.gameObject.name.Contains("TMP"))
                    {
                        this.TMPConvoText = txt;
                        break;
                    }
                }
            }
#endif
        }
    }

    public enum GibberishMode
    {
        NONE, CYCLE_RANDOM, LOOP_SINGLE, PLAY_RANDOM_PER_ITEM
    }
}