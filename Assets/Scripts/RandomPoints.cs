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

        //return UseTestPoints();

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

        string arrStr = "";
        foreach (Vector2 p in arr) arrStr = arrStr + p + ",";
        if (verbose) Debug.Log(arrStr);

        return arr;
    }

    private Vector2[] UseTestPoints() {
        /*
                */
        Vector2[] arr = new Vector2[] {
            new Vector2(2.8f, 9.1f),new Vector2(3.6f, 9.0f),new Vector2(0.9f, 3.4f),new Vector2(9.6f, 3.6f),new Vector2(9.8f, 6.7f),
            new Vector2(9.5f, 6.1f),new Vector2(9.9f, 3.1f),new Vector2(8.2f, 7.1f),new Vector2(5.8f, 8.7f),new Vector2(0.1f, 6.8f),
            new Vector2(8.9f, 2.8f),new Vector2(0.2f, 4.7f),new Vector2(4.7f, 5.3f),new Vector2(4.2f, 7.6f)
            
            //,new Vector2(1.8f, 3.1f),
            //new Vector2(9.4f, 1.9f),new Vector2(9.4f, 8.6f),new Vector2(9.1f, 7.3f),new Vector2(6.3f, 9.3f),new Vector2(1.0f, 1.7f),
            //new Vector2(3.9f, 6.9f),new Vector2(8.4f, 5.8f),new Vector2(6.4f, 7.7f),new Vector2(9.3f, 5.6f),new Vector2(3.1f, 3.5f),
            
            //new Vector2(2.6f, 5.2f),new Vector2(4.9f, 2.0f),new Vector2(4.8f, 2.1f),new Vector2(6.9f, 7.7f),new Vector2(3.2f, 0.5f),
            //new Vector2(5.2f, 8.8f),new Vector2(8.2f, 1.8f),new Vector2(2.4f, 8.9f),new Vector2(6.8f, 5.7f),new Vector2(2.4f, 5.5f)
        };
        return arr;
        // 24 ok
        // 25 not ok

        // actually 14 not ok

        // seems like triangles are being repeated
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
