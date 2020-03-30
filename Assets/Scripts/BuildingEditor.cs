using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
Generate the roofs of the buildings
*/
public class BuildingEditor : MonoBehaviour
{
    public int levels;

    public void UpdateBuilding()
    {
        Mesh buildingMesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = buildingMesh.vertices;

        int numRoofVertices = 4;
        const float FLOOR_HEIGHT = 5;
        for(int i = 0; i < numRoofVertices; i++)
        {
            vertices[i].y = FLOOR_HEIGHT * levels;
        }
        buildingMesh.vertices = vertices;
        buildingMesh.RecalculateBounds();
    }
}
