using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Indicator : MonoBehaviour
{
    private GameObject playerReference;

    // Separate offset values for each axis
    public float offsetY = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // Attempt to find the GameObject with the name "CenterEyeAnchor"
        playerReference = GameObject.Find("CenterEyeAnchor");

        // Check if the GameObject was found
        if (playerReference == null)
        {
            Debug.LogError("CenterEyeAnchor GameObject not found. Please ensure it exists in the scene.");
        }

        // Rotate the target GameObject by 90 degrees around the Y-axis
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Only update the position if the playerReference has been successfully assigned
        if (playerReference != null)
        {
            // Calculate the new position with the offsets
            Vector3 newPosition = playerReference.transform.position;
            newPosition.y += offsetY;

            // Apply the new position
            transform.position = newPosition;
        }
    }
}
