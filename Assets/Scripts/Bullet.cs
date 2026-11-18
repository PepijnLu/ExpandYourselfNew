using System;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    BossEnemy bossEnemy;
    GameObject bulletOwner;
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
    float timeAlive;
    Vector3 debugStartPosition;

    BHECollisionManager collisionManager;

    bool inTool;
    bool destructed;
    Animator animator;
    MathUtil mathUtil;
    SineVariables sineVariablesX, sineVariablesY;
    LinearVariables linearVariablesX, linearVariablesY;

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
        Vector2 newBulletDirection = bulletDirection;
        if (!linearVariablesX.Equals(default(LinearVariables)))
        {
            float extraDirection = mathUtil.SolveLinearEquation(linearVariablesX, timeAlive);
            newBulletDirection.x += extraDirection;
            newBulletDirection.Normalize();
        }

        if (!linearVariablesY.Equals(default(LinearVariables)))
        {
            float extraDirection = mathUtil.SolveLinearEquation(linearVariablesY, timeAlive);
            newBulletDirection.y += extraDirection;
            newBulletDirection.Normalize();
        }

        if (!sineVariablesX.Equals(default(LinearVariables)))
        {
            float extraDirection = mathUtil.SolveSineEquation(sineVariablesX, timeAlive);
            newBulletDirection.x += extraDirection;
            newBulletDirection.Normalize();
        }

        if (!sineVariablesY.Equals(default(LinearVariables)))
        {
            float extraDirection = mathUtil.SolveSineEquation(sineVariablesY, timeAlive);
            newBulletDirection.y += extraDirection;
            newBulletDirection.Normalize();
        }

        transform.position += new Vector3(newBulletDirection.x * bulletSpeed * _deltaTime, newBulletDirection.y * bulletSpeed * _deltaTime, 0);

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

        timeAlive += _deltaTime;
    }

    public void InitializeBullet(GameObject _owner, BossEnemy _owningBossEnemy, IFireable _firee, float _bulletSpeed, int _bulletDamage, float _bulletSize, Vector2 _bulletDirection, Vector3 _bulletStartPosition, bool _inTool, BHECollisionManager _collisionManager, int _attackIndex)
    {
        inTool = _inTool;

        bulletOwner = _owner;
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
        mathUtil = _owningBossEnemy.mathUtil;
        //testinggg
        //linearVariablesX = new()
        //{
        //    coefficient = -.1f,
        //    start = 0
        //};
        //testinggg
        //linearVariablesY = new()
        //{
        //    coefficient = .2f,
        //    start = 0
        //};
        sineVariablesX = new()
        {
            amp = 2f,
            freq = 5f,
            phase = 0
        };

        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (destructed) return;

        CheckCollision(col, collider);
    }

    private void CollisionEnterInPreview(Collider2D _owner, Collider2D _other)
    {
        CheckCollision(col, _other);
    }

    private void CheckCollision(Collider2D _owner, Collider2D _other)
    {
        Debug.Log($"{bulletOwner.name} fired and hit {_other.gameObject.name}");
        try
        {
            if (_other.TryGetComponent(out IDamagable damagable))
            {
                if(_other.gameObject != bulletOwner)
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
        if (_other.gameObject.layer == 8)
        {
            firee.DestroyBullet(this);
        }

        Debug.Log($"Collision with {_other.gameObject.name}");
    }

    public void MarkForDestruction()
    {
        destructed = true;
        animator.SetTrigger("Destruct");
    }
}
