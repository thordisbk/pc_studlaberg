﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoronoiCell
{
    public Vector2 center;
    public List<Edge> boundaryEdges;
    public List<Vector2> boundaryPoints;

    public Vector2 averageCenter;

    public VoronoiCell(Vector2 c, List<Edge> edges, bool drawCenterToPoints=false) {
        center = c;

        // create boundaryPoints
        boundaryPoints = new List<Vector2>();
        foreach (Edge edge in edges) {
            boundaryPoints.Add(edge.pointA);
            boundaryPoints.Add(edge.pointB);
        }
        // remove duplicates from boundaryPoints
        boundaryPoints = boundaryPoints.Distinct().ToList();
        // sort boundaryPoints clockwise (needed for mesh generation)
        ClockwiseComparerVector2 cwc = new ClockwiseComparerVector2(center);
        boundaryPoints.Sort(cwc);

        // create boundaryEdges such that the edges are in a clockwise order
        //  not just boundaryEdges = edges;
        boundaryEdges = new List<Edge>();
        for (int i = 1; i < boundaryPoints.Count; i++) {
            Edge newEdge = new Edge(boundaryPoints[i-1], boundaryPoints[i]);
            boundaryEdges.Add(newEdge);
        }
        // last edge, from last point to first
        Edge lastNewEdge = new Edge(boundaryPoints[boundaryPoints.Count-1], boundaryPoints[0]);
        boundaryEdges.Add(lastNewEdge);

        averageCenter = boundaryPoints.Aggregate(Vector2.zero, (acc, v) => acc + v) / boundaryPoints.Count;
        Debug.Log("Average center: " + averageCenter);

        Debug.Log("VoronoiCell vertices around center " + center + ":");
        foreach (Vector2 v in boundaryPoints) Debug.Log(v);

        if (drawCenterToPoints) {
            // draw lines from center to all points
            foreach (Vector2 p in boundaryPoints) {
                Edge newEdge = new Edge(center, p);
                newEdge.DrawEdgeColored(Color.green);
            }
        }
    }

    public bool ShareEdge(VoronoiCell cell) {
        // returns true if cell and this cell share an edge
        // since edges are created clockwise, it should not be neccessary to flip edges
        if (cell == null) return false;
        foreach (Edge e1 in boundaryEdges) {
            foreach (Edge e2 in cell.boundaryEdges) {
                if (e1.isEqual(e2)) {
                    return true;
                }   
            }
        }
        return false;
    }

    public bool isClosed() {
        // return true if the cell is closed, that is, the edges form a polygon
        // boundaryEdges are in clockwise order, so going through them like this should work
        Vector2 firstPoint = boundaryEdges[0].pointA;
        Vector2 currPoint = boundaryEdges[0].pointB;
        Debug.Log("Check if closed:");
        for (int i = 0; i < boundaryEdges.Count; i++) {
            /*if (currPoint != boundaryPoints) {
                return false;
            }
            currPoint = boundaryEdges[i].pointB;*/
            Debug.Log(boundaryEdges[i].ToString());
        }
        if (boundaryEdges[0].pointA == boundaryEdges[boundaryEdges.Count-1].pointB) {
            Debug.Log("cell is Closed");
            return true;
        }
        Debug.Log("cell is Open");
        return false;
    }
}