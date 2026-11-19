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
       //bulletHellEditor.AddParameterInputField("bulletAmount", bulletAmountField);

        // Bullet Spread
        bulletSpreadField = new FloatField("Bullet Spread");
        bulletSpreadField.value = 180;
        Add(bulletSpreadField);
        //bulletHellEditor.AddParameterInputField("bulletSpread", bulletSpreadField);

        // Bullet Angle
        attackAngleField = new FloatField("Attack Angle");
        attackAngleField.value = 45;
        Add(attackAngleField);
        //bulletHellEditor.AddParameterInputField("attackAngle", attackAngleField);

        // Bullet type
        bulletTypeField = new ObjectField("Bullet Type");
        bulletTypeField.objectType = typeof(Bullet);
        bulletTypeField.value = AssetDatabase.LoadAssetAtPath<Bullet>("Assets/Prefabs/Bullets/StandardBullet.prefab"); 
        Add(bulletTypeField);
        //bulletHellEditor.AddParameterInputField("bulletType", bulletTypeField);

        //Bullet overrides
        Label overrideLabel = new Label("Shared Bullet Overrides:");
        overrideLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        overrideLabel.style.paddingTop = 20;
        Add(overrideLabel);

        bulletOverrideFields = new BulletOverrideFields(bulletHellEditor);
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

        patternNameField.RegisterValueChangedCallback(CheckPatternName);
        patternField.RegisterValueChangedCallback(SelectNewPattern);
    }

    public void Initialize()
    {
        bulletOverrideEditor = new(bulletHellEditor, this);
        Add(bulletOverrideEditor);
    }

    public void SelectNewPattern(ChangeEvent<Object> evt)
    {
        EnemyAttack newAttack = new();
        BulletPattern newPattern = evt.newValue as BulletPattern;

        if (newPattern != null)
        {
            currentPattern = newPattern;

            //keep the same (not bullet values)
            bulletAmountField.value = currentPattern.bulletAmount;
            bulletSpreadField.value = currentPattern.bulletSpread;
            attackAngleField.value = currentPattern.attackAngle;
            bulletTypeField.value = currentPattern.bulletType;
            patternNameField.value = currentPattern.name;

            //bullet values
            foreach(KeyValuePair<string, VisualElement> _pair in bulletHellEditor.parameterInputFields)
            {
                bool hasOverride = currentPattern.allBulletsOverrideValues.ContainsKey(_pair.Key);

                if (_pair.Value is FloatField _floatField)
                {
                    if(hasOverride)
                    {
                        _floatField.value = (currentPattern.allBulletsOverrideValues[_pair.Key] as FloatValue).value;
                    }
                    else
                    {
                        _floatField.value = 0;
                    }
                }
                else if (_pair.Value is IntegerField _integerField)
                {
                    if (hasOverride)
                    {
                        _integerField.value = (currentPattern.allBulletsOverrideValues[_pair.Key] as IntValue).value;
                    }
                    else
                    {
                        _integerField.value = 0;
                    }
                }
                else if (_pair.Value is ObjectField _objectField)
                {
                    if (hasOverride)
                    {
                        _objectField.value = (currentPattern.allBulletsOverrideValues[_pair.Key] as ObjectValue).value;
                    }
                    else
                    {
                        _objectField.value = null;
                    }
                }
            }

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
                //currentPattern.individualBulletOverrides.Add(new BulletValues());
            }
        }

        //BulletValues overrideValues = currentPattern.individualBulletOverrides[bulletIndex];

        //switch(value)
        //{
        //    case "speed":
        //        overrideValues.bulletSpeed = newValue; 
        //        break;
        //    case "damage":
        //        overrideValues.bulletDamage = (int)newValue; 
        //        break;
        //    case "size": 
        //        overrideValues.bulletSize = newValue;
        //        break;
        //    case "angle":
        //        overrideValues.bulletAngle = newValue;
        //        break;
        //    case "posX":
        //        overrideValues.posOffset.x = newValue;
        //        break;
        //    case "posY":
        //        overrideValues.posOffset.y = newValue;
        //        break;
        //}

        //currentPattern.individualBulletOverrides[bulletIndex] = overrideValues;

        //bulletHellEditor.UpdateBulletCountFromPatternEditor(currentPattern.individualBulletOverrides);
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

        foreach(KeyValuePair<string, VisualElement> _pair in bulletHellEditor.parameterInputFields)
        {
            Debug.Log($"PARAMTER INPUT FIELD: {_pair.Key}");
            Dictionary<string, Value> dictToAddTo = _pattern.allBulletsOverrideValues;

            if (_pair.Value is FloatField _floatField)
            {
                if (_floatField.value == 0) continue;

                if (dictToAddTo.ContainsKey(_pair.Key))
                {
                    (dictToAddTo[_pair.Key] as FloatValue).value = _floatField.value;
                }
                else
                {
                    dictToAddTo.Add(_pair.Key, new FloatValue(_floatField.value));
                }
            }
            else if (_pair.Value is IntegerField _integerField)
            {
                if (_integerField.value == 0) continue;

                if (dictToAddTo.ContainsKey(_pair.Key))
                {
                    (dictToAddTo[_pair.Key] as IntValue).value = _integerField.value;
                }
                else
                {
                    dictToAddTo.Add(_pair.Key, new IntValue(_integerField.value));
                }
            }
            else if (_pair.Value is ObjectField _objectField)
            {
                if (_objectField.value == null) continue;

                if (dictToAddTo.ContainsKey(_pair.Key))
                {
                    (dictToAddTo[_pair.Key] as ObjectValue).value = _objectField.value;
                }
                else
                {
                    dictToAddTo.Add(_pair.Key, new ObjectValue(_objectField.value));
                }
            }
        }

        _pattern.name = patternNameField.value;

        
        //if (_pattern.allBulletsOverrideValues.ContainsKey("sharedBulletSpeed"))
        //{
        //    Debug.Log($"Changed Bullet Count, new speed = {(_pattern.allBulletsOverrideValues["sharedBulletSpeed"] as FloatValue).value}");
        //}

        return _pattern;
    }
}
