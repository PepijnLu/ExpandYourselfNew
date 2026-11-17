using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector] public BossEnemy currentBoss;

    void Awake()
    {
        instance = this;
    }

    public void StartBossfight(BossEnemy _newBoss)
    {
        currentBoss = _newBoss;

        UIManager.instance.ToggleElement(UIManager.instance.bossUI, true);
        UIManager.instance.ToggleElement(UIManager.instance.timerText.gameObject, true);

        UIManager.instance.UpdateBossHealthBar(currentBoss.startHPPercent, currentBoss.currentHP, currentBoss.totalHP);
        UIManager.instance.SetTMProString(UIManager.instance.bossNameText, currentBoss.gameObject.name);
    }

    public void EndBossfight()
    {
        currentBoss = null;
        UIManager.instance.ToggleElement(UIManager.instance.bossUI, false);
    }
}
