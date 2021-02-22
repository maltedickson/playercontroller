using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterMover))]
public class CharacterControllerExample : MonoBehaviour
{

    private Controls _controls;
    private CharacterMover _mover = null;

    [Header("Ground Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _maxAcceleration = 10f;
    [SerializeField] private float _friction = 0.2f;

    [Header("Air Movement")]
    [SerializeField] private float _gravity = -19.62f;
    [SerializeField] private float _maxAccelerationInAir = 6f;

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.1f;
    private bool _isJumpButtonDown = false;

    [Header("Mouse Look")]
    [SerializeField] private float _mouseSensitivity = 0.05f;
    [SerializeField] private float _minVerticalRotation = -90f;
    [SerializeField] private float _maxVerticalRotation = 90f;
    [SerializeField] private float _cameraOffset = 1.5f;
    private float _horizontalRotation = 0f;
    private float _verticalRotation = 0f;
    private Transform _cameraHolder = null;
    private Camera _camera = null;

    private void Awake()
    {
        _mover = GetComponent<CharacterMover>();

        SetupControls();
        SetupCamera();
    }

    private void SetupControls()
    {
        _controls = new Controls();

        _controls.Gameplay.JumpDown.performed += _ => _isJumpButtonDown = true;
        _controls.Gameplay.JumpUp.performed += _ => _isJumpButtonDown = false;
    }

    private void SetupCamera()
    {
        _cameraHolder = transform.Find("Camera Holder");
        if (_cameraHolder == null)
            _cameraHolder = new GameObject("Camera Holder").transform;

        _cameraHolder.parent = transform;
        _cameraHolder.localPosition = Vector3.up * _cameraOffset;
        _cameraHolder.localEulerAngles = Vector3.zero;

        GameObject cameraObject;

        if (_cameraHolder.Find("Camera") == null)
            cameraObject = new GameObject("Camera");
        else
            cameraObject = _cameraHolder.Find("Camera").gameObject;

        cameraObject.transform.parent = _cameraHolder;
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localEulerAngles = Vector3.zero;

        _camera = cameraObject.GetComponent<Camera>();
        if (_camera == null)
            _camera = cameraObject.AddComponent<Camera>();

        _camera.fieldOfView = 90f;
        _camera.nearClipPlane = 0.01f;
        _camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

        if (cameraObject.GetComponent<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();
    }

    private void OnEnable() => _controls.Enable();
    private void OnDisable() => _controls.Disable();

    private void Start()
    {
        LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Vector3 velocity = _mover.Velocity;
        if (_mover.IsGrounded)
            velocity.y = 0f;
        Vector3 newVelocity = velocity;
        ApplyGravity(ref newVelocity);
        Jump(ref newVelocity);
        ApplyFriction(ref newVelocity);
        Accelerate(ref newVelocity);
        _mover.Move(newVelocity);
    }

    private void Look()
    {
        Vector2 mouseDelta = _controls.Gameplay.Look.ReadValue<Vector2>();

        _horizontalRotation += mouseDelta.x * _mouseSensitivity;
        transform.localEulerAngles = Vector3.up * _horizontalRotation;

        _verticalRotation = Mathf.Clamp(_verticalRotation - mouseDelta.y * _mouseSensitivity, _minVerticalRotation, _maxVerticalRotation);
        _cameraHolder.localEulerAngles = Vector3.right * _verticalRotation;
    }

    private void ApplyGravity(ref Vector3 velocity)
    {
        velocity += Vector3.up * _gravity * Time.deltaTime;
    }

    private void Jump(ref Vector3 velocity)
    {
        if (_isJumpButtonDown && _mover.IsGrounded)
        {
            velocity.y = Mathf.Sqrt(_jumpHeight * 2f * -_gravity);
        }
    }

    private void ApplyFriction(ref Vector3 velocity)
    {
        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

        if (_mover.IsGrounded)
        {
            float speed = horVel.magnitude;
            if (speed != 0f)
            {
                float drop = speed / _friction * Time.deltaTime;
                Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;
                velocity = new Vector3(newHorVel.x, velocity.y, newHorVel.z);
            }
        }
    }

    private void Accelerate(ref Vector3 velocity)
    {
        Vector2 moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 wishVel = wishDir * _moveSpeed;

        if (_mover.IsGrounded)
        {
            Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 horVelAfterAcceleration = horVel + wishVel * _maxAcceleration * Time.deltaTime;
            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, _moveSpeed);
            velocity = new Vector3(
                clampedHorVelAfterAcceleration.x,
                velocity.y,
                clampedHorVelAfterAcceleration.z
            );
        }
        else
        {
            Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);
            float speed = horVel.magnitude;
            float currentSpeedInWishDir = Vector3.Dot(horVel, wishVel);
            float addSpeed = Mathf.Clamp(_moveSpeed - currentSpeedInWishDir, 0f, _maxAccelerationInAir * Time.deltaTime);
            Vector3 horVelAfterAcceleration = horVel + wishVel * addSpeed;

            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(speed, _moveSpeed));
            velocity = new Vector3(
                clampedHorVelAfterAcceleration.x,
                velocity.y,
                clampedHorVelAfterAcceleration.z
            );
        }
    }

}
