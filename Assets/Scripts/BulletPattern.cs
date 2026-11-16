using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttack", menuName = "Scriptable Objects/EnemyAttack")]
public class BulletPattern : ScriptableObject
{
    public int bulletAmount;
    public float bulletSpread;
    public float attackAngle;

    public Bullet bulletType;

    public BulletValues allBulletsOverrideValues;

    public List<BulletValues> individualBulletOverrides = new();
}
