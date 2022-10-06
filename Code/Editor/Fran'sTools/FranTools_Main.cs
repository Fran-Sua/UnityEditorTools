using UnityEngine;
using UnityEditor;

/// <summary>
/// This is the main class for all the tools contained in Fran's Tools
/// It construct the main UI.
/// Each tool has its own UI constructor + methods in their own .cs file in the subfolder .../Fran'sTools/Tools
/// </summary>
public class FranTools_Main : EditorWindow
{
    private TransformRandomizer transformRandomizer;
    private MaterialsReplacement materialsReplacement;
    private MeshCombiner meshCombiner;

    public void OnEnable()
    {
        transformRandomizer = ScriptableObject.CreateInstance<TransformRandomizer>();
        materialsReplacement = ScriptableObject.CreateInstance<MaterialsReplacement>();
        meshCombiner = ScriptableObject.CreateInstance<MeshCombiner>();
    }


    [MenuItem("Fran's Tools/Tools %M")]
    public static void ShowWindow()
    {
        GetWindow<FranTools_Main>("Fran's Tools");
    }

    private void OnGUI()
    {
        transformRandomizer.TransformRandomizer_UI();

        EditorGUILayout.Space(10.0f);
        materialsReplacement.ReplaceMaterialsUI();

        EditorGUILayout.Space(10.0f);
        meshCombiner.MeshCombinerUI();
     } 

}

