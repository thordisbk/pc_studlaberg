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
    public float width = 5;
    public float height = 5;


    public bool doTriangulation = false;

    RandomPoints randomPoints;

    public GameObject spherePrefab;

    //private List<Triangle> triangles;

    // Start is called before the first frame update
    void Start()
    {
        randomPoints = new RandomPoints();
        //triangles = new List<Triangle>();
    }

    // Update is called once per frame
    void Update()
    {
        if (doTriangulation) {
            doTriangulation = false;
            InitTriangulation();   
        }
    }

    void InitTriangulation() {
        // create random points
        Vector2[] points = randomPoints.CreateRandomPoints(pointsNum, width, height);

        // create starting points (the big triangle)
        Vector2[] startPoints = new Vector2[3];
        startPoints[0] = new Vector2(0f, 0f);
        startPoints[1] = new Vector2(0f, 2f * height);
        startPoints[2] = new Vector2(2f * width, 0f);

        // visualize points
        foreach (Vector2 p in points) {
            Instantiate(spherePrefab, p, Quaternion.identity);
        }
        Instantiate(spherePrefab, startPoints[0], Quaternion.identity);
        Instantiate(spherePrefab, startPoints[1], Quaternion.identity);
        Instantiate(spherePrefab, startPoints[2], Quaternion.identity);

        // initialize the list ot hold the triangles
        List<Triangle> triangles = new List<Triangle>();

        // create base case triangle
        //Triangle firstTriangle = new Triangle(startPoints[0], startPoints[1], startPoints[2]);
        //Triangle firstTriangle = new Triangle(new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[0], startPoints[2]));
        Triangle firstTriangle = new Triangle(new Edge(startPoints[0], startPoints[1]), new Edge(startPoints[1], startPoints[2]), new Edge(startPoints[2], startPoints[0]));
        triangles.Add(firstTriangle);

        // add a new point to an existing triangulation
        foreach (Vector2 p in points) {
            Debug.Log("ADD NEW POINT --------------- " + p);
            Triangulation(ref triangles, p);
        }

        // show
        Debug.Log("-------");
        Debug.Log("num of triangles: " + triangles.Count);
        foreach(Triangle t in triangles) {
            t.DrawTriangle();
        }
    }

    void Triangulation(ref List<Triangle> triangles, Vector2 newPoint) {
        List<Triangle> badTriangles = new List<Triangle>();
        List<Edge> polygonHole = new List<Edge>(); 

        Debug.Log("Number of triangles: " + triangles.Count);

        FindInvalidatedTriangles(ref triangles, newPoint, ref badTriangles, ref polygonHole);
        RemoveDuplicateEdgesFromPolygonHole(ref polygonHole);
        RemoveBadTrianglesFromTriangulation(ref triangles, ref badTriangles);
        FillInPolygonHole(ref triangles, newPoint, ref polygonHole);
    }

    void FindInvalidatedTriangles(ref List<Triangle> triangles, Vector2 newPoint, 
                                  ref List<Triangle> badTriangles, ref List<Edge> polygonHole) {
        
        Debug.Log("badtriangles: " + badTriangles.Count);
        foreach (Triangle t in triangles) {
            // check if the circumcircle of t contains newPoint
            bool contains = t.IsPointInsideCircumcircle(newPoint);
            if (contains) {
                Debug.Log("newPoint is within the circumcircle (add edged to polygonHole)");
                badTriangles.Add(t);
                polygonHole.Add(t.edgeAB);
                polygonHole.Add(t.edgeBC);
                polygonHole.Add(t.edgeCA);
            }
            else Debug.Log("newPoint is NOT within the circumcircle");
        }
        Debug.Log("(after) badtriangles: " + badTriangles.Count);
    }

    void RemoveDuplicateEdgesFromPolygonHole(ref List<Edge> polygonHole) {
        foreach (Edge edge in polygonHole) {
            //  IF the edge's second vertex is left of its first coordinate:
            if (edge.pointB.x < edge.pointA.x) {
                // flip the edge around by swapping its vertices
                //Debug.Log("flip edge");
                edge.FlipEdge();
            }
        }

        // sort
        //Debug.Log("polygonHole unsorted: " + polygonHole.Count);
        //foreach (Edge e in polygonHole) Debug.Log(e.ToString());
        polygonHole.Sort((e1, e2) => e1.pointA.x.CompareTo(e2.pointA.x));
        //Debug.Log("polygonHole sorted: " + polygonHole.Count);
        //foreach (Edge e in polygonHole) Debug.Log(e.ToString());

        //Debug.Log("polygonHole size before removal: " + polygonHole.Count);
        for (int i = polygonHole.Count-1; i >= 1; i--) {
            // IF edge is equivalent to previous edge, remove current edge
            if (polygonHole[i].pointA == polygonHole[i-1].pointA && polygonHole[i].pointB == polygonHole[i-1].pointB) {
                polygonHole.RemoveAt(i);
            }
        }
        //Debug.Log("polygonHole size after removal: " + polygonHole.Count);
        //Debug.Log("polygonHole after removal: " + polygonHole.Count);
        //foreach (Edge e in polygonHole) Debug.Log(e.ToString());
    }

    void RemoveBadTrianglesFromTriangulation(ref List<Triangle> triangles, ref List<Triangle> badTriangles) {
        Debug.Log("# bad triangles: " + badTriangles.Count);
        Debug.Log("triangles size before removal: " + triangles.Count);
        foreach (Triangle t in badTriangles) {
            triangles.Remove(t);
        }
        Debug.Log("triangles size after removal: " + triangles.Count);
    }

    void FillInPolygonHole(ref List<Triangle> triangles, Vector2 newPoint, ref List<Edge> polygonHole) {
        Debug.Log("polygonHole size: " + polygonHole.Count);
        foreach (Edge edge in polygonHole) {
            Vector2 v1 = edge.pointA;
            Vector2 v2 = edge.pointB;
            //Triangle t = new Triangle(edge, new Edge(newPoint, v1), new Edge(newPoint, v2));  // orig
            Triangle t = new Triangle(edge, new Edge(v2, newPoint), new Edge(newPoint, v1));
            triangles.Add(t);
        }
    }

}
