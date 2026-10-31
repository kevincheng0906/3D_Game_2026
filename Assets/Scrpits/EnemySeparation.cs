
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemySeparation : MonoBehaviour
{
    [Header("Separation")]
    public float separationRadius = 1.5f;
    public float separationStrength = 8f;
    public LayerMask enemyLayer;

    private Rigidbody rb;

    private readonly Collider[] nearby =
        new Collider[32];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            separationRadius,
            nearby,
            enemyLayer,
            QueryTriggerInteraction.Ignore
        );

        Vector3 separation = Vector3.zero;
        int neighbors = 0;

        for (int i = 0; i < count; i++)
        {
            Rigidbody otherRb =
                nearby[i].attachedRigidbody;

            if (otherRb == null || otherRb == rb)
                continue;

            Vector3 away =
                rb.position - otherRb.position;

            away.y = 0f;

            float distance = away.magnitude;

            if (distance < 0.001f ||
                distance >= separationRadius)
                continue;

            float weight =
                1f - distance / separationRadius;

            separation += away.normalized * weight;
            neighbors++;
        }

        if (neighbors == 0)
            return;

        Vector3 force =
            separation / neighbors * separationStrength;

        rb.AddForce(
            force,
            ForceMode.Acceleration
        );
    }
}
