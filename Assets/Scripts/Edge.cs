using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge 
{
    public Vector2 pointA;
    public Vector2 pointB;

    private float duration = 5000f;

    public float Length;

    public Edge(Vector2 pA, Vector2 pB) {
        pointA = pA;
        pointB = pB;

        Length = (pA - pB).magnitude;
    }

    public void DrawEdge() {
        Debug.DrawLine(pointA, pointB, Color.white, duration);
    }

    public void DrawEdgeColored(Color color) {
        Debug.DrawLine(pointA, pointB, color, duration);
    }

    public void FlipEdge() {
        // flip the edge around by swapping its vertices
        Vector2 temp = pointA;
        pointA = pointB;
        pointB = temp;
    }

    public override string ToString() {
        return "Edge from " + pointA + " to " + pointB;
    }  

    public bool isEqual(Edge other) {
        // return true if the edges as equal
        if (other == null) return false;
        return (pointA == other.pointA && pointB == other.pointB);
    }

    public bool isSame(Edge other) {
        // returns true if the edges lie between the same points
        if (other == null) return false;
        bool same = (pointA == other.pointA && pointB == other.pointB);
        bool sameFlipped = (pointA == other.pointB && pointB == other.pointA);
        return (same || sameFlipped);
    }
}