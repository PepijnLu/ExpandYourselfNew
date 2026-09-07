using UnityEngine;

public class Bullet : MonoBehaviour
{
    float bulletSpeed;
    Vector2 bulletDirection;
    bool initialized;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        transform.position += new Vector3(bulletDirection.x * bulletSpeed * Time.deltaTime, bulletDirection.y * bulletSpeed * Time.deltaTime, 0);
    }

    public void InitializeBullet(float _bulletSpeed, Vector2 _bulletDirection)
    {
        bulletSpeed = _bulletSpeed;
        bulletDirection = _bulletDirection;

        initialized = true;
    }
}
