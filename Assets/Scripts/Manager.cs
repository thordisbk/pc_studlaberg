using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public bool VERBOSE = false;
    public bool showTimer = true;
    private float Yplane = 0f;  // y-value of the columns' top surface, the height
    
    [Header("Random Points")]
    public int pointsNum = 20;
    public float max_x = 10;
    public float max_z = 10;
    public bool showPoints = true;
    private RandomPoints randomPoints;
    private Vector3[] points;
    
    [Header("Triangulation")]
    public bool validateAfterEveryPoint = true;
    public bool showTriangulation = true;
    public bool doTriangulation = false;
    private Triangulation triangulation;
    private bool firstTriangulationDone = false;

    [Header("Voronoi")]
    public bool showVoronoiCenterEdges = false;
    public bool showVoronoiWhole = true;
    public bool showVoronoiOnlyWithinBounds = true;
    public bool doVoronoi = false;
    private Voronoi voronoi;
    private bool firstVoronoiDone = false;

    [Header("Voronoi Relaxation")]
    public bool doRelaxation = false;
    public int relaxTimes = 1;
    private int relaxTimesCounter = 0;
    private int totalRelaxesDone = 0;

    [Header("Columns")]
    public GameObject columnMeshPrefab;
    public float columnLength = 10f;
    private bool meshesCreated = false;
    private List<GameObject> meshObjects;
    public bool createBottomFace = true;
    public bool doMesh = false;

    [Header("Perlin")]
    public bool doPerlin = false;
    //private bool firstPerlinDone = false;
    public float perlinScale = 5f;
    public float perlinMultiplyer = 1f;
    public float offsetX = 0f;
    public float offsetZ = 0f;

    [Header("Ripple")]
    public float rippleMoveLength = 1f;
    public float rippleLerpTime = 0.5f;
    public float percStartRipple = 0.5f;
    public bool doRandomRipple = false;
    public bool doRandomPathing = false;
    //private bool rippleResetNeeded = false;
    [Tooltip("Wait until ripple is done to reset, or not.")]
    public bool resetRipples = false;
    //private float waitBeforeNextRipple = 0f;
    //private float waitBeforeNextRippleCounter = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (columnMeshPrefab == null) {
            Debug.LogError("The variable columnMeshPrefab has not been assigned.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float rt = Time.realtimeSinceStartup;
        if (doTriangulation) {
            doTriangulation = false;
            if (!firstTriangulationDone) {
                Debug.Log("Get random points");
                // create random points
                randomPoints = new RandomPoints(max_x, max_z, Yplane);
                points = randomPoints.CreateRandomPoints(pointsNum, VERBOSE);

                Debug.Log("Compute triangulation");
                triangulation = null;
                triangulation = new Triangulation(max_x, max_z, Yplane, points, VERBOSE, validateAfterEveryPoint);
                triangulation.ComputeTriangulation();   
                firstTriangulationDone = true;

                if (showTimer) Debug.Log("(Triangulation) Timer: " + (Time.realtimeSinceStartup - rt) + " s");
            }
            else {
                Debug.LogError("Triangulation has already been computed.");
            }
        }
        if (doVoronoi) {
            doVoronoi = false;
            if (!firstVoronoiDone && firstTriangulationDone) {
                Debug.Log("Convert triangulation to Voronoi");
                ConvertTriangulationToVoronoi();
                firstVoronoiDone = true;

                int numOfValidCells = 0;
                foreach (VoronoiCell cell in voronoi.voronoiCells) { if (cell.isValid) numOfValidCells++; } 
                Debug.Log("Number of cells = " + numOfValidCells);
            }
            else if (!firstTriangulationDone) {
                Debug.LogError("Triangulation must be computed before the Voronoi.");
            }
            else {
                Debug.LogError("First Voronoi has already been computed.");
            }
        }

        if (doRelaxation) {
            doRelaxation = false;
            if (firstVoronoiDone) {
                if (relaxTimes < 0) relaxTimes = 0;
                Debug.Log("The Voronoi will be relaxed " + relaxTimes + " time" + (relaxTimes != 1 ? "s." : "."));
                while (relaxTimesCounter < relaxTimes) {
                    totalRelaxesDone++;
                    Debug.Log("Apply Relaxation #" + totalRelaxesDone);
                    RelaxVoronoi();
                    ConvertTriangulationToVoronoi();
                    relaxTimesCounter++;

                    if (showTimer) Debug.Log("(Relaxation) Timer: " + (Time.realtimeSinceStartup - rt) + " s");
                }
                relaxTimesCounter = 0;
            }
            else {
                Debug.LogError("The first Voronoi must be computed before the diagram is relaxed.");
            }
        }

        if (doMesh) {
            doMesh = false;
            if (!meshesCreated && firstVoronoiDone) {
                Debug.Log("Generate meshes");
                CreateMeshColumns();
                FindCellNeighbors();
                meshesCreated = true;
            }
            else if (meshesCreated) {
                Debug.LogError("Meshes have already been generated.");
            }
            else {
                Debug.LogError("The first Voronoi must be computed before the meshes are created.");
            }
        }

        if (doPerlin) {
            doPerlin = false;
            if (meshesCreated) {
                Debug.Log("Apply Perlin Noise to column heights");
                ApplyPerlinToVoronoi();
            }
            else {
                Debug.LogError("Meshes must be generated before noise is applied.");
            }
        }

        if (doRandomRipple) {
            doRandomRipple = false;
            doRandomPathing = false;
            if (meshesCreated) {
                Debug.Log("Do random ripple (may need to reset first)");
                // rippleResetNeeded = true;
                ChooseRandomObjectToStartRipple();
            }
            else {
                Debug.LogError("Meshes must be generated before ripple is applied.");
            }
        }

        if (doRandomPathing) {
            doRandomPathing = false;
            if (meshesCreated) {
                Debug.Log("Do random pathing (may need to reset first)");
                // rippleResetNeeded = true;
                ChooseRandomObjectToStartPathing();
            }
            else {
                Debug.LogError("Meshes must be generated before pathing is applied.");
            }
        }

        if (resetRipples) {
            resetRipples = false;
            if (meshesCreated) {
                Debug.Log("Reset ripple");
                // reset all RippleColumn
                foreach (GameObject obj in meshObjects) {
                    obj.GetComponent<RippleColumn>().RESET = true;
                }
                // rippleResetNeeded = false;
            }
        } 
        //showTimer = false;  // so timer only gets shown the first time Update() runs
    }

    private void ConvertTriangulationToVoronoi() {
        // check if there are invalid triangles
        int invalids = triangulation.CountInvalidTriangles(points, true);  // findFirst=true
        if (invalids > 0) {
            Debug.LogError("Found invalid triangles; triangulation is faulty.");
        }

        voronoi = new Voronoi(max_x, max_z);
        if (invalids == 0) {
            voronoi.ComputeVoronoi(triangulation.triangles, points);
        }
    }

    private void RelaxVoronoi() {
        // instead of random points, use the average of voronoi cell boundary points
        
        // the points from the big base triangle get added here, but removed in ComputeTriangulation()
        List<Vector3> pointsNew = new List<Vector3>();
        foreach (VoronoiCell voronoiCell in voronoi.voronoiCells) {
            // if average center is within initial boundary, use that
            // else use the previous random center
            if (IsPointWithinBoundary(voronoiCell.averageCenter)) {
                pointsNew.Add(voronoiCell.averageCenter);
            }
            else {
                pointsNew.Add(voronoiCell.center);
            }
        }

        points = null;
        points = pointsNew.ToArray();

        // clear the current voronoi
        voronoi = null;
        // to the triangulation again
        triangulation = null;
        triangulation = new Triangulation(max_x, max_z, Yplane, points, VERBOSE, validateAfterEveryPoint);
        triangulation.ComputeTriangulation();
    }

    private void CreateMeshColumns() {
        meshObjects = new List<GameObject>();
        foreach (VoronoiCell cell in voronoi.voronoiCells) {
            if (cell.isValid) {
                int corners = cell.boundaryPoints.Count;
                if (VERBOSE) Debug.Log("Corners: " + corners);
                if (corners >= 3) {
                    // set object at cell.center, in cmg.Init() all boundary points get converted with that in mind
                    //GameObject obj = Instantiate(columnMeshPrefab, Vector3.zero, Quaternion.identity);
                    GameObject obj = Instantiate(columnMeshPrefab, cell.center, Quaternion.identity);
                    ColumnMeshGenerator cmg = obj.AddComponent<ColumnMeshGenerator>() as ColumnMeshGenerator;
                    cmg.Init(cell, columnLength, createBottomFace);
                    meshObjects.Add(obj);
                    cell.theMeshObject = obj;
                }
            }
        }
        if (VERBOSE) Debug.Log("Number of meshObjects: " + meshObjects.Count);
    }

    private void FindCellNeighbors() {
        for (int i = 0; i < voronoi.voronoiCells.Count; i++) {
            if (voronoi.voronoiCells[i].isValid) {
                // only do this to valid cells that create meshes
                List<GameObject> neighbors = new List<GameObject>();
                for (int j = 0; j < voronoi.voronoiCells.Count; j++) {
                    if (i != j && voronoi.voronoiCells[j].isValid 
                        && voronoi.voronoiCells[i].ShareEdge(voronoi.voronoiCells[j])) {
                        // this cell is a neighbor to the cell above if both are valid and they share an edge
                        neighbors.Add(voronoi.voronoiCells[j].theMeshObject);
                    }
                }
                if (VERBOSE) Debug.Log("Neighbors: " + neighbors.Count);
                RippleColumn ripple = voronoi.voronoiCells[i].theMeshObject.GetComponent<RippleColumn>();
                ripple.SetValues(neighbors, rippleMoveLength, rippleLerpTime, percStartRipple);
            }
        }
    }

    private void ApplyPerlinToVoronoi() {
        PerlinHeight perlinHeight = new PerlinHeight(max_x, max_z, Yplane, perlinScale, perlinMultiplyer, 
                                                     offsetX, offsetZ, meshObjects);
    }

    private void ChooseRandomObjectToStartRipple() {
        // first update moveLength and lerpTime in case it has been changed
        foreach (GameObject obj in meshObjects) {
            obj.GetComponent<RippleColumn>().UpdateValues(rippleMoveLength, rippleLerpTime, percStartRipple);
        }
        // then choose a random index and start the ripple at the column with that index
        int randIdx = Random.Range(0, meshObjects.Count);
        meshObjects[randIdx].GetComponent<RippleColumn>().doRipple = true;
    }

    private void ChooseRandomObjectToStartPathing() {
        // first update moveLength and lerpTime in case it has been changed
        foreach (GameObject obj in meshObjects) {
            obj.GetComponent<RippleColumn>().UpdateValues(rippleMoveLength, rippleLerpTime, percStartRipple);
        }
         // then choose a random index and start the ripple at the column with that index
        int randIdx = Random.Range(0, meshObjects.Count);
        meshObjects[randIdx].GetComponent<RippleColumn>().doPathing = true;
    }

    private bool IsPointWithinBoundary(Vector3 point) {
        bool xOK = (0f <= point.x && point.x <= max_x); 
        bool zOK = (0f <= point.z && point.z <= max_z); 
        return (xOK && zOK);
    }

    void OnDrawGizmos() {

        if (triangulation != null && showTriangulation) {
            foreach (Triangle t in triangulation.triangles) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(t.pointA, t.pointB);
                Gizmos.DrawLine(t.pointB, t.pointC);
                Gizmos.DrawLine(t.pointC, t.pointA);
            }
        }

        if (points != null && showPoints) {
            foreach (Vector3 p in points) {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(p, 0.05f);
            }
        }

        if (voronoi != null && voronoi.voronoiCells != null && showVoronoiWhole) {
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                foreach (Edge e in cell.boundaryEdges) {
                    //e.DrawEdgeColored(Color.red);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(e.pointA, e.pointB);
                }
            }
        }

        if (voronoi != null && voronoi.voronoiCells != null && showVoronoiOnlyWithinBounds) {
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                if (cell.isValid) {
                    foreach (Edge e in cell.boundaryEdges) {
                        //e.DrawEdgeColored(Color.red);
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(e.pointA, e.pointB);
                    }
                }
                
            }
        }

        if (voronoi != null && voronoi.voronoiCells != null && showVoronoiCenterEdges) {
            // draw lines from center to all points
            foreach (VoronoiCell cell in voronoi.voronoiCells) {
                foreach (Vector3 p in cell.boundaryPoints) {
                    //Edge newEdge = new Edge(cell.center, p);
                    //newEdge.DrawEdgeColored(Color.green);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(cell.center, p);
                }
            }
        }
    }
}
