using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 bulletStartPosition;

    float bulletSpeed;
    public Vector2 bulletDirection;
    bool initialized;

    // Update is called once per frame
    void Update()
    {
       
    }

    public void PreviewUpdate(float _deltaTime)
    {
        transform.position += new Vector3(bulletDirection.x * bulletSpeed * _deltaTime, bulletDirection.y * bulletSpeed * _deltaTime, 0);
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        transform.position += new Vector3(bulletDirection.x * bulletSpeed * Time.deltaTime, bulletDirection.y * bulletSpeed * Time.deltaTime, 0);
    }

    public void InitializeBullet(float _bulletSpeed, Vector2 _bulletDirection, Vector3 _bulletStartPosition)
    {
        bulletSpeed = _bulletSpeed;
        bulletDirection = _bulletDirection;
        bulletStartPosition = _bulletStartPosition;

        Debug.Log($"Bullet Start Pos: {bulletStartPosition}");
        initialized = true;
    }
}
