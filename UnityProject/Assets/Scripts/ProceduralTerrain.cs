using UnityEngine;
using System.Collections;

public class ProceduralTerrain : MonoBehaviour {

    Terrain terrain;
    TerrainData terrainData;

    public float scale = 1.0f;
    public float power = 1.0f;
    public float power2 = 1.0f;
    public float h1offset = 0.0f;
    public float h2offset = 0.0f;
    public float scale2 = 1.0f;
    public bool update = false;

    float prevScale;
    float prevPower;
    float prevPower2;
    float prevh1off;
    float prevh2off;
    float prevScale2;

	// Use this for initialization
	void Start () {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
	}

    /*
     function OctavedSimplexNoise(x, y, octaves, roughness, scale, inputFunction)
	{
		var frequency = scale;
		var weight = 1;
		var total = 0;
		var totalWeight = 0;

		for(var i = 0; i < octaves; ++i)
		{
			var n = SimplexNoise(x / frequency, y / frequency); 
			
			total += (typeof inputFunction === "function" ? inputFunction(n) : n) * weight; //SimplexNoise(x / frequency, y / frequency) * weight;

			totalWeight += weight;
			frequency *= 2;
			weight *= roughness;
		}

		var result = total / totalWeight;
		return result;
	};
     */

    float Octave(float x, float y, int o, float r, float s)
    {
        float f = s;
        float w = 1.0f;
        float t = 0.0f;
        float tw = 0.0f;

        for (float i = 0.0f; i < o; ++i)
        {
            float n = Mathf.PerlinNoise(x / f, y / f);
            t  += n * w;
            tw += w;
            f  *= 2.0f;
            w  *= r;
        }

        return t / tw;
    }

    float step(float edge, float n)
    {
        return n > edge ? 1.0f : 0.0f;
    }

    float threshold(float edge, float n)
    {
        return n > edge ? n : 0.0f; 
    }

    float smoothstep(float e1, float e2, float n)
    {
        float t = Mathf.Clamp((n - e1) / (e2 - e1), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }

    void Generate()
    {
        float[,] heightData = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        float invWidth = 1.0f / terrainData.heightmapWidth;
        float invHeight = 1.0f / terrainData.heightmapHeight;

        for (int y = 0; y < terrainData.heightmapHeight; ++y)
        {
            for (int x = 0; x < terrainData.heightmapWidth; ++x)
            {
                float h = Mathf.Sin((float)x * invWidth * scale) * Mathf.Cos((float)y * invHeight * scale2);
                float n = Mathf.PerlinNoise((float)x * invWidth * scale, (float)y * invHeight * scale);
                h = (h + 1.0f) * 0.5f * n;
                //float th = threshold(power, h);
                //float t = h / power;
                //float bh = Mathf.Lerp(h1offset, power, t);
                float d = Mathf.Clamp(h - h1offset, 0.0f, 1.0f);
                float t = Mathf.Exp(-50.0f * Mathf.Pow(d, 1.5f));
                heightData[x, y] = Mathf.Lerp(h, h1offset, t); //Mathf.Lerp(bh, h, smoothstep(0.0f, power2, h - power));//h;
                /*
                //Octave((float)x * invWidth * scale, (float)y * invHeight * scale, (int)power, power2, h1offset);
                float h1 = Mathf.PerlinNoise((float)x * invWidth * scale, (float)y * invHeight * scale);
                float h2 = Mathf.PerlinNoise((float)x * invWidth * scale2, (float)y * invHeight * scale2);
                //h = Mathf.PerlinNoise(h, 1.0f - h);
                h1 = (h1 + 1.0f) * 0.5f;
                h2 = (h2 + 1.0f) * 0.5f;
                h1 = h1offset + Mathf.Pow(h1, power);
                h2 = h2offset + Mathf.Pow(h2, power2);
                 //exp(-1 * exp(-4 * pow(x, 2.0)))
                //float h3 = Mathf.Exp(-1.0f * Mathf.Exp(-4.0f * Mathf.Pow(h, power)));

                heightData[x, y] = h2 + h1;//Mathf.Lerp(h2, h1, Mathf.SmoothStep(0.0f, 0.01f, h1 - h2));//Mathf.Max(h3 - 0.4f, h1);
                 * */
            }
        }

        terrainData.SetHeights(0, 0, heightData);
        CalcSplatMap();
    }

    void CalcSplatMap()
    {
        float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        
        for (int i = 0; i < terrainData.alphamapHeight; ++i)
        {
            for (int j = 0; j < terrainData.alphamapWidth; ++j)
            {
                float x = (float)j / (float)terrainData.alphamapWidth;
                float y = (float)i / (float)terrainData.alphamapHeight;

                float height = terrainData.GetHeight(Mathf.RoundToInt(y * terrainData.heightmapHeight), Mathf.RoundToInt(x * terrainData.heightmapWidth));
                Vector3 normal = terrainData.GetInterpolatedNormal(y, x);
                float steepness = terrainData.GetSteepness(y, x);
                float[] weights = new float[terrainData.alphamapLayers];

                // ToDo: Calculate new weights depending on the 3 factors above
                //float isSand = step(50.0f, height);
                float sand = smoothstep(0.35f, 0.6f, Vector3.Dot(normal, Vector3.up));
                //sand *= (1.0f - step(50.0f, height));
                sand *= (1.0f - smoothstep(45.0f, 70.0f, height)) * Mathf.Exp(-3.0f * Mathf.Pow(steepness / terrainData.size.y, 3.0f));
                weights[0] = sand;
                weights[1] = 1.0f - sand;

                for (int k = 0; k < terrainData.alphamapLayers; ++k)
                {
                    splatmapData[j, i, k] = weights[k];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    // Update is called once per frame
    void Update()
    {
        if (!update)
            return;

        if (scale != prevScale || power != prevPower || power2 != prevPower2 || h1offset != prevh1off || h2offset != prevh2off || scale2 != prevScale2)
        {
            prevScale = scale;
            prevPower = power;
            prevPower2 = power2;
            prevh1off = h1offset;
            prevh2off = h2offset;
            prevScale2 = scale2;
            Generate();
        }
	}
}
