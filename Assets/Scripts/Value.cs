using UnityEngine;

public class Value
{

}

public class IntValue : Value
{
    public int value;

    public IntValue(int _value)
    {
        value = _value;
    }
}

public class FloatValue : Value
{
    public float value;

    public FloatValue(float _value)
    {
        value = _value;
    }
}

public class Vector2Value : Value
{
    public Vector2 value;

    public Vector2Value(Vector2 _value)
    {
        value = _value;
    }
}

public class ObjectValue : Value
{
    public Object value;
    public ObjectValue(Object _value)
    {
        value = _value;
    }
}

public class LinearVariablesValue : Value
{
    public LinearVariables value;
    public LinearVariablesValue(LinearVariables _value)
    {
        value = _value;
    }
}

public class SineVariablesValue : Value
{
    public SineVariables value;
    public SineVariablesValue(SineVariables _value)
    {
        value = _value;
    }
}
