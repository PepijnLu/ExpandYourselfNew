using NUnit.Framework;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
public class Weapon : MonoBehaviour, IFireable
{
    GameObject owner;
    List<Bullet> bullets;
    List<Bullet> bulletsToDestroy;
    [SerializeField] Bullet bullet;
    [SerializeField] float cooldown;
    float cooldownTimer;

    public void InitializeWeapon(Player _player)
    {
        bullets = new();
        bulletsToDestroy = new();
        owner = _player.gameObject;

        transform.parent = _player.gameObject.transform;
        transform.localPosition = Vector3.zero;

    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

    }

    private void FixedUpdate()
    {
        foreach (Bullet _bullet in bullets)
        {
            if(_bullet) _bullet.BulletUpdate(Time.deltaTime);
        }

        if (bulletsToDestroy.Count > 0)
        {
            foreach (Bullet _bullet in bulletsToDestroy)
            {
                bullets.Remove(_bullet);
                Destroy(_bullet.gameObject);
            }
            bulletsToDestroy.Clear();
        }
    }

    public void TryFire()
    {
        if (GameManager.instance.currentBoss == null) return;

        if(cooldownTimer >= cooldown)
        {
            Fire();
        }
    }

    void Fire()
    {
        cooldownTimer = 0;
        //get boss enemy differently here
        Vector2 bulletDirection = (FindAnyObjectByType<BossEnemy>().transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(bulletDirection.x, bulletDirection.y) * Mathf.Rad2Deg;

        Quaternion bulletRotation = Quaternion.Euler(new Vector3(0, 0, -angle));

        Bullet newBullet = Instantiate(bullet, transform.position, bulletRotation);
        bullets.Add(newBullet);

        BulletValues bulletValues = new()
        {
            bulletSpeed = bullet.bulletSpeed,
            bulletDamage = bullet.bulletDamage,
            bulletSize = bullet.transform.localScale.x,
            bulletDirection = bulletDirection,
        };

        newBullet.InitializeBullet(owner, null, this, bulletValues, false, null, 0);
        Debug.Log("FIRE");
    }

    void IFireable.DestroyBullet(Bullet _bullet)
    {
        bulletsToDestroy.Add(_bullet);
    }
}
