using UnityEngine;

public class ApplyTransform : MonoBehaviour
{
    // Public properties for assigning the objects and transformation parameters
    public GameObject Object1;
    public GameObject Object2;
    public float XShift = 0;
    public float YShift  = 0;
    public float ZShift = 0;



    // Initialization method called once at the start
    void Start()
    {
        // Check if Object1 and Object2 are assigned
        if (Object1 == null || Object2 == null)
        {
            Debug.LogWarning("Object1 or Object2 is not assigned.");
            return;
        }
    }

    // Update method called once per frame
    void Update()
    {
        // Apply position from Object1 to Object2
        Object2.transform.position = Object1.transform.position;

        // Apply translation from Object1 to Object2
        Object2.transform.Translate(new Vector3(XShift, YShift, ZShift));

        // Apply rotation from Object1 to Object2
        Quaternion rotation = Object1.transform.rotation;
        Object2.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);

    }

}