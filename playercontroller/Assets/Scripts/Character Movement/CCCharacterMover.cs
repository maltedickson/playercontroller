using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCCharacterMover : MonoBehaviour, ICharacterMover
{

    private CharacterController _cc = null;

    public Vector3 Velocity { get; private set; }
    public bool IsGrounded { get; private set; }
    private bool _wasGrounded;

    [Header("Collider")]
    [SerializeField] private float _height = 2f;
    [SerializeField] private float _radius = 0.5f;
    [SerializeField] private float _skinWidth = 0.08f;

    [Header("Movement")]
    [SerializeField] private float _slopeLimit = 50f;
    [SerializeField] private float _maxStepUpHeight = 0.3f;
    [SerializeField] private float _maxStepDownHeight = 0.4f;


    private void Awake()
    {
        _cc = GetComponent<CharacterController>() ? GetComponent<CharacterController>() : gameObject.AddComponent<CharacterController>();
    }

    private void Start()
    {
        _cc.slopeLimit = _slopeLimit;
        _cc.stepOffset = _maxStepUpHeight;
        _cc.skinWidth = _skinWidth;
        _cc.minMoveDistance = 0f;
        _cc.height = _height;
        _cc.radius = _radius;
        _cc.center = Vector3.up * _height / 2f;
    }

    public void Move(Vector3 wishVel)
    {
        Vector3 originalPos = transform.position;

        Vector3 wishMovement = wishVel * Time.deltaTime;
        _cc.Move(wishMovement);
        IsGrounded = _cc.isGrounded;

        Vector3 newPos = transform.position;

        if (_wasGrounded && wishVel.y <= 0f)
        {
            newPos += MoveDown(newPos);
            transform.position = newPos;
        }

        _wasGrounded = IsGrounded;

        Velocity = (newPos - originalPos) / Time.deltaTime;
    }

    private Vector3 MoveDown(Vector3 pos)
    {
        RaycastHit hit;

        bool isGroundBelow = Physics.SphereCast(
            pos + Vector3.up * _radius,
            _radius,
            Vector3.down,
            out hit,
            _maxStepDownHeight + _skinWidth
        );

        bool isGroundBelowCenter = Physics.Raycast(
            pos,
            Vector3.down,
            _maxStepDownHeight + _skinWidth
        );

        if (isGroundBelow && isGroundBelowCenter)
        {
            IsGrounded = true;
            return Vector3.down * (hit.distance - _skinWidth);
        }

        return Vector3.zero;
    }

}
