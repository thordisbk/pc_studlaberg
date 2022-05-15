using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoints
{
    private float max_x;
    private float max_z;

    public bool useMinDist = false;
    private float minDist = 0.11f;
    private float minDistSqr;

    private float _y = 0f;

    public RandomPoints(float maxX, float maxZ, float y) 
    {
        max_x = maxX;
        max_z = maxZ;
        _y = y;
    }

    public Vector3[] CreateRandomPoints(int num, bool verbose=false) 
    {
        // returns a Vector3 array of random values on a specified plane

        //return UseTestPoints();

        minDistSqr = minDist * minDist;

        Vector3[] arr = new Vector3[num];
        for (int i = 0; i < num; i++) 
        {
            float x = Random.Range(0f, max_x);
            float z = Random.Range(0f, max_z);
            Vector3 newPoint = new Vector3(x, _y, z);

            if (useMinDist) 
            {
                if (!isPointTooClose(arr, newPoint)) 
                {
                    arr[i] = newPoint;
                    if (verbose) Debug.Log("newPoint: " + newPoint);
                }
                else 
                {
                    Debug.Log("RandomPoints: Point too close, create another...");
                    i--;
                }
            }
            else 
            {
                arr[i] = newPoint;
                if (verbose) Debug.Log("newPoint: " + newPoint);
            }
        }

        string arrStr = "";
        foreach (Vector3 p in arr) arrStr = arrStr + p + ",";
        if (verbose) Debug.Log(arrStr);

        return arr;
    }

    // FIXME remove or change to Vector3
    // TODO figure out why there are duplicate triangles
    private Vector2[] UseTestPoints() 
    {
        // used for testing bad triangles, as these points produce and invalid triange after 14 points
        Vector2[] arr = new Vector2[] 
        {
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

    private bool isPointTooClose(Vector3[] points, Vector3 point) 
    { 
        // check if point is at least minDist away from all other points
        //  if not, create a new point
        foreach (Vector3 p in points) 
        {
            float distSqr = (p - point).sqrMagnitude;
            if (distSqr < minDistSqr)
                return true;
        }
        return false;
    }
}
