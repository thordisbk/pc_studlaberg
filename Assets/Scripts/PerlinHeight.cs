using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinHeight
{
    public float max_x;
    public float max_z;
    public float scale;
    private float multiplyer = 1f;
    private float orig_y;

    private float offsetX = 100f;
    private float offsetZ = 100f;

    public PerlinHeight(float maxX, float maxZ, float y, float s, float m, float osX, float osZ, List<GameObject> objects) {
        max_x = maxX;
        max_z = maxZ;
        orig_y = y;
        scale = s;
        multiplyer = m;
        offsetX = osX;
        offsetZ = osZ;

        foreach (GameObject obj in objects) {
            Vector3 pos = obj.transform.position;
            float xCoord = pos.x / max_x * scale + offsetX;
            float yCoord = orig_y;
            float zCoord = pos.z / max_z * scale + offsetZ;
            // Debug.Log("x = " + xCoord + ", z = " + zCoord);
            // Debug.Log("y = " + yCoord);

            // 0 black, 1 white
            // black pushes columns up
            float value = (1f - Mathf.PerlinNoise(xCoord, zCoord)) * multiplyer;
            // Debug.Log("Value: " + value);
            obj.transform.position = new Vector3(pos.x, value, pos.z);
        }
        
    }


}
