using EasyTalk.Animation;
using EasyTalk.Display;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(WorldspaceConversationDisplay))]
    public class WorldSpaceConversationDisplayEditor : ConversationDisplayEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override List<UIAnimator.Animation> GetSupportedAnimationTypes()
        {
            return new List<UIAnimator.Animation> { UIAnimator.Animation.NONE, UIAnimator.Animation.FADE };
        }

        protected override void CreateGeneralFields()
        {
            base.CreateGeneralFields();

            EditorGUILayout.PropertyField(displayObj.FindProperty("pointAtTransform"));

            if (displayObj.FindProperty("pointAtTransform").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayObj.FindProperty("lookAt"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(displayObj.FindProperty("forceStandardText"));
        }
    }
}
