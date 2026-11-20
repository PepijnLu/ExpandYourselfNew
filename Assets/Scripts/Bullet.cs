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
    public float range;

    [HideInInspector] public Vector2 bulletDirection;
    [HideInInspector] public Vector3 bulletStartPosition;
    [HideInInspector] public Collider2D col;
    
    bool initialized;

    float startTime;
    float timeAlive;

    float angleOffset;

    //sine wave stuff
    float sineWaveAmp;
    float sineWaveFreq;
    float sineWavePhase;

    //linear stuff
    float linearCoefficient;
    float linearStart;

    BHECollisionManager collisionManager;

    bool inTool;
    bool destructed;
    Animator animator;

    Dictionary<string, Value> bulletValues;

    private void Start()
    {
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

        //Handle Linear in X
        if(linearCoefficient != 0)
        {
            float linearEquation = MathUtil.SolveLinearEquation(linearCoefficient, linearStart, timeAlive);
            float extraDirection = linearEquation;
            newBulletDirection.x += extraDirection;
            newBulletDirection.Normalize();

        }
        //Handle Sine Wave in X
        if(sineWaveAmp != 0 && sineWaveFreq != 0)
        {
            float sineEquation = MathUtil.SolveSineEquation(sineWaveAmp, sineWaveFreq, sineWavePhase, timeAlive);
            float extraDirection = sineEquation;
            newBulletDirection.x += extraDirection;
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
        bulletValues = _bulletValues;

        //Get position if you already have one
        bool noDirection = false;
        Vector2 newBulletDirection = Vector2.zero;

        Value bulletDirectionXValue = BulletValueDict.TryGetValue(bulletValues, "bulletDirectionX");
        if (bulletDirectionXValue is FloatValue _bdx)
        {
            newBulletDirection.x = _bdx.value;
        }
        else noDirection = true;

        Value bulletDirectionYValue = BulletValueDict.TryGetValue(bulletValues, "bulletDirectionY");
        if (bulletDirectionYValue is FloatValue _bdy)
        {
            newBulletDirection.y = _bdy.value;
        }
        else noDirection = true;

        //SHOULD ALWAYS HAVE A VALUE
        bulletSpeed = (bulletValues["bulletSpeed"] as FloatValue).value;
        bulletDamage = (bulletValues["bulletDamage"] as IntValue).value;
        if (noDirection)
        {
            bulletAngle = (bulletValues["bulletAngle"] as FloatValue).value;
            bulletAngle += angleOffset;
            bulletDirection = GetBulletDirection(bulletAngle);
        }
        else
        {
            bulletDirection = newBulletDirection;
        }

        float bulletSize = (bulletValues["bulletSize"] as FloatValue).value;
        gameObject.transform.localScale = new Vector2(bulletSize, bulletSize);


        //CAN BE EMPTY
        Value angleOffsetValue = BulletValueDict.TryGetValue(bulletValues, "angleOffset");
        if (angleOffsetValue is FloatValue _ao) angleOffset = _ao.value;

        //Linear
        Value linearCoefficientValue = BulletValueDict.TryGetValue(bulletValues, "linearCoefficient");
        if (linearCoefficientValue is FloatValue _lc) linearCoefficient = _lc.value;

        Value linearStartValue = BulletValueDict.TryGetValue(bulletValues, "linearStart");
        if (linearStartValue is FloatValue _ls) linearStart = _ls.value;

        //Sine wave
        Value sineWaveAmpValue = BulletValueDict.TryGetValue(bulletValues, "sineAmp");
        if (sineWaveAmpValue is FloatValue _swa) sineWaveAmp = _swa.value;

        Value sineWaveFreqValue = BulletValueDict.TryGetValue(bulletValues, "sineFreq");
        if (sineWaveFreqValue is FloatValue _swv) sineWaveFreq = _swv.value;

        Value sineWavePhaseValue = BulletValueDict.TryGetValue(bulletValues, "sinePhase");
        if (sineWavePhaseValue is FloatValue _swp) sineWavePhase = _swp.value;


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
    }

    public void MarkForDestruction()
    {
        destructed = true;
        animator.SetTrigger("Destruct");
    }
}
