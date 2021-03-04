using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoverConfig", menuName = "PlayerMoverConfig")]
public class PlayerMoverConfig : ScriptableObject
{
    [Header("Physics")]
    [SerializeField] public float gravity = -19.62f;
    [SerializeField] public float jumpForce = 1f;
    [SerializeField] public float maxForce = 15f;
    [SerializeField] public float friction = 0.1f;

    [Header("Collider")]
    [SerializeField] public float height = 2f;
    [SerializeField] public float radius = 0.5f;
    [SerializeField] public PhysicMaterial noFrictionPhysicMaterial = null;

    [Header("Crouching")]
    [SerializeField, Range(0.5f, 1f)] public float crouchHeightMultiplier = 0.75f;
    [SerializeField] public float crouchMoveSpeedMultiplier = 0.33f;
    [SerializeField] public float crouchTransitionTime = 0.1f;

    [Header("Movement")]
    [SerializeField] public float speed = 6f;
    [SerializeField] public float groundAcceleration = 10f;
    [SerializeField] public float airAcceleration = 6f;

    [Header("Slopes and Steps")]
    [SerializeField] public float slopeLimit = 50f;
    [SerializeField] public float maxStepUpHeight = 0.3f;
    [SerializeField] public float maxStepDownHeight = 0.4f;
}
