using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private WayPoint[] _points = null;
    private int _targetIndex = 0;
    private bool _isUpdatingTargetIndex = false;

    [SerializeField] private float _speed = 4f;

    enum MoveMode { Circle, BackAndForth }
    [SerializeField] private MoveMode _moveMode = MoveMode.Circle;
    [SerializeField] private bool _isMovingForward = true;

    private void Awake()
    {
    }

    private void Start()
    {
        foreach (WayPoint point in _points) point.SetPosition();
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

        Vector3 newPos = Vector3.MoveTowards(transform.position, _points[_targetIndex].Position(), _speed * Time.fixedDeltaTime);
        Vector3 movement = newPos - transform.position;
        transform.position = transform.position + movement;

        if (transform.position != _points[_targetIndex].Position() || _isUpdatingTargetIndex) return;

        StartCoroutine(SetTargetIndex(_isMovingForward ? _targetIndex + 1 : _targetIndex - 1, _points[_targetIndex].WaitTime()));
    }

    private IEnumerator SetTargetIndex(int newTargetIndex, float time)
    {
        _isUpdatingTargetIndex = true;

        yield return new WaitForSeconds(time);
        _targetIndex = newTargetIndex;

        _isUpdatingTargetIndex = false;
    }

    [System.Serializable]
    private class WayPoint
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