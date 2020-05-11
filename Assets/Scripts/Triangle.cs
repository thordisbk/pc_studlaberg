using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle 
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;

    public Edge edgeAB;
    public Edge edgeBC;
    public Edge edgeCA;

    private Vector3 circumcenter;
    private float radiusSquared;
    private float radius;

    private float _y = 0f;

    public Triangle(Edge e1, Edge e2, Edge e3) {
        pointA = e1.pointA;
        pointB = e2.pointA;
        pointC = e3.pointA;

        edgeAB = e1;
        edgeBC = e2;
        edgeCA = e3;

        //if (edgeAB.isSame(edgeBC) || edgeAB.isSame(edgeCA) || edgeBC.isSame(edgeCA)) {
        if (pointA == pointB || pointB == pointC || pointC == pointA) {
            Debug.LogError("Same edge added twice to triangle!");
            //Debug.Log("pointA: " + pointA + " pointB: " + pointB + " pointC: " + pointC);
            // Debug.Log(" e1: " + e1.ToString() + " e2: " +e2.ToString() + " e3: " +e3.ToString());
            //edgeAB.DrawEdgeColored(Color.yellow);
            //edgeBC.DrawEdgeColored(Color.magenta);
            //edgeCA.DrawEdgeColored(Color.cyan);
        }

        bool isCCW = MakeCounterClockwise(pointA, pointB, pointC);

        FindCircumcircle();
    }

    public Vector3 GetCircumcenter() {
        return circumcenter;
    }

    private bool MakeCounterClockwise(Vector3 point1, Vector3 point2, Vector3 point3) {
        float result = (point2.x - point1.x) * (point3.z - point1.z) - (point3.x - point1.x) * (point2.z - point1.z);
        if (result > 0) {
            // then points are CCW and nothing needs to be done
            return true;
        }
        else {
            // then points are not CCW and points B and C should be switched
            //Debug.Log("Make CCW");
            Vector3 tmp = pointB;
            pointB = pointC;
            pointC = tmp;

            edgeAB = new Edge(pointA, pointB);
            edgeBC = new Edge(pointB, pointC);
            edgeCA = new Edge(pointC, pointA);

            return false;
        }
    }

    private void FindCircumcircle() {
        // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
        // https://en.wikipedia.org/wiki/Circumscribed_circle
        var p0 = pointA;
        var p1 = pointB;
        var p2 = pointC;
        var dA = p0.x * p0.x + p0.z * p0.z;
        var dB = p1.x * p1.x + p1.z * p1.z;
        var dC = p2.x * p2.x + p2.z * p2.z;

        var aux1 = (dA * (p2.z - p1.z) + dB * (p0.z - p2.z) + dC * (p1.z - p0.z));
        var aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
        var div = (2 * (p0.x * (p2.z - p1.z) + p1.x * (p0.z - p2.z) + p2.x * (p1.z - p0.z)));

        if (div == 0) {
            Debug.LogError("Divide by zero!");
        }

        //var center = new Vector3(aux1 / div, _y, aux2 / div);
        //circumcenter = center;
        circumcenter = new Vector3(aux1 / div, _y, aux2 / div);
        // Debug.Log("circumcenter: " + circumcenter);
        //radiusSquared = (center.x - p0.x) * (center.x - p0.x) + (center.z - p0.z) * (center.z - p0.z);
        radius = (circumcenter - p0).magnitude;
    }

    public bool IsPointInsideCircumcircle(Vector3 point) {
        //var d_squared = (point.x - circumcenter.x) * (point.x - circumcenter.x) + (point.z - circumcenter.z) * (point.z - circumcenter.z);
        //return d_squared < radiusSquared;
        float dist = (circumcenter - point).magnitude;
        if (dist < radius) return true;
        else return false;
    }

    public bool IsPointACorner(Vector3 point) {
        // returns true if point is part of the Triangles 3 vertices
        return (point == pointA || point == pointB || point == pointC);
    }

    public void DrawTriangle() {
        edgeAB.DrawEdge();
        edgeBC.DrawEdge();
        edgeCA.DrawEdge();
    }

    public override string ToString() {
        return "Triangle defined by:\n" + edgeAB + "\n" + edgeBC + "\n" + edgeCA + "\n";
    }  

    public bool isSame(Triangle other) {
        // returns true if the triangles share their vertices
        if (other == null) return false;
        if (!(pointA == other.pointA || pointA == other.pointB || pointA == other.pointC)) return false;
        if (!(pointB == other.pointA || pointB == other.pointB || pointB == other.pointC)) return false;
        if (!(pointC == other.pointA || pointC == other.pointB || pointC == other.pointC)) return false;
        // then found pointA, pointB and pointC in other
        return true;
    }

    public bool isAdjacent(Triangle other) {
        // returns true if the triangles are adjacent, that is, they share an edge
        if (other == null) return false;
        // they may not be the same triangle
        if (this.isSame(other)) return false;
        // they have to share at least one edge
        if (edgeAB.isSame(other.edgeAB) || edgeAB.isSame(other.edgeBC) || edgeAB.isSame(other.edgeCA)) return true;
        if (edgeBC.isSame(other.edgeAB) || edgeBC.isSame(other.edgeBC) || edgeBC.isSame(other.edgeCA)) return true;
        if (edgeCA.isSame(other.edgeAB) || edgeCA.isSame(other.edgeBC) || edgeCA.isSame(other.edgeCA)) return true;
        return false;
    }
}