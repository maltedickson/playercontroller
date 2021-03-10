using UnityEngine;

public class PlayerFriction
{
    float friction;

    public PlayerFriction(float friction)
    {
        this.friction = friction;
    }

    public Vector3 ApplyFriction(Vector3 velocity, bool isGrounded)
    {
        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

        if (!isGrounded || horVel.magnitude == 0) return Vector3.zero;

        float speed = horVel.magnitude;
        float drop = speed / friction * Time.fixedDeltaTime;

        Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;
        Vector3 value = new Vector3(newHorVel.x, 0, newHorVel.z) - horVel;

        return value;
    }
}