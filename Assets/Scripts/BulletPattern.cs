using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttack", menuName = "Scriptable Objects/EnemyAttack")]
public class BulletPattern : ScriptableObject
{
    public int bulletAmount;
    public float bulletSpread;
    public float attackAngle;

    public Bullet bulletType;

    public float bulletSpeedOverride;
    public int bulletDamageOverride;
    public float bulletSizeOverride;
}
