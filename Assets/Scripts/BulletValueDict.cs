using System.Collections.Generic;
using UnityEngine;

public static class BulletValueDict
{
    public static Value TryGetValue(Dictionary<string, Value> _dict, string _key)
    {
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
}
