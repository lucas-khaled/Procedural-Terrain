using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TerrainInfoClick
{
    static TerrainInfoClick()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 1 && e.shift) // Clique esquerdo
        {
            Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(worldRay, out RaycastHit hit, 10000, LayerMask.GetMask("Terrain")))
            {
                var terrain = hit.collider.gameObject.GetComponent<Terrain>();
                var data = terrain.terrainData;

                var normX = hit.point.x / data.size.x;
                var normZ = hit.point.z / data.size.z;

                var steepness = data.GetSteepness(normX, normZ);

                int x = (int)(normX * (data.alphamapWidth - 1));
                int z = (int)(normZ * (data.alphamapHeight - 1));

                float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
                float height = heights[x, z];

                Debug.Log($"Indexes: ({x},{z}) -> Normalized: ({normX},{normZ}) -> Position: ({hit.point.x},{hit.point.z})" +
                    $"\nHeight: {hit.point.y} -> Relative Height: {height} -> GetHeight: {data.GetHeight(x, z)} -> Steepness {steepness}");

                Debug.Log($"{data.size.y}");
            }
            else
            {
                Debug.Log("Raycast não acertou nada.");
            }

            e.Use();
        }
    }
}
