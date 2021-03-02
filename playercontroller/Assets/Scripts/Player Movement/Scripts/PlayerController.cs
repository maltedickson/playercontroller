using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerControllerConfig config = null;

    Controls controls = null;
    bool isJumpButtonDown = false;

    PlayerMover mover = null;

    Vector3 velocity = Vector3.zero;

    void Awake()
    {
        controls = new Controls();

        mover = GetComponent<PlayerMover>();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        controls.Gameplay.JumpDown.performed += _ => isJumpButtonDown = true;
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
        velocity = mover.velocity;

        if (mover.isGrounded)
            velocity.y = 0f;
    }

    void Gravity()
    {
        if (mover.isGrounded) return;

        velocity += Vector3.up * config.gravity * Time.fixedDeltaTime;
    }

    void Jump()
    {
        if (!isJumpButtonDown) return;
        isJumpButtonDown = false;

        if (!mover.isGrounded) return;

        velocity.y = Mathf.Sqrt(config.jumpHeight * 2f * -config.gravity);
    }

    void Friction()
    {
        if (!mover.isGrounded) return;

        Vector3 horVel = new Vector3(velocity.x, 0f, velocity.z);

        float speed = horVel.magnitude;
        if (speed == 0f) return;

        float drop = speed / config.friction * Time.fixedDeltaTime;

        Vector3 newHorVel = horVel * Mathf.Max(speed - drop, 0f) / speed;

        velocity = new Vector3(newHorVel.x, velocity.y, newHorVel.z);
    }

    void Acceleration()
    {
        Vector2 moveInput = controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 wishVel = wishDir * config.moveSpeed;

        if (mover.isGrounded)
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
        mover.Move(velocity);
    }
}
