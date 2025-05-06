using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor
{
    [InitializeOnLoad]
    public class ETThumbnailRenderer
    {
        static string screenspacePath = "Assets/Okitoki Games/EasyTalk/Prefabs/Dialogue Displays/Screenspace/Thumbnails/";

        static string worldspacePath = "Assets/Okitoki Games/EasyTalk/Prefabs/Dialogue Displays/Speech Bubbles/Thumbnails/";

        static Dictionary<string, string> previewMap = new Dictionary<string, string>()
        {
            { "Dialogue_UI_Directional_Arrow_Long", screenspacePath + "dd3b.png" },
            { "Dialogue_UI_Directional_Arrow_Short", screenspacePath + "dd3.png" },
            { "Dialogue_UI_Directional_Circle_4_Segment", screenspacePath + "dd2.png" },
            { "Dialogue_UI_Directional_Circle_4_Segment_Even", screenspacePath + "dd1.png" },
            { "Dialogue_UI_ListColumn_BottomHorizontal_External_Center", screenspacePath + "dd9.png" },
            { "Dialogue_UI_ListColumn_BottomHorizontal_External_Left", screenspacePath + "dd10.png" },
            { "Dialogue_UI_ListColumn_BottomHorizontal_External_Right", screenspacePath + "dd11.png" },
            { "Dialogue_UI_ListColumn_BottomHorizontal_Internal_Left", screenspacePath + "dd6b.png" },
            { "Dialogue_UI_ListColumn_BottomHorizontal_Internal_Right", screenspacePath + "dd6.png" },
            { "Dialogue_UI_ListColumn_LeftVertical_Internal_Bottom", screenspacePath + "dd7b.png" },
            { "Dialogue_UI_ListColumn_RightVertical_Internal_Bottom", screenspacePath + "dd7.png" },
            { "Dialogue_UI_ListRow_BottomHorizontal_External_Center", screenspacePath + "dd4.png" },
            { "Dialogue_UI_ListRow_BottomHorizontal_Internal_Left", screenspacePath + "dd5b.png" },
            { "Dialogue_UI_ListRow_BottomHorizontal_Internal_Right", screenspacePath + "dd5.png" },
            { "Dialogue_UI_Scroll_BottomHorizontal_External_Center", screenspacePath + "dd13.png" },
            { "Dialogue_UI_Scroll_BottomHorizontal_Internal_Left", screenspacePath + "dd12b.png" },
            { "Dialogue_UI_Scroll_BottomHorizontal_Internal_Right", screenspacePath + "dd12.png" },
            { "Dialogue_UI_Scroll_LeftVertical_Internal_Bottom", screenspacePath + "dd8b.png" },
            { "Dialogue_UI_Scroll_RightVertical_Internal_Bottom", screenspacePath + "dd8.png" },
            { "SpeechBubble1", worldspacePath + "wsdd1.png" },
            { "SpeechBubble2", worldspacePath + "wsdd2.png" },
            { "SpeechBubble3", worldspacePath + "wsdd3.png" },
            { "SpeechBubble4", worldspacePath + "wsdd4.png" }
        };

        static ETThumbnailRenderer()
        {
            EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemOnGUICallback;
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUICallback;
        }

        static void ProjectWindowItemOnGUICallback(string guid, Rect selectionRect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (assetPath != null && assetPath.EndsWith(".prefab"))
            {
                string prefabName = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
                prefabName = prefabName.Substring(0, prefabName.IndexOf("."));

                if (previewMap.ContainsKey(prefabName))
                {
                    string previewPath = previewMap[prefabName];

                    if (File.Exists(previewPath))
                    {
                        Texture2D thumbnail = (Texture2D)AssetDatabase.LoadAssetAtPath(previewPath, typeof(Texture2D));
                        Rect rect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.height - 16);

                        EditorGUI.DrawPreviewTexture(rect, thumbnail);
                    }
                }

            }

        }
    }
}
