// Case 05 — review the public API of this type.

namespace Contoso.Text;

public sealed class Slugifier
{
    public int MaxLength = 80;

    public List<string> ReservedWords { get; set; } = new();

    public string Slugify(string text, bool lowercase, bool stripDiacritics)
    {
        ArgumentNullException.ThrowIfNull(text);
        var slug = Normalize(text, lowercase, stripDiacritics);
        if (slug.Length > MaxLength) slug = slug[..MaxLength];
        return ReservedWords.Contains(slug, StringComparer.OrdinalIgnoreCase) ? slug + "-1" : slug;
    }

    public string Slugify(string text) => Slugify(text, true, true);

    private static string Normalize(string text, bool lowercase, bool stripDiacritics) => throw null!;
}
