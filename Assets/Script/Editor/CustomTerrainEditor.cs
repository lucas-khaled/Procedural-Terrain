using UnityEngine;
using EditorGUITable;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    private CustomTerrain terrain;
    private SerializedProperty randomHeightRange;

    private bool showRandom = false;

    private void OnEnable()
    {
        terrain = (CustomTerrain)target;
        randomHeightRange = serializedObject.FindProperty(nameof(terrain.randomHeightRange));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(randomHeightRange);

            if(GUILayout.Button("Random Heights")) 
            {
                terrain.RandomTerrain();
            }

            
        }

        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
