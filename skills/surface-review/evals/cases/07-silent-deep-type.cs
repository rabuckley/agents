// Case 07 — review the public API of this type.

namespace Contoso.Routing;

/// <summary>
/// A parsed route template such as "/orders/{id:int}/items/{sku?}". A parameter may carry a constraint
/// (":int", ":guid", ":alpha") that a path segment must satisfy to match, and a trailing "?" marks it optional.
/// </summary>
public sealed class RouteTemplate : IEquatable<RouteTemplate>
{
    private readonly Segment[] _segments;

    private RouteTemplate(string text, Segment[] segments)
    {
        Text = text;
        _segments = segments;
        Parameters = segments.Where(s => s.IsParameter).Select(s => s.Name!).ToArray();
    }

    public string Text { get; }
    public IReadOnlyList<string> Parameters { get; }

    /// <summary>Parses <paramref name="text"/>; throws <see cref="FormatException"/> if it is not a valid template.</summary>
    public static RouteTemplate Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return TryParse(text, out var template) ? template : throw new FormatException($"Invalid route template '{text}'.");
    }

    /// <summary>Returns false, without throwing, for null or invalid <paramref name="text"/>.</summary>
    public static bool TryParse(string? text, [NotNullWhen(true)] out RouteTemplate? template)
    {
        template = text is not null && ParseCore(text, out var segments) ? new RouteTemplate(text, segments) : null;
        return template is not null;
    }

    /// <summary>
    /// Returns true and the parameter values when <paramref name="path"/> matches the template, including
    /// every constraint; false otherwise. Omitted optional parameters are absent from <paramref name="values"/>.
    /// </summary>
    public bool TryMatch(string path, [NotNullWhen(true)] out IReadOnlyDictionary<string, string>? values)
    {
        ArgumentNullException.ThrowIfNull(path);
        return MatchCore(path, _segments, out values);
    }

    /// <summary>
    /// Builds a path from <paramref name="values"/>. Optional parameters may be omitted; a missing required
    /// parameter or a value that violates its constraint throws <see cref="ArgumentException"/>, since the
    /// values are the caller's own data rather than input to be probed.
    /// </summary>
    public string Bind(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return BindCore(_segments, values);
    }

    public bool Equals(RouteTemplate? other) => other is not null && string.Equals(Text, other.Text, StringComparison.Ordinal);
    public override bool Equals(object? obj) => Equals(obj as RouteTemplate);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Text);
    public override string ToString() => Text;

    private readonly record struct Segment(string? Name, string? Literal, string? Constraint, bool Optional)
    {
        public bool IsParameter => Name is not null;
    }

    private static bool ParseCore(string text, out Segment[] segments) => throw null!;
    private static bool MatchCore(string path, Segment[] segments, out IReadOnlyDictionary<string, string>? values) => throw null!;
    private static string BindCore(Segment[] segments, IReadOnlyDictionary<string, string> values) => throw null!;
}
