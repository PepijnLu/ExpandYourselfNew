using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideEditor : VisualElement
{
    BulletOverrideFields bulletOverrideFields;
    IntegerField selectedBulletField;
    PatternEditor patternEditor;
    public BulletOverrideEditor(BulletHellEditor _bulletHellEditor, PatternEditor _patternEditor)
    {
        style.backgroundColor = new Color(.02f, 0.39f, 0.29f);
        style.marginTop = 20;

        patternEditor = _patternEditor;

        //Label
        Label label = new Label("Individual Bullet Overrides:");
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(label);

        //Bullet selector
        selectedBulletField = new IntegerField("Bullet");
        selectedBulletField.style.paddingBottom = 20;
        Add(selectedBulletField);

        selectedBulletField.RegisterValueChangedCallback(ChangeSelectedBulletCallback);

        //Overrides
        //bulletOverrideFields = new BulletOverrideFields(_bulletHellEditor, "individual");
        //Add(bulletOverrideFields);

        //bulletOverrideFields.bulletSpeedField.RegisterValueChangedCallback(UpdateOverrideSpeed);
        //bulletOverrideFields.bulletDamageField.RegisterValueChangedCallback(UpdateOverrideDamage);
        //bulletOverrideFields.bulletSizeField.RegisterValueChangedCallback(UpdateOverrideSize);

        //bulletOverrideFields.angleField.RegisterValueChangedCallback(UpdateOverrideAngle);
        //bulletOverrideFields.positionOffsetXField.RegisterValueChangedCallback(UpdateOverridePosX);
        //bulletOverrideFields.positionOffsetYField.RegisterValueChangedCallback(UpdateOverridePosY);

        //Clear override button
        Button clearOverrideButton = new Button(() =>
        {
            ClearOverrides();
        })
        {
            text = "Clear Overrides"
        };
        Add(clearOverrideButton);

        ChangeSelectedBullet(0);
    }

    void ChangeSelectedBulletCallback(ChangeEvent<int> i)
    {
        ChangeSelectedBullet(i.newValue);
    }

    public void ChangeSelectedBullet(int newBulletIndex)
    {
        //if (!patternEditor.currentPattern) return;

        //selectedBulletField.value = newBulletIndex;
        //int bulletCount = patternEditor.currentPattern.bulletAmount;

        //if (newBulletIndex >= bulletCount)
        //{
        //    bulletOverrideFields.SetEnabled(false);
        //}
        //else
        //{
        //    bulletOverrideFields.SetEnabled(true);
        //}

        //if (newBulletIndex < patternEditor.currentPattern.individualBulletOverrides.Count)
        //{
        //    BulletValues values = patternEditor.currentPattern.individualBulletOverrides[newBulletIndex];
        //    bulletOverrideFields.bulletSpeedField.value = values.bulletSpeed;
        //    bulletOverrideFields.bulletDamageField.value = values.bulletDamage;
        //    bulletOverrideFields.bulletSizeField.value = values.bulletSize;

        //    bulletOverrideFields.angleOffsetField.value = values.bulletAngle;
        //    bulletOverrideFields.positionOffsetXField.value = values.posOffset.x;
        //    bulletOverrideFields.positionOffsetYField.value = values.posOffset.y;
        //}
        //else
        //{
        //    bulletOverrideFields.bulletSpeedField.value = 0;
        //    bulletOverrideFields.bulletDamageField.value = 0;
        //    bulletOverrideFields.bulletSizeField.value = 0;

        //    bulletOverrideFields.angleOffsetField.value = 0;
        //    bulletOverrideFields.positionOffsetXField.value = 0;
        //    bulletOverrideFields.positionOffsetYField.value = 0;
        //}

    }

    void ClearOverrides()
    {
        //Clear values
        patternEditor.currentPattern.individualBulletOverrides.Clear();

        //Clear fields
        bulletOverrideFields.bulletSpeedField.value = 0;
        bulletOverrideFields.bulletDamageField.value = 0;
        bulletOverrideFields.bulletSizeField.value = 0;

        bulletOverrideFields.angleField.value = 0;
        bulletOverrideFields.positionOffsetXField.value = 0;
        bulletOverrideFields.positionOffsetYField.value = 0;

        //Update bullets
        //patternEditor.SetBulletOverrideValue(selectedBulletField.value, 0, "speed");
    }
}
