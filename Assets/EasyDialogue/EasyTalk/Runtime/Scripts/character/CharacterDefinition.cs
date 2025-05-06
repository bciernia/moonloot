using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Character
{
    /// <summary>
    /// Defines attributes for a character, such as their name, icons, and portrayal images/spritesheets.
    /// </summary>
    [Serializable]
    public class CharacterDefinition
    {
        /// <summary>
        /// The name of the character.
        /// </summary>
        [SerializeField]
        private string characterName;

        /// <summary>
        /// A collection of displayable icons for the character.
        /// </summary>
        [SerializeField]
        private List<AnimatableDisplayImage> iconsSprites = new List<AnimatableDisplayImage>();

        /// <summary>
        /// A collection of displayable portraits for the character.
        /// </summary>
        [SerializeField]
        private List<AnimatableDisplayImage> portrayalSprites = new List<AnimatableDisplayImage>();

        /// <summary>
        /// Indicates whether the character should use their own unique gibberish audio, rather than the defaults.
        /// </summary>
        [SerializeField]
        private bool overrideDefaultGibberishAudio = false;

        /// <summary>
        /// The collection of gibberish audio clips used for the character.
        /// </summary>
        [SerializeField]
        private List<AudioClip> gibberishAudioClips = new List<AudioClip>();

        /// <summary>
        /// Gets or sets the name of the character.
        /// </summary>
        public string CharacterName
        {
            get { return characterName; }
            set { this.characterName = value; }
        }

        /// <summary>
        /// GEts the List of icons for the character.
        /// </summary>
        public List<AnimatableDisplayImage> IconsSprites
        {
            get { return iconsSprites; }
        }

        /// <summary>
        /// Gets the List of portraits for the character.
        /// </summary>
        public List<AnimatableDisplayImage> PortrayalSprites
        {
            get { return portrayalSprites; }
        }

        public bool OverrideDefaultGibberishAudio
        {
            get { return overrideDefaultGibberishAudio; }
            set { overrideDefaultGibberishAudio = value; }
        }

        /// <summary>
        /// Gets the List of gibberish audio clips used by the character.
        /// </summary>
        public List<AudioClip> GibberishAudioClips
        {
            get { return gibberishAudioClips; }
        }

        /// <summary>
        /// Returns the character's icon which has the specified ID, if it exists.
        /// </summary>
        /// <param name="id">The ID of the icon to retrieve.</param>
        /// <returns>The specified icon, if it exists; otherwise null.</returns>
        public AnimatableDisplayImage GetIconSprite(string id)
        {
            foreach(AnimatableDisplayImage icon in iconsSprites)
            {
                if(icon.ID.Equals(id))
                {
                    return icon;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the character's portrait which has the specified ID, if it exists.
        /// </summary>
        /// <param name="id">The ID of the portrait to retrieve.</param>
        /// <returns>The specified portrait, if it exists; otherwise null.</returns>
        public AnimatableDisplayImage GetPortrayalSprite(string id)
        {
            foreach (AnimatableDisplayImage sprite in portrayalSprites)
            {
                if (sprite.ID.Equals(id))
                {
                    return sprite;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Defines attributes of an image, which can also be animated if it includes more than one sprite.
    /// </summary>
    [Serializable]
    public class AnimatableDisplayImage
    {
        /// <summary>
        /// The ID used to reference the image.
        /// </summary>
        [SerializeField]
        private string id = "";

        /// <summary>
        /// The default target display ID on which the image should be displayed.
        /// </summary>
        [SerializeField]
        private string targetId = "";

        /// <summary>
        /// The sprite sequence used for the image's animation, or a single image in the case of non-animated images.
        /// </summary>
        [SerializeField]
        private List<Sprite> sprites = new List<Sprite>();

        /// <summary>
        /// When set to true, the initial image will be chosen from the sprite list randomly when shown.
        /// </summary>
        [SerializeField]
        private bool randomizeImageWhenShown = false;

        /// <summary>
        /// The animation mode used for the animation sequence when multiple sprites are defined.
        /// </summary>
        [SerializeField]
        private AnimationMode animationMode = AnimationMode.NONE;

        /// <summary>
        /// The number of times per second that the image index will be updated.
        /// </summary>
        [SerializeField]
        private float frameRate = 16;
        
        /// <summary>
        /// Gets or sets the ID of the image.
        /// </summary>
        public string ID
        {
            get { return id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the default target display ID for the image.
        /// </summary>
        public string TargetID
        {
            get { return targetId; }
            set { this.targetId = value; }
        }

        /// <summary>
        /// Gets or sets the List of sprites for the image's sprite sequence.
        /// </summary>
        public List<Sprite> Sprites
        {
            get { return sprites; }
            set { this.sprites = value; }
        }

        /// <summary>
        /// Gets or sets whether the starting image will be selected randomly from the sprite list when shown.
        /// </summary>
        public bool RandomizeImageWhenShown
        {
            get { return randomizeImageWhenShown; }
            set { this.randomizeImageWhenShown = value; }
        }

        /// <summary>
        /// Gets or sets the sprite sequence animation mode to use.
        /// </summary>
        public AnimationMode AnimationMode
        {
            get { return animationMode; }
            set { this.animationMode = value; }
        }

        /// <summary>
        /// Gets or sets the frame rate (in frames per second) for the animation.
        /// </summary>
        public float FrameRate
        {
            get { return frameRate; }
            set { this.frameRate = value; }
        }
    }

    /// <summary>
    /// Defines animation modes for AnimatableDisplayImages.
    /// </summary>
    [Serializable]
    public enum AnimationMode
    {
        NONE, LOOP, PING_PONG, RANDOM
    }
}
