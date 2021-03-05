using System.Collections;
using UnityEngine;

public class HealthPackSpawner : MonoBehaviour
{
    [SerializeField] GameObject healthPackPrefab = null;
    [SerializeField] float respawnTime = 5f;

    void Start()
    {
        StartCoroutine(SpawnHealthPack(0f));
    }

    public void SpawnHealthPack()
    {
        StartCoroutine(SpawnHealthPack(respawnTime));
    }

    IEnumerator SpawnHealthPack(float time)
    {
        yield return new WaitForSeconds(time);

        HealthPack healthPack = Instantiate(healthPackPrefab, transform.position, transform.rotation).GetComponent<HealthPack>();
        healthPack.SetSpawner(this);
    }
}