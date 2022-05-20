using System.Text;

namespace PRANA;

internal readonly struct CharSource
{
    public readonly int Length;

    public char this[int index] => _string?[index] ?? _builder[index];

    private readonly string _string;

    private readonly StringBuilder _builder;

    public CharSource(string str)
    {
        _string = str;
        _builder = null;
        Length = str.Length;
    }

    public CharSource(StringBuilder builder)
    {
        _builder = builder;
        _string = null;
        Length = builder.Length;
    }
}