using System;
using Codice.Client.BaseCommands.BranchExplorer.ExplorerData;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    private Waypoint WaypointTarget => target as Waypoint;

    private void OnSceneGUI()
    {
        if (WaypointTarget.Points.Length <= 0) return;
        
        Handles.color = Color.red;

        for (var i = 0; i < WaypointTarget.Points.Length; i++)
        {
            EditorGUI.BeginChangeCheck();

            var currentPosition = WaypointTarget.EntityPosition + WaypointTarget.Points[i];

            var newPosition =
                Handles.FreeMoveHandle(currentPosition, 0.5f, Vector3.one * 0.5f, Handles.SphereHandleCap);

            var text = new GUIStyle();
            text.fontStyle = FontStyle.Bold;
            text.fontSize = 16;
            text.normal.textColor = Color.black;
            var textPos = new Vector3(0.2f, -0.2f);
            Handles.Label(WaypointTarget.EntityPosition + WaypointTarget.Points[i] + textPos, $"{i + 1}", text);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Free move");
                WaypointTarget.Points[i] = newPosition - WaypointTarget.EntityPosition;
            }
        }
    }
}