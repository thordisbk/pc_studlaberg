using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoronoiColorTexture : MonoBehaviour
{
    // dimensions
    public int width;
    public int height;
    
    public int cellCount;

    Sprite sprite;

    public bool reevaluateCentroids = false;  // if set to true, reevaluate the centroids once

    // private Vector2[][] centroidMembers;
    private Dictionary<int, List<Vector2>> centroidMembers;

    Color[] cells;

    private bool init = false;

    // Start is called before the first frame update
    void Start()
    {
        float time = Time.time;
        centroidMembers = new Dictionary<int, List<Vector2>>();

        cells = new Color[cellCount];
        // give cells random colors
        for (int i = 0; i < cellCount; i++) {
            cells[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }

        init = true;
    }

    IEnumerator Init() {
        Texture2D texture = CreateDiagram();
        Rect rect = new Rect(0, 0, width, height);
        Vector2 pivot = new Vector2(1, 1) * 0.5f;

        sprite = Sprite.Create(texture, rect, pivot);
        GetComponent<SpriteRenderer>().sprite = sprite;
        Debug.Log("[Start()] Timer: " + Time.realtimeSinceStartup + " s");

        yield return null;
    }

    private Texture2D CreateDiagram() {
        Vector2[] centroids = new Vector2[cellCount];
        
        // give centroids random positions
        for (int i = 0; i < cellCount; i++) {
            centroids[i] = new Vector2((int) Random.Range(0, width), (int) Random.Range(0, height));

            centroidMembers.Add(i, new List<Vector2>());
        }

        Texture2D result = CreateTexture(centroids);

        Debug.Log("[CreateDiagram()] Timer: " + Time.realtimeSinceStartup + " s");
        return result;
    }

    private Texture2D CreateTexture(Vector2[] centroids) {
        Color[] pixelColors = new Color[width * height];

        Debug.Log("[CreateTexture() 1] Timer: " + Time.realtimeSinceStartup + " s");
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int pixelIdx = x * width + y;
                Vector2 pixelPoint = new Vector2(x, y);
                int cellIdx = GetIdxOfClosestCentroid(pixelPoint, centroids);
                pixelColors[pixelIdx] = cells[cellIdx];
                // color the centroid black
                //if (centroids.Contains(pixelPoint)) pixelColors[pixelIdx] = Color.black;

                centroidMembers[cellIdx].Add(pixelPoint);
            }            
        }
        Debug.Log("[CreateTexture() 2] Timer: " + Time.realtimeSinceStartup + " s");

        Texture2D result = GetImageFromColorArray(pixelColors);
        return result;
    }

    int GetIdxOfClosestCentroid(Vector2 pixelPoint, Vector2[] centroids) {
        float distance = float.MaxValue;
        int idx = 0;
        
        for (int i = 0; i < centroids.Length; i++) {
            float currDistance = (pixelPoint - centroids[i]).sqrMagnitude;  
            //float currDistance = Vector2.Distance(pixelPoint, centroids[i]);
            if (currDistance < distance) {
                distance = currDistance;
                idx = i;
            }
        }
        
        return idx;
    }

    Texture2D GetImageFromColorArray(Color[] pixelColors) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(pixelColors);
        texture.Apply();
        return texture;
    }

    void Update() {
        if (init) {
            init = false;
            StartCoroutine("Init");
        }
        if (reevaluateCentroids) {
            reevaluateCentroids = false;
            StartCoroutine("ApplyCentroidReevaluation");
        }
    }

    IEnumerator ApplyCentroidReevaluation() {
        Vector2[] centroids = new Vector2[cellCount];

        // create a copy of the dictionary and empty the original, because it will be filled again in CreateTexture()
        Dictionary<int, List<Vector2>> centroidMembersTemp = new Dictionary<int, List<Vector2>>(centroidMembers);
        centroidMembers.Clear();

        for (int i = 0; i < cellCount; i++) {
            int avgX = (int) centroidMembersTemp[i].Average(v => v.x);
            int avgY = (int) centroidMembersTemp[i].Average(v => v.y);

            centroids[i] = new Vector2(avgX, avgY);

            centroidMembers.Add(i, new List<Vector2>());
            yield return null;
        }

        Texture2D texture = CreateTexture(centroids);
        Rect rect = new Rect(0, 0, width, height);
        Vector2 pivot = new Vector2(1, 1) * 0.5f;

        sprite = Sprite.Create(texture, rect, pivot);
        GetComponent<SpriteRenderer>().sprite = sprite;

        yield return null;
    }
}
