using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] PlayerMoverConfig config = null;

    CapsuleCollider collider = null;
    Rigidbody rb = null;

    [HideInInspector] public Vector3 velocity { get; private set; }

    [HideInInspector] public bool isGrounded { get; private set; }
    bool wasGrounded = false;

    bool wasFixedUpdateThisFrame = false;

    void Awake()
    {
        CreateRigidbody();
        CreateCollider();
    }

    void CreateRigidbody()
    {
        rb = GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
    }

    void CreateCollider()
    {
        collider = GetComponent<CapsuleCollider>() ? GetComponent<CapsuleCollider>() : gameObject.AddComponent<CapsuleCollider>();
    }

    void Start()
    {
        SetupRigidbody();
        SetupCollider();
        LimitMaxStepUpHeight();
    }

    void SetupRigidbody()
    {
        rb.mass = 75f;

        rb.drag = 0f;
        rb.angularDrag = 0f;

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void SetupCollider()
    {
        collider.height = config.height;
        collider.radius = config.radius;

        collider.center = Vector3.up * config.height / 2f;

        collider.material = config.noFrictionPhysicMaterial;
    }

    void LimitMaxStepUpHeight()
    {
        config.maxStepUpHeight = Mathf.Min(config.maxStepUpHeight, config.radius);
    }

    public void Move(Vector3 newVelocity)
    {
        wasFixedUpdateThisFrame = true;

        velocity = newVelocity;
        rb.velocity = velocity;

        if (velocity.y > 0f)
        {
            isGrounded = false;
        }

        PreparePhysics();

        isGrounded = false;
    }

    void PreparePhysics()
    {
        SetColliderHeight(config.height);
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (!isGrounded) return;

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (!ShouldStepUp()) return;

        SetColliderHeight(config.height - config.maxStepUpHeight);


        void SetColliderHeight(float height)
        {
            collider.height = height;
            collider.center = Vector3.up * (config.height - height / 2f);
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
                Vector3 moveDir = rb.velocity.normalized;

                Vector3 checkStart = transform.position + moveDir * -0.1f;

                return Physics.CapsuleCast(
                    checkStart + Vector3.up * config.radius,
                    checkStart + Vector3.up * (config.height - config.radius),
                    config.radius,
                    moveDir,
                    out hit,
                    rb.velocity.magnitude * Time.fixedDeltaTime + 0.1f,
                    ~0,
                    QueryTriggerInteraction.Ignore
                );
            }

            bool WouldBeInGround(out RaycastHit hit)
            {
                Vector3 newPos = transform.position + rb.velocity * Time.fixedDeltaTime;
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
        if (velocity.y > 0f) return;

        foreach (ContactPoint contact in other.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) <= config.slopeLimit)
            {
                isGrounded = true;
            }
        }
    }

    void Update()
    {
        if (!wasFixedUpdateThisFrame) return;
        wasFixedUpdateThisFrame = false;

        MoveUp();

        StickToGround();
        wasGrounded = isGrounded;
    }

    void MoveUp()
    {
        if (velocity.y > 0f) return;

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
    }

    void StickToGround()
    {
        if (!wasGrounded || velocity.y > 0f) return;

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
    }
}