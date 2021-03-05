using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public class Healthbar : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] Image outlineImage = null;
    [SerializeField] float outlineScale = 1;
    [SerializeField] Image fillImage = null;
    Slider slider = null;

    [SerializeField] Color fillNormal;
    [SerializeField] Color fillDamaged;

    public int MaxHealth { get; private set; }
    public int Health { get; private set; }

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        SetMaxHealth(150);
        SetHealth(MaxHealth);
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        MaxHealth = newMaxHealth;
        slider.maxValue = MaxHealth;
    }

    public void SetHealth(float newHealth)
    {
        Health = Mathf.RoundToInt(newHealth);
        slider.value = Mathf.Clamp(Health, 0, MaxHealth);
        text.text = Health.ToString();

        if (Health < MaxHealth / 2)
        {
            outlineImage.enabled = true;

            int damageBelowHalf = MaxHealth / 2 - Health;
            float addScale = outlineScale / (MaxHealth / 2) * damageBelowHalf;
            float scale = 1 + addScale;
            outlineImage.transform.localScale = new Vector3(scale, scale, 0);

            fillImage.color = fillDamaged;
        }
        else
        {
            outlineImage.enabled = false;

            fillImage.color = fillNormal;
        }
    }

}
