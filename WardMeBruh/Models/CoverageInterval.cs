namespace WardMeBruh.Models;

public readonly struct CoverageInterval
{
    public readonly float Start;
    public readonly float End;

    public CoverageInterval(float start, float end)
    {
        Start = start;
        End = end;
    }
}
