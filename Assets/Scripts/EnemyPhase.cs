using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Wait,
    Attack
}

[CreateAssetMenu(fileName = "EnemyPhase", menuName = "Scriptable Objects/EnemyPhase")]
public class EnemyPhase : ScriptableObject
{
    public float duration;
    public Vector2 movementDirection;

    public List<EnemyAttack> attacks;
}

[System.Serializable]
public class BossPhase
{
    //Below this % this phase will activate. Can only be 1 at a time
    public float hpPercentThreshold;
    //Enemy phases in this boss phase
    public List<EnemyPhase> enemyPhases;
}
