using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private float _speed = 4f;

    [SerializeField] private Transform[] _pointTransforms = null;
    private List<Vector3> _points;
    private int _targetIndex = 0;
    private List<Rigidbody> _rigidbodies = new List<Rigidbody>();

    private Transform _transform = null;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        _points = new List<Vector3>(_pointTransforms.Length);
        foreach (Transform pointTransform in _pointTransforms)
        {
            if (pointTransform == null) continue;
            _points.Add(pointTransform.position);
        }
    }

    private void FixedUpdate()
    {
        if (_points.Count == 0) return;

        if (_targetIndex >= _points.Count)
            _targetIndex = 0;

        Vector3 newPos = Vector3.MoveTowards(_transform.position, _points[_targetIndex], _speed * Time.fixedDeltaTime);
        Vector3 movement = newPos - _transform.position;
        _transform.position = newPos;

        foreach (Rigidbody rb in _rigidbodies)
        {
            Transform rbTransform = rb.transform;
            rbTransform.position = rbTransform.position + movement;
        }

        if (_transform.position == _points[_targetIndex])
            _targetIndex++;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic || _rigidbodies.Contains(rb) || !IsCollisionOnTop(collision)) return;
        _rigidbodies.Add(rb);
    }

    private bool IsCollisionOnTop(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
            if (contact.normal.y < 0f) return true;

        return false;
    }

    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (!_rigidbodies.Contains(rb)) return;
        _rigidbodies.Remove(rb);
    }

}
