using UnityEngine;
using UnityEngine.InputSystem;
public class HorseController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 25f;
    public float maxSpeed = 8f;
    public float turnSpeed = 10f;

    private Rigidbody rb;
    private Vector3 inputDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
{
    Vector2 movement = Vector2.zero;

    if (Keyboard.current == null)
        return;

    if (Keyboard.current.wKey.isPressed) movement.y += 1;
    if (Keyboard.current.sKey.isPressed) movement.y -= 1;
    if (Keyboard.current.aKey.isPressed) movement.x -= 1;
    if (Keyboard.current.dKey.isPressed) movement.x += 1;

    inputDirection = new Vector3(
        movement.x, 0f, movement.y
    ).normalized;
}

void FixedUpdate()
    {
        Move();
        Rotate();
        LimitSpeed();
    }

    void Move()
    {
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            rb.AddForce(
                inputDirection * acceleration,
                ForceMode.Acceleration
            );
        }
    }

    void Rotate()
    {
        Vector3 horizontalVelocity =
            new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.sqrMagnitude < 0.1f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(horizontalVelocity.normalized);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    void LimitSpeed()
    {
        Vector3 horizontalVelocity =
            new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity =
                horizontalVelocity.normalized * maxSpeed;

            rb.linearVelocity = new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
        }
    }
}