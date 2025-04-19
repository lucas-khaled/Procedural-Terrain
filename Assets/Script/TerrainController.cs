using UnityEngine;

public class TerrainController : MonoBehaviour
{
    public float perlinOffsetX = 0;
    public float perlinOffsetZ = 0;
    public float perlinXScale = 0.001f;
    public float perlinZScale = 0.001f;
    public float perlinHeightScale = 0.5f;
    public float perlinPersistance = 8;
    public int perlinOctaves = 3;

    private float perlinZeroOffset;

    private void OnValidate()
    {
        perlinZeroOffset = (Terrain.activeTerrains.Length + 1) * Terrain.activeTerrains[0].terrainData.size.x;
        foreach (Terrain terrain in Terrain.activeTerrains)
            Perlin(terrain);

        foreach(Terrain terrain in Terrain.activeTerrains) 
            FixSeams(terrain);
        
    }

    public void Perlin(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        var hmr = terrainData.heightmapResolution;
        float[,] heightMap = new float[hmr, hmr];

        for (int z = 0; z < hmr; z++)
        {
            for (int x = 0; x < hmr; x++)
            {
                float worldPosX = x / (float)hmr * terrainData.size.x + terrain.transform.position.x;
                float worldPosZ = z / (float)hmr * terrainData.size.z + terrain.transform.position.z;

                heightMap[z, x] += Utils.fBM((worldPosX + perlinOffsetX + perlinZeroOffset) * perlinXScale,
                                            (worldPosZ + perlinOffsetZ + perlinZeroOffset) * perlinZScale,
                                            perlinOctaves,
                                            perlinPersistance) * perlinHeightScale;

            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void FixSeams(Terrain terrain) 
    {
        TerrainData terrainData = terrain.terrainData;
        int hmr = terrainData.heightmapResolution;
        float[,] heigthMap = terrainData.GetHeights(0, 0, hmr, hmr);

        if(terrain.topNeighbor != null) 
        {
            float[,] topNeighbourHeightMap = terrain.topNeighbor.terrainData.GetHeights(0, 0, hmr, hmr);
            for (int z = 0; z < hmr; ++z)
            {
                float avgHeight = (heigthMap[hmr - 1, z] + topNeighbourHeightMap[0, z]) * 0.5f;
                topNeighbourHeightMap[0, z] = avgHeight;
                heigthMap[hmr - 1, z] = avgHeight;
            }

            terrain.topNeighbor.terrainData.SetHeights(0, 0, topNeighbourHeightMap);
        }

        if(terrain.rightNeighbor != null) 
        {
            float[,] rightNeighbourHeightMap = terrain.rightNeighbor.terrainData.GetHeights(0, 0, hmr, hmr);
            for (int x = 0; x < hmr; ++x)
            {
                float avgHeight = (heigthMap[x, hmr - 1] + rightNeighbourHeightMap[x, 0]) * 0.5f;
                rightNeighbourHeightMap[x, 0] = avgHeight;
                heigthMap[x, hmr - 1] = avgHeight;
            }

            terrain.rightNeighbor.terrainData.SetHeights(0, 0, rightNeighbourHeightMap);
            
        }
        terrainData.SetHeights(0, 0, heigthMap);
    }
}
