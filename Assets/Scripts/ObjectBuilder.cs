using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OsmSharp.Complete;

// How to use:
// Add this script as a component of a GameObject in your scene. Then, in the 
// inspector, set the public field values and click the "Build Object" button
// to generate building GameObjects on the XZ plane of the scene.
// Note: The meshes will only be visible from above

// TODO: Come up with sceme for only generating GameObjects for new buildings?
// TODO: Support generation of buildings with "holes"

public class ObjectBuilder : MonoBehaviour 
{
    public TextAsset geojsonData;
    public float floorHeight;
    public float windowDistance;
    public float doorDistance;
    
    // Uses GeoJsonParser to get building information from a file. Then, for each building
    // described by the file, creates a GameObject in the scene by dynamically creating the 
    // buiding's mesh and triangles and placing it in the scene accordingly.
    // Right now only generates buildings with flat roofs.
    // Useful links:
    // https://docs.unity3d.com/Manual/AnatomyofaMesh.html
    // http://wiki.unity3d.com/index.php/ProceduralPrimitives
    public void BuildObject()
    {
        // Get building data from GEOjson file
        GeoJsonParser p = new GeoJsonParser(geojsonData);
        List<BuildingData> buildings = p.GetBuildings();

        foreach (BuildingData building in buildings)
        {
            GameObject obj = new GameObject(building.id); // Adds to the scene
            obj.AddComponent<Building>();
            obj.GetComponent<Building>().initBuilding(building, floorHeight, windowDistance, doorDistance);
        }
    }
}
