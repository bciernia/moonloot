using EasyTalk.Utils;
using TMPro;
using UnityEngine;

namespace EasyTalk.Display
{
    /// <summary>
    /// A conversation display which can be used on characters in 2D or 3D world space. The display can be set to automatically rotate to face a camera or other object.
    /// </summary>
    public class WorldspaceConversationDisplay : ConversationDisplay
    {
        /// <summary>
        /// Whether the display should automatically point at a transform location.
        /// </summary>
        [Tooltip("If set to true, the Conversation Display will look at the Transform specified in 'Look At'.")]
        [SerializeField]
        private bool pointAtTransform;

        /// <summary>
        /// The transform which the conversation display should point toward whenever pointAtTransform is set to true.
        /// </summary>
        [Tooltip("The Transform the look at.")]
        [SerializeField]
        private Transform lookAt;

        /// <summary>
        /// Update the rotation of the display to point at the lookAt transform if pointAtTransform is set to true.
        /// </summary>
        private void Update()
        {
            if (pointAtTransform)
            {
                this.transform.LookAt(lookAt, Vector3.up);
            }
        }

        /// <summary>
        /// Gets or sets whether the conversation display should automatically look at a transform location.
        /// </summary>
        public bool PointAtTransform
        {
            get { return pointAtTransform; }
            set { pointAtTransform = value; }
        }

        /// <summary>
        /// Gets or sets the transform that the conversation display should point toward whenever pointAtTransform is set to true.
        /// </summary>
        public Transform LookAt
        {
            get { return lookAt; }
            set { lookAt = value; }
        }

        private void OnValidate()
        {
#if TEXTMESHPRO_INSTALLED
            if (this.TMPCharacterNameText == null)
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

            if (this.TMPConvoText == null)
            {
                TMP_Text[] tmpTextComponents = GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text txt in tmpTextComponents)
                {
                    if (txt.gameObject.name.Contains("Conversation") && txt.gameObject.name.Contains("TMP"))
                    {
                        this.TMPConvoText = txt;
                        break;
                    }
                }
            }
#endif
        }
    }
}