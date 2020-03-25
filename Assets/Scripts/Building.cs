using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Building : MonoBehaviour {

    private float floorHeight = 2.5f;
    private Vector3[] floorBase;

    // private Color redsand = new Color(0.55f, 0.23f, 0.23f);
    // private Color green = new Color(0.28f, 0.38f, 0.28f);
    // private Color beige = new Color(0.55f, 0.47f, 0.23f);
    // private List<Color> colors = new List<Color>{redsand, green, beige};
    // above doesn't compile so use ugly version instead:
    private List<Color> colors = new List<Color>{
        new Color(0.55f, 0.23f, 0.23f),     // red sand
        new Color(0.28f, 0.38f, 0.28f),     // dark green
        new Color(0.55f, 0.47f, 0.23f)      // beige
    };

    public void initBuilding(BuildingData data) {
        // Init the floorbase of the building
        this.floorBase = data.basePolygon;
        this.transform.position = data.position;

        // TODO: Init material (floor and upper) depending on building type

        // Create first floor with specific arguments
        // TODO: Not hardcode material
        Material firstFloorMaterial = Resources.Load("Materials/brick", typeof(Material)) as Material;
        generateFloor(0, floorHeight * 1.25f, firstFloorMaterial);

        // Handle upper floors if they exist
        // TODO: Not hardcode material
        Material upperFloorsMaterial =
            Resources.Load("Materials/wall08/wall08b", typeof(Material)) as Material;
        // randomize stucco color for rest of buildning
        var rand = new System.Random();
        Color thisHouseColor = colors[rand.Next(colors.Count)];
        // TODO: add random color deviation
        upperFloorsMaterial.SetColor("_Color", thisHouseColor);
        for (int i = 1 ; i < data.levels; i++) {
            generateFloor(i, floorHeight, upperFloorsMaterial);
        }

        //GameObject meshObj = new GameObject();
        //meshObj.transform.parent = transform;
        //this.gameObject.transform = this.buildingPosition;
        // Add roof mesh to GameObject
        Triangulator roofMesh = new Triangulator(this.floorBase);
        int[] triangles = roofMesh.Triangulate();

        Mesh mesh = new Mesh();
        mesh.vertices = this.floorBase;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        this.gameObject.AddComponent<MeshRenderer>();
        // Change to roof material
        Material roofMaterial =
            Resources.Load("Materials/Ground2", typeof(Material)) as Material;
        this.gameObject.GetComponent<MeshRenderer>().material = roofMaterial;
        this.gameObject.AddComponent<MeshFilter>();
        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // Generate a floor based on "floor specific arguments"
    private void generateFloor(int id, float currentFloorHeight, Material material) {
        GameObject obj = new GameObject($"Floor {id}");
        obj.transform.parent = transform; // Set this building as parent
        obj.AddComponent<Floor>();
        obj.GetComponent<Floor>().initFloor(this.floorBase, currentFloorHeight, this.transform.position, material);
        // Update global floor base for next floor
        this.floorBase = Array.ConvertAll(this.floorBase, (baseVector => new Vector3(baseVector.x, baseVector.y + currentFloorHeight, baseVector.z)));
    }
}
