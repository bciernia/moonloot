using EasyTalk.Editor.Settings;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Utils
{
    /// <summary>
    /// This class is used to automatically handle some installation tasks, such as setting up scripting directives.
    /// </summary>
    [InitializeOnLoad]
    public class ETInstallationUtls
    {
        static double lastUpdateTime = 0.0;

        static EasyTalkEditorSettings editorSettings = null;

        /// <summary>
        /// Static constructor called when loaded. Starts running an update loop to monitor whether TextMeshPro is installed or not.
        /// </summary>
        static ETInstallationUtls()
        {
            EditorApplication.update += Update;
        }

        /// <summary>
        /// Checks every 3 seconds to see if TextMeshPro is installed or not.
        /// </summary>
        static void Update()
        {
            editorSettings = Resources.Load<EasyTalkEditorSettings>("settings/EasyTalk Editor Settings");

            if (editorSettings == null || editorSettings.autoDetectTMP)
            {
                if (EditorApplication.timeSinceStartup - lastUpdateTime > 3.0)
                {
                    SetupTMPDirective();
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                }
            }
        }

        /// <summary>
        /// Adds or remves the TEXTMESHPRO_INSTALLED directive depending on if TMP essentials are installed, if needed.
        /// </summary>
        private static void SetupTMPDirective()
        {
            string tmpPath = Application.dataPath;
            tmpPath += "/TextMesh Pro/Resources/TMP Settings.asset";

            if (File.Exists(tmpPath))
            {
                AddTMPDirective();
            }
            else
            {
                RemoveTMPDirective();
            }
        }

        /// <summary>
        /// Adds a TEXTMESHPRO_INSTALLED scripting directive if it doesn't exist.
        /// </summary>
        private static void AddTMPDirective()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            UnityEditor.Build.NamedBuildTarget target = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);

            if (!currentDefines.Contains("TEXTMESHPRO_INSTALLED"))
            {
                currentDefines += ";TEXTMESHPRO_INSTALLED";
                PlayerSettings.SetScriptingDefineSymbols(target, currentDefines);

                Debug.Log("Created scripting define 'TEXTMESHPRO_INSTALLED'. EasyTalk will now use TextMeshPro unless Dialogue Displays are set to 'Force Standard Text' = 'true'");
            }
        }

        /// <summary>
        /// Removes the TEXTMESHPRO_INSTALLED scripting directive if it exists.
        /// </summary>
        private static void RemoveTMPDirective()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            UnityEditor.Build.NamedBuildTarget target = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);

            if (currentDefines.Contains("TEXTMESHPRO_INSTALLED"))
            {
                currentDefines = currentDefines.Replace("TEXTMESHPRO_INSTALLED", "");
                currentDefines = currentDefines.Replace(";;", ";");
                currentDefines = currentDefines.Trim(';');
                PlayerSettings.SetScriptingDefineSymbols(target, currentDefines);

                Debug.Log("Removed scripting define 'TEXTMESHPRO_INSTALLED'. EasyTalk will now use standard Unity Text components instead of TextMeshPro.");
            }
        }
    }
}
