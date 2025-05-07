using EasyTalk.Character;
using EasyTalk.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Settings
{
    [CustomEditor(typeof(CharacterLibrary))]
    public class CharacterLibraryEditor : UnityEditor.Editor
    {
        private static List<CharacterDefinitionExpansionInfo> foldoutSettings = new List<CharacterDefinitionExpansionInfo>();

        private static Color charBGColorA = new Color(0.0f, 0.0f, 0.0f);
        private static Color imageColorAA = new Color(0.2f, 0.2f, 0.2f);
        private static Color imageColorAB = new Color(0.3f, 0.3f, 0.3f);

        public override void OnInspectorGUI()
        {
            CharacterLibrary lib = (CharacterLibrary)target;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Character Definitions"), EditorStyles.boldLabel);
            if(GUILayout.Button(new GUIContent("+"), GUILayout.MaxWidth(20.0f)))
            {
                lib.Characters.Add(new CharacterDefinition());
            }
            EditorGUILayout.EndHorizontal();

            //For each character definition, create fields
            for (int i = 0; i < lib.Characters.Count; i++)
            {
                CreateCharacterFoldout(lib, i);
                EditorGUILayout.Space();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(lib);
            }
        }

        private void CreateCharacterFoldout(CharacterLibrary lib, int characterIdx)
        {
            if (foldoutSettings.Count <= characterIdx)
            {
                foldoutSettings.Add(new CharacterDefinitionExpansionInfo());
            }

            Color bgColor = charBGColorA;
            EditorGUI.DrawRect(EditorGUILayout.BeginVertical(), bgColor);
            CharacterDefinitionExpansionInfo foldoutInfo = foldoutSettings[characterIdx];
            CharacterDefinition def = (CharacterDefinition)lib.Characters[characterIdx];

            //Create character foldout group
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("-"), GUILayout.MaxWidth(20.0f)))
            {
                lib.Characters.RemoveAt(characterIdx);
                characterIdx--;
            }
            foldoutInfo.expandSettings = EditorGUILayout.Foldout(foldoutInfo.expandSettings, def.CharacterName != null ? def.CharacterName : "Character " + (characterIdx + 1));
            EditorGUILayout.EndHorizontal();

            if (foldoutInfo.expandSettings)
            {
                CreateCharacterSettingsFields(def, characterIdx, foldoutInfo);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void CreateCharacterSettingsFields(CharacterDefinition def, int characterIndex, CharacterDefinitionExpansionInfo foldoutInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;
            def.CharacterName = EditorGUILayout.TextField(new GUIContent("Character Name", "The name of the character the configuration is for."), def.CharacterName);

            CreateIconFoldout(def, characterIndex, foldoutInfo);
            CreateImageFoldout(def, characterIndex, foldoutInfo);
            CreateAudioSettings(def, foldoutInfo);

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        private void CreateAudioSettings(CharacterDefinition def, CharacterDefinitionExpansionInfo foldoutInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            foldoutInfo.expandAudio = EditorGUILayout.Foldout(foldoutInfo.expandAudio, new GUIContent("Audio", "Contains configurations for character gibberish audio clips."));
            if (GUILayout.Button(new GUIContent("+", "Adds a new gibberish audio clip for the character."), GUILayout.MaxWidth(20.0f)))
            {
                def.GibberishAudioClips.Add(null);
            }
            EditorGUILayout.EndHorizontal();

            if(foldoutInfo.expandAudio)
            {
                def.OverrideDefaultGibberishAudio = EditorGUILayout.Toggle(new GUIContent("Override Gibberish?"), def.OverrideDefaultGibberishAudio);

                if (def.OverrideDefaultGibberishAudio)
                {
                    ETGUI.DrawLineSeparator();

                    for (int i = 0; i < def.GibberishAudioClips.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        def.GibberishAudioClips[i] = EditorGUILayout.ObjectField(new GUIContent("Gibberish Clip " + (i + 1)), def.GibberishAudioClips[i], typeof(AudioClip), false) as AudioClip;

                        if (GUILayout.Button(new GUIContent("-", "Remove this clip."), GUILayout.MaxWidth(20.0f)))
                        {
                            def.GibberishAudioClips.RemoveAt(i);
                            i--;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        private void CreateIconFoldout(CharacterDefinition def, int characterIndex, CharacterDefinitionExpansionInfo foldoutInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            foldoutInfo.expandIcons = EditorGUILayout.Foldout(foldoutInfo.expandIcons, new GUIContent("Icons", "Contains configurations for character icons."));
            if (GUILayout.Button(new GUIContent("+", "Adds a new configurable icon for."), GUILayout.MaxWidth(20.0f)))
            {
                def.IconsSprites.Add(new AnimatableDisplayImage());
            }
            EditorGUILayout.EndHorizontal();

            if (foldoutInfo.expandIcons)
            {
                ETGUI.DrawLineSeparator();

                for (int j = 0; j < def.IconsSprites.Count; j++)
                {
                    CreateImageFoldoutDetails("Icon", def, characterIndex, ref j, def.IconsSprites, foldoutInfo.iconFoldoutSettings);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void CreateImageFoldout(CharacterDefinition def, int characterIndex, CharacterDefinitionExpansionInfo foldoutInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            foldoutInfo.expandImages = EditorGUILayout.Foldout(foldoutInfo.expandImages, new GUIContent("Portrayals", "Contains configurations for visual portrayals of characters."));
            if (GUILayout.Button(new GUIContent("+", "Adds a new configurable portrayal."), GUILayout.MaxWidth(20.0f)))
            {
                def.PortrayalSprites.Add(new AnimatableDisplayImage());
            }
            EditorGUILayout.EndHorizontal();

            if (foldoutInfo.expandImages)
            {
                ETGUI.DrawLineSeparator();

                for (int j = 0; j < def.PortrayalSprites.Count; j++)
                {
                    CreateImageFoldoutDetails("Portrayal", def, characterIndex, ref j, def.PortrayalSprites, foldoutInfo.imageFoldoutSettings);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void CreateImageFoldoutDetails(string labelPrefix, CharacterDefinition def, int characterIndex, ref int imageIndex, List<AnimatableDisplayImage> imageList, ImageExpansionInfo imageFoldoutSettings)
        {
            AnimatableDisplayImage image = imageList[imageIndex];

            if (imageFoldoutSettings.imageFoldouts.Count <= imageIndex)
            {
                imageFoldoutSettings.imageFoldouts.Add(false);
                imageFoldoutSettings.expandImageSprites.Add(false);
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            Color iconColor = (imageIndex % 2 == 0) ? imageColorAA: imageColorAB;

            EditorGUI.DrawRect(EditorGUILayout.BeginVertical(), iconColor);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(40.0f);
            if (GUILayout.Button(new GUIContent("-", "Remove the " + labelPrefix), GUILayout.MaxWidth(20.0f)))
            {
                imageList.RemoveAt(imageIndex);
                imageIndex--;

                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                imageFoldoutSettings.imageFoldouts[imageIndex] = EditorGUILayout.Foldout(imageFoldoutSettings.imageFoldouts[imageIndex], image.ID != null ? image.ID : labelPrefix + " " + (imageIndex + 1));

                //CreatePreview(icon);

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                if (imageFoldoutSettings.imageFoldouts[imageIndex])
                {
                    CreateAnimatableImageFields(labelPrefix, image, imageIndex, imageFoldoutSettings);
                }
            }

            EditorGUILayout.EndVertical();
            ETGUI.DrawLineSeparator();
            EditorGUI.indentLevel++;
        }

        private void CreateAnimatableImageFields(string labelPrefix, AnimatableDisplayImage image, int imageIndex, ImageExpansionInfo imageFoldoutSettings)
        {
            image.ID = EditorGUILayout.TextField(new GUIContent(labelPrefix + " ID", "The ID of the configured image. This ID needs to be unique within each character, but multiple characters can have configurations with the same ID."), image.ID);

            if (labelPrefix.Equals("Portrayal"))
            {
                image.TargetID = EditorGUILayout.TextField(new GUIContent("Default Target ID", "The default Display ID of the target display where the image/sprite animation will be shown, unless overridden."), image.TargetID);
            }
            
            image.AnimationMode = (Character.AnimationMode)EditorGUILayout.EnumPopup(new GUIContent("Animation Mode", "The animation mode for the image. " +
                "If using a single image, this doesn't matter. For an animated sprite set, the options are as follows:" +
                "\nNONE - The image will not be animated." +
                "\nLOOP - The animation will move to the next sprite in the sequence. When the last frame is reached, the sequence will be reset and will start at the first sprite again." +
                "\nPING_PONG - Whenever the last sprite is reached, the sequence will reverse direction." +
                "\nRANDOM - The next frame in the sequence will be chosen randomly each time the sprite is changed."), image.AnimationMode);

            if (image.AnimationMode != Character.AnimationMode.NONE)
            {
                image.FrameRate = EditorGUILayout.FloatField(new GUIContent("Frame Rate (FPS)", "The number of times per second at which the next sprite in the sequence will be chosen."), image.FrameRate);
            }

            if (image.Sprites.Count > 1)
            {
                image.RandomizeImageWhenShown = EditorGUILayout.Toggle(new GUIContent("Randomize Sprite?", "Whether or not the initial sprite chosen will be randomly selected from the sequence. If this is set to false, the sequence will always start with the first sprite."), image.RandomizeImageWhenShown);
            }

            EditorGUI.indentLevel++;

            Rect spriteDropArea = EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            imageFoldoutSettings.expandImageSprites[imageIndex] = EditorGUILayout.Foldout(imageFoldoutSettings.expandImageSprites[imageIndex], new GUIContent("Sprites (" + image.Sprites.Count + ")", "The list of sprites composing the sequence for the " + labelPrefix + "."));

            if (GUILayout.Button(new GUIContent("+"), GUILayout.MaxWidth(20.0f)))
            {
                image.Sprites.Add(null);
            }
            EditorGUILayout.EndHorizontal();

            if (imageFoldoutSettings.expandImageSprites[imageIndex])
            {
                EditorGUI.indentLevel++;
                for (int k = 0; k < image.Sprites.Count; k++)
                {
                    CreateSpriteField(image, k);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;

            HandleSpriteDragAndDrop(spriteDropArea, image);
        }

        private void CreateSpriteField(AnimatableDisplayImage image, int spriteIndex)
        {
            EditorGUILayout.BeginHorizontal();
            image.Sprites[spriteIndex] = EditorGUILayout.ObjectField(new GUIContent("Sprite " + (spriteIndex + 1)), image.Sprites[spriteIndex], typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
            if (spriteIndex > 0 && GUILayout.Button(new GUIContent("^", "Move the sprite up in the sequence."), GUILayout.MaxWidth(20.0f)))
            {
                Sprite temp = image.Sprites[spriteIndex];
                image.Sprites.RemoveAt(spriteIndex);
                image.Sprites.Insert(spriteIndex - 1, temp);
                spriteIndex--;
            }
            if (GUILayout.Button(new GUIContent("-", "Remove the sprite from the sequence."), GUILayout.MaxWidth(20.0f)))
            {
                image.Sprites.RemoveAt(spriteIndex);
                spriteIndex--;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void HandleSpriteDragAndDrop(Rect spriteDropArea, AnimatableDisplayImage image)
        {
            Event evt = Event.current;

            if (spriteDropArea.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object dragObj in DragAndDrop.objectReferences)
                        {
                            if (AssetDatabase.IsSubAsset(dragObj) && dragObj is Sprite)
                            {
                                image.Sprites.Add(dragObj as Sprite);
                            }
                            else
                            {
                                string assetPath = AssetDatabase.GetAssetPath(dragObj);
                                Object[] childAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                                foreach (Object child in childAssets)
                                {
                                    if (child != dragObj && child is Sprite)
                                    {
                                        image.Sprites.Add(child as Sprite);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreatePreview(AnimatableDisplayImage icon)
        {
            if (icon.Sprites.Count > 0 && icon.Sprites[0] != null)
            {
                Texture2D tex = icon.Sprites[0].texture;
                Rect spriteRect = icon.Sprites[0].rect;
                Rect uvRect = new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height);
                float thumbnailSize = 64f;

                if (spriteRect.width > spriteRect.height)
                {
                    GUILayout.Box("", GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize * (spriteRect.height / spriteRect.width)));
                }
                else
                {
                    GUILayout.Box("", GUILayout.Width(thumbnailSize * (spriteRect.width / spriteRect.height)), GUILayout.Height(thumbnailSize));
                }
                GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetLastRect(), tex, uvRect);
            }
        }

        private class CharacterDefinitionExpansionInfo
        {
            public bool expandSettings = false;
            public bool expandIcons = false;
            public bool expandImages = false;
            public bool expandAudio = false;

            public ImageExpansionInfo iconFoldoutSettings = new ImageExpansionInfo();
            public ImageExpansionInfo imageFoldoutSettings = new ImageExpansionInfo();
        }

        private class ImageExpansionInfo
        {
            public List<bool> imageFoldouts = new List<bool>();
            public List<bool> expandImageSprites = new List<bool>();
        }
    }
}
