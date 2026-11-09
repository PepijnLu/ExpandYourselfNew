
using UnityEngine;

public class PlayerMovement
{
    private float moveSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    public PlayerMovement(Rigidbody2D _rb, float _moveSpeed)
    {
        rb = _rb;
        moveSpeed = _moveSpeed;
    }

    public void Update()
    {
        // Get player input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize to prevent faster diagonal movement
        movement = movement.normalized;
    }

    public void FixedUpdate()
    {
        // Move the player
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
