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
    private BulletOverrideFields bulletOverrideFields;

    private BulletOverrideEditor bulletOverrideEditor;

    public BulletPattern currentPattern;

    public PatternEditor(BulletHellEditor _bulletHellEditor)
    {
        style.backgroundColor = Color.black;
        style.marginBottom = 20;
        style.paddingBottom = 20;

        //Label
        Label patternEditorLabel = new Label("Attack values:");
        patternEditorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(patternEditorLabel);

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

        // Bullet type
        bulletTypeField = new ObjectField("Bullet Type");
        bulletTypeField.objectType = typeof(Bullet);
        bulletTypeField.value = AssetDatabase.LoadAssetAtPath<Bullet>("Assets/Prefabs/Bullets/StandardBullet.prefab"); 
        Add(bulletTypeField);

        //Bullet overrides
        Label overrideLabel = new Label("All Bullet Overrides:");
        overrideLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        overrideLabel.style.paddingTop = 20;
        Add(overrideLabel);

        bulletOverrideFields = new BulletOverrideFields();
        Add(bulletOverrideFields);

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

        bulletOverrideFields.bulletSpeedField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletOverrideFields.bulletDamageField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletOverrideFields.bulletSizeField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletOverrideFields.angleOffsetField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletOverrideFields.positionOffsetXField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);
        bulletOverrideFields.positionOffsetYField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);

        patternNameField.RegisterValueChangedCallback(CheckPatternName);
        patternField.RegisterValueChangedCallback(SelectNewPattern);
    }

    public void Initialize()
    {
        bulletOverrideEditor = new(this);
        Add(bulletOverrideEditor);
    }

    public void SelectNewPattern(ChangeEvent<Object> evt)
    {
        EnemyAttack newAttack = new();
        BulletPattern newPattern = evt.newValue as BulletPattern;

        if (newPattern != null)
        {
            currentPattern = newPattern;

            bulletAmountField.value = currentPattern.bulletAmount;
            bulletSpreadField.value = currentPattern.bulletSpread;
            attackAngleField.value = currentPattern.attackAngle;
            bulletTypeField.value = currentPattern.bulletType;
            patternNameField.value = currentPattern.name;

            bulletOverrideFields.bulletSpeedField.value = currentPattern.allBulletsOverrideValues.bulletSpeed;
            bulletOverrideFields.bulletDamageField.value = currentPattern.allBulletsOverrideValues.bulletDamage;
            bulletOverrideFields.bulletSizeField.value = currentPattern.allBulletsOverrideValues.bulletSize;
            bulletOverrideFields.angleOffsetField.value = currentPattern.allBulletsOverrideValues.bulletAngle;
            bulletOverrideFields.positionOffsetXField.value = currentPattern.allBulletsOverrideValues.posOffset.x;
            bulletOverrideFields.positionOffsetYField.value = currentPattern.allBulletsOverrideValues.posOffset.y;

            newAttack.bulletPattern = newPattern;

            bulletHellEditor.bossEnemy.UpdateBulletCount(newAttack, true, bulletHellEditor.AddBulletsToPreview);
            saveButton.enabledSelf = true;
        }
        else
        {
            saveButton.enabledSelf = false;
            currentPattern = null;
        }

        if(bulletOverrideEditor != null) bulletOverrideEditor.ChangeSelectedBullet(0);
    }

    //0 = speed, 1 = damage, 2 = size
    public void SetBulletOverrideValue(int bulletIndex, float newValue, string value)
    {
        if (currentPattern.individualBulletOverrides.Count <= bulletIndex)
        {
            int difference = (bulletIndex + 1) - currentPattern.individualBulletOverrides.Count;
            for (int i = 0; i < difference; i++)
            {
                currentPattern.individualBulletOverrides.Add(new BulletValues());
            }
        }

        BulletValues overrideValues = currentPattern.individualBulletOverrides[bulletIndex];

        switch(value)
        {
            case "speed":
                overrideValues.bulletSpeed = newValue; 
                break;
            case "damage":
                overrideValues.bulletDamage = (int)newValue; 
                break;
            case "size": 
                overrideValues.bulletSize = newValue;
                break;
            case "angle":
                overrideValues.bulletAngle = newValue;
                break;
            case "posX":
                overrideValues.posOffset.x = newValue;
                break;
            case "posY":
                overrideValues.posOffset.y = newValue;
                break;
        }

        currentPattern.individualBulletOverrides[bulletIndex] = overrideValues;

        bulletHellEditor.UpdateBulletCountFromPatternEditor(currentPattern.individualBulletOverrides);
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
        currentPattern = FillBulletPatternFromEditorFields(currentPattern);

        EditorUtility.SetDirty(currentPattern);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreateBulletPatternSO()
    {
        BulletPattern bulletPattern = ScriptableObject.CreateInstance<BulletPattern>();

        bulletPattern = FillBulletPatternFromEditorFields(bulletPattern);

        AssetDatabase.CreateAsset(bulletPattern, $"Assets/BossAI/Attacks/{patternNameField.value}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        patternField.value = bulletPattern;

        CheckPatternName(patternNameField.value);
    }

    //Does not change individual bullet overrides!!
    public BulletPattern FillBulletPatternFromEditorFields(BulletPattern _pattern)
    {
        _pattern.bulletAmount = bulletAmountField.value;
        _pattern.bulletSpread = bulletSpreadField.value;
        _pattern.attackAngle = attackAngleField.value;
        _pattern.bulletType = bulletTypeField.value as Bullet;

        _pattern.allBulletsOverrideValues.bulletSpeed = bulletOverrideFields.bulletSpeedField.value;
        _pattern.allBulletsOverrideValues.bulletDamage = bulletOverrideFields.bulletDamageField.value;
        _pattern.allBulletsOverrideValues.bulletSize = bulletOverrideFields.bulletSizeField.value;

        _pattern.allBulletsOverrideValues.bulletAngle = bulletOverrideFields.angleOffsetField.value;
        _pattern.allBulletsOverrideValues.posOffset = new Vector2(bulletOverrideFields.positionOffsetXField.value, bulletOverrideFields.positionOffsetYField.value);

        _pattern.name = patternNameField.value;

        return _pattern;
    }
}
