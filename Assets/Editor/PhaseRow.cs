using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PhaseRow : VisualElement
{
    public EnumField phaseTypeField;
    public FloatField waitTimeField;
    public ObjectField patternField;
    public FloatField angleOffsetField;

    Texture trashIcon;

    public StyleColor defaultColor;

    public PhaseRow(PhaseEditor _phaseEditor)
    {
        style.flexDirection = FlexDirection.Row;
        trashIcon = EditorGUIUtility.IconContent("TreeEditor.Trash").image;

        phaseTypeField = new EnumField(
        AttackType.Wait
        );

        phaseTypeField.RegisterValueChangedCallback(UpdateRow);

        phaseTypeField.style.minWidth = 60;
        Add(phaseTypeField);

        waitTimeField = new FloatField();
        waitTimeField.style.minWidth = 30;
        Add(waitTimeField);

        angleOffsetField = new FloatField();
        angleOffsetField.style.minWidth = 30;
        Add(angleOffsetField);


        patternField = new ObjectField();

        defaultColor = patternField.style.backgroundColor;

        patternField.objectType = typeof(BulletPattern);
        patternField.enabledSelf = false;
        patternField.style.maxWidth = 95;
        Add(patternField);

        //'Remove row' Button
        Button removeRowButton = new Button(() =>
        {
            _phaseEditor.RemoveRow(this);
        });
        Image removeRowButtonIcon = new Image();
        removeRowButtonIcon.image = trashIcon;
        removeRowButton.Add(removeRowButtonIcon);
        Add(removeRowButton);
    }

    public void SetValues(AttackType _attackType, float _waitTime, BulletPattern _bulletPattern, float _angleOffset)
    {
        phaseTypeField.value = _attackType;
        waitTimeField.value = _waitTime;
        patternField.value = _bulletPattern;
        angleOffsetField.value = _angleOffset;

        UpdateEnabledField(_attackType);
    }

    void UpdateRow(ChangeEvent<Enum> evt)
    {
        AttackType phase = (AttackType)evt.newValue;
        UpdateEnabledField(phase);
    }

    public void HighlightRow(bool _highlight)
    {
        if(_highlight)
        {
            style.backgroundColor = Color.white;
        }
        else
        {
            style.backgroundColor = defaultColor;
        }
    }

    void UpdateEnabledField(AttackType _attackType)
    {
        switch (_attackType)
        {
            case AttackType.Wait:
                waitTimeField.enabledSelf = true;
                patternField.enabledSelf = false;
                angleOffsetField.enabledSelf = false;
                break;
            case AttackType.Attack:
                waitTimeField.enabledSelf = false;
                patternField.enabledSelf = true;
                angleOffsetField.enabledSelf = true;
                break;
        }
    }
}
