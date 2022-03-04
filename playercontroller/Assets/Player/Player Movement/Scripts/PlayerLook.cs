using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] PlayerLookConfig config;
    [Space]
    [SerializeField] Transform orientation = null;
    [SerializeField] Transform camPos = null;
    [SerializeField] Transform pivot = null;
    [SerializeField] Camera cam = null;

    Vector2 input = Vector2.zero;
    float horizontalRotation = 0f;
    float verticalRotation = 0f;

    void Start()
    {
        cam.fieldOfView = config.fieldOfView;

        LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnLook(InputValue value) { input = value.Get<Vector2>(); }

    void Update()
    {
        transform.position = camPos.position;

        horizontalRotation += input.x * config.mouseSensitivity;
        orientation.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        verticalRotation = Mathf.Clamp(verticalRotation - input.y * config.mouseSensitivity, config.minVerticalRotation, config.maxVerticalRotation);
        pivot.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}
