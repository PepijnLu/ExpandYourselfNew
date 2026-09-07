using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttack", menuName = "Scriptable Objects/EnemyAttack")]
public class EnemyAttack : ScriptableObject
{
    public int bulletAmount;
    public float bulletSpread;
    public float bulletStartAngle;
}
