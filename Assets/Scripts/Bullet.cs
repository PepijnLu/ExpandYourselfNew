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

    private void Start()
    {
        debugStartPosition = transform.position;
        startTime = 0;
    }

    public void BulletUpdate(float _deltaTime)
    {
        transform.position += new Vector3(bulletDirection.x * bulletSpeed * _deltaTime, bulletDirection.y * bulletSpeed * _deltaTime, 0);

        startTime += _deltaTime;
    }

    public void InitializeBullet(BossEnemy _parent, float _bulletSpeed, int _bulletDamage, Vector2 _bulletDirection, Vector3 _bulletStartPosition)
    {
        parent = _parent;
        bulletSpeed = _bulletSpeed;
        bulletDamage = _bulletDamage;
        bulletDirection = _bulletDirection;
        bulletStartPosition = _bulletStartPosition;

        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.TryGetComponent(out IDamagable damagable))
        {
            damagable.TakeDamage(bulletDamage);
        }
        //Walls
        if (collider.gameObject.layer == 7)
        {
            parent.DestroyBullet(this);
        }
    }
}
