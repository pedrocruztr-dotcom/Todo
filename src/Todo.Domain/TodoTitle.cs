namespace Todo.Domain;

// Título válido por construção: nunca vazio, nunca acima de MaxLength, já sem espaços à volta.
public readonly record struct TodoTitle
{
    public const int MaxLength = 200;

    private TodoTitle(string value) => Value = value;

    public string Value { get; }

    public static bool TryParse(string? candidate, out TodoTitle title)
    {
        var trimmed = candidate?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxLength)
        {
            title = default;
            return false;
        }

        title = new TodoTitle(trimmed);
        return true;
    }

    public override string ToString() => Value;
}
