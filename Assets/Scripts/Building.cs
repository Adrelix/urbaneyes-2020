using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Building : MonoBehaviour {

    private float floorHeight = 2.5f;
    private Vector3[] floorBase;

    // magnitude of random noise when picking stucco colors
    private const float hueVar = 0.05f;
    private const float satVar = 0.2f;
    private const float valVar = 0.2f;

    // Stockholm colors in HSV format (add more...)
    private List<float[]> baseColorsHSV = new List<float[]> {
        new float[] {0f, 0.58f, 0.55f},     // red sand
        new float[] {0.33f, 0.27f, 0.28f},  // dark green
        new float[] {0.125f, 0.58f, 0.55f}  // beige
    };

    public void initBuilding(BuildingData data) {
        // Init the floorbase of the building
        this.floorBase = data.basePolygon;
        this.transform.position = data.position;

        // TODO: Init material (floor and upper) depending on building type

        // Create first floor with specific arguments
        // TODO: Not hardcode material
        Material firstFloorMaterial = getRandomFirstFloorMaterial();
        generateFloor(0, floorHeight * 1.25f, firstFloorMaterial);

        // Handle upper floors if they exist
        // TODO: Not hardcode material
        Material upperFloorsMaterial = getRandomUpperFloorMaterial();
        // generate upper floors
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
        // TODO: add proper roof material
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
        obj.GetComponent<Floor>().initFloor(id, this.floorBase, currentFloorHeight, this.transform.position, material);
        // Update global floor base for next floor
        this.floorBase = Array.ConvertAll(this.floorBase, (baseVector => new Vector3(baseVector.x, baseVector.y + currentFloorHeight, baseVector.z)));
    }

    // get random first floor material
    private Material getRandomFirstFloorMaterial() {
        // TODO: alternatively, choose material according to probabilities
        // (some materials should be more common than others)
        var rand = new System.Random();
        string matPath = new string[]
            {"wall01/wall01","wall04/wall04","wall13/wall13","brick"}[rand.Next(4)];
        return Resources.Load("Materials/" + matPath, typeof(Material)) as Material;
    }

    // get random wall material for upper floors, set color if stucco
    private Material getRandomUpperFloorMaterial() {
        // TODO: alternatively, choose material according to probabilities
        // (some materials should be more common than others)
        Material mat;
        var rand = new System.Random();
        switch (rand.Next(4)) {
            case 0:
                // Create new Material object so as not to change original
                mat = new Material(Resources.Load("Materials/wall08/wall08", typeof(Material)) as Material);
                mat.SetColor("_Color", getStuccoColor());
                break;
            case 1:
                // Create new Material object so as not to change original
                mat = new Material(Resources.Load("Materials/wall16/wall16", typeof(Material)) as Material);
                mat.SetColor("_Color", getStuccoColor());
                break;
            case 2:
                mat = Resources.Load("Materials/wall17/wall17", typeof(Material)) as Material;
                // TODO: different colors?
                break;
            default:
                mat = Resources.Load("Materials/wall18/wall18", typeof(Material)) as Material;
                // TODO: different colors?
                break;
        }
        return mat;
    }

    // get random stucco color: base color from global baseColorsHSV,
    // returns perturbed Color object
    private Color getStuccoColor() {
        var rand = new System.Random();
        float[] thisBaseColor = baseColorsHSV[rand.Next(baseColorsHSV.Count)];
        return getNoisyColorFromHSVArray(thisBaseColor);
    }

    // takes a HSV color array and makes a Color object
    // colors are perturbed according to global variables hueVar, satVar, valVar
    private Color getNoisyColorFromHSVArray(float[] colorHSV) {
        Color thisColor = Color.HSVToRGB(
            Mathf.Clamp(colorHSV[0] + UnityEngine.Random.Range(-hueVar, hueVar),
                        0f, 1f),
            Mathf.Clamp(colorHSV[1] + UnityEngine.Random.Range(-satVar, satVar),
                        0f, 1f),
            Mathf.Clamp(colorHSV[2] + UnityEngine.Random.Range(-valVar, valVar),
                        0f, 1f)
        );
        return thisColor;
    }
}
