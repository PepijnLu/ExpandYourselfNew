using UnityEngine;

public static class MathUtil
{
    //y = at + b
    public static float SolveLinearEquation(float _coefficient, float _start, float t)
    {
        return (_coefficient * t) + _start;
    }

    public static float SolveSineEquation(float _amp, float _freq, float _phase, float _t)
    {
        float degrees = (2 * Mathf.PI * _freq * _t) + _phase;

        float toRadians = degrees * Mathf.Deg2Rad;

        float radians = Mathf.Sin(toRadians);

        return _amp * Mathf.Sin(radians);
    }
}
