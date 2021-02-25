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

    private Rigidbody _rb = null;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();
    }

    private void Start()
    {
        _points = new List<Vector3>(_pointTransforms.Length);
        foreach (Transform pointTransform in _pointTransforms)
        {
            if (pointTransform == null) continue;
            _points.Add(pointTransform.position);
        }

        _rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (_points.Count == 0) return;

        if (_targetIndex >= _points.Count)
            _targetIndex = 0;

        Vector3 newPos = Vector3.MoveTowards(_rb.position, _points[_targetIndex], _speed * Time.fixedDeltaTime);
        Vector3 movement = newPos - _rb.position;
        _rb.position = _rb.position + movement;

        foreach (Rigidbody rb in _rigidbodies)
        {
            rb.position = rb.position + movement;
        }

        if (_rb.position == _points[_targetIndex])
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
