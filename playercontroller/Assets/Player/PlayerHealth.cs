using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] Healthbar healthbar;

    public int maxHealth { get; private set; }
    public int health { get; private set; }

    void Start()
    {
        SetMaxHealth(150);
        SetHealth(this.maxHealth);
    }

    public void SetHealth(int health)
    {
        this.health = Mathf.Clamp(health, 0, this.maxHealth);
        UpdateHealthbar();
    }

    public void SetMaxHealth(int maxHealth)
    {
        this.maxHealth = Mathf.Max(1, maxHealth);
        this.health = Mathf.Min(this.health, this.maxHealth);
        UpdateHealthbar();
    }

    public void AddHealth(int health)
    {
        SetHealth(this.health + health);
    }

    void UpdateHealthbar()
    {
        healthbar.SetHealth(this.health);
        healthbar.SetMaxHealth(this.maxHealth);
    }
}