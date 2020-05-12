using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Deulaunay Triangulation using the Bowyer-Watson algorithm
// It depends on the following: if I already have a Delaunay triangulation for a given set of points, 
//   I can add a new point and update my triangulation.
// https://leatherbee.org/index.php/2018/10/06/terrain-generation-3-voronoi-diagrams/


public class Triangulation
{
    public bool VERBOSE = false;
    private float _y = 0f;  // y-value of the columns' top surface, the height
    
    private float max_x = 5;
    private float max_z = 5;
    private Vector3[] points;
    
    private bool validateAfterEveryPoint = true;
    public List<Triangle> triangles;

    public Triangulation(float maxX, float maxZ, float y, Vector3[] thePoints, bool verbose=false, bool validateAlways=true) {
        max_x = maxX;
        max_z = maxZ;
        _y = y;
        validateAfterEveryPoint = validateAlways;
        points = thePoints;
        VERBOSE = verbose;
    }

    public void ComputeTriangulation() {
        
        // create starting points (the big triangle)
        Vector3[] startPoints = new Vector3[3];
        startPoints[0] = new Vector3(0f, _y, 0f);
        startPoints[1] = new Vector3(0f, _y, 2f * max_z);
        startPoints[2] = new Vector3(2f * max_x, _y, 0f);

        // remove any instances of startPoints from points
        List<Vector3> pointsList = new List<Vector3>(points);
        bool rem0 = pointsList.Remove(startPoints[0]);
        bool rem1 = pointsList.Remove(startPoints[1]);
        bool rem2 = pointsList.Remove(startPoints[2]);
        int removedCounter = (rem0 ? 1 : 0) + (rem1 ? 1 : 0) + (rem2 ? 1 : 0);
        //if (removedCounter > 0) Debug.Log(removedCounter + " values removed from points[] before triangulation");
        points = pointsList.ToArray();

        // initialize the list to hold the triangles
        triangles = new List<Triangle>();

        // create base triangle
        Triangle firstTriangle = new Triangle(new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[2], startPoints[0]));
        triangles.Add(firstTriangle);

        // add a new point to an existing triangulation
        List<Vector3> tmpPoints = new List<Vector3>();
        foreach (Vector3 p in points) {
            if (VERBOSE) Debug.Log("ADD NEW POINT --------------- " + p);
            TriangulationStart(p);

            if (validateAfterEveryPoint) {
                tmpPoints.Add(p);
                int invalidsCheck = CountInvalidTriangles(tmpPoints.ToArray(), false);  // findFirst=true
                if (invalidsCheck > 0) {
                    Debug.LogError("------ !!!: found error");
                }
            }
        }

        // check if there are invalid triangles
        int invalids = CountInvalidTriangles(points, true);  // findFirst=true
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

    private void TriangulationStart(Vector3 newPoint) {
        List<Triangle> badTriangles = new List<Triangle>();
        List<Edge> polygonHole = new List<Edge>(); 

        if (VERBOSE) Debug.Log("Number of triangles: " + triangles.Count);

        FindInvalidatedTriangles(newPoint, ref badTriangles, ref polygonHole);
        // AddEdgesToPolygonHole(ref polygonHole, badTriangles);
        RemoveDuplicateEdgesFromPolygonHole(ref polygonHole);
        RemoveBadTrianglesFromTriangulation(ref badTriangles);
        FillInPolygonHole(newPoint, ref polygonHole);
        RemoveDuplicateTriangles();  // FIXME should not be needed
    }

    private void FindInvalidatedTriangles(Vector3 newPoint, ref List<Triangle> badTriangles, ref List<Edge> polygonHole) {
        
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

    private void AddEdgesToPolygonHole(ref List<Edge> polygonHole, List<Triangle> badTriangles) {
        for (int i = 0; i < badTriangles.Count; i++) {
            Edge e1 = badTriangles[i].edgeAB;
            Edge e2 = badTriangles[i].edgeBC;
            Edge e3 = badTriangles[i].edgeCA;
            bool e1clear = true, e2clear = true, e3clear = true;

            for (int j = i+1; j < badTriangles.Count; j++) {
                // check if badTriangles[j] contains one of the 3 edges
                if (badTriangles[j].hasEdge(e1)) {
                    e1clear = false;
                }
                if (badTriangles[j].hasEdge(e2)) {
                    e2clear = false;
                }
                if (badTriangles[j].hasEdge(e3)) {
                    e3clear = false;
                }
            }

            // then the edges are not shared by any other triangle in badTriangles and can be added to polygonHole
            if (e1clear) polygonHole.Add(e1);
            if (e2clear) polygonHole.Add(e2);
            if (e3clear) polygonHole.Add(e3);
        }
    }

    private void RemoveDuplicateEdgesFromPolygonHole(ref List<Edge> polygonHole) {
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

    private void RemoveBadTrianglesFromTriangulation(ref List<Triangle> badTriangles) {
        if (VERBOSE) Debug.Log("# bad triangles: " + badTriangles.Count);
        if (VERBOSE) Debug.Log("triangles size before removal: " + triangles.Count);
        foreach (Triangle t in badTriangles) {
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

    private void FillInPolygonHole(Vector3 newPoint, ref List<Edge> polygonHole) {
        if (VERBOSE) Debug.Log("polygonHole size: " + polygonHole.Count);
        foreach (Edge edge in polygonHole) {
            Vector3 v1 = edge.pointA;
            Vector3 v2 = edge.pointB;

            Edge new1 = new Edge(v2, newPoint);
            Edge new2 = new Edge(newPoint, v1);
            Triangle t = new Triangle(edge, new1, new2);

            if (VERBOSE) Debug.Log("New triangle circumcenter: " + t.GetCircumcenter());
            triangles.Add(t);
        }
        if (VERBOSE) Debug.Log("After filling:");
        if (VERBOSE) { foreach (Triangle t in triangles) Debug.Log(t.ToString()); }
    }

    private void RemoveDuplicateTriangles() {
        // TODO optimize or make this function unneccesary
        List<int> indices = new List<int>();
        for (int i = triangles.Count - 1; i >= 0; i--) {
            for (int j = i-1; j >= 0; j--) {
                if (i != j && triangles[i].isSame(triangles[j])) {
                    if (VERBOSE) Debug.Log("! found duplicate triangle");
                    indices.Add(i);
                    indices.Add(j);
                }
            }            
        }
        if (VERBOSE) Debug.Log(triangles.Count + " triangles, remove:");
        indices = indices.Distinct().ToList();
        indices.Sort((a, b) => a.CompareTo(b));  // sort ascending
        if (VERBOSE) {  for (int i = 0; i < indices.Count; i++) Debug.Log("i: " + indices[i]); }
        // go from highest to lowest index
        for (int i = indices.Count - 1; i >= 0; i--) {
            triangles.RemoveAt(indices[i]);
        }
    }

    public int CountInvalidTriangles(Vector3[] points, bool findFirst=true) {
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

    private void CleanupBase(Vector3[] startPoints) {
        // only do this if the triangulation should not be converted to Voronoi
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
}
