using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideEditor : VisualElement
{
    BulletOverrideFields bulletOverrideFields;
    IntegerField selectedBulletField;
    public BulletOverrideEditor(PatternEditor _patternEditor)
    {
        style.backgroundColor = new Color(.02f, 0.39f, 0.29f);
        style.marginTop = 20;

        //Label
        Label label = new Label("Individual Bullet Overrides:");
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(label);

        //Bullet selector
        selectedBulletField = new IntegerField("Bullet");
        selectedBulletField.style.paddingBottom = 20;
        Add(selectedBulletField);

        //Overrides
        bulletOverrideFields = new BulletOverrideFields();
        Add(bulletOverrideFields);
    }

    public void UpdateDropdown()
    {

    }
}
