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
    [SerializeField] Transform cameraHolder;
    bool isCrouchDown = false;
    bool isCrouching = false;
    float currentHeight;
    float heightVelocity = 0f;
    float speedVelocity = 0f;
    float currentMaxMoveSpeed;

    CapsuleCollider playerCol = null;
    Rigidbody playerRb = null;

    bool isGrounded = false;
    bool wasGrounded = false;

    IMovementModifier[] movementModifiers;

    void OnEnable()
    {
        crouchStart.Enable();
        crouchEnd.Enable();
    }

    void OnDisable()
    {
        crouchStart.Disable();
        crouchEnd.Disable();
    }

    void Start()
    {
        crouchStart.performed += _ => isCrouchDown = true;
        crouchEnd.performed += _ => isCrouchDown = false;

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

        movementModifiers = GetComponents<IMovementModifier>();

        config.maxStepUpHeight = Mathf.Min(config.maxStepUpHeight, config.radius);
    }

    void FixedUpdate()
    {
        Crouching();

        Vector3 newVelocity = playerRb.velocity;
        if (isGrounded) newVelocity.y = 0f;

        ModifierInfo info = new ModifierInfo()
        {
            IsGrounded = isGrounded,
            CurrentVelocity = playerRb.velocity,
            CurrentHorizontalVelocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z),
            CurrentMaxMoveSpeed = currentMaxMoveSpeed,
            IsCrouching = isCrouching
        };

        foreach (IMovementModifier modifier in movementModifiers) { newVelocity += modifier.Modify(info, config); }

        if (newVelocity.y > 0f)
        {
            isGrounded = false;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        playerRb.velocity = newVelocity;

        PreparePhysics();

        isGrounded = false;

        StartCoroutine(AfterPhysics());
    }

    void Crouching()
    {
        SetIsCrouching();
        SetCurrentHeight();
        SetCurrentSpeed();
        cameraHolder.transform.localPosition = Vector3.up * (currentHeight - 0.5f);

        void SetIsCrouching()
        {
            if (isCrouchDown)
            {
                isCrouching = true;
            }
            else if (isCrouching)
            {
                isCrouching = false;

                Collider[] colliders = Physics.OverlapCapsule(
                    transform.position + Vector3.up * config.radius,
                    transform.position + Vector3.up * (config.height - config.radius),
                    config.radius - 0.01f,
                    ~0,
                    QueryTriggerInteraction.Ignore
                );
                foreach (Collider collider in colliders)
                {
                    if (collider.transform != transform) isCrouching = true;
                }
            }
        }

        void SetCurrentHeight()
        {
            float targetHeight = isCrouching ? config.height * config.crouchHeightMultiplier : config.height;
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, config.crouchTransitionTime);
        }

        void SetCurrentSpeed()
        {
            float targetSpeed = isCrouching ? config.speed * config.crouchMoveSpeedMultiplier : config.speed;
            currentMaxMoveSpeed = Mathf.SmoothDamp(currentMaxMoveSpeed, targetSpeed, ref speedVelocity, config.crouchTransitionTime);
        }
    }

    void PreparePhysics()
    {
        SetColliderHeight(currentHeight);
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        if (!isGrounded) return;

        playerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (!ShouldStepUp()) return;

        SetColliderHeight(currentHeight - config.maxStepUpHeight);


        void SetColliderHeight(float height)
        {
            playerCol.height = height;
            playerCol.center = Vector3.up * (currentHeight - height / 2f);
        }

        bool ShouldStepUp()
        {
            RaycastHit hit;
            if (!IsObjectInFront(out hit)) return false;

            Rigidbody otherRb = hit.transform.GetComponent<Rigidbody>();
            if (otherRb != null) return false;

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
            if (Vector3.Angle(contact.normal, Vector3.up) <= config.slopeLimit) isGrounded = true;
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

            if (Vector3.Angle(hit.normal, Vector3.up) > config.slopeLimit) return;

            transform.position = transform.position + Vector3.up * (config.maxStepUpHeight + margin - hit.distance);
            isGrounded = true;
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

            if (Vector3.Angle(hit.normal, Vector3.up) > config.slopeLimit) return;

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
        }
    }
}