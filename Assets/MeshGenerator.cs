using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public Transform[] points;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
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
        int v = pl * 2;
        vertices = new Vector3[v];
        for (int i = 0; i < pl; i++) {
            vertices[i] = points[i].position;

            vertices[pl+i] = points[i].position;
            vertices[pl+i].y = vertices[pl+i].y - 5;
        }

        print("Number of vertices: " + v);
        if (pl-1 == 5) CreatePentagonalPrism();
        if (pl-1 == 6) CreateHexagonalPrism();
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    void CreatePentagonalPrism() {
        // 5 corners
        int a = 6;
        triangles = new int[] {
            // first pentagon
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 1,

            // the sides
            1, 1+a, 2,   2, 1+a, 2+a,
            2, 2+a, 3,   3, 2+a, 3+a,
            3, 3+a, 4,   4, 3+a, 4+a,
            4, 4+a, 5,   5, 4+a, 5+a,
            5, 5+a, 1,   1, 5+a, 1+a,

            // second pentagon, opposite direction
            //0+a, 2+a, 1+a,
            //0+a, 3+a, 2+a,
            //0+a, 4+a, 3+a,
            //0+a, 5+a, 4+a,
            //0+a, 1+a, 5+a
            // second pentagon, same direction
            /*0+a, 1+a, 2+a,
            0+a, 2+a, 3+a,
            0+a, 3+a, 4+a,
            0+a, 4+a, 5+a,
            0+a, 5+a, 1+a,*/

        };
    }

    void CreateHexagonalPrism() {
        // 6 corners
    }

    void CreateHeptagonalPrism() {
        // 7 corners
    }

    void CreateSquarePrism() {
        // 4 corners
        // actually a cube
        
    }

    void CreateTriangularPrism() {
        // 3 corners
    }
}
