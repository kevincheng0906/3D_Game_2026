
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallDamage : MonoBehaviour
{
    [Header("Damage Thresholds")]
    public float minHitSpeed = 2f;
    public float mediumHitSpeed = 5f;
    public float heavyHitSpeed = 10f;

    [Header("Damage")]
    public float lightDamage = 10f;
    public float mediumDamage = 25f;
    public float heavyDamage = 50f;

    [Header("Knockback")]
    public float knockbackMultiplier = 1.5f;
    public float upwardForce = 1.5f;

    [Header("Hit Cooldown")]
    public float hitCooldown = 0.4f;

    [Header("Hit Effects")]
    public GameObject hitEffectPrefab;

    private readonly Dictionary<EnemyHealth, float>
        lastHitTimes = new Dictionary<EnemyHealth, float>();

    private void OnCollisionEnter(Collision collision)
    {
        EnemyHealth enemy =
            collision.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        float hitSpeed = collision.relativeVelocity.magnitude;

        if (hitSpeed < minHitSpeed)
            return;

        // Prevent repeated damage to the same enemy.
        if (lastHitTimes.TryGetValue(enemy, out float lastTime))
        {
            if (Time.time - lastTime < hitCooldown)
                return;
        }

        float damage = CalculateDamage(hitSpeed);

        lastHitTimes[enemy] = Time.time;

        // Calculate knockback away from the ball.
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();

        if (enemyRb != null)
        {
            Vector3 direction =
                enemyRb.worldCenterOfMass -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = collision.relativeVelocity;
                direction.y = 0f;
            }

            direction = direction.normalized;

            Vector3 knockback =
                direction * hitSpeed * knockbackMultiplier +
                Vector3.up * upwardForce;

            enemyRb.AddForce(
                knockback,
                ForceMode.Impulse
            );

            EnemyChase chase =
                enemy.GetComponent<EnemyChase>();

            if (chase != null)
            {
                chase.ApplyKnockbackPause();
            }
        }

        Debug.Log(
            $"Ball hit {enemy.name} | " +
            $"Speed: {hitSpeed:F2} | " +
            $"Damage: {damage:F1}"
        );


        if (hitEffectPrefab != null)
        {
            Vector3 hitPosition =
                collision.GetContact(0).point +
                Vector3.up * 1.0f;

            GameObject effect = Instantiate(
                hitEffectPrefab,
                hitPosition,
                Quaternion.identity
            );

            ParticleSystem[] particles =
                effect.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particles)
            {
                ps.Play(true);
            }

            Debug.Log(
                $"HitEffect Spawned at {hitPosition}, " +
                $"Particle Systems: {particles.Length}"
            );
        }


        enemy.TakeDamage(damage);
    }

    private float CalculateDamage(float speed)
    {
        if (speed >= heavyHitSpeed)
            return heavyDamage;

        if (speed >= mediumHitSpeed)
            return mediumDamage;

        return lightDamage;
    }
}
