using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = Vector3.one;

    public bool resetTerrain = true;

    #region Perlin Noise
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;
    #endregion

    #region Multiple Perlin

    [System.Serializable]
    public class PerlinParameters 
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new()
    {
        new PerlinParameters()
    };

    #endregion

    #region Voronoi

    public int voronoiPeakCount = 1;
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMinHeight = 0f;
    public float voronoiMaxHeight = 1f;
    public enum VoronoiType { Linear, Power, Combine, SinePow, Perlin }
    public VoronoiType voronoiType = VoronoiType.Linear;

    #endregion

    public Terrain terrain;
    public TerrainData terrainData;

    private int hmr => terrainData.heightmapResolution;

    public void Perlin() 
    {
        float[,] heightMap = GetHeights();

        for(int y = 0; y < hmr; y++) 
        {
            for (int x = 0; x < hmr; x++)
            {
                heightMap[x, y] += Utils.fBM((x+perlinOffsetX) * perlinXScale,
                                            (y+perlinOffsetY) * perlinYScale,
                                            perlinOctaves,
                                            perlinPersistance) * perlinHeightScale;
                    
            }
        }

        SetHeights(heightMap);
    }

    public void MultiplePerlinTerrain() 
    {
        float[,] heightMap = GetHeights();
        for (int y = 0; y < hmr; y++)
        {
            for (int x = 0; x < hmr; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale,
                                                (y + p.mPerlinOffsetY) * p.mPerlinYScale,
                                                p.mPerlinOctaves,
                                                p.mPerlinPersistance) * p.mPerlinHeightScale;
                }

            }
        }

        SetHeights(heightMap);
    }

    public void ApplyRigdeNoise() 
    {
        ResetTerrain();
        MultiplePerlinTerrain();
        float[,] heightMap = GetHeights(false);

        for (int y = 0; y < hmr; y++)
        {
            for (int x = 0; x < hmr; x++)
            {
                float old = heightMap[x, y];
                heightMap[x, y] = 1 - Mathf.Abs(old - 0.5f);
            }
        }

        SetHeights(heightMap);
    }

    public void AddNewPerlin() 
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin() 
    {
        perlinParameters.RemoveAll(x => x.remove);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeights();

        for(int x = 0; x < hmr; ++x) 
        {
            for(int z = 0; z < hmr; ++z) 
            {
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }

        SetHeights(heightMap);
    }

    public void LoadTexture() 
    {
        float[,] heightMap = GetHeights();

        for (int x = 0; x < hmr; ++x)
        {
            for (int z = 0; z < hmr; ++z)
            {
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }

        SetHeights(heightMap);
    }

    public void Voronoi() 
    {
        float[,] heightMap = GetHeights();

        for (int i = 0; i < voronoiPeakCount; i++)
        {
            var height = UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight);
            var xIndex = UnityEngine.Random.Range(0, hmr);
            var yIndex = UnityEngine.Random.Range(0, hmr);

            if (heightMap[xIndex, yIndex] >= height) continue;

            heightMap[xIndex, yIndex] = height;

            Vector2 peakLocation = new Vector2(xIndex, yIndex);
            float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(hmr, hmr));

            for (int y = 0; y < hmr; y++)
            {
                for (int x = 0; x < hmr; x++)
                {
                    if (x == peakLocation.x && y == peakLocation.y) continue;

                    float distance = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                    float h = GetVoronoiHeight(height, distance, x, y);

                    if (heightMap[x, y] < h)
                        heightMap[x, y] = h;
                }
            }
        }

        SetHeights(heightMap);
    }

    private float GetVoronoiHeight(float height, float distance, float x, float y)
    {
        float h = 0;
        switch (voronoiType)
        {
            case VoronoiType.Linear:
                h = height - distance * voronoiFallOff;
                break;
            case VoronoiType.Power:
                h = height - Mathf.Pow(distance, voronoiDropOff) * voronoiFallOff;
                break;
            case VoronoiType.Combine:
                h = height - distance * voronoiFallOff - Mathf.Pow(distance, voronoiDropOff);
                break;
            case VoronoiType.SinePow:
                h = height - Mathf.Pow(distance * 3, voronoiFallOff) - Mathf.Sin(distance * 2 * Mathf.PI) / voronoiDropOff;
                break;
            case VoronoiType.Perlin:
                h = (height - distance * voronoiFallOff) 
                    + Utils.fBM((x + perlinOffsetX) * perlinXScale,
                                (y + perlinOffsetY) * perlinYScale,
                                perlinOctaves,
                                perlinPersistance) * perlinHeightScale;
                break;
        }

        return h;
    }

    public void MidPointDisplacement() 
    {
        float[,] heightMap = GetHeights();
        int width = hmr - 1;
        int squareSize = width;
        float height = squareSize * 0.005f;
        float roughness = 2.0f;
        float heightDampener = (float)Mathf.Pow(2, -1 * roughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /*heightMap[0, 0] = UnityEngine.Random.Range(0, 0.2f);
        heightMap[0, hmr - 2] = UnityEngine.Random.Range(0, 0.2f);
        heightMap[hmr - 2, 0] = UnityEngine.Random.Range(0, 0.2f);
        heightMap[hmr - 2, hmr - 2] = UnityEngine.Random.Range(0, 0.2f);*/

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = (int)(x + squareSize * 0.5f);
                    midY = (int)(y + squareSize * 0.5f);

                    heightMap[midX, midY] = (float)((heightMap[x, y]
                                                    + heightMap[cornerX, y]
                                                    + heightMap[x, cornerY]
                                                    + heightMap[cornerX, cornerY]) * 0.25f
                                                    + UnityEngine.Random.Range(-height, height));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = (int)(x + squareSize * 0.5f);
                    midY = (int)(y + squareSize * 0.5f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    //bottom side
                    heightMap[midX, y] = (float)((heightMap[midX, midY] 
                                                    + heightMap[x, y] 
                                                    + heightMap[midX, pmidYD]
                                                    + heightMap[cornerX, y]) * 0.25f
                                                    + UnityEngine.Random.Range(-height, height));

                    //top side
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY]
                                                    + heightMap[midX, midY]
                                                    + heightMap[cornerX, cornerY]
                                                    + heightMap[midX, pmidYU]) * 0.25f
                                                    + UnityEngine.Random.Range(-height, height));

                    //left side
                    heightMap[x, midY] = (float)((heightMap[x, y]
                                                    + heightMap[pmidXL, midY]
                                                    + heightMap[x, cornerY]
                                                    + heightMap[midX, midY]) * 0.25f
                                                    + UnityEngine.Random.Range(-height, height));

                    //right side
                    heightMap[cornerX, midY] = (float)((heightMap[cornerX, y]
                                                    + heightMap[midX, midY]
                                                    + heightMap[cornerX, cornerY]
                                                    + heightMap[pmidXR, midY]) * 0.25f
                                                    + UnityEngine.Random.Range(-height, height));
                }
            }
                squareSize = (int)(squareSize * 0.5f);
                height *= heightDampener;
        }

        SetHeights(heightMap);
    }

    public void ResetTerrain()
    {
        float[,] heightMap = new float[hmr, hmr];
        SetHeights(heightMap);
    }

    private void OnEnable()
    {
        Debug.Log("Initialising Data");
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        tagManager.ApplyModifiedProperties();
        this.gameObject.tag = "Terrain";
    }

    private void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        for(int i = 0; i < tagsProp.arraySize; i++) 
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) 
            {
                found = true; 
                break;
            }
        }

        if (!found) 
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }

    private float[,] GetHeights(bool checkReset = true) 
    {
        return (!resetTerrain || !checkReset) ? terrainData.GetHeights(0, 0, hmr, hmr) : new float[hmr, hmr];
    }

    private void SetHeights(float[,] heightMap) 
    {
        terrainData.SetHeights(0, 0, heightMap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
