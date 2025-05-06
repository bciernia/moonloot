using EasyTalk.Nodes.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

namespace EasyTalk.Display
{
    /// <summary>
    /// A display for obtaining text input from the plpayer during dialogue playback.
    /// </summary>
    public class TextInputDisplay : DialoguePanel
    {
        /// <summary>
        /// The text input field.
        /// </summary>
        [SerializeField]
        private InputField inputField;

#if TEXTMESHPRO_INSTALLED
        /// <summary>
        /// The text input field (TextMesh Pro version).
        /// </summary>
        [SerializeField]
        private TMP_InputField textMeshProInputField;
#endif

        /// <summary>
        /// The PlayerInput node being processed.
        /// </summary>
        private PlayerInputNode inputNode;

        /// <summary>
        /// Whether the input display should be hidden immediately on awake.
        /// </summary>
        private bool hideOnAwake = true;

        /// <inheritdoc>
        private void Awake()
        {
            Init();

            if (hideOnAwake)
            {
                HideImmediately();
            }

            onShowComplete.AddListener(FocusTextInput);

            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.onSubmit.AddListener(TextInputEntered);
                }
            }
            else
            {

#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.onSubmit.AddListener(TextInputEntered);
                }
#else
                if (inputField != null)
                {
                    inputField.onSubmit.AddListener(TextInputEntered);
                }
#endif
            }
        }

        /// <summary>
        /// Gets or sets wether or not the display will b e hidden immediately on Awake().
        /// </summary>
        public bool HideOnAwake
        {
            get { return hideOnAwake; }
            set { this.hideOnAwake = value; }
        }

        /// <summary>
        /// Resets the text input.
        /// </summary>
        public void Reset()
        {
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.text = "";
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.text = "";
                }
#else
                if (inputField != null)
                {
                    inputField.text = "";
                }
#endif
            }
        }

        /// <summary>
        /// Sets the active PlayerInput node. This method also sets the hint/placeholder text configured in the node.
        /// </summary>
        /// <param name="node">The PlayerInput node to use.</param>
        public void SetInputNode(PlayerInputNode node)
        {
            this.inputNode = node;

            if (forceStandardText)
            {
                if (inputField != null)
                {
                    inputField.placeholder.GetComponent<Text>().text = inputNode.HintText;
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    textMeshProInputField.placeholder.GetComponent<TMP_Text>().text = inputNode.HintText;
                }
#else
                if (inputField != null)
                {
                    inputField.placeholder.GetComponent<Text>().text = inputNode.HintText;
                }
#endif
            }
        }

        /// <summary>
        /// Gives the text input focus.
        /// </summary>
        private void FocusTextInput()
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (forceStandardText)
            {
                if (inputField != null)
                {
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    EventSystem.current.SetSelectedGameObject(textMeshProInputField.gameObject);
                }
#else
                if (inputField != null)
                {
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                }
#endif
            }
        }

        /// <summary>
        /// When called, this will call TextInputEntered() with the current text of the input field.
        /// </summary>
        public void TextInputEntered()
        {
            if (forceStandardText)
            {
                if (inputField != null)
                {
                    TextInputEntered(inputField.text);
                }
            }
            else
            {
#if TEXTMESHPRO_INSTALLED
                if (textMeshProInputField != null)
                {
                    TextInputEntered(textMeshProInputField.text);
                }
#else
                if (inputField != null)
                {
                    TextInputEntered(inputField.text);
                }
#endif
            }
        }

        /// <summary>
        /// Sets the text entered by the player on the active PlayerInput node and calls ExecutionCompleted();
        /// </summary>
        /// <param name="text"></param>
        private void TextInputEntered(string text)
        {
            this.inputNode.InputText = text;
            this.inputNode.ExecutionCompleted();
            this.inputNode = null;
            Hide();
        }

        private void OnValidate()
        {
#if TEXTMESHPRO_INSTALLED
            if (this.textMeshProInputField == null)
            {
                TMP_InputField[] tmpTextComponents = GetComponentsInChildren<TMP_InputField>(true);
                foreach (TMP_InputField txt in tmpTextComponents)
                {
                    if (txt.gameObject.name.Contains("InputField") && txt.gameObject.name.Contains("TMP"))
                    {
                        this.textMeshProInputField = txt;
                        break;
                    }
                }
            }
#endif
        }
    }
}
