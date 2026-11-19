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
        List<Bullet> newBullets = new();

        BulletPattern pattern = _enemyAttack.bulletPattern;
        pattern.Deserialize();

        Bullet bulletPrefab = pattern.bulletType;

        //Get the values from the bullet prefab
        float bulletAmount = pattern.bulletAmount;
        float bulletSpread = pattern.bulletSpread;
        float attackAngle = pattern.attackAngle;

        Vector3 bulletPosition = transform.position;
        Vector2 posOffset = _enemyAttack.positionOffset;

        float _attackBulletSpeed = bulletPrefab.bulletSpeed;
        int _attackBulletDamage = bulletPrefab.bulletDamage;
        float _attackBulletSize = bulletPrefab.gameObject.transform.localScale.x;

        //Create the initial dictionary with bullet values
        Dictionary<string, Value> bulletValues = new()
        {
            ["bulletSpeed"] = new FloatValue(_attackBulletSpeed),
            ["bulletDamage"] = new IntValue(_attackBulletDamage),
            ["bulletSize"] = new FloatValue(_attackBulletSize),
            ["bulletAngle"] = new FloatValue(attackAngle)
        };

        //Override the previous dictionary with values from shared bullets overrides (if set, otherwise wont appear here)
        //foreach (KeyValuePair<string, Value> _pair in pattern.sharedOverrides.dict)
        //{
        //    if (bulletValues.ContainsKey(_pair.Key))
        //    {
        //        bulletValues[_pair.Key] = _pair.Value;
        //    }
        //    else
        //    {
        //        bulletValues.Add(_pair.Key, _pair.Value);
        //    }
        //}

        //Position needs to be known before instantiating bullet, so do this manually
        float posOffsetX = 0;
        float posOffsetY = 0;

        Value posOffsetXValue = BulletValueDict.TryGetValue(bulletValues, "posOffsetX");
        if (posOffsetXValue is FloatValue _pox) posOffsetX = _pox.value;

        Value posOffsetYValue = BulletValueDict.TryGetValue(bulletValues, "posOffsetY");
        if (posOffsetYValue is FloatValue _poy) posOffsetY = _poy.value;

        posOffset += new Vector2(posOffsetX, posOffsetY);
        bulletPosition += new Vector3(posOffset.x, posOffset.y, 0);

        //Calculate bullet angle
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

        //Destroy old bullets?
        if (_destroyOld)
        {
            DestroyAllBullets(true);
        }


        //Create New Bullets
        for (int i = 0; i < bulletAmount; i++)
        {
            //Every bullet has a different angle in an attack regardless, so always check this manually
            Dictionary<string, Value> individualBulletValues = BulletValueDict.GetCopyOfDict(bulletValues);
            float bulletAngle = startAngle + i * angleStep;
            (individualBulletValues["bulletAngle"] as FloatValue).value = bulletAngle;

            Debug.Log($"Bullet speed values =  {(individualBulletValues["bulletSpeed"] as FloatValue).value}");

            //if(individualBulletValues.ContainsKey("angleOffset"))
            //{
            //    individualBulletValues["angleOffset"] = new FloatValue(angleOffset);
            //}
            //else
            //{
            //    individualBulletValues.Add("angleOffset", new FloatValue(angleOffset));
            //}

            //Check for individual overrides
            /*
            if (pattern.individualBulletOverrides != null)
            {
                if (i < pattern.individualBulletOverrides.Count)
                {
                    Debug.Log($"Found individual overrides: {pattern.individualBulletOverrides[i].Count}");
                    //Override the previous dictionary with values from individual bullets (if set, otherwise wont appear here)
                    foreach(KeyValuePair<string, Value> _pair in pattern.individualBulletOverrides[i])
                    {
                        if(bulletValues.ContainsKey(_pair.Key))
                        {
                            bulletValues[_pair.Key] = _pair.Value;
                        }
                        else
                        {
                            bulletValues.Add(_pair.Key, _pair.Value);
                        }
                    }

                    //Check for a poss offset, since we need that for spawning the bullet at the right position
                    if(bulletValues.TryGetValue("posOffset", out Value _value))
                    {
                        posOffset = (_value as Vector2Value).value;
                    }

                    //Increment bullet position by its position offset
                    bulletPosition += new Vector3(posOffset.x, posOffset.y, 0);   
                }
            }
            else Debug.LogWarning($"NO LIST FOUND FOR OVERRIDES");
            */

            //Instantiate bullet
            Bullet newBullet = CreateBullet(bulletPrefab, bulletPosition, individualBulletValues);
            newBullets.Add(newBullet);
        }

        //Invoke callback for tool
        _callback?.Invoke(newBullets);
    }

    private Bullet CreateBullet(Bullet _bulletPrefab, Vector3 _bulletPosition, Dictionary<string, Value> _bulletValues)
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
