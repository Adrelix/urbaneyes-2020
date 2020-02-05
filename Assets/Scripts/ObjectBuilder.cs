using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// How to use:
// Add this script as a component of a GameObject in your scene. Then, in the 
// inspector, set the public field values and click the "Build Object" button
// to generate an object with a rectangular mesh on the XY plane
// Note: The mesh will only be visible from one side

// TODO: Generate 3D meshes of buildings given GEOjson data 
// TODO: Come up with sceme for only generating GameObjects for new buildings

public class ObjectBuilder : MonoBehaviour 
{
    public TextAsset geojsonData;
    public Material material;
    public Vector3 spawnPoint;
    public float width = 5f;
    public float height = 5f;

    // Adds a GameObject to the scene with a dynamically created rectangular mesh
    public void BuildObject()
    {

        GeoJsonParser p = new GeoJsonParser(geojsonData);


        // Define rectangles coords
        Vector2[] vertices2D = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,height),
            new Vector2(width,height),
            new Vector2(width,0),
        };

        // Triangulation
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i=0; i<vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }
 
        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
 
        // Set up game object with mesh;
        GameObject obj = new GameObject("Empty"); // Gets added to scene without calling Instantiate for some reason
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.GetComponent<Transform>().position = spawnPoint;
        obj.GetComponent<MeshRenderer>().material = material;
        obj.GetComponent<MeshFilter>().mesh = mesh;
    }


    // Taken from: http://wiki.unity3d.com/index.php?title=Triangulator#C.23-_Triangulator.cs
    // Takes a list of 2d points describing a polygon and divides the polygon in triangle
    // vertices to be used for creating a Unity mesh. Can handle concave polygons but not holes or 3D polygons
    // TODO: Use better triangulation algorithm (Delaunay triangulation?)
    private class Triangulator
    {
        private List<Vector2> m_points = new List<Vector2>();
    
        public Triangulator (Vector2[] points) {
            m_points = new List<Vector2>(points);
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
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }
    
        private bool Snip (int u, int v, int w, int n, int[] V) {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++) {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }
    
        private bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;
    
            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;
    
            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;
    
            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}