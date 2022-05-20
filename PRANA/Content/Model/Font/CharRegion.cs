namespace PRANA;

public struct CharRegion
{
    public char Start;
    public char End;
    public int StartIndex;

    public CharRegion(char start, int startIndex)
    {
        Start = start;
        End = start;
        StartIndex = startIndex;
    }
}