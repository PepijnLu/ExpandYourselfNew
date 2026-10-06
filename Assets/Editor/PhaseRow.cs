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
    Texture trashIcon;

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

        patternField = new ObjectField();
        patternField.objectType = typeof(BulletPattern);
        patternField.enabledSelf = false;
        patternField.style.maxWidth = 125;
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

    public void SetValues(AttackType attackType, float waitTime, BulletPattern bulletPattern)
    {
        phaseTypeField.value = attackType;
        waitTimeField.value = waitTime;
        patternField.value = bulletPattern;

        UpdateEnabledField(attackType);
    }

    void UpdateRow(ChangeEvent<Enum> evt)
    {
        AttackType phase = (AttackType)evt.newValue;
        UpdateEnabledField(phase);
    }

    void UpdateEnabledField(AttackType _attackType)
    {
        switch (_attackType)
        {
            case AttackType.Wait:
                waitTimeField.enabledSelf = true;
                patternField.enabledSelf = false;
                break;
            case AttackType.Attack:
                waitTimeField.enabledSelf = false;
                patternField.enabledSelf = true;
                break;
        }
    }
}
