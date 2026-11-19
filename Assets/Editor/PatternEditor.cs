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
        Add(bulletAmountField);
        bulletAmountField.RegisterValueChangedCallback(_bulletHellEditor.UpdateBulletCountFromPatternEditor);

        //bulletHellEditor.AddParameterInputField("bulletAmount", bulletAmountField);

        // Bullet Spread
        bulletSpreadField = new FloatField("Bullet Spread");
        Add(bulletSpreadField);
        bulletSpreadField.RegisterValueChangedCallback(_bulletHellEditor.UpdateBulletCountFromPatternEditor);

        // Bullet Angle
        attackAngleField = new FloatField("Attack Angle");
        Add(attackAngleField);
        attackAngleField.RegisterValueChangedCallback(_bulletHellEditor.UpdateBulletCountFromPatternEditor);

        // Bullet type
        bulletTypeField = new ObjectField("Bullet Type");
        bulletTypeField.objectType = typeof(Bullet);
        Add(bulletTypeField);
        bulletTypeField.RegisterValueChangedCallback(_bulletHellEditor.UpdateBulletCountFromPatternEditor);

        //Bullet overrides
        Label overrideLabel = new Label("Shared Bullet Overrides:");
        overrideLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        overrideLabel.style.paddingTop = 20;
        Add(overrideLabel);

        bulletOverrideFields = new BulletOverrideFields(bulletHellEditor, true);
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
            currentPattern.Deserialize();

            Debug.Log($"keyyyys");
            foreach (var kvp in currentPattern.sharedOverrides.dict)
            {
                Debug.Log($"keyyyy: {kvp.Key}");
            }
            //keep the same (not bullet values)
            bulletAmountField.value = currentPattern.bulletAmount;
            bulletSpreadField.value = currentPattern.bulletSpread;
            attackAngleField.value = currentPattern.attackAngle;
            bulletTypeField.value = currentPattern.bulletType;
            patternNameField.value = currentPattern.name;

            //bullet values
            foreach (KeyValuePair<string, VisualElement> _pair in bulletHellEditor.sharedParameterInputFields)
            {
                bool hasOverride = currentPattern.sharedOverrides.dict.ContainsKey(_pair.Key);

                if (_pair.Value is FloatField _floatField)
                {
                    if (hasOverride)
                    {
                        _floatField.value = (currentPattern.sharedOverrides.dict[_pair.Key] as FloatValue).value;
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
                        _integerField.value = (currentPattern.sharedOverrides.dict[_pair.Key] as IntValue).value;
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
                        _objectField.value = (currentPattern.sharedOverrides.dict[_pair.Key] as ObjectValue).value;
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
        currentPattern.Serialize();

        EditorUtility.SetDirty(currentPattern);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreateBulletPatternSO()
    {
        BulletPattern bulletPattern = ScriptableObject.CreateInstance<BulletPattern>();
        bulletPattern.sharedOverrides = new();

        bulletPattern = FillBulletPatternFromEditorFields(bulletPattern);

        AssetDatabase.CreateAsset(bulletPattern, $"Assets/BossAI/Attacks/{patternNameField.value}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        patternField.value = bulletPattern;

        CheckPatternName(patternNameField.value);
    }

    public BulletPattern FillBulletPatternFromEditorFields(BulletPattern _pattern)
    {
        _pattern.bulletAmount = bulletAmountField.value;
        _pattern.bulletSpread = bulletSpreadField.value;
        _pattern.attackAngle = attackAngleField.value;
        _pattern.bulletType = bulletTypeField.value as Bullet;

        //Loop over shared override fields
        if (_pattern.sharedOverrides.dict == null) _pattern.sharedOverrides.dict = new();
        Dictionary<string, Value> dictToAddTo = _pattern.sharedOverrides.dict;
        foreach(KeyValuePair<string, VisualElement> _pair in bulletHellEditor.sharedParameterInputFields)
        {
            FillValuesFromInputField(dictToAddTo, _pair);
        }

        //Loop over individual override fields
        if (_pattern.individualBulletOverrides == null) _pattern.individualBulletOverrides = new();

        //Fix this so it does find the correct int
        if (bulletOverrideEditor != null)
        {
            int currentBulletInt = bulletOverrideEditor.selectedBulletField.value;
            SerializedBulletValues bulletOverrideValues = BulletValueDict.GetIndividualBulletOverride(_pattern, currentBulletInt);

            if (bulletOverrideValues != null)
            {
                //Found individual overrides
                dictToAddTo = bulletOverrideValues.dict;
            }
            else
            {
                bulletOverrideValues = new();
                bulletOverrideValues.dict = new();
                dictToAddTo = bulletOverrideValues.dict;
                _pattern.individualBulletOverrides.Add(bulletOverrideValues);
            }

            foreach (KeyValuePair<string, VisualElement> _pair in bulletHellEditor.individualParameterInputFields)
            {
                FillValuesFromInputField(dictToAddTo, _pair);
            }
        }
        else Debug.LogWarning($"Bullet Override Editor is null");

        //Return
        return _pattern;
    }

    void FillValuesFromInputField(Dictionary<string, Value> dictToAddTo, KeyValuePair<string, VisualElement> _pair)
    {
        if (_pair.Value is FloatField _floatField)
        {
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
}
