using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle 
{
    public Vector2 pointA;
    public Vector2 pointB;
    public Vector2 pointC;

    public Edge edgeAB;
    public Edge edgeBC;
    public Edge edgeCA;

    private Vector2 circumcenter;
    private float radiusSquared;
    private float radius;

    /*public Triangle(Vector2 pA, Vector2 pB, Vector2 pC) {
        pointA = pA;
        pointB = pB;
        pointC = pC;

        edgeAB = new Edge(pointA, pointB);
        edgeBC = new Edge(pointB, pointC);
        edgeCA = new Edge(pointC, pointA);

        bool isCCW = MakeCounterClockwise(pointA, pointB, pointC);
        Debug.Log("is CCW? " + isCCW);
        if (isCCW) {
            // switch points B and C
            Vector2 tmp = pointB;
            pointB = pointC;
            pointC = tmp;
        }
        
        FindCircumcircle();
    }*/

    public Triangle(Edge e1, Edge e2, Edge e3) {
        pointA = e1.pointA;
        pointB = e2.pointA;
        pointC = e3.pointA;

        edgeAB = e1;
        edgeBC = e2;
        edgeCA = e3;

        bool isCCW = MakeCounterClockwise(pointA, pointB, pointC);

        FindCircumcircle();
    }

    private bool MakeCounterClockwise(Vector2 point1, Vector2 point2, Vector2 point3) {
        float result = (point2.x - point1.x) * (point3.y - point1.y) - (point3.x - point1.x) * (point2.y - point1.y);
        if (result > 0) {
            // then points are CCW and nothing needs to be done
            return true;
        }
        else {
            // then points are not CCW and points B and C should be switched
            //Debug.Log("Make CCW");
            Vector2 tmp = pointB;
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
        var dA = p0.x * p0.x + p0.y * p0.y;
        var dB = p1.x * p1.x + p1.y * p1.y;
        var dC = p2.x * p2.x + p2.y * p2.y;

        var aux1 = (dA * (p2.y - p1.y) + dB * (p0.y - p2.y) + dC * (p1.y - p0.y));
        var aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
        var div = (2 * (p0.x * (p2.y - p1.y) + p1.x * (p0.y - p2.y) + p2.x * (p1.y - p0.y)));

        if (div == 0) {
            Debug.LogError("Divide by zero!");
        }

        //var center = new Vector2(aux1 / div, aux2 / div);
        //circumcenter = center;
        circumcenter = new Vector2(aux1 / div, aux2 / div);
        Debug.Log("circumcenter: " + circumcenter);
        //radiusSquared = (center.x - p0.x) * (center.x - p0.x) + (center.y - p0.y) * (center.y - p0.y);
        radius = (circumcenter - p0).magnitude;
    }

    public bool IsPointInsideCircumcircle(Vector2 point) {
        //var d_squared = (point.x - circumcenter.x) * (point.x - circumcenter.x) + (point.y - circumcenter.y) * (point.y - circumcenter.y);
        //return d_squared < radiusSquared;
        float dist = (circumcenter - point).magnitude;
        if (dist < radius) return true;
        else return false;
    }

    public void DrawTriangle() {
        edgeAB.DrawEdge();
        edgeBC.DrawEdge();
        edgeCA.DrawEdge();
    }
}