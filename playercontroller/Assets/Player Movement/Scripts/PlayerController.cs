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
        GetInfoFromMover();

        Gravity();

        Jump();

        Friction();

        Acceleration();

        Move();
    }

    void GetInfoFromMover()
    {
        isGrounded = mover.IsGrounded;

        velocity = rb.velocity;
        if (isGrounded)
            velocity.y = 0f;
    }

    void Gravity()
    {
        if (isGrounded) return;

        velocity += Vector3.up * config.gravity * Time.fixedDeltaTime;
    }

    void Jump()
    {
        if (!isGrounded || !_isJumpButtonDown) return;

        isGrounded = false;
        velocity.y = Mathf.Sqrt(config.jumpHeight * 2f * -config.gravity);
    }

    void Friction()
    {
        if (!isGrounded) return;

        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

        float speed = horVel.magnitude;
        if (speed == 0f) return;

        float drop = speed / config.friction * Time.fixedDeltaTime;

        Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;

        velocity = new Vector3(newHorVel.x, velocity.y, newHorVel.z);
    }

    void Acceleration()
    {
        Vector2 moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 wishVel = wishDir * config.moveSpeed;

        if (isGrounded)
            GroundAcceleration();
        else
            AirAcceleration();

        void GroundAcceleration()
        {
            Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

            Vector3 horVelAfterAcceleration = horVel + wishVel * config.maxAcceleration * Time.fixedDeltaTime;

            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, config.moveSpeed);

            velocity = new Vector3(
                clampedHorVelAfterAcceleration.x,
                velocity.y,
                clampedHorVelAfterAcceleration.z
            );
        }

        void AirAcceleration()
        {
            Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

            float speed = horVel.magnitude;

            float currentSpeedInWishDir = Vector3.Dot(horVel, wishVel);

            float addSpeed = Mathf.Clamp(config.moveSpeed - currentSpeedInWishDir, 0f, config.maxAccelerationInAir * Time.fixedDeltaTime);

            Vector3 horVelAfterAcceleration = horVel + wishVel * addSpeed;

            Vector3 clampedHorVelAfterAcceleration = Vector3.ClampMagnitude(horVelAfterAcceleration, Mathf.Max(speed, config.moveSpeed));

            velocity = new Vector3(
                clampedHorVelAfterAcceleration.x,
                velocity.y,
                clampedHorVelAfterAcceleration.z
            );
        }
    }

    void Move()
    {
        mover.IsGrounded = isGrounded;
        mover.Move(velocity);
    }

}
