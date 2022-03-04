using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] WayPoint[] wayPoints = null;
    int targetIndex = 0;
    bool isUpdatingTargetIndex = false;

    [SerializeField] float moveSpeed = 4f;

    enum MoveMode { Circle, BackAndForth }
    [SerializeField] MoveMode moveMode = MoveMode.Circle;
    [SerializeField] bool isMovingForward = true;

    public Vector3 movement { get; private set; }
    public float deltaTime { get; private set; }

    Rigidbody rb = null;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
    }

    void Start()
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        foreach (WayPoint point in wayPoints) point.ClearParent();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (wayPoints.Length == 0) return;

        switch (moveMode)
        {
            case MoveMode.Circle:
                if (targetIndex >= wayPoints.Length)
                {
                    targetIndex = 0;
                }
                else if (targetIndex < 0)
                {
                    targetIndex = wayPoints.Length - 1;
                }
                break;

            case MoveMode.BackAndForth:
                if (targetIndex >= wayPoints.Length)
                {
                    targetIndex = wayPoints.Length - 1;
                    isMovingForward = false;
                }
                else if (targetIndex < 0)
                {
                    targetIndex = 0;
                    isMovingForward = true;
                }
                break;
        }

        Vector3 newPos = Vector3.MoveTowards(rb.position, wayPoints[targetIndex].Position, moveSpeed * Time.fixedDeltaTime);
        movement = newPos - rb.position;
        rb.MovePosition(rb.position + movement);

        deltaTime = Time.fixedDeltaTime;

        if (rb.position != wayPoints[targetIndex].Position || isUpdatingTargetIndex) return;

        StartCoroutine(SetTargetIndex(isMovingForward ? targetIndex + 1 : targetIndex - 1, wayPoints[targetIndex].WaitTime));
    }

    IEnumerator SetTargetIndex(int newTargetIndex, float time)
    {
        isUpdatingTargetIndex = true;

        yield return new WaitForSeconds(time);
        targetIndex = newTargetIndex;

        isUpdatingTargetIndex = false;
    }

    void OnDrawGizmos()
    {
        if (moveMode == MoveMode.Circle)
        {
            for (int i = 0; i < wayPoints.Length; i++)
            {
                WayPoint point = wayPoints[i];
                WayPoint next = wayPoints[(i + 1) % wayPoints.Length];
                Gizmos.DrawLine(point.Position, next.Position);
            }
        }
        else
        {
            for (int i = 0; i < wayPoints.Length - 1; i++)
            {
                WayPoint point = wayPoints[i];
                WayPoint next = wayPoints[i + 1];
                Gizmos.DrawLine(point.Position, next.Position);
            }
        }
    }

    [System.Serializable]
    class WayPoint
    {
        [SerializeField] Transform _transform = null;
        [SerializeField] float _waitTime = 0f;
        Vector3 _position = Vector3.zero;

        public void ClearParent()
        {
            _transform.parent = null;
        }

        public Vector3 Position
        {
            get { return _transform.position; }
        }

        public float WaitTime
        {
            get { return _waitTime; }
        }
    }

}