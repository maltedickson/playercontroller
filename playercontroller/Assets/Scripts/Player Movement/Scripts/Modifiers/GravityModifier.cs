using UnityEngine;

public class GravityModifier : MonoBehaviour, IMovementModifier
{
    [SerializeField] float gravity = -19.62f;

    public Vector3 ModifyVelocity(Vector3 velocity, bool isGrounded)
    {
        if (isGrounded) return velocity;

        velocity += Vector3.up * gravity * Time.fixedDeltaTime;
        return velocity;
    }
}