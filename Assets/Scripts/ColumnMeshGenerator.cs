using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColumnMeshGenerator : MonoBehaviour
{
    List<Vector2> points;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    private float z = 0f;
    private float columnLength = 5f;

    // this function must be called on creation
    public void Init(VoronoiCell cell) {
        
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        points = new List<Vector2>();
        points.Add(cell.center);
        for (int i = 0; i < cell.boundaryPoints.Count; i++) {
            points.Add(cell.boundaryPoints[i]);
        }

        CreateShape();
        UpdateMesh();
    }

    private void CreateShape() {
        
        int corners = points.Count - 1;  // subtract the center point
        
        vertices = CreateVertices(corners);
        triangles = CreatePrismTriangles(corners);
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    Vector3[] CreateVertices(int corners) {
        int pl = points.Count;
        int numOfSplits;  // min number of vertex splitting for disconected surfaces
        int v;  // number of vertices needed for the mesh

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

        Vector3[] vertices_ = new Vector3[v];
        Debug.Log("Number of vertices: " + v);

        // split the vertices to have hard edges between surfaces
        Vector3[] verticesTemp = new Vector3[pl*2];
        for (int i = 0; i < pl; i++) {
            // for the top of the column
            verticesTemp[i] = points[i];

            // for the bottom of the column
            Vector3 p = new Vector3(points[i].x, 
                                    points[i].y - columnLength, 
                                    z);
            verticesTemp[i+pl] = p;
        }

        if (numOfSplits == 4) {
            vertices_ = verticesTemp.Concat(verticesTemp).Concat(verticesTemp).Concat(verticesTemp).ToArray();
        }
        else {
            vertices_ = verticesTemp.Concat(verticesTemp).Concat(verticesTemp).ToArray();
        }

        return vertices_;
    }

    int[] CreatePrismTriangles(int corners, bool bottomMesh=false) {
        if (corners < 3) {
            Debug.LogError("CreatePrism(): minimum corner number is 3");
            return (new int[0]);
        }
        int a = points.Count;  // corners + 1 (the center)

        // for a vertex x, three faces of the prism can use it
        //  points from which split to use (1st, 2nd, 3rd, or 4th)
        int split_0 = 0 * points.Count*2;  // x+_0 for the first triangle-face that uses it
        int split_1 = 1 * points.Count*2;  // x+_1 for the second triangle-face that uses it
        int split_2 = 2 * points.Count*2;  // x+_2 for the third triangle-face that uses it
        int split_3 = 3 * points.Count*2;  // x+_3 for the third/fourth triangle-face that uses it
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
            split = split_1;
            if (i % 2 == 0) {
                split = split_2;
            }
            trianglesList.Add(i+split);
            trianglesList.Add(i+a+split);
            trianglesList.Add((i+1)+split);
            trianglesList.Add((i+1)+split);
            trianglesList.Add(i+a+split);
            trianglesList.Add((i+1)+a+split);
        }
        // if the number of corners is even, use split_2, else use split_3
        split = split_2;
        if (corners % 2 == 1) {
            split = split_3;
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
        return trianglesList.ToArray();
    }
}
