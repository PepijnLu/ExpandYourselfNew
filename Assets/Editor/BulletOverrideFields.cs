using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideFields : VisualElement
{
    public FloatField bulletSpeedField;
    public IntegerField bulletDamageField;
    public FloatField bulletSizeField;
    public FloatField angleOffsetField;
    public FloatField positionOffsetXField;
    public FloatField positionOffsetYField;
    public BulletOverrideFields()
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

        //Angle offset
        angleOffsetField = new FloatField("Angle Offset");
        Add(angleOffsetField);

        //Position Offset
        VisualElement posOffsetRow = new();
        posOffsetRow.style.flexDirection = FlexDirection.Row;
        posOffsetRow.style.marginLeft = 3;
        Add(posOffsetRow);

        Label posOffsetRowLabel = new Label("Pos Offset");
        posOffsetRow.Add(posOffsetRowLabel);

        positionOffsetXField = new FloatField();
        positionOffsetXField.style.marginLeft = 58;
        posOffsetRow.Add(positionOffsetXField);

        positionOffsetYField = new FloatField();
        posOffsetRow.Add(positionOffsetYField);
    }
}
