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

        int numWalls = baseVertices.Length;

        for (int i = 0; i < numWalls; i++) {
            // Create wall vertices
            // Each face of the polygon must not share vertices with the other faces in order
            // for shaders to consider them as seperate faces and draw sharp edges between them
            Vector3[] vertices = {
                roofVertices[i],
                i == numWalls-1 ? roofVertices[0] : roofVertices[i+1], // If last iteration connect with beginning of polygon
                i == numWalls-1 ? baseVertices[0] : baseVertices[i+1],
                baseVertices[i]
            };

            // Generate wall triangles
            // Each wall is rectangular and therefore divided into two triangles.
            // Triangles are described as three integers referring to the index of their
            // corresponding vertex in vertices. The index the triangles are placed at ensures 
            // that they are "clockwise" if facing the face of the rectangle.
            int[] triangles = {
                0,1,2,2,3,0
            };

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // Generate the wall
            generateWall(i, mesh, position, material);
        }
            
    }

    // Generate a wall based on "wall specific arguments"
    private void generateWall(int id, Mesh wallMesh, Vector3 wallPosition, Material wallMaterial) {
        GameObject obj = new GameObject($"Wall {id}");
        obj.transform.parent = transform; // Set this building as parent
        obj.AddComponent<Wall>();
        obj.GetComponent<Wall>().initWall(wallMesh, wallPosition, wallMaterial);
    }
}
