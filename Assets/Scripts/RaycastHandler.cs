using UnityEngine;

public class RaycastHandler : MonoBehaviour
{
    public LayerMask layerMask;                  // Layer mask to specify which layers the raycast should hit
    public Vector3 raycastOffset = new Vector3(0.1f, 0, 0); // Offset for the raycast start position
    public GameObject raycastHost;
    private LineRenderer lineRenderer;           // Line Renderer for visualizing the ray
    private bool isActive = false;               // Whether the raycast is active
    private Vector3 targetPoint;                 // Target point for the raycast

    void Start()
    {
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
        if (!isActive) return;

        // Perform a raycast from the raycastHost's position in its forward direction
        Vector3 rayOrigin = raycastHost.transform.position + raycastHost.transform.TransformDirection(raycastOffset);
        Ray ray = new Ray(rayOrigin, raycastHost.transform.forward); // Changed to raycastHost's forward direction
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
    }

    public void ActivateRaycast()
    {
        isActive = true;
        lineRenderer.enabled = true;
    }

    public void DeactivateRaycast()
    {
        isActive = false;
        lineRenderer.enabled = false;
    }

    public Vector3 GetTargetPoint()
    {
        return targetPoint;
    }
}
