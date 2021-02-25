using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DynamicRbMovement : MonoBehaviour
{

    #region Variables

    private Controls _controls;
    private bool _isJumpButtonDown = false;
    private Vector2 _mouseDelta = Vector2.zero;

    private Rigidbody _rb = null;

    [Header("Collider")]
    [SerializeField] private float _height = 2f;
    [SerializeField] private float _radius = 0.5f;
    [SerializeField] private PhysicMaterial _noFrictionPhysicMaterial = null;
    private CapsuleCollider _collider = null;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _maxAcceleration = 10f;
    [SerializeField] private float _maxAccelerationInAir = 6f;
    [SerializeField] private float _friction = 0.1f;
    [SerializeField] private float _jumpHeight = 1f;
    [SerializeField] private float _gravity = -19.62f;
    [SerializeField] private float _slopeLimit = 50f;
    [SerializeField] private float _maxStepUpHeight = 0.3f;
    [SerializeField] private float _maxStepDownHeight = 0.4f;
    private bool _isGrounded = false;
    private bool _wasGrounded = false;

    [Header("Mouse Look")]
    [SerializeField] private float _mouseSensitivity = 0.05f;
    [SerializeField] private float _minVerticalRotation = -90f;
    [SerializeField] private float _maxVerticalRotation = 90f;
    [SerializeField] private float _cameraOffset = 1.5f;
    private float _horizontalRotation = 0f;
    private float _verticalRotation = 0f;
    private Transform _cameraHolder = null;
    private Camera _camera = null;

    #endregion



    #region Awake

    private void Awake()
    {
        SetupControls();
        SetupRigidbody();
        SetupCollider();
        SetupCamera();
    }

    private void SetupControls()
    {
        _controls = new Controls();

        _controls.Gameplay.JumpDown.performed += _ => _isJumpButtonDown = true;
        _controls.Gameplay.JumpUp.performed += _ => _isJumpButtonDown = false;
    }

    private void SetupRigidbody()
    {
        _rb = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();

        _rb.mass = 75f;
        _rb.drag = 0f;
        _rb.angularDrag = 0f;
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void SetupCollider()
    {
        _collider = GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>() : gameObject.AddComponent<CapsuleCollider>();

        _collider.height = _height;
        _collider.radius = _radius;
        _collider.center = Vector3.up * _height / 2f;

        _collider.material = _noFrictionPhysicMaterial;
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

    #endregion



    #region OnEnable, OnDisable

    private void OnEnable() => _controls.Enable();
    private void OnDisable() => _controls.Disable();

    #endregion



    #region Start

    private void Start()
    {
        LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion



    #region FixedUpdate

    private void FixedUpdate()
    {
        Look();

        Vector3 previousVelocity = _rb.velocity;
        if (_isGrounded)
            previousVelocity.y = 0f;

        Vector3 newVelocity = previousVelocity;

        if (!_isGrounded)
            newVelocity = ApplyGravity(newVelocity, Time.fixedDeltaTime);

        if (_isGrounded && _isJumpButtonDown)
        {
            _isGrounded = false;
            newVelocity = Jump(newVelocity, Time.fixedDeltaTime);
        }

        if (_isGrounded)
            newVelocity = ApplyFriction(newVelocity, Time.fixedDeltaTime);

        Vector2 moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 wishVel = wishDir * _moveSpeed;
        newVelocity = Accelerate(newVelocity, wishVel, _isGrounded, Time.fixedDeltaTime);

        _rb.velocity = newVelocity;

        _isGrounded = false;
    }

    private void Look()
    {
        _horizontalRotation += _mouseDelta.x * _mouseSensitivity;
        transform.localEulerAngles = Vector3.up * _horizontalRotation;

        _verticalRotation = Mathf.Clamp(_verticalRotation - _mouseDelta.y * _mouseSensitivity, _minVerticalRotation, _maxVerticalRotation);
        _cameraHolder.localEulerAngles = Vector3.right * _verticalRotation;

        _mouseDelta = Vector2.zero;
    }

    private Vector3 ApplyGravity(Vector3 currentVel, float deltaTime)
    {
        return currentVel + Vector3.up * _gravity * Time.fixedDeltaTime;
    }

    private Vector3 Jump(Vector3 currentVel, float deltaTime)
    {
        return currentVel + Vector3.up * Mathf.Sqrt(_jumpHeight * 2f * -_gravity);
    }

    private Vector3 ApplyFriction(Vector3 currentVel, float deltaTime)
    {
        Vector3 horVel = new Vector3(currentVel.x, 0f, currentVel.z);

        float speed = horVel.magnitude;
        if (speed != 0f)
        {
            float drop = speed / _friction * deltaTime;
            Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;
            return new Vector3(newHorVel.x, currentVel.y, newHorVel.z);
        }

        return currentVel;
    }

    private Vector3 Accelerate(Vector3 currentVel, Vector3 wishVel, bool isGrounded, float deltaTime)
    {
        if (isGrounded)
        {
            Vector3 horVel = new Vector3(currentVel.x, 0f, currentVel.z);
            Vector3 horVelAfterAcceleration = horVel + wishVel * _maxAcceleration * Time.deltaTime;
            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, _moveSpeed);
            return new Vector3(
                clampedHorVelAfterAcceleration.x,
                currentVel.y,
                clampedHorVelAfterAcceleration.z
            );
        }
        else
        {
            Vector3 horVel = new Vector3(currentVel.x, 0f, currentVel.z);
            float speed = horVel.magnitude;
            float currentSpeedInWishDir = Vector3.Dot(horVel, wishVel);
            float addSpeed = Mathf.Clamp(_moveSpeed - currentSpeedInWishDir, 0f, _maxAccelerationInAir * Time.deltaTime);
            Vector3 horVelAfterAcceleration = horVel + wishVel * addSpeed;

            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(speed, _moveSpeed));
            return new Vector3(
                clampedHorVelAfterAcceleration.x,
                currentVel.y,
                clampedHorVelAfterAcceleration.z
            );
        }
    }

    #endregion



    #region OnCollision

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) <= _slopeLimit)
            {
                _isGrounded = true;
            }
        }
    }

    #endregion



    #region Update

    private void Update()
    {
        _mouseDelta += _controls.Gameplay.Look.ReadValue<Vector2>();
    }

    #endregion

}