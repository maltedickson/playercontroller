using UnityEngine;
using UnityEngine.InputSystem;

public class JumpModifier : MonoBehaviour, IMovementModifier
{
    [SerializeField] float jumpHeight = 1;
    [SerializeField] float gravity = -19.62f;

    [Header("Input")]
    [SerializeField] InputAction jumpInput;
    bool wasJumpPressed = false;

    void OnEnable() => jumpInput.Enable();
    void OnDisable() => jumpInput.Disable();

    void Start()
    {
        jumpInput.performed += _ => wasJumpPressed = true;
    }

    public Vector3 ModifyVelocity(Vector3 velocity, bool isGrounded)
    {
        if (!wasJumpPressed) return velocity;
        wasJumpPressed = false;

        if (!isGrounded) return velocity;

        velocity.y = Mathf.Sqrt(jumpHeight * 2f * -gravity);
        return velocity;
    }
}