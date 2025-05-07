using System.Collections;
using System.Collections.Generic;

#if TEXTMESHPRO_INSTALLED
using TMPro;
#endif

using UnityEngine;
using System.Threading;
using UnityEngine.UI;

namespace EasyTalk.Animation
{
    /// <summary>
    /// The UIAnimator can be used to animate UI components with fading or sliding animations.
    /// </summary>
    public class UIAnimator
    {
        /// <summary>
        /// A mapping of component instance IDs to the original alpha values of various UI elements.
        /// </summary>
        private static Dictionary<int, float> storedElementAlphas = new Dictionary<int, float>();

        /// <summary>
        /// Mapping used to keep track of the number of animations running for a given UI element.
        /// </summary>
        public static Dictionary<int, int> itemsAnimationCounter = new Dictionary<int, int>();

        /// <summary>
        /// Returns true if the specified UI element is currently being animated.
        /// </summary>
        /// <param name="id">The instance ID of the UI element to check.</param>
        /// <returns>Whether the specified UI element is animating.</returns>
        public static bool IsItemAnimating(int id)
        {
            if (itemsAnimationCounter.ContainsKey(id))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the specified GameObject is currently being animated.
        /// </summary>
        /// <param name="gameObject">The GameObject to check.</param>
        /// <returns>Whether the specified GameObject is animating.</returns>
        public static bool IsItemAnimating(GameObject gameObject)
        {
            if (itemsAnimationCounter.ContainsKey(gameObject.GetInstanceID()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Increments the running animation counter for the specified ID.
        /// </summary>
        /// <param name="id">The instance ID of the object an animation is starting on.</param>
        private static void StartingItemAnimation(int id)
        {
            if (itemsAnimationCounter.ContainsKey(id))
            {
                itemsAnimationCounter[id]++;
            }
            else
            {
                itemsAnimationCounter.Add(id, 1);
            }
        }

        /// <summary>
        /// Decrements the running animation counter for the specified ID.
        /// </summary>
        /// <param name="id">The isntance ID of the object an animation is ending on.</param>
        private static void FinishedItemAnimation(int id)
        {
            if (itemsAnimationCounter.ContainsKey(id))
            {
                itemsAnimationCounter[id]--;

                if (itemsAnimationCounter[id] == 0)
                {
                    itemsAnimationCounter.Remove(id);
                }
            }
        }

        /// <summary>
        /// Fades text to a specified alpha value over the specified time period.
        /// </summary>
        /// <param name="text">The text element to animate the alpha value of.</param>
        /// <param name="duration">The duration, in seconds, to spend animating the text.</param>
        /// <param name="finalAlpha">The final alpha value desired for the text.</param>
        /// <param name="curve">An animation curve controlling how quickly the text is faded.</param>
        /// <returns></returns>
    #if TEXTMESHPRO_INSTALLED
        public static IEnumerator FadeText(TMP_Text text, float duration, float finalAlpha, AnimationCurve curve = null)
    #else
        public static IEnumerator FadeText(Text text, float duration, float finalAlpha, AnimationCurve curve = null)
    #endif
        {
            StartingItemAnimation(text.GetInstanceID());
            float startTime = Time.time;
            Color oldColor = text.color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, finalAlpha);

            while (Thread.CurrentThread.IsAlive && (Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }
                text.color = Color.Lerp(oldColor, newColor, progress);

                yield return new WaitForEndOfFrame();
            }

            text.color = newColor;
            FinishedItemAnimation(text.GetInstanceID());
        }

        /// <summary>
        /// Fade text to a specified alpha value immediately.
        /// </summary>
        /// <param name="text">The text element to fade.</param>
        /// <param name="finalAlpha">The final alpha value.</param>
    #if TEXTMESHPRO_INSTALLED
        public static void FadeTextImmediately(TMP_Text text, float finalAlpha)
    #else
        public static void FadeTextImmediately(Text text, float finalAlpha)
    #endif
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, finalAlpha);
        }

        /// <summary>
        /// Fade text in to its original alpha value. (This assumes that the original alpha value is stored)
        /// </summary>
        /// <param name="text">The text element to fade in.</param>
        /// <param name="duration">The duration of the fade in animation in seconds.</param>
        /// <param name="curve">The animation curve used to control the animation timing.</param>
        /// <returns></returns>
    #if TEXTMESHPRO_INSTALLED
        public static IEnumerator FadeTextIn(TMP_Text text, float duration, AnimationCurve curve = null)
    #else
        public static IEnumerator FadeTextIn(Text text, float duration, AnimationCurve curve = null)
    #endif
        {
            yield return FadeText(text, duration, GetOriginalOpacity(text.GetInstanceID()), curve);
        }

        /// <summary>
        /// Fades text in to its original alpha value immediately.
        /// </summary>
        /// <param name="text">The text element to fade in.</param>
    #if TEXTMESHPRO_INSTALLED
        public static void FadeTextInImmediately(TMP_Text text)
    #else
        public static void FadeTextInImmediately(Text text)
    #endif
        {
            FadeTextImmediately(text, GetOriginalOpacity(text.GetInstanceID()));
        }

        /// <summary>
        /// Fades text out to be completely transparent over a specified duration.
        /// </summary>
        /// <param name="text">The text element to fade out.</param>
        /// <param name="duration">The duration of the animation, in seconds.</param>
        /// <param name="curve">The animation curve used to control the animation timing.</param>
        /// <returns></returns>
    #if TEXTMESHPRO_INSTALLED
        public static IEnumerator FadeTextOut(TMP_Text text, float duration, AnimationCurve curve = null)
    #else
        public static IEnumerator FadeTextOut(Text text, float duration, AnimationCurve curve = null)
    #endif
        {
            StoreOriginalOpacity(text, text.color.a);
            yield return FadeText(text, duration, 0.0f, curve);
        }

        /// <summary>
        /// Fades text out to be completely transparent immediately.
        /// </summary>
        /// <param name="text">The text element to fade out.</param>
    #if TEXTMESHPRO_INSTALLED
        public static void FadeTextOutImmediately(TMP_Text text)
    #else
        public static void FadeTextOutImmediately(Text text)
    #endif
        {
            StoreOriginalOpacity(text, text.color.a);
            FadeTextImmediately(text, 0.0f);
        }

        /// <summary>
        /// Fades an image to a specified alpha value over time.
        /// </summary>
        /// <param name="image">The image to animate.</param>
        /// <param name="duration">The duration of the fade animation, in seconds.</param>
        /// <param name="finalAlpha">The final alpha value to fade to.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator FadeImage(Image image, float duration, float finalAlpha, AnimationCurve curve = null)
        {
            StartingItemAnimation(image.GetInstanceID());

            float startTime = Time.time;
            Color oldColor = image.color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, finalAlpha);

            while (Thread.CurrentThread.IsAlive && (Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }
                image.color = Color.Lerp(oldColor, newColor, progress);

                yield return new WaitForEndOfFrame();
            }

            image.color = newColor;
            FinishedItemAnimation(image.GetInstanceID());
        }

        /// <summary>
        /// Fades an image to the specified alpha value immediately.
        /// </summary>
        /// <param name="image">The image to fade.</param>
        /// <param name="finalAlpha">The final alpha value to use.</param>
        public static void FadeImageImmediately(Image image, float finalAlpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, finalAlpha);
        }

        /// <summary>
        /// Fades an image in to its original alpha value. (This assumes that the original alpha value is stored)
        /// </summary>
        /// <param name="image">The image to fade in.</param>
        /// <param name="duration">The duration of the fade in animation, in seconds.</param>
        /// <param name="curve">The animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator FadeImageIn(Image image, float duration, AnimationCurve curve = null)
        {
            yield return FadeImage(image, duration, GetOriginalOpacity(image.GetInstanceID()), curve);
        }

        /// <summary>
        /// Fades an image in to its stored alpha value immediately.
        /// </summary>
        /// <param name="image">The image to fade in.</param>
        public static void FadeImageInImmediately(Image image)
        {
            FadeImageImmediately(image, GetOriginalOpacity(image.GetInstanceID()));
        }

        /// <summary>
        /// Fades an image out to be completely transparent over a specified duration.
        /// </summary>
        /// <param name="image">The image to fade out.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator FadeImageOut(Image image, float duration, AnimationCurve curve = null)
        {
            StoreOriginalOpacity(image, image.color.a);
            yield return FadeImage(image, duration, 0.0f, curve);
        }

        /// <summary>
        /// Fades an image out to be completely transparent immediately.
        /// </summary>
        /// <param name="image">The image to make transparent.</param>
        public static void FadeImageOutImmediately(Image image)
        {
            StoreOriginalOpacity(image, image.color.a);
            FadeImageImmediately(image, 0.0f);
        }

        /// <summary>
        /// Fades all text and image components which are components of, or child components of the specified GameObject to an alpha value over a specified duration.
        /// </summary>
        /// <param name="gameObject">The GameObject to fade.</param>
        /// <param name="duration">The duration of the fade animation, in seconds.</param>
        /// <param name="finalAlpha">The final desired alpha value for the components being faded.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator Fade(GameObject gameObject, float duration, float finalAlpha, AnimationCurve curve = null)
        {
            StartingItemAnimation(gameObject.GetInstanceID());
            Image[] images = gameObject.GetComponentsInChildren<Image>();

        #if TEXTMESHPRO_INSTALLED
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        #else
            Text[] texts = gameObject.GetComponentsInChildren<Text>();
        #endif

            Dictionary<int, float> oldElementAlphas = new Dictionary<int, float>();

            float startTime = Time.time;

            while (Thread.CurrentThread.IsAlive && (Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }

                foreach (Image image in images)
                {
                    if (image.GetComponent<Mask>() != null) { continue; }

                    float oldOpacity;
                    if (!oldElementAlphas.ContainsKey(image.GetInstanceID()))
                    {
                        oldElementAlphas.Add(image.GetInstanceID(), image.color.a);
                    }

                    oldOpacity = oldElementAlphas[image.GetInstanceID()];

                    Color oldColor = new Color(image.color.r, image.color.g, image.color.b, oldOpacity);
                    Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, finalAlpha);
                    image.color = Color.Lerp(oldColor, newColor, progress);
                }

            #if TEXTMESHPRO_INSTALLED
                foreach (TMP_Text text in texts)
            #else
                foreach (Text text in texts)
            #endif
                {
                    float oldOpacity;
                    if (!oldElementAlphas.ContainsKey(text.GetInstanceID()))
                    {
                        oldElementAlphas.Add(text.GetInstanceID(), text.color.a);
                    }

                    oldOpacity = oldElementAlphas[text.GetInstanceID()];

                    Color oldColor = new Color(text.color.r, text.color.g, text.color.b, oldOpacity);
                    Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, finalAlpha);
                    text.color = Color.Lerp(oldColor, newColor, progress);
                }

                yield return new WaitForEndOfFrame();
            }

            foreach (Image image in images)
            {
                if (image.GetComponent<Mask>() != null) { continue; }
                image.color = new Color(image.color.r, image.color.g, image.color.b, finalAlpha);
            }

        #if TEXTMESHPRO_INSTALLED
            foreach (TMP_Text text in texts)
        #else
            foreach (Text text in texts)
        #endif
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, finalAlpha);
            }

            oldElementAlphas.Clear();
            FinishedItemAnimation(gameObject.GetInstanceID());
        }

        /// <summary>
        /// Immediately fades all text and image elements which are components of, or child components of the specified GameObject to the specified alpha value.
        /// </summary>
        /// <param name="gameObject">The GameObject to fade.</param>
        /// <param name="finalAlpha">The final desired alpha value.</param>
        public static void FadeImmediately(GameObject gameObject, float finalAlpha)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>();

        #if TEXTMESHPRO_INSTALLED
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        #else
            Text[] texts = gameObject.GetComponentsInChildren<Text>();
        #endif

            foreach (Image image in images)
            {
                if (image.GetComponent<Mask>() != null) { continue; }
                image.color = new Color(image.color.r, image.color.g, image.color.b, finalAlpha);
            }

        #if TEXTMESHPRO_INSTALLED
            foreach (TMP_Text text in texts)
        #else
            foreach (Text text in texts)
        #endif
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, finalAlpha);
            }
        }

        /// <summary>
        /// Clears the stored alpha values for the specified instance ID.
        /// </summary>
        /// <param name="instanceID">The instance ID of the component or GameObject to clear.</param>
        public static void ClearStoredAlpha(int instanceID)
        {
            if (storedElementAlphas.ContainsKey(instanceID))
            {
                storedElementAlphas.Remove(instanceID);
            }
        }

        /// <summary>
        /// Clears the stored alpha values for all components and child components of the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to clear stored alpha values for.</param>
        public static void ClearStoredAlphas(GameObject gameObject)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>();

        #if TEXTMESHPRO_INSTALLED
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        #else
            Text[] texts = gameObject.GetComponentsInChildren<Text>();
        #endif

            foreach (Image image in images)
            {
                ClearStoredAlpha(image.GetInstanceID());
            }

        #if TEXTMESHPRO_INSTALLED
            foreach (TMP_Text text in texts)
        #else
            foreach (Text text in texts)
        #endif
            {
                ClearStoredAlpha(text.GetInstanceID());
            }
        }

        /// <summary>
        /// Clears all stored alpha values.
        /// </summary>
        public static void ClearAllStoredAlphas()
        {
            storedElementAlphas.Clear();
        }

        /// <summary>
        /// Fades in the components and child components of the specified GameObject to their original stored alpha values over the specified duration.
        /// </summary>
        /// <param name="gameObject">The GameObject to fade in.</param>
        /// <param name="duration">The duration of the fade in animation, in seconds.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator FadeIn(GameObject gameObject, float duration, AnimationCurve curve = null)
        {
            StartingItemAnimation(gameObject.GetInstanceID());
            Image[] images = gameObject.GetComponentsInChildren<Image>();

        #if TEXTMESHPRO_INSTALLED
                TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        #else
                Text[] texts = gameObject.GetComponentsInChildren<Text>();
        #endif

            Dictionary<int, float> oldElementAlphas = new Dictionary<int, float>();

            float startTime = Time.time;

            while (Thread.CurrentThread.IsAlive && (Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }

                foreach (Image image in images)
                {
                    if (image.GetComponent<Mask>() != null) { continue; }

                    if (!oldElementAlphas.ContainsKey(image.GetInstanceID()))
                    {
                        oldElementAlphas.Add(image.GetInstanceID(), image.color.a);
                    }

                    float oldOpacity = oldElementAlphas[image.GetInstanceID()];

                    Color oldColor = new Color(image.color.r, image.color.g, image.color.b, oldOpacity);
                    Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, GetOriginalOpacity(image.GetInstanceID()));

                    image.color = Color.Lerp(oldColor, newColor, progress);
                }

            #if TEXTMESHPRO_INSTALLED
                foreach (TMP_Text text in texts)
            #else
                foreach (Text text in texts)
            #endif
                {
                    if (!oldElementAlphas.ContainsKey(text.GetInstanceID()))
                    {
                        oldElementAlphas.Add(text.GetInstanceID(), text.color.a);
                    }

                    float oldOpacity = oldElementAlphas[text.GetInstanceID()];

                    Color oldColor = new Color(text.color.r, text.color.g, text.color.b, oldOpacity);
                    Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, GetOriginalOpacity(text.GetInstanceID()));
                    text.color = Color.Lerp(oldColor, newColor, progress);
                }

                yield return new WaitForEndOfFrame();
            }

            foreach (Image image in images)
            {
                if (image.GetComponent<Mask>() != null) { continue; }
                image.color = new Color(image.color.r, image.color.g, image.color.b, GetOriginalOpacity(image.GetInstanceID()));
            }

        #if TEXTMESHPRO_INSTALLED
            foreach (TMP_Text text in texts)
        #else
            foreach (Text text in texts)
        #endif
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, GetOriginalOpacity(text.GetInstanceID()));
            }

            oldElementAlphas.Clear();
            FinishedItemAnimation(gameObject.GetInstanceID());
        }

        /// <summary>
        /// Fades in all text and image components and child components of the specified GameObject to their original stored alpha values immediately.
        /// </summary>
        /// <param name="gameObject">The GameObject to fade in.</param>
        public static void FadeInImmediately(GameObject gameObject)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>();

        #if TEXTMESHPRO_INSTALLED
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        #else
            Text[] texts = gameObject.GetComponentsInChildren<Text>();
        #endif

            foreach (Image image in images)
            {
                if (image.GetComponent<Mask>() != null) { continue; }
                image.color = new Color(image.color.r, image.color.g, image.color.b, GetOriginalOpacity(image.GetInstanceID()));
            }

        #if TEXTMESHPRO_INSTALLED
            foreach (TMP_Text text in texts)
        #else
            foreach (Text text in texts)
        #endif
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, GetOriginalOpacity(text.GetInstanceID()));
            }
        }

        /// <summary>
        /// Fades out all text and image components and child components of the specified GameObject to be completely transparent over the specified duration.
        /// </summary>
        /// <param name="gameObject">The GameObject to fade out.</param>
        /// <param name="duration">The duration of the fade out animation, in seconds.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator FadeOut(GameObject gameObject, float duration, AnimationCurve curve = null)
        {
            StoreOriginalOpacities(gameObject);
            yield return Fade(gameObject, duration, 0.0f, curve);
        }

        /// <summary>
        /// Fades out all text and image components and child components of the specified GameObject to be completely transparent immediately.
        /// </summary>
        /// <param name="gameObject">The GameObject to fade out.</param>
        public static void FadeOutImmediately(GameObject gameObject)
        {
            StoreOriginalOpacities(gameObject);
            FadeImmediately(gameObject, 0.0f);
        }

        /// <summary>
        /// Returns the original opacity value which is stored for the specified instance ID, or a value of 1.0 if no stored value is found.
        /// </summary>
        /// <param name="instanceId">The instance ID to retrieve a stored opacity value for.</param>
        /// <returns>The original opacity value of the specified instance ID, or 1.0 if no value is stored.</returns>
        private static float GetOriginalOpacity(int instanceId)
        {
            if (storedElementAlphas.ContainsKey(instanceId))
            {
                return storedElementAlphas[instanceId];
            }

            return 1.0f;
        }

        /// <summary>
        /// Stores an opacity value for the specified object instance.
        /// </summary>
        /// <param name="obj">The object to store an opacity value of.</param>
        /// <param name="opacity">The opacity value to store.</param>
        private static void StoreOriginalOpacity(Object obj, float opacity)
        {
            if (!storedElementAlphas.ContainsKey(obj.GetInstanceID()))
            {
                storedElementAlphas.Add(obj.GetInstanceID(), opacity);
            }
        }

        /// <summary>
        /// Stores opacity values for each text and image component in the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to store opacity values of.</param>
        public static void StoreOriginalOpacities(GameObject gameObject)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>();

        #if TEXTMESHPRO_INSTALLED
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        #else
            Text[] texts = gameObject.GetComponentsInChildren<Text>();
        #endif

            foreach (Image image in images)
            {
                StoreOriginalOpacity(image, image.color.a);
            }

        #if TEXTMESHPRO_INSTALLED
            foreach (TMP_Text text in texts)
        #else
            foreach (Text text in texts)
        #endif
            {
                StoreOriginalOpacity(text, text.color.a);
            }
        }

        /// <summary>
        /// Slides the specified RectTransform into a Canvas' bounds over a specified period of time.
        /// </summary>
        /// <param name="canvas">The Canvas which the transform should slide into.</param>
        /// <param name="transform">The RectTransform of the component to slide in.</param>
        /// <param name="duration">The duration of the slide animation.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <param name="margin">An optional additional margin offset for the final slide in position.</param>
        /// <returns></returns>
        public static IEnumerator SlideInComponent(Canvas canvas, RectTransform transform, float duration, AnimationCurve curve = null, float margin = 0.0f)
        {
            StartingItemAnimation(transform.gameObject.GetInstanceID());
            Vector3 originalPos = transform.localPosition;
            Vector3 finalPos = FindPositionToShow(canvas, transform, margin);

            float startTime = Time.time;

            while ((Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }
                transform.localPosition = Vector3.Lerp(originalPos, finalPos, progress);

                yield return new WaitForEndOfFrame();
            }

            transform.localPosition = finalPos;
            FinishedItemAnimation(transform.gameObject.GetInstanceID());
        }

        /// <summary>
        /// Slides the specified RectTransform into a Canvas' bounds immediately.
        /// </summary>
        /// <param name="canvas">The Canvas which the transform should slide into.</param>
        /// <param name="transform">The RectTransform of the component to slide in.</param>
        /// <param name="margin">An optional additional margin offset for the final slide in position.</param>
        public static void SlideInComponentImmediately(Canvas canvas, RectTransform transform, float margin = 0.0f)
        {
            transform.localPosition = FindPositionToShow(canvas, transform, margin);
        }

        /// <summary>
        /// Slides the specified RectTransform out of a Canvas' bounds over a specified period of time.
        /// </summary>
        /// <param name="canvas">The Canvas which the transform should slide out of.</param>
        /// <param name="transform">The RectTransform of the component to slide out.</param>
        /// <param name="direction">The direction that the component should slide out.</param>
        /// <param name="duration">The duration of the animation, in seconds.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator SlideOutComponent(Canvas canvas, RectTransform transform, SlideDirection direction, float duration, AnimationCurve curve = null)
        {
            StartingItemAnimation(transform.gameObject.GetInstanceID());
            Vector3 originalPos = transform.localPosition;
            Vector3 finalPos = FindPositionToHide(canvas, transform, direction);

            float startTime = Time.time;

            while ((Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }
                transform.localPosition = Vector3.Lerp(originalPos, finalPos, progress);

                yield return new WaitForEndOfFrame();
            }

            transform.localPosition = finalPos;
            FinishedItemAnimation(transform.gameObject.GetInstanceID());
        }

        /// <summary>
        /// Slides the specified RectTransform out of a Canvas' bounds immediately.
        /// </summary>
        /// <param name="canvas">The Canvas which the transform should slide out of.</param>
        /// <param name="transform">The RectTransform of the component to slide out.</param>
        /// <param name="direction">The direction that the component should slide out.</param>
        public static void SlideOutComponentImmediately(Canvas canvas, RectTransform transform, SlideDirection direction)
        {
            transform.localPosition = FindPositionToHide(canvas, transform, direction);
        }

        /// <summary>
        /// Calculates the position to move the specified RectTransform to in order for it to be hidden and outside of the Canvas provided.
        /// </summary>
        /// <param name="canvas">The Canvas which the RectTransform should be hidden in.</param>
        /// <param name="transform">The RectTransform to hide.</param>
        /// <param name="direction">The direction to slide the RectTransform in when hiding it.</param>
        /// <returns>The position to move the RectTransform to so that it is hidden.</returns>
        private static Vector3 FindPositionToHide(Canvas canvas, RectTransform transform, SlideDirection direction)
        {
            Vector3[] objectCorners = new Vector3[4];
            transform.GetLocalCorners(objectCorners);

            RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
            Vector3[] canvasCorners = new Vector3[4];
            canvasTransform.GetWorldCorners(canvasCorners);

            canvasCorners[0] = transform.InverseTransformPoint(canvasCorners[0]);
            canvasCorners[1] = transform.InverseTransformPoint(canvasCorners[1]);
            canvasCorners[2] = transform.InverseTransformPoint(canvasCorners[2]);
            canvasCorners[3] = transform.InverseTransformPoint(canvasCorners[3]);

            switch (direction)
            {
                case SlideDirection.LEFT:
                    return transform.localPosition - new Vector3(objectCorners[2].x - canvasCorners[0].x, 0.0f, 0.0f);
                case SlideDirection.RIGHT:
                    return transform.localPosition + new Vector3(canvasCorners[2].x - objectCorners[0].x, 0.0f, 0.0f);
                case SlideDirection.UP:
                    return transform.localPosition + new Vector3(0.0f, canvasCorners[2].y - objectCorners[0].y, 0.0f);
                case SlideDirection.DOWN:
                    return transform.localPosition - new Vector3(0.0f, objectCorners[2].y - canvasCorners[0].y, 0.0f);
            }

            return transform.localPosition;
        }

        /// <summary>
        /// Calculates the position to move the specified RectTransform to in order for it to be shown inside the Canvas provided.
        /// </summary>
        /// <param name="canvas">The Canvas which the RectTransform should be shown in.</param>
        /// <param name="transform">The RectTransform to show.</param>
        /// <param name="margin">An option margin offset.</param>
        /// <returns>The position which the RectTransform should be moved to in order to show it.</returns>
        private static Vector3 FindPositionToShow(Canvas canvas, RectTransform transform, float margin = 0.0f)
        {
            Vector3[] optionCorners = new Vector3[4];
            transform.GetLocalCorners(optionCorners);

            RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
            Vector3[] canvasCorners = new Vector3[4];
            canvasTransform.GetWorldCorners(canvasCorners);

            canvasCorners[0] = transform.InverseTransformPoint(canvasCorners[0]);
            canvasCorners[1] = transform.InverseTransformPoint(canvasCorners[1]);
            canvasCorners[2] = transform.InverseTransformPoint(canvasCorners[2]);
            canvasCorners[3] = transform.InverseTransformPoint(canvasCorners[3]);

            if (optionCorners[2].x > canvasCorners[2].x)
            {
                //Top right is to the right of the screen, so we have to slide left.
                return transform.localPosition - new Vector3((optionCorners[2].x - canvasCorners[2].x) + margin, 0.0f, 0.0f);
            }
            else if (optionCorners[0].x < canvasCorners[0].x)
            {
                //Bottom left is to the left of the screen, so we have to slide right.
                return transform.localPosition + new Vector3((canvasCorners[0].x - optionCorners[0].x) + margin, 0.0f, 0.0f);
            }
            else if (optionCorners[0].y < canvasCorners[0].y)
            {
                //Bottom left is below the bottom of the screen, so we have to slide up.
                return transform.localPosition + new Vector3(0.0f, (canvasCorners[0].y - optionCorners[0].y) + margin, 0.0f);
            }
            else if (optionCorners[1].y > canvasCorners[1].y)
            {
                //Top left is above the top of the screen, so we have to slide down. 
                return transform.localPosition - new Vector3(0.0f, (optionCorners[2].y - canvasCorners[2].y) + margin, 0.0f);
            }

            else return transform.localPosition;
        }

        /// <summary>
        /// Slides the specified RectTransform to a position over the specified duration.
        /// </summary>
        /// <param name="transform">The RectTransform to slide.</param>
        /// <param name="newPos">The position to move the RectTransform to.</param>
        /// <param name="duration">The duration of the animation.</param>
        /// <param name="curve">An animation curve used to control the animation timing.</param>
        /// <returns></returns>
        public static IEnumerator SlideComponentToPosition(RectTransform transform, Vector3 newPos, float duration, AnimationCurve curve = null)
        {
            StartingItemAnimation(transform.gameObject.GetInstanceID());
            float startTime = Time.time;
            Vector3 oldPos = transform.localPosition;

            while (Thread.CurrentThread.IsAlive && (Time.time - startTime) < duration)
            {
                float progress = (Time.time - startTime) / duration;
                if (curve != null) { progress = curve.Evaluate(progress); }
                transform.localPosition = Vector3.Lerp(oldPos, newPos, progress);

                yield return new WaitForEndOfFrame();
            }

            transform.localPosition = newPos;
            FinishedItemAnimation(transform.gameObject.GetInstanceID());
        }

        /// <summary>
        /// Slides the specified RectTransform to a position immediately.
        /// </summary>
        /// <param name="transform">The RectTransform to slide.</param>
        /// <param name="newPos">The position to move the RectTransform to.</param>
        public static void SlideComponentToPositionImmediately(RectTransform transform, Vector3 newPos)
        {
            transform.localPosition = newPos;
        }

        /// <summary>
        /// Returns the corresponding SlideDirection associated with the specified Animation type.
        /// </summary>
        /// <param name="animation">The animation type to get a slide direction for.</param>
        /// <returns>The associated slide direction for the specified animation, or NONE if there is none.</returns>
        public static UIAnimator.SlideDirection GetSlideDirection(UIAnimator.Animation animation)
        {
            switch (animation)
            {
                case Animation.SLIDE_LEFT: return SlideDirection.LEFT;
                case Animation.SLIDE_RIGHT: return SlideDirection.RIGHT;
                case Animation.SLIDE_UP: return SlideDirection.UP;
                case Animation.SLIDE_DOWN: return SlideDirection.DOWN;
            }

            return SlideDirection.NONE;
        }

        /// <summary>
        /// An enum of possible slide animation directions.
        /// </summary>
        public enum SlideDirection { NONE, LEFT, RIGHT, UP, DOWN }

        /// <summary>
        /// An enum of supported animation types.
        /// </summary>
        public enum Animation { NONE, SLIDE_LEFT, SLIDE_RIGHT, SLIDE_UP, SLIDE_DOWN, FADE }
    }
}