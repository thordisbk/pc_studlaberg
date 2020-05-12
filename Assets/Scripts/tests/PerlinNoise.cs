
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    private GameObject quad;
    private int width = 256;
    private int height = 256;

    public int depth = 20;  // height of terrain
    public float scale = 20f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    Renderer r;

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Renderer>();
    }

    void Update() {
        r.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture() {
        Texture2D texture = new Texture2D(width, height);

        // generate a perlin noise map for the texture
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y) {
        float xCoord = (float) x / width * scale + offsetX;
        float yCoord = (float) y / height * scale + offsetY;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        Color color = new Color(sample, sample, sample);
        return color;
    }
}
