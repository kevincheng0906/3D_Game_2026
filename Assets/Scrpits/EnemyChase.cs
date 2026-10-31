
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float acceleration = 12f;
    public float turnSpeed = 8f;
    public float stopDistance = 1.3f;

    [Header("Knockback Recovery")]
    public float knockbackPause = 0.45f;

    private Rigidbody rb;
    private float pauseUntil;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Automatically find the player if not assigned.
        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
        }
    }

    void FixedUpdate()
    {
        if (target == null)
            return;

        // Let physics knockback happen without AI interference.
        if (Time.time < pauseUntil)
            return;

        Vector3 direction = target.position - rb.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stopDistance)
            return;

        direction.Normalize();

        // Only control horizontal velocity.
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(
            currentVelocity.x, 0f, currentVelocity.z
        );

        Vector3 desiredVelocity = direction * moveSpeed;

        Vector3 velocityChange = desiredVelocity - horizontalVelocity;

        // Gradual acceleration, preserving physical knockback.
        Vector3 accelerationForce = Vector3.ClampMagnitude(
            velocityChange / Time.fixedDeltaTime,
            acceleration
        );

        rb.AddForce(accelerationForce, ForceMode.Acceleration);

        // Face the movement direction.
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Quaternion nextRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    public void ApplyKnockbackPause()
    {
        pauseUntil = Time.time + knockbackPause;
    }
}
