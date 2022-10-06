using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor class to randomize values in the selected GameObjects transforms
/// </summary>
public class TransformRandomizer : Editor
{
    private float minScaleMultiplier = 0.5f;
    private float maxScaleMultiplier = 1.5f;
    private float minPositionDisplacement = -5.0f;
    private float maxPositionDisplacement = 5.0f;
    private float minRotationDelta = -90f;
    private float maxRotationDelta = 90.0f;
    bool showSection = true;

    #region UI sections

    /// <summary>
    /// Main UI for the Transform Randomizer Tool
    /// </summary>
    public void TransformRandomizer_UI()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //Horizontal line to start the section

        showSection = EditorGUILayout.BeginFoldoutHeaderGroup(showSection, "Randomize Transform");

        if (showSection)
        {
            PositionRandomizer_UI();
            RotationRandomizer_UI();
            ScaleRandomizer_UI();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }


    /// <summary>
    /// UI section for the Position Randomizer tool.
    /// </summary>
    public void PositionRandomizer_UI()
    {
        minPositionDisplacement = EditorGUILayout.FloatField("Min Position Delta:", minPositionDisplacement);
        minPositionDisplacement = Mathf.Min(minPositionDisplacement, maxPositionDisplacement);
        maxPositionDisplacement = EditorGUILayout.FloatField("Max Position Delta:", maxPositionDisplacement);
        maxPositionDisplacement = Mathf.Max(minPositionDisplacement, maxPositionDisplacement);

        if (GUILayout.Button("Randomize Position"))
        {
            RandomizePosition(minPositionDisplacement, maxPositionDisplacement);
        }
    }


    /// <summary>
    /// UI section for the Rotation Randomizer tool.
    /// </summary>
    public void RotationRandomizer_UI()
    {
        minRotationDelta = EditorGUILayout.FloatField("Min Rotation Delta:", minRotationDelta);
        minRotationDelta = Mathf.Min(minRotationDelta, maxRotationDelta);

        maxRotationDelta = EditorGUILayout.FloatField("Max Rotation Delta:", maxRotationDelta);
        maxRotationDelta = Mathf.Max(minRotationDelta, maxRotationDelta);

        if (GUILayout.Button("Randomize Rotation"))
        {
            RandomizeRotation(minRotationDelta, maxRotationDelta);
        }
    }


    /// <summary>
    /// UI section for the Scale Randomizer tool.
    /// </summary>
    public void ScaleRandomizer_UI()
    {
        minScaleMultiplier = EditorGUILayout.FloatField("Min Scale multiplier:", minScaleMultiplier);
        minScaleMultiplier = Mathf.Min(minScaleMultiplier, maxScaleMultiplier); //validate min always being lower or equals to max

        maxScaleMultiplier = EditorGUILayout.FloatField("Max Scale multiplier:", maxScaleMultiplier);
        maxScaleMultiplier = Mathf.Max(minScaleMultiplier, maxScaleMultiplier);//validate max always being greater or equals to max

        if (GUILayout.Button("Randomize Scale"))
        {
            RandomizeScale(minScaleMultiplier, maxScaleMultiplier);
        }
    }

    #endregion


    #region Randomizers

    /// <summary>
    /// Randomize the position of all the selected objects in the scene adding a random value to its position in all 3 axis.
    /// </summary>
    /// <param name="minPositionDelta">Minumum value that the random number can take (Negative numbers allowed)</param>
    /// <param name="maxPositionDelta">Maximum value that the random number can take (Negative numbers allowed)</param>
    static void RandomizePosition(float minPositionDelta, float maxPositionDelta)
    {
        foreach (GameObject GObject in Selection.gameObjects)
        {
            float randomX = Random.Range(minPositionDelta, maxPositionDelta);
            float randomY = Random.Range(minPositionDelta, maxPositionDelta);
            float randomZ = Random.Range(minPositionDelta, maxPositionDelta);
            Vector3 randomVector = new Vector3(randomX, randomY, randomZ);
            GObject.transform.position = GObject.transform.position + randomVector;
        }
    }


    /// <summary>
    /// Randomize the scale of all selected objects in the scene, multiplying all 3 scale axis by a random number between minMultiplier and maxMultiplier.
    /// </summary>
    /// <param name="minMultiplier"> Min multiplier to be applied to all 3 scale axis values</param>
    /// <param name="maxMultiplier"> Min multiplier to be applied to all 3 scale axis values</param>
    static void RandomizeScale(float minMultiplier, float maxMultiplier)
    {
        foreach (GameObject GObject in Selection.gameObjects)
        {
            if (GObject.GetComponent<Collider>() != null)
            {
                float random = Random.Range(minMultiplier, maxMultiplier);
                GObject.transform.localScale = GObject.transform.localScale * random;
            }
        }
    }


    /// <summary>
    /// Randomize the rotation of all the selected objects in the scene adding a different random value to its rotation in each axis.
    /// </summary>
    /// <param name="minRotationDelta">Minumum value that any random number can take (Negative numbers allowed)</param>
    /// <param name="maxRotationDelta">Maximum value that any random number can take (Negative numbers allowed)</param>
    static void RandomizeRotation(float minRotationDelta, float maxRotationDelta)
    {
        foreach (GameObject GObject in Selection.gameObjects)
        {
            float randomX = Random.Range(minRotationDelta, maxRotationDelta);
            float randomY = Random.Range(minRotationDelta, maxRotationDelta);
            float randomZ = Random.Range(minRotationDelta, maxRotationDelta);
            Vector3 randomVector = new Vector3(randomX, randomY, randomZ);
            Vector3 currentRotation = GObject.transform.localEulerAngles;
            GObject.transform.localEulerAngles = (currentRotation + randomVector);
        }
    }

    #endregion
}
