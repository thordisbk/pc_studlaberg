using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi 
{
    private float max_x;
    private float max_y;

    public Voronoi(float maxX, float maxY) {
        max_x = maxX;
        max_y = maxY;
    }

    public void ComputeVoronoi(List<Triangle> triangles, Vector2[] points) {
        // find distinct edges
        /*List<Edge> edges;

        foreach (Edge edge in edges) {
            bool hasTriangleOnBothSides = false;
            if (hasTriangleOnBothSides) {

            }
        }*/

        List<Edge> vorEdges = new List<Edge>();
        for (int i = 0; i < triangles.Count; i++) {
            for (int j = 0; j < triangles.Count; j++) {
                if (i != j) {
                    if (triangles[i].isAdjacent(triangles[j])) {
                        // then the triangles share an edge
                        Vector2 cc1 = triangles[i].GetCircumcenter();
                        Vector2 cc2 = triangles[j].GetCircumcenter();

                        if (IsPointWithinBoundary(cc1) && IsPointWithinBoundary(cc2)) {
                            Edge newEdge = new Edge(cc1, cc2);
                            vorEdges.Add(newEdge);
                        }
                    }
                }
            }
        }

        foreach (Edge e in vorEdges) {
            e.DrawEdgeColored(Color.red);
        }
    }

    private bool IsPointWithinBoundary(Vector2 point) {
        bool xOK = (0f <= point.x && point.x <= max_x); 
        bool yOK = (0f <= point.y && point.y <= max_y); 
        return (xOK && yOK);
    }
}
