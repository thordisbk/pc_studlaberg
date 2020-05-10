using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMany : MonoBehaviour
{
    public GameObject meshGenerator;
    private MeshFilter meshFilter;

    public GameObject prefab;
    private float addX = 2f;
    private float addZ = 2f;
    private float addVal = 2f;

    public int newObjects = 100;  // 10000 ok
    private int inRow;// = 10;

    // Start is called before the first frame update
    void Start()
    {
        inRow = (int) Mathf.Sqrt((float) newObjects);
        Debug.Log("in row: " + inRow);

        meshFilter = meshGenerator.GetComponent<MeshFilter>();
        for (int i = 0; i < newObjects; i++) {
            createNewMesh();
        }
    }

    void createNewMesh() {
        Vector3 newPos = meshGenerator.transform.position;
        newPos.x = newPos.x + addX;
        newPos.z = newPos.z + addZ;
        addX += addVal;
        if (addX > inRow * addVal) {
            addZ += addVal;
            addX = addVal;
        }
        GameObject newMeshObj = Instantiate(prefab, newPos, meshGenerator.transform.rotation);

        Mesh mesh = meshFilter.mesh;
        newMeshObj.GetComponent<MeshFilter>().mesh = mesh;      
    }

}
