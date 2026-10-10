using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 25f;
    public float deceleration = 30f;
    [Tooltip("If true, movement vector is normalized (diagonal same speed)")]
    public bool normalizeInput = true;

    [Header("Rotation")]
    public bool rotateTowardMouse = true;
    public Camera sceneCamera; // leave null to auto-assign Camera.main

    [Header("Map Boundary (choose one)")]
    [Tooltip("If true the script uses the assigned Collider's bounds; otherwise uses Min/Max bounds.")]
    public bool useBoundaryCollider = true;
    public Collider boundaryCollider; // 3D collider used as boundary
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);

    [Header("Debug")]
    public bool drawGizmos = true;
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.15f);
    public Color gizmoOutline = Color.green;

    [Header("Camera")]
    public Camera followCamera;
    public Vector3 cameraOffset = new Vector3(0f, 10f, -10f);
    public float cameraSmoothTime = 0.1f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    CharacterController controller;
    Vector3 velocity; // smoothed velocity (x,z used)

    // Camera runtime state
    Vector3 initialCamOffset;
    float currentZoom;
    Vector3 cameraVelocity = Vector3.zero;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (sceneCamera == null) sceneCamera = Camera.main;
        if (followCamera == null) followCamera = Camera.main;
        // cache initial camera offset and zoom
        if (followCamera != null)
        {
            initialCamOffset = followCamera.transform.position - transform.position;
            if (followCamera.orthographic)
                currentZoom = followCamera.orthographicSize;
            else
                currentZoom = initialCamOffset.magnitude;
        }
        // Ensure CharacterController does not apply unwanted gravity for top-down
        controller.detectCollisions = true;
    }

    void Update()
    {
        // Read input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (normalizeInput && input.sqrMagnitude > 1f) input = input.normalized;

        Vector3 target = new Vector3(input.x, 0f, input.y) * moveSpeed;

        float accel = (target.sqrMagnitude > 0.001f) ? acceleration : deceleration;
        velocity = Vector3.MoveTowards(velocity, target, accel * Time.deltaTime);

        // Rotation toward mouse or movement direction
        if (rotateTowardMouse && sceneCamera != null)
        {
            // Cast a ray from the camera to a horizontal plane at player's y
            Plane plane = new Plane(Vector3.up, transform.position);
            Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                Vector3 dir = hit - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                }
            }
        }
        else
        {
            // rotate to movement direction if moving
            Vector3 moveDir = new Vector3(velocity.x, 0f, velocity.z);
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
            }
        }

        // Camera zoom input
        if (followCamera != null)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                if (followCamera.orthographic)
                {
                    currentZoom -= scroll * zoomSpeed;
                    currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                    // smooth change
                    followCamera.orthographicSize = Mathf.Lerp(followCamera.orthographicSize, currentZoom, 0.15f);
                }
                else
                {
                    currentZoom -= scroll * zoomSpeed;
                    currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                }
            }
        }
    }

    void LateUpdate()
    {
        // Camera follow and zoom smoothing
        if (followCamera != null)
        {
            Vector3 desiredPos;
            if (followCamera.orthographic)
            {
                desiredPos = transform.position + initialCamOffset;
            }
            else
            {
                desiredPos = transform.position + initialCamOffset.normalized * currentZoom;
            }

            followCamera.transform.position = Vector3.SmoothDamp(followCamera.transform.position, desiredPos, ref cameraVelocity, cameraSmoothTime);
            // Keep camera looking at player
            followCamera.transform.LookAt(transform.position);
        }
    }

    void FixedUpdate()
    {
        // Move using CharacterController
        Vector3 move = velocity * Time.fixedDeltaTime;
        controller.Move(move);

        // Clamp position to bounds after movement applied
        Vector3 pos = transform.position;
        if (useBoundaryCollider && boundaryCollider != null)
        {
            Bounds b = boundaryCollider.bounds;
            pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
            pos.z = Mathf.Clamp(pos.z, b.min.z, b.max.z);
            transform.position = pos;
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
            transform.position = pos;
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.color = gizmoColor;

        if (useBoundaryCollider && boundaryCollider != null)
        {
            Bounds b = boundaryCollider.bounds;
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = gizmoOutline;
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else
        {
            Vector3 center = (new Vector3(minBounds.x, 0f, minBounds.y) + new Vector3(maxBounds.x, 0f, maxBounds.y)) * 0.5f;
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, 0.1f, maxBounds.y - minBounds.y);
            Gizmos.DrawCube(center, size);
            Gizmos.color = gizmoOutline;
            Gizmos.DrawWireCube(center, size);
        }
    }

    // Optional helpers
    public void SetBounds(Vector2 min, Vector2 max)
    {
        useBoundaryCollider = false;
        minBounds = min;
        maxBounds = max;
    }

    public void SetBoundaryCollider(Collider col)
    {
        if (col != null)
        {
            useBoundaryCollider = true;
            boundaryCollider = col;
        }
    }
}

