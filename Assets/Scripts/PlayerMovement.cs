
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement
{
    Player player;
    float baseMoveSpeed;
    float focusMoveSpeed;
    float moveSpeed;
    private Transform playerTransform;
    private Vector2 movement;
    bool inFocus;
    public PlayerMovement(Player _player, Transform _transform, float _baseMoveSpeed, float _focusMoveSpeed)
    {
        player = _player;
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

        List<Collider2D> overlappingBlockingColliders = player.GetOverlappingBlockingColliders();

        //NOTE: WALL COLLISION ONLY WORKS WITH 90 DEGREE ANGLES AND RECTANGLES
        foreach (Collider2D collider in overlappingBlockingColliders)
        {
            Vector3 objectDirection = collider.gameObject.transform.up;

            //Left Wall, Facing Right
            if (objectDirection.x > .9f)
            {
                previewMovement.x = Mathf.Max(0, previewMovement.x);
            }
            //Right Wall, Facing Left
            else if (objectDirection.x < -.9f)
            {
                previewMovement.x = Mathf.Min(0, previewMovement.x);
            }
            //Bottom Wall, Facing Up
            else if (objectDirection.y > .9f)
            {
                previewMovement.y = Mathf.Max(0, previewMovement.y);
            }
            //Top Wall, Facing Down
            else if (objectDirection.y < -.9f)
            {
                previewMovement.y = Mathf.Min(0, previewMovement.y);
            }
        }
        previewMovement = previewMovement.normalized;

        //Debug.Log($"Movement Vector: {previewMovement}");

        inFocus = _keysPressed["Focus"];

        if (inFocus)
        {
            moveSpeed = focusMoveSpeed;
        }
        else
        {
            moveSpeed = baseMoveSpeed;
        }

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
