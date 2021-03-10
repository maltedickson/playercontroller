using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] PlayerMoverConfig config = null;
    [Space]
    [SerializeField] InputAction crouchStart;
    [SerializeField] InputAction crouchEnd;
    [Space]
    [SerializeField] InputAction jumpStart;
    [SerializeField] InputAction jumpEnd;
    bool wantsToJump = false;
    [Space]
    [SerializeField] InputAction moveInputAction;
    Vector2 moveInput = Vector2.zero;
    [Space]
    [SerializeField] Transform cameraHolder;

    bool isCrouchDown = false;
    float speedVelocity = 0f;
    float currentMaxMoveSpeed;

    CapsuleCollider playerCol = null;
    Rigidbody playerRb = null;

    bool isGrounded = false;
    bool wasGrounded = false;

    Crouching crouching;

    PlayerGravity playerGravity;
    PlayerJump playerJump;
    PlayerFriction playerFriction;
    PlayerAcceleration playerAcceleration;
    PlayerForce playerForce;

    // MovingPlatform previousMovingPlatform;
    MovingPlatform movingPlatform;
    Vector3 previousMovingPlatformVelocity = Vector3.zero;

    public void AddForce(Vector3 force)
    {
        playerForce.AddForce(force);
    }

    void OnEnable()
    {
        crouchStart.Enable();
        crouchEnd.Enable();

        jumpStart.Enable();
        jumpEnd.Enable();

        moveInputAction.Enable();
    }

    void OnDisable()
    {
        crouchStart.Disable();
        crouchEnd.Disable();

        jumpStart.Disable();
        jumpEnd.Disable();

        moveInputAction.Disable();
    }

    void Start()
    {
        crouchStart.performed += _ => isCrouchDown = true;
        crouchEnd.performed += _ => isCrouchDown = false;

        jumpStart.performed += _ => wantsToJump = true;
        jumpEnd.performed += _ => wantsToJump = false;

        crouching = new Crouching(config.height, config.height * config.crouchHeightMultiplier, config.crouchTransitionTime);

        playerGravity = new PlayerGravity(config.gravity);
        playerJump = new PlayerJump(config.jumpHeight, config.gravity);
        playerFriction = new PlayerFriction(config.friction);
        playerAcceleration = new PlayerAcceleration(config.groundAcceleration, config.airAcceleration, transform);
        playerForce = new PlayerForce();

        Initialize();
    }

    void Initialize()
    {
        playerRb = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
        playerRb.mass = 75f;
        playerRb.drag = 0f;
        playerRb.angularDrag = 0f;
        playerRb.useGravity = false;
        playerRb.isKinematic = false;
        playerRb.interpolation = RigidbodyInterpolation.None;
        playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        playerCol = GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>() : gameObject.AddComponent<CapsuleCollider>();
        playerCol.height = config.height;
        playerCol.radius = config.radius;
        playerCol.center = Vector3.up * config.height / 2f;
        playerCol.material = config.noFrictionPhysicMaterial;

        config.maxStepUpHeight = Mathf.Min(config.maxStepUpHeight, config.radius);
    }

    void FixedUpdate()
    {
        MovePlatforms();
        MoveWithPlatform();
        // previousMovingPlatform = movingPlatform;
        previousMovingPlatformVelocity = movingPlatform != null ? movingPlatform.movement / movingPlatform.deltaTime : Vector3.zero;

        Crouching();

        SetCurrentSpeed();

        Vector3 newVelocity = playerRb.velocity;
        if (isGrounded) newVelocity.y = 0f;

        newVelocity = UpdateVelocity(newVelocity);

        if (newVelocity.y > 0f)
        {
            isGrounded = false;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        playerRb.velocity = newVelocity;

        PreparePhysics();

        isGrounded = false;

        StartCoroutine(AfterPhysics());

        movingPlatform = null;
    }

    void Crouching()
    {
        crouching.SetIsCrouching(isCrouchDown, transform, config.radius);
        crouching.UpdateHeight();
        cameraHolder.transform.localPosition = Vector3.up * (crouching.currentHeight - 0.5f);
    }

    void SetCurrentSpeed()
    {
        float targetSpeed = crouching.isCrouching ? config.speed * config.crouchMoveSpeedMultiplier : config.speed;
        currentMaxMoveSpeed = Mathf.SmoothDamp(currentMaxMoveSpeed, targetSpeed, ref speedVelocity, config.crouchTransitionTime);
    }

    Vector3 UpdateVelocity(Vector3 velocity)
    {
        velocity += playerGravity.ApplyGravity(isGrounded);
        velocity += playerJump.Jump(wantsToJump, isGrounded, crouching.isCrouching);
        velocity += playerFriction.ApplyFriction(velocity, isGrounded);
        velocity += playerAcceleration.Accelerate(moveInput.normalized, velocity, currentMaxMoveSpeed, isGrounded);
        moveInput = Vector2.zero;
        velocity += playerForce.ApplyForce();
        return velocity;
    }

    void PreparePhysics()
    {
        SetColliderHeight(crouching.currentHeight);
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        if (!isGrounded) return;
        playerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        if (ShouldStepUp()) SetColliderHeight(crouching.currentHeight - config.maxStepUpHeight);

        void SetColliderHeight(float height)
        {
            playerCol.height = height;
            playerCol.center = Vector3.up * (crouching.currentHeight - height / 2f);
        }

        bool ShouldStepUp()
        {
            RaycastHit hit;
            if (!IsObjectInFront(out hit)) return false;

            Rigidbody otherRb = hit.transform.GetComponent<Rigidbody>();
            if (otherRb != null && !otherRb.isKinematic) return false;

            RaycastHit groundHit;
            if (!WouldBeInGround(out groundHit)) return false;

            bool isObstacleTooHigh = groundHit.point.y - transform.position.y > config.maxStepUpHeight;
            if (isObstacleTooHigh) return false;

            return true;


            bool IsObjectInFront(out RaycastHit hit)
            {
                Vector3 moveDir = playerRb.velocity.normalized;

                Vector3 checkStart = transform.position + moveDir * -0.1f;

                return Physics.CapsuleCast(
                    checkStart + Vector3.up * config.radius,
                    checkStart + Vector3.up * (config.height - config.radius),
                    config.radius,
                    moveDir,
                    out hit,
                    playerRb.velocity.magnitude * Time.fixedDeltaTime + 0.1f,
                    ~0,
                    QueryTriggerInteraction.Ignore
                );
            }

            bool WouldBeInGround(out RaycastHit hit)
            {
                Vector3 newPos = transform.position + playerRb.velocity * Time.fixedDeltaTime;
                float margin = 0.01f;
                return Physics.SphereCast(
                    newPos + Vector3.up * (config.maxStepUpHeight + config.radius + margin),
                    config.radius,
                    Vector3.down,
                    out hit,
                    config.maxStepUpHeight + margin,
                    ~0,
                    QueryTriggerInteraction.Ignore
                );
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (playerRb.velocity.y > 0f && wasGrounded) return;

        foreach (ContactPoint contact in other.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) <= config.slopeLimit)
            {
                isGrounded = true;
                movingPlatform = GetMovingPlatform(other.transform);
            }
        }
    }

    IEnumerator AfterPhysics()
    {
        yield return new WaitForFixedUpdate();

        MoveUp();
        StickToGround();
        wasGrounded = isGrounded;

        void MoveUp()
        {
            float margin = 0.01f;
            RaycastHit hit;
            bool isInGround = Physics.SphereCast(
                transform.position + Vector3.up * (config.maxStepUpHeight + config.radius + margin),
                config.radius,
                Vector3.down,
                out hit,
                config.maxStepUpHeight + margin,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            if (!isInGround) return;

            if (hit.point.y - transform.position.y > config.maxStepUpHeight) return;

            transform.position = transform.position + Vector3.up * (config.maxStepUpHeight + margin - hit.distance);
            isGrounded = true;
            if (movingPlatform == null) movingPlatform = GetMovingPlatform(hit.transform);
        }

        void StickToGround()
        {
            if (!wasGrounded || playerRb.velocity.y > 0f) return;

            float margin = 0.01f;
            RaycastHit hit;
            bool isGroundBelow = Physics.SphereCast(
                transform.position + Vector3.up * (config.radius + margin),
                config.radius,
                Vector3.down,
                out hit,
                config.maxStepDownHeight + margin,
                ~0,
                QueryTriggerInteraction.Ignore
            );
            bool isGroundBelowCenter = Physics.Raycast(
                transform.position + Vector3.up * margin,
                Vector3.down,
                config.maxStepDownHeight + margin,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            if (!isGroundBelow || !isGroundBelowCenter) return;

            Vector3 newPos = transform.position + Vector3.down * Mathf.Max(0f, (hit.distance * 0.9f - margin));
            Collider[] collidersAtNewPos = Physics.OverlapCapsule(
                newPos + Vector3.up * config.radius,
                newPos + Vector3.up * (config.height - config.radius),
                config.radius,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            if (collidersAtNewPos.Length > 1) return;

            transform.position = newPos;
            isGrounded = true;
            if (movingPlatform == null) movingPlatform = GetMovingPlatform(hit.transform);
        }
    }

    void MovePlatforms()
    {
        MovingPlatform[] platforms = FindObjectsOfType<MovingPlatform>();
        foreach (MovingPlatform platform in platforms) platform.Move();
    }

    void MoveWithPlatform()
    {
        if (movingPlatform == null)
        {
            if (previousMovingPlatformVelocity != Vector3.zero)
            {
                AddForce(previousMovingPlatformVelocity);
            }
            return;
        }

        transform.position = transform.position + movingPlatform.movement;
    }

    MovingPlatform GetMovingPlatform(Transform transform)
    {
        MovingPlatform platform = null;

        while (transform != null && platform == null)
        {
            platform = transform.GetComponent<MovingPlatform>();
            transform = transform.parent;
        }

        return platform;
    }

    void Update()
    {
        moveInput += moveInputAction.ReadValue<Vector2>();
    }
}