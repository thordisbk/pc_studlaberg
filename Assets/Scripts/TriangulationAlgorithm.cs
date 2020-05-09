using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deulaunay Triangulation using the Bowyer-Watson algorithm
// It depends on the following: if I already have a Delaunay triangulation for a given set of points, 
//   I can add a new point and update my triangulation.
// https://leatherbee.org/index.php/2018/10/06/terrain-generation-3-voronoi-diagrams/

/*

PSEUDO-CODE:

Triangulation(List triangles, Point newPoint):
    Let List badTriangles, List polygonHole be empty;
    CALL FindInvalidatedTriangles(triangles, newPoint, badTriangles, polygonHole);
    CALL RemoveDuplicateEdgesFromPolygonHole(polygonHole);
    CALL RemoveBadTrianglesFromTriangulation(triangles, badTriangles);
    CALL FillInPolygonHole(triangles, newPoint, polygonHole);

FindInvalidatedTriangles(List triangles, Point newPoint, List badTriangles, List polygonHole):
    FOR each t in triangles:
        IF the circumcircle of t contains newPoint:
            add t to badTriangles;
            add all of t's 3 edges to polygonHole;

RemoveDuplicateEdgesFromPolygonHole(List polygonHole):
    FOR each edge in polygonHole:
        IF the edge's second vertex is left of its first coordinate:
            flip the edge around by swapping its vertices
  
    sort the edges in polygonHole by first vertex x-coordinate
    FOR each edge in polygonHole:
        IF edge is equivalent to previous edge, remove current edge

RemoveBadTrianglesFromTriangulation(List triangles, List badTriangles):
    FOR each t in badTriangles:
        find t in triangles and remove

FillInPolygonHole(List triangles, Point newPoint, List polygonHole):
    FOR each edge in polygonHole:
        Let v1 and v2 be the two vertices of edge
        Let t be a new triangle with edges {edge, newPoint to v1, newPoint to v2}
        add t to triangles
*/


public class TriangulationAlgorithm : MonoBehaviour
{
    public int pointsNum = 10;
    public float max_x = 5;
    public float max_y = 5;
    public GameObject spherePrefab;

    private RandomPoints randomPoints;

    public bool VERBOSE = false;
    public bool doTriangulation = false;
    public bool doRelaxation = false;
    [Header("Triangulation")]
    public bool cleanUpBaseTriangle = true;
    public bool validateAfterEveryPoint = true;
    public bool showTriangulation = true;
    [Header("Voronoi")]
    public bool onlyUseVoronoiWithinBoundaries = true;
    public bool removeOpenVoronoiCells = true;
    public bool drawVoronoiCenterEdges = false;
    public bool showVoronoi = true;

    private bool firstTriangulationDone = false;
    private Voronoi voronoi;
    private int relaxTimes = 0;

    //private List<Triangle> triangles;

    // Start is called before the first frame update
    void Start()
    {
        randomPoints = new RandomPoints(max_x, max_y);
        //triangles = new List<Triangle>();
        doRelaxation = false;
        
        voronoi = new Voronoi(max_x, max_y);
    }

    // Update is called once per frame
    void Update()
    {
        if (doTriangulation && !firstTriangulationDone) {
            doTriangulation = false;

            // create random points
            Vector2[] points = randomPoints.CreateRandomPoints(pointsNum, VERBOSE);

            InitTriangulation(points);   
            firstTriangulationDone = true;
        }

        if (doRelaxation) {
            if (!firstTriangulationDone) {
                Debug.Log("Cannot to relaxation until triangulation is ready.");
            }
            else {
                relaxTimes++;
                Debug.Log("Apply Relaxation #" + relaxTimes);
                RelaxVoronoi();
            }
            doRelaxation = false;
        }
    }

    void RelaxVoronoi() {
        // instead of random points, use the average of voronoi cell boundary points
        List<Vector2> points = new List<Vector2>();
        foreach (VoronoiCell voronoiCell in voronoi.voronoiCells) {
            points.Add(voronoiCell.averageCenter);
        }
        // clear the current voronoi
        voronoi = null;
        voronoi = new Voronoi(max_x, max_y);
        // to the triangulation again
        InitTriangulation(points.ToArray());
    }

    void InitTriangulation(Vector2[] points) {
        
        // create starting points (the big triangle)
        Vector2[] startPoints = new Vector2[3];
        startPoints[0] = new Vector2(0f, 0f);
        startPoints[1] = new Vector2(0f, 2f * max_y);
        startPoints[2] = new Vector2(2f * max_x, 0f);

        // visualize points
        foreach (Vector2 p in points) {
            Instantiate(spherePrefab, p, Quaternion.identity);
        }
        if (!cleanUpBaseTriangle) {
            Instantiate(spherePrefab, startPoints[0], Quaternion.identity);
            Instantiate(spherePrefab, startPoints[1], Quaternion.identity);
            Instantiate(spherePrefab, startPoints[2], Quaternion.identity);
        }

        // initialize the list to hold the triangles
        List<Triangle> triangles = new List<Triangle>();

        // create base triangle
        //Triangle firstTriangle = new Triangle(new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[0], startPoints[2]));
        Triangle firstTriangle = new Triangle(new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[2], startPoints[0]));
        triangles.Add(firstTriangle);

        // add a new point to an existing triangulation
        List<Vector2> tmpPoints = new List<Vector2>();
        foreach (Vector2 p in points) {
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

        if (invalids == 0) {
            // compute VoronoiDiagram TODO do in another class
            voronoi.ComputeVoronoi(triangles, points, onlyUseVoronoiWithinBoundaries, removeOpenVoronoiCells, drawVoronoiCenterEdges);
        }

        // cleanup 
        if (cleanUpBaseTriangle) {
            Cleanup(ref triangles, startPoints);
        }

        // show triangulation
        if (VERBOSE) Debug.Log("------- num of triangles: " + triangles.Count);
        if (showTriangulation) {
            foreach (Triangle t in triangles) {
                t.DrawTriangle();
            }
        }

        if (showVoronoi) {
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                foreach (Edge e in cell.boundaryEdges) {
                    e.DrawEdgeColored(Color.red);
                }
            }
        }

    }

    void Triangulation(ref List<Triangle> triangles, Vector2 newPoint) {
        List<Triangle> badTriangles = new List<Triangle>();
        List<Edge> polygonHole = new List<Edge>(); 

        if (VERBOSE) Debug.Log("Number of triangles: " + triangles.Count);

        FindInvalidatedTriangles(ref triangles, newPoint, ref badTriangles, ref polygonHole);
        RemoveDuplicateEdgesFromPolygonHole(ref polygonHole);
        RemoveBadTrianglesFromTriangulation(ref triangles, ref badTriangles);
        FillInPolygonHole(ref triangles, newPoint, ref polygonHole);
        RemoveDuplicateTriangles(ref triangles);  // TODO should not be needed
    }

    void FindInvalidatedTriangles(ref List<Triangle> triangles, Vector2 newPoint, 
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

    int CountInvalidTriangles(List<Triangle> triangles, Vector2[] points, bool findFirst=true) {
        int counter = 0;
        foreach (Vector2 p in points) {
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
        polygonHole.Sort((e1, e2) => e1.pointB.y.CompareTo(e2.pointB.y));
        polygonHole.Sort((e1, e2) => e1.pointB.x.CompareTo(e2.pointB.x));
        polygonHole.Sort((e1, e2) => e1.pointA.y.CompareTo(e2.pointA.y));
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

    void FillInPolygonHole(ref List<Triangle> triangles, Vector2 newPoint, ref List<Edge> polygonHole) {
        if (VERBOSE) Debug.Log("polygonHole size: " + polygonHole.Count);
        foreach (Edge edge in polygonHole) {
            Vector2 v1 = edge.pointA;
            Vector2 v2 = edge.pointB;
            //Triangle t = new Triangle(edge, new Edge(newPoint, v1), new Edge(newPoint, v2));  // orig
            Triangle t = new Triangle(edge, new Edge(v2, newPoint), new Edge(newPoint, v1));
            if (VERBOSE) Debug.Log("New triangle circumcenter: " + t.GetCircumcenter());
            triangles.Add(t);
        }
        if (VERBOSE) Debug.Log("After filling:");
        if (VERBOSE) { foreach (Triangle t in triangles) Debug.Log(t.ToString()); }
    }

    private void RemoveDuplicateTriangles(ref List<Triangle> triangles) {
        /*foreach (Triangle t1 in triangles) {
            int counter = 0;
            foreach (Triangle t2 in triangles) {
                if (t1.isSame(t2)) {
                    counter++;
                }
            }
            if (VERBOSE) Debug.Log("Counter: " + counter + ". Triangle: " + t1);
        }*/
        List<int> indices = new List<int>();
        for (int i = triangles.Count - 1; i >= 0; i--) {
            for (int j = i; j >= 0; j--) {
                if (i != j && triangles[i].isSame(triangles[j])) {
                    if (VERBOSE) Debug.Log("! found duplicate triangle");
                    //triangles.RemoveAt(i);
                    indices.Add(i);
                    indices.Add(j);
                    // reset
                }
            }            
        }
        indices.Sort((a, b) => b.CompareTo(a));
        if (VERBOSE) { foreach (int i in indices) Debug.Log("i: " + i); }
        foreach (int i in indices) {
            triangles.RemoveAt(i);  // will go from highest to lowest index
        }
    }

    private void Cleanup(ref List<Triangle> triangles, Vector2[] startPoints) {
        // remove all triangles that contain points from the base triangle
        //if (VERBOSE)
        Debug.Log("CLEANUP (before) triangles: " + triangles.Count);
        for (int i = triangles.Count-1; i >= 0; i--) {
            if (triangles[i].IsPointACorner(startPoints[0]) || 
                triangles[i].IsPointACorner(startPoints[1]) || 
                triangles[i].IsPointACorner(startPoints[2])) {
                // then this triangle uses the base triangle points
                triangles.RemoveAt(i);
            }
        }
        //if (VERBOSE)
        Debug.Log("CLEANUP (after) triangles: " + triangles.Count);
    }
        

    void OnDrawGizmos() {

    }
}
