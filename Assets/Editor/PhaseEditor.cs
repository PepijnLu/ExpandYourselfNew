using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PhaseEditor : VisualElement
{
    public List<PhaseRow> phaseRows;

    TextField phaseNameField;
    Button createButton, saveButton;
    ObjectField phaseField;
    VisualElement rowContainer;

    public EnemyPhase currentPhase;
    public PhaseEditor(BulletHellEditor _bulletHellEditor)
    {
        style.backgroundColor = Color.blue;

        VisualElement header = new();
        header.style.flexDirection = FlexDirection.Row;

        Label phaseTypeLabel = new Label("Phase Type");
        header.Add(phaseTypeLabel);

        Label waitTimeLabel = new Label("Wait Time");
        waitTimeLabel.style.color = Color.black;
        header.Add(waitTimeLabel);

        Label attackLabel = new Label("Attack");
        header.Add(attackLabel);

        Add(header);

        rowContainer = new VisualElement();
        Add(rowContainer);

        Button newRowButton = new Button(() =>
        {
            PhaseRow newRow = new PhaseRow(this);
            rowContainer.Add(newRow);
            phaseRows.Add(newRow);
        })
        {
            text = "Add Row"
        };

        Add(newRowButton);

        // SO name
        phaseNameField = new TextField("Phase Name");
        phaseNameField.style.marginTop = 15;
        Add(phaseNameField);

        createButton = new Button(() =>
        {
            CreatePhaseSO();
        })
        {
            text = "Create New"
        };
        createButton.enabledSelf = false;
        Add(createButton);

        phaseField = new ObjectField("Selected Phase");
        phaseField.style.marginTop = 15;
        phaseField.objectType = typeof(EnemyPhase);
        Add(phaseField);

        saveButton = new Button(() =>
        {
            SavePhaseSO();
        })
        {
            text = "Save"
        };
        saveButton.enabledSelf = false;
        Add(saveButton);

        phaseNameField.RegisterValueChangedCallback(CheckPhaseName);
        phaseField.RegisterValueChangedCallback(SelectNewPhase);

        phaseRows = new();
    }

    public void SelectNewPhase(ChangeEvent<Object> evt)
    {
        EnemyPhase newPhase = evt.newValue as EnemyPhase;

        if(newPhase != null)
        {
            foreach (PhaseRow phaseRow in phaseRows)
            {
                phaseRow.RemoveFromHierarchy();
            }
            phaseRows.Clear();

            currentPhase = newPhase;

            foreach (EnemyAttack attack in currentPhase.attacks)
            {
                PhaseRow newRow = new(this);
                newRow.SetValues(attack.attackType, attack.waitTime, attack.bulletPattern);
                phaseRows.Add(newRow);
                rowContainer.Add(newRow);

                switch (attack.attackType)
                {
                    case AttackType.Attack:
                        newRow.patternField.enabledSelf = true;
                        break;
                    case AttackType.Wait:
                        newRow.waitTimeField.enabledSelf = true;
                        break;
                }
            }
            saveButton.enabledSelf = true;
        }
        else
        {
            saveButton.enabledSelf = false;
            currentPhase = null;
        }
    }

    public void RemoveRow(PhaseRow row)
    {
        phaseRows.Remove(row);
        row.RemoveFromHierarchy();
    }
    void SavePhaseSO()
    {
        for(int i = 0; i < phaseRows.Count; i++)
        {
            AttackType rowAttackType = (AttackType)phaseRows[i].phaseTypeField.value;
            float rowWaitTime = phaseRows[i].waitTimeField.value;
            BulletPattern rowBulletPattern = (BulletPattern)phaseRows[i].patternField.value;

            if (i < currentPhase.attacks.Count)
            {
                currentPhase.attacks[i].attackType = rowAttackType;
                currentPhase.attacks[i].waitTime = rowWaitTime;
                currentPhase.attacks[i].bulletPattern = rowBulletPattern;
            }
            else
            {
                EnemyAttack newAttack = new();

                newAttack.attackType = rowAttackType;
                newAttack.waitTime = rowWaitTime;
                newAttack.bulletPattern = rowBulletPattern;

                currentPhase.attacks.Add(newAttack);
            }
        }

        EditorUtility.SetDirty(currentPhase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreatePhaseSO()
    {
        EnemyPhase enemyPhase = ScriptableObject.CreateInstance<EnemyPhase>();
        enemyPhase.attacks = new();

        foreach(PhaseRow phaseRow in phaseRows)
        {
            EnemyAttack newAttack = new();

            newAttack.attackType = (AttackType)phaseRow.phaseTypeField.value;

            switch(newAttack.attackType)
            {
                case AttackType.Wait:
                    newAttack.waitTime = phaseRow.waitTimeField.value;
                    break;
                case AttackType.Attack:
                    newAttack.bulletPattern = (BulletPattern)phaseRow.patternField.value;
                    break;
            }

            enemyPhase.attacks.Add(newAttack);
        }

        Debug.Log($"Saving Attacks: {enemyPhase.attacks.Count}");

        AssetDatabase.CreateAsset(enemyPhase, $"Assets/BossAI/Phases/{phaseNameField.value}.asset");
        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh();

        phaseField.value = enemyPhase;

        CheckPhaseName(phaseNameField.value);
    }

    void CheckPhaseName(string _newValue)
    {
        bool valid = true;

        //Check if empty
        if (_newValue == "") valid = false;

        //Check if name already exists
        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/BossAI/Phases" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset.name == _newValue) valid = false;
        }

        createButton.enabledSelf = valid;
    }

    void CheckPhaseName(ChangeEvent<string> evt)
    {
        CheckPhaseName(evt.newValue);
    }
}
