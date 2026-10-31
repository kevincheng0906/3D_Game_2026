
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 12f, -8f);
    public float followSpeed = 8f;

    private float fixedHeight;

    void Start()
    {
        fixedHeight = target.position.y;
        transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Follow only horizontal movement.
        Vector3 targetPosition = new Vector3(
            target.position.x,
            fixedHeight,
            target.position.z
        ) + offset;

        float smoothFactor =
            1f - Mathf.Exp(-followSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothFactor
        );
    }
}
