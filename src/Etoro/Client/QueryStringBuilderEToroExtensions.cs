using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

internal static class QueryStringBuilderEToroExtensions
{
    public static QueryStringBuilder Add(this QueryStringBuilder qsb, string name, Enum value)
        => qsb.Add(name, value.ToApiString());
}
