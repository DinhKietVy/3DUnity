using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Camera Position")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 3f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 1.5f;
    [SerializeField] private float mouseSmooth = 8f;

    [Header("Touch")]
    [SerializeField] private float touchSensitivity = 0.08f;
    [SerializeField] private float touchSmooth = 12f;

    [Header("Vertical Rotation")]
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 45f;

    [Header("Camera Smooth")]
    [SerializeField] private float positionSmooth = 8f;
    [SerializeField] private float rotationSmooth = 10f;

    private float yaw;
    private float pitch = 15f;

    private float currentMouseX;
    private float currentMouseY;

    private float mouseXVelocity;
    private float mouseYVelocity;

    private Vector2 touchRotation;

    private void Start()
    {
        yaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouse();
        HandleCursor();
    }

    private void LateUpdate()
    {
        FollowPlayer();

        // Reset touch rotation sau mỗi frame
        touchRotation = Vector2.zero;
    }

    private void HandleMouse()
    {
        // Khi đang sử dụng mobile thì không xử lý chuột
        if (Input.touchCount > 0)
            return;

        float targetMouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        float targetMouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;

        currentMouseX = Mathf.SmoothDamp(
            currentMouseX,
            targetMouseX,
            ref mouseXVelocity,
            1f / mouseSmooth
        );

        currentMouseY = Mathf.SmoothDamp(
            currentMouseY,
            targetMouseY,
            ref mouseYVelocity,
            1f / mouseSmooth
        );

        yaw += currentMouseX;

        pitch -= currentMouseY;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );
    }

    public void RotateByTouch(Vector2 delta)
    {
        float x = delta.x * touchSensitivity;
        float y = delta.y * touchSensitivity;

        // Smooth touch
        touchRotation = Vector2.Lerp(
            touchRotation,
            new Vector2(x, y),
            touchSmooth * Time.deltaTime
        );

        yaw += touchRotation.x;

        pitch -= touchRotation.y;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );
    }

    private void FollowPlayer()
    {
        if (player == null)
            return;

        Quaternion targetRotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        Vector3 targetPosition =
            player.position +
            targetRotation *
            new Vector3(
                0f,
                height,
                -distance
            );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmooth * Time.deltaTime
        );

        Vector3 lookTarget =
            player.position +
            Vector3.up * 1.5f;

        Quaternion lookRotation =
            Quaternion.LookRotation(
                lookTarget - transform.position
            );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}