
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;

    [Header("Damage Settings")]
    public float invincibilityDuration = 1f;

    [Header("UI")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public GameObject gameOverPanel;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action OnDied;

    private float nextDamageTime;

    void Awake()
    {
        Time.timeScale = 1f;

        CurrentHealth = Mathf.Max(1, maxHealth);
        IsDead = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
            return;

        if (Time.time < nextDamageTime)
            return;

        CurrentHealth = Mathf.Max(
            0,
            CurrentHealth - damage
        );

        nextDamageTime =
            Time.time + invincibilityDuration;

        Debug.Log(
            $"Player HP: {CurrentHealth}/{maxHealth}"
        );

        UpdateHealthUI();

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = Mathf.Max(1, maxHealth);
            healthSlider.value = CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"HP: {CurrentHealth}/{Mathf.Max(1, maxHealth)}";
        }
    }

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        Debug.Log("GAME OVER - Player Died");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        OnDied?.Invoke();

        Time.timeScale = 0f;
    }
}
