using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class Bullet : MonoBehaviour
{
    BossEnemy bossEnemy;
    GameObject bulletOwner;
    IFireable firee;

    int attackIndex;

    public float bulletSpeed;
    public int bulletDamage;
    public float bulletAngle;
    float angleOffset;
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

    Dictionary<string, Value> bulletValues;

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
        //if (!linearVariablesX.Equals(default(LinearVariables)))
        //{
        //    float extraDirection = mathUtil.SolveLinearEquation(linearVariablesX, timeAlive);
        //    newBulletDirection.x += extraDirection;
        //    newBulletDirection.Normalize();
        //}

        //if (!linearVariablesY.Equals(default(LinearVariables)))
        //{
        //    float extraDirection = mathUtil.SolveLinearEquation(linearVariablesY, timeAlive);
        //    newBulletDirection.y += extraDirection;
        //    newBulletDirection.Normalize();
        //}

        if (!sineVariablesX.Equals(default(LinearVariables)))
        {
            float sineEquation = mathUtil.SolveSineEquation(sineVariablesX, timeAlive);
            Debug.Log($"Sine Equation: {sineEquation}");
            float extraDirection = sineEquation;
            newBulletDirection.x += extraDirection;
            newBulletDirection.Normalize();
        }

        //if (!sineVariablesY.Equals(default(LinearVariables)))
        //{
        //    float extraDirection = (sineVariablesX.amp / 2) - mathUtil.SolveSineEquation(sineVariablesY, timeAlive);
        //    newBulletDirection.y += extraDirection;
        //    newBulletDirection.Normalize();
        //}

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

    public void InitializeBullet(GameObject _owner, BossEnemy _owningBossEnemy, IFireable _firee, Dictionary<string, Value> _bulletValues, bool _inTool, BHECollisionManager _collisionManager, int _attackIndex)
    {
        if (initialized) return;

        inTool = _inTool;
        bulletStartPosition = transform.position;

        bulletOwner = _owner;
        bossEnemy = _owningBossEnemy;
        firee = _firee;
        collisionManager = _collisionManager;
        attackIndex = _attackIndex;
        mathUtil = _owningBossEnemy.mathUtil;

        bulletValues = _bulletValues;

        //SHOULD ALWAYS HAVE A VALUE
        bulletSpeed = (bulletValues["bulletSpeed"] as FloatValue).value;
        bulletDamage = (bulletValues["bulletDamage"] as IntValue).value;
        bulletAngle = (bulletValues["bulletAngle"] as FloatValue).value;

        float bulletSize = (bulletValues["bulletSize"] as FloatValue).value;
        gameObject.transform.localScale = new Vector2(bulletSize, bulletSize);


        //CAN BE EMPTY
        Value angleOffsetValue = BulletValueDict.TryGetValue(bulletValues, "angleOffset");
        if (angleOffsetValue is FloatValue _ao) angleOffset = _ao.value;

        Value linearVarsXValue = BulletValueDict.TryGetValue(bulletValues, "linearVariablesX");
        if(linearVarsXValue is LinearVariablesValue _lvx) linearVariablesX = _lvx.value;

        Value linearVarsYValue = BulletValueDict.TryGetValue(bulletValues, "linearVariablesY");
        if (linearVarsYValue is LinearVariablesValue _lvy) linearVariablesY = _lvy.value;

        Value sineVarsXValue = BulletValueDict.TryGetValue(bulletValues, "sineVariablesX");
        if (sineVarsXValue is SineVariablesValue _svx) sineVariablesX = _svx.value;

        Value sineVarsYValue = BulletValueDict.TryGetValue(bulletValues, "sineVariablesY");
        if (sineVarsYValue is SineVariablesValue _svy) sineVariablesY = _svy.value;

        bulletAngle += angleOffset;
        bulletDirection = GetBulletDirection(bulletAngle);
        col = GetComponent<Collider2D>();
        initialized = true;
    }

    Vector2 GetBulletDirection(float _angle)
    {
        Quaternion directionRotation = Quaternion.Euler(0, 0, _angle);
        Vector2 direction = directionRotation * Vector2.down;

        return direction;
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
