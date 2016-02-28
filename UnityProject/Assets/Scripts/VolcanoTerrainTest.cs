using UnityEngine;
using System.Collections;

public class VolcanoTerrainTest : MonoBehaviour {

    Terrain terrain;
    TerrainData terrainData;

    public float radius = 250.0f;
    public float vheight = 0.5f;

	// Use this for initialization
	void Start () {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        Generate();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Generate()
    {
        Vector2 up = new Vector2(0.0f, 1.0f);

        float[,] heightData = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        float invWidth = 1.0f / terrainData.heightmapWidth;
        float invHeight = 1.0f / terrainData.heightmapHeight;
        Vector2 center = new Vector2(terrainData.heightmapWidth * 0.5f, terrainData.heightmapWidth * 0.5f);

        for (int y = 0; y < terrainData.heightmapHeight; ++y)
        {
            for (int x = 0; x < terrainData.heightmapWidth; ++x)
            {
                
                float dx = x - center.x;
                float dy = y - center.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                //heightData[x, y] = (1.0f - dist / radius) * vheight;

                float angle = Mathf.Atan2(dy, dx) * 8.0f;
                heightData[x, y] = (1.0f - dist / radius) * vheight + ((Mathf.Sin(angle) + 1.0f) * 0.5f) * 0.05f * (dist / radius);
            }
        }

        terrainData.SetHeights(0, 0, heightData);
    }
}
