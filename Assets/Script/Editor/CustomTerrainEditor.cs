using UnityEngine;
using EditorGUITable;
using UnityEditor;
using static CustomTerrain;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    private CustomTerrain terrain;
    private Vector2 scrollPos;

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
    private SerializedProperty peakCount;
    private SerializedProperty voronoiFallOff;
    private SerializedProperty voronoiDropOff;
    private SerializedProperty voronoiMinHeight;
    private SerializedProperty voronoiMaxHeight;
    private SerializedProperty voronoiType;
    private SerializedProperty MPDHeightMin;
    private SerializedProperty MPDHeightMax;
    private SerializedProperty MPDHeightDampenerPower;
    private SerializedProperty MPDRoughness;
    private SerializedProperty SmoothAmount;
    private SerializedProperty maximumTrees;
    private SerializedProperty treesSpacing;
    private SerializedProperty maxDetails;
    private SerializedProperty detailSpacing;
    private SerializedProperty waterHeight;
    private SerializedProperty waterGO;
    private SerializedProperty erosionType;
    private SerializedProperty erosionStregth;
    private SerializedProperty springsPerRiver;
    private SerializedProperty solubility;
    private SerializedProperty droplets;
    private SerializedProperty erosionSmoothAmount;
    private SerializedProperty thermalStrength;
    private SerializedProperty erosionAmount;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    GUITableState treesTable;
    SerializedProperty trees;

    GUITableState detailMapTable;
    SerializedProperty details;

    #endregion

    #region Fold outs

    private bool showRandom = false;
    private bool showLoadHeights = false;
    private bool showPerlin = false;
    private bool showMultiplePerlin = false;
    private bool showVoronoi = false;
    private bool showMPD = false;
    private bool showSmooth = false;
    private bool showSplatMaps = false;
    private bool showVegetation = false;
    private bool showDetail = false;
    private bool showWater = false;
    private bool showErosion = false;

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
        peakCount = serializedObject.FindProperty(nameof(terrain.voronoiPeakCount));
        voronoiFallOff = serializedObject.FindProperty(nameof(terrain.voronoiFallOff));
        voronoiDropOff = serializedObject.FindProperty(nameof(terrain.voronoiDropOff));
        voronoiMinHeight = serializedObject.FindProperty(nameof(terrain.voronoiMinHeight));
        voronoiMaxHeight = serializedObject.FindProperty(nameof(terrain.voronoiMaxHeight));
        voronoiType = serializedObject.FindProperty(nameof(terrain.voronoiType));
        MPDHeightMin = serializedObject.FindProperty(nameof(terrain.MPDHeightMin));
        MPDHeightMax = serializedObject.FindProperty(nameof(terrain.MPDHeightMax));
        MPDHeightDampenerPower = serializedObject.FindProperty(nameof(terrain.MPDHeightDampenerPower));
        MPDRoughness = serializedObject.FindProperty(nameof(terrain.MPDRoughness));
        SmoothAmount = serializedObject.FindProperty(nameof(terrain.smoothAmount));
        maximumTrees = serializedObject.FindProperty(nameof(terrain.maximumTrees));
        treesSpacing = serializedObject.FindProperty(nameof(terrain.treesSpacing));
        maxDetails = serializedObject.FindProperty(nameof(terrain.maxDetails));
        detailSpacing = serializedObject.FindProperty(nameof(terrain.detailSpacing));
        waterHeight = serializedObject.FindProperty(nameof(terrain.waterHeight));
        waterGO = serializedObject.FindProperty(nameof(terrain.waterGO));
        erosionType = serializedObject.FindProperty(nameof(terrain.erosionType));
        erosionStregth = serializedObject.FindProperty(nameof(terrain.erosionStrength));
        springsPerRiver = serializedObject.FindProperty(nameof(terrain.springsPerRiver));
        solubility = serializedObject.FindProperty(nameof(terrain.solubility));
        droplets = serializedObject.FindProperty(nameof(terrain.droplets));
        erosionSmoothAmount = serializedObject.FindProperty(nameof(terrain.erosionSmoothAmount));
        thermalStrength = serializedObject.FindProperty(nameof(terrain.thermalStrength));
        erosionAmount = serializedObject.FindProperty(nameof(terrain.erosionQuantity));

        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty(nameof(terrain.perlinParameters));

        splatMapTable = new GUITableState("splatHeightsTable");
        splatHeights = serializedObject.FindProperty(nameof(terrain.splatHeights));

        treesTable = new GUITableState("treesTable");
        trees = serializedObject.FindProperty(nameof(terrain.vegetation));

        detailMapTable = new GUITableState("detailsTable");
        details = serializedObject.FindProperty(nameof(terrain.details));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Rect r = EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

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

            GUILayout.Space(20);
            if (GUILayout.Button("Apply Ridge Noise"))
                terrain.ApplyRigdeNoise();
        }

        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi) 
        {
            EditorGUILayout.IntSlider(peakCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0.01f, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(voronoiDropOff, 0.01f, 10, new GUIContent("Dropoff"));
            EditorGUILayout.Slider(voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);


            if (GUILayout.Button("Voronoi"))
                terrain.Voronoi();
        }

        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD) 
        {
            EditorGUILayout.PropertyField(MPDHeightMin);
            EditorGUILayout.PropertyField(MPDHeightMax);
            EditorGUILayout.PropertyField(MPDHeightDampenerPower);
            EditorGUILayout.PropertyField(MPDRoughness);

            if (GUILayout.Button("MPD"))
                terrain.MidPointDisplacement();
        }

        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth");
        if (showSmooth) 
        {
            EditorGUILayout.PropertyField(SmoothAmount);

            if (GUILayout.Button("Smooth"))
                terrain.Smooth();
        }

        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);

            splatMapTable = GUITableLayout.DrawTable(splatMapTable, splatHeights);
            GUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                terrain.AddNewSplatHeight();

            if (GUILayout.Button("-"))
                terrain.RemoveSplatHeights();
            EditorGUILayout.EndHorizontal();


            if(GUILayout.Button("Apply Splatmap")) 
            {
                terrain.SplatMaps();
            }
        }

        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maximumTrees, 1, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treesSpacing, 2, 20, new GUIContent("Trees Spacing"));

            treesTable = GUITableLayout.DrawTable(treesTable, trees);
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                terrain.AddNewTree();

            if (GUILayout.Button("-"))
                terrain.RemoveTrees();
            EditorGUILayout.EndHorizontal();

            if(GUILayout.Button("Apply Vegetation")) 
            {
                terrain.PlantVegetation();
            }
        }

        showDetail = EditorGUILayout.Foldout(showDetail, "Detail");
        if (showDetail) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Detail", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Maximum Details"));
            EditorGUILayout.IntSlider(detailSpacing, 1, 20, new GUIContent("Detail Spacing"));
            detailMapTable = GUITableLayout.DrawTable(detailMapTable, details);
            GUILayout.Space(20);

            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) 
            {
                terrain.AddNewDetail();
            }
            if (GUILayout.Button("-")) 
            {
                terrain.RemoveDetails();
            }
            EditorGUILayout.EndHorizontal();

            if(GUILayout.Button("Apply Details")) 
            {
                terrain.ApplyDetails();
            }

        }

        showWater = EditorGUILayout.Foldout(showWater, "Water");
        if (showWater) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);

            EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO);

            if(GUILayout.Button("Add Water")) 
            {
                terrain.AddWater();
            }
        }

        showErosion = EditorGUILayout.Foldout(showErosion, "Erosion");
        if (showErosion) 
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Erosion", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(erosionType);
            EditorGUILayout.Slider(erosionStregth, 0, 1, new GUIContent("Erosion Strength"));
            EditorGUILayout.IntSlider(droplets, 0, 500, new GUIContent("Droplets"));
            EditorGUILayout.Slider(solubility, 0.001f, 1f, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Springs Per River"));
            EditorGUILayout.Slider(thermalStrength, 0.001f, 1f, new GUIContent("Thermal Strength"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));
            EditorGUILayout.PropertyField(erosionAmount);

            if (GUILayout.Button("Erode")) 
            {
                terrain.Erode();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
