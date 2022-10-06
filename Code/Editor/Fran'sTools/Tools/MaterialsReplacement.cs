using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MaterialsReplacement : Editor
{
    string MaterialLibraryPath = "";
    bool showSection = true;
    /// <summary>
    /// UI section for the Replace Materials Tool.
    /// </summary>
    public void ReplaceMaterialsUI()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //Horizontal line to start the section

        showSection = EditorGUILayout.BeginFoldoutHeaderGroup(showSection, "Replace Materials Tool");

        if (showSection)
        {
            //EditorGUILayout.LabelField("Replace Materials");

            EditorGUILayout.BeginHorizontal();

            MaterialLibraryPath = EditorGUILayout.TextField("Material Library Path:", MaterialLibraryPath);

            if (GUILayout.Button("..."))
            {
                MaterialLibraryPath = EditorUtility.OpenFolderPanel("Material Library Root Folder", "", "");
                GUIUtility.keyboardControl = 0; //work around to change focus to refresh the TextField
            }
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Replace Materials"))
            {
                if (MaterialLibraryPath == "")
                {
                    EditorUtility.DisplayDialog("Error!", "You must select a Material Library root folder path", "ok");
                    return;
                }
                ReplaceMaterials(MaterialLibraryPath);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }


    /// <summary>
    /// Replace the materials in all selected objects with matching names materials in the Assets/MaterialsLibrary folder.
    /// </summary>
    private static void ReplaceMaterials(string MaterialLibraryPath = "Assets/MaterialsLibrary")
    {
        foreach (GameObject GObject in Selection.gameObjects)
        {
            var rendererComponent = GObject.GetComponent<Renderer>();

            if (rendererComponent) //making sure the object has a renderer component
            {
                var ObjectMaterials = new List<Material>();
                rendererComponent.GetSharedMaterials(ObjectMaterials);

                //find all existing materials in the Material Library and replace them in the shared materials array copy.
                for (int i = 0; i < ObjectMaterials.Count; ++i)
                {
                    string materialName = ObjectMaterials[i].name + ".mat";
                    var materialPathArray = System.IO.Directory.GetFiles(MaterialLibraryPath, materialName, SearchOption.AllDirectories);

                    if (materialPathArray.Length != 0) //if a replacement material was found in the library
                    {
                        Material materialAsset = (Material)AssetDatabase.LoadAssetAtPath(materialPathArray[0], typeof(Material));
                        ObjectMaterials[i] = materialAsset;
                    }
                }
                //assign the shared materials array copy to the object
                GObject.GetComponent<MeshRenderer>().sharedMaterials = ObjectMaterials.ToArray();
            }
        }
    }
}
