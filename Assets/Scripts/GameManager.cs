using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public BossEnemy currentBoss;

    void Awake()
    {
        instance = this;
    }
}
