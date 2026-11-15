using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class PatternEditor : VisualElement
{
    public IntegerField bulletAmountField;
    public FloatField bulletSpreadField, attackAngleField;
    public TextField patternNameField;
    private BulletHellEditor bulletHellEditor;
    public ObjectField patternField, bulletTypeField;
    private Button saveButton, createButton;

    private BulletPattern currentPattern;

    public PatternEditor(BulletHellEditor _bulletHellEditor)
    {
        style.backgroundColor = Color.black;
        style.marginBottom = 20;

        bulletHellEditor = _bulletHellEditor;
        // Bullet Amount
        bulletAmountField = new IntegerField("Bullet Amount");
        bulletAmountField.value = 4;
        Add(bulletAmountField);

        // Bullet Spread
        bulletSpreadField = new FloatField("Bullet Spread");
        bulletSpreadField.value = 180;
        Add(bulletSpreadField);

        // Bullet Angle
        attackAngleField = new FloatField("Attack Angle");
        attackAngleField.value = 45;
        Add(attackAngleField);

        // Origin Angle
        //originAngleField = new FloatField("Origin Angle");
        //Add(originAngleField);

        // Bullet type
        bulletTypeField = new ObjectField("Bullet Type");
        bulletTypeField.objectType = typeof(Bullet);
        bulletTypeField.value = AssetDatabase.LoadAssetAtPath<Bullet>("Assets/Prefabs/Bullets/StandardBullet.prefab"); 
        Add(bulletTypeField);

        // SO name
        patternNameField = new TextField("Pattern Name");
        patternNameField.style.marginTop = 15;
        Add(patternNameField);

        createButton = new Button(() =>
        {
            CreateBulletPatternSO();
        })
        {
            text = "Create New"
        };
        createButton.enabledSelf = false;
        Add(createButton);

        patternField = new ObjectField("Selected Pattern");
        patternField.style.marginTop = 15;
        patternField.objectType = typeof(BulletPattern);
        Add(patternField);

        saveButton = new Button(() =>
        {
            SaveBulletPatternSO();
        })
        {
            text = "Save"
        };
        saveButton.enabledSelf = false;
        Add(saveButton);

        bulletAmountField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletSpreadField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        attackAngleField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletTypeField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);

        patternNameField.RegisterValueChangedCallback(CheckPatternName);
        patternField.RegisterValueChangedCallback(SelectNewPattern);
    }

    public void SelectNewPattern(ChangeEvent<Object> evt)
    {
        BulletPattern newPattern = evt.newValue as BulletPattern;

        if (newPattern != null)
        {
            currentPattern = newPattern;

            bulletAmountField.value = currentPattern.bulletAmount;
            bulletSpreadField.value = currentPattern.bulletSpread;
            attackAngleField.value = currentPattern.attackAngle;
            bulletTypeField.value = currentPattern.bulletType;
            patternNameField.value = currentPattern.name;

            bulletHellEditor.bossEnemy.UpdateBulletCount(currentPattern, true, bulletHellEditor.AddBulletsToPreview);
            saveButton.enabledSelf = true;
        }
        else
        {
            saveButton.enabledSelf = false;
            currentPattern = null;
        }
    }

    void CheckPatternName(string _newValue)
    {
        bool valid = true;

        //Check if empty
        if (_newValue == "") valid = false;

        //Check if name already exists
        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/BossAI/Attacks" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset.name == _newValue) valid = false;
        }

        createButton.enabledSelf = valid;
    }

    void CheckPatternName(ChangeEvent<string> evt)
    {
        CheckPatternName(evt.newValue);
    }


    void SaveBulletPatternSO()
    {
        currentPattern.bulletAmount = bulletAmountField.value;
        currentPattern.bulletSpread = bulletSpreadField.value;
        currentPattern.attackAngle = attackAngleField.value;
        currentPattern.bulletType = bulletTypeField.value as Bullet;

        EditorUtility.SetDirty(currentPattern);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreateBulletPatternSO()
    {
        BulletPattern bulletPattern = ScriptableObject.CreateInstance<BulletPattern>();

        bulletPattern.bulletAmount = bulletAmountField.value;
        bulletPattern.bulletSpread = bulletSpreadField.value;
        bulletPattern.attackAngle = attackAngleField.value;

        AssetDatabase.CreateAsset(bulletPattern, $"Assets/BossAI/Attacks/{patternNameField.value}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        patternField.value = bulletPattern;

        CheckPatternName(patternNameField.value);
    }
}
