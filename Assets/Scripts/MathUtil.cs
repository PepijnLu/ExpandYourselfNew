using UnityEngine;

public struct SineVariables
{
    public float amp;
    public float freq;
    public float phase;
}

public struct LinearVariables
{
    public float coefficient;
    public float start;
}
public class MathUtil
{
    //y = at + b
    public float SolveLinearEquation(LinearVariables _vars, float t)
    {
        return (_vars.coefficient * t) + _vars.start;
    }

    public float SolveSineEquation(SineVariables _vars, float t)
    {
        float degrees = (2 * Mathf.PI * _vars.freq * t) + _vars.phase;

        float toRadians = degrees * Mathf.Deg2Rad;

        float radians = Mathf.Sin(toRadians);

        return _vars.amp * Mathf.Sin(radians);
    }
}
