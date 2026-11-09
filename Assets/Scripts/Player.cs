using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{
    public PlayerMovement playerMovement;
    Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] public float baseMoveSpeed;
    [SerializeField] public float focusMoveSpeed;
    [SerializeField] int health;
    Camera camera;

    public bool inTool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = new(transform, baseMoveSpeed, focusMoveSpeed);
        camera = Camera.main;
        UIManager.instance.UpdateMaxHealth(health);
    }

    // Update is called once per frame
    void Update()
    {
        playerMovement.Update();
    }

    void FixedUpdate()
    {
        playerMovement.FixedUpdate();
    }

    void IDamagable.TakeDamage(int _amount)
    {
        health = Mathf.Max((health - _amount), 0);
        UpdateHealth(health);

    }

    void UpdateHealth(int _newAmount)
    {
        UIManager.instance.UpdateHealth(_newAmount);
        Debug.Log($"New Player Health: {health}");
        if(health <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log($"PLAYER DEATH");
    }

    public void TeleportPlayer(Vector3 _teleportPosition, Vector3 _cameraPosition)
    {
        transform.position = _teleportPosition;
        camera.transform.position = new Vector3(_cameraPosition.x, _cameraPosition.y, -10);
    }
}
