using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{

    [SerializeField] private float _speed = 4f;

    [SerializeField] private float _minDistanceFromPoint = 0.1f;

    [SerializeField] private Transform[] _pointTransforms = null;
    private Vector3[] _points = null;
    private int _targetIndex = 0;


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

        transform.position = Vector3.MoveTowards(transform.position, _points[_targetIndex], _speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, _points[_targetIndex]) < _minDistanceFromPoint)
        {
            _targetIndex++;
        }

    }

}
