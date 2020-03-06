using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

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
        this.gameObject.GetComponent<Transform>().position = position;
        this.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        this.gameObject.GetComponent<MeshRenderer>().material = material;

        var renderer = this.gameObject.GetComponent<MeshRenderer>();
        var tempMaterial = new Material(renderer.sharedMaterial);

        // TODO: Remove arbitrary scalar (1.5f)
        // TODO: Find pattern for matching very long objects
        float scaleX = mesh.bounds.size.x * transform.localScale.x * 1.5f;
        float scaleY = mesh.bounds.size.y * transform.localScale.y;

        tempMaterial.mainTextureScale = new Vector2(scaleX, scaleY);
        renderer.sharedMaterial = tempMaterial;

        this.gameObject.AddComponent<MeshCollider>();  
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
