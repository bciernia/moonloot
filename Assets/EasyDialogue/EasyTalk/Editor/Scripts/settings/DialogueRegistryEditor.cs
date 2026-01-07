using EasyTalk.Settings;
using EasyTalk.Nodes.Variable;
using UnityEditor;
using UnityEngine;
using EasyTalk.Editor.Utils;
using EasyTalk.Character;
using EasyTalk.Nodes;
using EasyTalk.Localization;

namespace EasyTalk.Editor.Settings
{
    [CustomEditor(typeof(DialogueRegistry))]
    public class DialogueRegistryEditor : UnityEditor.Editor
    {
        private static bool expandGlobalVariables = true;
        private static bool expandTranslationSettings = true;
        private static SerializedObject registryObject;

        public override void OnInspectorGUI()
        {
            DialogueRegistry registry = target as DialogueRegistry;
            registryObject = new SerializedObject(registry);

            expandGlobalVariables = EditorGUILayout.BeginFoldoutHeaderGroup(expandGlobalVariables, "Global Variables");

            if (expandGlobalVariables)
            {
                CreateGlobalVariableSettings(registry);
            }

            EditorGUI.EndFoldoutHeaderGroup();

            CreateCharacterSettings(registry);

            EditorGUI.BeginChangeCheck();

            expandTranslationSettings = EditorGUILayout.BeginFoldoutHeaderGroup(expandTranslationSettings, "Translation Settings");

            if (expandTranslationSettings)
            {
                registry.UseSingleTranslationLibrary = EditorGUILayout.Toggle(new GUIContent("Use Single Translation Library?"), registry.UseSingleTranslationLibrary);
                
                if(registry.UseSingleTranslationLibrary)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(registryObject.FindProperty("translationLibrary"));
                    if(GUILayout.Button("Sync"))
                    {
                        SyncTranslations(registry);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                CreateTranslatableNodeTypeSettings();
                EditorGUI.indentLevel--;
            }

            ETGUI.DrawLineSeparator();

            if(EditorGUI.EndChangeCheck())
            {
                registryObject.ApplyModifiedProperties();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(registry);
            }
        }

        private void SyncTranslations(DialogueRegistry registry)
        {
            //Add each language to the core library.
            if(!registry.TranslationLibrary.HasTranslationSet(EasyTalkNodeEditor.Instance.EditorSettings.defaultOriginalLanguage))
            {
                registry.TranslationLibrary.GetOrCreateOriginalTranslationSet();
            }

            //Go through each dialogue asset's translation libraries. For any translations which exist there and don't have entries or translations in
            //the single library, add them to the single library, and vice versa.
            string[] dialogueAssets = AssetDatabase.FindAssets("t:Dialogue");

            foreach(string dialogueGuid in dialogueAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(dialogueGuid);
                Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(assetPath);

                TranslationLibrary dialogueTranslationLib = dialogue.TranslationLibrary;

                if (dialogueTranslationLib != null && dialogueTranslationLib.translationSets.Count > 0)
                {
                    //Copy translations from dialogue asset's translation libraries into the core library where entries are blank.
                    SyncTranslationLibraries(dialogueTranslationLib, registry.TranslationLibrary, true);

                    //Copy translations from the core library into all dialogue asset's translation libraries if they have an identical entry.
                    SyncTranslationLibraries(registry.TranslationLibrary, dialogueTranslationLib);

                    EditorUtility.SetDirty(dialogue);
                    EditorUtility.SetDirty(dialogue.TranslationLibrary);

                    Debug.Log("Synced " + dialogue.name + " with core translation library.");
                }
            }

            EditorUtility.SetDirty(registry.TranslationLibrary);

            AssetDatabase.SaveAssets();
        }

        private static void SyncTranslationLibraries(TranslationLibrary sourceLibrary, TranslationLibrary destinationLibrary, bool addWhenNonexistent = false)
        {
            //Go through each translation in the origin language
            TranslationSet sourceDefaultSet = sourceLibrary.GetOrCreateOriginalTranslationSet();

            foreach (Translation sourceText in sourceDefaultSet.translations)
            {
                if (sourceText.text == null || sourceText.text.Equals(""))
                {
                    continue;
                }

                //Get the ID of the translation in the destination library which matches the text of the current source text.
                int textId = destinationLibrary.GetTextDefinitionId(sourceText.text);

                //If a matching text entry exists in both source and destination, procede to sync translations across sets in the destination.
                if (textId >= 0)
                {
                    //Proceed through each source translation set to sync.
                    for (int i = 0; i < sourceLibrary.translationSets.Count; i++)
                    {
                        TranslationSet sourceSet = sourceLibrary.translationSets[i];

                        //We don't need to update the translations for the original language, since those should be the same in both libraries.
                        if(sourceSet.languageCode.Equals(sourceDefaultSet.languageCode))
                        {
                            continue;
                        }

                        //Get the translation entry for the current language from the translation set.
                        Translation sourceTranslation = sourceSet.GetTranslation(sourceText.id);

                        if (sourceTranslation != null)
                        {
                            //If the destination doesn't have a translation set for the current source language, add one.
                            if (!destinationLibrary.HasTranslationSet(sourceSet.languageCode))
                            {
                                destinationLibrary.AddSecondaryTranslationSet(sourceSet.languageCode);
                            }

                            //Retrieve the translation set in the destination for the current language.
                            TranslationSet destinationSet = destinationLibrary.FindTranslationSetForLanguage(sourceSet.languageCode);

                            //Retrieve the alternate language translation for the entry with the current ID.
                            Translation destinationTranslation = destinationSet.GetTranslation(textId);

                            //If the dialogue asset has an entry for the translation, replace the value with the value from the core translation library.
                            if (destinationTranslation != null)
                            {
                                //Only update the text of the destination translation if it is currently blank and the source text isn't.
                                if ((destinationTranslation.text.Equals("") || destinationTranslation.text == null) &&
                                    (!sourceTranslation.text.Equals("") && sourceTranslation.text != null))
                                {
                                    destinationTranslation.text = sourceTranslation.text;
                                }
                            }
                            else //There is no entry for current language in the dialogue asset, so insert one.
                            {
                                destinationSet.SetTranslation(textId, sourceTranslation.text);
                            }
                        }

                    }
                }
                else if (addWhenNonexistent)
                {
                    Translation destinationTranslation = destinationLibrary.AddOrFindTranslation(sourceText.text);

                    //Copy the values for each language in the dialogue asset's translation sets to the destination.
                    for (int i = 0; i < sourceLibrary.translationSets.Count; i++)
                    {
                        TranslationSet sourceSet = sourceLibrary.translationSets[i];

                        TranslationSet destinationSet = null;

                        //If the destination doesn't have a translation set for the current source set's language, create a new one.
                        if (!destinationLibrary.HasTranslationSet(sourceSet.languageCode))
                        {
                            destinationSet = destinationLibrary.AddSecondaryTranslationSet(sourceSet.languageCode);
                        }
                        else //Use the destination translation set for the current language.
                        {
                            destinationSet = destinationLibrary.FindTranslationSetForLanguage(sourceSet.languageCode);
                        }

                        //Get the translation entries for the current language.
                        Translation sourceTranslation = sourceSet.GetTranslation(sourceText.id);
                        Translation destinationCurrentLangTranslation = destinationSet.GetTranslation(destinationTranslation.id);

                        //If we have an entry in the source, copy it into the destination if the destination is blank.
                        if (sourceTranslation != null && sourceTranslation.text != null && !sourceTranslation.text.Equals("") && 
                            (destinationCurrentLangTranslation.text == null || destinationCurrentLangTranslation.text.Equals(""))) 
                        {
                            destinationSet.SetTranslation(destinationTranslation.id, sourceTranslation.text);
                        }
                    }
                }
            }
        }

        private static void CreateCharacterSettings(DialogueRegistry registry)
        {
            ETGUI.DrawLineSeparator();
            EditorGUILayout.LabelField(new GUIContent("Characters"), EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            registry.CharacterLibrary = EditorGUILayout.ObjectField(new GUIContent("Character Library"), registry.CharacterLibrary, typeof(CharacterLibrary), false) as CharacterLibrary;
            EditorGUI.indentLevel--;

            ETGUI.DrawLineSeparator();
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

        private static void CreateTranslatableNodeTypeSettings()
        {
            EditorGUILayout.PropertyField(registryObject.FindProperty("translatedNodeTypes"));
        }
    }
}
