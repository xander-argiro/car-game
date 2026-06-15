using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    float ACCEL = 5f;
    float DECEL = 5f;
    float MAX_SPEED = 6f;
    float TURN_SPEED = 200f;

    float currentSpeed = 0f;
    bool gasPressed;
    float turnAmount;

    Rigidbody2D rb;

    Vector2 moveInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Forward-Backward movement
        float moveAmount = moveInput.y;
        if (moveAmount != 0)
        {
            gasPressed = true;
        }
        else
        {
            gasPressed = false;
        }

        if (gasPressed)
        {
            currentSpeed += moveAmount * ACCEL * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -MAX_SPEED, MAX_SPEED);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, DECEL * Time.fixedDeltaTime);
        }

        // Turn movement
        turnAmount = -moveInput.x * TURN_SPEED * Time.fixedDeltaTime;
        
        rb.MoveRotation(rb.rotation + turnAmount);
        rb.linearVelocity = transform.up * currentSpeed;
    }
}
