using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{

    public void initWall(Mesh mesh, Vector3 position, Material material) {
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
        
        // TODO: get acutal window dimensions
        float windowLength = 2f;
        float windowHeight = 1.5f;
        // Offset is the distance from the corner of the wall to the first window
        float minOffset = 2f; 

        // Calculate the maximum num. of windows you can fit on the wall
        int numOfWindows = (int)((wallLength - (2f * minOffset)) / windowLength);
        // Calculate the actual margins
        float offset = (wallLength - (numOfWindows * windowLength)) / 2f;
        // Calculate direction along the wall
        Vector3 wallNormal = mesh.normals[0];
        Vector3 wallPath = Quaternion.AngleAxis(-90f, Vector3.up) * wallNormal;

        Vector3 windowPosition = position + offset * wallPath;
        windowPosition.y += (wallHeight - windowHeight) / 2; // place windows in center of wall (y-axis)
        for (int i = 0; i < numOfWindows; i++) {
            // Center window, x/y-axis
            windowPosition += wallPath * windowLength / 2f;

            // Load window prefab and set as child
            GameObject windowInst = Instantiate(Resources.Load("Window_low_poly2"), windowPosition, Quaternion.identity) as GameObject;
            windowInst.transform.parent = this.transform;

            // Rotate window to be parallel with wall
            windowInst.transform.rotation = Quaternion.FromToRotation(Vector3.forward, wallNormal);

            // Add second half
            windowPosition += wallPath * windowLength / 2f;
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
}
