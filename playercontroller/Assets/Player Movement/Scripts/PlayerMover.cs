using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{

    [SerializeField] PlayerMoverConfig config = null;

    CapsuleCollider collider = null;
    Rigidbody rb = null;

    [HideInInspector] public Vector3 Velocity = Vector3.zero;

    [HideInInspector] public bool IsGrounded = false;
    bool wasGrounded = false;

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

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
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
        rb.velocity = newVelocity;

        PreparePhysics();

        IsGrounded = false;
    }

    void PreparePhysics()
    {
        SetColliderHeight(config.height);

        if (IsGrounded)
        {
            rb.constraints
                = RigidbodyConstraints.FreezeRotation
                | RigidbodyConstraints.FreezePositionY;

            RaycastHit hit;
            Vector3 moveDir = rb.velocity.normalized;
            Vector3 checkStart = transform.position + moveDir * -0.1f;
            bool isObjectInFront = Physics.CapsuleCast(
                checkStart + Vector3.up * config.radius,
                checkStart + Vector3.up * (config.height - config.radius),
                config.radius,
                moveDir,
                out hit,
                rb.velocity.magnitude * Time.fixedDeltaTime + 0.1f,
                ~0,
                QueryTriggerInteraction.Ignore
            );
            if (isObjectInFront)
            {
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    Vector3 newPos = transform.position + this.rb.velocity * Time.fixedDeltaTime;
                    float margin = 0.01f;
                    RaycastHit groundHit;
                    bool wouldBeInGround = Physics.SphereCast(
                        newPos + Vector3.up * (config.maxStepUpHeight + config.radius + margin),
                        config.radius,
                        Vector3.down,
                        out groundHit,
                        config.maxStepUpHeight + margin,
                        ~0,
                        QueryTriggerInteraction.Ignore
                    );
                    if (wouldBeInGround)
                    {
                        if (groundHit.point.y - transform.position.y <= config.maxStepUpHeight)
                        {
                            SetColliderHeight(config.height - config.maxStepUpHeight);
                        }
                    }
                }
            }
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void SetColliderHeight(float height)
    {
        collider.height = height;
        collider.center = Vector3.up * (config.height - height / 2f);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) <= config.slopeLimit)
            {
                IsGrounded = true;
            }
        }
    }

    void Update()
    {
        MoveUp();

        StickToGround();
        wasGrounded = IsGrounded;
    }

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

        if (isInGround)
        {
            if (hit.point.y - transform.position.y <= config.maxStepUpHeight)
            {
                transform.position = transform.position + Vector3.up * (config.maxStepUpHeight + margin - hit.distance);
                IsGrounded = true;
            }
        }
    }

    void StickToGround()
    {
        if (!wasGrounded || rb.velocity.y > 0f) return;

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
        IsGrounded = true;
    }

}
