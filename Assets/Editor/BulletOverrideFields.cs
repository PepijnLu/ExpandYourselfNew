using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletOverrideFields : VisualElement
{
    //Declare at the top here, or will cause errors in dictionaries
    public FloatField bulletSpeedField;
    public IntegerField bulletDamageField;
    public FloatField bulletSizeField;
    public FloatField angleField;

    public FloatField positionOffsetXField;
    public FloatField positionOffsetYField;

    public FloatField sineAmpField, sineFreqField, sinePhaseField;
    public FloatField linearCoefficientField, linearStartField;
    public BulletOverrideFields(BulletHellEditor _bulletHellEditor, bool _shared)
    {
        Dictionary<string, VisualElement> dict;

        if (_shared)
        {
            dict = _bulletHellEditor.sharedParameterInputFields;
        }
        else
        {
            dict = _bulletHellEditor.individualParameterInputFields;
        }
        //Bullet Speed
        bulletSpeedField = new FloatField("Bullet Speed");
        Add(bulletSpeedField);
        _bulletHellEditor.AddParameterInputField($"bulletSpeed", bulletSpeedField, dict);

        //Bullet Damage
        bulletDamageField = new IntegerField("Bullet Damage");
        Add(bulletDamageField);
        _bulletHellEditor.AddParameterInputField($"bulletDamage", bulletDamageField, dict);

        //Bullet Size
        bulletSizeField = new FloatField("Bullet Size");
        Add(bulletSizeField);
        _bulletHellEditor.AddParameterInputField($"bulletSize", bulletSizeField, dict);

        //Angle offset
        angleField = new FloatField("Angle Offset");
        Add(angleField);
        _bulletHellEditor.AddParameterInputField($"angleOffset", angleField, dict);

        //Position Offset
        VisualElement posOffsetRow = new();
        posOffsetRow.style.flexDirection = FlexDirection.Row;
        posOffsetRow.style.marginLeft = 3;
        Add(posOffsetRow);

        Label posOffsetRowLabel = new Label("Pos Offset");
        posOffsetRow.Add(posOffsetRowLabel);

        FloatField positionOffsetXField = new FloatField();
        positionOffsetXField.style.marginLeft = 58;
        posOffsetRow.Add(positionOffsetXField);
        _bulletHellEditor.AddParameterInputField($"posOffsetX", positionOffsetXField, dict);
        positionOffsetYField = new FloatField();
        posOffsetRow.Add(positionOffsetYField);
        _bulletHellEditor.AddParameterInputField($"posOffsetY", positionOffsetYField, dict);

        //Linear
        VisualElement linearRow = new();
        linearRow.style.marginLeft = 3;
        linearRow.style.flexDirection = FlexDirection.Row;
        Add(linearRow);

        Label linearLabel = new Label("Linear:");
        linearRow.Add(linearLabel);

        //Linear Coefficient Field
        Label coefficientFieldLabel = new Label("Coefficient:");
        coefficientFieldLabel.style.marginLeft = 80;
        linearRow.Add(coefficientFieldLabel);
        linearCoefficientField = new FloatField();
        linearRow.Add(linearCoefficientField);
        _bulletHellEditor.AddParameterInputField($"linearCoefficient", linearCoefficientField, dict);

        //Linear Start Field
        Label startFieldLabel = new Label("Start:");
        startFieldLabel.style.marginLeft = 5;
        linearRow.Add(startFieldLabel);
        linearStartField = new FloatField();
        linearRow.Add(linearStartField);
        _bulletHellEditor.AddParameterInputField($"linearStart", linearStartField, dict);

        //Sine Wave
        VisualElement sineWaveRow = new();
        sineWaveRow.style.marginLeft = 3;
        sineWaveRow.style.flexDirection = FlexDirection.Row;
        Add(sineWaveRow);

        Label sineWaveRowLabel = new Label("Sine wave:");
        sineWaveRow.Add(sineWaveRowLabel);


        //Sine Amp Field
        Label ampFieldLabel = new Label("Amp:");
        ampFieldLabel.style.marginLeft = 60;
        sineWaveRow.Add(ampFieldLabel);
        sineAmpField = new FloatField();
        sineWaveRow.Add(sineAmpField);
        _bulletHellEditor.AddParameterInputField($"sineAmp", sineAmpField, dict);

        //Sine Freq Field
        Label freqFieldLabel = new Label("Freq:");
        freqFieldLabel.style.marginLeft = 5;
        sineWaveRow.Add(freqFieldLabel);
        sineFreqField = new FloatField();
        sineWaveRow.Add(sineFreqField);
        _bulletHellEditor.AddParameterInputField($"sineFreq", sineFreqField, dict);

        //Sine Phase Field
        Label phaseFieldLabel = new Label("Phase:");
        phaseFieldLabel.style.marginLeft = 5;
        sineWaveRow.Add(phaseFieldLabel);
        sinePhaseField = new FloatField();
        sineWaveRow.Add(sinePhaseField);
        _bulletHellEditor.AddParameterInputField($"sinePhase", sinePhaseField, dict);

    }
}
