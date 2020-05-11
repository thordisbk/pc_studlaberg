using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deulaunay Triangulation using the Bowyer-Watson algorithm
// It depends on the following: if I already have a Delaunay triangulation for a given set of points, 
//   I can add a new point and update my triangulation.
// https://leatherbee.org/index.php/2018/10/06/terrain-generation-3-voronoi-diagrams/


public class TriangulationAlgorithm : MonoBehaviour
{
    public bool VERBOSE = false;
    public GameObject spherePrefab;
    private float _y = 0f;  // y-value of the columns' top surface, the height
    
    [Header("Random Points")]
    public int pointsNum = 10;
    public float max_x = 5;
    public float max_z = 5;
    private RandomPoints randomPoints;
    private Vector3[] points;
    
    [Header("Triangulation")]
    //public bool cleanUpBaseTriangle = true;
    public bool validateAfterEveryPoint = true;
    public bool showTriangulation = true;
    public bool doTriangulation = false;
    private bool firstTriangulationDone = false;
    private List<Triangle> triangles;

    [Header("Voronoi")]
    public bool onlyUseVoronoiWithinBoundaries = true;
    public bool removeOpenVoronoiCells = true;
    public bool removeLonerVoronoiCells = false;
    public bool showVoronoiCenterEdges = false;
    public bool showVoronoi = true;
    public bool showVoronoiOnlyWithinBounds = true;
    public bool showVoronoiCenters = true;
    public bool doVoronoi = false;
    private Voronoi voronoi;
    private bool firstVoronoiDone = false;

    [Header("Voronoi Relaxation")]
    public bool doRelaxation = false;
    private int relaxTimes = 0;

    [Header("Columns")]
    public GameObject ColumnMeshPrefab;
    private bool meshesCreated = false;
    private List<GameObject> meshes;
    public bool createBottomFace = true;
    public bool doMesh = false;


    // Start is called before the first frame update
    void Start()
    {
        randomPoints = new RandomPoints(max_x, max_z);
        //triangles = new List<Triangle>();
        doRelaxation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (doTriangulation) {
            doTriangulation = false;
            if (!firstTriangulationDone) {
                // create random points
                points = randomPoints.CreateRandomPoints(pointsNum, VERBOSE);

                ComputeTriangulation();   
                firstTriangulationDone = true;
            }
            else {
                Debug.LogError("Triangulation has already been computed.");
            }
        }

        if (doVoronoi) {
            doVoronoi = false;
            if (!firstVoronoiDone && firstTriangulationDone) {
                ConvertTriangulationToVoronoi();
                firstVoronoiDone = true;
            }
            else if (!firstTriangulationDone) {
                Debug.LogError("Triangulation must be computed before the Voronoi.");
            }
            else {
                Debug.LogError("First Voronoi has already been computed.");
            }
        }

        if (doRelaxation) {
            doRelaxation = false;
            if (firstVoronoiDone) {
                relaxTimes++;
                Debug.Log("Apply Relaxation #" + relaxTimes);
                RelaxVoronoi();
                ConvertTriangulationToVoronoi();
            }
            else {
                Debug.LogError("The first Voronoi must be computed before the diagram is relaxed.");
            }
        }

        if (doMesh) {
            doMesh = false;
            if (!meshesCreated && firstVoronoiDone) {
                CreateMeshColumns();
                meshesCreated = true;
            }
            else if (meshesCreated) {
                Debug.LogError("Meshes have already been generated.");
            }
            else {
                Debug.LogError("The first Voronoi must be computed before the meshes are created.");
            }
        }
    }

    void RelaxVoronoi() {
        // instead of random points, use the average of voronoi cell boundary points
        //List<Vector3> 
        
        List<Vector3> pointsNew = new List<Vector3>();
        foreach (VoronoiCell voronoiCell in voronoi.voronoiCells) {
            // if average center is within initial boundary, use that
            // else use the previous random center
            if (IsPointWithinBoundary(voronoiCell.averageCenter)) {
                pointsNew.Add(voronoiCell.averageCenter);
            }
            else {
                pointsNew.Add(voronoiCell.center);
            }
        }

        points = null;
        points = pointsNew.ToArray();

        //foreach (Vector3 vec in points) { Instantiate(spherePrefab, vec, Quaternion.identity); }

        // clear the current voronoi
        voronoi = null;
        // to the triangulation again
        ComputeTriangulation();
    }

    void CreateMeshColumns() {
        meshes = new List<GameObject>();
        foreach (VoronoiCell cell in voronoi.voronoiCells) {
            if (cell.isValid) {
                int corners = cell.boundaryPoints.Count;
                if (VERBOSE) Debug.Log("Corners: " + corners);
                if (corners >= 3) {
                    GameObject obj = Instantiate(ColumnMeshPrefab, Vector3.zero, Quaternion.identity);
                    ColumnMeshGenerator cmg = obj.AddComponent<ColumnMeshGenerator>() as ColumnMeshGenerator;
                    cmg.Init(cell, createBottomFace);
                    meshes.Add(obj);
                }
            }
        }
        if (VERBOSE) Debug.Log("Number of meshes: " + meshes.Count);
    }

    void ComputeTriangulation() {
        
        // create starting points (the big triangle)
        Vector3[] startPoints = new Vector3[3];
        startPoints[0] = new Vector3(0f, _y, 0f);
        startPoints[1] = new Vector3(0f, _y, 2f * max_z);
        startPoints[2] = new Vector3(2f * max_x, _y, 0f);

        triangles = null;
        // initialize the list to hold the triangles
        //List<Triangle> triangles = new List<Triangle>();
        triangles = new List<Triangle>();

        // create base triangle
        //Triangle firstTriangle = new Triangle(new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[0], startPoints[2]));
        Triangle firstTriangle = new Triangle(new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[2], startPoints[0]));
        triangles.Add(firstTriangle);

        // add a new point to an existing triangulation
        List<Vector3> tmpPoints = new List<Vector3>();
        foreach (Vector3 p in points) {
            if (VERBOSE) Debug.Log("ADD NEW POINT --------------- " + p);
            Triangulation(ref triangles, p);

            if (validateAfterEveryPoint) {
                tmpPoints.Add(p);
                int invalidsCheck = CountInvalidTriangles(triangles, tmpPoints.ToArray(), false);  // findFirst=true
                if (invalidsCheck > 0) {
                    Debug.LogError("------ !!!: found error");
                }
            }
        }

        // check if there are invalid triangles
        int invalids = CountInvalidTriangles(triangles, points, true);  // findFirst=true
        if (invalids > 0) {
            Debug.LogError("Found invalid triangles; triangulation is faulty.");
        }

        // remove base triangle points and the triangles that use them
        //if (cleanUpBaseTriangle) {
        //    CleanupBase(ref triangles, startPoints);
        //}

        // show triangulation
        if (VERBOSE) Debug.Log("------- num of triangles: " + triangles.Count);
        
    }

    void ConvertTriangulationToVoronoi() {
        // check if there are invalid triangles
        int invalids = CountInvalidTriangles(triangles, points, true);  // findFirst=true
        if (invalids > 0) {
            Debug.LogError("Found invalid triangles; triangulation is faulty.");
        }

        voronoi = new Voronoi(max_x, max_z);
        if (invalids == 0) {
            voronoi.ComputeVoronoi(triangles, points);
            voronoi.Cleanup(onlyUseVoronoiWithinBoundaries, removeOpenVoronoiCells, removeLonerVoronoiCells);
        }
    }

    void Triangulation(ref List<Triangle> triangles, Vector3 newPoint) {
        List<Triangle> badTriangles = new List<Triangle>();
        List<Edge> polygonHole = new List<Edge>(); 

        if (VERBOSE) Debug.Log("Number of triangles: " + triangles.Count);

        FindInvalidatedTriangles(ref triangles, newPoint, ref badTriangles, ref polygonHole);
        RemoveDuplicateEdgesFromPolygonHole(ref polygonHole);
        RemoveBadTrianglesFromTriangulation(ref triangles, ref badTriangles);
        FillInPolygonHole(ref triangles, newPoint, ref polygonHole);
        RemoveDuplicateTriangles(ref triangles);  // TODO should not be needed
    }

    void FindInvalidatedTriangles(ref List<Triangle> triangles, Vector3 newPoint, 
                                  ref List<Triangle> badTriangles, ref List<Edge> polygonHole) {
        
        if (VERBOSE) Debug.Log("(before) badtriangles: " + badTriangles.Count);
        if (VERBOSE) Debug.Log("(before) polygonHole: " + polygonHole.Count);
        foreach (Triangle t in triangles) {
            // check if the circumcircle of t contains newPoint
            bool contains = t.IsPointInsideCircumcircle(newPoint);
            if (VERBOSE) Debug.Log(t);
            if (contains) {
                if (VERBOSE) Debug.Log("newPoint is within the circumcircle (add edges to polygonHole)");
                badTriangles.Add(t);
                polygonHole.Add(t.edgeAB);
                polygonHole.Add(t.edgeBC);
                polygonHole.Add(t.edgeCA);
            }
            else {
                if (VERBOSE) Debug.Log("newPoint is NOT within the circumcircle");
            }
        }
        if (VERBOSE) Debug.Log("(after) badtriangles: " + badTriangles.Count);
        if (VERBOSE) Debug.Log("(after) polygonHole: " + polygonHole.Count);
    }

    int CountInvalidTriangles(List<Triangle> triangles, Vector3[] points, bool findFirst=true) {
        int counter = 0;
        foreach (Vector3 p in points) {
            foreach(Triangle t in triangles) {
                if (t.IsPointInsideCircumcircle(p) && !t.IsPointACorner(p)) {
                    if (VERBOSE) Debug.Log(" - Found invalid. Point " + p + " : \n" + t.ToString());
                    counter++;
                    if (findFirst) {
                        // only return first to avoid comparing the rest
                        return counter;
                    }
                }
            }
        }
        if (VERBOSE) Debug.Log(" - Invalid triangles: " + counter);
        return counter;
    }

    void RemoveDuplicateEdgesFromPolygonHole(ref List<Edge> polygonHole) {
        foreach (Edge edge in polygonHole) {
            //  IF the edge's second vertex is left of its first coordinate:
            if (edge.pointB.x < edge.pointA.x) {
                // flip the edge around by swapping its vertices
                //if (VERBOSE) Debug.Log("flip edge");
                edge.FlipEdge();
            }
        }

        // sort
        if (VERBOSE) Debug.Log("polygonHole unsorted: " + polygonHole.Count);
        if (VERBOSE) { foreach (Edge e in polygonHole) Debug.Log(e.ToString()); }
        polygonHole.Sort((e1, e2) => e1.pointB.z.CompareTo(e2.pointB.z));
        polygonHole.Sort((e1, e2) => e1.pointB.x.CompareTo(e2.pointB.x));
        polygonHole.Sort((e1, e2) => e1.pointA.z.CompareTo(e2.pointA.z));
        polygonHole.Sort((e1, e2) => e1.pointA.x.CompareTo(e2.pointA.x));
        if (VERBOSE) Debug.Log("polygonHole sorted: " + polygonHole.Count);
        if (VERBOSE) { foreach (Edge e in polygonHole) Debug.Log(e.ToString()); }

        if (VERBOSE) Debug.Log("polygonHole size before removal: " + polygonHole.Count);
        RemoveRepeatedEdges(ref polygonHole);
        if (VERBOSE) Debug.Log("polygonHole size after removal: " + polygonHole.Count);
        if (VERBOSE) { foreach (Edge e in polygonHole) Debug.Log(e.ToString()); }
    }

    private void RemoveRepeatedEdges(ref List<Edge> polygonHole) {
        // According to leatherbee: IF edge is equivalent to previous edge, remove current edge
        // But here: https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
        //  it says only to include edges that are not shared by 2 or more triangles

        // this functions removes edges from polygonHole that appear once or more
        // given that polygonHole has already been sorted

        if (polygonHole.Count < 2) {
            // then there are either 1 or 0 edges and threfore no duplicates
            return;
        }

        Edge prevEdge = polygonHole[polygonHole.Count-1];
        bool prevDup = false;
        for (int i = polygonHole.Count - 2; i >= 0; i--) {
            if (prevEdge.isEqual(polygonHole[i])) {
                polygonHole.RemoveAt(i+1);
                prevDup = true;
            }
            else {
                if (prevDup) {
                    polygonHole.RemoveAt(i+1);
                    prevDup = false;
                }
            }
            prevEdge = polygonHole[i];
        }
        if (prevDup) {
            polygonHole.RemoveAt(0);
        }
    }

    void RemoveBadTrianglesFromTriangulation(ref List<Triangle> triangles, ref List<Triangle> badTriangles) {
        if (VERBOSE) Debug.Log("# bad triangles: " + badTriangles.Count);
        if (VERBOSE) Debug.Log("triangles size before removal: " + triangles.Count);
        foreach (Triangle t in badTriangles) {
            // removing is not done correctly !
            //triangles.Remove(t);
            for (int i = triangles.Count - 1; i >= 0; i--) {
                if (triangles[i].isSame(t)) {
                    triangles.RemoveAt(i);
                    if (VERBOSE) Debug.Log("Removed triangle: " + t);
                }
            }
        }
        if (VERBOSE) Debug.Log("triangles size after removal: " + triangles.Count);
        if (VERBOSE) { foreach (Triangle t in triangles) Debug.Log(t.ToString()); }
    }

    void FillInPolygonHole(ref List<Triangle> triangles, Vector3 newPoint, ref List<Edge> polygonHole) {
        if (VERBOSE) Debug.Log("polygonHole size: " + polygonHole.Count);
        foreach (Edge edge in polygonHole) {
            Vector3 v1 = edge.pointA;
            Vector3 v2 = edge.pointB;
            //Triangle t = new Triangle(edge, new Edge(newPoint, v1), new Edge(newPoint, v2));  // orig
            Triangle t = new Triangle(edge, new Edge(v2, newPoint), new Edge(newPoint, v1));
            if (VERBOSE) Debug.Log("New triangle circumcenter: " + t.GetCircumcenter());
            triangles.Add(t);
        }
        if (VERBOSE) Debug.Log("After filling:");
        if (VERBOSE) { foreach (Triangle t in triangles) Debug.Log(t.ToString()); }
    }

    private void RemoveDuplicateTriangles(ref List<Triangle> triangles) {
        List<int> indices = new List<int>();
        for (int i = triangles.Count - 1; i >= 0; i--) {
            for (int j = i-1; j >= 0; j--) {
                if (i != j && triangles[i].isSame(triangles[j])) {
                    if (VERBOSE) Debug.Log("! found duplicate triangle");
                    //triangles.RemoveAt(i);
                    if (!indices.Contains(i)) indices.Add(i);
                    if (!indices.Contains(j)) indices.Add(j);
                    // reset
                }
            }            
        }
        if (VERBOSE) Debug.Log(triangles.Count + " triangles, remove:");
        indices.Sort((a, b) => a.CompareTo(b));  // sort ascending
        if (VERBOSE) { for (int i = 0; i < indices.Count; i++) Debug.Log("i: " + indices[i]); }
        // go from highest to lowest index
        for (int i = indices.Count - 1; i >= 0; i--) {
            triangles.RemoveAt(indices[i]);
        }
    }

    private void CleanupBase(ref List<Triangle> triangles, Vector3[] startPoints) {
        // TODO remove this function? doesn't seem to work
        // https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
        // remove all triangles that contain points from the base triangle
        if (VERBOSE) Debug.Log("CLEANUP (before) triangles: " + triangles.Count);
        for (int i = triangles.Count-1; i >= 0; i--) {
            if (triangles[i].IsPointACorner(startPoints[0]) || 
                triangles[i].IsPointACorner(startPoints[1]) || 
                triangles[i].IsPointACorner(startPoints[2])) {
                // then this triangle uses the base triangle points
                triangles.RemoveAt(i);
            }
        }
        if (VERBOSE) Debug.Log("CLEANUP (after) triangles: " + triangles.Count);
    }
        

    void OnDrawGizmos() {

        if (triangles !=  null && showTriangulation) {
            foreach (Triangle t in triangles) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(t.pointA, t.pointB);
                Gizmos.DrawLine(t.pointB, t.pointC);
                Gizmos.DrawLine(t.pointC, t.pointA);
            }
        }

        if (voronoi != null && showVoronoiCenters) {
            // TODO visualize all random points
            /*foreach (Vector3 p in points) {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(p, 0.05f);
            }
            if (!cleanUpBaseTriangle) {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(startPoints[0], 0.05f);
                Gizmos.DrawSphere(startPoints[1], 0.05f);
                Gizmos.DrawSphere(startPoints[2], 0.05f);
            }*/ 
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(cell.center, 0.05f);
            }
            
        }

        if (voronoi != null && showVoronoi) {
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                foreach (Edge e in cell.boundaryEdges) {
                    //e.DrawEdgeColored(Color.red);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(e.pointA, e.pointB);
                }
            }
        }

        if (voronoi != null && showVoronoiOnlyWithinBounds) {
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                if (cell.isValid) {
                    foreach (Edge e in cell.boundaryEdges) {
                        //e.DrawEdgeColored(Color.red);
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(e.pointA, e.pointB);
                    }
                }
                
            }
        }

        if (voronoi != null && showVoronoiCenterEdges) {
            // draw lines from center to all points
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                foreach (Vector3 p in cell.boundaryPoints) {
                    //Edge newEdge = new Edge(cell.center, p);
                    //newEdge.DrawEdgeColored(Color.green);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(cell.center, p);
                }
            }
        }
    }

    private bool IsPointWithinBoundary(Vector3 point) {
        bool xOK = (0f <= point.x && point.x <= max_x); 
        bool zOK = (0f <= point.z && point.z <= max_z); 
        return (xOK && zOK);
    }
}
