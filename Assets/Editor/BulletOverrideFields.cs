using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideFields : VisualElement
{
    public FloatField bulletSpeedField;
    public IntegerField bulletDamageField;
    public FloatField bulletSizeField;
    public FloatField angleField;
    public FloatField positionOffsetXField;
    public FloatField positionOffsetYField;
    public BulletOverrideFields(BulletHellEditor _bulletHellEditor, bool _shared)
    {
        Dictionary<string, VisualElement> dict;

        if (_shared)
        {
            dict = _bulletHellEditor.sharedParameterInputFields;
        }
        else
        {
            dict = _bulletHellEditor.individualParameterInputFields;
        }
        //Bullet Speed
        bulletSpeedField = new FloatField("Bullet Speed");
        Add(bulletSpeedField);
        _bulletHellEditor.AddParameterInputField($"bulletSpeed", bulletSpeedField, dict);

        //Bullet Damage
        bulletDamageField = new IntegerField("Bullet Damage");
        Add(bulletDamageField);
        _bulletHellEditor.AddParameterInputField($"bulletDamage", bulletDamageField, dict);

        //Bullet Size
        bulletSizeField = new FloatField("Bullet Size");
        Add(bulletSizeField);
        _bulletHellEditor.AddParameterInputField($"bulletSize", bulletSizeField, dict);

        //Angle offset
        angleField = new FloatField("Angle Offset");
        Add(angleField);
        _bulletHellEditor.AddParameterInputField($"angleOffset", angleField, dict);

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
        _bulletHellEditor.AddParameterInputField($"posOffsetX", positionOffsetXField, dict);
        positionOffsetYField = new FloatField();
        posOffsetRow.Add(positionOffsetYField);
        _bulletHellEditor.AddParameterInputField($"posOffsetY", positionOffsetYField, dict);
    }
}
