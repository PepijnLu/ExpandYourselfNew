using System.Collections.Generic;
using UnityEngine;

public static class BulletValueDict
{
    public static Value TryGetValue(Dictionary<string, Value> _dict, string _key)
    {
        if (_dict == null)
        {
            Debug.LogWarning("Dictionary is null, cant get value");
            return null;
        }
        if(_dict.TryGetValue(_key, out Value _value))
        { 
            return _value;
        }

        else return null;
    }

    public static Dictionary<string, Value> GetCopyOfDict(Dictionary<string, Value> _dict)
    {
        Dictionary<string, Value> newDict = new();

        foreach(KeyValuePair<string, Value> _pair in _dict)
        {
            newDict.Add(_pair.Key, _pair.Value);
        }

        return newDict;
    }

    public static SerializedBulletValues GetIndividualBulletOverride(BulletPattern _pattern, int _i)
    {
        SerializedBulletValues _value = null;

        if(_pattern.individualBulletOverrides == null)
        {
            Debug.Log($"Individual Overrides are null");
            return _value;
        }

        foreach (SerializedBulletValues _bulletValues in _pattern.individualBulletOverrides)
        {
            Value intValue = TryGetValue(_bulletValues.dict, "bulletInt");
            if (intValue != null)
            {
                int overrideInt = (intValue as IntValue).value;
                if (_i == overrideInt)
                {
                    _value = _bulletValues;
                    break;
                }
            }
        }

        return _value;
    }
}
