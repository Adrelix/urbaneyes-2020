using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{

    public void initWall(Mesh mesh, Vector3 position, float windowDistance, float doorDistance, Material material) {
        // For lighting
        mesh.RecalculateNormals();
        // TODO: Necessary?
        // mesh.RecalculateBounds();
        mesh.uv = generateUV(mesh);
        // TODO: Necessary?
        // mesh.RecalculateTangents();
        mesh.Optimize();

        // Set up game object with mesh;
        this.gameObject.AddComponent<MeshFilter>();
        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.AddComponent<MeshCollider>();
        this.gameObject.GetComponent<Transform>().position = position;
        this.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        this.gameObject.GetComponent<MeshRenderer>().material = material;

        MeshRenderer renderer = this.gameObject.GetComponent<MeshRenderer>();
        Material tempMaterial = new Material(renderer.sharedMaterial);

        // Scale texture according to width and height of the wall
        float x = mesh.bounds.size.x;
        float z = mesh.bounds.size.z;
        float wallLength = Mathf.Sqrt((x * x) + (z * z));
        float wallHeight = mesh.bounds.size.y;

        // TODO: Remove arbitrary scalar for brick size (5f)
        tempMaterial.mainTextureScale = new Vector2(wallLength / 5f, wallHeight / 5f);
        renderer.sharedMaterial = tempMaterial;

        if (transform.parent.GetComponentInParent<Floor>().isFirstFloor()) {
            generateDoors(wallLength, wallHeight, mesh, position, doorDistance);
        } else {
            generateWindows(wallLength, wallHeight, mesh, position, windowDistance);
        }
    }

    // Return uv mapping relative to a rectangle (our wall)
    private Vector2[] generateUV(Mesh mesh) {
        Vector2[] uvs = new Vector2[] {
            new Vector2(1,1),
            new Vector2(0,1),
            new Vector2(0,0),
            new Vector2(1,0)
        };
        return uvs;
    }

    private void generateWindows(float wallLength, float wallHeight, Mesh mesh, Vector3 position, float minWindowDistance) {
        // TODO: get actual window dimensions
        float windowLength = 0.6f;
        float windowHeight = 1.5f;
        // Offset is the distance from the corner of the wall to the first window
        float minOffset = 2f;

        float totalWindowDist = Mathf.Max(windowLength + minWindowDistance, windowLength);

        // Calculate the maximum num. of windows you can fit on the wall
        int numOfWindows = (int)((wallLength - (2f * minOffset)) / totalWindowDist);
        // Calculate the actual margins
        float offset = (wallLength - (numOfWindows * totalWindowDist)) / 2f;
        // Calculate direction along the wall
        Vector3 wallNormal = mesh.normals[0];
        Vector3 wallPath = Quaternion.AngleAxis(-90f, Vector3.up) * wallNormal;

        Vector3 windowPosition = position + offset * wallPath;
        windowPosition.y += (wallHeight - windowHeight) / 2; // place windows in center of wall (y-axis)
        for (int i = 0; i < numOfWindows; i++) {
            // Center window, x/y-axis
            windowPosition += wallPath * totalWindowDist / 2f;

            // Load window prefab and set as child
            GameObject windowInst = Instantiate(Resources.Load("Window_low_poly2"), windowPosition, Quaternion.identity) as GameObject;
            windowInst.transform.parent = this.transform;

            // Rotate window to be parallel with wall
            windowInst.transform.rotation = Quaternion.FromToRotation(Vector3.forward, wallNormal);

            // Add second half
            windowPosition += wallPath * totalWindowDist / 2f;
        }
    }

    private void generateDoors(float wallLength, float wallHeight, Mesh mesh, Vector3 position, float minDoorDistance) {
        // TODO: get actual door dimensions
        float doorLength = 2f;
        float doorHeight = 2f;
        // Offset is the distance from the corner of the wall to the first door
        float minOffset = 2f;

        float totalDoorDist = Mathf.Max(doorLength + minDoorDistance, doorLength);

        // Calculate the maximum num. of doors you can fit on the wall
        int numOfdoors = (int)((wallLength - (2f * minOffset)) / totalDoorDist);
        // Calculate the actual margins
        float offset = (wallLength - (numOfdoors * totalDoorDist)) / 2f;
        // Calculate direction along the wall
        Vector3 wallNormal = mesh.normals[0];
        Vector3 wallPath = Quaternion.AngleAxis(-90f, Vector3.up) * wallNormal;

        Vector3 doorPosition = position + offset * wallPath;
        doorPosition += 0.09f * wallNormal; // Offset prefab so its not inside the wall
        doorPosition.y += doorHeight / 2; // place doors at bottom of wall (y-axis)
        
        float halfTotalDoorDist = 0.5f*totalDoorDist;

        for (int i = 0; i < numOfdoors; i++) {
            // Center door, x/y-axis
            doorPosition += wallPath * halfTotalDoorDist;

            // Load door prefab and set as child
            Vector3 randShift = wallPath * halfTotalDoorDist * UnityEngine.Random.Range(-0.8f, 0.8f);
            GameObject doorInst = Instantiate(Resources.Load("Door N110913rz"), doorPosition + randShift, Quaternion.identity) as GameObject;
            doorInst.transform.parent = this.transform;

            // Rotate door to be parallel with wall
            doorInst.transform.rotation = Quaternion.FromToRotation(Vector3.forward, wallNormal);
            doorInst.transform.Rotate(0f, 90f, 0f);

            // Add second half
            doorPosition += wallPath * halfTotalDoorDist;
        }
    }
}
