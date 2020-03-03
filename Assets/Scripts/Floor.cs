using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{   
    public void initFloor(Vector3[] baseVertices, float floorHeight, Vector3 position, Material material) {
        // Create roof vertices from basePolygon
        Vector3[] roofVertices = new Vector3[baseVertices.Length];
        for (int i = 0; i<baseVertices.Length; i++) {
            roofVertices[i] = new Vector3(baseVertices[i].x, baseVertices[i].y + floorHeight, baseVertices[i].z);
        }

        // Create wall vertices
        // Each face of the polygon must not share vertices with the other faces in order
        // for shaders to consider them as seperate faces and draw sharp edges between them
        int numWalls = baseVertices.Length;
        Vector3[] vertices = new Vector3[4 * numWalls];

        for (int i = 0; i < numWalls; i++) {
            vertices[(i*4)    ] = roofVertices[i];
            vertices[(i*4) + 1] = i == numWalls-1 ? roofVertices[0] : roofVertices[i+1]; // If last iteration connect with beginning of polygon
            vertices[(i*4) + 2] = i == numWalls-1 ? baseVertices[0] : baseVertices[i+1];
            vertices[(i*4) + 3] = baseVertices[i];
        }

        // Generate wall triangles
        // Each wall is rectangular and therefore divided into two triangles.
        // Triangles are described as three integers referring to the index of their
        // corresponding vertex in vertices. The index the triangles are placed at ensures 
        // that they are "clockwise" if facing the face of the rectangle.
        int[] triangles = new int[numWalls * 6];

        for (int i = 0; i < numWalls; i++) {
            triangles[(i*6)    ] = (i*4);
            triangles[(i*6) + 1] = (i*4) + 1;
            triangles[(i*6) + 2] = (i*4) + 2;
            triangles[(i*6) + 3] = (i*4) + 2;
            triangles[(i*6) + 4] = (i*4) + 3;
            triangles[(i*6) + 5] = (i*4);
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //mesh.uv = GeneratePerTriangleUV(mesh);
        //mesh.Optimize();

        // Set up game object with mesh;
        this.gameObject.AddComponent<MeshFilter>();
        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.GetComponent<Transform>().position = position;
        this.gameObject.GetComponent<MeshRenderer>().material = material;
        this.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        this.gameObject.AddComponent<MeshCollider>();
    }
}
