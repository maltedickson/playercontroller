using UnityEngine;

public class PlayerJump
{
    float jumpForce;

    public PlayerJump(float jumpHeight, float gravity)
    {
        jumpForce = Mathf.Sqrt(jumpHeight * 2f * -gravity);
    }

    public PlayerJump(float jumpForce)
    {
        this.jumpForce = jumpForce;
    }

    public Vector3 Jump(bool wantsToJump, bool isGrounded, bool isCrouching)
    {
        if (!wantsToJump) return Vector3.zero;
        if (!isGrounded) return Vector3.zero;
        if (isCrouching) return Vector3.zero;

        return new Vector3(0f, jumpForce, 0f);
    }
}