using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 bulletStartPosition;

    float bulletSpeed;
    public Vector2 bulletDirection;
    bool initialized;

    float startTime;
    bool didTheDebugThnig;
    Vector3 debugStartPosition;

    private void Start()
    {
        debugStartPosition = transform.position;
        startTime = 0;
        didTheDebugThnig = false;
    }
    // Update is called once per frame
    void Update()
    {
       
    }

    public void PreviewUpdate(float _deltaTime)
    {
        transform.position += new Vector3(bulletDirection.x * bulletSpeed * _deltaTime, bulletDirection.y * bulletSpeed * _deltaTime, 0);

        startTime += _deltaTime;

        if (startTime >= 1 && !didTheDebugThnig)
        {
            Vector3 displacement = transform.position - debugStartPosition;

            Debug.Log(
                $"Start: {debugStartPosition}\n" +
                $"End: {transform.position}\n" +
                $"Displacement: {displacement}\n" +
                $"Simulated time: {startTime}\n" +
                $"Speed: {bulletSpeed}"
            );

            didTheDebugThnig = true;
        }
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
