
using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Attack")]
    public int damage = 1;
    public float attackRange = 1.6f;
    public float attackInterval = 1f;

    private PlayerHealth playerHealth;
    private float nextAttackTime;

    void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "EnemyContactDamage: PlayerHealth not found."
            );
        }
    }

    void Update()
    {
        if (playerHealth == null ||
            playerHealth.IsDead)
            return;

        if (Time.time < nextAttackTime)
            return;

        Vector3 direction =
            playerHealth.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <=
            attackRange * attackRange)
        {
            playerHealth.TakeDamage(damage);

            nextAttackTime =
                Time.time + Mathf.Max(0.1f, attackInterval);
        }
    }
}
