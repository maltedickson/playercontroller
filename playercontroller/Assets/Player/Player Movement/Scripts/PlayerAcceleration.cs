using UnityEngine;

public class PlayerAcceleration
{
    float groundAcceleration;
    float airAcceleration;

    Transform transform;

    public PlayerAcceleration(float groundAcceleration, float airAcceleration, Transform transform)
    {
        this.groundAcceleration = groundAcceleration;
        this.airAcceleration = airAcceleration;
        this.transform = transform;
    }

    public Vector3 Accelerate(Vector2 input, Vector3 velocity, float CurrentMaxMoveSpeed, bool isGrounded)
    {
        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
        Vector3 wishVel = wishDir * CurrentMaxMoveSpeed;
        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 newHorVel;

        if (isGrounded)
        {
            newHorVel = Vector3.ClampMagnitude(horVel + wishVel * groundAcceleration * Time.fixedDeltaTime, CurrentMaxMoveSpeed);
        }
        else
        {
            float speed = Mathf.Clamp(CurrentMaxMoveSpeed - Vector3.Dot(horVel, wishVel), 0f, airAcceleration * Time.fixedDeltaTime);
            Vector3 horVelAfterAcceleration = horVel + wishVel * speed;

            newHorVel = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(horVel.magnitude, CurrentMaxMoveSpeed));
        }

        return new Vector3(newHorVel.x, 0, newHorVel.z) - horVel;
    }
}