using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPhase", menuName = "Scriptable Objects/EnemyPhase")]
public class EnemyPhase : ScriptableObject
{
    public float duration;
    public Vector2 movementDirection;

    public List<EnemyAttack> attacks;
    public List<float> attackCooldowns;
}
