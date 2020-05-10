﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi 
{
    private float max_x;
    private float max_z;

    public List<VoronoiCell> voronoiCells;

    public Voronoi(float maxX, float maxZ) {
        max_x = maxX;
        max_z = maxZ;
    }

    public void ComputeVoronoi(List<Triangle> triangles, Vector3[] points, bool onlyWithinBoundary=true, bool removeOpenCells=true) {
        // find distinct edges
        /*List<Edge> edges;

        foreach (Edge edge in edges) {
            bool hasTriangleOnBothSides = false;
            if (hasTriangleOnBothSides) {

            }
        }*/

        // centroids and their boundaries
        Dictionary<Vector3, List<Edge>> centroids = new Dictionary<Vector3, List<Edge>>();

        //List<Edge> vorEdges = new List<Edge>();
        for (int i = 0; i < triangles.Count; i++) {
            for (int j = 0; j < triangles.Count; j++) {
                if (i != j) {
                    if (triangles[i].isAdjacent(triangles[j])) {
                        // then the triangles share an edge
                        Vector3 cc1 = triangles[i].GetCircumcenter();
                        Vector3 cc2 = triangles[j].GetCircumcenter();

                        // then the triangles have two points in common, get both of those points
                        Vector3 point1 = Vector3.zero;
                        Vector3 point2 = Vector3.zero;
                        GetCommonPoints(triangles[i], triangles[j], ref point1, ref point2);

                        //if (!onlyWithinBoundary || (onlyWithinBoundary && IsPointWithinBoundary(cc1) && IsPointWithinBoundary(cc2))) {
                            // including those from outside the boundary will look strange and is best avoided
                            Edge newEdge = new Edge(cc1, cc2);
                            //vorEdges.Add(newEdge);

                            // add cc1 to the dictionary
                            if (!centroids.ContainsKey(point1)) {
                                centroids.Add(point1, new List<Edge>());
                            }
                            centroids[point1].Add(newEdge);
                            // add cc2 to the dictionary
                            if (!centroids.ContainsKey(point2)) {
                                centroids.Add(point2, new List<Edge>());
                            }
                            centroids[point2].Add(newEdge);
                        //}
                    }
                }
            }
        }

        voronoiCells = new List<VoronoiCell>();
        foreach (Vector3 key in centroids.Keys) {
            VoronoiCell newCell = new VoronoiCell(key, centroids[key]);
            voronoiCells.Add(newCell);
        }

        if (onlyWithinBoundary) {
            // TODO remove cells that have boundaryPoints outside the main boundary
            RemoveOutOfBoundsCells();
        }

        // TODO remove cells that are only connected to one other cell
        // RemoveLonerCells();
        if (removeOpenCells) {
            RemoveOpenCells();
        }

        /*foreach (VoronoiCell cell in voronoiCells) {
            foreach (Edge e in cell.boundaryEdges) {
                //vorEdges.Add(e);
                e.DrawEdgeColored(Color.red);
            }
        }*/

        //foreach (Edge e in vorEdges) {
        //    e.DrawEdgeColored(Color.red);
        //}
    }

    private bool IsPointWithinBoundary(Vector3 point) {
        bool xOK = (0f <= point.x && point.x <= max_x); 
        bool zOK = (0f <= point.z && point.z <= max_z); 
        return (xOK && zOK);
    }

    private void RemoveOutOfBoundsCells() {
        int counter = 0;
        for (int i = voronoiCells.Count - 1; i >= 0; i--) {
            foreach (Vector3 bp in voronoiCells[i].boundaryPoints) {
                if (!IsPointWithinBoundary(bp)) {
                    voronoiCells.RemoveAt(i);
                    counter++;
                    break;  // break out of current foreach loop
                }
            }
        }
        Debug.Log("Removed " + counter + " out of bounds cells");
    }

    private void RemoveLonerCells() {
        int counter = 0;
        // remove cells that are only connected to one other cell
        for (int i = voronoiCells.Count - 1; i >= 0; i--) {
            // count how many edges this cell shares with other cells
            int edgesShared = 0;
            for (int j = voronoiCells.Count - 1; j >= 0; j--) {
                if (i != j && voronoiCells[i].ShareEdge(voronoiCells[j])) {
                    edgesShared++;
                }
            }
            // if it shares fewer than 2 edges, remove it
            if (edgesShared < 2) {
                voronoiCells.RemoveAt(i);
                counter++;
            }
        }
        Debug.Log("RemovedLonerCells: " + counter);
    }

    private void RemoveOpenCells() {
        // Remove cells that are not closed to form a polygon
        int counter = 0;
        for (int i = voronoiCells.Count - 1; i >= 0; i--) {
            if (!voronoiCells[i].isClosed()) {
                voronoiCells.RemoveAt(i);
                counter++;
            }
        }
        Debug.Log("Removed " + counter + " open cells");
    }

    private void GetCommonPoints(Triangle t1, Triangle t2, ref Vector3 point1, ref Vector3 point2) {
        // t1 and t2 must be adjacent before calling this function

        //Debug.Log("GetCommonPoints from\n" + t1 + "\nand\n" + t2);

        if (t1.IsPointACorner(t2.pointA)) {
            // then t1 and t2 have t2.pointA in common
            if (t1.IsPointACorner(t2.pointB)) {
                // then t1 and t2 have t2.pointA and t2.pointB in common
                point1 = t2.pointA;
                point2 = t2.pointB;
            }
            else {
                // then t1 and t2 have t2.pointA and t2.pointC in common
                point1 = t2.pointA;
                point2 = t2.pointC;
            }
        }
        else {
            // then t1 and t2 have points t2.pointB and t2.pointC in common
            point1 = t2.pointB;
            point2 = t2.pointC;
        }

        //Debug.Log("Common points are " + point1 + " and " + point2);

    }
}
