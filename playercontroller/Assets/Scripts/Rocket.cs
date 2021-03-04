using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] float maxDistance = 5f;
    [SerializeField] float maxPower = 20f;

    Rigidbody rb = null;
    float speed = 0f;
    float radius = 0.05f;
    float length = 0.3f;

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb is null) rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        bool isSomethingInFront = Physics.SphereCast(
            transform.position + transform.forward * (length / 2f - radius),
            radius,
            transform.forward,
            out hit,
            speed * Time.fixedDeltaTime,
            ~0,
            QueryTriggerInteraction.Ignore
        );
        if (isSomethingInFront)
        {
            rb.MovePosition(rb.position + transform.forward * hit.distance);
            Explode(hit.point);
        }
        else
        {
            rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
        }

        if (Vector3.Distance(Vector3.zero, rb.position) > 500f)
        {
            Destroy(gameObject);
        }
    }

    void Explode(Vector3 explosionPosition)
    {
        ForceModifier[] forceModifiers = FindObjectsOfType<ForceModifier>();
        foreach (ForceModifier forceModifier in forceModifiers)
        {
            Rigidbody rb = forceModifier.GetComponent<Rigidbody>();
            Vector3 center = rb.worldCenterOfMass;

            Collider collider = forceModifier.GetComponent<Collider>();
            Vector3 closestPoint = collider.ClosestPoint(explosionPosition);

            Vector3 direction = ((center - explosionPosition) * 100f).normalized;
            float distance = Vector3.Distance(explosionPosition, closestPoint);
            float power = Mathf.Max(0f, (maxPower * (maxDistance - distance)) / (maxDistance * (distance + 1f)));

            forceModifier.AddForce(direction * power);
        }

        Destroy(gameObject);
    }
}