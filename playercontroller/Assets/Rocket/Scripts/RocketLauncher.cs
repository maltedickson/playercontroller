using UnityEngine;
using UnityEngine.InputSystem;

public class RocketLauncher : MonoBehaviour
{
    [SerializeField] GameObject rocketPrefab = null;
    [SerializeField] InputAction shootInput;
    [SerializeField] float rocketSpeed = 18f;

    void OnEnable()
    {
        shootInput.Enable();
    }

    void OnDisable()
    {
        shootInput.Disable();
    }

    void Start()
    {
        shootInput.started += _ => Shoot();
    }

    void Shoot()
    {
        Rocket rocket = Instantiate(rocketPrefab, transform.position, transform.rotation).GetComponent<Rocket>();
        rocket.SetSpeed(rocketSpeed);
    }
}