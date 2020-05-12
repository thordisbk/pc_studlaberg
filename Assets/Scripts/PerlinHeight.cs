using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinHeight
{
    public float max_x;
    public float max_z;
    public float scale;

    public PerlinHeight(float maxX, float maxZ, float s, List<GameObject> objects) {
        max_x = maxX;
        max_z = maxZ;
        scale = s;

        foreach (GameObject obj in objects) {
            Vector3 pos = obj.transform.position;
            float xCoord = pos.x / max_x * scale;
            float yCoord = pos.y;
            float zCoord = pos.z / max_z * scale;
            // Debug.Log("x = " + xCoord + ", z = " + zCoord);
            // Debug.Log("y = " + yCoord);

            float value = Mathf.PerlinNoise(xCoord, zCoord);
            // Debug.Log("Value: " + value);
            obj.transform.position = new Vector3(pos.x, value, pos.z);
        }
        
    }


}
