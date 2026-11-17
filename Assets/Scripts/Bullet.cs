using System;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    BossEnemy bossEnemy;
    GameObject owner;
    IFireable firee;

    int attackIndex;

    public float bulletSpeed;
    public int bulletDamage;
    public float range;

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

        if(range != 0)
        {
            float distanceFromStart = (transform.position - bulletStartPosition).magnitude;

            if (distanceFromStart > range)
            {
                firee.DestroyBullet(this);
            }
        }
    }

    public void InitializeBullet(GameObject _owner, BossEnemy _owningBossEnemy, IFireable _firee, float _bulletSpeed, int _bulletDamage, float _bulletSize, Vector2 _bulletDirection, Vector3 _bulletStartPosition, bool _inTool, BHECollisionManager _collisionManager, int _attackIndex)
    {
        inTool = _inTool;

        bossEnemy = _owningBossEnemy;
        bulletSpeed = _bulletSpeed;
        bulletDamage = _bulletDamage;
        bulletDirection = _bulletDirection;
        bulletStartPosition = _bulletStartPosition;
        attackIndex = _attackIndex;
        firee = _firee;

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
            if (other.TryGetComponent(out IDamagable damagable))
            {
                Debug.Log($"{owner.name} fired and hit {other.gameObject.name}");
                if(other.gameObject != owner)
                {
                    if(inTool)
                    {
                        bossEnemy.shouldPauseTool = true;
                        bossEnemy.hitAttackInPhaseIndex = attackIndex;
                    }
                    damagable.TakeDamage(bulletDamage);
                }
            }
        }
        catch (Exception e) 
        {
            //Debug.Log($"Collision with player failed: {e.Message}: {other}");
        }
        //Walls
        if (other.gameObject.layer == 8)
        {
            firee.DestroyBullet(this);
        }

        Debug.Log($"Collision with {other.gameObject.name}");
    }

    public void MarkForDestruction()
    {
        destructed = true;
        animator.SetTrigger("Destruct");
    }
}
