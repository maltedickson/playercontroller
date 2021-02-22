using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMover : MonoBehaviour
{

    public Vector3 Velocity { get; private set; }
    public bool IsGrounded { get; private set; }
    private bool _wasGrounded;

    [SerializeField] private float _height = 2f;
    [SerializeField] private float _radius = 0.5f;
    private Transform _colliderTransform = null;
    private CapsuleCollider _collider = null;
    private Rigidbody _rb = null;

    [SerializeField] private float _maxStepUpHeight = 0.3f;
    [SerializeField] private float _maxStepDownHeight = 0.4f;

    private void Awake()
    {
        SetupCollider();
        SetupRigidbody();
    }

    private void SetupCollider()
    {
        _colliderTransform = transform.Find("Collider Object");
        if (_colliderTransform == null)
            _colliderTransform = new GameObject("Collider Object").transform;

        _colliderTransform.gameObject.layer = gameObject.layer;
        _colliderTransform.SetParent(transform);
        _colliderTransform.rotation = Quaternion.identity;
        _colliderTransform.localPosition = Vector3.zero;
        _colliderTransform.SetSiblingIndex(0);

        _collider = _colliderTransform.GetComponent<CapsuleCollider>();
        if (_collider == null)
            _collider = _colliderTransform.gameObject.AddComponent<CapsuleCollider>();

        _collider.height = _height;
        _collider.center = Vector3.up * _height / 2f;
    }

    private void SetupRigidbody()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();

        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.drag = 0f;
        _rb.angularDrag = 0f;
        _rb.mass = 75f;
    }

    public void Move(Vector3 wishVel)
    {
        Vector3 newPos = transform.position + wishVel * Time.deltaTime;

        if (IsGrounded)
        {
            IsGrounded = false;
            MoveOutPartial(ref newPos);
            MoveUp(ref newPos);
        }
        else
        {
            MoveOutFull(ref newPos);
        }

        if (_wasGrounded && wishVel.y <= 0f)
        {
            MoveDown(ref newPos);
        }

        _wasGrounded = IsGrounded;

        Velocity = (newPos - transform.position) / Time.deltaTime;
        transform.position = newPos;
    }

    private void MoveOutPartial(ref Vector3 newPos)
    {
        _colliderTransform.rotation = Quaternion.identity;
        _collider.height = _height - _maxStepUpHeight;
        _collider.center = Vector3.up * (_height + _maxStepUpHeight) / 2f;

        Collider[] neighbours = new Collider[128];

        int count = Physics.OverlapCapsuleNonAlloc(
            newPos + Vector3.up * (_height - _radius),
            newPos + Vector3.up * (_maxStepUpHeight + _radius),
            _radius,
            neighbours
        );

        var thisCollider = _colliderTransform.GetComponent<Collider>();

        for (int i = 0; i < count; ++i)
        {
            var collider = neighbours[i];

            if (collider == thisCollider)
                continue;

            Vector3 otherPosition = collider.transform.position;
            Quaternion otherRotation = collider.transform.rotation;

            Vector3 direction;
            float distance;

            bool overlapped = Physics.ComputePenetration(
                thisCollider, newPos, Quaternion.identity,
                collider, otherPosition, otherRotation,
                out direction, out distance
            );

            if (overlapped)
            {
                newPos += direction * distance;
            }
        }
    }

    private void MoveOutFull(ref Vector3 newPos)
    {
        _colliderTransform.rotation = Quaternion.identity;
        _collider.height = _height;
        _collider.center = Vector3.up * _height / 2f;

        Collider[] neighbours = new Collider[128];

        int count = Physics.OverlapCapsuleNonAlloc(
            newPos + Vector3.up * (_height - _radius),
            newPos + Vector3.up * _radius,
            _radius,
            neighbours
        );

        var thisCollider = _colliderTransform.GetComponent<Collider>();

        for (int i = 0; i < count; ++i)
        {
            var collider = neighbours[i];

            if (collider == thisCollider)
                continue;

            Vector3 otherPosition = collider.transform.position;
            Quaternion otherRotation = collider.transform.rotation;

            Vector3 direction;
            float distance;

            bool overlapped = Physics.ComputePenetration(
                thisCollider, newPos, Quaternion.identity,
                collider, otherPosition, otherRotation,
                out direction, out distance
            );

            if (overlapped)
            {
                newPos += direction * distance;

                if (direction.y > 0f)
                    IsGrounded = true;
            }
        }
    }

    private void MoveUp(ref Vector3 newPos)
    {
        RaycastHit hit;
        bool isInStep = Physics.SphereCast(
            newPos + Vector3.up * (_maxStepUpHeight + _radius),
            _radius,
            Vector3.down,
            out hit,
            _maxStepUpHeight
        );

        if (isInStep)
        {
            if (hit.point.y - newPos.y <= _maxStepUpHeight)
            {
                newPos += Vector3.up * (_maxStepUpHeight - hit.distance);
                IsGrounded = true;
            }
            else
            {
                MoveOutFull(ref newPos);
                IsGrounded = false;
            }
        }
        else
        {
            IsGrounded = false;
        }
    }

    private void MoveDown(ref Vector3 newPos)
    {
        RaycastHit hit;
        bool isGroundBelow = Physics.SphereCast(
            newPos + Vector3.up * (_maxStepUpHeight + _radius),
            _radius,
            Vector3.down,
            out hit,
            _maxStepUpHeight + _maxStepDownHeight
        );
        bool isGroundBelowCenter = Physics.Raycast(
            newPos + Vector3.up * _maxStepUpHeight,
            Vector3.down,
            _maxStepUpHeight + _maxStepDownHeight
        );

        if (isGroundBelow && isGroundBelowCenter)
        {
            newPos += Vector3.down * (hit.distance - _maxStepUpHeight);
            IsGrounded = true;
        }
    }

}
