using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework;
using System.Collections.Generic;

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
