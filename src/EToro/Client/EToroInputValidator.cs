namespace GrznarAi.Trading.ReadOnly.Client;

internal static class EToroInputValidator
{
    internal static void ValidateOptionalString(string? value, string paramName, int maxLength)
    {
        if (value is null) return;
        if (value.Length > maxLength)
            throw new ArgumentException($"Value exceeds maximum allowed length of {maxLength}.", paramName);
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsControl(value[i]))
                throw new ArgumentException("Value contains invalid control characters.", paramName);
        }
    }

    internal static void ValidateRequiredString(string value, string paramName, int maxLength)
    {
        // Callers perform null/whitespace checks first; this validates API-safe content constraints.
        if (value.Length > maxLength)
            throw new ArgumentException($"Value exceeds maximum allowed length of {maxLength}.", paramName);
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsControl(value[i]))
                throw new ArgumentException("Value contains invalid control characters.", paramName);
        }
    }
}
