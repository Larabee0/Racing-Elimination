using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackToCheckPoint))]
public class CheckPointBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        TrackToCheckPoint trackToCheckPoint = (TrackToCheckPoint)target;
        DrawDefaultInspector();
        if (GUILayout.Button("PlaceWayPoints"))
        {
            trackToCheckPoint.PlaceCheckPoints();
        }
    }
}
