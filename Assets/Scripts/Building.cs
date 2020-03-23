using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Building : MonoBehaviour {

    private float floorHeight = 2.5f;
    private Vector3[] floorBase;
    private Vector3 buildingPosition;

    public void initBuilding(BuildingData data) {
        // Init the floorbase of the building
        this.floorBase = data.basePolygon;
        this.buildingPosition = data.position;

        // TODO: Init material (floor and upper) depending on building type

        // Create first floor with specific arguments
        // TODO: Not hardcode material
        Material firstFloorMaterial = Resources.Load("Materials/brick", typeof(Material)) as Material;
        generateFloor(0, floorHeight * 1.25f, firstFloorMaterial);

        // Handle upper floors if they exist
        // TODO: Not hardcode material
        Material upperFloorsMaterial = Resources.Load("Materials/Ground2", typeof(Material)) as Material;
        for (int i = 1 ; i < data.levels; i++) {
            generateFloor(i, floorHeight, upperFloorsMaterial);
        }
    }

    // Generate a floor based on "floor specific arguments"
    private void generateFloor(int id, float currentFloorHeight, Material material) {
        GameObject obj = new GameObject($"Floor {id}");
        obj.transform.parent = transform; // Set this building as parent
        obj.AddComponent<Floor>();
        obj.GetComponent<Floor>().initFloor(this.floorBase, currentFloorHeight, this.buildingPosition, material);
        // Update global floor base for next floor
        this.floorBase = Array.ConvertAll(this.floorBase, (baseVector => new Vector3(baseVector.x, baseVector.y + currentFloorHeight, baseVector.z)));
    }
}
