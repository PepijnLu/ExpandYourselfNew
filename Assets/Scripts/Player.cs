using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{
    public PlayerMovement playerMovement;
    BHECollisionManager bheCollisionManager;
    Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] public float baseMoveSpeed;
    [SerializeField] public float focusMoveSpeed;
    [SerializeField] int health;
    Camera cam;
    Collider2D col;

    public bool inTool;
    List<Collider2D> overlappingBlockingColliders;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializePlayer(null, false);
    }

    public void InitializePlayer(BHECollisionManager _bheCollisionManager, bool _inTool)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        inTool = _inTool;
        bheCollisionManager = _bheCollisionManager;

        playerMovement = new(this, transform, baseMoveSpeed, focusMoveSpeed);

        try
        {
            cam = Camera.main;
            UIManager.instance.UpdateMaxHealth(health);
        }
        catch (Exception e)
        {
            Debug.Log($"Player Start Error: {e.Message}");
        }

        overlappingBlockingColliders = new();
    }

    // Update is called once per frame
    void Update()
    {
        playerMovement.Update();
    }

    void FixedUpdate()
    {
        playerMovement.FixedUpdate();
    }

    public void PreviewUpdate(float _deltaTime)
    {
        bheCollisionManager.UpdatePreviewCollisions(col, CollisionEnterInPreview, CollisionExitInPreview);
    }

    void IDamagable.TakeDamage(int _amount)
    {
        health = Mathf.Max((health - _amount), 0);
        UpdateHealth(health);

        Debug.Log("PLAYER HIT");
    }

    void UpdateHealth(int _newAmount)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(_newAmount);
        }
        Debug.Log($"New Player Health: {health}");
        if (health <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log($"PLAYER DEATH");
    }

    public void TeleportPlayer(Vector3 _teleportPosition, Vector3 _cameraPosition)
    {
        transform.position = _teleportPosition;
        cam.transform.position = new Vector3(_cameraPosition.x, _cameraPosition.y, -10);
    }

    public List<Collider2D> GetOverlappingBlockingColliders()
    {
        return overlappingBlockingColliders;
    }

    void CollisionEnterInPreview(Collider2D owner, Collider2D other)
    {
        if(!other.isTrigger)
        {
            if (!overlappingBlockingColliders.Contains(other)) overlappingBlockingColliders.Add(other);
        }
    }

    void CollisionExitInPreview(Collider2D owner, Collider2D other)
    {
        if (!other.isTrigger)
        {
            if (overlappingBlockingColliders.Contains(other)) overlappingBlockingColliders.Remove(other);
        }
    }
}
