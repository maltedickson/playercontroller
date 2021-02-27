using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private Point[] _points = null;
    private int _targetIndex = 0;

    [SerializeField] private float _speed = 4f;

    enum MoveMode { Circle, BackAndForth }
    [SerializeField] private MoveMode _moveMode = MoveMode.Circle;
    [SerializeField] private bool _isMovingForward = true;

    BoxCollider[] _triggers = null;
    private List<Rigidbody> _rigidbodiesInTrigger = new List<Rigidbody>();
    private List<Rigidbody> _rigidbodiesTouchingCollider = new List<Rigidbody>();

    private Rigidbody _rb = null;

    private bool _isUpdatingTargetIndex = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();
    }

    private void Start()
    {
        _rb.isKinematic = true;

        _triggers = GetComponents<BoxCollider>();
        foreach (BoxCollider trigger in _triggers)
        {
            trigger.isTrigger = true;
        }

        foreach (Point point in _points)
        {
            point.SetPosition();
        }
    }

    private void FixedUpdate()
    {
        if (_points.Length == 0) return;

        switch (_moveMode)
        {
            case MoveMode.Circle:
                if (_targetIndex >= _points.Length)
                {
                    _targetIndex = 0;
                }
                else if (_targetIndex < 0)
                {
                    _targetIndex = _points.Length - 1;
                }
                break;

            case MoveMode.BackAndForth:
                if (_targetIndex >= _points.Length)
                {
                    _targetIndex = _points.Length - 1;
                    _isMovingForward = false;
                }
                else if (_targetIndex < 0)
                {
                    _targetIndex = 0;
                    _isMovingForward = true;
                }
                break;
        }

        Vector3 newPos = Vector3.MoveTowards(_rb.position, _points[_targetIndex].Position(), _speed * Time.fixedDeltaTime);
        Vector3 movement = newPos - _rb.position;
        _rb.position = _rb.position + movement;

        foreach (Rigidbody rb in _rigidbodiesInTrigger)
        {
            if (!_rigidbodiesTouchingCollider.Contains(rb)) continue;
            rb.transform.position = rb.transform.position + movement;
        }

        if (_rb.position != _points[_targetIndex].Position() || _isUpdatingTargetIndex) return;

        StartCoroutine(SetTargetIndex(_isMovingForward ? _targetIndex + 1 : _targetIndex - 1, _points[_targetIndex].WaitTime()));
    }

    private IEnumerator SetTargetIndex(int newTargetIndex, float time)
    {
        _isUpdatingTargetIndex = true;

        yield return new WaitForSeconds(time);
        _targetIndex = newTargetIndex;

        _isUpdatingTargetIndex = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic || _rigidbodiesInTrigger.Contains(rb)) return;
        _rigidbodiesInTrigger.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if (rb == null || !_rigidbodiesInTrigger.Contains(rb)) return;
        _rigidbodiesInTrigger.Remove(rb);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic || _rigidbodiesTouchingCollider.Contains(rb)) return;
        _rigidbodiesTouchingCollider.Add(rb);
    }

    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (!_rigidbodiesTouchingCollider.Contains(rb)) return;
        _rigidbodiesTouchingCollider.Remove(rb);
    }

    private void OnDrawGizmosSelected()
    {
        _triggers = GetComponents<BoxCollider>();

        foreach (BoxCollider trigger in _triggers)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 1f);
            Gizmos.DrawWireCube(transform.position + trigger.center, trigger.size);

            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawCube(transform.position + trigger.center, trigger.size);
        }
    }



    [System.Serializable]
    private class Point
    {
        [SerializeField] private Transform _transform = null;
        [SerializeField] private float _waitTime = 0f;
        private Vector3 _position = Vector3.zero;

        public void SetPosition()
        {
            _position = _transform.position;
        }

        public Vector3 Position()
        {
            return _position;
        }

        public float WaitTime()
        {
            return _waitTime;
        }
    }

}