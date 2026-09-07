using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    EnemyPhase currentPhase;
    float timeInCurrentPhase;
    float attackTimer;
    int timesAttackedInPhase;

    [SerializeField] List<EnemyPhase> phaseList;
    [SerializeField] float baseMoveSpeed;

    [SerializeField] Bullet bullet;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AdvancePhase();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPhase != null)
        {
            timeInCurrentPhase += Time.deltaTime;
            attackTimer += Time.deltaTime;

            Debug.Log($"PhaseTime: {timeInCurrentPhase}, AttackTime: {attackTimer}");
        }
    }

    void FixedUpdate()
    {
        BossMovement();
        BossAttack();
        CheckPhaseTime();
    }

    void BossMovement()
    {
        if (currentPhase == null) return;

        Vector3 movement = new Vector3(currentPhase.movementDirection.x * baseMoveSpeed * Time.deltaTime, currentPhase.movementDirection.y * baseMoveSpeed * Time.deltaTime, 0);
        transform.position += movement;
    }

    void BossAttack()
    {
        if (currentPhase == null) return;
        if (timesAttackedInPhase >= currentPhase.attackCooldowns.Count) return;

        if (attackTimer < currentPhase.attackCooldowns[timesAttackedInPhase]) return;

        EnemyAttack attack = currentPhase.attacks[timesAttackedInPhase];

        float angleStep = attack.bulletSpread / (attack.bulletAmount - 1);
        float startAngle = -attack.bulletSpread / 2f;
        startAngle += attack.bulletStartAngle;

        for (int i = 0; i < attack.bulletAmount; i++)
        {
            float bulletAngle = startAngle + i * angleStep;
            Vector2 bulletDirection = Quaternion.Euler(0, 0, bulletAngle) * transform.right;

            Bullet newBullet = Instantiate(bullet, transform.position, transform.rotation);
            newBullet.InitializeBullet(2f, bulletDirection);
        }

        attackTimer = 0;
        timesAttackedInPhase++;
    }

    void CheckPhaseTime()
    {
        if (currentPhase == null) return;

        if (timeInCurrentPhase >= currentPhase.duration)
        {
            AdvancePhase();
        }
    }

    void AdvancePhase()
    {
        EnemyPhase nextPhase = phaseList[Random.Range(0, phaseList.Count)];
        if (nextPhase == null) return;

        timeInCurrentPhase = 0;
        attackTimer = 0;
        timesAttackedInPhase = 0;

        currentPhase = nextPhase;
    }
}
