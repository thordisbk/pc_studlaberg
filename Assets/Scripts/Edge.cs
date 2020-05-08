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

    public void FlipEdge() {
        // flip the edge around by swapping its vertices
        Vector2 temp = pointA;
        pointA = pointB;
        pointB = temp;
    }

    public override string ToString() {
        return "Edge from " + pointA + " to " + pointB;
    }  
}