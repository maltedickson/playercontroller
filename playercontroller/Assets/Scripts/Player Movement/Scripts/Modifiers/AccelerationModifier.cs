using UnityEngine;
using UnityEngine.InputSystem;

public class AccelerationModifier : MonoBehaviour, IMovementModifier
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float maxAcceleration = 10f;
    [SerializeField] float maxAccelerationInAir = 6f;

    [Header("Input")]
    [SerializeField] InputAction moveInput;
    Vector2 input;

    void OnEnable() => moveInput.Enable();
    void OnDisable() => moveInput.Disable();

    public Vector3 ModifyVelocity(Vector3 velocity, bool isGrounded)
    {
        input.Normalize();
        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
        input = Vector2.zero;

        Vector3 wishVel = wishDir * moveSpeed;

        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 newHorVel;

        if (isGrounded) GroundAcceleration();
        else AirAcceleration();

        return new Vector3(newHorVel.x, velocity.y, newHorVel.z);

        void GroundAcceleration()
        {
            Vector3 horVelAfterAcceleration = horVel + wishVel * maxAcceleration * Time.fixedDeltaTime;
            newHorVel = Vector3.ClampMagnitude(horVelAfterAcceleration, moveSpeed);
        }

        void AirAcceleration()
        {
            float speed = horVel.magnitude;
            float currentSpeedInWishDir = Vector3.Dot(horVel, wishVel);
            float addSpeed = Mathf.Clamp(moveSpeed - currentSpeedInWishDir, 0f, maxAccelerationInAir * Time.fixedDeltaTime);

            Vector3 horVelAfterAcceleration = horVel + wishVel * addSpeed;
            newHorVel = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(speed, moveSpeed));
        }
    }

    void Update()
    {
        input += moveInput.ReadValue<Vector2>();
    }
}