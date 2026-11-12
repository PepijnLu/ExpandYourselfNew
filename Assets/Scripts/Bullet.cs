using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 bulletStartPosition;
    BossEnemy parent;

    float bulletSpeed;
    int bulletDamage;

    public Vector2 bulletDirection;
    bool initialized;

    float startTime;
    Vector3 debugStartPosition;

    public Collider2D col;
    BHECollisionManager collisionManager;
    bool inTool;

    private void Start()
    {
        debugStartPosition = transform.position;
        startTime = 0;
    }

    public void BulletUpdate(float _deltaTime)
    {
        transform.position += new Vector3(bulletDirection.x * bulletSpeed * _deltaTime, bulletDirection.y * bulletSpeed * _deltaTime, 0);

        startTime += _deltaTime;

        if(inTool)
        {
            try
            {
                collisionManager.UpdatePreviewCollisions(col, CollisionInPreview);
            }
            catch (Exception e)
            {
                //Debug.Log($"Cant Update Collisions: {e.Message} ({bheCollisionManager} , {bullet.col})");
            }
        }
    }

    public void InitializeBullet(BossEnemy _parent, float _bulletSpeed, int _bulletDamage, Vector2 _bulletDirection, Vector3 _bulletStartPosition, bool _inTool, BHECollisionManager _collisionManager)
    {
        inTool = _inTool;

        parent = _parent;
        bulletSpeed = _bulletSpeed;
        bulletDamage = _bulletDamage;
        bulletDirection = _bulletDirection;
        bulletStartPosition = _bulletStartPosition;

        initialized = true;

        collisionManager = _collisionManager;
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        CheckCollision(col, collider);
    }

    private void CollisionInPreview(Collider2D owner, Collider2D other)
    {
        CheckCollision(col, other);
    }

    private void CheckCollision(Collider2D owner, Collider2D other)
    {
        try
        {
            //In tool: player hit
            if (other.TryGetComponent(out IDamagable damagable))
            {
                parent.shouldPauseTool = true;
                damagable.TakeDamage(bulletDamage);
            }
        }
        catch (Exception e) 
        {
            //Debug.Log($"Collision with player failed: {e.Message}: {other}");
        }
        //Walls
        if (other.gameObject.layer == 7)
        {
            parent.DestroyBullet(this);
        }

    }
}
