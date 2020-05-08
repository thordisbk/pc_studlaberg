using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoints
{
    public bool useMinDist = true;
    private float minDist = 0.11f;
    private float minDistSqr;

    public Vector2[] CreateRandomPoints(int num, float maxX, float maxY, bool verbose=false) {
        // returns a Vector2 array of random values on a specified plane
        minDistSqr = minDist * minDist;

        Vector2[] arr = new Vector2[num];
        for (int i = 0; i < num; i++) {
            float x = Random.Range(0f, maxX);
            float y = Random.Range(0f, maxY);
            Vector2 newPoint = new Vector2(x, y);

            if (useMinDist) {
                if (!isPointTooClose(arr, newPoint)) {
                    arr[i] = newPoint;
                    if (verbose) Debug.Log("newPoint: " + newPoint);
                }
                else {
                    Debug.Log("RandomPoints: Point too close, create another...");
                    i--;
                }
            }
            else {
                arr[i] = newPoint;
                if (verbose) Debug.Log("newPoint: " + newPoint);
            }
        }
        //arr[0] = new Vector2(4.7f, 4.7f);
        //if (num > 1) arr[1] = new Vector2(3.2f, 2.2f);
        return arr;
    }

    private bool isPointTooClose(Vector2[] points, Vector2 point) { 
        // check if point is at least minDist away from all other points
        //  if not, create a new point
        foreach (Vector2 p in points) {
            float distSqr = (p - point).sqrMagnitude;
            if (distSqr < minDistSqr) {
                return true;
            }
        }
        return false;
    }
}
