using System.Collections.Generic;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    public GameObject referenceObject;  // Reference object set via the editor
    public ModelBuilder modelBuilder;  // Model builder set via the editor
    public float xShift = 0f; // Value to shift model in the x direction
    public float yShift = 0f; // Value to shift model in the y direction
    public float zShift = 0f; // Value to shift model in the z direction
    private GameObject annParent;

    public GameObject SpawnModel(ApiDataFetcher.LayerInfo[] layers)
    {

        modelBuilder.InstantiateLayers(layers);

        // Assuming you have a method getModelCreated() that returns a boolean
        while (!modelBuilder.getModelCreated())
        {
            // Add a small delay to prevent freezing the main thread
            System.Threading.Thread.Sleep(10);
        }

        annParent = modelBuilder.getAnnParent();
        SetParentToReferenceParent(annParent, referenceObject);
        SetPosition(annParent, referenceObject);
        RotateModel(annParent);
        FitScaleToReferenceObject(annParent, referenceObject);

        return annParent;
    }

    private void SetParentToReferenceParent(GameObject model, GameObject referenceObject)
    {
        if (model != null && referenceObject != null)
        {
            model.transform.SetParent(referenceObject.transform.parent, true);
        }
    }

    private void FitScaleToReferenceObject(GameObject model, GameObject referenceObject)
    {
        if (model != null && referenceObject != null)
        {
            BoxCollider referenceCollider = referenceObject.GetComponent<BoxCollider>();
            if (referenceCollider != null)
            {
                Vector3 referenceSize = referenceCollider.size;
                BoxCollider modelCollider = model.GetComponent<BoxCollider>();
                if (modelCollider != null)
                {
                    Vector3 modelSize = modelCollider.size;
                    Vector3 scaleAdjustment = new Vector3(
                        referenceSize.x / modelSize.x,
                        referenceSize.y / modelSize.y,
                        referenceSize.z / modelSize.z
                    );

                    model.transform.localScale = Vector3.Scale(model.transform.localScale, scaleAdjustment);
                }
            }
        }
    }

    private void SetPosition(GameObject model, GameObject referenceObject)
    {
        if (model != null && referenceObject != null)
        {
            Vector3 referencePosition = referenceObject.transform.position;
            referencePosition.x += xShift;
            referencePosition.y += yShift;
            referencePosition.z += zShift;
            model.transform.position = referencePosition;
        }
    }

    private void RotateModel(GameObject model)
    {
        if (model != null)
        {
            model.transform.Rotate(0, 90, 0);
        }
    }
}
