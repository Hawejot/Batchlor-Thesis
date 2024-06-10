using UnityEngine;
using Oculus.Interaction;


public class PlayerMovement : MonoBehaviour
{
    public float speed = 10.0f; // Speed of the player's movement

    public GameObject ground; // Reference to the ground GameObject

    private Transform playerTransform; // Reference to the Player GameObject's Transform
    private ControllerAxis2D rightControllerAxis; // Reference to the ControllerAxis2D script
    private Collider groundCollider; // Reference to the ground's collider

    void Start()
    {
        // Find the GameObject with the "Player" tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Get the Transform component attached to the "Player" GameObject
        playerTransform = player.GetComponent<Transform>();

        // Find the ControllerAxis2D component on the same GameObject this script is attached to
        rightControllerAxis = GetComponent<ControllerAxis2D>();

        // Get the Collider component attached to the ground GameObject
        groundCollider = ground.GetComponent<Collider>();

        // Check if the required components are found
        if (playerTransform == null)
        {
            Debug.LogError("Transform component not found on the Player GameObject.");
        }
        if (rightControllerAxis == null)
        {
            Debug.LogError("ControllerAxis2D component not found on the same GameObject.");
        }
        if (groundCollider == null)
        {
            Debug.LogError("Collider component not found on the Ground GameObject.");
        }
    }

    void Update()
    {
        // Only update movement if the required components are available
        if (playerTransform != null && rightControllerAxis != null && groundCollider != null)
        {
            UpdateMovement();
        }
    }

    void UpdateMovement()
    {
        // Get the joystick input values from the right controller
        Vector2 inputAxis = rightControllerAxis.Value();

        // Convert the 2D input into a 3D movement vector
        Vector3 move = new Vector3(inputAxis[0], 0, inputAxis[1]);

        // Transform the movement vector to align with the player's orientation
        move = playerTransform.TransformDirection(move);

        // Calculate the potential new position
        Vector3 potentialNewPosition = playerTransform.position + move * Time.deltaTime * speed;

        //Debug.Log(potentialNewPosition);

        if (IsPositionOnGround(potentialNewPosition))
        {
            playerTransform.position = potentialNewPosition;
        }
    }

    bool IsPositionOnGround(Vector3 position)
    {

        // Get the bounds of the ground collider
        Bounds groundBounds = groundCollider.bounds;

        // Check if the position's x and z are within the bounds of the ground collider
        return (position.x >= groundBounds.min.x && position.x <= groundBounds.max.x &&
                position.z >= groundBounds.min.z && position.z <= groundBounds.max.z);
    }
}
