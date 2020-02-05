using UnityEngine;
using System.Collections;
using UnityEditor;

// Defines the inspector menu of an ObjectBuilder component
// Unity requires this to be at the path "Assets/Editor"
// Allows ObjectBuilder.BuildObject() to create GameObjects directly in the scene

[CustomEditor(typeof(ObjectBuilder))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ObjectBuilder builderScript = (ObjectBuilder)target;
        if(GUILayout.Button("Build Object"))
        {
            builderScript.BuildObject();
        }
    }
}