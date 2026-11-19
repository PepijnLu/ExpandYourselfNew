using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BulletValues
{
    public float bulletSpeed;
    public int bulletDamage;
    public float bulletSize;
    public Vector2 bulletDirection;
    public float bulletAngle;
    public Vector2 posOffset;

    public LinearVariables linearVariablesX, linearVariablesY;
    public SineVariables sineVariablesX, sineVariablesY;


}
