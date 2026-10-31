
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 50f;

    public event Action<EnemyHealth> OnDied;

    private float currentHealth;
    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth -= damage;

        EnemyHitFlash hitFlash =
            GetComponent<EnemyHitFlash>();

        if (hitFlash != null)
        {
            hitFlash.Flash();
        }

        Debug.Log(
            $"{gameObject.name} took {damage:F1} damage. " +
            $"HP: {Mathf.Max(currentHealth, 0f):F1}"
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log($"{gameObject.name} destroyed!");

        OnDied?.Invoke(this);

        Destroy(gameObject);
    }
}
