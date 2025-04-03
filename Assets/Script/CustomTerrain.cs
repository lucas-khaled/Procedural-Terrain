using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;
using System;
using UnityEngine.UIElements;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = Vector3.one;

    public bool resetTerrain = true;

    public enum TagType { Tag = 0, Layer = 1 }
    [SerializeField]
    private int terrainLayer = -1;

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

    #region MPD

    public float MPDHeightMin = -5;
    public float MPDHeightMax = 5;
    public float MPDHeightDampenerPower = 2;
    public float MPDRoughness = 2;

    #endregion

    #region Smooth

    public int smoothAmount = 1;

    #endregion

    #region Splatmaps
    [Serializable]
    public class SplatHeight 
    {
        public Texture2D texture = null;
        public Texture2D normalMap = null;
        public float normalMapScale = 1;

        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;

        public float splatOffset = 0.1f;
        public float splatNoiseXScale = 0.01f;
        public float splatNoiseYScale = 0.01f;
        public float splatNoiseZScale = 0.1f;

        public float minSlope = 0;
        public float maxSlope = 90;

        public Vector2 tileOffset = Vector2.zero;
        public Vector2 tileSize = new Vector2(50f, 50f);

        public bool remove = false;
    }

    public List<SplatHeight> splatHeights = new()
    {
        new SplatHeight()
    };
    #endregion

    #region Vegetation

    public int maximumTrees = 5000;
    public int treesSpacing = 5;

    public List<Tree> vegetation = new()
    {
        new Tree()
    };

    [Serializable]
    public class Tree
    {
        public GameObject mesh;

        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;

        public float minSlope = 0;
        public float maxSlope = 90;

        public float minScale = 0.5f;
        public float maxScale = 1;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightColor;
        public float minRotation = 0f;
        public float maxRotation = 360f;
        public float density = 0.5f;

        public bool remove = false;
    }

    #endregion

    #region Detail

    [Serializable]
    public class Detail 
    {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public Color healthyColor = Color.white;
        public Color dryColor = Color.white;
        [Range(0,1)] public float alignToGround = 0f;
        public float maxBillboardHeight = 2;
        public float minBillboardHeight = 1;
        public float maxBillboardWidth = 2;
        public float minBillboardWidth = 1;
        [Range(0, 1)] public float minHeight = 0.1f;
        [Range(0, 1)] public float maxHeight = 0.2f;
        [Range(0, 90)] public float minSlope = 0f;
        [Range(0, 90)] public float maxSlope = 90f;
        [Range(1, 16)] public int maxInstances = 16;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float spread = 1f;
        public float density = 0.5f;
        public float padding = 1f;
        public bool remove = false;
    }

    public List<Detail> details = new()
    {
        new Detail()
    };

    public int maxDetails = 5000;
    public int detailSpacing = 5;

    #endregion

    public Terrain terrain;
    public TerrainData terrainData;

    private int hmr => terrainData.heightmapResolution;

    public void ApplyDetails() 
    {
        DetailPrototype[] newDetailsPrototypes = new DetailPrototype[details.Count];

        for(int i = 0; i < details.Count; i++) 
        {
            newDetailsPrototypes[i] = new DetailPrototype();
            newDetailsPrototypes[i].prototype = details[i].prototype;
            newDetailsPrototypes[i].prototypeTexture = details[i].prototypeTexture;
            newDetailsPrototypes[i].healthyColor = details[i].healthyColor;
            newDetailsPrototypes[i].dryColor = details[i].dryColor;
            newDetailsPrototypes[i].minHeight = details[i].minBillboardHeight;
            newDetailsPrototypes[i].maxHeight = details[i].maxBillboardHeight;
            newDetailsPrototypes[i].minWidth = details[i].minBillboardWidth;
            newDetailsPrototypes[i].maxWidth = details[i].maxBillboardWidth;
            newDetailsPrototypes[i].noiseSpread = details[i].spread;
            newDetailsPrototypes[i].holeEdgePadding = details[i].padding;
            newDetailsPrototypes[i].alignToGround = details[i].alignToGround;

            if (newDetailsPrototypes[i].prototype) 
            {
                newDetailsPrototypes[i].usePrototypeMesh = true;
                newDetailsPrototypes[i].renderMode = DetailRenderMode.VertexLit;
            }
            else 
            {
                newDetailsPrototypes[i].usePrototypeMesh = false;
                newDetailsPrototypes[i].renderMode = DetailRenderMode.GrassBillboard;
            }
        }

        terrainData.detailPrototypes = newDetailsPrototypes;

        float minDetailMapValue = 0;
        float[,] heightMap = terrainData.GetHeights(0, 0, hmr, hmr);

        for(int j = 0; j < terrainData.detailPrototypes.Length; j++) 
        {
            float maxDetailMapValue = (terrainData.detailScatterMode == DetailScatterMode.CoverageMode)
                                    ? details[j].maxInstances / 16 * 255 : details[j].maxInstances;

            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];

            for(int x = 0; x < terrainData.detailHeight; x+= detailSpacing) 
            {
                for(int y = 0; y < terrainData.detailWidth; y += detailSpacing) 
                {
                    if (UnityEngine.Random.Range(0f, 1f) > details[j].density) continue;

                    int xHM = (int)(x/(float)terrainData.detailWidth * (hmr-1));
                    int yHM = (int)(y/(float)terrainData.detailHeight * (hmr-1));

                    float thisNoise = Utils.Map(Mathf.PerlinNoise(y * details[j].feather,
                                                                  x * details[j].feather), 0, 1, 0.5f, 1);
                    float thisHeightStart = details[j].minHeight * thisNoise - details[j].overlap * thisNoise;
                    float thisHeightEnd = details[j].maxHeight * thisNoise + details[j].overlap * thisNoise;

                    var height = heightMap[xHM, yHM];

                    var normX = xHM / (float)hmr;
                    var normY = yHM / (float)hmr;
                    var steepness = terrainData.GetSteepness(normX, normY);

                    if(xHM == 442 && yHM == 444) 
                    {
                        Debug.Log($"Detail: x: {x} - y: {y} -> xHM: {xHM} - yHM: {yHM} -> norm X: {normX} - norm Y: {normY} -> steepness: {steepness}");
                    }

                    if (height > thisHeightEnd || height < thisHeightStart
                        || steepness > details[j].maxSlope || steepness < details[j].minSlope)
                        continue;
                    
                    detailMap[y, x] = (int)UnityEngine.Random.Range(minDetailMapValue, maxDetailMapValue);
                }
            }

            terrainData.SetDetailLayer(0, 0, j, detailMap);

            Debug.Log("Added Details");
        }
    }

    public void AddNewDetail() 
    {
        details.Add(new Detail());
    }

    public void RemoveDetails() 
    {
        List<Detail> keep = new();

        for(int i = 0; i < details.Count; i++) 
        {
            if (details[i].remove is false)
                keep.Add(details[i]);
        }

        if(keep.Count == 0) 
        {
            keep.Add(new Detail());
        }

        details = keep;
    }

    public void SplatMaps() 
    {
        int tah = terrainData.alphamapHeight;
        int taw = terrainData.alphamapWidth;
        int aml = terrainData.alphamapLayers;

        TerrainLayer[] newSplatPrototypes = new TerrainLayer[splatHeights.Count];

        for(int i = 0; i < splatHeights.Count; i++) 
        {
            newSplatPrototypes[i] = new TerrainLayer();
            newSplatPrototypes[i].diffuseTexture = splatHeights[i].texture;
            newSplatPrototypes[i].diffuseTexture.Apply(true);
            newSplatPrototypes[i].tileOffset = splatHeights[i].tileOffset;
            newSplatPrototypes[i].tileSize = splatHeights[i].tileSize;
            newSplatPrototypes[i].normalMapTexture = splatHeights[i].normalMap;
            newSplatPrototypes[i].normalScale = splatHeights[i].normalMapScale;

            string path = "Assets/New TerrainLayer " + i + ".terrainLayer";
            AssetDatabase.CreateAsset(newSplatPrototypes[i], path);

            Selection.activeObject = this.gameObject;
        }

        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heights = terrainData.GetHeights(0, 0, hmr, hmr);
        float[,,] splatMapData = new float[taw, tah, aml];

        for(int x = 0; x < tah; x++) 
        {
            for(int y = 0; y < taw; y++) 
            {
                int hmY = y * ((hmr - 1) / taw);
                int hmX = x * ((hmr - 1) / tah);

                var height = heights[hmX, hmY];

                float[] splat = new float[aml];
                bool emptySplat = true;

                float normY = y / (float)(taw - 1);
                float normX = x / (float)(tah - 1);

                var steepness = terrainData.GetSteepness(normY, normX);

                for (int i = 0; i < splatHeights.Count; i++) 
                {
                    float noise = Mathf.PerlinNoise(y * splatHeights[i].splatNoiseXScale, 
                                                    x * splatHeights[i].splatNoiseYScale) *
                                                    splatHeights[i].splatNoiseZScale;

                    float offset = splatHeights[i].splatOffset + noise;

                    float thisHeightsStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;

                    if (height >= thisHeightsStart && height <= thisHeightStop 
                        && steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope) 
                    {
                        if (height <= splatHeights[i].minHeight)
                            splat[i] = 1f - Mathf.Abs(height - splatHeights[i].minHeight) / offset;
                        else if (height >= splatHeights[i].maxHeight)
                            splat[i] = 1f - Mathf.Abs(height - splatHeights[i].maxHeight) / offset;
                        else
                            splat[i] = 1;

                        emptySplat = false;
                    }
                }

                NormalizeVector(ref splat);

                if (emptySplat) 
                {
                    splatMapData[x, y, 0] = 1;
                    continue;
                }

                for(int j = 0; j < splatHeights.Count; j++) 
                {
                    splatMapData[x, y, j] = splat[j];
                }
                    
            }
        }

        terrainData.SetAlphamaps(0, 0, splatMapData);
        Debug.Log("Splatmap applied");
    }

    private void NormalizeVector(ref float[] splat)
    {
        float total = 0;
        for(int i = 0; i < splat.Length; i++) 
        {
            total += splat[i];
        }

        if (total == 0) return;

        for(int i = 0; i < splat.Length; i++) 
        {
            splat[i] /= total;
        }

    }

    public void AddNewSplatHeight() 
    {
        splatHeights.Add(new SplatHeight());
    }

    public void RemoveSplatHeights() 
    {
        List<SplatHeight> keep = new();

        for(int i = 0; i < splatHeights.Count; i++) 
        {
            if (splatHeights[i].remove) continue;

            keep.Add(splatHeights[i]);
        }

        if(keep.Count == 0) 
        {
            keep.Add(new SplatHeight());
        }

        splatHeights = keep;
    }

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
        float maxHeight = MPDHeightMax;
        float minHeight = MPDHeightMin;
        float heightDampener = (float)Mathf.Pow(MPDHeightDampenerPower, -1 * MPDRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

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
                                                    + UnityEngine.Random.Range(minHeight, maxHeight));
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
                                                    + UnityEngine.Random.Range(minHeight, maxHeight));

                    //top side
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY]
                                                    + heightMap[midX, midY]
                                                    + heightMap[cornerX, cornerY]
                                                    + heightMap[midX, pmidYU]) * 0.25f
                                                    + UnityEngine.Random.Range(minHeight, maxHeight));

                    //left side
                    heightMap[x, midY] = (float)((heightMap[x, y]
                                                    + heightMap[pmidXL, midY]
                                                    + heightMap[x, cornerY]
                                                    + heightMap[midX, midY]) * 0.25f
                                                    + UnityEngine.Random.Range(minHeight, maxHeight));

                    //right side
                    heightMap[cornerX, midY] = (float)((heightMap[cornerX, y]
                                                    + heightMap[midX, midY]
                                                    + heightMap[cornerX, cornerY]
                                                    + heightMap[pmidXR, midY]) * 0.25f
                                                    + UnityEngine.Random.Range(minHeight, maxHeight));
                }
            }

            squareSize = (int)(squareSize * 0.5f);
            maxHeight *= heightDampener;
            minHeight *= heightDampener;
        }

        SetHeights(heightMap);
    }

    public void Smooth() 
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, hmr, hmr);
        float smoothProgress = 0;

        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < smoothAmount; i++)
        {
            for (int y = 0; y < hmr; y++)
            {
                for (int x = 0; x < hmr; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y), hmr, hmr);

                    foreach (var n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }

            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress/smoothAmount);
        }

        SetHeights(heightMap);
        EditorUtility.ClearProgressBar();
    }

    private List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height) 
    {
        List<Vector2> neighbours = new List<Vector2>();

        for (int y = -1; y < 2; ++y)
        {
            for (int x = -1; x < 2; ++x)
            {
                if (!(x == 0 && y == 0))
                {

                    Vector2 nPos = new Vector2(
                        Mathf.Clamp(pos.x + x, 0.0f, width - 1),
                        Mathf.Clamp(pos.y + y, 0.0f, height - 1));

                    if (!neighbours.Contains(nPos))
                        neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }

    public void PlantVegetation() 
    {
        TreePrototype[] newTreeProptotypes = new TreePrototype[vegetation.Count];
        for(int i = 0; i < vegetation.Count; i++) 
        {
            newTreeProptotypes[i] = new TreePrototype();
            newTreeProptotypes[i].prefab = vegetation[i].mesh;
        }

        terrainData.treePrototypes = newTreeProptotypes;

        List<TreeInstance> allVegetation = new();

        int tah = terrainData.alphamapHeight;
        int taw = terrainData.alphamapWidth;
        var heights = terrainData.GetHeights(0, 0, hmr, hmr);

        for(int z = 0; z < tah; z += treesSpacing) 
        {
            for (int x = 0; x < taw; x += treesSpacing)
            {
                var height = heights[z,x];

                float normX = x * 1f / (taw - 1);
                float normZ = z * 1f / (tah - 1);

                var steepness = terrainData.GetSteepness(normX, normZ);

                for (int i = 0; i < terrainData.treePrototypes.Length; ++i)
                {
                    if (UnityEngine.Random.Range(0f, 1f) > vegetation[i].density) break;

                    if (height > vegetation[i].maxHeight || height < vegetation[i].minHeight
                        || steepness > vegetation[i].maxSlope || steepness < vegetation[i].minSlope)
                        continue;

                    float normHeight = terrainData.GetHeight(x, z) / terrainData.size.y;

                    TreeInstance instance = new TreeInstance();
                    instance.position = new Vector3((x + UnityEngine.Random.Range(-5f, 5f))/ taw, 
                                                     normHeight, 
                                                    (z + UnityEngine.Random.Range(-5f, 5f)) / tah);

                    Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,
                                                        instance.position.y * terrainData.size.y,
                                                        instance.position.z * terrainData.size.z) + this.transform.position;

                    RaycastHit hit;
                    int layerMask = 1 << terrainLayer;

                    if(Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), Vector3.down, out hit, 100, layerMask) 
                        || Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask)) 
                    {
                        float treeHeight = (hit.point.y - transform.position.y) / terrainData.size.y;
                        instance.position = new Vector3(instance.position.x, treeHeight, instance.position.z);
                    }

                    instance.rotation = UnityEngine.Random.Range(vegetation[i].minRotation, vegetation[i].maxRotation);
                    instance.prototypeIndex = i;
                    instance.color = Color.Lerp(vegetation[i].colour1, vegetation[i].colour2, UnityEngine.Random.Range(0f, 1f));
                    instance.lightmapColor = vegetation[i].lightColor;

                    float scale = UnityEngine.Random.Range(vegetation[i].minScale, vegetation[i].maxScale);
                    instance.heightScale = scale;
                    instance.widthScale = scale;


                    allVegetation.Add(instance);

                    if (allVegetation.Count >= maximumTrees) goto TREESDONE;
                }
            }
        }

    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();
    }

    public void AddNewTree()
    {
        vegetation.Add(new Tree());
    }

    public void RemoveTrees()
    {
        List<Tree> keep = new();

        for (int i = 0; i < vegetation.Count; i++)
        {
            if (vegetation[i].remove) continue;

            keep.Add(vegetation[i]);
        }

        if (keep.Count == 0)
        {
            keep.Add(new Tree());
        }

        vegetation = keep;
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

        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);

        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }

    private int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;

        for(int i = 0; i < tagsProp.arraySize; i++) 
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) 
            {
                found = true; 
                return i;
            }
        }


        if (!found && tType == TagType.Tag) 
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        else if(!found && tType == TagType.Layer) 
        {
            for(int j = 8; j < tagsProp.arraySize; j++) 
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);

                if (newLayer.stringValue == "") 
                {
                    Debug.Log("Adding New Layer: "+newTag);
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }

        return -1;
    }

    private float[,] GetHeights(bool checkReset = true) 
    {
        return (!resetTerrain || !checkReset) ? terrainData.GetHeights(0, 0, hmr, hmr) : new float[hmr, hmr];
    }

    private void SetHeights(float[,] heightMap) 
    {
        terrainData.SetHeights(0, 0, heightMap);
    }
}
