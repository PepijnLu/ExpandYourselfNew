using UnityEngine;

[System.Serializable]
public class Value
{
    public string type;
    public Value()
    { 
        if(this is FloatValue)
        {
            type = "float";
        }
        else if (this is IntValue)
        {
            type = "int";
        }
        else if (this is ObjectValue)
        {
            type = "object";
        }
    }
}
[System.Serializable]
public class IntValue : Value
{
    public int value;

    public IntValue(int _value)
    {
        value = _value;
    }
}
[System.Serializable]
public class FloatValue : Value
{
    public float value;

    public FloatValue(float _value)
    {
        value = _value;
    }
}
//[System.Serializable]
//public class Vector2Value : Value
//{
//    public Vector2 value;

//    public Vector2Value(Vector2 _value)
//    {
//        value = _value;
//    }
//}
[System.Serializable]
public class ObjectValue : Value
{
    public Object value;
    public ObjectValue(Object _value)
    {
        value = _value;
    }
}
//[System.Serializable]
//public class LinearVariablesValue : Value
//{
//    public LinearVariables value;
//    public LinearVariablesValue(LinearVariables _value)
//    {
//        value = _value;
//    }
//}
//[System.Serializable]
//public class SineVariablesValue : Value
//{
//    public SineVariables value;
//    public SineVariablesValue(SineVariables _value)
//    {
//        value = _value;
//    }
//}
