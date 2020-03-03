using UnityEngine;
using System.Collections;
using UnityEditor;

// Defines the inspector menu of an ObjectBuilder component
// Unity requires this to be at the path "Assets/Editor"
// Allows ObjectBuilder.BuildObject() to create GameObjects directly in the scene

[CustomEditor(typeof(StreetGen))]
public class StreetGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StreetGen streetScript = (StreetGen)target;
        if(GUILayout.Button("Create street"))
        {
            streetScript.makeRoad();
        }
    }
}