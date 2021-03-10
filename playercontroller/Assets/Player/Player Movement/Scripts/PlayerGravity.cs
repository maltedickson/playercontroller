using UnityEngine;

public class PlayerGravity
{
    float gravity;

    public PlayerGravity(float gravity)
    {
        this.gravity = gravity;
    }

    public Vector3 ApplyGravity(bool isGrounded)
    {
        if (isGrounded) return Vector3.zero;
        return Vector3.up * gravity * Time.fixedDeltaTime;
    }
}