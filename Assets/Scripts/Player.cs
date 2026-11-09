using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{

    [SerializeField] UIManager uiManager;

    PlayerMovement playerMovement;
    Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] float moveSpeed;
    [SerializeField] int health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = new(rb, moveSpeed);

        uiManager.UpdateMaxHealth(health);
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
        uiManager.UpdateHealth(_newAmount);
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
}
