using UnityEngine;

public class FrictionModifier : MonoBehaviour, IMovementModifier
{
    [SerializeField] float friction = 0.1f;

    public Vector3 ModifyVelocity(Vector3 velocity, bool isGrounded)
    {
        if (!isGrounded) return velocity;

        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

        float speed = horVel.magnitude;
        if (speed == 0f) return velocity;

        float drop = speed / friction * Time.fixedDeltaTime;

        Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;

        return new Vector3(newHorVel.x, velocity.y, newHorVel.z);
    }
}