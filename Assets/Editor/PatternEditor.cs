using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PatternEditor : VisualElement
{
    public IntegerField bulletAmountField;
    public FloatField bulletSpreadField, bulletAngleField;
    public TextField patternNameField;
    private BulletHellEditor bulletHellEditor;
    public ObjectField patternField;
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
        bulletAngleField = new FloatField("Bullet Angle");
        bulletAngleField.value = 45;
        Add(bulletAngleField);

        // Origin Angle
        //originAngleField = new FloatField("Origin Angle");
        //Add(originAngleField);

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
        bulletAngleField.RegisterValueChangedCallback(bulletHellEditor.UpdateBulletCountFromPatternEditor);

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
            bulletAngleField.value = currentPattern.bulletAngle;

            bulletHellEditor.UpdateBulletCount(currentPattern.bulletAmount, currentPattern.bulletSpread, currentPattern.bulletAngle, true);
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
        Debug.Log($"BULLET PATTERN SAVE");

        currentPattern.bulletAmount = bulletAmountField.value;
        currentPattern.bulletSpread = bulletSpreadField.value;
        currentPattern.bulletAngle = bulletAngleField.value;

        EditorUtility.SetDirty(currentPattern);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreateBulletPatternSO()
    {
        Debug.Log($"BULLET PATTERN CREATE");

        BulletPattern bulletPattern = ScriptableObject.CreateInstance<BulletPattern>();

        bulletPattern.bulletAmount = bulletAmountField.value;
        bulletPattern.bulletSpread = bulletSpreadField.value;
        bulletPattern.bulletAngle = bulletAngleField.value;

        AssetDatabase.CreateAsset(bulletPattern, $"Assets/BossAI/Attacks/{patternNameField.value}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        patternField.value = bulletPattern;

        CheckPatternName(patternNameField.value);
    }
}
