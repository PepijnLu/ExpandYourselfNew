
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement
{
    float baseMoveSpeed;
    float focusMoveSpeed;
    float moveSpeed;
    private Transform playerTransform;
    private Vector2 movement;
    bool inFocus;
    public PlayerMovement(Transform _transform, float _baseMoveSpeed, float _focusMoveSpeed)
    {
        playerTransform = _transform;
        baseMoveSpeed = _baseMoveSpeed;
        focusMoveSpeed = _focusMoveSpeed;

        moveSpeed = baseMoveSpeed;
    }

    public void Update()
    {
        // Get player input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
        Debug.Log($"Movement: {movement.x} , {movement.y}");


        bool inFocus = Input.GetKey(KeyCode.X);
        if(inFocus)
        {
            moveSpeed = focusMoveSpeed;
        }
        else
        {
            moveSpeed = baseMoveSpeed;
        }

       
    }

    public void PreviewUpdate(float _deltaTime, Dictionary<string, bool> _keysPressed)
    {
        Vector2 previewMovement = new();

        if (_keysPressed["Up"]) previewMovement.y += 1;
        if (_keysPressed["Down"]) previewMovement.y -= 1;

        if (_keysPressed["Right"]) previewMovement.x += 1;
        if (_keysPressed["Left"]) previewMovement.x -= 1;

        previewMovement = previewMovement.normalized;
        Debug.Log($"Movement Vector: {previewMovement}");

        previewMovement *= moveSpeed * Time.deltaTime;
        playerTransform.position += new Vector3(previewMovement.x, previewMovement.y, 0);
    }

    public void FixedUpdate()
    {
        // Move the player
        movement *= moveSpeed * Time.deltaTime;
        playerTransform.position += new Vector3(movement.x, movement.y, 0);
    }
}
