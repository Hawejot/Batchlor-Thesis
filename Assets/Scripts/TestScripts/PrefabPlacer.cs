using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    public PrefabPositionFinder positionFinder;  // Reference to the PrefabPositionFinder
    public GameObject prefab;                    // The prefab to be placed
    private Vector3 targetPoint;                 // The target point for placement
    public GameObject cameraRig;                 // The target GameObject that the prefab should face
    public GameObject visualIndicatorPrefab;     // The prefab for the visual indicator
    public LayerMask layerMask;                  // Layer mask to specify which layers the raycast should hit

    private GameObject visualIndicatorInstance;  // Instance of the visual indicator
    private LineRenderer lineRenderer;           // Line Renderer for visualizing the ray
    public Vector3 raycastOffset = new Vector3(0.1f, 0, 0); // Offset for the raycast start position

    void Start()
    {
        // Initialize the target point to some default value
        targetPoint = cameraRig.transform.position + cameraRig.transform.forward * 2f;

        // Instantiate the visual indicator at the target point
        visualIndicatorInstance = Instantiate(visualIndicatorPrefab, targetPoint, Quaternion.identity);

        // Initialize the LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    void Update()
    {
        // Offset the ray's starting position from the cameraRig's position
        Vector3 rayOrigin = cameraRig.transform.position + cameraRig.transform.TransformDirection(raycastOffset);
        Ray ray = new Ray(rayOrigin, cameraRig.transform.forward);
        RaycastHit hit;
        lineRenderer.SetPosition(0, ray.origin);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            // If the raycast hits a mesh, update the target point to the hit point
            targetPoint = hit.point;
            Debug.Log("Raycast hit at: " + hit.point);
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            Debug.Log("Raycast did not hit any target.");
            lineRenderer.SetPosition(1, ray.origin + ray.direction * 100);  // Extend the line to a default distance if no hit
        }

        // Update the position of the visual indicator
        if (visualIndicatorInstance != null)
        {
            visualIndicatorInstance.transform.position = targetPoint;
        }
    }

    public void PlacePrefabAtNearestPosition()
    {
        // Use the PrefabPositionFinder to find the nearest valid position and required rotation
        (Vector3 position, Quaternion rotation) = positionFinder.FindNearestPosition(prefab, targetPoint, cameraRig);

        Debug.Log("Placing prefab at: " + position);

        // Instantiate the prefab at the found position and with the required rotation
        Instantiate(prefab, position, rotation);
    }
}
