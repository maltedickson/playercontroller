using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{

    [SerializeField] PlayerLookConfig config;
    [Space]
    [SerializeField] InputAction lookInput;
    Vector2 input = Vector2.zero;

    float horizontalRotation = 0f;
    float verticalRotation = 0f;

    Transform cameraHolder = null;
    GameObject cameraObject = null;
    Camera cam = null;

    void OnEnable() => lookInput.Enable();
    void OnDisable() => lookInput.Disable();

    void Awake()
    {
        CreateCameraHolder();
        CreateCameraObject();
        CreateCamera();
        CreateAudioListener();
    }

    void CreateCameraHolder()
    {
        cameraHolder = transform.Find("Camera Holder");
        if (cameraHolder == null)
            cameraHolder = new GameObject("Camera Holder").transform;
    }

    void CreateCameraObject()
    {
        if (cameraHolder.Find("Camera") == null)
            cameraObject = new GameObject("Camera");
        else
            cameraObject = cameraHolder.Find("Camera").gameObject;
    }

    void CreateCamera()
    {
        cam = cameraObject.GetComponent<Camera>();
        if (cam == null)
            cam = cameraObject.AddComponent<Camera>();
    }

    void CreateAudioListener()
    {
        if (cameraObject.GetComponent<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();
    }

    void Start()
    {
        SetupCameraHolder();
        SetupCameraObject();
        SetupCamera();

        LockCursor();
    }

    void SetupCameraHolder()
    {
        cameraHolder.parent = transform;
        cameraHolder.localPosition = Vector3.up * config.cameraOffset;
        cameraHolder.localEulerAngles = Vector3.zero;
    }

    void SetupCameraObject()
    {
        cameraObject.transform.parent = cameraHolder;
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localEulerAngles = Vector3.zero;
    }

    void SetupCamera()
    {
        cam.fieldOfView = 90f;
        cam.nearClipPlane = 0.01f;
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        Look();
    }

    void Look()
    {
        horizontalRotation += input.x * config.mouseSensitivity;
        transform.localEulerAngles = Vector3.up * horizontalRotation;

        verticalRotation = Mathf.Clamp(verticalRotation - input.y * config.mouseSensitivity, config.minVerticalRotation, config.maxVerticalRotation);
        cameraHolder.localEulerAngles = Vector3.right * verticalRotation;

        input = Vector2.zero;
    }

    void Update()
    {
        input += lookInput.ReadValue<Vector2>();
    }

}
