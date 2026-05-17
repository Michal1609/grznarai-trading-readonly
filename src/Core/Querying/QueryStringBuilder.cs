using System.Globalization;

namespace GrznarAi.Trading.ReadOnly.Querying;

/// <summary>
/// Fluent builder for URL query strings. Not thread-safe — use one instance per request.
/// </summary>
public sealed class QueryStringBuilder
{
    private readonly List<string> _parts = [];

    public QueryStringBuilder Add(string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            _parts.Add($"{Escape(name)}={Escape(value)}");

        return this;
    }

    public QueryStringBuilder Add(string name, int value)
    {
        _parts.Add($"{Escape(name)}={value.ToString(CultureInfo.InvariantCulture)}");
        return this;
    }

    public QueryStringBuilder Add(string name, bool value)
    {
        _parts.Add($"{Escape(name)}={(value ? "true" : "false")}");
        return this;
    }

    public QueryStringBuilder AddIfHasValue(string name, int? value)
    {
        if (value.HasValue)
            Add(name, value.Value);

        return this;
    }

    public QueryStringBuilder AddIfHasValue(string name, bool? value)
    {
        if (value.HasValue)
            Add(name, value.Value);

        return this;
    }

    public QueryStringBuilder AddDate(string name, DateOnly value)
    {
        _parts.Add($"{Escape(name)}={value:yyyy-MM-dd}");
        return this;
    }

    public QueryStringBuilder AddCsv(string name, IEnumerable<int>? values)
    {
        if (values is null)
            return this;

        var list = values.ToList();
        if (list.Count > 0)
            _parts.Add($"{Escape(name)}={string.Join(",", list)}");

        return this;
    }

    public QueryStringBuilder AddCsv(string name, IEnumerable<string>? values)
    {
        if (values is null) return this;
        var list = values.Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
        if (list.Count > 0)
            _parts.Add($"{Escape(name)}={string.Join(",", list.Select(Escape))}");

        return this;
    }

    /// <summary>
    /// Appends one query-string entry per value (e.g. ?ids=A&amp;ids=B).
    /// Skips null/whitespace values.
    /// </summary>
    public QueryStringBuilder AddRepeated(string name, IEnumerable<string>? values)
    {
        if (values is null) return this;
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                _parts.Add($"{Escape(name)}={Escape(v)}");
        }
        return this;
    }

    public override string ToString() => _parts.Count == 0 ? string.Empty : "?" + string.Join("&", _parts);

    public static string EscapePathSegment(string value) => Uri.EscapeDataString(value);

    private static string Escape(string value) => Uri.EscapeDataString(value);
}
