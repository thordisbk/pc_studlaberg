using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoints
{
    // Start is called before the first frame update
    void Start()
    {
        // seed the random generator
        //Random.seed = (int) System.DateTime.Now.Ticks;
    }

    public Vector2[] CreateRandomPoints(int num, float maxX, float maxY, bool verbose=false) {
        // returns a Vector2 array of random values on a specified plane

        Vector2[] arr = new Vector2[num];
        for (int i = 0; i < num; i++) {
            float x = Random.Range(0f, maxX);
            float y = Random.Range(0f, maxY);
            Vector2 newPoint = new Vector2(x, y);
            arr[i] = newPoint;
            if (verbose) Debug.Log("newPoint: " + newPoint);
        }
        return arr;
    }
}
