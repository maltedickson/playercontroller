using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] float healthPercentage = 0.5f;

    HealthPackSpawner spawner;

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth is null) return;

        if (playerHealth.health >= playerHealth.maxHealth) return;

        playerHealth.AddHealth(Mathf.RoundToInt(playerHealth.maxHealth * healthPercentage));

        if (spawner != null) spawner.SpawnHealthPack();

        Destroy(gameObject);
    }

    public void SetSpawner(HealthPackSpawner spawner)
    {
        this.spawner = spawner;
    }
}