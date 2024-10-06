using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public static class Utils
{
    public static Vector3 GetRandomSpawnPoint()
    {
        // Find all GameObjects with the tag "SpawnPoint"
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        // If there are no spawn points, return a default position (e.g., Vector3.zero)
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found. Using default position.");
            return Vector3.zero; // or any other default position you prefer
        }

        // Choose a random spawn point from the array
        GameObject randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Return the position of the selected spawn point
        return randomSpawnPoint.transform.position;
    }

    public static void SetRenderLayerInChildren(Transform transform, int layerNumber)
    {
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true))
        {
            if (trans.CompareTag("IgnoreLayerChange"))
                continue;

            trans.gameObject.layer = layerNumber;
        }
    }
}
