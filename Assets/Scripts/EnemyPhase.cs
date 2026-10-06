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
