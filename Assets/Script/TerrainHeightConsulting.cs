using UnityEngine;

[RequireComponent(typeof(Terrain))]
[ExecuteInEditMode]
public class TerrainHeightConsulting : MonoBehaviour
{
    [SerializeField] Vector2 pos;

    Terrain terrain;

    private void Start()
    {
        terrain = GetComponent<Terrain>();    
    }

    [ContextMenu("Consult")]
    public void Consult() 
    {
        var heights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
        var height = heights[(int)pos.x, (int)pos.y];

        Debug.Log($"<color=green>({pos.x}, {pos.y}) Height is {height}</color>");
    }
}
