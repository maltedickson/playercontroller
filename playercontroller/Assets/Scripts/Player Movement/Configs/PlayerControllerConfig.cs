using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerConfig", menuName = "PlayerControllerConfig")]
public class PlayerControllerConfig : ScriptableObject
{

    [Header("Ground Movement")]
    [SerializeField] public float moveSpeed = 6f;
    [SerializeField] public float maxAcceleration = 10f;
    [SerializeField] public float friction = 0.1f;

    [Header("Air Movement")]
    [SerializeField] public float maxAccelerationInAir = 6f;
    [SerializeField] public float gravity = -19.62f;

    [Header("Jump")]
    [SerializeField] public float jumpHeight = 1f;

}
