using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
public class PlayerController : MonoBehaviour
{

    [SerializeField] PlayerControllerConfig config = null;

    Vector2 moveInput = Vector2.zero;

    Controls _controls = null;
    bool _isJumpButtonDown = false;

    PlayerMover mover = null;

    Vector3 velocity = Vector3.zero;
    bool isGrounded = false;
    bool wasGrounded = false;

    Rigidbody rb = null;

    void Awake()
    {
        _controls = new Controls();

        mover = GetComponent<PlayerMover>();
    }

    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();

    void Start()
    {
        _controls.Gameplay.JumpDown.performed += _ => _isJumpButtonDown = true;
        _controls.Gameplay.JumpUp.performed += _ => _isJumpButtonDown = false;

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        isGrounded = mover.IsGrounded;

        Vector3 previousVelocity = rb.velocity;
        if (isGrounded)
            previousVelocity.y = 0f;

        Vector3 newVelocity = previousVelocity;

        if (!isGrounded)
            newVelocity = ApplyGravity(newVelocity, Time.fixedDeltaTime);

        if (isGrounded && _isJumpButtonDown)
        {
            isGrounded = false;
            newVelocity = Jump(newVelocity, Time.fixedDeltaTime);
        }

        if (isGrounded)
            newVelocity = ApplyFriction(newVelocity, Time.fixedDeltaTime);

        Vector2 moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 wishVel = wishDir * config.moveSpeed;
        newVelocity = Accelerate(newVelocity, wishVel);

        mover.IsGrounded = isGrounded;
        mover.Move(newVelocity);
    }

    Vector3 ApplyGravity(Vector3 currentVel, float deltaTime)
    {
        return currentVel + Vector3.up * config.gravity * Time.fixedDeltaTime;
    }

    Vector3 Jump(Vector3 currentVel, float deltaTime)
    {
        return currentVel + Vector3.up * Mathf.Sqrt(config.jumpHeight * 2f * -config.gravity);
    }

    Vector3 ApplyFriction(Vector3 currentVel, float deltaTime)
    {
        Vector3 horVel = new Vector3(currentVel.x, 0f, currentVel.z);

        float speed = horVel.magnitude;
        if (speed != 0f)
        {
            float drop = speed / config.friction * deltaTime;
            Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;
            return new Vector3(newHorVel.x, currentVel.y, newHorVel.z);
        }

        return currentVel;
    }

    Vector3 Accelerate(Vector3 currentVel, Vector3 wishVel)
    {
        if (isGrounded)
        {
            Vector3 horVel = new Vector3(currentVel.x, 0f, currentVel.z);
            Vector3 horVelAfterAcceleration = horVel + wishVel * config.maxAcceleration * Time.fixedDeltaTime;
            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, config.moveSpeed);
            return new Vector3(
                clampedHorVelAfterAcceleration.x,
                currentVel.y,
                clampedHorVelAfterAcceleration.z
            );
        }
        else
        {
            Vector3 horVel = new Vector3(currentVel.x, 0f, currentVel.z);
            float speed = horVel.magnitude;
            float currentSpeedInWishDir = Vector3.Dot(horVel, wishVel);
            float addSpeed = Mathf.Clamp(config.moveSpeed - currentSpeedInWishDir, 0f, config.maxAccelerationInAir * Time.fixedDeltaTime);
            Vector3 horVelAfterAcceleration = horVel + wishVel * addSpeed;

            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(speed, config.moveSpeed));
            return new Vector3(
                clampedHorVelAfterAcceleration.x,
                currentVel.y,
                clampedHorVelAfterAcceleration.z
            );
        }
    }

}
