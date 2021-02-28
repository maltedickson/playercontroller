using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLook : MonoBehaviour
{

    [SerializeField] PlayerLookConfig config;

    float horizontalRotation = 0f;
    float verticalRotation = 0f;

    Transform cameraHolder = null;
    GameObject cameraObject = null;
    Camera cam = null;

    Controls controls;
    Vector2 mouseDelta = Vector2.zero;

    public void TakeInput(Vector2 mouseDelta)
    {
        this.mouseDelta += mouseDelta;
    }

    void Awake()
    {
        controls = new Controls();

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

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

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
        horizontalRotation += mouseDelta.x * config.mouseSensitivity;
        transform.localEulerAngles = Vector3.up * horizontalRotation;

        verticalRotation = Mathf.Clamp(verticalRotation - mouseDelta.y * config.mouseSensitivity, config.minVerticalRotation, config.maxVerticalRotation);
        cameraHolder.localEulerAngles = Vector3.right * verticalRotation;

        mouseDelta = Vector2.zero;
    }

    void Update()
    {
        TakeInput(controls.Gameplay.Look.ReadValue<Vector2>());
    }

}
