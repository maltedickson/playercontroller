using UnityEngine;
using UnityEngine.InputSystem;

public class AccelerationModifier : MonoBehaviour, IMovementModifier
{
    [SerializeField] InputAction moveInput;
    Vector2 input;

    void OnEnable() => moveInput.Enable();
    void OnDisable() => moveInput.Disable();

    void Update()
    {
        input += moveInput.ReadValue<Vector2>();
    }

    public Vector3 Modify(ModifierInfo info, PlayerMoverConfig config)
    {
        input.Normalize();
        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
        input = Vector2.zero;

        Vector3 wishVel = wishDir * config.speed;
        Vector3 newHorVel;

        if (info.IsGrounded)
        {
            newHorVel = Vector3.ClampMagnitude(info.CurrentHorizontalVelocity + wishVel * config.groundAcceleration * Time.fixedDeltaTime, config.speed);
        }
        else
        {
            float speed = Mathf.Clamp(config.speed - Vector3.Dot(info.CurrentHorizontalVelocity, wishVel), 0f, config.airAcceleration * Time.fixedDeltaTime);
            Vector3 horVelAfterAcceleration = info.CurrentHorizontalVelocity + wishVel * speed;

            newHorVel = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(info.CurrentHorizontalVelocity.magnitude, config.speed));
        }

        return new Vector3(newHorVel.x, 0, newHorVel.z) - info.CurrentHorizontalVelocity;
    }
}