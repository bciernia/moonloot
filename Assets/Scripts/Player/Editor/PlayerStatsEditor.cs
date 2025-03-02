using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerStatsSO))]
public class PlayerStatsEditor : Editor
{
    private PlayerStatsSO StatsTarget => target as PlayerStatsSO;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Reset Player"))
        {
            StatsTarget.ResetPlayerStats();
        }
    }
}
