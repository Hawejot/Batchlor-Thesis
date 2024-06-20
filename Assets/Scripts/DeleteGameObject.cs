using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to delete a specified GameObject.
/// </summary>
public class DeleteGameObject : MonoBehaviour
{
    /// <summary>
    /// The GameObject to be deleted.
    /// </summary>
    public GameObject gameObjectToDelete;

    /// <summary>
    /// Deletes the specified GameObject if it is assigned.
    /// </summary>
    public void DestroyObject()
    {
        if (gameObjectToDelete != null)
        {
            Destroy(gameObjectToDelete);
        }
        else
        {
            Debug.LogWarning("No GameObject specified to delete.");
        }
    }
}
