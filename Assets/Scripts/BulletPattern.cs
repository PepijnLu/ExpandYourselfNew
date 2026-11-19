using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletPattern", menuName = "Scriptable Objects/EnemyAttack")]
public class BulletPattern : ScriptableObject
{
    public int bulletAmount;
    public float bulletSpread;
    public float attackAngle;
    public Bullet bulletType;

    public SerializedBulletValues sharedOverrides;

    public List<SerializedBulletValues> individualBulletOverrides = new();

    public void Serialize()
    {
        sharedOverrides.Serialize();

        foreach (SerializedBulletValues _values in individualBulletOverrides)
        {
            _values.Serialize();
        }
    }

    public void Deserialize()
    {
        sharedOverrides.Deserialize();

        foreach (SerializedBulletValues _values in individualBulletOverrides)
        {
            _values.Deserialize();
        }
    }

}

[System.Serializable]
public class SerializedBulletValues
{
    public List<string> strings;
    public List<float> floatValues;
    public List<Object> objectValues;
    public List<string> types;

    public Dictionary<string, Value> dict;

    public Dictionary<string, Value> Deserialize()
    {
        dict = new();
        if(types == null)
        {
            Debug.LogWarning($"No types found");
            return dict;
        }
        for (int i = 0; i < types.Count; i++)
        {
            if (types[i] == "float")
            {
                dict.Add(strings[i], new FloatValue(floatValues[i]));
            }
            else if (types[i] == "int")
            {
                dict.Add(strings[i], new IntValue((int)floatValues[i]));
            }
            else if (types[i] == "object")
            {
                dict.Add(strings[i], new ObjectValue(objectValues[i]));
            }
        }

        return dict;
    }


    public void Serialize()
    {
        Debug.Log($"Serializing Pattern");

        strings = new();
        floatValues = new();
        objectValues = new();
        types = new();

        foreach (KeyValuePair<string, Value> _pair in dict)
        {
            strings.Add(_pair.Key);
            types.Add(_pair.Value.type);

            float floatToAdd = 0;
            Object objectToAdd = null;

            if (_pair.Value is FloatValue _fv)
            {
                floatToAdd = _fv.value;
            }
            else if (_pair.Value is IntValue _iv)
            {
                floatToAdd = _iv.value;
            }
            else if (_pair.Value is ObjectValue _ov)
            {
                objectToAdd = _ov.value;
            }

            floatValues.Add(floatToAdd);
            objectValues.Add(objectToAdd);
        }
    }
}

