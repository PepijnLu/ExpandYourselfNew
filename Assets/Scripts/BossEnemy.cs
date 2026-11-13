using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [SerializeField] List<EnemyPhase> phaseList;

    [SerializeField] Bullet tempBulletPrefab;

    public BHECollisionManager bheCollisionManager;

    int currentAttackInt;
    int totalPhaseAttacks;
    float attackTimer;

    public EnemyPhase currentPhase;
    public List<Bullet> bullets;
    private List<Bullet> bulletsToDestroy;

    public bool inTool;

    public bool shouldPauseTool = false;
    public int hitAttackInPhaseIndex;

    public int attackIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awaken()
    {
        currentPhase = phaseList[0];
        StartCoroutine(StartFight());
    }

    public void InitializeBoss(BHECollisionManager _bheCollisionManager)
    {
        bulletsToDestroy = new();
        bullets = new();
        inTool = true;
        bheCollisionManager = _bheCollisionManager;
    }

    IEnumerator StartFight()
    {
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "3");
        UIManager.instance.ToggleElement(UIManager.instance.heartsContainer.gameObject, true);
        UIManager.instance.ToggleElement(UIManager.instance.timerText.gameObject, true);
        yield return new WaitForSeconds(1);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "2");
        yield return new WaitForSeconds(1);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "1");
        yield return new WaitForSeconds(1);
        UIManager.instance.SetTMProString(UIManager.instance.timerText, "GO");
        yield return new WaitForSeconds(1);
        UIManager.instance.ToggleElement(UIManager.instance.timerText.gameObject, false);
        StartPhase(currentPhase);
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

        currentPhase = _newPhase;

        totalPhaseAttacks = currentPhase.attacks.Count;
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

    void HandlePhase(float _deltaTime, Action<List<Bullet>> _updateBulletCallback)
    {
        if (currentPhase == null) return;
        
        if (!(currentAttackInt > totalPhaseAttacks - 1))
        {
            EnemyAttack newAttack = currentPhase.attacks[currentAttackInt];

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
                    BulletPattern attack = newAttack.bulletPattern;
                    UpdateBulletCount(attack.bulletAmount, attack.bulletSpread, attack.bulletAngle, false, updateBulletCallback);
                    currentAttackInt++;
                    break;
            }

            if(inTool)
            {
                attackIndex = currentPhase.attacks.IndexOf(newAttack);
            }

            //Skip no wait time frames
            if (currentAttackInt < currentPhase.attacks.Count)
            {
                if ((currentPhase.attacks[currentAttackInt].waitTime == 0) || (currentPhase.attacks[currentAttackInt].attackType == AttackType.Attack) )
                {
                    HandlePhase(_deltaTime, updateBulletCallback);
                }
            }
        }
    }

    public void UpdateBulletCount(float _bulletAmount, float _bulletSpread, float _bulletStartAngle, bool _destroyOld, Action<List<Bullet>> _callback)
    {
        List<Bullet> newBullets = new();

        if (_destroyOld)
        {
            DestroyAllBullets(true);
        }

        float angleStep = 0;

        if (_bulletAmount == 1)
        {
            angleStep = 0;
        }
        else
        {
            angleStep = _bulletSpread / (_bulletAmount);
        }
        float startAngle = _bulletSpread / 2f;
        startAngle += _bulletStartAngle;

        //Create New Bullets
        for (int i = 0; i < _bulletAmount; i++)
        {
            float bulletAngle = startAngle + i * angleStep;

            Vector2 direction = new Vector2(
                Mathf.Cos(bulletAngle * Mathf.Deg2Rad),
                Mathf.Sin(bulletAngle * Mathf.Deg2Rad)
            );

            Quaternion bulletRotation =
                 Quaternion.Euler(0f, 0f, bulletAngle);

            Bullet newBullet = CreateBullet((transform.position + new Vector3(direction.x * 0.2f, direction.y * 0.2f, -1)), bulletRotation, .2f, _bulletStartAngle, Color.red);
            newBullets.Add(newBullet);
        }

        if(_callback != null) _callback.Invoke(newBullets);
    }

    private Bullet CreateBullet(Vector3 _bulletPosition, Quaternion _bulletRotation, float _bulletSize, float _bulletAngle, Color _color)
    {
        Bullet newBullet = Instantiate(tempBulletPrefab);

        newBullet.transform.localScale = new Vector2(_bulletSize, _bulletSize);
        newBullet.transform.position = _bulletPosition;
        newBullet.transform.rotation = _bulletRotation;
        newBullet.GetComponent<SpriteRenderer>().color = _color;

        Vector2 bulletDirection = GetBulletDirection(_bulletAngle, _bulletRotation);

        float bulletSpeed = 2;


        int currentAttackIndex = currentAttackInt;

        Debug.Log($"Attack Index: {currentAttackIndex}");

        newBullet.InitializeBullet(this, bulletSpeed, 1, bulletDirection, _bulletPosition, inTool, bheCollisionManager, currentAttackIndex);
        bullets.Add(newBullet);

        return newBullet;
    }

    public Vector2 GetBulletDirection(float _angle, Quaternion _bulletRotation)
    {
        Quaternion directionRotation = _bulletRotation * Quaternion.Euler(0, 0, _angle);

        Vector2 direction = directionRotation * Vector2.right;
        return direction;
    }

    public void DestroyBullet(Bullet _bullet)
    {
        if (inTool)
        {
            bulletsToDestroy.Add(_bullet);
        }
        else
        {
            bullets.Remove(_bullet);
            Destroy(_bullet.gameObject);
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
