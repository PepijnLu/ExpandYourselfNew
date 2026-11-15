using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrides : VisualElement
{
    public FloatField bulletSpeedField;
    public IntegerField bulletDamageField;
    public FloatField bulletSizeField;
    public BulletOverrides()
    {
        //Bullet Speed
        bulletSpeedField = new FloatField("Bullet Speed");
        Add(bulletSpeedField);

        //Bullet Damage
        bulletDamageField = new IntegerField("Bullet Damage");
        Add(bulletDamageField);

        //Bullet Size
        bulletSizeField = new FloatField("Bullet Size");
        Add(bulletSizeField);
    }
}
