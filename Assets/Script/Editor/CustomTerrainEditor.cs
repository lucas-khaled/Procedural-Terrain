using UnityEngine;
using EditorGUITable;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    private CustomTerrain terrain;

    #region Properties

    private SerializedProperty resetTerrain;
    private SerializedProperty randomHeightRange;
    private SerializedProperty heightMapScale;
    private SerializedProperty heightMapImage;
    private SerializedProperty perlinXScale;
    private SerializedProperty perlinYScale;
    private SerializedProperty perlinXOffset;
    private SerializedProperty perlinYOffset;
    private SerializedProperty perlinOctaves;
    private SerializedProperty perlinPersistance;
    private SerializedProperty perlinHeightScale;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    #endregion

    #region Fold outs

    private bool showRandom = false;
    private bool showLoadHeights = false;
    private bool showPerlin = false;
    private bool showMultiplePerlin = false;

    #endregion

    private void OnEnable()
    {
        terrain = (CustomTerrain)target;
        randomHeightRange = serializedObject.FindProperty(nameof(terrain.randomHeightRange));
        heightMapScale = serializedObject.FindProperty(nameof(terrain.heightMapScale));
        heightMapImage = serializedObject.FindProperty(nameof(terrain.heightMapImage));
        perlinXScale = serializedObject.FindProperty(nameof(terrain.perlinXScale));
        perlinYScale = serializedObject.FindProperty(nameof(terrain.perlinYScale));
        perlinXOffset = serializedObject.FindProperty(nameof(terrain.perlinOffsetX));
        perlinYOffset = serializedObject.FindProperty(nameof(terrain.perlinOffsetY));
        perlinOctaves = serializedObject.FindProperty(nameof(terrain.perlinOctaves));
        perlinPersistance = serializedObject.FindProperty(nameof(terrain.perlinPersistance));
        perlinHeightScale = serializedObject.FindProperty(nameof(terrain.perlinHeightScale));
        resetTerrain = serializedObject.FindProperty(nameof(terrain.resetTerrain));
        
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty(nameof(terrain.perlinParameters));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(resetTerrain);

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

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);

            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        showPerlin = EditorGUILayout.Foldout(showPerlin, "Single Perlin Noise");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlins", EditorStyles.boldLabel);

            EditorGUILayout.Slider(perlinXScale, 0, 0.05f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 0.05f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 10000, new GUIContent("Offset X"));
            EditorGUILayout.IntSlider(perlinYOffset, 0, 10000, new GUIContent("Offset Y"));
            EditorGUILayout.IntSlider(perlinOctaves, 0, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));

            if (GUILayout.Button("Do Perlin Noise"))
            {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);

            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, perlinParameters);
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+"))
                terrain.AddNewPerlin();
            if (GUILayout.Button("-"))
                terrain.RemovePerlin();

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Mulitple Perlin"))
                terrain.MultiplePerlinTerrain();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
