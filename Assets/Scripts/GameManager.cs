using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector] public BossEnemy currentBoss;
    float timeLastFrame;

    float frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;
    float updateRate = 4.0f;  // 4 updates per sec.

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 144;
        timeLastFrame = Time.time;
    }

    public void Update()
    {
        ////seconds per frame
        //float deltaTime = Time.deltaTime;

        //float fps = 1 / deltaTime;

        //Debug.Log($"FPS: {fps}");

        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRate;
        }

        UIManager.instance.SetTMProString(UIManager.instance.fpsCounterText, $"FPS: {(int)fps}");
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
