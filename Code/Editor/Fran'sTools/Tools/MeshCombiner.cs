using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor class to combine GameObject meshes that share the same Materials.
/// </summary>
public class MeshCombiner : Editor
{
    bool showSection = true;

    /// <summary>
    /// UI section for the Combine Meshes Tool.
    /// </summary>
    public void MeshCombiner_UI()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //Horizontal line to start the section
        
        showSection = EditorGUILayout.BeginFoldoutHeaderGroup(showSection, "Combine Meshes");

        if (showSection)
        {
            if (GUILayout.Button("Combine Meshes"))
            {
                MergeSameMaterialMeshes();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    /// <summary>
    /// For all selected objects in the scene, find all the meshes containing the same material list and merge them into a single mesh.
    /// The result is placed in the scene.
    /// </summary>
    private static void MergeSameMaterialMeshes()
    {
        //Get all GameObjects that share materials with other objects
        var objectsToMerge = GetObjectsToMerge();
        
        //Merge objects that shares the same Materials
        for (int i=0; i<objectsToMerge.Count; ++i)
        {
            MergeMeshes(objectsToMerge[i], "NewMergedObject(" + (i+1) + ")" );
        }

    }

    


    /// <summary>
    /// From the selected objects in the scene, finds which ones share the same materials list.<br />These objects can be safely merged to lower drawcalls.
    /// </summary>
    /// <returns>A list of lists of objects sharing the same materials list. <br /> E.G.: { {BlueSphere, BlueCube} , {PurpleCone, PurpleTorus, PurpleCylinder} }</returns>
    private static List<List<GameObject>> GetObjectsToMerge()
    {
        var objectsToMerge_return = new List<List<GameObject>>();                       //Variable to return. Contains all the objects that can be merged.
        var materialsCheckedList = new List<List<Material>>();                          //Unique list of materials arrays 
        var selectedSceneObjects = new List<GameObject>(Selection.gameObjects);         //Copy of the selected objects in the scene

        for (int i = 0; i < selectedSceneObjects.Count; ++i)
        {
            var localObjectsToMerge = new List<GameObject>();
            var mainObjectMaterials = new List<Material>();
            var rendererComponent = selectedSceneObjects[i].GetComponent<Renderer>();

            if (!rendererComponent)                                                     //making sure the object has a renderer component
                continue;

            rendererComponent.GetSharedMaterials(mainObjectMaterials);                  //getting all the materials in the object and storing them in mainObjectMaterials

            if (!MaterialListContains(materialsCheckedList, mainObjectMaterials))       //Skip it if the exact same materials has been already checked
            {
                localObjectsToMerge.Add(selectedSceneObjects[i]);                       //Add current object to the temporal array containing Objects with the same Material List
                materialsCheckedList.Add(mainObjectMaterials);

                for (int j = i + 1; j < selectedSceneObjects.Count; ++j)                //check the following objects in the list for a match in all the materials
                {
                    rendererComponent = selectedSceneObjects[j].GetComponent<Renderer>();
                    if (!rendererComponent)                                             //making sure the object has a renderer component
                        continue;

                    var currentObjectMaterials = new List<Material>();
                    rendererComponent.GetSharedMaterials(currentObjectMaterials);

                    if (MaterialListEquals(mainObjectMaterials, currentObjectMaterials))//if both material lists matches, add the object to the list of objects to merge
                    {
                        localObjectsToMerge.Add(selectedSceneObjects[j]);

                        selectedSceneObjects.RemoveAt(j);                               //remove the object from the list to avoid re-checking it.                 
                        --j;
                    }
                }

                if (localObjectsToMerge.Count > 1)                                      //if we found more than one object with the same materials
                {
                    objectsToMerge_return.Add(localObjectsToMerge);
                    //Debug.Log("---------------------" + localObjectsToMerge.Count); Debug.Log("Merging Objects:"); foreach (GameObject objectToMerge in localObjectsToMerge) Debug.Log(objectToMerge.name);
                }
            }
        }
        return objectsToMerge_return;
    }


    /// <summary>
    /// Merges meshes containing the same material list in a new object. 
    /// </summary>
    /// <param name="objectsToMergeLists"></param>
    private static void MergeMeshes(List<GameObject> objectsToMergeList, string newObjectName)
    {        
        var combineList = new List<CombineInstance>();
        var allMeshFilters = new List<MeshFilter[]>();

        foreach (GameObject gameObject in objectsToMergeList)
        {
            MeshFilter[] meshFilters = gameObject.GetComponents<MeshFilter>();
            allMeshFilters.Add(gameObject.GetComponents<MeshFilter>());

            for (int i = 0; i < meshFilters.Length; ++i)
            {
                var tempCombineInstance = new CombineInstance();
                tempCombineInstance.mesh = meshFilters[i].sharedMesh;

                tempCombineInstance.transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                var submeshesCount = meshFilters[i].sharedMesh.subMeshCount;
                //submeshesCount = 1;
                for (int z = 0; z < submeshesCount; ++z)
                {
                    tempCombineInstance.subMeshIndex = z;
                    combineList.Add(tempCombineInstance);
                }
            }
        }

        //create the final Object
        GameObject newGameObject = new GameObject();
        newGameObject.gameObject.AddComponent<UnityEngine.MeshFilter>();
        newGameObject.gameObject.AddComponent<UnityEngine.MeshRenderer>();

        newGameObject.name = newObjectName;

        Mesh finalMesh = new Mesh();

        try
        {
            finalMesh.CombineMeshes(combineList.ToArray(), true);
        }
        catch (System.ArgumentException)
        {
            //If the combination failed due to the maximum number of vertex being exceeded in the UInt16 index format
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalMesh.CombineMeshes(combineList.ToArray(), true);
        }

        newGameObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        //copy the materials
        newGameObject.GetComponent<MeshRenderer>().sharedMaterials = objectsToMergeList[0].GetComponent<MeshRenderer>().sharedMaterials;

    }



    #region Helpers
    
    /// <summary>
    /// Checks if a material list from an object is contained in a list of list of materials
    /// </summary>
    /// <param name="materialsListOFLists">List of lists of materials</param>
    /// <param name="MaterialListToCheck">The list to be checked if it is contained in the materialListOfLists param.</param>
    /// <returns></returns>
    private static bool MaterialListContains(List<List<Material>> materialsListOFLists, List<Material> MaterialListToCheck)
    {
        //checks if a material list from an object is contained in a list of list of materials
        foreach (List<Material> materialList in materialsListOFLists)
            if (MaterialListEquals(materialList, MaterialListToCheck))
                return true;
        return false;
    }


    /// <summary>
    /// Checks if two material lists are exactly the same. Materials must be in the same order to return true.
    /// </summary>
    /// <param name="materialListA">First Material List to compare</param>
    /// <param name="materialListB">Second Material List to compare</param>
    /// <returns>boolean: true if both lists are exactly the same, false otherwise.</returns>
    private static bool MaterialListEquals(List<Material> materialListA, List<Material> materialListB)
    {
        //if not the same lenght, they are not equals
        if (materialListA.Count != materialListB.Count)
            return false;

        //if each element is not the same in the same order, then they are not equals
        for (int i = 0; i < materialListA.Count; ++i)
            //if (a[i].name != b[i].name)
            if (AssetDatabase.GetAssetPath(materialListA[i]) != AssetDatabase.GetAssetPath(materialListB[i]))
                return false;

        return true;
    }

    #endregion
}
