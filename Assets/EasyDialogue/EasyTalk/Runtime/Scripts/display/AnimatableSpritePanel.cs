using EasyTalk.Character;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Display
{
    /// <summary>
    /// Implements logic for handling basic animation of a sprite sequence and displaying a sprite on an Image component.
    /// </summary>
    public class AnimatableSpritePanel : DialoguePanel
    {
        /// <summary>
        /// The Image component to use for displaying the sprite(s).
        /// </summary>
        [SerializeField]
        private Image image;

        /// <summary>
        /// The image/sequence currently being displayed.
        /// </summary>
        private AnimatableDisplayImage animatableImage;

        /// <summary>
        /// The current frame/sprite number of the animation sequence being shown.
        /// </summary>
        private int frameNumber = 0;

        /// <summary>
        /// Determines the direction in which the animation will be incremented. In Ping-pong mode, this number alternates between -1 and +1.
        /// </summary>
        private int increment = 1;

        /// <summary>
        /// The last time the sprite was changed. Used internally to update based on a specific frames-per-second target.
        /// </summary>
        float lastUpdateTime = 0.0f;

        /// <inheritdoc/>
        private void Awake()
        {
            Init();
            HideImmediately();
        }

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            if (image == null)
            {
                image = GetComponent<Image>();
            }
        }

        /// <inheritdoc/>
        private void Update()
        {
            if (animatableImage != null && !isHidden && animatableImage.AnimationMode != AnimationMode.NONE)
            {
                if (Time.time - lastUpdateTime > (1.0f / animatableImage.FrameRate))
                {
                    Sprite newSprite = null;

                    //Move to the next sprite.
                    if (animatableImage.AnimationMode == AnimationMode.LOOP)
                    {
                        frameNumber += 1;

                        if (frameNumber >= animatableImage.Sprites.Count)
                        {
                            frameNumber = 0;
                        }

                        newSprite = animatableImage.Sprites[frameNumber];

                    }
                    else if (animatableImage.AnimationMode == AnimationMode.PING_PONG)
                    {
                        frameNumber += increment;

                        if (increment > 0 && frameNumber >= animatableImage.Sprites.Count)
                        {
                            increment *= -1;
                            frameNumber = Mathf.Max(0, animatableImage.Sprites.Count - 2);
                        }
                        else if (increment < 0 && frameNumber < 0)
                        {
                            increment *= -1;
                            frameNumber = Mathf.Min(1, animatableImage.Sprites.Count - 1);
                        }

                        newSprite = animatableImage.Sprites[frameNumber];
                    }
                    else if (animatableImage.AnimationMode == AnimationMode.RANDOM)
                    {
                        newSprite = animatableImage.Sprites[Random.Range(0, animatableImage.Sprites.Count)];
                    }

                    if (newSprite != null)
                    {
                        image.sprite = newSprite;
                    }

                    lastUpdateTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Sets the AnimatableDisplayImage to display on the panel.
        /// </summary>
        /// <param name="animImage">The AnimatableDisplayImage to use.</param>
        public void SetImageOnPanel(AnimatableDisplayImage animImage)
        {
            if (image != null)
            {
                animatableImage = animImage;
                lastUpdateTime = Time.time;
                frameNumber = 0;

                Sprite sprite;

                if (animatableImage.RandomizeImageWhenShown)
                {
                    //Randomize the starting sprite if configured to do so.
                    frameNumber = Random.Range(0, animatableImage.Sprites.Count);
                    sprite = animatableImage.Sprites[frameNumber];
                }
                else
                {
                    //Use the first sprite.
                    sprite = animatableImage.Sprites[0];
                }

                image.sprite = sprite;
            }
        }
    }
}
