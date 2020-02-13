using UnityEngine;
using System.Collections;
using UnityEditor;

// Defines the inspector menu of an ObjectBuilder component
// Unity requires this to be at the path "Assets/Editor"
// Allows ObjectBuilder.BuildObject() to create GameObjects directly in the scene

[CustomEditor(typeof(BuildingEditor))]
public class BuildingEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BuildingEditor buildingScript = (BuildingEditor)target;
        if(GUILayout.Button("Update Building"))
        {
            buildingScript.UpdateBuilding();
        }
    }
}