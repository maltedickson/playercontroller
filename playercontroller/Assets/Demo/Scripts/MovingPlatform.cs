using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private float _speed = 4f;

    [SerializeField] private Transform[] _pointTransforms = null;
    private Vector3[] _points = null;
    private int _targetIndex = 0;

    private List<Rigidbody> _rigidbodies = new List<Rigidbody>();

    private Transform _transform = null;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        _points = new Vector3[_pointTransforms.Length];
        for (int i = 0; i < _pointTransforms.Length; i++)
        {
            _points[i] = _pointTransforms[i].position;
        }
    }

    private void FixedUpdate()
    {
        if (_points.Length == 0) return;

        if (_targetIndex >= _points.Length)
            _targetIndex = 0;

        Vector3 newPos = Vector3.MoveTowards(_transform.position, _points[_targetIndex], _speed * Time.fixedDeltaTime);

        Vector3 movement = newPos - _transform.position;

        foreach (Rigidbody rb in _rigidbodies)
        {
            Transform rbTransform = rb.transform;
            rbTransform.position = rbTransform.position + movement;
        }

        _transform.position = newPos;

        if (_transform.position == _points[_targetIndex])
            _targetIndex++;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsCollisionOnTop(collision)) return;

        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb == null) return;
        if (rb.isKinematic) return;
        if (_rigidbodies.Contains(rb)) return;

        _rigidbodies.Add(rb);
    }

    private bool IsCollisionOnTop(Collision collision)
    {
        bool isCollisionOnTop = false;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y < 0f) isCollisionOnTop = true;
        }
        return isCollisionOnTop;
    }

    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (!_rigidbodies.Contains(rb)) return;
        _rigidbodies.Remove(rb);
    }

}
