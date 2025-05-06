using EasyTalk.Editor.Utils;
using EasyTalk.Nodes;
using EasyTalk.Nodes.Common;
using EasyTalk.Nodes.Core;
using EasyTalk.Nodes.Flow;
using EasyTalk.Nodes.Logic;
using EasyTalk.Nodes.Variable;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static EasyTalk.Nodes.Logic.LogicNode;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(Dialogue))]
    public class DialogueAssetEditor : UnityEditor.Editor
    {
        private Dialogue dialogue;

        private static SerializedObject dialogueObj;

        private Dictionary<NodeType, bool> nodeTypeExpansionStates = new Dictionary<NodeType, bool>();

        private Dictionary<int, bool> nodeItemsExpanded = new Dictionary<int, bool>();
        private Dictionary<int, bool> nodeInputsExpanded = new Dictionary<int, bool>();
        private Dictionary<int, bool> nodeOutputsExpanded = new Dictionary<int, bool>();

        public override void OnInspectorGUI()
        {
            dialogue = (Dialogue)target;

            dialogueObj = new SerializedObject(dialogue);

            EditorGUILayout.LabelField(new GUIContent("Total Nodes: " + dialogue.Nodes.Count));

            ETGUI.DrawLineSeparator();

            Dictionary<NodeType, List<Node>> nodesByType = new Dictionary<NodeType, List<Node>>();

            foreach(Node node in dialogue.Nodes)
            {
                if(!nodesByType.ContainsKey(node.NodeType))
                {
                    nodesByType.Add(node.NodeType, new List<Node>());
                }

                nodesByType[node.NodeType].Add(node);
            }

            foreach(NodeType nodeType in nodesByType.Keys)
            {
                if(!nodeTypeExpansionStates.ContainsKey(nodeType))
                {
                    nodeTypeExpansionStates.Add(nodeType, false);
                }

                nodeTypeExpansionStates[nodeType] = EditorGUILayout.Foldout(nodeTypeExpansionStates[nodeType], new GUIContent("" + nodeType));

                if (nodeTypeExpansionStates[nodeType])
                {
                    EditorGUI.indentLevel++;
                    foreach(Node node in nodesByType[nodeType])
                    {
                        EditorGUILayout.LabelField("" + node.NodeType + " - " + node.ID);
                        EditorGUI.indentLevel++;
                        CreateNodeSpecificSettings(node);
                        CreateConnectionSettings(node);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                ETGUI.DrawLineSeparator();
            }

            EditorGUILayout.PropertyField(dialogueObj.FindProperty("translationLibrary"), new GUIContent("Translation Library"));

            ETGUI.DrawLineSeparator();

            dialogueObj.ApplyModifiedProperties();
        }

        private void CreateNodeSpecificSettings(Node node)
        {
            switch(node.NodeType)
            {
                case NodeType.ENTRY: CreateSettingsForEntryNode(node as EntryNode); break;
                case NodeType.EXIT: CreateSettingsForExitNode(node as ExitNode); break;
                case NodeType.CONVO: CreateSettingsForConvoNode(node as ConversationNode); break;
                case NodeType.OPTION: CreateSettingsForOptionNode(node as OptionNode); break;
                case NodeType.STORY: CreateSettingsForStoryNode(node as StoryNode); break;
                case NodeType.TRIGGER: CreateSettingsForTriggerNode(node as TriggerScriptNode); break;
                case NodeType.JUMPIN: CreateSettingsForJumpInNode(node as JumpInNode); break;
                case NodeType.JUMPOUT: CreateSettingsForJumpOutNode(node as JumpOutNode); break;
                case NodeType.BOOL_LOGIC: CreateSettingsForBoolLogicNode(node as LogicNode); break;
                case NodeType.NUMBER_COMPARE: CreateSettingsForCompareNumbersNode(node as CompareNumbersNode); break;
                case NodeType.STRING_COMPARE: CreateSettingsForCompareStringsNode(node as CompareStringsNode); break;
                case NodeType.MATH: CreateSettingsForMathNode(node as MathNode); break;
                case NodeType.BOOL_VARIABLE: CreateSettingsForVariableNode(node as VariableNode); break;
                case NodeType.FLOAT_VARIABLE: CreateSettingsForVariableNode(node as VariableNode); break;
                case NodeType.INT_VARIABLE: CreateSettingsForVariableNode(node as VariableNode); break;
                case NodeType.STRING_VARIABLE: CreateSettingsForVariableNode(node as VariableNode); break;
                case NodeType.GET_VARIABLE_VALUE: CreateSettingsForGetVariableNode(node as GetVariableNode); break;
                case NodeType.SET_VARIABLE_VALUE: CreateSettingsForSetVariableNode(node as SetVariableNode); break;
                case NodeType.WAIT: CreateSettingsForWaitNode(node as WaitNode); break;
                case NodeType.PAUSE: CreateSettingsForPauseNode(node as PauseNode); break;
            }
        }

        private void CreateSettingsForEntryNode(EntryNode node)
        {
            string entryName = EditorGUILayout.TextField("Entry Point", node.EntryPointName);
            if(entryName != node.EntryPointName)
            {
                node.EntryPointName = entryName;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForExitNode(ExitNode node)
        {
            string exitName = EditorGUILayout.TextField("Exit Point", node.ExitPointName);
            if (exitName != node.ExitPointName)
            {
                node.ExitPointName = exitName;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForConvoNode(ConversationNode node)
        {
            string characterName = EditorGUILayout.TextField("Character", node.CharacterName);
            if(characterName != node.CharacterName)
            {
                node.CharacterName = characterName;
                EditorUtility.SetDirty(dialogue);
            }

            EditorGUI.indentLevel++;
            for(int i = 0; i < node.Items.Count; i++)
            {
                ConversationItem item = node.Items[i] as ConversationItem;
                string text = EditorGUILayout.TextField(new GUIContent("Line " + (i+1)), item.Text);
                if(text != item.Text)
                {
                    item.Text = text;
                    EditorUtility.SetDirty(dialogue);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void CreateSettingsForOptionNode(OptionNode node)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < node.Items.Count; i++)
            {
                OptionItem item = node.Items[i] as OptionItem;
                string optionText = EditorGUILayout.TextField("Option " + (i+1), item.text);
                if(optionText != item.text)
                {
                    item.text = optionText;
                    EditorUtility.SetDirty(dialogue);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void CreateSettingsForStoryNode(StoryNode node)
        {
            string story = EditorGUILayout.TextField("Story Text", node.Summary);
            if(story != node.Summary)
            {
                node.Summary = story;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForTriggerNode(TriggerScriptNode node)
        {
            string className = EditorGUILayout.TextField("Class Name", node.TriggeredClassName);
            if(className != node.TriggeredClassName)
            {
                node.TriggeredClassName = className;
                EditorUtility.SetDirty(dialogue);
            }

            string methodSignature = EditorGUILayout.TextField("Method Signature", node.MethodSignature);
            if(methodSignature != node.MethodSignature)
            {
                node.MethodSignature = methodSignature;
                EditorUtility.SetDirty(dialogue);
            }

            TriggerFilterType triggerType = (TriggerFilterType)EditorGUILayout.EnumPopup(new GUIContent("Trigger Type"), node.TriggerType);
            if(triggerType != node.TriggerType)
            {
                node.TriggerType = triggerType;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForJumpInNode(JumpInNode node)
        {
            string key = EditorGUILayout.TextField("Key", node.Key);
            if(key != node.Key)
            {
                node.Key = key;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForJumpOutNode(JumpOutNode node)
        {
            string key = EditorGUILayout.TextField("Key", node.Key);
            if (key != node.Key)
            {
                node.Key = key;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForBoolLogicNode(LogicNode node)
        {
            LogicOperation operation = (LogicOperation)EditorGUILayout.EnumPopup(new GUIContent("Operation"), node.LogicMode);
            if(operation != node.LogicMode)
            {
                node.LogicMode = operation;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForCompareNumbersNode(CompareNumbersNode node)
        {
            NumberComparisonType comparisonType = (NumberComparisonType)EditorGUILayout.EnumPopup(new GUIContent("Comparison Type"), node.ComparisonType);
            if(comparisonType != node.ComparisonType)
            {
                node.ComparisonType = comparisonType;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForMathNode(MathNode node)
        {
            MathOperation mathOperation = (MathOperation)EditorGUILayout.EnumPopup(new GUIContent("Math Operation"), node.MathOperation);
            if(mathOperation != node.MathOperation)
            {
                node.MathOperation = mathOperation;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForCompareStringsNode(CompareStringsNode node)
        {
            StringComparisonType comparisonType = (StringComparisonType)EditorGUILayout.EnumPopup(new GUIContent("Comparison Type"), node.ComparisonType);
            if(comparisonType != node.ComparisonType)
            {
                node.ComparisonType = comparisonType;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForVariableNode(VariableNode node)
        {
            string variableName = EditorGUILayout.TextField("Variable Name", node.VariableName);
            if(variableName != node.VariableName)
            {
                node.VariableName = variableName;
                EditorUtility.SetDirty(dialogue);
            }

            string initialValue = EditorGUILayout.TextField("Initial Value", node.VariableValue);
            if(initialValue != node.VariableValue)
            {
                node.VariableValue = initialValue;
                EditorUtility.SetDirty(dialogue);
            }

            bool resetOnEntry = EditorGUILayout.Toggle(new GUIContent("Reset on Entry?"), node.ResetOnEntry);
            if(resetOnEntry != node.ResetOnEntry)
            {
                node.ResetOnEntry = resetOnEntry;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForGetVariableNode(GetVariableNode node)
        {
            string variableName = EditorGUILayout.TextField("Variable Name", node.VariableName);

            if(variableName != node.VariableName)
            {
                node.VariableName = variableName;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForSetVariableNode(SetVariableNode node)
        {
            string variableName = EditorGUILayout.TextField("Variable Name", node.VariableName);

            if (variableName != node.VariableName)
            {
                node.VariableName = variableName;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForWaitNode(WaitNode node)
        {
            float time = 0.0f;
            float.TryParse(node.WaitTime, out time);
            string waitTime = "" + EditorGUILayout.FloatField("Wait TIme", time);
            if(waitTime != node.WaitTime)
            {
                node.WaitTime = waitTime;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateSettingsForPauseNode(PauseNode node)
        {
            string signal = EditorGUILayout.TextField("Signal", node.Signal);
            if(signal != node.Signal)
            {
                node.Signal = signal;
                EditorUtility.SetDirty(dialogue);
            }
        }

        private void CreateConnectionSettings(Node node)
        {
            if(!nodeInputsExpanded.ContainsKey(node.ID) && node.Inputs.Count > 0)
            {
                nodeInputsExpanded.Add(node.ID, false);
            }

            if(!nodeOutputsExpanded.ContainsKey(node.ID) && node.Outputs.Count > 0)
            {
                nodeOutputsExpanded.Add(node.ID, false);
            }

            EditorGUI.indentLevel++;

            if (node.Inputs.Count > 0)
            {
                nodeInputsExpanded[node.ID] = EditorGUILayout.Foldout(nodeInputsExpanded[node.ID], new GUIContent("Inputs"));
                if (nodeInputsExpanded[node.ID])
                {
                    ListConnections(node.Inputs);
                }
            }

            if (node.Outputs.Count > 0)
            {
                nodeOutputsExpanded[node.ID] = EditorGUILayout.Foldout(nodeOutputsExpanded[node.ID], new GUIContent("Outputs"));
                if (nodeOutputsExpanded[node.ID])
                {
                    ListConnections(node.Outputs);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void ListConnections(List<NodeConnection> nodeConnections)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < nodeConnections.Count; i++)
            {
                NodeConnection conn = nodeConnections[i];
                EditorGUILayout.LabelField(conn.ConnectionType + " -> " + conn.ID);

                EditorGUI.indentLevel++;
                for (int j = 0; j < conn.AttachedIDs.Count; j++)
                {
                    int attachedID = EditorGUILayout.IntField(new GUIContent("Attached ID"), conn.AttachedIDs[j]);

                    if (attachedID != conn.AttachedIDs[j])
                    {
                        conn.AttachedIDs[j] = attachedID;
                        EditorUtility.SetDirty(dialogue);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }
}
