using EasyTalk.Display;
using EasyTalk.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(DirectionalOptionDisplay))]
    public class DirectionalOptionDisplayEditor : OptionDisplayEditor
    {
        private static bool expandOptionElements = false;
        private static List<bool> expandElementState = new List<bool>();

        protected static bool expandDirectionalSettings = false;

        public override void OnInspectorGUI()
        {
            DirectionalOptionDisplay display = target as DirectionalOptionDisplay;

            //if(displayObj == null || (displayObj != null && displayObj.targetObject != display))
            {
                displayObj = new SerializedObject(display);
            }

            EditorGUI.BeginChangeCheck();

            CreateDisplaySettings();
            CreateGeneralSettings();
            CreateFontSettings(display);
            CreateDirectionalDisplaySettings(display);
            CreateOptionElementSettings(display);
            CreateAnimationSettings(display);
            CreateImageSettings();
            CreateOptionDisplayListenerSettings();
            CreateDialoguePanelEventSettings();
            CreateOptionDisplayEventSettings();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(display);
            }

            displayObj.ApplyModifiedProperties();
        }

        private static void CreateOptionElementSettings(DirectionalOptionDisplay display)
        {
            ETGUI.DrawLineSeparator();
            expandOptionElements = EditorGUILayout.Foldout(expandOptionElements, new GUIContent("Option Element Configuration"), EditorStyles.foldoutHeader);

            if (expandOptionElements)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Number of Options");
                int numberOfOptions = EditorGUILayout.IntField(display.OptionElements.Count);
                EditorGUILayout.EndHorizontal();

                while (numberOfOptions > display.OptionElements.Count)
                {
                    display.OptionElements.Add(new DirectionalOptionElement());
                }

                while (numberOfOptions < display.OptionElements.Count)
                {
                    display.OptionElements.RemoveAt(numberOfOptions);
                }

                while (expandElementState.Count < numberOfOptions)
                {
                    expandElementState.Add(false);
                }

                for (int i = 0; i < numberOfOptions; i++)
                {
                    expandElementState[i] = EditorGUILayout.Foldout(expandElementState[i], new GUIContent("Option Element " + (i + 1)));

                    if (expandElementState[i])
                    {
                        EditorGUI.indentLevel++;

                        DirectionalOptionElement element = display.OptionElements[i];

                        element.button = EditorGUILayout.ObjectField(new GUIContent("Option Button"),
                            element.button, typeof(DialogueButton), true) as DialogueButton;

                        element.linkedImage = EditorGUILayout.ObjectField(new GUIContent("Linked Image"),
                            element.linkedImage, typeof(Image), true) as Image;

                        element.useCustomDirectionVector = EditorGUILayout.Toggle("Use Custom Direction Vector? ",
                            element.useCustomDirectionVector);

                        if (element.useCustomDirectionVector)
                        {
                            EditorGUI.indentLevel++;
                            element.directionVector = EditorGUILayout.Vector3Field(new GUIContent("Direction Vector"), element.directionVector);
                            EditorGUI.indentLevel--;
                        }

                        CreateActivationMaskSettings(numberOfOptions, element);

                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private static void CreateActivationMaskSettings(int numberOfOptions, DirectionalOptionElement element)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(new GUIContent("Activation Mask", "This mask determines when the button/link is activated based on the number of " +
                "options being presented. For example, if 2 options are presented and the second toggle is checked, this element will be activated" +
                " whenever 2 options are presented, but if the second toggle isn't checked, this option element will not be used."), EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Space(40.0f);

            for (int j = 0; j < numberOfOptions; j++)
            {
                if (element.activationMask.Count < (j + 1))
                {
                    element.activationMask.Add(false);
                }

                EditorGUILayout.BeginVertical(GUILayout.Width(12.0f), GUILayout.ExpandWidth(false));

                GUILayout.Label("" + (j + 1));
                element.activationMask[j] = GUILayout.Toggle(element.activationMask[j], "");

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateDirectionalDisplaySettings(DirectionalOptionDisplay display)
        {
            ETGUI.DrawLineSeparator();
            expandDirectionalSettings = EditorGUILayout.Foldout(expandDirectionalSettings, new GUIContent("Directional Display Settings"), EditorStyles.foldoutHeader);

            if (expandDirectionalSettings)
            {
                EditorGUILayout.PropertyField(displayObj.FindProperty("centerTransform"), new GUIContent("Center Transform"));
                EditorGUILayout.PropertyField(displayObj.FindProperty("mainImage"), new GUIContent("Main Image"));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Use Option Button Colors for Links?");

                display.UseOptionButtonColors = EditorGUILayout.Toggle(display.UseOptionButtonColors, GUILayout.MaxWidth(20.0f));
                EditorGUILayout.EndHorizontal();

                if (!display.UseOptionButtonColors)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(displayObj.FindProperty("linkNormalColor"), new GUIContent("Link Normal Color"));
                    EditorGUILayout.PropertyField(displayObj.FindProperty("linkDisabledColor"), new GUIContent("Link Disabled Color"));
                    EditorGUILayout.PropertyField(displayObj.FindProperty("linkHighlightColor"), new GUIContent("Link Highlight Color"));
                    EditorGUILayout.PropertyField(displayObj.FindProperty("linkPressedColor"), new GUIContent("Link Pressed Color"));
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}