using EasyTalk.Settings;
using EasyTalk.Nodes.Variable;
using UnityEditor;
using UnityEngine;
using EasyTalk.Editor.Utils;
using Microsoft.Win32;
using EasyTalk.Character;
using EasyTalk.Nodes;

namespace EasyTalk.Editor.Settings
{
    [CustomEditor(typeof(DialogueRegistry))]
    public class DialogueRegistryEditor : UnityEditor.Editor
    {
        private static bool expandGlobalVariables = true;

        public override void OnInspectorGUI()
        {
            DialogueRegistry registry = target as DialogueRegistry;

            expandGlobalVariables = EditorGUILayout.BeginFoldoutHeaderGroup(expandGlobalVariables, "Global Variables");

            if (expandGlobalVariables)
            {
                CreateGlobalVariableSettings(registry);
            }

            EditorGUI.EndFoldoutHeaderGroup();

            ETGUI.DrawLineSeparator();
            EditorGUILayout.LabelField(new GUIContent("Characters"), EditorStyles.boldLabel);
            registry.CharacterLibrary = EditorGUILayout.ObjectField(new GUIContent("Character Library"), registry.CharacterLibrary, typeof(CharacterLibrary), false) as CharacterLibrary;
            ETGUI.DrawLineSeparator();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(registry);
            }
        }

        private void CreateGlobalVariableSettings(DialogueRegistry registry)
        {
            EditorGUI.indentLevel++;

            int numVariables = EditorGUILayout.IntField(new GUIContent("Variable Count"), registry.GlobalVariables.Count);
            if (numVariables < registry.GlobalVariables.Count)
            {
                //Remove variables from the end until the number is the same
                for (int i = numVariables; i < registry.GlobalVariables.Count; i++)
                {
                    registry.GlobalVariables.RemoveAt(i);
                    i--;
                }
            }
            else if (numVariables > registry.GlobalVariables.Count)
            {
                for (int i = registry.GlobalVariables.Count; i < numVariables; i++)
                {
                    GlobalNodeVariable var = new GlobalNodeVariable();
                    var.VariableName = "variable_" + i;
                    var.VariableType = GlobalVariableType.STRING;
                    var.InitialValue = "";

                    registry.GlobalVariables.Add(var);
                }
            }

            for (int i = 0; i < registry.GlobalVariables.Count; i++)
            {
                ETGUI.DrawLineSeparator();
                GlobalNodeVariable var = registry.GlobalVariables[i];
                EditorGUILayout.LabelField(new GUIContent(var.VariableName), EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                var.VariableName = EditorGUILayout.TextField(new GUIContent("Variable Name"), var.VariableName);

                if (var.VariableName.Length == 0)
                {
                    var.VariableName = "variable_" + i;
                }

                GlobalVariableType oldVarType = var.VariableType;
                var.VariableType = (GlobalVariableType)EditorGUILayout.EnumPopup(new GUIContent("Variable Type"), var.VariableType);

                if (oldVarType != var.VariableType)
                {
                    switch (var.VariableType)
                    {
                        case GlobalVariableType.STRING: var.InitialValue = ""; break;
                        case GlobalVariableType.INT: var.InitialValue = "0"; break;
                        case GlobalVariableType.FLOAT: var.InitialValue = "0.0"; break;
                        case GlobalVariableType.BOOL: var.InitialValue = "true"; break;
                    }
                }

                CreateVariableValueField(var);

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        private static void CreateVariableValueField(GlobalNodeVariable var)
        {
            switch (var.VariableType)
            {
                case GlobalVariableType.STRING:
                    var.InitialValue = EditorGUILayout.TextField(new GUIContent("Initial Value"), (string)var.InitialValue);
                    break;
                case GlobalVariableType.INT:
                    int intValue = 0;
                    int.TryParse(var.InitialValue, out intValue);
                    var.InitialValue = EditorGUILayout.IntField(new GUIContent("Initial Value"), intValue).ToString();
                    break;
                case GlobalVariableType.FLOAT:
                    float floatValue = 0.0f;
                    float.TryParse(var.InitialValue,out floatValue);
                    var.InitialValue = EditorGUILayout.FloatField(new GUIContent("Initial Value"), floatValue).ToString();
                    break;
                case GlobalVariableType.BOOL:
                    bool boolValue = true;
                    bool.TryParse(var.InitialValue, out boolValue);
                    var.InitialValue = EditorGUILayout.Toggle(new GUIContent("Initial Value"), boolValue).ToString();
                    break;
            }
        }
    }
}
