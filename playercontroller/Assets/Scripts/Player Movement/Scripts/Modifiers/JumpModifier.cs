using UnityEngine;
using UnityEngine.InputSystem;

public class JumpModifier : MonoBehaviour, IMovementModifier
{
    [SerializeField] InputAction jumpInput;
    bool wasJumpPressed = false;

    void OnEnable() => jumpInput.Enable();
    void OnDisable() => jumpInput.Disable();

    void Start()
    {
        jumpInput.performed += _ => wasJumpPressed = true;
    }

    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        if (!wasJumpPressed) return Vector3.zero;
        wasJumpPressed = false;
        if (!info.IsGrounded) return Vector3.zero;

        return new Vector3(0, Mathf.Sqrt(config.jumpForce * 2f * -config.gravity), 0);
    }
}