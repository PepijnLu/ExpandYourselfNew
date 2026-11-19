using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideEditor : VisualElement
{
    BulletOverrideFields bulletOverrideFields;
    public IntegerField selectedBulletField;
    PatternEditor patternEditor;
    BulletHellEditor bulletHellEditor;

    public int previousBulletValue;
    public BulletOverrideEditor(BulletHellEditor _bulletHellEditor, PatternEditor _patternEditor)
    {
        style.backgroundColor = new Color(.02f, 0.39f, 0.29f);
        style.marginTop = 20;

        patternEditor = _patternEditor;
        bulletHellEditor = _bulletHellEditor;

        //Label
        Label label = new Label("Individual Bullet Overrides:");
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(label);

        //Bullet selector
        selectedBulletField = new IntegerField("Bullet");
        selectedBulletField.style.paddingBottom = 20;
        Add(selectedBulletField);
        selectedBulletField.RegisterValueChangedCallback(ChangeSelectedBulletCallback);
        bulletHellEditor.AddParameterInputField("bulletInt", selectedBulletField, bulletHellEditor.individualParameterInputFields);

        //Overrides
        bulletOverrideFields = new BulletOverrideFields(_bulletHellEditor, false);
        Add(bulletOverrideFields);

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

    public void ChangeSelectedBullet(int _newBulletIndex)
    {
        if (patternEditor.currentPattern == null) return;
        //Debug.Log($"Current bullet amount = {patternEditor.currentPattern.bulletAmount}");
        if (_newBulletIndex >= patternEditor.currentPattern.bulletAmount)
        {
            bulletOverrideFields.SetEnabled(false);
            return;
        }
        bulletOverrideFields.SetEnabled(true);
        SerializedBulletValues bulletOverrideValues = BulletValueDict.GetIndividualBulletOverride(patternEditor.currentPattern, _newBulletIndex);
        bool foundValues = (bulletOverrideValues != null);

        Debug.Log($"CHANGE SELECTED BULLET:  {bulletHellEditor.individualParameterInputFields.Count}");

        foreach (KeyValuePair<string, VisualElement> _pair in bulletHellEditor.individualParameterInputFields)
        {
            if (_pair.Key == "bulletInt") continue;

            if (_pair.Value is IntegerField _intField)
            {
                if (foundValues)
                {
                    Value _value = BulletValueDict.TryGetValue(bulletOverrideValues.dict, _pair.Key);
                    if (_value != null)
                    {
                        IntValue _intValue = _value as IntValue;
                        _intField.value = _intValue.value;
                    }
                    else _intField.value = 0;
                }
                else _intField.value = 0;
            }
            if (_pair.Value is FloatField _floatField)
            {
                if (foundValues)
                {
                    Value _value = BulletValueDict.TryGetValue(bulletOverrideValues.dict, _pair.Key);
                    if (_value != null)
                    {
                        FloatValue _floatValue = _value as FloatValue;
                        _floatField.value = _floatValue.value;
                    }
                    else _floatField.value = 0;
                }
                else _floatField.value = 0;
            }
            if (_pair.Value is ObjectField _objectField)
            {
                if (foundValues)
                {
                    Value _value = BulletValueDict.TryGetValue(bulletOverrideValues.dict, _pair.Key);
                    if (_value != null)
                    {
                        ObjectValue _objectValue = _value as ObjectValue;
                        _objectField.value = _objectValue.value;
                    }
                    else _objectField.value = null;
                }
                else _objectField.value = null;
            }
        }

        //Update bullets
        bulletHellEditor.UpdateBulletCountFromPatternEditor();
    }

    void ClearOverrides()
    {
        //Clear values
        patternEditor.currentPattern.individualBulletOverrides.Clear();

        //Update bullets
        bulletHellEditor.UpdateBulletCountFromPatternEditor();
    }
}
