using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BossEnemy : MonoBehaviour, IDamagable, IFireable
{
    [SerializeField] public float totalHP;
    [SerializeField] public float startHPPercent;
    [SerializeField] private List<BossPhase> bossPhases;
    [SerializeField] Teleporter teleporterPrefab;
    public float currentHP;
    private bool damageCapped;
    private int currentAttackInt;
    private int totalPhaseAttacks;
    private float attackTimer;
    private List<Bullet> bulletsToDestroy;
    private BossPhase currentBossPhase;
    [HideInInspector] public EnemyPhase currentAttackPhase;
    [HideInInspector] public List<Bullet> bullets;
    [HideInInspector] public BHECollisionManager bheCollisionManager;

    [HideInInspector] public bool inTool;

    [HideInInspector] public bool shouldPauseTool = false;
    [HideInInspector] public int hitAttackInPhaseIndex;

    [HideInInspector] public int attackIndex;
    [HideInInspector] public MathUtil mathUtil;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awaken()
    {
        currentHP = totalHP * (startHPPercent / 100);
        damageCapped = true;
        StartCoroutine(StartFight());
    }

    public void InitializeBoss(BHECollisionManager _bheCollisionManager, MathUtil _mathUtil)
    {
        bulletsToDestroy = new();
        bullets = new();
        inTool = true;
        bheCollisionManager = _bheCollisionManager;
        mathUtil = _mathUtil;
    }

    IEnumerator StartFight()
    {
        GameManager.instance.StartBossfight(this);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "3");
        yield return new WaitForSeconds(1);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "2");
        yield return new WaitForSeconds(1);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "1");
        yield return new WaitForSeconds(1);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "GO");
        yield return new WaitForSeconds(1);
        UIManager.instance.ToggleElement(UIManager.instance.timerText.gameObject, false);
        CheckStagger();
    }

    // Update is called once per frame
    void Update()
    {
        HandlePhase(Time.deltaTime, null);

        foreach (Bullet bullet in bullets)
        {
            bullet.BulletUpdate(Time.deltaTime);
            
        }
    }

    public void StartPhase(EnemyPhase _newPhase)
    {
        if (_newPhase == null) return;

        currentAttackPhase = _newPhase;

        totalPhaseAttacks = currentAttackPhase.attacks.Count;
        currentAttackInt = 0;
        attackTimer = 0;
    }

    public void PreviewUpdate(float _deltaTime, Action<List<Bullet>> _updateBulletCallback)
    {
        HandlePhase(_deltaTime, _updateBulletCallback);

        foreach (Bullet bullet in bullets)
        {
            if (!bullet) continue;

            bullet.BulletUpdate(_deltaTime);
        }

        ClearBullets();
    }

    public void Death()
    {
        GameManager.instance.EndBossfight();

        //do this better
        Transform startingRoom = GameObject.Find("StartingRoom").transform;
        Teleporter newTeleporter = Instantiate(teleporterPrefab, transform.position, Quaternion.identity);

        newTeleporter.teleportTarget = startingRoom;
        newTeleporter.cameraTarget = startingRoom;

        foreach(Bullet _bullet in bullets)
        {
            Destroy(_bullet.gameObject);
        }
        
        Destroy(gameObject);
    }

    void IDamagable.TakeDamage(float _amount)
    {
        if (damageCapped) return;

        currentHP -= _amount;

        if(currentHP <= 0)
        {
            currentHP = 0;
            Death();
        }
        else
        {
            CheckStagger();
        }

        UIManager.instance.UpdateBossHealthBar(GetHPPercent(), currentHP, totalHP);
    }

    void HandlePhase(float _deltaTime, Action<List<Bullet>> _updateBulletCallback)
    {
        if (currentAttackPhase == null) return;
        
        if (!(currentAttackInt > totalPhaseAttacks - 1))
        {
            EnemyAttack newAttack = currentAttackPhase.attacks[currentAttackInt];

            Action<List<Bullet>> updateBulletCallback = null;
            if (inTool) updateBulletCallback = _updateBulletCallback;

            switch (newAttack.attackType)
            {
                case AttackType.Wait:
                    attackTimer += _deltaTime;

                    if (attackTimer >= newAttack.waitTime)
                    {
                        currentAttackInt++;
                        attackTimer = 0;
                    }
                    break;
                case AttackType.Attack:
                    UpdateBulletCount(newAttack, false, updateBulletCallback);
                    currentAttackInt++;
                    break;
            }

            if(inTool)
            {
                attackIndex = currentAttackPhase.attacks.IndexOf(newAttack);
            }

            //Skip no wait time frames
            if (currentAttackInt < currentAttackPhase.attacks.Count)
            {
                if ((currentAttackPhase.attacks[currentAttackInt].waitTime == 0) || (currentAttackPhase.attacks[currentAttackInt].attackType == AttackType.Attack) )
                {
                    HandlePhase(_deltaTime, updateBulletCallback);
                }
            }
        }
        else
        {
            currentAttackPhase = null;
            StartCoroutine(StartPhaseAfterTime(GetRandomEnemyPhase(), .5f));
        }
    }

    IEnumerator StartPhaseAfterTime(EnemyPhase _newPhase, float _time)
    {
        while(bullets.Count > 0) yield return null;
        yield return new WaitForSeconds(_time);
        StartPhase(_newPhase);
    }

    private void CheckStagger()
    {
        BossPhase newBossPhase = GetNewBossPhase();
        if ((newBossPhase != null) && (newBossPhase != currentBossPhase))
        {
            currentBossPhase = newBossPhase;
            damageCapped = true;

            StartCoroutine(StaggerPhase());
        }
    }

    private IEnumerator StaggerPhase()
    {
        //return to start position? 

        //animations / sprite change for new phase

        //Choose new attack phase
        EnemyPhase newPhase = GetRandomEnemyPhase();

        //Become un-damage capped
        damageCapped = false;

        //Start new phase
        StartPhase(newPhase);

        yield return null;
    }

    private BossPhase GetNewBossPhase()
    {
        BossPhase newBossPhase = null;
        float lowestThreshold = 101f;

        foreach(BossPhase _phase in bossPhases)
        {
            if((_phase.hpPercentThreshold >= GetHPPercent()) && (_phase.hpPercentThreshold <= lowestThreshold))
            {
                newBossPhase = _phase;
                lowestThreshold = _phase.hpPercentThreshold;
            }
        }

        return newBossPhase;
    }

    private EnemyPhase GetRandomEnemyPhase()
    {
        if (currentBossPhase == null)
        {
            Debug.LogWarning($"No current boss phase");
            return null;
        }
        if (currentBossPhase.enemyPhases == null)
        {
            Debug.LogWarning($"Current boss phase at index {bossPhases.IndexOf(currentBossPhase)} has no list");
            return null;
        }
        if (currentBossPhase.enemyPhases.Count == 0)
        {
            Debug.LogWarning($"Current boss phase at index {bossPhases.IndexOf(currentBossPhase)} has no phases");
            return null;
        }

        int possiblePhasesCount = currentBossPhase.enemyPhases.Count;

        int randomPhaseInt = UnityEngine.Random.Range(0, possiblePhasesCount);
        EnemyPhase randomPhase = currentBossPhase.enemyPhases[randomPhaseInt];

        return randomPhase;
    }

    public float GetHPPercent()
    {
        return (currentHP / totalHP) * 100;
    }

    public void UpdateBulletCount(EnemyAttack _enemyAttack, bool _destroyOld, Action<List<Bullet>> _callback)
    {
        Debug.Log($"UPDATE BULLET COUNT");

        List<Bullet> newBullets = new();

        BulletPattern pattern = _enemyAttack.bulletPattern;
        Bullet bulletPrefab = pattern.bulletType;

        float bulletAmount = pattern.bulletAmount;
        float bulletSpread = pattern.bulletSpread;
        float attackAngle = pattern.attackAngle;

        Vector2 attackPosOffset = _enemyAttack.positionOffset;

        float _attackBulletSpeed = bulletPrefab.bulletSpeed;
        int _attackBulletDamage = bulletPrefab.bulletDamage;
        float _attackBulletSize = bulletPrefab.gameObject.transform.localScale.x;

        //Check for override for all bullets in attack
        if (pattern.allBulletsOverrideValues.bulletSpeed != 0) _attackBulletSpeed = pattern.allBulletsOverrideValues.bulletSpeed;
        if (pattern.allBulletsOverrideValues.bulletDamage != 0) _attackBulletDamage = pattern.allBulletsOverrideValues.bulletDamage;
        if (pattern.allBulletsOverrideValues.bulletSize != 0) _attackBulletSize = pattern.allBulletsOverrideValues.bulletSize;

        attackAngle += pattern.allBulletsOverrideValues.bulletAngle;
        attackPosOffset += pattern.allBulletsOverrideValues.posOffset;

        if (_destroyOld)
        {
            DestroyAllBullets(true);
        }

        float angleStep;

        if (bulletAmount == 1)
        {
            angleStep = 0;
        }
        else
        {
            angleStep = bulletSpread / (bulletAmount);
        }
        float startAngle = (bulletSpread / 2f) + _enemyAttack.angleOffset;
        startAngle += attackAngle;

        //Create New Bullets
        for (int i = 0; i < bulletAmount; i++)
        {
            float _bulletSpeed = _attackBulletSpeed;
            int _bulletDamage = _attackBulletDamage;
            float _bulletSize = _attackBulletSize;

            float bulletAngle = startAngle + i * angleStep;

            Vector2 bulletDirection = GetBulletDirection(bulletAngle);

            Vector3 bulletPosition = transform.position + new Vector3(attackPosOffset.x, attackPosOffset.y, 0);

            //Check for individual overrides
            if (pattern.individualBulletOverrides != null)
            {
                if (i < pattern.individualBulletOverrides.Count)
                {
                    BulletValues values = pattern.individualBulletOverrides[i];

                    if (values.bulletSpeed != 0) _bulletSpeed = values.bulletSpeed;
                    if (values.bulletDamage != 0) _bulletDamage = values.bulletDamage;
                    if (values.bulletSize != 0) _bulletSize = values.bulletSize;

                    bulletPosition += new Vector3(values.posOffset.x, values.posOffset.y, 0);
                    
                    if(values.bulletAngle != 0)
                    {
                        float newBulletAngle = bulletAngle + values.bulletAngle;
                        bulletDirection = GetBulletDirection(newBulletAngle);
                    }
                }
            }
            else Debug.LogWarning($"NO LIST FOUND FOR OVERRIDES");

            BulletValues overrideValues = new()
            {
                bulletSpeed = _bulletSpeed,
                bulletDamage = _bulletDamage,
                bulletSize = _bulletSize,
                bulletDirection = bulletDirection
            };

            Bullet newBullet = CreateBullet(bulletPrefab, bulletPosition, overrideValues);
            newBullets.Add(newBullet);
        }

        _callback?.Invoke(newBullets);
    }

    private Bullet CreateBullet(Bullet _bulletPrefab, Vector3 _bulletPosition, BulletValues _bulletValues)
    {
        Bullet newBullet = Instantiate(_bulletPrefab, _bulletPosition, Quaternion.identity);

        newBullet.InitializeBullet(gameObject, this, this, _bulletValues, inTool, bheCollisionManager, currentAttackInt);
        
        bullets.Add(newBullet);

        return newBullet;
    }

    public Vector2 GetBulletDirection(float _angle)
    {
        Quaternion directionRotation = Quaternion.Euler(0, 0, _angle);
        Vector2 direction = directionRotation * Vector2.down;

        return direction;
    }

    void IFireable.DestroyBullet(Bullet _bullet)
    {
        DestroyBullet(_bullet);
    }

    public void DestroyBullet(Bullet _bullet)
    {
        if (inTool)
        {
            bulletsToDestroy.Add(_bullet);
        }
        else
        {
            Destroy(_bullet.gameObject);
            bullets.Remove(_bullet);
        }
    }


    public void DestroyAllBullets(bool _immediatelyDestroy)
    {
        
        foreach (Bullet bullet in bullets)
        {
            DestroyBullet(bullet);
        }
        bullets.Clear();

        if (_immediatelyDestroy) ClearBullets();
    }

    private void ClearBullets()
    {
        if(!inTool)
        {
            throw new Exception("Can't destroy immediate in play mode");
        }

        foreach (Bullet bullet in bulletsToDestroy)
        {
            if (!bullet) continue;

            bullets.Remove(bullet);
            DestroyImmediate(bullet.gameObject);
            Debug.Log("Clear Bullet");
        }

        bulletsToDestroy.Clear();

    }
}
