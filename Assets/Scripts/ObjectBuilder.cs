using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// How to use:
// Add this script as a component of a GameObject in your scene. Then, in the 
// inspector, set the public field values and click the "Build Object" button
// to generate building GameObjects on the XZ plane of the scene.
// Note: The meshes will only be visible from above

// TODO: Come up with sceme for only generating GameObjects for new buildings?
// TODO: Support generation of buildings with "holes"

public class ObjectBuilder : MonoBehaviour 
{
    public TextAsset geojsonData;
    public Material material;


    // Uses GeoJsonParser to get building information from a file. Then, for each building
    // described by the file, creates a GameObject in the scene by dynamically creating the 
    // buiding's mesh and triangles and placing it in the scene accordingly.
    // Right now only generates buildings with flat roofs.
    public void BuildObject()
    {
        // Get building data from GEOjson file
        GeoJsonParser p = new GeoJsonParser(geojsonData);
        List<Building> buildings = p.GetBuildings();

        foreach (Building building in buildings)
        {
            // Triangulation of roof vertices
            Vector3[] roofVertices = building.roofPolygon;
            Triangulator tr = new Triangulator(roofVertices);
            int[] roofTriangles = tr.Triangulate();

            // Create wall vertices
            // Each face of the polygon must not share vertices with the other faces in order
            // for shaders to consider them as seperate faces and draw sharp edges between them
            Vector3[] baseVertices = building.basePolygon;
            int numWalls = baseVertices.Length;
            Vector3[] vertices = new Vector3[roofVertices.Length + (4 * numWalls)];
            roofVertices.CopyTo(vertices, 0);

            for(int i = 0; i < numWalls; i++){
                vertices[(i*4)     + roofVertices.Length] = roofVertices[i];
                vertices[(i*4) + 1 + roofVertices.Length] = i == numWalls-1 ? roofVertices[0] : roofVertices[i+1]; // If last iteration connect with beginning of polygon
                vertices[(i*4) + 2 + roofVertices.Length] = i == numWalls-1 ? baseVertices[0] : baseVertices[i+1];
                vertices[(i*4) + 3 + roofVertices.Length] = baseVertices[i];
            }

            // Generate wall triangles
            // Each triangle if drawn by connecting points from first to last must
            // be drawn clockwise assuming you are looking at the polygon face
            int[] triangles = new int[roofTriangles.Length + (numWalls * 6)];
            roofTriangles.CopyTo(triangles, 0);

            for(int i = 0; i < numWalls; i++)
            {
                triangles[(i*6)     + roofTriangles.Length] = (i*4)     + numWalls;
                triangles[(i*6) + 1 + roofTriangles.Length] = (i*4) + 1 + numWalls; 
                triangles[(i*6) + 2 + roofTriangles.Length] = (i*4) + 2 + numWalls;
                triangles[(i*6) + 3 + roofTriangles.Length] = (i*4) + 2 + numWalls;
                triangles[(i*6) + 4 + roofTriangles.Length] = (i*4) + 3 + numWalls;
                triangles[(i*6) + 5 + roofTriangles.Length] = (i*4)     + numWalls;  
            }

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            //mesh.Optimize();
    
            // Set up game object with mesh;
            GameObject obj = new GameObject(building.id); // Gets added to scene without calling Instantiate for some reason
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            obj.GetComponent<Transform>().position = building.position;
            obj.GetComponent<MeshRenderer>().material = material;
            obj.GetComponent<MeshFilter>().mesh = mesh;
            obj.AddComponent<MeshCollider>();
        }
    }


    // Taken from: http://wiki.unity3d.com/index.php?title=Triangulator#C.23-_Triangulator.cs
    // Modified to take list of 3d points describing a polygon on the XZ plane. Takes this list and
    // divides the polygon into clockwise triangle vertices to be used for creating a Unity mesh.
    // Can handle concave polygons but not holes or 3D polygons
    // TODO: Use better triangulation algorithm (Delaunay triangulation?)
    private class Triangulator
    {
        private List<Vector3> m_points = new List<Vector3>();
    
        public Triangulator (Vector3[] points) {
            m_points = new List<Vector3>(points);
        }
    
        public int[] Triangulate() {
            List<int> indices = new List<int>();
    
            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();
    
            int[] V = new int[n];
            if (Area() > 0) {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }
    
            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2; ) {
                if ((count--) <= 0)
                    return indices.ToArray();
    
                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;
    
                if (Snip(u, v, w, nv, V)) {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }
    
            indices.Reverse();
            return indices.ToArray();
        }
    
        private float Area () {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++) {
                Vector3 pval = m_points[p];
                Vector3 qval = m_points[q];
                A += pval.x * qval.z - qval.x * pval.z;
            }
            return (A * 0.5f);
        }
    
        private bool Snip (int u, int v, int w, int n, int[] V) {
            int p;
            Vector3 A = m_points[V[u]];
            Vector3 B = m_points[V[v]];
            Vector3 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.z - A.z)) - ((B.z - A.z) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++) {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector3 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }
    
        private bool InsideTriangle (Vector3 A, Vector3 B, Vector3 C, Vector3 P) {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;
    
            ax = C.x - B.x; ay = C.z - B.z;
            bx = A.x - C.x; by = A.z - C.z;
            cx = B.x - A.x; cy = B.z - A.z;
            apx = P.x - A.x; apy = P.z - A.z;
            bpx = P.x - B.x; bpy = P.z - B.z;
            cpx = P.x - C.x; cpy = P.z - C.z;
    
            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;
    
            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}