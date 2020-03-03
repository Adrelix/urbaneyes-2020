using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Building : MonoBehaviour {

    private float floorHeight = 5f;
    public void initBuilding(BuildingData data) {

        Vector3[] floorBase = data.basePolygon;
        // create floor objects and add floor components to them
        for (int i = 0 ; i < data.levels; i++) {
            GameObject obj = new GameObject($"Floor {i}");
            obj.transform.parent = transform; // Set this building as parent
            obj.AddComponent<Floor>();
            // TODO: Fetch material depending on building type
            obj.GetComponent<Floor>().initFloor(floorBase, floorHeight, data.position, null);

            // Lift the floor base to next level
            floorBase = Array.ConvertAll(floorBase, (baseVector => new Vector3(baseVector.x, baseVector.y + floorHeight, baseVector.z)));

        }
    }
}
