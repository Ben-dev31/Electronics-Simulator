using UnityEngine;

public class Word : MonoBehaviour
{
    public Transform target;       // The target object to rotate around
    public float rotationSpeed = 30f;  // Speed of camera rotation
    public float zoomSpeed = 5f;       // Speed of camera zoom
    public float minZoomDistance = 2f; // Minimum distance for zoom
    public float maxZoomDistance = 20f; // Maximum distance for zoom

    private Vector3 offset;         // Initial offset from the target
    private float currentZoom = 10f; // Current zoom level
    private float pitch = 2f;       // Pitch angle for the camera
    private float yaw = 0f;         // Yaw angle for the camera

    void Start()
    {
        // Initialize the offset and currentZoom
        offset = transform.position - target.position;
        currentZoom = offset.magnitude;
    }

    void Update()
    {
        // Handle camera rotation
        HandleRotation();

        // Handle camera zoom
        HandleZoom();

        // Update camera position
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        // Right mouse button for rotation
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // Clamp the pitch to avoid extreme angles
            pitch = Mathf.Clamp(pitch, -30f, 60f);
        }
    }
 
    void HandleZoom()
    {
        // Use mouse scroll wheel for zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoomDistance, maxZoomDistance);
    }

    void UpdateCameraPosition()
    {
        // Calculate the new position based on yaw and pitch
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.position = target.position - rotation * Vector3.forward * currentZoom + Vector3.up * 2f;

        // Look at the target object
        transform.LookAt(target.position);
    }
}
