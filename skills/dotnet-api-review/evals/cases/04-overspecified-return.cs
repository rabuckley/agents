// Case 04 — review the public API of this type.

namespace Contoso.Search;

public sealed class TagIndex
{
    private readonly SortedDictionary<string, List<int>> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _tags = new();

    public SortedDictionary<string, List<int>> Entries => _entries;

    public void Add(string tag, int documentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(tag);
        if (!_entries.TryGetValue(tag, out var ids))
        {
            ids = new List<int>();
            _entries[tag] = ids;
            _tags.Add(tag);
        }
        ids.Add(documentId);
    }

    public void AddRange(Dictionary<string, int> tagsByDocument)
    {
        foreach (var (tag, documentId) in tagsByDocument) Add(tag, documentId);
    }

    public List<string> GetTags() => _tags;

    public int[] Find(string tag) => _entries.TryGetValue(tag, out var ids) ? ids.ToArray() : Array.Empty<int>();
}
