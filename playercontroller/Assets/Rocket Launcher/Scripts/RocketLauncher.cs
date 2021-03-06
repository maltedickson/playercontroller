using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RocketLauncher : MonoBehaviour
{
    [SerializeField] GameObject rocketPrefab = null;
    [SerializeField] float rocketSpeed = 18f;
    [SerializeField] float attackInterval = 0.8f;
    bool isShooting = false;
    [Space]
    [SerializeField] InputAction shootDown;
    [SerializeField] InputAction shootUp;
    bool wantsToShoot = false;

    void OnEnable()
    {
        shootDown.Enable();
        shootUp.Enable();
    }

    void OnDisable()
    {
        shootDown.Disable();
        shootUp.Disable();
    }

    void Start()
    {
        shootDown.performed += _ => wantsToShoot = true;
        shootUp.performed += _ => wantsToShoot = false;
    }

    void Update()
    {
        if (wantsToShoot && !isShooting) StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        Rocket rocket = Instantiate(rocketPrefab, transform.position, transform.rotation).GetComponent<Rocket>();
        rocket.SetSpeed(rocketSpeed);
        yield return new WaitForSeconds(attackInterval);
        isShooting = false;
    }
}