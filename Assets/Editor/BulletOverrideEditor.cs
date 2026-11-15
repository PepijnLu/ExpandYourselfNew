using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideEditor : VisualElement
{
    BulletOverrideFields bulletOverrideFields;
    IntegerField selectedBulletField;
    PatternEditor patternEditor;
    public BulletOverrideEditor(PatternEditor _patternEditor)
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
        bulletOverrideFields = new BulletOverrideFields();
        Add(bulletOverrideFields);

        bulletOverrideFields.bulletSpeedField.RegisterValueChangedCallback(UpdateOverrideSpeed);
        bulletOverrideFields.bulletDamageField.RegisterValueChangedCallback(UpdateOverrideDamage);
        bulletOverrideFields.bulletSizeField.RegisterValueChangedCallback(UpdateOverrideSize);
    }

    void ChangeSelectedBulletCallback(ChangeEvent<int> i)
    {
        ChangeSelectedBullet(i.newValue);
    }

    void ChangeSelectedBullet(int newBulletIndex)
    {
        int bulletCount = patternEditor.currentPattern.bulletAmount;

        if (newBulletIndex >= bulletCount)
        {
            bulletOverrideFields.SetEnabled(false);
        }
        else
        {
            bulletOverrideFields.SetEnabled(true);
        }

        if (patternEditor.currentPattern.individualBulletOverrides.TryGetValue(newBulletIndex, out BulletValues values))
        {
            bulletOverrideFields.bulletSpeedField.value = values.bulletSpeed;
            bulletOverrideFields.bulletDamageField.value = values.bulletDamage;
            bulletOverrideFields.bulletSizeField.value = values.bulletSize;
        }
        else
        {
            bulletOverrideFields.bulletSpeedField.value = 0;
            bulletOverrideFields.bulletDamageField.value = 0;
            bulletOverrideFields.bulletSizeField.value = 0;
        }

    }

    public void UpdateOverrideSpeed(ChangeEvent<float> f)
    {
        patternEditor.SetBulletOverrideValue(selectedBulletField.value, f.newValue, 0);
    }

    public void UpdateOverrideDamage(ChangeEvent<int> i)
    {
        patternEditor.SetBulletOverrideValue(selectedBulletField.value, i.newValue, 1);
    }

    public void UpdateOverrideSize(ChangeEvent<float> f)
    {
        patternEditor.SetBulletOverrideValue(selectedBulletField.value, f.newValue, 2);
    }
}
