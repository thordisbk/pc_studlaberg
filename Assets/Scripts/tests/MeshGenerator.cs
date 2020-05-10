using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public GameObject shapePoints;
    private Transform[] points;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;

    // uv is position in texture that gets applied to that vertex
    // use X and Z for top and bottom, use X and Y for sides (or X and Z?)
    float maxX = 0f;
    float maxY = 0f;
    float maxZ = 0f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        points = new Transform[shapePoints.transform.childCount];
        for (int i = 0; i < points.Length; i++) {
            points[i] = shapePoints.transform.GetChild(i);
            // Debug.Log(points[i].gameObject.name);
        }
        // foreach(Transform child in shapePoints) {}
        Debug.Log("Shape: " + shapePoints.name);

        // so mesh wont be updated twice at start 
        foreach (Transform t in points) {
            if (t.hasChanged) {
                t.hasChanged = false;
            }
        }

        // set maxX maxY maxZ
        foreach (Transform p in points) {
            if (p.position.x > maxX) maxX = p.position.x;
            if (p.position.y > maxY) maxY = p.position.y;
            if (p.position.z > maxZ) maxZ = p.position.z;
        }
        Debug.Log("MaxX: " + maxX + " MaxY: " + maxY + " MaxZ: " + maxZ);

        CreateShape();
        FindUVs();
        //uv = new Vector2[vertices.Length];
        UpdateMesh();
    }

    void Update()
    {
        foreach (Transform t in points) {
            if (t.hasChanged) {
                Debug.Log("Update mesh");
                CreateShape();
                UpdateMesh();
                t.hasChanged = false;
            }
        }
    }

    void CreateShape() {
        int pl = points.Length;
        int numOfSplits;  // min number of vertex splitting for disconected surfaces
        int v;  // number of vertices needed for the mesh

        int corners = pl-1;
        float columnLength = 5f;
        
        /*int v = pl * 2;
        vertices = new Vector3[v];
        for (int i = 0; i < pl; i++) {
            vertices[i] = points[i].position;

            Vector3 p = new Vector3(points[i].position.x, points[i].position.y - 5f, points[i].position.z);
            vertices[i+pl] = p;
        }*/

        if (corners % 2 == 1) {
            // then the shape has an odd number of corners (eg triangular, pentagonal)
            //  so there must be four splits
            numOfSplits = 4;
            v = (pl * 2) * 4;
        }
        else {
            numOfSplits = 3;
            v = (pl * 2) * 3;
        }

        vertices = new Vector3[v];
        uv = new Vector2[v];
        print("Number of vertices: " + v);

        // split the vertices to have hard edges between surfaces
        Vector3[] verticesTemp = new Vector3[pl*2];
        Vector2[] uvTemp = new Vector2[pl*2];
        for (int i = 0; i < pl; i++) {
            // for the top of the column
            verticesTemp[i] = points[i].position;
            
            //uvTemp[i] = new Vector2(points[i].position.x/maxX, points[i].position.y/maxY);
            if (i == 0) uvTemp[i] = new Vector2(0.5f, 0.5f);
            else if (i % 2 == 0) uvTemp[i] = new Vector2(0f, 0f);
            else if (corners % 2 == 1 && i == pl-1) uvTemp[i] = new Vector2(1f, 0f);
            else uvTemp[i] = new Vector2(0f, 1f);

            // for the bottom of the column
            Vector3 p = new Vector3(points[i].position.x, 
                                    points[i].position.y - columnLength, 
                                    points[i].position.z);
            verticesTemp[i+pl] = p;
            //uvTemp[i+pl] = new Vector2(p.x/maxX, p.y/maxY);
            if (i == 0) uvTemp[i+pl] = new Vector2(0.5f, 0.5f);
            else if (i % 2 == 0) uvTemp[i+pl] = new Vector2(1f, 0f);
            else if (corners % 2 == 1 && i == pl-1) uvTemp[i+pl] = new Vector2(0f, 1f);
            else uvTemp[i+pl] = new Vector2(1f, 1f);
        }
        // for the sides of the UV
        Vector2[] uvTempS = new Vector2[pl*2];
        for (int i = 0; i < pl; i++) {

            if (i % 2 == 0) uvTempS[i] = new Vector2(0f, 0f);
            else uvTempS[i] = new Vector2(0f, 1f);

            if (i % 2 == 0) uvTempS[i+pl] = new Vector2(1f, 0f);
            else uvTempS[i+pl] = new Vector2(1f, 1f);
        }
        // for the last odd size
        Vector2[] uvTempL = new Vector2[pl*2];
        for (int i = 0; i < pl; i++) {

            if (i % 2 == 0) uvTempL[i] = new Vector2(0f, 0f);
            else if (corners % 2 == 1 && i == pl-1) uvTempL[i] = new Vector2(1f, 0f);
            else uvTempL[i] = new Vector2(1f, 0f);

            if (i % 2 == 0) uvTempL[i+pl] = new Vector2(1f, 1f);
            else if (corners % 2 == 1 && i == pl-1) uvTempL[i+pl] = new Vector2(0f, 1f);
            else uvTempL[i+pl] = new Vector2(0f, 1f);
        }


        if (numOfSplits == 4) {
            vertices = verticesTemp.Concat(verticesTemp).Concat(verticesTemp).Concat(verticesTemp).ToArray();
            //uv = uvTemp.Concat(uvTemp).Concat(uvTemp).Concat(uvTemp).ToArray();
            uv = uvTemp.Concat(uvTempS).Concat(uvTempS).Concat(uvTempL).ToArray();
        }
        else {
            vertices = verticesTemp.Concat(verticesTemp).Concat(verticesTemp).ToArray();
            //uv = uvTemp.Concat(uvTemp).Concat(uvTemp).ToArray();
            uv = uvTemp.Concat(uvTempS).Concat(uvTempS).ToArray();
        }

        CreatePrism(corners);
        /*if (corners == 4) CreateSquarePrism();
        if (corners == 5) CreatePentagonalPrism();
        if (corners == 6) CreateHexagonalPrism();
        if (corners == 7) CreateHeptagonalPrism();
        if (corners == 8) CreateOctagonalPrism();*/
    }

    void FindUVs() {

    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        /*Bounds bounds = mesh.bounds;
        Vector2[] uvs = new Vector2[vertices.Length];
        for(int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
        }
        mesh.uv = uvs;*/

        //mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        //mesh.RecalculateTangents();

        /*Vector3[] normals = mesh.normals;
        // edit the normals in an external array
        Quaternion rotation = Quaternion.AngleAxis(Time.deltaTime * 100f, Vector3.up);
        for (int i = 0; i < normals.Length; i++)
            normals[i] = rotation * normals[i];
        // assign the array of normals to the mesh
        mesh.normals = normals;*/
    }

    void CreatePrism(int corners, bool bottomMesh=false) {
        if (corners < 3) {
            Debug.LogError("CreatePrism(): minimum corner number is 3");
            return;
        }
        int a = points.Length;  // corners + 1 (the center)

        // for a vertex x, three faces of the prism can use it
        //  points from which split to use (1st, 2nd, 3rd, or 4th)
        int _0 = 0 * points.Length*2;  // x+_0 for the first triangle-face that uses it
        int _1 = 1 * points.Length*2;  // x+_1 for the second triangle-face that uses it
        int _2 = 2 * points.Length*2;  // x+_2 for the third triangle-face that uses it
        int _3 = 3 * points.Length*2;  // x+_3 for the third/fourth triangle-face that uses it
                                       //  (needed when there is an odd number of corners)
        
        // create a list which the vertices get added to, then convert that to an array
        
        // first top shape mesh (triangle, square, pentagon, hexagon, heptagon, octagon, ...)
        // _0 == 0 so no need to add it
        List<int> trianglesList = new List<int>();
        for (int i = 1; i < corners; i++) {
            trianglesList.Add(0);
            trianglesList.Add(i);
            trianglesList.Add(i+1);
        }
        trianglesList.Add(0);
        trianglesList.Add(corners);
        trianglesList.Add(1);

        // the sides mesh (in a CW form)
        int split;
        for (int i = 1; i < corners; i++) {
            // use split_1 if i is odd, else use split_2
            split = _1;
            if (i % 2 == 0) {
                split = _2;
            }
            trianglesList.Add(i+split);
            trianglesList.Add(i+a+split);
            trianglesList.Add((i+1)+split);
            trianglesList.Add((i+1)+split);
            trianglesList.Add(i+a+split);
            trianglesList.Add((i+1)+a+split);
        }
        // if the number of corners is even, use split_2, else use split_3
        split = _2;
        if (corners % 2 == 1) {
            split = _3;
        }
        trianglesList.Add(corners+split);
        trianglesList.Add(corners+a+split);
        trianglesList.Add(1+split);
        trianglesList.Add(1+split);
        trianglesList.Add(corners+a+split);
        trianglesList.Add(1+a+split);

        // the bottom shape mesh
        // _0 == 0 so no need to add
        if (bottomMesh) {
            for (int i = 1; i < corners; i++) {
                trianglesList.Add(0+a);
                trianglesList.Add((i+1)+a);
                trianglesList.Add(i+a);
            }
            trianglesList.Add(0+a);
            trianglesList.Add(1+a);
            trianglesList.Add(corners+a);
        }

        // convert list to array
        triangles = trianglesList.ToArray();
    }

    void CreateOctagonalPrism() {
        // 8 corners
        int a = points.Length; // a = 9

        // for a vertex x, three faces of the prism can use it
        int _0 = 0 * points.Length*2;  // x+_0 for the first triangle-face that uses it
        int _1 = 1 * points.Length*2;  // x+_1 for the second triangle-face that uses it
        int _2 = 2 * points.Length*2;  // x+_2 for the third triangle-face that uses it

        triangles = new int[] {
            // first hexagon
            // _0 == 0 so no need to add
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 7,
            0, 7, 8,
            0, 8, 1,

            // the sides (CW)
            1+_1, 1+a+_1, 2+_1,   2+_1, 1+a+_1, 2+a+_1,
            2+_2, 2+a+_2, 3+_2,   3+_2, 2+a+_2, 3+a+_2,
            3+_1, 3+a+_1, 4+_1,   4+_1, 3+a+_1, 4+a+_1,
            4+_2, 4+a+_2, 5+_2,   5+_2, 4+a+_2, 5+a+_2,
            5+_1, 5+a+_1, 6+_1,   6+_1, 5+a+_1, 6+a+_1,
            6+_2, 6+a+_2, 7+_2,   7+_2, 6+a+_2, 7+a+_2,
            7+_1, 7+a+_1, 8+_1,   8+_1, 7+a+_1, 8+a+_1,
            8+_2, 8+a+_2, 1+_2,   1+_2, 8+a+_2, 1+a+_2,
            
            // the sides (CCW)
            // ...

            // second hexagon, opposite direction
            // _0 == 0 so no need to add
            0+a, 2+a, 1+a,
            0+a, 3+a, 2+a,
            0+a, 4+a, 3+a,
            0+a, 5+a, 4+a,
            0+a, 6+a, 5+a,
            0+a, 7+a, 6+a,
            0+a, 8+a, 7+a,
            0+a, 1+a, 8+a
        };

    }

    void CreateHeptagonalPrism() {
        // 7 corners
        int a = points.Length; // a = 8

        // for a vertex x, three faces of the prism can use it
        int _0 = 0 * points.Length*2;  // x+_0 for the first triangle-face that uses it
        int _1 = 1 * points.Length*2;  // x+_1 for the second triangle-face that uses it
        int _2 = 2 * points.Length*2;  // x+_2 for the third triangle-face that uses it
        int _3 = 3 * points.Length*2;  // x+_3 for the third/fourth triangle-face that uses it
                                       //  (needed when there is an odd number of corners)

        triangles = new int[] {
            // first hexagon
            // _0 == 0 so no need to add
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 7,
            0, 7, 1,

            // the sides (CW)
            1+_1, 1+a+_1, 2+_1,   2+_1, 1+a+_1, 2+a+_1,
            2+_2, 2+a+_2, 3+_2,   3+_2, 2+a+_2, 3+a+_2,
            3+_1, 3+a+_1, 4+_1,   4+_1, 3+a+_1, 4+a+_1,
            4+_2, 4+a+_2, 5+_2,   5+_2, 4+a+_2, 5+a+_2,
            5+_1, 5+a+_1, 6+_1,   6+_1, 5+a+_1, 6+a+_1,
            6+_2, 6+a+_2, 7+_2,   7+_2, 6+a+_2, 7+a+_2,
            7+_3, 7+a+_3, 1+_3,   1+_3, 7+a+_3, 1+a+_3,

            // the sides (CCW)
            // ...

            // second hexagon, opposite direction
            // _0 == 0 so no need to add
            0+a, 2+a, 1+a,
            0+a, 3+a, 2+a,
            0+a, 4+a, 3+a,
            0+a, 5+a, 4+a,
            0+a, 6+a, 5+a,
            0+a, 7+a, 6+a,
            0+a, 1+a, 7+a
        };
    }

    void CreateHexagonalPrism() {
        // 6 corners
        int a = points.Length; // a = 7

        // for a vertex x, three faces of the prism can use it
        int _0 = 0 * points.Length*2;  // x+_0 for the first triangle-face that uses it
        int _1 = 1 * points.Length*2;  // x+_1 for the second triangle-face that uses it
        int _2 = 2 * points.Length*2;  // x+_2 for the third triangle-face that uses it

        triangles = new int[] {
            // first hexagon
            // _0 == 0 so no need to add
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 1,

            // the sides (CW)
            1+_1, 1+a+_1, 2+_1,   2+_1, 1+a+_1, 2+a+_1,
            2+_2, 2+a+_2, 3+_2,   3+_2, 2+a+_2, 3+a+_2,
            3+_1, 3+a+_1, 4+_1,   4+_1, 3+a+_1, 4+a+_1,
            4+_2, 4+a+_2, 5+_2,   5+_2, 4+a+_2, 5+a+_2,
            5+_1, 5+a+_1, 6+_1,   6+_1, 5+a+_1, 6+a+_1,
            6+_2, 6+a+_2, 1+_2,   1+_2, 6+a+_2, 1+a+_2,

            // the sides (CCW)
            // ...

            // second hexagon, opposite direction
            // _0 == 0 so no need to add
            0+a, 2+a, 1+a,
            0+a, 3+a, 2+a,
            0+a, 4+a, 3+a,
            0+a, 5+a, 4+a,
            0+a, 6+a, 5+a,
            0+a, 1+a, 6+a
        };
    }

    void CreatePentagonalPrism() {
        // 5 corners
        int a = points.Length; // a = 6

        // for a vertex x, three faces of the prism can use it
        int _0 = 0 * points.Length*2;  // x+_0 for the first triangle-face that uses it
        int _1 = 1 * points.Length*2;  // x+_1 for the second triangle-face that uses it
        int _2 = 2 * points.Length*2;  // x+_2 for the third triangle-face that uses it
        int _3 = 3 * points.Length*2;  // x+_3 for the third/fourth triangle-face that uses it
                                       //  (needed when there is an odd number of corners)
        triangles = new int[] {
            // first pentagon
            // _0 == 0 so no need to add
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 1,

            // the sides (CW)
            /*1, 1+a, 2,   2, 1+a, 2+a,
            2, 2+a, 3,   3, 2+a, 3+a,
            3, 3+a, 4,   4, 3+a, 4+a,
            4, 4+a, 5,   5, 4+a, 5+a,
            5, 5+a, 1,   1, 5+a, 1+a,*/
            1+_1, 1+a+_1, 2+_1,   2+_1, 1+a+_1, 2+a+_1,
            2+_2, 2+a+_2, 3+_2,   3+_2, 2+a+_2, 3+a+_2,
            3+_1, 3+a+_1, 4+_1,   4+_1, 3+a+_1, 4+a+_1,
            4+_2, 4+a+_2, 5+_2,   5+_2, 4+a+_2, 5+a+_2,
            5+_3, 5+a+_3, 1+_3,   1+_3, 5+a+_3, 1+a+_3,

            // the sides (CCW)
            /*5, 5+a, 1,   1, 5+a, 1+a,
            4, 4+a, 5,   5, 4+a, 5+a,
            3, 3+a, 4,   4, 3+a, 4+a,
            2, 2+a, 3,   3, 2+a, 3+a,
            1, 1+a, 2,   2, 1+a, 2+a*/

            // second pentagon, opposite direction
            // _0 == 0 so no need to add
            0+a, 2+a, 1+a,
            0+a, 3+a, 2+a,
            0+a, 4+a, 3+a,
            0+a, 5+a, 4+a,
            0+a, 1+a, 5+a
            // second pentagon, same direction
            /*0+a, 1+a, 2+a,
            0+a, 2+a, 3+a,
            0+a, 3+a, 4+a,
            0+a, 4+a, 5+a,
            0+a, 5+a, 1+a,*/
        };
    }

    void CreateSquarePrism() {
        // 4 corners, actually a cube
        int a = points.Length; // a = 5

        // for a vertex x, three faces of the prism can use it
        int _0 = 0 * points.Length*2;  // x+_0 for the first triangle-face that uses it
        int _1 = 1 * points.Length*2;  // x+_1 for the second triangle-face that uses it
        int _2 = 2 * points.Length*2;  // x+_2 for the third triangle-face that uses it

        triangles = new int[] {
            // first hexagon
            // _0 == 0 so no need to add
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 1,

            // the sides (CW)
            1+_1, 1+a+_1, 2+_1,   2+_1, 1+a+_1, 2+a+_1,
            2+_2, 2+a+_2, 3+_2,   3+_2, 2+a+_2, 3+a+_2,
            3+_1, 3+a+_1, 4+_1,   4+_1, 3+a+_1, 4+a+_1,
            4+_2, 4+a+_2, 1+_2,   1+_2, 4+a+_2, 1+a+_2,

            // the sides (CCW)
            // ...

            // second hexagon, opposite direction
            // _0 == 0 so no need to add
            0+a, 2+a, 1+a,
            0+a, 3+a, 2+a,
            0+a, 4+a, 3+a,
            0+a, 1+a, 4+a
        };
        
    }

    void CreateTriangularPrism() {
        // 3 corners
    }
}
