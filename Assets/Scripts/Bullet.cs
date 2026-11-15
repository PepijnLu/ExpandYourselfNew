using System;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    BossEnemy parent;

    int attackIndex;

    [SerializeField] float bulletSpeed;
    [SerializeField] int bulletDamage;

    [HideInInspector] public Vector2 bulletDirection;
    [HideInInspector] public Vector3 bulletStartPosition;
    [HideInInspector] public Collider2D col;
    
    bool initialized;

    float startTime;
    Vector3 debugStartPosition;

    BHECollisionManager collisionManager;

    bool inTool;
    bool destructed;
    Animator animator;

    private void Start()
    {
        debugStartPosition = transform.position;
        startTime = 0;

        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (animator && destructed)
        {
            //lowk terrible way of doing this
            if (AnimatorIsPlaying())
            {
                Destroy(gameObject);
            }
        }
        if (destructed) return;
    }

    bool AnimatorIsPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length >
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public void BulletUpdate(float _deltaTime)
    {
        transform.position += new Vector3(bulletDirection.x * bulletSpeed * _deltaTime, bulletDirection.y * bulletSpeed * _deltaTime, 0);

        startTime += _deltaTime;

        if(inTool)
        {
            try
            {
                collisionManager.UpdatePreviewCollisions(col, CollisionEnterInPreview, null);
            }
            catch (Exception e)
            {
                //Debug.Log($"Cant Update Collisions: {e.Message} ({bheCollisionManager} , {bullet.col})");
            }
        }
    }

    public void InitializeBullet(BossEnemy _parent, float _bulletSpeed, int _bulletDamage, float _bulletSize, Vector2 _bulletDirection, Vector3 _bulletStartPosition, bool _inTool, BHECollisionManager _collisionManager, int _attackIndex)
    {
        inTool = _inTool;

        parent = _parent;
        bulletSpeed = _bulletSpeed;
        bulletDamage = _bulletDamage;
        bulletDirection = _bulletDirection;
        bulletStartPosition = _bulletStartPosition;
        attackIndex = _attackIndex;

        gameObject.transform.localScale = new Vector2(_bulletSize, _bulletSize);

        collisionManager = _collisionManager;
        col = GetComponent<Collider2D>();
        
        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (destructed) return;

        CheckCollision(col, collider);
    }

    private void CollisionEnterInPreview(Collider2D owner, Collider2D other)
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
                parent.hitAttackInPhaseIndex = attackIndex;
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

        Debug.Log($"Collision with {other.gameObject.name}");
    }

    public void MarkForDestruction()
    {
        destructed = true;
        animator.SetTrigger("Destruct");
    }
}
