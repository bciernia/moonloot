using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Increments the patch number in Player Settings before each build.
/// The game menu displays this value through Application.version.
/// </summary>
public sealed class AutoIncrementBuildVersion : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var currentVersion = PlayerSettings.bundleVersion;
        var parts = currentVersion.Split('.');

        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var patch))
        {
            throw new BuildFailedException(
                $"Cannot automatically increment version '{currentVersion}'. " +
                "Set Player Settings > Version to major.minor.patch, for example 0.1.0.");
        }

        patch++;
        var nextVersion = $"{major}.{minor}.{patch}";
        PlayerSettings.bundleVersion = nextVersion;

        Debug.Log($"Build version updated: {currentVersion} -> {nextVersion}");
    }
}
