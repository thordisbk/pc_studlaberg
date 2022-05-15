using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColumnMeshGenerator : MonoBehaviour
{
    List<Vector3> points;

    bool verbose = false;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;

    private float columnLength = 5f;

    private bool bottomMesh = true;

    // this function must be called on creation
    public void Init(VoronoiCell cell, float colLen, bool createBottomMesh=true) 
    { 
        columnLength = colLen;
        bottomMesh = createBottomMesh;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        points = new List<Vector3>();
        points.Add(cell.center);
        for (int i = 0; i < cell.boundaryPoints.Count; i++)
            points.Add(cell.boundaryPoints[i]);

        // currently the object is positioned at (0,0,0) for the points get mapped to their correct place
        //  since Manager is set at (0,0,0)
        // therefore the cell.center (where the object is instantiated) must be subtracted from all points
        for (int i = 0; i < points.Count; i++)
            points[i] = points[i] - cell.center;

        CreateShape();
        UpdateMesh();

        // visualize the vertices
        // foreach (Vector3 p in vertices) { Instantiate(spherePrefab, p, Quaternion.identity); }
    }

    private void CreateShape() 
    {      
        int corners = points.Count - 1;  // subtract the center point
        
        vertices = CreateVertices(corners);
        uv = CreateUVs(corners);
        triangles = CreatePrismTriangles(corners);    
    }

    void UpdateMesh() 
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
    }

    Vector3[] CreateVertices(int corners) 
    {
        int pl = points.Count;
        int numOfSplits;  // min number of vertex splitting for disconected surfaces
        int v;  // number of vertices needed for the mesh

        if (corners % 2 == 1) 
        {
            // then the shape has an odd number of corners (eg triangular, pentagonal)
            //  so there must be four splits
            numOfSplits = 4;
            v = (pl * 2) * 4;
        }
        else 
        {
            numOfSplits = 3;
            v = (pl * 2) * 3;
        }

        Vector3[] vertices_ = new Vector3[v];
        if (verbose) Debug.Log("Number of vertices_: " + v);

        // split the vertices to have hard edges between surfaces
        Vector3[] verticesTemp = new Vector3[pl*2];
        for (int i = 0; i < pl; i++) 
        {
            // for the top of the column
            verticesTemp[i] = points[i];

            // for the bottom of the column
            Vector3 p = new Vector3(points[i].x, 
                                    points[i].y - columnLength, 
                                    points[i].z);
            verticesTemp[i+pl] = p;
        }

        if (numOfSplits == 4)
            vertices_ = verticesTemp.Concat(verticesTemp).Concat(verticesTemp).Concat(verticesTemp).ToArray();
        else
            vertices_ = verticesTemp.Concat(verticesTemp).Concat(verticesTemp).ToArray();

        return vertices_;
    }

    Vector2[] CreateUVs(int corners) 
    {
        int pl = points.Count;
        int numOfSplits;  // min number of vertex splitting for disconected surfaces
        int v;  // number of vertices needed for the mesh

        if (corners % 2 == 1) 
        {
            // then the shape has an odd number of corners (eg triangular, pentagonal)
            //  so there must be four splits
            numOfSplits = 4;
            v = (pl * 2) * 4;
        }
        else 
        {
            numOfSplits = 3;
            v = (pl * 2) * 3;
        }

        Vector2[] uv_ = new Vector2[v];
        if (verbose) Debug.Log("Number of uv_: " + v);

        // split the vertices to have hard edges between surfaces
        Vector2[] uvTemp = new Vector2[pl*2];
        for (int i = 0; i < pl; i++) 
        {
            // for the top of the column
            if (i == 0) uvTemp[i] = new Vector2(0.5f, 0.5f);
            else if (i % 2 == 0) uvTemp[i] = new Vector2(0f, 0f);
            else if (corners % 2 == 1 && i == pl-1) uvTemp[i] = new Vector2(1f, 0f);
            else uvTemp[i] = new Vector2(0f, 1f);

            // for the bottom of the column
            if (i == 0) uvTemp[i+pl] = new Vector2(0.5f, 0.5f);
            else if (i % 2 == 0) uvTemp[i+pl] = new Vector2(1f, 0f);
            else if (corners % 2 == 1 && i == pl-1) uvTemp[i+pl] = new Vector2(0f, 1f);
            else uvTemp[i+pl] = new Vector2(1f, 1f);
        }

        // for the sides of the UV
        Vector2[] uvTempS = new Vector2[pl*2];
        for (int i = 0; i < pl; i++) 
        {

            if (i % 2 == 0) uvTempS[i] = new Vector2(0f, 1f);
            else uvTempS[i] = new Vector2(1f, 1f);

            if (i % 2 == 0) uvTempS[i+pl] = new Vector2(0f, 0f);
            else uvTempS[i+pl] = new Vector2(1f, 0f);
        }
        // for the last odd size
        Vector2[] uvTempL = new Vector2[pl*2];
        for (int i = 0; i < pl; i++) 
        {

            if (i % 2 == 0 || i == corners) uvTempL[i] = new Vector2(0f, 1f);
            else uvTempL[i] = new Vector2(1f, 1f);

            if (i % 2 == 0 || i == corners) uvTempL[i+pl] = new Vector2(0f, 0f);
            else uvTempL[i+pl] = new Vector2(1f, 0f);
        }

        if (numOfSplits == 4)
            uv_ = uvTemp.Concat(uvTempS).Concat(uvTempS).Concat(uvTempL).ToArray();
        else
            uv_ = uvTemp.Concat(uvTempS).Concat(uvTempS).ToArray();

        return uv_;
    }

    int[] CreatePrismTriangles(int corners) 
    {
        if (corners < 3) 
        {
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
        for (int i = 1; i < corners; i++) 
        {
            trianglesList.Add(0);
            trianglesList.Add(i);
            trianglesList.Add(i+1);
        }
        trianglesList.Add(0);
        trianglesList.Add(corners);
        trianglesList.Add(1);

        // the sides mesh (in a CW form)
        // all except the last side
        int split;
        for (int i = 1; i < corners; i++) 
        {
            // use split_1 if i is odd, else use split_2
            split = split_1;
            if (i % 2 == 0)
                split = split_2;

            trianglesList.Add(i+split);
            trianglesList.Add(i+a+split);
            trianglesList.Add((i+1)+split);
            trianglesList.Add((i+1)+split);
            trianglesList.Add(i+a+split);
            trianglesList.Add((i+1)+a+split);
        }
        // if the number of corners is even, use split_2, else use split_3
        // the last side
        split = split_2;
        if (corners % 2 == 1)
            split = split_3;

        trianglesList.Add(corners+split);
        trianglesList.Add(corners+a+split);
        trianglesList.Add(1+split);
        trianglesList.Add(1+split);
        trianglesList.Add(corners+a+split);
        trianglesList.Add(1+a+split);

        // the bottom shape mesh
        // _0 == 0 so no need to add
        if (bottomMesh) 
        {
            for (int i = 1; i < corners; i++) 
            {
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
